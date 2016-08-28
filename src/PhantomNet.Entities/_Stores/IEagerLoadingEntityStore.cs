using System;
using System.Linq;

namespace PhantomNet.Entities
{
    public interface IEagerLoadingEntityStore<TEntity> : IDisposable
        where TEntity : class
    {
        IQueryable<TEntity> EagerLoad(IQueryable<TEntity> entities);
    }
}
