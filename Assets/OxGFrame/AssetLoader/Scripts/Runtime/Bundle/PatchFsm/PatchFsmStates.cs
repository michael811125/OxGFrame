﻿using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using OxGFrame.AssetLoader.Bundle;
using OxGFrame.AssetLoader.PatchEvent;
using OxGFrame.AssetLoader.Utility;
using OxGKit.Utilities.Request;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            public FsmPatchRepair() { }

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
                // Delete Last Group Info record
                PatchManager.DelLastGroupInfo();

                // Get preset app package names
                bool isCleared = false;
                var packageNames = PackageManager.GetPresetAppPackageNames();
                foreach (var packageName in packageNames)
                {
                    // Clear cache and files of package
                    isCleared = await PackageManager.UnloadPackageAndClearCacheFiles(packageName);
                    if (!isCleared) isCleared = false;
                }

                if (isCleared) this._machine.ChangeState<FsmPatchPrepare>();
                else PatchEvents.PatchRepairFailed.SendEventMessage();
            }
        }

        /// <summary>
        /// 1. 流程準備工作
        /// </summary>
        public class FsmPatchPrepare : IStateNode
        {
            private StateMachine _machine;

            public FsmPatchPrepare() { }

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

            public FsmAppVersionUpdate() { }

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

                string hostCfgJson = await Requester.RequestText(url, null, PatchEvents.PatchAppVersionUpdateFailed.SendEventMessage, null, false);

                AppConfig hostCfg = JsonConvert.DeserializeObject<AppConfig>(hostCfgJson);

                await this._AppVersionComparison(hostCfg);
            }

            private async UniTask _AppVersionComparison(AppConfig hostCfg)
            {
                AppConfig saCfg = new AppConfig();
                AppConfig localCfg = new AppConfig();

                // 確保本地端的儲存目錄是否存在, 無存在則建立
                if (!Directory.Exists(BundleConfig.GetLocalSandboxRootPath()))
                {
                    Directory.CreateDirectory(BundleConfig.GetLocalSandboxRootPath());
                }

                // 把資源配置文件拷貝到持久化目錄 Application.persistentDataPath
                // ※備註: 因為更新文件後是需要改寫版本號, 而在手機平台上的 StreamingAssets 是不可寫入的
                if (!File.Exists(BundleConfig.GetLocalSandboxAppConfigPath()))
                {
                    // 從 StreamingAssets 中取得配置檔 (InApp)
                    string saCfgPath = BundleConfig.GetStreamingAssetsAppConfigPath();
                    string saCfgJson = await Requester.RequestText(saCfgPath, null, null, null, false);

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
                    string saCfgJson = await Requester.RequestText(saCfgPath, null, null, null, false);
                    saCfg = JsonConvert.DeserializeObject<AppConfig>(saCfgJson);

                    // 從本地端讀取配置檔 (持久化路徑使用 File.Read)
                    string localCfgPath = BundleConfig.GetLocalSandboxAppConfigPath();
                    string localCfgJson = File.ReadAllText(localCfgPath);
                    localCfg = JsonConvert.DeserializeObject<AppConfig>(localCfgJson);

                    // 如果是離線模式, Local Config = StreamingAssets Config
                    if (BundleConfig.playMode == BundleConfig.PlayMode.OfflineMode)
                    {
                        // 寫入 StreamingAssets 的配置檔至 Local
                        File.WriteAllText(localCfgPath, JsonConvert.SerializeObject(saCfg));
                    }
                    else
                    {
                        if (BundleConfig.semanticRule.patch)
                        {
                            // 比對完整版號
                            if (saCfg.APP_VERSION != localCfg.APP_VERSION)
                            {
                                // 寫入 StreamingAssets 的配置檔至 Local
                                File.WriteAllText(localCfgPath, JsonConvert.SerializeObject(saCfg));
                            }
                        }
                        else
                        {
                            // 比對大小版號
                            string[] saAppVersionArgs = saCfg.APP_VERSION.Split('.');
                            string[] localAppVersionArgs = localCfg.APP_VERSION.Split('.');

                            string saVersion = $"{saAppVersionArgs[0]}.{saAppVersionArgs[1]}";
                            string localVersion = $"{localAppVersionArgs[0]}.{localAppVersionArgs[1]}";

                            // 如果 StreamingAssets 中的 App 大小版號與 Local 大小版號不一致表示有更新 App, 則會重新寫入 AppConfig 至 Local
                            if (saVersion != localVersion)
                            {
                                // 寫入 StreamingAssets 的配置檔至 Local
                                File.WriteAllText(localCfgPath, JsonConvert.SerializeObject(saCfg));
                            }
                        }
                    }
                }

                {
                    try
                    {
                        // 重新讀取本地端配置檔
                        string localCfgPath = BundleConfig.GetLocalSandboxAppConfigPath();
                        string localCfgJson = File.ReadAllText(localCfgPath);
                        localCfg = JsonConvert.DeserializeObject<AppConfig>(localCfgJson);
                    }
                    catch
                    {
                        Debug.Log("<color=#FF0000>Read Local Config failed.</color>");
                    }

                    if (BundleConfig.semanticRule.patch)
                    {
                        // 比對 Local 與 Host 的主程式完整版號
                        if (localCfg.APP_VERSION != hostCfg.APP_VERSION)
                        {
                            // Do GoToAppStore

                            // emit go to app store event
                            PatchEvents.PatchGoToAppStore.SendEventMessage();
                            // remove last group name
                            PatchManager.DelLastGroupInfo();

                            Debug.Log("<color=#ff8c00>Application version inconsistent, require to update application (go to store)</color>");
                            Debug.Log($"<color=#ff8c00>【App Version Unpassed (X.Y.Z)】LOCAL APP_VER: v{localCfg.APP_VERSION} != SERVER APP_VER: v{hostCfg.APP_VERSION}</color>");
                            return;
                        }
                        else
                        {
                            PatchManager.appVersion = hostCfg.APP_VERSION;
                            this._machine.ChangeState<FsmInitPatchMode>();

                            Debug.Log($"<color=#00ff00>【App Version Passed (X.Y.Z)】LOCAL APP_VER: v{localCfg.APP_VERSION} == SERVER APP_VER: v{hostCfg.APP_VERSION}</color>");
                        }
                    }
                    else
                    {
                        // 比對大小版號
                        string[] localAppVersionArgs = localCfg.APP_VERSION.Split('.');
                        string[] hostAppVersionArgs = hostCfg.APP_VERSION.Split('.');

                        string localVersion = $"{localAppVersionArgs[0]}.{localAppVersionArgs[1]}";
                        string hostVersion = $"{hostAppVersionArgs[0]}.{hostAppVersionArgs[1]}";

                        // 比對 Local 與 Host 的主程式大小版號
                        if (localVersion != hostVersion)
                        {
                            // Do GoToAppStore

                            // emit go to app store event
                            PatchEvents.PatchGoToAppStore.SendEventMessage();
                            // remove last group name
                            PatchManager.DelLastGroupInfo();

                            Debug.Log("<color=#ff8c00>Application version inconsistent, require to update application (go to store)</color>");
                            Debug.Log($"<color=#ff8c00>【App Version Unpassed (X.Y)】LOCAL APP_VER: v{localVersion} ({localCfg.APP_VERSION}) != SERVER APP_VER: v{hostVersion} ({hostCfg.APP_VERSION})</color>");
                            return;
                        }
                        else
                        {
                            // 寫入完整版號至 Local
                            if (localCfg.APP_VERSION != hostCfg.APP_VERSION)
                            {
                                // 寫入 Host 的配置檔至 Local
                                string localCfgPath = BundleConfig.GetLocalSandboxAppConfigPath();
                                File.WriteAllText(localCfgPath, JsonConvert.SerializeObject(hostCfg));
                            }

                            PatchManager.appVersion = hostCfg.APP_VERSION;
                            this._machine.ChangeState<FsmInitPatchMode>();

                            Debug.Log($"<color=#00ff00>【App Version Passed (X.Y)】LOCAL APP_VER: v{localVersion} ({localCfg.APP_VERSION}) == SERVER APP_VER: v{hostVersion} ({hostCfg.APP_VERSION})</color>");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 3. 初始 Patch Mode
        /// </summary>
        public class FsmInitPatchMode : IStateNode
        {
            private StateMachine _machine;

            public FsmInitPatchMode() { }

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
                    PackageManager.isInitialized)
                {
                    // SimulateMode and OfflineMode doesn't need to update patch version and manifest
                    if (BundleConfig.playMode == BundleConfig.PlayMode.EditorSimulateMode ||
                        BundleConfig.playMode == BundleConfig.PlayMode.OfflineMode)
                    {
                        this._machine.ChangeState<FsmPatchDone>();
                        return;
                    }

                    this._machine.ChangeState<FsmPatchVersionUpdate>();
                    Debug.Log("<color=#ffcf67>(Repair) Repair Patch</color>");
                    return;
                }
                else if (PackageManager.isInitialized)
                {
                    this._machine.ChangeState<FsmPatchVersionUpdate>();
                    Debug.Log("<color=#ffcf67>(Check) Check Patch</color>");
                    return;
                }

                bool isInitialized = await PackageManager.InitPresetAppPackages();
                if (isInitialized)
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

            public FsmPatchVersionUpdate() { }

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

                var packages = PackageManager.GetPresetAppPackages();

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
                        PatchEvents.PatchVersionUpdateFailed.SendEventMessage();
                        Debug.Log($"<color=#ff3696>Package: {package.PackageName} update version failed.</color>");
                        break;
                    }
                }

                if (succeed)
                {
                    PatchManager.patchVersions = patchVersions.ToArray();
                    this._machine.ChangeState<FsmPatchManifestUpdate>();
                }
            }
        }

        /// <summary>
        /// 5. 更新 Patch Manifest
        /// </summary>
        public class FsmPatchManifestUpdate : IStateNode
        {
            private StateMachine _machine;

            public FsmPatchManifestUpdate() { }

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

                var packages = PackageManager.GetPresetAppPackages();

                bool succeed = false;
                for (int i = 0; i < packages.Length; i++)
                {
                    var operation = packages[i].UpdatePackageManifestAsync(PatchManager.patchVersions[i]);
                    await operation;

                    if (operation.Status == EOperationStatus.Succeed)
                    {
                        succeed = true;
                        operation.SavePackageVersion();
                        Debug.Log($"<color=#85cf0f>Package: {packages[i].PackageName} <color=#00c1ff>Update</color> completed successfully.</color>");
                    }
                    else
                    {
                        succeed = false;
                        PatchEvents.PatchManifestUpdateFailed.SendEventMessage();
                        Debug.Log($"<color=#ff3696>Package: {packages[i].PackageName} update manifest failed.</color>");
                        break;
                    }
                }

                if (succeed)
                {
                    if (BundleConfig.skipCreateMainDownloder) this._machine.ChangeState<FsmDownloadOver>();
                    else this._machine.ChangeState<FsmCreateDownloader>();
                }
            }
        }

        /// <summary>
        /// 6. 創建資源下載器
        /// </summary>
        public class FsmCreateDownloader : IStateNode
        {
            private StateMachine _machine;

            public FsmCreateDownloader() { }

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

                #region Create Downaloder by Tags
                string url = await BundleConfig.GetHostServerPatchConfigPath();
                string hostCfgJson = await Requester.RequestText(url, null, null, null, false);
                PatchConfig patchCfg = JsonConvert.DeserializeObject<PatchConfig>(hostCfgJson);
                List<GroupInfo> patchGroupInfos = patchCfg.GROUP_INFOS;

                var packages = PackageManager.GetPresetAppPackages();

                string key;
                int totalDownloadCount;
                long totalDownloadBytes;
                Dictionary<string, GroupInfo> newGroupInfos = new Dictionary<string, GroupInfo>();
                for (int i = 0; i < packages.Length; i++)
                {
                    var package = packages[i];

                    if (lastGroupInfo == null)
                    {
                        // all package
                        var downloader = package.CreateResourceDownloader(BundleConfig.maxConcurrencyDownloadCount, BundleConfig.failedRetryCount);
                        totalDownloadCount = downloader.TotalDownloadCount;
                        totalDownloadBytes = downloader.TotalDownloadBytes;
                        key = defaultGroupTag;
                        if (totalDownloadCount > 0)
                        {
                            if (!newGroupInfos.ContainsKey(key))
                            {
                                newGroupInfos.Add
                                (
                                    key,
                                    new GroupInfo()
                                    {
                                        groupName = key,
                                        tags = new string[] { },
                                        totalCount = totalDownloadCount,
                                        totalBytes = totalDownloadBytes
                                    }
                                );
                            }
                            else
                            {
                                newGroupInfos[key].totalCount += totalDownloadCount;
                                newGroupInfos[key].totalBytes += totalDownloadBytes;
                            }
                        }

                        // package by tags
                        if (patchGroupInfos != null && patchGroupInfos.Count > 0)
                        {
                            foreach (var groupInfo in patchGroupInfos)
                            {
                                downloader = package.CreateResourceDownloader(groupInfo.tags, BundleConfig.maxConcurrencyDownloadCount, BundleConfig.failedRetryCount);
                                totalDownloadCount = downloader.TotalDownloadCount;
                                totalDownloadBytes = downloader.TotalDownloadBytes;
                                key = groupInfo.groupName;
                                if (totalDownloadCount > 0)
                                {
                                    if (!newGroupInfos.ContainsKey(key))
                                    {
                                        newGroupInfos.Add
                                        (
                                            key,
                                            new GroupInfo()
                                            {
                                                groupName = key,
                                                tags = groupInfo.tags,
                                                totalCount = totalDownloadCount,
                                                totalBytes = totalDownloadBytes
                                            }
                                        );
                                    }
                                    else
                                    {
                                        newGroupInfos[key].totalCount += totalDownloadCount;
                                        newGroupInfos[key].totalBytes += totalDownloadBytes;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // all package
                        if (defaultGroupTag == lastGroupInfo.groupName)
                        {
                            var downloader = package.CreateResourceDownloader(BundleConfig.maxConcurrencyDownloadCount, BundleConfig.failedRetryCount);
                            totalDownloadCount = downloader.TotalDownloadCount;
                            totalDownloadBytes = downloader.TotalDownloadBytes;
                            key = defaultGroupTag;
                            if (totalDownloadCount > 0)
                            {
                                if (!newGroupInfos.ContainsKey(key))
                                {
                                    newGroupInfos.Add
                                    (
                                        key,
                                        new GroupInfo()
                                        {
                                            groupName = key,
                                            tags = new string[] { },
                                            totalCount = totalDownloadCount,
                                            totalBytes = totalDownloadBytes
                                        }
                                    );
                                }
                                else
                                {
                                    newGroupInfos[key].totalCount += totalDownloadCount;
                                    newGroupInfos[key].totalBytes += totalDownloadBytes;
                                }
                            }
                        }
                        // package by tags
                        else if (patchGroupInfos != null && patchGroupInfos.Count > 0)
                        {
                            foreach (var groupInfo in patchGroupInfos)
                            {
                                if (groupInfo.groupName == lastGroupInfo.groupName)
                                {
                                    var downloader = package.CreateResourceDownloader(groupInfo.tags, BundleConfig.maxConcurrencyDownloadCount, BundleConfig.failedRetryCount);
                                    totalDownloadCount = downloader.TotalDownloadCount;
                                    totalDownloadBytes = downloader.TotalDownloadBytes;
                                    key = groupInfo.groupName;
                                    if (totalDownloadCount > 0)
                                    {
                                        if (!newGroupInfos.ContainsKey(key))
                                        {
                                            newGroupInfos.Add
                                            (
                                                key,
                                                new GroupInfo()
                                                {
                                                    groupName = key,
                                                    tags = groupInfo.tags,
                                                    totalCount = totalDownloadCount,
                                                    totalBytes = totalDownloadBytes
                                                }
                                            );
                                        }
                                        else
                                        {
                                            newGroupInfos[key].totalCount += totalDownloadCount;
                                            newGroupInfos[key].totalBytes += totalDownloadBytes;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
                #endregion

                if (newGroupInfos.Count > 0)
                {
                    Debug.Log($"<color=#ffce54>Auto check last GroupName: {lastGroupInfo?.groupName}</color>");

                    Debug.Log($"<color=#54beff>Found total group {newGroupInfos.Count} to choose download =>\n{JsonConvert.SerializeObject(newGroupInfos)}</color>");

                    PatchEvents.PatchCreateDownloader.SendEventMessage(newGroupInfos.Values.ToArray());

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

            public FsmBeginDownload() { }

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
                var packages = PackageManager.GetPresetAppPackages();

                // Get last GroupInfo by UserEvent Set
                GroupInfo lastGroupInfo = PatchManager.GetLastGroupInfo();

                Debug.Log($"<color=#54ffad>Start Download Group Name: {lastGroupInfo?.groupName}, Tags: {JsonConvert.SerializeObject(lastGroupInfo?.tags)}</color>");

                List<ResourceDownloaderOperation> mainDownloaders = new List<ResourceDownloaderOperation>();
                foreach (var package in packages)
                {
                    if (lastGroupInfo != null)
                    {
                        if (lastGroupInfo.groupName == PatchManager.DEFAULT_GROUP_TAG) mainDownloaders.Add(package.CreateResourceDownloader(BundleConfig.maxConcurrencyDownloadCount, BundleConfig.failedRetryCount));
                        else mainDownloaders.Add(package.CreateResourceDownloader(lastGroupInfo.tags, BundleConfig.maxConcurrencyDownloadCount, BundleConfig.failedRetryCount));
                    }
                }

                // Set Main Downloaders
                PatchManager.GetInstance().mainDownloaders = mainDownloaders.ToArray();

                // Combine all main downloaders count and bytes
                int totalCount = 0;
                long totalBytes = 0;
                foreach (var downloader in mainDownloaders)
                {
                    totalCount += downloader.TotalDownloadCount;
                    totalBytes += downloader.TotalDownloadBytes;
                }

                // Begin Download
                int currentCount = 0;
                long currentBytes = 0;
                var downloadSpeedCalculator = new DownloadSpeedCalculator();
                downloadSpeedCalculator.onDownloadSpeedProgress = PatchEvents.PatchDownloadProgression.SendEventMessage;
                foreach (var downloader in mainDownloaders)
                {
                    int lastCount = 0;
                    long lastBytes = 0;
                    downloader.OnDownloadErrorCallback = PatchEvents.PatchDownloadFailed.SendEventMessage;
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
        /// 8. 資源下載完成
        /// </summary>
        public class FsmDownloadOver : IStateNode
        {
            private StateMachine _machine;

            public FsmDownloadOver() { }

            void IStateNode.OnCreate(StateMachine machine)
            {
                this._machine = machine;
            }

            void IStateNode.OnEnter()
            {
                // 資源下載完成
                PatchEvents.PatchFsmState.SendEventMessage(this);
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

                this._machine.ChangeState<FsmClearCache>();
            }
        }

        /// <summary>
        /// 9. 清理未使用的緩存文件
        /// </summary>
        public class FsmClearCache : IStateNode
        {
            private StateMachine _machine;

            public FsmClearCache() { }

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
                await UniTask.Delay(TimeSpan.FromSeconds(0.1f), true);

                var packages = PackageManager.GetPresetAppPackages();
                foreach (var package in packages)
                {
                    var operation = package.ClearUnusedCacheFilesAsync();
                    await operation;
                }

                this._machine.ChangeState<FsmPatchDone>();
            }
        }

        /// <summary>
        /// 10. 更新完畢
        /// </summary>
        public class FsmPatchDone : IStateNode
        {
            public FsmPatchDone() { }

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
