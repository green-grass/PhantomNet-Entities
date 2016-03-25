using System;
using System.ComponentModel;
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
    public static partial class EntityStoreExtensions
    {
        public static Task<T> FindEntityOrSubEntityByIdAsync<TEntity, TSubEntity, TContext, TKey, T>(
            this IEntityStoreMarker<TEntity, TContext, TKey> store,
            string id, CancellationToken cancellationToken)
            where TEntity : class, IIdWiseEntity<TKey>
            where TSubEntity : class, IIdWiseEntity<TKey>
            where TContext : DbContext
            where TKey : IEquatable<TKey>
            where T : class
        {
            return FindEntityOrSubEntityByIdAsync<TEntity, TSubEntity, TContext, TKey, T>(store, id, cancellationToken, null, null);
        }

        public static async Task<T> FindEntityOrSubEntityByIdAsync<TEntity, TSubEntity, TContext, TKey, T>(
            this IEntityStoreMarker<TEntity, TContext, TKey> store,
            string id, CancellationToken cancellationToken,
            Func<TKey, Task<TEntity>> directFindEntityByIdAsync,
            Func<TKey, Task<TSubEntity>> directFindSubEntityByIdAsync)
            where TEntity : class
            where TSubEntity : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
            where T : class
        {
            cancellationToken.ThrowIfCancellationRequested();
            store.ThrowIfDisposed();
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var key = ConvertIdFromString<TKey>(id);
            if (typeof(T) == typeof(TEntity))
            {
                if (directFindEntityByIdAsync == null
                    && typeof(IIdWiseEntity<TKey>).IsAssignableFrom(typeof(TEntity))
                    && store is IQueryableEntityStore<TEntity, TSubEntity>)
                {
                    return await ((IQueryableEntityStore<TEntity, TSubEntity>)store).Entities
                        .SingleOrDefaultAsync(x => ((IIdWiseEntity<TKey>)x).Id.Equals(key), cancellationToken) as T;
                }
                else
                {
                    return await directFindEntityByIdAsync(key) as T;
                }
            }
            else if (typeof(T) == typeof(TSubEntity))
            {
                if (directFindSubEntityByIdAsync == null
                    && typeof(IIdWiseEntity<TKey>).IsAssignableFrom(typeof(TSubEntity))
                    && store is IQueryableEntityStore<TEntity, TSubEntity>)
                {
                    return await ((IQueryableEntityStore<TEntity, TSubEntity>)store).SubEntities
                        .SingleOrDefaultAsync(x => ((IIdWiseEntity<TKey>)x).Id.Equals(key), cancellationToken) as T;
                }
                else
                {
                    return await directFindSubEntityByIdAsync(key) as T;
                }
            }

            throw new InvalidOperationException(string.Format(Resources.EntityTypeOrSubEntityTypeNotSupported, nameof(T), nameof(TEntity), nameof(TSubEntity)));
        }
    }

    public static partial class EntityStoreExtensions
    {
        public static async Task<EntityResult> CreateEntityAsync<TEntity, TContext, TKey>(
            this IEntityStoreMarker<TEntity, TContext, TKey> store,
            TEntity entity, CancellationToken cancellationToken)
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

            store.Context.Add(entity);

            await store.SaveChangesAsync(cancellationToken);

            return EntityResult.Success;
        }

        public static Task<EntityResult> UpdateEntityAsync<TEntity, TContext, TKey>(
            this IEntityStoreMarker<TEntity, TContext, TKey> store,
            TEntity entity, CancellationToken cancellationToken)
            where TEntity : class, IConcurrencyStampWiseEntity
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            return UpdateEntityAsync(store, entity, cancellationToken, null, null);
        }

        public static async Task<EntityResult> UpdateEntityAsync<TEntity, TContext, TKey>(
            this IEntityStoreMarker<TEntity, TContext, TKey> store,
            TEntity entity, CancellationToken cancellationToken,
            Action<string> setConcurrencyStamp,
            Func<EntityError> describeConcurrencyFailureError)
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

            store.Context.Attach(entity);
            if (setConcurrencyStamp == null && entity is IConcurrencyStampWiseEntity)
            {
                ((IConcurrencyStampWiseEntity)entity).ConcurrencyStamp = Guid.NewGuid().ToString();
            }
            else
            {
                setConcurrencyStamp(Guid.NewGuid().ToString());
            }
            store.Context.Update(entity);

            try
            {
                await store.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return EntityResult.Failed(describeConcurrencyFailureError == null ?
                    new EntityErrorDescriber().ConcurrencyFailure() :
                    describeConcurrencyFailureError());
            }

            return EntityResult.Success;
        }

        public static Task<EntityResult> DeleteEntityAsync<TEntity, TContext, TKey>(
            this IEntityStoreMarker<TEntity, TContext, TKey> store,
            TEntity entity, CancellationToken cancellationToken)
            where TEntity : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            return DeleteEntityAsync(store, entity, cancellationToken, null);
        }

        public static async Task<EntityResult> DeleteEntityAsync<TEntity, TContext, TKey>(
            this IEntityStoreMarker<TEntity, TContext, TKey> store,
            TEntity entity, CancellationToken cancellationToken,
            Func<EntityError> describeConcurrencyFailureError)
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

            store.Context.Remove(entity);

            try
            {
                await store.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return EntityResult.Failed(describeConcurrencyFailureError == null ?
                    new EntityErrorDescriber().ConcurrencyFailure() :
                    describeConcurrencyFailureError());
            }

            return EntityResult.Success;
        }

        public static Task<T> FindEntityByIdAsync<TEntity, TContext, TKey, T>(
            this IEntityStoreMarker<TEntity, TContext, TKey> store,
            string id, CancellationToken cancellationToken)
            where TEntity : class, IIdWiseEntity<TKey>
            where TContext : DbContext
            where TKey : IEquatable<TKey>
            where T : class
        {
            return FindEntityByIdAsync<TEntity, TContext, TKey, T>(store, id, cancellationToken, null);
        }

        public static async Task<T> FindEntityByIdAsync<TEntity, TContext, TKey, T>(
            this IEntityStoreMarker<TEntity, TContext, TKey> store,
            string id, CancellationToken cancellationToken,
            Func<TKey, Task<TEntity>> directFindByIdAsync)
            where TEntity : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
            where T : class
        {
            cancellationToken.ThrowIfCancellationRequested();
            store.ThrowIfDisposed();
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
            if (typeof(T) != typeof(TEntity))
            {
                throw new InvalidOperationException(string.Format(Resources.EntityTypeNotSupported, nameof(T), nameof(TEntity)));
            }

            var key = ConvertIdFromString<TKey>(id);
            if (directFindByIdAsync == null && typeof(IIdWiseEntity<TKey>).IsAssignableFrom(typeof(TEntity))
                && store is IQueryableEntityStore<TEntity>)
            {
                return await ((IQueryableEntityStore<TEntity>)store).Entities
                    .SingleOrDefaultAsync(x => ((IIdWiseEntity<TKey>)x).Id.Equals(key), cancellationToken) as T;
            }
            else
            {
                return await directFindByIdAsync(key) as T;
            }
        }

        public static IQueryable<TEntity> FilterEntities<TEntity, TContext, TKey>(
            this IEntityStoreMarker<TEntity, TContext, TKey> store,
            IQueryable<TEntity> query, string filter,
            Func<IQueryable<TEntity>> directFilter)
            where TEntity : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            return directFilter();
        }

        public static IQueryable<TEntity> PreSortEntities<TEntity, TContext, TKey>(
            this IEntityStoreMarker<TEntity, TContext, TKey> store,
            IQueryable<TEntity> query,
            Func<IQueryable<TEntity>> directPreSort)
            where TEntity : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return directPreSort();
        }

        public static IQueryable<TEntity> DefaultSortEntities<TEntity, TContext, TKey>(
            this IEntityStoreMarker<TEntity, TContext, TKey> store,
            IQueryable<TEntity> query,
            Func<IQueryable<TEntity>> directDefaultSort,
            Func<IOrderedQueryable<TEntity>, IQueryable<TEntity>> directOrderedDefaultSort)
            where TEntity : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }
            if (query.Expression.Type == typeof(IOrderedQueryable<TEntity>))
            {
                return directOrderedDefaultSort((IOrderedQueryable<TEntity>)query);
            }
            else
            {
                return directDefaultSort();
            }
        }

        public static Task<int> CountEntitiesAsync<TEntity, TContext, TKey>(
            this IEntityStoreMarker<TEntity, TContext, TKey> store,
            IQueryable<TEntity> entities, CancellationToken cancellationToken)
            where TEntity : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            cancellationToken.ThrowIfCancellationRequested();
            store.ThrowIfDisposed();
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            return entities.CountAsync(cancellationToken);
        }

        #region Helpers

        private static TKey ConvertIdFromString<TKey>(string id)
            where TKey : IEquatable<TKey>
        {
            if (id == null)
            {
                return default(TKey);
            }

            return (TKey)TypeDescriptor.GetConverter(typeof(TKey)).ConvertFromInvariantString(id);
        }

        #endregion
    }
}
