using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using YooAsset;

namespace OxGFrame.AssetLoader.Bundle
{
    internal static class PackageManager
    {
        internal static bool isInitialized = false;

        private static string _currentPackageName;
        private static ResourcePackage _currentPackage;
        private static IDecryptionServices _decryption;

        /// <summary>
        /// Init settings
        /// </summary>
        public async static UniTask InitSetup()
        {
            #region Init YooAssets
            YooAssets.Destroy();
            YooAssets.Initialize();
            YooAssets.SetOperationSystemMaxTimeSlice(30);
            #endregion

            #region Init Decryption Type
            // Init decryption type
            var cryptogramType = BundleConfig.cryptogramArgs[0].ToUpper();
            switch (cryptogramType)
            {
                case BundleConfig.CryptogramType.NONE:
                    break;
                case BundleConfig.CryptogramType.OFFSET:
                    _decryption = new OffsetDecryption();
                    break;
                case BundleConfig.CryptogramType.XOR:
                    _decryption = new XorDecryption();
                    break;
                case BundleConfig.CryptogramType.HT2XOR:
                    _decryption = new HT2XorDecryption();
                    break;
                case BundleConfig.CryptogramType.AES:
                    _decryption = new AesDecryption();
                    break;
            }
            Debug.Log($"<color=#ffe45a>Init Bundle Decryption: {cryptogramType}</color>");
            #endregion

            #region Init Preset App Packages
            isInitialized = await InitPresetAppPackages();
            #endregion

            #region Set Default Package
            // Set default package by first element
            SetDefaultPackage(0);
            #endregion
        }

