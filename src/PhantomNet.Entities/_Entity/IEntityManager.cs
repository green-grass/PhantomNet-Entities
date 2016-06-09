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
        Task<GenericResult> CreateAsync(TEntity entity);

        Task<GenericResult> UpdateAsync(TEntity entity);

        Task<GenericResult> DeleteAsync(TEntity entity);

        Task<TEntity> FindByIdAsync(string id);

        Task<EntityQueryResult<TEntity>> SearchAsync(IEntitySearchDescriptor<TEntity> searchDescriptor);
    }
}
