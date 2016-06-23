using System;
using System.Threading;
using System.Threading.Tasks;

namespace PhantomNet.Entities
{
    public interface ICodeBasedEntityStore<TEntity> : IDisposable
        where TEntity : class
    {
        Task<TEntity> FindByCodeAsync(string normalizedCode, CancellationToken cancellationToken);
    }
}
