using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine;

namespace OxGFrame.AssetLoader.Cacher
{
    public class CacheResource : AssetCache<ResourcePack>, IResource
    {
        private static CacheResource _instance = null;
        public static CacheResource GetInstance()
        {
            if (_instance == null) _instance = new CacheResource();
            return _instance;
        }

        public override bool HasInCache(string assetName)
        {
            return this._cacher.ContainsKey(assetName);
        }

        public override ResourcePack GetFromCache(string assetName)
        {
            if (this.HasInCache(assetName))
            {
                if (this._cacher.TryGetValue(assetName, out ResourcePack resPack)) return resPack;
            }

            return null;
        }

        /// <summary>
        /// 預加載資源至快取中
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        public override async UniTask Preload(string assetName, Progression progression = null)
        {
            if (string.IsNullOrEmpty(assetName)) return;

            // 如果有進行 Loading 標記後, 直接 return
            if (this.HasInLoadingFlags(assetName))
            {
                Debug.Log($"<color=#FFDC8A>asset: {assetName} Loading...</color>");
                return;
            }

            // 先初始加載進度
            this.reqSize = 0;
            this.totalSize = this.GetAssetsLength(assetName);

            // Loading 標記
            this._hashLoadingFlags.Add(assetName);

            // 如果有在快取中就不進行預加載
            if (this.HasInCache(assetName))
            {
                // 在快取中請求大小就直接指定為資源總大小 (單個)
                this.reqSize = this.totalSize;
                // 處理進度回調
                progression?.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
                // 移除標記
                this._hashLoadingFlags.Remove(assetName);
                return;
            }

            ResourcePack resPack = new ResourcePack();
            {
                var req = Resources.LoadAsync<Object>(assetName);

                float lastSize = 0;
                while (req != null)
                {
                    if (progression != null)
                    {
                        req.completed += (AsyncOperation ao) =>
                        {
                            this.reqSize += ao.progress - lastSize;
                            lastSize += ao.progress;

                            progression.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
                        };
                    }

                    if (req.isDone)
                    {
                        resPack.assetName = assetName;
                        resPack.asset = req.asset;
                        break;
                    }

                    await UniTask.Yield();
                }
            }

            if (resPack != null)
            {
                // skipping duplicate keys
                if (!this.HasInCache(assetName)) this._cacher.Add(assetName, resPack);
            }

            // 移除標記
            this._hashLoadingFlags.Remove(assetName);

            Debug.Log($"<color=#ff9600>【Preload】 => Current << CacheResource >> Cache Count: {this.Count}, asset: {assetName}</color>");
        }

        public override async UniTask Preload(string[] assetNames, Progression progression = null)
        {
            if (assetNames == null || assetNames.Length == 0) return;

            // 先初始加載進度
            this.reqSize = 0;
            this.totalSize = this.GetAssetsLength(assetNames);

            for (int i = 0; i < assetNames.Length; i++)
            {
                var assetName = assetNames[i];

                if (string.IsNullOrEmpty(assetName)) continue;

                // 如果有進行 Loading 標記後, 直接 return
                if (this.HasInLoadingFlags(assetName))
                {
                    Debug.Log($"<color=#FFDC8A>asset: {assetName} Loading...</color>");
                    return;
                }

                // Loading 標記
                this._hashLoadingFlags.Add(assetName);

                // 如果有在快取中就不進行預加載
                if (this.HasInCache(assetName))
                {
                    // 在快取中請求進度大小需累加當前資源的總size (因為迴圈)
                    this.reqSize += this.GetAssetsLength(assetName);
                    // 處理進度回調
                    progression?.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
                    // 移除標記
                    this._hashLoadingFlags.Remove(assetName);
                    continue;
                }

                ResourcePack resPack = new ResourcePack();
                {
                    var req = Resources.LoadAsync<Object>(assetName);

                    float lastSize = 0;
                    while (req != null)
                    {
                        if (progression != null)
                        {
                            req.completed += (AsyncOperation ao) =>
                            {
                                this.reqSize += (ao.progress - lastSize);
                                lastSize = ao.progress;

                                progression.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
                            };
                        }

                        if (req.isDone)
                        {
                            resPack.assetName = assetName;
                            resPack.asset = req.asset;
                            break;
                        }

                        await UniTask.Yield();
                    }
                }

                if (resPack != null)
                {
                    // skipping duplicate keys
                    if (!this.HasInCache(assetName)) this._cacher.Add(assetName, resPack);
                }

                // 移除標記
                this._hashLoadingFlags.Remove(assetName);

                Debug.Log($"<color=#ff9600>【Preload】 => Current << CacheResource >> Cache Count: {this.Count}, asset: {assetName}</color>");
            }
        }

