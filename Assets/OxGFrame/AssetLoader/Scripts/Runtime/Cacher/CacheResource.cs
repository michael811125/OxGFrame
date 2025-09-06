using Cysharp.Threading.Tasks;
using OxGKit.LoggingSystem;
using System.Linq;
using UnityEngine;

namespace OxGFrame.AssetLoader.Cacher
{
    internal class CacheResource : AssetCache<ResourcePack>
    {
        private static CacheResource _instance = null;
        public static CacheResource GetInstance()
        {
            if (_instance == null)
                _instance = new CacheResource();
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

        #region Asset
        public async UniTask PreloadAssetAsync<T>(string[] assetNames, Progression progression, byte maxRetryCount) where T : Object
        {
            if (assetNames == null || assetNames.Length == 0) return;

            // 先初始加載進度
            this.currentCount = 0;
            this.totalCount = assetNames.Length;

            for (int i = 0; i < assetNames.Length; i++)
            {
                var assetName = assetNames[i];

                if (string.IsNullOrEmpty(assetName)) continue;

                // 如果有進行 Loading 標記後, 直接 return
                if (this.HasInLoadingFlags(assetName) && !this.GetRetryCounter(assetName).IsRetryValid())
                {
                    if (this.GetRetryCounter(assetName).IsRetryActive())
                    {
                        this.RemoveLoadingFlags(assetName);
                        Logging.PrintWarning<Logger>($"Asset: {assetName} Load failed and cannot retry anymore!!! Please to check asset is existing.");
                    }
                    else Logging.PrintWarning<Logger>($"Asset: {assetName} Loading...");
                    return;
                }

                // Loading 標記
                this.AddLoadingFlags(assetName, maxRetryCount);

                // 如果有在緩存中就不進行預加載
                if (this.HasInCache(assetName))
                {
                    this.currentCount++;
                    progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);
                    this.RemoveLoadingFlags(assetName);
                    Logging.PrintWarning<Logger>($"【Preload】 => Current << CacheResource >> Cache Count: {this.Count}, asset: [{assetName}] already preloaded!!!");
                    continue;
                }

                bool loaded = false;
                ResourcePack pack = new ResourcePack();

                var req = Resources.LoadAsync<T>(assetName);
                if (req != null)
                {
                    float lastCount = 0;
                    do
                    {
                        this.currentCount += (req.progress - lastCount);
                        lastCount = req.progress;
                        progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);

                        if (req.isDone)
                        {
                            loaded = true;
                            pack.SetPack(assetName, req.asset);
                            break;
                        }
                        await UniTask.Yield();
                    } while (true);
                }

                if (loaded)
                {
                    // skipping duplicate keys
                    if (!this.HasInCache(assetName))
                    {
                        this._cacher.Add(assetName, pack);
                        Logging.Print<Logger>($"【Preload】 => Current << CacheResource >> Cache Count: {this.Count}, asset: {assetName}");
                    }
                }
                else
                {
                    if (this.GetRetryCounter(assetName).IsRetryActive()) Logging.Print<Logger>($"【Preload】 => << CacheResource >> Asset: {assetName} doing retry. Retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}");
                    else Logging.Print<Logger>($"【Preload】 => << CacheResource >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}");
                    this.GetRetryCounter(assetName).AddRetryCount();
                    await this.PreloadAssetAsync<T>(new string[] { assetName }, progression, maxRetryCount);
                    continue;
                }

                // 移除標記
                this.RemoveLoadingFlags(assetName);
            }
        }

        public void PreloadAsset<T>(string[] assetNames, Progression progression, byte maxRetryCount) where T : Object
        {
            if (assetNames == null || assetNames.Length == 0) return;

            // 先初始加載進度
            this.currentCount = 0;
            this.totalCount = assetNames.Length;

            for (int i = 0; i < assetNames.Length; i++)
            {
                var assetName = assetNames[i];

                if (string.IsNullOrEmpty(assetName)) continue;

                // 如果有進行 Loading 標記後, 直接 return
                if (this.HasInLoadingFlags(assetName) && !this.GetRetryCounter(assetName).IsRetryValid())
                {
                    if (this.GetRetryCounter(assetName).IsRetryActive())
                    {
                        this.RemoveLoadingFlags(assetName);
                        Logging.PrintWarning<Logger>($"Asset: {assetName} Load failed and cannot retry anymore!!! Please to check asset is existing.");
                    }
                    else Logging.PrintWarning<Logger>($"Asset: {assetName} Loading...");
                    return;
                }

                // Loading 標記
                this.AddLoadingFlags(assetName, maxRetryCount);

                // 如果有在緩存中就不進行預加載
                if (this.HasInCache(assetName))
                {
                    this.currentCount++;
                    progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);
                    this.RemoveLoadingFlags(assetName);
                    Logging.PrintWarning<Logger>($"【Preload】 => Current << CacheResource >> Cache Count: {this.Count}, asset: [{assetName}] already preloaded!!!");
                    continue;
                }

                bool loaded = false;
                ResourcePack pack = new ResourcePack();

                var asset = Resources.Load<T>(assetName);
                if (asset != null)
                {
                    this.currentCount++;
                    progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);

                    loaded = true;
                    pack.SetPack(assetName, asset);
                }

