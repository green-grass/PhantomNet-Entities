using System;

namespace PhantomNet.Entities
{
    public interface IScopedNameBasedEntitySearchDescriptor<TKey>
        where TKey : IEquatable<TKey>
    {
        TKey ScopeId { get; set; }
    }
}
