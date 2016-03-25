using System;
using System.Threading.Tasks;

namespace PhantomNet.Entities
{
    public interface IEntityManager<TEntity, TSubEntity> :
        IEntityManager<TEntity>
        where TEntity : class
        where TSubEntity : class
    { }

    public interface IEntityManager<TEntity> : IDisposable
        where TEntity : class
    {
        Task<EntityResult> CreateAsync(TEntity entity);

        Task<EntityResult> UpdateAsync(TEntity entity);

        Task<EntityResult> DeleteAsync(TEntity entity);

        Task<TEntity> FindByIdAsync(string id);

        Task<EntityQueryResult<TEntity>> SearchAsync(string search, int? pageNumber, int? pageSize, string sort, bool reverse);
    }
}
