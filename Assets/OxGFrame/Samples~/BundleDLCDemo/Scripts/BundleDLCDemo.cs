using OxGFrame.AssetLoader;
using OxGFrame.AssetLoader.Bundle;
using OxGFrame.AssetLoader.PatchEvent;
using OxGFrame.AssetLoader.PatchFsm;
using OxGFrame.AssetLoader.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static OxGFrame.AssetLoader.Bundle.BundleConfig;

public class BundleDLCDemo : MonoBehaviour
{
    [Serializable]
    public class PkgCtrlPanel
    {
        public Text dlcName;
        public Toggle readyState;
        public Toggle beginState;
        public Toggle repairState;
        public Toggle doneState;
        public Button beginBtn;
        public Button cancelBtn;
        public Button repairBtn;
        public GameObject progressGroup;
        public Scrollbar progress;
        public Text percentage;
        public Text patchInfo;
        public Text dlcInfo;
    }

    public List<PkgCtrlPanel> pkgCtrlPanels = new List<PkgCtrlPanel>();

    private PackageOperation[] _packageOperations;
    private bool _isInitialized = false;

    private IEnumerator Start()
    {
        this._isInitialized = false;

        // Wait Until IsInitialized
        while (!AssetPatcher.IsInitialized()) yield return null;

        // Create package operations
        this._packageOperations = new PackageOperation[]
        {
            new PackageOperation
            (
                "DLC Package 1",
                new DlcPackageInfoWithBuild()
                {
                    buildMode = BuildMode.ScriptableBuildPipeline,
                    packageName = "Dlc1Package",
                    dlcVersion = "latest"
                },
                false
            ),
            new PackageOperation
            (
                "DLC Package 2",
                new DlcPackageInfoWithBuild()
                {
                    buildMode = BuildMode.ScriptableBuildPipeline,
                    packageName = "Dlc2Package",
                    dlcVersion = "latest"
                },
                false
            )
        };

        // Init events and display
        for (int i = 0; i < this._packageOperations.Length; i++)
        {
            int idx = i;
            var packageOperation = this._packageOperations[idx];

            // Show dlc name
            this.pkgCtrlPanels[idx].dlcName.text = packageOperation.groupInfo.groupName;

            #region Btn events
            // Begin download
            this.pkgCtrlPanels[idx].beginBtn.onClick.RemoveAllListeners();
            this.pkgCtrlPanels[idx].beginBtn.onClick.AddListener(() =>
            {
                packageOperation.Begin();
            });

            // Delete files
            this.pkgCtrlPanels[idx].repairBtn.onClick.RemoveAllListeners();
            this.pkgCtrlPanels[idx].repairBtn.onClick.AddListener(() =>
            {
                packageOperation.Repair();
            });

            // Cancel download
            this.pkgCtrlPanels[idx].cancelBtn.onClick.RemoveAllListeners();
            this.pkgCtrlPanels[idx].cancelBtn.onClick.AddListener(() =>
            {
                packageOperation.Cancel();
            });
            #endregion

            #region Package events
            // Package state event
            packageOperation.eventGroup.AddListener<PackageEvents.PatchFsmState>((message) =>
            {
                if (message is PackageEvents.PatchFsmState)
                {
                    PackageEvents.PatchFsmState msgData = message as PackageEvents.PatchFsmState;
                    switch (msgData.stateNode)
                    {
                        case PackageFsmStates.FsmPatchRepair:
                            break;
                        case PackageFsmStates.FsmPatchPrepare:
                            if (this.pkgCtrlPanels[idx].progressGroup.activeSelf) this.pkgCtrlPanels[idx].progressGroup.SetActive(false);
                            if (this.pkgCtrlPanels[idx].dlcInfo.gameObject.activeSelf) this.pkgCtrlPanels[idx].dlcInfo.gameObject.SetActive(false);
                            break;
                        case PackageFsmStates.FsmInitPatchMode:
                            break;
                        case PackageFsmStates.FsmPatchVersionUpdate:
                            break;
                        case PackageFsmStates.FsmPatchManifestUpdate:
                            break;
                        case PackageFsmStates.FsmCreateDownloader:
                            break;
                        case PackageFsmStates.FsmBeginDownload:
                            if (!this.pkgCtrlPanels[idx].progressGroup.activeSelf) this.pkgCtrlPanels[idx].progressGroup.SetActive(true);
                            break;
                        case PackageFsmStates.FsmDownloadOver:
                            break;
                        case PackageFsmStates.FsmClearCache:
                            break;
                        case PackageFsmStates.FsmPatchDone:
                            if (this.pkgCtrlPanels[idx].progressGroup.activeSelf) this.pkgCtrlPanels[idx].progressGroup.SetActive(false);
                            if (!this.pkgCtrlPanels[idx].dlcInfo.gameObject.activeSelf) this.pkgCtrlPanels[idx].dlcInfo.gameObject.SetActive(true);
                            this.pkgCtrlPanels[idx].dlcInfo.text = $"DLC Size: {BundleUtility.GetBytesToString((ulong)packageOperation.groupInfo.totalBytes)}, Skip Download: {packageOperation.skipDownload}";
                            break;
                    }
                }
            });

            // Download Progression event
            packageOperation.eventGroup.AddListener<PackageEvents.PatchDownloadProgression>((message) =>
            {
                if (message is PackageEvents.PatchDownloadProgression)
                {
                    var downloadInfo = message as PackageEvents.PatchDownloadProgression;

                    this.pkgCtrlPanels[idx].progress.size = downloadInfo.progress;
                    this.pkgCtrlPanels[idx].percentage.text = (downloadInfo.progress * 100).ToString("f0") + "%";
                    this.pkgCtrlPanels[idx].patchInfo.text = $"Patch Size: {BundleUtility.GetBytesToString((ulong)downloadInfo.totalDownloadSizeBytes)}";

                    Debug.Log($"{packageOperation.groupInfo.groupName}, Load => Progress: {downloadInfo.progress}, CurrentCount: {downloadInfo.currentDownloadCount}, TotalCount: {downloadInfo.totalDownloadCount}, TotalBytes: {downloadInfo.totalDownloadSizeBytes}");
                }
            });

            // Download cancel event
            packageOperation.eventGroup.AddListener<PackageEvents.PatchDownloadCanceled>((message) =>
            {
                if (message is PackageEvents.PatchDownloadCanceled)
                {
                    Debug.Log($"{packageOperation.groupInfo.groupName} canceled.");
                }
            });
            #endregion
        }

        this._isInitialized = true;
    }

    private void Update()
    {
        if (!this._isInitialized) return;

        // Update states
        for (int i = 0; i < this._packageOperations.Length; i++)
        {
            this.pkgCtrlPanels[i].readyState.isOn = this._packageOperations[i].IsReady();
            this.pkgCtrlPanels[i].beginState.isOn = this._packageOperations[i].IsBegin();
            this.pkgCtrlPanels[i].repairState.isOn = this._packageOperations[i].IsRepair();
            this.pkgCtrlPanels[i].doneState.isOn = this._packageOperations[i].IsDone();
        }
    }
}
