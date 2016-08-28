using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PhantomNet.Entities.EntityFrameworkCore
{
    public static class QueryableCodeBasedEntityStoreExtensions
    {
        public static Task<TEntity> FindEntityByCodeAsync<TEntity, TContext, TKey>(
            this IQueryableCodeBasedEntityStoreMarker<TEntity, TContext, TKey> store,
            string normalizedCode,
            CancellationToken cancellationToken)
            where TEntity : class, ICodeWiseEntity
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            store.ThrowIfDisposed();
            return store.FindEntityByCodeAsync(store.Entities, normalizedCode, cancellationToken);
        }

        public static Task<TEntity> FindEntityByCodeAsync<TEntity, TContext, TKey>(
            this IQueryableCodeBasedEntityStoreMarker<TEntity, TContext, TKey> store,
            string normalizedCode,
            Expression<Func<TEntity, string>> codeSelector,
            CancellationToken cancellationToken)
            where TEntity : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            store.ThrowIfDisposed();
            return store.FindEntityByCodeAsync(store.Entities, normalizedCode, codeSelector, cancellationToken);
        }
    }

    public static class CodeBasedEntityStoreExtensions
    {
        public static Task<TEntity> FindEntityByCodeAsync<TEntity, TContext, TKey>(
            this ICodeBasedEntityStoreMarker<TEntity, TContext, TKey> store,
            IQueryable<TEntity> entities,
            string normalizedCode,
            CancellationToken cancellationToken)
            where TEntity : class, ICodeWiseEntity
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            return FindEntityByCodeInternalAsync(store, entities, normalizedCode, null, cancellationToken);
        }

        public static Task<TEntity> FindEntityByCodeAsync<TEntity, TContext, TKey>(
            this ICodeBasedEntityStoreMarker<TEntity, TContext, TKey> store,
            IQueryable<TEntity> entities,
            string normalizedCode,
            Expression<Func<TEntity, string>> codeSelector,
            CancellationToken cancellationToken)
            where TEntity : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            if (codeSelector == null)
            {
                throw new ArgumentNullException(nameof(codeSelector));
            }

            return FindEntityByCodeInternalAsync(store, entities, normalizedCode, codeSelector, cancellationToken);
        }

        private static async Task<TEntity> FindEntityByCodeInternalAsync<TEntity>(
            object store,
            IQueryable<TEntity> entities,
            string normalizedCode,
            Expression<Func<TEntity, string>> codeSelector,
            CancellationToken cancellationToken)
            where TEntity : class
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }
            if (normalizedCode == null)
            {
                throw new ArgumentNullException(nameof(normalizedCode));
            }

            if (store is IEagerLoadingEntityStore<TEntity>)
            {
                entities = ((IEagerLoadingEntityStore<TEntity>)store).EagerLoad(entities);
            }

            TEntity entity;

            if (codeSelector != null)
            {
                entity = await entities.SingleOrDefaultAsync(codeSelector, normalizedCode, cancellationToken);
            }
            else if (typeof(ICodeWiseEntity).IsAssignableFrom(typeof(TEntity)))
            {
                entity = await entities.SingleOrDefaultAsync(x => ((ICodeWiseEntity)x).Code == normalizedCode, cancellationToken);
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
