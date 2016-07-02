using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PhantomNet.Entities.EntityFramework
{
    public static partial class QueryableEntityStoreExtensions
    {
        public static Task<T> FindEntityOrSubEntityByIdAsync<TEntity, TSubEntity, TContext, TKey, T>(
            this IQueryableEntityStoreMarker<TEntity, TSubEntity, TContext, TKey> store,
            string id,
            CancellationToken cancellationToken)
            where TEntity : class, IIdWiseEntity<TKey>
            where TSubEntity : class, IIdWiseEntity<TKey>
            where TContext : DbContext
            where TKey : IEquatable<TKey>
            where T : class
        {
            store.ThrowIfDisposed();
            return store.FindEntityOrSubEntityByIdAsync<TEntity, TSubEntity, TContext, TKey, T>(store.Entities, store.SubEntities, id, cancellationToken);
        }

        public static Task<T> FindEntityOrSubEntityByIdAsync<TEntity, TSubEntity, TContext, TKey, T>(
            this IQueryableEntityStoreMarker<TEntity, TSubEntity, TContext, TKey> store,
            string id,
            Expression<Func<TEntity, TKey>> entityIdSelector,
            Expression<Func<TSubEntity, TKey>> subEntityIdSelector,
            CancellationToken cancellationToken)
            where TEntity : class, IIdWiseEntity<TKey>
            where TSubEntity : class, IIdWiseEntity<TKey>
            where TContext : DbContext
            where TKey : IEquatable<TKey>
            where T : class
        {
            store.ThrowIfDisposed();
            return store.FindEntityOrSubEntityByIdAsync<TEntity, TSubEntity, TContext, TKey, T>(store.Entities, store.SubEntities, id, entityIdSelector, subEntityIdSelector, cancellationToken);
        }
    }

    public static partial class QueryableEntityStoreExtensions
    {
        public static Task<T> FindEntityByIdAsync<TEntity, TContext, TKey, T>(
            this IQueryableEntityStoreMarker<TEntity, TContext, TKey> store,
            string id,
            CancellationToken cancellationToken)
            where TEntity : class, IIdWiseEntity<TKey>
            where TContext : DbContext
            where TKey : IEquatable<TKey>
            where T : class
        {
            store.ThrowIfDisposed();
            return store.FindEntityByIdAsync<TEntity, TContext, TKey, T>(store.Entities, id, cancellationToken);
        }

        public static Task<T> FindEntityByIdAsync<TEntity, TContext, TKey, T>(
            this IQueryableEntityStoreMarker<TEntity, TContext, TKey> store,
            string id,
            Expression<Func<TEntity, TKey>> idSelector,
            CancellationToken cancellationToken)
            where TEntity : class, IIdWiseEntity<TKey>
            where TContext : DbContext
            where TKey : IEquatable<TKey>
            where T : class
        {
            store.ThrowIfDisposed();
            return store.FindEntityByIdAsync<TEntity, TContext, TKey, T>(store.Entities, id, idSelector, cancellationToken);
        }
    }

    public static partial class EntityStoreExtensions
    {
        public static Task<T> FindEntityOrSubEntityByIdAsync<TEntity, TSubEntity, TContext, TKey, T>(
            this IEntityStoreMarker<TEntity, TSubEntity, TContext, TKey> store,
            IQueryable<TEntity> entities,
            IQueryable<TSubEntity> subEntities,
            string id,
            CancellationToken cancellationToken)
            where TEntity : class, IIdWiseEntity<TKey>
            where TSubEntity : class, IIdWiseEntity<TKey>
            where TContext : DbContext
            where TKey : IEquatable<TKey>
            where T : class
        {
            return FindEntityOrSubEntityByIdInternalAsync<TEntity, TSubEntity, TKey, T>(entities, subEntities, id, null, null, cancellationToken);
        }

        public static Task<T> FindEntityOrSubEntityByIdAsync<TEntity, TSubEntity, TContext, TKey, T>(
            this IEntityStoreMarker<TEntity, TSubEntity, TContext, TKey> store,
            IQueryable<TEntity> entities,
            IQueryable<TSubEntity> subEntities,
            string id,
            Expression<Func<TEntity, TKey>> entityIdSelector,
            Expression<Func<TSubEntity, TKey>> subEntityIdSelector,
            CancellationToken cancellationToken)
            where TEntity : class, IIdWiseEntity<TKey>
            where TSubEntity : class, IIdWiseEntity<TKey>
            where TContext : DbContext
            where TKey : IEquatable<TKey>
            where T : class
        {
            if (typeof(T) == typeof(TEntity) && entityIdSelector == null)
            {
                throw new ArgumentNullException(nameof(entityIdSelector));
            }
            if (typeof(T) == typeof(TSubEntity) && subEntityIdSelector == null)
            {
                throw new ArgumentNullException(nameof(subEntityIdSelector));
            }

            return FindEntityOrSubEntityByIdInternalAsync<TEntity, TSubEntity, TKey, T>(entities, subEntities, id, entityIdSelector, subEntityIdSelector, cancellationToken);
        }

        #region Helpers

        private static async Task<T> FindEntityOrSubEntityByIdInternalAsync<TEntity, TSubEntity, TKey, T>(
            IQueryable<TEntity> entities,
            IQueryable<TSubEntity> subEntities,
            string id,
            Expression<Func<TEntity, TKey>> entityIdSelector,
            Expression<Func<TSubEntity, TKey>> subEntityIdSelector,
            CancellationToken cancellationToken)
            where TEntity : class
            where TSubEntity : class
            where TKey : IEquatable<TKey>
            where T : class
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var key = ConvertIdFromString<TKey>(id);

            if (typeof(T) == typeof(TEntity))
            {
                if (entities == null)
                {
                    throw new ArgumentNullException(nameof(entities));
                }

                if (entityIdSelector != null)
                {
                    return await entities.SingleOrDefaultAsync(entityIdSelector, key, cancellationToken) as T;
                }

                if (typeof(IIdWiseEntity<TKey>).IsAssignableFrom(typeof(TEntity)))
                {
                    return await entities.SingleOrDefaultAsync(x => ((IIdWiseEntity<TKey>)x).Id.Equals(key), cancellationToken) as T;
                }

                throw new InvalidOperationException();
            }
            else if (typeof(T) == typeof(TSubEntity))
            {
                if (subEntities == null)
                {
                    throw new ArgumentNullException(nameof(subEntities));
                }

                if (subEntityIdSelector != null)
                {
                    return await subEntities.SingleOrDefaultAsync(subEntityIdSelector, key, cancellationToken) as T;
                }

                if (typeof(IIdWiseEntity<TKey>).IsAssignableFrom(typeof(TSubEntity)))
                {
                    return await subEntities.SingleOrDefaultAsync(x => ((IIdWiseEntity<TKey>)x).Id.Equals(key), cancellationToken) as T;
                }

                throw new InvalidOperationException();
            }

            throw new InvalidOperationException(string.Format(Resources.EntityTypeOrSubEntityTypeNotSupported, nameof(T), nameof(TEntity), nameof(TSubEntity)));
        }

        #endregion
    }

    public static partial class EntityStoreExtensions
    {
        #region Create

        public static async Task<GenericResult> CreateEntityAsync<TEntity, TContext, TKey>(
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

            return GenericResult.Success;
        }

        #endregion

        #region Update

        public static Task<GenericResult> UpdateEntityAsync<TEntity, TContext, TKey>(
            this IEntityStoreMarker<TEntity, TContext, TKey> store,
            TEntity entity,
            CancellationToken cancellationToken)
            where TEntity : class, IConcurrencyStampWiseEntity
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            return UpdateEntityAsync(store, entity, null, cancellationToken);
        }

        public static Task<GenericResult> UpdateEntityAsync<TEntity, TContext, TKey>(
            this IEntityStoreMarker<TEntity, TContext, TKey> store,
            TEntity entity,
            Func<GenericError> describeConcurrencyFailureError,
            CancellationToken cancellationToken)
            where TEntity : class, IConcurrencyStampWiseEntity
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            return UpdateEntityInternalAsync(store, entity, null, describeConcurrencyFailureError, cancellationToken);
        }

        public static Task<GenericResult> UpdateEntityAsync<TEntity, TContext, TKey>(
            this IEntityStoreMarker<TEntity, TContext, TKey> store,
            TEntity entity,
            Action<string> setConcurrencyStamp,
            CancellationToken cancellationToken)
            where TEntity : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            return UpdateEntityAsync(store, entity, setConcurrencyStamp, null, cancellationToken);
        }

        public static Task<GenericResult> UpdateEntityAsync<TEntity, TContext, TKey>(
            this IEntityStoreMarker<TEntity, TContext, TKey> store,
            TEntity entity,
            Action<string> setConcurrencyStamp,
            Func<GenericError> describeConcurrencyFailureError,
            CancellationToken cancellationToken)
            where TEntity : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            if (setConcurrencyStamp == null)
            {
                throw new ArgumentNullException(nameof(setConcurrencyStamp));
            }

            return UpdateEntityInternalAsync(store, entity, setConcurrencyStamp, describeConcurrencyFailureError, cancellationToken);
        }

        #endregion

        #region Delete

        public static Task<GenericResult> DeleteEntityAsync<TEntity, TContext, TKey>(
            this IEntityStoreMarker<TEntity, TContext, TKey> store,
            TEntity entity, CancellationToken cancellationToken)
            where TEntity : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            return DeleteEntityAsync(store, entity, cancellationToken, null);
        }

        public static async Task<GenericResult> DeleteEntityAsync<TEntity, TContext, TKey>(
            this IEntityStoreMarker<TEntity, TContext, TKey> store,
            TEntity entity, CancellationToken cancellationToken,
            Func<GenericError> describeConcurrencyFailureError)
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
                return GenericResult.Failed(describeConcurrencyFailureError == null ?
                    new EntityErrorDescriber().ConcurrencyFailure() :
                    describeConcurrencyFailureError());
            }

            return GenericResult.Success;
        }

        #endregion

        public static Task<T> FindEntityByIdAsync<TEntity, TContext, TKey, T>(
            this IEntityStoreMarker<TEntity, TContext, TKey> store,
            IQueryable<TEntity> entities,
            string id,
            CancellationToken cancellationToken)
            where TEntity : class, IIdWiseEntity<TKey>
            where TContext : DbContext
            where TKey : IEquatable<TKey>
            where T : class
        {
            return FindEntityByIdInternalAsync<TEntity, TKey, T>(entities, id, null, cancellationToken);
        }

        public static Task<T> FindEntityByIdAsync<TEntity, TContext, TKey, T>(
            this IEntityStoreMarker<TEntity, TContext, TKey> store,
            IQueryable<TEntity> entities,
            string id,
            Expression<Func<TEntity, TKey>> idSelector,
            CancellationToken cancellationToken)
            where TEntity : class, IIdWiseEntity<TKey>
            where TContext : DbContext
            where TKey : IEquatable<TKey>
            where T : class
        {
            if (idSelector == null)
            {
                throw new ArgumentNullException(nameof(idSelector));
            }

            return FindEntityByIdInternalAsync<TEntity, TKey, T>(entities, id, idSelector, cancellationToken);
        }

        public static Task<int> CountEntitiesAsync<TEntity, TContext, TKey>(
            this IEntityStoreMarker<TEntity, TContext, TKey> store,
            IQueryable<TEntity> query, CancellationToken cancellationToken)
            where TEntity : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            cancellationToken.ThrowIfCancellationRequested();
            store.ThrowIfDisposed();
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return query.CountAsync(cancellationToken);
        }

        #region Helpers

        private static async Task<GenericResult> UpdateEntityInternalAsync<TEntity, TContext, TKey>(
            IEntityStoreMarker<TEntity, TContext, TKey> store,
            TEntity entity,
            Action<string> setConcurrencyStamp,
            Func<GenericError> describeConcurrencyFailureError,
            CancellationToken cancellationToken)
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

            if (setConcurrencyStamp != null)
            {
                setConcurrencyStamp(Guid.NewGuid().ToString());
            }
            else if (entity is IConcurrencyStampWiseEntity)
            {
                ((IConcurrencyStampWiseEntity)entity).ConcurrencyStamp = Guid.NewGuid().ToString();
            }

            store.Context.Update(entity);

            try
            {
                await store.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return GenericResult.Failed(describeConcurrencyFailureError == null ?
                    new EntityErrorDescriber().ConcurrencyFailure() :
                    describeConcurrencyFailureError());
            }

            return GenericResult.Success;
        }

        private static async Task<T> FindEntityByIdInternalAsync<TEntity, TKey, T>(
            IQueryable<TEntity> entities,
            string id,
            Expression<Func<TEntity, TKey>> idSelector,
            CancellationToken cancellationToken)
            where TEntity : class
            where TKey : IEquatable<TKey>
            where T : class
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
            if (typeof(T) != typeof(TEntity))
            {
                throw new InvalidOperationException(string.Format(Resources.EntityTypeNotSupported, nameof(T), nameof(TEntity)));
            }

            var key = ConvertIdFromString<TKey>(id);

            if (idSelector != null)
            {
                return await entities.SingleOrDefaultAsync(idSelector, key, cancellationToken) as T;
            }

            if (typeof(IIdWiseEntity<TKey>).IsAssignableFrom(typeof(TEntity)))
            {
                return await entities.SingleOrDefaultAsync(x => ((IIdWiseEntity<TKey>)x).Id.Equals(key), cancellationToken) as T;
            }

            throw new InvalidOperationException();
        }

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
