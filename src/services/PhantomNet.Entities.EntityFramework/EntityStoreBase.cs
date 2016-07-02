using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PhantomNet.Entities.EntityFramework
{
    public abstract class EntityStoreBase<TEntity, TContext, TKey>
        where TEntity : class
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    {
        public EntityStoreBase(TContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            Context = context;
        }

        public TContext Context { get; }

        public bool AutoSaveChanges { get; set; } = true;

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            if (AutoSaveChanges)
            {
                await Context.SaveChangesAsync(cancellationToken);
            }
        }

        protected void UpdateManyToMany<TSubEntity, TRelationship>(
            TEntity entity,
            IQueryable<TRelationship> relationships,
            Expression<Func<TEntity, ICollection<TSubEntity>>> subEntitiesSelector,
            Expression<Func<TEntity, TKey>> entityKeySelector,
            Expression<Func<TSubEntity, TKey>> subEntityKeySelector,
            Expression<Func<TRelationship, TKey>> relationshipEntityKeySelector,
            Expression<Func<TRelationship, TKey>> relationshipSubEntityKeySelector)
            where TSubEntity : class
            where TRelationship : class, new()
        {
            var entityId = entityKeySelector.GetPropertyAccess().GetValue(entity);

            var oldIds = relationships.Where(x => relationshipEntityKeySelector.GetPropertyAccess().GetValue(x)
                                                                               .Equals(entityId))
                                      .Select(x => (TKey)relationshipSubEntityKeySelector.GetPropertyAccess().GetValue(x))
                                      .ToList()
                                      .AsQueryable();
            var newIds = ((ICollection<TSubEntity>)subEntitiesSelector.GetPropertyAccess().GetValue(entity))
                                                                      .AsQueryable()
                                                                      .Select(x => (TKey)subEntityKeySelector.GetPropertyAccess().GetValue(x))
                                                                      .ToList()
                                                                      .AsQueryable();

            var removedIds = oldIds.Where(x => !newIds.Contains(x)).ToList();
            var removedRelationships = new List<TRelationship>();
            foreach (var id in removedIds)
            {
                var relationship = new TRelationship();
                relationshipEntityKeySelector.GetPropertyAccess().SetValue(relationship, entityId);
                relationshipSubEntityKeySelector.GetPropertyAccess().SetValue(relationship, id);
                removedRelationships.Add(relationship);
            }

            var addedIds = newIds.Where(x => !oldIds.Contains(x)).ToList();
            var addedRelationships = new List<TRelationship>();
            foreach (var id in addedIds)
            {
                var relationship = new TRelationship();
                relationshipEntityKeySelector.GetPropertyAccess().SetValue(relationship, entityId);
                relationshipSubEntityKeySelector.GetPropertyAccess().SetValue(relationship, id);
                addedRelationships.Add(relationship);
            }

            Context.RemoveRange(removedRelationships);
            Context.AddRange(addedRelationships);
        }

        #region IDisposable Support

        private bool _disposed = false;

        public void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    Context.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
