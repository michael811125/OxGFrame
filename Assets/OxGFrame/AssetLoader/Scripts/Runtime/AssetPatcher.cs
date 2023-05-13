using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader.Bundle;
using OxGFrame.AssetLoader.Utility;
using YooAsset;

namespace OxGFrame.AssetLoader
{
    public static class AssetPatcher
    {
        #region Other
        /// <summary>
        /// Clear user's last selected group info
        /// </summary>
        public static void ClearLastGroupInfo()
        {
            PatchManager.DelLastGroupInfo();
        }

        /// <summary>
        /// Get default group tag (#all)
        /// </summary>
        /// <returns></returns>
        public static string GetDefaultGroupTag()
        {
            return PatchManager.DEFAULT_GROUP_TAG;
        }

        /// <summary>
        /// Get app store link
        /// </summary>
        /// <returns></returns>
        public static async UniTask<string> GetAppStoreLink()
        {
            return await BundleConfig.GetAppStoreLink();
        }

        /// <summary>
        /// Go to app store (Application.OpenURL)
        /// </summary>
        public static void GoToAppStore()
        {
            BundleConfig.GoToAppStore().Forget();
        }

        /// <summary>
        /// Get local save persistent path
        /// </summary>
        /// <returns></returns>
        public static string GetLocalSandboxPath()
        {
            return BundleConfig.GetLocalSandboxPath();
        }

        /// <summary>
        /// Get request streaming assets path
        /// </summary>
        /// <returns></returns>
        public static string GetRequestStreamingAssetsPath()
        {
            return BundleConfig.GetRequestStreamingAssetsPath();
        }
        #endregion

        #region Patch Status
        /// <summary>
        /// Return patch mode initialized
        /// </summary>
        /// <returns></returns>
        public static bool IsInitialized()
        {
            return PatchLauncher.isInitialized;
        }

        /// <summary>
        /// Return patch check state
        /// </summary>
        /// <returns></returns>
        public static bool IsCheck()
        {
            return PatchManager.GetInstance().IsCheck();
        }

        /// <summary>
        /// Return patch repair state
        /// </summary>
        /// <returns></returns>
        public static bool IsRepair()
        {
            return PatchManager.GetInstance().IsRepair();
        }

        /// <summary>
        /// Return patch done state
        /// </summary>
        /// <returns></returns>
        public static bool IsDone()
        {
            return PatchManager.GetInstance().IsDone();
        }
        #endregion

        #region Patch Operation
        /// <summary>
        /// Start patch update
        /// </summary>
        public static void Check()
        {
            PatchManager.GetInstance().Check();
        }

        /// <summary>
        /// Start patch repair
        /// </summary>
        public static void Repair()
        {
            PatchManager.GetInstance().Repair();
        }

        /// <summary>
        /// Pause main downloader
        /// </summary>
        public static void Pause()
        {
            PatchManager.GetInstance().Pause();
        }

        /// <summary>
        /// Resume main downloader
        /// </summary>
        public static void Resume()
        {
            PatchManager.GetInstance().Resume();
        }

        /// <summary>
        /// Cancel main downloader
        /// </summary>
        public static void Cancel()
        {
            PatchManager.GetInstance().Cancel();
        }

        /// <summary>
        /// Get app version
        /// </summary>
        /// <returns></returns>
        public static string GetAppVersion()
        {
            return PatchManager.appVersion;
        }

        /// <summary>
        /// Get patch version (Recommend use encode to display)
        /// </summary>
        /// <returns></returns>
        public static string GetPatchVersion(bool encode = false, int encodeLength = 6, string separator = "-")
        {
            string patchVersion = PatchManager.patchVersion;

            if (encode)
            {
                string versionHash = BundleUtility.GetVersionHash(separator, patchVersion, 1 << 5);
                string versionNumber = BundleUtility.GetVersionNumber(versionHash, encodeLength);
                return versionNumber;
            }

            return patchVersion;
        }
        #endregion

        #region Package Operation
        /// <summary>
        /// Unload package and clear package files from sandbox
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static async UniTask<bool> UnloadPackageAndClearCacheFiles(string packageName)
        {
            return await PackageManager.UnloadPackageAndClearCacheFiles(packageName);
        }

        /// <summary>
        /// Init package by package name (If PlayMode is HostMode will request from default host path)
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="autoUpdate"></param>
        /// <returns></returns>
        public static async UniTask<bool> InitPackage(string packageName, bool autoUpdate = false)
        {
            string hostServer = null;
            string fallbackHostServer = null;
            IQueryServices queryService = null;

            // Only Host Mode
            if (BundleConfig.playMode == BundleConfig.PlayMode.HostMode)
            {
                hostServer = await BundleConfig.GetHostServerUrl(packageName);
                fallbackHostServer = await BundleConfig.GetFallbackHostServerUrl(packageName);
                queryService = new RequestBuiltinQuery();
            }

            return await PackageManager.InitPackage(packageName, autoUpdate, hostServer, fallbackHostServer, queryService);
        }

