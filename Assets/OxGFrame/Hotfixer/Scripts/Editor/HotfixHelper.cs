using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace OxGFrame.Hotfixer.Editor
{
    public static class HotfixHelper
    {
        internal const string MENU_ROOT = "OxGFrame/Hotfixer/";

        internal const string HOTFIX_COLLECTOR_DIR = "HotfixCollector";
        internal const string AOT_DLLS_DIR = "AOTDlls";
        internal const string HOTFIX_DLLS_DIR = "HotfixDlls";

        public static string ToRelativeAssetPath(string s)
        {
            return s.Substring(s.IndexOf("Assets/"));
        }

        #region MenuItems
        [MenuItem("HybridCLR/OxGFrame With HybridCLR/Compile And Copy To HotfixCollector")]
        public static void CompileAndCopyToHotfixCollector()
        {
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            CompileDllCommand.CompileDll(target);
            CopyAOTDllsAndHotfixDlls();
            AssetDatabase.Refresh();
        }
        #endregion

        /// <summary>
        /// Copy AOT Assembly and Hotfix Assembly files to HotfixCollector
        /// </summary>
        public static void CopyAOTDllsAndHotfixDlls()
        {
            CopyAOTAssembliesToDestination();
            CopyHotfixAssembliesToDestination();
        }

        /// <summary>
        /// Copy AOT Assembly files to destination
        /// </summary>
        /// <param name="dstDir"></param>
        public static void CopyAOTAssembliesToDestination(string dstDir = null)
        {
            var target = EditorUserBuildSettings.activeBuildTarget;

            string aotAssembliesSrcDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);
            string aotAssembliesDstDir = string.IsNullOrEmpty(dstDir) ? Path.Combine(Application.dataPath, HOTFIX_COLLECTOR_DIR, AOT_DLLS_DIR) : dstDir;

            if (Directory.Exists(aotAssembliesDstDir)) Directory.Delete(aotAssembliesDstDir, true);
            if (!Directory.Exists(aotAssembliesDstDir)) Directory.CreateDirectory(aotAssembliesDstDir);

            ulong totalBytesSize = 0;
            foreach (var dll in SettingsUtil.AOTAssemblyNames)
            {
                string dllPath = $"{aotAssembliesSrcDir}/{dll}.dll";
                if (!File.Exists(dllPath))
                {
                    Debug.LogError($"Add Metadata for AOT dll: {dllPath} is failed (File does not exist). The stripped AOT dlls can only be generated during BuildPlayer. Please do generate AOTDlls, after finished and then do copy again.");
                    continue;
                }
                FileInfo fileInfo = new FileInfo(dllPath);
                ulong bytesSize = (ulong)fileInfo.Length;
                totalBytesSize += bytesSize;
                string dllBytesPath = $"{aotAssembliesDstDir}/{dll}.dll.bytes";
                File.Copy(dllPath, dllBytesPath, true);

                Debug.Log($"[Copy AOTAssemblies To HotfixCollector] (AOT) {dll}.dll.bytes, Size: {GetBytesToString(bytesSize)}, {dllPath} -> {dllBytesPath}");
            }

            Debug.Log($"AOT Assemblies TotalSize: {GetBytesToString(totalBytesSize)}");
        }

        /// <summary>
        /// Copy Hotfix Assembly files to destination
        /// </summary>
        /// <param name="dstDir"></param>
        public static void CopyHotfixAssembliesToDestination(string dstDir = null)
        {
            var target = EditorUserBuildSettings.activeBuildTarget;

            string hotfixDllSrcDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
            string hotfixAssembliesDstDir = string.IsNullOrEmpty(dstDir) ? Path.Combine(Application.dataPath, HOTFIX_COLLECTOR_DIR, HOTFIX_DLLS_DIR) : dstDir;

            if (Directory.Exists(hotfixAssembliesDstDir)) Directory.Delete(hotfixAssembliesDstDir, true);
            if (!Directory.Exists(hotfixAssembliesDstDir)) Directory.CreateDirectory(hotfixAssembliesDstDir);

            ulong totalBytesSize = 0;
            foreach (var dll in SettingsUtil.HotUpdateAssemblyFilesExcludePreserved)
            {
                string dllPath = $"{hotfixDllSrcDir}/{dll}";
                FileInfo fileInfo = new FileInfo(dllPath);
                ulong bytesSize = (ulong)fileInfo.Length;
                totalBytesSize += bytesSize;
                string dllBytesPath = $"{hotfixAssembliesDstDir}/{dll}.bytes";
                File.Copy(dllPath, dllBytesPath, true);

                Debug.Log($"[Copy Hotfix Assemblies To HotfixCollector] (Hotfix) {dll}.bytes, Size: {GetBytesToString(bytesSize)}, {dllPath} -> {dllBytesPath}");
            }

            Debug.Log($"Hotfix Assemblies TotalSize: {GetBytesToString(totalBytesSize)}");
        }

        /// <summary>
        /// Bytes ToString (KB, MB, GB)
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        internal static string GetBytesToString(ulong bytes)
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

        /// <summary>
        /// 產生 Hotfix Dll 配置檔至輸出路徑 (Export HotfixDllConfig to StreamingAssets [for Built-in])
        /// </summary>
        /// <param name="aotDlls"></param>
        /// <param name="hotfixDlls"></param>
        /// <param name="outputPath"></param>
        public static void ExportHotfixDllConfig(List<string> aotDlls, List<string> hotfixDlls, bool cipher)
        {
            HotfixDllConfig config = new HotfixDllConfig(aotDlls, hotfixDlls);

            // 寫入配置文件
            WriteConfig(config, cipher ? ConfigFileType.Bytes : ConfigFileType.Json);

            Debug.Log($"【Export {HotfixConfig.HOTFIX_DLL_CFG_NAME} Completes】");
        }

        [MenuItem("Assets/OxGFrame/Hotfixer/Convert hotfixdllconfig.conf (BYTES [Cipher] <-> JSON [Plaintext])", false, -99)]
        private static void _ConvertConfigFile()
        {
            UnityEngine.Object selectedObject = Selection.activeObject;

            if (selectedObject != null)
            {
                // 獲取選中的資源的相對路徑
                string assetPath = AssetDatabase.GetAssetPath(selectedObject);

                // 檢查選中的文件是否位於 StreamingAssets 資料夾內
                if (assetPath.StartsWith("Assets/StreamingAssets"))
                {
                    // Application.dataPath 返回的是 Assets 資料夾的完整路徑
                    string fullPath = Path.Combine(Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length), assetPath);

                    // 確保文件存在
                    if (File.Exists(fullPath))
                    {
                        string fileName = HotfixConfig.HOTFIX_DLL_CFG_NAME;
                        if (fullPath.IndexOf(fileName) == -1)
                        {
                            Debug.LogWarning($"Incorrect file selected. Please select the {fileName} file.");
                            return;
                        }

                        // 讀取文件內容
                        byte[] data = File.ReadAllBytes(fullPath);
                        var info = BinaryHelper.DecryptToString(data);
                        HotfixDllConfig config = null;
                        bool isJsonConvertible;

                        try
                        {
                            config = JsonUtility.FromJson<HotfixDllConfig>(info.content);
                            isJsonConvertible = true;
                        }
                        catch (Exception ex)
                        {
                            isJsonConvertible = false;
                            Debug.LogException(new Exception("Convert failed: The content format is not valid JSON or it does not match the expected structure.", ex));
                        }

                        if (isJsonConvertible && config != null)
                        {
                            // 根據文件類型進行轉換
                            switch (info.type)
                            {
                                case ConfigFileType.Json:
                                    // JSON to Bytes
                                    WriteConfig(config, ConfigFileType.Bytes);
                                    break;
                                case ConfigFileType.Bytes:
                                    // Bytes to JSON
                                    WriteConfig(config, ConfigFileType.Json);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError($"File does not exist at path: {fullPath}");
                    }
                }
                else
                {
                    Debug.LogWarning("Selected file is not in StreamingAssets folder.");
                }
            }
            else
            {
                Debug.LogWarning("No file selected.");
            }
        }

        /// <summary>
        /// 寫入配置文件
        /// </summary>
        /// <param name="hotfixDllConfig"></param>
        /// <param name="configFileType"></param>
        internal static void WriteConfig(HotfixDllConfig hotfixDllConfig, ConfigFileType configFileType = ConfigFileType.Bytes)
        {
            string fileName = HotfixConfig.HOTFIX_DLL_CFG_NAME;
            string savePath = Path.Combine(Application.streamingAssetsPath, fileName);

            // 獲取文件夾路徑
            string directoryPath = Path.GetDirectoryName(savePath);

            // 檢查文件夾是否存在, 如果不存在則創建
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                Debug.Log($"Created directory: {directoryPath}");
            }

            switch (configFileType)
            {
                // Json 類型
                case ConfigFileType.Json:
                    {
                        // 將配置轉換為 JSON 字符串
                        string json = JsonUtility.ToJson(hotfixDllConfig, true);

                        // 寫入文件
                        File.WriteAllText(savePath, json);
                        AssetDatabase.Refresh();
                        Debug.Log($"Saved Hotfix Dll Config JSON to: {savePath}");
                    }
                    break;

                // Bytes 類型
                case ConfigFileType.Bytes:
                    {
                        // 將配置轉換為 JSON 字符串
                        string json = JsonUtility.ToJson(hotfixDllConfig, false);

                        // Binary
                        var writeBuffer = BinaryHelper.EncryptToBytes(json);

                        // 寫入配置文件
                        File.WriteAllBytes(savePath, writeBuffer);
                        AssetDatabase.Refresh();
                        Debug.Log($"Saved Hotfix Dll Config BYTES to: {savePath}");
                    }
                    break;
            }
        }

        /// <summary>
        /// 寫入文字文件檔
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="outputPath"></param>
        internal static void WriteTxt(string txt, string outputPath)
        {
            // 寫入配置文件
            var file = File.CreateText(outputPath);
            file.Write(txt);
            file.Close();
        }
    }
}