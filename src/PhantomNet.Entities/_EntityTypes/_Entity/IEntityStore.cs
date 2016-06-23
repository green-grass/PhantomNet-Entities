using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PhantomNet.Entities
{
    public interface IEntityStore<TEntity, TSubEntity>
        : IEntityStore<TEntity>
        where TEntity : class
        where TSubEntity : class
    { }

    public interface IEntityStore<TEntity> : IDisposable
        where TEntity : class
    {
        Task<GenericResult> CreateAsync(TEntity entity, CancellationToken cancellationToken);

        Task<GenericResult> UpdateAsync(TEntity entity, CancellationToken cancellationToken);

        Task<GenericResult> DeleteAsync(TEntity entity, CancellationToken cancellationToken);

        Task<T> FindByIdAsync<T>(string id, CancellationToken cancellationToken)
            where T : class;

        Task<int> CountAsync(IQueryable<TEntity> entities, CancellationToken cancellationToken);
    }
}
