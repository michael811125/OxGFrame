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
            return this._cacher.ContainsKey(assetName) || this._additiveSceneCounter.ContainsKey(assetName);
        }

        public override BundlePack GetFromCache(string assetName)
        {
            if (this.HasInCache(assetName))
            {
                if (this._cacher.TryGetValue(assetName, out BundlePack pack))
                    return pack;
            }

            return null;
        }

        #region Scene
        public async UniTask<BundlePack> LoadSceneAsync(string packageName, string assetName, LoadSceneMode loadSceneMode, LocalPhysicsMode localPhysicsMode, bool activateOnLoad, uint priority, Progression progression)
        {
            /**
             * Single Scene will auto unload and release
             */

            if (string.IsNullOrEmpty(assetName))
                return null;

            if (this.GetRetryCounter(assetName) != null &&
                this.GetRetryCounter(assetName).IsOutOfRetries())
            {
                this.StopRetryCounter(assetName);
                Logging.PrintWarning<Logger>($"Asset: {assetName} Load failed and cannot retry anymore!!! Please to check asset is existing.");
                return null;
            }

            // Additive 場景不使用單飛, 因為可以載入多個實例
            if (loadSceneMode == LoadSceneMode.Additive)
            {
                // Additive 場景直接載入, 不檢查單飛
                return await this._LoadSceneCoreAsync(packageName, assetName, loadSceneMode, localPhysicsMode, activateOnLoad, priority, progression);
            }

            // Single 場景才使用單飛, 檢查是否有進行中的載入任務
            if (this.TryGetLoadingTask(assetName, out var existingTask))
            {
                Logging.Print<Logger>($"Scene: {assetName} is loading, waiting for existing task...");
                var source = (UniTaskCompletionSource<BundlePack>)existingTask;
                return await source.Task;
            }

            // 建立載入任務並加入緩存 (只有 Single 場景會走到這裡)
            var completionSource = new UniTaskCompletionSource<BundlePack>();
            this.TryAddLoadingTask(assetName, completionSource);

            try
            {
                var result = await this._LoadSceneCoreAsync(packageName, assetName, loadSceneMode, localPhysicsMode, activateOnLoad, priority, progression);
                completionSource.TrySetResult(result);
                return result;
            }
            catch (System.Exception ex)
            {
                completionSource.TrySetException(ex);
                throw;
            }
            finally
            {
                // 任務完成後移除
                this.TryRemoveLoadingTask(assetName);
                this.ProcessPendingUnloads(assetName, ProcessType.Scene, this._UnloadSceneCore);
            }
        }

        /// <summary>
        /// 實際的場景載入邏輯
        /// </summary>
        private async UniTask<BundlePack> _LoadSceneCoreAsync(string packageName, string assetName, LoadSceneMode loadSceneMode, LocalPhysicsMode localPhysicsMode, bool activateOnLoad, uint priority, Progression progression)
        {
            // 初始加載進度
            this.currentCount = 0;
            this.totalCount = 1;

            // 場景最多嘗試 1 次
            byte maxRetryCount = 1;

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

                        if (req.IsDone || suspendLoaded)
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

                                        // 【每次載入都增加計數】Additive 需要進行計數, 要手動卸載子場景
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
                if (!loaded)
                {
                    // Retry 邏輯
                    if (this.GetRetryCounter(assetName) == null)
                    {
                        this.StartRetryCounter(assetName, maxRetryCount);
                        Logging.Print<Logger>($"【Load Scene】 => << {nameof(CacheBundle)} >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}");
                    }
                    else
                    {
                        Logging.Print<Logger>($"【Load Scene】 => << {nameof(CacheBundle)} >> Asset: {assetName} doing retry. Remaining retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}");
                    }

                    this.GetRetryCounter(assetName).DelRetryCount();
                    this.TryRemoveLoadingTask(assetName);
                    return await this.LoadSceneAsync(packageName, assetName, loadSceneMode, localPhysicsMode, activateOnLoad, priority, progression);
                }
            }

            return pack;
        }

        public BundlePack LoadScene(string packageName, string assetName, LoadSceneMode loadSceneMode, LocalPhysicsMode localPhysicsMode, Progression progression)
        {
            /**
             * Single Scene will auto unload and release
             */

            if (string.IsNullOrEmpty(assetName))
                return null;

            if (this.GetRetryCounter(assetName) != null &&
                this.GetRetryCounter(assetName).IsOutOfRetries())
            {
                this.StopRetryCounter(assetName);
                Logging.PrintWarning<Logger>($"Asset: {assetName} Load failed and cannot retry anymore!!! Please to check asset is existing.");
                return null;
            }

            // 初始加載進度
            this.currentCount = 0;
            this.totalCount = 1;

            // 場景最多嘗試 1 次
            byte maxRetryCount = 1;

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
                if (this.GetRetryCounter(assetName) == null)
                {
                    this.StartRetryCounter(assetName, maxRetryCount);
                    Logging.Print<Logger>($"【Load Scene】 => << {nameof(CacheBundle)} >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}");
                }
                else
                {
                    Logging.Print<Logger>($"【Load Scene】 => << {nameof(CacheBundle)} >> Asset: {assetName} doing retry. Remaining retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}");
                }

                this.GetRetryCounter(assetName).DelRetryCount();
                return this.LoadScene(packageName, assetName, loadSceneMode, localPhysicsMode, progression);
            }

            return pack;
        }

        public void UnloadScene(string assetName, bool recursively)
        {
            this._UnloadSceneCore(assetName, recursively, ProcessType.Scene);
        }

        private void _UnloadSceneCore(string assetName, bool recursively, ProcessType processType)
        {
            /**
             * Single Scene will auto unload and release
             */

            if (string.IsNullOrEmpty(assetName))
                return;

            // 如果正在載入, 將卸載請求加入待執行隊列
            if (this.HasLoadingTask(assetName))
            {
                this.AddPendingUnload(assetName, recursively);
                Logging.PrintWarning<Logger>($"【Pending Unload】 Asset: {assetName} is loading, queued unload request.");
                return;
            }

            // 如果正在執行卸載, 跳過
            if (this.HasUnloadingFlag(assetName))
            {
                Logging.PrintWarning<Logger>($"【Try Unload】 Asset: {assetName} is already unloading...");
                return;
            }

            if (this._additiveSceneCounter.ContainsKey(assetName))
            {
                this.AddUnloadingFlag(assetName);

                try
                {
                    if (recursively)
                    {
                        ResourcePackage package = null;
                        for (int topCount = this._additiveSceneCounter[assetName]; topCount >= 1; --topCount)
                        {
                            string key = $"{assetName}#{topCount}";
                            if (this._additiveScenes.ContainsKey(key))
                            {
                                var pack = this._additiveScenes[key];
                                if (package == null)
                                    package = PackageManager.GetPackage(pack.packageName);
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
                        package?.TryUnloadUnusedAsset(assetName, _DEFAULT_LOOP_COUNT);

                        Logging.Print<Logger>($"【Unload Additive Scene Completes】 => << {nameof(CacheBundle)} >> sceneName: {assetName}, recursively: {recursively}");
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

                            topCount = --this._additiveSceneCounter[assetName];

                            Logging.Print<Logger>($"【Unload Additive Scene】 => << {nameof(CacheBundle)} >> scene: {key}, count: {topCount}");

                            // 移除計數緩存
                            if (topCount <= 0)
                            {
                                ResourcePackage package = PackageManager.GetPackage(packageName);
                                this._additiveSceneCounter.Remove(assetName);
                                package?.TryUnloadUnusedAsset(assetName, _DEFAULT_LOOP_COUNT);

                                Logging.Print<Logger>($"【Unload Additive Scene Completes】 => << {nameof(CacheBundle)} >> sceneName: {assetName}, recursively: {recursively}");
                            }
                        }
                    }
                }
                finally
                {
                    this.RemoveUnloadingFlag(assetName);
                }
            }
            else Logging.PrintError<Logger>($"【Unload Scene Invalid】 => << {nameof(CacheBundle)} >> sceneName: {assetName} maybe not Additive or is Single");
        }

        public void ReleaseScenes()
        {
            if (this._additiveSceneCounter.Count == 0)
                return;

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

        #region RawFile
        public async UniTask PreloadRawFileAsync(string packageName, string[] assetNames, uint priority, Progression progression, byte maxRetryCount)
        {
            if (assetNames == null || assetNames.Length == 0)
                return;

            // 先初始加載進度
            this.currentCount = 0;
            this.totalCount = assetNames.Length;

            for (int i = 0; i < assetNames.Length; i++)
            {
                var assetName = assetNames[i];

                if (string.IsNullOrEmpty(assetName))
                    continue;

                if (this.GetRetryCounter(assetName) != null &&
                    this.GetRetryCounter(assetName).IsOutOfRetries())
                {
                    this.StopRetryCounter(assetName);
                    Logging.PrintWarning<Logger>($"Asset: {assetName} Load failed and cannot retry anymore!!! Please to check asset is existing.");
                    continue;
                }

                // 檢查是否有進行中的載入任務, 如果有就等待它完成
                if (this.TryGetLoadingTask(assetName, out var existingTask))
                {
                    Logging.Print<Logger>($"RawFile: {assetName} is loading, waiting for existing task...");

                    try
                    {
                        var source = (UniTaskCompletionSource)existingTask;
                        await source.Task;

                        // 等待完成後更新進度
                        this.currentCount++;
                        progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);
                    }
                    catch (System.Exception ex)
                    {
                        Logging.PrintError<Logger>($"【Preload】Asset: {assetName} failed: {ex.Message}");
                    }
                    continue;
                }

                // 如果有在緩存中就不進行預加載
                if (this.HasInCache(assetName))
                {
                    BundlePack pack = this.GetFromCache(assetName);
                    if (pack.IsRawFileOperationHandleValid())
                    {
                        this.currentCount++;
                        progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);
                        Logging.PrintWarning<Logger>($"【Preload】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}, asset: [{assetName}] already preloaded!!!");
                    }
                    continue;
                }

                // 建立載入任務並加入緩存
                var completionSource = new UniTaskCompletionSource();
                this.TryAddLoadingTask(assetName, completionSource);

                try
                {
                    await this._PreloadRawFileCoreAsync(packageName, assetName, priority, progression, maxRetryCount);
                    completionSource.TrySetResult();

                    // 載入完成後更新進度
                    this.currentCount++;
                    progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);
                }
                catch (System.Exception ex)
                {
                    completionSource.TrySetException(ex);
                    Logging.PrintError<Logger>($"【Preload】Asset: {assetName} failed: {ex.Message}");
                }
                finally
                {
                    // 任務完成後移除
                    this.TryRemoveLoadingTask(assetName);
                    this.ProcessPendingUnloads(assetName, ProcessType.RawFile, this._UnloadCore);
                }
            }
        }

        /// <summary>
        /// 實際的原始文件預載入邏輯
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="assetName"></param>
        /// <param name="priority"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        private async UniTask _PreloadRawFileCoreAsync(string packageName, string assetName, uint priority, Progression progression, byte maxRetryCount)
        {
            bool loaded = false;
            BundlePack pack = new BundlePack();

            var package = PackageManager.GetPackage(packageName);
            if (package != null && package.CheckLocationValid(assetName))
            {
                var req = package.LoadRawFileAsync(assetName, priority);
                if (req != null)
                {
                    // 注意: 這裡不更新 currentCount, 因為進度由外層控制
                    do
                    {
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
                // Retry 邏輯
                if (this.GetRetryCounter(assetName) == null)
                {
                    this.StartRetryCounter(assetName, maxRetryCount);
                    Logging.Print<Logger>($"【Preload】 => << {nameof(CacheBundle)} >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}");
                }
                else
                {
                    Logging.Print<Logger>($"【Preload】 => << {nameof(CacheBundle)} >> Asset: {assetName} doing retry. Remaining retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}");
                }

                this.GetRetryCounter(assetName).DelRetryCount();
                this.TryRemoveLoadingTask(assetName);
                await this.PreloadRawFileAsync(packageName, new string[] { assetName }, priority, progression, maxRetryCount);
            }
        }

        public void PreloadRawFile(string packageName, string[] assetNames, Progression progression, byte maxRetryCount)
        {
            if (assetNames == null || assetNames.Length == 0)
                return;

            // 先初始加載進度
            this.currentCount = 0;
            this.totalCount = assetNames.Length;

            for (int i = 0; i < assetNames.Length; i++)
            {
                var assetName = assetNames[i];

                if (string.IsNullOrEmpty(assetName))
                    continue;

                if (this.GetRetryCounter(assetName) != null &&
                    this.GetRetryCounter(assetName).IsOutOfRetries())
                {
                    this.StopRetryCounter(assetName);
                    Logging.PrintWarning<Logger>($"Asset: {assetName} Load failed and cannot retry anymore!!! Please to check asset is existing.");
                    continue;
                }

                // 如果有在緩存中就不進行預加載
                if (this.HasInCache(assetName))
                {
                    BundlePack pack = this.GetFromCache(assetName);
                    if (pack.IsRawFileOperationHandleValid())
                    {
                        this.currentCount++;
                        progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);
                        Logging.PrintWarning<Logger>($"【Preload】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}, asset: [{assetName}] already preloaded!!!");
                    }
                    continue;
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
                        if (this.GetRetryCounter(assetName) == null)
                        {
                            this.StartRetryCounter(assetName, maxRetryCount);
                            Logging.Print<Logger>($"【Preload】 => << {nameof(CacheBundle)} >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}");
                        }
                        else
                        {
                            Logging.Print<Logger>($"【Preload】 => << {nameof(CacheBundle)} >> Asset: {assetName} doing retry. Remaining retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}");
                        }

                        this.GetRetryCounter(assetName).DelRetryCount();
                        this.PreloadRawFile(packageName, new string[] { assetName }, progression, maxRetryCount);
                        continue;
                    }
                }
            }
        }

        public async UniTask<T> LoadRawFileAsync<T>(string packageName, string assetName, uint priority, Progression progression, byte maxRetryCount)
        {
            if (string.IsNullOrEmpty(assetName))
                return default;

            if (this.GetRetryCounter(assetName) != null &&
                this.GetRetryCounter(assetName).IsOutOfRetries())
            {
                this.StopRetryCounter(assetName);
                Logging.PrintWarning<Logger>($"Asset: {assetName} Load failed and cannot retry anymore!!! Please to check asset is existing.");
                return default;
            }

            BundlePack pack = null;

            // 檢查是否有進行中的載入任務
            if (this.TryGetLoadingTask(assetName, out var existingTask))
            {
                Logging.Print<Logger>($"RawFile: {assetName} is loading, waiting for existing task...");
                var source = (UniTaskCompletionSource<T>)existingTask;
                var data = await source.Task;
                if (data != null)
                {
                    pack = this.GetFromCache(assetName);
                    if (pack != null)
                    {
                        pack.AddRef();
                        Logging.Print<Logger>($"【Load Shared】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}, asset: {assetName}, ref: {pack.refCount}");
                    }
                }
                return data;
            }

            // 先從緩存拿
            pack = this.GetFromCache(assetName);
            if (pack != null)
            {
                // 緩存命中, 直接返回
                this.currentCount = this.totalCount = 1;
                progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);

                object data = this._GetRawFileData<T>(pack);
                if (data != null)
                {
                    pack.AddRef();
                    Logging.Print<Logger>($"【Load】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}, asset: {assetName}, ref: {pack.refCount}");
                    return (T)data;
                }
            }

            // 建立載入任務並加入緩存
            var completionSource = new UniTaskCompletionSource<T>();
            this.TryAddLoadingTask(assetName, completionSource);

            try
            {
                var result = await this._LoadRawFileCoreAsync<T>(packageName, assetName, priority, progression, maxRetryCount);
                completionSource.TrySetResult(result);
                return result;
            }
            catch (System.Exception ex)
            {
                completionSource.TrySetException(ex);
                throw;
            }
            finally
            {
                // 任務完成後移除
                this.TryRemoveLoadingTask(assetName);
                this.ProcessPendingUnloads(assetName, ProcessType.RawFile, this._UnloadCore);
            }
        }

        /// <summary>
        /// 實際的原始文件載入邏輯
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="packageName"></param>
        /// <param name="assetName"></param>
        /// <param name="priority"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        private async UniTask<T> _LoadRawFileCoreAsync<T>(string packageName, string assetName, uint priority, Progression progression, byte maxRetryCount)
        {
            // 初始加載進度
            this.currentCount = 0;
            this.totalCount = 1;

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
                    this._cacher.Add(assetName, pack);
            }

            object data = this._GetRawFileData<T>(pack);

            if (data != null)
            {
                // 引用計數++
                pack.AddRef();
                Logging.Print<Logger>($"【Load】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}, asset: {assetName}, ref: {pack.refCount}");
                return (T)data;
            }
            else
            {
                // Retry 邏輯
                if (this.GetRetryCounter(assetName) == null)
                {
                    this.StartRetryCounter(assetName, maxRetryCount);
                    Logging.Print<Logger>($"【Load】 => << {nameof(CacheBundle)} >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}");
                }
                else
                {
                    Logging.Print<Logger>($"【Load】 => << {nameof(CacheBundle)} >> Asset: {assetName} doing retry. Remaining retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}");
                }

                this.GetRetryCounter(assetName).DelRetryCount();
                this.TryRemoveLoadingTask(assetName);
                return await this.LoadRawFileAsync<T>(packageName, assetName, priority, progression, maxRetryCount);
            }
        }

        /// <summary>
        /// 提取資料的輔助方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pack"></param>
        /// <returns></returns>
        private object _GetRawFileData<T>(BundlePack pack)
        {
            if (typeof(T) == typeof(string))
            {
                return pack.GetRawFileText();
            }
            else if (typeof(T) == typeof(byte[]))
            {
                return pack.GetRawFileData();
            }
            return null;
        }

        public T LoadRawFile<T>(string packageName, string assetName, Progression progression, byte maxRetryCount)
        {
            if (string.IsNullOrEmpty(assetName))
                return default;

            if (this.GetRetryCounter(assetName) != null &&
                this.GetRetryCounter(assetName).IsOutOfRetries())
            {
                this.StopRetryCounter(assetName);
                Logging.PrintWarning<Logger>($"Asset: {assetName} Load failed and cannot retry anymore!!! Please to check asset is existing.");
                return default;
            }

            // 初始加載進度
            this.currentCount = 0;
            this.totalCount = 1;

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
                    if (!this.HasInCache(assetName))
                        this._cacher.Add(assetName, pack);
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
                if (this.GetRetryCounter(assetName) == null)
                {
                    this.StartRetryCounter(assetName, maxRetryCount);
                    Logging.Print<Logger>($"【Load】 => << {nameof(CacheBundle)} >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}");
                }
                else
                {
                    Logging.Print<Logger>($"【Load】 => << {nameof(CacheBundle)} >> Asset: {assetName} doing retry. Remaining retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}");
                }

                this.GetRetryCounter(assetName).DelRetryCount();
                return this.LoadRawFile<T>(packageName, assetName, progression, maxRetryCount);
            }

            return (T)data;
        }

        public void UnloadRawFile(string assetName, bool forceUnload)
        {
            this.CheckUnload(assetName, forceUnload, ProcessType.RawFile, this._UnloadCore);
        }

        public void ReleaseRawFiles()
        {
            this._ReleaseCore(ProcessType.RawFile);
        }
        #endregion

        #region Asset
        public async UniTask PreloadAssetAsync<T>(string packageName, string[] assetNames, uint priority, Progression progression, byte maxRetryCount) where T : Object
        {
            if (assetNames == null || assetNames.Length == 0)
                return;

            // 先初始加載進度
            this.currentCount = 0;
            this.totalCount = assetNames.Length;

            for (int i = 0; i < assetNames.Length; i++)
            {
                var assetName = assetNames[i];

                if (string.IsNullOrEmpty(assetName))
                    continue;

                if (this.GetRetryCounter(assetName) != null &&
                    this.GetRetryCounter(assetName).IsOutOfRetries())
                {
                    this.StopRetryCounter(assetName);
                    Logging.PrintWarning<Logger>($"Asset: {assetName} Load failed and cannot retry anymore!!! Please to check asset is existing.");
                    continue;
                }

                // 檢查是否有進行中的載入任務, 如果有就等待它完成
                if (this.TryGetLoadingTask(assetName, out var existingTask))
                {
                    Logging.Print<Logger>($"Asset: {assetName} is loading, waiting for existing task...");

                    try
                    {
                        var source = (UniTaskCompletionSource)existingTask;
                        await source.Task;

                        // 等待完成後更新進度
                        this.currentCount++;
                        progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);
                    }
                    catch (System.Exception ex)
                    {
                        Logging.PrintError<Logger>($"【Preload】Asset: {assetName} failed: {ex.Message}");
                    }
                    continue;
                }

                // 如果有在緩存中就不進行預加載
                if (this.HasInCache(assetName))
                {
                    BundlePack pack = this.GetFromCache(assetName);
                    if (pack.IsAssetOperationHandleValid())
                    {
                        this.currentCount++;
                        progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);
                        Logging.PrintWarning<Logger>($"【Preload】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}, asset: [{assetName}] already preloaded!!!");
                    }
                    continue;
                }

                // 建立載入任務並加入緩存
                var completionSource = new UniTaskCompletionSource();
                this.TryAddLoadingTask(assetName, completionSource);

                try
                {
                    await this._PreloadAssetCoreAsync<T>(packageName, assetName, priority, progression, maxRetryCount);
                    completionSource.TrySetResult();

                    // 載入完成後更新進度
                    this.currentCount++;
                    progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);
                }
                catch (System.Exception ex)
                {
                    completionSource.TrySetException(ex);
                    Logging.PrintError<Logger>($"【Preload】Asset: {assetName} failed: {ex.Message}");
                }
                finally
                {
                    // 任務完成後移除
                    this.TryRemoveLoadingTask(assetName);
                    this.ProcessPendingUnloads(assetName, ProcessType.Asset, this._UnloadCore);
                }
            }
        }

        /// <summary>
        /// 實際的預載入邏輯
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="packageName"></param>
        /// <param name="assetName"></param>
        /// <param name="priority"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        private async UniTask _PreloadAssetCoreAsync<T>(string packageName, string assetName, uint priority, Progression progression, byte maxRetryCount) where T : Object
        {
            bool loaded = false;
            BundlePack pack = new BundlePack();

            var package = PackageManager.GetPackage(packageName);
            if (package != null && package.CheckLocationValid(assetName))
            {
                var req = package.LoadAssetAsync<T>(assetName, priority);
                if (req != null)
                {
                    // 注意: 這裡不更新 currentCount, 因為進度由外層控制
                    do
                    {
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
                // Retry 邏輯
                if (this.GetRetryCounter(assetName) == null)
                {
                    this.StartRetryCounter(assetName, maxRetryCount);
                    Logging.Print<Logger>($"【Preload】 => << {nameof(CacheBundle)} >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}");
                }
                else
                {
                    Logging.Print<Logger>($"【Preload】 => << {nameof(CacheBundle)} >> Asset: {assetName} doing retry. Remaining retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}");
                }

                this.GetRetryCounter(assetName).DelRetryCount();
                this.TryRemoveLoadingTask(assetName);
                await this.PreloadAssetAsync<T>(packageName, new string[] { assetName }, priority, progression, maxRetryCount);
            }
        }

        public void PreloadAsset<T>(string packageName, string[] assetNames, Progression progression, byte maxRetryCount) where T : Object
        {
            if (assetNames == null || assetNames.Length == 0)
                return;

            // 先初始加載進度
            this.currentCount = 0;
            this.totalCount = assetNames.Length;

            for (int i = 0; i < assetNames.Length; i++)
            {
                var assetName = assetNames[i];

                if (string.IsNullOrEmpty(assetName))
                    continue;

                if (this.GetRetryCounter(assetName) != null &&
                    this.GetRetryCounter(assetName).IsOutOfRetries())
                {
                    this.StopRetryCounter(assetName);
                    Logging.PrintWarning<Logger>($"Asset: {assetName} Load failed and cannot retry anymore!!! Please to check asset is existing.");
                    continue;
                }

                // 如果有在緩存中就不進行預加載
                if (this.HasInCache(assetName))
                {
                    BundlePack pack = this.GetFromCache(assetName);
                    if (pack.IsAssetOperationHandleValid())
                    {
                        this.currentCount++;
                        progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);
                        Logging.PrintWarning<Logger>($"【Preload】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}, asset: [{assetName}] already preloaded!!!");
                    }
                    continue;
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
                        if (this.GetRetryCounter(assetName) == null)
                        {
                            this.StartRetryCounter(assetName, maxRetryCount);
                            Logging.Print<Logger>($"【Preload】 => << {nameof(CacheBundle)} >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}");
                        }
                        else
                        {
                            Logging.Print<Logger>($"【Preload】 => << {nameof(CacheBundle)} >> Asset: {assetName} doing retry. Remaining retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}");
                        }

                        this.GetRetryCounter(assetName).DelRetryCount();
                        this.PreloadAsset<T>(packageName, new string[] { assetName }, progression, maxRetryCount);
                        continue;
                    }
                }
            }
        }

        public async UniTask<T> LoadAssetAsync<T>(string packageName, string assetName, uint priority, Progression progression, byte maxRetryCount) where T : Object
        {
            if (string.IsNullOrEmpty(assetName))
                return null;

            if (this.GetRetryCounter(assetName) != null &&
                this.GetRetryCounter(assetName).IsOutOfRetries())
            {
                this.StopRetryCounter(assetName);
                Logging.PrintWarning<Logger>($"Asset: {assetName} Load failed and cannot retry anymore!!! Please to check asset is existing.");
                return null;
            }

            BundlePack pack = null;

            // 檢查是否有進行中的載入任務
            if (this.TryGetLoadingTask(assetName, out var existingTask))
            {
                Logging.Print<Logger>($"Asset: {assetName} is loading, waiting for existing task...");
                // 等待現有的任務完成並返回結果
                var source = (UniTaskCompletionSource<T>)existingTask;
                var asset = await source.Task;
                if (asset != null)
                {
                    pack = this.GetFromCache(assetName);
                    if (pack != null)
                    {
                        pack.AddRef();
                        Logging.Print<Logger>($"【Load Shared】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}, asset: {assetName}, ref: {pack.refCount}");
                    }
                }
                return asset;
            }

            // 先從緩存拿
            pack = this.GetFromCache(assetName);
            if (pack != null)
            {
                // 緩存命中, 直接返回
                this.currentCount = this.totalCount;
                progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);

                var asset = pack.GetAsset<T>();
                if (asset != null)
                {
                    pack.AddRef();
                    Logging.Print<Logger>($"【Load】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}, asset: {assetName}, ref: {pack.refCount}");
                }
                return asset;
            }

            // 建立載入任務並緩存
            var completionSource = new UniTaskCompletionSource<T>();
            this.TryAddLoadingTask(assetName, completionSource);

            try
            {
                var result = await this._LoadAssetCoreAsync<T>(packageName, assetName, priority, progression, maxRetryCount);
                completionSource.TrySetResult(result);
                return result;
            }
            catch (System.Exception ex)
            {
                completionSource.TrySetException(ex);
                throw;
            }
            finally
            {
                // 任務完成後移除緩存 (無論成功或失敗)
                this.TryRemoveLoadingTask(assetName);
                this.ProcessPendingUnloads(assetName, ProcessType.Asset, this._UnloadCore);
            }
        }

        /// <summary>
        /// 實際的載入邏輯
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="packageName"></param>
        /// <param name="assetName"></param>
        /// <param name="priority"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        private async UniTask<T> _LoadAssetCoreAsync<T>(string packageName, string assetName, uint priority, Progression progression, byte maxRetryCount) where T : Object
        {
            // 初始加載進度
            this.currentCount = 0;
            this.totalCount = 1;

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
                if (!this.HasInCache(assetName))
                    this._cacher.Add(assetName, pack);
            }

            var asset = pack.GetAsset<T>();

            if (asset != null)
            {
                pack.AddRef();
                Logging.Print<Logger>($"【Load】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}, asset: {assetName}, ref: {pack.refCount}");
                return asset;
            }
            else
            {
                // Retry 邏輯
                if (this.GetRetryCounter(assetName) == null)
                {
                    this.StartRetryCounter(assetName, maxRetryCount);
                    Logging.Print<Logger>($"【Load】 => << {nameof(CacheBundle)} >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}");
                }
                else
                {
                    Logging.Print<Logger>($"【Load】 => << {nameof(CacheBundle)} >> Asset: {assetName} doing retry. Remaining retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}");
                }

                this.GetRetryCounter(assetName).DelRetryCount();
                this.TryRemoveLoadingTask(assetName);
                return await this.LoadAssetAsync<T>(packageName, assetName, priority, progression, maxRetryCount);
            }
        }

        public T LoadAsset<T>(string packageName, string assetName, Progression progression, byte maxRetryCount) where T : Object
        {
            if (string.IsNullOrEmpty(assetName))
                return null;

            if (this.GetRetryCounter(assetName) != null &&
                this.GetRetryCounter(assetName).IsOutOfRetries())
            {
                this.StopRetryCounter(assetName);
                Logging.PrintWarning<Logger>($"Asset: {assetName} Load failed and cannot retry anymore!!! Please to check asset is existing.");
                return null;
            }

            // 初始加載進度
            this.currentCount = 0;
            this.totalCount = 1;

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
                    if (!this.HasInCache(assetName))
                        this._cacher.Add(assetName, pack);
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
                if (this.GetRetryCounter(assetName) == null)
                {
                    this.StartRetryCounter(assetName, maxRetryCount);
                    Logging.Print<Logger>($"【Load】 => << {nameof(CacheBundle)} >> Asset: {assetName} start doing retry. Max retry count: {maxRetryCount}");
                }
                else
                {
                    Logging.Print<Logger>($"【Load】 => << {nameof(CacheBundle)} >> Asset: {assetName} doing retry. Remaining retry count: {this.GetRetryCounter(assetName).retryCount}, Max retry count: {maxRetryCount}");
                }

                this.GetRetryCounter(assetName).DelRetryCount();
                return this.LoadAsset<T>(packageName, assetName, progression, maxRetryCount);
            }

            return asset;
        }

        public void UnloadAsset(string assetName, bool forceUnload)
        {
            this.CheckUnload(assetName, forceUnload, ProcessType.Asset, this._UnloadCore);
        }

        public void ReleaseAssets()
        {
            this._ReleaseCore(ProcessType.Asset);
        }
        #endregion

        #region Asset & RawFile Unload Core
        /// <summary>
        /// 實際執行卸載
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="forceUnload"></param>
        /// <param name="processType"></param>
        private void _UnloadCore(string assetName, bool forceUnload, ProcessType processType)
        {
            if (!this.HasInCache(assetName))
            {
                Logging.PrintWarning<Logger>($"【Unload】 Asset: {assetName} not in cache, skipping.");
                return;
            }

            BundlePack pack = this.GetFromCache(assetName);
            string packageName = pack.packageName;

            if (pack.IsRawFileOperationHandle() ||
                pack.IsAssetOperationHandle())
            {
                // 標記為正在卸載
                this.AddUnloadingFlag(assetName);

                try
                {
                    // 引用計數--
                    pack.DelRef();
                    Logging.Print<Logger>($"【Unload】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}, asset: {assetName}, ref: {pack.refCount}");

                    // 判斷是否需要實際卸載
                    bool shouldUnload = forceUnload || pack.IsReleasable();

                    if (shouldUnload)
                    {
                        if (processType == ProcessType.RawFile)
                            pack.UnloadRawFile();
                        else if (processType == ProcessType.Asset)
                            pack.UnloadAsset();

                        this._cacher[assetName] = null;
                        this._cacher.Remove(assetName);

                        var package = PackageManager.GetPackage(packageName);
                        package?.TryUnloadUnusedAsset(assetName, _DEFAULT_LOOP_COUNT);

                        Logging.Print<Logger>($"【Unload Completes】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}, asset: {assetName}");
                    }
                }
                finally
                {
                    // 移除卸載標記
                    this.RemoveUnloadingFlag(assetName);
                }
            }
            else
            {
                Logging.PrintError<Logger>($"【Unload Type Error】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}, asset: {assetName}, ref: {pack.refCount}");
            }
        }

        private void _ReleaseCore(ProcessType processType)
        {
            if (this.count == 0)
                return;

            HashSet<ResourcePackage> packages = new HashSet<ResourcePackage>();

            string type = processType == ProcessType.RawFile ? "RawFiles" : "Assets";

            // 強制釋放緩存與資源
            foreach (var assetName in this._cacher.Keys.ToArray())
            {
                if (this.HasInCache(assetName))
                {
                    BundlePack pack = this.GetFromCache(assetName);
                    var package = PackageManager.GetPackage(pack.packageName);
                    if (!packages.Contains(package))
                        packages.Add(package);

                    if (processType == ProcessType.RawFile)
                        this.UnloadRawFile(assetName, true);
                    else if (processType == ProcessType.Asset)
                        this.UnloadAsset(assetName, true);
                }
            }

            // UnloadUnusedAssets
            foreach (var package in packages)
                this.UnloadUnusedAssets(package, false);

            // 調用底層接口釋放資源
            Resources.UnloadUnusedAssets();

            Logging.Print<Logger>($"【Release All {type}】 => Current << {nameof(CacheBundle)} >> Cache Count: {this.count}");
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
    }
}