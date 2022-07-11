using AssetLoader;
using AssetLoader.AssetCacher;
using AssetLoader.AssetObject;
using AssetLoader.Bundle;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class CacheBundle : AssetCache<BundlePack>, IBundle
{
    protected HashSet<string> _hashBundleLoadingFlags = new HashSet<string>();

    private static CacheBundle _instance = null;
    public static CacheBundle GetInstance()
    {
        if (_instance == null) _instance = new CacheBundle();
        return _instance;
    }

    public CacheBundle()
    {
        this._cacher = new Dictionary<string, BundlePack>();
    }

    public override bool HasInCache(string bundleName)
    {
        bundleName = bundleName.ToLower();

        return this._cacher.ContainsKey(bundleName);
    }

    public bool HasInBundleLoadingFlags(string bundleName)
    {
        if (string.IsNullOrEmpty(bundleName)) return false;
        return this._hashBundleLoadingFlags.Contains(bundleName);
    }

    public override BundlePack GetFromCache(string bundleName)
    {
        bundleName = bundleName.ToLower();

        if (this.HasInCache(bundleName))
        {
            if (this._cacher.TryGetValue(bundleName, out BundlePack bundlePack)) return bundlePack;
        }

        return null;
    }

    /// <summary>
    /// 預加載Bundle至快取中
    /// </summary>
    /// <param name="bundleName"></param>
    /// <param name="progression"></param>
    /// <returns></returns>
    public override async UniTask PreloadInCache(string bundleName, Progression progression = null)
    {
#if UNITY_EDITOR
        if (BundleConfig.bAssetDatabaseMode) return;
#endif

        bundleName = bundleName?.ToLower();

        if (string.IsNullOrEmpty(bundleName)) return;

        // 如果有進行Loading標記後, 直接return;
        if (this.HasInBundleLoadingFlags(bundleName))
        {
            Debug.Log($"<color=#FFDC8A>ab: {bundleName} Loading...</color>");
            return;
        }

        // 先設置加載進度
        this.reqSize = 0;                                        // 會由LoadBundlePack累加, 所以需歸0
        this.totalSize = await this.GetAssetsLength(bundleName); // 返回當前要預加載的總大小

        // Bundle Loading標記
        this._hashBundleLoadingFlags.Add(bundleName);

        // 如果有在快取中就不進行預加載
        if (this.HasInCache(bundleName))
        {
            // 在快取中請求大小就直接指定為資源總大小 (單個)
            this.reqSize = this.totalSize;
            // 處理進度回調
            progression?.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
            // 移除標記
            this._hashBundleLoadingFlags.Remove(bundleName);
            return;
        }

        var bundlePack = await this.LoadBundlePack(bundleName, progression);
        if (bundlePack != null)
        {
            if (!this.HasInCache(bundleName)) this._cacher.Add(bundleName, bundlePack);

            // 取得主要的Menifest中的AssetBundleManifest (因為記錄著所有資源的依賴性)
            var manifest = await this.GetManifest();
            if (manifest != null)
            {
                string[] dependencies = manifest.GetAllDependencies(bundleName);
                for (int i = 0; i < dependencies.Length; i++)
                {
                    if (this.HasInCache(dependencies[i])) continue;

                    BundlePack dependBundlePack = await this.LoadBundlePack(dependencies[i], progression);
                    if (dependBundlePack != null)
                    {
                        // skipping duplicate keys
                        if (!this.HasInCache(dependencies[i])) this._cacher.Add(dependencies[i], dependBundlePack);
                    }
                }
            }
        }

        // 移除標記
        this._hashBundleLoadingFlags.Remove(bundleName);

        await UniTask.Yield();

        Debug.Log("【預加載】 => 當前<< CacheBundle >>快取數量 : " + this.Count);
    }

    public override async UniTask PreloadInCache(string[] bundleNames, Progression progression = null)
    {
#if UNITY_EDITOR
        if (BundleConfig.bAssetDatabaseMode) return;
#endif

        if (bundleNames == null || bundleNames.Length == 0) return;

        // 先設置加載進度
        this.reqSize = 0;                                         // 會由LoadBundlePack累加, 所以需歸0
        this.totalSize = await this.GetAssetsLength(bundleNames); // 返回當前要預加載的總大小

        for (int i = 0; i < bundleNames.Length; i++)
        {
            var bundleName = bundleNames[i]?.ToLower();

            if (string.IsNullOrEmpty(bundleName)) continue;

            // 如果有進行Loading標記後, 直接return;
            if (this.HasInBundleLoadingFlags(bundleName))
            {
                Debug.Log($"<color=#FFDC8A>ab: {bundleName} Loading...</color>");
                continue;
            }

            // Bundle Loading標記
            this._hashBundleLoadingFlags.Add(bundleName);

            // 如果有在快取中就不進行預加載
            if (this.HasInCache(bundleName))
            {
                // 在快取中請求進度大小需累加當前資源的總size (因為迴圈)
                this.reqSize += await this.GetAssetsLength(bundleName);
                // 處理進度回調
                progression?.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
                // 移除標記
                this._hashBundleLoadingFlags.Remove(bundleName);
                continue;
            }

            // 開始進行預加載
            var bundlePack = await this.LoadBundlePack(bundleName, progression);
            if (bundlePack != null)
            {
                if (!this.HasInCache(bundleName)) this._cacher.Add(bundleName, bundlePack);

                // 取得主要的Menifest中的AssetBundleManifest (因為記錄著所有資源的依賴性)
                var manifest = await this.GetManifest();
                if (manifest != null)
                {
                    string[] dependencies = manifest.GetAllDependencies(bundleName);
                    for (int j = 0; j < dependencies.Length; j++)
                    {
                        if (this.HasInCache(dependencies[j])) continue;

                        BundlePack dependBundlePack = await this.LoadBundlePack(dependencies[j], progression);
                        if (dependBundlePack != null)
                        {
                            // skipping duplicate keys
                            if (!this.HasInCache(dependencies[j])) this._cacher.Add(dependencies[j], dependBundlePack);
                        }
                    }
                }
            }

            // 移除標記
            this._hashBundleLoadingFlags.Remove(bundleName);

            await UniTask.Yield();

            Debug.Log($"【預加載】 => 當前<< CacheBundle >>快取數量 : " + this.Count);
        }
    }

    /// <summary>
    /// [使用計數管理] 載入Bundle => 會優先從快取中取得Bundle, 如果快取中沒有才進行Bundle加載
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="bundleName"></param>
    /// <param name="assetName"></param>
    /// <param name="dependency"></param>
    /// <param name="progression"></param>
    /// <returns></returns>
    public async UniTask<T> Load<T>(string bundleName, string assetName, bool dependency = true, Progression progression = null) where T : Object
    {
        bundleName = bundleName.ToLower();

        BundlePack bundlePack = null;
        BundlePack dependBundlePack;
        T asset;

#if UNITY_EDITOR
        if (BundleConfig.bAssetDatabaseMode)
        {
            asset = this.LoadEditorAsset<T>(bundleName, assetName);
            return asset;
        }
#endif

        // 如果有進行Loading標記後, 直接return;
        if (this.HasInBundleLoadingFlags(bundleName))
        {
            Debug.Log($"<color=#FFDC8A>ab: {bundleName} Loading...</color>");
            return null;
        }

        // Bundle Loading標記
        this._hashBundleLoadingFlags.Add(bundleName);

        // 先設置加載進度
        this.reqSize = 0;
        this.totalSize = await this.GetAssetsLength(bundleName);

        // 先從快取拿, 以下判斷沒有才執行加載
        bundlePack = this.GetFromCache(bundleName);

        // 加載Bundle包中的資源
        if (bundlePack == null)
        {
            bundlePack = await this.LoadBundlePack(bundleName, progression);
            asset = bundlePack?.GetAsset<T>(assetName);

            if (bundlePack != null && asset != null)
            {
                // skipping duplicate keys
                if (!this.HasInCache(bundleName)) this._cacher.Add(bundleName, bundlePack);
            }
        }
        else
        {
            // 直接更新進度
            this.reqSize = this.totalSize;
            // 處理進度回調
            progression?.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
            // 直接取得Bundle中的資源
            asset = bundlePack.GetAsset<T>(assetName);
        }

        if (asset != null)
        {
            // 主資源引用計數++
            bundlePack.AddRef();

            // 判斷是否加載依賴資源 (要先確定有找到Bundle包內的資源才進行)
            if (dependency)
            {
                // 取得主要的Menifest中的AssetBundleManifest (因為記錄著所有資源的依賴性)
                var manifest = await this.GetManifest();
                if (manifest != null)
                {
                    string[] dependencies = manifest.GetAllDependencies(bundleName);
                    for (int i = 0; i < dependencies.Length; i++)
                    {
                        var depBundelPack = this.GetFromCache(dependencies[i]);
                        if (depBundelPack != null)
                        {
                            // 依賴資源引用計數++
                            depBundelPack.AddRef();
                            continue;
                        }

                        dependBundlePack = await this.LoadBundlePack(dependencies[i], progression);
                        if (dependBundlePack != null)
                        {
                            // skipping duplicate keys
                            if (!this.HasInCache(dependencies[i])) this._cacher.Add(dependencies[i], dependBundlePack);
                        }
                    }
                }
            }
        }

        Debug.Log("【載入】 => 當前<< CacheBundle >>快取數量 : " + this.Count);

        this._hashBundleLoadingFlags.Remove(bundleName);

        return asset;
    }

    /// <summary>
    /// [使用計數管理] 從快取【釋放】單個Bundle (釋放Bundle記憶體, 連動銷毀實例對象也會 Missing 場景上有引用的對象)
    /// </summary>
    /// <param name="bundleName"></param>
    public override void ReleaseFromCache(string bundleName)
    {
#if UNITY_EDITOR
        if (BundleConfig.bAssetDatabaseMode) return;
#endif

        bundleName = bundleName.ToLower();

        if (this.HasInBundleLoadingFlags(bundleName))
        {
            Debug.Log($"<color=#FFDC8A>ab: {bundleName} Loading...</color>");
            return;
        }

        // 主資源
        if (this.HasInCache(bundleName))
        {
            // 主資源引用計數--
            this._cacher[bundleName].DelRef();
            if (this._cacher[bundleName].refCount <= 0)
            {
                this._cacher[bundleName].assetBundle.Unload(true);
                this._cacher[bundleName] = null;
                this._cacher.Remove(bundleName);
            }
        }

        // 依賴資源
        var manifest = this._manifest;
        string[] dependencies = manifest.GetAllDependencies(bundleName);
        for (int i = 0; i < dependencies.Length; i++)
        {
            if (this.HasInCache(dependencies[i]))
            {
                // 依賴資源引用計數--
                this._cacher[dependencies[i]].DelRef();
                if (this._cacher[dependencies[i]].refCount <= 0)
                {
                    this._cacher[dependencies[i]].assetBundle.Unload(true);
                    this._cacher[dependencies[i]] = null;
                    this._cacher.Remove(dependencies[i]);
                }
            }
        }

        Debug.Log("【單個釋放】 => 當前<< CacheBundle >>快取數量 : " + this.Count);
    }

    /// <summary>
    /// [強制釋放] 從快取中【釋放】全部Bundle (釋放Bundle記憶體, 連動銷毀實例對象也會 Missing 場景上有引用的對象)
    /// </summary>
    public override void ReleaseCache()
    {
#if UNITY_EDITOR
        if (BundleConfig.bAssetDatabaseMode) return;
#endif

        if (this.Count == 0) return;

        // 強制釋放全部快取與資源
        foreach (var bundleName in this._cacher.Keys.ToArray())
        {
            if (this.HasInBundleLoadingFlags(bundleName))
            {
                Debug.Log($"<color=#FFDC8A>ab: {bundleName} Loading...</color>");
                continue;
            }

            // 主資源
            if (this.HasInCache(bundleName))
            {
                this._cacher[bundleName].assetBundle.Unload(true);
                this._cacher[bundleName] = null;
                this._cacher.Remove(bundleName);
            }

            // 依賴資源
            var manifest = this._manifest;
            string[] dependencies = manifest.GetAllDependencies(bundleName);
            for (int i = 0; i < dependencies.Length; i++)
            {
                if (this.HasInCache(dependencies[i]))
                {
                    this._cacher[dependencies[i]].assetBundle.Unload(true);
                    this._cacher[dependencies[i]] = null;
                    this._cacher.Remove(dependencies[i]);
                }
            }
        }

        this._cacher.Clear();
        AssetBundle.UnloadAllAssetBundles(true);

        Debug.Log("【全部釋放】 => 當前<< CacheBundle >>快取數量 : " + this.Count);
    }

    public async UniTask<BundlePack> LoadBundlePack(string fileName, Progression progression)
    {
        fileName = fileName.ToLower();

        BundlePack bundlePack = new BundlePack();
        bundlePack.bundleName = fileName;

#if UNITY_STANDALONE_WIN
        if (BundleConfig.bBundleStreamMode)
        {
            // 解密方式
            string cryptogramType = BundleConfig.cryptogramArgs[0].ToUpper();

            Stream fs;
            switch (cryptogramType)
            {
                case BundleConfig.CryptogramType.NONE:
                    fs = new FileStream(this.GetFilePathFromStreamingAssetsOrSavePath(fileName), FileMode.Open, FileAccess.Read, FileShare.None);
                    //bundlePack.assetBundle = await AssetBundle.LoadFromStreamAsync(fs);
                    {
                        var req = AssetBundle.LoadFromStreamAsync(fs);

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
                                bundlePack.assetBundle = req.assetBundle;
                                break;
                            }

                            await UniTask.Yield();
                        }
                    }
                    break;
                case BundleConfig.CryptogramType.OFFSET:
                    fs = FileCryptogram.Offset.OffsetDecryptStream
                    (
                        this.GetFilePathFromStreamingAssetsOrSavePath(fileName),
                        System.Convert.ToInt32((BundleConfig.cryptogramArgs.Length >= 2) ? BundleConfig.cryptogramArgs[1] : "0")
                    );
                    //bundlePack.assetBundle = await AssetBundle.LoadFromStreamAsync(fs);
                    {
                        var req = AssetBundle.LoadFromStreamAsync(fs);

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
                                bundlePack.assetBundle = req.assetBundle;
                                break;
                            }

                            await UniTask.Yield();
                        }
                    }
                    break;
                case BundleConfig.CryptogramType.XOR:
                    fs = FileCryptogram.XOR.XorDecryptStream
                    (
                        this.GetFilePathFromStreamingAssetsOrSavePath(fileName),
                        System.Convert.ToByte((BundleConfig.cryptogramArgs.Length >= 2) ? BundleConfig.cryptogramArgs[1] : "0")
                    );
                    //bundlePack.assetBundle = await AssetBundle.LoadFromStreamAsync(fs);
                    {
                        var req = AssetBundle.LoadFromStreamAsync(fs);

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
                                bundlePack.assetBundle = req.assetBundle;
                                break;
                            }

                            await UniTask.Yield();
                        }
                    }
                    break;
                case BundleConfig.CryptogramType.AES:
                    fs = FileCryptogram.AES.AesDecryptStream
                    (
                        this.GetFilePathFromStreamingAssetsOrSavePath(fileName),
                        (BundleConfig.cryptogramArgs.Length >= 3) ? BundleConfig.cryptogramArgs[1] : string.Empty,
                        (BundleConfig.cryptogramArgs.Length >= 3) ? BundleConfig.cryptogramArgs[2] : string.Empty
                    );
                    //bundlePack.assetBundle = await AssetBundle.LoadFromStreamAsync(fs);
                    {
                        var req = AssetBundle.LoadFromStreamAsync(fs);

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
                                bundlePack.assetBundle = req.assetBundle;
                                break;
                            }

                            await UniTask.Yield();
                        }
                    }
                    break;
            }
        }
        else
        {
            // 解密方式
            string cryptogramType = BundleConfig.cryptogramArgs[0].ToUpper();

            byte[] bytes = File.ReadAllBytes(this.GetFilePathFromStreamingAssetsOrSavePath(fileName));
            switch (cryptogramType)
            {
                case BundleConfig.CryptogramType.NONE:
                    //bundlePack.assetBundle = await AssetBundle.LoadFromMemoryAsync(bytes);
                    //bytes = null;
                    {
                        var req = AssetBundle.LoadFromMemoryAsync(bytes);

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
                                bundlePack.assetBundle = req.assetBundle;
                                bytes = null;
                                break;
                            }

                            await UniTask.Yield();
                        }
                    }
                    break;
                case BundleConfig.CryptogramType.OFFSET:
                    FileCryptogram.Offset.OffsetDecryptFile
                    (
                        ref bytes,
                        System.Convert.ToInt32((BundleConfig.cryptogramArgs.Length >= 2) ? BundleConfig.cryptogramArgs[1] : "0")
                    );
                    //bundlePack.assetBundle = await AssetBundle.LoadFromMemoryAsync(bytes);
                    //bytes = null;
                    {
                        var req = AssetBundle.LoadFromMemoryAsync(bytes);

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
                                bundlePack.assetBundle = req.assetBundle;
                                bytes = null;
                                break;
                            }

                            await UniTask.Yield();
                        }
                    }
                    break;
                case BundleConfig.CryptogramType.XOR:
                    FileCryptogram.XOR.XorDecryptFile
                    (
                        bytes,
                        System.Convert.ToByte((BundleConfig.cryptogramArgs.Length >= 2) ? BundleConfig.cryptogramArgs[1] : "0")
                    );
                    //bundlePack.assetBundle = await AssetBundle.LoadFromMemoryAsync(bytes);
                    //bytes = null;
                    {
                        var req = AssetBundle.LoadFromMemoryAsync(bytes);

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
                                bundlePack.assetBundle = req.assetBundle;
                                bytes = null;
                                break;
                            }

                            await UniTask.Yield();
                        }
                    }
                    break;
                case BundleConfig.CryptogramType.AES:
                    FileCryptogram.AES.AesDecryptFile
                    (
                        bytes,
                        (BundleConfig.cryptogramArgs.Length >= 3) ? BundleConfig.cryptogramArgs[1] : string.Empty,
                        (BundleConfig.cryptogramArgs.Length >= 3) ? BundleConfig.cryptogramArgs[2] : string.Empty
                    );
                    //bundlePack.assetBundle = await AssetBundle.LoadFromMemoryAsync(bytes);
                    //bytes = null;
                    {
                        var req = AssetBundle.LoadFromMemoryAsync(bytes);

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
                                bundlePack.assetBundle = req.assetBundle;
                                bytes = null;
                                break;
                            }

                            await UniTask.Yield();
                        }
                    }
                    break;
            }
        }
