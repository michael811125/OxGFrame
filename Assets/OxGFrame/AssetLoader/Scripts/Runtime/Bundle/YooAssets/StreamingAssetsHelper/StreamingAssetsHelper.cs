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
                    Logging.PrintInfo<Logger>($"【Try Query Builtin-Package】Search succeeded (Package exists). Package: {packageName}");
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
#if YOOASSET_2
            var yooDefaultFolderName = YooAssetSettingsData.GetDefaultYooFolderName();
#else
            // [YooAsset v3] YooAssetSettingsData 更名為 YooAssetConfiguration
            var yooDefaultFolderName = YooAssetConfiguration.GetYooFolderName();
#endif
            string dirPath = Path.Combine(Application.streamingAssetsPath, yooDefaultFolderName, packageName);

            // 直接檢查文件夾是否存在
            bool exists = Directory.Exists(dirPath);
            if (exists)
            {
                Logging.PrintInfo<Logger>($"【Try Query Builtin-Package】Package exists. Package: {packageName}");
                return true;
            }

            Logging.PrintWarning<Logger>($"【Try Query Builtin-Package】Package doesn't exist. Package: {packageName}");
            return false;
        }
        #endregion

        #region Try Request Builtin-Package
        /// <summary>
        /// 嘗試請求內置資源清單是否存在 (true = 存在，false = 不存在)
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
            // 使用 using 統一釋放 CancellationTokenSource 與 UnityWebRequest，避免任何路徑發生洩漏
            using (var cts = new CancellationTokenSource())
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                cts.CancelAfterSlim(TimeSpan.FromSeconds(10));
                request.downloadHandler = new DownloadHandlerBuffer();

                try
                {
                    await request.SendWebRequest().WithCancellation(cts.Token);

                    if (request.result == UnityWebRequest.Result.DataProcessingError ||
                        request.result == UnityWebRequest.Result.ProtocolError ||
                        request.result == UnityWebRequest.Result.ConnectionError)
                    {
                        Logging.PrintWarning<Logger>($"【Try Query Builtin-Package】Package doesn't exist (Built-in package not found). URL: {url}");
                        return false;
                    }

                    if (request.downloadedBytes > 0 && request.responseCode < 400)
                    {
                        Logging.PrintInfo<Logger>($"【Try Query Builtin-Package】Package exists (Built-in package found). Code: {request.responseCode}, PartialBytes: {request.downloadedBytes}, URL: {url}");
                        return true;
                    }
                }
                catch (OperationCanceledException ex) when (ex.CancellationToken == cts.Token)
                {
                    Logging.PrintError<Logger>($"【Try Query Builtin-Package】Request timed out. URL: {url}");
                    return false;
                }
                catch (Exception ex)
                {
                    Logging.PrintError<Logger>($"【Try Query Builtin-Package】Request failed (Package doesn't exist) The package may not exist. URL: {url}, Exception: {ex}");
                    return false;
                }

                return false;
            }
        }
        #endregion
    }
}