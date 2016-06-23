using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PhantomNet.Entities
{
    public interface IGroupedEntityStore<TEntity, TEntityGroup> : IDisposable
        where TEntity : class
        where TEntityGroup : class
    {
        Task<IEnumerable<TEntityGroup>> GetAllGroupsAsync(CancellationToken cancellationToken);

        Task<IEnumerable<TEntityGroup>> GetGroupsWithEntitiesAsync(CancellationToken cancellationToken);
    }
}
