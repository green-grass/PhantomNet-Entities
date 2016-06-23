using System;
using System.Linq.Expressions;

namespace PhantomNet.Entities
{
    public static class GroupedEntityAccessorExtensions
    {
        public static TEntityGroup GetEntityGroup<TEntity, TEntityGroup>(
            this IGroupedEntityAccessor<TEntity, TEntityGroup> accessor,
            TEntity entity,
            Expression<Func<TEntity, TEntityGroup>> groupSelector,
            Func<TEntityGroup> directGetGroup)
            where TEntity : class
            where TEntityGroup : class
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (groupSelector != null)
            {
                return groupSelector.Compile()(entity);
            }
            else if (directGetGroup != null)
            {
                return directGetGroup();
            }

            throw new ArgumentNullException($"{nameof(groupSelector)}, {nameof(directGetGroup)}");
        }

        public static void SetEntityGroup<TEntity, TEntityGroup, TKey>(
            this IGroupedEntityAccessor<TEntity, TEntityGroup> accessor,
            TEntity entity, TEntityGroup group,
            Expression<Func<TEntity, TKey>> groupIdSelector,
            Expression<Func<TEntity, TEntityGroup>> groupSelector,
            Action directSetGroup)
            where TEntity : class
            where TEntityGroup : class
            where TKey : IEquatable<TKey>
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            if (group == null)
            {
                throw new ArgumentNullException(nameof(group));
            }

            if (groupIdSelector != null && groupSelector != null && group is IIdWiseEntity<TKey>)
            {
                groupIdSelector.GetPropertyAccess().SetValue(entity, ((IIdWiseEntity<TKey>)group).Id);
                groupSelector.GetPropertyAccess().SetValue(entity, group);
            }
            else if (directSetGroup != null)
            {
                directSetGroup();
            }
            else
            {
                throw new ArgumentNullException($"{nameof(groupSelector)}, {nameof(directSetGroup)}");
            }
        }
    }
}
