using System;
using System.Linq;
using Microsoft.Data.Entity;

namespace PhantomNet.Entities.EntityFramework
{
    public abstract class QueryableEntityStoreBase<TEntity, TSubEntity, TContext, TKey> :
        QueryableEntityStoreBase<TEntity, TContext, TKey>
        where TEntity : class
        where TSubEntity : class
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    {
        public QueryableEntityStoreBase(TContext context) : base(context) { }

        public virtual IQueryable<TSubEntity> SubEntities => Context.Set<TSubEntity>();
    }

    public abstract class QueryableEntityStoreBase<TEntity, TContext, TKey> :
        EntityStoreBase<TEntity, TContext, TKey>
        where TEntity : class
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    {
        public QueryableEntityStoreBase(TContext context) : base(context) { }

        public virtual IQueryable<TEntity> Entities => Context.Set<TEntity>();
    }
}
