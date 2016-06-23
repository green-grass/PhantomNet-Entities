using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PhantomNet.Entities
{
    public interface IMasterDetailsEntityStore<TEntity, TEntityDetail> : IDisposable
        where TEntity : class
        where TEntityDetail : class
    { }
}
