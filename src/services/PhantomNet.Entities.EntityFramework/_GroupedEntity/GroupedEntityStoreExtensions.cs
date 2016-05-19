using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace PhantomNet.Entities.EntityFramework
{
    public static class GroupedEntityStoreExtensions
    {
        public static IQueryable<TEntity> FilterEntitiesByGroup<TEntity, TEntityGroup, TContext, TKey>(
            this IGroupedEntityStoreMarker<TEntity, TEntityGroup, TContext, TKey> store,
            IQueryable<TEntity> query, TEntityGroup group,
            Expression<Func<TEntity, TKey>> groupIdSelector)
            where TEntity : class
            where TEntityGroup : class, IIdWiseEntity<TKey>
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            return FilterEntitiesByGroupInternal(query, group, groupIdSelector, null);
        }

        public static IQueryable<TEntity> FilterEntitiesByGroup<TEntity, TEntityGroup, TContext, TKey>(
            this IGroupedEntityStoreMarker<TEntity, TEntityGroup, TContext, TKey> store,
            IQueryable<TEntity> query, TEntityGroup group,
            Expression<Func<TEntity, TKey>> groupIdSelector,
            Expression<Func<TEntityGroup, TKey>> idSelector)
            where TEntity : class
            where TEntityGroup : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            if (idSelector == null)
            {
                throw new ArgumentNullException(nameof(idSelector));
            }

            return FilterEntitiesByGroupInternal(query, group, groupIdSelector, idSelector);
        }

        public static IQueryable<TEntity> FilterEntitiesByGroupInternal<TEntity, TEntityGroup, TKey>(
            IQueryable<TEntity> query, TEntityGroup group,
            Expression<Func<TEntity, TKey>> groupIdSelector,
            Expression<Func<TEntityGroup, TKey>> idSelector)
            where TEntity : class
            where TEntityGroup : class
            where TKey : IEquatable<TKey>
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }
            if (group == null)
            {
                throw new ArgumentNullException(nameof(group));
            }
            if (groupIdSelector == null)
            {
                throw new ArgumentNullException(nameof(groupIdSelector));
            }

            TKey groupId;
            if (idSelector != null)
            {
                groupId = idSelector.Compile().Invoke(group);
            }
            else if (group is IIdWiseEntity<TKey>)
            {
                groupId = ((IIdWiseEntity<TKey>)group).Id;
            }
            else
            {
                throw new InvalidOperationException();
            }

            var param = Expression.Parameter(typeof(TEntity), "x");
            var subEntityIdMember = Expression.Property(param, groupIdSelector.GetPropertyAccess().Name);
            var expression = Expression.Equal(subEntityIdMember, Expression.Constant(groupId, typeof(TKey)));
            var predicate = Expression.Lambda<Func<TEntity, bool>>(expression, param);
            return query.Where(predicate);
        }
    }
}
