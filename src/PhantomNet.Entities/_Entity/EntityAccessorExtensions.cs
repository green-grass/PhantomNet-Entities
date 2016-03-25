using System;
using System.Linq.Expressions;
using PhantomNet.Entities.EntityMarkers;

namespace PhantomNet.Entities
{
    public static class EntityAccessorExtensions
    {
        public static string GetEntityId<TEntity, TKey>(
            this IEntityAccessor<TEntity> accessor,
            TEntity entity)
            where TEntity : class
            where TKey : IEquatable<TKey>
        {
            return GetEntityId<TEntity, TKey>(accessor, entity, null, null);
        }

        public static string GetEntityId<TEntity, TKey>(
            this IEntityAccessor<TEntity> accessor,
            TEntity entity,
            Expression<Func<TEntity, TKey>> idSelector,
            Func<string> directGetId)
            where TEntity : class
            where TKey : IEquatable<TKey>
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (idSelector == null &&
                directGetId == null &&
                entity is IIdWiseEntity<TKey>)
            {
                return ConvertIdToString(((IIdWiseEntity<TKey>)entity).Id);
            }
            else if (idSelector != null)
            {
                return ConvertIdToString(idSelector.Compile().Invoke(entity));
            }
            else if (directGetId != null)
            {
                return directGetId();
            }

            throw new ArgumentNullException($"{nameof(idSelector)}, {nameof(directGetId)}");
        }

        #region Helpers

        private static string ConvertIdToString<TKey>(TKey id)
            where TKey : IEquatable<TKey>
        {
            if (id.Equals(default(TKey)))
            {
                return null;
            }

            return id.ToString();
        }

        #endregion
    }
}
