namespace PhantomNet.Entities
{
    public interface IScopedNameBasedEntityAccessor<TEntity, TEntityScope> : INameBasedEntityAccessor<TEntity>
        where TEntity : class
        where TEntityScope : class
    {
        string GetScopeId(TEntity entity);

        TEntityScope GetScope(TEntity entity);

        void SetScope(TEntity entity, TEntityScope scope);
    }
}
