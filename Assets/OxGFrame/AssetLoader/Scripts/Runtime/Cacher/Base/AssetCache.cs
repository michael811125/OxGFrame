using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace OxGFrame.AssetLoader.Cacher
{
    public abstract class AssetCache<T> : ICache<T>
    {
        protected Dictionary<string, T> _cacher;

        public float reqSize { get; protected set; }

        public float totalSize { get; protected set; }

        public int Count { get { return this._cacher.Count; } }

        public abstract bool HasInCache(string name);

        public abstract T GetFromCache(string name);

        public abstract UniTask Preload(string name, Progression progression);

        public abstract UniTask Preload(string[] names, Progression progression);

        public abstract void Unload(string name);

        public abstract void Release();

        public abstract UniTask<int> GetAssetsLength(params string[] names);
    }
}
