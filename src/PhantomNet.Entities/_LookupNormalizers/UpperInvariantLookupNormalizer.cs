using System;

namespace PhantomNet.Entities
{
    public class UpperInvariantLookupNormalizer<TMarker>
        : UpperInvariantLookupNormalizer,
          ILookupNormalizer<TMarker>
    { }

    public class UpperInvariantLookupNormalizer : ILookupNormalizer
    {
        public virtual string Normalize(string key)
        {
             return key?.Normalize().ToUpperInvariant();
        }
    }
}
