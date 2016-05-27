using System.Linq;

namespace PhantomNet.Entities
{
    public abstract class EntitySearchParametersBase<TEntity> : IEntitySearchParameters<TEntity>
        where TEntity : class
    {
        public string SearchText { get; set; }

        public int? PageNumber { get; set; }

        public int? PageSize { get; set; }

        public string SortExpression { get; set; }

        public bool SortReverse { get; set; }

        public virtual IQueryable<TEntity> PreFilter(IQueryable<TEntity> entities)
        {
            return entities;
        }

        public abstract IQueryable<TEntity> Filter(IQueryable<TEntity> entities);

        public virtual IQueryable<TEntity> PreSort(IQueryable<TEntity> entities)
        {
            return entities;
        }

        public abstract IQueryable<TEntity> DefaultSort(IQueryable<TEntity> entities);
    }
}
