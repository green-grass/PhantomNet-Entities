using System;
using System.Collections.Generic;

namespace PhantomNet.Entities
{
    public class IncrementalCodeGeneratorOptions
    {
        public IDictionary<Type, string> Prefixes { get; set; } = new Dictionary<Type, string>();
    }
}
