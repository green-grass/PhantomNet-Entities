using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PhantomNet.Entities.EntityFramework
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
            return FindEntityByCodeInternalAsync(entities, normalizedCode, null, cancellationToken);
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

            return FindEntityByCodeInternalAsync(entities, normalizedCode, codeSelector, cancellationToken);
        }

        private static Task<TEntity> FindEntityByCodeInternalAsync<TEntity>(
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

            if (codeSelector != null)
            {
                return entities.SingleOrDefaultAsync(codeSelector, normalizedCode, cancellationToken);
            }

            if (typeof(ICodeWiseEntity).IsAssignableFrom(typeof(TEntity)))
            {
                return entities.SingleOrDefaultAsync(x => ((ICodeWiseEntity)x).Code == normalizedCode, cancellationToken);
            }

            throw new InvalidOperationException();
        }
    }
}
