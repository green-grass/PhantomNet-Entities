namespace PhantomNet.Entities
{
    public class EntityErrorDescriber : ErrorDescriber
    {
        public virtual EntityError ConcurrencyFailure()
        {
            return new EntityError {
                Code = nameof(ConcurrencyFailure),
                Description = Resources.ConcurrencyFailure
            };
        }
    }
}
