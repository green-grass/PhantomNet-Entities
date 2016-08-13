using System;
using System.Linq.Expressions;

namespace PhantomNet.Entities
{
    public static class MultilingualEntityAccessorExtensions
    {
        public static string GetEntityLanguage<TEntity>(
            this IMultilingualEntityAccessor<TEntity> accessor,
            TEntity entity)
            where TEntity : class, ICodeWiseEntity
        {
            return GetEntityLanguage(accessor, entity, null, null);
        }

        public static string GetEntityLanguage<TEntity>(
            this IMultilingualEntityAccessor<TEntity> accessor,
            TEntity entity,
            Expression<Func<TEntity, string>> languageSelector,
            Func<string> directGetLanguage)
            where TEntity : class
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (languageSelector == null &&
                directGetLanguage == null &&
                entity is ILanguageWiseEntity)
            {
                return ((ILanguageWiseEntity)entity).Language;
            }
            else if (languageSelector != null)
            {
                return languageSelector.Compile()(entity);
            }
            else if (directGetLanguage != null)
            {
                return directGetLanguage();
            }

            throw new ArgumentNullException($"{nameof(languageSelector)}, {nameof(directGetLanguage)}");
        }
    }
}
