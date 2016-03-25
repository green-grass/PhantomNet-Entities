using System;
using System.Linq.Expressions;

namespace PhantomNet.Entities
{
    public interface IScopedNameBasedEntityAccessor<TEntity, TEntityScope>
        where TEntity : class
        where TEntityScope : class
    {

        string GetName(TEntity entity);

        void SetName(TEntity entity, string name);

        void SetNormalizedName(TEntity entity, string normalizedName);

        TEntityScope GetScope(TEntity entity);

        void SetScope(TEntity entity, TEntityScope scope);
    }
}
