﻿using Cysharp.Threading.Tasks;
using MyBox;
using Newtonsoft.Json;
using OxGFrame.AssetLoader.Utility.SecureMemory;
using OxGKit.Utilities.Request;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OxGFrame.AssetLoader.Bundle
{
    public static class BundleConfig
    {
        [Serializable]
        public class SemanticRule
        {
            [ReadOnly]
            public bool major = true;
            [ReadOnly]
            public bool minor = true;
            [SerializeField]
            private bool _patch = false;
            public bool patch { get { return this._patch; } }
        }

        public enum PlayMode
        {
            EditorSimulateMode,
            OfflineMode,
            HostMode,
            WebGLMode
        }

        public enum BuildMode
        {
            BuiltinBuildPipeline = 1,
            ScriptableBuildPipeline = 0,
            RawFileBuildPipeline = 2
        }

        public enum BuiltinQueryMode
        {
            WebRequest,
            BuiltinFileManifest,
            BuiltinFileManifestWithCRC
        }

        public class CryptogramType
        {
            public const string NONE = "NONE";
            public const string OFFSET = "OFFSET";
            public const string XOR = "XOR";
            public const string HT2XOR = "HT2XOR";
            public const string AES = "AES";
        }

        #region 執行配置
        /// <summary>
        /// Patch 執行模式
        /// </summary>
        public static PlayMode playMode = PlayMode.EditorSimulateMode;

        /// <summary>
        /// Semantic 規則設定
        /// </summary>
        public static SemanticRule semanticRule = new SemanticRule();

        /// <summary>
        /// 跳過 Patch 創建主要下載器階段 (強制邊玩邊下載) 
        /// </summary>
        public static bool skipMainDownload = false;

        /// <summary>
        /// 是否檢查磁碟空間
        /// </summary>
        public static bool checkDiskSpace = true;

        /// <summary>
        /// App Preset Package 清單
        /// </summary>
        public static List<AppPackageInfoWithBuild> listAppPackages;

        /// <summary>
        /// DLC Preset Package 清單
        /// </summary>
        public static List<DlcPackageInfoWithBuild> listDlcPackages;

        /// <summary>
        /// 預設同時併發下載數量
        /// </summary>
        internal const int DEFAULT_MAX_CONCURRENCY_MAX_DOWNLOAD_COUNT = 10;

        /// <summary>
        /// 同時併發下載數量
        /// </summary>
        public static int maxConcurrencyDownloadCount = DEFAULT_MAX_CONCURRENCY_MAX_DOWNLOAD_COUNT;

        /// <summary>
        /// 預設下載失敗重新嘗試次數
        /// </summary>
        internal const int DEFAULT_FAILED_RETRY_COUNT = 3;

        /// <summary>
        /// 下載失敗重新嘗試次數
        /// </summary>
        public static int failedRetryCount = DEFAULT_FAILED_RETRY_COUNT;

        /// <summary>
        /// 斷點續傳門檻
        /// </summary>
        public static uint breakpointFileSizeThreshold = 20 * 1 << 20;

        /// <summary>
        /// 查找內置資源方式
        /// </summary>
        public static BuiltinQueryMode builtinQueryMode = BuiltinQueryMode.WebRequest;

        /// <summary>
        /// 解密 Key, [NONE], [OFFSET, dummySize], [XOR, key], [HT2XOR, hKey, tKey, jKey], [AES, key, iv]
        /// </summary>
        private static SecureString[] _decryptArgs = null;
        internal static SecureString[] decryptArgs => _decryptArgs;

        /// <summary>
        /// Init decryption args
        /// </summary>
        /// <param name="args"></param>
        /// <param name="secured"></param>
        /// <param name="saltSize"></param>
        /// <param name="dummySize"></param>
        internal static void InitDecryptInfo(string args, bool secured, int saltSize, int dummySize)
        {
            if (_decryptArgs == null)
            {
                // Check args first, if is none don't need to secure memory
                bool isNone = args.Substring(0, CryptogramType.NONE.Length).ToUpper().Equals(CryptogramType.NONE);
                secured = !isNone && secured;

                // Parsing decrypt keys
                string[] decryptKeys = args.Trim().Split(',');
                _decryptArgs = new SecureString[decryptKeys.Length];
                for (int i = 0; i < decryptKeys.Length; i++)
                {
                    decryptKeys[i] = decryptKeys[i].Trim();
                    _decryptArgs[i] = new SecureString(decryptKeys[i], secured, saltSize, dummySize);
                    decryptKeys[i] = null;
                }
            }
        }

        /// <summary>
        /// Release secure string
        /// </summary>
        internal static void ReleaseSecureString()
        {
            if (_decryptArgs != null)
                foreach (var decryptArg in _decryptArgs)
                    decryptArg.Dispose();
        }
        #endregion

        /// <summary>
        /// 緩存 url config
        /// </summary>
        private static Dictionary<string, string> _urlCfgFileMap = null;

        /// <summary>
        /// 緩存 app config
        /// </summary>
        private static AppConfig _appConfig = null;

        /// <summary>
        /// 取得 burlconfig 佈署配置檔的數據
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static async UniTask<string> GetValueFromUrlCfg(string key)
        {
            if (_urlCfgFileMap == null)
            {
                string bundleUrlFileName = $"{PatchSetting.setting.bundleUrlCfgName}{PatchSetting.BUNDLE_URL_CFG_EXTENSION}";
                string pathName = Path.Combine(GetRequestStreamingAssetsPath(), bundleUrlFileName);
                var content = await Requester.RequestText(pathName, null, null, null, false);
                if (string.IsNullOrEmpty(content)) return string.Empty;
                var allWords = content.Split('\n');
                var lines = new List<string>(allWords);

                _urlCfgFileMap = new Dictionary<string, string>();
                foreach (var readLine in lines)
                {
                    if (readLine.IndexOf('#') != -1 && readLine[0] == '#') continue;
                    var args = readLine.Split(' ', 2);
                    if (args.Length >= 2)
                    {
                        if (!_urlCfgFileMap.ContainsKey(args[0])) _urlCfgFileMap.Add(args[0], args[1].Replace("\n", "").Replace("\r", ""));
                    }
                }
            }

            _urlCfgFileMap.TryGetValue(key, out string value);
            return value;
        }

        /// <summary>
        /// 從 StreamingAssets 中取得 AppConfig
        /// </summary>
        /// <returns></returns>
        public static async UniTask<AppConfig> GetAppConfigFromStreamingAssets()
        {
            if (_appConfig == null)
            {
                string cfgJson = await Requester.RequestText(GetStreamingAssetsAppConfigPath(), null, null, null, false);
                if (!string.IsNullOrEmpty(cfgJson)) _appConfig = JsonConvert.DeserializeObject<AppConfig>(cfgJson);
            }
            return _appConfig;
        }

        /// <summary>
        /// 取得 StreamingAssets 中的 Bundle URL
        /// </summary>
        /// <returns></returns>
        public static async UniTask<string> GetHostServerUrl(string packageName)
        {
            var appConfig = await GetAppConfigFromStreamingAssets();
            string host = await GetValueFromUrlCfg(PatchSetting.BUNDLE_IP);
            string productName = appConfig.PRODUCT_NAME;
            string platform = appConfig.PLATFORM;
            string appVersion = appConfig.APP_VERSION;
            string refineAppVersion = $@"v{appVersion.Split('.')[0]}.{appVersion.Split('.')[1]}";
            string rootFolderName = PatchSetting.setting.rootFolderName;

            // 預設組合路徑
            return $"{host}/{rootFolderName}/{productName}/{platform}/{refineAppVersion}/{packageName}";
        }

        /// <summary>
        /// 取得 StreamingAssets 中的 Bundle Fallback URL
        /// </summary>
        /// <returns></returns>
        public static async UniTask<string> GetFallbackHostServerUrl(string packageName)
        {
            var appConfig = await GetAppConfigFromStreamingAssets();
            string host = await GetValueFromUrlCfg(PatchSetting.BUNDLE_FALLBACK_IP);
            string productName = appConfig.PRODUCT_NAME;
            string platform = appConfig.PLATFORM;
            string appVersion = appConfig.APP_VERSION;
            string refineAppVersion = $@"v{appVersion.Split('.')[0]}.{appVersion.Split('.')[1]}";
            string rootFolderName = PatchSetting.setting.rootFolderName;

            // 預設組合路徑
            return $"{host}/{rootFolderName}/{productName}/{platform}/{refineAppVersion}/{packageName}";
        }

        /// <summary>
        /// 取得 StreamingAssets 中的 Bundle URL (DLC)
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="dlcVersion"></param>
        /// <returns></returns>
        public static async UniTask<string> GetDlcHostServerUrl(string packageName, string dlcVersion, bool withoutPlatform = false)
        {
            var appConfig = await GetAppConfigFromStreamingAssets();
            string host = await GetValueFromUrlCfg(PatchSetting.BUNDLE_IP);
            string productName = appConfig.PRODUCT_NAME;
            string platform = appConfig.PLATFORM;
            string rootFolderName = PatchSetting.setting.rootFolderName;
            string dlcFolderName = PatchSetting.setting.dlcFolderName;

            // 預設 DLC 組合路徑
            if (withoutPlatform) return $"{host}/{rootFolderName}/{productName}/{dlcFolderName}/{packageName}/{dlcVersion}";
            else return $"{host}/{rootFolderName}/{productName}/{platform}/{dlcFolderName}/{packageName}/{dlcVersion}";
        }

        /// <summary>
        /// 取得 StreamingAssets 中的 Bundle Fallback URL (DLC)
        /// </summary>
        /// <returns></returns>
        public static async UniTask<string> GetDlcFallbackHostServerUrl(string packageName, string dlcVersion, bool withoutPlatform = false)
        {
            var appConfig = await GetAppConfigFromStreamingAssets();
            string host = await GetValueFromUrlCfg(PatchSetting.BUNDLE_FALLBACK_IP);
            string productName = appConfig.PRODUCT_NAME;
            string platform = appConfig.PLATFORM;
            string rootFolderName = PatchSetting.setting.rootFolderName;
            string dlcFolderName = PatchSetting.setting.dlcFolderName;

            // 預設 DLC 組合路徑
            if (withoutPlatform) return $"{host}/{rootFolderName}/{productName}/{dlcFolderName}/{packageName}/{dlcVersion}";
            else return $"{host}/{rootFolderName}/{productName}/{platform}/{dlcFolderName}/{packageName}/{dlcVersion}";
        }

        /// <summary>
        /// 取得主程式商店連結
        /// </summary>
        /// <returns></returns>
        public static async UniTask<string> GetAppStoreLink()
        {
            return await GetValueFromUrlCfg(PatchSetting.STORE_LINK);
        }

        /// <summary>
        /// 前往主程式商店
        /// </summary>
        public static async UniTaskVoid GoToAppStore()
        {
            string storeLink = await GetAppStoreLink();
            Application.OpenURL(storeLink);
        }

        /// <summary>
        /// 取得本地持久化根目錄
        /// </summary>
        /// <returns></returns>
        public static string GetLocalSandboxRootPath()
        {
            var yooDefaultFolderName = PatchSetting.yooSetting.DefaultYooFolderName;
#if UNITY_EDITOR
            string projectPath = Path.GetDirectoryName(Application.dataPath);
            projectPath = projectPath.Replace('\\', '/').Replace("\\", "/");
            return Path.Combine(projectPath, yooDefaultFolderName);
#elif UNITY_STANDALONE
            return Path.Combine(Application.dataPath, yooDefaultFolderName);
#else
            return Path.Combine(Application.persistentDataPath, yooDefaultFolderName);
#endif
        }

        /// <summary>
        /// 依照 Package 取得本地持久化路徑 (下載的儲存路徑)
        /// </summary>
        /// <returns></returns>
        public static string GetLocalSandboxPackagePath(string packageName)
        {
            var package = PackageManager.GetPackage(packageName);
            string rootPath = package?.GetPackageSandboxRootDirectory();
            return Path.Combine(rootPath, packageName);
        }

        /// <summary>
        /// 依照 Package 取得內置路徑
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static string GetBuiltinPackagePath(string packageName)
        {
            var package = PackageManager.GetPackage(packageName);
            string rootPath = package?.GetPackageBuildinRootDirectory();
            return Path.Combine(rootPath, packageName);
        }

        /// <summary>
        /// 取得本地持久化路徑中的 AppConfig 路徑 (下載的儲存路徑)
        /// </summary>
        /// <returns></returns>
        public static string GetLocalSandboxAppConfigPath()
        {
            string appCfgFileName = $"{PatchSetting.setting.appCfgName}{PatchSetting.APP_CFG_EXTENSION}";
            return Path.Combine(GetLocalSandboxRootPath(), appCfgFileName);
        }

        /// <summary>
        /// 取得 StreamingAssets 中的 AppConfig 路徑
        /// </summary>
        /// <returns></returns>
        public static string GetStreamingAssetsAppConfigPath()
        {
            string appCfgFileName = $"{PatchSetting.setting.appCfgName}{PatchSetting.APP_CFG_EXTENSION}";
            return Path.Combine(GetRequestStreamingAssetsPath(), appCfgFileName);
        }

        /// <summary>
        /// 取得資源伺服器的 AppConfig (URL)
        /// </summary>
        /// <returns></returns>
        public static async UniTask<string> GetHostServerAppConfigPath()
        {
            var appConfig = await GetAppConfigFromStreamingAssets();
            string host = await GetValueFromUrlCfg(PatchSetting.BUNDLE_IP);
            string productName = appConfig.PRODUCT_NAME;
            string platform = appConfig.PLATFORM;
            string rootFolderName = PatchSetting.setting.rootFolderName;
            string appCfgFileName = $"{PatchSetting.setting.appCfgName}{PatchSetting.APP_CFG_EXTENSION}";

            return Path.Combine($"{host}/{rootFolderName}/{productName}/{platform}", appCfgFileName);
        }

        /// <summary>
        /// 取得資源伺服器的 PatchConfig (URL)
        /// </summary>
        /// <returns></returns>
        public static async UniTask<string> GetHostServerPatchConfigPath()
        {
            var appConfig = await GetAppConfigFromStreamingAssets();
            string host = await GetValueFromUrlCfg(PatchSetting.BUNDLE_IP);
            string productName = appConfig.PRODUCT_NAME;
            string platform = appConfig.PLATFORM;
            string rootFolderName = PatchSetting.setting.rootFolderName;
            string patchCfgFileName = $"{PatchSetting.setting.patchCfgName}{PatchSetting.PATCH_CFG_EXTENSION}";

            return Path.Combine($"{host}/{rootFolderName}/{productName}/{platform}", patchCfgFileName);
        }

        /// <summary>
        /// 取得 UnityWebRequest StreamingAssets 路徑 (OSX and iOS 需要 + file://)
        /// </summary>
        /// <returns></returns>
        public static string GetRequestStreamingAssetsPath()
        {
#if UNITY_STANDALONE_OSX || UNITY_IOS
            return $"file://{Application.streamingAssetsPath}";
#else
            return Application.streamingAssetsPath;
#endif
        }
    }
}
