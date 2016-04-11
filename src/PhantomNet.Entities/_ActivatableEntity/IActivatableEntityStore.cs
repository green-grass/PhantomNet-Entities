using System;
using System.Linq;

namespace PhantomNet.Entities
{
    public interface IActivatableEntityStore<TEntity> : IDisposable
        where TEntity : class
    {
        IQueryable<TEntity> FilterByIsActive(IQueryable<TEntity> query, bool? isActive);
    }
}
