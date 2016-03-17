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

        public static Task SetEntityDataCreateDateAsync<TEntity, TContext, TKey>(
            this ITimeTrackedEntityStoreMarker<TEntity, TContext, TKey> store,
            TEntity entity, DateTime dataCreateDate, CancellationToken cancellationToken)
            where TEntity : class, ITimeWiseEntity
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            return SetEntityDataCreateDateAsync(store, entity, dataCreateDate, cancellationToken, null);
        }

        public static Task SetEntityDataCreateDateAsync<TEntity, TContext, TKey>(
            this ITimeTrackedEntityStoreMarker<TEntity, TContext, TKey> store,
            TEntity entity, DateTime dataCreateDate, CancellationToken cancellationToken,
            Action directSetDataCreateDate)
            where TEntity : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            cancellationToken.ThrowIfCancellationRequested();
            store.ThrowIfDisposed();
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (directSetDataCreateDate == null && typeof(ITimeWiseEntity).IsAssignableFrom(typeof(TEntity)))
            {
                ((ITimeWiseEntity)entity).DataCreateDate = dataCreateDate;
            }
            else
            {
                directSetDataCreateDate();
            }
            return Task.FromResult(0);
        }

        public static Task SetEntityDataLastModifyDateAsync<TEntity, TContext, TKey>(
            this ITimeTrackedEntityStoreMarker<TEntity, TContext, TKey> store,
            TEntity entity, DateTime dataLastModifyDate, CancellationToken cancellationToken)
            where TEntity : class, ITimeWiseEntity
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            return SetEntityDataLastModifyDateAsync(store, entity, dataLastModifyDate, cancellationToken, null);
        }

        public static Task SetEntityDataLastModifyDateAsync<TEntity, TContext, TKey>(
            this ITimeTrackedEntityStoreMarker<TEntity, TContext, TKey> store,
            TEntity entity, DateTime dataLastModifyDate, CancellationToken cancellationToken,
            Action directSetDataLastModifyDate)
            where TEntity : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            cancellationToken.ThrowIfCancellationRequested();
            store.ThrowIfDisposed();
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (directSetDataLastModifyDate == null && typeof(ITimeWiseEntity).IsAssignableFrom(typeof(TEntity)))
            {
                ((ITimeWiseEntity)entity).DataLastModifyDate = dataLastModifyDate;
            }
            else
            {
                directSetDataLastModifyDate();
            }
            return Task.FromResult(0);
        }
    }
}
