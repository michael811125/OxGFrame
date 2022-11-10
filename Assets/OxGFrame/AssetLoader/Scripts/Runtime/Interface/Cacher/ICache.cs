using Cysharp.Threading.Tasks;

namespace OxGFrame.AssetLoader
{
    public delegate void Progression(float progress, float reqSize, float totalSize);

    public interface ICache<T>
    {
        bool HasInCache(string name);

        T GetFromCache(string name);

        UniTask Preload(string name, Progression progression);

        UniTask Preload(string[] names, Progression progression);

        void Unload(string name);

        void Release();
    }
}
