using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PhantomNet.Entities
{
    public interface IExplicitLoadingEntityStore<TEntity> : IDisposable
        where TEntity : class
    {
        Task ExplicitLoadAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken);

        Task ExplicitLoadAsync(TEntity entity, CancellationToken cancellationToken);
    }
}
