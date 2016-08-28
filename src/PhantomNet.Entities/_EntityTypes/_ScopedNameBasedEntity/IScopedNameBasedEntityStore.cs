using System.Threading;
using System.Threading.Tasks;

namespace PhantomNet.Entities
{
    public interface IScopedNameBasedEntityStore<TEntity, TEntityScope>
        : IGroupedEntityStore<TEntity, TEntityScope>
        where TEntity : class
        where TEntityScope : class
    {
        Task<TEntity> FindByNameAsync(string normalizedName, TEntityScope scope, CancellationToken cancellationToken);
    }
}
