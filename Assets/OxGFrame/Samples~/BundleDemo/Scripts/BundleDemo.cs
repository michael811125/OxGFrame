using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader;
using OxGFrame.AssetLoader.Bundle;
using OxGFrame.AssetLoader.PatchEvent;
using OxGFrame.AssetLoader.PatchFsm;
using OxGFrame.AssetLoader.Utility;
using System.Collections;
using System.Text;
using UniFramework.Event;
using UnityEngine;
using UnityEngine.UI;

public class BundleDemo : MonoBehaviour
{
    public Text msg = null;
    public Scrollbar progress = null;
    public Text percentage = null;
    public Text info = null;
    public Text versionTxt = null;

    public Toggle checkState = null;
    public Toggle repairState = null;
    public Toggle doneState = null;

    public GameObject controlBtns = null;
    public GameObject ctrlCheckBtn = null;
    public GameObject ctrlRepairBtn = null;
    public GameObject downloadBtns = null;
    public GameObject bundleBtns = null;
    public GameObject progressGroup = null;

    public GameObject retryWindow = null;
    public GameObject fixWindow = null;
    public GameObject confirmWindow = null;

    public ToggleGroup groupToggleContainer = null;
    public Toggle groupToggle = null;

    private EventGroup _patchEvents = new EventGroup();
    private int _retryType = 0;

    private IEnumerator Start()
    {
        this.progress.size = 0f;
        this.info.text = string.Empty;

        this.downloadBtns.SetActive(false);
        this.progressGroup.SetActive(false);
        this.bundleBtns.SetActive(false);
        this.versionTxt.gameObject.SetActive(false);

        // Init Patch Events
        this._InitPatchEvents();

        // Disable Buttons
        var btns = this.controlBtns.GetComponentsInChildren<Button>();
        foreach (var btn in btns)
        {
            btn.interactable = false;
        }

        // Wait Until IsInitialized
        while (!AssetPatcher.IsInitialized()) yield return null;

        // Show control buttons
        if (!this.controlBtns.activeSelf) this.controlBtns.SetActive(true);

        // Enable Buttons
        foreach (var btn in btns)
        {
            btn.interactable = true;
        }
    }

    private void Update()
    {
        this.checkState.isOn = AssetPatcher.IsCheck();
        this.repairState.isOn = AssetPatcher.IsRepair();
        this.doneState.isOn = AssetPatcher.IsDone();
    }

    #region Patch Event
    private void _InitPatchEvents()
    {
        // 0. PatchRepairFailed
        // 1. PatchFsmState
        // 2. PatchGoToAppStore
        // 3. PatchAppVersionUpdateFailed
        // 4. PatchInitPatchModeFailed
        // 5. PatchVersionUpdateFailed
        // 6. PatchManifestUpdateFailed
        // 7. PatchCreateDownloader
        // 8. PatchCheckDiskNotEnoughSpace
        // 9. PatchDownloadProgression
        // 10. PatchDownloadFailed
        // 11. PatchDownloadCanceled

        #region Add PatchEvents Handle
        this._patchEvents.AddListener<PatchEvents.PatchRepairFailed>(this._OnHandleEventMessage);
        this._patchEvents.AddListener<PatchEvents.PatchFsmState>(this._OnHandleEventMessage);
        this._patchEvents.AddListener<PatchEvents.PatchGoToAppStore>(this._OnHandleEventMessage);
        this._patchEvents.AddListener<PatchEvents.PatchAppVersionUpdateFailed>(this._OnHandleEventMessage);
        this._patchEvents.AddListener<PatchEvents.PatchInitPatchModeFailed>(this._OnHandleEventMessage);
        this._patchEvents.AddListener<PatchEvents.PatchVersionUpdateFailed>(this._OnHandleEventMessage);
        this._patchEvents.AddListener<PatchEvents.PatchManifestUpdateFailed>(this._OnHandleEventMessage);
        this._patchEvents.AddListener<PatchEvents.PatchCreateDownloader>(this._OnHandleEventMessage);
        this._patchEvents.AddListener<PatchEvents.PatchCheckDiskNotEnoughSpace>(this._OnHandleEventMessage);
        this._patchEvents.AddListener<PatchEvents.PatchDownloadProgression>(this._OnHandleEventMessage);
        this._patchEvents.AddListener<PatchEvents.PatchDownloadFailed>(this._OnHandleEventMessage);
        this._patchEvents.AddListener<PatchEvents.PatchDownloadCanceled>(this._OnHandleEventMessage);
        #endregion
    }

