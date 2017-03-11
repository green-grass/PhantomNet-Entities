using System;
using System.Reflection;

namespace PhantomNet.Entities
{
    public class DefaultCodeBasedEntityAccessor<TEntity> : ICodeBasedEntityAccessor<TEntity>
        where TEntity : class
    {
        public string GetCode(TEntity entity)
        {
            if (!typeof(TEntity).IsAssignableFrom(typeof(ICodeWiseEntity)))
            {
                // TODO:: Message
                throw new ArgumentException();
            }

            return this.GetEntityCode(entity, null, null);
        }

        public void SetCode(TEntity entity, string code)
        {
            if (!typeof(TEntity).IsAssignableFrom(typeof(ICodeWiseEntity)))
            {
                // TODO:: Message
                throw new ArgumentException();
            }

            this.SetEntityCode(entity, code, null, null);
        }
    }
}
