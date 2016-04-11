using System;
using Microsoft.Data.Entity;

namespace PhantomNet.Entities.EntityFramework
{
    public interface IQueryableNameBasedEntityStoreMarker<TEntity, TContext, TKey> :
        INameBasedEntityStoreMarker<TEntity, TContext, TKey>,
        IQueryableEntityStore<TEntity>
        where TEntity : class
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    { }

    public interface INameBasedEntityStoreMarker<TEntity, TContext, TKey> :
        IEntityStoreMarker<TEntity, TContext, TKey>
        where TEntity : class
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    { }
}
