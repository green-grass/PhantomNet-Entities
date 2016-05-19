using System;
using Microsoft.EntityFrameworkCore;

namespace PhantomNet.Entities.EntityFramework
{
    public interface IQueryableScopedNameBasedEntityStoreMarker<TEntity, TEntityScope, TContext, TKey>
        : IScopedNameBasedEntityStoreMarker<TEntity, TEntityScope, TContext, TKey>,
          IQueryableEntityStoreMarker<TEntity, TEntityScope, TContext, TKey>
        where TEntity : class
        where TEntityScope : class
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    { }

    public interface IScopedNameBasedEntityStoreMarker<TEntity, TEntityScope, TContext, TKey>
        : IEntityStoreMarker<TEntity, TEntityScope, TContext, TKey>
        where TEntity : class
        where TEntityScope : class
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    { }
}
