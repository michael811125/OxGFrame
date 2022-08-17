using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace AssetLoader.Bundle
{
    public static class BundleDistributorEditor
    {
        public const string MenuRoot = "BundleDistributor/";

        public static void ExportBundleAndConfig(string sourceDir, string destDir, string productName = null)
        {
            // 生成配置檔數據
            var cfg = GenerateCfg(productName, sourceDir);

            productName = (productName == null) ? cfg.PRODUCT_NAME : productName;

            if (Directory.Exists(destDir)) DeleteFolder(destDir);

            Directory.CreateDirectory(destDir);

            // 進行資料夾之間的複製
            string sourceFullDir = sourceDir;
            string destFullDir = Path.GetFullPath(destDir + $@"/{productName}" + $@"/{cfg.EXPORT_NAME}");
            CopyFolderRecursively(sourceFullDir, destFullDir);

            // 配置檔序列化, 將進行寫入
            string jsonCfg = JsonConvert.SerializeObject(cfg);

            // 寫入配置文件
            string writePath = Path.Combine(destDir + $@"/{productName}", BundleConfig.bundleCfgName + BundleConfig.cfgExt);
            WriteTxt(jsonCfg, writePath);
            // BAK
            string writeBakPath = Path.Combine(destDir + $@"/{productName}" + $@"/{cfg.EXPORT_NAME}", BundleConfig.bundleCfgName + BundleConfig.cfgExt + ".bak");
            WriteTxt(jsonCfg, writeBakPath);

            Debug.Log($"<color=#00FF00>【成功輸出目錄】 主程式版本: {cfg.APP_VERSION}, 資源檔版本: {cfg.RES_VERSION}, 輸出名稱: {cfg.EXPORT_NAME}</color>");
        }

        public static void GenerateBundleCfg(string productName, string sourceDir, string destDir)
        {
            // 生成配置檔數據
            var cfg = GenerateCfg(productName, sourceDir);

            // 配置檔序列化, 將進行寫入
            string jsonCfg = JsonConvert.SerializeObject(cfg);

            // 寫入配置文件
            string writePath = Path.Combine(destDir, BundleConfig.bundleCfgName + BundleConfig.cfgExt);
            WriteTxt(jsonCfg, writePath);

            Debug.Log($"<color=#00FF00>【成功建立配置檔】主程式版本: {cfg.APP_VERSION}, 資源檔版本: {cfg.RES_VERSION}</color>");
        }

        public static VersionFileCfg GenerateCfg(string productName, string sourcePath = null)
        {
            var cfg = new VersionFileCfg(); // 生成配置檔

            string bPath = (sourcePath == null) ? BundleConfig.GetBuildedBundlePath() : sourcePath; // 取得Bundle路徑

            FileInfo[] files = GetFilesRecursively(bPath);
            foreach (var file in files)
            {
                string fileName = Path.GetFileName(file.FullName);                                                    // 取得檔案名稱
                string dirName = Path.GetDirectoryName(file.FullName).Replace(Path.GetFullPath(bPath), string.Empty); // 取得目錄名稱
                long fileSize = file.Length;                                                                          // 取出檔案大小
                string fileMd5 = MakeMd5ForFile(file);                                                                // 生成檔案Md5

                if (fileName == $"{BundleConfig.bundleCfgName}{BundleConfig.cfgExt}") continue;

                ResFileInfo rf = new ResFileInfo();      // 建立資源檔資訊
                rf.fileName = fileName;                  // 檔案名稱
                rf.dirName = dirName.Replace(@"\", "/"); // 目錄名稱
                rf.size = fileSize;                      // 檔案大小
                rf.md5 = fileMd5;                        // 檔案md5

                string fullName = $@"{rf.dirName}/{rf.fileName}";
                fullName = fullName.Substring(1, fullName.Length - 1);
                cfg.AddResFileInfo(fullName, rf);        // 加入配置檔的快取中
            }

            // 產品名稱
            cfg.PRODUCT_NAME = productName;

            // 主程式版本
            cfg.APP_VERSION = Application.version;
            PlayerPrefs.SetString(BundleConfig.APP_VERSION, cfg.APP_VERSION);

            // 資源檔版本 (建議使用時間加入資源版本控制)
            var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            cfg.RES_VERSION = $"{cfg.APP_VERSION}<{timestamp}>";
            PlayerPrefs.SetString(BundleConfig.RES_VERSION, cfg.RES_VERSION);

            // 輸出名稱
            DateTime dt = (new DateTime(1970, 1, 1, 0, 0, 0)).AddHours(8).AddSeconds(timestamp);
            cfg.EXPORT_NAME = dt.ToString("yyyyMMddHHmmss");

            Debug.Log($"<color=#00FF00>成功建立配置檔數據!!!</color>");

            return cfg;
        }

        /// <summary>
        /// 寫入文字文件檔
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="writePath"></param>
        public static void WriteTxt(string txt, string writePath)
        {
            // 寫入配置文件
            var file = File.CreateText(writePath);
            file.Write(txt);
            file.Close();
        }

        public static void CopyToStreamingAssets(string sourceDir)
        {
            int dialogState = EditorUtility.DisplayDialogComplex(
                "StreamingAssets Copy Notification",
                "Choose [copy and delete] will delete folder first and copy files to StremingAssets.\nChoose [copy and merge] will not delete folder but kept files with merge.",
                "copy and delete",
                "cancel",
                "copy and merge");

            if (dialogState == 1) return;

            string destDir = Application.streamingAssetsPath;

            if (Directory.Exists(destDir) && (dialogState == 0)) DeleteFolder(destDir);

            Directory.CreateDirectory(destDir);

            string sourceFullDir = sourceDir;
            string destFullDir = Path.GetFullPath(destDir);
            CopyFolderRecursively(sourceFullDir, destFullDir);

            // auto refresh
            AssetDatabase.Refresh();

            Debug.Log("<color=#FFD500>Copy bundles to [StreamingAssets] successfully (Auto Refresh).</color>");
        }

        public static void CopyToStreamingAssets(string sourceDir, bool clearFolders = false)
        {
            string destDir = Application.streamingAssetsPath;

            if (Directory.Exists(destDir) && clearFolders) DeleteFolder(destDir);

            Directory.CreateDirectory(destDir);

            string sourceFullDir = sourceDir;
            string destFullDir = Path.GetFullPath(destDir);
            CopyFolderRecursively(sourceFullDir, destFullDir);

            // auto refresh
            AssetDatabase.Refresh();

            Debug.Log("<color=#FFD500>Copy bundles to [StreamingAssets] successfully (Auto Refresh).</color>");
        }

        [MenuItem(MenuRoot + "Local Download Directory (Persistent Data Path)/Open Download Directory", false, 298)]
        public static void OpenDownloadDir()
        {
            var dir = BundleConfig.GetLocalDlFileSaveDirectory();
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            System.Diagnostics.Process.Start(dir);
        }

        [MenuItem(MenuRoot + "Local Download Directory (Persistent Data Path)/Clear Download Directory", false, 299)]
        public static void ClearDownloadDir()
        {
            bool operate = EditorUtility.DisplayDialog(
                "Clear Download Folder",
                "Are you sure you want to delete download folder?",
                "yes",
                "no");

            if (!operate) return;

            var dir = BundleConfig.GetLocalDlFileSaveDirectory();

            DeleteFolder(dir);

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        public static void AesEncryptBundleFiles(string dir, string key = null, string iv = null)
        {
            // 取得目錄下所有檔案
            FileInfo[] files = GetFilesRecursively(dir);

            // 對所有檔案進行加密
            for (int i = 0; i < files.Length; i++)
            {
                // 略過Config檔案
                if (files[i].Name == BundleConfig.bundleCfgName + BundleConfig.cfgExt) continue;

                // 執行各檔案的加密
                string fPath = files[i].Directory.ToString() + $@"\{files[i].Name}";
                FileCryptogram.AES.AesEncryptFile(fPath, key, iv);
            }
        }

        public static void AesDecryptBundleFiles(string dir, string key = null, string iv = null)
        {
            // 取得目錄下所有檔案
            FileInfo[] files = GetFilesRecursively(dir);

            // 對所有檔案進行解密
            for (int i = 0; i < files.Length; i++)
            {
                // 略過Config檔案
                if (files[i].Name == BundleConfig.bundleCfgName + BundleConfig.cfgExt) continue;

                // 執行各檔案的解密
                string fPath = files[i].Directory.ToString() + $@"\{files[i].Name}";
                FileCryptogram.AES.AesDecryptFile(fPath, key, iv);
            }
        }

        public static void XorEncryptBundleFiles(string dir, byte key = 0)
        {
            // 取得目錄下所有檔案
            FileInfo[] files = GetFilesRecursively(dir);

            // 對所有檔案進行加密
            for (int i = 0; i < files.Length; i++)
            {
                // 略過Config檔案
                if (files[i].Name == BundleConfig.bundleCfgName + BundleConfig.cfgExt) continue;

                // 執行各檔案的加密
                string fPath = files[i].Directory.ToString() + $@"\{files[i].Name}";
                FileCryptogram.XOR.XorEncryptFile(fPath, key);
            }
        }

        public static void XorDecryptBundleFiles(string dir, byte key = 0)
        {
            // 取得目錄下所有檔案
            FileInfo[] files = GetFilesRecursively(dir);

            // 對所有檔案進行解密
            for (int i = 0; i < files.Length; i++)
            {
                // 略過Config檔案
                if (files[i].Name == BundleConfig.bundleCfgName + BundleConfig.cfgExt) continue;

                // 執行各檔案的解密
                string fPath = files[i].Directory.ToString() + $@"\{files[i].Name}";
                FileCryptogram.XOR.XorDecryptFile(fPath, key);
            }
        }

        public static void OffsetEncryptBundleFiles(string dir, int randomSeed, int dummySize = 0)
        {
            // 取得目錄下所有檔案
            FileInfo[] files = GetFilesRecursively(dir);

            // 對所有檔案進行加密
            for (int i = 0; i < files.Length; i++)
            {
                // 略過Config檔案
                if (files[i].Name == BundleConfig.bundleCfgName + BundleConfig.cfgExt) continue;

                // 執行各檔案的加密
                string fPath = files[i].Directory.ToString() + $@"\{files[i].Name}";
                FileCryptogram.Offset.OffsetEncryptFile(fPath, randomSeed, dummySize);
            }
        }

        public static void OffsetDecryptBundleFiles(string dir, int dummySize = 0)
        {
            // 取得目錄下所有檔案
            FileInfo[] files = GetFilesRecursively(dir);

            // 對所有檔案進行解密
            for (int i = 0; i < files.Length; i++)
            {
                // 略過Config檔案
                if (files[i].Name == BundleConfig.bundleCfgName + BundleConfig.cfgExt) continue;

                // 執行各檔案的解密
                string fPath = files[i].Directory.ToString() + $@"\{files[i].Name}";
                FileCryptogram.Offset.OffsetDecryptFile(fPath, dummySize);
            }
        }

        [MenuItem(MenuRoot + "Show Last Exported Versions Log", false, 1099)]
        public static void ShowVersions()
        {
            if (string.IsNullOrEmpty(PlayerPrefs.GetString(BundleConfig.APP_VERSION)) || string.IsNullOrEmpty(PlayerPrefs.GetString(BundleConfig.RES_VERSION)))
            {
                Debug.Log("<color=#FF0000>Cannot found Versions record !!! (Execute Generate BundleConfig first.)</color>");
            }
            else Debug.Log($"<color=#00FF00>【最後一次輸出的結果】主程式版本: {PlayerPrefs.GetString(BundleConfig.APP_VERSION)}, 資源檔版本: {PlayerPrefs.GetString(BundleConfig.RES_VERSION)}</color>");
        }

        public static void CopyFolderRecursively(string sourceDir, string destDir)
        {
            if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);

            // Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourceDir, destDir));
            }

            // Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourceDir, destDir), true);
            }
        }

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

        public static FileInfo[] GetFilesRecursively(string sourcePath)
        {
            DirectoryInfo root;
            FileInfo[] files;
            List<FileInfo> combineFiles = new List<FileInfo>();

            // STEP1. 先執行來源目錄下的檔案
            root = new DirectoryInfo(sourcePath); // 取得該路徑目錄
            files = root.GetFiles();              // 取得該路徑目錄中的所有檔案
            foreach (var file in files)
            {
                combineFiles.Add(file);
            }

            // STEP2. 再執行來源目錄下的目錄檔案 (Recursively)
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
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

        /// <summary>
        /// 生成檔案的MD5碼
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
    }
}