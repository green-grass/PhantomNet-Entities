using System;
using System.Threading;
using System.Threading.Tasks;

namespace PhantomNet.Entities
{
    public interface INameBasedEntityStore<TEntity> : IDisposable
        where TEntity : class
    {
        Task<TEntity> FindByNameAsync(string normalizedName, CancellationToken cancellationToken);
    }
}
