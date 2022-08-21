using Cysharp.Threading.Tasks;
using System.Collections.Generic;
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

        public CacheResource()
        {
            this._cacher = new Dictionary<string, ResourcePack>();
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

            // 先初始加載進度
            this.reqSize = 0;
            this.totalSize = await this.GetAssetsLength(assetName);

            // 如果有在快取中就不進行預加載
            if (this.HasInCache(assetName))
            {
                // 在快取中請求大小就直接指定為資源總大小 (單個)
                this.reqSize = this.totalSize;
                // 處理進度回調
                progression?.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
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

            Debug.Log("【預加載】 => 當前<< CacheResource >>快取數量 : " + this.Count);
        }

        public override async UniTask Preload(string[] assetNames, Progression progression = null)
        {
            if (assetNames == null || assetNames.Length == 0) return;

            // 先初始加載進度
            this.reqSize = 0;
            this.totalSize = await this.GetAssetsLength(assetNames);

            for (int i = 0; i < assetNames.Length; i++)
            {
                var assetName = assetNames[i];

                if (string.IsNullOrEmpty(assetName)) continue;

                // 如果有在快取中就不進行預加載
                if (this.HasInCache(assetName))
                {
                    // 在快取中請求進度大小需累加當前資源的總size (因為迴圈)
                    this.reqSize += await this.GetAssetsLength(assetName);
                    // 處理進度回調
                    progression?.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
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

                Debug.Log("【預加載】 => 當前<< CacheResource >>快取數量 : " + this.Count);
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
            // 初始加載進度
            this.reqSize = 0;
            this.totalSize = await this.GetAssetsLength(assetName);

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

            Debug.Log("【載入】 => 當前<< CacheResource >>快取數量 : " + this.Count);

            return (T)resPack.asset;
        }

        /// <summary>
        /// [使用計數管理] 從快取【釋放】單個資源 (釋放快取, 並且釋放資源記憶體)
        /// </summary>
        /// <param name="assetName"></param>
        public override void Unload(string assetName)
        {
            if (this.HasInCache(assetName))
            {
                // 引用計數--
                this._cacher[assetName].DelRef();
                if (this._cacher[assetName].refCount <= 0)
                {
                    //Resources.(this._cacher[assetName].asset); // 刪除快取前, 釋放資源
                    this._cacher[assetName] = null;
                    this._cacher.Remove(assetName);
                    Resources.UnloadUnusedAssets();
                }
            }

            Debug.Log("【單個釋放】 => 當前<< CacheResource >>快取數量 : " + this.Count);
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
                if (this.HasInCache(assetName))
                {
                    //Resources.UnloadAsset(this._cacher[assetName].asset); // 刪除快取前, 釋放資源
                    this._cacher[assetName] = null;
                    this._cacher.Remove(assetName);
                }
            }

            this._cacher.Clear();
            Resources.UnloadUnusedAssets();

            Debug.Log("【全部釋放】 => 當前<< CacheResource >>快取數量 : " + this.Count);
        }

        public override async UniTask<int> GetAssetsLength(params string[] assetNames)
        {
            return assetNames.Length;
        }
    }
}