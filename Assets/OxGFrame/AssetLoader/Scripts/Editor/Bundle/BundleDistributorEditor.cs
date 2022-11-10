using Newtonsoft.Json;
using OxGFrame.AssetLoader.Bundle;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Cysharp.Threading.Tasks;
using AssetLoader.Zip;
using UnityEngine.Networking;
using AssetLoader.Utility;

public static class BundleDistributorEditor
{
    public const string MenuRoot = "BundleDistributor/";

    /// <summary>
    /// 產生配置檔案來源路徑, 並且複製一份一樣的配置檔至輸出路徑 (如果 AssetBundle 採用全部皆為內置資源, 選擇此方式)
    /// </summary>
    /// <param name="productName"></param>
    /// <param name="inputPath"></param>
    /// <param name="outputPath"></param>
    /// <param name="isClearOutputPath"></param>
    public static void GenerateConfigToSourceFolderAndOnlyExportSameConfig(string productName, string inputPath, string outputPath, bool isClearOutputPath = true)
    {
        // 先產生配置檔至來源路徑
        GenerateBundleConfig(productName, inputPath, inputPath);

        // 合併輸出路徑 (專案名稱區分)
        string fullExportFolder = $"{outputPath}/{productName}";

        // 清空輸出路徑
        if (isClearOutputPath && Directory.Exists(fullExportFolder)) Directory.Delete(fullExportFolder, true);
        if (!Directory.Exists(fullExportFolder)) Directory.CreateDirectory(fullExportFolder);

        // 來源配置檔路徑
        string sourceFileName = Path.Combine(inputPath, BundleConfig.bundleCfgName + BundleConfig.cfgExtension);
        string destFileName = Path.Combine(fullExportFolder, BundleConfig.bundleCfgName + BundleConfig.cfgExtension);

        // 最後將來源配置檔複製至輸出路徑
        File.Copy(sourceFileName, destFileName);
    }

    /// <summary>
    /// 輸出 AssetBundle 並且製作一份輸出的配置檔 (更新資源)
    /// </summary>
    /// <param name="inputPath"></param>
    /// <param name="outputPath"></param>
    /// <param name="productName"></param>
    /// <param name="compressed"></param>
    /// <param name="isClearOutputPath"></param>
    public static void ExportBundleAndConfig(string inputPath, string outputPath, string productName = null, int compressed = 0, bool isClearOutputPath = true)
    {
        // 生成配置檔數據
        var cfg = GenerateCfg(productName, inputPath, compressed);

        productName = string.IsNullOrEmpty(productName) ? cfg.PRODUCT_NAME : productName;

        // 清空輸出路徑
        if (isClearOutputPath && Directory.Exists(outputPath)) BundleUtility.DeleteFolder(outputPath);
        if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

        // 進行資料夾之間的複製
        string sourceFullDir = inputPath;
        string destFullDir = Path.GetFullPath(outputPath + $@"/{productName}" + $@"/{cfg.EXPORT_NAME}");
        CopyFolderRecursively(sourceFullDir, destFullDir);

        // 針對壓縮包製作過程中, 會產生一個 record cfg 用於後續比對版本用的配置檔, 需要重新移動路徑
        if (Convert.ToBoolean(compressed))
        {
            string[] files = Directory.GetFiles(destFullDir, $"{BundleConfig.recordCfgName}{BundleConfig.cfgExtension}", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (file.IndexOf($"{BundleConfig.recordCfgName}{BundleConfig.cfgExtension}") != -1)
                {
                    string newDestFullDir = Path.Combine(outputPath + $@"/{productName}", BundleConfig.recordCfgName + BundleConfig.cfgExtension);
                    if (File.Exists(file))
                    {
                        // 備份
                        File.Copy(file, $"{file}{BundleConfig.bakCfgExtension}");
                        // 移動至根目錄
                        File.Move(file, newDestFullDir);
                    }
                }
            }
        }

        // 配置檔序列化, 將進行寫入
        string jsonCfg = JsonConvert.SerializeObject(cfg);

        // 寫入配置文件
        string writePath = Path.Combine(outputPath + $@"/{productName}", BundleConfig.bundleCfgName + BundleConfig.cfgExtension);
        WriteTxt(jsonCfg, writePath);
        // BAK
        string writeBakPath = Path.Combine(outputPath + $@"/{productName}" + $@"/{cfg.EXPORT_NAME}", BundleConfig.bundleCfgName + BundleConfig.cfgExtension + BundleConfig.bakCfgExtension);
        WriteTxt(jsonCfg, writeBakPath);

        Debug.Log($"<color=#00FF00>【Export Completes】 App Version: {cfg.APP_VERSION}, Patch Version: {cfg.RES_VERSION}, Export Name: {cfg.EXPORT_NAME}</color>");
    }

