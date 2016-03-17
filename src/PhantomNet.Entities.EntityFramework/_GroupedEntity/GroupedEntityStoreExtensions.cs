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
    public static class GroupedEntityStoreExtensions
    {
        public static IQueryable<TEntity> FilterEntitiesByGroup<TEntity, TEntityGroup, TContext, TKey>(
            this IGroupedEntityStoreMarker<TEntity, TEntityGroup, TContext, TKey> store,
            IQueryable<TEntity> query, TEntityGroup group,
            Expression<Func<TEntity, TKey>> groupIdSelector)
            where TEntity : class
            where TEntityGroup : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            return FilterEntitiesByGroup(store, query, group, groupIdSelector, null);
        }

        public static IQueryable<TEntity> FilterEntitiesByGroup<TEntity, TEntityGroup, TContext, TKey>(
            this IGroupedEntityStoreMarker<TEntity, TEntityGroup, TContext, TKey> store,
            IQueryable<TEntity> query, TEntityGroup group,
            Expression<Func<TEntity, TKey>> groupIdSelector,
            Func<IQueryable<TEntity>> directFilterByGroup)
            where TEntity : class
            where TEntityGroup : class
            where TContext : DbContext
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

            IQueryable<TEntity> entities;
            if (directFilterByGroup == null && groupIdSelector != null
                && group is IIdWiseEntity<TKey>)
            {
                var groupId = ((IIdWiseEntity<TKey>)group).Id;
                var param = Expression.Parameter(typeof(TEntity), "x");
                var subEntityIdMember = Expression.Property(param, groupIdSelector.GetPropertyAccess().Name);
                var expression = Expression.Equal(subEntityIdMember, Expression.Constant(groupId, typeof(TKey)));
                var predicate = Expression.Lambda<Func<TEntity, bool>>(expression, param);
                entities = query.Where(predicate);
            }
            else
            {
                entities = directFilterByGroup();
            }
            return entities;
        }
    }
}
