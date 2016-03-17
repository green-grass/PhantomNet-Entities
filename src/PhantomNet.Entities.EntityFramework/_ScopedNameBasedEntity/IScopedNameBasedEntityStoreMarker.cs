using System;
using Microsoft.Data.Entity;

namespace PhantomNet.Entities.EntityFramework
{
    public interface IScopedNameBasedEntityStoreMarker<TEntity, TEntityScope, TContext, TKey> :
        IEntityStoreMarker<TEntity, TEntityScope, TContext, TKey>
        where TEntity : class
        where TEntityScope : class
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    { }
}
