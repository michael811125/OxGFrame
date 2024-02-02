using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using OxGFrame.AssetLoader.Bundle;
using OxGFrame.AssetLoader.PatchEvent;
using OxGFrame.AssetLoader.Utility;
using OxGKit.LoggingSystem;
using System;
using System.Collections.Generic;
using UniFramework.Machine;
using YooAsset;

namespace OxGFrame.AssetLoader.PatchFsm
{
    public static class PackageFsmStates
    {
        /// <summary>
        /// 0. 修復流程
        /// </summary>
        public class FsmPatchRepair : IStateNode
        {
            private StateMachine _machine;
            private int _hashId;
            private int _retryCount = _RETRY_COUNT;

            private const int _RETRY_COUNT = 1;

            public FsmPatchRepair() { }

            void IStateNode.OnCreate(StateMachine machine)
            {
                this._machine = machine;
                this._hashId = (this._machine.Owner as PackageOperation).hashId;
            }

            void IStateNode.OnEnter()
            {
                // 流程準備
                PackageEvents.PatchFsmState.SendEventMessage(this._hashId, this);
                (this._machine.Owner as PackageOperation).MarkRepairState();
                this._DeleteLocalSaveFiles().Forget();
            }

            void IStateNode.OnUpdate()
            {
            }

            void IStateNode.OnExit()
            {
            }

            private async UniTask _DeleteLocalSaveFiles()
            {
                // EditorSimulateMode skip repair
                if (BundleConfig.playMode == BundleConfig.PlayMode.EditorSimulateMode)
                {
                    this._machine.ChangeState<FsmPatchPrepare>();
                    return;
                }

                // Cancel main download first
                (this._machine.Owner as PackageOperation).Cancel(false);

                // Wait a frame
                await UniTask.NextFrame();

                // Get package names
                var packageNames = (this._machine.Owner as PackageOperation).GetPackageNames();

                bool isCleared = false;
                foreach (var packageName in packageNames)
                {
                    // Clear cache and files of package
                    isCleared = await PackageManager.UnloadPackageAndClearCacheFiles(packageName);
                    if (!isCleared) break;
                }

                if (isCleared || this._retryCount <= 0)
                {
                    this._retryCount = _RETRY_COUNT;
                    this._machine.ChangeState<FsmPatchPrepare>();
                }
                else
                {
                    this._retryCount--;
                    PackageEvents.PatchRepairFailed.SendEventMessage(this._hashId);
                }
            }
        }

        /// <summary>
        /// 1. 流程準備工作
        /// </summary>
        public class FsmPatchPrepare : IStateNode
        {
            private StateMachine _machine;
            private int _hashId;

            public FsmPatchPrepare() { }

            void IStateNode.OnCreate(StateMachine machine)
            {
                this._machine = machine;
                this._hashId = (this._machine.Owner as PackageOperation).hashId;
            }

            void IStateNode.OnEnter()
            {
                // 流程準備
                PackageEvents.PatchFsmState.SendEventMessage(this._hashId, this);
                (this._machine.Owner as PackageOperation).MarkReadyAsDone();
                this._machine.ChangeState<FsmInitPatchMode>();
            }

            void IStateNode.OnUpdate()
            {
            }

            void IStateNode.OnExit()
            {
            }
        }

        /// <summary>
        /// 2. 初始 Patch Mode
        /// </summary>
        public class FsmInitPatchMode : IStateNode
        {
            private StateMachine _machine;
            private int _hashId;

            public FsmInitPatchMode() { }

            void IStateNode.OnCreate(StateMachine machine)
            {
                this._machine = machine;
                this._hashId = (this._machine.Owner as PackageOperation).hashId;
            }

            void IStateNode.OnEnter()
            {
                // 初始更新資源配置
                PackageEvents.PatchFsmState.SendEventMessage(this._hashId, this);
                this._InitPatchMode().Forget();
            }

            void IStateNode.OnUpdate()
            {
            }

            void IStateNode.OnExit()
            {
            }

