using System;
using System.Threading;
using System.Threading.Tasks;

namespace PhantomNet.Entities
{
    public interface IScopedNameBasedEntityStore<TEntity, TEntityScope> : IDisposable
        where TEntity : class
        where TEntityScope : class
    {
        Task<TEntity> FindByNameAsync(string normalizedName, TEntityScope scope, CancellationToken cancellationToken);

        Task<string> GetNameAsync(TEntity entity, CancellationToken cancellationToken);

        Task SetNameAsync(TEntity entity, string name, CancellationToken cancellationToken);

        Task SetNormalizedNameAsync(TEntity entity, string normalizedName, CancellationToken cancellationToken);

        Task<TEntityScope> GetScopeAsync(TEntity entity, CancellationToken cancellationToken);

        Task SetScopeAsync(TEntity entity, TEntityScope scope, CancellationToken cancellationToken);
    }
}
