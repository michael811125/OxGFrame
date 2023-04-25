using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader.Bundle;
using YooAsset;

namespace OxGFrame.AssetLoader
{
    public static class AssetPatcher
    {
        #region Other
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
        /// Start to check patch update
        /// </summary>
        public static void Check()
        {
            PatchManager.GetInstance().Check();
        }

        /// <summary>
        /// Start to run patch repair
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
        /// Get app version
        /// </summary>
        /// <returns></returns>
        public static string GetAppVersion()
        {
            return PatchManager.appVersion;
        }

        /// <summary>
        /// Get patch version
        /// </summary>
        /// <returns></returns>
        public static string GetPatchVersion()
        {
            return PatchManager.patchVersion;
        }
        #endregion

        #region Package Operation
        /// <summary>
        /// Set default pacakge. If is not exist will auto register and set it be default
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
        /// Get package name by idx
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public static string GetPackageNameByIdx(int idx)
        {
            return PackageManager.GetPackageNameByIdx(idx);
        }

        /// <summary>
        /// Get downloader from package with tags
        /// </summary>
        /// <param name="pacakge"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static ResourceDownloaderOperation GetPacakgeDownloaderByTags(ResourcePackage pacakge, params string[] tags)
        {
            return PackageManager.GetPacakgeDownloaderByTags(pacakge, tags);
        }
        #endregion
    }
}