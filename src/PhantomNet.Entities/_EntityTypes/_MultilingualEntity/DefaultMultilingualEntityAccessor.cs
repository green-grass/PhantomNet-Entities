using System;
using System.Reflection;

namespace PhantomNet.Entities
{
    public class DefaultMultilingualEntityAccessor<TEntity> : IMultilingualEntityAccessor<TEntity>
        where TEntity : class
    {
        public string GetLanguage(TEntity entity)
        {
            if (!typeof(TEntity).IsAssignableFrom(typeof(ILanguageWiseEntity)))
            {
                // TODO:: Message
                throw new ArgumentException();
            }

            return this.GetEntityLanguage(entity, null, null);
        }
    }
}
