using System;

namespace PhantomNet.Entities
{
    public interface IGroupedEntitySearchDescriptor<TKey>
        where TKey : IEquatable<TKey>
    {
        TKey GroupId { get; set; }
    }
}
