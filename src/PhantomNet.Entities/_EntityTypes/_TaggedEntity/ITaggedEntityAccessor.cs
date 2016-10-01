namespace PhantomNet.Entities
{
    public interface ITaggedEntityAccessor<TEntity>
        where TEntity : class
    {
        string GetTags(TEntity entity);

        void SetTags(TEntity entity, string tags);
    }
}
