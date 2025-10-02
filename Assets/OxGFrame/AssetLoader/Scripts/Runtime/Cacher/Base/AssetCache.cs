using OxGKit.LoggingSystem;
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
            public int retryCount;
            public int maxRetryCount;

            public RetryCounter(int maxRetryCount)
            {
                this.retryCount = this.maxRetryCount = maxRetryCount;
            }

            public bool IsOutOfRetries()
            {
                return this.retryCount < 0;
            }

            public void DelRetryCount()
            {
                this.retryCount--;
            }
        }

        /// <summary>
        /// 資源 Pack 緩存
        /// </summary>
        protected Dictionary<string, T> _cacher;

        /// <summary>
        /// Blocker 加載標記緩存
        /// </summary>
        protected HashSet<string> _loadingFlags;

        /// <summary>
        /// 嘗試計數器緩存
        /// </summary>
        protected Dictionary<string, RetryCounter> _retryCounters;

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
            this._loadingFlags = new HashSet<string>();
            this._retryCounters = new Dictionary<string, RetryCounter>();
        }

        #region Loading Flag
        protected bool HasInLoadingFlag(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
                return false;
            return this._loadingFlags.Contains(assetName);
        }

        protected void AddLoadingFlag(string assetName)
        {
            if (!this.HasInLoadingFlag(assetName))
            {
                this._loadingFlags.Add(assetName);
                Logging.Print<Logger>($"Marked asset as loading: {assetName}.");
            }
        }

        protected void RemoveLoadingFlag(string assetName)
        {
            if (this.HasInLoadingFlag(assetName))
            {
                this._loadingFlags.Remove(assetName);
                Logging.Print<Logger>($"Cleared loading flag: {assetName}.");
            }
        }
        #endregion

        #region Retry Counter
        protected void StartRetryCounter(string assetName, byte maxRetryCount)
        {
            if (!this._retryCounters.ContainsKey(assetName))
                this._retryCounters.Add(assetName, new RetryCounter(maxRetryCount));
        }

        protected RetryCounter GetRetryCounter(string assetName)
        {
            this._retryCounters.TryGetValue(assetName, out RetryCounter retryCounter);
            return retryCounter;
        }

        protected void StopRetryCounter(string assetName)
        {
            if (this._retryCounters.ContainsKey(assetName))
                this._retryCounters.Remove(assetName);
        }
        #endregion
    }
}
