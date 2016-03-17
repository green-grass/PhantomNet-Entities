using System;
using Microsoft.Data.Entity;

namespace PhantomNet.Entities.EntityFramework
{
    public interface IReadOnlyCodeBasedEntityStoreMarker<TEntity, TContext, TKey> :
        IReadOnlyEntityStoreMarker<TEntity, TContext, TKey>
        where TEntity : class
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    { }
}
