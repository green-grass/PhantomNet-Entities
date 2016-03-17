using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PhantomNet.Entities
{
    public interface IEntityStore<TEntity, TSubEntity> :
        IReadOnlyEntityStore<TEntity, TSubEntity>,
        IEntityStore<TEntity>
        where TEntity : class
        where TSubEntity : class
    {
        Task<string> GetIdAsync(TSubEntity subEntity, CancellationToken cancellationToken);
    }

    public interface IEntityStore<TEntity> : IReadOnlyEntityStore<TEntity>
        where TEntity : class
    {
        Task<EntityResult> CreateAsync(TEntity entity, CancellationToken cancellationToken);

        Task<EntityResult> UpdateAsync(TEntity entity, CancellationToken cancellationToken);

        Task<EntityResult> DeleteAsync(TEntity entity, CancellationToken cancellationToken);

        Task<string> GetIdAsync(TEntity entity, CancellationToken cancellationToken);
    }

    public interface IReadOnlyEntityStore<TEntity, TSubEntity> : IReadOnlyEntityStore<TEntity>
        where TEntity : class
        where TSubEntity : class
    { }

    public interface IReadOnlyEntityStore<TEntity> : IDisposable
        where TEntity : class
    {
        Task<T> FindByIdAsync<T>(string id, CancellationToken cancellationToken)
            where T : class;

        IQueryable<TEntity> Filter(IQueryable<TEntity> query, string filter);

        IQueryable<TEntity> PreSort(IQueryable<TEntity> query);

        IQueryable<TEntity> DefaultSort(IQueryable<TEntity> query);

        Task<int> CountAsync(IQueryable<TEntity> entities, CancellationToken cancellationToken);
    }
}
