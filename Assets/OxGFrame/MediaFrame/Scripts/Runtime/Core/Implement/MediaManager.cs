using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader;
using OxGFrame.MediaFrame.AudioFrame;
using OxGFrame.MediaFrame.Cacher;
using OxGFrame.MediaFrame.VideoFrame;
using OxGKit.LoggingSystem;
using OxGKit.Utilities.Cacher;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace OxGFrame.MediaFrame
{
    internal abstract class MediaManager<T> : MonoBehaviour where T : MediaBase
    {
        /// <summary>
        /// 【常駐】所有資源緩存
        /// </summary>
        protected Dictionary<string, GameObject> _dictAssetCache = new Dictionary<string, GameObject>();

        /// <summary>
        /// 用來標記正在加載中的資源 (暫存緩存)
        /// </summary>
        protected HashSet<string> _loadingFlags = new HashSet<string>();

        /// <summary>
        /// 【常駐】所有進入播放的影音柱列緩存 (只會在 Destroy 時, Remove 對應的緩存)
        /// </summary>
        protected List<T> _listAllCache = new List<T>();

        /// <summary>
        /// 處理沒有啟用 onDestroyAndUnload 設置的資源
        /// </summary>
        protected LRUCache<string, string> _mediaLruCache = new LRUCache<string, string>(64, new MediaObjectRemoveCacheHandler());

        private static float _fdt;

        private void FixedUpdate()
        {
            if (this._listAllCache.Count > 0)
            {
                int count = this._listAllCache.Count;
                for (int i = 0; i < count; i++)
                {
                    if (this._listAllCache.Count != count) break;

                    // 僅刷新激活的物件
                    if (this._listAllCache[i].gameObject.activeSelf)
                    {
                        if (this._listAllCache[i].ignoreTimeScale) _fdt = Time.fixedDeltaTime;
                        else _fdt = Time.fixedUnscaledDeltaTime;
                        this._listAllCache[i].HandleFixedUpdate(_fdt);
                    }
                }
            }
        }

        /// <summary>
        /// 檢查是否有資源緩存
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public bool HasAssetInCache(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return false;
            return this._dictAssetCache.ContainsKey(assetName);
        }

        /// <summary>
        /// 加載中的標記緩存
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        protected bool HasInLoadingFlags(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return false;
            return this._loadingFlags.Contains(assetName);
        }

        /// <summary>
        /// 處理不常用的資源, 僅針對沒有啟用 onDestroyAndUnload 的設置
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="assetName"></param>
        protected void TryLRUCache<U>(string assetName) where U : T
        {
            string key = $"{typeof(U).Name}_{assetName}";
            if (!this._mediaLruCache.Contains(key))
                this._mediaLruCache.Add(key, assetName);
            this._mediaLruCache.Get(key);
        }

        /// <summary>
        /// 從緩存中取的資源
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        protected GameObject GetAssetFromCache(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return null;
            this._dictAssetCache.TryGetValue(assetName, out var go);
            return go;
        }

        /// <summary>
        /// 取得該對應路徑名稱的媒體組件陣列
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public U[] GetMediaComponents<U>(string assetName) where U : T
        {
            if (string.IsNullOrEmpty(assetName) || this._listAllCache.Count == 0) return new U[] { };

            List<T> filter = new List<T>();
            for (int i = 0; i < this._listAllCache.Count; i++)
            {
                if (this._listAllCache[i].assetName == assetName) filter.Add(this._listAllCache[i]);
            }

            return (U[])filter.ToArray();
        }

        /// <summary>
        /// 建立節點物件
        /// </summary>
        /// <param name="nodeName"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        protected virtual GameObject CreateNode(string nodeName, Transform parent)
        {
            GameObject nodeChecker = parent.transform.Find(nodeName)?.gameObject;
            if (nodeChecker != null) return nodeChecker;

            GameObject nodeGo = new GameObject(nodeName);
            nodeGo.transform.SetParent(parent);

            // 校正 Transform
            nodeGo.transform.localScale = Vector3.one;
            nodeGo.transform.localPosition = Vector3.zero;
            nodeGo.transform.localRotation = Quaternion.identity;

            return nodeGo;
        }

        /// <summary>
        /// 實際運行加載物件資源
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        protected virtual async UniTask<GameObject> LoadGameObject(string packageName, string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return null;

            GameObject obj = await AssetLoaders.LoadAssetAsync<GameObject>(packageName, assetName);

            if (obj == null)
            {
                Logging.PrintError<Logger>($"Media Object -> Asset not found at path or name: {assetName}");
                return null;
            }

            return obj;
        }

        /// <summary>
        /// 加載資源至資源緩存中
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        protected virtual async UniTask<GameObject> LoadingAsset(string packageName, string assetName)
        {
            GameObject go = await this.LoadGameObject(packageName, assetName);
            if (go == null) return null;

            this._dictAssetCache.Add(assetName, go);

            return go;
        }

        /// <summary>
        /// 加載資源至緩存, 判斷是否已有加載過, 如果有則返回該資源物件
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        protected async UniTask<GameObject> LoadAssetIntoCache(string packageName, string assetName)
        {
            GameObject go;
            // 判斷不在 AllCache 中, 也不在 LoadingFlags 中, 才進行加載程序
            if (!this.HasAssetInCache(assetName) && !this.HasInLoadingFlags(assetName))
            {
                this._loadingFlags.Add(assetName);                    // 標記 LoadingFlag
                go = await this.LoadingAsset(packageName, assetName); // 開始加載
                this._loadingFlags.Remove(assetName);                 // 移除 LoadingFlag
            }
            else
                go = this.GetAssetFromCache(assetName); // 如果判斷沒有要執行加載程序, 就直接從 AllCache 中取得

            return go;
        }

        /// <summary>
        /// 實例化媒體組件
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="go"></param>
        /// <returns></returns>
        protected async virtual UniTask<U> CloneAsset<U>(string assetName, GameObject go, UnityEngine.Object sourceClip, Transform parent, Transform spwanParent) where U : T
        {
            if (go == null)
                return default;

            U sourceComponent = go.GetComponent<U>();
            bool sourceMonoDriveFlag = false;
            if (sourceComponent != null)
            {
                if (sourceComponent.monoDrive)
                {
                    // 記錄標記
                    sourceMonoDriveFlag = sourceComponent.monoDrive;
                    // 動態加載時, 必須取消 monoDrive
                    sourceComponent.monoDrive = false;
                }
            }

            GameObject instGo = Instantiate(sourceComponent.gameObject, (parent != null) ? parent : spwanParent);
            instGo.name = instGo.name.Replace("(Clone)", "");
            U mBase = instGo.GetComponent<U>();
            if (mBase == null)
                return default;

            // 激活檢查, 如果主體 Active 為 false 必須打開
            if (!instGo.activeSelf)
                instGo.SetActive(true);

            // 先加入緩存
            this._listAllCache.Add(mBase);
            this.SetParent(mBase, parent);

            // 設置管理名稱
            string mediaName = assetName;
            mediaName = (mediaName.IndexOf('/') != -1) ? mediaName.Substring(mediaName.LastIndexOf('/')).Replace("/", string.Empty) : mediaName;
            mBase.SetNames(assetName, mediaName);

            // 如果判斷有 clip 來源, 會重新 assign
            if (sourceClip != null)
            {
                if (typeof(U) == typeof(AudioBase))
                {
                    // 處理 AudioBase 類型
                    (mBase as AudioBase).audioClip = sourceClip as AudioClip;
                }
                else if (typeof(U) == typeof(VideoBase))
                {
                    // 處理 VideoBase 類型
                    (mBase as VideoBase).videoClip = sourceClip as VideoClip;
                }
            }

            bool isInitialized = await mBase.Init();
            if (!isInitialized)
            {
                this.Destroy(mBase);
                return default;
            }

            // >>> 需在 Init 之後, 以下設定開始生效 <<<

            if (mBase == null || mBase.gameObject.IsDestroyed())
                return default;

            mBase.gameObject.SetActive(false);

            // 還原來源設置
            if (sourceMonoDriveFlag)
                sourceComponent.monoDrive = true;

            return mBase;
        }

        /// <summary>
        /// 資源預加載
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="assetNames"></param>
        /// <returns></returns>
        public async UniTask Preload(string packageName, string[] assetNames)
        {
            if (assetNames.Length > 0)
            {
                for (int i = 0; i < assetNames.Length; i++)
                {
                    if (string.IsNullOrEmpty(assetNames[i])) continue;
                    await this.LoadAssetIntoCache(packageName, assetNames[i]);
                }
            }
        }

        protected virtual void SetParent(T mBase, Transform parent) { }

        #region Play
        public abstract UniTask<T[]> Play(string packageName, string assetName, UnityEngine.Object sourceClip, Transform parent = null, int loops = 0, float volume = 0f);
        public abstract void ResumeAll();
        #endregion

        #region Stop & Pause
        public abstract void Stop(string assetName, bool disabledEndEvent = false, bool forceDestroy = false);
        public abstract void StopAll(bool disabledEndEvent = false, bool forceDestroy = false);
        public abstract void Pause(string assetName);
        public abstract void PauseAll();
        #endregion

        #region Load Play & Exit Stop
        protected virtual void LoadAndPlay(T mBase, int loops, float volume) { }

        protected virtual void ExitAndStop(T mBase, bool pause, bool disabledEndEvent) { }
        #endregion

        /// <summary>
        /// 銷毀釋放
        /// </summary>
        /// <param name="mBase"></param>
        /// <param name="assetName"></param>
        protected virtual void Destroy(T mBase)
        {
            string assetName = mBase.assetName;

            // 調用釋放接口
            mBase.OnRelease();

            // 刪除物件
            if (!mBase.gameObject.IsDestroyed())
            {
                mBase.isDestroying = true;
                Destroy(mBase.gameObject);
            }

            // 刪除柱列緩存
            this._listAllCache.Remove(mBase);

            // 最後判斷是否要卸載
            if (mBase.onDestroyAndUnload)
            {
                // 取得柱列緩存中的群組數量
                int groupCount = this.GetMediaComponents<T>(assetName).Length;

                // 確保影音在卸載前, 是沒被引用的狀態
                if (groupCount == 0)
                {
                    // 刪除資源緩存 (皆使用 assetName 作為 key)
                    if (this.HasAssetInCache(assetName)) this._dictAssetCache.Remove(assetName);

                    // 卸載
                    AssetLoaders.UnloadAsset(assetName);

                    Logging.PrintInfo<Logger>($"[MediaManager] Unload Asset: {assetName}");
                }
            }

            Logging.PrintInfo<Logger>($"[MediaManager] Destroy Object: {assetName}, All Count: {this._listAllCache.Count}");
        }

        /// <summary>
        /// 強制卸載源頭資源
        /// </summary>
        /// <param name="assetName"></param>
        public virtual void ForceUnload(string assetName)
        {
            // 取得柱列緩存中的群組數量
            int groupCount = this.GetMediaComponents<T>(assetName).Length;

            // 判斷群組柱列緩存 > 0, 則全部強制關閉並且刪除
            if (groupCount > 0)
            {
                // 刪除全部柱列緩存
                this.StopAll(false, true);
            }

            // 刪除資源緩存 (皆使用 assetName 作為 key)
            if (this.HasAssetInCache(assetName)) this._dictAssetCache.Remove(assetName);

            // 卸載
            AssetLoaders.UnloadAsset(assetName);
        }
    }

    internal static class GameObjectExtensions
    {
        /// <summary>
        /// Checks if a GameObject has been destroyed.
        /// </summary>
        /// <param name="gameObject">GameObject reference to check for destructedness</param>
        /// <returns>If the game object has been marked as destroyed by UnityEngine</returns>
        public static bool IsDestroyed(this GameObject gameObject)
        {
            // UnityEngine overloads the == opeator for the GameObject type
            // and returns null when the object has been destroyed, but 
            // actually the object is still there but has not been cleaned up yet
            // if we test both we can determine if the object has been destroyed.
            return gameObject == null && !ReferenceEquals(gameObject, null);
        }
    }
}
