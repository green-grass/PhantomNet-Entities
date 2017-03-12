using System.Threading.Tasks;

namespace PhantomNet.Entities
{
    public interface IEntityValidator<in TEntity, in TSubEntity>
        : IEntityValidator<TEntity>
        where TEntity : class
        where TSubEntity : class
    {
        Task<GenericResult> ValidateAsync(object manager, TSubEntity subEntity);
    }

    public interface IEntityValidator<in TEntity>
        where TEntity : class
    {
        Task<GenericResult> ValidateAsync(object manager, TEntity entity);
    }
}
