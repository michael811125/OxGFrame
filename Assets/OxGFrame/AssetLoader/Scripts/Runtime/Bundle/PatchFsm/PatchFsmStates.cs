using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using OxGFrame.AssetLoader.Bundle;
using OxGFrame.AssetLoader.PatchEvent;
using OxGFrame.AssetLoader.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using UniFramework.Machine;
using UnityEngine;
using YooAsset;

namespace OxGFrame.AssetLoader.PatchFsm
{
    public static class PatchFsmStates
    {
        /// <summary>
        /// 0. 修復流程
        /// </summary>
        public class FsmPatchRepair : IStateNode
        {
            private StateMachine _machine;

            void IStateNode.OnCreate(StateMachine machine)
            {
                this._machine = machine;
            }

            void IStateNode.OnEnter()
            {
                // 流程準備
                PatchEvents.PatchFsmState.SendEventMessage(this);
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
                // 刪除 Last Group Info 記錄
                PatchManager.DelLastGroupInfo();

                // 取得本地持久化路徑
                var dir = BundleConfig.GetLocalSandboxPath();

                // 刪除資源數據
                BundleUtility.DeleteFolder(dir);

                // 建立目錄
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                await UniTask.Delay(TimeSpan.FromSeconds(1), true);

                // 判斷檢查目錄是否為空 (表示數據已完成清除)
                if (BundleUtility.GetFilesRecursively(dir).Length <= 0)
                {
                    this._machine.ChangeState<FsmPatchPrepare>();
                }
            }
        }

        /// <summary>
        /// 1. 流程準備工作
        /// </summary>
        public class FsmPatchPrepare : IStateNode
        {
            private StateMachine _machine;

            void IStateNode.OnCreate(StateMachine machine)
            {
                this._machine = machine;
            }

            void IStateNode.OnEnter()
            {
                // 流程準備
                PatchEvents.PatchFsmState.SendEventMessage(this);
                this._machine.ChangeState<FsmAppVersionUpdate>();
            }

            void IStateNode.OnUpdate()
            {
            }

            void IStateNode.OnExit()
            {
            }
        }

        /// <summary>
        /// 2. 比對 App Version
        /// </summary>
        public class FsmAppVersionUpdate : IStateNode
        {
            private StateMachine _machine;

            void IStateNode.OnCreate(StateMachine machine)
            {
                this._machine = machine;
            }

            public void OnEnter()
            {
                // 比對版本
                PatchEvents.PatchFsmState.SendEventMessage(this);
                this._AppConfigRequest().Forget();
            }

            public void OnExit()
            {
            }

            public void OnUpdate()
            {
            }

            private async UniTask _AppConfigRequest()
            {
                // EditorSimulateMode skip app version comparison directly
                if (BundleConfig.playMode == BundleConfig.PlayMode.EditorSimulateMode)
                {
                    this._machine.ChangeState<FsmInitPatchMode>();
                    return;
                }

                string saCfgPath = BundleConfig.GetStreamingAssetsAppConfigPath();

                string url;

                // 判斷是否離線版, 如果是離線版則請求 StreamingAssets 中的 Cfg
                if (BundleConfig.playMode == BundleConfig.PlayMode.OfflineMode) url = saCfgPath;
                // 反之, 請求 Server 的 Cfg
                else url = await BundleConfig.GetHostServerAppConfigPath();

                string hostCfgJson = await BundleUtility.FileRequestString(url, () =>
                {
                    PatchEvents.PatchAppVersionUpdateFailed.SendEventMessage();
                });

                AppConfig hostCfg = JsonConvert.DeserializeObject<AppConfig>(hostCfgJson);

                await this._AppVersionComparison(hostCfg);
            }

