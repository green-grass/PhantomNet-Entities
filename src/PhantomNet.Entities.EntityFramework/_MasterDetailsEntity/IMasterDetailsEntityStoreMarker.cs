using System;
using Microsoft.Data.Entity;

namespace PhantomNet.Entities.EntityFramework
{
    public interface IMasterDetailsEntityStoreMarker<TEntity, TEntityDetail, TContext, TKey> :
        IEntityStoreMarker<TEntity, TEntityDetail, TContext, TKey>
        where TEntity : class
        where TEntityDetail : class
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    { }
}
