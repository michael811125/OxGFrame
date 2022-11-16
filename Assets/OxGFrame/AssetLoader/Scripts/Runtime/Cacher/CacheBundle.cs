using AssetLoader.Utility;
using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader.Bundle;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace OxGFrame.AssetLoader.Cacher
{
    public class CacheBundle : AssetCache<BundlePack>, IBundle
    {
        /// <summary>
        /// Manifest 區分內部與外部 (Built-in, Patch)
        /// </summary>
        public enum Manifest
        {
            BUILTIN = 0,
            PATCH = 1
        }

        protected BundlePack[] _manifestBundlePack;
        protected AssetBundleManifest[] _manifest;

        protected string[] cryptogramArgs;
        protected string cryptogramType;

        private static CacheBundle _instance = null;
        public static CacheBundle GetInstance()
        {
            if (_instance == null) _instance = new CacheBundle();
            return _instance;
        }

        public CacheBundle()
        {
            // 解密參數
            this.cryptogramArgs = BundleConfig.cryptogramArgs;
            // 解密方式
            this.cryptogramType = this.cryptogramArgs[0].ToUpper();

            int manifestLength = Enum.GetNames(typeof(Manifest)).Length;
            this._manifestBundlePack = new BundlePack[manifestLength];
            this._manifest = new AssetBundleManifest[manifestLength];
        }

        public override bool HasInCache(string bundleName)
        {
            bundleName = bundleName.ToLower();

            return this._cacher.ContainsKey(bundleName);
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
        /// 預加載 Bundle 至快取中
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        public override async UniTask Preload(string bundleName, Progression progression = null)
        {
#if UNITY_EDITOR
            if (BundleConfig.assetDatabaseMode) return;
#endif

            if (string.IsNullOrEmpty(bundleName)) return;

            bundleName = bundleName.ToLower();

            // 如果有進行 Loading 標記後, 直接 return
            if (this.HasInLoadingFlags(bundleName))
            {
                Debug.Log($"<color=#FFDC8A>ab: {bundleName} Loading...</color>");
                return;
            }

            // 先設置加載進度
            this.reqSize = 0;                                  // 會由 LoadBundlePack 累加, 所以需歸0
            this.totalSize = this.GetAssetsLength(bundleName); // 返回當前要預加載的總大小

            // Loading 標記
            this._hashLoadingFlags.Add(bundleName);

            // 取得主要的 Menifest 中的 AssetBundleManifest (因為記錄著所有資源的依賴性)
            var manifest = await this.GetManifestAsync(bundleName);

            // 預加載依賴資源
            if (manifest != null)
            {
                string[] dependencies = manifest.GetAllDependencies(bundleName);
                for (int i = 0; i < dependencies.Length; i++)
                {
                    string dependName = dependencies[i].ToLower();

                    if (this.HasInCache(dependName)) continue;

                    BundlePack dependBundlePack = await this.LoadBundlePack(dependName, null);
                    if (dependBundlePack != null)
                    {
                        // skipping duplicate keys
                        if (!this.HasInCache(dependName)) this._cacher.Add(dependName, dependBundlePack);

                        Debug.Log($"<color=#ff9600>【Preload Dependency】 => Current << CacheBundle >> Cache Count: {this.Count}, ab: {dependName}</color>");
                    }
                }
            }

            // 如果有在快取中就不進行預加載
            if (this.HasInCache(bundleName))
            {
                // 在快取中請求大小就直接指定為資源總大小 (單個)
                this.reqSize = this.totalSize;
                // 處理進度回調
                progression?.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
                // 移除標記
                this._hashLoadingFlags.Remove(bundleName);
                return;
            }

            var bundlePack = await this.LoadBundlePack(bundleName, progression);
            if (bundlePack != null)
            {
                if (!this.HasInCache(bundleName)) this._cacher.Add(bundleName, bundlePack);
            }

            // 移除標記
            this._hashLoadingFlags.Remove(bundleName);

            Debug.Log($"<color=#ff9600>【Preload Main】 => Current << CacheBundle >> Cache Count: {this.Count}, ab: {bundleName}</color>");
        }

        public override async UniTask Preload(string[] bundleNames, Progression progression = null)
        {
#if UNITY_EDITOR
            if (BundleConfig.assetDatabaseMode) return;
#endif

            if (bundleNames == null || bundleNames.Length == 0) return;

            // 先設置加載進度
            this.reqSize = 0;                                   // 會由 LoadBundlePack 累加, 所以需歸0
            this.totalSize = this.GetAssetsLength(bundleNames); // 返回當前要預加載的總大小

            for (int i = 0; i < bundleNames.Length; i++)
            {
                if (string.IsNullOrEmpty(bundleNames[i])) continue;

                string bundleName = bundleNames[i].ToLower();

                // 如果有進行 Loading 標記後, 直接 return
                if (this.HasInLoadingFlags(bundleName))
                {
                    Debug.Log($"<color=#FFDC8A>ab: {bundleName} Loading...</color>");
                    continue;
                }

                // Loading 標記
                this._hashLoadingFlags.Add(bundleName);

                // 取得主要的 Menifest 中的 AssetBundleManifest (因為記錄著所有資源的依賴性)
                var manifest = await this.GetManifestAsync(bundleName);

                // 預加載依賴資源
                if (manifest != null)
                {
                    string[] dependencies = manifest.GetAllDependencies(bundleName);
                    for (int j = 0; j < dependencies.Length; j++)
                    {
                        string dependName = dependencies[j].ToLower();

                        if (this.HasInCache(dependName)) continue;

                        BundlePack dependBundlePack = await this.LoadBundlePack(dependName, null);
                        if (dependBundlePack != null)
                        {
                            // skipping duplicate keys
                            if (!this.HasInCache(dependName)) this._cacher.Add(dependName, dependBundlePack);

                            Debug.Log($"<color=#ff9600>【Preload Dependency】 => Current << CacheBundle >> Cache Count: {this.Count}, ab: {dependName}</color>");
                        }
                    }
                }

                // 如果有在快取中就不進行預加載
                if (this.HasInCache(bundleName))
                {
                    // 在快取中請求進度大小需累加當前資源的總 size (因為迴圈)
                    this.reqSize += this.GetAssetsLength(bundleName);
                    // 處理進度回調
                    progression?.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
                    // 移除標記
                    this._hashLoadingFlags.Remove(bundleName);
                    continue;
                }

                // 開始進行預加載
                var bundlePack = await this.LoadBundlePack(bundleName, progression);
                if (bundlePack != null)
                {
                    if (!this.HasInCache(bundleName)) this._cacher.Add(bundleName, bundlePack);
                }

                // 移除標記
                this._hashLoadingFlags.Remove(bundleName);

                Debug.Log($"<color=#ff9600>【Preload Main】 => Current << CacheBundle >> Cache Count: {this.Count}, ab: {bundleName}</color>");
            }
        }

        /// <summary>
        /// [使用計數管理] 載入 Bundle => 會優先從快取中取得 Bundle, 如果快取中沒有才進行 Bundle 加載
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <param name="dependency"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        public async UniTask<T> Load<T>(string bundleName, string assetName, bool dependency = true, Progression progression = null) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(bundleName)) return null;

            bundleName = bundleName.ToLower();

            BundlePack bundlePack = null;
            BundlePack dependBundlePack = null;
            T asset;

#if UNITY_EDITOR
            if (BundleConfig.assetDatabaseMode)
            {
                asset = this.LoadEditorAsset<T>(bundleName, assetName);
                return asset;
            }
#endif

            // 如果有進行 Loading 標記後, 直接 return
            if (this.HasInLoadingFlags(bundleName))
            {
                Debug.Log($"<color=#FFDC8A>ab: {bundleName} Loading...</color>");
                return null;
            }

            // 先設置加載進度
            this.reqSize = 0;
            this.totalSize = this.GetAssetsLength(bundleName);

            // Loading 標記
            this._hashLoadingFlags.Add(bundleName);

            // 判斷是否加載依賴資源 (要先確定有找到 Bundle 包內的資源才進行)
            if (dependency)
            {
                var manifest = await this.GetManifestAsync(bundleName);

                // 取得主要的 Menifest 中的 AssetBundleManifest (因為記錄著所有資源的依賴性)
                if (manifest != null)
                {
                    string[] dependencies = manifest.GetAllDependencies(bundleName);
                    for (int i = 0; i < dependencies.Length; i++)
                    {
                        string dependName = dependencies[i].ToLower();

                        dependBundlePack = this.GetFromCache(dependName);
                        if (dependBundlePack == null)
                        {
                            dependBundlePack = await this.LoadBundlePack(dependName, null);
                            if (dependBundlePack != null)
                            {
                                // skipping duplicate keys
                                if (!this.HasInCache(dependName)) this._cacher.Add(dependName, dependBundlePack);
                            }
                        }

                        if (dependBundlePack != null)
                        {
                            // 依賴資源引用計數++
                            dependBundlePack.AddRef();

                            Debug.Log($"<color=#90FF71>【Load Dependency】 => Current << CacheBundle >> Cache Count: {this.Count}, ab: {dependName}, ref: {dependBundlePack.refCount}</color>");
                        }
                    }
                }
            }

            // 先從快取拿, 以下判斷沒有才執行加載
            bundlePack = this.GetFromCache(bundleName);

            // 加載 Bundle 包中的資源
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
                // 直接取得 Bundle 中的資源
                asset = bundlePack.GetAsset<T>(assetName);
            }

            if (asset != null)
            {
                // 主資源引用計數++
                bundlePack.AddRef();
            }

            this._hashLoadingFlags.Remove(bundleName);

            Debug.Log($"<color=#90FF71>【Load Main】 => Current << CacheBundle >> Cache Count: {this.Count}, ab: {bundleName}, ref: {bundlePack.refCount}</color>");

            return asset;
        }

        /// <summary>
        /// [使用計數管理] 從快取【釋放】單個Bundle (釋放Bundle記憶體, 連動銷毀實例對象也會 Missing 場景上有引用的對象)
        /// </summary>
        /// <param name="bundleName"></param>
        public override void Unload(string bundleName)
        {
#if UNITY_EDITOR
            if (BundleConfig.assetDatabaseMode) return;
#endif

            if (string.IsNullOrEmpty(bundleName)) return;

            bundleName = bundleName.ToLower();

            if (this.HasInLoadingFlags(bundleName))
            {
                Debug.Log($"<color=#FFDC8A>ab: {bundleName} Loading...</color>");
                return;
            }

            // 主資源
            if (this.HasInCache(bundleName))
            {
                // 主資源引用計數--
                this._cacher[bundleName].DelRef();

                { Debug.Log($"<color=#00e5ff>【<color=#ffcf92>Unload Main</color>】 => Current << CacheBundle >> Cache Count: {this.Count}, ab: {bundleName}, ref: {this._cacher.TryGetValue(bundleName, out var v)} {v?.refCount}</color>"); }

                if (this._cacher[bundleName].refCount <= 0)
                {
                    this._cacher[bundleName].assetBundle.Unload(true);
                    this._cacher[bundleName] = null;
                    this._cacher.Remove(bundleName);

                    Debug.Log($"<color=#00e5ff>【<color=#ff92ef>Unload Main Completes</color>】 => Current << CacheBundle >> Cache Count: {this.Count}, ab: {bundleName}</color>");
                }

                // 依賴資源
                string[] dependencies = this.GetManifest(bundleName)?.GetAllDependencies(bundleName);
                if (dependencies != null)
                {
                    for (int i = 0; i < dependencies.Length; i++)
                    {
                        string dependName = dependencies[i].ToLower();

                        if (this.HasInCache(dependName))
                        {
                            // 依賴資源引用計數--
                            this._cacher[dependName].DelRef();

                            { Debug.Log($"<color=#00e5ff>【<color=#ffcf92>Unload Dependency</color>】 => Current << CacheBundle >> Cache Count: {this.Count}, ab: {dependencies[i]}, ref: {this._cacher.TryGetValue(dependencies[i], out var v)} {v?.refCount}</color>"); }

                            if (this._cacher[dependName].refCount <= 0)
                            {
                                this._cacher[dependName].assetBundle.Unload(true);
                                this._cacher[dependName] = null;
                                this._cacher.Remove(dependName);

                                Debug.Log($"<color=#00e5ff>【<color=#ff92ef>Unload Dependency Completes</color>】 => Current << CacheBundle >> Cache Count: {this.Count}, ab: {dependencies[i]}</color>");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// [強制釋放] 從快取中【釋放】全部 Bundle (釋放Bundle記憶體, 連動銷毀實例對象也會 Missing 場景上有引用的對象)
        /// </summary>
        public override void Release()
        {
#if UNITY_EDITOR
            if (BundleConfig.assetDatabaseMode) return;
#endif

            if (this.Count == 0) return;

            // 強制釋放全部快取與資源
            foreach (var bundleName in this._cacher.Keys.ToArray())
            {
                if (this.HasInLoadingFlags(bundleName))
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

                    // 依賴資源
                    string[] dependencies = this.GetManifest(bundleName)?.GetAllDependencies(bundleName);
                    if (dependencies != null)
                    {
                        for (int i = 0; i < dependencies.Length; i++)
                        {
                            string dependName = dependencies[i].ToLower();

                            if (this.HasInCache(dependName))
                            {
                                this._cacher[dependName].assetBundle.Unload(true);
                                this._cacher[dependName] = null;
                                this._cacher.Remove(dependName);
                            }
                        }
                    }
                }
            }

            // 最後執行
            this._cacher.Clear();
            // 卸載 Manifest
            this.UnloadManifest();
            // 卸載全部 Bundles
            AssetBundle.UnloadAllAssetBundles(true);

            Debug.Log($"<color=#ff71b7>【Release All】 => Current << CacheBundle >> Cache Count: {this.Count}</color>");
        }

        public async UniTask<BundlePack> LoadBundlePack(string bundleName, Progression progression, bool forceNoMd5 = false)
        {
            if (string.IsNullOrEmpty(bundleName))
            {
                Debug.Log("<color=#FF0000>Load BundlePack failed. BundleName cannot be null or empty.</color>");
                return null;
            }

            bundleName = bundleName.ToLower();

            // 會判斷是否進行 BundleName & Md5 替換
            if (!forceNoMd5) bundleName = BundleNameToMd5(bundleName, BundleConfig.readMd5BundleName);

            BundlePack bundlePack = new BundlePack();
            bundlePack.bundleName = bundleName;

#if UNITY_STANDALONE_WIN
            if (BundleConfig.bundleStreamMode)
            {
                Stream fs;
                switch (this.cryptogramType)
                {
                    case BundleConfig.CryptogramType.NONE:
                        fs = new FileStream(GetFilePathFromStreamingAssetsOrSavePath(bundleName), FileMode.Open, FileAccess.Read, FileShare.None);

                        {
                            var dataBytes = new byte[fs.Length];
                            fs.Read(dataBytes, 0, dataBytes.Length);
                            fs.Dispose();

                            var ms = new MemoryStream();
                            ms.Write(dataBytes, 0, dataBytes.Length);

                            bundlePack.assetBundle = await this._LoadFromStreamAsync(ms, progression);
                        }
                        break;
                    case BundleConfig.CryptogramType.OFFSET:
                        fs = FileCryptogram.Offset.OffsetDecryptStream
                        (
                            GetFilePathFromStreamingAssetsOrSavePath(bundleName),
                            System.Convert.ToInt32(this.cryptogramArgs[1])
                        );

                        {
                            bundlePack.assetBundle = await this._LoadFromStreamAsync(fs, progression);
                        }
                        break;
                    case BundleConfig.CryptogramType.XOR:
                        fs = FileCryptogram.XOR.XorDecryptStream
                        (
                            GetFilePathFromStreamingAssetsOrSavePath(bundleName),
                            System.Convert.ToByte(this.cryptogramArgs[1])
                        );

                        {
                            bundlePack.assetBundle = await this._LoadFromStreamAsync(fs, progression);
                        }
                        break;
                    case BundleConfig.CryptogramType.HTXOR:
                        fs = FileCryptogram.HTXOR.HTXorDecryptStream
                        (
                            GetFilePathFromStreamingAssetsOrSavePath(bundleName),
                            System.Convert.ToByte(this.cryptogramArgs[1]),
                            System.Convert.ToByte(this.cryptogramArgs[2])
                        );

                        {
                            bundlePack.assetBundle = await this._LoadFromStreamAsync(fs, progression);
                        }
                        break;
                    case BundleConfig.CryptogramType.AES:
                        fs = FileCryptogram.AES.AesDecryptStream
                        (
                            GetFilePathFromStreamingAssetsOrSavePath(bundleName),
                            this.cryptogramArgs[1],
                            this.cryptogramArgs[2]
                        );

                        {
                            bundlePack.assetBundle = await this._LoadFromStreamAsync(fs, progression);
                        }
                        break;
                }
            }
            else
            {
                byte[] bytes = File.ReadAllBytes(GetFilePathFromStreamingAssetsOrSavePath(bundleName));
                switch (this.cryptogramType)
                {
                    case BundleConfig.CryptogramType.NONE:
                        {
                            bundlePack.assetBundle = await this._LoadFromMemoryAsync(bytes, progression);
                        }
                        break;
                    case BundleConfig.CryptogramType.OFFSET:
                        FileCryptogram.Offset.OffsetDecryptFile
                        (
                            ref bytes,
                            System.Convert.ToInt32(this.cryptogramArgs[1])
                        );

                        {
                            bundlePack.assetBundle = await this._LoadFromMemoryAsync(bytes, progression);
                        }
                        break;
                    case BundleConfig.CryptogramType.XOR:
                        FileCryptogram.XOR.XorDecryptFile
                        (
                            bytes,
                            System.Convert.ToByte(this.cryptogramArgs[1])
                        );

                        {
                            bundlePack.assetBundle = await this._LoadFromMemoryAsync(bytes, progression);
                        }
                        break;
                    case BundleConfig.CryptogramType.HTXOR:
                        FileCryptogram.HTXOR.HTXorDecryptFile
                        (
                            bytes,
                            System.Convert.ToByte(this.cryptogramArgs[1]),
                            System.Convert.ToByte(this.cryptogramArgs[2])
                        );

                        {
                            bundlePack.assetBundle = await this._LoadFromMemoryAsync(bytes, progression);
                        }
                        break;
                    case BundleConfig.CryptogramType.AES:
                        FileCryptogram.AES.AesDecryptFile
                        (
                            bytes,
                            this.cryptogramArgs[1],
                            this.cryptogramArgs[2]
                        );

                        {
                            bundlePack.assetBundle = await this._LoadFromMemoryAsync(bytes, progression);
                        }
                        break;
                }
            }
#endif

#if UNITY_STANDALONE_OSX || UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL
            // 使用[文件流]加載方式, 只能存在於Persistent的路徑 (因為 StreamingAssets 只使用 UnityWebRequest 方式請求)
            if (BundleConfig.bundleStreamMode && !HasInStreamingAssets(bundleName))
            {
                Stream fs;
                switch (this.cryptogramType)
                {
                    case BundleConfig.CryptogramType.NONE:
                        fs = new FileStream(GetFilePathFromSavePath(bundleName), FileMode.Open, FileAccess.Read, FileShare.None);

                        {
                            var dataBytes = new byte[fs.Length];
                            fs.Read(dataBytes, 0, dataBytes.Length);
                            fs.Dispose();

                            var ms = new MemoryStream();
                            ms.Write(dataBytes, 0, dataBytes.Length);

                            bundlePack.assetBundle = await this._LoadFromStreamAsync(ms, progression);
                        }
                        break;
                    case BundleConfig.CryptogramType.OFFSET:
                        fs = FileCryptogram.Offset.OffsetDecryptStream
                        (
                            GetFilePathFromSavePath(bundleName),
                            System.Convert.ToInt32(this.cryptogramArgs[1])
                        );

                        {
                            bundlePack.assetBundle = await this._LoadFromStreamAsync(fs, progression);
                        }
                        break;
                    case BundleConfig.CryptogramType.XOR:
                        fs = FileCryptogram.XOR.XorDecryptStream
                        (
                            GetFilePathFromSavePath(bundleName),
                            System.Convert.ToByte(this.cryptogramArgs[1])
                        );

                        {
                            bundlePack.assetBundle = await this._LoadFromStreamAsync(fs, progression);
                        }
                        break;
                    case BundleConfig.CryptogramType.HTXOR:
                        fs = FileCryptogram.HTXOR.HTXorDecryptStream
                        (
                            GetFilePathFromSavePath(bundleName),
                            System.Convert.ToByte(this.cryptogramArgs[1]),
                            System.Convert.ToByte(this.cryptogramArgs[2])
                        );

                        {
                            bundlePack.assetBundle = await this._LoadFromStreamAsync(fs, progression);
                        }
                        break;
                    case BundleConfig.CryptogramType.AES:
                        fs = FileCryptogram.AES.AesDecryptStream
                        (
                            GetFilePathFromSavePath(bundleName),
                            this.cryptogramArgs[1],
                            this.cryptogramArgs[2]
                        );

                        {
                            bundlePack.assetBundle = await this._LoadFromStreamAsync(fs, progression);
                        }
                        break;
                }
            }
            // 使用[內存]加載方式, 會判斷資源如果不在 StreamingAssets 中就從 Persistent 中加載 (反之)
            else
            {
                if (!HasInStreamingAssets(bundleName))
                {
                    byte[] bytes = File.ReadAllBytes(GetFilePathFromSavePath(bundleName));
                    switch (this.cryptogramType)
                    {
                        case BundleConfig.CryptogramType.NONE:
                            {
                                bundlePack.assetBundle = await this._LoadFromMemoryAsync(bytes, progression);
                            }
                            break;
                        case BundleConfig.CryptogramType.OFFSET:
                            FileCryptogram.Offset.OffsetDecryptFile
                            (
                                ref bytes,
                                System.Convert.ToInt32(this.cryptogramArgs[1])
                            );

                            {
                                bundlePack.assetBundle = await this._LoadFromMemoryAsync(bytes, progression);
                            }
                            break;
                        case BundleConfig.CryptogramType.XOR:
                            FileCryptogram.XOR.XorDecryptFile
                            (
                                bytes,
                                System.Convert.ToByte(this.cryptogramArgs[1])
                            );

                            {
                                bundlePack.assetBundle = await this._LoadFromMemoryAsync(bytes, progression);
                            }
                            break;
                        case BundleConfig.CryptogramType.HTXOR:
                            FileCryptogram.HTXOR.HTXorDecryptFile
                            (
                                bytes,
                                System.Convert.ToByte(this.cryptogramArgs[1]),
                                System.Convert.ToByte(this.cryptogramArgs[2])
                            );

                            {
                                bundlePack.assetBundle = await this._LoadFromMemoryAsync(bytes, progression);
                            }
                            break;
                        case BundleConfig.CryptogramType.AES:
                            FileCryptogram.AES.AesDecryptFile
                            (
                                bytes,
                                this.cryptogramArgs[1],
                                this.cryptogramArgs[2]
                            );

                            {
                                bundlePack.assetBundle = await this._LoadFromMemoryAsync(bytes, progression);
                            }
                            break;
                    }
                }
                else
                {
                    byte[] bytes = await BundleUtility.FileRequestByte(GetFilePathFromStreamingAssets(bundleName));
                    switch (this.cryptogramType)
                    {
                        case BundleConfig.CryptogramType.NONE:
                            {
                                bundlePack.assetBundle = await this._LoadFromMemoryAsync(bytes, progression);
                            }
                            break;
                        case BundleConfig.CryptogramType.OFFSET:
                            FileCryptogram.Offset.OffsetDecryptFile
                            (
                                ref bytes,
                                System.Convert.ToInt32(this.cryptogramArgs[1])
                            );

                            {
                                bundlePack.assetBundle = await this._LoadFromMemoryAsync(bytes, progression);
                            }
                            break;
                        case BundleConfig.CryptogramType.XOR:
                            FileCryptogram.XOR.XorDecryptFile
                            (
                                bytes,
                                System.Convert.ToByte(this.cryptogramArgs[1])
                            );

                            {
                                bundlePack.assetBundle = await this._LoadFromMemoryAsync(bytes, progression);
                            }
                            break;
                        case BundleConfig.CryptogramType.HTXOR:
                            FileCryptogram.HTXOR.HTXorDecryptFile
                            (
                                bytes,
                                System.Convert.ToByte(this.cryptogramArgs[1]),
                                System.Convert.ToByte(this.cryptogramArgs[2])
                            );

                            {
                                bundlePack.assetBundle = await this._LoadFromMemoryAsync(bytes, progression);
                            }
                            break;
                        case BundleConfig.CryptogramType.AES:
                            FileCryptogram.AES.AesDecryptFile
                            (
                                bytes,
                                this.cryptogramArgs[1],
                                this.cryptogramArgs[2]
                            );

                            {
                                bundlePack.assetBundle = await this._LoadFromMemoryAsync(bytes, progression);
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

        private async UniTask<AssetBundle> _LoadFromStreamAsync(Stream stream, Progression progression)
        {
            AssetBundle ab = null;
            var req = AssetBundle.LoadFromStreamAsync(stream);

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
                    ab = req.assetBundle;
                    break;
                }

                await UniTask.Yield();
            }

            return ab;
        }

        private async UniTask<AssetBundle> _LoadFromMemoryAsync(byte[] bytes, Progression progression)
        {
            AssetBundle ab = null;
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
                    ab = req.assetBundle;
                    bytes = null;
                    break;
                }

                await UniTask.Yield();
            }

            return ab;
        }

        /// <summary>
        /// 載入 Manifest, 僅讀取 StreamingAssets 中的 Manifest (用於引用依賴資源)
        /// </summary>
        /// <returns></returns>
        public async UniTask<BundlePack> LoadManifest(string manifestName)
        {
            if (this.HasInLoadingFlags(manifestName))
            {
                Debug.Log($"<color=#FFDC8A>ab: {manifestName} Loading...</color>");
                return null;
            }

            // Loading 標記
            this._hashLoadingFlags.Add(manifestName);

            BundlePack bundlePack = await this.LoadBundlePack(manifestName, null, true);

            // 移除 Loading 標記
            this._hashLoadingFlags.Remove(manifestName);

            return bundlePack;
        }

        /// <summary>
        /// 取得對應的 Manifest, 包含加載過程 (for Load)
        /// </summary>
        /// <returns></returns>
        public async UniTask<AssetBundleManifest> GetManifestAsync(string bundleName)
        {
            if (string.IsNullOrEmpty(bundleName)) return null;

            bundleName = bundleName.ToLower();

            // 會判斷是否進行 BundleName & Md5 替換
            bundleName = BundleNameToMd5(bundleName, BundleConfig.readMd5BundleName);

            int index;
            bool inApp = HasInStreamingAssets(bundleName);

            // 取得 manifeset 名稱 (判斷選擇使用 Builtin or Patch 的 manifest)
            string manifestName = BundleConfig.GetManifestFileName(inApp);

            // 透過 Bundle 檢查, 屬於 Built-in 資源 or Patch 資源 (會依照判斷讀取不同的 manifest)
            switch (inApp)
            {
                case true:
                    index = (int)Manifest.BUILTIN;
                    break;

                case false:
                    index = (int)Manifest.PATCH;
                    break;
            }

            // 加載 manifest 文本數據
            if (this._manifest[index] == null)
            {
                // 加載 manifest bundle pack
                if (this._manifestBundlePack[index] == null)
                {
                    this.UnloadManifest();
                    this._manifestBundlePack[index] = await this.LoadManifest(manifestName);
                }

                // 加載 manifest 數據
                if (this._manifestBundlePack[index] != null)
                {
                    this._manifest[index] = this._manifestBundlePack[index].GetAsset<AssetBundleManifest>("AssetBundleManifest");
                }
            }

            if (this._manifest[index] != null)
            {
                Debug.Log($"<color=#f1d088>Load Manifest Index: {index}</color>");
                return this._manifest[index];
            }

            Debug.Log("<color=#FF0000>Load Manifest failed</color>");

            return null;
        }

        /// <summary>
        /// 取得對應的 Manifest (for Unload)
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        public AssetBundleManifest GetManifest(string bundleName)
        {
            if (string.IsNullOrEmpty(bundleName)) return null;

            bundleName = bundleName.ToLower();

            // 會判斷是否進行 BundleName & Md5 替換
            bundleName = BundleNameToMd5(bundleName, BundleConfig.readMd5BundleName);

            int index;
            bool inApp = HasInStreamingAssets(bundleName);

            // 透過 Bundle 檢查, 屬於 Built-in 資源 or Patch 資源 (會依照判斷讀取不同的 manifest)
            switch (inApp)
            {
                case true:
                    index = (int)Manifest.BUILTIN;
                    break;

                case false:
                    index = (int)Manifest.PATCH;
                    break;
            }

            if (this._manifest[index] != null)
            {
                Debug.Log($"<color=#f1d088>Get Manifest Index: {index}</color>");
                return this._manifest[index];
            }

            return null;
        }

        /// <summary>
        /// 卸載 Manifest (透過 Bundle 區別判斷)
        /// </summary>
        public void UnloadManifest(string bundleName)
        {
            if (string.IsNullOrEmpty(bundleName)) return;

            bundleName = bundleName.ToLower();

            // 會判斷是否進行 BundleName & Md5 替換
            bundleName = BundleNameToMd5(bundleName, BundleConfig.readMd5BundleName);

            int index;
            bool inApp = HasInStreamingAssets(bundleName);

            // 透過 Bundle 檢查, 屬於 Built-in 資源 or Patch 資源 (會依照判斷讀取不同的 manifest)
            switch (inApp)
            {
                case true:
                    index = (int)Manifest.BUILTIN;
                    break;

                case false:
                    index = (int)Manifest.PATCH;
                    break;
            }

            if (this._manifestBundlePack[index] != null) this._manifestBundlePack[index].assetBundle.Unload(true);
            this._manifestBundlePack[index] = null;
            this._manifest[index] = null;

            Debug.Log($"<color=#f1d088>Unload Manifest Index: {index}</color>");
        }

        /// <summary>
        /// 強制卸載 All Manifest
        /// </summary>
        public void UnloadManifest()
        {
            int manifestLength = Enum.GetNames(typeof(Manifest)).Length;
            for (int i = 0; i < manifestLength; i++)
            {
                if (this._manifestBundlePack[i] != null) this._manifestBundlePack[i].assetBundle.Unload(true);
                this._manifestBundlePack[i] = null;
                this._manifest[i] = null;
            }

            Debug.Log("<color=#f1d088>Unload All Manifests</color>");
        }

        /// <summary>
        /// 自動判斷返回 PersistentData or StreamingAssets 中的資源路徑
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetFilePathFromStreamingAssetsOrSavePath(string fileName)
        {
            fileName = fileName.ToLower();

            if (!HasInStreamingAssets(fileName)) return GetFilePathFromSavePath(fileName);
            else return GetFilePathFromStreamingAssets(fileName);
        }

        /// <summary>
        /// 取得位於 StreamingAssets 中的資源路徑
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetFilePathFromStreamingAssets(string fileName)
        {
            fileName = fileName.ToLower();
            // 透過配置檔取得完整檔案目錄名稱 (StreamingAssets 中的配置檔)
            string fullPathName = Path.Combine(Application.streamingAssetsPath, fileName);
            return fullPathName;
        }

        /// <summary>
        /// 取得位於 PersistentData 中的資源路徑
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetFilePathFromSavePath(string fileName)
        {
            fileName = fileName.ToLower();
            // 透過配置檔取得完整檔案目錄名稱 (Local 中的記錄配置黨)
            string fullPathName = Path.Combine(BundleConfig.GetLocalDlFileSaveDirectory(), fileName);
            return fullPathName;
        }

        /// <summary>
        /// 透過配置檔返回是否有資源位於 StreamingAssets
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool HasInStreamingAssets(string fileName)
        {
            fileName = fileName.ToLower();

            if (BundleDistributor.GetInstance().GetRecordCfg() != null)
            {
                if (BundleDistributor.GetInstance().GetRecordCfg().HasFile(fileName)) return false;
            }

            return true;
        }

        /// <summary>
        /// 透過 BundleName 加密成 MD5 進行替換
        /// </summary>
        /// <param name="manifest"></param>
        /// <param name="bundleName"></param>
        /// <param name="swap"></param>
        /// <returns></returns>
        public static string BundleNameToMd5(string bundleName, bool swap)
        {
            if (swap)
            {
                // 取得 Bundle 檔案名稱
                bundleName = bundleName.Replace("\\", "/");
                string[] pathArgs = bundleName.Split('/');
                string originFileName = pathArgs[pathArgs.Length - 1];
                // 將路徑中的 Bundle 名稱替換成 Md5
                bundleName = bundleName.Replace(originFileName, BundleUtility.MakeMd5ForString(originFileName));

                return bundleName;
            }

            return bundleName;
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
        public T LoadEditorAsset<T>(string bundleName, string assetName) where T : UnityEngine.Object
        {
            bundleName = bundleName.ToLower();

            // 取得資源位於 AssetDatabase 中的路徑
            var assetPaths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(bundleName, assetName);
            if (assetPaths.Length <= 0)
            {
                throw new System.Exception($@"Cannot found an asset path from Editor AssetDatabase => bundleName: {bundleName}, assetName: {assetName}");
            }
            string assetPath = assetPaths[0];

            // 從路徑中取得資源
            T resObj = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);

            Debug.Log($@"<color=#A5FFA5>Load asset from Editor AssetDatabase => bundleName: {bundleName}, assetName: {assetName}</color>");

            return resObj;
        }
#endif
    }
}