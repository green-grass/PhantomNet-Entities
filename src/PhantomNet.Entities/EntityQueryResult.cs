using System.Linq;

namespace PhantomNet.Entities
{
    public class EntityQueryResult<TEntity>
        where TEntity : class
    {
        public int TotalCount { get; set; }
        public int FilterredCount { get; set; }
        public IQueryable<TEntity> Results { get; set; }
    }
}
