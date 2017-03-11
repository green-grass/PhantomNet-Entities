using System;
using System.Reflection;

namespace PhantomNet.Entities
{
    public class DefaultTaggedEntityAccessor<TEntity> : ITaggedEntityAccessor<TEntity>
        where TEntity : class
    {
        public string GetTags(TEntity entity)
        {
            if (!typeof(TEntity).IsAssignableFrom(typeof(ITagsWiseEntity)))
            {
                // TODO:: Message
                throw new ArgumentException();
            }

            return this.GetEntityTags(entity, null, null);
        }

        public void SetTags(TEntity entity, string tags)
        {
            if (!typeof(TEntity).IsAssignableFrom(typeof(ITagsWiseEntity)))
            {
                // TODO:: Message
                throw new ArgumentException();
            }

            this.SetEntityTags(entity, tags, null, null);
        }
    }
}
