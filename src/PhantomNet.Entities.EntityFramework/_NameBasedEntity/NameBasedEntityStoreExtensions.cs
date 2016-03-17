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

        public static Task<string> GetEntityNameAsync<TEntity, TContext, TKey>(
            this INameBasedEntityStoreMarker<TEntity, TContext, TKey> store,
            TEntity entity, CancellationToken cancellationToken)
            where TEntity : class, INameWiseEntity
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            return GetEntityNameAsync(store, entity, cancellationToken, null);
        }

        public static Task<string> GetEntityNameAsync<TEntity, TContext, TKey>(
            this INameBasedEntityStoreMarker<TEntity, TContext, TKey> store,
            TEntity entity, CancellationToken cancellationToken,
            Func<string> directGetName)
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

            if (directGetName == null && entity is INameWiseEntity)
            {
                return Task.FromResult(((INameWiseEntity)entity).Name);
            }
            else
            {
                return Task.FromResult(directGetName());
            }
        }

        public static Task SetEntityNameAsync<TEntity, TContext, TKey>(
            this INameBasedEntityStoreMarker<TEntity, TContext, TKey> store,
            TEntity entity, string name, CancellationToken cancellationToken)
            where TEntity : class, INameWiseEntity
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            return SetEntityNameAsync(store, entity, name, cancellationToken, null);
        }

        public static Task SetEntityNameAsync<TEntity, TContext, TKey>(
            this INameBasedEntityStoreMarker<TEntity, TContext, TKey> store,
            TEntity entity, string name, CancellationToken cancellationToken,
            Action directSetName)
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

            if (directSetName == null && entity is INameWiseEntity)
            {
                ((INameWiseEntity)entity).Name = name;
            }
            else
            {
                directSetName();
            }
            return Task.FromResult(0);
        }

        public static Task SetEntityNormalizedNameAsync<TEntity, TContext, TKey>(
            this INameBasedEntityStoreMarker<TEntity, TContext, TKey> store,
            TEntity entity, string normalizedName, CancellationToken cancellationToken)
            where TEntity : class, INameWiseEntity
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            return SetEntityNormalizedNameAsync(store, entity, normalizedName, cancellationToken, null);
        }

        public static Task SetEntityNormalizedNameAsync<TEntity, TContext, TKey>(
            this INameBasedEntityStoreMarker<TEntity, TContext, TKey> store,
            TEntity entity, string normalizedName, CancellationToken cancellationToken,
            Action directSetNormalizedName)
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

            if (directSetNormalizedName == null && entity is INameWiseEntity)
            {
                ((INameWiseEntity)entity).NormalizedName = normalizedName;
            }
            else
            {
                directSetNormalizedName();
            }
            return Task.FromResult(0);
        }
    }
}
