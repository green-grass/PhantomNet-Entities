using System;
using System.Collections.Generic;
using System.Reflection;

namespace PhantomNet.Entities
{
    public class DefaultMasterDetailsEntityAccessor<TEntity, TEntityDetail> : IMasterDetailsEntityAccessor<TEntity, TEntityDetail>
        where TEntity : class
        where TEntityDetail : class
    {
        public ICollection<TEntityDetail> GetDetails(TEntity entity)
        {
            if (!typeof(TEntity).IsAssignableFrom(typeof(IDetailsWiseEntity<TEntityDetail>)))
            {
                // TODO:: Message
                throw new ArgumentException();
            }

            return this.GetEntityDetails(entity, null, null);
        }
    }
}
