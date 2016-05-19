using System.Collections.Generic;

namespace PhantomNet.Entities
{
    public interface IDetailsWiseEntity<TEntityDetail>
        where TEntityDetail : class
    {
        ICollection<TEntityDetail> Details { get; set; }
    }
}
