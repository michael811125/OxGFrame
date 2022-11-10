using AssetLoader.Utility;
using OxGFrame.AssetLoader.Bundle;
using OxGFrame.AssetLoader.Cacher;
using OxGFrame.AssetLoader.KeyCahcer;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class BundleDemo : MonoBehaviour
{
    public Text msg = null;
    public GameObject controlBtns = null;
    public GameObject retryWindow = null;
    public GameObject progressGroup = null;
    public Scrollbar progress = null;
    public Text percentage = null;
    public Text info = null;
    public GameObject bundleBtns = null;
    public GameObject fixWindow = null;
    public GameObject confirmWindow = null;

    private bool _isStart = false;
    private bool _isStopDownload = false;

    public void Start()
    {
        this.progressGroup.SetActive(false);

        this.progress.size = 0;
        this.info.text = string.Empty;
        this.bundleBtns.SetActive(false);
    }

    private void Update()
    {
        if (!this._isStart) return;

        var status = BundleDistributor.GetInstance().executeStatus;
        switch (status)
        {
            // 正在從服務器下載配置文件
            case BundleDistributor.ExecuteStatus.DOWLOADING_CONFIG:
                this.msg.text = "Check Update\nDownloading config from server.";
                break;
            // 服務器請求錯誤 (連接錯誤)
            case BundleDistributor.ExecuteStatus.SERVER_REQUEST_ERROR:
                this.msg.text = "Server Request Error!!!";
                break;
            // 正在處理中...
            case BundleDistributor.ExecuteStatus.COMPARISON_PROCESSING:
                this.msg.text = "Check Update\nProcessing...";
                break;
            // 主程式版本不一致
            case BundleDistributor.ExecuteStatus.APP_VERSION_INCONSISTENT:
                this.msg.text = "Application version inconsistent.\nPlease go to AppStore to Download!";
                break;
            // 無需更新資源
            case BundleDistributor.ExecuteStatus.ALREADY_UP_TO_DATE:
                this.msg.text = "Already up-to-date.";
                if (this.progressGroup.activeSelf) this.progressGroup.SetActive(false);
                if (!this.controlBtns.activeSelf) this.controlBtns.SetActive(true);
                break;
            // 檢查更新包
            case BundleDistributor.ExecuteStatus.CHECKING_PATCH:
                this.msg.text = "Checking patch...";
                break;
            // 等待確認下載
            case BundleDistributor.ExecuteStatus.WAITING_FOR_CONFIRM_TO_DOWNLOAD:
                this.msg.text = "Waiting for confirm...";
                this.ShowConfirmWindow();
                break;
            // 下載更新包
            case BundleDistributor.ExecuteStatus.DOWNLOAD_PATCH:
                this.msg.text = "Downloading patch...";
                break;
            // 解壓更新包
            case BundleDistributor.ExecuteStatus.UNZIP_PATCH:
                this.msg.text = "Unzip patch...";
                if (this.controlBtns.activeSelf) this.controlBtns.SetActive(false);
                break;
            // 寫入配置文件
            case BundleDistributor.ExecuteStatus.WRITE_CONFIG:
                this.msg.text = "Processing config...";
                break;
            // 完成更新配置文件
            case BundleDistributor.ExecuteStatus.COMPLETE_UPDATE_CONFIG:
                this.msg.text = "Update config successfully.";
                break;

            // 重新嘗試下載
            case BundleDistributor.ExecuteStatus.RETRYING_DOWNLOAD:
                if (!this.retryWindow.gameObject.activeSelf) this.retryWindow.gameObject.SetActive(true);
                break;

            // AssetDatabase Mode (無需執行更新)
            case BundleDistributor.ExecuteStatus.ASSET_DATABASE_MODE:
                this.msg.text = "AssetDatabase Mode";
                break;
        }

        switch (status)
        {
            case BundleDistributor.ExecuteStatus.NONE:
            case BundleDistributor.ExecuteStatus.DOWLOADING_CONFIG:
            case BundleDistributor.ExecuteStatus.COMPARISON_PROCESSING:
            case BundleDistributor.ExecuteStatus.APP_VERSION_INCONSISTENT:
                this.info.text = string.Empty;
                break;

            case BundleDistributor.ExecuteStatus.ALREADY_UP_TO_DATE:
            case BundleDistributor.ExecuteStatus.ASSET_DATABASE_MODE:
                this.info.text = string.Empty;
                this._isStart = false;
                break;
        }

        switch (status)
        {
            case BundleDistributor.ExecuteStatus.ALREADY_UP_TO_DATE:
            case BundleDistributor.ExecuteStatus.ASSET_DATABASE_MODE:
                if (!this.bundleBtns.activeSelf) this.bundleBtns.SetActive(true);
                break;
            default:
                if (this.bundleBtns.activeSelf) this.bundleBtns.SetActive(false);
                break;
        }
    }

    private void _UpdateDownloadInfo(float progress, int dlCount, long dlBytes, int dlSpeed, ulong totalBytes)
    {
        if (!this.progressGroup.activeSelf) this.progressGroup.SetActive(true);

        var strBuilder = new StringBuilder();
        strBuilder.Append($"Patch Size: {BundleUtility.GetBytesToString(totalBytes)}");
        strBuilder.Append($", {dlCount} (DC) / {BundleUtility.GetUpdateCount(BundleDistributor.GetInstance().GetUpdateCfg())} (PC)");
        strBuilder.Append($"\nDownload Size: {BundleUtility.GetBytesToString((ulong)dlBytes)}, Download Speed: {BundleUtility.GetSpeedBytesToString((ulong)dlSpeed)}");
        this.info.text = strBuilder.ToString();

        this.progress.size = progress;
        this.percentage.text = (progress * 100).ToString("f0") + "%";

        // Patch Size: 00,  00(DC) / 00(PC)
        // Download Size: 00 , Download Speed: 00 / s
    }

    private void _UpdateCompressionInfo(float progress, long actualSize, long totalSize)
    {
        if (!this.progressGroup.activeSelf) this.progressGroup.SetActive(true);

        var strBuilder = new StringBuilder();
        strBuilder.Append($"Zip Size: {BundleUtility.GetBytesToString((ulong)totalSize)}");
        strBuilder.Append($"\nActual Size: {BundleUtility.GetBytesToString((ulong)actualSize)}");
        this.info.text = strBuilder.ToString();

        this.progress.size = progress;
        if (progress > 1f) progress = 1f;
        this.percentage.text = (progress * 100).ToString("f0") + "%";

        // Zip Size: 00
        // Actual Size: 00
    }

    private void _Complete()
    {
        Debug.Log("Complete Callback!!!");
    }

    public void StartExecute()
    {
        this._isStart = true;

        if (!this._isStopDownload) BundleDistributor.GetInstance().Check(this._Complete, this._UpdateDownloadInfo, this._UpdateCompressionInfo);
        else BundleDistributor.GetInstance().ContinueDownload();
    }

    public void RetryDownload()
    {
        BundleDistributor.GetInstance().RetryDownload();
        this.retryWindow.gameObject.SetActive(false);
    }

    public void StopDownload()
    {
        BundleDistributor.GetInstance().StopDownload();
        this._isStopDownload = true;
    }

    public void ShowRepairWindow()
    {
        if (!this.fixWindow.activeSelf) this.fixWindow.SetActive(true);
    }

    public void CloseRepairWindow()
    {
        if (this.fixWindow.activeSelf) this.fixWindow.SetActive(false);
    }

    public void RepairBundle()
    {
        this._isStart = true;

        // Repair 就是刪除下載目錄中的數據與檔案, 反為 true 表示確定清空
        bool isEmpty = BundleDistributor.GetInstance().Repair();
        // 確定下載目錄為空, 則再次進行檢測步驟
        if (isEmpty) BundleDistributor.GetInstance().Check(this._Complete, this._UpdateDownloadInfo, this._UpdateCompressionInfo);

        if (this.fixWindow.activeSelf) this.fixWindow.SetActive(false);
    }

    public void ShowConfirmWindow()
    {
        if (!this.confirmWindow.activeSelf) this.confirmWindow.SetActive(true);

        var patchSize = BundleDistributor.GetInstance().GetPatchSizeBytes();

        Text msg = this.confirmWindow.transform.Find("bg/Text").GetComponent<Text>();
        if (msg != null) msg.text = $"Patch size: {BundleUtility.GetBytesToString(patchSize)},\nDo you want to download?";
    }

    public void StartDownloadPatch()
    {
        BundleDistributor.GetInstance().DownloadPatch().Forget();

        if (this.confirmWindow.activeSelf) this.confirmWindow.SetActive(false);
    }

    public void GoToAppStore()
    {
        BundleDistributor.GetInstance().GoToAppStore().Forget();
    }

    [Header("BundleInfo")]
    public string bundleName = "";
    public string assetName = "";
    public GameObject container = null;
    private int _groupId = 9453;

    /*
     * 1. 使用 CacheBundle Load 或 CacheResource Load, 必須搭配使用 CacheBundle Unload 或 CacheResource Unload (成對式)
     * 2. [群組化] 使用 KeyBundle Load 或 KeyResource Load, 必須搭配使用 KeyBundle Unload 或 KeyResource Unload (成對式)
     * 
     * 以上則一使用, 如果沒有群組化的需求, 可以直接使用 Cache 系列, 反之如果有群組化需求, 則選用 KeyCache 系列
     * 例如: 針對戰鬥的所有資源, 使用 KeyCache 系列進行群組式加載, 在戰鬥結束後, 只需要指定 GroupId 就可以進行群組式卸載
     * 
     * ※備註: 切記一定要成對式 Load 跟 Unload
     */

    public async void PreloadBundle()
    {
        // 方法一. 直接 CacheBundle 進行預加載
        //await CacheBundle.GetInstance().Preload(this.bundleName);

        // 方法二. 透過 KeyBundle 連動 Key 進行預加載 => 群組式
        await KeyBundle.GetInstance().Preload(this._groupId, this.bundleName);
    }

    public async void LoadBundle()
    {
        // 方法一. 直接 CacheBundle 進行加載 (自行實例化)
        //GameObject go = await CacheBundle.GetInstance().Load<GameObject>(this.bundleName, this.assetName);
        //if (go != null) Instantiate(go, this.container.transform);

        // 方法二. 透過 KeyBundle 連動 Key 進行加載 (自行實例化)
        //GameObject go = await KeyBundle.GetInstance().Load<GameObject>(this._taskId, this.bundleName, this.assetName);
        //if (go != null) Instantiate(go, this.container.transform);

        // 方法三. 透過 KeyBundle 連動 Key 進行加載 (自動實例化) => 群組式
        await KeyBundle.GetInstance().LoadWithClone(this._groupId, this.bundleName, this.assetName, this.container.transform);
    }

    public void UnloadBundle()
    {
        // 依照 Destroy 次數進行 Unload
        foreach (Transform t in this.container.transform)
        {
            Destroy(t.gameObject);

            // 方法一. 直接 CacheBundle 進行單個卸載
            //CacheBundle.GetInstance().Unload(this.bundleName);

            // 方法二. 透過 KeyBundle 連動 Key 進行單個卸載 => 群組式
            KeyBundle.GetInstance().Unload(this._groupId, this.bundleName);
        }
    }

    public void ReleaseBundle()
    {
        foreach (Transform t in this.container.transform)
        {
            Destroy(t.gameObject);
        }

        // 方法一. 直接 CacheBundle 進行強制全部釋放
        //CacheBundle.GetInstance().Release();

        // 方法二. 透過 KeyBundle 連動 Key 進行強制全部釋放 => 群組式
        KeyBundle.GetInstance().Release(this._groupId);
    }
}
