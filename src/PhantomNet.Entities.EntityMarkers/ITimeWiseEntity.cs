using System;

namespace PhantomNet.Entities
{
    public interface ITimeWiseEntity
    {
        DateTime DataCreateDate { get; set; }

        DateTime DataLastModifyDate { get; set; }
    }
}
