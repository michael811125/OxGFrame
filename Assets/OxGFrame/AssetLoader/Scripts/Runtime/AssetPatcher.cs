using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader.Bundle;
using OxGFrame.AssetLoader.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YooAsset;
using static OxGFrame.AssetLoader.Utility.DownloadSpeedCalculator;
using static YooAsset.DownloaderOperation;

namespace OxGFrame.AssetLoader
{
    public static class AssetPatcher
    {
        public struct DownloadInfo
        {
            public int totalCount;
            public ulong totalBytes;
        }

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
            return BundleConfig.DEFAULT_GROUP_TAG;
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
        /// Get local save persistent root path (.../yoo)
        /// </summary>
        /// <returns></returns>
        public static string GetLocalSandboxRootPath()
        {
            return BundleConfig.GetLocalSandboxRootPath();
        }

        /// <summary>
        /// Get local save persistent path by pakcage (...yoo/PackageName)
        /// </summary>
        /// <returns></returns>
        public static string GetLocalSandboxPackagePath(string packageName)
        {
            return BundleConfig.GetLocalSandboxPackagePath(packageName);
        }

        /// <summary>
        /// Get built-in root path (.../StreamingAssets/yoo)
        /// </summary>
        /// <returns></returns>
        public static string GetBuiltinRootPath()
        {
            return BundleConfig.GetBuiltinRootPath();
        }

        /// <summary>
        /// Get built-in path by package (.../StreamingAssets/yoo/PackageName)
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static string GetBuiltinPackagePath(string packageName)
        {
            return BundleConfig.GetBuiltinPackagePath(packageName);
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
            return PackageManager.isInitialized;
        }

