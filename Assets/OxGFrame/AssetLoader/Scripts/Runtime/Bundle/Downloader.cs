using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace AssetLoader.Bundle
{
    public class Downloader
    {

        public delegate void Progression(float progress, int dlCount, string dlSize, string dlSpeed); // Progression回調(下載進度, 下載數量, 下載大小, 下載速度)

        private ulong _allSizeBytes = 0;                                                              // 取得下載的總大小
        private bool _retryDownload = false;                                                          // 嘗試下載的開關
        private Stack<string> _stackWaitFiles = new Stack<string>();                                  // 柱列下載快取

        public float dlProgress { get; private set; }                                                 // 下載進度 (0.00 - 1.00)
        public int dlSpeed { get; private set; }                                                      // 下載速度
        public long dlBytes { get; private set; }                                                     // 下載大小
        public int dlCount { get; private set; }                                                      // 下載數量

        private BundleDistributor _bd = null;

        public Downloader(BundleDistributor bd)
        {
            this._bd = bd;
        }

        public void Reset()
        {
            this._stackWaitFiles.Clear();
            this.dlBytes = 0;
            this.dlCount = 0;

            this._retryDownload = false;
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
        /// <param name="pr"></param>
        public void SetDownloadProgress(float pr)
        {
            this.dlProgress = pr;
        }

        /// <summary>
        /// 調用執行下載
        /// </summary>
        /// <returns></returns>
        public async UniTask DownloadFiles(Progression progression = null)
        {
            Debug.Log("<color=#FFE700>【開始斷點續傳模式下載】</color>");

            this.Reset();

            // 取得更新配置檔中要下載的資源文件, 進行下載等待柱列
            foreach (var file in this._bd.GetUpdateCfg().RES_FILES)
            {
                string fileName = file.Key;
                this._stackWaitFiles.Push(fileName);
            }

            // 取得更新配置檔中所有資源文件的檔案大小 (即更新包大小)
            this._allSizeBytes = this._bd.GetUpdateSizeBytes(this._bd.GetUpdateCfg());

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
            // fileName本身夾帶目錄名稱 (才會需要特殊字串處理只取出單純的檔案名稱)
            var dlFileName = (fileName.IndexOf('/') != -1) ? fileName.Substring(fileName.LastIndexOf('/')).Replace("/", string.Empty) : fileName;
            var dlFile = file;
            var dlFullName = $@"/{fileName}";
            var filePath = BundleConfig.GetLocalDlFileSaveDirectory() + $@"{dlFullName}";
            string rdlMd5Name = $"sdlMd5: {dlFullName}";

            Debug.Log("Downloading FilePath: " + filePath);

            var folderPath = filePath.Replace(dlFileName, string.Empty);
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var rdlMd5 = PlayerPrefs.GetString(rdlMd5Name, "");
            if (string.IsNullOrEmpty(rdlMd5))
            {
                // 下載前紀錄該資源的Md5
                PlayerPrefs.SetString(rdlMd5Name, dlFile.md5);
            }
            else
            {
                // 判斷檔案是否已經存在於本地端, 再去判斷資源是否過期 or 下載完成了
                if (File.Exists(filePath))
                {
                    // 判斷需要下載的資源Md5是否過期, 有則刪除舊檔案
                    if (rdlMd5 != dlFile.md5)
                    {
                        File.Delete(filePath);
                        // 紀錄新資源的Md5
                        PlayerPrefs.SetString(rdlMd5Name, dlFile.md5);
                    }
                    else
                    {
                        var localMd5 = this._bd.MakeMd5ForFile(filePath);
                        // 假如已經下載好了, 但是該資源檔的Md5與服務端的Md5一致, 則跳過處理執行下一個的下載
                        if (localMd5 == dlFile.md5)
                        {
                            this.dlBytes += dlFile.size;
                            this.dlCount += 1;
                            Debug.Log($"<color=#00FFE7>Already has file: {dlFullName}. Skip to next! </color>");

                            this.NextFileDownload().Forget();
                            return;
                        }
                    }
                }
            }

            // 資源檔案的Url
            var fileUrl = await BundleConfig.GetServerBundleUrl() + $@"/{this._bd.GetServerCfg().PRODUCT_NAME}" + $@"/{this._bd.GetServerCfg().EXPORT_NAME}" + $@"{dlFullName}";

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

                    header?.Dispose();

                    return;
                }
            }
            catch
            {
                Debug.Log($"<color=#FF0000>Header failed. URL: {fileUrl}</color>");

                this._retryDownload = true;

                header?.Dispose();
            }

            // 從標檔頭中取出請求該檔案的總長度 (總大小)
            long totalSize = long.Parse(header.GetResponseHeader("Content-Length"));
            header?.Dispose();

            // 從本地端讀取檔案, 並且累加該檔案大小作為下載的大小
            FileStream fs = null;
            try
            {
                fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            }
            catch
            {
                this._retryDownload = true;

                fs?.Close();
                fs?.Dispose();

                return;
            }
            long fileSize = fs.Length;
            this.dlBytes += fileSize;

            // 如果本地端檔案大小 < 請求的檔案大小, 表示還沒有傳輸完成, 則進行請求下載
            if (fileSize < totalSize)
            {
                // 將文件的index起始位移至該檔案長度的end, 是作為斷點續傳的主要依據 (中斷後, 就會依照檔案長度的end作為起始進行寫入)
                fs.Seek(fileSize, SeekOrigin.Begin);

                // 檔案請求下載
                UnityWebRequest request = null;
                try
                {
                    request = UnityWebRequest.Get(fileUrl);
                    request.SetRequestHeader("Range", $"bytes={fileSize}-{totalSize}");
                    request.SendWebRequest().WithCancellation(this._bd.cts.Token).Forget();
                }
                catch
                {
                    Debug.Log($"<color=#FF0000>Request failed. URL: {fileUrl}</color>");

                    this._retryDownload = true;

                    request?.Dispose();
                }

                int pos = 0;
                while (true)
                {
                    if (request == null)
                    {
                        Debug.Log($"<color=#FF0000>Request is null. URL: {fileUrl}</color>");
                        this._retryDownload = true;
                        break;
                    }

                    // 網路中斷後主線程異常, 編輯器下無法正常運行, 不過發佈模式沒問題
                    if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
                    {
                        Debug.Log($"<color=#FF0000>Request failed. URL: {fileUrl}</color>");

                        this._retryDownload = true;

                        fs?.Close();
                        fs?.Dispose();
                        request?.Dispose();

                        break;
                    }

                    byte[] buffer = request?.downloadHandler?.data; // 持續接收資料
                    if (buffer != null && buffer.Length > 0)
                    {
                        int fragLen = buffer.Length - pos;          // buffer長度 - 記錄下來的pos = 資料分片長度
                        fs.Write(buffer, pos, fragLen);             // 進行持續寫入, ex: 資料分片長度(fragment length) = 10000(buffer.len) - 9800(pos)
                        this.dlSpeed += fragLen;                    // 資料分片長度 = download speed
                        this.dlBytes += fragLen;                    // 資料分片長度 = download bytes

                        pos += fragLen;                             // 記錄已接收資料分片寫入的長度pos
                        fileSize += fragLen;                        // 記錄已接收資料分片的大小
                    }

                    // 最後將資料分片的size累加後, 如果有 >= totalSize 表示完成傳輸, 進行break
                    if ((fileSize >= totalSize) && request.isDone) break;

                    // 等待一幀
                    await UniTask.Yield(this._bd.cts.Token);
                }
            }
            // 如果本地端大小 > 請求的檔案大小, 表示檔案大小異常, 則進行刪檔重新下載
            else
            {
                Debug.Log($"<color=#FF00A0>FileSize Error, Auto delete and download again. FileName: {dlFullName}</color>");

                // 先執行fileStream的釋放 (主要這樣才能進行File.Delete)
                fs?.Close();
                fs?.Dispose();

                // 檔案異常進行刪除檔案, 並且將上次本地端檔案大小累加至dlBytes從中扣除
                File.Delete(filePath);
                this.dlBytes -= fileSize;

                // 等待一幀
                await UniTask.Yield(this._bd.cts.Token);

                // 最後再嘗試下載一次 (重新下載)
                this.DownloadFile(dlFile, dlFileName).Forget();
                return;
            }

            fs?.Close();
            fs?.Dispose();

            if (this._retryDownload) return;

            Debug.Log($"<color=#3FFFD2>FileDownload Completed. FileName: {dlFullName}</color>");
            this.dlCount += 1;

            // 執行下一個檔案的下載
            this.NextFileDownload().Forget();
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
        }

        /// <summary>
        /// 計算下載資訊 (Per Seconds)
        /// </summary>
        /// <returns></returns>
        protected async UniTask CalculateDownloadInfo(Progression progression)
        {
            while (true)
            {
                // 如果allSize = 0, 直接歸零 + progression handle, 最後break
                if (this._allSizeBytes == 0)
                {
                    this.dlSpeed = 0;
                    this.dlProgress = 1;
                    progression?.Invoke(this.dlProgress, this.dlCount, this._bd.GetBytesToString(this.dlBytes), this._bd.GetSpeedBytesToString(this.dlSpeed));
                    break;
                }

                // 計算進度 (pr = current size / total size)
                var pr = (float)this.dlBytes / this._allSizeBytes;
                this.dlProgress = pr;

                progression?.Invoke(this.dlProgress, this.dlCount, this._bd.GetBytesToString(this.dlBytes), this._bd.GetSpeedBytesToString(this.dlSpeed));
                Debug.Log($"下載進度: {this.dlProgress.ToString("f2")}%, 下載速度: {this._bd.GetSpeedBytesToString(this.dlSpeed)}");

                this.dlSpeed = 0; // 在progression handle後才進行歸零

                // 完成後 (以上已經執行歸零程序了), 也執行progression handle, 最後break
                if (pr >= 1)
                {
                    progression?.Invoke(this.dlProgress, this.dlCount, this._bd.GetBytesToString(this.dlBytes), this._bd.GetSpeedBytesToString(this.dlSpeed));
                    Debug.Log($"下載進度: {this.dlProgress.ToString("f2")}%, 下載速度: {this._bd.GetSpeedBytesToString(this.dlSpeed)}");
                    break;
                }

                // 每隔一秒執行一次 (下載速率需要歸零)
                await UniTask.Delay(TimeSpan.FromSeconds(1), ignoreTimeScale: true, PlayerLoopTiming.FixedUpdate, this._bd.cts.Token);
            }
        }
    }
}
