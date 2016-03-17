using System;
using System.Threading.Tasks;

namespace PhantomNet.Entities
{
    public interface INameBasedEntityManager<TEntity> : IReadOnlyNameBasedEntityManager<TEntity>
        where TEntity : class
    {
        Task<string> GetNameAsync(TEntity entity);
    }

    public interface IReadOnlyNameBasedEntityManager<TEntity> : IDisposable
        where TEntity : class
    {
        Task<TEntity> FindByNameAsync(string name);
    }
}
