namespace PhantomNet.Entities
{
    public interface IGroupedEntityAccessor<TEntity, TEntityGroup>
        where TEntity : class
        where TEntityGroup : class
    {
        string GetGroupId(TEntity entity);

        TEntityGroup GetGroup(TEntity entity);

        void SetGroup(TEntity entity, TEntityGroup group);
    }
}
