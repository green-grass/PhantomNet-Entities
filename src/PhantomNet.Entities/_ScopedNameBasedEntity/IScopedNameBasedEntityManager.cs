using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhantomNet.Entities
{
    public interface IScopedNameBasedEntityManager<TEntity, TEntityScope>
        : IGroupedEntityManager<TEntity, TEntityScope>
        where TEntity : class
        where TEntityScope : class
    {
        Task<TEntity> FindByNameAsync(string name, TEntityScope scope);
    }
}
