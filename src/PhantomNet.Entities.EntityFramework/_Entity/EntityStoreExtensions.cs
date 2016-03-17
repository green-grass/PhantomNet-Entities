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
    public static partial class EntityStoreExtensions
    {
        public static Task<string> GetSubEntityIdAsync<TEntity, TSubEntity, TContext, TKey>(
            this IEntityStoreMarker<TEntity, TSubEntity, TContext, TKey> store,
            TSubEntity subEntity, CancellationToken cancellationToken)
            where TEntity : class
            where TSubEntity : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            return GetSubEntityIdAsync(store, subEntity, cancellationToken, null);
        }

        public static Task<string> GetSubEntityIdAsync<TEntity, TSubEntity, TContext, TKey>(
            this IEntityStoreMarker<TEntity, TSubEntity, TContext, TKey> store,
            TSubEntity subEntity, CancellationToken cancellationToken,
            Func<TKey> directGetId)
            where TEntity : class
            where TSubEntity : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            cancellationToken.ThrowIfCancellationRequested();
            store.ThrowIfDisposed();
            if (subEntity == null)
            {
                throw new ArgumentNullException(nameof(subEntity));
            }

            TKey id;
            if (directGetId == null && subEntity is IIdWiseEntity<TKey>)
            {
                id = ((IIdWiseEntity<TKey>)subEntity).Id;
            }
            else
            {
                id = directGetId();
            }
            return Task.FromResult(ConvertIdToString(id));
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

        public static Task<string> GetEntityIdAsync<TEntity, TContext, TKey>(
            this IEntityStoreMarker<TEntity, TContext, TKey> store,
            TEntity entity, CancellationToken cancellationToken)
            where TEntity : class, IIdWiseEntity<TKey>
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            return GetEntityIdAsync(store, entity, cancellationToken, null);
        }

        public static Task<string> GetEntityIdAsync<TEntity, TContext, TKey>(
            this IEntityStoreMarker<TEntity, TContext, TKey> store,
            TEntity entity, CancellationToken cancellationToken,
            Func<TKey> directGetId)
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

            TKey id;
            if (directGetId == null && entity is IIdWiseEntity<TKey>)
            {
                id = ((IIdWiseEntity<TKey>)entity).Id;
            }
            else
            {
                id = directGetId();
            }
            return Task.FromResult(ConvertIdToString(id));
        }

        #region Helpers

        private static string ConvertIdToString<TKey>(TKey id)
            where TKey : IEquatable<TKey>
        {
            if (id.Equals(default(TKey)))
            {
                return null;
            }

            return id.ToString();
        }

        #endregion
    }
}
