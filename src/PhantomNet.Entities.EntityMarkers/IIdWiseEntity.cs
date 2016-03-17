using System;

namespace PhantomNet.Entities.EntityMarkers
{
    public interface IIdWiseEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        TKey Id { get; set; }
    }
}
