using OxGKit.LoggingSystem;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OxGFrame.AssetLoader.Bundle
{
    // Reference: YooAsset

    /// <summary>
    /// StreamingAssets目录下资源查询帮助类
    /// </summary>
    public sealed class StreamingAssetsHelper
    {
        private class PackageQuery
        {
            public readonly Dictionary<string, BuiltinFileManifest.Element> Elements;

            public PackageQuery()
            {
                this.Elements = new Dictionary<string, BuiltinFileManifest.Element>();
            }
        }

        private const string _MANIFEST_FILE_NAME = "BuiltinFileManifest";

        private static bool _isInit = false;
        private static Dictionary<string, PackageQuery> _packages;

        /// <summary>
        /// 内置文件查询方法
        /// </summary>
        public static bool FileExists(string packageName, string fileName, string fileCRC)
        {
#if UNITY_EDITOR
            return _FileExistsAtEditor(packageName, fileName, fileCRC);
#else
            return _FileExistsAtRuntime(packageName, fileName, fileCRC);
#endif
        }

        #region At Runtime
        private static bool _FileExistsAtRuntime(string packageName, string fileName, string fileCRC)
        {
            // Initialized
            {
                if (_isInit == false)
                {
                    _isInit = true;

                    if (_packages == null)
                        _packages = new Dictionary<string, PackageQuery>();

                    var manifest = Resources.Load<BuiltinFileManifest>(_MANIFEST_FILE_NAME);
                    if (manifest != null)
                    {
                        foreach (var element in manifest.BuiltinFiles)
                        {
                            if (_packages.TryGetValue(element.PackageName, out PackageQuery package) == false)
                            {
                                package = new PackageQuery();
                                _packages.Add(element.PackageName, package);
                            }
                            package.Elements.Add(element.FileName, element);
                        }
                    }
                }
            }

            // Check
            {
                if (_packages.TryGetValue(packageName, out PackageQuery package) == false)
                    return false;

                if (package.Elements.TryGetValue(fileName, out var element) == false)
                    return false;

                if (!string.IsNullOrEmpty(fileCRC))
                {
                    bool isCrcCorrect = element.FileCRC32 == fileCRC;
                    if (isCrcCorrect)
                    {
                        Logging.Print<Logger>($"<color=#00FF00>【Query】Search succeeded (File exists with CRC). File: {fileName}, CRC in manifest: {element.FileCRC32}, CRC of bundle: {fileCRC}</color>");
                        return true;
                    }
                    else
                    {
                        Logging.Print<Logger>($"<color=#FF0000>【Query】Search failed (CRC error). File: {fileName}, CRC in manifest: {element.FileCRC32}, CRC of bundle: {fileCRC}</color>");
                        return false;
                    }
                }
                else
                {
                    Logging.Print<Logger>($"<color=#00FF00>【Query】Search succeeded (File exists). File: {fileName}</color>");
                    return true;
                }
            }
        }
        #endregion

        #region At Editor
        private static bool _FileExistsAtEditor(string packageName, string fileName, string fileCRC)
        {
            var yooDefaultFolderName = PatchSetting.yooSetting.DefaultYooFolderName;
            string filePath = Path.Combine(Application.streamingAssetsPath, yooDefaultFolderName, packageName, fileName);
            if (File.Exists(filePath))
            {
                if (!string.IsNullOrEmpty(fileCRC))
                {
                    string crc32 = YooAsset.HashUtility.FileCRC32(filePath);
                    bool isCrcCorrect = crc32 == fileCRC;
                    if (isCrcCorrect)
                    {
                        Logging.Print<Logger>($"<color=#00FF00>【Query】Search succeeded (File exists with CRC). File: {fileName}, CRC in manifest: {crc32}, CRC of bundle: {fileCRC}</color>");
                        return true;
                    }
                    else
                    {
                        Logging.Print<Logger>($"<color=#FF0000>【Query】Search failed (CRC error). File: {fileName}, CRC in manifest: {crc32}, CRC of bundle: {fileCRC}</color>");
                        return false;
                    }
                }
                else
                {
                    Logging.Print<Logger>($"<color=#00FF00>【Query】Search succeeded (File exists). File: {filePath}</color>");
                    return true;
                }
            }

            Logging.Print<Logger>($"<color=#FF0000>【Query】Search failed (File doesn't exist). File: {filePath}</color>");
            return false;
        }
        #endregion
    }

#if UNITY_EDITOR
    internal class PreprocessBuild : UnityEditor.Build.IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }

        private static readonly string _resourcesPath = Path.Combine("Assets", "Resources");
        private const string _MANIFEST_FILE_NAME = "BuiltinFileManifest";
        private const string _MANIFEST_FILE_EXTENSION = ".asset";
        private const string _META_FILE_EXTENSION = ".meta";

        /// <summary>
        /// 在构建应用程序前处理
        /// </summary>
        public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            ExportBuiltinFileManifest();
        }

        [UnityEditor.MenuItem("OxGFrame/AssetLoader/" + "Export Built-in File Manifest (BuiltinFileManifest.asset)", false, 879)]
        private static void ExportBuiltinFileManifest()
        {
            string saveFilePath = Path.Combine(_resourcesPath, _MANIFEST_FILE_NAME + _MANIFEST_FILE_EXTENSION);
            string saveFileMetaPath = Path.Combine(_resourcesPath, _MANIFEST_FILE_NAME + _MANIFEST_FILE_EXTENSION + _META_FILE_EXTENSION);
            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
                File.Delete(saveFileMetaPath);
                UnityEditor.AssetDatabase.SaveAssets();
                UnityEditor.AssetDatabase.Refresh();
            }

            var yooDefaultFolderName = PatchSetting.yooSetting.DefaultYooFolderName;
            string folderPath = Path.Combine(Application.streamingAssetsPath, yooDefaultFolderName);
            DirectoryInfo root = new DirectoryInfo(folderPath);
            if (root.Exists == false)
            {
                Debug.Log($"<color=#43ffce>No Built-in Bundles Found: {folderPath}</color>");
                return;
            }

            var manifest = ScriptableObject.CreateInstance<BuiltinFileManifest>();
            FileInfo[] files = root.GetFiles("*", SearchOption.AllDirectories);
            foreach (var fileInfo in files)
            {
                if (fileInfo.Extension == _META_FILE_EXTENSION)
                    continue;
                if (fileInfo.Name.StartsWith(PatchSetting.yooSetting.ManifestFileName))
                    continue;

                BuiltinFileManifest.Element element = new BuiltinFileManifest.Element();
                element.PackageName = fileInfo.Directory.Name;
                element.FileCRC32 = YooAsset.HashUtility.FileCRC32(fileInfo.FullName);
                element.FileName = fileInfo.Name;
                manifest.BuiltinFiles.Add(element);
            }

            if (Directory.Exists(_resourcesPath) == false)
                Directory.CreateDirectory(_resourcesPath);
            UnityEditor.AssetDatabase.CreateAsset(manifest, saveFilePath);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            Debug.Log($"<color=#00FF00>Total File Count: {manifest.BuiltinFiles.Count} in Built-in (StreamingAssets). The BuiltinFileManifest Save Succeeded: {saveFilePath}</color>", manifest);
        }
    }
#endif
}