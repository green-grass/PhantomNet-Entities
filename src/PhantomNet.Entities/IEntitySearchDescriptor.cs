using System.Linq;

namespace PhantomNet.Entities
{
    public interface IEntitySearchDescriptor<TEntity>
        where TEntity : class
    {
        string SearchText { get; set; }

        int? PageNumber { get; set; }

        int? PageSize { get; set; }

        string SortExpression { get; set; }

        bool SortReverse { get; set; }

        IQueryable<TEntity> PreFilter(IQueryable<TEntity> entities);

        IQueryable<TEntity> Filter(IQueryable<TEntity> entities);

        IQueryable<TEntity> PreSort(IQueryable<TEntity> entities);

        IQueryable<TEntity> DefaultSort(IQueryable<TEntity> entities);
    }
}
