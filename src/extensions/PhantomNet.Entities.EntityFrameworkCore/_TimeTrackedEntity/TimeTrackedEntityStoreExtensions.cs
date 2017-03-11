using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PhantomNet.Entities.EntityFrameworkCore
{
    public static class QueryableTimeTrackedEntityStoreExtensions
    {
        public static Task<TEntity> FindLatestEntityAsync<TEntity, TContext, TKey>(
            this IQueryableTimeTrackedEntityStoreMarker<TEntity, TContext, TKey> store,
            CancellationToken cancellationToken)
            where TEntity : class, ITimeWiseEntity
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            store.ThrowIfDisposed();
            return store.FindLatestEntityAsync(store.Entities, cancellationToken);
        }

        public static Task<TEntity> FindLatestEntityAsync<TEntity, TContext, TKey>(
            this IQueryableTimeTrackedEntityStoreMarker<TEntity, TContext, TKey> store,
            Expression<Func<TEntity, string>> dataCreateDateSelector,
            CancellationToken cancellationToken)
            where TEntity : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            store.ThrowIfDisposed();
            return store.FindLatestEntityAsync(store.Entities, dataCreateDateSelector, cancellationToken);
        }
    }

    public static class TimeTrackedEntityStoreExtensions
    {
        public static Task<TEntity> FindLatestEntityAsync<TEntity, TContext, TKey>(
            this ITimeTrackedEntityStoreMarker<TEntity, TContext, TKey> store,
            IQueryable<TEntity> entities,
            CancellationToken cancellationToken)
            where TEntity : class, ITimeWiseEntity
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            return FindLatestEntityInternalAsync(store, entities, null, cancellationToken);
        }

        public static Task<TEntity> FindLatestEntityAsync<TEntity, TContext, TKey>(
            this ITimeTrackedEntityStoreMarker<TEntity, TContext, TKey> store,
            IQueryable<TEntity> entities,
            Expression<Func<TEntity, string>> dataCreateDateSelector,
            CancellationToken cancellationToken)
            where TEntity : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            if (dataCreateDateSelector == null)
            {
                throw new ArgumentNullException(nameof(dataCreateDateSelector));
            }

            return FindLatestEntityInternalAsync(store, entities, dataCreateDateSelector, cancellationToken);
        }

        private static async Task<TEntity> FindLatestEntityInternalAsync<TEntity>(
            object store,
            IQueryable<TEntity> entities,
            Expression<Func<TEntity, string>> dataCreateDateSelector,
            CancellationToken cancellationToken)
            where TEntity : class
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            if (store is IEagerLoadingEntityStore<TEntity>)
            {
                entities = ((IEagerLoadingEntityStore<TEntity>)store).EagerLoad(entities);
            }

            TEntity entity;

            if (dataCreateDateSelector != null)
            {
                entity = await entities.OrderByDescending(dataCreateDateSelector).FirstOrDefaultAsync(cancellationToken);
            }
            else if (typeof(ITimeWiseEntity).IsAssignableFrom(typeof(TEntity)))
            {
                entity = await entities.OrderByDescending(x => ((ITimeWiseEntity)x).DataCreateDate).FirstOrDefaultAsync(cancellationToken);
            }
            else
            {
                throw new InvalidOperationException();
            }

            if (store is IExplicitLoadingEntityStore<TEntity> && entity != null)
            {
                await ((IExplicitLoadingEntityStore<TEntity>)store).ExplicitLoadAsync(entity, cancellationToken);
            }

            return entity;
        }
    }
}
