namespace OxGFrame.AssetLoader
{
    public delegate void Progression(float progress, float reqSize, float totalSize);

    public interface ICache<T>
    {
        bool HasInCache(string name);

        T GetFromCache(string name);
    }
}
