using Cysharp.Threading.Tasks;
using OxGKit.LoggingSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YooAsset;

namespace OxGFrame.AssetLoader.Bundle
{
    internal static class PackageManager
    {
        /// <summary>
        /// 初始標記
        /// </summary>
        internal static bool isInitialized = false;

        /// <summary>
        /// 釋放標記
        /// </summary>
        internal static bool isReleased = false;

        /// <summary>
        /// 當前預設包裹名稱
        /// </summary>
        private static string _currentPackageName;

        /// <summary>
        /// 當前預設包裹
        /// </summary>
        private static ResourcePackage _currentPackage;

        /// <summary>
        /// 資源解密服務
        /// </summary>
        private static DecryptionServices _bundleDecryptionServices;

        /// <summary>
        /// 清單解密服務
        /// </summary>
        private static DecryptionServices _manifestDecryptionServices;

        /// <summary>
        /// Init settings
        /// </summary>
        public async static UniTask InitSetup()
        {
            #region Init YooAssets
            YooAssets.Initialize();
            YooAssets.SetOperationSystemMaxTimeSlice(BundleConfig.operationSystemMaxTimeSlice);
            #endregion

            #region Init Decryption Type
            // Init decryption type
            var decryptType = BundleConfig.bundleDecryptArgs[0].Decrypt().ToUpper();
            switch (decryptType)
            {
                case BundleConfig.CryptogramType.NONE:
                    break;
                case BundleConfig.CryptogramType.OFFSET:
                    _bundleDecryptionServices = new OffsetDecryption(FileOperationType.Bundle);
                    break;
                case BundleConfig.CryptogramType.XOR:
                    _bundleDecryptionServices = new XorDecryption(FileOperationType.Bundle);
                    break;
                case BundleConfig.CryptogramType.HT2XOR:
                    _bundleDecryptionServices = new HT2XorDecryption(FileOperationType.Bundle);
                    break;
                case BundleConfig.CryptogramType.HT2XORPLUS:
                    _bundleDecryptionServices = new HT2XorPlusDecryption(FileOperationType.Bundle);
                    break;
                case BundleConfig.CryptogramType.AES:
                    _bundleDecryptionServices = new AesDecryption(FileOperationType.Bundle);
                    break;
                case BundleConfig.CryptogramType.CHACHA20:
                    _bundleDecryptionServices = new ChaCha20Decryption(FileOperationType.Bundle);
                    break;
                case BundleConfig.CryptogramType.XXTEA:
                    _bundleDecryptionServices = new XXTEADecryption(FileOperationType.Bundle);
                    break;
                case BundleConfig.CryptogramType.OFFSETXOR:
                    _bundleDecryptionServices = new OffsetXorDecryption(FileOperationType.Bundle);
                    break;
            }
            Logging.Print<Logger>($"<color=#ffe45a>Init Bundle Decryption: {decryptType}</color>");

            decryptType = BundleConfig.manifestDecryptArgs[0].Decrypt().ToUpper();
            switch (decryptType)
            {
                case BundleConfig.CryptogramType.NONE:
                    break;
                case BundleConfig.CryptogramType.OFFSET:
                    _manifestDecryptionServices = new OffsetDecryption(FileOperationType.Manifest);
                    break;
                case BundleConfig.CryptogramType.XOR:
                    _manifestDecryptionServices = new XorDecryption(FileOperationType.Manifest);
                    break;
                case BundleConfig.CryptogramType.HT2XOR:
                    _manifestDecryptionServices = new HT2XorDecryption(FileOperationType.Manifest);
                    break;
                case BundleConfig.CryptogramType.HT2XORPLUS:
                    _manifestDecryptionServices = new HT2XorPlusDecryption(FileOperationType.Manifest);
                    break;
                case BundleConfig.CryptogramType.AES:
                    _manifestDecryptionServices = new AesDecryption(FileOperationType.Manifest);
                    break;
                case BundleConfig.CryptogramType.CHACHA20:
                    _manifestDecryptionServices = new ChaCha20Decryption(FileOperationType.Manifest);
                    break;
                case BundleConfig.CryptogramType.XXTEA:
                    _manifestDecryptionServices = new XXTEADecryption(FileOperationType.Manifest);
                    break;
                case BundleConfig.CryptogramType.OFFSETXOR:
                    _manifestDecryptionServices = new OffsetXorDecryption(FileOperationType.Manifest);
                    break;
            }
            Logging.Print<Logger>($"<color=#ffe45a>Init Manifest Decryption: {decryptType}</color>");
            #endregion

            #region Init Preset Packages
            bool appInitialized = await InitPresetAppPackages();
            bool dlcInitialized = await InitPresetDlcPackages();
            #endregion

            #region Set Default Package
            // Set default package by first element (only for preset app packages)
            SetDefaultPackage(0);
            #endregion

            isInitialized = appInitialized && dlcInitialized;

            Logging.Print<Logger>($"<color=#ffe45a>InitSetup -> Initialized: {isInitialized}</color>");
        }

