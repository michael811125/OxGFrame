using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace OxGFrame.Hotfixer.Editor
{
    public static class HotfixHelper
    {
        public const string hotfixCollectorDir = "HotfixCollector";
        public const string aotDllsDir = "AOTDlls";
        public const string hotfixDllsDir = "HotfixDlls";

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
            string aotAssembliesDstDir = string.IsNullOrEmpty(dstDir) ? Path.Combine(Application.dataPath, hotfixCollectorDir, aotDllsDir) : dstDir;

            if (Directory.Exists(aotAssembliesDstDir)) Directory.Delete(aotAssembliesDstDir, true);
            if (!Directory.Exists(aotAssembliesDstDir)) Directory.CreateDirectory(aotAssembliesDstDir);

            ulong totalBytesSize = 0;
            foreach (var dll in SettingsUtil.AOTAssemblyNames)
            {
                string dllPath = $"{aotAssembliesSrcDir}/{dll}.dll";
                if (!File.Exists(dllPath))
                {
                    Debug.LogError($"<color=#ff868a>Add Metadata for AOT dll: <color=#42ddff>{dllPath}</color> is failed (File does not exist). The stripped AOT dlls can only be generated during BuildPlayer. Please do generate AOTDlls, after finished and then do copy again.</color>");
                    continue;
                }
                FileInfo fileInfo = new FileInfo(dllPath);
                ulong bytesSize = (ulong)fileInfo.Length;
                totalBytesSize += bytesSize;
                string dllBytesPath = $"{aotAssembliesDstDir}/{dll}.dll.bytes";
                File.Copy(dllPath, dllBytesPath, true);

                Debug.Log($"<color=#a4ff86>[Copy AOTAssemblies To HotfixCollector] (AOT) <color=#ffcbde>{dll}.dll.bytes</color>, <color=#ffb542>Size: {GetBytesToString(bytesSize)}</color>, <color=#42ddff>{dllPath}</color> -> <color=#c686ff>{dllBytesPath}</color></color>");
            }

            Debug.Log($"<color=#ffb542>AOT Assemblies TotalSize: {GetBytesToString(totalBytesSize)}</color>");
        }

        /// <summary>
        /// Copy Hotfix Assembly files to destination
        /// </summary>
        /// <param name="dstDir"></param>
        public static void CopyHotfixAssembliesToDestination(string dstDir = null)
        {
            var target = EditorUserBuildSettings.activeBuildTarget;

            string hotfixDllSrcDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
            string hotfixAssembliesDstDir = string.IsNullOrEmpty(dstDir) ? Path.Combine(Application.dataPath, hotfixCollectorDir, hotfixDllsDir) : dstDir;

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

                Debug.Log($"<color=#a4ff86>[Copy Hotfix Assemblies To HotfixCollector] (Hotfix) <color=#ffcbde>{dll}.bytes</color>, <color=#ffb542>Size: {GetBytesToString(bytesSize)}</color>, <color=#42ddff>{dllPath}</color> -> <color=#c686ff>{dllBytesPath}</color></color>");
            }

            Debug.Log($"<color=#ffb542>Hotfix Assemblies TotalSize: {GetBytesToString(totalBytesSize)}</color>");
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
    }
}
