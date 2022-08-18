using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace OxGFrame.AssetLoader.Bundle
{
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

            DOWLOADING_CONFIG,          // 正在從服務器下載配置文件
            SERVER_REQUEST_ERROR,       // 服務器請求錯誤 (連接錯誤)
            PROCESSING,                 // 正在處理中...

            APP_VERSION_INCONSISTENT,   // 主程式版本不一致
            NO_NEED_TO_UPDATE_PATCH,    // 無需更新資源

            CHECKING_PATCH,             // 檢查更新包
            DOWNLOAD_PATH,              // 下載更新包

            WRITE_CONFIG,               // 寫入配置文件
            COMPLETE_UPDATE_CONFIG,     // 完成更新配置文件

            ASSET_DATABASE_MODE         // AssetDatabase 加載模式 (Editor)
        }

        public ExecuteStatus executeStatus { get; protected set; }
        public CancellationTokenSource cts = new CancellationTokenSource();

        private VersionFileCfg _streamingCfg;
        private VersionFileCfg _localCfg;
        private VersionFileCfg _recordCfg;
        private VersionFileCfg _updateCfg;
        private VersionFileCfg _serverCfg;

        private Downloader _downloader = null;
        private ulong _patchSizeBytes = 0;

        private Action _complete = null;
        private Downloader.Progression _progression = null;

        public BundleDistributor()
        {
            this.executeStatus = ExecuteStatus.NONE;
            this._downloader = new Downloader(this);
        }

        /// <summary>
        /// 重置
        /// </summary>
        public void Reset()
        {
            this.executeStatus = ExecuteStatus.DOWLOADING_CONFIG;
            this._patchSizeBytes = 0;

            this._complete = null;
            this._progression = null;
        }

        /// <summary>
        /// 取消UniTask執行
        /// </summary>
        public void TaskCancel()
        {
            this.cts.Cancel();
            this.cts.Dispose();
            this.cts = new CancellationTokenSource();
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
        /// 取得StreamingAssets中的配置檔
        /// </summary>
        /// <returns></returns>
        public VersionFileCfg GetStreamingCfg()
        {
            return this._streamingCfg;
        }

        /// <summary>
        /// 取得本地紀錄配置檔
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
        public void Check(Action complete = null, Downloader.Progression progression = null)
        {
            this.TaskCancel();
            this.Reset();
            this._Execute(complete, progression).Forget();
        }

        /// <summary>
        /// 刪除所有緩存數據跟配置檔 (即清空下載目錄, 以致重新修復並且下載)
        /// </summary>
        public void Repair(Action complete = null, Downloader.Progression progression = null)
        {
            var dir = BundleConfig.GetLocalDlFileSaveDirectory();

            _DeleteFolder(dir);

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            this.Check(complete, progression);
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
            this.SetExecuteStatus(BundleDistributor.ExecuteStatus.PROCESSING);
            this._Execute().Forget();
        }

        /// <summary>
        /// 重新嘗試下載
        /// </summary>
        public void RetryDownload()
        {
            this.TaskCancel();
            this.SetExecuteStatus(BundleDistributor.ExecuteStatus.PROCESSING);
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
        private async UniTask _Execute(Action complete = null, Downloader.Progression progression = null)
        {
            this._complete = (complete == null) ? this._complete : complete;
            this._progression = (progression == null) ? this._progression : progression;

#if UNITY_EDITOR
            // 如果使用 AssetDatabase 加載模式, 將執行狀態切換至 ASSET_DATABASE_MODE (表示無需執行任何事項)
            if (BundleConfig.bAssetDatabaseMode)
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
                    Debug.Log("正在從Server下載配置文件");
                    await this._DownloadServerBundleConfig();
                    break;
                // 步驟2. 處理資源更新, 內部將會進行 ExecuteStatus 的切換
                case ExecuteStatus.PROCESSING:
                    Debug.Log("正在處理...");
                    await this._ProcessUpdate();
                    break;
            }
        }

        /// <summary>
        /// 下載服務端的配置檔案 (STEP 1.)
        /// </summary>
        /// <returns></returns>
        private async UniTask _DownloadServerBundleConfig()
        {
            // 取得內置在StreamingAssets中的Cfg, 需要從中取得ProductName
            string streamingAssetsCfgPath = BundleConfig.GetStreamingAssetsBundleConfigPath();
            string streamingCfgJson = await this._GetFileFromStreamingAssets(streamingAssetsCfgPath);
            var streamingCfg = JsonConvert.DeserializeObject<VersionFileCfg>(streamingCfgJson);
            this._streamingCfg = streamingCfg;

            // 取得Server端配置檔的URL
            this._serverCfg = new VersionFileCfg();
            string url = await BundleConfig.GetServerBundleUrl() + $@"/{streamingCfg.PRODUCT_NAME}/{BundleConfig.bundleCfgName}{BundleConfig.cfgExt}";

            // 請求Server的配置檔
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

                        this.executeStatus = ExecuteStatus.PROCESSING;

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
        private async UniTask _ProcessUpdate()
        {
            // 【切換狀態 - 開始檢查更新】
            this.executeStatus = ExecuteStatus.CHECKING_PATCH;

            // 確保本地端的儲存目錄是否存在, 無存在則建立
            if (!Directory.Exists(BundleConfig.GetLocalDlFileSaveDirectory()))
            {
                Directory.CreateDirectory(BundleConfig.GetLocalDlFileSaveDirectory());
            }

            // 把資源配置文件拷貝到持久化目錄Application.persistentDataPath
            // ※備註: 因為更新文件後是需要改寫版本號, 而在手機平台上的StreamingAssets是不可寫入的
            if (!File.Exists(BundleConfig.GetLocalDlFileSaveBundleConfigPath()))
            {
                string streamingAssetsCfgPath = BundleConfig.GetStreamingAssetsBundleConfigPath();
                string localCfgPath = BundleConfig.GetLocalDlFileSaveBundleConfigPath();

#if UNITY_STANDALONE_WIN
                if (File.Exists(streamingAssetsCfgPath))
                {
                    File.Copy(streamingAssetsCfgPath, localCfgPath);
                }
#endif

#if UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL
                string streamingCfgJson = await this._GetFileFromStreamingAssets(streamingAssetsCfgPath);
                if (!string.IsNullOrEmpty(streamingCfgJson))
                {
                    await this._CopyFileFromStreamingAssets(streamingAssetsCfgPath, localCfgPath);
                }
#endif
            }
            // 如果本地已經有配置檔, 則需要去比對主程式版本, 並且從新的App中的配置檔寫入至本地配置檔中
            else
            {
                // 從StreamingAssets讀取配置檔 (StreamingAssets 使用 Request)
                string streamingAssetsCfgPath = BundleConfig.GetStreamingAssetsBundleConfigPath();
                string streamingCfgJson = await this._GetFileFromStreamingAssets(streamingAssetsCfgPath);
                var streamingAssetsCfg = JsonConvert.DeserializeObject<VersionFileCfg>(streamingCfgJson);

                // 從本地端讀取配置檔 (持久化路徑使用 File.Read)
                string localCfgPath = BundleConfig.GetLocalDlFileSaveBundleConfigPath();
                string localCfgJson = File.ReadAllText(localCfgPath);
                var localCfg = JsonConvert.DeserializeObject<VersionFileCfg>(localCfgJson);

                // 如果主程式版本不一致表示有更新App, 則將本地配置檔的主程式版本寫入成StreamingAssets配置檔中的APP_VERSION
                if (streamingAssetsCfg.APP_VERSION != localCfg.APP_VERSION)
                {
                    localCfg.APP_VERSION = streamingAssetsCfg.APP_VERSION;
                    localCfgJson = JsonConvert.SerializeObject(localCfg);
                    File.WriteAllText(localCfgPath, localCfgJson); // 進行寫入存儲
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

                    // 取消Task
                    this.TaskCancel();

                    Debug.Log("主程式版本不符, 需更新主程式 (商店下載更新)");
                    Debug.Log($"LOCAL APP_VER: {localCfg.APP_VERSION}, SERVER APP_VER: {this._serverCfg.APP_VERSION}");
                    return;
                }
                // 比對資源版本
                else if (localCfg.RES_VERSION == this._serverCfg.RES_VERSION)
                {
                    this._ReadLocalCfg();                    // 讀取本地端配置檔
                    this._ReadLocalRecordCfg();              // 讀取本地端紀錄配置檔
                    this._downloader.SetDownloadProgress(1); // 設置Downloader的Progress = 1 (表示無需更新 = 完成)
                    this._complete?.Invoke();                // 完成處理的Handle

                    // 【切換狀態 - 無需更新】
                    this.executeStatus = ExecuteStatus.NO_NEED_TO_UPDATE_PATCH;

                    // 取消Task
                    this.TaskCancel();

                    Debug.Log($"無需更新資源 (當前已是最新版本)");
                    Debug.Log($"LOCAL RES_VER: {localCfg.RES_VERSION}, SERVER RES_VER: {this._serverCfg.RES_VERSION}");
                    return;
                }

                // 如果資源版本與服務端的資源版本不一致, 以下開始執行系列程序
                Debug.Log($"LOCAL RES_VER: {localCfg.RES_VERSION}, SERVER RES_VER: {this._serverCfg.RES_VERSION}");

                // Local配置檔與Server配置檔進行資源文件比對, 並且建立更新用的配置檔 (用於文件更新的依據配置檔)
                this._updateCfg = new VersionFileCfg();
                foreach (var svrFile in this._serverCfg.RES_FILES)
                {
                    var sFileName = svrFile.Key; // Server FileName (服務端資源名稱)
                    var sFile = svrFile.Value;   // Server File     (服務端資源資訊)

                    // 檢查[本地端的配置文件]是否有[服務端的配置文件]中的資源名稱 (如果沒有表示【新資源】)
                    if (!localCfg.HasFile(sFileName))
                    {
                        // 如果沒有, 則進行新增資源檔至要更新的配置檔中
                        this._updateCfg.AddResFileInfo(sFileName, sFile);
                    }
                    else
                    {
                        string localMd5 = localCfg.GetResFileInfo(sFileName).md5; // 本地端的檔案Md5
                        string svrMd5 = svrFile.Value.md5;                        // 服務端的檔案Md5

                        // 檢查[本地端的資源檔Md5]是否與[服務端的資源檔Md5]有不一致
                        if (localMd5 != svrMd5)
                        {
                            // 如果不一致 (表示資源有變更), 則進行新增資源檔至要更新的配置檔中
                            this._updateCfg.AddResFileInfo(sFileName, sFile);
                        }
                    }
                }

                // 取得更新包大小
                this._patchSizeBytes = this.GetUpdateSizeBytes(this._updateCfg);
                Debug.Log($"更新包大小: {this.GetUpdateTotalSizeToString(this._updateCfg)}, 更新數量: {this.GetUpdateCount(this._updateCfg)}");
                await UniTask.Delay(TimeSpan.FromSeconds(1f), ignoreTimeScale: false, PlayerLoopTiming.FixedUpdate, this.cts.Token);

                // 【切換狀態 - 下載更新包】, 下載更新包
                this.executeStatus = ExecuteStatus.DOWNLOAD_PATH;
                await this._downloader.DownloadFiles(this._progression);
                await UniTask.Delay(TimeSpan.FromSeconds(0.5f), ignoreTimeScale: false, PlayerLoopTiming.FixedUpdate, this.cts.Token);

                // 【切換狀態 - 開始寫入配置檔】, 等待所有文件寫入完畢
                this.executeStatus = ExecuteStatus.WRITE_CONFIG;
                this._DownloadFinishedAndWirteCfg(this._updateCfg);
            }
        }

        /// <summary>
        /// 從StreamingAssets中取得文字檔案
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private async UniTask<string> _GetFileFromStreamingAssets(string filePath)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(filePath))
            {
                await request.SendWebRequest().WithCancellation(this.cts.Token);

                if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log("<color=#FF0000>BundleConfig.json Not Exist in StreamingAssets.</color>");
                    Debug.Log(request.error);
                }
                else
                {
                    string json = request.downloadHandler.text;
                    Debug.Log("BundleConfig.json is Exist.");
                    return json;
                }
            }

            return "";
        }

        /// <summary>
        /// 從StreamingAssets中複製檔案
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="destFile"></param>
        /// <returns></returns>
        private async UniTask _CopyFileFromStreamingAssets(string sourceFile, string destFile)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(sourceFile))
            {
                await request.SendWebRequest().WithCancellation(this.cts.Token);

                if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log("<color=#FF0000>BundleConfig.json Not Exist in StreamingAssets.</color>");
                    Debug.Log(request.error);
                }
                else
                {
                    string json = request.downloadHandler.text;
                    File.WriteAllText(destFile, json);
                }
            }
        }

        /// <summary>
        /// 讀取本地端的記錄配置檔 (Persistent Data Path)
        /// </summary>
        private void _ReadLocalRecordCfg()
        {
            string localFilePath = Path.Combine(BundleConfig.GetLocalDlFileSaveDirectory(), $"{BundleConfig.recordCfgName}{BundleConfig.cfgExt}");
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
            string localFilePath = Path.Combine(BundleConfig.GetLocalDlFileSaveDirectory(), $"{BundleConfig.bundleCfgName}{BundleConfig.cfgExt}");
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
        private void _DownloadFinishedAndWirteCfg(VersionFileCfg updateCfg)
        {
            // 將[服務端的配置文件]寫入[本地端的配置文件]
            string localCfgPath = BundleConfig.GetLocalDlFileSaveBundleConfigPath();
            var svrCfgJson = JsonConvert.SerializeObject(this._serverCfg);
            File.WriteAllText(localCfgPath, svrCfgJson); // 進行寫入存儲

            // 取得[記錄配置檔]的路徑
            string recordCfgPath = Path.Combine(BundleConfig.GetLocalDlFileSaveDirectory(), $"{BundleConfig.recordCfgName}{BundleConfig.cfgExt}");

            // 檢查[記錄配置檔]是否存在
            if (File.Exists(recordCfgPath))
            {
                string recordFile = File.ReadAllText(recordCfgPath);

                VersionFileCfg recordCfg = JsonConvert.DeserializeObject<VersionFileCfg>(recordFile);
                foreach (var file in recordCfg.RES_FILES)
                {
                    string fileName = file.Key;
                    // 將上一次的[紀錄配置檔]進行合併至[更新配置檔] (有則修改, 無則添加)
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
                // 將[更新配置檔], 更新至[紀錄配置檔]
                var updateCfgJson = JsonConvert.SerializeObject(updateCfg);
                File.WriteAllText(recordCfgPath, updateCfgJson); // 進行寫入存儲
                this._ReadLocalCfg();                            // 讀取本地端配置檔
                this._ReadLocalRecordCfg();                      // 讀取本地[記錄配置檔] (因為已經進行重新寫入了)
                //this._complete?.Invoke();                        // 完成處理的Handle

                // 【切換狀態 - 完成配置寫入與更新】
                this.executeStatus = ExecuteStatus.COMPLETE_UPDATE_CONFIG;

                // 完成後取消Task
                this.TaskCancel();

                // 完成寫入後, 再執行一次檢查是否無需更新了
                this.Check(this._complete, this._progression);

                Debug.Log("完成下載, 並且完成寫入配置檔案.");
            }
            catch
            {
                Debug.Log("<color=#FF0000>Update RecordConfig.json failed.</color>");
            }
        }

        /// <summary>
        /// 製造Md5碼 (小寫)
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public string MakeMd5ForFile(string filePath)
        {
            FileStream fs = null;

            try
            {
                fs = new FileStream(filePath, FileMode.Open);
            }
            catch
            {
                fs?.Close();
                fs?.Dispose();
                return string.Empty;
            }


            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] fileMd5 = md5.ComputeHash(fs);
            fs.Close();
            fs.Dispose();

            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < fileMd5.Length; i++)
            {
                sBuilder.Append(fileMd5[i].ToString("x2"));
            }
            return sBuilder.ToString();

        }

        /// <summary>
        /// 取得配置檔中的資源檔總大小 (Bytes)
        /// </summary>
        /// <param name="cfg"></param>
        /// <returns></returns>
        public ulong GetUpdateSizeBytes(VersionFileCfg cfg)
        {
            ulong size = 0;
            foreach (var file in cfg.RES_FILES)
            {
                size += (ulong)file.Value.size;
            }

            return size;
        }

        /// <summary>
        /// 取得配置檔中的資源數
        /// </summary>
        /// <param name="cfg"></param>
        /// <returns></returns>
        public int GetUpdateCount(VersionFileCfg cfg)
        {
            if (cfg == null) return 0;

            return cfg.RES_FILES.Count;
        }

        /// <summary>
        /// 取得配置檔中的資源檔總大小
        /// </summary>
        /// <param name="cfg"></param>
        /// <returns></returns>
        public string GetUpdateTotalSizeToString(VersionFileCfg cfg)
        {
            if (cfg == null) return "";

            float size = 0;
            foreach (var file in cfg.RES_FILES)
            {
                size += file.Value.size;
            }

            if (size < (1024 * 1024))
            {
                return (size / 1024).ToString("f2") + "KB";
            }
            else if (size >= (1024 * 1024) && size < (1024 * 1024 * 1024))
            {
                return (size / (1024 * 1024)).ToString("f2") + "MB";
            }
            else
            {
                return (size / (1024 * 1024 * 1024)).ToString("f2") + "GB";
            }
        }

        /// <summary>
        /// Bytes傳輸速率轉換
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public string GetSpeedBytesToString(long bytes)
        {
            if (bytes < (1024 * 1024))
            {
                return ((float)bytes / 1024).ToString("f2") + "KB/s";
            }
            else if (bytes >= (1024 * 1024) && bytes < (1024 * 1024 * 1024))
            {
                return ((float)bytes / (1024 * 1024)).ToString("f2") + "MB/s";
            }
            else
            {
                return ((float)bytes / (1024 * 1024 * 1024)).ToString("f2") + "GB/s";
            }
        }

        /// <summary>
        /// 轉換Bytes為大小字串 (KB, MB, GB)
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public string GetBytesToString(long bytes)
        {
            if (bytes < (1024 * 1024))
            {
                return ((float)bytes / 1024).ToString("f2") + "KB";
            }
            else if (bytes >= (1024 * 1024) && bytes < (1024 * 1024 * 1024))
            {
                return ((float)bytes / (1024 * 1024)).ToString("f2") + "MB";
            }
            else
            {
                return ((float)bytes / (1024 * 1024 * 1024)).ToString("f2") + "GB";
            }
        }

        /// <summary>
        /// 刪除目錄
        /// </summary>
        /// <param name="dir"></param>
        private static void _DeleteFolder(string dir)
        {
            if (Directory.Exists(dir))
            {
                string[] fileEntries = Directory.GetFileSystemEntries(dir);
                for (int i = 0; i < fileEntries.Length; i++)
                {
                    string path = fileEntries[i];
                    if (File.Exists(path)) File.Delete(path);
                    else _DeleteFolder(path);
                }
                Directory.Delete(dir);
            }
        }
    }
}