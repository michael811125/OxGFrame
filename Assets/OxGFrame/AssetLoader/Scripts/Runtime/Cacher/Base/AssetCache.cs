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

        public abstract bool HasInCache(string assetName);

        public abstract T GetFromCache(string assetName);

        public AssetCache()
        {
            this._cacher = new Dictionary<string, T>();
            this._hashLoadingFlags = new HashSet<string>();
        }

        public virtual bool HasInLoadingFlags(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return false;
            return this._hashLoadingFlags.Contains(assetName);
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
