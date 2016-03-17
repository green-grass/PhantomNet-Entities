using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using PhantomNet.Entities.EntityMarkers;
#if DOTNET5_4
using System.Reflection;
#endif

namespace PhantomNet.Entities.EntityFramework
{
    public static class ReadOnlyCodeBasedEntityStoreExtensions
    {
        public static Task<TEntity> FindEntityByCodeAsync<TEntity, TContext, TKey>(
            this IReadOnlyCodeBasedEntityStoreMarker<TEntity, TContext, TKey> store,
            string normalizedCode, CancellationToken cancellationToken)
            where TEntity : class, ICodeWiseEntity
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            return FindEntityByCodeAsync(store, normalizedCode, cancellationToken, null, null);
        }

        public static Task<TEntity> FindEntityByCodeAsync<TEntity, TContext, TKey>(
            this IReadOnlyCodeBasedEntityStoreMarker<TEntity, TContext, TKey> store,
            string normalizedCode, CancellationToken cancellationToken,
            Expression<Func<TEntity, string>> codeSelector,
            Func<Task<TEntity>> directFindByCodeAsync)
            where TEntity : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            cancellationToken.ThrowIfCancellationRequested();
            store.ThrowIfDisposed();
            if (normalizedCode == null)
            {
                throw new ArgumentNullException(nameof(normalizedCode));
            }

            if (directFindByCodeAsync == null &&
                codeSelector == null &&
                typeof(ICodeWiseEntity).IsAssignableFrom(typeof(TEntity)) &&
                store is IQueryableEntityStore<TEntity>)
            {
                return ((IQueryableEntityStore<TEntity>)store).Entities.SingleOrDefaultAsync(x => ((ICodeWiseEntity)x).Code == normalizedCode, cancellationToken);
            }
            else if (codeSelector != null)
            {
                return ((IQueryableEntityStore<TEntity>)store).Entities.SingleOrDefaultAsync(x => codeSelector.Compile().Invoke(x) == normalizedCode, cancellationToken);
            }
            else
            {
                if (directFindByCodeAsync == null)
                {
                    throw new ArgumentNullException(nameof(directFindByCodeAsync));
                }

                return directFindByCodeAsync();
            }
        }
    }
}
