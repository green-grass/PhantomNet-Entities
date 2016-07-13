using System;
using Microsoft.EntityFrameworkCore;

namespace PhantomNet.Entities.EntityFrameworkCore
{
    public interface IQueryableNameBasedEntityStoreMarker<TEntity, TContext, TKey>
        : INameBasedEntityStoreMarker<TEntity, TContext, TKey>,
          IQueryableEntityStore<TEntity>
        where TEntity : class
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    { }

    public interface INameBasedEntityStoreMarker<TEntity, TContext, TKey>
        : IEntityStoreMarker<TEntity, TContext, TKey>
        where TEntity : class
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    { }
}
