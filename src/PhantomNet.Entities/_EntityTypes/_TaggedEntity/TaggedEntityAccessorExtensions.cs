using System;
using System.Linq.Expressions;

namespace PhantomNet.Entities
{
    public static class TaggedEntityAccessorExtensions
    {
        public static string GetEntityTags<TEntity>(
            this ITaggedEntityAccessor<TEntity> accessor,
            TEntity entity)
            where TEntity : class, ITagsWiseEntity
        {
            return GetEntityTags(accessor, entity, null, null);
        }

        public static string GetEntityTags<TEntity>(
            this ITaggedEntityAccessor<TEntity> accessor,
            TEntity entity,
            Expression<Func<TEntity, string>> tagsSelector,
            Func<string> directGetTags)
            where TEntity : class
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (tagsSelector == null &&
                directGetTags == null &&
                entity is ITagsWiseEntity)
            {
                return ((ITagsWiseEntity)entity).Tags;
            }
            else if (tagsSelector != null)
            {
                return tagsSelector.Compile()(entity);
            }
            else if (directGetTags != null)
            {
                return directGetTags();
            }

            throw new ArgumentNullException($"{nameof(tagsSelector)}, {nameof(directGetTags)}");
        }

        public static void SetEntityTags<TEntity>(
            this ITaggedEntityAccessor<TEntity> accessor,
            TEntity entity, string tags)
            where TEntity : class, ITagsWiseEntity
        {
            SetEntityTags(accessor, entity, tags, null, null);
        }

        public static void SetEntityTags<TEntity>(
            this ITaggedEntityAccessor<TEntity> accessor,
            TEntity entity, string tags,
            Expression<Func<TEntity, string>> tagsSelector,
            Action directSetTags)
            where TEntity : class
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (tagsSelector == null &&
                directSetTags == null &&
                entity is ITagsWiseEntity)
            {
                ((ITagsWiseEntity)entity).Tags = tags;
            }
            else if (tagsSelector != null)
            {
                tagsSelector.GetPropertyAccess().SetValue(entity, tags);
            }
            else if (directSetTags != null)
            {
                directSetTags();
            }
            else
            {
                throw new ArgumentNullException($"{nameof(tagsSelector)}, {nameof(directSetTags)}");
            }
        }
    }
}
