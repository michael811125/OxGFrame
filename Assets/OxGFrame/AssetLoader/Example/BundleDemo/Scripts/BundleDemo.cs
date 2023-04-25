using OxGFrame.AssetLoader;
using OxGFrame.AssetLoader.Bundle;
using OxGFrame.AssetLoader.PatchEvent;
using OxGFrame.AssetLoader.PatchFsm;
using OxGFrame.AssetLoader.Utility;
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

    public GameObject controlBtns = null;
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

    public void Start()
    {
        this.progress.size = 0f;
        this.info.text = string.Empty;

        this.downloadBtns.SetActive(false);
        this.progressGroup.SetActive(false);
        this.bundleBtns.SetActive(false);

        // Init Patch Events
        this._InitPatchEvents();
    }

    #region Patch Event
    private void _InitPatchEvents()
    {
        // 1. PatchFsmState
        // 2. PatchGoToAppStore
        // 3. PatchAppVersionUpdateFailed
        // 4. PatchInitPatchModeFailed
        // 5. PatchCreateDownloader
        // 6. PatchDownloadProgression
        // 7. PatchVersionUpdateFailed
        // 8. PatchManifestUpdateFailed
        // 9. PatchDownloadFailed

        #region Add PatchEvents Handle
        this._patchEvents.AddListener<PatchEvents.PatchFsmState>(this._OnHandleEventMessage);
        this._patchEvents.AddListener<PatchEvents.PatchGoToAppStore>(this._OnHandleEventMessage);
        this._patchEvents.AddListener<PatchEvents.PatchAppVersionUpdateFailed>(this._OnHandleEventMessage);
        this._patchEvents.AddListener<PatchEvents.PatchInitPatchModeFailed>(this._OnHandleEventMessage);
        this._patchEvents.AddListener<PatchEvents.PatchCreateDownloader>(this._OnHandleEventMessage);
        this._patchEvents.AddListener<PatchEvents.PatchDownloadProgression>(this._OnHandleEventMessage);
        this._patchEvents.AddListener<PatchEvents.PatchVersionUpdateFailed>(this._OnHandleEventMessage);
        this._patchEvents.AddListener<PatchEvents.PatchManifestUpdateFailed>(this._OnHandleEventMessage);
        this._patchEvents.AddListener<PatchEvents.PatchDownloadFailed>(this._OnHandleEventMessage);
        #endregion
    }

    private void _OnHandleEventMessage(IEventMessage message)
    {
        if (message is PatchEvents.PatchFsmState)
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
                    if (!this.controlBtns.activeSelf) this.controlBtns.SetActive(true);
                    if (!this.bundleBtns.activeSelf) this.bundleBtns.SetActive(true);
                    if (this.progressGroup.activeSelf) this.progressGroup.SetActive(false);
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
                $"CurrentSize: {BundleUtility.GetBytesToString((ulong)downloadInfo.currentDownloadSizeBytes)}, " +
                $"Speed: {BundleUtility.GetSpeedBytesToString((ulong)downloadInfo.downloadSpeedSizeBytes)}"
            );

            this._UpdateDownloadInfo
            (
                downloadInfo.progress,
                downloadInfo.currentDownloadCount,
                downloadInfo.currentDownloadSizeBytes,
                downloadInfo.downloadSpeedSizeBytes,
                downloadInfo.totalDownloadCount,
                downloadInfo.totalDownloadSizeBytes
            );
            #endregion
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
        else if (message is PatchEvents.PatchDownloadFailed)
        {
            // Show Patch Download Files Failed Retry UI

            this._retryType = 5;
            this.ShowRetryWindow("Patch Download Failed");
        }
        else
        {
            throw new System.NotImplementedException($"{message.GetType()}");
        }
    }

    private void _UpdateDownloadInfo(float progress, int dlCount, long dlBytes, long dlSpeed, int totalCount, long totalBytes)
    {
        if (!this.progressGroup.activeSelf) this.progressGroup.SetActive(true);

        var strBuilder = new StringBuilder();
        strBuilder.Append($"Patch Size: {BundleUtility.GetBytesToString((ulong)totalBytes)}");
        strBuilder.Append($", {dlCount} (DC) / {totalCount} (PC)");
        strBuilder.Append($"\nDownload Size: {BundleUtility.GetBytesToString((ulong)dlBytes)}, Download Speed: {BundleUtility.GetSpeedBytesToString((ulong)dlSpeed)}");
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

    public void StartDownload()
    {
        // Send select groupInfo and start download
        UserEvents.UserBeginDownload.SendEventMessage(this._selectGroupInfo);

        if (this.confirmWindow.activeSelf) this.confirmWindow.SetActive(false);
    }

    public void RetryEvent()
    {
        #region Retry Event
        switch (this._retryType)
        {
            case 1:
                // Add send event in Retry UI (click event)
                UserEvents.UserTryAppVersionUpdate.SendEventMessage();
                break;
            case 2:
                // Add send event in Retry UI (click event)
                UserEvents.UserTryInitPatchMode.SendEventMessage();
                break;
            case 3:
                // Add send event in Retry UI (click event)
                UserEvents.UserTryPatchVersionUpdate.SendEventMessage();
                break;

            case 4:
                // Add send event in Retry UI (click event)
                UserEvents.UserTryPatchManifestUpdate.SendEventMessage();
                break;

            case 5:
                // Add send event in Retry UI (click event)
                UserEvents.UserTryCreateDownloader.SendEventMessage();
                break;
        }
        #endregion

        if (this.retryWindow.activeSelf) this.retryWindow.SetActive(false);
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

            LayoutRebuilder.ForceRebuildLayoutImmediate(this.confirmWindow.transform.Find("bg").GetComponent<RectTransform>());
        }
    }

    public void ShowRetryWindow(string msg)
    {
        var label = this.retryWindow.transform.Find("bg/Text").GetComponent<Text>();
        label.text = msg;

        if (!this.retryWindow.activeSelf) this.retryWindow.SetActive(true);
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

    public async void PreloadBundle()
    {
        // if assetName has prefix "res#" will use Resources to load
        await AssetLoaders.PreloadAssetAsync(this.assetName);
    }

    public async void LoadBundle()
    {
        // Async LoadAsset
        GameObject go = await AssetLoaders.LoadAssetAsync<GameObject>(this.assetName, (p, r, t) =>
        {
            Debug.Log($"Load: {p}, {r}, {t}");
        });
        if (go != null) Instantiate(go, this.container.transform);

        // Sync LoadAsset
        //GameObject go = AssetLoaders.LoadAsset<GameObject>(this.assetName, (p, r, t) =>
        //{
        //    Debug.Log($"Load: {p}, {r}, {t}");
        //});
        //if (go != null) Instantiate(go, this.container.transform);

        // Async InstantiateAsset
        //await AssetLoaders.InstantiateAssetAsync<GameObject>(this.assetName, this.container.transform, (p, r, t) =>
        //{
        //    Debug.Log($"Load: {p}, {r}, {t}");
        //});

        // Sync InstantiateAsset
        //AssetLoaders.InstantiateAsset<GameObject>(this.assetName, this.container.transform, (p, r, t) =>
        //{
        //    Debug.Log($"Load: {p}, {r}, {t}");
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
            AssetLoaders.UnloadAsset(this.assetName);
        }
    }

    public void ReleaseBundle()
    {
        // Destroy all first
        foreach (Transform t in this.container.transform) Destroy(t.gameObject);
        // Release all
        AssetLoaders.ReleaseBundleAssets();
    }
    #endregion
}
