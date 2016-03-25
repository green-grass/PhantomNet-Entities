using System;
using System.Linq.Expressions;
using PhantomNet.Entities.EntityMarkers;

namespace PhantomNet.Entities
{
    public static class TimeTrackedEntityAccessorExtensions
    {
        public static void SetEntityDataCreateDate<TEntity>(
            this ITimeTrackedEntityAccessor<TEntity> accessor,
            TEntity entity, DateTime dataCreateDate)
            where TEntity : class
        {
            SetEntityDataCreateDate(accessor, entity, dataCreateDate, null, null);
        }

        public static void SetEntityDataCreateDate<TEntity>(
            this ITimeTrackedEntityAccessor<TEntity> accessor,
            TEntity entity, DateTime dataCreateDate,
            Expression<Func<TEntity, DateTime>> dataCreateDateSelector,
            Action directSetDataCreateDate)
            where TEntity : class
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (dataCreateDateSelector == null &&
                directSetDataCreateDate == null &&
                entity is ITimeWiseEntity)
            {
                ((ITimeWiseEntity)entity).DataCreateDate = dataCreateDate;
            }
            else if (dataCreateDateSelector != null)
            {
                dataCreateDateSelector.GetPropertyAccess().SetValue(entity, dataCreateDate);
            }
            else if (directSetDataCreateDate != null)
            {
                directSetDataCreateDate();
            }
            else
            {
                throw new ArgumentNullException($"{nameof(dataCreateDateSelector)}, {nameof(directSetDataCreateDate)}");
            }
        }

        public static void SetEntityDataLastModifyDate<TEntity>(
            this ITimeTrackedEntityAccessor<TEntity> accessor,
            TEntity entity, DateTime dataLastModifyDate)
            where TEntity : class
        {
            SetEntityDataLastModifyDate(accessor, entity, dataLastModifyDate, null, null);
        }

        public static void SetEntityDataLastModifyDate<TEntity>(
            this ITimeTrackedEntityAccessor<TEntity> accessor,
            TEntity entity, DateTime dataLastModifyDate,
            Expression<Func<TEntity, DateTime>> dataLastModifyDateSelector,
            Action directSetDataLastModifyDate)
            where TEntity : class
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (dataLastModifyDateSelector == null &&
                directSetDataLastModifyDate == null &&
                entity is ITimeWiseEntity)
            {
                ((ITimeWiseEntity)entity).DataLastModifyDate = dataLastModifyDate;
            }
            else if (dataLastModifyDateSelector != null)
            {
                dataLastModifyDateSelector.GetPropertyAccess().SetValue(entity, dataLastModifyDate);
            }
            else if (directSetDataLastModifyDate != null)
            {
                directSetDataLastModifyDate();
            }
            else
            {
                throw new ArgumentNullException($"{nameof(dataLastModifyDateSelector)}, {nameof(directSetDataLastModifyDate)}");
            }
        }
    }
}
