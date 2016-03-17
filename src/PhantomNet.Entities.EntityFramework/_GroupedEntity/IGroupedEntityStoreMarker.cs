using System;
using Microsoft.Data.Entity;

namespace PhantomNet.Entities.EntityFramework
{
    public interface IGroupedEntityStoreMarker<TEntity, TEntityGroup, TContext, TKey> :
        IEntityStoreMarker<TEntity, TEntityGroup, TContext, TKey>
        where TEntity : class
        where TEntityGroup : class
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    { }
}
