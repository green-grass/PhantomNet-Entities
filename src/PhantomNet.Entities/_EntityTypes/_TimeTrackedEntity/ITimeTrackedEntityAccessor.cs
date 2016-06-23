using System;
using System.Linq.Expressions;

namespace PhantomNet.Entities
{
    public interface ITimeTrackedEntityAccessor<TEntity>
        where TEntity : class
    {

        void SetDataCreateDate(TEntity entity, DateTime dataCreateDate);

        void SetDataLastModifyDate(TEntity entity, DateTime dataLastModifyDate);
    }
}
