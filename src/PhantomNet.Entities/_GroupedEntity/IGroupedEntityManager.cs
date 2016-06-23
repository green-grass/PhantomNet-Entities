using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhantomNet.Entities
{
    public interface IGroupedEntityManager<TEntity, TEntityGroup> : IDisposable
        where TEntity : class
        where TEntityGroup : class
    {
        Task<IEnumerable<TEntityGroup>> GetAllGroupsAsync();

        Task<IEnumerable<TEntityGroup>> GetGroupsWithEntitiesAsync();
    }
}
