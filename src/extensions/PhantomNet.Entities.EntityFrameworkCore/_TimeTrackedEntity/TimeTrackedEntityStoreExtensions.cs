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
            return FindLatestEntityInternalAsync(entities, null, cancellationToken);
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

            return FindLatestEntityInternalAsync(entities, dataCreateDateSelector, cancellationToken);
        }

        private static Task<TEntity> FindLatestEntityInternalAsync<TEntity>(
            IQueryable<TEntity> entities,
            Expression<Func<TEntity, string>> dataCreateDateSelector,
            CancellationToken cancellationToken)
            where TEntity : class
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (dataCreateDateSelector != null)
            {
                return entities.OrderByDescending(dataCreateDateSelector).FirstOrDefaultAsync(cancellationToken);
            }

            if (typeof(ITimeWiseEntity).IsAssignableFrom(typeof(TEntity)))
            {
                return entities.OrderByDescending(x => ((ITimeWiseEntity)x).DataCreateDate).FirstOrDefaultAsync(cancellationToken);
            }

            throw new InvalidOperationException();
        }
    }
}
