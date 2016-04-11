using System;
using System.Linq;
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
    public static class TimeTrackedEntityStoreExtensions
    {
        public static Task<TEntity> FindLatestEntityAsync<TEntity, TContext, TKey>(
            this ITimeTrackedEntityStoreMarker<TEntity, TContext, TKey> store,
            CancellationToken cancellationToken)
            where TEntity : class, ITimeWiseEntity
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            return FindLatestEntityInternalAsync(store, null, cancellationToken);
        }

        public static Task<TEntity> FindLatestEntityAsync<TEntity, TContext, TKey>(
            this ITimeTrackedEntityStoreMarker<TEntity, TContext, TKey> store,
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

            return FindLatestEntityInternalAsync(store, dataCreateDateSelector, cancellationToken);
        }

        private static Task<TEntity> FindLatestEntityInternalAsync<TEntity, TContext, TKey>(
            ITimeTrackedEntityStoreMarker<TEntity, TContext, TKey> store,
            Expression<Func<TEntity, string>> dataCreateDateSelector,
            CancellationToken cancellationToken)
            where TEntity : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            cancellationToken.ThrowIfCancellationRequested();
            store.ThrowIfDisposed();

            if (dataCreateDateSelector != null)
            {
                return store.Entities.OrderByDescending(x => dataCreateDateSelector.Compile().Invoke(x)).FirstOrDefaultAsync(cancellationToken);
            }

            if (typeof(ITimeWiseEntity).IsAssignableFrom(typeof(TEntity)))
            {
                return store.Entities.OrderByDescending(x => ((ITimeWiseEntity)x).DataCreateDate).FirstOrDefaultAsync(cancellationToken);
            }

            throw new InvalidOperationException();
        }
    }
}
