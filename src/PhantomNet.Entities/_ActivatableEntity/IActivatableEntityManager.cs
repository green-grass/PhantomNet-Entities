using System;
using System.Threading.Tasks;

namespace PhantomNet.Entities
{
    public interface IActivatableEntityManager<TEntity> : IDisposable
        where TEntity : class
    {
        Task<EntityQueryResult<TEntity>> SearchAsync(bool? isActive, string search, int? pageNumber, int? pageSize, string sort, bool reverse);
    }
}
