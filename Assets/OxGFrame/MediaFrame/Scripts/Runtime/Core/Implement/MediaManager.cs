using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader;
using System.Collections.Generic;
using UnityEngine;

namespace OxGFrame.MediaFrame
{
    public abstract class MediaManager<T> : MonoBehaviour where T : MediaBase
    {
        protected Dictionary<string, GameObject> _dictAssetCache = new Dictionary<string, GameObject>();  // 【常駐】所有資源快取
        protected HashSet<string> _loadingFlags = new HashSet<string>();                                  // 用來標記正在加載中的資源 (暫存快取)
        protected List<T> _listAllCache = new List<T>();                                                  // 【常駐】所有進入播放的影音柱列快取 (只會在 Destroy 時, Remove 對應的快取)

        /// <summary>
        /// 檢查是否有資源快取
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public bool HasAssetInCache(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return false;
            return this._dictAssetCache.ContainsKey(assetName);
        }

        /// <summary>
        /// 加載中的標記快取
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        protected bool HasInLoadingFlags(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return false;
            return this._loadingFlags.Contains(assetName);
        }

        /// <summary>
        /// 從快取中取的資源
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
        /// <param name="assetName"></param>
        /// <returns></returns>
        protected virtual async UniTask<GameObject> LoadGameObject(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return null;

            GameObject obj = await AssetLoaders.LoadAssetAsync<GameObject>(assetName);

            if (obj == null)
            {
                Debug.LogWarning(string.Format("【 path: {0} 】asset not found at this path!!!", assetName));
                return null;
            }

            return obj;
        }

        /// <summary>
        /// 加載資源至資源快取中
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        protected virtual async UniTask<GameObject> LoadingAsset(string assetName)
        {
            GameObject go = await this.LoadGameObject(assetName);
            if (go == null) return null;

            this._dictAssetCache.Add(assetName, go);

            return go;
        }

        /// <summary>
        /// 加載資源至快取, 判斷是否已有加載過, 如果有則返回該資源物件
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        protected async UniTask<GameObject> LoadAssetIntoCache(string assetName)
        {
            GameObject go;
            // 判斷不在 AllCache 中, 也不在 LoadingFlags 中, 才進行加載程序
            if (!this.HasAssetInCache(assetName) && !this.HasInLoadingFlags(assetName))
            {
                this._loadingFlags.Add(assetName);       // 標記 LoadingFlag
                go = await this.LoadingAsset(assetName); // 開始加載
                this._loadingFlags.Remove(assetName);    // 移除 LoadingFlag
            }
            else go = this.GetAssetFromCache(assetName); // 如果判斷沒有要執行加載程序, 就直接從 AllCache 中取得

            return go;
        }

        /// <summary>
        /// 實例化媒體組件
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="go"></param>
        /// <returns></returns>
        protected async virtual UniTask<U> CloneAsset<U>(string assetName, GameObject go, Transform parent, Transform spwanParent) where U : T
        {
            if (go == null) return default;

            GameObject instGo = Instantiate(go, (parent != null) ? parent : spwanParent);
            instGo.name = instGo.name.Replace("(Clone)", "");
            U mBase = instGo.GetComponent<U>();
            if (mBase == null) return default;

            // 激活檢查, 如果主體 Active 為 false 必須打開
            if (!instGo.activeSelf) instGo.SetActive(true);

            this._listAllCache.Add(mBase); // 先加入快取
            this.SetParent(mBase, parent);

            // 設置管理名稱
            string mediaName = assetName;
            mediaName = (mediaName.IndexOf('/') != -1) ? mediaName.Substring(mediaName.LastIndexOf('/')).Replace("/", string.Empty) : mediaName;
            mBase.SetNames(assetName, mediaName);

            await mBase.Init();

            // >>> 需在 Init 之後, 以下設定開始生效 <<<

            if (mBase == null || mBase.gameObject.IsDestroyed()) return default;

            mBase.gameObject.SetActive(false);

            return mBase;
        }

        public async UniTask Preload(string assetName)
        {
            if (!string.IsNullOrEmpty(assetName)) await this.LoadAssetIntoCache(assetName);
        }

        /// <summary>
        /// 資源預加載
        /// </summary>
        /// <param name="assetNames"></param>
        /// <returns></returns>
        public async UniTask Preload(string[] assetNames)
        {
            if (assetNames.Length > 0)
            {
                for (int i = 0; i < assetNames.Length; i++)
                {
                    if (string.IsNullOrEmpty(assetNames[i])) continue;
                    await this.LoadAssetIntoCache(assetNames[i]);
                }
            }
        }

        protected virtual void SetParent(T mBase, Transform parent) { }

        #region Play
        public abstract UniTask<T[]> Play(string assetName, Transform parent = null, int loops = 0);
        public abstract void ResumeAll();
        #endregion

        #region Stop & Pause
        public abstract void Stop(string assetName, bool disableEndEvent = false, bool forceDestroy = false);
        public abstract void StopAll(bool disableEndEvent = false, bool forceDestroy = false);
        public abstract void Pause(string assetName);
        public abstract void PauseAll();
        #endregion

        #region Load Play & Exit Stop
        protected virtual void LoadAndPlay(T mBase, int loops) { }

        protected virtual void ExitAndStop(T mBase, bool pause, bool disableEndEvent) { }
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
            if (!mBase.gameObject.IsDestroyed()) Destroy(mBase.gameObject);

            // 刪除柱列快取
            this._listAllCache.Remove(mBase);

            // 最後判斷是否要卸載
            if (mBase.onDestroyAndUnload)
            {
                // 取得柱列快取中的群組數量
                int groupCount = this.GetMediaComponents<T>(assetName).Length;

                // 確保影音在卸載前, 是沒被引用的狀態
                if (groupCount == 0)
                {
                    // 刪除資源快取 (皆使用 assetName 作為 key)
                    if (this.HasAssetInCache(assetName)) this._dictAssetCache.Remove(assetName);

                    // 卸載
                    AssetLoaders.UnloadAsset(assetName);

                    Debug.Log($"<color=#ffb6db>[MediaManager] Unload Asset: {assetName}</color>");
                }
            }

            Debug.Log($"<color=#ff9d55>[MediaManager] Destroy Object: {assetName}, <color=#ffdc55>All Count: {this._listAllCache.Count}</color></color>");
        }

        /// <summary>
        /// 強制卸載源頭資源
        /// </summary>
        /// <param name="assetName"></param>
        public virtual void ForceUnload(string assetName)
        {
            // 取得柱列快取中的群組數量
            int groupCount = this.GetMediaComponents<T>(assetName).Length;

            // 判斷群組柱列快取 > 0, 則全部強制關閉並且刪除
            if (groupCount > 0)
            {
                // 刪除全部柱列快取
                this.StopAll(false, true);
            }

            // 刪除資源快取 (皆使用 assetName 作為 key)
            if (this.HasAssetInCache(assetName)) this._dictAssetCache.Remove(assetName);

            // 卸載
            AssetLoaders.UnloadAsset(assetName);
        }
    }

    public static class GameObjectExtensions
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
