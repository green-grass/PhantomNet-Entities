using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PhantomNet.Entities
{
    public interface IExplicitLoadingEntityStore<TEntity> : IDisposable
        where TEntity : class
    {
        Task ExplicitLoadAsync(IQueryable<TEntity> entities, CancellationToken cancellationToken);

        Task ExplicitLoadAsync(TEntity entity, CancellationToken cancellationToken);
    }
}
