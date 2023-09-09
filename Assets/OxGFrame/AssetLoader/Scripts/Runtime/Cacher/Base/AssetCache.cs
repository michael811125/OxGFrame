using System.Collections.Generic;

namespace OxGFrame.AssetLoader.Cacher
{
    public abstract class AssetCache<T> : ICache<T>
    {
        public struct RetryCounter
        {
            public byte retryCount;
            public byte maxRetryCount;

            public RetryCounter(byte maxRetryCount)
            {
                this.retryCount = 0;
                this.maxRetryCount = maxRetryCount;
            }

            public bool IsRetryValid()
            {
                return this.retryCount < maxRetryCount;
            }

            public void AddRetryCount()
            {
                this.retryCount++;
            }
        }

        protected Dictionary<string, T> _cacher;

        protected Dictionary<string, RetryCounter> _loadingFlags;

        public float reqSize { get; protected set; }

        public float totalSize { get; protected set; }

        public int Count { get { return this._cacher.Count; } }

        public abstract bool HasInCache(string assetName);

        public abstract T GetFromCache(string assetName);

        public AssetCache()
        {
            this._cacher = new Dictionary<string, T>();
            this._loadingFlags = new Dictionary<string, RetryCounter>();
        }

        protected bool HasInLoadingFlags(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return false;
            return this._loadingFlags.ContainsKey(assetName);
        }

        protected void AddLoadingFlags(string assetName, byte maxRetryCount)
        {
            if (!this.HasInLoadingFlags(assetName)) this._loadingFlags.Add(assetName, new RetryCounter(maxRetryCount));
        }

        protected void RemoveLoadingFlags(string assetName)
        {
            if (this.HasInLoadingFlags(assetName)) this._loadingFlags.Remove(assetName);
        }

        protected RetryCounter GetRetryCounter(string assetName)
        {
            this._loadingFlags.TryGetValue(assetName, out RetryCounter retryCounter);
            return retryCounter;
        }

        ~AssetCache()
        {
            this._cacher.Clear();
            this._cacher = null;
            this._loadingFlags.Clear();
            this._loadingFlags = null;
        }
    }
}
