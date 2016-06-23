using System;
using System.Threading.Tasks;

namespace PhantomNet.Entities
{
    public interface ICodeBasedEntityManager<TEntity> : IDisposable
        where TEntity : class
    {
        Task<TEntity> FindByCodeAsync(string code);
    }
}