            private async UniTask _InitPatchMode()
            {
                await UniTask.Delay(TimeSpan.FromSeconds(0.1f), true);

                var packageInfos = (this._machine.Owner as PackageOperation).GetPackageInfos();
                if (packageInfos != null)
                {
                    bool isInitialized = false;
                    foreach (var packageInfo in packageInfos)
                    {
                        string hostServer;
                        string fallbackHostServer;
                        IBuildinQueryServices builtinQueryService;
                        IDeliveryQueryServices deliveryQueryService;
                        IDeliveryLoadServices deliveryLoadService;

                        if (packageInfo is DlcPackageInfoWithBuild)
                        {
                            var packageDetail = packageInfo as DlcPackageInfoWithBuild;
                            hostServer = packageDetail.hostServer;
                            fallbackHostServer = packageDetail.fallbackHostServer;
                            builtinQueryService = packageDetail.builtinQueryService;
                            deliveryQueryService = packageDetail.deliveryQueryService;
                            deliveryLoadService = packageDetail.deliveryLoadService;

                            // Host Mode or WebGL Mode
                            if (BundleConfig.playMode == BundleConfig.PlayMode.HostMode ||
                                BundleConfig.playMode == BundleConfig.PlayMode.WebGLMode)
                            {
                                hostServer = string.IsNullOrEmpty(hostServer) ? await BundleConfig.GetDlcHostServerUrl(packageDetail.packageName, packageDetail.dlcVersion, packageDetail.withoutPlatform) : hostServer;
                                fallbackHostServer = string.IsNullOrEmpty(fallbackHostServer) ? await BundleConfig.GetDlcFallbackHostServerUrl(packageDetail.packageName, packageDetail.dlcVersion, packageDetail.withoutPlatform) : fallbackHostServer;
                                builtinQueryService = builtinQueryService == null ? new RequestBuiltinQuery() : builtinQueryService;
                                deliveryQueryService = deliveryQueryService == null ? new RequestDeliveryQuery() : deliveryQueryService;
                                deliveryLoadService = deliveryLoadService == null ? new RequestDeliveryQuery() : deliveryLoadService;
                            }
                        }
                        else if (packageInfo is AppPackageInfoWithBuild)
                        {
                            var packageDetail = packageInfo as AppPackageInfoWithBuild;
                            hostServer = null;
                            fallbackHostServer = null;
                            builtinQueryService = null;
                            deliveryQueryService = null;
                            deliveryLoadService = null;

                            // Host Mode or WebGL Mode
                            if (BundleConfig.playMode == BundleConfig.PlayMode.HostMode ||
                                BundleConfig.playMode == BundleConfig.PlayMode.WebGLMode)
                            {
                                hostServer = await BundleConfig.GetHostServerUrl(packageDetail.packageName);
                                fallbackHostServer = await BundleConfig.GetFallbackHostServerUrl(packageDetail.packageName);
                                builtinQueryService = new RequestBuiltinQuery();
                                deliveryQueryService = new RequestDeliveryQuery();
                                deliveryLoadService = new RequestDeliveryQuery();
                            }
                        }
                        else throw new Exception("Package info type error.");

                        isInitialized = await PackageManager.InitPackage(packageInfo, false, hostServer, fallbackHostServer, builtinQueryService, deliveryQueryService, deliveryLoadService);
                        if (!isInitialized)
                        {
                            PackageEvents.PatchInitPatchModeFailed.SendEventMessage(this._hashId);
                            break;
                        }
                    }

                    if (isInitialized)
                    {
                        this._machine.ChangeState<FsmPatchVersionUpdate>();
                        Logging.Print<Logger>("<color=#ffcf67>(Init) Init Patch</color>");
                    }
                    else
                    {
                        PackageEvents.PatchInitPatchModeFailed.SendEventMessage(this._hashId);
                    }
                }
                else throw new Exception("Cannot get package infos (NULL).");
            }
        }

        /// <summary>
        /// 3. 更新 Patch Version
        /// </summary>
        public class FsmPatchVersionUpdate : IStateNode
        {
            private StateMachine _machine;
            private int _hashId;

