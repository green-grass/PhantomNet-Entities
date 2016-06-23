using System;
using System.Linq.Expressions;

namespace PhantomNet.Entities
{
    public interface INameBasedEntityAccessor<TEntity>
        where TEntity : class
    {
        string GetName(TEntity entity);

        void SetName(TEntity entity, string name);

        void SetNormalizedName(TEntity entity, string normalizedName);
    }
}
