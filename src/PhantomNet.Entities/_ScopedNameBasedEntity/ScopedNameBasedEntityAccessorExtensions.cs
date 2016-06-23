using System;
using System.Linq.Expressions;

namespace PhantomNet.Entities
{
    public static class ScopedNameBasedEntityAccessorExtensions
    {
        public static string GetEntityName<TEntity, TEntityScope>(
            this IScopedNameBasedEntityAccessor<TEntity, TEntityScope> accessor,
            TEntity entity)
            where TEntity : class, INameWiseEntity
            where TEntityScope : class
        {
            return GetEntityName(accessor, entity, null, null);
        }

        public static string GetEntityName<TEntity, TEntityScope>(
            this IScopedNameBasedEntityAccessor<TEntity, TEntityScope> accessor,
            TEntity entity,
            Expression<Func<TEntity, string>> nameSelector,
            Func<string> directGetName)
            where TEntity : class
            where TEntityScope : class
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (nameSelector == null &&
                directGetName == null &&
                entity is INameWiseEntity)
            {
                return ((INameWiseEntity)entity).Name;
            }
            else if (nameSelector != null)
            {
                return nameSelector.Compile()(entity);
            }
            else if (directGetName != null)
            {
                return directGetName();
            }

            throw new ArgumentNullException($"{nameof(nameSelector)}, {nameof(directGetName)}");
        }

        public static void SetEntityName<TEntity, TEntityScope>(
            this IScopedNameBasedEntityAccessor<TEntity, TEntityScope> accessor,
            TEntity entity, string name)
            where TEntity : class, INameWiseEntity
            where TEntityScope : class
        {
            SetEntityName(accessor, entity, name, null, null);
        }

        public static void SetEntityName<TEntity, TEntityScope>(
            this IScopedNameBasedEntityAccessor<TEntity, TEntityScope> accessor,
            TEntity entity, string name,
            Expression<Func<TEntity, string>> nameSelector,
            Action directSetName)
            where TEntity : class
            where TEntityScope : class
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (nameSelector == null &&
                directSetName == null &&
                entity is INameWiseEntity)
            {
                ((INameWiseEntity)entity).Name = name;
            }
            else if (nameSelector != null)
            {
                nameSelector.GetPropertyAccess().SetValue(entity, name);
            }
            else if (directSetName != null)
            {
                directSetName();
            }
            else
            {
                throw new ArgumentNullException($"{nameof(nameSelector)}, {nameof(directSetName)}");
            }
        }

        public static void SetEntityNormalizedName<TEntity, TEntityScope>(
            this IScopedNameBasedEntityAccessor<TEntity, TEntityScope> accessor,
            TEntity entity, string name)
            where TEntity : class, INameWiseEntity
            where TEntityScope : class
        {
            SetEntityNormalizedName(accessor, entity, name, null, null);
        }

        public static void SetEntityNormalizedName<TEntity, TEntityScope>(
            this IScopedNameBasedEntityAccessor<TEntity, TEntityScope> accessor,
            TEntity entity, string normalizedName,
            Expression<Func<TEntity, string>> normalizedNameSelector,
            Action directSetNormalizedName)
            where TEntity : class
            where TEntityScope : class
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (normalizedNameSelector == null &&
                directSetNormalizedName == null &&
                entity is INameWiseEntity)
            {
                ((INameWiseEntity)entity).NormalizedName = normalizedName;
            }
            else if (normalizedNameSelector != null)
            {
                normalizedNameSelector.GetPropertyAccess().SetValue(entity, normalizedName);
            }
            else if (directSetNormalizedName != null)
            {
                directSetNormalizedName();
            }
            else
            {
                throw new ArgumentNullException($"{nameof(normalizedNameSelector)}, {nameof(directSetNormalizedName)}");
            }
        }
    }
}
