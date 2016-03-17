using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity;

namespace PhantomNet.Entities.EntityFramework
{
    public interface IEntityStoreMarker<TEntity, TSubEntity, TContext, TKey> :
        IReadOnlyEntityStoreMarker<TEntity, TSubEntity, TContext, TKey>,
        IEntityStoreMarker<TEntity, TContext, TKey>
        where TEntity : class
        where TSubEntity : class
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    { }

    public interface IEntityStoreMarker<TEntity, TContext, TKey> :
        IReadOnlyEntityStoreMarker<TEntity, TContext, TKey>
        where TEntity : class
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    {
        bool AutoSaveChanges { get; set; }

        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
