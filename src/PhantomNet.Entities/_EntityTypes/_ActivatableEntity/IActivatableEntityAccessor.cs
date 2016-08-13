namespace PhantomNet.Entities
{
    public interface IActivatableEntityAccessor<TEntity>
        where TEntity : class
    {
        bool GetIsActive(TEntity entity);
    }
}
