using System.Threading;
using System.Threading.Tasks;

namespace PhantomNet.Entities
{
    public interface IEntityCodeGenerator<in TEntity, in TEntityManager>
        where TEntity : class
        where TEntityManager : class
    {
        Task<string> GenerateCodeAsync(TEntityManager manager, TEntity entity, CancellationToken cancellationToken);
    }
}
