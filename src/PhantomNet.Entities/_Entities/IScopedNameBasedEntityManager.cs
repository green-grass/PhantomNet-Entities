using System;
using System.Threading.Tasks;

namespace PhantomNet.Entities
{
    public interface IScopedNameBasedEntityManager<TEntity, TEntityScope> : IDisposable
        where TEntity : class
        where TEntityScope : class
    {
        Task<TEntity> FindByNameAsync(string name, TEntityScope scope);

        Task<string> GetNameAsync(TEntity entity);

        Task<TEntityScope> GetScopeAsync(TEntity entity);
    }
}