        /// <summary>
        /// Return yooasset is released
        /// </summary>
        /// <returns></returns>
        public static bool IsReleased()
        {
            return PackageManager.isReleased;
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
        /// Get app os platform
        /// </summary>
        /// <returns></returns>
        public static string GetPlatform()
        {
            // For simulate mode
            if (string.IsNullOrEmpty(PatchManager.platform))
                return Application.platform.ToString();
            return PatchManager.platform;
        }

        /// <summary>
        /// Get app version
        /// </summary>
        /// <returns></returns>
        public static string GetAppVersion()
        {
            // For simulate mode
            if (string.IsNullOrEmpty(PatchManager.appVersion))
                return Application.version;
            return PatchManager.appVersion;
        }

        /// <summary>
        /// Get newest patch version (Recommend use encode to display)
        /// <para> Min and Max length = 11 to 32 </para>
        /// </summary>
        /// <returns></returns>
        public static string GetPatchVersion(bool encode = false, int length = 16, string separator = "-")
        {
            string[] versions = PatchManager.patchVersions.Values.ToArray();
            string newestVersion = BundleUtility.NewestPackageVersion(versions);
            string patchVersion = string.IsNullOrEmpty(newestVersion) ? string.Empty : newestVersion;

            // For simulate mode
            if (string.IsNullOrEmpty(patchVersion))
                patchVersion = BundleUtility.GetDefaultPackageVersion();

            if (encode)
            {
                string versionNumber = BundleUtility.GetVersionNumber(patchVersion, length, separator);
                return versionNumber;
            }

            return patchVersion;
        }
        #endregion

        #region Package Operation
        /// <summary>
        /// Check package has any files in local sandbox
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static bool CheckPackageHasAnyFilesInLocal(string packageName)
        {
            return PackageManager.CheckPackageHasAnyFilesInLocal(packageName);
        }

        /// <summary>
        /// Get package files size in local sandbox
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static ulong GetPackageSizeInLocal(string packageName)
        {
            return PackageManager.GetPackageSizeInLocal(packageName);
        }

        /// <summary>
        /// Unload package and clear package files from local sandbox
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="destroyPackage">Remove package from cache memory</param>
        /// <returns></returns>
        public static async UniTask<bool> UnloadPackageAndClearCacheFiles(string packageName, bool destroyPackage = false)
        {
            return await PackageManager.UnloadPackageAndClearCacheFiles(packageName, destroyPackage);
        }

        /// <summary>
        /// Unload (Destroy) package from cache memory
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static async UniTask<bool> UnloadPackage(string packageName)
        {
            return await PackageManager.UnloadPackage(packageName);
        }

        #region Init Package
        /// <summary>
        /// Init package by type
        /// </summary>
        /// <param name="packageInfo"></param>
        /// <param name="autoUpdate"></param>
        /// <returns></returns>
        public static async UniTask<bool> InitPackage(PackageInfoWithBuild packageInfo, bool autoUpdate = false)
        {
            if (packageInfo is AppPackageInfoWithBuild)
            {
                return await InitAppPackage(packageInfo as AppPackageInfoWithBuild, autoUpdate);
            }
            else if (packageInfo is DlcPackageInfoWithBuild)
            {
                return await InitDlcPackage(packageInfo as DlcPackageInfoWithBuild, autoUpdate);
            }

            return false;
        }

        #region App Package
        /// <summary>
        /// Init app package (If PlayMode is HostMode will request from default host path)
        /// </summary>
        /// <param name="packageInfo"></param>
        /// <param name="autoUpdate"></param>
        /// <returns></returns>
        public static async UniTask<bool> InitAppPackage(AppPackageInfoWithBuild packageInfo, bool autoUpdate = false)
        {
            string hostServer = null;
            string fallbackHostServer = null;

            // Host Mode or WebGL Mode
            if (BundleConfig.playMode == BundleConfig.PlayMode.HostMode ||
                BundleConfig.playMode == BundleConfig.PlayMode.WeakHostMode ||
                BundleConfig.playMode == BundleConfig.PlayMode.WebGLRemoteMode)
            {
                hostServer = await BundleConfig.GetHostServerUrl(packageInfo.packageName);
                fallbackHostServer = await BundleConfig.GetFallbackHostServerUrl(packageInfo.packageName);
            }

            return await PackageManager.InitPackage(packageInfo, autoUpdate, hostServer, fallbackHostServer);
        }
        #endregion

        #region DLC Package
        /// <summary>
        /// Init dlc package (If PlayMode is HostMode will request from default host dlc path)
        /// </summary>
        /// <param name="packageInfo"></param>
        /// <param name="autoUpdate"></param>
        /// <returns></returns>
        public static async UniTask<bool> InitDlcPackage(DlcPackageInfoWithBuild packageInfo, bool autoUpdate = false)
        {
            string hostServer = packageInfo.hostServer;
            string fallbackHostServer = packageInfo.fallbackHostServer;

            // Host Mode or WebGL Mode
            if (BundleConfig.playMode == BundleConfig.PlayMode.HostMode ||
                BundleConfig.playMode == BundleConfig.PlayMode.WeakHostMode ||
                BundleConfig.playMode == BundleConfig.PlayMode.WebGLRemoteMode)
            {
                hostServer = string.IsNullOrEmpty(hostServer) ? await BundleConfig.GetDlcHostServerUrl(packageInfo.packageName, packageInfo.dlcVersion, packageInfo.withoutPlatform) : hostServer;
                fallbackHostServer = string.IsNullOrEmpty(fallbackHostServer) ? await BundleConfig.GetDlcFallbackHostServerUrl(packageInfo.packageName, packageInfo.dlcVersion, packageInfo.withoutPlatform) : fallbackHostServer;
            }

            return await PackageManager.InitPackage(packageInfo, autoUpdate, hostServer, fallbackHostServer);
        }
        #endregion
        #endregion

        #region Update Package
        /// <summary>
        /// Update package manifest by package name
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static async UniTask<bool> UpdatePackage(string packageName)
        {
            return await PackageManager.UpdatePackage(packageName);
        }
        #endregion

        #region Default Package
        /// <summary>
        /// Set default package. If is not exist will auto register and set it be default
        /// </summary>
        /// <param name="packageName"></param>
        public static void SetDefaultPackage(string packageName)
        {
            PackageManager.SetDefaultPackage(packageName);
        }

        /// <summary>
        /// Switch already register package
        /// </summary>
        /// <param name="packageName"></param>
        public static void SwitchDefaultPackage(string packageName)
        {
            PackageManager.SwitchDefaultPackage(packageName);
        }

        /// <summary>
        /// Get default package name
        /// </summary>
        /// <returns></returns>
        public static string GetDefaultPackageName()
        {
            return PackageManager.GetDefaultPackageName();
        }
        #endregion

        #region Get Package
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
        /// Get package by names
        /// </summary>
        /// <param name="packageNames"></param>
        /// <returns></returns>
        public static ResourcePackage[] GetPackages(params string[] packageNames)
        {
            return PackageManager.GetPackages(packageNames);
        }
        #endregion

        #region Get Preset Package
        /// <summary>
        /// Get preset app packages
        /// </summary>
        /// <returns></returns>
        public static ResourcePackage[] GetPresetAppPackages()
        {
            return PackageManager.GetPresetAppPackages();
        }

        /// <summary>
        /// Get preset dlc packages
        /// </summary>
        /// <returns></returns>
        public static ResourcePackage[] GetPresetDlcPackages()
        {
            return PackageManager.GetPresetDlcPackages();
        }
        #endregion

        #region Get Preset Package Name
        /// <summary>
        /// Get preset app package info list from PatchLauncher
        /// </summary>
        /// <returns></returns>
        public static PackageInfoWithBuild[] GetPresetAppPackageInfos()
        {
            return PackageManager.GetPresetAppPackageInfos();
        }

        /// <summary>
        /// Get preset app package name list from PatchLauncher
        /// </summary>
        /// <returns></returns>
        public static string[] GetPresetAppPackageNames()
        {
            return PackageManager.GetPresetAppPackageNames();
        }

        /// <summary>
        /// Get preset dlc package name list from PatchLauncher
        /// </summary>
        /// <returns></returns>
        public static string[] GetPresetDlcPackageNames()
        {
            return PackageManager.GetPresetDlcPackageNames();
        }

        /// <summary>
        /// Get preset dlc package info list from PatchLauncher
        /// </summary>
        /// <returns></returns>
        public static DlcPackageInfoWithBuild[] GetPresetDlcPackageInfos()
        {
            return PackageManager.GetPresetDlcPackageInfos();
        }
        #endregion

        #region AssetInfo
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
        #endregion

        #region Downloader
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

        #region Combine Donwloaders
        public static ResourceDownloaderOperation[] GetDownloadersWithCombinePackages(ResourcePackage[] packages)
        {
            List<ResourceDownloaderOperation> downloaders = new List<ResourceDownloaderOperation>();
            foreach (var package in packages)
            {
                downloaders.Add(GetPackageDownloader(package));
            }

            return downloaders.ToArray();
        }

        public static ResourceDownloaderOperation[] GetDownloadersWithCombinePackages(ResourcePackage[] packages, int maxConcurrencyDownloadCount, int failedRetryCount)
        {
            List<ResourceDownloaderOperation> downloaders = new List<ResourceDownloaderOperation>();
            foreach (var package in packages)
            {
                downloaders.Add(GetPackageDownloader(package, maxConcurrencyDownloadCount, failedRetryCount));
            }

            return downloaders.ToArray();
        }

        public static ResourceDownloaderOperation[] GetDownloadersWithCombinePackagesByTags(ResourcePackage[] packages, params string[] tags)
        {
            List<ResourceDownloaderOperation> downloaders = new List<ResourceDownloaderOperation>();
            foreach (var package in packages)
            {
                downloaders.Add(GetPackageDownloaderByTags(package, tags));
            }

            return downloaders.ToArray();
        }

        public static ResourceDownloaderOperation[] GetDownloadersWithCombinePackagesByTags(ResourcePackage[] packages, int maxConcurrencyDownloadCount, int failedRetryCount, params string[] tags)
        {
            List<ResourceDownloaderOperation> downloaders = new List<ResourceDownloaderOperation>();
            foreach (var package in packages)
            {
                downloaders.Add(GetPackageDownloaderByTags(package, maxConcurrencyDownloadCount, failedRetryCount, tags));
            }

            return downloaders.ToArray();
        }

        public static ResourceDownloaderOperation[] GetDownloadersWithCombinePackagesByAssetNames(ResourcePackage[] packages, params string[] assetNames)
        {
            List<ResourceDownloaderOperation> downloaders = new List<ResourceDownloaderOperation>();
            foreach (var package in packages)
            {
                downloaders.Add(GetPackageDownloaderByAssetNames(package, assetNames));
            }

            return downloaders.ToArray();
        }

        public static ResourceDownloaderOperation[] GetDownloadersWithCombinePackagesByAssetNames(ResourcePackage[] packages, int maxConcurrencyDownloadCount, int failedRetryCount, params string[] assetNames)
        {
            List<ResourceDownloaderOperation> downloaders = new List<ResourceDownloaderOperation>();
            foreach (var package in packages)
            {
                downloaders.Add(GetPackageDownloaderByAssetNames(package, maxConcurrencyDownloadCount, failedRetryCount, assetNames));
            }

            return downloaders.ToArray();
        }

        public static ResourceDownloaderOperation[] GetDownloadersWithCombinePackagesByAssetInfos(ResourcePackage[] packages, params AssetInfo[] assetInfos)
        {
            List<ResourceDownloaderOperation> downloaders = new List<ResourceDownloaderOperation>();
            foreach (var package in packages)
            {
                downloaders.Add(GetPackageDownloaderByAssetInfos(package, assetInfos));
            }

            return downloaders.ToArray();
        }

        public static ResourceDownloaderOperation[] GetDownloadersWithCombinePackagesByAssetInfos(ResourcePackage[] packages, int maxConcurrencyDownloadCount, int failedRetryCount, params AssetInfo[] assetInfos)
        {
            List<ResourceDownloaderOperation> downloaders = new List<ResourceDownloaderOperation>();
            foreach (var package in packages)
            {
                downloaders.Add(GetPackageDownloaderByAssetInfos(package, maxConcurrencyDownloadCount, failedRetryCount, assetInfos));
            }

            return downloaders.ToArray();
        }

        public static async UniTask<bool> BeginDownloadWithCombinePackages(ResourcePackage[] packages, OnDownloadSpeedProgress onDownloadSpeedProgress = null, DownloadError onDownloadError = null)
        {
            ResourceDownloaderOperation[] downloaders = GetDownloadersWithCombinePackages(packages);
            return await BeginDownloadWithCombineDownloaders(downloaders, onDownloadSpeedProgress, onDownloadError);
        }

        public static async UniTask<bool> BeginDownloadWithCombinePackages(ResourcePackage[] packages, int maxConcurrencyDownloadCount, int failedRetryCount, OnDownloadSpeedProgress onDownloadSpeedProgress = null, DownloadError onDownloadError = null)
        {
            ResourceDownloaderOperation[] downloaders = GetDownloadersWithCombinePackages(packages, maxConcurrencyDownloadCount, failedRetryCount);
            return await BeginDownloadWithCombineDownloaders(downloaders, onDownloadSpeedProgress, onDownloadError);
        }

        public static async UniTask<bool> BeginDownloadWithCombinePackagesByTags(ResourcePackage[] packages, string[] tags = null, OnDownloadSpeedProgress onDownloadSpeedProgress = null, DownloadError onDownloadError = null)
        {
            ResourceDownloaderOperation[] downloaders = GetDownloadersWithCombinePackagesByTags(packages, tags);
            return await BeginDownloadWithCombineDownloaders(downloaders, onDownloadSpeedProgress, onDownloadError);
        }

        public static async UniTask<bool> BeginDownloadWithCombinePackagesByTags(ResourcePackage[] packages, int maxConcurrencyDownloadCount, int failedRetryCount, string[] tags = null, OnDownloadSpeedProgress onDownloadSpeedProgress = null, DownloadError onDownloadError = null)
        {
            ResourceDownloaderOperation[] downloaders = GetDownloadersWithCombinePackagesByTags(packages, maxConcurrencyDownloadCount, failedRetryCount, tags);
            return await BeginDownloadWithCombineDownloaders(downloaders, onDownloadSpeedProgress, onDownloadError);
        }

        public static async UniTask<bool> BeginDownloadWithCombinePackagesByAssetNames(ResourcePackage[] packages, string[] assetNames = null, OnDownloadSpeedProgress onDownloadSpeedProgress = null, DownloadError onDownloadError = null)
        {
            ResourceDownloaderOperation[] downloaders = GetDownloadersWithCombinePackagesByAssetNames(packages, assetNames);
            return await BeginDownloadWithCombineDownloaders(downloaders, onDownloadSpeedProgress, onDownloadError);
        }

        public static async UniTask<bool> BeginDownloadWithCombinePackagesByAssetNames(ResourcePackage[] packages, int maxConcurrencyDownloadCount, int failedRetryCount, string[] assetNames = null, OnDownloadSpeedProgress onDownloadSpeedProgress = null, DownloadError onDownloadError = null)
        {
            ResourceDownloaderOperation[] downloaders = GetDownloadersWithCombinePackagesByAssetNames(packages, maxConcurrencyDownloadCount, failedRetryCount, assetNames);
            return await BeginDownloadWithCombineDownloaders(downloaders, onDownloadSpeedProgress, onDownloadError);
        }

        public static async UniTask<bool> BeginDownloadWithCombinePackagesByAssetInfos(ResourcePackage[] packages, AssetInfo[] assetInfos = null, OnDownloadSpeedProgress onDownloadSpeedProgress = null, DownloadError onDownloadError = null)
        {
            ResourceDownloaderOperation[] downloaders = GetDownloadersWithCombinePackagesByAssetInfos(packages, assetInfos);
            return await BeginDownloadWithCombineDownloaders(downloaders, onDownloadSpeedProgress, onDownloadError);
        }

        public static async UniTask<bool> BeginDownloadWithCombinePackagesByAssetInfos(ResourcePackage[] packages, int maxConcurrencyDownloadCount, int failedRetryCount, AssetInfo[] assetInfos = null, OnDownloadSpeedProgress onDownloadSpeedProgress = null, DownloadError onDownloadError = null)
        {
            ResourceDownloaderOperation[] downloaders = GetDownloadersWithCombinePackagesByAssetInfos(packages, maxConcurrencyDownloadCount, failedRetryCount, assetInfos);
            return await BeginDownloadWithCombineDownloaders(downloaders, onDownloadSpeedProgress, onDownloadError);
        }

        public static DownloadInfo GetDownloadInfoWithCombinePackages(ResourcePackage[] packages)
        {
            // Get downloaders
            ResourceDownloaderOperation[] downloaders = GetDownloadersWithCombinePackages(packages);

            // Combine all downloaders count and bytes
            int totalCount = 0;
            long totalBytes = 0;
            foreach (var downloader in downloaders)
            {
                if (downloader == null) continue;

                totalCount += downloader.TotalDownloadCount;
                totalBytes += downloader.TotalDownloadBytes;
            }

            DownloadInfo downloadInfo;
            downloadInfo.totalCount = totalCount;
            downloadInfo.totalBytes = (ulong)totalBytes;

            return downloadInfo;
        }

        public static DownloadInfo GetDownloadInfoWithCombinePackagesByTags(ResourcePackage[] packages, params string[] tags)
        {
            // Get downloaders
            ResourceDownloaderOperation[] downloaders = GetDownloadersWithCombinePackagesByTags(packages, tags);

            // Combine all downloaders count and bytes
            int totalCount = 0;
            long totalBytes = 0;
            foreach (var downloader in downloaders)
            {
                if (downloader == null) continue;

                totalCount += downloader.TotalDownloadCount;
                totalBytes += downloader.TotalDownloadBytes;
            }

            DownloadInfo downloadInfo;
            downloadInfo.totalCount = totalCount;
            downloadInfo.totalBytes = (ulong)totalBytes;

            return downloadInfo;
        }

        public static DownloadInfo GetDownloadInfoWithCombinePackagesByAssetNames(ResourcePackage[] packages, params string[] assetNames)
        {
            // Get downloaders
            ResourceDownloaderOperation[] downloaders = GetDownloadersWithCombinePackagesByAssetNames(packages, assetNames);

            // Combine all downloaders count and bytes
            int totalCount = 0;
            long totalBytes = 0;
            foreach (var downloader in downloaders)
            {
                if (downloader == null) continue;

                totalCount += downloader.TotalDownloadCount;
                totalBytes += downloader.TotalDownloadBytes;
            }

            DownloadInfo downloadInfo;
            downloadInfo.totalCount = totalCount;
            downloadInfo.totalBytes = (ulong)totalBytes;

            return downloadInfo;
        }

        public static DownloadInfo GetDownloadInfoWithCombinePackagesByAssetInfos(ResourcePackage[] packages, params AssetInfo[] assetInfos)
        {
            // Get downloaders
            ResourceDownloaderOperation[] downloaders = GetDownloadersWithCombinePackagesByAssetInfos(packages, assetInfos);

            // Combine all downloaders count and bytes
            int totalCount = 0;
            long totalBytes = 0;
            foreach (var downloader in downloaders)
            {
                if (downloader == null) continue;

                totalCount += downloader.TotalDownloadCount;
                totalBytes += downloader.TotalDownloadBytes;
            }

            DownloadInfo downloadInfo;
            downloadInfo.totalCount = totalCount;
            downloadInfo.totalBytes = (ulong)totalBytes;

            return downloadInfo;
        }

        public static async UniTask<bool> BeginDownloadWithCombineDownloaders(ResourceDownloaderOperation[] downloaders, OnDownloadSpeedProgress onDownloadSpeedProgress = null, DownloadError onDownloadError = null)
        {
            // Combine all downloaders count and bytes
            int totalCount = 0;
            long totalBytes = 0;
            foreach (var downloader in downloaders)
            {
                if (downloader == null) continue;

                totalCount += downloader.TotalDownloadCount;
                totalBytes += downloader.TotalDownloadBytes;
            }

            // Check total count
            if (totalCount > 0)
            {
                // Begin Download
                int currentCount = 0;
                long currentBytes = 0;
                var downloadSpeedCalculator = new DownloadSpeedCalculator();
                downloadSpeedCalculator.onDownloadSpeedProgress = onDownloadSpeedProgress;
                foreach (var downloader in downloaders)
                {
                    if (downloader == null) continue;

                    int lastCount = 0;
                    long lastBytes = 0;
                    downloader.DownloadErrorCallback = onDownloadError;
                    downloader.DownloadUpdateCallback =
                    (DownloadUpdateData data) =>
                    {
                        currentCount += data.CurrentDownloadCount - lastCount;
                        lastCount = data.CurrentDownloadCount;
                        currentBytes += data.CurrentDownloadBytes - lastBytes;
                        lastBytes = data.CurrentDownloadBytes;
                        downloadSpeedCalculator.OnDownloadProgress(totalCount, currentCount, totalBytes, currentBytes);
                    };
                    downloader.BeginDownload();

                    await downloader;

                    if (downloader.Status != EOperationStatus.Succeed) return false;
                }
            }

            return true;
        }
        #endregion
        #endregion
        #endregion

        /// <summary>
        /// Release all packages and bundle settings for quit game
        /// </summary>
        public async static UniTask Release()
        {
            await PackageManager.Release();
            BundleConfig.Release();
        }
    }
}