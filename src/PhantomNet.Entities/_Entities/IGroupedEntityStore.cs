using System;
using System.Linq;

namespace PhantomNet.Entities
{
    public interface IGroupedEntityStore<TEntity, TEntityGroup> : IDisposable
        where TEntity : class
        where TEntityGroup : class
    {
        IQueryable<TEntity> FilterByGroup(IQueryable<TEntity> query, TEntityGroup group);
    }
}
