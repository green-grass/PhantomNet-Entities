using System;
using System.Linq.Expressions;
using PhantomNet.Entities.EntityMarkers;

namespace PhantomNet.Entities
{
    public static class CodeBasedEntityAccessorExtensions
    {
        public static string GetEntityCode<TEntity>(
            this ICodeBasedEntityAccessor<TEntity> accessor,
            TEntity entity)
            where TEntity : class,
                            ICodeWiseEntity
        {
            return GetEntityCode(accessor, entity, null, null);
        }

        public static string GetEntityCode<TEntity>(
            this ICodeBasedEntityAccessor<TEntity> accessor,
            TEntity entity,
            Expression<Func<TEntity, string>> codeSelector,
            Func<string> directGetCode)
            where TEntity : class
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (codeSelector == null &&
                directGetCode == null &&
                entity is ICodeWiseEntity)
            {
                return ((ICodeWiseEntity)entity).Code;
            }
            else if (codeSelector != null)
            {
                return codeSelector.Compile().Invoke(entity);
            }
            else if (directGetCode != null)
            {
                return directGetCode();
            }

            throw new ArgumentNullException($"{nameof(codeSelector)}, {nameof(directGetCode)}");
        }

        public static void SetEntityCode<TEntity>(
            this ICodeBasedEntityAccessor<TEntity> accessor,
            TEntity entity, string code)
            where TEntity : class,
                            ICodeWiseEntity
        {
            SetEntityCode(accessor, entity, code, null, null);
        }

        public static void SetEntityCode<TEntity>(
            this ICodeBasedEntityAccessor<TEntity> accessor,
            TEntity entity, string code,
            Expression<Func<TEntity, string>> codeSelector,
            Action directSetCode)
            where TEntity : class
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (codeSelector == null &&
                directSetCode == null &&
                entity is ICodeWiseEntity)
            {
                ((ICodeWiseEntity)entity).Code = code;
            }
            else if (codeSelector != null)
            {
                codeSelector.GetPropertyAccess().SetValue(entity, code);
            }
            else if (directSetCode != null)
            {
                directSetCode();
            }
            else
            {
                throw new ArgumentNullException($"{nameof(codeSelector)}, {nameof(directSetCode)}");
            }
        }
    }
}