    /// <summary>
    /// 產生配置檔至輸出路徑
    /// </summary>
    /// <param name="productName"></param>
    /// <param name="inputPath"></param>
    /// <param name="outputPath"></param>
    /// <param name="compressed"></param>
    public static void GenerateBundleConfig(string productName, string inputPath, string outputPath, int compressed = 0)
    {
        // 生成配置檔數據
        var cfg = GenerateCfg(productName, inputPath, compressed);

        // 配置檔序列化, 將進行寫入
        string jsonCfg = JsonConvert.SerializeObject(cfg);

        // 寫入配置文件
        string writePath = Path.Combine(outputPath, BundleConfig.bundleCfgName + BundleConfig.cfgExtension);
        WriteTxt(jsonCfg, writePath);

        Debug.Log($"<color=#00FF00>【Export Config Completes】App Version: {cfg.APP_VERSION}, Patch Version: {cfg.RES_VERSION}</color>");
    }

    /// <summary>
    /// 返回來源路徑的配置檔數據
    /// </summary>
    /// <param name="productName"></param>
    /// <param name="inputPath"></param>
    /// <param name="compressed"></param>
    /// <returns></returns>
    public static VersionFileCfg GenerateCfg(string productName, string inputPath = null, int compressed = 0)
    {
        var cfg = new VersionFileCfg(); // 生成配置檔

        string bPath = (inputPath == null) ? BundleConfig.GetBuildBundlePath() : inputPath; // 取得Bundle路徑

        FileInfo[] files = BundleUtility.GetFilesRecursively(bPath);
        foreach (var file in files)
        {
            string fileName = Path.GetFileName(file.FullName);                                                    // 取得檔案名稱
            string dirName = Path.GetDirectoryName(file.FullName).Replace(Path.GetFullPath(bPath), string.Empty); // 取得目錄名稱
            long fileSize = file.Length;                                                                          // 取出檔案大小
            string fileMd5 = BundleUtility.MakeMd5ForFile(file);                                              // 生成檔案Md5

            if (fileName.IndexOf($"{BundleConfig.bundleCfgName}{BundleConfig.cfgExtension}") != -1 ||
                fileName.IndexOf($"{BundleConfig.recordCfgName}{BundleConfig.cfgExtension}") != -1) continue;

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

        // 是否為壓縮包 (0 = false, 1 = true)
        cfg.COMPRESSED = compressed;

        // 輸出名稱
        DateTime dt = (new DateTime(1970, 1, 1, 0, 0, 0)).AddHours(8).AddSeconds(timestamp);
        cfg.EXPORT_NAME = dt.ToString("yyyyMMddHHmmss");

        Debug.Log($"<color=#00FF00>Create Bundle Config Completes.</color>");

        return cfg;
    }

    /// <summary>
    /// 寫入文字文件檔
    /// </summary>
    /// <param name="txt"></param>
    /// <param name="outputPath"></param>
    public static void WriteTxt(string txt, string outputPath)
    {
        // 寫入配置文件
        var file = File.CreateText(outputPath);
        file.Write(txt);
        file.Close();
    }

    /// <summary>
    /// 複製來源路徑至 StreamingAssets
    /// </summary>
    /// <param name="inputPath"></param>
    public static void CopyToStreamingAssets(string inputPath)
    {
        int dialogState = EditorUtility.DisplayDialogComplex(
            "StreamingAssets Copy Notification",
            "Choose [copy and delete] will delete folder first and copy files to StremingAssets.\nChoose [copy and merge] will not delete folder but kept files with merge.",
            "copy and delete",
            "cancel",
            "copy and merge");

        if (dialogState == 1) return;

        string outputPath = Application.streamingAssetsPath;

        if (Directory.Exists(outputPath) && (dialogState == 0)) BundleUtility.DeleteFolder(outputPath);

        Directory.CreateDirectory(outputPath);

        string sourceFullDir = inputPath;
        string destFullDir = Path.GetFullPath(outputPath);
        CopyFolderRecursively(sourceFullDir, destFullDir);

        // auto refresh
        AssetDatabase.Refresh();

        Debug.Log("<color=#FFD500>Copy bundles to [StreamingAssets] successfully (Auto Refresh).</color>");
    }

    /// <summary>
    /// 複製來源路徑至 StreamingAssets
    /// </summary>
    /// <param name="inputPath"></param>
    /// <param name="clearFolders"></param>
    public static void CopyToStreamingAssets(string inputPath, bool clearFolders = false)
    {
        string destDir = Application.streamingAssetsPath;

        if (Directory.Exists(destDir) && clearFolders) BundleUtility.DeleteFolder(destDir);

        Directory.CreateDirectory(destDir);

        string sourceFullDir = inputPath;
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

        BundleUtility.DeleteFolder(dir);

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }

    public static void AesEncryptBundleFiles(string dir, string key = null, string iv = null)
    {
        // 取得目錄下所有檔案
        FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

        // 對所有檔案進行加密
        for (int i = 0; i < files.Length; i++)
        {
            // 執行各檔案的加密
            string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
            FileCryptogram.AES.AesEncryptFile(fPath, key, iv);
        }
    }

    public static void AesDecryptBundleFiles(string dir, string key = null, string iv = null)
    {
        // 取得目錄下所有檔案
        FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

        // 對所有檔案進行解密
        for (int i = 0; i < files.Length; i++)
        {
            // 執行各檔案的解密
            string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
            FileCryptogram.AES.AesDecryptFile(fPath, key, iv);
        }
    }

    public static void XorEncryptBundleFiles(string dir, byte key = 0)
    {
        // 取得目錄下所有檔案
        FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

        // 對所有檔案進行加密
        for (int i = 0; i < files.Length; i++)
        {
            // 執行各檔案的加密
            string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
            FileCryptogram.XOR.XorEncryptFile(fPath, key);
        }
    }

    public static void XorDecryptBundleFiles(string dir, byte key = 0)
    {
        // 取得目錄下所有檔案
        FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

        // 對所有檔案進行解密
        for (int i = 0; i < files.Length; i++)
        {
            // 執行各檔案的解密
            string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
            FileCryptogram.XOR.XorDecryptFile(fPath, key);
        }
    }

    public static void HTXorEncryptBundleFiles(string dir, byte hKey = 0, byte tKey = 0)
    {
        // 取得目錄下所有檔案
        FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

        // 對所有檔案進行加密
        for (int i = 0; i < files.Length; i++)
        {
            // 執行各檔案的加密
            string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
            FileCryptogram.HTXOR.HTXorEncryptFile(fPath, hKey, tKey);
        }
    }

    public static void HTXorDecryptBundleFiles(string dir, byte hKey = 0, byte tKey = 0)
    {
        // 取得目錄下所有檔案
        FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

        // 對所有檔案進行解密
        for (int i = 0; i < files.Length; i++)
        {
            // 執行各檔案的解密
            string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
            FileCryptogram.HTXOR.HTXorDecryptFile(fPath, hKey, tKey);
        }
    }

    public static void OffsetEncryptBundleFiles(string dir, int randomSeed, int dummySize = 0)
    {
        // 取得目錄下所有檔案
        FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

        // 對所有檔案進行加密
        for (int i = 0; i < files.Length; i++)
        {
            // 執行各檔案的加密
            string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
            FileCryptogram.Offset.OffsetEncryptFile(fPath, randomSeed, dummySize);
        }
    }

    public static void OffsetDecryptBundleFiles(string dir, int dummySize = 0)
    {
        // 取得目錄下所有檔案
        FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

        // 對所有檔案進行解密
        for (int i = 0; i < files.Length; i++)
        {
            // 執行各檔案的解密
            string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
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
        else Debug.Log($"<color=#00FF00>【Last Export Result】App Version: {PlayerPrefs.GetString(BundleConfig.APP_VERSION)}, Path Version: {PlayerPrefs.GetString(BundleConfig.RES_VERSION)}</color>");
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

    /// <summary>
    /// 執行版本檔案 Diff 並且取得刪除清單 (File Path)
    /// </summary>
    /// <param name="inputPath"></param>
    /// <param name="configFilePath"></param>
    /// <param name="diffMsg"></param>
    /// <param name="errorHandler"></param>
    /// <param name="delFiles"></param>
    /// <returns></returns>
    public static bool DiffBundleWithConfig(string inputPath, VersionFileCfg diffCfg, ref CompressorEditor.DiffMsg diffMsg, List<string> delFiles = null, Action<string> errorHandler = null)
    {
        diffMsg?.Clear();

        if (string.IsNullOrEmpty(inputPath))
        {
            errorHandler?.Invoke($"InputPath:\n{inputPath}\nis null or empty.");
            return false;
        }

        // 取得來源路徑中的所有檔案
        string[] sourceFiles = Directory.GetFiles(inputPath, "*.*", SearchOption.AllDirectories);

        if (diffCfg == null)
        {
            errorHandler?.Invoke($"Error cannot found config.");
            return false;
        }

        if (sourceFiles.Length == 0)
        {
            errorHandler?.Invoke($"Cannot found any files.");
            return false;
        }

        // 依照來源進行 Diff 檢查
        foreach (var file in sourceFiles)
        {
            // 路徑處理 (win 環境下路徑為 \ 需要取代 /)
            string sFilePath = (file.IndexOf("\\") != -1 ? file.Replace("\\", "/") : file).Replace(inputPath.IndexOf("\\") != -1 ? inputPath.Replace("\\", "/") : inputPath, string.Empty);
            // 去除第 1 字元 = /
            sFilePath = sFilePath.Substring(1, sFilePath.Length - 1);

            // 取得來源檔案的 MD5
            string sFileMd5 = BundleUtility.MakeMd5ForFile(file);

            bool checker = false;
            foreach (var pair in diffCfg.RES_FILES)
            {
                // 取得配置檔中的 Key = FilePathName
                string tFilePath = pair.Key;
                // 取得配置檔中的 Md5
                string tFileMd5 = pair.Value.md5;

                // 相同路徑 + 不同 MD5 = diff
                if (sFilePath == tFilePath && sFileMd5 != tFileMd5)
                {
                    diffMsg.AddDiffMsg($"{sFilePath} (MD5 {sFileMd5})", $"{tFilePath} (MD5 {tFileMd5})");

                    checker = true;
                    break;
                }
                // 相同路徑 + 相同 MD5 = same
                else if (sFilePath == tFilePath && sFileMd5 == tFileMd5)
                {
                    diffMsg.AddSameMsg($"{sFilePath} (MD5 {sFileMd5})", $"{tFilePath} (MD5 {tFileMd5})");

                    // 將相同路徑 + 相同 MD5 的檔案加入欲刪除清單中, 因為相同檔案則不必再打包
                    if (delFiles != null) delFiles.Add(sFilePath);

                    checker = true;
                    break;
                }
            }

            // 以上皆非 = new
            if (!checker) diffMsg.AddNewMsg($"{sFilePath} (MD5 {sFileMd5})");
        }

        return true;
    }

    /// <summary>
    /// 返回讀取本地的配置檔
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="errorHandler"></param>
    /// <returns></returns>
    public static VersionFileCfg GetConfigFromLocal(string filePath, Action<string> errorHandler = null)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            errorHandler?.Invoke($"ConfigFilePath:\n{filePath}\nis null or empty.");
            return null;
        }

        if (!File.Exists(filePath))
        {
            errorHandler?.Invoke($"ConfigFilePath file does not exist Error:\n{filePath}");
            return null;
        }

        // 讀取 Local 配置檔
        VersionFileCfg config = null;
        try
        {
            string json = File.ReadAllText(filePath);
            Debug.Log($"Read json:\n{json}");

            if (CheckConfigJsonFormat(json)) config = JsonConvert.DeserializeObject<VersionFileCfg>(json);
            else
            {
                errorHandler?.Invoke($"Config format incorrect read Error");
                return null;
            }
        }
        catch (Exception ex)
        {
            errorHandler?.Invoke($"File read Error: {ex}");
            return null;
        }

        return config;
    }

    /// <summary>
    /// 【異步 UnityWebRequest】返回請求 Server 的配置檔 (僅編輯器使用 or 使用 EditorCoroutine 調用)
    /// ※備註: 如果自行建立 BuildTool 使用 EditorCoroutine 調用, 需要 without -quit + 自行 EditorApplication.Exit(0)
    /// </summary>
    /// <param name="fileUrl"></param>
    /// <param name="errorHandler"></param>
    /// <returns></returns>
    public static async UniTask<VersionFileCfg> GetConfigFromServerAsyncUnityWebRequest(string fileUrl, Action<string> errorHandler = null)
    {
        if (string.IsNullOrEmpty(fileUrl))
        {
            errorHandler?.Invoke($"ConfigFileURL:\n{fileUrl}\nis null or empty.");
            return null;
        }

        // 請求 Server 的配置檔
        VersionFileCfg config = null;
        try
        {
            using (UnityWebRequest request = UnityWebRequest.Get(fileUrl))
            {
                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
                {
                    errorHandler?.Invoke($"Config request failed. URL: {fileUrl}");
                    return null;
                }
                else
                {
                    string json = request.downloadHandler.text;
                    Debug.Log($"Request json:\n{json}");

                    if (CheckConfigJsonFormat(json)) config = JsonConvert.DeserializeObject<VersionFileCfg>(json);
                    else
                    {
                        errorHandler?.Invoke($"Config format incorrect read Error");
                        return null;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            errorHandler?.Invoke($"Config request failed. URL: {fileUrl}\n{ex}");
            return null;
        }

        return config;
    }

    /// <summary>
    /// 【異步 WWW】返回請求 Server 的配置檔 (僅編輯器使用 or 使用 EditorCoroutine 調用)
    /// ※備註: 如果自行建立 BuildTool 使用 EditorCoroutine 調用, 需要 without -quit + 自行 EditorApplication.Exit(0)
    /// </summary>
    /// <param name="fileUrl"></param>
    /// <param name="errorHandler"></param>
    /// <returns></returns>
    public static async UniTask<VersionFileCfg> GetConfigFromServerAsyncWWW(string fileUrl, Action<string> errorHandler = null)
    {
        if (string.IsNullOrEmpty(fileUrl))
        {
            errorHandler?.Invoke($"ConfigFileURL:\n{fileUrl}\nis null or empty.");
            return null;
        }

        // 請求 Server 的配置檔
        VersionFileCfg config = null;
        string json = string.Empty;
        try
        {
            using (WWW www = new WWW(fileUrl))
            {
                while (www != null)
                {
                    if (www.isDone)
                    {
                        json = www.text;
                        Debug.Log($"Request json:\n{json}");
                        break;
                    }

                    await UniTask.Yield();
                }

                if (!string.IsNullOrEmpty(www.error))
                {
                    errorHandler?.Invoke($"Config request failed. URL: {fileUrl}");
                    return null;
                }

                if (CheckConfigJsonFormat(json)) config = JsonConvert.DeserializeObject<VersionFileCfg>(json);
                else
                {
                    errorHandler?.Invoke($"Config format incorrect read Error");
                    return null;
                }
            }
        }
        catch (Exception ex)
        {
            errorHandler?.Invoke($"Config request failed. URL: {fileUrl}\n{ex}");
            return null;
        }

        return config;
    }

    /// <summary>
    /// UnityWebRequest 無法運行於 batchmode (建議使用 WWW)
    /// </summary>
    /// <param name="fileUrl"></param>
    /// <param name="errorHandler"></param>
    /// <returns></returns>
    public static VersionFileCfg GetConfigFromServerUnityWebRequest(string fileUrl, Action<string> errorHandler = null)
    {
        if (string.IsNullOrEmpty(fileUrl))
        {
            errorHandler?.Invoke($"ConfigFileURL:\n{fileUrl}\nis null or empty.");
            return null;
        }

        // 請求 Server 的配置檔
        VersionFileCfg config = null;
        try
        {
            UnityWebRequest request = UnityWebRequest.Get(fileUrl);
            string json = string.Empty;
            while (request != null)
            {
                if (request.isDone)
                {
                    json = request.downloadHandler.text;
                    Debug.Log($"Request json:\n{json}");
                    request.Dispose();
                    break;
                }
            }

            if (CheckConfigJsonFormat(json)) config = JsonConvert.DeserializeObject<VersionFileCfg>(json);
            else
            {
                errorHandler?.Invoke($"Config format incorrect read Error");
                request.Dispose();
                return null;
            }
        }
        catch (Exception ex)
        {
            errorHandler?.Invoke($"Config request failed. URL: {fileUrl}\n{ex}");
            return null;
        }

        return config;
    }

    /// <summary>
    /// WWW 可運行於 batchmode (自行建立 BuildTool 時, 建議使用此方法調用)
    /// </summary>
    /// <param name="fileUrl"></param>
    /// <param name="errorHandler"></param>
    /// <returns></returns>
    public static VersionFileCfg GetConfigFromServerWWW(string fileUrl, Action<string> errorHandler = null)
    {
        if (string.IsNullOrEmpty(fileUrl))
        {
            errorHandler?.Invoke($"ConfigFileURL:\n{fileUrl}\nis null or empty.");
            return null;
        }

        // 請求 Server 的配置檔
        VersionFileCfg config = null;
        try
        {
            WWW www = new WWW(fileUrl);
            string json = string.Empty;
            while (www != null)
            {
                if (www.isDone)
                {
                    json = www.text;
                    Debug.Log($"Request json:\n{json}");
                    www.Dispose();
                    break;
                }
            }

            if (CheckConfigJsonFormat(json)) config = JsonConvert.DeserializeObject<VersionFileCfg>(json);
            else
            {
                errorHandler?.Invoke($"Config format incorrect read Error");
                www.Dispose();
                return null;
            }
        }
        catch (Exception ex)
        {
            errorHandler?.Invoke($"Config request failed. URL: {fileUrl}\n{ex}");
            return null;
        }

        return config;
    }

    /// <summary>
    /// 檢查配置檔案格式
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static bool CheckConfigJsonFormat(string json)
    {
        if (string.IsNullOrEmpty(json)) return false;

        // 檢查配置檔案格式 (文本中需要符合全部包含的 keys)
        for (int i = 0; i < VersionFileCfg.keys.Length; i++)
        {
            if (json.IndexOf(VersionFileCfg.keys[i]) == -1) return false;
        }

        return true;
    }

    /// <summary>
    /// 【異步】壓縮並且執行刪除清單的處理
    /// </summary>
    /// <param name="inputPath"></param>
    /// <param name="outputPath"></param>
    /// <param name="zipName"></param>
    /// <param name="md5ForZipName"></param>
    /// <param name="password"></param>
    /// <param name="delFiles"></param>
    /// <param name="delTemp"></param>
    /// <param name="errorHandler"></param>
    /// <param name="progression"></param>
    /// <returns></returns>
    public static async UniTask<bool> ZipAndDelDiffAsync(bool outputPathIncludingSourceFiles, string inputPath, string outputPath, string zipName, bool md5ForZipName = true, string password = null, List<string> delFiles = null, bool delTemp = true, bool clearOutputPath = true, Action<string> errorHandler = null, Compressor.Progression progression = null)
    {
        if (string.IsNullOrEmpty(inputPath) || string.IsNullOrEmpty(outputPath))
        {
            errorHandler?.Invoke($"InputPath:\n{inputPath}\nor\nOutputPath:\n{outputPath}\nis null or empty.");
            return false;
        }

        // 清空輸出路徑
        if (clearOutputPath && Directory.Exists(outputPath)) Directory.Delete(outputPath, true);
        if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

        // 將要壓縮的來源路徑進行複製作為壓縮處理用 (因為盡量不要直接更動來源檔案)
        var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
        string tempFolderName = $"source_zip_temp_{timestamp}";
        string tempFolderPath = Path.Combine(outputPath, tempFolderName);
        if (!Directory.Exists(tempFolderPath)) Directory.CreateDirectory(tempFolderPath);

        // 複製來源路徑至暫存路徑 (作為壓縮處理用)
        CopyFolderRecursively(inputPath, tempFolderPath);

        // 是否輸出包含來源檔案 (散檔)
        if (outputPathIncludingSourceFiles) CopyFolderRecursively(inputPath, outputPath);

        // 再過濾之前, 需要製作一份來源檔案的完整配置檔案, 用於後面熱更新打包比對用
        var cfg = GenerateCfg(null, inputPath);
        cfg.APP_VERSION = null;
        cfg.RES_VERSION = null;
        cfg.EXPORT_NAME = null;

        // 配置檔序列化, 將進行寫入
        string jsonCfg = JsonConvert.SerializeObject(cfg);

        // 寫入配置文件
        string writePath = Path.Combine(outputPath, BundleConfig.recordCfgName + BundleConfig.cfgExtension);
        WriteTxt(jsonCfg, writePath);

        // 開始刪除不壓進包內的檔案 (delFiles)
        if (delFiles != null)
        {
            foreach (var delFile in delFiles)
            {
                string delFilePath = Path.Combine(tempFolderPath, delFile);
                if (File.Exists(delFilePath)) File.Delete(delFilePath);
            }
        }

        // 開始執行壓縮
        bool result = await ZipAsync(tempFolderPath, outputPath, zipName, md5ForZipName, password, progression);

        // 壓縮完成後, 刪除暫存來源
        if (delTemp) BundleUtility.DeleteFolder(tempFolderPath);

        return result;
    }

    /// <summary>
    /// 壓縮並且執行刪除清單的處理
    /// </summary>
    /// <param name="inputPath"></param>
    /// <param name="outputPath"></param>
    /// <param name="zipName"></param>
    /// <param name="md5ForZipName"></param>
    /// <param name="password"></param>
    /// <param name="delFiles"></param>
    /// <param name="delTemp"></param>
    /// <param name="clearOutputPath"></param>
    /// <param name="errorHandler"></param>
    /// <returns></returns>
    public static bool ZipAndDelDiff(bool outputPathIncludingSourceFiles, string inputPath, string outputPath, string zipName, bool md5ForZipName = true, string password = null, List<string> delFiles = null, bool delTemp = true, bool clearOutputPath = true, Action<string> errorHandler = null)
    {
        if (string.IsNullOrEmpty(inputPath) || string.IsNullOrEmpty(outputPath))
        {
            errorHandler?.Invoke($"InputPath:\n{inputPath}\nor\nOutputPath:\n{outputPath}\nis null or empty.");
            return false;
        }

        // 清空輸出路徑
        if (clearOutputPath && Directory.Exists(outputPath)) Directory.Delete(outputPath, true);
        if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

        // 將要壓縮的來源路徑進行複製作為壓縮處理用 (因為盡量不要直接更動來源檔案)
        var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
        string tempFolderName = $"source_zip_temp_{timestamp}";
        string tempFolderPath = Path.Combine(outputPath, tempFolderName);
        if (!Directory.Exists(tempFolderPath)) Directory.CreateDirectory(tempFolderPath);

        // 複製來源路徑至暫存路徑 (作為壓縮處理用)
        CopyFolderRecursively(inputPath, tempFolderPath);

        // 是否輸出包含來源檔案 (散檔)
        if (outputPathIncludingSourceFiles) CopyFolderRecursively(inputPath, outputPath);

        // 再過濾之前, 需要製作一份來源檔案的完整配置檔案, 用於後面熱更新打包比對用
        var cfg = GenerateCfg(null, inputPath);
        cfg.APP_VERSION = null;
        cfg.RES_VERSION = null;
        cfg.EXPORT_NAME = null;

        // 配置檔序列化, 將進行寫入
        string jsonCfg = JsonConvert.SerializeObject(cfg);

        // 寫入配置文件
        string writePath = Path.Combine(outputPath, BundleConfig.recordCfgName + BundleConfig.cfgExtension);
        WriteTxt(jsonCfg, writePath);

        // 開始刪除不壓進包內的檔案 (delFiles)
        if (delFiles != null)
        {
            foreach (var delFile in delFiles)
            {
                string delFilePath = Path.Combine(tempFolderPath, delFile);
                if (File.Exists(delFilePath)) File.Delete(delFilePath);
            }
        }

        // 開始執行壓縮
        bool result = Zip(tempFolderPath, outputPath, zipName, md5ForZipName, password);

        // 壓縮完成後, 刪除暫存來源
        if (delTemp) BundleUtility.DeleteFolder(tempFolderPath);

        return result;
    }

    public static async UniTask<bool> ZipAsync(string inputPath, string outputPath, string zipName, bool md5ForZipName = true, string password = null, Compressor.Progression progression = null)
    {
        if (string.IsNullOrEmpty(inputPath) || string.IsNullOrEmpty(outputPath)) return false;

        if (string.IsNullOrEmpty(zipName)) zipName = "abzip";

        string[] fileNameArgs = zipName.Split('.');
        string fileName = fileNameArgs[0];
        string fileExtension = fileNameArgs.Length > 1 ? fileNameArgs[1] : string.Empty;

        // 只針對檔案名稱進行 md5
        if (md5ForZipName) fileName = BundleUtility.MakeMd5ForString(fileName);

        zipName = string.IsNullOrEmpty(fileExtension) ? fileName : $@"{fileName}.{fileExtension}";
        outputPath = Path.Combine(outputPath, zipName);

        bool result = await Compressor.ZipAsync(inputPath, outputPath, password, 6, ICSharpCode.SharpZipLib.Zip.CompressionMethod.Deflated, null, progression);

        return result;
    }

    public static bool Zip(string inputPath, string outputPath, string zipName, bool md5ForZipName = true, string password = null)
    {
        if (string.IsNullOrEmpty(inputPath) || string.IsNullOrEmpty(outputPath)) return false;

        if (string.IsNullOrEmpty(zipName)) zipName = "abzip";

        string[] fileNameArgs = zipName.Split('.');
        string fileName = fileNameArgs[0];
        string fileExtension = fileNameArgs.Length > 1 ? fileNameArgs[1] : string.Empty;

        // 只針對檔案名稱進行 md5
        if (md5ForZipName) fileName = BundleUtility.MakeMd5ForString(fileName);

        zipName = string.IsNullOrEmpty(fileExtension) ? fileName : $@"{fileName}.{fileExtension}";
        outputPath = Path.Combine(outputPath, zipName);

        bool result = Compressor.Zip(inputPath, outputPath, password, 6, ICSharpCode.SharpZipLib.Zip.CompressionMethod.Deflated, null);

        return result;
    }

    public static async UniTask<bool> UnzipAsync(string filePath, string outputPath, string password = null, int bufferSize = 65536, bool clearOutputPath = true, Action<string> errorHandler = null, Compressor.Progression progression = null)
    {
        if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(outputPath))
        {
            errorHandler?.Invoke($"FilePath:\n{filePath}\nor\nOutputPath:\n{outputPath}\nis null or empty.");
            return false;
        }

        // 清空輸出路徑
        if (clearOutputPath && Directory.Exists(outputPath)) Directory.Delete(outputPath, true);
        if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

        bool result = await Compressor.UnzipAsync(filePath, outputPath, password, null, bufferSize, progression);

        return result;
    }

    public static bool Unzip(string filePath, string outputPath, string password = null, int bufferSize = 65536, bool clearOutputPath = true, Action<string> errorHandler = null)
    {
        if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(outputPath))
        {
            errorHandler?.Invoke($"FilePath:\n{filePath}\nor\nOutputPath:\n{outputPath}\nis null or empty.");
            return false;
        }

        // 清空輸出路徑
        if (clearOutputPath && Directory.Exists(outputPath)) Directory.Delete(outputPath, true);
        if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

        bool result = Compressor.Unzip(filePath, outputPath, password, null, bufferSize);

        return result;
    }
}