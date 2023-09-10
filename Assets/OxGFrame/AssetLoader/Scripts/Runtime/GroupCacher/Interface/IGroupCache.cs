namespace OxGFrame.AssetLoader.GroupCacher
{
    public interface IGroupCache<T>
    {
        bool HasInCache(int id, string assetName);

        void AddIntoCache(int id, string assetName);

        void DelFromCache(int id, string assetName);
    }
}