        /// <summary>
        /// Init package by package list idx (If PlayMode is HostMode will request from default host path)
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="autoUpdate"></param>
        /// <returns></returns>
        public static async UniTask<bool> InitPackage(int idx, bool autoUpdate = false)
        {
            string packageName = GetPackageNameByIdx(idx);

            string hostServer = null;
            string fallbackHostServer = null;
            IQueryServices queryService = null;

            // Only Host Mode
            if (BundleConfig.playMode == BundleConfig.PlayMode.HostMode)
            {
                hostServer = await BundleConfig.GetHostServerUrl(packageName);
                fallbackHostServer = await BundleConfig.GetFallbackHostServerUrl(packageName);
                queryService = new RequestBuiltinQuery();
            }

            return await PackageManager.InitPackage(packageName, autoUpdate, hostServer, fallbackHostServer, queryService);
        }

        /// <summary>
        /// Init dlc package (If PlayMode is HostMode will request from default host dlc path)
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="dlcVersion"></param>
        /// <param name="autoUpdate"></param>
        /// <returns></returns>
        public static async UniTask<bool> InitDlcPackage(string packageName, string dlcVersion, bool autoUpdate = false)
        {
            string hostServer = null;
            string fallbackHostServer = null;
            IQueryServices queryService = null;

            // Only Host Mode
            if (BundleConfig.playMode == BundleConfig.PlayMode.HostMode)
            {
                hostServer = await BundleConfig.GetDlcHostServerUrl(packageName, dlcVersion);
                fallbackHostServer = await BundleConfig.GetDlcFallbackHostServerUrl(packageName, dlcVersion);
                queryService = new RequestSandboxQuery(packageName);
            }

            return await PackageManager.InitPackage(packageName, autoUpdate, hostServer, fallbackHostServer, queryService);
        }

        /// <summary>
        /// Init dlc package (If PlayMode is HostMode will request from default host dlc path)
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="dlcVersion"></param>
        /// <param name="autoUpdate"></param>
        /// <param name="queryService"></param>
        /// <returns></returns>
        public static async UniTask<bool> InitDlcPackage(string packageName, string dlcVersion, bool autoUpdate = false, IQueryServices queryService = null)
        {
            string hostServer = null;
            string fallbackHostServer = null;

            // Only Host Mode
            if (BundleConfig.playMode == BundleConfig.PlayMode.HostMode)
            {
                hostServer = await BundleConfig.GetDlcHostServerUrl(packageName, dlcVersion);
                fallbackHostServer = await BundleConfig.GetDlcFallbackHostServerUrl(packageName, dlcVersion);
                queryService = (queryService == null) ? new RequestSandboxQuery(packageName) : queryService;
            }

            return await PackageManager.InitPackage(packageName, autoUpdate, hostServer, fallbackHostServer, queryService);
        }

        /// <summary>
        /// Init package by package name (Custom your host path and query service)
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="hostServer"></param>
        /// <param name="fallbackHostServer"></param>
        /// <param name="autoUpdate"></param>
        /// <param name="queryService"></param>
        /// <returns></returns>
        public static async UniTask<bool> InitCustomPackage(string packageName, string hostServer, string fallbackHostServer, IQueryServices queryService, bool autoUpdate = false)
        {
            return await PackageManager.InitPackage(packageName, autoUpdate, hostServer, fallbackHostServer, queryService);
        }

        /// <summary>
        /// Init package by package list idx (Custom your host path and query service)
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="hostServer"></param>
        /// <param name="fallbackHostServer"></param>
        /// <param name="autoUpdate"></param>
        /// <param name="queryService"></param>
        /// <returns></returns>
        public static async UniTask<bool> InitCustomPackage(int idx, string hostServer, string fallbackHostServer, IQueryServices queryService, bool autoUpdate = false)
        {
            string packageName = GetPackageNameByIdx(idx);
            return await PackageManager.InitPackage(packageName, autoUpdate, hostServer, fallbackHostServer, queryService);
        }

        /// <summary>
        /// Update package manifest by package name
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static async UniTask<bool> UpdatePackage(string packageName)
        {
            return await PackageManager.UpdatePackage(packageName);
        }

        /// <summary>
        /// Update package manifest by package list idx
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public static async UniTask<bool> UpdatePackage(int idx)
        {
            string packageName = GetPackageNameByIdx(idx);
            return await PackageManager.UpdatePackage(packageName);
        }

