namespace OxGFrame.AssetLoader.Cacher
{
    public interface ICache<T>
    {
        bool HasInCache(string name);

        T GetFromCache(string name);
    }
}
