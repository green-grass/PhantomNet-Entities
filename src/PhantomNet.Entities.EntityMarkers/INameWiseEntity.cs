namespace PhantomNet.Entities.EntityMarkers
{
    public interface INameWiseEntity
    {
        string Name { get; set; }

        string NormalizedName { get; set; }
    }
}