            public FsmPatchVersionUpdate() { }

            void IStateNode.OnCreate(StateMachine machine)
            {
                this._machine = machine;
                this._hashId = (this._machine.Owner as PackageOperation).hashId;
            }

            void IStateNode.OnEnter()
            {
                // 獲取最新的資源版本
                PackageEvents.PatchFsmState.SendEventMessage(this._hashId, this);
                (this._machine.Owner as PackageOperation).MarkRepairAsDone();
                this._UpdatePatchVersion().Forget();
            }

            void IStateNode.OnUpdate()
            {
            }

            void IStateNode.OnExit()
            {
            }

            private async UniTask _UpdatePatchVersion()
            {
                await UniTask.Delay(TimeSpan.FromSeconds(0.1f), true);

                // Get packages
                var packages = (this._machine.Owner as PackageOperation).GetPackages();

                bool succeed = false;
                List<string> patchVersions = new List<string>();
                foreach (var package in packages)
                {
                    var operation = package.UpdatePackageVersionAsync();
                    await operation;

                    if (operation.Status == EOperationStatus.Succeed)
                    {
                        succeed = true;
                        patchVersions.Add(operation.PackageVersion);
                    }
                    else
                    {
                        succeed = false;
                        PackageEvents.PatchVersionUpdateFailed.SendEventMessage(this._hashId);
                        Logging.Print<Logger>($"<color=#ff3696>Package: {package.PackageName} update version failed.</color>");
                        break;
                    }
                }

                if (succeed)
                {
                    this._machine.SetBlackboardValue(PackageOperation.KEY_PACKAGE_VERSIONS, patchVersions.ToArray());
                    this._machine.ChangeState<FsmPatchManifestUpdate>();
                }
            }
        }

        /// <summary>
        /// 4. 更新 Patch Manifest
        /// </summary>
        public class FsmPatchManifestUpdate : IStateNode
        {
            private StateMachine _machine;
            private int _hashId;

            public FsmPatchManifestUpdate() { }

            void IStateNode.OnCreate(StateMachine machine)
            {
                this._machine = machine;
                this._hashId = (this._machine.Owner as PackageOperation).hashId;
            }

            void IStateNode.OnEnter()
            {
                // 更新資源清單
                PackageEvents.PatchFsmState.SendEventMessage(this._hashId, this);
                this._UpdatePatchManifest().Forget();
            }

            void IStateNode.OnUpdate()
            {
            }

            void IStateNode.OnExit()
            {
            }

            private async UniTask _UpdatePatchManifest()
            {
                await UniTask.Delay(TimeSpan.FromSeconds(0.1f), true);

                // Get packages
                var packages = (this._machine.Owner as PackageOperation).GetPackages();
                var packageVersions = (string[])this._machine.GetBlackboardValue(PackageOperation.KEY_PACKAGE_VERSIONS);

                bool succeed = false;
                for (int i = 0; i < packages.Length; i++)
                {
                    var operation = packages[i].UpdatePackageManifestAsync(packageVersions[i]);
                    await operation;

                    if (operation.Status == EOperationStatus.Succeed)
                    {
                        succeed = true;
                        operation.SavePackageVersion();
                        Logging.Print<Logger>($"<color=#85cf0f>Package: {packages[i].PackageName} <color=#00c1ff>Update</color> completed successfully.</color>");
                    }
                    else
                    {
                        succeed = false;
                        PackageEvents.PatchManifestUpdateFailed.SendEventMessage(this._hashId);
                        Logging.Print<Logger>($"<color=#ff3696>Package: {packages[i].PackageName} update manifest failed.</color>");
                        break;
                    }
                }

                if (succeed) this._machine.ChangeState<FsmCreateDownloader>();
            }
        }

        /// <summary>
        /// 5. 創建資源下載器
        /// </summary>
        public class FsmCreateDownloader : IStateNode
        {
            private StateMachine _machine;
            private int _hashId;

