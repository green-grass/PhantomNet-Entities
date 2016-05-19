using System;
using Microsoft.EntityFrameworkCore;

namespace PhantomNet.Entities.EntityFramework
{
    public interface IQueryableTimeTrackedEntityStoreMarker<TEntity, TContext, TKey>
        : ITimeTrackedEntityStoreMarker<TEntity, TContext, TKey>,
          IQueryableEntityStore<TEntity>
        where TEntity : class
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    { }

    public interface ITimeTrackedEntityStoreMarker<TEntity, TContext, TKey>
        : IEntityStoreMarker<TEntity, TContext, TKey>
        where TEntity : class
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    { }
}
