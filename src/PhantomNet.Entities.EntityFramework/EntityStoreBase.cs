using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity;

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