#endif

#if UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL
        // 使用[文件流]加載方式, 只能存在於Persistent的路徑 (因為StreamingAssets只使用UnityWebRequest方式請求)
        if (BundleConfig.bBundleStreamMode && !this.HasInStreamingAssets(fileName))
        {
            // 解密方式
            string cryptogramType = BundleConfig.cryptogramArgs[0].ToUpper();

            Stream fs;
            switch (cryptogramType)
            {
                case BundleConfig.CryptogramType.NONE:
                    fs = new FileStream(this.GetFilePathFromSavePath(fileName), FileMode.Open, FileAccess.Read, FileShare.None, 1024 * 4, false);
                    //bundlePack.assetBundle = await AssetBundle.LoadFromStreamAsync(fs);
                    {
                        var req = AssetBundle.LoadFromStreamAsync(fs);

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
                                bundlePack.assetBundle = req.assetBundle;
                                break;
                            }

                            await UniTask.Yield();
                        }
                    }
                    break;
                case BundleConfig.CryptogramType.OFFSET:
                    fs = FileCryptogram.Offset.OffsetDecryptStream
                    (
                        this.GetFilePathFromSavePath(fileName),
                        System.Convert.ToInt32((BundleConfig.cryptogramArgs.Length >= 2) ? BundleConfig.cryptogramArgs[1] : "0")
                    );
                    //bundlePack.assetBundle = await AssetBundle.LoadFromStreamAsync(fs);
                    {
                        var req = AssetBundle.LoadFromStreamAsync(fs);

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
                                bundlePack.assetBundle = req.assetBundle;
                                break;
                            }

                            await UniTask.Yield();
                        }
                    }
                    break;
                case BundleConfig.CryptogramType.XOR:
                    fs = FileCryptogram.XOR.XorDecryptStream
                    (
                        this.GetFilePathFromSavePath(fileName),
                        System.Convert.ToByte((BundleConfig.cryptogramArgs.Length >= 2) ? BundleConfig.cryptogramArgs[1] : "0")
                    );
                    //bundlePack.assetBundle = await AssetBundle.LoadFromStreamAsync(fs);
                    {
                        var req = AssetBundle.LoadFromStreamAsync(fs);

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
                                bundlePack.assetBundle = req.assetBundle;
                                break;
                            }

                            await UniTask.Yield();
                        }
                    }
                    break;
                case BundleConfig.CryptogramType.AES:
                    fs = FileCryptogram.AES.AesDecryptStream
                    (
                        this.GetFilePathFromSavePath(fileName),
                        (BundleConfig.cryptogramArgs.Length >= 3) ? BundleConfig.cryptogramArgs[1] : string.Empty,
                        (BundleConfig.cryptogramArgs.Length >= 3) ? BundleConfig.cryptogramArgs[2] : string.Empty
                    );
                    //bundlePack.assetBundle = await AssetBundle.LoadFromStreamAsync(fs);
                    {
                        var req = AssetBundle.LoadFromStreamAsync(fs);

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
                                bundlePack.assetBundle = req.assetBundle;
                                break;
                            }

                            await UniTask.Yield();
                        }
                    }
                    break;
            }
        }
        // 使用[內存]加載方式, 會判斷資源如果不在StreamingAssets中就從Persistent中加載 (反之)
        else
        {
            if (!this.HasInStreamingAssets(fileName))
            {
                // 解密方式
                string cryptogramType = BundleConfig.cryptogramArgs[0].ToUpper();

                byte[] bytes = File.ReadAllBytes(this.GetFilePathFromSavePath(fileName));
                switch (cryptogramType)
                {
                    case BundleConfig.CryptogramType.NONE:
                        //bundlePack.assetBundle = await AssetBundle.LoadFromMemoryAsync(bytes);
                        //bytes = null;
                        {
                            var req = AssetBundle.LoadFromMemoryAsync(bytes);

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
                                    bundlePack.assetBundle = req.assetBundle;
                                    bytes = null;
                                    break;
                                }

                                await UniTask.Yield();
                            }
                        }
                        break;
                    case BundleConfig.CryptogramType.OFFSET:
                        FileCryptogram.Offset.OffsetDecryptFile
                        (
                            ref bytes,
                            System.Convert.ToInt32((BundleConfig.cryptogramArgs.Length >= 2) ? BundleConfig.cryptogramArgs[1] : "0")
                        );
                        //bundlePack.assetBundle = await AssetBundle.LoadFromMemoryAsync(bytes);
                        //bytes = null;
                        {
                            var req = AssetBundle.LoadFromMemoryAsync(bytes);

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
                                    bundlePack.assetBundle = req.assetBundle;
                                    bytes = null;
                                    break;
                                }

                                await UniTask.Yield();
                            }
                        }
                        break;
                    case BundleConfig.CryptogramType.XOR:
                        FileCryptogram.XOR.XorDecryptFile
                        (
                            bytes,
                            System.Convert.ToByte((BundleConfig.cryptogramArgs.Length >= 2) ? BundleConfig.cryptogramArgs[1] : "0")
                        );
                        //bundlePack.assetBundle = await AssetBundle.LoadFromMemoryAsync(bytes);
                        //bytes = null;
                        {
                            var req = AssetBundle.LoadFromMemoryAsync(bytes);

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
                                    bundlePack.assetBundle = req.assetBundle;
                                    bytes = null;
                                    break;
                                }

                                await UniTask.Yield();
                            }
                        }
                        break;
                    case BundleConfig.CryptogramType.AES:
                        FileCryptogram.AES.AesDecryptFile
                        (
                            bytes,
                            (BundleConfig.cryptogramArgs.Length >= 3) ? BundleConfig.cryptogramArgs[1] : string.Empty,
                            (BundleConfig.cryptogramArgs.Length >= 3) ? BundleConfig.cryptogramArgs[2] : string.Empty
                        );
                        //bundlePack.assetBundle = await AssetBundle.LoadFromMemoryAsync(bytes);
                        //bytes = null;
                        {
                            var req = AssetBundle.LoadFromMemoryAsync(bytes);

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
                                    bundlePack.assetBundle = req.assetBundle;
                                    bytes = null;
                                    break;
                                }

                                await UniTask.Yield();
                            }
                        }
                        break;
                }
            }
            else
            {
                // 解密方式
                string cryptogramType = BundleConfig.cryptogramArgs[0].ToUpper();

                byte[] bytes = await this.FileRequest(this.GetFilePathFromStreamingAssets(fileName));
                switch (cryptogramType)
                {
                    case BundleConfig.CryptogramType.NONE:
                        //bundlePack.assetBundle = await AssetBundle.LoadFromMemoryAsync(bytes);
                        //bytes = null;
                        {
                            var req = AssetBundle.LoadFromMemoryAsync(bytes);

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
                                    bundlePack.assetBundle = req.assetBundle;
                                    bytes = null;
                                    break;
                                }

                                await UniTask.Yield();
                            }
                        }
                        break;
                    case BundleConfig.CryptogramType.OFFSET:
                        FileCryptogram.Offset.OffsetDecryptFile
                        (
                            ref bytes,
                            System.Convert.ToInt32((BundleConfig.cryptogramArgs.Length >= 2) ? BundleConfig.cryptogramArgs[1] : "0")
                        );
                        //bundlePack.assetBundle = await AssetBundle.LoadFromMemoryAsync(bytes);
                        //bytes = null;
                        {
                            var req = AssetBundle.LoadFromMemoryAsync(bytes);

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
                                    bundlePack.assetBundle = req.assetBundle;
                                    bytes = null;
                                    break;
                                }

                                await UniTask.Yield();
                            }
                        }
                        break;
                    case BundleConfig.CryptogramType.XOR:
                        FileCryptogram.XOR.XorDecryptFile
                        (
                            bytes,
                            System.Convert.ToByte((BundleConfig.cryptogramArgs.Length >= 2) ? BundleConfig.cryptogramArgs[1] : "0")
                        );
                        //bundlePack.assetBundle = await AssetBundle.LoadFromMemoryAsync(bytes);
                        //bytes = null;
                        {
                            var req = AssetBundle.LoadFromMemoryAsync(bytes);

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
                                    bundlePack.assetBundle = req.assetBundle;
                                    bytes = null;
                                    break;
                                }

                                await UniTask.Yield();
                            }
                        }
                        break;
                    case BundleConfig.CryptogramType.AES:
                        FileCryptogram.AES.AesDecryptFile
                        (
                            bytes,
                            (BundleConfig.cryptogramArgs.Length >= 3) ? BundleConfig.cryptogramArgs[1] : string.Empty,
                            (BundleConfig.cryptogramArgs.Length >= 3) ? BundleConfig.cryptogramArgs[2] : string.Empty
                        );
                        //bundlePack.assetBundle = await AssetBundle.LoadFromMemoryAsync(bytes);
                        //bytes = null;
                        {
                            var req = AssetBundle.LoadFromMemoryAsync(bytes);

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
                                    bundlePack.assetBundle = req.assetBundle;
                                    bytes = null;
                                    break;
                                }

                                await UniTask.Yield();
                            }
                        }
                        break;
                }
            }
        }
