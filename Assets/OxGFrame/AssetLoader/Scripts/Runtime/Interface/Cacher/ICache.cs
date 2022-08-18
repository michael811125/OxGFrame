using Cysharp.Threading.Tasks;

namespace OxGFrame.AssetLoader
{
    public delegate void Progression(float progress, float reqSize, float totalSize);

    public interface ICache<T>
    {
        bool HasInCache(string name);

        T GetFromCache(string name);

        UniTask PreloadInCache(string name, Progression progression);

        UniTask PreloadInCache(string[] names, Progression progression);

        void ReleaseFromCache(string name);

        void ReleaseCache();

        UniTask<int> GetAssetsLength(params string[] names);
    }
}
