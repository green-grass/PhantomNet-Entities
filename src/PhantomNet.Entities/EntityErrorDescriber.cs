namespace PhantomNet.Entities
{
    public class EntityErrorDescriber : ErrorDescriber
    {
        public virtual GenericError ConcurrencyFailure()
        {
            return new GenericError {
                Code = nameof(ConcurrencyFailure),
                Description = Strings.ConcurrencyFailure
            };
        }
    }
}