    private void _OnHandleEventMessage(IEventMessage message)
    {
        if (message is PatchEvents.PatchRepairFailed)
        {
            // Show Patch Failed Retry UI

            this._retryType = 0;
            this.ShowRetryWindow("Patch Repair Failed");
        }
        else if (message is PatchEvents.PatchFsmState)
        {
            // Display Patch State Msg
            #region PatchFsmState
            PatchEvents.PatchFsmState msgData = message as PatchEvents.PatchFsmState;

            switch (msgData.stateNode)
            {
                case PatchFsmStates.FsmPatchRepair:
                    this.msg.text = "Patch Repair";
                    break;
                case PatchFsmStates.FsmPatchPrepare:
                    this.msg.text = "Patch Prepare";
                    if (this.controlBtns.activeSelf) this.controlBtns.SetActive(false);
                    if (this.downloadBtns.activeSelf) this.downloadBtns.SetActive(false);
                    if (this.progressGroup.activeSelf) this.progressGroup.SetActive(false);
                    if (this.bundleBtns.activeSelf) this.bundleBtns.SetActive(false);
                    if (this.versionTxt.gameObject.activeSelf) this.versionTxt.gameObject.SetActive(false);
                    break;
                case PatchFsmStates.FsmAppVersionUpdate:
                    this.msg.text = "App Version Update";
                    break;
                case PatchFsmStates.FsmInitPatchMode:
                    this.msg.text = "Init Patch Mode";
                    break;
                case PatchFsmStates.FsmPatchVersionUpdate:
                    this.msg.text = "Patch Version Update";
                    break;
                case PatchFsmStates.FsmPatchManifestUpdate:
                    this.msg.text = "Patch Manifest Update";
                    break;
                case PatchFsmStates.FsmCreateDownloader:
                    this.msg.text = "Create Downloader";
                    break;
                case PatchFsmStates.FsmBeginDownload:
                    this.msg.text = "Begin Download Files";
                    if (!this.progressGroup.activeSelf) this.progressGroup.SetActive(true);
                    if (!this.downloadBtns.activeSelf) this.downloadBtns.SetActive(true);
                    break;
                case PatchFsmStates.FsmDownloadOver:
                    this.msg.text = "Download Over";
                    if (this.downloadBtns.activeSelf) this.downloadBtns.SetActive(false);
                    break;
                case PatchFsmStates.FsmClearCache:
                    this.msg.text = "Clear Cache";
                    break;
                case PatchFsmStates.FsmPatchDone:
                    this.msg.text = "Patch Done";
                    // get app version to display
                    string appVersion = AssetPatcher.GetAppVersion();
                    if (!string.IsNullOrEmpty(appVersion)) appVersion = $"v{appVersion}";
                    // get encoded patch version to display (recommend)
                    string encodePatchVersion = AssetPatcher.GetPatchVersion(true);
                    // get original patch version to display
                    string patchVersion = AssetPatcher.GetPatchVersion();
                    // show version text
                    this.versionTxt.text = $"version: {appVersion}<{encodePatchVersion}>\napp_version: {appVersion}\npatch_version (encoded): {encodePatchVersion}\npatch_version: {patchVersion}";
                    if (!this.controlBtns.activeSelf)
                    {
                        this.controlBtns.SetActive(true);
                        this.ctrlRepairBtn.SetActive(true);
                    }
                    if (!this.bundleBtns.activeSelf) this.bundleBtns.SetActive(true);
                    if (this.progressGroup.activeSelf) this.progressGroup.SetActive(false);
                    if (!this.versionTxt.gameObject.activeSelf) this.versionTxt.gameObject.SetActive(true);
                    break;
            }
            #endregion
        }
        else if (message is PatchEvents.PatchGoToAppStore)
        {
            // Show Go To App Store Confirm UI (add below event on confirm button)

            AssetPatcher.GoToAppStore();
        }
        else if (message is PatchEvents.PatchAppVersionUpdateFailed)
        {
            // Show App Version Update Failed Retry UI

            this._retryType = 1;
            this.ShowRetryWindow("App Version Update Failed");
        }
        else if (message is PatchEvents.PatchInitPatchModeFailed)
        {
            // Show Patch Init Patch Failed Retry UI

            this._retryType = 2;
            this.ShowRetryWindow("Init Patch Mode Failed");
        }
        else if (message is PatchEvents.PatchVersionUpdateFailed)
        {
            // Show Patch Version Update Failed Retry UI

            this._retryType = 3;
            this.ShowRetryWindow("Patch Version Update Failed");

        }
        else if (message is PatchEvents.PatchManifestUpdateFailed)
        {
            // Show Patch Manifest Update Failed Retry UI

            this._retryType = 4;
            this.ShowRetryWindow("Patch Manifest Update Failed");
        }
        else if (message is PatchEvents.PatchCreateDownloader)
        {
            // Show GroupInfos UI for user to choose which one they want to download

            // Node: Recommend foreach GroupInfos to find max size and check user disk space

            #region Show GroupInfos
            var msgData = message as PatchEvents.PatchCreateDownloader;

            // show groupInfos option
            this.ShowConfirmWindow(msgData.groupInfos);
            #endregion
        }
        else if (message is PatchEvents.PatchCheckDiskNotEnoughSpace)
        {
            // Show Disk Not Enough Space Retry UI

            // Note: You can retry create downloader again (unless, user frees up space) or submit Application.Quit event!!!

            var msgData = message as PatchEvents.PatchCheckDiskNotEnoughSpace;

            // Here use action type is 6 (Application.Quit)
            this._retryType = 6; // 5 or 6
            this.ShowRetryWindow($"Disk Not Enough Space!!!\nAvailable Disk Space Size: {BundleUtility.GetMegabytesToString(msgData.availableMegabytes)}\nPatch Total Size: {BundleUtility.GetBytesToString((ulong)msgData.patchTotalBytes)}");
        }
        else if (message is PatchEvents.PatchDownloadProgression)
        {
            #region Download Progression
            // Receive Progression
            var downloadInfo = message as PatchEvents.PatchDownloadProgression;
            Debug.Log
            (
                $"Progress: {downloadInfo.progress}, " +
                $"TotalCount: {downloadInfo.totalDownloadCount}, " +
                $"TotalSize: {BundleUtility.GetBytesToString((ulong)downloadInfo.totalDownloadSizeBytes)}, " +
                $"CurrentCount: {downloadInfo.currentDownloadCount}, " +
                $"CurrentSize: {BundleUtility.GetBytesToString((ulong)downloadInfo.currentDownloadSizeBytes)}" +
                $"DownloadSpeed: {BundleUtility.GetSpeedBytesToString((ulong)downloadInfo.downloadSpeedBytes)}"
            );

            this._UpdateDownloadInfo
            (
                downloadInfo.progress,
                downloadInfo.currentDownloadCount,
                downloadInfo.currentDownloadSizeBytes,
                downloadInfo.totalDownloadCount,
                downloadInfo.totalDownloadSizeBytes,
                downloadInfo.downloadSpeedBytes
            );
            #endregion
        }
        else if (message is PatchEvents.PatchDownloadFailed)
        {
            // Show Patch Download Files Failed Retry UI

            this._retryType = 5;
            this.ShowRetryWindow("Patch Download Failed");
        }
        else if (message is PatchEvents.PatchDownloadCanceled)
        {
            // Show Patch Download Canceled Retry UI

            this._retryType = 5;
            this.ShowRetryWindow("Patch Download Canceled");
        }
        else
        {
            throw new System.NotImplementedException($"{message.GetType()}");
        }
    }

