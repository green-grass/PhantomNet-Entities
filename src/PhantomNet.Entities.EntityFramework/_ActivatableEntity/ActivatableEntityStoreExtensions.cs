using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Data.Entity;
using PhantomNet.Entities.EntityMarkers;
#if DOTNET5_4
using System.Reflection;
#endif

namespace PhantomNet.Entities.EntityFramework
{
    public static class ActivatableEntityStoreExtensions
    {
        public static IQueryable<TEntity> FilterEntitiesByIsActive<TEntity, TContext, TKey>(
            this IActivatableEntityStoreMarker<TEntity, TContext, TKey> store,
            IQueryable<TEntity> query, bool? isActive)
            where TEntity : class, IIsActiveWiseEntity
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            return FilterEntitiesByIsActiveInternal(query, isActive, null);
        }

        public static IQueryable<TEntity> FilterEntitiesByIsActive<TEntity, TContext, TKey>(
            this IActivatableEntityStoreMarker<TEntity, TContext, TKey> store,
            IQueryable<TEntity> query, bool? isActive,
            Expression<Func<TEntity, bool>> isActiveSelector)
            where TEntity : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            if (isActiveSelector == null)
            {
                throw new ArgumentNullException(nameof(isActiveSelector));
            }

            return FilterEntitiesByIsActiveInternal(query, isActive, isActiveSelector);
        }

        private static IQueryable<TEntity> FilterEntitiesByIsActiveInternal<TEntity>(
            IQueryable<TEntity> query, bool? isActive,
            Expression<Func<TEntity, bool>> isActiveSelector)
            where TEntity : class
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (!isActive.HasValue)
            {
                return query;
            }

            if (isActiveSelector != null)
            {
                return query.Where(x => isActiveSelector.Compile().Invoke(x) == isActive.Value);
            }

            if (typeof(IIsActiveWiseEntity).IsAssignableFrom(typeof(TEntity)))
            {
                return query.Where(x => ((IIsActiveWiseEntity)x).IsActive == isActive.Value);
            }

            throw new InvalidOperationException();
        }
    }
}
