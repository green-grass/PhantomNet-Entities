namespace PhantomNet.Entities
{
    public interface ILookupNormalizer<T> : ILookupNormalizer { }

    public interface ILookupNormalizer
    {
        string Normalize(string key);
    }
}
