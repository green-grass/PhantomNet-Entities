using System;
using System.Threading.Tasks;

namespace PhantomNet.Entities
{
    public interface ITimeTrackedEntityManager<TEntity> : IDisposable
        where TEntity : class
    {
        Task<TEntity> FindLatestAsync();
    }
}