            private async UniTask _AppVersionComparison(AppConfig hostCfg)
            {
                AppConfig saCfg = new AppConfig();
                AppConfig localCfg = new AppConfig();

                // 確保本地端的儲存目錄是否存在, 無存在則建立
                if (!Directory.Exists(BundleConfig.GetLocalSandboxPath()))
                {
                    Directory.CreateDirectory(BundleConfig.GetLocalSandboxPath());
                }

                // 把資源配置文件拷貝到持久化目錄 Application.persistentDataPath
                // ※備註: 因為更新文件後是需要改寫版本號, 而在手機平台上的 StreamingAssets 是不可寫入的
                if (!File.Exists(BundleConfig.GetLocalSandboxAppConfigPath()))
                {
                    // 從 StreamingAssets 中取得配置檔 (InApp)
                    string saCfgPath = BundleConfig.GetStreamingAssetsAppConfigPath();
                    string saCfgJson = await BundleUtility.FileRequestString(saCfgPath);

                    // Local save path (Sandbox)
                    string localCfgPath = BundleConfig.GetLocalSandboxAppConfigPath();

                    // Save to Sandbox
                    if (!string.IsNullOrEmpty(saCfgJson))
                    {
                        await BundleUtility.RequestAndCopyFileFromStreamingAssets(saCfgPath, localCfgPath);
                    }
                    else
                    {
                        Debug.Log("<color=#FF0000>Cannot found bundle config from StreamingAssets.</color>");
                        return;
                    }
                }
                // 如果本地已經有配置檔, 則需要去比對主程式版本, 並且從新 App 中的配置檔寫入至本地配置檔中
                else
                {
                    // 從 StreamingAssets 讀取配置檔 (StreamingAssets 使用 Request)
                    string saCfgPath = BundleConfig.GetStreamingAssetsAppConfigPath();
                    string saCfgJson = await BundleUtility.FileRequestString(saCfgPath);
                    saCfg = JsonConvert.DeserializeObject<AppConfig>(saCfgJson);

                    // 從本地端讀取配置檔 (持久化路徑使用 File.Read)
                    string localCfgPath = BundleConfig.GetLocalSandboxAppConfigPath();
                    string localCfgJson = File.ReadAllText(localCfgPath);
                    localCfg = JsonConvert.DeserializeObject<AppConfig>(localCfgJson);

                    // 如果是離線模式, Local Config = StreamingAssets Config
                    if (BundleConfig.playMode == BundleConfig.PlayMode.OfflineMode)
                    {
                        localCfg = saCfg;
                        // 寫入儲存本地配置檔
                        localCfgJson = JsonConvert.SerializeObject(localCfg);
                        // 進行寫入存儲
                        File.WriteAllText(localCfgPath, localCfgJson);
                    }
                    else
                    {
                        // 如果主程式版本不一致表示有更新 App, 則將本地配置檔的主程式版本寫入成 StreamingAssets 配置檔中的 APP_VERSION
                        if (saCfg.APP_VERSION != localCfg.APP_VERSION)
                        {
                            localCfg.APP_VERSION = saCfg.APP_VERSION;
                            // 寫入儲存本地配置檔
                            localCfgJson = JsonConvert.SerializeObject(localCfg);
                            // 進行寫入存儲
                            File.WriteAllText(localCfgPath, localCfgJson);
                        }
                    }
                }

                try
                {
                    // 從本地端讀取配置檔
                    string localCfgPath = BundleConfig.GetLocalSandboxAppConfigPath();
                    string localCfgJson = File.ReadAllText(localCfgPath);
                    localCfg = JsonConvert.DeserializeObject<AppConfig>(localCfgJson);
                }
                catch
                {
                    Debug.Log("<color=#FF0000>Read Local Save BundleConfig.json failed.</color>");
                }

                // 比對大版號
                string[] localAppVersionArgs = localCfg.APP_VERSION.Split('.');
                string[] hostAppVersionArgs = hostCfg.APP_VERSION.Split('.');

                string localVersion = $"{localAppVersionArgs[0]}.{localAppVersionArgs[1]}";
                string hostVersion = $"{hostAppVersionArgs[0]}.{hostAppVersionArgs[1]}";

                // 比對主程式版本
                if (localVersion != hostVersion)
                {
                    // Do GoToAppStore

                    // emit go to app store event
                    PatchEvents.PatchGoToAppStore.SendEventMessage();
                    // remove last group name
                    PatchManager.DelLastGroupInfo();

                    Debug.Log("<color=#ff8c00>Application version inconsistent, require to update application (go to store)</color>");
                    Debug.Log($"<color=#ff8c00>【App Version Unpassed (Only Major and Minor)】LOCAL APP_VER: v{localVersion} ({localCfg.APP_VERSION}) != SERVER APP_VER: v{hostVersion} ({hostCfg.APP_VERSION})</color>");
                    return;
                }
                else
                {
                    Debug.Log($"<color=#00ff00>【App Version Passed (Only Major and Minor)】LOCAL APP_VER: v{localVersion} ({localCfg.APP_VERSION}) == SERVER APP_VER: v{hostVersion} ({hostCfg.APP_VERSION})</color>");

                    PatchManager.appVersion = hostCfg.APP_VERSION;
                    this._machine.ChangeState<FsmInitPatchMode>();
                }
            }
        }

