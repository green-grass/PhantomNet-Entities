using System;

namespace PhantomNet.Entities
{
    public interface IMasterDetailsEntityManager<TEntity, TEntityDetail> : IDisposable
        where TEntity : class
        where TEntityDetail : class
    { }
}
