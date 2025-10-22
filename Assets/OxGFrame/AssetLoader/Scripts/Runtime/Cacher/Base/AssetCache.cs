using OxGKit.LoggingSystem;
using System.Collections.Generic;

namespace OxGFrame.AssetLoader.Cacher
{
    internal abstract class AssetCache<T>
    {
        /// <summary>
        /// 處理類型
        /// </summary>
        internal enum ProcessType
        {
            RawFile,
            Scene,
            Asset
        }

        /// <summary>
        /// 資源 Pack 緩存
        /// </summary>
        protected Dictionary<string, T> _cacher;

        /// <summary>
        /// 任務追蹤器
        /// </summary>
        protected LoadingTasker _loadingTasker;

        /// <summary>
        /// 正在卸載標記
        /// </summary>
        protected readonly HashSet<string> _unloadingAssets;

        /// <summary>
        /// 待卸載任務請求
        /// </summary>
        protected readonly Dictionary<string, List<bool>> _pendingUnloads;

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
            this._loadingTasker = new LoadingTasker();
            this._unloadingAssets = new HashSet<string>();
            this._pendingUnloads = new Dictionary<string, List<bool>>();
            this._retryCounters = new Dictionary<string, RetryCounter>();
        }

        #region Loading Task
        /// <summary>
        /// 檢查是否有正在執行的載入任務
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        protected bool HasLoadingTask(string assetName)
        {
            return this._loadingTasker.HasLoadingTask(assetName);
        }

        /// <summary>
        /// 獲取正在執行的載入任務
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="task"></param>
        /// <returns></returns>
        protected bool TryGetLoadingTask(string assetName, out object task)
        {
            return this._loadingTasker.TryGetLoadingTask(assetName, out task);
        }

        /// <summary>
        /// 嘗試添加正在執行的載入任務
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="task"></param>
        protected void TryAddLoadingTask(string assetName, object task)
        {
            this._loadingTasker.TryAddLoadingTask(assetName, task);
        }

        /// <summary>
        /// 嘗試移除正在執行的載入任務
        /// </summary>
        /// <param name="assetName"></param>
        protected void TryRemoveLoadingTask(string assetName)
        {
            this._loadingTasker.TryRemoveLoadingTask(assetName);
        }
        #endregion

        #region Unloading Flag
        /// <summary>
        /// 檢查資產是否正在執行卸載
        /// </summary>
        protected bool HasUnloadingFlag(string assetName)
        {
            return this._unloadingAssets.Contains(assetName);
        }

        /// <summary>
        /// 標記資產正在執行卸載
        /// </summary>
        protected void AddUnloadingFlag(string assetName)
        {
            this._unloadingAssets.Add(assetName);
        }

        /// <summary>
        /// 移除資產卸載標記
        /// </summary>
        protected void RemoveUnloadingFlag(string assetName)
        {
            this._unloadingAssets.Remove(assetName);
        }
        #endregion

        #region Unloading Flag
        /// <summary>
        /// 檢查是否有待執行的卸載請求
        /// </summary>
        protected bool HasPendingUnload(string assetName)
        {
            return this._pendingUnloads.ContainsKey(assetName) && this._pendingUnloads[assetName].Count > 0;
        }

        /// <summary>
        /// 添加待執行的卸載請求
        /// </summary>
        protected void AddPendingUnload(string assetName, bool forceUnload)
        {
            if (!this._pendingUnloads.ContainsKey(assetName))
                this._pendingUnloads[assetName] = new List<bool>();

            this._pendingUnloads[assetName].Add(forceUnload);
            Logging.Print<Logger>($"Added pending unload for: {assetName}, total pending: {this._pendingUnloads[assetName].Count}");
        }

        /// <summary>
        /// 獲取並清除所有待執行的卸載請求
        /// </summary>
        protected List<bool> GetAndClearPendingUnloads(string assetName)
        {
            if (!this._pendingUnloads.ContainsKey(assetName))
                return null;

            var pendingList = this._pendingUnloads[assetName];
            this._pendingUnloads.Remove(assetName);

            Logging.Print<Logger>($"Retrieved {pendingList.Count} pending unloads for: {assetName}");
            return pendingList;
        }
        #endregion

        #region Retry Counter
        /// <summary>
        /// 開始與建立重試計數器
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="maxRetryCount"></param>
        protected void StartRetryCounter(string assetName, byte maxRetryCount)
        {
            if (!this._retryCounters.ContainsKey(assetName))
                this._retryCounters.Add(assetName, new RetryCounter(maxRetryCount));
        }

        /// <summary>
        /// 獲取重試計數器
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        protected RetryCounter GetRetryCounter(string assetName)
        {
            this._retryCounters.TryGetValue(assetName, out RetryCounter retryCounter);
            return retryCounter;
        }

        /// <summary>
        /// 停止重試計數器
        /// </summary>
        /// <param name="assetName"></param>
        protected void StopRetryCounter(string assetName)
        {
            if (this._retryCounters.ContainsKey(assetName))
                this._retryCounters.Remove(assetName);
        }
        #endregion

        /// <summary>
        /// 檢查卸載條件
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="forceUnload"></param>
        /// <param name="processType"></param>
        /// <param name="unloadAction"></param>
        protected void CheckUnload(string assetName, bool forceUnload, ProcessType processType, System.Action<string, bool, ProcessType> unloadAction)
        {
            if (string.IsNullOrEmpty(assetName))
                return;

            // 如果正在載入, 將卸載請求加入待執行隊列
            if (this.HasLoadingTask(assetName))
            {
                this.AddPendingUnload(assetName, forceUnload);
                Logging.PrintWarning<Logger>($"【Pending Unload】 Asset: {assetName} is loading, queued unload request.");
                return;
            }

            // 如果正在執行卸載, 跳過
            if (this.HasUnloadingFlag(assetName))
            {
                Logging.PrintWarning<Logger>($"【Try Unload】 Asset: {assetName} is already unloading...");
                return;
            }

            unloadAction(assetName, forceUnload, processType);
        }

        /// <summary>
        /// 處理所有待執行的卸載
        /// </summary>
        /// <param name="assetName">資產名稱</param>
        /// <param name="unloadAction">自定義卸載方法</param>
        protected void ProcessPendingUnloads(string assetName, ProcessType processType, System.Action<string, bool, ProcessType> unloadAction)
        {
            var pendingList = this.GetAndClearPendingUnloads(assetName);
            if (pendingList == null || pendingList.Count == 0)
                return;

            Logging.Print<Logger>($"【Processing Pending Unloads】 Asset: {assetName}, pending count: {pendingList.Count}");

            // 按順序執行所有待執行的卸載
            foreach (var forceUnload in pendingList)
            {
                unloadAction(assetName, forceUnload, processType);
            }
        }
    }
}
