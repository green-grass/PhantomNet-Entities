using System;
using System.Threading;
using System.Threading.Tasks;

namespace PhantomNet.Entities
{
    public interface INameBasedEntityStore<TEntity> : IReadOnlyNameBasedEntityStore<TEntity>
        where TEntity : class
    {
        Task<string> GetNameAsync(TEntity entity, CancellationToken cancellationToken);

        Task SetNameAsync(TEntity entity, string name, CancellationToken cancellationToken);

        Task SetNormalizedNameAsync(TEntity entity, string normalizedName, CancellationToken cancellationToken);
    }

    public interface IReadOnlyNameBasedEntityStore<TEntity> : IDisposable
        where TEntity : class
    {
        Task<TEntity> FindByNameAsync(string normalizedName, CancellationToken cancellationToken);
    }
}
