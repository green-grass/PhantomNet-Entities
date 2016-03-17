using System;
using System.Threading.Tasks;

namespace PhantomNet.Entities
{
    public interface IEntityManager<TEntity, TSubEntity> :
        IReadOnlyEntityManager<TEntity, TSubEntity>,
        IEntityManager<TEntity>
        where TEntity : class
        where TSubEntity : class
    { }

    public interface IEntityManager<TEntity> : IReadOnlyEntityManager<TEntity>
        where TEntity : class
    {
        Task<EntityResult> CreateAsync(TEntity entity);

        Task<EntityResult> UpdateAsync(TEntity entity);

        Task<EntityResult> DeleteAsync(TEntity entity);

        Task<string> GetIdAsync(TEntity entity);
    }

    public interface IReadOnlyEntityManager<TEntity, TSubEntity> : IReadOnlyEntityManager<TEntity>
        where TEntity : class
        where TSubEntity : class
    { }

    public interface IReadOnlyEntityManager<TEntity> : IDisposable
        where TEntity : class
    {
        Task<TEntity> FindByIdAsync(string id);

        Task<EntityQueryResult<TEntity>> SearchAsync(string search, int? pageNumber, int? pageSize, string sort, bool reverse);
    }
}
