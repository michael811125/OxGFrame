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
        /// <summary>
        /// 緩存 Scene BundlePack
        /// </summary>
        private Dictionary<string, BundlePack> _additiveScenes;

        /// <summary>
        /// 子場景堆疊式計數緩存
        /// </summary>
        private Dictionary<string, int> _additiveSceneCounter;

        /// <summary>
        /// 預設卸載循環處理次數
        /// </summary>
        private const int _DEFAULT_LOOP_COUNT = 3;

        public CacheBundle() : base()
        {
            this._additiveScenes = new Dictionary<string, BundlePack>();
            this._additiveSceneCounter = new Dictionary<string, int>();
        }

        private static CacheBundle _instance = null;
        public static CacheBundle GetInstance()
        {
            if (_instance == null)
                _instance = new CacheBundle();
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
                    BundlePack pack = this.GetFromCache(assetName);
                    if (pack.IsRawFileOperationHandleValid())
                    {
                        this.currentCount++;
                        progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);
                        this.RemoveLoadingFlags(assetName);
                        Logging.PrintWarning<Logger>($"【Preload】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}, asset: [{assetName}] already preloaded!!!");
                        continue;
                    }
                    else
                    {
                        this.UnloadRawFile(assetName, true);
                        if (this.GetRetryCounter(assetName).IsRetryActive()) Logging.Print<Logger>($"【Preload】 => << {nameof(CacheBundle)} >> Asset: {assetName} doing retry. Retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}");
                        else Logging.Print<Logger>($"【Preload】 => << {nameof(CacheBundle)} >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}");
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
                                this.currentCount += (req.Progress - lastCount);
                                lastCount = req.Progress;
                                progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);

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
                        Logging.PrintError<Logger>($"Package: {packageName} doesn't exist or Asset: {assetName} location invalid.");
                    }

                    if (loaded)
                    {
                        // skipping duplicate keys
                        if (!this.HasInCache(assetName))
                        {
                            this._cacher.Add(assetName, pack);
                            Logging.Print<Logger>($"【Preload】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}, asset: {assetName}");
                        }
                    }
                    else
                    {
                        if (this.GetRetryCounter(assetName).IsRetryActive()) Logging.Print<Logger>($"【Preload】 => << {nameof(CacheBundle)} >> Asset: {assetName} doing retry. Retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}");
                        else Logging.Print<Logger>($"【Preload】 => << {nameof(CacheBundle)} >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}");
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
                    BundlePack pack = this.GetFromCache(assetName);
                    if (pack.IsRawFileOperationHandleValid())
                    {
                        this.currentCount++;
                        progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);
                        this.RemoveLoadingFlags(assetName);
                        Logging.PrintWarning<Logger>($"【Preload】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}, asset: [{assetName}] already preloaded!!!");
                        continue;
                    }
                    else
                    {
                        this.UnloadRawFile(assetName, true);
                        if (this.GetRetryCounter(assetName).IsRetryActive()) Logging.Print<Logger>($"【Preload】 => << {nameof(CacheBundle)} >> Asset: {assetName} doing retry. Retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}");
                        else Logging.Print<Logger>($"【Preload】 => << {nameof(CacheBundle)} >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}");
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
                                this.currentCount++;
                                progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);

                                loaded = true;
                                pack.SetPack(packageName, assetName, req);
                            }
                        }
                    }
                    else
                    {
                        Logging.PrintError<Logger>($"Package: {packageName} doesn't exist or Asset: {assetName} location invalid.");
                    }

                    if (loaded)
                    {
                        // skipping duplicate keys
                        if (!this.HasInCache(assetName))
                        {
                            this._cacher.Add(assetName, pack);
                            Logging.Print<Logger>($"【Preload】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}, asset: {assetName}");
                        }
                    }
                    else
                    {
                        if (this.GetRetryCounter(assetName).IsRetryActive()) Logging.Print<Logger>($"【Preload】 => << {nameof(CacheBundle)} >> Asset: {assetName} doing retry. Retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}");
                        else Logging.Print<Logger>($"【Preload】 => << {nameof(CacheBundle)} >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}");
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
                    Logging.PrintWarning<Logger>($"Asset: {assetName} Load failed and cannot retry anymore!!! Please to check asset is existing.");
                }
                else Logging.PrintWarning<Logger>($"Asset: {assetName} Loading...");
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
                            this.currentCount += (req.Progress - lastCount);
                            lastCount = req.Progress;
                            progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);

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
                    Logging.PrintError<Logger>($"Package: {packageName} doesn't exist or Asset: {assetName} location invalid.");
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
                Logging.Print<Logger>($"【Load】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}, asset: {assetName}, ref: {pack.refCount}");
            }
            else
            {
                this.UnloadRawFile(assetName, true);
                if (this.GetRetryCounter(assetName).IsRetryActive()) Logging.Print<Logger>($"【Load】 => << {nameof(CacheBundle)} >> Asset: {assetName} doing retry. Retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}");
                else Logging.Print<Logger>($"【Load】 => << {nameof(CacheBundle)} >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}");
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
                    Logging.PrintWarning<Logger>($"Asset: {assetName} Load failed and cannot retry anymore!!! Please to check asset is existing.");
                }
                else Logging.PrintWarning<Logger>($"Asset: {assetName} Loading...");
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
                            this.currentCount++;
                            progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);

                            loaded = true;
                            pack.SetPack(packageName, assetName, req);
                        }
                    }
                }
                else
                {
                    Logging.PrintError<Logger>($"Package: {packageName} doesn't exist or Asset: {assetName} location invalid.");
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
                Logging.Print<Logger>($"【Load】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}, asset: {assetName}, ref: {pack.refCount}");
            }
            else
            {
                this.UnloadRawFile(assetName, true);
                if (this.GetRetryCounter(assetName).IsRetryActive()) Logging.Print<Logger>($"【Load】 => << {nameof(CacheBundle)} >> Asset: {assetName} doing retry. Retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}");
                else Logging.Print<Logger>($"【Load】 => << {nameof(CacheBundle)} >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}");
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
                Logging.PrintWarning<Logger>($"Asset: {assetName} Loading...");
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

                    Logging.Print<Logger>($"【Unload】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}, asset: {assetName}, ref: {this._cacher.TryGetValue(assetName, out var v)} {v?.refCount}");

                    if (forceUnload)
                    {
                        pack.UnloadRawFile();
                        this._cacher[assetName] = null;
                        this._cacher.Remove(assetName);

                        var package = PackageManager.GetPackage(packageName);
                        package?.TryUnloadUnusedAsset(assetName, _DEFAULT_LOOP_COUNT);

                        Logging.Print<Logger>($"【Force Unload Completes】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}, asset: {assetName}");
                    }
                    else if (this._cacher[assetName].IsReleasable())
                    {
                        pack.UnloadRawFile();
                        this._cacher[assetName] = null;
                        this._cacher.Remove(assetName);

                        var package = PackageManager.GetPackage(packageName);
                        package?.TryUnloadUnusedAsset(assetName, _DEFAULT_LOOP_COUNT);

                        Logging.Print<Logger>($"【Unload Completes】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}, asset: {assetName}");
                    }
                }
                else Logging.PrintError<Logger>($"【Unload Type Error】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}, asset: {assetName}, ref: {this._cacher.TryGetValue(assetName, out var v)} {v?.refCount}");
            }
        }

        public void ReleaseRawFiles()
        {
            if (this.count == 0)
                return;

            HashSet<ResourcePackage> packages = new HashSet<ResourcePackage>();

            // 強制釋放緩存與資源
            foreach (var assetName in this._cacher.Keys.ToArray())
            {
                if (this.HasInCache(assetName))
                {
                    BundlePack pack = this.GetFromCache(assetName);
                    var package = PackageManager.GetPackage(pack.packageName);
                    if (!packages.Contains(package))
                        packages.Add(package);
                    this.UnloadRawFile(assetName, true);
                }
            }

            // UnloadUnusedAssets
            foreach (var package in packages)
                this.UnloadUnusedAssets(package, false);

            // 調用底層接口釋放資源
            Resources.UnloadUnusedAssets();

            Logging.Print<Logger>($"【Release All RawFiles】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}");
        }
        #endregion

        #region Scene
        public async UniTask<BundlePack> LoadSceneAsync(string packageName, string assetName, LoadSceneMode loadSceneMode, LocalPhysicsMode localPhysicsMode, bool activateOnLoad, uint priority, Progression progression)
        {
            /**
             * Single Scene will auto unload and release
             */

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

            // 場景最多嘗試 1 次
            byte maxRetryCount = 1;
            // Loading 標記
            this.AddLoadingFlags(assetName, maxRetryCount);

            bool loaded = false;
            var pack = new BundlePack();

            // 是否 Suspend
            bool suspendLoad = !activateOnLoad;
            bool suspendLoaded = false;

            // 場景需特殊處理
            var package = PackageManager.GetPackage(packageName);
            if (package != null && package.CheckLocationValid(assetName))
            {
                var req = package.LoadSceneAsync(assetName, loadSceneMode, localPhysicsMode, suspendLoad, priority);
                if (req != null)
                {
                    float lastCount = 0;
                    do
                    {
                        this.currentCount += (req.Progress - lastCount);
                        lastCount = req.Progress;
                        if (this.currentCount >= 0.9f) this.currentCount = 1f;
                        progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);

                        // 處理 Suspend load
                        suspendLoaded = suspendLoad && this.currentCount >= 1f;

                        if (req.IsDone ||
                            suspendLoaded)
                        {
                            loaded = true;
                            switch (loadSceneMode)
                            {
                                case LoadSceneMode.Single:
                                    {
                                        pack.SetPack(packageName, assetName, req);

                                        // 清除 Additive 計數緩存 (主場景無需緩存, 因為會自動釋放子場景)
                                        this._additiveScenes.Clear();
                                        this._additiveSceneCounter.Clear();
                                    }
                                    break;
                                case LoadSceneMode.Additive:
                                    {
                                        pack.SetPack(packageName, assetName, req);

                                        // 加載場景的計數緩存 (Additive 需要進行計數, 要手動卸載子場景)
                                        if (!this._additiveSceneCounter.ContainsKey(assetName))
                                        {
                                            this._additiveSceneCounter.Add(assetName, 1);
                                            var count = this._additiveSceneCounter[assetName];
                                            string key = $"{assetName}#{count}";
                                            this._additiveScenes.Add(key, pack);
                                            Logging.Print<Logger>($"【Load Scene Additive】 => << {nameof(CacheBundle)} >> scene: {key}");
                                        }
                                        else
                                        {
                                            var count = ++this._additiveSceneCounter[assetName];
                                            string key = $"{assetName}#{count}";
                                            this._additiveScenes.Add(key, pack);
                                            Logging.Print<Logger>($"【Load Scene Additive】 => << {nameof(CacheBundle)} >> scene: {key}");
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
                Logging.PrintError<Logger>($"Package: {packageName} doesn't exist or Asset: {assetName} location invalid.");
            }

            if (!suspendLoaded)
            {
                if (!loaded || !pack.GetScene().isLoaded)
                {
                    this.UnloadScene(assetName, true);
                    if (this.GetRetryCounter(assetName).IsRetryActive()) Logging.Print<Logger>($"【Load Scene】 => << {nameof(CacheBundle)} >> Asset: {assetName} doing retry. Retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}");
                    else Logging.Print<Logger>($"【Load Scene】 => << {nameof(CacheBundle)} >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}");
                    this.GetRetryCounter(assetName).AddRetryCount();
                    return await this.LoadSceneAsync(packageName, assetName, loadSceneMode, localPhysicsMode, activateOnLoad, priority, progression);
                }
            }

            this.RemoveLoadingFlags(assetName);

            return pack;
        }

        public BundlePack LoadScene(string packageName, string assetName, LoadSceneMode loadSceneMode, LocalPhysicsMode localPhysicsMode, Progression progression)
        {
            /**
             * Single Scene will auto unload and release
             */

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
                var req = package.LoadSceneSync(assetName, loadSceneMode, localPhysicsMode);
                if (req != null)
                {
                    int frame = 1000;
                    do
                    {
                        if (req.IsDone)
                        {
                            this.currentCount++;
                            progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);
                            loaded = true;
                            switch (loadSceneMode)
                            {
                                case LoadSceneMode.Single:
                                    {
                                        pack.SetPack(packageName, assetName, req);

                                        // 清除 Additive 計數緩存 (主場景無需緩存, 因為會自動釋放子場景)
                                        this._additiveScenes.Clear();
                                        this._additiveSceneCounter.Clear();
                                    }
                                    break;
                                case LoadSceneMode.Additive:
                                    {
                                        pack.SetPack(packageName, assetName, req);

                                        // 加載場景的計數緩存 (Additive 需要進行計數, 要手動卸載子場景)
                                        if (!this._additiveSceneCounter.ContainsKey(assetName))
                                        {
                                            this._additiveSceneCounter.Add(assetName, 1);
                                            var count = this._additiveSceneCounter[assetName];
                                            string key = $"{assetName}#{count}";
                                            this._additiveScenes.Add(key, pack);
                                            Logging.Print<Logger>($"【Load Scene Additive】 => << {nameof(CacheBundle)} >> scene: {key}");
                                        }
                                        else
                                        {
                                            var count = ++this._additiveSceneCounter[assetName];
                                            string key = $"{assetName}#{count}";
                                            this._additiveScenes.Add(key, pack);
                                            Logging.Print<Logger>($"【Load Scene Additive】 => << {nameof(CacheBundle)} >> scene: {key}");
                                        }
                                    }
                                    break;
                            }
                            break;
                        }

                        // 保險機制 (Fuse breaker)
                        frame--;
                        if (frame <= 0)
                            break;
                    } while (true);
                }
            }
            else
            {
                Logging.PrintError<Logger>($"Package: {packageName} doesn't exist or Asset: {assetName} location invalid.");
            }

            // (Caution) If use sync to load scene.isLoaded return false -> Why??
            if (!loaded)
            {
                this.UnloadScene(assetName, true);
                if (this.GetRetryCounter(assetName).IsRetryActive()) Logging.Print<Logger>($"【Load Scene】 => << {nameof(CacheBundle)} >> Asset: {assetName} doing retry. Retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}");
                else Logging.Print<Logger>($"【Load Scene】 => << {nameof(CacheBundle)} >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}");
                this.GetRetryCounter(assetName).AddRetryCount();
                return this.LoadScene(packageName, assetName, loadSceneMode, localPhysicsMode, progression);
            }

            this.RemoveLoadingFlags(assetName);

            return pack;
        }

        public void UnloadScene(string assetName, bool recursively)
        {
            /**
             * Single Scene will auto unload and release
             */

            if (string.IsNullOrEmpty(assetName)) return;

            if (this._additiveSceneCounter.ContainsKey(assetName))
            {
                if (recursively)
                {
                    for (int topCount = this._additiveSceneCounter[assetName]; topCount >= 1; --topCount)
                    {
                        string key = $"{assetName}#{topCount}";
                        if (this._additiveScenes.ContainsKey(key))
                        {
                            var pack = this._additiveScenes[key];
                            if (pack.IsSceneOperationHandle())
                            {
                                pack.UnloadScene();
                                this._additiveScenes[key] = null;
                                this._additiveScenes.Remove(key);

                                Logging.Print<Logger>($"【Unload Additive Scene】 => << {nameof(CacheBundle)} >> scene: {key}, count: {topCount}");
                            }
                        }
                    }

                    // 遞迴完, 移除計數緩存
                    this._additiveSceneCounter.Remove(assetName);

                    Logging.Print<Logger>($"【Unload Additive Scene Completes】 => << {nameof(CacheBundle)} >> sceneName: {assetName}, recursively: {recursively}");
                }
                else
                {
                    bool safetyChecker = false;

                    // 安全檢測無效場景卸載 (有可能被 Build 方法卸載掉)
                    for (int topCount = this._additiveSceneCounter[assetName]; topCount >= 1; --topCount)
                    {
                        string key = $"{assetName}#{topCount}";
                        var pack = this._additiveScenes[key];
                        if (pack.IsSceneOperationHandle())
                        {
                            if (pack.GetScene().isLoaded)
                            {
                                safetyChecker = true;
                                break;
                            }
                        }
                    }

                    // 啟用安全檢測卸載方法 (直接遞迴強制全部卸載)
                    if (safetyChecker)
                    {
                        ResourcePackage package = null;
                        for (int topCount = this._additiveSceneCounter[assetName]; topCount >= 1; --topCount)
                        {
                            string key = $"{assetName}#{topCount}";
                            if (this._additiveScenes.ContainsKey(key))
                            {
                                var pack = this._additiveScenes[key];
                                package = PackageManager.GetPackage(pack.packageName);
                                if (pack.IsSceneOperationHandle())
                                {
                                    pack.UnloadScene();
                                    this._additiveScenes[key] = null;
                                    this._additiveScenes.Remove(key);

                                    Logging.Print<Logger>($"【Safety Unload Additive Scene】 => << {nameof(CacheBundle)} >> scene: {key}, count: {topCount}");
                                }
                            }
                        }

                        // 遞迴完, 移除計數緩存
                        this._additiveSceneCounter.Remove(assetName);
                        package?.TryUnloadUnusedAsset(assetName, _DEFAULT_LOOP_COUNT);

                        Logging.Print<Logger>($"【Safety Unload Additive Scene Completes】 => << {nameof(CacheBundle)} >> sceneName: {assetName}, recursively: {recursively}");
                    }
                    else
                    {
                        int topCount = this._additiveSceneCounter[assetName];
                        string key = $"{assetName}#{topCount}";
                        var pack = this._additiveScenes[key];
                        string packageName = pack.packageName;

                        if (pack.IsSceneOperationHandle())
                        {
                            pack.UnloadScene();
                            this._additiveScenes[key] = null;
                            this._additiveScenes.Remove(key);

                            Logging.Print<Logger>($"【Unload Additive Scene】 => << {nameof(CacheBundle)} >> scene: {key}, count: {topCount}");

                            topCount = --this._additiveSceneCounter[assetName];

                            // 移除計數緩存
                            if (topCount <= 0)
                            {
                                var package = PackageManager.GetPackage(packageName);
                                this._additiveSceneCounter.Remove(assetName);
                                package?.TryUnloadUnusedAsset(assetName, _DEFAULT_LOOP_COUNT);
                                Logging.Print<Logger>($"【Unload Additive Scene Completes】 => << {nameof(CacheBundle)} >> sceneName: {assetName}, recursively: {recursively}");
                            }
                        }
                    }
                }
            }
            else Logging.PrintError<Logger>($"【Unload Scene Invalid】 => << {nameof(CacheBundle)} >> sceneName: {assetName} maybe not Additive or is Single");
        }

        public void ReleaseScenes()
        {
            if (this._additiveSceneCounter.Count == 0) return;

            HashSet<ResourcePackage> packages = new HashSet<ResourcePackage>();

            // 強制釋放緩存與資源
            foreach (var assetName in this._additiveSceneCounter.Keys.ToArray())
            {
                if (this.HasInCache(assetName))
                {
                    BundlePack pack = this.GetFromCache(assetName);
                    var package = PackageManager.GetPackage(pack.packageName);
                    if (!packages.Contains(package))
                        packages.Add(package);
                    this.UnloadScene(assetName, true);
                }
            }

            // 清除 Additive 計數緩存
            this._additiveScenes.Clear();
            this._additiveSceneCounter.Clear();

            // UnloadUnusedAssets
            foreach (var package in packages)
                this.UnloadUnusedAssets(package, false);

            // 調用底層接口釋放資源
            Resources.UnloadUnusedAssets();

            Logging.Print<Logger>($"【Release All Scenes (Addtive)】 => Current << {nameof(CacheBundle)} >> Additive Scene Cache Count: {this._additiveSceneCounter.Count}");
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
                    BundlePack pack = this.GetFromCache(assetName);
                    if (pack.IsAssetOperationHandleValid())
                    {
                        this.currentCount++;
                        progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);
                        this.RemoveLoadingFlags(assetName);
                        Logging.PrintWarning<Logger>($"【Preload】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}, asset: [{assetName}] already preloaded!!!");
                        continue;
                    }
                    else
                    {
                        this.UnloadAsset(assetName, true);
                        if (this.GetRetryCounter(assetName).IsRetryActive()) Logging.Print<Logger>($"【Preload】 => << {nameof(CacheBundle)} >> Asset: {assetName} doing retry. Retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}");
                        else Logging.Print<Logger>($"【Preload】 => << {nameof(CacheBundle)} >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}");
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
                                this.currentCount += (req.Progress - lastCount);
                                lastCount = req.Progress;
                                progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);

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
                        Logging.PrintError<Logger>($"Package: {packageName} doesn't exist or Asset: {assetName} location invalid.");
                    }

                    if (loaded)
                    {
                        // skipping duplicate keys
                        if (!this.HasInCache(assetName))
                        {
                            this._cacher.Add(assetName, pack);
                            Logging.Print<Logger>($"【Preload】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}, asset: {assetName}");
                        }
                    }
                    else
                    {
                        if (this.GetRetryCounter(assetName).IsRetryActive()) Logging.Print<Logger>($"【Preload】 => << {nameof(CacheBundle)} >> Asset: {assetName} doing retry. Retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}");
                        else Logging.Print<Logger>($"【Preload】 => << {nameof(CacheBundle)} >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}");
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
                    BundlePack pack = this.GetFromCache(assetName);
                    if (pack.IsAssetOperationHandleValid())
                    {
                        this.currentCount++;
                        progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);
                        this.RemoveLoadingFlags(assetName);
                        Logging.PrintWarning<Logger>($"【Preload】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}, asset: [{assetName}] already preloaded!!!");
                        continue;
                    }
                    else
                    {
                        this.UnloadAsset(assetName, true);
                        if (this.GetRetryCounter(assetName).IsRetryActive()) Logging.Print<Logger>($"【Preload】 => << {nameof(CacheBundle)} >> Asset: {assetName} doing retry. Retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}");
                        else Logging.Print<Logger>($"【Preload】 => << {nameof(CacheBundle)} >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}");
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
                                this.currentCount++;
                                progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);

                                loaded = true;
                                pack.SetPack(packageName, assetName, req);
                            }
                        }
                    }
                    else
                    {
                        Logging.PrintError<Logger>($"Package: {packageName} doesn't exist or Asset: {assetName} location invalid.");
                    }

                    if (loaded)
                    {
                        // skipping duplicate keys
                        if (!this.HasInCache(assetName))
                        {
                            this._cacher.Add(assetName, pack);
                            Logging.Print<Logger>($"【Preload】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}, asset: {assetName}");
                        }
                    }
                    else
                    {
                        if (this.GetRetryCounter(assetName).IsRetryActive()) Logging.Print<Logger>($"【Preload】 => << {nameof(CacheBundle)} >> Asset: {assetName} doing retry. Retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}");
                        else Logging.Print<Logger>($"【Preload】 => << {nameof(CacheBundle)} >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}");
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
                            this.currentCount += (req.Progress - lastCount);
                            lastCount = req.Progress;
                            progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);

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
                    Logging.PrintError<Logger>($"Package: {packageName} doesn't exist or Asset: {assetName} location invalid.");
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
                Logging.Print<Logger>($"【Load】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}, asset: {assetName}, ref: {pack.refCount}");
            }
            else
            {
                this.UnloadAsset(assetName, true);
                if (this.GetRetryCounter(assetName).IsRetryActive()) Logging.Print<Logger>($"【Load】 => << {nameof(CacheBundle)} >> Asset: {assetName} doing retry. Retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}");
                else Logging.Print<Logger>($"【Load】 => << {nameof(CacheBundle)} >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}");
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
                            this.currentCount++;
                            progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);

                            loaded = true;
                            pack.SetPack(packageName, assetName, req);
                        }
                    }
                }
                else
                {
                    Logging.PrintError<Logger>($"Package: {packageName} doesn't exist or Asset: {assetName} location invalid.");
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
                Logging.Print<Logger>($"【Load】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}, asset: {assetName}, ref: {pack.refCount}");
            }
            else
            {
                this.UnloadAsset(assetName, true);
                if (this.GetRetryCounter(assetName).IsRetryActive()) Logging.Print<Logger>($"【Load】 => << {nameof(CacheBundle)} >> Asset: {assetName} doing retry. Retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}");
                else Logging.Print<Logger>($"【Load】 => << {nameof(CacheBundle)} >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}");
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
                Logging.PrintWarning<Logger>($"Asset: {assetName} Loading...");
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

                    Logging.Print<Logger>($"【Unload】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}, asset: {assetName}, ref: {this._cacher.TryGetValue(assetName, out var v)} {v?.refCount}");

                    if (forceUnload)
                    {
                        pack.UnloadAsset();
                        this._cacher[assetName] = null;
                        this._cacher.Remove(assetName);

                        var package = PackageManager.GetPackage(packageName);
                        package?.TryUnloadUnusedAsset(assetName, _DEFAULT_LOOP_COUNT);

                        Logging.Print<Logger>($"【Force Unload Completes】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}, asset: {assetName}");
                    }
                    else if (this._cacher[assetName].IsReleasable())
                    {
                        pack.UnloadAsset();
                        this._cacher[assetName] = null;
                        this._cacher.Remove(assetName);

                        var package = PackageManager.GetPackage(packageName);
                        package?.TryUnloadUnusedAsset(assetName, _DEFAULT_LOOP_COUNT);

                        Logging.Print<Logger>($"【Unload Completes】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}, asset: {assetName}");
                    }
                }
                else Logging.PrintError<Logger>($"【Unload Type Error】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}, asset: {assetName}, ref: {this._cacher.TryGetValue(assetName, out var v)} {v?.refCount}");
            }
        }

        public void ReleaseAssets()
        {
            if (this.count == 0)
                return;

            HashSet<ResourcePackage> packages = new HashSet<ResourcePackage>();

            // 強制釋放緩存與資源
            foreach (var assetName in this._cacher.Keys.ToArray())
            {
                if (this.HasInCache(assetName))
                {
                    BundlePack pack = this.GetFromCache(assetName);
                    var package = PackageManager.GetPackage(pack.packageName);
                    if (!packages.Contains(package))
                        packages.Add(package);
                    this.UnloadAsset(assetName, true);
                }
            }

            // UnloadUnusedAssets
            foreach (var package in packages)
                this.UnloadUnusedAssets(package, false);

            // 調用底層接口釋放資源
            Resources.UnloadUnusedAssets();

            Logging.Print<Logger>($"【Release All Assets】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}");
        }
        #endregion

        #region Common
        /// <summary>
        /// 同步處理 UnloadUnusedAssets
        /// </summary>
        /// <param name="package"></param>
        /// <param name="tryResourcesUnloadUnusedAssets"></param>
        internal void UnloadUnusedAssets(ResourcePackage package, bool tryResourcesUnloadUnusedAssets)
        {
            var operation = package?.UnloadUnusedAssetsAsync(_DEFAULT_LOOP_COUNT);

            // 同步處理
            if (operation != null)
                operation.WaitForAsyncComplete();

            // 調用底層接口釋放資源
            if (tryResourcesUnloadUnusedAssets)
                Resources.UnloadUnusedAssets();
        }
        #endregion

        ~CacheBundle()
        {
            this._additiveScenes.Clear();
            this._additiveScenes = null;
            this._additiveSceneCounter.Clear();
            this._additiveSceneCounter = null;
        }
    }
}