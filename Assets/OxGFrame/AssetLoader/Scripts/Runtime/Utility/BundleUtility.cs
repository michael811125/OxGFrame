using System.IO;
using System.Text;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;

namespace OxGFrame.AssetLoader.Utility
{
    public static class BundleUtility
    {
        #region ToString
        /// <summary>
        /// 轉換 Bytes 傳輸速率
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string GetSpeedBytesToString(ulong bytes)
        {
            if (bytes < (1024 * 1024 * 1f))
            {
                return (bytes / 1024f).ToString("f2") + "KB/s";
            }
            else if (bytes >= (1024 * 1024 * 1f) && bytes < (1024 * 1024 * 1024 * 1f))
            {
                return (bytes / (1024 * 1024 * 1f)).ToString("f2") + "MB/s";
            }
            else
            {
                return (bytes / (1024 * 1024 * 1024 * 1f)).ToString("f2") + "GB/s";
            }
        }

        /// <summary>
        /// 轉換 Bytes 為大小字串 (KB, MB, GB)
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string GetBytesToString(ulong bytes)
        {
            if (bytes < (1024 * 1024 * 1f))
            {
                return (bytes / 1024f).ToString("f2") + "KB";
            }
            else if (bytes >= (1024 * 1024 * 1f) && bytes < (1024 * 1024 * 1024 * 1f))
            {
                return (bytes / (1024 * 1024 * 1f)).ToString("f2") + "MB";
            }
            else
            {
                return (bytes / (1024 * 1024 * 1024 * 1f)).ToString("f2") + "GB";
            }
        }
        #endregion

        #region MD5
        /// <summary>
        /// 【FilePath】生成檔案的 MD5 碼
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string MakeMd5ForFile(string filePath)
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
        /// 【FileInfo】生成檔案的 MD5 碼
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string MakeMd5ForFile(FileInfo file)
        {
            FileStream fs = file.OpenRead();
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] fileMd5 = md5.ComputeHash(fs);
            fs.Close();

            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < fileMd5.Length; i++)
            {
                sBuilder.Append(fileMd5[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        /// <summary>
        /// 【String】生成字串的 MD5 碼
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string MakeMd5ForString(string str)
        {
            using (var createMd5 = System.Security.Cryptography.MD5.Create())
            {
                // 將字串編碼成 UTF8 位元組陣列
                var bytes = Encoding.UTF8.GetBytes(str);

                // 取得雜湊值位元組陣列
                var hash = createMd5.ComputeHash(bytes);

                // 取得 MD5
                var md5 = BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();

                return md5;
            }
        }
        #endregion

        #region File Request
        /// <summary>
        /// 檔案請求 (byte)
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async UniTask<byte[]> FileRequestByte(string url, Action errorAction = null, CancellationTokenSource cts = null)
        {
            UnityWebRequest request = null;
            try
            {
                request = UnityWebRequest.Get(url);

                if (cts != null) await request.SendWebRequest().WithCancellation(cts.Token);
                else await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
                {
                    request.Dispose();
                    errorAction?.Invoke();
                    Debug.Log($"<color=#FF0000>Request failed, URL: {url}</color>");
                    return new byte[] { };
                }

                byte[] bytes = request.downloadHandler.data;
                request.Dispose();

                return bytes;
            }
            catch
            {
                request?.Dispose();
                errorAction?.Invoke();
                Debug.Log($"<color=#FF0000>Request failed, URL: {url}</color>");
                return new byte[] { };
            }
        }

        /// <summary>
        /// 檔案請求 (string)
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static async UniTask<string> FileRequestString(string url, Action errorAction = null, CancellationTokenSource cts = null)
        {
            UnityWebRequest request = null;
            try
            {
                request = UnityWebRequest.Get(url);

                if (cts != null) await request.SendWebRequest().WithCancellation(cts.Token);
                else await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
                {
                    request.Dispose();
                    errorAction?.Invoke();
                    Debug.Log($"<color=#FF0000>Request failed, URL: {url}</color>");
                    return null;
                }

                string text = request.downloadHandler.text;
                request.Dispose();

                return text;
            }
            catch
            {
                request?.Dispose();
                errorAction?.Invoke();
                Debug.Log($"<color=#FF0000>Request failed, URL: {url}</color>");
                return null;
            }
        }

        /// <summary>
        /// 從 StreamingAssets 中複製檔案 (for Android, iOS, WebGL)
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="destFile"></param>
        /// <returns></returns>
        public static async UniTask RequestAndCopyFileFromStreamingAssets(string sourceFile, string destFile, CancellationTokenSource cts = null)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(sourceFile))
            {
                if (cts != null) await request.SendWebRequest().WithCancellation(cts.Token);
                else await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log("<color=#FF0000>Request failed. Cannot found file in StreamingAssets.</color>");
                    Debug.Log(request.error);
                }
                else
                {
                    string json = request.downloadHandler.text;
                    File.WriteAllText(destFile, json);
                }
            }
        }
        #endregion