        /// <summary>
        /// Set default package. If is not exist will auto register and set it be default
        /// </summary>
        /// <param name="packageName"></param>
        public static void SetDefaultPackage(string packageName)
        {
            PackageManager.SetDefaultPackage(packageName);
        }

        public static void SetDefaultPackage(int idx)
        {
            PackageManager.SetDefaultPackage(idx);
        }

        /// <summary>
        /// Switch already register package
        /// </summary>
        /// <param name="packageName"></param>
        public static void SwitchDefaultPackage(string packageName)
        {
            PackageManager.SwitchDefaultPackage(packageName);
        }

        public static void SwitchDefaultPackage(int idx)
        {
            PackageManager.SwitchDefaultPackage(idx);
        }

        /// <summary>
        /// Get default package name
        /// </summary>
        /// <returns></returns>
        public static string GetDefaultPackageName()
        {
            return PackageManager.GetDefaultPackageName();
        }

        /// <summary>
        /// Get default package
        /// </summary>
        /// <returns></returns>
        public static ResourcePackage GetDefaultPackage()
        {
            return PackageManager.GetDefaultPackage();
        }

        /// <summary>
        /// Get package by name
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static ResourcePackage GetPackage(string packageName)
        {
            return PackageManager.GetPackage(packageName);
        }

        /// <summary>
        /// Get package by idx
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public static ResourcePackage GetPackage(int idx)
        {
            return PackageManager.GetPackage(idx);
        }

        /// <summary>
        /// Retrun package name list
        /// </summary>
        /// <returns></returns>
        public static string[] GetPackageNames()
        {
            return PackageManager.GetPackageNames();
        }

        /// <summary>
        /// Get package name by idx
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public static string GetPackageNameByIdx(int idx)
        {
            return PackageManager.GetPackageNameByIdx(idx);
        }

        /// <summary>
        /// Get specific package assetInfos (Tags)
        /// </summary>
        /// <param name="package"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static AssetInfo[] GetPackageAssetInfosByTags(ResourcePackage package, params string[] tags)
        {
            return PackageManager.GetPackageAssetInfosByTags(package, tags);
        }

        /// <summary>
        /// Get specific package assetInfos (AssetNames)
        /// </summary>
        /// <param name="package"></param>
        /// <param name="assetNames"></param>
        /// <returns></returns>
        public static AssetInfo[] GetPackageAssetInfosByAssetNames(ResourcePackage package, params string[] assetNames)
        {
            return PackageManager.GetPackageAssetInfosByAssetNames(package, assetNames);
        }

        /// <summary>
        /// Get specific package downloader
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        public static ResourceDownloaderOperation GetPackageDownloader(ResourcePackage package)
        {
            return PackageManager.GetPackageDownloader(package, -1, -1);
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
            return PackageManager.GetPackageDownloader(package, maxConcurrencyDownloadCount, failedRetryCount);
        }

        /// <summary>
        /// Get specific package downloader (Tags)
        /// </summary>
        /// <param name="package"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static ResourceDownloaderOperation GetPackageDownloaderByTags(ResourcePackage package, params string[] tags)
        {
            return PackageManager.GetPackageDownloaderByTags(package, -1, -1, tags);
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
            return PackageManager.GetPackageDownloaderByTags(package, maxConcurrencyDownloadCount, failedRetryCount, tags);
        }

        /// <summary>
        /// Get specific package downloader (AssetNames)
        /// </summary>
        /// <param name="package"></param>
        /// <param name="assetNames"></param>
        /// <returns></returns>
        public static ResourceDownloaderOperation GetPackageDownloaderByAssetNames(ResourcePackage package, params string[] assetNames)
        {
            return PackageManager.GetPackageDownloaderByAssetNames(package, -1, -1, assetNames);
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
            return PackageManager.GetPackageDownloaderByAssetNames(package, maxConcurrencyDownloadCount, failedRetryCount, assetNames);
        }

        /// <summary>
        /// Get specific package downloader (AssetInfos)
        /// </summary>
        /// <param name="package"></param>
        /// <param name="assetInfos"></param>
        /// <returns></returns>
        public static ResourceDownloaderOperation GetPackageDownloaderByAssetInfos(ResourcePackage package, params AssetInfo[] assetInfos)
        {
            return PackageManager.GetPackageDownloaderByAssetInfos(package, -1, -1, assetInfos);
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
            return PackageManager.GetPackageDownloaderByAssetInfos(package, maxConcurrencyDownloadCount, failedRetryCount, assetInfos);
        }
    }
    #endregion
}