            public FsmCreateDownloader() { }

            void IStateNode.OnCreate(StateMachine machine)
            {
                this._machine = machine;
                this._hashId = (this._machine.Owner as PackageOperation).hashId;
            }

            void IStateNode.OnEnter()
            {
                // 創建資源下載器
                PackageEvents.PatchFsmState.SendEventMessage(this._hashId, this);
                this._CreateDownloader();
                (this._machine.Owner as PackageOperation).MarkReadyState();
            }

            void IStateNode.OnUpdate()
            {
            }

            void IStateNode.OnExit()
            {
            }

            private void _CreateDownloader()
            {
                // EditorSimulateMode or OfflineMode skip directly
                if (BundleConfig.playMode == BundleConfig.PlayMode.EditorSimulateMode ||
                    BundleConfig.playMode == BundleConfig.PlayMode.OfflineMode)
                {
                    this._machine.ChangeState<FsmPatchDone>();
                    return;
                }

                #region Create Downloader by Tags
                // Get packages
                var packages = (this._machine.Owner as PackageOperation).GetPackages();
                var groupInfo = (this._machine.Owner as PackageOperation).groupInfo;

                // Reset group info
                groupInfo.totalCount = 0;
                groupInfo.totalBytes = 0;

                int totalDownloadCount;
                long totalDownloadBytes;
                for (int i = 0; i < packages.Length; i++)
                {
                    var package = packages[i];

                    // all package
                    if (groupInfo.tags == null || (groupInfo.tags != null && groupInfo.tags.Length == 0))
                    {
                        var downloader = package.CreateResourceDownloader(BundleConfig.maxConcurrencyDownloadCount, BundleConfig.failedRetryCount);
                        totalDownloadCount = downloader.TotalDownloadCount;
                        totalDownloadBytes = downloader.TotalDownloadBytes;
                        if (totalDownloadCount > 0)
                        {
                            groupInfo.totalCount += totalDownloadCount;
                            groupInfo.totalBytes += totalDownloadBytes;
                        }
                    }
                    // package by tags
                    else
                    {
                        var downloader = package.CreateResourceDownloader(groupInfo.tags, BundleConfig.maxConcurrencyDownloadCount, BundleConfig.failedRetryCount);
                        totalDownloadCount = downloader.TotalDownloadCount;
                        totalDownloadBytes = downloader.TotalDownloadBytes;
                        if (totalDownloadCount > 0)
                        {
                            groupInfo.totalCount += totalDownloadCount;
                            groupInfo.totalBytes += totalDownloadBytes;
                        }
                    }
                }
                #endregion

                if (groupInfo.totalCount > 0)
                {
                    bool skipDownload = (this._machine.Owner as PackageOperation).skipDownload;
                    if (skipDownload) this._machine.ChangeState<FsmDownloadOver>();
                    else if ((this._machine.Owner as PackageOperation).IsBegin()) this._machine.ChangeState<FsmBeginDownload>();

                    // 開始等待使用者選擇是否開始下載
                }
                else
                {
                    Logging.Print<Logger>($"<color=#54ff75><color=#ffce54>GroupName: {groupInfo.groupName}</color> not found any download files!!!</color>");
                    this._machine.ChangeState<FsmDownloadOver>();
                }
            }
        }

        /// <summary>
        /// 6. 下載資源檔案
        /// </summary>
        public class FsmBeginDownload : IStateNode
        {
            private StateMachine _machine;
            private int _hashId;

            public FsmBeginDownload() { }

            void IStateNode.OnCreate(StateMachine machine)
            {
                this._machine = machine;
                this._hashId = (this._machine.Owner as PackageOperation).hashId;
            }

            void IStateNode.OnEnter()
            {
                // 下載資源檔案中
                PackageEvents.PatchFsmState.SendEventMessage(this._hashId, this);
                (this._machine.Owner as PackageOperation).MarkBeginState();
                this._StartDownload().Forget();
            }

            void IStateNode.OnUpdate()
            {
            }