    private void _UpdateDownloadInfo(float progress, int dlCount, long dlBytes, int totalCount, long totalBytes, long dlSpeedBytes)
    {
        if (!this.progressGroup.activeSelf) this.progressGroup.SetActive(true);

        var strBuilder = new StringBuilder();
        strBuilder.Append($"Patch Size: {BundleUtility.GetBytesToString((ulong)totalBytes)}");
        strBuilder.Append($", {dlCount} (DC) / {totalCount} (PC)");
        strBuilder.Append($"\nCurrent Download Size: {BundleUtility.GetBytesToString((ulong)dlBytes)}, Download Speed: {BundleUtility.GetSpeedBytesToString((ulong)dlSpeedBytes)}");
        this.info.text = strBuilder.ToString();

        this.progress.size = progress;
        this.percentage.text = (progress * 100).ToString("f0") + "%";

        // Patch Size: 00,  00(DC) / 00(PC)
        // Download Size: 00 , Download Speed: 00 / s
    }
    #endregion

    #region Operation Events
    public void StartExecute()
    {
        // patch check
        AssetPatcher.Check();
    }

    public void RepairPatch()
    {
        // patch repair (will delete all save patch and run patch fsm again)
        AssetPatcher.Repair();

        if (this.fixWindow.activeSelf) this.fixWindow.SetActive(false);
    }

