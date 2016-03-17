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
    public static class ScopedNameBasedEntityStoreExtensions
    {
        public static Task<TEntity> FindEntityByNameAsync<TEntity, TEntityScope, TContext, TKey>(
            this IScopedNameBasedEntityStoreMarker<TEntity, TEntityScope, TContext, TKey> store,
            string normalizedName, TEntityScope scope, CancellationToken cancellationToken,
            Expression<Func<TEntity, TKey>> scopeIdSelector)
            where TEntity : class, INameWiseEntity
            where TEntityScope : class, IIdWiseEntity<TKey>
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            return FindEntityByNameAsync(store, normalizedName, scope, cancellationToken, scopeIdSelector, null);
        }

        public static Task<TEntity> FindEntityByNameAsync<TEntity, TEntityScope, TContext, TKey>(
            this IScopedNameBasedEntityStoreMarker<TEntity, TEntityScope, TContext, TKey> store,
            string normalizedName, TEntityScope scope, CancellationToken cancellationToken,
            Expression<Func<TEntity, TKey>> scopeIdSelector,
            Func<Task<TEntity>> directFindByNameAsync)
            where TEntity : class
            where TEntityScope : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            cancellationToken.ThrowIfCancellationRequested();
            store.ThrowIfDisposed();
            if (normalizedName == null)
            {
                throw new ArgumentNullException(nameof(normalizedName));
            }
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            if (directFindByNameAsync == null && scopeIdSelector != null
                && typeof(INameWiseEntity).IsAssignableFrom(typeof(TEntity))
                && scope is IIdWiseEntity<TKey>
                && store is IQueryableEntityStore<TEntity>)
            {
                var scopeId = ((IIdWiseEntity<TKey>)scope).Id;
                var param = Expression.Parameter(typeof(TEntity), "x");
                var scopeIdMember = Expression.Property(param, scopeIdSelector.GetPropertyAccess().Name);
                var scopeIdExpression = Expression.Equal(scopeIdMember, Expression.Constant(scopeId, typeof(TKey)));
                var normalizedNameMember = Expression.Property(param, nameof(INameWiseEntity.NormalizedName));
                var normalizedNameExpression = Expression.Equal(normalizedNameMember, Expression.Constant(normalizedName));
                var expression = Expression.AndAlso(scopeIdExpression, normalizedNameExpression);
                var predicate = Expression.Lambda<Func<TEntity, bool>>(expression, param);
                return ((IQueryableEntityStore<TEntity>)store).Entities.SingleOrDefaultAsync(predicate, cancellationToken);
            }
            else
            {
                return directFindByNameAsync();
            }
        }

        public static Task<string> GetEntityNameAsync<TEntity, TEntityScope, TContext, TKey>(
            this IScopedNameBasedEntityStoreMarker<TEntity, TEntityScope, TContext, TKey> store,
            TEntity entity, CancellationToken cancellationToken)
            where TEntity : class, INameWiseEntity
            where TEntityScope : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            return GetEntityNameAsync(store, entity, cancellationToken, null);
        }

        public static Task<string> GetEntityNameAsync<TEntity, TEntityScope, TContext, TKey>(
            this IScopedNameBasedEntityStoreMarker<TEntity, TEntityScope, TContext, TKey> store,
            TEntity entity, CancellationToken cancellationToken,
            Func<string> directGetName)
            where TEntity : class
            where TEntityScope : class
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

        public static Task SetEntityNameAsync<TEntity, TEntityScope, TContext, TKey>(
            this IScopedNameBasedEntityStoreMarker<TEntity, TEntityScope, TContext, TKey> store,
            TEntity entity, string name, CancellationToken cancellationToken)
            where TEntity : class, INameWiseEntity
            where TEntityScope : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            return SetEntityNameAsync(store, entity, name, cancellationToken, null);
        }

        public static Task SetEntityNameAsync<TEntity, TEntityScope, TContext, TKey>(
            this IScopedNameBasedEntityStoreMarker<TEntity, TEntityScope, TContext, TKey> store,
            TEntity entity, string name, CancellationToken cancellationToken,
            Action directSetName)
            where TEntity : class
            where TEntityScope : class
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

        public static Task SetEntityNormalizedNameAsync<TEntity, TEntityScope, TContext, TKey>(
            this IScopedNameBasedEntityStoreMarker<TEntity, TEntityScope, TContext, TKey> store,
            TEntity entity, string normalizedName, CancellationToken cancellationToken)
            where TEntity : class, INameWiseEntity
            where TEntityScope : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            return SetEntityNormalizedNameAsync(store, entity, normalizedName, cancellationToken, null);
        }

        public static Task SetEntityNormalizedNameAsync<TEntity, TEntityScope, TContext, TKey>(
            this IScopedNameBasedEntityStoreMarker<TEntity, TEntityScope, TContext, TKey> store,
            TEntity entity, string normalizedName, CancellationToken cancellationToken,
            Action directSetNormalizedName)
            where TEntity : class
            where TEntityScope : class
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

        public static Task<TEntityScope> GetEntityScopeAsync<TEntity, TEntityScope, TContext, TKey>(
            this IScopedNameBasedEntityStoreMarker<TEntity, TEntityScope, TContext, TKey> store,
            TEntity entity, CancellationToken cancellationToken,
            Expression<Func<TEntity, TEntityScope>> scopeSelector)
            where TEntity : class
            where TEntityScope : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            return GetEntityScopeAsync(store, entity, cancellationToken, scopeSelector, null);
        }

        public static Task<TEntityScope> GetEntityScopeAsync<TEntity, TEntityScope, TContext, TKey>(
            this IScopedNameBasedEntityStoreMarker<TEntity, TEntityScope, TContext, TKey> store,
            TEntity entity, CancellationToken cancellationToken,
            Expression<Func<TEntity, TEntityScope>> scopeSelector,
            Func<TEntityScope> directGetEntityScope)
            where TEntity : class
            where TEntityScope : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            cancellationToken.ThrowIfCancellationRequested();
            store.ThrowIfDisposed();
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            TEntityScope scope;
            if (directGetEntityScope == null && scopeSelector != null)
            {
                scope = scopeSelector.Compile()(entity);
            }
            else
            {
                scope = directGetEntityScope();
            }
            return Task.FromResult(scope);
        }

        public static Task SetEntityScopeAsync<TEntity, TEntityScope, TContext, TKey>(
            this IScopedNameBasedEntityStoreMarker<TEntity, TEntityScope, TContext, TKey> store,
            TEntity entity, TEntityScope scope, CancellationToken cancellationToken,
            Expression<Func<TEntity, TKey>> scopeIdSelector,
            Expression<Func<TEntity, TEntityScope>> scopeSelector)
            where TEntity : class
            where TEntityScope : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            return SetEntityScopeAsync(store, entity, scope, cancellationToken, scopeIdSelector, scopeSelector, null);
        }

        public static Task SetEntityScopeAsync<TEntity, TEntityScope, TContext, TKey>(
            this IScopedNameBasedEntityStoreMarker<TEntity, TEntityScope, TContext, TKey> store,
            TEntity entity, TEntityScope scope, CancellationToken cancellationToken,
            Expression<Func<TEntity, TKey>> scopeIdSelector,
            Expression<Func<TEntity, TEntityScope>> scopeSelector,
            Action directSetEntityScope)
            where TEntity : class
            where TEntityScope : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            cancellationToken.ThrowIfCancellationRequested();
            store.ThrowIfDisposed();
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            if (directSetEntityScope == null && scopeIdSelector != null && scopeSelector != null
                && typeof(IIdWiseEntity<TKey>).IsAssignableFrom(typeof(TEntityScope)))
            {
                scopeIdSelector.GetPropertyAccess().SetValue(entity, ((IIdWiseEntity<TKey>)scope).Id);
                scopeSelector.GetPropertyAccess().SetValue(entity, scope);
            }
            else
            {
                directSetEntityScope();
            }
            return Task.FromResult(0);
        }
    }
}
