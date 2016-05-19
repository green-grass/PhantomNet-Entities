using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PhantomNet.Entities.EntityFramework
{
    public static class QueryableScopedNameBasedEntityStoreExtensions
    {
        public static Task<TEntity> FindEntityByNameAsync<TEntity, TEntityScope, TContext, TKey>(
            this IQueryableScopedNameBasedEntityStoreMarker<TEntity, TEntityScope, TContext, TKey> store,
            string normalizedName, TEntityScope scope,
            Expression<Func<TEntity, TKey>> scopeIdSelector,
            CancellationToken cancellationToken)
            where TEntity : class
            where TEntityScope : class, IIdWiseEntity<TKey>
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            store.ThrowIfDisposed();
            return store.FindEntityByNameAsync(store.Entities, normalizedName, scope, scopeIdSelector, cancellationToken);
        }

        public static Task<TEntity> FindEntityByNameAsync<TEntity, TEntityScope, TContext, TKey>(
            this IQueryableScopedNameBasedEntityStoreMarker<TEntity, TEntityScope, TContext, TKey> store,
            string normalizedName, TEntityScope scope,
            Expression<Func<TEntity, TKey>> scopeIdSelector,
            Expression<Func<TEntityScope, TKey>> idSelector,
            CancellationToken cancellationToken)
            where TEntity : class
            where TEntityScope : class, IIdWiseEntity<TKey>
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            store.ThrowIfDisposed();
            return store.FindEntityByNameAsync(store.Entities, normalizedName, scope, scopeIdSelector, idSelector, cancellationToken);
        }
    }

    public static class ScopedNameBasedEntityStoreExtensions
    {
        public static Task<TEntity> FindEntityByNameAsync<TEntity, TEntityScope, TContext, TKey>(
            this IScopedNameBasedEntityStoreMarker<TEntity, TEntityScope, TContext, TKey> store,
            IQueryable<TEntity> entities,
            string normalizedName, TEntityScope scope,
            Expression<Func<TEntity, TKey>> scopeIdSelector,
            CancellationToken cancellationToken)
            where TEntity : class
            where TEntityScope : class, IIdWiseEntity<TKey>
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            return FindEntityByNameInternalAsync(entities, normalizedName, scope, scopeIdSelector, null, cancellationToken);
        }

        public static Task<TEntity> FindEntityByNameAsync<TEntity, TEntityScope, TContext, TKey>(
            this IScopedNameBasedEntityStoreMarker<TEntity, TEntityScope, TContext, TKey> store,
            IQueryable<TEntity> entities,
            string normalizedName, TEntityScope scope,
            Expression<Func<TEntity, TKey>> scopeIdSelector,
            Expression<Func<TEntityScope, TKey>> idSelector,
            CancellationToken cancellationToken)
            where TEntity : class
            where TEntityScope : class, IIdWiseEntity<TKey>
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            if (idSelector == null)
            {
                throw new ArgumentNullException(nameof(idSelector));
            }

            return FindEntityByNameInternalAsync(entities, normalizedName, scope, scopeIdSelector, idSelector, cancellationToken);
        }

        public static Task<TEntity> FindEntityByNameInternalAsync<TEntity, TEntityScope, TKey>(
            IQueryable<TEntity> entities,
            string normalizedName, TEntityScope scope,
            Expression<Func<TEntity, TKey>> scopeIdSelector,
            Expression<Func<TEntityScope, TKey>> idSelector,
            CancellationToken cancellationToken)
            where TEntity : class
            where TEntityScope : class
            where TKey : IEquatable<TKey>
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (normalizedName == null)
            {
                throw new ArgumentNullException(nameof(normalizedName));
            }
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }
            if (scopeIdSelector == null)
            {
                throw new ArgumentNullException(nameof(scopeIdSelector));
            }

            TKey scopeId;
            if (idSelector != null)
            {
                scopeId = idSelector.Compile().Invoke(scope);
            }
            else if (scope is IIdWiseEntity<TKey>)
            {
                scopeId = ((IIdWiseEntity<TKey>)scope).Id;
            }
            else
            {
                throw new InvalidOperationException();
            }

            var param = Expression.Parameter(typeof(TEntity), "x");
            var scopeIdMember = Expression.Property(param, scopeIdSelector.GetPropertyAccess().Name);
            var scopeIdExpression = Expression.Equal(scopeIdMember, Expression.Constant(scopeId, typeof(TKey)));
            var normalizedNameMember = Expression.Property(param, nameof(INameWiseEntity.NormalizedName));
            var normalizedNameExpression = Expression.Equal(normalizedNameMember, Expression.Constant(normalizedName));
            var expression = Expression.AndAlso(scopeIdExpression, normalizedNameExpression);
            var predicate = Expression.Lambda<Func<TEntity, bool>>(expression, param);
            return entities.SingleOrDefaultAsync(predicate, cancellationToken);
        }
    }
}
