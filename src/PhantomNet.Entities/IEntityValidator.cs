using System.Threading.Tasks;

namespace PhantomNet.Entities
{
    public interface IEntityValidator<in TEntity, in TSubEntity, in TEntityManager> :
        IEntityValidator<TEntity, TEntityManager>
        where TEntity : class
        where TSubEntity : class
        where TEntityManager : class
    {
        Task<EntityResult> ValidateAsync(TEntityManager manager, TSubEntity subEntity);
    }

    public interface IEntityValidator<in TEntity, in TEntityManager>
        where TEntity : class
        where TEntityManager : class
    {
        Task<EntityResult> ValidateAsync(TEntityManager manager, TEntity entity);
    }
}
