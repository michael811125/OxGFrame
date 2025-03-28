using Newtonsoft.Json;
using OxGFrame.AssetLoader.Bundle;
using OxGFrame.AssetLoader.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using YooAsset.Editor;
using static OxGFrame.AssetLoader.Bundle.AppConfig;

namespace OxGFrame.AssetLoader.Editor
{
    public static class BundleHelper
    {
        internal const string MENU_ROOT = "OxGFrame/AssetLoader/";

        #region Public Methods
        #region Exporter
        /// <summary>
        /// 輸出 App 配置檔至輸出路徑 (Export AppConfig to StreamingAssets [for Built-in])
        /// </summary>
        /// <param name="productName"></param>
        /// <param name="semanticRule"></param>
        /// <param name="appVersion"></param>
        /// <param name="outputPath"></param>
        /// <param name="activeBuildTarget"></param>
        /// <param name="buildTarget"></param>
        public static void ExportAppConfig(string productName, SemanticRule semanticRule, string appVersion, string outputPath, bool activeBuildTarget, BuildTarget buildTarget)
        {
            if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

            // 生成配置檔數據
            var cfg = GenerateAppConfig(productName, semanticRule, appVersion, activeBuildTarget, buildTarget);

            // 配置檔序列化, 將進行寫入
            string jsonCfg = JsonConvert.SerializeObject(cfg, Formatting.Indented);

            // 寫入配置文件
            string appCfgFileName = $"{PatchSetting.setting.appCfgName}{PatchSetting.APP_CFG_EXTENSION}";
            string writePath = Path.Combine(outputPath, appCfgFileName);
            WriteTxt(jsonCfg, writePath);

            Debug.Log($"<color=#00FF00>【Export {appCfgFileName} Completes】App Version: {cfg.APP_VERSION}</color>");
        }

        /// <summary>
        /// 輸出 Configs 跟 App Bundles (Export Configs and App Bundles for CDN Server)
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="outputPath"></param>
        /// <param name="productName"></param>
        /// <param name="semanticRule"></param>
        /// <param name="appVersion"></param>
        /// <param name="exportPackages"></param>
        /// <param name="groupInfos"></param>
        /// <param name="packageInfos"></param>
        /// <param name="activeBuildTarget"></param>
        /// <param name="buildTarget"></param>
        /// <param name="isClearOutputPath"></param>
        public static void ExportConfigsAndAppBundles(string inputPath, string outputPath, string productName, SemanticRule semanticRule, string appVersion, string[] exportPackages, List<GroupInfo> groupInfos, string[] packageInfos, bool activeBuildTarget, BuildTarget buildTarget, bool isClearOutputPath = true)
        {
            // 生成配置檔數據 (AppConfig)
            var appCfg = GenerateAppConfig(productName, semanticRule, appVersion, activeBuildTarget, buildTarget);
            var patchCfg = GeneratePatchConfig(groupInfos, packageInfos, inputPath);

            // 清空輸出路徑
            if (isClearOutputPath && Directory.Exists(outputPath)) BundleUtility.DeleteFolder(outputPath);
            if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

            #region YooAsset Bundle
            ExportNewestYooAssetBundles(inputPath, outputPath, appCfg.PLATFORM, productName, semanticRule, appVersion, exportPackages);
            #endregion

            #region AppConfig Write
            // 配置檔序列化, 將進行寫入
            string jsonCfg = JsonConvert.SerializeObject(appCfg, Formatting.Indented);

            // 寫入配置文件
            string appCfgFileName = $"{PatchSetting.setting.appCfgName}{PatchSetting.APP_CFG_EXTENSION}";
            string writePath = Path.Combine(outputPath + $@"/{productName}" + $@"/{appCfg.PLATFORM}", appCfgFileName);
            WriteTxt(jsonCfg, writePath);

            // 寫入配置文件 (BAK)
            string appCfgBakFileName = $"{PatchSetting.setting.appCfgName}{PatchSetting.APP_CFG_BAK_EXTENSION}";
            writePath = Path.Combine(outputPath + $@"/{productName}" + $@"/{appCfg.PLATFORM}" + (semanticRule.PATCH ? $@"/v{appVersion.Split('.')[0]}.{appVersion.Split('.')[1]}.{appVersion.Split('.')[2]}" : $@"/v{appVersion.Split('.')[0]}.{appVersion.Split('.')[1]}"), appCfgBakFileName);
            WriteTxt(jsonCfg, writePath);
            #endregion

            #region PatchConfig Write
            // 配置檔序列化, 將進行寫入
            jsonCfg = JsonConvert.SerializeObject(patchCfg, Formatting.Indented);

            // 寫入配置文件
            string patchCfgFileName = $"{PatchSetting.setting.patchCfgName}{PatchSetting.PATCH_CFG_EXTENSION}";
            writePath = Path.Combine(outputPath + $@"/{productName}" + $@"/{appCfg.PLATFORM}", patchCfgFileName);
            WriteTxt(jsonCfg, writePath);

            // 寫入配置文件 (BAK)
            string patchCfgBakFileName = $"{PatchSetting.setting.patchCfgName}{PatchSetting.PATCH_CFG_BAK_EXTENSION}";
            writePath = Path.Combine(outputPath + $@"/{productName}" + $@"/{appCfg.PLATFORM}" + (semanticRule.PATCH ? $@"/v{appVersion.Split('.')[0]}.{appVersion.Split('.')[1]}.{appVersion.Split('.')[2]}" : $@"/v{appVersion.Split('.')[0]}.{appVersion.Split('.')[1]}"), patchCfgBakFileName);
            WriteTxt(jsonCfg, writePath);
            #endregion

            Debug.Log($"<color=#00FF00>【Export Configs And App Bundles Completes】 App Version: {appCfg.APP_VERSION}</color>");
        }

