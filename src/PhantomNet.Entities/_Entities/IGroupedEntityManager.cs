using System;
using System.Threading.Tasks;

namespace PhantomNet.Entities
{
    public interface IGroupedEntityManager<TEntity, TEntityGroup> : IDisposable
        where TEntity : class
        where TEntityGroup : class
    {
        Task<EntityQueryResult<TEntity>> SearchAsync(TEntityGroup group, string search, int? pageNumber, int? pageSize, string sort, bool reverse);
    }
}
