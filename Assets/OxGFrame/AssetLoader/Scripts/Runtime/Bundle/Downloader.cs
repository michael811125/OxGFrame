using AssetLoader.Utility;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace OxGFrame.AssetLoader.Bundle
{
    public class Downloader
    {
        /// <summary>
        /// Progression 回調 (下載進度, 下載數量, 下載大小, 下載速度, 更新包大小)
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="dlCount"></param>
        /// <param name="dlBytes"></param>
        /// <param name="dlSpeed"></param>
        /// <param name="totalBytes"></param>
        public delegate void Progression(float progress, int dlCount, long dlBytes, int dlSpeed, ulong totalBytes);

        private bool _retryDownload = false;                         // 嘗試下載的開關
        private Stack<string> _stackWaitFiles;                       // 柱列下載快取

        public ulong totalBytes { get; private set; }                // 更新包大小
        public float dlProgress { get; private set; }                // 下載進度 (0.00 - 1.00)
        public int dlSpeed { get; private set; }                     // 下載速度
        public long dlBytes { get; private set; }                    // 下載大小
        public int dlCount { get; private set; }                     // 下載數量

        private FileStream _fs = null;
        private long _fileSize = 0;
        private BundleDistributor _bd = null;

        public Downloader()
        {
            this._stackWaitFiles = new Stack<string>();
        }

        public Downloader(BundleDistributor bd)
        {
            this._bd = bd;
            this._stackWaitFiles = new Stack<string>();
        }

        public void Reset()
        {
            if (this._fs != null) this._fs.Dispose();
            this._fs = null;
            this._fileSize = 0;

            this._stackWaitFiles.Clear();
            this.dlBytes = 0;
            this.dlCount = 0;

            this._retryDownload = false;
        }

        /// <summary>
        /// 設置 BundleDistributor (主要 Bundle 核心)
        /// </summary>
        /// <param name="bd"></param>
        public void SetBundleDistributor(BundleDistributor bd)
        {
            this._bd = bd;
        }

        /// <summary>
        /// 返回是否嘗試重新下載
        /// </summary>
        /// <returns></returns>
        public bool IsRetryDownload()
        {
            return this._retryDownload;
        }

        /// <summary>
        /// 設置下載進度值
        /// </summary>
        /// <param name="progress"></param>
        public void SetDownloadProgress(float progress)
        {
            this.dlProgress = progress;
        }

        /// <summary>
        /// 調用執行下載
        /// </summary>
        /// <returns></returns>
        public async UniTask DownloadFiles(Progression progression = null)
        {
            Debug.Log("<color=#ff0>【Resume from break-point】</color>");

            this.Reset();

            // 取得更新配置檔中要下載的資源文件, 進行下載等待柱列
            foreach (var file in this._bd.GetUpdateCfg().RES_FILES)
            {
                string fileName = file.Key;
                this._stackWaitFiles.Push(fileName);
            }

            // 取得更新配置檔中所有資源文件的檔案大小 (即更新包大小)
            this.totalBytes = BundleUtility.GetUpdateSizeBytes(this._bd.GetUpdateCfg());

            // 開始從下載等待柱列中取出下載 (後續則會自動進行下一個的取出下載)
            if (this._stackWaitFiles.Count > 0)
            {
                var fileName = this._stackWaitFiles.Pop();
                this.DownloadFile(this._bd.GetUpdateCfg().RES_FILES[fileName], fileName).Forget();
            }

            // 計算下載資訊
            await this.CalculateDownloadInfo(progression);
        }

        /// <summary>
        /// 單個檔案下載的執行程序
        /// </summary>
        /// <param name="file"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        protected async UniTask DownloadFile(ResFileInfo file, string fileName)
        {
            // fileName 本身夾帶目錄名稱 (才會需要特殊字串處理只取出單純的檔案名稱)
            var dlFileName = (fileName.IndexOf('/') != -1) ? fileName.Substring(fileName.LastIndexOf('/')).Replace("/", string.Empty) : fileName;
            var dlFile = file;
            var dlFullName = $@"/{fileName}";
            var filePath = BundleConfig.GetLocalDlFileSaveDirectory() + $@"{dlFullName}";
            string rdlMd5Name = BundleUtility.MakeMd5ForString($"sdlMd5-{dlFullName}");

            Debug.Log($"<color=#00FFE7>Downloading File, SavePath: {filePath}</color>");

            var folderPath = filePath.Replace(dlFileName, string.Empty);
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var rdlMd5 = PlayerPrefs.GetString(rdlMd5Name, string.Empty);
            if (string.IsNullOrEmpty(rdlMd5))
            {
                // 下載前記錄該資源的 Md5
                PlayerPrefs.SetString(rdlMd5Name, dlFile.md5);
            }

            // 判斷檔案是否已經存在於本地端, 再去判斷資源是否過期 or 下載完成了
            if (File.Exists(filePath))
            {
                // 判斷需要下載的資源 Md5 是否過期, 有則刪除舊檔案
                if (rdlMd5 != dlFile.md5)
                {
                    // 刪除資源檔
                    File.Delete(filePath);
                    // 記錄新資源的 Md5
                    PlayerPrefs.SetString(rdlMd5Name, dlFile.md5);
                }
                else
                {
                    // 讀取本地端檔案內容的 Md5
                    var localMd5 = BundleUtility.MakeMd5ForFile(filePath);
                    // 假如已經完成下載, 並且該資源檔的 Md5 與服務端的 Md5 一致, 則跳過處理執行下一個的下載
                    if (localMd5 == dlFile.md5)
                    {
                        this.dlBytes += dlFile.size;
                        this.dlCount += 1;
                        Debug.Log($"<color=#fff09f>Already has file: {dlFullName}. Skip to next! </color>");

                        this.NextFileDownload().Forget();
                        return;
                    }
                }
            }

            // 資源檔案的 Url
            var fileUrl = await BundleConfig.GetServerBundleUrl() + $@"/{this._bd.GetServerCfg().PRODUCT_NAME}" + $@"/{this._bd.GetServerCfg().EXPORT_NAME}" + $@"{dlFullName}";

            Debug.Log($"<color=#dbb6ff>Request FileUrl: {fileUrl}</color>");

            // 標檔頭請求
            UnityWebRequest header = null;
            try
            {
                header = UnityWebRequest.Head(fileUrl);
                await header.SendWebRequest().WithCancellation(this._bd.cts.Token);

                // 如果資源請求不到, 將進行重新請求嘗試
                if (header.result == UnityWebRequest.Result.ProtocolError || header.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log($"<color=#FF0000>Header failed. URL: {fileUrl}</color>");

                    this._retryDownload = true;
                    this._bd.SetExecuteStatus(BundleDistributor.ExecuteStatus.RETRYING_DOWNLOAD);

                    header?.Dispose();

                    return;
                }
            }
            catch
            {
                Debug.Log($"<color=#FF0000>Header failed. URL: {fileUrl}</color>");

                this._retryDownload = true;
                this._bd.SetExecuteStatus(BundleDistributor.ExecuteStatus.RETRYING_DOWNLOAD);

                header?.Dispose();
            }

            // 從標檔頭中取出請求該檔案的總長度 (總大小)
            long totalSize = long.Parse(header.GetResponseHeader("Content-Length"));
            header?.Dispose();

            // 從本地端讀取檔案, 並且累加該檔案大小作為下載的大小
            this._fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            this._fileSize = this._fs.Length;
            this.dlBytes += this._fileSize;

            // 如果本地端檔案大小 < 請求的檔案大小, 表示還沒有傳輸完成, 則進行請求下載
            if (this._fileSize < totalSize)
            {
                // 將文件的 index 起始位移至該檔案長度的 end, 是作為斷點續傳的主要依據 (中斷後, 就會依照檔案長度的 end 作為起始進行寫入)
                this._fs.Seek(this._fileSize, SeekOrigin.Begin);

                // [切片下載模式] 單個檔案的 totalSize > maxDownloadSliceSize
                if (totalSize > BundleConfig.maxDownloadSliceSize)
                {
                    #region 計算檔案切片數量
                    decimal decTotalSize = Convert.ToDecimal(totalSize);
                    decimal decMaxDownloadSliceSize = Convert.ToDecimal(BundleConfig.maxDownloadSliceSize);
                    // 取得檔案切片數量 (無條件進位)
                    int sliceCount = Convert.ToInt32(Math.Ceiling(decTotalSize / decMaxDownloadSliceSize));
                    long[] slices = new long[sliceCount];
                    #endregion

                    #region 如果中斷下載, 需要計算上一次的檔案下載的索引值
                    decimal decFileSize = Convert.ToDecimal(this._fileSize);
                    // 取得上一次檔案寫入數量 (無條件進位)
                    int lastCount = Convert.ToInt32(Math.Ceiling(decFileSize / decMaxDownloadSliceSize));
                    // 計算上一次的檔案寫入的 index
                    int lastIndex = (lastCount > 0) ? --lastCount : lastCount;
                    #endregion

                    long sliceTotalSize = 0;
                    for (int i = 0; i < slices.Length; i++)
                    {
                        // 最後一個切片需要計算差值
                        if (i == slices.Length - 1)
                        {
                            long maxSliceTotalSize = BundleConfig.maxDownloadSliceSize * sliceCount;
                            long lastSliceDiffSize = maxSliceTotalSize - totalSize;
                            slices[i] = BundleConfig.maxDownloadSliceSize - lastSliceDiffSize;
                        }
                        else slices[i] = BundleConfig.maxDownloadSliceSize;

                        // 切片累計總大小
                        sliceTotalSize += slices[i];

                        // 判斷小於上一次完成切片的 index 則跳過下載
                        if (i < lastIndex) continue;

                        Debug.Log($"<color=#f5ff6d>【Slice Downloading Mode】CurrentFileSize: {this._fileSize}, CurrentSliceTotalSize: {sliceTotalSize}, TotalSize: {totalSize}</color>");

                        await this._RequestAndDownload(fileUrl, sliceTotalSize);
                    }
                }
                // [正常下載模式] 單個檔案的 totalSize < maxDownloadSliceSize
                else
                {
                    Debug.Log($"<color=#f5ff6d>【Regular Downloading Mode】FileSize: {this._fileSize}, TotalSize: {totalSize}</color>");

                    await this._RequestAndDownload(fileUrl, totalSize);
                }
            }
            // 如果本地端大小 > 請求的檔案大小, 表示檔案大小異常, 則進行刪檔重新下載
            else
            {
                Debug.Log($"<color=#FF00A0>File size abnormal, Auto delete and download again. FileName: {fileName}</color>");

                // 先執行 fileStream 的釋放 (主要這樣才能進行 File.Delete)
                if (this._fs != null) this._fs.Dispose();
                this._fs = null;

                // 檔案異常進行刪除檔案, 並且將上次本地端檔案大小累加至 dlBytes 從中扣除
                File.Delete(filePath);
                this.dlBytes -= this._fileSize;

                // 最後再嘗試下載一次 (重新下載)
                this.DownloadFile(file, fileName).Forget();

                return;
            }

            // 下載完成後, Dispose FileStream 
            if (this._fs != null) this._fs.Dispose();
            this._fs = null;

            if (this._retryDownload) return;

            Debug.Log($"<color=#37ff46>FileDownload Completed. FileName: {dlFullName}</color>");
            this.dlCount += 1;

            // 執行下一個檔案的下載
            this.NextFileDownload().Forget();
        }

        /// <summary>
        /// 請求下載檔案
        /// </summary>
        /// <param name="fileUrl"></param>
        /// <param name="totalSize"></param>
        /// <returns></returns>
        private async UniTask _RequestAndDownload(string fileUrl, long totalSize)
        {
            // 檔案請求下載
            UnityWebRequest request = null;
            try
            {
                request = UnityWebRequest.Get(fileUrl);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Range", $"bytes={this._fileSize}-{totalSize}");
                request.SendWebRequest().WithCancellation(this._bd.cts.Token).Forget();
            }
            catch
            {
                Debug.Log($"<color=#FF0000>Request failed. URL: {fileUrl}</color>");

                this._retryDownload = true;
                this._bd.SetExecuteStatus(BundleDistributor.ExecuteStatus.RETRYING_DOWNLOAD);

                if (this._fs != null) this._fs.Dispose();
                this._fs = null;
                request?.Dispose();
            }

            int pos = 0;
            while (true)
            {
                if (request == null)
                {
                    Debug.Log($"<color=#FF0000>Request is null. URL: {fileUrl}</color>");
                    this._retryDownload = true;
                    this._bd.SetExecuteStatus(BundleDistributor.ExecuteStatus.RETRYING_DOWNLOAD);
                    break;
                }

                // 網路中斷後主線程異常, 編輯器下無法正常運行, 不過發佈模式沒問題
                if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log($"<color=#FF0000>Request failed. URL: {fileUrl}</color>");

                    this._retryDownload = true;
                    this._bd.SetExecuteStatus(BundleDistributor.ExecuteStatus.RETRYING_DOWNLOAD);

                    if (this._fs != null)
                    {
                        this._fs.Dispose();
                        this._fs = null;
                    }
                    request?.Dispose();

                    break;
                }

                byte[] buffer = request.downloadHandler.data; // 持續接收資料
                if (buffer != null && buffer.Length > 0)
                {
                    int fragLen = buffer.Length - pos;        // buffer 長度 - 記錄下來的 pos = 資料分片長度
                    this._fs.Write(buffer, pos, fragLen);     // 進行持續寫入, ex: 資料分片長度 (fragment length) = 10000 (buffer.len) - 9800 (pos)

                    this.dlSpeed += fragLen;                  // 資料分片長度 = download speed
                    this.dlBytes += fragLen;                  // 資料分片長度 = download bytes

                    pos += fragLen;                           // 記錄已接收資料分片寫入的長度pos
                    this._fileSize += fragLen;                // 記錄已接收資料分片的大小
                }

                // 最後將資料分片的 size 累加後, 如果有 >= totalSize 表示完成傳輸, 進行 break
                if ((this._fileSize >= totalSize) && request.isDone) break;

                // 等待一幀
                await UniTask.Yield(this._bd.cts.Token);
            }
        }

        /// <summary>
        /// 執行下一個要下載的檔案 (表示每當一個檔案下載完成後, 則調用執行下一個下載)
        /// </summary>
        /// <returns></returns>
        protected async UniTask NextFileDownload()
        {
            if (this._stackWaitFiles.Count > 0)
            {
                await UniTask.Yield(this._bd.cts.Token);

                var fileName = this._stackWaitFiles.Pop();

                this.DownloadFile(this._bd.GetUpdateCfg().RES_FILES[fileName], fileName).Forget();
            }
            else
            {
                // 如果找不到檔案下載, 則嘗試再次釋放 FileStream
                this._stackWaitFiles.Clear();
                if (this._fs != null) this._fs.Dispose();
                this._fs = null;
            }
        }

        /// <summary>
        /// 計算下載資訊 (Per Seconds)
        /// </summary>
        /// <returns></returns>
        protected async UniTask CalculateDownloadInfo(Progression progression)
        {
            while (true)
            {
                // 如果 allSize = 0, 直接歸零 + progression handle, 最後 break
                if (this.totalBytes == 0)
                {
                    this.dlSpeed = 0;
                    this.dlProgress = 1;
                    progression?.Invoke(this.dlProgress, this.dlCount, this.dlBytes, this.dlSpeed, this.totalBytes);
                    break;
                }

                // 計算進度 (pr = current size / total size)
                var progress = (float)this.dlBytes / this.totalBytes;
                this.dlProgress = progress;

                progression?.Invoke(this.dlProgress, this.dlCount, this.dlBytes, this.dlSpeed, this.totalBytes);
                Debug.Log($"<color=#ffcd00>Download Progress: {this.dlProgress.ToString("f2")}%, Download Speed: {BundleUtility.GetSpeedBytesToString((ulong)this.dlSpeed)}</color>");

                this.dlSpeed = 0; // 在 progression handle 後才進行歸零

                // 完成後 (以上已經執行歸零程序了), 也執行 progression handle, 最後 break
                if (progress >= 1)
                {
                    progression?.Invoke(this.dlProgress, this.dlCount, this.dlBytes, this.dlSpeed, this.totalBytes);
                    Debug.Log($"<color=#ffcd00>Download Progress: {this.dlProgress.ToString("f2")}%, Download Speed: {BundleUtility.GetSpeedBytesToString((ulong)this.dlSpeed)}</color>");
                    break;
                }

                // 每隔一秒執行一次 (下載速率需要歸零)
                await UniTask.Delay(TimeSpan.FromSeconds(1), ignoreTimeScale: true, PlayerLoopTiming.FixedUpdate, this._bd.cts.Token);
            }
        }

        ~Downloader()
        {
            this._stackWaitFiles.Clear();
            this._stackWaitFiles = null;
            if (this._fs != null) this._fs.Dispose();
            this._fs = null;
            this._bd = null;
        }
    }
}