        /// <summary>
        /// 3. 初始 Patch Mode
        /// </summary>
        public class FsmInitPatchMode : IStateNode
        {
            private StateMachine _machine;

            void IStateNode.OnCreate(StateMachine machine)
            {
                this._machine = machine;
            }

            void IStateNode.OnEnter()
            {
                // 初始更新資源配置
                PatchEvents.PatchFsmState.SendEventMessage(this);
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

                if (PatchManager.GetInstance().IsRepair() &&
                    PackageManager.GetDefaultPackage().InitializeStatus == EOperationStatus.Succeed)
                {
                    if (BundleConfig.playMode == BundleConfig.PlayMode.EditorSimulateMode ||
                        BundleConfig.playMode == BundleConfig.PlayMode.OfflineMode)
                    {
                        this._machine.ChangeState<FsmPatchDone>();
                        return;
                    }

                    // Add static method in YooAssets and call CacheSystem.ClearAll()
                    YooAssets.ResetCacheSystem();
                    this._machine.ChangeState<FsmPatchVersionUpdate>();
                    Debug.Log("<color=#ffcf67>(Repair) Repair Patch</color>");
                    return;
                }
                else if (PackageManager.GetDefaultPackage().InitializeStatus == EOperationStatus.Succeed)
                {
                    this._machine.ChangeState<FsmPatchVersionUpdate>();
                    Debug.Log("<color=#ffcf67>(Check) Check Patch</color>");
                    return;
                }

                var operation = await PackageManager.InitDefaultPackage();
                if (operation.Status == EOperationStatus.Succeed)
                {
                    this._machine.ChangeState<FsmPatchVersionUpdate>();
                    Debug.Log("<color=#ffcf67>(Init) Init Patch</color>");
                }
                else
                {
                    PatchEvents.PatchInitPatchModeFailed.SendEventMessage();
                }
            }
        }

        /// <summary>
        /// 4. 更新 Patch Version
        /// </summary>
        public class FsmPatchVersionUpdate : IStateNode
        {
            private StateMachine _machine;

            void IStateNode.OnCreate(StateMachine machine)
            {
                this._machine = machine;
            }

            void IStateNode.OnEnter()
            {
                // 獲取最新的資源版本
                PatchEvents.PatchFsmState.SendEventMessage(this);
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

                var package = PackageManager.GetDefaultPackage();
                var operation = package.UpdatePackageVersionAsync();
                await operation;

                if (operation.Status == EOperationStatus.Succeed)
                {
                    PatchManager.patchVersion = operation.PackageVersion;
                    this._machine.ChangeState<FsmPatchManifestUpdate>();
                }
                else
                {
                    Debug.LogWarning(operation.Error);
                    PatchEvents.PatchVersionUpdateFailed.SendEventMessage();
                }
            }
        }

        /// <summary>
        /// 5. 更新 Patch Manifest
        /// </summary>
        public class FsmPatchManifestUpdate : IStateNode
        {
            private StateMachine _machine;

            void IStateNode.OnCreate(StateMachine machine)
            {
                this._machine = machine;
            }

            void IStateNode.OnEnter()
            {
                // 更新資源清單
                PatchEvents.PatchFsmState.SendEventMessage(this);
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

                var package = PackageManager.GetDefaultPackage();
                var operation = package.UpdatePackageManifestAsync(PatchManager.patchVersion);
                await operation;

                if (operation.Status == EOperationStatus.Succeed)
                {
                    if (BundleConfig.skipPatchDownloadStep) this._machine.ChangeState<FsmDownloadOver>();
                    else this._machine.ChangeState<FsmCreateDownloader>();
                }
                else
                {
                    Debug.LogWarning(operation.Error);
                    PatchEvents.PatchManifestUpdateFailed.SendEventMessage();
                }
            }
        }

        /// <summary>
        /// 6. 創建資源下載器
        /// </summary>
        public class FsmCreateDownloader : IStateNode
        {
            private StateMachine _machine;

