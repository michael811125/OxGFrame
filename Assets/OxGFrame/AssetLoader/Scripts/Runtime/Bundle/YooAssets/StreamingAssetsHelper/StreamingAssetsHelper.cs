using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using OxGKit.LoggingSystem;
using OxGKit.Utilities.Request;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YooAsset;

namespace OxGFrame.AssetLoader.Bundle
{
    /// <summary>
    /// StreamingAssets 目錄下文件查詢幫助類
    /// </summary>
    public sealed class StreamingAssetsHelper
    {
        /// <summary>
        /// 是否初始清單
        /// </summary>
        private static bool _isInitialized = false;

        /// <summary>
        /// 緩存 Built-in Package 清單
        /// </summary>
        private static HashSet<string> _queryPackages;

        /// <summary>
        /// 内置 Package 查詢方法
        /// </summary>
        public async static UniTask<bool> PackageExists(string packageName)
        {
#if UNITY_EDITOR
            return _PackageExistsAtEditor(packageName);
#else
            return await _PackageExistsAtRuntime(packageName);
#endif
        }

        #region At Runtime
        private async static UniTask<bool> _PackageExistsAtRuntime(string packageName)
        {
            // Initialized
            {
                if (!_isInitialized)
                {
                    _isInitialized = true;

                    if (_queryPackages == null)
                        _queryPackages = new HashSet<string>();

                    var manifest = await _GetBuildinPackageCatalogFileFromStreamingAssets();
                    if (manifest != null)
                    {
                        foreach (var package in manifest.Packages)
                        {
                            if (!_queryPackages.Contains(package))
                                _queryPackages.Add(package);
                        }
                    }
                }
            }

            // Check
            {
                if (!string.IsNullOrEmpty(packageName))
                {
                    // 透過清單查詢
                    bool exists = _queryPackages.Contains(packageName);
                    if (exists)
                    {
                        Logging.Print<Logger>($"<color=#00FF00>【Query Builtin-Package】Search succeeded (Package exists). Package: {packageName}</color>");
                        return exists;
                    }
                }

                Logging.Print<Logger>($"<color=#FF0000>【Query Builtin-Package】Search failed (Package doesn't exist). Package: {packageName}</color>");
                return false;
            }
        }
        #endregion

        #region At Editor
        private static bool _PackageExistsAtEditor(string packageName)
        {
            var yooDefaultFolderName = YooAssetSettingsData.GetDefaultYooFolderName();
            string dirPath = Path.Combine(Application.streamingAssetsPath, yooDefaultFolderName, packageName);

            // 直接檢查文件夾是否存在
            bool exists = Directory.Exists(dirPath);
            if (exists)
            {
                Logging.Print<Logger>($"<color=#00FF00>【Query Builtin-Package】Search succeeded (Package exists). Package: {packageName}</color>");
                return true;
            }

            Logging.Print<Logger>($"<color=#FF0000>【Query Builtin-Package】Search failed (Package doesn't exist). Package: {packageName}</color>");
            return false;
        }
        #endregion

        /// <summary>
        /// 從 StreamingAssets 中取得配置文件
        /// </summary>
        /// <returns></returns>
        private static async UniTask<BuiltinPackageCatalog> _GetBuildinPackageCatalogFileFromStreamingAssets()
        {
            string url = Path.Combine(BundleConfig.GetRequestStreamingAssetsPath(), $"{PatchSetting.setting.builtinPkgCatalogName}{PatchSetting.BUILTIN_CATALOG_EXTENSION}");
            string cfgJson = await Requester.RequestText(url, null, null, null, false);
            if (!string.IsNullOrEmpty(cfgJson))
                return JsonConvert.DeserializeObject<BuiltinPackageCatalog>(cfgJson);
            return null;
        }
    }

#if UNITY_EDITOR
    internal class PreprocessBuild : UnityEditor.Build.IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }

        private static readonly string _destinationPath = Application.streamingAssetsPath;

        public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            _ExportBuiltinPackageCatalogFile();
        }

        [UnityEditor.MenuItem("OxGFrame/AssetLoader/" + "Pre-Export Built-in Package Catalog File (builtinpkgcatalog.json)", false, 879)]
        private static void _ExportBuiltinPackageCatalogFile()
        {
            string saveFilePath = Path.Combine(_destinationPath, PatchSetting.setting.builtinPkgCatalogName + PatchSetting.BUILTIN_CATALOG_EXTENSION);
            string saveFileMetaPath = Path.Combine(_destinationPath, PatchSetting.setting.builtinPkgCatalogName + PatchSetting.BUILTIN_CATALOG_EXTENSION + PatchSetting.META_FILE_EXTENSION);
            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
                File.Delete(saveFileMetaPath);
                UnityEditor.AssetDatabase.Refresh();
            }

            var yooDefaultFolderName = YooAssetSettingsData.GetDefaultYooFolderName();
            string folderPath = Path.Combine(Application.streamingAssetsPath, yooDefaultFolderName);
            DirectoryInfo root = new DirectoryInfo(folderPath);
            if (root.Exists == false)
            {
                Debug.Log($"<color=#43ffce>No Built-in Packages Found: {folderPath}</color>");
                return;
            }

            var catalog = _CollectBuiltinPackages(root);

            if (Directory.Exists(_destinationPath) == false)
                Directory.CreateDirectory(_destinationPath);
            File.WriteAllText(saveFilePath, JsonConvert.SerializeObject(catalog, Formatting.Indented));
            UnityEditor.AssetDatabase.Refresh();
            Debug.Log($"<color=#00FF00>Total Package Count: {catalog.Packages.Count} in Built-in (StreamingAssets). The {PatchSetting.setting.builtinPkgCatalogName}{PatchSetting.BUILTIN_CATALOG_EXTENSION} Save Succeeded: {saveFilePath}</color>");
        }

        /// <summary>
        /// From YooAsset DefaultBuildinFileSystemBuild
        /// </summary>
        /// <exception cref="System.Exception"></exception>
        [UnityEditor.MenuItem("YooAsset/" + "OxGFrame Pre-Export Built-in Catalog File (BuildinCatalog) used by YooAsset", false, 1099)]
        private static void _ExportBuiltinCatalogFile()
        {
            DefaultBuildinFileSystemBuild.ExportBuildinCatalogFile();
        }

        private static BuiltinPackageCatalog _CollectBuiltinPackages(DirectoryInfo root)
        {
            var catalog = new BuiltinPackageCatalog();
            var dirs = root.GetDirectories("*", SearchOption.AllDirectories);

            // 避免重複設置
            var packageNames = new HashSet<string>();
            foreach (var dir in dirs)
            {
                string packageName = dir.Name;

                // 檢查 package name 是否已存在
                if (!packageNames.Contains(packageName))
                {
                    packageNames.Add(packageName);
                    catalog.Packages.Add(packageName);
                }
            }

            return catalog;
        }
    }
#endif
}