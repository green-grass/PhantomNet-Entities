using System;

namespace PhantomNet.Entities
{
    public class LowerInvariantLookupNormalizer : ILookupNormalizer
    {
        public virtual string Normalize(string key)
        {
             return key?.Normalize().ToLowerInvariant();
        }
    }
}
