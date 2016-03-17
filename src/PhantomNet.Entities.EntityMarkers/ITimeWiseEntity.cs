using System;

namespace PhantomNet.Entities.EntityMarkers
{
    public interface ITimeWiseEntity
    {
        DateTime DataCreateDate { get; set; }

        DateTime DataLastModifyDate { get; set; }
    }
}
