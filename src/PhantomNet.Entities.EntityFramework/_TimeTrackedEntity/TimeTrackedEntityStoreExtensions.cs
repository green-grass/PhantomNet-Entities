using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using PhantomNet.Entities.EntityMarkers;
#if DOTNET5_4
using System.Reflection;
#endif

namespace PhantomNet.Entities.EntityFramework
{
    public static class TimeTrackedEntityStoreExtensions
    {
        public static Task<TEntity> FindLatestEntityAsync<TEntity, TContext, TKey>(
            this ITimeTrackedEntityStoreMarker<TEntity, TContext, TKey> store,
            CancellationToken cancellationToken)
            where TEntity : class, ITimeWiseEntity
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            return FindLatestEntityAsync(store, cancellationToken, null);
        }

        public static Task<TEntity> FindLatestEntityAsync<TEntity, TContext, TKey>(
            this ITimeTrackedEntityStoreMarker<TEntity, TContext, TKey> store,
            CancellationToken cancellationToken,
            Func<Task<TEntity>> directFindLatestAsync)
            where TEntity : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            cancellationToken.ThrowIfCancellationRequested();
            store.ThrowIfDisposed();
            if (directFindLatestAsync == null && typeof(ITimeWiseEntity).IsAssignableFrom(typeof(TEntity))
                && store is IQueryableEntityStore<TEntity>)
            {
                return ((IQueryableEntityStore<TEntity>)store).Entities.OrderByDescending(x => ((ITimeWiseEntity)x).DataCreateDate).FirstOrDefaultAsync(cancellationToken);
            }
            else
            {
                return directFindLatestAsync();
            }
        }
    }
}
