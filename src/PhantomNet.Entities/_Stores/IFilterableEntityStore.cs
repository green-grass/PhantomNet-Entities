using System;
using System.Linq;

namespace PhantomNet.Entities
{
    public interface IFilterableEntityStore<TEntity> : IDisposable
        where TEntity : class
    {
        IQueryable<TEntity> PreFilter(IQueryable<TEntity> entities, IEntitySearchDescriptor<TEntity> searchDescriptor);

        IQueryable<TEntity> Filter(IQueryable<TEntity> entities, IEntitySearchDescriptor<TEntity> searchDescriptor);
    }
}
