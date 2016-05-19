using System;
using Microsoft.EntityFrameworkCore;

namespace PhantomNet.Entities.EntityFramework
{
    public interface IQueryableCodeBasedEntityStoreMarker<TEntity, TContext, TKey>
        : ICodeBasedEntityStoreMarker<TEntity, TContext, TKey>,
          IQueryableEntityStore<TEntity>
        where TEntity : class
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    { }

    public interface ICodeBasedEntityStoreMarker<TEntity, TContext, TKey>
        : IEntityStoreMarker<TEntity, TContext, TKey>
        where TEntity : class
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    { }
}