        /// <summary>
        /// [使用計數管理] 載入資源 => 會優先從快取中取得資源, 如果快取中沒有才進行資源加載
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        public async UniTask<T> Load<T>(string assetName, Progression progression = null) where T : Object
        {
            if (string.IsNullOrEmpty(assetName)) return null;

            // 如果有進行 Loading 標記後, 直接 return
            if (this.HasInLoadingFlags(assetName))
            {
                Debug.Log($"<color=#FFDC8A>asset: {assetName} Loading...</color>");
                return null;
            }

            // 初始加載進度
            this.reqSize = 0;
            this.totalSize = this.GetAssetsLength(assetName);

            // Loading 標記
            this._hashLoadingFlags.Add(assetName);

            // 先從快取拿
            ResourcePack resPack = this.GetFromCache(assetName);

            if (resPack == null)
            {
                resPack = new ResourcePack();
                {
                    var req = Resources.LoadAsync<Object>(assetName);

                    float lastSize = 0;
                    while (req != null)
                    {
                        if (progression != null)
                        {
                            req.completed += (AsyncOperation ao) =>
                            {
                                this.reqSize += (ao.progress - lastSize);
                                lastSize = ao.progress;

                                progression.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
                            };
                        }

                        if (req.isDone)
                        {
                            resPack.assetName = assetName;
                            resPack.asset = req.asset as T;
                            break;
                        }

                        await UniTask.Yield();
                    }
                }

                if (resPack != null)
                {
                    // skipping duplicate keys
                    if (!this.HasInCache(assetName)) this._cacher.Add(assetName, resPack);
                }
            }
            else
            {
                // 直接更新進度
                this.reqSize = this.totalSize;
                // 處理進度回調
                progression?.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
            }

            if (resPack.asset != null)
            {
                // 引用計數++
                resPack.AddRef();
            }

            this._hashLoadingFlags.Remove(assetName);

            Debug.Log($"<color=#90FF71>【Load】 => Current << CacheResource >> Cache Count: {this.Count}, asset: {assetName}, ref: {resPack.refCount}</color>");

            return (T)resPack.asset;
        }

        /// <summary>
        /// [使用計數管理] 從快取【釋放】單個資源 (釋放快取, 並且釋放資源記憶體)
        /// </summary>
        /// <param name="assetName"></param>
        public override void Unload(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return;

            if (this.HasInLoadingFlags(assetName))
            {
                Debug.Log($"<color=#FFDC8A>asset: {assetName} Loading...</color>");
                return;
            }

            if (this.HasInCache(assetName))
            {
                // 引用計數--
                this._cacher[assetName].DelRef();

                Debug.Log($"<color=#00e5ff>【<color=#ffcf92>Unload</color>】 => Current << CacheResource >> Cache Count: {this.Count}, asset: {assetName}, ref: {this._cacher.TryGetValue(assetName, out var v)} {v?.refCount}</color>");

                if (this._cacher[assetName].refCount <= 0)
                {
                    this._cacher[assetName] = null;
                    this._cacher.Remove(assetName);
                    Resources.UnloadUnusedAssets();

                    Debug.Log($"<color=#00e5ff>【<color=#ff92ef>Unload Completes</color>】 => Current << CacheResource >> Cache Count: {this.Count}, asset: {assetName}</color>");
                }
            }
        }

        /// <summary>
        /// [強制釋放] 從快取中【釋放】全部資源 (釋放快取, 並且釋放資源記憶體)
        /// </summary>
        public override void Release()
        {
            if (this.Count == 0) return;

            // 強制釋放快取與資源
            foreach (var assetName in this._cacher.Keys.ToArray())
            {
                if (this.HasInLoadingFlags(assetName))
                {
                    Debug.Log($"<color=#FFDC8A>asset: {assetName} Loading...</color>");
                    continue;
                }

                if (this.HasInCache(assetName))
                {
                    this._cacher[assetName] = null;
                    this._cacher.Remove(assetName);
                }
            }

            this._cacher.Clear();
            Resources.UnloadUnusedAssets();

            Debug.Log($"<color=#ff71b7>【Release All】 => Current << CacheResource >> Cache Count: {this.Count}</color>");
        }
    }
}