        /// <summary>
        /// Init preset app packages from PatchLauncher
        /// </summary>
        /// <returns></returns>
        public static async UniTask<bool> InitPresetAppPackages()
        {
            if (BundleConfig.listPackages != null && BundleConfig.listPackages.Count > 0)
            {
                foreach (var packageName in BundleConfig.listPackages)
                {
                    // Register first
                    RegisterPackage(packageName);

                    // Init preset app package
                    string hostServer = null;
                    string fallbackHostServer = null;
                    IBuildinQueryServices builtinQueryService = null;
                    IDeliveryQueryServices deliveryQueryService = null;

                    // Host Mode or WebGL Mode
                    if (BundleConfig.playMode == BundleConfig.PlayMode.HostMode ||
                        BundleConfig.playMode == BundleConfig.PlayMode.WebGLMode)
                    {
                        hostServer = await BundleConfig.GetHostServerUrl(packageName);
                        fallbackHostServer = await BundleConfig.GetFallbackHostServerUrl(packageName);
                        builtinQueryService = new RequestBuiltinQuery();
                        deliveryQueryService = new RequestDeliveryQuery();
                    }
                    bool isInitialized = await InitPackage(packageName, false, hostServer, fallbackHostServer, builtinQueryService, deliveryQueryService);
                    if (!isInitialized) return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Init package by package name
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="autoUpdate"></param>
        /// <param name="hostServer"></param>
        /// <param name="fallbackHostServer"></param>
        /// <param name="builtinQueryService"></param>
        /// <param name="deliveryQueryService"></param>
        /// <returns></returns>
        public static async UniTask<bool> InitPackage(string packageName, bool autoUpdate, string hostServer, string fallbackHostServer, IBuildinQueryServices builtinQueryService, IDeliveryQueryServices deliveryQueryService)
        {
            var package = RegisterPackage(packageName);
            if (package.InitializeStatus == EOperationStatus.Succeed)
            {
                if (autoUpdate) await UpdatePackage(packageName);
                Debug.Log($"<color=#e2ec00>Package: {packageName} is initialized. Status: {package.InitializeStatus}.</color>");
                return true;
            }

            // Simulate Mode
            InitializationOperation initializationOperation = null;
            if (BundleConfig.playMode == BundleConfig.PlayMode.EditorSimulateMode)
            {
                var createParameters = new EditorSimulateModeParameters();
                createParameters.SimulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(packageName);
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // Offline Mode
            if (BundleConfig.playMode == BundleConfig.PlayMode.OfflineMode)
            {
                var createParameters = new OfflinePlayModeParameters();
                createParameters.DecryptionServices = _decryption;
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // Host Mode
            if (BundleConfig.playMode == BundleConfig.PlayMode.HostMode)
            {
                var createParameters = new HostPlayModeParameters();
                createParameters.DecryptionServices = _decryption;
                createParameters.BuildinQueryServices = builtinQueryService;
                createParameters.DeliveryQueryServices = deliveryQueryService;
                createParameters.RemoteServices = new HostServers(hostServer, fallbackHostServer);
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // WebGL Mode
            if (BundleConfig.playMode == BundleConfig.PlayMode.WebGLMode)
            {
                var createParameters = new WebPlayModeParameters();
                createParameters.DecryptionServices = _decryption;
                createParameters.BuildinQueryServices = builtinQueryService;
                createParameters.RemoteServices = new HostServers(hostServer, fallbackHostServer);
                initializationOperation = package.InitializeAsync(createParameters);
            }

            await initializationOperation;

            if (initializationOperation.Status == EOperationStatus.Succeed)
            {
                if (autoUpdate) await UpdatePackage(packageName);
                Debug.Log($"<color=#85cf0f>Package: {packageName} <color=#00c1ff>Init</color> completed successfully.</color>");
                return true;
            }
            else
            {
                Debug.Log($"<color=#ff3696>Package: {packageName} init failed.</color>");
                return false;
            }
        }

        /// <summary>
        /// Update package manifest by package name
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static async UniTask<bool> UpdatePackage(string packageName)
        {
            var package = GetPackage(packageName);
            var versionOperation = package.UpdatePackageVersionAsync();
            await versionOperation;
            if (versionOperation.Status == EOperationStatus.Succeed)
            {
                var version = versionOperation.PackageVersion;

                var manifestOperation = package.UpdatePackageManifestAsync(version);
                await manifestOperation;
                if (manifestOperation.Status == EOperationStatus.Succeed)
                {
                    Debug.Log($"<color=#85cf0f>Package: {packageName} <color=#00c1ff>Update</color> completed successfully.</color>");
                    return true;
                }
                else
                {
                    Debug.Log($"<color=#ff3696>Package: {packageName} update manifest failed.</color>");
                    return false;
                }
            }
            else
            {
                Debug.Log($"<color=#ff3696>Package: {packageName} update version failed.</color>");
                return false;
            }
        }

        /// <summary>
        /// Check package has any files in local
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static bool CheckPackageHasAnyFilesInLocal(string packageName)
        {
            if (BundleConfig.playMode == BundleConfig.PlayMode.EditorSimulateMode)
            {
                Debug.Log($"<color=#ffce00><color=#0fa>[{BundleConfig.PlayMode.EditorSimulateMode}]</color> Check Package In Local <color=#0fa>return true</color></color>");
                return true;
            }

            try
            {
                var package = GetPackage(packageName);
                if (package == null) return false;

                string path = BundleConfig.GetLocalSandboxPackagePath(packageName);
                if (!Directory.Exists(path)) return false;

                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                return directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).Any();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get package files size in local
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static ulong GetPackageSizeInLocal(string packageName)
        {
            if (BundleConfig.playMode == BundleConfig.PlayMode.EditorSimulateMode)
            {
                Debug.Log($"<color=#ffce00><color=#0fa>[{BundleConfig.PlayMode.EditorSimulateMode}]</color> Get Package Size In Local <color=#0fa>return 1</color></color>");
                return 1;
            }

            try
            {
                var package = GetPackage(packageName);
                if (package == null) return 0;

                string path = BundleConfig.GetLocalSandboxPackagePath(packageName);
                if (!Directory.Exists(path)) return 0;

                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                return (ulong)directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Unload package and clear package files from sandbox
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static async UniTask<bool> UnloadPackageAndClearCacheFiles(string packageName)
        {
            var package = GetPackage(packageName);
            if (package == null) return false;

            try
            {
                // delete local files first
                package.ClearPackageSandbox();

                await UniTask.Delay(TimeSpan.FromSeconds(1f));

                // after clear package cache files
                var operation = package.ClearAllCacheFilesAsync();
                await operation;

                if (operation.Status == EOperationStatus.Succeed) return true;
            }
            catch
            {
                return false;
            }

            return false;
        }

        /// <summary>
        /// Set default package by package name
        /// </summary>
        /// <param name="packageName"></param>
        public static void SetDefaultPackage(string packageName)
        {
            var package = RegisterPackage(packageName);
            YooAssets.SetDefaultPackage(package);
            _currentPackageName = package.PackageName;
            _currentPackage = package;
        }

        /// <summary>
        /// Set default package by package list idx
        /// </summary>
        /// <param name="idx"></param>
        internal static void SetDefaultPackage(int idx)
        {
            var package = RegisterPackage(idx);
            YooAssets.SetDefaultPackage(package);
            _currentPackageName = package.PackageName;
            _currentPackage = package;
        }

        /// <summary>
        /// Switch default package by package name
        /// </summary>
        /// <param name="packageName"></param>
        public static void SwitchDefaultPackage(string packageName)
        {
            var package = GetPackage(packageName);
            if (package != null)
            {
                YooAssets.SetDefaultPackage(package);
                _currentPackageName = package.PackageName;
                _currentPackage = package;
            }
        }

        /// <summary>
        /// Get decryption service
        /// </summary>
        /// <returns></returns>
        public static IDecryptionServices GetDecryptionService()
        {
            return _decryption;
        }

        /// <summary>
        /// Get default package name
        /// </summary>
        /// <returns></returns>
        public static string GetDefaultPackageName()
        {
            return _currentPackageName;
        }

        /// <summary>
        /// Get default package
        /// </summary>
        /// <returns></returns>
        public static ResourcePackage GetDefaultPackage()
        {
            return _currentPackage;
        }

        /// <summary>
        /// Register package by package name
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static ResourcePackage RegisterPackage(string packageName)
        {
            var package = GetPackage(packageName);
            if (package == null) package = YooAssets.CreatePackage(packageName);
            return package;
        }

        /// <summary>
        /// Register package by package list idx
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        internal static ResourcePackage RegisterPackage(int idx)
        {
            var package = GetPresetAppPackage(idx);
            if (package == null) package = YooAssets.CreatePackage(GetPresetAppPackageNameByIdx(idx));
            return package;
        }

        /// <summary>
        /// Get package by package name
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static ResourcePackage GetPackage(string packageName)
        {
            return YooAssets.TryGetPackage(packageName);
        }

        /// <summary>
        /// Get packages by package names
        /// </summary>
        /// <param name="packageNames"></param>
        /// <returns></returns>
        public static ResourcePackage[] GetPackages(string[] packageNames)
        {
            if (packageNames != null && packageNames.Length > 0)
            {
                List<ResourcePackage> packages = new List<ResourcePackage>();
                foreach (string packageName in packageNames)
                {
                    var package = GetPackage(packageName);
                    if (package != null) packages.Add(package);
                }

                return packages.ToArray();
            }

            return null;
        }

        /// <summary>
        /// Get preset app packages
        /// </summary>
        /// <returns></returns>
        public static ResourcePackage[] GetPresetAppPackages()
        {
            if (BundleConfig.listPackages != null && BundleConfig.listPackages.Count > 0)
            {
                List<ResourcePackage> packages = new List<ResourcePackage>();
                foreach (var packageName in BundleConfig.listPackages)
                {
                    var package = GetPackage(packageName);
                    if (package != null) packages.Add(package);
                }

                return packages.ToArray();
            }

            return null;
        }

        /// <summary>
        /// Get preset app package from PatchLauncher by package list idx
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        internal static ResourcePackage GetPresetAppPackage(int idx)
        {
            string packageName = GetPresetAppPackageNameByIdx(idx);
            return GetPackage(packageName);
        }

        /// <summary>
        /// Get preset app package name list from PatchLauncher
        /// </summary>
        /// <returns></returns>
        public static string[] GetPresetAppPackageNames()
        {
            return BundleConfig.listPackages.ToArray();
        }

        /// <summary>
        /// Get preset app package name from PatchLauncher by package list idx
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public static string GetPresetAppPackageNameByIdx(int idx)
        {
            if (BundleConfig.listPackages.Count == 0) return null;

            if (idx >= BundleConfig.listPackages.Count)
            {
                idx = BundleConfig.listPackages.Count - 1;
                Debug.Log($"<color=#ff41d5>Package Idx Warning: {idx} is out of range will be auto set last idx.</color>");
            }
            else if (idx < 0) idx = 0;

            return BundleConfig.listPackages[idx];
        }

        /// <summary>
        /// Get specific package assetInfos (Tags)
        /// </summary>
        /// <param name="package"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static AssetInfo[] GetPackageAssetInfosByTags(ResourcePackage package, params string[] tags)
        {
            if (tags == null || tags.Length == 0) return default;

            return package.GetAssetInfos(tags);
        }

        /// <summary>
        /// Get specific package assetInfos (AssetNames)
        /// </summary>
        /// <param name="package"></param>
        /// <param name="assetNames"></param>
        /// <returns></returns>
        public static AssetInfo[] GetPackageAssetInfosByAssetNames(ResourcePackage package, params string[] assetNames)
        {
            if (assetNames == null || assetNames.Length == 0) return default;

            var assetInfos = new List<AssetInfo>();
            foreach (string assetName in assetNames)
            {
                var assetInfo = package.GetAssetInfo(assetName);
                if (assetInfo != null) assetInfos.Add(assetInfo);
            }

            return assetInfos.ToArray();
        }

        /// <summary>
        /// Get specific package downloader
        /// </summary>
        /// <param name="package"></param>
        /// <param name="maxConcurrencyDownloadCount"></param>
        /// <param name="failedRetryCount"></param>
        /// <returns></returns>
        public static ResourceDownloaderOperation GetPackageDownloader(ResourcePackage package, int maxConcurrencyDownloadCount, int failedRetryCount)
        {
            // if <= -1 will set be default values
            if (maxConcurrencyDownloadCount <= -1) maxConcurrencyDownloadCount = BundleConfig.maxConcurrencyDownloadCount;
            if (failedRetryCount <= -1) failedRetryCount = BundleConfig.failedRetryCount;

            // create all
            ResourceDownloaderOperation downloader = package.CreateResourceDownloader(maxConcurrencyDownloadCount, failedRetryCount);

            return downloader;
        }

        /// <summary>
        /// Get specific package downloader (Tags)
        /// </summary>
        /// <param name="package"></param>
        /// <param name="maxConcurrencyDownloadCount"></param>
        /// <param name="failedRetryCount"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static ResourceDownloaderOperation GetPackageDownloaderByTags(ResourcePackage package, int maxConcurrencyDownloadCount, int failedRetryCount, params string[] tags)
        {
            ResourceDownloaderOperation downloader;

            // if <= -1 will set be default values
            if (maxConcurrencyDownloadCount <= -1) maxConcurrencyDownloadCount = BundleConfig.maxConcurrencyDownloadCount;
            if (failedRetryCount <= -1) failedRetryCount = BundleConfig.failedRetryCount;

            // create all
            if (tags == null || tags.Length == 0)
            {
                downloader = package.CreateResourceDownloader(maxConcurrencyDownloadCount, failedRetryCount);
                return downloader;
            }

            // create by tags
            downloader = package.CreateResourceDownloader(tags, maxConcurrencyDownloadCount, failedRetryCount);

            return downloader;
        }

        /// <summary>
        /// Get specific package downloader (AssetNames)
        /// </summary>
        /// <param name="package"></param>
        /// <param name="maxConcurrencyDownloadCount"></param>
        /// <param name="failedRetryCount"></param>
        /// <param name="assetNames"></param>
        /// <returns></returns>
        public static ResourceDownloaderOperation GetPackageDownloaderByAssetNames(ResourcePackage package, int maxConcurrencyDownloadCount, int failedRetryCount, params string[] assetNames)
        {
            ResourceDownloaderOperation downloader;

            // if <= -1 will set be default values
            if (maxConcurrencyDownloadCount <= -1) maxConcurrencyDownloadCount = BundleConfig.maxConcurrencyDownloadCount;
            if (failedRetryCount <= -1) failedRetryCount = BundleConfig.failedRetryCount;

            // create all
            if (assetNames == null || assetNames.Length == 0)
            {
                downloader = package.CreateResourceDownloader(maxConcurrencyDownloadCount, failedRetryCount);
                return downloader;
            }

            // create by assetNames
            downloader = package.CreateBundleDownloader(assetNames, maxConcurrencyDownloadCount, failedRetryCount);

            return downloader;
        }

        /// <summary>
        /// Get specific package downloader (AssetInfos)
        /// </summary>
        /// <param name="package"></param>
        /// <param name="maxConcurrencyDownloadCount"></param>
        /// <param name="failedRetryCount"></param>
        /// <param name="assetInfos"></param>
        /// <returns></returns>
        public static ResourceDownloaderOperation GetPackageDownloaderByAssetInfos(ResourcePackage package, int maxConcurrencyDownloadCount, int failedRetryCount, params AssetInfo[] assetInfos)
        {
            ResourceDownloaderOperation downloader;

            // if <= -1 will set be default values
            if (maxConcurrencyDownloadCount <= -1) maxConcurrencyDownloadCount = BundleConfig.maxConcurrencyDownloadCount;
            if (failedRetryCount <= -1) failedRetryCount = BundleConfig.failedRetryCount;

            // create all
            if (assetInfos == null || assetInfos.Length == 0)
            {
                downloader = package.CreateResourceDownloader(maxConcurrencyDownloadCount, failedRetryCount);
                return downloader;
            }

            // create by assetInfos
            downloader = package.CreateBundleDownloader(assetInfos, maxConcurrencyDownloadCount, failedRetryCount);

            return downloader;
        }

        /// <summary>
        /// Release YooAsset (all packages)
        /// </summary>
        public static void Release()
        {
            YooAssets.Destroy();
        }
    }
}
