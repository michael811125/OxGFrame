using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader.Bundle;
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
        public async UniTask PreloadRawFileAsync(string packageName, string[] assetNames, Progression progression = null)
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

                // 如果有在緩存中就不進行預加載
                if (this.HasInCache(assetName))
                {
                    BundlePack pack = this.GetFromCache(assetName);
                    if (pack.IsValid())
                    {
                        this.reqSize++;
                        progression?.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
                        this._hashLoadingFlags.Remove(assetName);
                        continue;
                    }
                    else
                    {
                        if (this.HasInLoadingFlags(assetName)) this._hashLoadingFlags.Remove(assetName);
                        this.UnloadAsset(assetName, true);
                        await this.PreloadRawFileAsync(packageName, new string[] { assetName }, progression);
                        continue;
                    }
                }

                {
                    BundlePack pack = new BundlePack();

                    var package = PackageManager.GetPackage(packageName);
                    if (package == null) Debug.Log($"<color=#ff33ae>Package: {packageName} is not exist.</color>");
                    var req = package?.LoadRawFileAsync(assetName);

                    if (req != null)
                    {
                        float lastSize = 0;
                        do
                        {
                            if (progression != null)
                            {
                                this.reqSize += (req.Progress - lastSize);
                                lastSize = req.Progress;
                                progression.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
                            }

                            if (req.IsDone)
                            {
                                pack.SetPack(packageName, assetName, req);
                                break;
                            }
                            await UniTask.Yield();
                        } while (true);
                    }

                    if (pack != null)
                    {
                        // skipping duplicate keys
                        if (!this.HasInCache(assetName)) this._cacher.Add(assetName, pack);
                    }
                }

                // 移除標記
                this._hashLoadingFlags.Remove(assetName);

                Debug.Log($"<color=#ff9600>【Preload】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: {assetName}</color>");
            }
        }

        public void PreloadRawFile(string packageName, string[] assetNames, Progression progression = null)
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

                // 如果有在緩存中就不進行預加載
                if (this.HasInCache(assetName))
                {
                    BundlePack pack = this.GetFromCache(assetName);
                    if (pack.IsValid())
                    {
                        this.reqSize++;
                        progression?.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
                        this._hashLoadingFlags.Remove(assetName);
                        continue;
                    }
                    else
                    {
                        if (this.HasInLoadingFlags(assetName)) this._hashLoadingFlags.Remove(assetName);
                        this.UnloadAsset(assetName, true);
                        this.PreloadRawFile(packageName, new string[] { assetName }, progression);
                        continue;
                    }
                }

                {
                    BundlePack pack = new BundlePack();

                    var package = PackageManager.GetPackage(packageName);
                    if (package == null) Debug.Log($"<color=#ff33ae>Package: {packageName} is not exist.</color>");
                    var req = package?.LoadRawFileSync(assetName);

                    if (req != null)
                    {
                        if (req.IsDone)
                        {
                            if (progression != null)
                            {
                                this.reqSize++;
                                progression.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
                            }

                            pack.SetPack(packageName, assetName, req);
                        }
                    }

                    if (pack != null)
                    {
                        // skipping duplicate keys
                        if (!this.HasInCache(assetName)) this._cacher.Add(assetName, pack);
                    }
                }

                // 移除標記
                this._hashLoadingFlags.Remove(assetName);

                Debug.Log($"<color=#ff9600>【Preload】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: {assetName}</color>");
            }
        }

        public async UniTask<T> LoadRawFileAsync<T>(string packageName, string assetName, Progression progression = null)
        {
            if (string.IsNullOrEmpty(assetName)) return default;

            // 如果有進行 Loading 標記後, 直接 return
            if (this.HasInLoadingFlags(assetName))
            {
                Debug.Log($"<color=#FFDC8A>asset: {assetName} Loading...</color>");
                return default;
            }

            // 初始加載進度
            this.reqSize = 0;
            this.totalSize = 1;

            // Loading 標記
            this._hashLoadingFlags.Add(assetName);

            // 先從緩存拿
            BundlePack pack = this.GetFromCache(assetName);

            if (pack == null)
            {
                pack = new BundlePack();
                {
                    var package = PackageManager.GetPackage(packageName);
                    if (package == null) Debug.Log($"<color=#ff33ae>Package: {packageName} is not exist.</color>");
                    var req = package?.LoadRawFileAsync(assetName);

                    if (req != null)
                    {
                        float lastSize = 0;
                        do
                        {
                            if (progression != null)
                            {
                                this.reqSize += (req.Progress - lastSize);
                                lastSize = req.Progress;
                                progression.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
                            }

                            if (req.IsDone)
                            {
                                pack.SetPack(packageName, assetName, req);
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

            if (pack.IsValid())
            {
                // 引用計數++
                pack.AddRef();
            }
            else
            {
                if (this.HasInLoadingFlags(assetName)) this._hashLoadingFlags.Remove(assetName);
                this.UnloadAsset(assetName, true);
                return await this.LoadRawFileAsync<T>(packageName, assetName, progression);
            }

            this._hashLoadingFlags.Remove(assetName);

            Debug.Log($"<color=#90FF71>【Load】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: {assetName}, ref: {pack.refCount}</color>");

            if (typeof(T) == typeof(string))
            {
                return (T)(object)pack.GetRawFileText();
            }
            else if (typeof(T) == typeof(byte[]))
            {
                return (T)(object)pack.GetRawFileData();
            }
            else return default;
        }

        public T LoadRawFile<T>(string packageName, string assetName, Progression progression = null)
        {
            if (string.IsNullOrEmpty(assetName)) return default;

            // 如果有進行 Loading 標記後, 直接 return
            if (this.HasInLoadingFlags(assetName))
            {
                Debug.Log($"<color=#FFDC8A>asset: {assetName} Loading...</color>");
                return default;
            }

            // 初始加載進度
            this.reqSize = 0;
            this.totalSize = 1;

            // Loading 標記
            this._hashLoadingFlags.Add(assetName);

            // 先從緩存拿
            BundlePack pack = this.GetFromCache(assetName);

            if (pack == null)
            {
                pack = new BundlePack();
                {
                    var package = PackageManager.GetPackage(packageName);
                    if (package == null) Debug.Log($"<color=#ff33ae>Package: {packageName} is not exist.</color>");
                    var req = package?.LoadRawFileSync(assetName);

                    if (req != null)
                    {
                        if (req.IsDone)
                        {
                            if (progression != null)
                            {
                                this.reqSize++;
                                progression.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
                            }

                            pack.SetPack(packageName, assetName, req);
                        }
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

            if (pack.IsValid())
            {
                // 引用計數++
                pack.AddRef();
            }
            else
            {
                if (this.HasInLoadingFlags(assetName)) this._hashLoadingFlags.Remove(assetName);
                this.UnloadAsset(assetName, true);
                return this.LoadRawFile<T>(packageName, assetName, progression);
            }

            this._hashLoadingFlags.Remove(assetName);

            Debug.Log($"<color=#90FF71>【Load】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: {assetName}, ref: {pack.refCount}</color>");

            if (typeof(T) == typeof(string))
            {
                return (T)(object)pack.GetRawFileText();
            }
            else if (typeof(T) == typeof(byte[]))
            {
                return (T)(object)pack.GetRawFileData();
            }
            else return default;
        }

        public void UnloadRawFile(string assetName, bool forceUnload = false)
        {
            if (string.IsNullOrEmpty(assetName)) return;

            if (this.HasInLoadingFlags(assetName))
            {
                Debug.Log($"<color=#FFDC8A>asset: {assetName} Loading...</color>");
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

                    Debug.Log($"<color=#00e5ff>【<color=#ffcf92>Unload</color>】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: {assetName}, ref: {this._cacher.TryGetValue(assetName, out var v)} {v?.refCount}</color>");

                    if (forceUnload)
                    {
                        pack.UnloadRawFile();
                        this._cacher[assetName] = null;
                        this._cacher.Remove(assetName);

                        var package = PackageManager.GetPackage(packageName);
                        package?.UnloadUnusedAssets();

                        Debug.Log($"<color=#00e5ff>【<color=#ff92ef>Force Unload Completes</color>】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: {assetName}</color>");
                    }
                    else if (this._cacher[assetName].refCount <= 0)
                    {
                        pack.UnloadRawFile();
                        this._cacher[assetName] = null;
                        this._cacher.Remove(assetName);

                        var package = PackageManager.GetPackage(packageName);
                        package?.UnloadUnusedAssets();

                        Debug.Log($"<color=#00e5ff>【<color=#ff92ef>Unload Completes</color>】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: {assetName}</color>");
                    }
                }
                else Debug.Log($"<color=#00e5ff>【<color=#ffcf92>Unload Type Error</color>】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: {assetName}, ref: {this._cacher.TryGetValue(assetName, out var v)} {v?.refCount}</color>");
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
                    if (pack.IsRawFileOperationHandle()) this.UnloadRawFile(assetName, true);
                }
            }

            foreach (var package in packages)
            {
                package?.UnloadUnusedAssets();
            }

            Debug.Log($"<color=#ff71b7>【Release All RawFiles】 => Current << CacheBundle >> Cache Count: {this.Count}</color>");
        }
        #endregion

        #region Scene
        public async UniTask<BundlePack> LoadSceneAsync(string packageName, string assetName, LoadSceneMode loadSceneMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100, Progression progression = null)
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

            var package = PackageManager.GetPackage(packageName);
            if (package == null) Debug.Log($"<color=#ff33ae>Package: {packageName} is not exist.</color>");
            var req = package?.LoadSceneAsync(assetName, loadSceneMode, activateOnLoad, priority);

            var pack = new BundlePack();

            if (req != null)
            {
                float lastSize = 0;
                do
                {
                    if (progression != null)
                    {
                        this.reqSize += (req.Progress - lastSize);
                        lastSize = req.Progress;
                        progression.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
                    }

                    if (req.IsDone)
                    {
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
                                        Debug.Log($"<color=#90FF71>【Load Scene Additive】 => << CacheBundle >> scene: {key}</color>");
                                    }
                                    else
                                    {
                                        var count = ++this._sceneCounter[assetName];
                                        string key = $"{assetName}#{count}";
                                        this._sceneCache.Add(key, pack);
                                        Debug.Log($"<color=#90FF71>【Load Scene Additive】 => << CacheBundle >> scene: {key}</color>");
                                    }
                                }
                                break;
                        }
                        break;
                    }
                    await UniTask.Yield();
                } while (true);
            }

            this._hashLoadingFlags.Remove(assetName);

            return pack;
        }

        public void UnloadScene(string assetName, bool recursively = false)
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

                                Debug.Log($"<color=#00e5ff>【Unload Additive Scene】 => << CacheBundle >> scene: {key}, count: {topCount}</color>");
                            }
                        }
                    }

                    // 遞迴完, 移除計數緩存
                    this._sceneCounter.Remove(assetName);

                    Debug.Log($"<color=#00e5ff>【<color=#ff92ef>Unload Additive Scene Completes</color>】 => << CacheBundle >> sceneName: {assetName}, recursively: {recursively}</color>");
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
                            if (!pack.GetScene().isLoaded)
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
                                if (package == null) package = PackageManager.GetPackage(pack.packageName);
                                if (pack.IsSceneOperationHandle())
                                {
                                    pack.UnloadScene();
                                    this._sceneCache[key] = null;
                                    this._sceneCache.Remove(key);

                                    Debug.Log($"<color=#00e5ff>【<color=#97ff3e>Safty</color> Unload Additive Scene】 => << CacheBundle >> scene: {key}, count: {topCount}</color>");
                                }
                            }
                        }

                        // 遞迴完, 移除計數緩存
                        this._sceneCounter.Remove(assetName);
                        package?.UnloadUnusedAssets();

                        Debug.Log($"<color=#00e5ff>【<color=#ff92ef><color=#97ff3e>Safty</color> Unload Additive Scene Completes</color>】 => << CacheBundle >> sceneName: {assetName}, recursively: {recursively}</color>");
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

                            Debug.Log($"<color=#00e5ff>【Unload Additive Scene】 => << CacheBundle >> scene: {key}, count: {topCount}</color>");

                            topCount = --this._sceneCounter[assetName];

                            // 移除計數緩存
                            if (topCount <= 0)
                            {
                                var package = PackageManager.GetPackage(packageName);
                                this._sceneCounter.Remove(assetName);
                                package?.UnloadUnusedAssets();
                                Debug.Log($"<color=#00e5ff>【<color=#ff92ef>Unload Additive Scene Completes</color>】 => << CacheBundle >> sceneName: {assetName}, recursively: {recursively}</color>");
                            }
                        }
                    }
                }
            }
            else Debug.Log($"<color=#00e5ff>【<color=#ff4a8d>Unload Scene Invalid</color>】 => << CacheBundle >> sceneName: {assetName} maybe not <color=#ffb33e>Additive</color> or is <color=#ffb33e>Single</color></color>");
        }
        #endregion

        #region Asset
        public async UniTask PreloadAssetAsync(string packageName, string[] assetNames, Progression progression = null)
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

                // 如果有在緩存中就不進行預加載
                if (this.HasInCache(assetName))
                {
                    BundlePack pack = this.GetFromCache(assetName);
                    if (pack.IsValid())
                    {
                        this.reqSize++;
                        progression?.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
                        this._hashLoadingFlags.Remove(assetName);
                        continue;
                    }
                    else
                    {
                        if (this.HasInLoadingFlags(assetName)) this._hashLoadingFlags.Remove(assetName);
                        this.UnloadAsset(assetName, true);
                        await this.PreloadAssetAsync(packageName, new string[] { assetName }, progression);
                        continue;
                    }
                }

                {
                    BundlePack pack = new BundlePack();

                    var package = PackageManager.GetPackage(packageName);
                    if (package == null) Debug.Log($"<color=#ff33ae>Package: {packageName} is not exist.</color>");
                    var req = package?.LoadAssetAsync<Object>(assetName);

                    if (req != null)
                    {
                        float lastSize = 0;
                        do
                        {
                            if (progression != null)
                            {
                                this.reqSize += (req.Progress - lastSize);
                                lastSize = req.Progress;
                                progression.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
                            }

                            if (req.IsDone)
                            {
                                pack.SetPack(packageName, assetName, req);
                                break;
                            }
                            await UniTask.Yield();
                        } while (true);
                    }

                    if (pack != null)
                    {
                        // skipping duplicate keys
                        if (!this.HasInCache(assetName)) this._cacher.Add(assetName, pack);
                    }
                }

                // 移除標記
                this._hashLoadingFlags.Remove(assetName);

                Debug.Log($"<color=#ff9600>【Preload】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: {assetName}</color>");
            }
        }

        public void PreloadAsset(string packageName, string[] assetNames, Progression progression = null)
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

                // 如果有在緩存中就不進行預加載
                if (this.HasInCache(assetName))
                {
                    BundlePack pack = this.GetFromCache(assetName);
                    if (pack.IsValid())
                    {
                        this.reqSize++;
                        progression?.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
                        this._hashLoadingFlags.Remove(assetName);
                        continue;
                    }
                    else
                    {
                        if (this.HasInLoadingFlags(assetName)) this._hashLoadingFlags.Remove(assetName);
                        this.UnloadAsset(assetName, true);
                        this.PreloadAsset(packageName, new string[] { assetName }, progression);
                        continue;
                    }
                }

                {
                    BundlePack pack = new BundlePack();

                    var package = PackageManager.GetPackage(packageName);
                    if (package == null) Debug.Log($"<color=#ff33ae>Package: {packageName} is not exist.</color>");
                    var req = package?.LoadAssetSync<Object>(assetName);

                    if (req != null)
                    {
                        if (req.IsDone)
                        {
                            if (progression != null)
                            {
                                this.reqSize++;
                                progression.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
                            }

                            pack.SetPack(packageName, assetName, req);
                        }
                    }

                    if (pack != null)
                    {
                        // skipping duplicate keys
                        if (!this.HasInCache(assetName)) this._cacher.Add(assetName, pack);
                    }
                }

                // 移除標記
                this._hashLoadingFlags.Remove(assetName);

                Debug.Log($"<color=#ff9600>【Preload】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: {assetName}</color>");
            }
        }

        public async UniTask<T> LoadAssetAsync<T>(string packageName, string assetName, Progression progression = null) where T : Object
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

            // 先從緩存拿
            BundlePack pack = this.GetFromCache(assetName);

            if (pack == null)
            {
                pack = new BundlePack();
                {
                    var package = PackageManager.GetPackage(packageName);
                    if (package == null) Debug.Log($"<color=#ff33ae>Package: {packageName} is not exist.</color>");
                    var req = package?.LoadAssetAsync<Object>(assetName);

                    if (req != null)
                    {
                        float lastSize = 0;
                        do
                        {
                            if (progression != null)
                            {
                                this.reqSize += (req.Progress - lastSize);
                                lastSize = req.Progress;
                                progression.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
                            }

                            if (req.IsDone)
                            {
                                pack.SetPack(packageName, assetName, req);
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

            if (pack.IsValid())
            {
                // 引用計數++
                pack.AddRef();
            }
            else
            {
                if (this.HasInLoadingFlags(assetName)) this._hashLoadingFlags.Remove(assetName);
                this.UnloadAsset(assetName, true);
                return await this.LoadAssetAsync<T>(packageName, assetName, progression);
            }

            this._hashLoadingFlags.Remove(assetName);

            Debug.Log($"<color=#90FF71>【Load】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: {assetName}, ref: {pack.refCount}</color>");

            return pack.GetAsset<T>();
        }

        public T LoadAsset<T>(string packageName, string assetName, Progression progression = null) where T : Object
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

            // 先從緩存拿
            BundlePack pack = this.GetFromCache(assetName);

            if (pack == null)
            {
                pack = new BundlePack();
                {
                    var package = PackageManager.GetPackage(packageName);
                    if (package == null) Debug.Log($"<color=#ff33ae>Package: {packageName} is not exist.</color>");
                    var req = package?.LoadAssetSync<Object>(assetName);

                    if (req != null)
                    {
                        if (req.IsDone)
                        {
                            if (progression != null)
                            {
                                this.reqSize++;
                                progression.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
                            }

                            pack.SetPack(packageName, assetName, req);
                        }
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

            if (pack.IsValid())
            {
                // 引用計數++
                pack.AddRef();
            }
            else
            {
                if (this.HasInLoadingFlags(assetName)) this._hashLoadingFlags.Remove(assetName);
                this.UnloadAsset(assetName, true);
                return this.LoadAsset<T>(packageName, assetName, progression);
            }

            this._hashLoadingFlags.Remove(assetName);

            Debug.Log($"<color=#90FF71>【Load】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: {assetName}, ref: {pack.refCount}</color>");

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
                BundlePack pack = this.GetFromCache(assetName);
                string packageName = pack.packageName;

                if (pack.IsAssetOperationHandle())
                {
                    // 引用計數--
                    pack.DelRef();

                    Debug.Log($"<color=#00e5ff>【<color=#ffcf92>Unload</color>】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: {assetName}, ref: {this._cacher.TryGetValue(assetName, out var v)} {v?.refCount}</color>");

                    if (forceUnload)
                    {
                        pack.UnloadAsset();
                        this._cacher[assetName] = null;
                        this._cacher.Remove(assetName);

                        var package = PackageManager.GetPackage(packageName);
                        package?.UnloadUnusedAssets();

                        Debug.Log($"<color=#00e5ff>【<color=#ff92ef>Force Unload Completes</color>】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: {assetName}</color>");
                    }
                    else if (this._cacher[assetName].refCount <= 0)
                    {
                        pack.UnloadAsset();
                        this._cacher[assetName] = null;
                        this._cacher.Remove(assetName);

                        var package = PackageManager.GetPackage(packageName);
                        package?.UnloadUnusedAssets();

                        Debug.Log($"<color=#00e5ff>【<color=#ff92ef>Unload Completes</color>】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: {assetName}</color>");
                    }
                }
                else Debug.Log($"<color=#00e5ff>【<color=#ffcf92>Unload Type Error</color>】 => Current << CacheBundle >> Cache Count: {this.Count}, asset: {assetName}, ref: {this._cacher.TryGetValue(assetName, out var v)} {v?.refCount}</color>");
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
                    if (pack.IsAssetOperationHandle()) this.UnloadAsset(assetName, true);
                }
            }

            foreach (var package in packages)
            {
                package?.UnloadUnusedAssets();
            }

            Debug.Log($"<color=#ff71b7>【Release All Assets】 => Current << CacheBundle >> Cache Count: {this.Count}</color>");
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