using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YooAsset;

namespace OxGFrame.AssetLoader.Bundle
{
    internal static class PackageManager
    {
        private static string _currentPackageName;
        private static ResourcePackage _currentPackage;
        private static IDecryptionServices _decryption;

        /// <summary>
        /// Init settings
        /// </summary>
        public static void InitSetup()
        {
            #region Init YooAssets
            YooAssets.Destroy();
            YooAssets.Initialize();
            YooAssets.SetOperationSystemMaxTimeSlice(10);
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
                case BundleConfig.CryptogramType.HTXOR:
                    _decryption = new HTXorDecryption();
                    break;
                case BundleConfig.CryptogramType.AES:
                    _decryption = new AesDecryption();
                    break;
            }
            #endregion

            #region Init Package
            // Init AssetsPackage first
            if (BundleConfig.listPackage != null && BundleConfig.listPackage.Count > 0)
            {
                foreach (var assetsPackage in BundleConfig.listPackage)
                {
                    RegisterPackage(assetsPackage);
                }

                // Set default AssetsPackage
                SetDefaultPackage(0);
            }
            #endregion
        }

        /// <summary>
        /// Init package by default package name
        /// </summary>
        /// <returns></returns>
        public static async UniTask<bool> InitDefaultPackage()
        {
            var hostServer = await BundleConfig.GetHostServerUrl(_currentPackageName);
            var fallbackHostServer = await BundleConfig.GetFallbackHostServerUrl(_currentPackageName);
            var queryService = new RequestBuiltinQuery();

            return await InitPackage(_currentPackageName, false, hostServer, fallbackHostServer, queryService);
        }

        /// <summary>
        /// Init package by package name
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="autoUpdate"></param>
        /// <param name="hostServer"></param>
        /// <param name="fallbackHostServer"></param>
        /// <param name="queryService"></param>
        /// <returns></returns>
        public static async UniTask<bool> InitPackage(string packageName, bool autoUpdate, string hostServer, string fallbackHostServer, IQueryServices queryService)
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
                createParameters.SimulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(_currentPackageName);
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
                createParameters.QueryServices = queryService;
                createParameters.DefaultHostServer = hostServer;
                createParameters.FallbackHostServer = fallbackHostServer;
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
        /// Unload package and clear package files from sandbox
        /// </summary>
        /// <param name="packageName"></param>
        public static void UnloadPackageAndClearCacheFiles(string packageName)
        {
            var package = GetPackage(packageName);
            if (package == null) return;

            var sandboxPath = BundleConfig.GetLocalSandboxPath();
            string packagePath = Path.Combine(sandboxPath, BundleConfig.yooCacheFolderName, packageName);
            BundleUtility.DeleteFolder(packagePath);
            YooAssets.DestroyPackage(packageName);
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
        public static void SetDefaultPackage(int idx)
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
        /// Switch default package by package list idx
        /// </summary>
        /// <param name="idx"></param>
        public static void SwitchDefaultPackage(int idx)
        {
            var package = GetPackage(idx);
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
        public static ResourcePackage RegisterPackage(int idx)
        {
            var package = GetPackage(idx);
            if (package == null) package = YooAssets.CreatePackage(GetPackageNameByIdx(idx));
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
        /// Get package by package list idx
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public static ResourcePackage GetPackage(int idx)
        {
            string packageName = GetPackageNameByIdx(idx);
            return GetPackage(packageName);
        }

        /// <summary>
        /// Get package name list from PatchLauncher
        /// </summary>
        /// <returns></returns>
        public static string[] GetPackageNames()
        {
            return BundleConfig.listPackage.ToArray();
        }

        /// <summary>
        /// Get package name by package list idx
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public static string GetPackageNameByIdx(int idx)
        {
            if (idx >= BundleConfig.listPackage.Count)
            {
                idx = BundleConfig.listPackage.Count - 1;
                Debug.Log($"<color=#ff41d5>Package Idx Warning: {idx} is out of range will be auto set last idx.</color>");
            }
            else if (idx < 0) idx = 0;

            return BundleConfig.listPackage[idx];
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
