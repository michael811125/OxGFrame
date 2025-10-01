using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using OxGFrame.AssetLoader.Bundle;
using OxGFrame.AssetLoader.PatchEvent;
using OxGFrame.AssetLoader.Utility;
using OxGKit.LoggingSystem;
using OxGKit.Utilities.Requester;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniFramework.Machine;
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
            private int _retryCount = _RETRY_COUNT;

            private const int _RETRY_COUNT = 1;

            public FsmPatchRepair() { }

            void IStateNode.OnCreate(StateMachine machine)
            {
                this._machine = machine;
            }

            void IStateNode.OnEnter()
            {
                // 流程準備
                PatchEvents.PatchFsmState.SendEventMessage(this);
                PatchManager.GetInstance().MarkRepairState();
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
                PatchManager.GetInstance().Cancel(false);

                // Wait a frame
                await UniTask.NextFrame();

                // Delete Last Group Info record
                PatchManager.DelLastGroupInfo();

                // Get preset app package names and combine packages
                var appPackageNames = PackageManager.GetPresetAppPackageNames();
                var dlcPackageNames = PackageManager.GetPresetDlcPackageNames();
                var packageNames = appPackageNames.Concat(dlcPackageNames).ToArray();

                bool isCleared = false;
                foreach (var packageName in packageNames)
                {
                    // Clear cache and files of package
                    isCleared = await PackageManager.UnloadPackageAndClearCacheFiles(packageName, false);
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
                    PatchEvents.PatchRepairFailed.SendEventMessage();
                }
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
                PatchManager.GetInstance().MarkCheckState();
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

                // 如果是離線模式或 WebGL 模式, 則以 StreamingAssets 的 appconfig 為主
                if (!BundleConfig.playModeParameters.fetchAppConfigFromServer)
                    url = saCfgPath;
                // 反之, 從 Server 請求 Cfg
                else
                    url = await BundleConfig.GetHostServerAppConfigPath();

                string hostCfgJson = await Requester.RequestText(url);
                if (string.IsNullOrEmpty(hostCfgJson))
                {
                    // 弱聯網處理
                    if (BundleConfig.playModeParameters.enableLastLocalVersionsCheckInWeakNetwork)
                    {
                        hostCfgJson = BundleConfig.saver.GetString(BundleConfig.LAST_APP_VERSION_KEY, string.Empty);
                        if (string.IsNullOrEmpty(hostCfgJson))
                        {
                            PatchEvents.PatchAppVersionUpdateFailed.SendEventMessage();
                            Logging.PrintError<Logger>($"[{nameof(BundleConfig.playModeParameters.enableLastLocalVersionsCheckInWeakNetwork)}] Failed to request the app config from the URL: {url}.");
                            return;
                        }
                    }
                    else
                    {
                        PatchEvents.PatchAppVersionUpdateFailed.SendEventMessage();
                        Logging.PrintError<Logger>($"Failed to request the app config from the URL: {url}.");
                        return;
                    }
                }

                AppConfig hostCfg = JsonConvert.DeserializeObject<AppConfig>(hostCfgJson);
                // 處理 App 版號比對
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
                // ※備註: 因為更新文件後是需要改寫版本號, 而在行動平台上的 StreamingAssets 是不可寫入的
                if (!File.Exists(BundleConfig.GetLocalSandboxAppConfigPath()))
                {
                    // 從 StreamingAssets 中取得配置文件 (InApp)
                    string saCfgPath = BundleConfig.GetStreamingAssetsAppConfigPath();

                    // Local save path (Sandbox)
                    string localCfgPath = BundleConfig.GetLocalSandboxAppConfigPath();

                    // Request appconfig from StreamginAssets and Save to local Sandbox
                    string saCfgJson = await Requester.RequestText(saCfgPath, null, null, null, false);
                    if (!string.IsNullOrEmpty(saCfgJson))
                    {
                        File.WriteAllText(localCfgPath, saCfgJson);
                    }
                    else
                    {
                        PatchEvents.PatchAppVersionUpdateFailed.SendEventMessage();
                        Logging.PrintError<Logger>($"Cannot find the app configuration in StreamingAssets.");
                        return;
                    }
                }
                // 如果本地已經有配置文件, 則需要去比對主程式版本, 並且從新 App 中的配置文件寫入至本地配置文件中
                else
                {
                    // 從 StreamingAssets 讀取配置文件 (StreamingAssets 使用 Request)
                    string saCfgPath = BundleConfig.GetStreamingAssetsAppConfigPath();
                    string saCfgJson = await Requester.RequestText(saCfgPath, null, null, null, false);
                    if (string.IsNullOrEmpty(saCfgJson))
                    {
                        PatchEvents.PatchAppVersionUpdateFailed.SendEventMessage();
                        Logging.PrintError<Logger>($"Cannot find the app configuration in StreamingAssets.");
                        return;
                    }
                    saCfg = JsonConvert.DeserializeObject<AppConfig>(saCfgJson);

                    // 從本地端讀取配置文件 (持久化路徑使用 File.Read)
                    string localCfgPath = BundleConfig.GetLocalSandboxAppConfigPath();
                    string localCfgJson = File.ReadAllText(localCfgPath);
                    localCfg = JsonConvert.DeserializeObject<AppConfig>(localCfgJson);

                    // 如果是離線模式或 WebGL 模式, 則以 StreamingAssets 的 appconfig 為主 (Local Config = StreamingAssets Config)
                    if (!BundleConfig.playModeParameters.fetchAppConfigFromServer)
                    {
                        // 寫入 StreamingAssets 的配置文件至 Local
                        File.WriteAllText(localCfgPath, JsonConvert.SerializeObject(saCfg));
                    }
                    else
                    {
                        if (BundleConfig.playModeParameters.semanticRule.patch)
                        {
                            // 比對完整版號 X.Y.Z
                            if (saCfg.APP_VERSION != localCfg.APP_VERSION)
                            {
                                // 寫入 StreamingAssets 的配置文件至 Local
                                File.WriteAllText(localCfgPath, JsonConvert.SerializeObject(saCfg));
                            }
                        }
                        else
                        {
                            // 比對 X.Y 版號
                            string[] saAppVersionArgs = saCfg.APP_VERSION.Split('.');
                            string[] localAppVersionArgs = localCfg.APP_VERSION.Split('.');

                            string saVersion = $"{saAppVersionArgs[0]}.{saAppVersionArgs[1]}";
                            string localVersion = $"{localAppVersionArgs[0]}.{localAppVersionArgs[1]}";

                            // 如果 StreamingAssets 中的 App X.Y 版號與 Local X.Y 版號不一致表示有更新 App, 則會重新寫入 AppConfig 至 Local
                            if (saVersion != localVersion)
                            {
                                // 寫入 StreamingAssets 的配置文件至 Local
                                File.WriteAllText(localCfgPath, JsonConvert.SerializeObject(saCfg));
                            }
                        }
                    }
                }

                // 開始 Local 與 Host 比對 App 版號
                {
                    try
                    {
                        // 重新讀取本地端配置文件
                        string localCfgPath = BundleConfig.GetLocalSandboxAppConfigPath();
                        string localCfgJson = File.ReadAllText(localCfgPath);
                        localCfg = JsonConvert.DeserializeObject<AppConfig>(localCfgJson);
                    }
                    catch
                    {
                        PatchEvents.PatchAppVersionUpdateFailed.SendEventMessage();
                        Logging.PrintError<Logger>("Failed to read local config file.");
                        return;
                    }

                    if (BundleConfig.playModeParameters.semanticRule.patch)
                    {
                        // 比對 Local 與 Host 的主程式完整版號 X.Y.Z
                        if (localCfg.APP_VERSION != hostCfg.APP_VERSION)
                        {
                            // Do GoToAppStore

                            // Emit go to app store event
                            PatchEvents.PatchGoToAppStore.SendEventMessage();
                            // Remove last group name
                            PatchManager.DelLastGroupInfo();

                            Logging.PrintWarning<Logger>("Application version inconsistent, require to update application (go to store)");
                            Logging.PrintWarning<Logger>($"【App Version Unpassed (X.Y.Z)】LOCAL APP_VER: v{localCfg.APP_VERSION} != SERVER APP_VER: v{hostCfg.APP_VERSION}");
                            return;
                        }
                        else
                        {
                            string hostCfgJson = JsonConvert.SerializeObject(hostCfg);

                            // 儲存本地主程式版號
                            BundleConfig.saver.SaveString(BundleConfig.LAST_APP_VERSION_KEY, hostCfgJson);
                            PatchManager.platform = hostCfg.PLATFORM;
                            PatchManager.appVersion = hostCfg.APP_VERSION;
                            Logging.PrintInfo<Logger>($"【App Version Passed (X.Y.Z)】LOCAL APP_VER: v{localCfg.APP_VERSION} == SERVER APP_VER: v{hostCfg.APP_VERSION}");
                            this._machine.ChangeState<FsmInitPatchMode>();
                        }
                    }
                    else
                    {
                        // 比對 X.Y 版號
                        string[] localAppVersionArgs = localCfg.APP_VERSION.Split('.');
                        string[] hostAppVersionArgs = hostCfg.APP_VERSION.Split('.');

                        string localVersion = $"{localAppVersionArgs[0]}.{localAppVersionArgs[1]}";
                        string hostVersion = $"{hostAppVersionArgs[0]}.{hostAppVersionArgs[1]}";

                        // 比對 Local 與 Host 的主程式 X.Y 版號
                        if (localVersion != hostVersion)
                        {
                            // Do GoToAppStore

                            // Emit go to app store event
                            PatchEvents.PatchGoToAppStore.SendEventMessage();
                            // Remove last group name
                            PatchManager.DelLastGroupInfo();

                            Logging.PrintWarning<Logger>("Application version inconsistent, require to update application (go to store)");
                            Logging.PrintWarning<Logger>($"【App Version Unpassed (X.Y)】LOCAL APP_VER: v{localVersion} ({localCfg.APP_VERSION}) != SERVER APP_VER: v{hostVersion} ({hostCfg.APP_VERSION})");
                            return;
                        }
                        else
                        {
                            // 僅 CDN 使用 (local 不需要序列化儲存該參數)
                            hostCfg.SEMANTIC_RULE = null;

                            // 序列化 Host 配置文件
                            string hostCfgJson = JsonConvert.SerializeObject(hostCfg);

                            // 寫入完整版號至 Local
                            if (localCfg.APP_VERSION != hostCfg.APP_VERSION)
                            {
                                // 寫入 Host 的配置文件至 Local
                                string localCfgPath = BundleConfig.GetLocalSandboxAppConfigPath();
                                File.WriteAllText(localCfgPath, hostCfgJson);
                            }

                            // 儲存本地主程式版號
                            BundleConfig.saver.SaveString(BundleConfig.LAST_APP_VERSION_KEY, hostCfgJson);
                            PatchManager.platform = hostCfg.PLATFORM;
                            PatchManager.appVersion = hostCfg.APP_VERSION;
                            Logging.PrintInfo<Logger>($"【App Version Passed (X.Y)】LOCAL APP_VER: v{localVersion} ({localCfg.APP_VERSION}) == SERVER APP_VER: v{hostVersion} ({hostCfg.APP_VERSION})");
                            this._machine.ChangeState<FsmInitPatchMode>();
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

                if (PackageManager.isInitialized)
                {
                    Logging.Print<Logger>("(Check) Check Patch");
                    this._machine.ChangeState<FsmPatchVersionUpdate>();
                    return;
                }

                // 確保 preset packages 初始
                bool appInitialized = await PackageManager.InitPresetAppPackages();
                bool dlcInitialized = await PackageManager.InitPresetDlcPackages();
                bool isInitialized = appInitialized && dlcInitialized;
                if (isInitialized)
                {
                    Logging.Print<Logger>("(Init) Init Patch");
                    this._machine.ChangeState<FsmPatchVersionUpdate>();
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
                PatchManager.GetInstance().MarkRepairAsDone();
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

                // Combine packages
                var appPackages = PackageManager.GetPresetAppPackages();
                var dlcPackages = PackageManager.GetPresetDlcPackages();
                var packages = appPackages.Concat(dlcPackages).ToArray();

                bool succeed = false;
                string currentPackageName = string.Empty;
                Dictionary<string, string> patchVersions = new Dictionary<string, string>();
                foreach (var package in packages)
                {
                    currentPackageName = package.PackageName;
                    var operation = package.RequestPackageVersionAsync();
                    await operation;

                    if (operation.Status == EOperationStatus.Succeed)
                    {
                        succeed = true;
                        patchVersions.TryAdd(package.PackageName, operation.PackageVersion);
                    }
                    else
                    {
                        succeed = false;
                        break;
                    }
                }

                // 如果尚未配置 Preset packages, 直接標記成 true
                if (packages.Length == 0)
                    succeed = true;

                if (succeed)
                {
                    PatchManager.patchVersions = patchVersions;
                    PatchManager.isLastPackageVersionInWeakHostMode = false;
                    this._machine.ChangeState<FsmPatchManifestUpdate>();
                }
                else
                {
                    #region Weak Host Mode
                    if (BundleConfig.playModeParameters.enableLastLocalVersionsCheckInWeakNetwork)
                    {
                        patchVersions.Clear();
                        foreach (var package in packages)
                        {
                            // 獲取上一次本地資源版號
                            string lastVersion = BundleConfig.saver.GetData(BundleConfig.LAST_PACKAGE_VERSIONS_KEY, package.PackageName, string.Empty);
                            if (string.IsNullOrEmpty(lastVersion))
                            {
                                PatchEvents.PatchVersionUpdateFailed.SendEventMessage();
                                Logging.PrintError<Logger>($"Package: {package.PackageName}. Local version record not found, resources need to be updated (Please connect to the network)!");
                                return;
                            }
                            patchVersions.TryAdd(package.PackageName, lastVersion);
                        }

                        PatchManager.patchVersions = patchVersions;
                        PatchManager.isLastPackageVersionInWeakHostMode = true;
                        this._machine.ChangeState<FsmPatchManifestUpdate>();
                    }
                    #endregion
                    else
                    {
                        PatchEvents.PatchVersionUpdateFailed.SendEventMessage();
                        Logging.PrintError<Logger>($"Package: {currentPackageName} update version failed.");
                    }
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

                // 判斷目前是否獲取的是本地版號 (如果是的話, 表示目前處於弱聯網)
                bool isLastPackageVersionInWeakHostMode = PatchManager.isLastPackageVersionInWeakHostMode;

                // Combine packages
                var appPackages = PackageManager.GetPresetAppPackages();
                var dlcPackages = PackageManager.GetPresetDlcPackages();
                var packages = appPackages.Concat(dlcPackages).ToArray();
                var packageVersions = PatchManager.patchVersions;

                bool succeed = false;
                string currentPackageName = string.Empty;
                for (int i = 0; i < packages.Length; i++)
                {
                    currentPackageName = packages[i].PackageName;
                    packageVersions.TryGetValue(packages[i].PackageName, out string version);
                    var operation = packages[i].UpdatePackageManifestAsync(version);
                    await operation;

                    if (operation.Status == EOperationStatus.Succeed)
                    {
                        // 儲存本地資源版本
                        BundleConfig.saver.SaveData(BundleConfig.LAST_PACKAGE_VERSIONS_KEY, currentPackageName, version);
                        succeed = true;
                        Logging.PrintInfo<Logger>($"Package: {packages[i].PackageName} Update completed successfully.");
                    }
                    else
                    {
                        succeed = false;
                        break;
                    }
                }

                // 如果尚未配置 Preset packages, 直接標記成 true
                if (packages.Length == 0)
                    succeed = true;

                if (succeed)
                {
                    if (!BundleConfig.playModeParameters.createPresetPackagesDownloader && !isLastPackageVersionInWeakHostMode)
                        this._machine.ChangeState<FsmDownloadOver>();
                    else
                        this._machine.ChangeState<FsmCreateDownloader>();
                }
                else
                {
                    #region Weak Host Mode
                    if (BundleConfig.playModeParameters.enableLastLocalVersionsCheckInWeakNetwork)
                    {
                        PatchEvents.PatchManifestUpdateFailed.SendEventMessage();
                        Logging.PrintError<Logger>($"Package: {currentPackageName}. Failed to load the local resource manifest file. Resource update is required (Please connect to the network)!");
                    }
                    #endregion
                    else
                    {
                        PatchEvents.PatchManifestUpdateFailed.SendEventMessage();
                        Logging.PrintError<Logger>($"Package: {currentPackageName} update manifest failed.");
                    }
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
                if (!PatchManager.GetInstance().IsCheck())
                    PatchManager.GetInstance().MarkCheckState();
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

                // EditorSimulateMode skip directly
                if (BundleConfig.playMode == BundleConfig.PlayMode.EditorSimulateMode)
                {
                    this._machine.ChangeState<FsmPatchDone>();
                    return;
                }

                // Combine packages
                var appPackages = PackageManager.GetPresetAppPackages();
                var dlcPackages = PackageManager.GetPresetDlcPackages();
                var packages = appPackages.Concat(dlcPackages).ToArray();

                // 判斷目前是否獲取的是本地版號 (如果是的話, 表示目前處於弱聯網)
                bool isLastPackageVersionInWeakHostMode = PatchManager.isLastPackageVersionInWeakHostMode;

                // 獲取預設群包標籤
                string defaultGroupTag = BundleConfig.DEFAULT_GROUP_TAG;

                // 獲取上一次群包訊息
                GroupInfo lastGroupInfo = PatchManager.GetLastGroupInfo();
                Logging.PrintInfo<Logger>($"Get last GroupName: {lastGroupInfo?.groupName}");

                #region Create Downloader by Tags (群包處理)
                Dictionary<string, GroupInfo> newGroupInfos = new Dictionary<string, GroupInfo>();
                if (packages.Length > 0)
                {
                    #region GroupInfo request
                    // 獲取 host patch config, 需要處理群組包下載邏輯
                    string url = await BundleConfig.GetHostServerPatchConfigPath();
                    string hostCfgJson = await Requester.RequestText(url, null, null, null, false);
                    bool isLastHostCfgJson = false;
                    if (string.IsNullOrEmpty(hostCfgJson))
                    {
                        // 弱聯網處理
                        if (BundleConfig.playModeParameters.enableLastLocalVersionsCheckInWeakNetwork)
                        {
                            hostCfgJson = BundleConfig.saver.GetString(BundleConfig.LAST_PATCH_CONFIG_KEY, string.Empty);
                            if (string.IsNullOrEmpty(hostCfgJson))
                            {
                                string errorMsg = $"Failed to request patch config from URL: {url}";
                                PatchEvents.PatchDownloadFailed.SendEventMessage(errorMsg);
                                Logging.PrintError<Logger>($"[{(BundleConfig.playModeParameters.enableLastLocalVersionsCheckInWeakNetwork)}] {errorMsg}.");
                                return;
                            }
                            else
                                isLastHostCfgJson = true;
                        }
                        else
                        {
                            string errorMsg = $"Failed to request patch config from URL: {url}";
                            PatchEvents.PatchDownloadFailed.SendEventMessage(errorMsg);
                            Logging.PrintError<Logger>($"{errorMsg}.");
                            return;
                        }
                    }

                    // 儲存本地資源群包配置數據
                    if (!isLastHostCfgJson)
                        BundleConfig.saver.SaveString(BundleConfig.LAST_PATCH_CONFIG_KEY, hostCfgJson);
                    PatchConfig patchCfg = JsonConvert.DeserializeObject<PatchConfig>(hostCfgJson);
                    List<GroupInfo> patchGroupInfos = patchCfg.GROUP_INFOS;
                    #endregion

                    string key;
                    int totalDownloadCount;
                    long totalDownloadBytes;
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
                }

                if (newGroupInfos.Count > 0)
                {
                    #region Weak Host Mode
                    if (isLastPackageVersionInWeakHostMode)
                    {
                        string errorMsg = "Local resources are incomplete. Update required (Please connect to the network)!";
                        Logging.PrintError<Logger>($"{errorMsg}");
                        // 當突然失去聯網時, 必須重新從獲取資源版本的流程開始運行, 因為當網絡恢復時, 則可以正確獲取遠端版本進行更新
                        PatchEvents.PatchVersionUpdateFailed.SendEventMessage();
                    }
                    #endregion
                    else
                    {
                        Logging.PrintInfo<Logger>($"Auto check last GroupName: {lastGroupInfo?.groupName}");
                        Logging.PrintInfo<Logger>($"Found total group {newGroupInfos.Count} to choose download =>\n{JsonConvert.SerializeObject(newGroupInfos)}");
                        PatchEvents.PatchCreateDownloader.SendEventMessage(newGroupInfos.Values.ToArray());

                        /**
                         * 開始等待使用者選擇是否開始下載
                         */
                    }
                }
                else
                {
                    Logging.PrintInfo<Logger>($"GroupName: {lastGroupInfo?.groupName} not found any download files!!!");
                    this._machine.ChangeState<FsmDownloadOver>();
                }
            }
        }

        /// <summary>
        /// 7. 下載資源文件
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
                // 下載資源文件中
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
                // Combine packages
                var appPackages = PackageManager.GetPresetAppPackages();
                var dlcPackages = PackageManager.GetPresetDlcPackages();
                var packages = appPackages.Concat(dlcPackages).ToArray();

                // Get last GroupInfo by UserEvent Set
                GroupInfo lastGroupInfo = PatchManager.GetLastGroupInfo();

                Logging.PrintInfo<Logger>($"Start Download Group Name: {lastGroupInfo?.groupName}, Tags: {JsonConvert.SerializeObject(lastGroupInfo?.tags)}");

                List<ResourceDownloaderOperation> mainDownloaders = new List<ResourceDownloaderOperation>();
                foreach (var package in packages)
                {
                    if (lastGroupInfo != null)
                    {
                        if (lastGroupInfo.groupName == BundleConfig.DEFAULT_GROUP_TAG)
                            mainDownloaders.Add(package.CreateResourceDownloader(BundleConfig.maxConcurrencyDownloadCount, BundleConfig.failedRetryCount));
                        else
                            mainDownloaders.Add(package.CreateResourceDownloader(lastGroupInfo.tags, BundleConfig.maxConcurrencyDownloadCount, BundleConfig.failedRetryCount));
                    }
                }

                // Set main downloaders
                PatchManager.GetInstance().mainDownloaders = mainDownloaders.ToArray();

                // Combine all main downloaders count and bytes
                int totalCount = 0;
                long totalBytes = 0;
                foreach (var downloader in mainDownloaders)
                {
                    totalCount += downloader.TotalDownloadCount;
                    totalBytes += downloader.TotalDownloadBytes;
                }

#if !UNITY_WEBGL
                // Check flag if enabled
                if (BundleConfig.playModeParameters.enableDiskSpaceCheckForPresetPackagesDownloader)
                {
                    // Check disk space
                    int availableDiskSpaceMegabytes = BundleUtility.CheckAvailableDiskSpaceMegabytes();
                    int patchTotalMegabytes = (int)(totalBytes / (1 << 20));
                    Logging.PrintInfo<Logger>($"[Disk Space Check] Available Disk Space Size: {BundleUtility.GetMegabytesToString(availableDiskSpaceMegabytes)}, Patch Total Size: {BundleUtility.GetBytesToString((ulong)totalBytes)}");
                    if (patchTotalMegabytes > availableDiskSpaceMegabytes)
                    {
                        PatchEvents.PatchCheckDiskNotEnoughSpace.SendEventMessage(availableDiskSpaceMegabytes, (ulong)totalBytes);
                        Logging.PrintError<Logger>($"Disk Not Enough Space!!! Available Disk Space Size: {BundleUtility.GetMegabytesToString(availableDiskSpaceMegabytes)}, Patch Total Size: {BundleUtility.GetBytesToString((ulong)totalBytes)}");
                        return;
                    }
                }
#endif

                // Begin Download
                int currentCount = 0;
                long currentBytes = 0;
                var downloadSpeedCalculator = new DownloadSpeedCalculator();
                downloadSpeedCalculator.onDownloadSpeedProgress = PatchEvents.PatchDownloadProgression.SendEventMessage;
                foreach (var downloader in mainDownloaders)
                {
                    int lastCount = 0;
                    long lastBytes = 0;
                    downloader.DownloadErrorCallback = (DownloadErrorData data) =>
                    {
                        PatchEvents.PatchDownloadFailed.SendEventMessage(data.FileName, data.ErrorInfo);
                    };
                    downloader.DownloadUpdateCallback = (DownloadUpdateData data) =>
                    {
                        currentCount += data.CurrentDownloadCount - lastCount;
                        lastCount = data.CurrentDownloadCount;
                        currentBytes += data.CurrentDownloadBytes - lastBytes;
                        lastBytes = data.CurrentDownloadBytes;
                        downloadSpeedCalculator.OnDownloadProgress(totalCount, currentCount, totalBytes, currentBytes);
                    };

                    downloader.BeginDownload();
                    await downloader;

                    if (downloader.Status != EOperationStatus.Succeed)
                    {
                        string errorMsg = $"Downloader did not succeed in completing the operation.";
                        Logging.PrintError<Logger>($"{errorMsg}.");
                        return;
                    }
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

                // Combine packages
                var appPackages = PackageManager.GetPresetAppPackages();
                var dlcPackages = PackageManager.GetPresetDlcPackages();
                var packages = appPackages.Concat(dlcPackages).ToArray();

                foreach (var package in packages)
                {
                    var clearUnusedBundleFilesOperation = package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
                    var clearUnusedManifestFilesOperation = package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedManifestFiles);
                    await clearUnusedBundleFilesOperation;
                    await clearUnusedManifestFilesOperation;
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
                PatchManager.GetInstance().MarkPatchAsDone();
                Logging.PrintInfo<Logger>("(Power by YooAsset) All preset patches are done.");
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
