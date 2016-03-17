using System;
using Microsoft.Data.Entity;

namespace PhantomNet.Entities.EntityFramework
{
    public interface IReadOnlyEntityStoreMarker<TEntity, TSubEntity, TContext, TKey> :
        IEntityStoreMarker<TEntity, TContext, TKey>
        where TEntity : class
        where TSubEntity : class
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    { }

    public interface IReadOnlyEntityStoreMarker<TEntity, TContext, TKey>
        where TEntity : class
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    {
        TContext Context { get; }

        void ThrowIfDisposed();
    }
}
