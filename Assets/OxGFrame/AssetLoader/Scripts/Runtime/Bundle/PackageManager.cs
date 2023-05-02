using Cysharp.Threading.Tasks;
using System;
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
        public static async UniTask<InitializationOperation> InitDefaultPackage()
        {
            return await InitPackage(_currentPackageName);
        }

        /// <summary>
        /// Init package by package name
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="autoUpdate"></param>
        /// <param name="hostServer"></param>
        /// <param name="fallbackHostServer"></param>
        /// <returns></returns>
        public static async UniTask<InitializationOperation> InitPackage(string packageName, bool autoUpdate = false, string hostServer = null, string fallbackHostServer = null, Action<string> errorHandler = null)
        {
            var package = RegisterPackage(packageName);
            if (package.InitializeStatus == EOperationStatus.Succeed)
            {
                Debug.Log($"<color=#ff9441>[return null] Package: {packageName} is already initialized, Status: {package.InitializeStatus}.</color>");
                return null;
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
                // if hostServer and fallbackHostServer are null will set from default config
                if (hostServer == null) hostServer = await BundleConfig.GetHostServerUrl(packageName);
                if (fallbackHostServer == null) fallbackHostServer = await BundleConfig.GetFallbackHostServerUrl(packageName);

                var createParameters = new HostPlayModeParameters();
                createParameters.DecryptionServices = _decryption;
                createParameters.QueryServices = new RequestQuery();
                createParameters.DefaultHostServer = hostServer;
                createParameters.FallbackHostServer = fallbackHostServer;
                initializationOperation = package.InitializeAsync(createParameters);
            }

            await initializationOperation;

            if (initializationOperation.Status != EOperationStatus.Succeed) errorHandler?.Invoke($"{packageName} is init failed.");

            if (autoUpdate) await UpdatePackage(packageName, errorHandler);

            return initializationOperation;
        }

        /// <summary>
        /// Init package by package list idx
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="autoUpdate"></param>
        /// <param name="hostServer"></param>
        /// <param name="fallbackHostServer"></param>
        /// <returns></returns>
        public static async UniTask<InitializationOperation> InitPackage(int idx, bool autoUpdate = false, string hostServer = null, string fallbackHostServer = null, Action<string> errorHandler = null)
        {
            string packageName = GetPackageNameByIdx(idx);
            return await InitPackage(packageName, autoUpdate, hostServer, fallbackHostServer);
        }

        /// <summary>
        /// Update package manifest by package name
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static async UniTask UpdatePackage(string packageName, Action<string> errorHandler = null)
        {
            var package = GetPackage(packageName);

            var versionOperation = package.UpdatePackageVersionAsync();
            await versionOperation;
            if (versionOperation.Status == EOperationStatus.Succeed)
            {
                var version = versionOperation.PackageVersion;

                var manifestOperation = package.UpdatePackageManifestAsync(version);
                await manifestOperation;
                if (manifestOperation.Status != EOperationStatus.Succeed) errorHandler?.Invoke($"{packageName} is update manifest failed.");
            }
            else errorHandler?.Invoke($"{packageName} is update version failed.");
        }

        /// <summary>
        /// Update package manifest by package list idx
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public static async UniTask UpdatePackage(int idx, Action<string> errorHandler = null)
        {
            var packageName = GetPackageNameByIdx(idx);
            await UpdatePackage(packageName, errorHandler);
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
        /// Get specific package downloader
        /// </summary>
        /// <param name="package"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static ResourceDownloaderOperation GetPackageDownloader(ResourcePackage package, params string[] tags)
        {
            ResourceDownloaderOperation downloader;
            // create all
            if (tags == null || tags.Length == 0) downloader = package.CreateResourceDownloader(BundleConfig.maxConcurrencyDownloadCount, BundleConfig.failedRetryCount);
            // create by tags
            else downloader = package.CreateResourceDownloader(tags, BundleConfig.maxConcurrencyDownloadCount, BundleConfig.failedRetryCount);

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
