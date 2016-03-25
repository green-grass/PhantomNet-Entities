using System.Collections.Generic;

namespace PhantomNet.Entities
{
    public interface IMasterDetailsEntityAccessor<TEntity, TEntityDetail>
        where TEntity : class
        where TEntityDetail : class
    {
        ICollection<TEntityDetail> GetDetails(TEntity entity);
    }
}
