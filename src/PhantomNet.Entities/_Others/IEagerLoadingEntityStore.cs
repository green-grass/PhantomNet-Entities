using System;
using System.Threading;
using System.Threading.Tasks;

namespace PhantomNet.Entities
{
    public interface IEagerLoadingEntityStore<TEntity> : IDisposable
        where TEntity : class
    {
        Task EagerLoadAsync(TEntity entity, CancellationToken cancellationToken);
    }
}
