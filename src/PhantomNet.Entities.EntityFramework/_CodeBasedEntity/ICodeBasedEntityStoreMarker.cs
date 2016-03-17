using System;
using Microsoft.Data.Entity;

namespace PhantomNet.Entities.EntityFramework
{
    public interface ICodeBasedEntityStoreMarker<TEntity, TContext, TKey> :
        IReadOnlyCodeBasedEntityStoreMarker<TEntity, TContext, TKey>
        where TEntity : class
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    { }
}