    public void ResumeDownload()
    {
        // resume main downloader
        AssetPatcher.Resume();
    }

    public void PauseDownload()
    {
        // puase main downloader
        AssetPatcher.Pause();
    }

    public void CancelDownload()
    {
        // cancel main downloader
        AssetPatcher.Cancel();
    }

    public void StartDownload()
    {
        // Send select groupInfo and start download
        PatchUserEvents.UserBeginDownload.SendEventMessage(this._selectGroupInfo);

        if (this.confirmWindow.activeSelf) this.confirmWindow.SetActive(false);
    }

    public void RetryEvent()
    {
        if (this.retryWindow.activeSelf) this.retryWindow.SetActive(false);

        #region Retry Event
        switch (this._retryType)
        {
            case 0:
                // Add send event in Retry UI (click event)
                PatchUserEvents.UserTryPatchRepair.SendEventMessage();
                break;

            case 1:
                // Add send event in Retry UI (click event)
                PatchUserEvents.UserTryAppVersionUpdate.SendEventMessage();
                break;

            case 2:
                // Add send event in Retry UI (click event)
                PatchUserEvents.UserTryInitPatchMode.SendEventMessage();
                break;

            case 3:
                // Add send event in Retry UI (click event)
                PatchUserEvents.UserTryPatchVersionUpdate.SendEventMessage();
                break;

            case 4:
                // Add send event in Retry UI (click event)
                PatchUserEvents.UserTryPatchManifestUpdate.SendEventMessage();
                break;

            case 5:
                // Add send event in Retry UI (click event)
                PatchUserEvents.UserTryCreateDownloader.SendEventMessage();
                break;

            case 6:
                // Application quit
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                break;
        }
        #endregion
    }
    #endregion

    #region Windows
    private GroupInfo _selectGroupInfo;
    public void ShowConfirmWindow(GroupInfo[] groupInfos)
    {
        if (!this.confirmWindow.activeSelf) this.confirmWindow.SetActive(true);

        foreach (Transform t in this.groupToggleContainer.transform)
        {
            Destroy(t.gameObject);
        }

        if (groupInfos != null)
        {
            // show groupInfo toggles
            for (int i = 0; i < groupInfos.Length; i++)
            {
                int index = i;

                // clone toggle
                GameObject cloneToggle = Instantiate(this.groupToggle.gameObject, this.groupToggleContainer.transform);

                Toggle toggle = cloneToggle.GetComponent<Toggle>();
                toggle.gameObject.SetActive(true);

                // set toggle label text
                var label = toggle.transform.Find("Label").GetComponent<Text>();
                label.text = $"{groupInfos[i].groupName}, Size: {BundleUtility.GetBytesToString((ulong)groupInfos[i].totalBytes)}";

                // set toggle group
                toggle.group = this.groupToggleContainer;

                // add toggle event
                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged.AddListener((isOn) =>
                {
                    if (!isOn) return;
                    this._selectGroupInfo = groupInfos[index];

                    Debug.Log($"<color=#71ffdd>Select GroupName: {this._selectGroupInfo.groupName}, Index: {index}</color>");
                });

                if (toggle.isOn)
                {
                    this._selectGroupInfo = groupInfos[index];
                    Debug.Log($"<color=#71ffdd>Select GroupName: {this._selectGroupInfo.groupName}, Index: {index}</color>");
                }
            }

            // only after active = true will affect
            LayoutRebuilder.ForceRebuildLayoutImmediate(this.confirmWindow.transform.Find("bg").GetComponent<RectTransform>());
        }
    }

