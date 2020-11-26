namespace Rock.Lava
{
    /// <summary>
    /// Specifies that this object can provide a LavaDataObject.
    /// </summary>
    public interface ILavaDataObjectSource
    {
        ILavaDataObject GetLavaDataObject();
    }
}
