namespace PhantomNet.Entities
{
    public class IncrementalCodeGeneratorOptions<TEntity>
        where TEntity : class
    {
        public string Prefix { get; set; }
    }
}
