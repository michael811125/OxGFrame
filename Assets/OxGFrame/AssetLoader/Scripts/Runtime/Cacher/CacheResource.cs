using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader.Utility;
using System.Linq;
using UnityEngine;
using YooAsset;

namespace OxGFrame.AssetLoader.Cacher
{
    internal class CacheResource : AssetCache<ResourcePack>
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
                if (this._cacher.TryGetValue(assetName, out ResourcePack pack)) return pack;
            }

            return null;
        }

        public async UniTask PreloadAssetAsync(string assetName, Progression progression = null)
        {
            await this.PreloadAssetAsync(new string[] { assetName }, progression);
        }

        public async UniTask PreloadAssetAsync(string[] assetNames, Progression progression = null)
        {
            if (assetNames == null || assetNames.Length == 0) return;

            // 先初始加載進度
            this.reqSize = 0;
            this.totalSize = assetNames.Length;

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
                    this.reqSize++;
                    progression?.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
                    this._hashLoadingFlags.Remove(assetName);
                    continue;
                }

                ResourcePack pack = new ResourcePack();
                {
                    var req = Resources.LoadAsync<Object>(assetName);

                    if (req != null)
                    {
                        float lastSize = 0;
                        do
                        {
                            if (progression != null)
                            {
                                this.reqSize += (req.progress - lastSize);
                                lastSize = req.progress;
                                progression.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
                            }

                            if (req.isDone)
                            {
                                pack.assetName = assetName;
                                pack.asset = req.asset;
                                break;
                            }
                            await UniTask.Yield();
                        } while (true);
                    }
                }

                if (pack != null)
                {
                    // skipping duplicate keys
                    if (!this.HasInCache(assetName)) this._cacher.Add(assetName, pack);
                }

                // 移除標記
                this._hashLoadingFlags.Remove(assetName);

                Debug.Log($"<color=#ff9600>【Preload】 => Current << CacheResource >> Cache Count: {this.Count}, asset: {assetName}</color>");
            }
        }

        public void PreloadAsset(string assetName, Progression progression = null)
        {
            this.PreloadAsset(new string[] { assetName }, progression);
        }

        public void PreloadAsset(string[] assetNames, Progression progression = null)
        {
            if (assetNames == null || assetNames.Length == 0) return;

            // 先初始加載進度
            this.reqSize = 0;
            this.totalSize = assetNames.Length;

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
                    this.reqSize++;
                    progression?.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
                    this._hashLoadingFlags.Remove(assetName);
                    continue;
                }

                ResourcePack pack = new ResourcePack();
                {
                    var req = Resources.Load<Object>(assetName);

                    this.reqSize++;
                    progression?.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);

                    pack.assetName = assetName;
                    pack.asset = req;
                }

                if (pack != null)
                {
                    // skipping duplicate keys
                    if (!this.HasInCache(assetName)) this._cacher.Add(assetName, pack);
                }

                // 移除標記
                this._hashLoadingFlags.Remove(assetName);

                Debug.Log($"<color=#ff9600>【Preload】 => Current << CacheResource >> Cache Count: {this.Count}, asset: {assetName}</color>");
            }
        }

        public async UniTask<T> LoadAssetAsync<T>(string assetName, Progression progression = null) where T : Object
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
            this.totalSize = 1;

            // Loading 標記
            this._hashLoadingFlags.Add(assetName);

            // 先從快取拿
            ResourcePack pack = this.GetFromCache(assetName);

            if (pack == null)
            {
                pack = new ResourcePack();
                {
                    var req = Resources.LoadAsync<Object>(assetName);

                    if (req != null)
                    {
                        float lastSize = 0;
                        do
                        {
                            if (progression != null)
                            {
                                this.reqSize += (req.progress - lastSize);
                                lastSize = req.progress;
                                progression.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
                            }

                            if (req.isDone)
                            {
                                pack.assetName = assetName;
                                pack.asset = req.asset;
                                break;
                            }
                            await UniTask.Yield();
                        } while (true);
                    }
                }

                if (pack != null)
                {
                    // skipping duplicate keys
                    if (!this.HasInCache(assetName)) this._cacher.Add(assetName, pack);
                }
            }
            else
            {
                this.reqSize = this.totalSize;
                progression?.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
            }

            if (pack.asset != null)
            {
                // 引用計數++
                pack.AddRef();
            }

            this._hashLoadingFlags.Remove(assetName);

            Debug.Log($"<color=#90FF71>【Load】 => Current << CacheResource >> Cache Count: {this.Count}, asset: {assetName}, ref: {pack.refCount}</color>");

            return pack.GetAsset<T>();
        }

        public T LoadAsset<T>(string assetName, Progression progression = null) where T : Object
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
            this.totalSize = 1;

            // Loading 標記
            this._hashLoadingFlags.Add(assetName);

            // 先從快取拿
            ResourcePack pack = this.GetFromCache(assetName);

            if (pack == null)
            {
                pack = new ResourcePack();
                {
                    var req = Resources.Load<Object>(assetName);

                    this.reqSize = this.totalSize;
                    progression?.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);

                    pack.assetName = assetName;
                    pack.asset = req as T;
                }

                if (pack != null)
                {
                    // skipping duplicate keys
                    if (!this.HasInCache(assetName)) this._cacher.Add(assetName, pack);
                }
            }
            else
            {
                this.reqSize = this.totalSize;
                progression?.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
            }

            if (pack.asset != null)
            {
                // 引用計數++
                pack.AddRef();
            }

            this._hashLoadingFlags.Remove(assetName);

            Debug.Log($"<color=#90FF71>【Load】 => Current << CacheResource >> Cache Count: {this.Count}, asset: {assetName}, ref: {pack.refCount}</color>");

            return pack.GetAsset<T>();
        }

        public void UnloadAsset(string assetName, bool forceUnload = false)
        {
            if (string.IsNullOrEmpty(assetName)) return;

            if (this.HasInLoadingFlags(assetName))
            {
                Debug.Log($"<color=#FFDC8A>asset: {assetName} Loading...</color>");
                return;
            }

            if (this.HasInCache(assetName))
            {
                ResourcePack pack = this.GetFromCache(assetName);

                // 引用計數--
                pack.DelRef();

                Debug.Log($"<color=#00e5ff>【<color=#ffcf92>Unload</color>】 => Current << CacheResource >> Cache Count: {this.Count}, asset: {assetName}, ref: {this._cacher.TryGetValue(assetName, out var v)} {v?.refCount}</color>");

                if (forceUnload)
                {
                    this._cacher[assetName] = null;
                    this._cacher.Remove(assetName);
                    Resources.UnloadUnusedAssets();

                    Debug.Log($"<color=#00e5ff>【<color=#ff92ef>Force Unload Completes</color>】 => Current << CacheResource >> Cache Count: {this.Count}, asset: {assetName}</color>");
                }
                else if (this._cacher[assetName].refCount <= 0)
                {
                    this._cacher[assetName] = null;
                    this._cacher.Remove(assetName);
                    Resources.UnloadUnusedAssets();

                    Debug.Log($"<color=#00e5ff>【<color=#ff92ef>Unload Completes</color>】 => Current << CacheResource >> Cache Count: {this.Count}, asset: {assetName}</color>");
                }
            }
        }

        public void ReleaseAssets()
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