using System;
using System.Threading.Tasks;

namespace PhantomNet.Entities
{
    public interface ICodeBasedEntityManager<TEntity> : IReadOnlyCodeBasedEntityManager<TEntity>
        where TEntity : class
    {
        Task<string> GetCodeAsync(TEntity entity);
    }

    public interface IReadOnlyCodeBasedEntityManager<TEntity> : IDisposable
        where TEntity : class
    {
        Task<TEntity> FindByCodeAsync(string code);
    }
}
