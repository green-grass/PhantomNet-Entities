namespace PhantomNet.Entities
{
    public interface IEntityAccessor<TEntity, TSubEntity> :
        IEntityAccessor<TEntity>
        where TEntity : class
        where TSubEntity : class
    {
        string GetId(TSubEntity subEntity);
    }

    public interface IEntityAccessor<TEntity>
        where TEntity : class
    {
        string GetId(TEntity entity);
    }
}
