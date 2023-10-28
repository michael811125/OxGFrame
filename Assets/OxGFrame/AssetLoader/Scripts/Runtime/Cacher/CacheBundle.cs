using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader.Bundle;
using OxGKit.LoggingSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;

namespace OxGFrame.AssetLoader.Cacher
{
    internal class CacheBundle : AssetCache<BundlePack>
    {
        private Dictionary<string, BundlePack> _sceneCache; // 緩存 Scene BundlePack
        private Dictionary<string, int> _sceneCounter;      // 子場景堆疊式計數緩存

        public CacheBundle() : base()
        {
            this._sceneCache = new Dictionary<string, BundlePack>();
            this._sceneCounter = new Dictionary<string, int>();
        }

        private static CacheBundle _instance = null;
        public static CacheBundle GetInstance()
        {
            if (_instance == null) _instance = new CacheBundle();
            return _instance;
        }

        public override bool HasInCache(string assetName)
        {
            return this._cacher.ContainsKey(assetName);
        }

        public override BundlePack GetFromCache(string assetName)
        {
            if (this.HasInCache(assetName))
            {
                if (this._cacher.TryGetValue(assetName, out BundlePack pack)) return pack;
            }

            return null;
        }

        #region RawFile
        public async UniTask PreloadRawFileAsync(string packageName, string[] assetNames, uint priority, Progression progression, byte maxRetryCount)
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
                        Logging.Print<Logger>($"<color=#ff9b3e>Asset: {assetName} Load failed and cannot retry anymore!!! Please to check asset is existing.</color>");
                    }
                    else Logging.Print<Logger>($"<color=#ff9b3e>Asset: {assetName} Loading...</color>");
                    return;
                }

                // Loading 標記
                this.AddLoadingFlags(assetName, maxRetryCount);

                // 如果有在緩存中就不進行預加載
                if (this.HasInCache(assetName))
                {
                    BundlePack pack = this.GetFromCache(assetName);
                    if (pack.IsRawFileOperationHandleValid())
                    {
                        this.currentCount++;
                        progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);
                        this.RemoveLoadingFlags(assetName);
                        Logging.Print<Logger>($"<color=#fff6ba>【Preload】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: [{assetName}] already preloaded!!!</color>");
                        continue;
                    }
                    else
                    {
                        this.UnloadRawFile(assetName, true);
                        if (this.GetRetryCounter(assetName).IsRetryActive()) Logging.Print<Logger>($"<color=#f7ff3e>【Preload】 => << CacheBundle >> Asset: {assetName} doing retry. Retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}</color>");
                        else Logging.Print<Logger>($"<color=#f7ff3e>【Preload】 => << CacheBundle >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}</color>");
                        this.GetRetryCounter(assetName).AddRetryCount();
                        await this.PreloadRawFileAsync(packageName, new string[] { assetName }, priority, progression, maxRetryCount);
                        continue;
                    }
                }

                {
                    bool loaded = false;
                    BundlePack pack = new BundlePack();

                    var package = PackageManager.GetPackage(packageName);
                    if (package != null && package.CheckLocationValid(assetName))
                    {
                        var req = package.LoadRawFileAsync(assetName, priority);
                        if (req != null)
                        {
                            float lastCount = 0;
                            do
                            {
                                if (progression != null)
                                {
                                    this.currentCount += (req.Progress - lastCount);
                                    lastCount = req.Progress;
                                    progression.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);
                                }

                                if (req.IsDone)
                                {
                                    loaded = true;
                                    pack.SetPack(packageName, assetName, req);
                                    break;
                                }
                                await UniTask.Yield();
                            } while (true);
                        }
                    }
                    else
                    {
                        Logging.Print<Logger>($"<color=#ff33ae>Package: {packageName} doesn't exist or location invalid.</color>");
                    }

                    if (loaded)
                    {
                        // skipping duplicate keys
                        if (!this.HasInCache(assetName))
                        {
                            this._cacher.Add(assetName, pack);
                            Logging.Print<Logger>($"<color=#ff9600>【Preload】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: {assetName}</color>");
                        }
                    }
                    else
                    {
                        if (this.GetRetryCounter(assetName).IsRetryActive()) Logging.Print<Logger>($"<color=#f7ff3e>【Preload】 => << CacheBundle >> Asset: {assetName} doing retry. Retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}</color>");
                        else Logging.Print<Logger>($"<color=#f7ff3e>【Preload】 => << CacheBundle >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}</color>");
                        this.GetRetryCounter(assetName).AddRetryCount();
                        await this.PreloadRawFileAsync(packageName, new string[] { assetName }, priority, progression, maxRetryCount);
                        continue;
                    }
                }

                // 移除標記
                this.RemoveLoadingFlags(assetName);
            }
        }

        public void PreloadRawFile(string packageName, string[] assetNames, Progression progression, byte maxRetryCount)
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
                        Logging.Print<Logger>($"<color=#ff9b3e>Asset: {assetName} Load failed and cannot retry anymore!!! Please to check asset is existing.</color>");
                    }
                    else Logging.Print<Logger>($"<color=#ff9b3e>Asset: {assetName} Loading...</color>");
                    return;
                }

                // Loading 標記
                this.AddLoadingFlags(assetName, maxRetryCount);

                // 如果有在緩存中就不進行預加載
                if (this.HasInCache(assetName))
                {
                    BundlePack pack = this.GetFromCache(assetName);
                    if (pack.IsRawFileOperationHandleValid())
                    {
                        this.currentCount++;
                        progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);
                        this.RemoveLoadingFlags(assetName);
                        Logging.Print<Logger>($"<color=#fff6ba>【Preload】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: [{assetName}] already preloaded!!!</color>");
                        continue;
                    }
                    else
                    {
                        this.UnloadRawFile(assetName, true);
                        if (this.GetRetryCounter(assetName).IsRetryActive()) Logging.Print<Logger>($"<color=#f7ff3e>【f7ff3e】 => << CacheBundle >> Asset: {assetName} doing retry. Retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}</color>");
                        else Logging.Print<Logger>($"<color=#f7ff3e>【f7ff3e】 => << CacheBundle >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}</color>");
                        this.GetRetryCounter(assetName).AddRetryCount();
                        this.PreloadRawFile(packageName, new string[] { assetName }, progression, maxRetryCount);
                        continue;
                    }
                }

                {
                    bool loaded = false;
                    BundlePack pack = new BundlePack();

                    var package = PackageManager.GetPackage(packageName);
                    if (package != null && package.CheckLocationValid(assetName))
                    {
                        var req = package.LoadRawFileSync(assetName);
                        if (req != null)
                        {
                            if (req.IsDone)
                            {
                                if (progression != null)
                                {
                                    this.currentCount++;
                                    progression.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);
                                }

                                loaded = true;
                                pack.SetPack(packageName, assetName, req);
                            }
                        }
                    }
                    else
                    {
                        Logging.Print<Logger>($"<color=#ff33ae>Package: {packageName} doesn't exist or location invalid.</color>");
                    }

                    if (loaded)
                    {
                        // skipping duplicate keys
                        if (!this.HasInCache(assetName))
                        {
                            this._cacher.Add(assetName, pack);
                            Logging.Print<Logger>($"<color=#ff9600>【Preload】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: {assetName}</color>");
                        }
                    }
                    else
                    {
                        if (this.GetRetryCounter(assetName).IsRetryActive()) Logging.Print<Logger>($"<color=#f7ff3e>【Preload】 => << CacheBundle >> Asset: {assetName} doing retry. Retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}</color>");
                        else Logging.Print<Logger>($"<color=#f7ff3e>【Preload】 => << CacheBundle >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}</color>");
                        this.GetRetryCounter(assetName).AddRetryCount();
                        this.PreloadRawFile(packageName, new string[] { assetName }, progression, maxRetryCount);
                        continue;
                    }
                }

                // 移除標記
                this.RemoveLoadingFlags(assetName);
            }
        }

        public async UniTask<T> LoadRawFileAsync<T>(string packageName, string assetName, uint priority, Progression progression, byte maxRetryCount)
        {
            if (string.IsNullOrEmpty(assetName)) return default;

            // 如果有進行 Loading 標記後, 直接 return
            if (this.HasInLoadingFlags(assetName) && !this.GetRetryCounter(assetName).IsRetryValid())
            {
                if (this.GetRetryCounter(assetName).IsRetryActive())
                {
                    this.RemoveLoadingFlags(assetName);
                    Logging.Print<Logger>($"<color=#ff9b3e>Asset: {assetName} Load failed and cannot retry anymore!!! Please to check asset is existing.</color>");
                }
                else Logging.Print<Logger>($"<color=#ff9b3e>Asset: {assetName} Loading...</color>");
                return default;
            }

            // 初始加載進度
            this.currentCount = 0;
            this.totalCount = 1;

            // Loading 標記
            this.AddLoadingFlags(assetName, maxRetryCount);

            // 先從緩存拿
            BundlePack pack = this.GetFromCache(assetName);

            if (pack == null)
            {
                bool loaded = false;
                pack = new BundlePack();

                var package = PackageManager.GetPackage(packageName);
                if (package != null && package.CheckLocationValid(assetName))
                {
                    var req = package.LoadRawFileAsync(assetName, priority);
                    if (req != null)
                    {
                        float lastCount = 0;
                        do
                        {
                            if (progression != null)
                            {
                                this.currentCount += (req.Progress - lastCount);
                                lastCount = req.Progress;
                                progression.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);
                            }

                            if (req.IsDone)
                            {
                                loaded = true;
                                pack.SetPack(packageName, assetName, req);
                                break;
                            }
                            await UniTask.Yield();
                        } while (true);
                    }
                }
                else
                {
                    Logging.Print<Logger>($"<color=#ff33ae>Package: {packageName} doesn't exist or location invalid.</color>");
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

            object data;
            if (typeof(T) == typeof(string))
            {
                data = pack.GetRawFileText();
            }
            else if (typeof(T) == typeof(byte[]))
            {
                data = pack.GetRawFileData();
            }
            else data = null;

            if (data != null)
            {
                // 引用計數++
                pack.AddRef();
                Logging.Print<Logger>($"<color=#90FF71>【Load】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: {assetName}, ref: {pack.refCount}</color>");
            }
            else
            {
                this.UnloadRawFile(assetName, true);
                if (this.GetRetryCounter(assetName).IsRetryActive()) Logging.Print<Logger>($"<color=#f7ff3e>【Load】 => << CacheBundle >> Asset: {assetName} doing retry. Retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}</color>");
                else Logging.Print<Logger>($"<color=#f7ff3e>【Load】 => << CacheBundle >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}</color>");
                this.GetRetryCounter(assetName).AddRetryCount();
                return await this.LoadRawFileAsync<T>(packageName, assetName, priority, progression, maxRetryCount);
            }

            this.RemoveLoadingFlags(assetName);

            return (T)data;
        }

        public T LoadRawFile<T>(string packageName, string assetName, Progression progression, byte maxRetryCount)
        {
            if (string.IsNullOrEmpty(assetName)) return default;

            // 如果有進行 Loading 標記後, 直接 return
            if (this.HasInLoadingFlags(assetName) && !this.GetRetryCounter(assetName).IsRetryValid())
            {
                if (this.GetRetryCounter(assetName).IsRetryActive())
                {
                    this.RemoveLoadingFlags(assetName);
                    Logging.Print<Logger>($"<color=#ff9b3e>Asset: {assetName} Load failed and cannot retry anymore!!! Please to check asset is existing.</color>");
                }
                else Logging.Print<Logger>($"<color=#ff9b3e>Asset: {assetName} Loading...</color>");
                return default;
            }

            // 初始加載進度
            this.currentCount = 0;
            this.totalCount = 1;

            // Loading 標記
            this.AddLoadingFlags(assetName, maxRetryCount);

            // 先從緩存拿
            BundlePack pack = this.GetFromCache(assetName);

            if (pack == null)
            {
                bool loaded = false;
                pack = new BundlePack();

                var package = PackageManager.GetPackage(packageName);
                if (package != null && package.CheckLocationValid(assetName))
                {
                    var req = package.LoadRawFileSync(assetName);
                    if (req != null)
                    {
                        if (req.IsDone)
                        {
                            if (progression != null)
                            {
                                this.currentCount++;
                                progression.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);
                            }

                            loaded = true;
                            pack.SetPack(packageName, assetName, req);
                        }
                    }
                }
                else
                {
                    Logging.Print<Logger>($"<color=#ff33ae>Package: {packageName} doesn't exist or location invalid.</color>");
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

            object data;
            if (typeof(T) == typeof(string))
            {
                data = pack.GetRawFileText();
            }
            else if (typeof(T) == typeof(byte[]))
            {
                data = pack.GetRawFileData();
            }
            else data = null;

            if (data != null)
            {
                // 引用計數++
                pack.AddRef();
                Logging.Print<Logger>($"<color=#90FF71>【Load】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: {assetName}, ref: {pack.refCount}</color>");
            }
            else
            {
                this.UnloadRawFile(assetName, true);
                if (this.GetRetryCounter(assetName).IsRetryActive()) Logging.Print<Logger>($"<color=#f7ff3e>【Load】 => << CacheBundle >> Asset: {assetName} doing retry. Retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}</color>");
                else Logging.Print<Logger>($"<color=#f7ff3e>【Load】 => << CacheBundle >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}</color>");
                this.GetRetryCounter(assetName).AddRetryCount();
                return this.LoadRawFile<T>(packageName, assetName, progression, maxRetryCount);
            }

            this.RemoveLoadingFlags(assetName);

            return (T)data;
        }

        public void UnloadRawFile(string assetName, bool forceUnload)
        {
            if (string.IsNullOrEmpty(assetName)) return;

            if (this.HasInLoadingFlags(assetName) && !this.GetRetryCounter(assetName).IsRetryValid())
            {
                Logging.Print<Logger>($"<color=#ff9b3e>Asset: {assetName} Loading...</color>");
                return;
            }

            if (this.HasInCache(assetName))
            {
                BundlePack pack = this.GetFromCache(assetName);
                string packageName = pack.packageName;

                if (pack.IsRawFileOperationHandle())
                {
                    // 引用計數--
                    pack.DelRef();

                    Logging.Print<Logger>($"<color=#00e5ff>【<color=#ffcf92>Unload</color>】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: {assetName}, ref: {this._cacher.TryGetValue(assetName, out var v)} {v?.refCount}</color>");

                    if (forceUnload)
                    {
                        pack.UnloadRawFile();
                        this._cacher[assetName] = null;
                        this._cacher.Remove(assetName);

                        var package = PackageManager.GetPackage(packageName);
                        package?.UnloadUnusedAssets();

                        Logging.Print<Logger>($"<color=#00e5ff>【<color=#ff92ef>Force Unload Completes</color>】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: {assetName}</color>");
                    }
                    else if (this._cacher[assetName].refCount <= 0)
                    {
                        pack.UnloadRawFile();
                        this._cacher[assetName] = null;
                        this._cacher.Remove(assetName);

                        var package = PackageManager.GetPackage(packageName);
                        package?.UnloadUnusedAssets();

                        Logging.Print<Logger>($"<color=#00e5ff>【<color=#ff92ef>Unload Completes</color>】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: {assetName}</color>");
                    }
                }
                else Logging.Print<Logger>($"<color=#00e5ff>【<color=#ffcf92>Unload Type Error</color>】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: {assetName}, ref: {this._cacher.TryGetValue(assetName, out var v)} {v?.refCount}</color>");
            }
        }

        public void ReleaseRawFiles()
        {
            if (this.Count == 0) return;

            HashSet<ResourcePackage> packages = new HashSet<ResourcePackage>();

            // 強制釋放緩存與資源
            foreach (var assetName in this._cacher.Keys.ToArray())
            {
                if (this.HasInCache(assetName))
                {
                    BundlePack pack = this.GetFromCache(assetName);
                    var package = PackageManager.GetPackage(pack.packageName);
                    if (!packages.Contains(package)) packages.Add(package);
                    this.UnloadRawFile(assetName, true);
                }
            }

            foreach (var package in packages)
            {
                package?.UnloadUnusedAssets();
            }

            Logging.Print<Logger>($"<color=#ff71b7>【Release All RawFiles】 => Current << CacheBundle >> Cache Count: {this.Count}</color>");
        }
        #endregion

        #region Scene
        public async UniTask<BundlePack> LoadSceneAsync(string packageName, string assetName, LoadSceneMode loadSceneMode, bool activateOnLoad, uint priority, Progression progression)
        {
            if (string.IsNullOrEmpty(assetName)) return null;

            // 如果有進行 Loading 標記後, 直接 return
            if (this.HasInLoadingFlags(assetName) && !this.GetRetryCounter(assetName).IsRetryValid())
            {
                if (this.GetRetryCounter(assetName).IsRetryActive())
                {
                    this.RemoveLoadingFlags(assetName);
                    Logging.Print<Logger>($"<color=#ff9b3e>Asset: {assetName} Load failed and cannot retry anymore!!! Please to check asset is existing.</color>");
                }
                else Logging.Print<Logger>($"<color=#ff9b3e>Asset: {assetName} Loading...</color>");
                return null;
            }

            // 初始加載進度
            this.currentCount = 0;
            this.totalCount = 1;

            // 場景最多嘗試 1 次
            byte maxRetryCount = 1;
            // Loading 標記
            this.AddLoadingFlags(assetName, maxRetryCount);

            bool loaded = false;
            var pack = new BundlePack();

            // 場景需特殊處理
            var package = PackageManager.GetPackage(packageName);
            if (package != null && package.CheckLocationValid(assetName))
            {
                var req = package.LoadSceneAsync(assetName, loadSceneMode, !activateOnLoad, priority);
                if (req != null)
                {
                    float lastCount = 0;
                    do
                    {
                        if (progression != null)
                        {
                            this.currentCount += (req.Progress - lastCount);
                            lastCount = req.Progress;
                            if (this.currentCount >= 0.9f) this.currentCount = 1f;
                            progression.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);
                        }

                        if (req.IsDone)
                        {
                            loaded = true;
                            switch (loadSceneMode)
                            {
                                case LoadSceneMode.Single:
                                    {
                                        pack.SetPack(packageName, assetName, req);

                                        // 清除 Additive 計數緩存 (主場景無需緩存, 因為會自動釋放子場景)
                                        this._sceneCache.Clear();
                                        this._sceneCounter.Clear();
                                    }
                                    break;
                                case LoadSceneMode.Additive:
                                    {
                                        pack.SetPack(packageName, assetName, req);

                                        // 加載場景的計數緩存 (Additive 需要進行計數, 要手動卸載子場景)
                                        if (!this._sceneCounter.ContainsKey(assetName))
                                        {
                                            this._sceneCounter.Add(assetName, 1);
                                            var count = this._sceneCounter[assetName];
                                            string key = $"{assetName}#{count}";
                                            this._sceneCache.Add(key, pack);
                                            Logging.Print<Logger>($"<color=#90FF71>【Load Scene Additive】 => << CacheBundle >> scene: {key}</color>");
                                        }
                                        else
                                        {
                                            var count = ++this._sceneCounter[assetName];
                                            string key = $"{assetName}#{count}";
                                            this._sceneCache.Add(key, pack);
                                            Logging.Print<Logger>($"<color=#90FF71>【Load Scene Additive】 => << CacheBundle >> scene: {key}</color>");
                                        }
                                    }
                                    break;
                            }
                            break;
                        }
                        await UniTask.Yield();
                    } while (true);
                }
            }
            else
            {
                Logging.Print<Logger>($"<color=#ff33ae>Package: {packageName} doesn't exist or location invalid.</color>");
            }

            if (!loaded || !pack.GetScene().isLoaded)
            {
                this.UnloadScene(assetName, true);
                if (this.GetRetryCounter(assetName).IsRetryActive()) Logging.Print<Logger>($"<color=#f7ff3e>【Load Scene】 => << CacheBundle >> Asset: {assetName} doing retry. Retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}</color>");
                else Logging.Print<Logger>($"<color=#f7ff3e>【Load Scene】 => << CacheBundle >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}</color>");
                this.GetRetryCounter(assetName).AddRetryCount();
                return await this.LoadSceneAsync(packageName, assetName, loadSceneMode, activateOnLoad, priority, progression);
            }

            this.RemoveLoadingFlags(assetName);

            return pack;
        }

        public void UnloadScene(string assetName, bool recursively)
        {
            if (this._sceneCounter.ContainsKey(assetName))
            {
                if (recursively)
                {
                    for (int topCount = this._sceneCounter[assetName]; topCount >= 1; --topCount)
                    {
                        string key = $"{assetName}#{topCount}";
                        if (this._sceneCache.ContainsKey(key))
                        {
                            var pack = this._sceneCache[key];
                            if (pack.IsSceneOperationHandle())
                            {
                                pack.UnloadScene();
                                this._sceneCache[key] = null;
                                this._sceneCache.Remove(key);

                                Logging.Print<Logger>($"<color=#00e5ff>【Unload Additive Scene】 => << CacheBundle >> scene: {key}, count: {topCount}</color>");
                            }
                        }
                    }

                    // 遞迴完, 移除計數緩存
                    this._sceneCounter.Remove(assetName);

                    Logging.Print<Logger>($"<color=#00e5ff>【<color=#ff92ef>Unload Additive Scene Completes</color>】 => << CacheBundle >> sceneName: {assetName}, recursively: {recursively}</color>");
                }
                else
                {
                    bool saftyChecker = false;

                    // 安全檢測無效場景卸載 (有可能被 Build 方法卸載掉)
                    for (int topCount = this._sceneCounter[assetName]; topCount >= 1; --topCount)
                    {
                        string key = $"{assetName}#{topCount}";
                        var pack = this._sceneCache[key];
                        if (pack.IsSceneOperationHandle())
                        {
                            if (pack.GetScene().isLoaded)
                            {
                                saftyChecker = true;
                                break;
                            }
                        }
                    }

                    // 啟用安全檢測卸載方法 (直接遞迴強制全部卸載)
                    if (saftyChecker)
                    {
                        ResourcePackage package = null;
                        for (int topCount = this._sceneCounter[assetName]; topCount >= 1; --topCount)
                        {
                            string key = $"{assetName}#{topCount}";
                            if (this._sceneCache.ContainsKey(key))
                            {
                                var pack = this._sceneCache[key];
                                package = PackageManager.GetPackage(pack.packageName);
                                if (pack.IsSceneOperationHandle())
                                {
                                    pack.UnloadScene();
                                    this._sceneCache[key] = null;
                                    this._sceneCache.Remove(key);

                                    Logging.Print<Logger>($"<color=#00e5ff>【<color=#97ff3e>Safty</color> Unload Additive Scene】 => << CacheBundle >> scene: {key}, count: {topCount}</color>");
                                }
                            }
                        }

                        // 遞迴完, 移除計數緩存
                        this._sceneCounter.Remove(assetName);
                        package?.UnloadUnusedAssets();

                        Logging.Print<Logger>($"<color=#00e5ff>【<color=#ff92ef><color=#97ff3e>Safty</color> Unload Additive Scene Completes</color>】 => << CacheBundle >> sceneName: {assetName}, recursively: {recursively}</color>");
                    }
                    else
                    {
                        int topCount = this._sceneCounter[assetName];
                        string key = $"{assetName}#{topCount}";
                        var pack = this._sceneCache[key];
                        string packageName = pack.packageName;

                        if (pack.IsSceneOperationHandle())
                        {
                            pack.UnloadScene();
                            this._sceneCache[key] = null;
                            this._sceneCache.Remove(key);

                            Logging.Print<Logger>($"<color=#00e5ff>【Unload Additive Scene】 => << CacheBundle >> scene: {key}, count: {topCount}</color>");

                            topCount = --this._sceneCounter[assetName];

                            // 移除計數緩存
                            if (topCount <= 0)
                            {
                                var package = PackageManager.GetPackage(packageName);
                                this._sceneCounter.Remove(assetName);
                                package?.UnloadUnusedAssets();
                                Logging.Print<Logger>($"<color=#00e5ff>【<color=#ff92ef>Unload Additive Scene Completes</color>】 => << CacheBundle >> sceneName: {assetName}, recursively: {recursively}</color>");
                            }
                        }
                    }
                }
            }
            else Logging.Print<Logger>($"<color=#00e5ff>【<color=#ff4a8d>Unload Scene Invalid</color>】 => << CacheBundle >> sceneName: {assetName} maybe not <color=#ffb33e>Additive</color> or is <color=#ffb33e>Single</color></color>");
        }
        #endregion

        #region Asset
        public async UniTask PreloadAssetAsync<T>(string packageName, string[] assetNames, uint priority, Progression progression, byte maxRetryCount) where T : Object
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
                        Logging.Print<Logger>($"<color=#ff9b3e>Asset: {assetName} Load failed and cannot retry anymore!!! Please to check asset is existing.</color>");
                    }
                    else Logging.Print<Logger>($"<color=#ff9b3e>Asset: {assetName} Loading...</color>");
                    return;
                }

                // Loading 標記
                this.AddLoadingFlags(assetName, maxRetryCount);

                // 如果有在緩存中就不進行預加載
                if (this.HasInCache(assetName))
                {
                    BundlePack pack = this.GetFromCache(assetName);
                    if (pack.IsAssetOperationHandleValid())
                    {
                        this.currentCount++;
                        progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);
                        this.RemoveLoadingFlags(assetName);
                        Logging.Print<Logger>($"<color=#fff6ba>【Preload】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: [{assetName}] already preloaded!!!</color>");
                        continue;
                    }
                    else
                    {
                        this.UnloadAsset(assetName, true);
                        if (this.GetRetryCounter(assetName).IsRetryActive()) Logging.Print<Logger>($"<color=#f7ff3e>【Preload】 => << CacheBundle >> Asset: {assetName} doing retry. Retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}</color>");
                        else Logging.Print<Logger>($"<color=#f7ff3e>【Preload】 => << CacheBundle >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}</color>");
                        this.GetRetryCounter(assetName).AddRetryCount();
                        await this.PreloadAssetAsync<T>(packageName, new string[] { assetName }, priority, progression, maxRetryCount);
                        continue;
                    }
                }

                {
                    bool loaded = false;
                    BundlePack pack = new BundlePack();

                    var package = PackageManager.GetPackage(packageName);
                    if (package != null && package.CheckLocationValid(assetName))
                    {
                        var req = package.LoadAssetAsync<T>(assetName, priority);
                        if (req != null)
                        {
                            float lastCount = 0;
                            do
                            {
                                if (progression != null)
                                {
                                    this.currentCount += (req.Progress - lastCount);
                                    lastCount = req.Progress;
                                    progression.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);
                                }

                                if (req.IsDone)
                                {
                                    loaded = true;
                                    pack.SetPack(packageName, assetName, req);
                                    break;
                                }
                                await UniTask.Yield();
                            } while (true);
                        }
                    }
                    else
                    {
                        Logging.Print<Logger>($"<color=#ff33ae>Package: {packageName} doesn't exist or location invalid.</color>");
                    }

                    if (loaded)
                    {
                        // skipping duplicate keys
                        if (!this.HasInCache(assetName))
                        {
                            this._cacher.Add(assetName, pack);
                            Logging.Print<Logger>($"<color=#ff9600>【Preload】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: {assetName}</color>");
                        }
                    }
                    else
                    {
                        if (this.GetRetryCounter(assetName).IsRetryActive()) Logging.Print<Logger>($"<color=#f7ff3e>【Preload】 => << CacheBundle >> Asset: {assetName} doing retry. Retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}</color>");
                        else Logging.Print<Logger>($"<color=#f7ff3e>【Preload】 => << CacheBundle >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}</color>");
                        this.GetRetryCounter(assetName).AddRetryCount();
                        await this.PreloadAssetAsync<T>(packageName, new string[] { assetName }, priority, progression, maxRetryCount);
                        continue;
                    }
                }

                // 移除標記
                this.RemoveLoadingFlags(assetName);
            }
        }

        public void PreloadAsset<T>(string packageName, string[] assetNames, Progression progression, byte maxRetryCount) where T : Object
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
                        Logging.Print<Logger>($"<color=#ff9b3e>Asset: {assetName} Load failed and cannot retry anymore!!! Please to check asset is existing.</color>");
                    }
                    else Logging.Print<Logger>($"<color=#ff9b3e>Asset: {assetName} Loading...</color>");
                    return;
                }

                // Loading 標記
                this.AddLoadingFlags(assetName, maxRetryCount);

                // 如果有在緩存中就不進行預加載
                if (this.HasInCache(assetName))
                {
                    BundlePack pack = this.GetFromCache(assetName);
                    if (pack.IsAssetOperationHandleValid())
                    {
                        this.currentCount++;
                        progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);
                        this.RemoveLoadingFlags(assetName);
                        Logging.Print<Logger>($"<color=#fff6ba>【Preload】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: [{assetName}] already preloaded!!!</color>");
                        continue;
                    }
                    else
                    {
                        this.UnloadAsset(assetName, true);
                        if (this.GetRetryCounter(assetName).IsRetryActive()) Logging.Print<Logger>($"<color=#f7ff3e>【Preload】 => << CacheBundle >> Asset: {assetName} doing retry. Retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}</color>");
                        else Logging.Print<Logger>($"<color=#f7ff3e>【Preload】 => << CacheBundle >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}</color>");
                        this.GetRetryCounter(assetName).AddRetryCount();
                        this.PreloadAsset<T>(packageName, new string[] { assetName }, progression, maxRetryCount);
                        continue;
                    }
                }

                {
                    bool loaded = false;
                    BundlePack pack = new BundlePack();

                    var package = PackageManager.GetPackage(packageName);
                    if (package != null && package.CheckLocationValid(assetName))
                    {
                        var req = package.LoadAssetSync<T>(assetName);
                        if (req != null)
                        {
                            if (req.IsDone)
                            {
                                if (progression != null)
                                {
                                    this.currentCount++;
                                    progression.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);
                                }

                                loaded = true;
                                pack.SetPack(packageName, assetName, req);
                            }
                        }
                    }
                    else
                    {
                        Logging.Print<Logger>($"<color=#ff33ae>Package: {packageName} doesn't exist or location invalid.</color>");
                    }

                    if (loaded)
                    {
                        // skipping duplicate keys
                        if (!this.HasInCache(assetName))
                        {
                            this._cacher.Add(assetName, pack);
                            Logging.Print<Logger>($"<color=#ff9600>【Preload】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: {assetName}</color>");
                        }
                    }
                    else
                    {
                        if (this.GetRetryCounter(assetName).IsRetryActive()) Logging.Print<Logger>($"<color=#f7ff3e>【Preload】 => << CacheBundle >> Asset: {assetName} doing retry. Retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}</color>");
                        else Logging.Print<Logger>($"<color=#f7ff3e>【Preload】 => << CacheBundle >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}</color>");
                        this.GetRetryCounter(assetName).AddRetryCount();
                        this.PreloadAsset<T>(packageName, new string[] { assetName }, progression, maxRetryCount);
                        continue;
                    }
                }

                // 移除標記
                this.RemoveLoadingFlags(assetName);
            }
        }

        public async UniTask<T> LoadAssetAsync<T>(string packageName, string assetName, uint priority, Progression progression, byte maxRetryCount) where T : Object
        {
            if (string.IsNullOrEmpty(assetName)) return null;

            // 如果有進行 Loading 標記後, 直接 return
            if (this.HasInLoadingFlags(assetName) && !this.GetRetryCounter(assetName).IsRetryValid())
            {
                if (this.GetRetryCounter(assetName).IsRetryActive())
                {
                    this.RemoveLoadingFlags(assetName);
                    Logging.Print<Logger>($"<color=#ff9b3e>Asset: {assetName} Load failed and cannot retry anymore!!! Please to check asset is existing.</color>");
                }
                else Logging.Print<Logger>($"<color=#ff9b3e>Asset: {assetName} Loading...</color>");
                return null;
            }

            // 初始加載進度
            this.currentCount = 0;
            this.totalCount = 1;

            // Loading 標記
            this.AddLoadingFlags(assetName, maxRetryCount);

            // 先從緩存拿
            BundlePack pack = this.GetFromCache(assetName);

            if (pack == null)
            {
                bool loaded = false;
                pack = new BundlePack();

                var package = PackageManager.GetPackage(packageName);
                if (package != null && package.CheckLocationValid(assetName))
                {
                    var req = package.LoadAssetAsync<T>(assetName, priority);
                    if (req != null)
                    {
                        float lastCount = 0;
                        do
                        {
                            if (progression != null)
                            {
                                this.currentCount += (req.Progress - lastCount);
                                lastCount = req.Progress;
                                progression.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);
                            }

                            if (req.IsDone)
                            {
                                loaded = true;
                                pack.SetPack(packageName, assetName, req);
                                break;
                            }
                            await UniTask.Yield();
                        } while (true);
                    }
                }
                else
                {
                    Logging.Print<Logger>($"<color=#ff33ae>Package: {packageName} doesn't exist or location invalid.</color>");
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
                Logging.Print<Logger>($"<color=#90FF71>【Load】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: {assetName}, ref: {pack.refCount}</color>");
            }
            else
            {
                this.UnloadAsset(assetName, true);
                if (this.GetRetryCounter(assetName).IsRetryActive()) Logging.Print<Logger>($"<color=#f7ff3e>【Load】 => << CacheBundle >> Asset: {assetName} doing retry. Retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}</color>");
                else Logging.Print<Logger>($"<color=#f7ff3e>【Load】 => << CacheBundle >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}</color>");
                this.GetRetryCounter(assetName).AddRetryCount();
                return await this.LoadAssetAsync<T>(packageName, assetName, priority, progression, maxRetryCount);
            }

            this.RemoveLoadingFlags(assetName);

            return asset;
        }

        public T LoadAsset<T>(string packageName, string assetName, Progression progression, byte maxRetryCount) where T : Object
        {
            if (string.IsNullOrEmpty(assetName)) return null;

            // 如果有進行 Loading 標記後, 直接 return
            if (this.HasInLoadingFlags(assetName) && !this.GetRetryCounter(assetName).IsRetryValid())
            {
                if (this.GetRetryCounter(assetName).IsRetryActive())
                {
                    this.RemoveLoadingFlags(assetName);
                    Logging.Print<Logger>($"<color=#ff9b3e>Asset: {assetName} Load failed and cannot retry anymore!!! Please to check asset is existing.</color>");
                }
                else Logging.Print<Logger>($"<color=#ff9b3e>Asset: {assetName} Loading...</color>");
                return null;
            }

            // 初始加載進度
            this.currentCount = 0;
            this.totalCount = 1;

            // Loading 標記
            this.AddLoadingFlags(assetName, maxRetryCount);

            // 先從緩存拿
            BundlePack pack = this.GetFromCache(assetName);

            if (pack == null)
            {
                bool loaded = false;
                pack = new BundlePack();

                var package = PackageManager.GetPackage(packageName);
                if (package != null && package.CheckLocationValid(assetName))
                {
                    var req = package.LoadAssetSync<T>(assetName);
                    if (req != null)
                    {
                        if (req.IsDone)
                        {
                            if (progression != null)
                            {
                                this.currentCount++;
                                progression.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);
                            }

                            loaded = true;
                            pack.SetPack(packageName, assetName, req);
                        }
                    }
                }
                else
                {
                    Logging.Print<Logger>($"<color=#ff33ae>Package: {packageName} doesn't exist or location invalid.</color>");
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
                Logging.Print<Logger>($"<color=#90FF71>【Load】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: {assetName}, ref: {pack.refCount}</color>");
            }
            else
            {
                this.UnloadAsset(assetName, true);
                if (this.GetRetryCounter(assetName).IsRetryActive()) Logging.Print<Logger>($"<color=#f7ff3e>【Load】 => << CacheBundle >> Asset: {assetName} doing retry. Retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}</color>");
                else Logging.Print<Logger>($"<color=#f7ff3e>【Load】 => << CacheBundle >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}</color>");
                this.GetRetryCounter(assetName).AddRetryCount();
                return this.LoadAsset<T>(packageName, assetName, progression, maxRetryCount);
            }

            this.RemoveLoadingFlags(assetName);

            return asset;
        }

        public void UnloadAsset(string assetName, bool forceUnload)
        {
            if (string.IsNullOrEmpty(assetName)) return;

            if (this.HasInLoadingFlags(assetName) && !this.GetRetryCounter(assetName).IsRetryValid())
            {
                Logging.Print<Logger>($"<color=#ff9b3e>Asset: {assetName} Loading...</color>");
                return;
            }

            if (this.HasInCache(assetName))
            {
                BundlePack pack = this.GetFromCache(assetName);
                string packageName = pack.packageName;

                if (pack.IsAssetOperationHandle())
                {
                    // 引用計數--
                    pack.DelRef();

                    Logging.Print<Logger>($"<color=#00e5ff>【<color=#ffcf92>Unload</color>】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: {assetName}, ref: {this._cacher.TryGetValue(assetName, out var v)} {v?.refCount}</color>");

                    if (forceUnload)
                    {
                        pack.UnloadAsset();
                        this._cacher[assetName] = null;
                        this._cacher.Remove(assetName);

                        var package = PackageManager.GetPackage(packageName);
                        package?.UnloadUnusedAssets();

                        Logging.Print<Logger>($"<color=#00e5ff>【<color=#ff92ef>Force Unload Completes</color>】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: {assetName}</color>");
                    }
                    else if (this._cacher[assetName].refCount <= 0)
                    {
                        pack.UnloadAsset();
                        this._cacher[assetName] = null;
                        this._cacher.Remove(assetName);

                        var package = PackageManager.GetPackage(packageName);
                        package?.UnloadUnusedAssets();

                        Logging.Print<Logger>($"<color=#00e5ff>【<color=#ff92ef>Unload Completes</color>】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: {assetName}</color>");
                    }
                }
                else Logging.Print<Logger>($"<color=#00e5ff>【<color=#ffcf92>Unload Type Error</color>】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: {assetName}, ref: {this._cacher.TryGetValue(assetName, out var v)} {v?.refCount}</color>");
            }
        }

        public void ReleaseAssets()
        {
            if (this.Count == 0) return;

            HashSet<ResourcePackage> packages = new HashSet<ResourcePackage>();

            // 強制釋放緩存與資源
            foreach (var assetName in this._cacher.Keys.ToArray())
            {
                if (this.HasInCache(assetName))
                {
                    BundlePack pack = this.GetFromCache(assetName);
                    var package = PackageManager.GetPackage(pack.packageName);
                    if (!packages.Contains(package)) packages.Add(package);
                    this.UnloadAsset(assetName, true);
                }
            }

            foreach (var package in packages)
            {
                package?.UnloadUnusedAssets();
            }

            Logging.Print<Logger>($"<color=#ff71b7>【Release All Assets】 => Current << CacheBundle >> Cache Count: {this.Count}</color>");
        }
        #endregion

        ~CacheBundle()
        {
            this._sceneCache.Clear();
            this._sceneCache = null;
            this._sceneCounter.Clear();
            this._sceneCounter = null;
        }
    }
}