using Cysharp.Threading.Tasks;
using OxGKit.LoggingSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using YooAsset;

namespace OxGFrame.AssetLoader.Bundle
{
    /// <summary>
    /// StreamingAssets 目錄下文件查詢幫助類
    /// </summary>
    public sealed class StreamingAssetsHelper
    {
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
            if (_queryPackages == null)
                _queryPackages = new HashSet<string>();

            // Check
            if (!string.IsNullOrEmpty(packageName))
            {
                bool exists = _queryPackages.Contains(packageName);
                if (exists)
                {
                    Logging.Print<Logger>($"<color=#00FF00>【Try Query Builtin-Package】Search succeeded (Package exists). Package: {packageName}</color>");
                    return true;
                }

                // Dynamic try query
                string fileName = YooAssetBridge.DefaultBuildinFileSystemDefine.BuildinCatalogBinaryFileName();
                if (await _TryQueryBuiltinPackageFileExists(packageName, fileName))
                {
                    _queryPackages.Add(packageName);
                    return true;
                }
            }

            return false;
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
                Logging.Print<Logger>($"<color=#00FF00>【Try Query Builtin-Package】Search succeeded (Package exists). Package: {packageName}</color>");
                return true;
            }

            Logging.Print<Logger>($"<color=#FF0000>【Try Query Builtin-Package】Search failed (Package doesn't exist). Package: {packageName}</color>");
            return false;
        }
        #endregion

        #region Try Request Builtin-Package
        /// <summary>
        /// 嘗試請求內置資源清單是否存在 (true = 存在, false = 不存在)
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private async static UniTask<bool> _TryQueryBuiltinPackageFileExists(string packageName, string fileName)
        {
            // Builtin (StreamingAssets)
            string builtinPackagePath = BundleConfig.GetBuiltinPackagePath(packageName);
            string url = Path.Combine(builtinPackagePath, fileName);
            // Convert url to www path
            return await _WebRequestPartialBytes(YooAssetBridge.DownloadSystemHelper.ConvertToWWWPath(url));
        }

        private async static UniTask<bool> _WebRequestPartialBytes(string url)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfterSlim(TimeSpan.FromSeconds(10));

            UnityWebRequest request = UnityWebRequest.Get(url);
            request.downloadHandler = new DownloadHandlerBuffer();

            try
            {
                await request.SendWebRequest().WithCancellation(cts.Token);

                if (request.result == UnityWebRequest.Result.DataProcessingError ||
                    request.result == UnityWebRequest.Result.ProtocolError ||
                    request.result == UnityWebRequest.Result.ConnectionError)
                {
                    Logging.Print<Logger>($"<color=#ff1743>【Try Query Builtin-Package】Request failed (Package doesn't exist). Built-in package not found. URL: {url}</color>");
                    request.Dispose();
                    return false;
                }

                if (request.downloadedBytes > 0 && request.responseCode < 400)
                {
                    Logging.Print<Logger>($"<color=#19ff17>【Try Query Builtin-Package】Request succeeded (Package exists). Built-in package found. Code: {request.responseCode}, PartialBytes: {request.downloadedBytes}, URL: {url}</color>");
                    request.Dispose();
                    return true;
                }
            }
            catch (OperationCanceledException ex) when (ex.CancellationToken == cts.Token)
            {
                Logging.PrintWarning<Logger>("【Try Query Builtin-Package】Request timed out");
                request.Dispose();
                return false;
            }
            catch (Exception ex)
            {
                Logging.PrintWarning<Logger>($"【Try Query Builtin-Package】Request failed (Package doesn't exist) The package may not exist: {ex}");
                request.Dispose();
                return false;
            }

            return false;
        }
        #endregion

#if UNITY_EDITOR
        internal class PreprocessExporter
        {
            /// <summary>
            /// From YooAsset DefaultBuildinFileSystemBuild
            /// </summary>
            /// <exception cref="System.Exception"></exception>
            [UnityEditor.MenuItem("YooAsset/" + "OxGFrame Pre-Export Built-in Catalog File (BuildinCatalog) used by YooAsset", false, 1099)]
            private static void _ExportBuiltinCatalogFile()
            {
                string rootPath = YooAssetBridge.YooAssetSettingsData.GetYooDefaultBuildinRoot();
                DirectoryInfo rootDirectory = new DirectoryInfo(rootPath);
                if (rootDirectory.Exists == false)
                {
                    UnityEngine.Debug.LogWarning($"Cannot found StreamingAssets root directory : {rootPath}");
                    return;
                }

                DirectoryInfo[] subDirectories = rootDirectory.GetDirectories();
                foreach (var subDirectory in subDirectories)
                {
                    string packageName = subDirectory.Name;
                    string pacakgeDirectory = subDirectory.FullName;
                    bool result = DefaultBuildinFileSystemBuild.CreateBuildinCatalogFile(packageName, pacakgeDirectory);
                    if (result == false)
                    {
                        throw new System.Exception($"Create package {packageName} catalog file failed ! See the detail error in console !");
                    }
                }
            }
        }
#endif
    }
}