#endif

        if (!string.IsNullOrEmpty(bundlePack.bundleName) && bundlePack.assetBundle != null)
        {
            Debug.Log($@"<color=#B7FCFF>Load AssetBundle. bundleName: {bundlePack.bundleName}</color>");
            return bundlePack;
        }

        bundlePack = null;
        return bundlePack;
    }

    /// <summary>
    /// 檔案請求 (Android, iOS, H5)
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public async UniTask<byte[]> FileRequest(string url)
    {
        try
        {
            var request = UnityWebRequest.Get(url);
            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log($"<color=#FF0000>Request failed, URL: {url}</color>");
                request.Dispose();

                return new byte[] { };
            }

            byte[] bytes = request.downloadHandler.data;
            request.Dispose();

            return bytes;
        }
        catch
        {
            Debug.Log($"<color=#FF0000>Request failed, URL: {url}</color>");
            return new byte[] { };
        }
    }

    public override async UniTask<int> GetAssetsLength(params string[] bundleNames)
    {
#if UNITY_EDITOR
        if (BundleConfig.bAssetDatabaseMode) return 0;
#endif

        int length = bundleNames.Length;

        var manifest = await this.GetManifest();
        foreach (var bundleName in bundleNames)
        {
            string[] dependencies = manifest.GetAllDependencies(bundleName);
            length += dependencies.Length;
        }

        return length;
    }

    /// <summary>
    /// 載入資源包的 Manifest (用於引用依賴資源)
    /// </summary>
    /// <returns></returns>
    public async UniTask<AssetBundleManifest> LoadManifest()
    {
        string manifestName = BundleConfig.GetManifestFileName();
        BundlePack bundlePack = await this.LoadBundlePack(manifestName, null);
        AssetBundleManifest manifest = bundlePack.assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

        return manifest;
    }

    private AssetBundleManifest _manifest = null;
    /// <summary>
    /// AssetBundleManifest (記錄著所有資源的依賴性)
    /// </summary>
    /// <returns></returns>
    public async UniTask<AssetBundleManifest> GetManifest()
    {
        if (this._manifest == null)
        {
            this._manifest = await this.LoadManifest();
        }

        return this._manifest;
    }

    /// <summary>
    /// 自動判斷返回 PersistentData or StreamingAssets 中的資源路徑
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public string GetFilePathFromStreamingAssetsOrSavePath(string fileName)
    {
        fileName = fileName.ToLower();

        if (!this.HasInStreamingAssets(fileName)) return this.GetFilePathFromSavePath(fileName);
        else return this.GetFilePathFromStreamingAssets(fileName);
    }

    /// <summary>
    /// 取得位於 StreamingAssets 中的資源路徑
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public string GetFilePathFromStreamingAssets(string fileName)
    {
        fileName = fileName.ToLower();
        // 透過配置檔取得完整檔案目錄名稱 (StreamingAssets中的配置檔)
        string fullPathName = Path.Combine(Application.streamingAssetsPath, fileName);
        return fullPathName;
    }

    /// <summary>
    /// 取得位於 PersistentData 中的資源路徑
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public string GetFilePathFromSavePath(string fileName)
    {
        fileName = fileName.ToLower();
        // 透過配置檔取得完整檔案目錄名稱 (Local中的記錄配置黨)
        string fullPathName = Path.Combine(BundleConfig.GetLocalDlFileSaveDirectory(), fileName);
        return fullPathName;
    }

    /// <summary>
    /// 透過配置檔返回是否有資源位於 StreamingAssets
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public bool HasInStreamingAssets(string fileName)
    {
        fileName = fileName.ToLower();

        if (BundleDistributor.GetInstance().GetRecordCfg() != null)
        {
            if (BundleDistributor.GetInstance().GetRecordCfg().HasFile(fileName)) return false;
        }

        return true;
    }

#if UNITY_EDITOR
    /// <summary>
    /// 直接讀取 AssetDatabase 拿取資源, 為了加快開發效率, 無需每次經過打包 (Editor Only)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="bundleName"></param>
    /// <param name="assetName"></param>
    /// <returns></returns>
    /// <exception cref="System.Exception"></exception>
    public T LoadEditorAsset<T>(string bundleName, string assetName) where T : Object
    {
        bundleName = bundleName.ToLower();

        // 取得資源位於AssetDatabase中的路徑
        var assetPaths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(bundleName, assetName);
        if (assetPaths.Length <= 0)
        {
            throw new System.Exception($@"Cannot found a asset path from AssetDatabase => bundleName: {bundleName}, assetName: {assetName}");
        }
        string assetPath = assetPaths[0];

        // 從路徑中取得資源
        T resObj = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);

        Debug.Log($@"<color=#A5FFA5>Load Asset From Editor AssetDatabase. bundleName: {bundleName}, assetName: {assetName}</color>");

        return resObj;
    }
#endif
}