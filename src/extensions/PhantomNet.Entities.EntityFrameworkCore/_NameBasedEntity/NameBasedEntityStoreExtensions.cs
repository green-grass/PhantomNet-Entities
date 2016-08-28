using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PhantomNet.Entities.EntityFrameworkCore
{
    public static class QueryableNameBasedEntityStoreExtensions
    {
        public static Task<TEntity> FindEntityByNameAsync<TEntity, TContext, TKey>(
            this IQueryableNameBasedEntityStoreMarker<TEntity, TContext, TKey> store,
            string normalizedName,
            CancellationToken cancellationToken)
            where TEntity : class, INameWiseEntity
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            store.ThrowIfDisposed();
            return store.FindEntityByNameAsync(store.Entities, normalizedName, cancellationToken);
        }

        public static Task<TEntity> FindEntityByNameAsync<TEntity, TContext, TKey>(
            this IQueryableNameBasedEntityStoreMarker<TEntity, TContext, TKey> store,
            string normalizedName,
            Expression<Func<TEntity, string>> normalizedNameSelector,
            CancellationToken cancellationToken)
            where TEntity : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            store.ThrowIfDisposed();
            return store.FindEntityByNameAsync(store.Entities, normalizedName, normalizedNameSelector, cancellationToken);
        }
    }

    public static class NameBasedEntityStoreExtensions
    {
        public static Task<TEntity> FindEntityByNameAsync<TEntity, TContext, TKey>(
            this INameBasedEntityStoreMarker<TEntity, TContext, TKey> store,
            IQueryable<TEntity> entities,
            string normalizedName,
            CancellationToken cancellationToken)
            where TEntity : class, INameWiseEntity
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            return FindEntityByNameInternalAsync(store, entities, normalizedName, null, cancellationToken);
        }

        public static Task<TEntity> FindEntityByNameAsync<TEntity, TContext, TKey>(
            this INameBasedEntityStoreMarker<TEntity, TContext, TKey> store,
            IQueryable<TEntity> entities,
            string normalizedName,
            Expression<Func<TEntity, string>> normalizedNameSelector,
            CancellationToken cancellationToken)
            where TEntity : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            if (normalizedNameSelector == null)
            {
                throw new ArgumentNullException(nameof(normalizedNameSelector));
            }

            return FindEntityByNameInternalAsync(store, entities, normalizedName, normalizedNameSelector, cancellationToken);
        }

        private static async Task<TEntity> FindEntityByNameInternalAsync<TEntity>(
            object store,
            IQueryable<TEntity> entities,
            string normalizedName,
            Expression<Func<TEntity, string>> normalizedNameSelector,
            CancellationToken cancellationToken)
            where TEntity : class
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }
            if (normalizedName == null)
            {
                throw new ArgumentNullException(nameof(normalizedName));
            }

            if (store is IEagerLoadingEntityStore<TEntity>)
            {
                entities = ((IEagerLoadingEntityStore<TEntity>)store).EagerLoad(entities);
            }

            TEntity entity;

            if (normalizedNameSelector != null)
            {
                entity = await entities.SingleOrDefaultAsync(normalizedNameSelector, normalizedName, cancellationToken);
            }
            else if (typeof(INameWiseEntity).IsAssignableFrom(typeof(TEntity)))
            {
                entity = await entities.SingleOrDefaultAsync(x => ((INameWiseEntity)x).Name == normalizedName, cancellationToken);
            }
            else
            {
                throw new InvalidOperationException();
            }

            if (store is IExplicitLoadingEntityStore<TEntity> && entity != null)
            {
                await((IExplicitLoadingEntityStore<TEntity>)store).ExplicitLoadAsync(entity, cancellationToken);
            }

            return entity;
        }
    }
}
