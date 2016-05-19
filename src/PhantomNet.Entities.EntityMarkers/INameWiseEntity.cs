namespace PhantomNet.Entities
{
    public interface INameWiseEntity
    {
        string Name { get; set; }

        string NormalizedName { get; set; }
    }
}
