using YooAsset;

#if !YOOASSET_2 && !YOOASSET_3
#error OxGFrame requires YooAsset 2.x (verified with 2.3.18) or YooAsset 3.0.5+ (3.0.0 ~ 3.0.4 are not supported). Please install a supported YooAsset version via UPM.
#endif

namespace OxGFrame.AssetLoader.Bundle
{
    /// <summary>
    /// YooAsset 跨版本 (v2 / v3) 相容輔助擴充
    /// </summary>
    internal static class YooAssetCompatExtensions
    {
        /// <summary>
        /// 檢查操作狀態是否成功 (v2: Succeed, v3: Succeeded)
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        internal static bool IsSucceed(this EOperationStatus status)
        {
#if YOOASSET_2
            return status == EOperationStatus.Succeed;
#else
            return status == EOperationStatus.Succeeded;
#endif
        }
    }

#if YOOASSET_3
    /// <summary>
    /// [YooAsset v3] 以 v2 同名 API 提供的轉接擴充，讓呼叫端維持 v2 寫法 (v2 環境下由原生實例方法優先取用，不會進入此處)
    /// </summary>
    internal static class YooAssetV3Polyfills
    {
        /// <summary>
        /// 預設的清單載入逾時秒數 (對齊 v2 UpdatePackageManifestAsync 的預設值)
        /// </summary>
        private const int _DEFAULT_TIMEOUT_SECONDS = 60;

        /// <summary>
        /// v2: CheckLocationValid -> v3: IsLocationValid
        /// </summary>
        /// <param name="package"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        internal static bool CheckLocationValid(this ResourcePackage package, string location)
        {
            return package.IsLocationValid(location);
        }

        /// <summary>
        /// v2: SceneHandle.UnSuspend -> v3: SceneHandle.AllowSceneActivation
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        internal static bool UnSuspend(this SceneHandle handle)
        {
            return handle.AllowSceneActivation();
        }

        /// <summary>
        /// v2: SceneHandle.UnloadAsync -> v3: SceneHandle.UnloadSceneAsync
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        internal static UnloadSceneOperation UnloadAsync(this SceneHandle handle)
        {
            return handle.UnloadSceneAsync();
        }

        /// <summary>
        /// v2: ResourcePackage.DestroyAsync -> v3: ResourcePackage.DestroyPackageAsync
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        internal static DestroyPackageOperation DestroyAsync(this ResourcePackage package)
        {
            return package.DestroyPackageAsync();
        }

        /// <summary>
        /// v2: ResourcePackage.UpdatePackageManifestAsync -> v3: ResourcePackage.LoadPackageManifestAsync
        /// </summary>
        /// <param name="package"></param>
        /// <param name="packageVersion"></param>
        /// <returns></returns>
        internal static LoadPackageManifestOperation UpdatePackageManifestAsync(this ResourcePackage package, string packageVersion)
        {
            var options = new LoadPackageManifestOptions(packageVersion, _DEFAULT_TIMEOUT_SECONDS);
            return package.LoadPackageManifestAsync(options);
        }

        /// <summary>
        /// v2: ResourcePackage.CreateResourceDownloader (All) -> v3: Options 版本
        /// </summary>
        /// <param name="package"></param>
        /// <param name="downloadingMaxNumber"></param>
        /// <param name="failedTryAgain"></param>
        /// <returns></returns>
        internal static ResourceDownloaderOperation CreateResourceDownloader(this ResourcePackage package, int downloadingMaxNumber, int failedTryAgain)
        {
            var options = new ResourceDownloaderOptions(downloadingMaxNumber, failedTryAgain);
            return package.CreateResourceDownloader(options);
        }