        /// <summary>
        /// Init preset app packages from PatchLauncher
        /// </summary>
        /// <returns></returns>
        public static async UniTask<bool> InitPresetAppPackages()
        {
            try
            {
                var presetAppPackageInfos = GetPresetAppPackageInfos();
                if (presetAppPackageInfos != null)
                {
                    foreach (var packageInfo in presetAppPackageInfos)
                    {
                        // autoUpdate = true, 因為新版 Yoo 必須獲取版號與 manifest 才能進行資源加載
                        bool isInitialized = await AssetPatcher.InitAppPackage(packageInfo, true);
                        if (isInitialized)
                        {
                            Logging.Print<Logger>($"<color=#85cf0f>Successfully initialized preset App package: <color=#ffe45a>{packageInfo.packageName}</color>.</color>");
                        }
                        else
                        {
                            Logging.PrintError<Logger>($"<color=#ff3696>Initialization failed for preset App package: <color=#ff8427>{packageInfo.packageName}</color>.</color>");
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logging.PrintException<Logger>(ex);
                return false;
            }
        }

        /// <summary>
        /// Init preset dlc packages from PatchLauncher
        /// </summary>
        /// <returns></returns>
        public static async UniTask<bool> InitPresetDlcPackages()
        {
            try
            {
                var presetDlcPackageInfos = GetPresetDlcPackageInfos();
                if (presetDlcPackageInfos != null)
                {
                    foreach (var packageInfo in presetDlcPackageInfos)
                    {
                        // autoUpdate = true, 因為新版 Yoo 必須獲取版號與 manifest 才能進行資源加載
                        bool isInitialized = await AssetPatcher.InitDlcPackage(packageInfo, true);
                        if (isInitialized)
                        {
                            Logging.Print<Logger>($"<color=#85cf0f>Successfully initialized preset DLC package: <color=#ffe45a>{packageInfo.packageName}</color>.</color>");
                        }
                        else
                        {
                            Logging.PrintError<Logger>($"<color=#ff3696>Initialization failed for preset DLC package: <color=#ff8427>{packageInfo.packageName}</color>.</color>");
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logging.PrintException<Logger>(ex);
                return false;
            }
        }

        /// <summary>
        /// Init package by package name
        /// </summary>
        /// <param name="packageInfo"></param>
        /// <param name="autoUpdate"></param>
        /// <param name="hostServer"></param>
        /// <param name="fallbackHostServer"></param>
        /// <returns></returns>
        public static async UniTask<bool> InitPackage(PackageInfoWithBuild packageInfo, bool autoUpdate, string hostServer, string fallbackHostServer)
        {
            var packageName = packageInfo.packageName;
            var buildMode = packageInfo.buildMode.ToString();

            var package = RegisterPackage(packageName);
            if (package.InitializeStatus == EOperationStatus.Succeed)
            {
                // The default initialized state is true
                bool isInitialized = true;
                if (autoUpdate) isInitialized = await UpdatePackage(packageName);
                Logging.Print<Logger>($"<color=#e2ec00>Package: {packageName} is initialized. Status: {package.InitializeStatus}.</color>");
                return isInitialized;
            }

            #region Simulate Mode
            InitializationOperation initializationOperation = null;
            if (BundleConfig.playMode == BundleConfig.PlayMode.EditorSimulateMode)
            {
                var buildResult = EditorSimulateModeHelper.SimulateBuild(packageName);
                var packageRoot = buildResult.PackageRootDirectory;
                var createParameters = new EditorSimulateModeParameters();
                createParameters.EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
                initializationOperation = package.InitializeAsync(createParameters);
            }
            #endregion

            #region Offline Mode
            if (BundleConfig.playMode == BundleConfig.PlayMode.OfflineMode)
            {
                var createParameters = new OfflinePlayModeParameters();
                var bundleDecryptionServices = _bundleDecryptionServices;
                var manifestDecryptionServices = _manifestDecryptionServices;
                bool builtinExists = await StreamingAssetsHelper.PackageExists(packageName);
                if (builtinExists)
                {
                    createParameters.BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters(bundleDecryptionServices);
                    createParameters.BuildinFileSystemParameters.AddParameter(FileSystemParametersDefine.MANIFEST_SERVICES, manifestDecryptionServices);

                    // Only raw file build pipeline need to append extension
                    if (buildMode.Equals(BundleConfig.BuildMode.RawFileBuildPipeline.ToString()))
                        createParameters.BuildinFileSystemParameters.AddParameter(FileSystemParametersDefine.APPEND_FILE_EXTENSION, true);
                }
                else
                {
                    createParameters.BuildinFileSystemParameters = null;
                }

                initializationOperation = package.InitializeAsync(createParameters);
            }
            #endregion

            #region Host Mode, Weak Host Mode
            if (BundleConfig.playMode == BundleConfig.PlayMode.HostMode ||
                BundleConfig.playMode == BundleConfig.PlayMode.WeakHostMode)
            {
                var createParameters = new HostPlayModeParameters();
                var remoteServices = new HostServers(hostServer, fallbackHostServer);
                var bundleDecryptionServices = _bundleDecryptionServices;
                var manifestDecryptionServices = _manifestDecryptionServices;
                bool builtinExists = await StreamingAssetsHelper.PackageExists(packageName);
                if (builtinExists)
                {
                    createParameters.BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters(bundleDecryptionServices);
                    createParameters.BuildinFileSystemParameters.AddParameter(FileSystemParametersDefine.MANIFEST_SERVICES, manifestDecryptionServices);

                    if (BundleConfig.playMode == BundleConfig.PlayMode.WeakHostMode)
                        createParameters.BuildinFileSystemParameters.AddParameter(FileSystemParametersDefine.COPY_BUILDIN_PACKAGE_MANIFEST, true);

                    // Only raw file build pipeline need to append extension
                    if (buildMode.Equals(BundleConfig.BuildMode.RawFileBuildPipeline.ToString()))
                        createParameters.BuildinFileSystemParameters.AddParameter(FileSystemParametersDefine.APPEND_FILE_EXTENSION, true);
                }
                else
                {
                    createParameters.BuildinFileSystemParameters = null;
                }

                #region Cache File System
                createParameters.CacheFileSystemParameters = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices, bundleDecryptionServices);
                createParameters.CacheFileSystemParameters.AddParameter(FileSystemParametersDefine.RESUME_DOWNLOAD_MINMUM_SIZE, BundleConfig.breakpointFileSizeThreshold);
                createParameters.CacheFileSystemParameters.AddParameter(FileSystemParametersDefine.MANIFEST_SERVICES, manifestDecryptionServices);

                if (BundleConfig.playMode == BundleConfig.PlayMode.WeakHostMode)
                    createParameters.CacheFileSystemParameters.AddParameter(FileSystemParametersDefine.INSTALL_CLEAR_MODE, EOverwriteInstallClearMode.None);

                // Only raw file build pipeline need to append extension
                if (buildMode.Equals(BundleConfig.BuildMode.RawFileBuildPipeline.ToString()))
                    createParameters.CacheFileSystemParameters.AddParameter(FileSystemParametersDefine.APPEND_FILE_EXTENSION, true);
                #endregion

                initializationOperation = package.InitializeAsync(createParameters);
            }
            #endregion

            #region WebGL Mode
            if (BundleConfig.playMode == BundleConfig.PlayMode.WebGLMode)
            {
                var createParameters = new WebPlayModeParameters();
                var bundleDecryptionServices = _bundleDecryptionServices;
                var manifestDecryptionServices = _manifestDecryptionServices;
                bool builtinExists = await StreamingAssetsHelper.PackageExists(packageName);
                if (builtinExists)
                {
                    createParameters.WebServerFileSystemParameters = FileSystemParameters.CreateDefaultWebServerFileSystemParameters(bundleDecryptionServices);
                    createParameters.WebServerFileSystemParameters.AddParameter(FileSystemParametersDefine.MANIFEST_SERVICES, manifestDecryptionServices);

                    // Only raw file build pipeline need to append extension
                    if (buildMode.Equals(BundleConfig.BuildMode.RawFileBuildPipeline.ToString()))
                        createParameters.WebServerFileSystemParameters.AddParameter(FileSystemParametersDefine.APPEND_FILE_EXTENSION, true);
                }
                else
                {
                    createParameters.WebServerFileSystemParameters = null;
                }

                initializationOperation = package.InitializeAsync(createParameters);
            }
            #endregion

            #region WebGL Remote Mode
            if (BundleConfig.playMode == BundleConfig.PlayMode.WebGLRemoteMode)
            {
                var createParameters = new WebPlayModeParameters();
                var remoteServices = new HostServers(hostServer, fallbackHostServer);
                var bundleDecryptionServices = _bundleDecryptionServices;
                var manifestDecryptionServices = _manifestDecryptionServices;
                bool builtinExists = await StreamingAssetsHelper.PackageExists(packageName);
                if (builtinExists)
                {
                    createParameters.WebServerFileSystemParameters = FileSystemParameters.CreateDefaultWebServerFileSystemParameters(bundleDecryptionServices);
                    createParameters.WebServerFileSystemParameters.AddParameter(FileSystemParametersDefine.MANIFEST_SERVICES, manifestDecryptionServices);

                    // Only raw file build pipeline need to append extension
                    if (buildMode.Equals(BundleConfig.BuildMode.RawFileBuildPipeline.ToString()))
                        createParameters.WebServerFileSystemParameters.AddParameter(FileSystemParametersDefine.APPEND_FILE_EXTENSION, true);
                }
                else
                {
                    createParameters.WebServerFileSystemParameters = null;
                }

                #region Web Remote File System
                createParameters.WebRemoteFileSystemParameters = FileSystemParameters.CreateDefaultWebRemoteFileSystemParameters(remoteServices, bundleDecryptionServices);
                createParameters.WebRemoteFileSystemParameters.AddParameter(FileSystemParametersDefine.RESUME_DOWNLOAD_MINMUM_SIZE, BundleConfig.breakpointFileSizeThreshold);
                createParameters.WebRemoteFileSystemParameters.AddParameter(FileSystemParametersDefine.MANIFEST_SERVICES, manifestDecryptionServices);

                // Only raw file build pipeline need to append extension
                if (buildMode.Equals(BundleConfig.BuildMode.RawFileBuildPipeline.ToString()))
                    createParameters.WebRemoteFileSystemParameters.AddParameter(FileSystemParametersDefine.APPEND_FILE_EXTENSION, true);
                #endregion

                initializationOperation = package.InitializeAsync(createParameters);
            }
            #endregion

            await initializationOperation;

            if (initializationOperation.Status == EOperationStatus.Succeed)
            {
                // The default initialized state is true
                bool isInitialized = true;
                if (autoUpdate) isInitialized = await UpdatePackage(packageName);
                Logging.Print<Logger>($"<color=#85cf0f>Package: <color=#ffe45a>{packageName}</color> <color=#00c1ff>Init</color> completed successfully.</color>");
                return isInitialized;
            }
            else
            {
                Logging.PrintError<Logger>($"<color=#ff3696>Package: <color=#ff8427>{packageName}</color> initialization failed.</color>");
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
            var versionOperation = package.RequestPackageVersionAsync();
            await versionOperation;
            if (versionOperation.Status == EOperationStatus.Succeed)
            {
                var version = versionOperation.PackageVersion;

                var manifestOperation = package.UpdatePackageManifestAsync(version);
                await manifestOperation;
                if (manifestOperation.Status == EOperationStatus.Succeed)
                {
                    #region Weak Host Mode
                    if (BundleConfig.playMode == BundleConfig.PlayMode.WeakHostMode)
                    {
                        // 儲存本地資源版本
                        BundleConfig.saver.SaveData(BundleConfig.LAST_PACKAGE_VERSIONS_KEY, packageName, version);
                    }
                    #endregion

                    Logging.Print<Logger>($"<color=#85cf0f>Package: <color=#ffe45a>{packageName}</color> <color=#00c1ff>Update</color> completed successfully.</color>");
                    return true;
                }
                else
                {
                    Logging.PrintError<Logger>($"<color=#ff3696>Package: <color=#ff8427>{packageName}</color> update manifest failed.</color>");
                    return false;
                }
            }
            else
            {
                #region Weak Host Mode
                if (BundleConfig.playMode == BundleConfig.PlayMode.WeakHostMode)
                {
                    // 獲取本地資源版本
                    string lastVersion = BundleConfig.saver.GetData(BundleConfig.LAST_PACKAGE_VERSIONS_KEY, packageName, string.Empty);
                    if (string.IsNullOrEmpty(lastVersion))
                    {
                        Logging.PrintError<Logger>($"<color=#ff3696>Package: <color=#ff8427>{packageName}</color>. Local version record not found, resources need to be updated (Please connect to the network)!</color>");
                        return false;
                    }
                    else
                    {
                        var manifestOperation = package.UpdatePackageManifestAsync(lastVersion);
                        await manifestOperation;
                        if (manifestOperation.Status == EOperationStatus.Succeed)
                        {
                            Logging.Print<Logger>($"<color=#85cf0f>Package: {packageName} <color=#00c1ff>Update</color> completed successfully.</color>");

                            // 驗證本地清單內容的完整性
                            var downloader = package.CreateResourceDownloader(BundleConfig.maxConcurrencyDownloadCount, BundleConfig.failedRetryCount);
                            if (downloader.TotalDownloadCount > 0)
                            {
                                Logging.PrintError<Logger>($"<color=#ff3696>Package: <color=#ff8427>{packageName}</color>. Local resources are incomplete. Update required (Please connect to the network)!</color>");
                                return false;
                            }

                            return true;
                        }
                        else
                        {
                            Logging.PrintError<Logger>($"<color=#ff3696>Package: <color=#ff8427>{packageName}</color>. Failed to load the local resource manifest file. Resource update is required (Please connect to the network)!</color>");
                            return false;
                        }
                    }
                }
                #endregion
                else
                {
                    Logging.PrintError<Logger>($"<color=#ff3696>Package: <color=#ff8427>{packageName}</color> update version failed.</color>");
                    return false;
                }
            }
        }

        /// <summary>
        /// Check package has any files in local sandbox
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static bool CheckPackageHasAnyFilesInLocal(string packageName)
        {
            if (BundleConfig.playMode == BundleConfig.PlayMode.EditorSimulateMode)
            {
                Logging.Print<Logger>($"<color=#ffce00><color=#0fa>[{BundleConfig.PlayMode.EditorSimulateMode}]</color> Check Package In Local <color=#0fa>return true</color></color>");
                return true;
            }

            try
            {
                var package = GetPackage(packageName);
                if (_PackageIsNull(packageName, package))
                    return false;

                string path = BundleConfig.GetLocalSandboxPackagePath(packageName);
                if (!Directory.Exists(path))
                    return false;

                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                return directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).Any();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get package files size in local sandbox
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static ulong GetPackageSizeInLocal(string packageName)
        {
            try
            {
                var package = GetPackage(packageName);
                if (_PackageIsNull(packageName, package))
                    return 0;

                if (BundleConfig.playMode == BundleConfig.PlayMode.EditorSimulateMode)
                {
                    Logging.Print<Logger>($"<color=#ffce00><color=#0fa>[{BundleConfig.PlayMode.EditorSimulateMode}]</color> Get Package Size In Local <color=#0fa>return 1</color></color>");
                    return 1;
                }

                string path = BundleConfig.GetLocalSandboxPackagePath(packageName);
                if (!Directory.Exists(path))
                    return 0;

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
        /// <param name="destroyPackage">Remove package from cache memory</param>
        /// <returns></returns>
        public static async UniTask<bool> UnloadPackageAndClearCacheFiles(string packageName, bool destroyPackage)
        {
            var package = GetPackage(packageName);
            if (_PackageIsNull(packageName, package))
                return true;

            bool processed = false;

            try
            {
                // Operation Tasks
                List<ClearCacheFilesOperation> operations = new List<ClearCacheFilesOperation>();

                // Clear cache files from sandbox
                var clearAllBundleFilesOperation = package.ClearCacheFilesAsync(EFileClearMode.ClearAllBundleFiles);
                operations.Add(clearAllBundleFilesOperation);

                // Clear manifest files from sandbox
                if (destroyPackage)
                {
                    var clearAllManifestFilesOperation = package.ClearCacheFilesAsync(EFileClearMode.ClearAllManifestFiles);
                    operations.Add(clearAllManifestFilesOperation);
                }

                foreach (var operation in operations)
                {
                    await operation;
                    if (operation.Status == EOperationStatus.Succeed)
                        processed = true;
                    else
                    {
                        processed = false;
                        break;
                    }
                }

                // Must ensure that processed is true
                if (processed && destroyPackage)
                    processed = await _UnloadPackage(package);

                return processed;
            }
            catch (Exception ex)
            {
                Logging.PrintException<Logger>(ex);
                processed = false;
                return processed;
            }
        }

        /// <summary>
        /// Unload (Destroy) package from cache memory
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static async UniTask<bool> UnloadPackage(string packageName)
        {
            var package = GetPackage(packageName);
            return await _UnloadPackage(package);
        }

        /// <summary>
        /// Unload (Destroy) package from cache memory
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        private static async UniTask<bool> _UnloadPackage(ResourcePackage package)
        {
            if (_PackageIsNull(string.Empty, package))
                return true;

            // Destroy package from cache memory
            await package.DestroyAsync();
            return YooAssets.RemovePackage(package);
        }

        /// <summary>
        /// Set default package by package name
        /// </summary>
        /// <param name="packageName"></param>
        public static void SetDefaultPackage(string packageName)
        {
            var package = RegisterPackage(packageName);
            if (package != null)
            {
                YooAssets.SetDefaultPackage(package);
                _currentPackageName = package.PackageName;
                _currentPackage = package;
            }
        }

        /// <summary>
        /// Set default package by preset app package list idx
        /// </summary>
        /// <param name="idx"></param>
        internal static void SetDefaultPackage(int idx)
        {
            var package = RegisterPackage(idx);
            if (package != null)
            {
                YooAssets.SetDefaultPackage(package);
                _currentPackageName = package.PackageName;
                _currentPackage = package;
            }
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
            else
                Logging.PrintError<Logger>($"<color=#ff2478>Switch default package failed! Cannot find package: {packageName}.</color>");
        }

        /// <summary>
        /// Get decryption service
        /// </summary>
        /// <returns></returns>
        public static IDecryptionServices GetDecryptionService()
        {
            return _bundleDecryptionServices;
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
            if (package == null)
            {
                if (!string.IsNullOrEmpty(packageName))
                    package = YooAssets.CreatePackage(packageName);
            }
            return package;
        }

        /// <summary>
        /// Register package by preset app package list idx
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        internal static ResourcePackage RegisterPackage(int idx)
        {
            var package = GetPresetAppPackage(idx);
            if (package == null)
            {
                var packageName = GetPresetAppPackageNameByIdx(idx);
                if (!string.IsNullOrEmpty(packageName))
                    package = YooAssets.CreatePackage(packageName);
            }
            return package;
        }

        /// <summary>
        /// Get package by package name
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static ResourcePackage GetPackage(string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
                return null;
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
            if (BundleConfig.listAppPackages != null && BundleConfig.listAppPackages.Count > 0)
            {
                List<ResourcePackage> packages = new List<ResourcePackage>();
                foreach (var packageInfo in BundleConfig.listAppPackages)
                {
                    var package = GetPackage(packageInfo.packageName);
                    if (package != null) packages.Add(package);
                }

                return packages.ToArray();
            }

            return new ResourcePackage[] { };
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
        /// Get preset dlc packages
        /// </summary>
        /// <returns></returns>
        public static ResourcePackage[] GetPresetDlcPackages()
        {
            if (BundleConfig.listDlcPackages != null && BundleConfig.listDlcPackages.Count > 0)
            {
                List<ResourcePackage> packages = new List<ResourcePackage>();
                foreach (var packageInfo in BundleConfig.listDlcPackages)
                {
                    var package = GetPackage(packageInfo.packageName);
                    if (package != null) packages.Add(package);
                }

                return packages.ToArray();
            }

            return new ResourcePackage[] { };
        }

        internal static PackageInfoWithBuild GetPresetPackageInfo(string packageName)
        {
            List<PackageInfoWithBuild> l1 = BundleConfig.listAppPackages.Cast<PackageInfoWithBuild>().ToList();
            List<PackageInfoWithBuild> l2 = BundleConfig.listDlcPackages.Cast<PackageInfoWithBuild>().ToList();
            PackageInfoWithBuild[] packageInfos = l1.Union(l2).ToArray();
            foreach (var packageInfo in packageInfos)
            {
                if (packageInfo.packageName.Equals(packageName))
                {
                    return packageInfo;
                }
            }
            return null;
        }

        /// <summary>
        /// Get preset app package info list from PatchLauncher
        /// </summary>
        /// <returns></returns>
        public static AppPackageInfoWithBuild[] GetPresetAppPackageInfos()
        {
            return BundleConfig.listAppPackages.ToArray();
        }

        /// <summary>
        /// Get preset app package name list from PatchLauncher
        /// </summary>
        /// <returns></returns>
        public static string[] GetPresetAppPackageNames()
        {
            List<string> packageNames = new List<string>();
            foreach (var packageInfo in BundleConfig.listAppPackages)
            {
                packageNames.Add(packageInfo.packageName);
            }
            return packageNames.ToArray();
        }

        /// <summary>
        /// Get preset app package name from PatchLauncher by package list idx
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public static string GetPresetAppPackageNameByIdx(int idx)
        {
            if (BundleConfig.listAppPackages.Count == 0) return null;

            if (idx >= BundleConfig.listAppPackages.Count)
            {
                idx = BundleConfig.listAppPackages.Count - 1;
                Logging.PrintWarning<Logger>($"<color=#ff41d5>Package Idx Warning: {idx} is out of range will be auto set last idx.</color>");
            }
            else if (idx < 0) idx = 0;

            return BundleConfig.listAppPackages[idx].packageName;
        }

        /// <summary>
        /// Get preset dlc package name list from PatchLauncher
        /// </summary>
        /// <returns></returns>
        public static string[] GetPresetDlcPackageNames()
        {
            List<string> packageNames = new List<string>();
            foreach (var packageInfo in BundleConfig.listDlcPackages)
            {
                packageNames.Add(packageInfo.packageName);
            }
            return packageNames.ToArray();
        }

        /// <summary>
        /// Get preset dlc package info list from PatchLauncher
        /// </summary>
        /// <returns></returns>
        public static DlcPackageInfoWithBuild[] GetPresetDlcPackageInfos()
        {
            return BundleConfig.listDlcPackages.ToArray();
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
        public async static UniTask Release()
        {
            if (!isReleased)
            {
                isReleased = true;

                // 遍歷卸載
                var packages = YooAssets.GetAllPackages();
                foreach (var package in packages)
                {
                    await package.DestroyAsync();
                    YooAssets.RemovePackage(package);
                }

                // 強制銷毀
                YooAssets.Destroy();
            }
        }

        #region Checker
        private static bool _PackageIsNull(string PackageName, ResourcePackage package)
        {
            if (package == null)
            {
                Logging.PrintWarning<Logger>($"[Invalid unload] Package is null (return true). Package Name: {PackageName}");
                return true;
            }
            return false;
        }
        #endregion
    }
}
