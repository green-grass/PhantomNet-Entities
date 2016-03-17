using System;
using System.Threading;
using System.Threading.Tasks;

namespace PhantomNet.Entities
{
    public interface ITimeTrackedEntityStore<TEntity> : IDisposable
        where TEntity : class
    {
        Task<TEntity> FindLatestAsync(CancellationToken cancellationToken);

        Task SetDataCreateDateAsync(TEntity entity, DateTime date, CancellationToken cancellationToken);

        Task SetDataLastModifyDateAsync(TEntity entity, DateTime date, CancellationToken cancellationToken);
    }
}