                if (loaded)
                {
                    // skipping duplicate keys
                    if (!this.HasInCache(assetName))
                    {
                        this._cacher.Add(assetName, pack);
                        Logging.Print<Logger>($"【Preload】 => Current << CacheResource >> Cache Count: {this.Count}, asset: {assetName}");
                    }
                }
                else
                {
                    if (this.GetRetryCounter(assetName).IsRetryActive()) Logging.Print<Logger>($"【Preload】 => << CacheResource >> Asset: {assetName} doing retry. Retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}");
                    else Logging.Print<Logger>($"【Preload】 => << CacheResource >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}");
                    this.GetRetryCounter(assetName).AddRetryCount();
                    this.PreloadAsset<T>(new string[] { assetName }, progression, maxRetryCount);
                    continue;
                }

                // 移除標記
                this.RemoveLoadingFlags(assetName);
            }
        }

        public async UniTask<T> LoadAssetAsync<T>(string assetName, Progression progression, byte maxRetryCount) where T : Object
        {
            if (string.IsNullOrEmpty(assetName)) return null;

            // 如果有進行 Loading 標記後, 直接 return
            if (this.HasInLoadingFlags(assetName) && !this.GetRetryCounter(assetName).IsRetryValid())
            {
                if (this.GetRetryCounter(assetName).IsRetryActive())
                {
                    this.RemoveLoadingFlags(assetName);
                    Logging.PrintWarning<Logger>($"Asset: {assetName} Load failed and cannot retry anymore!!! Please to check asset is existing.");
                }
                else Logging.PrintWarning<Logger>($"Asset: {assetName} Loading...");
                return null;
            }

            // 初始加載進度
            this.currentCount = 0;
            this.totalCount = 1;

            // Loading 標記
            this.AddLoadingFlags(assetName, maxRetryCount);

            // 先從緩存拿
            ResourcePack pack = this.GetFromCache(assetName);

            if (pack == null)
            {
                bool loaded = false;
                pack = new ResourcePack();

                var req = Resources.LoadAsync<T>(assetName);
                if (req != null)
                {
                    float lastCount = 0;
                    do
                    {
                        this.currentCount += (req.progress - lastCount);
                        lastCount = req.progress;
                        progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);

                        if (req.isDone)
                        {
                            loaded = true;
                            pack.SetPack(assetName, req.asset);
                            break;
                        }
                        await UniTask.Yield();
                    } while (true);
                }

                if (loaded)
                {
                    // skipping duplicate keys
                    if (!this.HasInCache(assetName)) this._cacher.Add(assetName, pack);
                }
            }
            else
            {
                this.currentCount = this.totalCount;
                progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);
            }

            var asset = pack.GetAsset<T>();
            if (asset != null)
            {
                // 引用計數++
                pack.AddRef();
                Logging.Print<Logger>($"【Load】 => Current << CacheResource >> Cache Count: {this.Count}, asset: {assetName}, ref: {pack.refCount}");
            }
            else
            {
                this.UnloadAsset(assetName, true);
                if (this.GetRetryCounter(assetName).IsRetryActive()) Logging.Print<Logger>($"【Load】 => << CacheResource >> Asset: {assetName} doing retry. Retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}");
                else Logging.Print<Logger>($"【Load】 => << CacheResource >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}");
                this.GetRetryCounter(assetName).AddRetryCount();
                return await this.LoadAssetAsync<T>(assetName, progression, maxRetryCount);
            }

            this.RemoveLoadingFlags(assetName);

            return asset;
        }

        public T LoadAsset<T>(string assetName, Progression progression, byte maxRetryCount) where T : Object
        {
            if (string.IsNullOrEmpty(assetName)) return null;

            // 如果有進行 Loading 標記後, 直接 return
            if (this.HasInLoadingFlags(assetName) && !this.GetRetryCounter(assetName).IsRetryValid())
            {
                if (this.GetRetryCounter(assetName).IsRetryActive())
                {
                    this.RemoveLoadingFlags(assetName);
                    Logging.PrintWarning<Logger>($"Asset: {assetName} Load failed and cannot retry anymore!!! Please to check asset is existing.");
                }
                else Logging.PrintWarning<Logger>($"Asset: {assetName} Loading...");
                return null;
            }

            // 初始加載進度
            this.currentCount = 0;
            this.totalCount = 1;

            // Loading 標記
            this.AddLoadingFlags(assetName, maxRetryCount);

            // 先從緩存拿
            ResourcePack pack = this.GetFromCache(assetName);
            T asset = default;

            if (pack == null)
            {
                bool loaded = false;
                pack = new ResourcePack();

                asset = Resources.Load<T>(assetName);
                if (asset != null)
                {
                    this.currentCount = this.totalCount;
                    progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);

                    loaded = true;
                    pack.SetPack(assetName, asset);
                }

                if (loaded)
                {
                    // skipping duplicate keys
                    if (!this.HasInCache(assetName)) this._cacher.Add(assetName, pack);
                }
            }
            else
            {
                this.currentCount = this.totalCount;
                progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);
            }

            asset = pack.GetAsset<T>();
            if (asset != null)
            {
                // 引用計數++
                pack.AddRef();
                Logging.Print<Logger>($"【Load】 => Current << CacheResource >> Cache Count: {this.Count}, asset: {assetName}, ref: {pack.refCount}");
            }
            else
            {
                this.UnloadAsset(assetName, true);
                if (this.GetRetryCounter(assetName).IsRetryActive()) Logging.Print<Logger>($"【Load】 => << CacheResource >> Asset: {assetName} doing retry. Retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}");
                else Logging.Print<Logger>($"【Load】 => << CacheResource >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}");
                this.GetRetryCounter(assetName).AddRetryCount();
                return this.LoadAsset<T>(assetName, progression, maxRetryCount);
            }

            this.RemoveLoadingFlags(assetName);

            return asset;
        }

        public void UnloadAsset(string assetName, bool forceUnload)
        {
            if (string.IsNullOrEmpty(assetName)) return;

            if (this.HasInLoadingFlags(assetName) && !this.GetRetryCounter(assetName).IsRetryValid())
            {
                Logging.PrintWarning<Logger>($"Asset: {assetName} Loading...");
                return;
            }

            if (this.HasInCache(assetName))
            {
                ResourcePack pack = this.GetFromCache(assetName);

                // 引用計數--
                pack.DelRef();

                Logging.Print<Logger>($"【Unload】 => Current << CacheResource >> Cache Count: {this.Count}, asset: {assetName}, ref: {this._cacher.TryGetValue(assetName, out var v)} {v?.refCount}");

                if (forceUnload)
                {
                    this._cacher[assetName] = null;
                    this._cacher.Remove(assetName);
                    Resources.UnloadUnusedAssets();

                    Logging.PrintInfo<Logger>($"【Force Unload Completes】 => Current << CacheResource >> Cache Count: {this.Count}, asset: {assetName}");
                }
                else if (this._cacher[assetName].refCount <= 0)
                {
                    this._cacher[assetName] = null;
                    this._cacher.Remove(assetName);
                    Resources.UnloadUnusedAssets();

                    Logging.PrintInfo<Logger>($"【Unload Completes】 => Current << CacheResource >> Cache Count: {this.Count}, asset: {assetName}");
                }
            }
        }

        public void ReleaseAssets()
        {
            if (this.Count == 0) return;

            // 強制釋放緩存與資源
            foreach (var assetName in this._cacher.Keys.ToArray())
            {
                if (this.HasInCache(assetName))
                {
                    this.UnloadAsset(assetName, true);
                }
            }

            this._cacher.Clear();
            Resources.UnloadUnusedAssets();

            Logging.PrintInfo<Logger>($"【Release All】 => Current << CacheResource >> Cache Count: {this.Count}");
        }
        #endregion
    }
}