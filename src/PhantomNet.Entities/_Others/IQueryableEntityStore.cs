using System;
using System.Linq;

namespace PhantomNet.Entities
{
    public interface IQueryableEntityStore<TEntity, TSubEntity> :
        IQueryableEntityStore<TEntity>
        where TEntity : class
        where TSubEntity : class
    {
        IQueryable<TSubEntity> SubEntities { get; }
    }

    public interface IQueryableEntityStore<TEntity> : IDisposable
        where TEntity : class
    {
        IQueryable<TEntity> Entities { get; }
    }
}