        /// <summary>
        /// v2: ResourcePackage.CreateResourceDownloader (Tags) -> v3: Options 版本
        /// </summary>
        /// <param name="package"></param>
        /// <param name="tags"></param>
        /// <param name="downloadingMaxNumber"></param>
        /// <param name="failedTryAgain"></param>
        /// <returns></returns>
        internal static ResourceDownloaderOperation CreateResourceDownloader(this ResourcePackage package, string[] tags, int downloadingMaxNumber, int failedTryAgain)
        {
            var options = new ResourceDownloaderOptions(tags, downloadingMaxNumber, failedTryAgain);
            return package.CreateResourceDownloader(options);
        }

        /// <summary>
        /// v2: ResourcePackage.CreateBundleDownloader (AssetNames) -> v3: Options 版本 (含依賴下載, 對齊 v2 行為)
        /// </summary>
        /// <param name="package"></param>
        /// <param name="assetNames"></param>
        /// <param name="downloadingMaxNumber"></param>
        /// <param name="failedTryAgain"></param>
        /// <returns></returns>
        internal static ResourceDownloaderOperation CreateBundleDownloader(this ResourcePackage package, string[] assetNames, int downloadingMaxNumber, int failedTryAgain)
        {
            var assetInfos = new AssetInfo[assetNames.Length];
            for (int i = 0; i < assetNames.Length; i++)
                assetInfos[i] = package.GetAssetInfo(assetNames[i]);
            return package.CreateBundleDownloader(assetInfos, downloadingMaxNumber, failedTryAgain);
        }

        /// <summary>
        /// v2: ResourcePackage.CreateBundleDownloader (AssetInfos) -> v3: Options 版本 (含依賴下載, 對齊 v2 行為)
        /// </summary>
        /// <param name="package"></param>
        /// <param name="assetInfos"></param>
        /// <param name="downloadingMaxNumber"></param>
        /// <param name="failedTryAgain"></param>
        /// <returns></returns>
        internal static ResourceDownloaderOperation CreateBundleDownloader(this ResourcePackage package, AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain)
        {
            var options = new BundleDownloaderOptions(assetInfos, true, downloadingMaxNumber, failedTryAgain);
            return package.CreateResourceDownloader(options);
        }

        /// <summary>
        /// v2: ResourcePackage.UnloadUnusedAssetsAsync (loopCount) -> v3: Options 版本
        /// </summary>
        /// <param name="package"></param>
        /// <param name="loopCount"></param>
        /// <returns></returns>
        internal static UnloadUnusedAssetsOperation UnloadUnusedAssetsAsync(this ResourcePackage package, int loopCount)
        {
            var options = new UnloadUnusedAssetsOptions(loopCount);
            return package.UnloadUnusedAssetsAsync(options);
        }

        /// <summary>
        /// v2: AsyncOperationBase.WaitForAsyncComplete -> v3: AsyncOperationBase.WaitForCompletion
        /// </summary>
        /// <param name="operation"></param>
        internal static void WaitForAsyncComplete(this AsyncOperationBase operation)
        {
            operation.WaitForCompletion();
        }

        /// <summary>
        /// v2: DownloaderOperation.BeginDownload -> v3: DownloaderOperation.StartDownload
        /// </summary>
        /// <param name="downloader"></param>
        internal static void BeginDownload(this DownloaderOperation downloader)
        {
            downloader.StartDownload();
        }

        /// <summary>
        /// v2: ResourcePackage.LoadRawFileAsync -> v3: 以 AssetHandle 載入 RawFileObject
        /// </summary>
        /// <param name="package"></param>
        /// <param name="location"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        internal static AssetHandle LoadRawFileAsync(this ResourcePackage package, string location, uint priority = 0)
        {
            return package.LoadAssetAsync<RawFileObject>(location, priority);
        }

        /// <summary>
        /// v2: ResourcePackage.LoadRawFileSync -> v3: 以 AssetHandle 載入 RawFileObject
        /// </summary>
        /// <param name="package"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        internal static AssetHandle LoadRawFileSync(this ResourcePackage package, string location)
        {
            return package.LoadAssetSync<RawFileObject>(location);
        }
    }
#endif
}
