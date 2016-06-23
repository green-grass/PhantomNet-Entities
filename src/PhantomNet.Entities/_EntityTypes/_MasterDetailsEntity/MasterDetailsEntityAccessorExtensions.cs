using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PhantomNet.Entities
{
    public static class MasterDetailsEntityAccessorExtensions
    {
        public static ICollection<TEntityDetail> GetEntityDetails<TEntity, TEntityDetail>(
            this IMasterDetailsEntityAccessor<TEntity, TEntityDetail> accessor,
            TEntity entity)
            where TEntity : class, IDetailsWiseEntity<TEntityDetail>
            where TEntityDetail : class
        {
            return GetEntityDetails(accessor, entity, null, null);
        }

        public static ICollection<TEntityDetail> GetEntityDetails<TEntity, TEntityDetail>(
            this IMasterDetailsEntityAccessor<TEntity, TEntityDetail> accessor,
            TEntity entity,
            Expression<Func<TEntity, ICollection<TEntityDetail>>> nameSelector,
            Func<ICollection<TEntityDetail>> directGetDetails)
            where TEntity : class
            where TEntityDetail : class
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (nameSelector == null &&
                directGetDetails == null &&
                entity is IDetailsWiseEntity<TEntityDetail>)
            {
                return ((IDetailsWiseEntity<TEntityDetail>)entity).Details;
            }
            else if (nameSelector != null)
            {
                return nameSelector.Compile()(entity);
            }
            else if (directGetDetails != null)
            {
                return directGetDetails();
            }

            throw new ArgumentNullException($"{nameof(nameSelector)}, {nameof(directGetDetails)}");
        }
    }
}