            void IStateNode.OnCreate(StateMachine machine)
            {
                this._machine = machine;
            }

            void IStateNode.OnEnter()
            {
                // 創建資源下載器
                PatchEvents.PatchFsmState.SendEventMessage(this);
                this._CreateDownloader().Forget();
            }

            void IStateNode.OnUpdate()
            {
            }

            void IStateNode.OnExit()
            {
            }

            private async UniTask _CreateDownloader()
            {
                await UniTask.Delay(TimeSpan.FromSeconds(0.1f), true);

                // EditorSimulateMode or OfflineMode skip directly
                if (BundleConfig.playMode == BundleConfig.PlayMode.EditorSimulateMode ||
                    BundleConfig.playMode == BundleConfig.PlayMode.OfflineMode)
                {
                    this._machine.ChangeState<FsmPatchDone>();
                    return;
                }

                string defaultGroupTag = PatchManager.DEFAULT_GROUP_TAG;

                GroupInfo lastGroupInfo = PatchManager.GetLastGroupInfo();
                Debug.Log($"<color=#ffce54>Get last GroupName: {lastGroupInfo?.groupName}</color>");

                List<GroupInfo> newGroupInfos = new List<GroupInfo>();

                #region Create Downaloder by Tags
                string url = await BundleConfig.GetHostServerPatchConfigPath();
                string hostCfgJson = await BundleUtility.FileRequestString(url);
                PatchConfig patchCfg = JsonConvert.DeserializeObject<PatchConfig>(hostCfgJson);
                List<GroupInfo> patchGroupInfos = patchCfg.GROUP_INFOS;

                var package = PackageManager.GetDefaultPackage();

                if (lastGroupInfo == null)
                {
                    // all package
                    var downloader = package.CreateResourceDownloader(BundleConfig.maxConcurrencyDownloadCount, BundleConfig.failedRetryCount);
                    int totalDownloadCount = downloader.TotalDownloadCount;
                    long totalDownloadBytes = downloader.TotalDownloadBytes;
                    if (totalDownloadCount > 0) newGroupInfos.Add(new GroupInfo() { groupName = defaultGroupTag, tags = new string[] { }, totalCount = totalDownloadCount, totalBytes = totalDownloadBytes });

                    // package by tags
                    if (patchGroupInfos != null && patchGroupInfos.Count > 0)
                    {
                        foreach (var groupInfo in patchGroupInfos)
                        {
                            downloader = package.CreateResourceDownloader(groupInfo.tags, BundleConfig.maxConcurrencyDownloadCount, BundleConfig.failedRetryCount);
                            totalDownloadCount = downloader.TotalDownloadCount;
                            totalDownloadBytes = downloader.TotalDownloadBytes;
                            if (totalDownloadCount > 0) newGroupInfos.Add(new GroupInfo() { groupName = groupInfo.groupName, tags = groupInfo.tags, totalCount = totalDownloadCount, totalBytes = totalDownloadBytes });
                        }
                    }
                }
                else
                {
                    if (defaultGroupTag == lastGroupInfo.groupName)
                    {
                        var downloader = package.CreateResourceDownloader(BundleConfig.maxConcurrencyDownloadCount, BundleConfig.failedRetryCount);
                        int totalDownloadCount = downloader.TotalDownloadCount;
                        long totalDownloadBytes = downloader.TotalDownloadBytes;
                        if (totalDownloadCount > 0) newGroupInfos.Add(new GroupInfo() { groupName = defaultGroupTag, tags = new string[] { }, totalCount = totalDownloadCount, totalBytes = totalDownloadBytes });
                    }
                    else if (patchGroupInfos != null && patchGroupInfos.Count > 0)
                    {
                        foreach (var groupInfo in patchGroupInfos)
                        {
                            if (groupInfo.groupName == lastGroupInfo.groupName)
                            {
                                var downloader = package.CreateResourceDownloader(groupInfo.tags, BundleConfig.maxConcurrencyDownloadCount, BundleConfig.failedRetryCount);
                                int totalDownloadCount = downloader.TotalDownloadCount;
                                long totalDownloadBytes = downloader.TotalDownloadBytes;
                                if (totalDownloadCount > 0) newGroupInfos.Add(new GroupInfo() { groupName = groupInfo.groupName, tags = groupInfo.tags, totalCount = totalDownloadCount, totalBytes = totalDownloadBytes });
                                break;
                            }
                        }
                    }
                }
                #endregion

                if (newGroupInfos.Count > 0)
                {
                    Debug.Log($"<color=#ffce54>Auto check last GroupName: {lastGroupInfo?.groupName}</color>");

                    Debug.Log($"<color=#54beff>Found total group {newGroupInfos.Count} to choose download =>\n{JsonConvert.SerializeObject(newGroupInfos)}</color>");

                    PatchEvents.PatchCreateDownloader.SendEventMessage(newGroupInfos.ToArray());

                    // 開始等待使用者選擇是否開始下載
                }
                else
                {
                    Debug.Log($"<color=#54ff75><color=#ffce54>GroupName: {lastGroupInfo?.groupName}</color> not found any download files!!!</color>");

                    this._machine.ChangeState<FsmDownloadOver>();
                }
            }
        }

