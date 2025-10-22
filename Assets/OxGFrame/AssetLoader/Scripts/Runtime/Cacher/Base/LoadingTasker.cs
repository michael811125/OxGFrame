using OxGKit.LoggingSystem;
using System.Collections.Generic;

namespace OxGFrame.AssetLoader.Cacher
{
    /// <summary>
    /// Single-Flight 載入任務追蹤器
    /// </summary>
    public class LoadingTasker
    {
        /// <summary>
        /// 追蹤進行中的載入任務 (Single-Filght for Async)
        /// </summary>
        private readonly Dictionary<string, object> _loadingTasks;

        public LoadingTasker()
        {
            this._loadingTasks = new Dictionary<string, object>();
        }

        /// <summary>
        /// 檢查是否有正在執行的載入任務
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public bool HasLoadingTask(string assetName)
        {
            return this._loadingTasks.ContainsKey(assetName);
        }

        /// <summary>
        /// 獲取正在執行的載入任務
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="task"></param>
        /// <returns></returns>
        public bool TryGetLoadingTask(string assetName, out object task)
        {
            task = default;
            if (this._loadingTasks.TryGetValue(assetName, out var existingTask))
            {
                task = existingTask;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 嘗試添加正在執行的載入任務
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="task"></param>
        public void TryAddLoadingTask(string assetName, object task)
        {
            if (this._loadingTasks.TryAdd(assetName, task))
                Logging.Print<Logger>($"Marked asset as loading: {assetName}.");
        }

        /// <summary>
        /// 嘗試移除正在執行的載入任務
        /// </summary>
        /// <param name="assetName"></param>
        public void TryRemoveLoadingTask(string assetName)
        {
            if (this._loadingTasks.ContainsKey(assetName))
            {
                this._loadingTasks.Remove(assetName);
                Logging.Print<Logger>($"Cleared loading flag: {assetName}.");
            }
        }
    }
}