        #region File
        /// <summary>
        /// 刪除目錄 (包含底下所有的檔案與資料夾)
        /// </summary>
        /// <param name="dir"></param>
        public static void DeleteFolder(string dir)
        {
            if (Directory.Exists(dir))
            {
                string[] fileEntries = Directory.GetFileSystemEntries(dir);
                for (int i = 0; i < fileEntries.Length; i++)
                {
                    string path = fileEntries[i];
                    if (File.Exists(path)) File.Delete(path);
                    else DeleteFolder(path);
                }

                Directory.Delete(dir);
            }
        }

        /// <summary>
        /// 取得路徑目錄下所有檔案
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static FileInfo[] GetFilesRecursively(string dir)
        {
            DirectoryInfo root;
            FileInfo[] files;
            List<FileInfo> combineFiles = new List<FileInfo>();

            // STEP1. 先執行來源目錄下的檔案
            root = new DirectoryInfo(dir); // 取得該路徑目錄
            files = root.GetFiles();       // 取得該路徑目錄中的所有檔案
            foreach (var file in files)
            {
                combineFiles.Add(file);
            }

            // STEP2. 再執行來源目錄下的目錄檔案 (Recursively)
            foreach (string dirPath in Directory.GetDirectories(dir, "*", SearchOption.AllDirectories))
            {
                root = new DirectoryInfo(dirPath);
                files = root.GetFiles();
                foreach (var file in files)
                {
                    combineFiles.Add(file);
                }
            }

            return combineFiles.ToArray();
        }

        /// <summary>
        /// 複製來源路徑至輸出路徑
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="outputPath"></param>
        public static void CopyFolderRecursively(string inputPath, string outputPath)
        {
            if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

            // Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(inputPath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(inputPath, outputPath));
            }

            // Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(inputPath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(inputPath, outputPath), true);
            }
        }

        /// <summary>
        /// 開啟路徑目錄
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="autoCreateFolder"></param>
        public static void OpenFolder(string dir, bool autoCreateFolder = false)
        {
            if (autoCreateFolder)
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
            System.Diagnostics.Process.Start(dir);
        }
        #endregion

        #region Version
        /// <summary>
        /// Get version hash
        /// </summary>
        /// <param name="separator"></param>
        /// <param name="seed"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GetVersionHash(string separator, string seed, int length)
        {
            if (string.IsNullOrEmpty(seed)) return seed;

            byte[] bytes = Encoding.UTF8.GetBytes(seed);
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hash = md5.ComputeHash(bytes);
                string version = BitConverter.ToString(hash).Replace(separator, "");
                version = version.Substring(0, length);
                return version;
            }
        }

        /// <summary>
        /// Get version number
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GetVersionNumber(string seed, int length)
        {
            if (string.IsNullOrEmpty(seed)) return seed;

            byte[] bytes = Encoding.UTF8.GetBytes(seed);
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hash = md5.ComputeHash(bytes);
                ulong version = BitConverter.ToUInt64(hash, 0);
                string versionString = version.ToString();
                if (versionString.Length < length)
                {
                    versionString = versionString.PadRight(length, '0');
                }
                else if (versionString.Length > length)
                {
                    versionString = versionString.Substring(0, length);
                }
                return versionString;
            }
        }
        #endregion
    }
}