            void IStateNode.OnExit()
            {
            }

            private async UniTask _StartDownload()
            {
                // Get packages
                var packages = (this._machine.Owner as PackageOperation).GetPackages();

                // Get groupInfo
                GroupInfo groupInfo = (this._machine.Owner as PackageOperation).groupInfo;

                Logging.Print<Logger>($"<color=#54ffad>Start Download Group Name: {groupInfo?.groupName}, Tags: {JsonConvert.SerializeObject(groupInfo?.tags)}</color>");

                List<ResourceDownloaderOperation> downloaders = new List<ResourceDownloaderOperation>();
                foreach (var package in packages)
                {
                    if (groupInfo != null)
                    {
                        if (groupInfo.tags != null && groupInfo.tags.Length > 0) downloaders.Add(package.CreateResourceDownloader(groupInfo.tags, BundleConfig.maxConcurrencyDownloadCount, BundleConfig.failedRetryCount));
                        else downloaders.Add(package.CreateResourceDownloader(BundleConfig.maxConcurrencyDownloadCount, BundleConfig.failedRetryCount));
                    }
                }

                // Set downloaders
                (this._machine.Owner as PackageOperation).SetDownloaders(downloaders.ToArray());

                // Combine all downloaders count and bytes
                int totalCount = 0;
                long totalBytes = 0;
                foreach (var downloader in downloaders)
                {
                    totalCount += downloader.TotalDownloadCount;
                    totalBytes += downloader.TotalDownloadBytes;
                }

#if !UNITY_WEBGL
                // Check flag if enabled
                if ((this._machine.Owner as PackageOperation).checkDiskSpace)
                {
                    // Check disk space
                    int availableDiskSpaceMegabytes = BundleUtility.CheckAvailableDiskSpaceMegabytes();
                    int patchTotalMegabytes = (int)(totalBytes / (1 << 20));
                    Logging.Print<Logger>($"<color=#2cff96>[Disk Space Check] Available Disk Space Size: {BundleUtility.GetMegabytesToString(availableDiskSpaceMegabytes)}</color>, <color=#2cbbff>Patch Total Size: {BundleUtility.GetBytesToString((ulong)totalBytes)}</color>");
                    if (patchTotalMegabytes > availableDiskSpaceMegabytes)
                    {
                        PackageEvents.PatchCheckDiskNotEnoughSpace.SendEventMessage(availableDiskSpaceMegabytes, (ulong)totalBytes);
                        Logging.Print<Logger>($"<color=#ff2c48>Disk Not Enough Space!!! Available Disk Space Size: {BundleUtility.GetMegabytesToString(availableDiskSpaceMegabytes)}</color>, <color=#2cbbff>Patch Total Size: {BundleUtility.GetBytesToString((ulong)totalBytes)}</color>");
                        return;
                    }
                }
#endif

                // Begin Download
                int currentCount = 0;
                long currentBytes = 0;
                var downloadSpeedCalculator = new DownloadSpeedCalculator();
                downloadSpeedCalculator.onDownloadSpeedProgress = (
                    int totalDownloadCount,
                    int currentDownloadCount,
                    long totalDownloadBytes,
                    long currentDownloadBytes,
                    long downloadSpeedBytes) =>
                {
                    PackageEvents.PatchDownloadProgression.SendEventMessage(
                        this._hashId,
                        totalDownloadCount,
                        currentDownloadCount,
                        totalDownloadBytes,
                        currentDownloadBytes,
                        downloadSpeedBytes);
                };
                foreach (var downloader in downloaders)
                {
                    int lastCount = 0;
                    long lastBytes = 0;
                    downloader.OnDownloadErrorCallback = (
                        string fileName,
                        string error) =>
                    {
                        PackageEvents.PatchDownloadFailed.SendEventMessage(
                            this._hashId,
                            fileName,
                            error);
                    };
                    downloader.OnDownloadProgressCallback =
                    (
                        int totalDownloadCount,
                        int currentDownloadCount,
                        long totalDownloadBytes,
                        long currentDownloadBytes) =>
                    {
                        currentCount += currentDownloadCount - lastCount;
                        lastCount = currentDownloadCount;
                        currentBytes += currentDownloadBytes - lastBytes;
                        lastBytes = currentDownloadBytes;
                        downloadSpeedCalculator.OnDownloadProgress(totalCount, currentCount, totalBytes, currentBytes);
                    };

                    downloader.BeginDownload();
                    await downloader;

                    if (downloader.Status != EOperationStatus.Succeed) return;
                }

                this._machine.ChangeState<FsmDownloadOver>();
            }
        }