        /// <summary>
        /// 輸出 App Bundles (Export App Bundles Without Configs for CDN Server)
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="outputPath"></param>
        /// <param name="productName"></param>
        /// <param name="semanticRule"></param>
        /// <param name="appVersion"></param>
        /// <param name="exportPackages"></param>
        /// <param name="activeBuildTarget"></param>
        /// <param name="buildTarget"></param>
        /// <param name="isClearOutputPath"></param>
        public static void ExportAppBundles(string inputPath, string outputPath, string productName, SemanticRule semanticRule, string appVersion, string[] exportPackages, bool activeBuildTarget, BuildTarget buildTarget, bool isClearOutputPath = true)
        {
            // 生成配置檔數據 (AppConfig)
            var appCfg = GenerateAppConfig(productName, semanticRule, appVersion, activeBuildTarget, buildTarget);

            // 清空輸出路徑
            if (isClearOutputPath && Directory.Exists(outputPath)) BundleUtility.DeleteFolder(outputPath);
            if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

            #region YooAsset Bundle
            ExportNewestYooAssetBundles(inputPath, outputPath, appCfg.PLATFORM, productName, semanticRule, appVersion, exportPackages);
            #endregion

            Debug.Log($"<color=#00FF00>【Export App Bundles Without Configs Completes】</color>");
        }

        /// <summary>
        /// 輸出 DLC Bundles (Export Individual DLC Bundles for CDN Server)
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="outputPath"></param>
        /// <param name="productName"></param>
        /// <param name="dlcInfos"></param>
        /// <param name="activeBuildTarget"></param>
        /// <param name="buildTarget"></param>
        /// <param name="isClearOutputPath"></param>
        public static void ExportIndividualDlcBundles(string inputPath, string outputPath, string productName, List<DlcInfo> dlcInfos, bool activeBuildTarget, BuildTarget buildTarget, bool isClearOutputPath = true)
        {
            // 平台
            string platform = activeBuildTarget ? $"{EditorUserBuildSettings.activeBuildTarget}" : $"{buildTarget}";

            // 清空輸出路徑
            if (isClearOutputPath && Directory.Exists(outputPath)) BundleUtility.DeleteFolder(outputPath);
            if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

            #region YooAsset Bundle
            ExportNewestYooAssetDlcBundles(inputPath, outputPath, platform, productName, dlcInfos);
            #endregion

            Debug.Log($"<color=#00FF00>【Export DLC Bundles Completes】</color>");
        }

