using AssetLoader.Bundle;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class BundleDemo : MonoBehaviour
{
    public Text msg = null;
    public GameObject retryWindow = null;
    public Scrollbar progress = null;
    public Text percentage = null;
    public Text info = null;
    public GameObject bundleBtns = null;
    public GameObject fixWindow = null;

    private bool _isStart = false;
    private bool _isStopDownload = false;

    private void Awake()
    {
        //BundleConfig.InitCryptogram("Xor, 123");
        BundleConfig.InitCryptogram("offset, 64");
    }

    public void Start()
    {
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
            case BundleDistributor.ExecuteStatus.PROCESSING:
                this.msg.text = "Check Update\nProcessing...";
                break;
            // 主程式版本不一致
            case BundleDistributor.ExecuteStatus.APP_VERSION_INCONSISTENT:
                this.msg.text = "Application version inconsistent.\nPlease go to AppStore to Download!";
                break;
            // 無需更新資源
            case BundleDistributor.ExecuteStatus.NO_NEED_TO_UPDATE_PATCH:
                this.msg.text = "No need to update.";
                break;
            // 檢查更新包
            case BundleDistributor.ExecuteStatus.CHECKING_PATCH:
                this.msg.text = "Checking patch...";
                break;
            // 下載更新包
            case BundleDistributor.ExecuteStatus.DOWNLOAD_PATH:
                this.msg.text = "Downloading patch...";
                break;
            // 寫入配置文件
            case BundleDistributor.ExecuteStatus.WRITE_CONFIG:
                this.msg.text = "Processing config...";
                break;
            // 完成更新配置文件
            case BundleDistributor.ExecuteStatus.COMPLETE_UPDATE_CONFIG:
                this.msg.text = "Update config successfully.";
                break;

            // AssetDatabase Mode (無需執行更新)
            case BundleDistributor.ExecuteStatus.ASSET_DATABASE_MODE:
                this.msg.text = "AssetDatabase Mode";
                break;
        }

        if (BundleDistributor.GetInstance().GetDownloader().IsRetryDownload())
        {
            if (!this.retryWindow.gameObject.activeSelf) this.retryWindow.gameObject.SetActive(true);
        }

        this._UpdateProgress();

        switch (status)
        {
            case BundleDistributor.ExecuteStatus.NONE:
            case BundleDistributor.ExecuteStatus.DOWLOADING_CONFIG:
            case BundleDistributor.ExecuteStatus.PROCESSING:
            case BundleDistributor.ExecuteStatus.APP_VERSION_INCONSISTENT:
                this.info.text = string.Empty;
                break;

            case BundleDistributor.ExecuteStatus.NO_NEED_TO_UPDATE_PATCH:
            case BundleDistributor.ExecuteStatus.ASSET_DATABASE_MODE:
                this.info.text = string.Empty;
                this._isStart = false;
                break;
        }

        switch (status)
        {
            case BundleDistributor.ExecuteStatus.NO_NEED_TO_UPDATE_PATCH:
            case BundleDistributor.ExecuteStatus.ASSET_DATABASE_MODE:
                if (!this.bundleBtns.activeSelf) this.bundleBtns.SetActive(true);
                break;
            default:
                if (this.bundleBtns.activeSelf) this.bundleBtns.SetActive(false);
                break;
        }
    }

    private void _UpdateProgress()
    {
        this.progress.size = BundleDistributor.GetInstance().GetDownloader().dlProgress;
        this.percentage.text = (BundleDistributor.GetInstance().GetDownloader().dlProgress * 100).ToString("f0") + "%";
    }

    private void _UpdateDownloadInfo(float progress, int dlCount, string dlSize, string dlSpeed)
    {
        var strBuilder = new StringBuilder();
        strBuilder.Append($"Patch Size: {BundleDistributor.GetInstance().GetUpdateTotalSizeToString(BundleDistributor.GetInstance().GetUpdateCfg())}");
        strBuilder.Append($", {dlCount} (DC) / {BundleDistributor.GetInstance().GetUpdateCount(BundleDistributor.GetInstance().GetUpdateCfg())} (PC)");
        strBuilder.Append($"\nDownload Size: {dlSize}, Download Speed: {dlSpeed}");
        this.info.text = strBuilder.ToString();

        //Patch Size: 00,  00(DC) / 00(PC)
        //Download Size: 00MB , Download Speed: 00 / s
    }

    private void _Complete()
    {
        Debug.Log("Complete Callback!!!");
    }

    public void StartExecute()
    {
        this._isStart = true;

        if (!this._isStopDownload) BundleDistributor.GetInstance().Check(this._Complete, this._UpdateDownloadInfo);
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

    public void RepairBundle()
    {
        this._isStart = true;

        BundleDistributor.GetInstance().Repair(this._Complete, this._UpdateDownloadInfo);

        if (this.fixWindow.activeSelf) this.fixWindow.SetActive(false);
    }

    public void GoToAppStore()
    {
        BundleDistributor.GetInstance().GoToAppStore();
    }

    [Header("BundleInfo")]
    public string bundleName = "";
    public string assetName = "";
    public GameObject container = null;
    private int _taskId = 9453;

    public async void PreloadBundle()
    {
        await KeyBundle.GetInstance().PreloadInCache(this._taskId, this.bundleName);
    }

    public async void LoadBundle()
    {
        // 方法一. 直接CacheBundle進行加載 (【自行】實例化)
        GameObject go = await CacheBundle.GetInstance().Load<GameObject>(this.bundleName, this.assetName);
        if (go != null) Instantiate(go, this.container.transform);

        // 方法二. 透過KeyBundle連動Key進行加載 (【自行】實例化)
        //GameObject go = await KeyBundle.GetInstance().Load(this._taskId, this.bundleName, this.assetName);
        //if (go != null) Instantiate(go, this.container.transform);

        // 方法三. 透過KeyBundle連動Key進行加載 (【自動】實例化)
        //await KeyBundle.GetInstance().LoadWithClone(this._taskId, this.bundleName, this.assetName, this.container.transform, 1.1f);
    }

    public void UnloadBundle()
    {
        foreach (Transform t in this.container.transform)
        {
            Destroy(t.gameObject);

            // 方法一. 直接CacheBundle進行單個移除
            CacheBundle.GetInstance().ReleaseFromCache(this.bundleName);

            // 方法二. 透過KeyBundle連動Key進行單個移除
            //KeyBundle.GetInstance().ReleaseFromCache(this._taskId, this.bundleName);
        }
    }

    public void ReleaseBundle()
    {
        foreach (Transform t in this.container.transform)
        {
            Destroy(t.gameObject);
        }

        // 方法一. 直接CacheBundle進行全部釋放
        CacheBundle.GetInstance().ReleaseCache();

        // 方法二. 透過KeyBundle連動Key進行全部釋放
        //KeyBundle.GetInstance().ReleaseCache(this._taskId);
    }
}
