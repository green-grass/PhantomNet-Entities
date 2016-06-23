namespace PhantomNet.Entities
{
    public interface IScopedNameBasedEntityAccessor<TEntity, TEntityScope>
        : IGroupedEntityAccessor<TEntity, TEntityScope>,
          INameBasedEntityAccessor<TEntity>
        where TEntity : class
        where TEntityScope : class
    { }
}
