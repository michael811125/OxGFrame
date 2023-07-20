using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace OxGFrame.AssetLoader.Bundle
{
    // Reference: YooAsset

#if UNITY_EDITOR
    /// <summary>
    /// StreamingAssets目录下资源查询帮助类
    /// </summary>
    public sealed class StreamingAssetsHelper
    {
        public static void Init() { }
        public static bool FileExists(string packageName, string fileName)
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, BundleConfig.yooDefaultFolderName, packageName, fileName);
            if (File.Exists(filePath))
            {
                Debug.Log($"<color=#00FF00>【Query】Request succeeded. File: {filePath}</color>");
                return true;
            }

            Debug.Log($"<color=#FF0000>【Query】Request failed. File: {filePath}</color>");
            return false;
        }
    }
#else
    /// <summary>
    /// StreamingAssets目录下资源查询帮助类
    /// </summary>
    public sealed class StreamingAssetsHelper
    {
        private static bool _isInit = false;
        private static readonly HashSet<string> _cacheData = new HashSet<string>();

        /// <summary>
        /// 初始化
        /// </summary>
        public static void Init()
        {
            if (_isInit == false)
            {
                _isInit = true;
                var manifest = Resources.Load<BuildinFileManifest>("BuildinFileManifest");
                if (manifest != null)
                {
                    foreach (string fileName in manifest.BuildinFiles)
                    {
                        _cacheData.Add(fileName);
                    }
                }
            }
        }

        /// <summary>
        /// 内置文件查询方法
        /// </summary>
        public static bool FileExists(string packageName, string fileName)
        {
            if (_isInit == false) Init();

            if (_cacheData.Contains(fileName))
            {
                Debug.Log($"<color=#00FF00>【Query】Request succeeded. File: {fileName}</color>");
                return true;
            }

            Debug.Log($"<color=#FF0000>【Query】Request failed. File: {fileName}</color>");
            return false;
        }
    }
#endif

#if UNITY_EDITOR && UNITY_WEBGL
    internal class PreprocessBuild : UnityEditor.Build.IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }

        /// <summary>
        /// 在构建应用程序前处理
        /// </summary>
        public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            string saveFilePath = "Assets/Resources/BuildinFileManifest.asset";
            if (File.Exists(saveFilePath))
                File.Delete(saveFilePath);

            string folderPath = $"{Application.dataPath}/StreamingAssets/{BundleConfig.yooDefaultFolderName}";
            DirectoryInfo root = new DirectoryInfo(folderPath);
            if (root.Exists == false)
            {
                Debug.Log($"<color=#43ffce>No Built-in Bundles Found: {folderPath}</color>");
                return;
            }

            var manifest = ScriptableObject.CreateInstance<BuildinFileManifest>();
            FileInfo[] files = root.GetFiles("*", SearchOption.AllDirectories);
            foreach (var fileInfo in files)
            {
                if (fileInfo.Extension == ".meta")
                    continue;
                if (fileInfo.Name.StartsWith("PackageManifest_"))
                    continue;
                manifest.BuildinFiles.Add(fileInfo.Name);
            }

            if (Directory.Exists("Assets/Resources") == false)
                Directory.CreateDirectory("Assets/Resources");
            UnityEditor.AssetDatabase.CreateAsset(manifest, saveFilePath);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            Debug.Log($"<color=#00FF00>Total File Count: {manifest.BuildinFiles.Count} in Built-in. The BuildinFileManifest Save Succeeded: {saveFilePath}</color>");
        }
    }
#endif
}