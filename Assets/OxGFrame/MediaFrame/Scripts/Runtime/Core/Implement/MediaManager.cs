using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader.Cacher;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace OxGFrame.MediaFrame
{
    public abstract class MediaManager<T> : MonoBehaviour where T : MediaBase
    {
        protected Dictionary<string, GameObject> _dictAssetCache = new Dictionary<string, GameObject>();  // 【常駐】所有資源快取
        protected HashSet<string> _loadingFlags = new HashSet<string>();                                  // 用來標記正在加載中的資源 (暫存快取)
        protected List<T> _listAllCache = new List<T>();                                                  // 【常駐】所有進入播放的影音柱列快取 (只會在Destroy時, Remove對應的快取)

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

            GameObject go = null;
            if (this.HasAssetInCache(assetName)) this._dictAssetCache.TryGetValue(assetName, out go);
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

            List<T> founds = new List<T>();
            for (int i = 0; i < this._listAllCache.Count; i++)
            {
                if (this._listAllCache[i].assetName == assetName) founds.Add(this._listAllCache[i]);
            }

            return (U[])founds.ToArray();
        }

        public U[] FilterMediaComponents<U>(string mediaName, U[] mediaArray) where U : T
        {
            if (string.IsNullOrEmpty(mediaName) || mediaArray.Length == 0) return new U[] { };

            List<U> founds = new List<U>();
            for (int i = 0; i < mediaArray.Length; i++)
            {
                if (mediaArray[i].mediaName == mediaName) founds.Add(founds[i]);
            }

            return founds.ToArray();
        }

        /// <summary>
        /// 返回在 List 中該對應名稱的 Indexes
        /// </summary>
        /// <param name="listCache"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        protected int[] GetIndexesByName(List<T> listCache, string assetName)
        {
            List<int> indexes = new List<int>();
            for (int i = 0; i < listCache.Count; i++)
            {
                if (listCache[i].assetName == assetName) indexes.Add(i);
            }

            return indexes.ToArray();
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

            // 校正Transform
            nodeGo.transform.localScale = Vector3.one;
            nodeGo.transform.localPosition = Vector3.zero;
            nodeGo.transform.localRotation = Quaternion.identity;

            return nodeGo;
        }

        /// <summary>
        /// 實際運行加載物件資源 (Resource)
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        protected virtual async UniTask<GameObject> LoadGameObject(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return null;

            GameObject obj = await CacheResource.GetInstance().Load<GameObject>(assetName);
            if (obj == null)
            {
                Debug.LogWarning(string.Format("【 path: {0} 】此路徑找不到媒體資源!!!", assetName));
                return null;
            }

            return obj;
        }

        /// <summary>
        /// 實際運行加載物件資源 (AssetBundle)
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        protected virtual async UniTask<GameObject> LoadGameObject(string bundleName, string assetName)
        {
            if (string.IsNullOrEmpty(bundleName) && string.IsNullOrEmpty(assetName)) return null;

            GameObject obj = await CacheBundle.GetInstance().Load<GameObject>(bundleName, assetName);
            if (obj == null)
            {
                Debug.LogWarning(string.Format("【 ab: {0}, asset: {1} 】此路徑找不到媒體資源!!!", bundleName, assetName));
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

        protected virtual async UniTask<GameObject> LoadingAsset(string bundleName, string assetName)
        {
            GameObject go = await this.LoadGameObject(bundleName, assetName);
            if (go == null) return null;

            this._dictAssetCache.Add(assetName, go);

            return go;
        }

        /// <summary>
        /// 無資源則加載, 反之有的話就從資源快取中返回資源
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        protected async UniTask<GameObject> LoadAssetIntoCache(string assetName)
        {
            GameObject go;
            // 判斷不在AllCache中, 也不在LoadingFlags中, 才進行加載程序
            if (!this.HasAssetInCache(assetName) && !this.HasInLoadingFlags(assetName))
            {
                this._loadingFlags.Add(assetName);       // 標記LoadingFlag
                go = await this.LoadingAsset(assetName); // 開始加載
                this._loadingFlags.Remove(assetName);    // 移除LoadingFlag
            }
            else go = this.GetAssetFromCache(assetName); // 如果判斷沒有要執行加載程序, 就直接從AllCache中取得

            return go;
        }

        protected async UniTask<GameObject> LoadAssetIntoCache(string bundleName, string assetName)
        {
            GameObject go;
            // 判斷不在AllCache中, 也不在LoadingFlags中, 才進行加載程序
            if (!this.HasAssetInCache(assetName) && !this.HasInLoadingFlags(assetName))
            {
                this._loadingFlags.Add(assetName);                   // 標記LoadingFlag
                go = await this.LoadingAsset(bundleName, assetName); // 開始加載
                this._loadingFlags.Remove(assetName);                // 移除LoadingFlag
            }
            else go = this.GetAssetFromCache(assetName);             // 如果判斷沒有要執行加載程序, 就直接從AllCache中取得

            return go;
        }

        /// <summary>
        /// 實例化媒體組件
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="go"></param>
        /// <returns></returns>
        protected async virtual UniTask<U> CloneAsset<U>(string bundleName, string assetName, GameObject go, Transform parent) where U : T
        {
            if (go == null) return default;

            GameObject instGo = Instantiate(go, parent);
            instGo.name = instGo.name.Replace("(Clone)", "");
            U mBase = instGo.GetComponent<U>();
            if (mBase == null) return default;

            // 激活檢查, 如果主體Active為false必須打開
            if (!instGo.activeSelf) instGo.SetActive(true);

            this._listAllCache.Add(mBase); // 先加入快取
            this.SetParent(mBase);

            // 設置管理名稱
            string mediaName = assetName;
            mediaName = (mediaName.IndexOf('/') != -1) ? mediaName.Substring(mediaName.LastIndexOf('/')).Replace("/", string.Empty) : mediaName;
            mBase.SetNames(bundleName, assetName, mediaName);

            await mBase.Init();
            var cts = new CancellationTokenSource();
            cts.CancelAfterSlim(TimeSpan.FromSeconds(5));
            try
            {
                await UniTask.WaitUntil(() => { return mBase.isPrepared; }, PlayerLoopTiming.LastUpdate, cts.Token);
            }
            catch (OperationCanceledException ex)
            {
                if (ex.CancellationToken == cts.Token)
                {
                    Debug.Log($"{mBase.mediaName} Media init WaitUntil Timeout");
                }
            }

            // >>> 需在Init之後, 以下設定開始生效 <<<

            mBase.gameObject.SetActive(false);

            return mBase;
        }

        public async UniTask Preload(string assetName)
        {
            if (!string.IsNullOrEmpty(assetName)) await this.LoadAssetIntoCache(assetName);

            await UniTask.Yield();
        }

        public async UniTask Preload(string bundleName, string assetName)
        {
            if (!string.IsNullOrEmpty(bundleName) && !string.IsNullOrEmpty(assetName))
            {
                await this.LoadAssetIntoCache(bundleName, assetName);
            }

            await UniTask.Yield();
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

            await UniTask.Yield();
        }

        public async UniTask Preload(string[,] bundleAssetNames)
        {
            if (bundleAssetNames.Length > 0)
            {
                for (int row = 0; row < bundleAssetNames.GetLength(0); row++)
                {
                    if (bundleAssetNames.GetLength(1) != 2) continue;
                    else if (string.IsNullOrEmpty(bundleAssetNames[row, 0]) || string.IsNullOrEmpty(bundleAssetNames[row, 1])) continue;
                    await this.LoadAssetIntoCache(bundleAssetNames[row, 0], bundleAssetNames[row, 1]);
                }
            }

            await UniTask.Yield();
        }

        protected virtual void SetParent(T mBase) { }

        #region Play
        public abstract UniTask<T[]> Play(string assetName, int loops = 0);
        public abstract UniTask<T[]> Play(string bundleName, string assetName, int loops = 0);
        public abstract void ResumeAll();
        #endregion

        #region Stop & Pause
        public abstract void Stop(string assetName, bool disableEndEvent = false, bool withDestroy = false);
        public abstract void StopAll(bool disableEndEvent = false, bool withDestroy = false);
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
        protected virtual void Destroy(T mBase, string assetName)
        {
            if (string.IsNullOrEmpty(mBase.bundleName)) CacheResource.GetInstance().ReleaseFromCache(mBase.assetName);
            else CacheBundle.GetInstance().ReleaseFromCache(mBase.bundleName);

            mBase.OnRelease();

            if (!mBase.gameObject.IsDestroyed()) Destroy(mBase.gameObject);                // 刪除MediaBase物件
            this._listAllCache.Remove(mBase);                                             // 刪除MediaBase柱列快取
            if (this.HasAssetInCache(assetName)) this._dictAssetCache.Remove(assetName); // 刪除資源快取

            Debug.Log(string.Format("Destroy Media: {0}", assetName));
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
