namespace PhantomNet.Entities
{
    public interface IMultilingualEntityAccessor<TEntity>
        where TEntity : class
    {
        string GetLanguage(TEntity entity);
    }
}
