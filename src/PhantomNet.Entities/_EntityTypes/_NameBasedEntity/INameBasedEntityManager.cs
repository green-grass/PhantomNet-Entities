using System;
using System.Threading.Tasks;

namespace PhantomNet.Entities
{
    public interface INameBasedEntityManager<TEntity> : IDisposable
        where TEntity : class
    {
        Task<TEntity> FindByNameAsync(string name);
    }
}