        /// <summary>
        /// 產生 Bundle URL 配置檔至輸出路徑 (Export BundleUrlConfig to StreamingAssets [for Built-in])
        /// </summary>
        /// <param name="bundleIp"></param>
        /// <param name="bundleFallbackIp"></param>
        /// <param name="storeLink"></param>
        /// <param name="outputPath"></param>
        /// <param name="cipher"></param>
        public static void ExportBundleUrlConfig(string bundleIp, string bundleFallbackIp, string storeLink, string outputPath, bool cipher)
        {
            if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

            if (string.IsNullOrEmpty(bundleIp)) bundleIp = "http://127.0.0.1";
            if (string.IsNullOrEmpty(bundleFallbackIp)) bundleFallbackIp = "http://127.0.0.1";
            if (string.IsNullOrEmpty(storeLink)) storeLink = "http://";

            IEnumerable<string> texts = new string[]
            {
                @$"# {PatchSetting.BUNDLE_IP} = First CDN Server IP or Domain (Plan A)",
                @$"# {PatchSetting.BUNDLE_FALLBACK_IP} = Second CDN Server IP or Domain (Plan B)",
                @$"# {PatchSetting.STORE_LINK} = GooglePlay Store Link (https://play.google.com/store/apps/details?id=YOUR_ID)",
                @$"# {PatchSetting.STORE_LINK} = Apple Store Link (https://apps.apple.com/app/idYOUR_ID)",
                "",
                $"{PatchSetting.BUNDLE_IP} {bundleIp}",
                $"{PatchSetting.BUNDLE_FALLBACK_IP} {bundleFallbackIp}",
                $"{PatchSetting.STORE_LINK} {storeLink}",
            };

            string bundleUrlFileName = $"{PatchSetting.setting.bundleUrlCfgName}{PatchSetting.BUNDLE_URL_CFG_EXTENSION}";
            string fullOutputPath = Path.Combine(outputPath, bundleUrlFileName);

            string content = string.Empty;
            int idx = 0;
            foreach (string txt in texts)
            {
                if (cipher)
                {
                    // Without useless texts
                    if (idx < texts.ToArray().Length - 3)
                    {
                        idx++;
                        continue;
                    }
                }
                content += txt + "\n";
            }

            byte[] writeBuffer;
            byte[] data = Encoding.UTF8.GetBytes(content);

            if (cipher)
            {
                // Encrypt
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] ^= BundleConfig.CIPHER << 1;
                }

                // Write data with header
                int pos = 0;
                byte[] dataWithHeader = new byte[data.Length + 2];
                // Write header (non-encrypted)
                BundleConfig.WriteInt16(BundleConfig.CIPHER_HEADER, dataWithHeader, ref pos);
                Buffer.BlockCopy(data, 0, dataWithHeader, pos, data.Length);
                writeBuffer = dataWithHeader;
            }
            else
                writeBuffer = data;

            // 寫入配置文件
            File.WriteAllBytes(fullOutputPath, writeBuffer);