        /// <summary>
        /// 7. 下載資源檔案
        /// </summary>
        public class FsmBeginDownload : IStateNode
        {
            private StateMachine _machine;

            void IStateNode.OnCreate(StateMachine machine)
            {
                this._machine = machine;
            }

            void IStateNode.OnEnter()
            {
                // 下載資源檔案中
                PatchEvents.PatchFsmState.SendEventMessage(this);
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
                var package = PackageManager.GetDefaultPackage();

                GroupInfo lastGroupInfo = PatchManager.GetLastGroupInfo();

                Debug.Log($"<color=#54ffad>Start Download Group Name: {lastGroupInfo?.groupName}, Tags: {JsonConvert.SerializeObject(lastGroupInfo?.tags)}</color>");

                if (lastGroupInfo != null)
                {
                    if (lastGroupInfo.groupName == PatchManager.DEFAULT_GROUP_TAG) PatchManager.GetInstance().mainDownloader = package.CreateResourceDownloader(BundleConfig.maxConcurrencyDownloadCount, BundleConfig.failedRetryCount);
                    else PatchManager.GetInstance().mainDownloader = package.CreateResourceDownloader(lastGroupInfo.tags, BundleConfig.maxConcurrencyDownloadCount, BundleConfig.failedRetryCount);
                }

                ResourceDownloaderOperation downloader = PatchManager.GetInstance().mainDownloader;

                downloader.OnDownloadErrorCallback = PatchEvents.PatchDownloadFailed.SendEventMessage;
                downloader.OnDownloadProgressCallback = PatchEvents.PatchDownloadProgression.SendEventMessage;
                downloader.BeginDownload();
                await downloader;

                if (downloader.Status != EOperationStatus.Succeed) return;

                this._machine.ChangeState<FsmDownloadOver>();
            }
        }

        /// <summary>
        /// 8. 資源下載完成
        /// </summary>
        public class FsmDownloadOver : IStateNode
        {
            private StateMachine _machine;

            void IStateNode.OnCreate(StateMachine machine)
            {
                this._machine = machine;
            }

            void IStateNode.OnEnter()
            {
                // 資源下載完成
                PatchEvents.PatchFsmState.SendEventMessage(this);
                this._machine.ChangeState<FsmClearCache>();
            }

            void IStateNode.OnUpdate()
            {
            }

            void IStateNode.OnExit()
            {
            }
        }

        /// <summary>
        /// 9. 清理未使用的緩存文件
        /// </summary>
        public class FsmClearCache : IStateNode
        {
            private StateMachine _machine;

            void IStateNode.OnCreate(StateMachine machine)
            {
                this._machine = machine;
            }

            void IStateNode.OnEnter()
            {
                // 清理未使用的緩存文件
                PatchEvents.PatchFsmState.SendEventMessage(this);
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
                var package = PackageManager.GetDefaultPackage();
                var operation = package.ClearUnusedCacheFilesAsync();
                await operation;

                if (operation.IsDone) this._machine.ChangeState<FsmPatchDone>();
            }
        }

        /// <summary>
        /// 10. 更新完畢
        /// </summary>
        public class FsmPatchDone : IStateNode
        {
            void IStateNode.OnCreate(StateMachine machine)
            {
            }

            void IStateNode.OnEnter()
            {
                // 更新完畢
                PatchEvents.PatchFsmState.SendEventMessage(this);
                // Patch 標記完成
                PatchManager.GetInstance().MarkAsDone();
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
