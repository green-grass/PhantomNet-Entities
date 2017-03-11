using System;
using System.Reflection;

namespace PhantomNet.Entities
{
    public class DefaultTimeTrackedEntityAccessor<TEntity> : ITimeTrackedEntityAccessor<TEntity>
        where TEntity : class
    {
        public void SetDataCreateDate(TEntity entity, DateTime dataCreateDate)
        {
            if (!typeof(TEntity).IsAssignableFrom(typeof(ITimeWiseEntity)))
            {
                // TODO:: Message
                throw new ArgumentException();
            }

            this.SetEntityDataCreateDate(entity, dataCreateDate, null, null);
        }

        public void SetDataLastModifyDate(TEntity entity, DateTime dataLastModifyDate)
        {
            if (!typeof(TEntity).IsAssignableFrom(typeof(ITimeWiseEntity)))
            {
                // TODO:: Message
                throw new ArgumentException();
            }

            this.SetEntityDataLastModifyDate((TEntity)(ITimeWiseEntity)entity, dataLastModifyDate, null, null);
        }
    }
}
