using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace OxGFrame.AssetLoader.Cacher
{
    public abstract class AssetCache<T> : ICache<T>
    {
        protected Dictionary<string, T> _cacher;

        protected HashSet<string> _hashLoadingFlags;

        public float reqSize { get; protected set; }

        public float totalSize { get; protected set; }

        public int Count { get { return this._cacher.Count; } }

        public abstract bool HasInCache(string name);

        public abstract T GetFromCache(string name);

        public abstract UniTask Preload(string name, Progression progression);

        public abstract UniTask Preload(string[] names, Progression progression);

        public abstract void Unload(string name);

        public abstract void Release();

        public AssetCache()
        {
            this._cacher = new Dictionary<string, T>();
            this._hashLoadingFlags = new HashSet<string>();
        }

        public virtual bool HasInLoadingFlags(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            return this._hashLoadingFlags.Contains(name);
        }

        public virtual int GetAssetsLength(params string[] names)
        {
            return names.Length;
        }

        ~AssetCache()
        {
            this._cacher.Clear();
            this._cacher = null;
            this._hashLoadingFlags.Clear();
            this._hashLoadingFlags = null;
        }
    }
}
