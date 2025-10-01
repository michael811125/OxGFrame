using System.Collections.Generic;

namespace OxGFrame.AssetLoader.Cacher
{
    internal abstract class AssetCache<T>
    {
        /// <summary>
        /// 嘗試計數器
        /// </summary>
        public class RetryCounter
        {
            public byte retryCount;
            public byte maxRetryCount;

            public RetryCounter(byte maxRetryCount)
            {
                this.retryCount = 0;
                this.maxRetryCount = maxRetryCount;
            }

            public bool IsRetryActive()
            {
                return this.retryCount > 0;
            }

            public bool IsRetryValid()
            {
                // 嘗試次數先++, 後判斷, 所以需使用 <= 進行判斷
                return this.retryCount <= maxRetryCount;
            }

            public void AddRetryCount()
            {
                this.retryCount++;
            }
        }

        /// <summary>
        /// 資源 Pack 緩存
        /// </summary>
        protected Dictionary<string, T> _cacher;

        /// <summary>
        /// Blocker 加載標記
        /// </summary>
        protected Dictionary<string, RetryCounter> _loadingFlags;

        /// <summary>
        /// 當前進度數量 (Progress)
        /// </summary>
        public float currentCount { get; protected set; }

        /// <summary>
        /// 總共進度數量 (Progress)
        /// </summary>
        public float totalCount { get; protected set; }

        /// <summary>
        /// 緩存數量
        /// </summary>
        public int count => this._cacher.Count;

        public abstract bool HasInCache(string assetName);

        public abstract T GetFromCache(string assetName);

        public AssetCache()
        {
            this._cacher = new Dictionary<string, T>();
            this._loadingFlags = new Dictionary<string, RetryCounter>();
        }

        protected bool HasInLoadingFlags(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
                return false;
            return this._loadingFlags.ContainsKey(assetName);
        }

        protected void AddLoadingFlags(string assetName, byte maxRetryCount)
        {
            if (!this.HasInLoadingFlags(assetName))
                this._loadingFlags.Add(assetName, new RetryCounter(maxRetryCount));
        }

        protected void RemoveLoadingFlags(string assetName)
        {
            if (this.HasInLoadingFlags(assetName))
                this._loadingFlags.Remove(assetName);
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
