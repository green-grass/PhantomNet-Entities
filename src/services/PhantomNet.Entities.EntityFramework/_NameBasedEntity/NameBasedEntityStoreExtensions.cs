using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PhantomNet.Entities.EntityFramework
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
            return FindEntityByNameInternalAsync(entities, normalizedName, null, cancellationToken);
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

            return FindEntityByNameInternalAsync(entities, normalizedName, normalizedNameSelector, cancellationToken);
        }

        private static Task<TEntity> FindEntityByNameInternalAsync<TEntity>(
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

            if (normalizedNameSelector != null)
            {
                return entities.SingleOrDefaultAsync(x => normalizedNameSelector.Compile().Invoke(x) == normalizedName, cancellationToken);
            }

            if (typeof(INameWiseEntity).IsAssignableFrom(typeof(TEntity)))
            {
                return entities.SingleOrDefaultAsync(x => ((INameWiseEntity)x).NormalizedName == normalizedName, cancellationToken);
            }

            throw new InvalidOperationException();
        }
    }
}
