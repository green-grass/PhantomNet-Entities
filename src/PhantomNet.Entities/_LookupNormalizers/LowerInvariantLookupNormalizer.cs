using System;

namespace PhantomNet.Entities
{
    public class LowerInvariantLookupNormalizer<T>
        : LowerInvariantLookupNormalizer,
          ILookupNormalizer<T>
    { }

    public class LowerInvariantLookupNormalizer : ILookupNormalizer
    {
        public virtual string Normalize(string key)
        {
            return key?.Normalize().ToLowerInvariant();
        }
    }
}
