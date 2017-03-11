using System;
using System.Reflection;

namespace PhantomNet.Entities
{
    public class DefaultNameBasedEntityAccessor<TEntity> : INameBasedEntityAccessor<TEntity>
        where TEntity : class
    {
        public string GetName(TEntity entity)
        {
            if (!typeof(TEntity).IsAssignableFrom(typeof(INameWiseEntity)))
            {
                // TODO:: Message
                throw new ArgumentException();
            }

            return this.GetEntityName(entity, null, null);
        }

        public void SetName(TEntity entity, string name)
        {
            if (!typeof(TEntity).IsAssignableFrom(typeof(INameWiseEntity)))
            {
                // TODO:: Message
                throw new ArgumentException();
            }

            this.SetEntityName(entity, name, null, null);
        }

        public void SetNormalizedName(TEntity entity, string normalizedName)
        {
            if (!typeof(TEntity).IsAssignableFrom(typeof(INameWiseEntity)))
            {
                // TODO:: Message
                throw new ArgumentException();
            }

            this.SetEntityNormalizedName(entity, normalizedName, null, null);
        }
    }
}
