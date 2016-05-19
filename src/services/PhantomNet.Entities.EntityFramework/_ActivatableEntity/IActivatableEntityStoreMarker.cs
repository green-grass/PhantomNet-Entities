using System;
using Microsoft.EntityFrameworkCore;

namespace PhantomNet.Entities.EntityFramework
{
    public interface IActivatableEntityStoreMarker<TEntity, TContext, TKey>
        : IEntityStoreMarker<TEntity, TContext, TKey>
        where TEntity : class
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    { }
}
