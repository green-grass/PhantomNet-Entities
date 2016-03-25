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
    }
}
