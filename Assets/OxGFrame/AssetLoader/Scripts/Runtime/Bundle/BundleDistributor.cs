using AssetLoader.Utility;
using AssetLoader.Zip;
using Cysharp.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace OxGFrame.AssetLoader.Bundle
{
    public class UnzipProcessor : Compressor.UnzipCallback
    {
        /// <summary>
        /// 解壓時的更新配置檔 (用於寫入用)
        /// </summary>
        private VersionFileCfg _updateCfg;

        public UnzipProcessor()
        {
            this._updateCfg = new VersionFileCfg();
        }

        public override void OnPostUnzip(ZipEntry zEntry)
        {
            // 取得持久化路徑 (壓縮包存於持久化路徑)
            string savePath = BundleConfig.GetLocalDlFileSaveDirectory();
            // 取得解壓檔案的位置 (解壓後)
            string filePath = Path.Combine(savePath, zEntry.Name);

            FileInfo file = new FileInfo(filePath);

            string fileName = Path.GetFileName(file.FullName);                                                                                    // 取得檔案名稱
            string dirName = Path.GetDirectoryName(file.FullName).Replace("\\", "/").Replace(savePath, string.Empty); // 取得目錄名稱
            long fileSize = file.Length;                                                                              // 取出檔案大小
            string fileMd5 = BundleUtility.MakeMd5ForFile(filePath);                                                  // 生成檔案Md5

            if (fileName.IndexOf($"{BundleConfig.bundleCfgName}{BundleConfig.cfgExtension}") != -1 ||
                fileName.IndexOf($"{BundleConfig.recordCfgName}{BundleConfig.cfgExtension}") != -1) return;

            ResFileInfo rf = new ResFileInfo();      // 建立資源檔資訊
            rf.fileName = fileName;                  // 檔案名稱
            rf.dirName = dirName.Replace(@"\", "/"); // 目錄名稱
            rf.size = fileSize;                      // 檔案大小
            rf.md5 = fileMd5;                        // 檔案md5

            string fullName = zEntry.Name;

            // 將解壓出來的檔案, 加入更新配置檔中
            this._updateCfg.AddResFileInfo(fullName, rf); // 加入配置檔的快取中
        }

        /// <summary>
        /// 取得更新配置檔
        /// </summary>
        /// <returns></returns>
        public VersionFileCfg GetUpdateCfg()
        {
            return this._updateCfg;
        }

        ~UnzipProcessor()
        {
            this._updateCfg = null;
        }
    }

    public class BundleDistributor
    {
        private static BundleDistributor _instance = null;
        public static BundleDistributor GetInstance()
        {
            if (_instance == null) _instance = new BundleDistributor();
            return _instance;
        }

        public enum ExecuteStatus
        {
            NONE,

            DOWLOADING_CONFIG,               // 正在從服務器下載配置文件
            SERVER_REQUEST_ERROR,            // 服務器請求錯誤 (連接錯誤)
            COMPARISON_PROCESSING,           // 正在比對處理中...

            APP_VERSION_INCONSISTENT,        // 主程式版本不一致
            ALREADY_UP_TO_DATE,              // 無需更新資源

            CHECKING_PATCH,                  // 檢查更新包
            WAITING_FOR_CONFIRM_TO_DOWNLOAD, // 等待確認下載
            DOWNLOAD_PATCH,                  // 下載更新包

            UNZIP_PATCH,                     // 解壓縮包

            WRITE_CONFIG,                    // 寫入配置文件
            COMPLETE_UPDATE_CONFIG,          // 完成更新配置文件

            RETRYING_DOWNLOAD = 99,          // 重新嘗試下載 (下載有誤)

            ASSET_DATABASE_MODE = 999        // AssetDatabase 加載模式 (Editor)
        }

        public ExecuteStatus executeStatus { get; protected set; }
        public CancellationTokenSource cts = new CancellationTokenSource();

        private VersionFileCfg _streamingCfg;  // 內置資源配置檔 (首次會從 StreamingAssets 中取得配置到至本地)
        private VersionFileCfg _localCfg;      // 本地配置檔
        private VersionFileCfg _recordCfg;     // 記錄配置檔 (記錄各版本檔案)
        private VersionFileCfg _updateCfg;     // 更新配置檔 (下載清單)
        private VersionFileCfg _serverCfg;     // 資源伺服器配置檔

        private bool _isCompressed = false;    // 標記是否有壓縮包
        private bool _isFirstInstall = false;  // 標記是否首次安裝, 僅提供首次下載使用壓縮包方式 (首次安裝則下載壓縮包, 反之非首次安裝的後續更新使用散檔)

        private Downloader _downloader = null; // 下載器
        private ulong _patchSizeBytes = 0;     // 更新包大小

        private Action _complete = null;
        private Downloader.Progression _downloaderProgression = null;
        private Compressor.Progression _compressorProgression = null;

        public BundleDistributor()
        {
            this.executeStatus = ExecuteStatus.NONE;
            this._downloader = new Downloader(this);
        }

        ~BundleDistributor()
        {
            this._streamingCfg = null;
            this._localCfg = null;
            this._recordCfg = null;
            this._updateCfg = null;
            this._serverCfg = null;
            this._downloader = null;
            this.cts.Dispose();
        }

        /// <summary>
        /// 重置
        /// </summary>
        public void Reset()
        {
            this._streamingCfg = null;
            this._localCfg = null;
            this._recordCfg = null;
            this._updateCfg = null;
            this._serverCfg = null;

            this._isCompressed = false;

            this.executeStatus = ExecuteStatus.DOWLOADING_CONFIG;
            this._patchSizeBytes = 0;

            this._complete = null;
            this._downloaderProgression = null;
            this._compressorProgression = null;
        }

        /// <summary>
        /// 取消任務
        /// </summary>
        public void TaskCancel(bool quit = false)
        {
            // 取消壓縮程序
            Compressor.CancelAsync();

            // 取消更新程序
            this.cts.Cancel();
            this.cts.Dispose();
            if (!quit) this.cts = new CancellationTokenSource();

            // 取消下載器程序
            this._downloader.Reset();
        }

        /// <summary>
        /// 設置執行狀態
        /// </summary>
        /// <param name="executeStatus"></param>
        public void SetExecuteStatus(ExecuteStatus executeStatus)
        {
            this.executeStatus = executeStatus;
        }

        /// <summary>
        /// 取得更新包大小 (Bytes)
        /// </summary>
        /// <returns></returns>
        public ulong GetPatchSizeBytes()
        {
            return this._patchSizeBytes;
        }

        /// <summary>
        /// 取得下載器
        /// </summary>
        /// <returns></returns>
        public Downloader GetDownloader()
        {
            return this._downloader;
        }

        /// <summary>
        /// 取得本地配置檔
        /// </summary>
        /// <returns></returns>
        public VersionFileCfg GetLocalCfg()
        {
            return this._localCfg;
        }

        /// <summary>
        /// 取得 StreamingAssets 中的配置檔
        /// </summary>
        /// <returns></returns>
        public VersionFileCfg GetStreamingCfg()
        {
            return this._streamingCfg;
        }

        /// <summary>
        /// 取得本地記錄配置檔
        /// </summary>
        /// <returns></returns>
        public VersionFileCfg GetRecordCfg()
        {
            return this._recordCfg;
        }

        /// <summary>
        /// 取得更新配置檔
        /// </summary>
        /// <returns></returns>
        public VersionFileCfg GetUpdateCfg()
        {
            return this._updateCfg;
        }

        /// <summary>
        /// 取得遠端配置檔
        /// </summary>
        /// <returns></returns>
        public VersionFileCfg GetServerCfg()
        {
            return this._serverCfg;
        }

        /// <summary>
        /// 前往主程式商店
        /// </summary>
        public async UniTaskVoid GoToAppStore()
        {
            Application.OpenURL(await BundleConfig.GetAppStoreLink());
        }

        /// <summary>
        /// 執行檢查, 並且下載
        /// </summary>
        /// <param name="complete"></param>
        public void Check(Action complete = null, Downloader.Progression downloaderProgression = null, Compressor.Progression compressorProgression = null)
        {
            // 取消任務
            this.TaskCancel();

            // 重置
            this.Reset();

            // 執行檢查
            this._Execute(complete, downloaderProgression, compressorProgression).Forget();
        }

        /// <summary>
        /// 刪除所有緩存數據跟配置檔 (即清空下載目錄)
        /// </summary>
        public bool Repair()
        {
            // 取消任務
            this.TaskCancel();

            // 重置 (需要將所有配置檔都重置)
            this.Reset();

            // 用戶下載確認標記 = 0
            this._SetUserConfirm(0);

            // 取得本地持久化路徑
            var dir = BundleConfig.GetLocalDlFileSaveDirectory();

            // 刪除資源數據
            BundleUtility.DeleteFolder(dir);

            // 建立目錄
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            // 判斷檢查目錄是否為空 (表示數據已完成清除)
            if (BundleUtility.GetFilesRecursively(dir).Length <= 0) return true;

            return false;
        }

        /// <summary>
        /// 暫停下載
        /// </summary>
        public void StopDownload()
        {
            this.TaskCancel();
        }

        /// <summary>
        /// 繼續下載
        /// </summary>
        public void ContinueDownload()
        {
            this.SetExecuteStatus(BundleDistributor.ExecuteStatus.COMPARISON_PROCESSING);
            this._Execute().Forget();
        }

        /// <summary>
        /// 重新嘗試下載
        /// </summary>
        public void RetryDownload()
        {
            this.TaskCancel();
            this.SetExecuteStatus(BundleDistributor.ExecuteStatus.COMPARISON_PROCESSING);
            this._downloader.Reset();
            this._Execute().Forget();
        }

        /// <summary>
        /// 重新嘗試下載配置檔
        /// </summary>
        public void RetryDownloadConfig()
        {
            this.TaskCancel();
            this.SetExecuteStatus(BundleDistributor.ExecuteStatus.DOWLOADING_CONFIG);
            this._Execute().Forget();
        }

        /// <summary>
        /// 調用執行 (Main)
        /// </summary>
        /// <param name="complete"></param>
        /// <returns></returns>
        private async UniTask _Execute(Action complete = null, Downloader.Progression downloaderProgression = null, Compressor.Progression compressorProgression = null)
        {
            this._complete = (complete == null) ? this._complete : complete;
            this._downloaderProgression = (downloaderProgression == null) ? this._downloaderProgression : downloaderProgression;
            this._compressorProgression = (compressorProgression == null) ? this._compressorProgression : compressorProgression;

#if UNITY_EDITOR
            // 如果使用 AssetDatabase 加載模式, 將執行狀態切換至 ASSET_DATABASE_MODE (表示無需執行任何事項)
            if (BundleConfig.assetDatabaseMode)
            {
                this._downloader.SetDownloadProgress(1);
                this._complete?.Invoke();
                this.executeStatus = ExecuteStatus.ASSET_DATABASE_MODE;
            }
#endif

            switch (this.executeStatus)
            {
                // 步驟1. 下載配置檔案
                case ExecuteStatus.DOWLOADING_CONFIG:
                    Debug.Log("<color=#ff8c00>Request config from server</color>");
                    await this._DownloadServerBundleConfig();
                    break;
                // 步驟2. 處理資源更新, 內部將會進行 ExecuteStatus 的切換
                case ExecuteStatus.COMPARISON_PROCESSING:
                    Debug.Log("<color=#ff8c00>Comparison processing...</color>");
                    await this._ComparisonProcess();
                    break;
            }
        }

        /// <summary>
        /// 下載服務端的配置檔案 (STEP 1.)
        /// </summary>
        /// <returns></returns>
        private async UniTask _DownloadServerBundleConfig()
        {
            // 取得內置在 StreamingAssets 中的 Cfg, 需要從中取得 ProductName
            string streamingAssetsCfgPath = BundleConfig.GetStreamingAssetsBundleConfigPath();
            string streamingCfgJson = await BundleUtility.FileRequestString(streamingAssetsCfgPath, this.cts);
            var streamingCfg = JsonConvert.DeserializeObject<VersionFileCfg>(streamingCfgJson);
            this._streamingCfg = streamingCfg;

            // 取得 Server 端配置檔的 URL
            this._serverCfg = new VersionFileCfg();
            string url = await BundleConfig.GetServerBundleUrl() + $@"/{streamingCfg.PRODUCT_NAME}/{BundleConfig.bundleCfgName}{BundleConfig.cfgExtension}";

            // 請求 Server 的配置檔
            try
            {
                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {
                    await request.SendWebRequest().WithCancellation(this.cts.Token);

                    if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
                    {
                        // Do retry connect to server

                        Debug.Log($"<color=#FF0000>Server request failed. URL: {url}</color>");

                        this.executeStatus = ExecuteStatus.SERVER_REQUEST_ERROR;
                    }
                    else
                    {
                        Debug.Log($"SERVER REQ: {url}");

                        string json = request.downloadHandler.text;
                        this._serverCfg = JsonConvert.DeserializeObject<VersionFileCfg>(json);

                        // 資源是否為壓縮包
                        this._isCompressed = Convert.ToBoolean(this._serverCfg.COMPRESSED);

                        this.executeStatus = ExecuteStatus.COMPARISON_PROCESSING;

                        this._Execute().Forget();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"<color=#FF0000>Server request failed. URL: {url}</color>\nError: {ex}");

                this.executeStatus = ExecuteStatus.SERVER_REQUEST_ERROR;
            }
        }

        /// <summary>
        /// 進行比對配置檔與更新 (STEP 2.)
        /// </summary>
        /// <returns></returns>
        private async UniTask _ComparisonProcess()
        {
            // 取得解析過的壓縮包名
            string zipName = this.GetParsedZipName();

            // 【切換狀態 - 開始檢查更新】
            this.executeStatus = ExecuteStatus.CHECKING_PATCH;

            // 確保本地端的儲存目錄是否存在, 無存在則建立
            if (!Directory.Exists(BundleConfig.GetLocalDlFileSaveDirectory()))
            {
                Directory.CreateDirectory(BundleConfig.GetLocalDlFileSaveDirectory());
            }

            // 把資源配置文件拷貝到持久化目錄 Application.persistentDataPath
            // ※備註: 因為更新文件後是需要改寫版本號, 而在手機平台上的 StreamingAssets 是不可寫入的
            if (!File.Exists(BundleConfig.GetLocalDlFileSaveBundleConfigPath()))
            {
                // 如果持久化目錄無配置檔, 表示首次安裝並且進行標記 (主要是因為會選擇下載方式)
                // ※備註: 因為首次安裝選擇壓縮包下載 (能夠加快下載速率, 因為非散檔), 反之非首次安裝則需要部分更新, 就切換成散檔模式下載
                this._isFirstInstall = true;
                Debug.Log($"<color=#00ff92>First Install ({this._isFirstInstall})</color>");

                string streamingAssetsCfgPath = BundleConfig.GetStreamingAssetsBundleConfigPath();
                string localCfgPath = BundleConfig.GetLocalDlFileSaveBundleConfigPath();

                // 從 StreamingAssets 中取得配置檔 (InApp)
                string streamingCfgJson = await BundleUtility.FileRequestString(streamingAssetsCfgPath, this.cts);
                if (!string.IsNullOrEmpty(streamingCfgJson))
                {
                    await BundleUtility.RequestAndCopyFileFromStreamingAssets(streamingAssetsCfgPath, localCfgPath, this.cts);
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
                string streamingAssetsCfgPath = BundleConfig.GetStreamingAssetsBundleConfigPath();
                string streamingCfgJson = await BundleUtility.FileRequestString(streamingAssetsCfgPath, this.cts);
                var streamingAssetsCfg = JsonConvert.DeserializeObject<VersionFileCfg>(streamingCfgJson);

                // 從本地端讀取配置檔 (持久化路徑使用 File.Read)
                string localCfgPath = BundleConfig.GetLocalDlFileSaveBundleConfigPath();
                string localCfgJson = File.ReadAllText(localCfgPath);
                var localCfg = JsonConvert.DeserializeObject<VersionFileCfg>(localCfgJson);

                // 如果主程式版本不一致表示有更新 App, 則將本地配置檔的主程式版本寫入成 StreamingAssets 配置檔中的 APP_VERSION
                if (streamingAssetsCfg.APP_VERSION != localCfg.APP_VERSION)
                {
                    localCfg.APP_VERSION = streamingAssetsCfg.APP_VERSION;
                    localCfgJson = JsonConvert.SerializeObject(localCfg);
                    File.WriteAllText(localCfgPath, localCfgJson); // 進行寫入存儲
                }

                // 檢查壓縮模式下, 是否還有壓縮檔案存在, 如果存在表示繼上次的壓縮包尚未下載完成與解壓縮, 則必須標記 FistInstall = true
                if (this._isCompressed)
                {
                    string zipFilePath = Path.Combine(BundleConfig.GetLocalDlFileSaveDirectory(), zipName);
                    if (File.Exists(zipFilePath))
                    {
                        this._isFirstInstall = true;
                        Debug.Log($"<color=#00ff92>【Compression Mode】Check has zip file in local, Continue first install (Path: {zipFilePath})</color>");
                    }
                }
            }

            {
                var localCfg = new VersionFileCfg();

                try
                {
                    // 從本地端讀取配置檔
                    string localCfgPath = BundleConfig.GetLocalDlFileSaveBundleConfigPath();
                    string localCfgJson = File.ReadAllText(localCfgPath);
                    localCfg = JsonConvert.DeserializeObject<VersionFileCfg>(localCfgJson);
                }
                catch
                {
                    Debug.Log("<color=#FF0000>Read Local Save BundleConfig.json failed.</color>");
                }

                // 比對主程式版本
                if (localCfg.APP_VERSION != this._serverCfg.APP_VERSION)
                {
                    // Do GoToAppStore

                    // 【切換狀態 - 主程式版本不一致】
                    this.executeStatus = ExecuteStatus.APP_VERSION_INCONSISTENT;

                    // 取消 Task
                    this.TaskCancel();

                    Debug.Log("<color=#ff8c00>Application version inconsistent, require to update application (go to store)</color>");
                    Debug.Log($"LOCAL APP_VER: {localCfg.APP_VERSION}, SERVER APP_VER: {this._serverCfg.APP_VERSION}");
                    return;
                }
                // 比對資源版本
                else if (localCfg.RES_VERSION == this._serverCfg.RES_VERSION)
                {
                    this._ReadLocalCfg();                    // 讀取本地端配置檔
                    this._ReadLocalRecordCfg();              // 讀取本地端記錄配置檔
                    this._downloader.SetDownloadProgress(1); // 設置 Downloader 的 Progress = 1 (表示無需更新 = 完成)
                    this._complete?.Invoke();                // 完成處理的 Callback
                    this._SetUserConfirm(0);                     // 重置確認下載參數
                    this._isFirstInstall = false;            // 設置首次安裝 = false (表示已經更新過)

                    // 更新完成後, 卸載 Manifest (後續才能自動加載新的 Manifest)
                    Cacher.CacheBundle.GetInstance().UnloadManifest();

                    // 【切換狀態 - 無需更新】
                    this.executeStatus = ExecuteStatus.ALREADY_UP_TO_DATE;

                    // 取消 Task
                    this.TaskCancel();

                    Debug.Log($"<color=#ff8c00>No update required (already up-to-date)</color>");
                    Debug.Log($"LOCAL RES_VER: {localCfg.RES_VERSION}, SERVER RES_VER: {this._serverCfg.RES_VERSION}");
                    return;
                }

                // 如果資源版本與服務端的資源版本不一致, 以下開始執行系列程序
                Debug.Log($"LOCAL RES_VER: {localCfg.RES_VERSION}, SERVER RES_VER: {this._serverCfg.RES_VERSION}");

                // Local 配置檔與 Server 配置檔進行資源文件比對, 並且建立更新用的配置檔 (用於文件更新的依據配置檔)
                this._updateCfg = new VersionFileCfg();
                foreach (var svrFile in this._serverCfg.RES_FILES)
                {
                    var sFileName = svrFile.Key; // Server FileName (服務端資源名稱)
                    var sFile = svrFile.Value;   // Server File     (服務端資源資訊)

                    // 壓縮包 + 首次安裝, 僅比對壓縮包名
                    if (this._isCompressed && this._isFirstInstall)
                    {
                        if (sFileName.IndexOf(zipName) != -1)
                        {
                            // 新增至更新配置檔中 (下載清單)
                            this._updateCfg.AddResFileInfo(sFileName, sFile);
                            Debug.Log($"<color=#00ff92>【Compression Mode】<color=#edff8d>[First Install]</color> Download Bundle Zip: {sFileName}</color>");
                            break;
                        }
                    }
                    else
                    {
                        // 散檔案更新時, 如果使用壓縮包方式, 則需要濾掉壓縮包, 因為 Server 配置檔還是會記錄壓縮包的檔案資訊, 所以需要略過處理, 否則會進行下載 (僅提供給首次安裝 + 使用壓縮包方式)
                        if (this._isCompressed && sFileName.IndexOf(zipName) != -1)
                        {
                            Debug.Log($"<color=#00ff92>【Compression Mode】<color=#edff8d>[Not First Install]</color> Skipped Bundle Zip: {sFileName}</color>");
                            continue;
                        }

                        // 檢查 [本地端的配置文件] 是否有 [服務端的配置文件] 中的資源名稱 (如果沒有表示【新資源】)
                        if (!localCfg.HasFile(sFileName))
                        {
                            // 如果沒有, 則進行新增資源檔至要更新的配置檔中 (下載清單)
                            this._updateCfg.AddResFileInfo(sFileName, sFile);
                        }
                        else
                        {
                            string localMd5 = localCfg.GetResFileInfo(sFileName).md5; // 本地端的檔案 Md5
                            string svrMd5 = svrFile.Value.md5;                        // 服務端的檔案 Md5

                            // 檢查 [本地端的資源檔 Md5] 是否與 [服務端的資源檔 Md5] 有不一致
                            if (localMd5 != svrMd5)
                            {
                                // 如果不一致 (表示資源有變更), 則進行新增資源檔至要更新的配置檔中 (下載清單)
                                this._updateCfg.AddResFileInfo(sFileName, sFile);
                            }
                        }
                    }
                }

                // 取得更新包大小
                this._patchSizeBytes = BundleUtility.GetUpdateSizeBytes(this._updateCfg);
                Debug.Log($"<color=#4fd2ff>Patch Size: {BundleUtility.GetUpdateTotalSizeToString(this._updateCfg)}, Patch Count: {BundleUtility.GetUpdateCount(this._updateCfg)}</color>");
                await UniTask.Delay(TimeSpan.FromSeconds(1f), ignoreTimeScale: false, PlayerLoopTiming.FixedUpdate, this.cts.Token);

                // 如果有更新資源, 則進行正常流程
                if (this._updateCfg.RES_FILES.Count > 0)
                {
                    // 檢查是否確認過, 如果應用程式中斷, 會直接跳過確認步驟直接下載 (會在無需更新步驟 reset 參數)
                    bool confirmed = this._GetUserConfirm();
                    if (confirmed)
                    {
                        // 已確認過, 則直接下載
                        this.DownloadPatch().Forget();
                    }
                    else
                    {
                        // 等待確認下載
                        this.executeStatus = ExecuteStatus.WAITING_FOR_CONFIRM_TO_DOWNLOAD;
                    }
                }
                // 反之, 如果沒有更新資源, 直接檢查並且寫入配置檔
                else
                {
                    this.DownloadPatch().Forget();
                }
            }
        }

        /// <summary>
        /// 開始下載更新包程序
        /// </summary>
        /// <returns></returns>
        public async UniTaskVoid DownloadPatch()
        {
            try
            {
                // 1. 下載更新包
                await this._DownloadPatch();
                // 清空下載器緩存
                this._downloader.Reset();
                // 下載後, 執行 GC
                System.GC.Collect();

                // 2. 判斷有無壓縮 + 首次安裝 (僅首次安裝並且使用壓縮包方式, 才需要進行解壓)
                if (this._isCompressed && this._isFirstInstall)
                {
                    // 執行解壓縮
                    await this._UnzipPatch();
                    // 解壓後, 執行 GC
                    System.GC.Collect();
                }
            }
            catch
            {
                // task cancel return

                return;
            }

            // 3. 最後寫入配置
            this._WriteConfig();
        }

        /// <summary>
        /// 下載更新包程序
        /// </summary>
        /// <returns></returns>
        private async UniTask _DownloadPatch()
        {
            // 標記已確認過
            this._SetUserConfirm(1);

            // 【切換狀態 - 下載更新包】, 下載更新包
            this.executeStatus = ExecuteStatus.DOWNLOAD_PATCH;

            // 開始下載
            await this._downloader.DownloadFiles(this._downloaderProgression);

            // 完成後, 緩衝一下
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), ignoreTimeScale: false, PlayerLoopTiming.FixedUpdate, this.cts.Token);
        }

        /// <summary>
        /// 解壓縮更新包程序
        /// </summary>
        /// <returns></returns>
        private async UniTask _UnzipPatch()
        {
            // 【切換狀態 - 執行解壓程序】
            this.executeStatus = ExecuteStatus.UNZIP_PATCH;

            // 取得解析過的壓縮包名
            string zipName = this.GetParsedZipName();

            // 解壓密碼
            string password = BundleConfig.unzipPassword;

            // 解壓緩存
            int bufferSize = BundleConfig.unzipBufferSize;

            // 解壓回調 (會處理解壓配置檔)
            UnzipProcessor unzipProcessor = new UnzipProcessor();

            // 壓縮包路徑
            var inputPath = Path.Combine(BundleConfig.GetLocalDlFileSaveDirectory(), zipName);

            // 開始解壓 (解壓後會執行壓縮包刪除)
            await Compressor.UnzipAsync(inputPath, BundleConfig.GetLocalDlFileSaveDirectory(), password, unzipProcessor, bufferSize, this._compressorProgression, true);

            // 完成後, 緩衝一下
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), ignoreTimeScale: false, PlayerLoopTiming.FixedUpdate, this.cts.Token);

            // 替換欲寫入的更新配置檔 = 解壓更新配置檔
            this._updateCfg = unzipProcessor.GetUpdateCfg();
        }

        /// <summary>
        /// 取得解析過的 ZipName (會從設定檔進行判斷解析)
        /// </summary>
        /// <returns></returns>
        public string GetParsedZipName()
        {
            // 壓縮包名稱 (需處理副檔名)
            string[] zipNameArgs = BundleConfig.zipFileName.Split('.');
            string zipName = zipNameArgs[0];
            string zipExtension = zipNameArgs.Length > 1 ? zipNameArgs[1] : string.Empty;
            // 判斷是否有 Md5 for ZipName
            if (BundleConfig.md5ForZipFileName) zipName = BundleUtility.MakeMd5ForString(zipName);
            // 合併壓縮包名 + 副檔名
            zipName = string.IsNullOrEmpty(zipExtension) ? $@"{zipName}" : $@"{zipName}.{zipExtension}";

            return zipName;
        }

        /// <summary>
        /// 寫入配置檔程序
        /// </summary>
        private void _WriteConfig()
        {
            // 【切換狀態 - 開始寫入配置檔】, 等待所有文件寫入完畢
            this.executeStatus = ExecuteStatus.WRITE_CONFIG;

            // 開始寫入更新配置檔至本地
            this._AfterProcessToWriteCfg(this._updateCfg);
        }

        private const string KEY_WAITING_FOR_CONFIRM_TO_DOWNLOAD = "KEY_WAITING_FOR_CONFIRM_TO_DOWNLOAD";
        /// <summary>
        /// 標記是否確認同意下載更新包 (0 = false, 1 = true)
        /// </summary>
        /// <param name="value"></param>
        private void _SetUserConfirm(int value)
        {
            string key = BundleUtility.MakeMd5ForString(KEY_WAITING_FOR_CONFIRM_TO_DOWNLOAD);
            PlayerPrefs.SetInt(key, value);
        }

        /// <summary>
        /// 取得確認標記
        /// </summary>
        /// <returns></returns>
        private bool _GetUserConfirm()
        {
            string key = BundleUtility.MakeMd5ForString(KEY_WAITING_FOR_CONFIRM_TO_DOWNLOAD);
            return Convert.ToBoolean(PlayerPrefs.GetInt(key, 0));
        }

        /// <summary>
        /// 讀取本地端的記錄配置檔 (Persistent Data Path)
        /// </summary>
        private void _ReadLocalRecordCfg()
        {
            string localFilePath = Path.Combine(BundleConfig.GetLocalDlFileSaveDirectory(), $"{BundleConfig.recordCfgName}{BundleConfig.cfgExtension}");
            if (File.Exists(localFilePath))
            {
                string json = File.ReadAllText(localFilePath);
                this._recordCfg = JsonConvert.DeserializeObject<VersionFileCfg>(json);
            }
        }

        /// <summary>
        /// 讀取本地端的配置檔 (Persistent Data Path)
        /// </summary>
        private void _ReadLocalCfg()
        {
            string localFilePath = Path.Combine(BundleConfig.GetLocalDlFileSaveDirectory(), $"{BundleConfig.bundleCfgName}{BundleConfig.cfgExtension}");
            if (File.Exists(localFilePath))
            {
                string json = File.ReadAllText(localFilePath);
                this._localCfg = JsonConvert.DeserializeObject<VersionFileCfg>(json);
            }
        }

        /// <summary>
        /// 執行完成後的配置檔寫入程序
        /// </summary>
        /// <param name="updateCfg"></param>
        private void _AfterProcessToWriteCfg(VersionFileCfg updateCfg)
        {
            // 將 [服務端的配置文件] 寫入 [本地端的配置文件]
            string localCfgPath = BundleConfig.GetLocalDlFileSaveBundleConfigPath();
            var svrCfgJson = JsonConvert.SerializeObject(this._serverCfg);
            File.WriteAllText(localCfgPath, svrCfgJson); // 進行寫入存儲

            // 取得 [記錄配置檔] 的路徑
            string recordCfgPath = Path.Combine(BundleConfig.GetLocalDlFileSaveDirectory(), $"{BundleConfig.recordCfgName}{BundleConfig.cfgExtension}");

            // 檢查 [記錄配置檔] 是否存在
            if (File.Exists(recordCfgPath))
            {
                // 讀取本地 [記錄配置檔]
                string recordFile = File.ReadAllText(recordCfgPath);
                // 進行 [記錄配置檔] 反序列化
                VersionFileCfg recordCfg = JsonConvert.DeserializeObject<VersionFileCfg>(recordFile);

                // 針對歷代版本的記錄配置檔, 進行更新合併
                foreach (var file in recordCfg.RES_FILES)
                {
                    string fileName = file.Key;

                    // 取得檔案路徑
                    var filePath = BundleConfig.GetLocalDlFileSaveDirectory() + $@"/{fileName}";
                    // 判斷檔案如果不存在於 PersistentData Path, 則不進行記錄 (因為表示本地無資源可加載)
                    if (!File.Exists(filePath)) continue;

                    // 將上一次的 [記錄配置檔] 進行合併至 [更新配置檔] (有則修改, 無則添加)
                    if (updateCfg.HasFile(fileName))
                    {
                        updateCfg.RES_FILES[fileName] = file.Value;
                    }
                    else
                    {
                        updateCfg.AddResFileInfo(fileName, file.Value);
                    }
                }
            }

            try
            {
                // 將 [更新配置檔], 更新至 [記錄配置檔]
                var updateCfgJson = JsonConvert.SerializeObject(updateCfg);
                File.WriteAllText(recordCfgPath, updateCfgJson); // 進行寫入存儲
                this._ReadLocalCfg();                            // 讀取本地端配置檔
                this._ReadLocalRecordCfg();                      // 讀取本地[記錄配置檔] (因為已經進行重新寫入了)

                // 【切換狀態 - 完成配置寫入與更新】
                this.executeStatus = ExecuteStatus.COMPLETE_UPDATE_CONFIG;

                // 完成後取消Task
                this.TaskCancel();

                // 完成寫入後, 再執行一次檢查是否無需更新了
                this.Check(this._complete, this._downloaderProgression);

                Debug.Log("<color=#ff8c00>Update and write config completes.</color>");
            }
            catch
            {
                Debug.Log("<color=#FF0000>Update RecordConfig.json failed.</color>");
            }
        }
    }
}