using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using PhantomNet.Entities.EntityMarkers;
#if DOTNET5_4
using System.Reflection;
#endif

namespace PhantomNet.Entities.EntityFramework
{
    public static class MasterDetailsEntityStoreExtensions
    {
        public static Task<ICollection<TEntityDetail>> GetEntityDetailsAsync<TEntity, TEntityDetail, TContext, TKey>(
            this IMasterDetailsEntityStoreMarker<TEntity, TEntityDetail, TContext, TKey> store,
            TEntity entity, CancellationToken cancellationToken,
            Expression<Func<TEntity, ICollection<TEntityDetail>>> detailsSelector)
            where TEntity : class
            where TEntityDetail : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            return GetEntityDetailsAsync(store, entity, cancellationToken, detailsSelector, null);
        }

        public static Task<ICollection<TEntityDetail>> GetEntityDetailsAsync<TEntity, TEntityDetail, TContext, TKey>(
            this IMasterDetailsEntityStoreMarker<TEntity, TEntityDetail, TContext, TKey> store,
            TEntity entity, CancellationToken cancellationToken,
            Expression<Func<TEntity, ICollection<TEntityDetail>>> detailsSelector,
            Func<ICollection<TEntityDetail>> directGetDetails)
            where TEntity : class
            where TEntityDetail : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            cancellationToken.ThrowIfCancellationRequested();
            store.ThrowIfDisposed();
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            ICollection<TEntityDetail> details;
            if (directGetDetails == null && detailsSelector != null)
            {
                details = detailsSelector.Compile()(entity);
            }
            else
            {
                details = directGetDetails();
            }
            return Task.FromResult(details);
        }
    }
}
