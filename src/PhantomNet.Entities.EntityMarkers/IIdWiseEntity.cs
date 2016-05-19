using System;

namespace PhantomNet.Entities
{
    public interface IIdWiseEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        TKey Id { get; set; }
    }
}
