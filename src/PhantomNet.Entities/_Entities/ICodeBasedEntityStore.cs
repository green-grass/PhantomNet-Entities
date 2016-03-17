using System;
using System.Threading;
using System.Threading.Tasks;

namespace PhantomNet.Entities
{
    public interface ICodeBasedEntityStore<TEntity> : IReadOnlyCodeBasedEntityStore<TEntity>
        where TEntity : class
    {
        Task<string> GetCodeAsync(TEntity entity, CancellationToken cancellationToken);

        Task SetCodeAsync(TEntity entity, string code, CancellationToken cancellationToken);
    }

    public interface IReadOnlyCodeBasedEntityStore<TEntity> : IDisposable
        where TEntity : class
    {
        Task<TEntity> FindByCodeAsync(string normalizedCode, CancellationToken cancellationToken);
    }
}
