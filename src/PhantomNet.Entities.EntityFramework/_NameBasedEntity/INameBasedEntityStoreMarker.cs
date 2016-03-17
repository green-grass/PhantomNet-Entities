using System;
using Microsoft.Data.Entity;

namespace PhantomNet.Entities.EntityFramework
{
    public interface INameBasedEntityStoreMarker<TEntity, TContext, TKey> :
        IEntityStoreMarker<TEntity, TContext, TKey>
        where TEntity : class
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    { }
}