        /// <summary>
        /// 7. 資源下載完成
        /// </summary>
        public class FsmDownloadOver : IStateNode
        {
            private StateMachine _machine;
            private int _hashId;

            public FsmDownloadOver() { }

            void IStateNode.OnCreate(StateMachine machine)
            {
                this._machine = machine;
                this._hashId = (this._machine.Owner as PackageOperation).hashId;
            }

            void IStateNode.OnEnter()
            {
                // 資源下載完成
                PackageEvents.PatchFsmState.SendEventMessage(this._hashId, this);
                this._DownloadOver().Forget();
            }

            void IStateNode.OnUpdate()
            {
            }

            void IStateNode.OnExit()
            {
            }

            private async UniTask _DownloadOver()
            {
                await UniTask.Delay(TimeSpan.FromSeconds(0.1f), true);

                // Get packages
                var packages = (this._machine.Owner as PackageOperation).GetPackages();
                var groupInfo = (this._machine.Owner as PackageOperation).groupInfo;

                int packageTotalCount = 0;
                ulong packageTotalSize = 0;
                for (int i = 0; i < packages.Length; i++)
                {
                    var package = packages[i];
                    packageTotalSize += AssetPatcher.GetPackageSizeInLocal(package.PackageName);
                    packageTotalCount += 1;
                }

                // Set packages total size
                groupInfo.totalCount = packageTotalCount;
                groupInfo.totalBytes = (long)packageTotalSize;

                this._machine.ChangeState<FsmClearCache>();
            }
        }

        /// <summary>
        /// 8. 清理未使用的緩存文件
        /// </summary>
        public class FsmClearCache : IStateNode
        {
            private StateMachine _machine;
            private int _hashId;

            public FsmClearCache() { }

            void IStateNode.OnCreate(StateMachine machine)
            {
                this._machine = machine;
                this._hashId = (this._machine.Owner as PackageOperation).hashId;
            }

            void IStateNode.OnEnter()
            {
                // 清理未使用的緩存文件
                PackageEvents.PatchFsmState.SendEventMessage(this._hashId, this);
                this._ClearUnusedCache().Forget();
            }

            void IStateNode.OnUpdate()
            {
            }

            void IStateNode.OnExit()
            {
            }

            private async UniTask _ClearUnusedCache()
            {
                await UniTask.Delay(TimeSpan.FromSeconds(0.1f), true);

                // Get packages
                var packages = (this._machine.Owner as PackageOperation).GetPackages();

                foreach (var package in packages)
                {
                    var operation = package.ClearUnusedCacheFilesAsync();
                    await operation;
                }

                this._machine.ChangeState<FsmPatchDone>();
            }
        }

        /// <summary>
        /// 9. 更新完畢
        /// </summary>
        public class FsmPatchDone : IStateNode
        {
            private StateMachine _machine;
            private int _hashId;

            public FsmPatchDone() { }

            void IStateNode.OnCreate(StateMachine machine)
            {
                this._machine = machine;
                this._hashId = (this._machine.Owner as PackageOperation).hashId;
            }

            void IStateNode.OnEnter()
            {
                // 更新完畢
                PackageEvents.PatchFsmState.SendEventMessage(this._hashId, this);
                (this._machine.Owner as PackageOperation).MarkPatchAsDone();
            }

            void IStateNode.OnUpdate()
            {
            }

            void IStateNode.OnExit()
            {
            }
        }
    }
}
