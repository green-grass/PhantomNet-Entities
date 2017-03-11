using System;
using System.Reflection;

namespace PhantomNet.Entities
{
    public class DefaultActivatableEntityAccessor<TEntity> : IActivatableEntityAccessor<TEntity>
        where TEntity : class
    {
        public bool GetIsActive(TEntity entity)
        {
            if (!typeof(TEntity).IsAssignableFrom(typeof(IIsActiveWiseEntity)))
            {
                // TODO:: Message
                throw new ArgumentException();
            }

            return this.GetEntityIsActive(entity, null, null);
        }
    }
}