            Debug.Log($"<color=#00FF00>【Export Source is Cipher: {cipher}, {bundleUrlFileName} Completes】</color>");
        }
        #endregion

        #region Parser and Converter
        /// <summary>
        /// Parsing group info args (g1,t1#g2,t1,t2...)
        /// </summary>
        /// <param name="groupInfoArgs"></param>
        /// <returns></returns>
        public static List<GroupInfo> ParsingGroupInfosByArgs(string groupInfoArgs)
        {
            if (string.IsNullOrEmpty(groupInfoArgs)) return new List<GroupInfo>();

            // Parsing GroupInfo Arguments
            List<GroupInfo> parsedGroupInfos = new List<GroupInfo>();

            try
            {
                string[] groups = groupInfoArgs.Trim().Split('#');
                foreach (string group in groups)
                {
                    GroupInfo groupInfo = new GroupInfo();

                    string[] args = group.Trim().Split(',');
                    List<string> tags = new List<string>();
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (i == 0) groupInfo.groupName = args[i];
                        else tags.Add(args[i]);
                    }
                    groupInfo.tags = tags.ToArray();

                    parsedGroupInfos.Add(groupInfo);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return new List<GroupInfo>();
            }

            return parsedGroupInfos;
        }

        /// <summary>
        /// Convert group info args to List<GroupInfo>
        /// </summary>
        /// <param name="groupInfos"></param>
        /// <returns></returns>
        public static string ConvertGroupInfosToArgs(List<GroupInfo> groupInfos)
        {
            string args = string.Empty;
            for (int i = 0; i < groupInfos.Count; i++)
            {
                string groupName = groupInfos[i].groupName;
                string tags = string.Empty;
                foreach (var tag in groupInfos[i].tags)
                {
                    tags += $",{tag}";
                }
                args += $"{groupName}{tags}" + ((i == groupInfos.Count - 1) ? string.Empty : "#");
            }

            return args;
        }

        /// <summary>
        /// Parsing dlc info args (dlc1,version#dlc2,version...)
        /// </summary>
        /// <param name="dlcInfoArgs"></param>
        /// <returns></returns>
        public static List<DlcInfo> ParsingDlcInfosByArgs(string dlcInfoArgs)
        {
            if (string.IsNullOrEmpty(dlcInfoArgs)) return new List<DlcInfo>();

            // Parsing DlcInfo Arguments
            List<DlcInfo> parsedDlcInfos = new List<DlcInfo>();

            try
            {
                string[] dlcs = dlcInfoArgs.Trim().Split('#');
                foreach (string dlc in dlcs)
                {
                    DlcInfo dlcInfo = new DlcInfo();

                    string[] args = dlc.Trim().Split(',');
                    dlcInfo.packageName = args[0];
                    dlcInfo.dlcVersion = args[1];

                    parsedDlcInfos.Add(dlcInfo);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return new List<DlcInfo>();
            }

            return parsedDlcInfos;
        }

        /// <summary>
        /// Convert dlc info args to List<DlcInfo>
        /// </summary>
        /// <param name="dlcInfos"></param>
        /// <returns></returns>
        public static string ConvertDlcInfosToArgs(List<DlcInfo> dlcInfos)
        {
            string args = string.Empty;
            for (int i = 0; i < dlcInfos.Count; i++)
            {
                string packageName = dlcInfos[i].packageName;
                string version = dlcInfos[i].dlcVersion;
                args += $"{packageName},{version}" + ((i == dlcInfos.Count - 1) ? string.Empty : "#");
            }

            return args;
        }
        #endregion
        #endregion

        #region Internal Methods
        /// <summary>
        /// 輸出最新的 YooAsset Bundles (找尋輸出時間最大值)
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="outputPath"></param>
        /// <param name="platform"></param>
        /// <param name="productName"></param>
        /// <param name="semanticRule"></param>
        /// <param name="appVersion"></param>
        /// <param name="exportPackages"></param>
        internal static void ExportNewestYooAssetBundles(string inputPath, string outputPath, string platform, string productName, SemanticRule semanticRule, string appVersion, string[] exportPackages)
        {
            #region YooAsset Bundle Process
            // 取得 Bundle 輸出路徑
            if (string.IsNullOrEmpty(inputPath)) inputPath = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();

            // 取得平台路徑
            string inputPathWithPlatform = Path.Combine(inputPath, $"{EditorUserBuildSettings.activeBuildTarget}");

            // 取得平台下的所有 Packages
            string[] packagePaths = Directory.GetDirectories(inputPathWithPlatform);

            // 進行資料夾之間的複製
            foreach (var packagePath in packagePaths)
            {
                bool isSkip = true;
                string packageName = Path.GetFileNameWithoutExtension(packagePath);
                foreach (var exportPackage in exportPackages)
                {
                    if (packageName == exportPackage)
                    {
                        isSkip = false;
                        break;
                    }
                }

                if (isSkip) continue;

                // 取得 NewestPackagePath
                string newestVersionPath = (string)NewestPackagePathFilter(packagePath)[0];

                // If the latest version path cannot be found, skip it
                if (string.IsNullOrEmpty(newestVersionPath)) continue;

                string destFullDir = Path.GetFullPath(outputPath + $@"/{productName}" + $@"/{platform}" + (semanticRule.PATCH ? $@"/v{appVersion.Split('.')[0]}.{appVersion.Split('.')[1]}.{appVersion.Split('.')[2]}" : $@"/v{appVersion.Split('.')[0]}.{appVersion.Split('.')[1]}") + $@"/{packageName}");
                BundleUtility.CopyFolderRecursively(newestVersionPath, destFullDir);

                {
                    string versionName = Path.GetFileNameWithoutExtension(newestVersionPath);
                    Debug.Log($"<color=#00FF00>【Copy Bundles】 Package Name: {packageName}, Package Version: {versionName}</color>");
                }
            }
            #endregion
        }

        /// <summary>
        /// 輸出最新的 YooAsset DLC Bundles (找尋輸出時間最大值)
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="outputPath"></param>
        /// <param name="platform"></param>
        /// <param name="productName"></param>
        /// <param name="dlcVersion"></param>
        internal static void ExportNewestYooAssetDlcBundles(string inputPath, string outputPath, string platform, string productName, List<DlcInfo> dlcInfos)
        {
            #region YooAsset Bundle Process
            // 取得 Bundle 輸出路徑
            if (string.IsNullOrEmpty(inputPath)) inputPath = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();

            // 取得平台路徑
            string inputPathWithPlatform = Path.Combine(inputPath, $"{EditorUserBuildSettings.activeBuildTarget}");

            // 取得平台下的所有 Packages
            string[] packagePaths = Directory.GetDirectories(inputPathWithPlatform);

            // 進行資料夾之間的複製
            foreach (var packagePath in packagePaths)
            {
                bool isSkip = true;
                string packageName = Path.GetFileNameWithoutExtension(packagePath);
                string dlcVersion = null;
                bool withoutPlatform = false;
                foreach (var dlcInfo in dlcInfos)
                {
                    if (packageName == dlcInfo.packageName)
                    {
                        isSkip = false;
                        dlcVersion = dlcInfo.dlcVersion;
                        withoutPlatform = dlcInfo.withoutPlatform;
                        break;
                    }
                }

                if (isSkip) continue;

                object[] newestData = NewestPackagePathFilter(packagePath);
                string newestVersionPath = (string)newestData[0];
                decimal newestVersion = (decimal)newestData[1];

                // If the latest version path cannot be found, skip it
                if (string.IsNullOrEmpty(newestVersionPath)) continue;

                if (string.IsNullOrEmpty(dlcVersion)) dlcVersion = $"{newestVersion}";

                string destFullDir;
                if (withoutPlatform) destFullDir = Path.GetFullPath(outputPath + $@"/{productName}" + $@"/{PatchSetting.setting.dlcFolderName}" + $@"/{packageName}" + $@"/{dlcVersion}");
                else destFullDir = Path.GetFullPath(outputPath + $@"/{productName}" + $@"/{platform}" + $@"/{PatchSetting.setting.dlcFolderName}" + $@"/{packageName}" + $@"/{dlcVersion}");
                BundleUtility.CopyFolderRecursively(newestVersionPath, destFullDir);

                {
                    string versionName = Path.GetFileNameWithoutExtension(newestVersionPath);
                    Debug.Log($"<color=#00FF00>【Copy DLC Bundles】 Package Name: {packageName}, Package Version: {versionName}</color>");
                }
            }
            #endregion
        }

        /// <summary>
        /// 返回來源路徑的配置檔數據
        /// </summary>
        /// <param name="productName"></param>
        /// <param name="inputPath"></param>
        /// <param name="compressed"></param>
        /// <returns></returns>
        internal static AppConfig GenerateAppConfig(string productName, SemanticRule semanticRule, string appVersion, bool activeBuildTarget, BuildTarget buildTarget)
        {
            // 生成配置檔
            var cfg = new AppConfig();

            // 平台
            cfg.PLATFORM = activeBuildTarget ? $"{EditorUserBuildSettings.activeBuildTarget}" : $"{buildTarget}";

            // 產品名稱
            cfg.PRODUCT_NAME = productName;

            // 主程式版本
            cfg.APP_VERSION = string.IsNullOrEmpty(appVersion) ? Application.version : appVersion;

            // 版號規則
            cfg.SEMANTIC_RULE = semanticRule;

            Debug.Log($"<color=#00FF00>【Generate】{PatchSetting.setting.appCfgName}{PatchSetting.APP_CFG_EXTENSION} Completes.</color>");

            return cfg;
        }

        internal static PatchConfig GeneratePatchConfig(List<GroupInfo> groupInfos, string[] exportPackages, string inputPath = null)
        {
            // 生成配置檔
            var cfg = new PatchConfig();

            // 取得 Bundle 路徑
            if (exportPackages != null)
            {
                if (string.IsNullOrEmpty(inputPath)) inputPath = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();

                string inputPathWithPlatform = Path.Combine(inputPath, $"{EditorUserBuildSettings.activeBuildTarget}");

                string[] packagePaths = Directory.GetDirectories(inputPathWithPlatform);

                foreach (var packagePath in packagePaths)
                {
                    bool isSkip = true;
                    string packageName = Path.GetFileNameWithoutExtension(packagePath);
                    foreach (var exportPackage in exportPackages)
                    {
                        if (packageName == exportPackage)
                        {
                            isSkip = false;
                            break;
                        }
                    }

                    if (isSkip) continue;

                    // 取得 NewestPackagePath
                    string newestVersionPath = (string)NewestPackagePathFilter(packagePath)[0];

                    // If the latest version path cannot be found, skip it
                    if (string.IsNullOrEmpty(newestVersionPath)) continue;

                    // 建立 Package info
                    Bundle.PackageInfo packageInfo = new Bundle.PackageInfo();
                    // 生成 Package total size
                    long packageSize = 0;

                    FileInfo[] files = BundleUtility.GetFilesRecursively(newestVersionPath);
                    foreach (var file in files)
                    {
                        // 累加文件大小
                        packageSize += file.Length;
                    }

                    {
                        string versionName = Path.GetFileNameWithoutExtension(newestVersionPath);
                        string refineVersionName = versionName.Replace("-", string.Empty);
                        #region Encode
                        string versionHash = BundleUtility.GetVersionHash("-", versionName, 1 << 5);
                        // Default length is 6
                        string versionNumber1 = BundleUtility.GetVersionNumber(versionHash, 6);
                        // Just show more
                        string versionNumber2 = BundleUtility.GetVersionNumber(versionHash, 32);
                        #endregion

                        packageInfo.packageName = packageName;
                        packageInfo.packageVersion = refineVersionName;
                        packageInfo.packageVersionEncoded = $"{versionNumber1}-{versionNumber2}";
                        packageInfo.packageSize = packageSize;
                    }

                    // 資源包清單
                    cfg.AddPackageInfo(packageInfo);
                }
            }

            cfg.GROUP_INFOS = groupInfos;

            Debug.Log($"<color=#00FF00>【Generate】PatchConfig Completes.</color>");

            return cfg;
        }

        /// <summary>
        /// Return the following parameters
        /// <para> object[0] = (string)key = version path </para>
        /// <para> object[1] = (decimal)value = version date number </para>
        /// </summary>
        /// <param name="packagePath"></param>
        /// <returns></returns>
        internal static object[] NewestPackagePathFilter(string packagePath)
        {
            #region Newest Filter
            string[] versionPaths = Directory.GetDirectories(packagePath);
            string newestVersionPath = null;
            decimal newestVersion = 0;

            foreach (var versionPath in versionPaths)
            {
                string versionName = Path.GetFileNameWithoutExtension(versionPath);

                // 確保符合預期格式, 跳過不符合格式的資料夾
                if (versionName.IndexOf('-') <= -1) continue;

                // 提取日期部分並處理為 "yyyyMMdd" 格式
                string major = versionName.Substring(0, versionName.LastIndexOf("-")).Replace("-", string.Empty);

                // 提取分鐘部分, 並確保其為 4 位數格式
                string minor = versionName.Substring(versionName.LastIndexOf("-") + 1).PadLeft(4, '0');

                // 合併日期與分鐘部分, 並轉換為數字
                string refinedVersionName = major + minor;

                if (decimal.TryParse(refinedVersionName, out decimal value))
                {
                    // 直接比較, 更新最新版本的路徑和版本號
                    if (value > newestVersion)
                    {
                        newestVersion = value;
                        newestVersionPath = versionPath;
                    }
                }
            }
            #endregion

            // 返回最新的版本路徑和版本數值
            return new object[] { newestVersionPath, newestVersion };
        }

        /// <summary>
        /// 寫入文字文件檔
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="outputPath"></param>
        internal static void WriteTxt(string txt, string outputPath)
        {
            // 檢查目錄是否存在, 若不存在則創建它
            string directoryPath = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            // 寫入配置文件
            var file = File.CreateText(outputPath);
            file.Write(txt);
            file.Close();
        }
        #endregion

        #region MenuItems
        [MenuItem(MENU_ROOT + "Local Download Directory (Sandbox)/Open Download Directory", false, 197)]
        internal static void OpenDownloadDir()
        {
            var dir = BundleConfig.GetLocalSandboxRootPath();
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            System.Diagnostics.Process.Start(dir);
        }

        [MenuItem(MENU_ROOT + "Local Download Directory (Sandbox)/Clear Download Directory", false, 198)]
        internal static void ClearDownloadDir()
        {
            bool operate = EditorUtility.DisplayDialog(
                "Clear Download Folder",
                "Are you sure you want to delete download folder?",
                "yes",
                "no");

            if (!operate) return;

            var dir = BundleConfig.GetLocalSandboxRootPath();
            BundleUtility.DeleteFolder(dir);

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        [MenuItem(MENU_ROOT + "Clear Last Group Info Record", false, 199)]
        internal static void ClearLastGroupRecord()
        {
            AssetPatcher.ClearLastGroupInfo();
        }
        #endregion
    }
}