    public void ShowRetryWindow(string msg)
    {
        var label = this.retryWindow.transform.Find("bg/Text").GetComponent<Text>();
        label.text = msg;

        if (!this.retryWindow.activeSelf) this.retryWindow.SetActive(true);

        // only after active = true will affect
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.retryWindow.transform.Find("bg").GetComponent<RectTransform>());
    }

    public void ShowRepairWindow()
    {
        if (!this.fixWindow.activeSelf) this.fixWindow.SetActive(true);
    }

    public void CloseRepairWindow()
    {
        if (this.fixWindow.activeSelf) this.fixWindow.SetActive(false);
    }
    #endregion

    #region Bundle Load
    [Header("BundleInfo")]
    public string assetName = "";
    public GameObject container = null;

    /*
     * [Load asset and download from specific package (Export App Bundles for CDN)]
     * 
     * var packageName = "OtherPackage";
     * bool isInitialized = await AssetPatcher.InitAppPackage(packageName, true);
     * if (isInitialized)
     * {
     *     var package = AssetPatcher.GetPackage(packageName);
     *     var downloader = AssetPatcher.GetPackageDownloader(package);
     *     Debug.Log($"Has In Local: {downloader.TotalDownloadCount == 0}, Patch Count: {downloader.TotalDownloadCount}, Patch Size: {BundleUtility.GetBytesToString((ulong)downloader.TotalDownloadBytes)}");
     *     await AssetLoaders.LoadAssetAsync<GameObject>(packageName, assetName);
     * }
     * 
     * ------------------------------------------------------------------------------------------------------
     * 
     * [Load asset and download from specific package (Export Individual DLC Bundles for CDN)]
     * 
     * var packageName = "DlcPackage";
     * bool isInitialized = await AssetPatcher.InitDlcPackage(packageName, "dlcVersion", true);
     * if (isInitialized)
     * {
     *     var package = AssetPatcher.GetPackage(packageName);
     *     var downloader = AssetPatcher.GetPackageDownloader(package);
     *     Debug.Log($"Has In Local: {downloader.TotalDownloadCount == 0}, Patch Count: {downloader.TotalDownloadCount}, Patch Size: {BundleUtility.GetBytesToString((ulong)downloader.TotalDownloadBytes)}");
     *     await AssetLoaders.LoadAssetAsync<GameObject>(packageName, assetName);
     * }
     */

    public async void PreloadBundle()
    {
        // if assetName has prefix "res#" will use Resources to load (From Default package)
        await AssetLoaders.PreloadAssetAsync<GameObject>(this.assetName);
    }

    public async void LoadBundle()
    {
        // Async LoadAsset (From Default package)
        GameObject go = await AssetLoaders.LoadAssetAsync<GameObject>(this.assetName, 0, (progress, currentCount, totalCount) =>
        {
            Debug.Log($"Load => Progress: {progress}, CurrentCount: {currentCount}, TotalCount: {totalCount}");
        });
        if (go != null) Instantiate(go, this.container.transform);

        // Sync LoadAsset
        //GameObject go = AssetLoaders.LoadAsset<GameObject>(this.assetName, (progress, currentCount, totalCount) =>
        //{
        //    Debug.Log($"Load => Progress: {progress}, CurrentCount: {currentCount}, TotalCount: {totalCount}");
        //});
        //if (go != null) Instantiate(go, this.container.transform);

        // Async InstantiateAsset
        //await AssetLoaders.InstantiateAssetAsync<GameObject>(this.assetName, this.container.transform, (progress, currentCount, totalCount) =>
        //{
        //    Debug.Log($"Load => Progress: {progress}, CurrentCount: {currentCount}, TotalCount: {totalCount}");
        //});

        // Sync InstantiateAsset
        //AssetLoaders.InstantiateAsset<GameObject>(this.assetName, this.container.transform, (progress, currentCount, totalCount) =>
        //{
        //    Debug.Log($"Load => Progress: {progress}, CurrentCount: {currentCount}, TotalCount: {totalCount}");
        //});
    }

    public void UnloadBundle()
    {
        // Destroy and Unload (must call pair)
        foreach (Transform t in this.container.transform)
        {
            // Destroy
            Destroy(t.gameObject);
            // Unload
            AssetLoaders.UnloadAsset(this.assetName).Forget();
        }

        // Unload for preload
        if (this.container.transform.childCount == 0)
        {
            if (AssetLoaders.HasInCache(this.assetName))
            {
                AssetLoaders.UnloadAsset(this.assetName).Forget();
            }
        }
    }

    public void ReleaseBundle()
    {
        // Destroy all first
        foreach (Transform t in this.container.transform)
            Destroy(t.gameObject);
        // Release all
        AssetLoaders.ReleaseBundleAssets().Forget();
    }
    #endregion
}