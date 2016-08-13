using System;
using System.Linq.Expressions;

namespace PhantomNet.Entities
{
    public static class ActivatableEntityAccessorExtensions
    {
        public static bool GetEntityIsActive<TEntity>(
            this IActivatableEntityAccessor<TEntity> accessor,
            TEntity entity)
            where TEntity : class, ICodeWiseEntity
        {
            return GetEntityIsActive(accessor, entity, null, null);
        }

        public static bool GetEntityIsActive<TEntity>(
            this IActivatableEntityAccessor<TEntity> accessor,
            TEntity entity,
            Expression<Func<TEntity, bool>> isActiveSelector,
            Func<bool> directGetIsActive)
            where TEntity : class
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (isActiveSelector == null &&
                directGetIsActive == null &&
                entity is IIsActiveWiseEntity)
            {
                return ((IIsActiveWiseEntity)entity).IsActive;
            }
            else if (isActiveSelector != null)
            {
                return isActiveSelector.Compile()(entity);
            }
            else if (directGetIsActive != null)
            {
                return directGetIsActive();
            }

            throw new ArgumentNullException($"{nameof(isActiveSelector)}, {nameof(directGetIsActive)}");
        }
    }
}
