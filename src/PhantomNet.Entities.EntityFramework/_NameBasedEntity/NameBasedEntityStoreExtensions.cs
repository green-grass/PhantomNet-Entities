using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using PhantomNet.Entities.EntityMarkers;
#if DOTNET5_4
using System.Reflection;
#endif

namespace PhantomNet.Entities.EntityFramework
{
    public static class NameBasedEntityStoreExtensions
    {
        public static Task<TEntity> FindEntityByNameAsync<TEntity, TContext, TKey>(
            this INameBasedEntityStoreMarker<TEntity, TContext, TKey> store,
            string normalizedName, CancellationToken cancellationToken)
            where TEntity : class, INameWiseEntity
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            return FindEntityByNameAsync(store, normalizedName, cancellationToken, null);
        }

        public static Task<TEntity> FindEntityByNameAsync<TEntity, TContext, TKey>(
            this INameBasedEntityStoreMarker<TEntity, TContext, TKey> store,
            string normalizedName, CancellationToken cancellationToken,
            Func<Task<TEntity>> directFindByNameAsync)
            where TEntity : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            cancellationToken.ThrowIfCancellationRequested();
            store.ThrowIfDisposed();
            if (normalizedName == null)
            {
                throw new ArgumentNullException(nameof(normalizedName));
            }

            if (directFindByNameAsync == null && typeof(INameWiseEntity).IsAssignableFrom(typeof(TEntity))
                && store is IQueryableEntityStore<TEntity>)
            {
                return ((IQueryableEntityStore<TEntity>)store).Entities.SingleOrDefaultAsync(x => ((INameWiseEntity)x).NormalizedName == normalizedName, cancellationToken);
            }
            else
            {
                return directFindByNameAsync();
            }
        }
    }
}
