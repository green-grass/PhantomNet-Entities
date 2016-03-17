using System;
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
    public static class CodeBasedEntityStoreExtensions
    {
        public static Task<string> GetEntityCodeAsync<TEntity, TContext, TKey>(
            this ICodeBasedEntityStoreMarker<TEntity, TContext, TKey> store,
            TEntity entity, CancellationToken cancellationToken)
            where TEntity : class, ICodeWiseEntity
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            return GetEntityCodeAsync(store, entity, cancellationToken, null, null);
        }

        public static Task<string> GetEntityCodeAsync<TEntity, TContext, TKey>(
            this ICodeBasedEntityStoreMarker<TEntity, TContext, TKey> store,
            TEntity entity, CancellationToken cancellationToken,
            Expression<Func<TEntity, string>> codeSelector,
            Func<string> directGetCode)
            where TEntity : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            cancellationToken.ThrowIfCancellationRequested();
            store.ThrowIfDisposed();
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (directGetCode == null &&
                codeSelector == null &&
                entity is ICodeWiseEntity)
            {
                return Task.FromResult(((ICodeWiseEntity)entity).Code);
            }
            else if (codeSelector != null)
            {
                return Task.FromResult((codeSelector.Compile().Invoke(entity)));
            }
            else
            {
                if (directGetCode == null)
                {
                    throw new ArgumentNullException(nameof(directGetCode));
                }

                return Task.FromResult(directGetCode());
            }
        }

        public static Task SetEntityCodeAsync<TEntity, TContext, TKey>(
            this ICodeBasedEntityStoreMarker<TEntity, TContext, TKey> store,
            TEntity entity, string code, CancellationToken cancellationToken)
            where TEntity : class, ICodeWiseEntity
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            return SetEntityCodeAsync(store, entity, code, cancellationToken, null, null);
        }

        public static Task SetEntityCodeAsync<TEntity, TContext, TKey>(
            this ICodeBasedEntityStoreMarker<TEntity, TContext, TKey> store,
            TEntity entity, string code, CancellationToken cancellationToken,
            Expression<Func<TEntity, string>> codeSelector,
            Action directSetCode)
            where TEntity : class
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            cancellationToken.ThrowIfCancellationRequested();
            store.ThrowIfDisposed();
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (directSetCode == null &&
                codeSelector == null &&
                entity is ICodeWiseEntity)
            {
                ((ICodeWiseEntity)entity).Code = code;
            }
            else if (codeSelector != null)
            {
                codeSelector.GetPropertyAccess().SetValue(entity, code);
            }
            else
            {
                if (directSetCode == null)
                {
                    throw new ArgumentNullException(nameof(directSetCode));
                }

                directSetCode();
            }
            return Task.FromResult(0);
        }
    }
}
