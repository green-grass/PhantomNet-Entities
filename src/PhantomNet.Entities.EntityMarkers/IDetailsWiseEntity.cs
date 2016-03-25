using System.Collections.Generic;

namespace PhantomNet.Entities.EntityMarkers
{
    public interface IDetailsWiseEntity<TEntityDetail>
        where TEntityDetail : class
    {
        ICollection<TEntityDetail> Details { get; set; }
    }
}
