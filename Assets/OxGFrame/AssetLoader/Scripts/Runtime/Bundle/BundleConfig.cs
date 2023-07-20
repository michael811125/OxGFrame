using Cysharp.Threading.Tasks;
using MyBox;
using Newtonsoft.Json;
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
        public static bool skipCreateMainDownloder = false;

        /// <summary>
        /// Package 設置清單
        /// </summary>
        public static List<string> listPackage;

        /// <summary>
        /// 預設同時併發下載數量
        /// </summary>
        public const int defaultMaxConcurrencyDownloadCount = 10;
        /// <summary>
        /// 同時併發下載數量
        /// </summary>
        public static int maxConcurrencyDownloadCount = defaultMaxConcurrencyDownloadCount;

        /// <summary>
        /// 預設下載失敗重新嘗試次數
        /// </summary>
        public const int defaultFailedRetryCount = 3;
        /// <summary>
        /// 下載失敗重新嘗試次數
        /// </summary>
        public static int failedRetryCount = defaultFailedRetryCount;

        /// <summary>
        /// 解密 Key, [NONE], [OFFSET, dummySize], [XOR, key], [HT2XOR, hKey, tKey, jKey], [AES, key, iv]
        /// </summary>
        private static string[] _cryptogramArgs = null;
        public static string[] cryptogramArgs => _cryptogramArgs;

        /// <summary>
        /// Init Decryption args
        /// </summary>
        /// <param name="args"></param>
        public static void InitCryptogram(string args)
        {
            _cryptogramArgs = args.Trim().Split(',');
            for (int i = 0; i < _cryptogramArgs.Length; i++)
            {
                _cryptogramArgs[i] = _cryptogramArgs[i].Trim();
            }
        }
        #endregion

        #region 常數配置
        // AppConfig 配置檔
        public const string appCfgBakExtension = ".bak";            // 主程式配置檔副檔名 (Backup)
        public const string appCfgExtension = ".json";              // 主程式配置檔副檔名
        public readonly static string appCfgName = "appconfig";     // 主程式配置檔的名稱 

        // PatchConfig 配置檔
        public const string patchCfgBakExtension = ".bak";          // 補丁配置檔副檔名 (Backup)
        public const string patchCfgExtension = ".json";            // 補丁配置檔副檔名
        public readonly static string patchCfgName = "patchconfig"; // 補丁配置檔的名稱 

        // 佈署配置檔中的 KEY
        public const string BUNDLE_IP = "bundle_ip";
        public const string BUNDLE_FALLBACK_IP = "bundle_fallback_ip";
        public const string STORE_LINK = "store_link";

        // 佈署配置檔
        public const string bundleUrlFileName = "burlconfig.conf";

        // Bundle 輸出歸類名稱
        public const string rootFolderName = "CDN"; // Root 資料夾名稱
        public const string dlcFolderName = "DLC";  // DLC 資料夾名稱

        // YooAsset 歸類名稱
        public const string yooDefaultFolderName = "yoo";
        public const string yooCacheBundleFolderName = "CacheBundleFiles";
        public const string yooCacheRawFolderName = "CacheRawFiles";
        public const string yooBundleFileName = "__data";
        #endregion

        private static Dictionary<string, string> _urlCfgFileMap = null;

        /// <summary>
        /// 取得 burlconfig 佈署配置檔的數據
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static async UniTask<string> GetValueFromUrlCfg(string key)
        {
            if (_urlCfgFileMap == null)
            {
                string pathName = Path.Combine(GetRequestStreamingAssetsPath(), bundleUrlFileName);
                var content = await Requester.RequestText(pathName, null, null, null, false);
                if (string.IsNullOrEmpty(content)) return string.Empty;
                var allWords = content.Split('\n');
                var lines = new List<string>(allWords);

                _urlCfgFileMap = new Dictionary<string, string>();
                foreach (var readLine in lines)
                {
                    if (readLine.IndexOf('#') != -1 && readLine[0] == '#') continue;
                    var args = readLine.Split(' ');
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
            string cfgJson = await Requester.RequestText(GetStreamingAssetsAppConfigPath(), null, null, null, false);
            if (!string.IsNullOrEmpty(cfgJson)) return JsonConvert.DeserializeObject<AppConfig>(cfgJson);

            return null;
        }

        /// <summary>
        /// 取得 StreamingAssets 中的 Bundle URL
        /// </summary>
        /// <returns></returns>
        public static async UniTask<string> GetHostServerUrl(string packageName)
        {
            var appConfig = await GetAppConfigFromStreamingAssets();
            string host = await GetValueFromUrlCfg(BUNDLE_IP);
            string productName = appConfig.PRODUCT_NAME;
            string platform = appConfig.PLATFORM;
            string appVersion = appConfig.APP_VERSION;
            string refineAppVersion = $@"v{appVersion.Split('.')[0]}.{appVersion.Split('.')[1]}";

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
            string host = await GetValueFromUrlCfg(BUNDLE_FALLBACK_IP);
            string productName = appConfig.PRODUCT_NAME;
            string platform = appConfig.PLATFORM;
            string appVersion = appConfig.APP_VERSION;
            string refineAppVersion = $@"v{appVersion.Split('.')[0]}.{appVersion.Split('.')[1]}";

            // 預設組合路徑
            return $"{host}/{rootFolderName}/{productName}/{platform}/{refineAppVersion}/{packageName}";
        }

        /// <summary>
        /// 取得 StreamingAssets 中的 Bundle URL (DLC)
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="dlcVersion"></param>
        /// <returns></returns>
        public static async UniTask<string> GetDlcHostServerUrl(string packageName, string dlcVersion)
        {
            var appConfig = await GetAppConfigFromStreamingAssets();
            string host = await GetValueFromUrlCfg(BUNDLE_IP);
            string productName = appConfig.PRODUCT_NAME;
            string platform = appConfig.PLATFORM;

            // 預設 DLC 組合路徑
            return $"{host}/{rootFolderName}/{productName}/{platform}/{dlcFolderName}/{packageName}/{dlcVersion}";
        }

        /// <summary>
        /// 取得 StreamingAssets 中的 Bundle Fallback URL (DLC)
        /// </summary>
        /// <returns></returns>
        public static async UniTask<string> GetDlcFallbackHostServerUrl(string packageName, string dlcVersion)
        {
            var appConfig = await GetAppConfigFromStreamingAssets();
            string host = await GetValueFromUrlCfg(BUNDLE_FALLBACK_IP);
            string productName = appConfig.PRODUCT_NAME;
            string platform = appConfig.PLATFORM;

            // 預設 DLC 組合路徑
            return $"{host}/{rootFolderName}/{productName}/{platform}/{dlcFolderName}/{packageName}/{dlcVersion}";
        }

        /// <summary>
        /// 取得主程式商店連結
        /// </summary>
        /// <returns></returns>
        public static async UniTask<string> GetAppStoreLink()
        {
            return await GetValueFromUrlCfg(STORE_LINK);
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
            return Path.Combine(GetLocalSandboxRootPath(), $"{appCfgName}{appCfgExtension}");
        }

        /// <summary>
        /// 取得 StreamingAssets 中的 AppConfig 路徑
        /// </summary>
        /// <returns></returns>
        public static string GetStreamingAssetsAppConfigPath()
        {
            return Path.Combine(GetRequestStreamingAssetsPath(), $"{appCfgName}{appCfgExtension}");
        }

        /// <summary>
        /// 取得資源伺服器的 AppConfig (URL)
        /// </summary>
        /// <returns></returns>
        public static async UniTask<string> GetHostServerAppConfigPath()
        {
            var appConfig = await GetAppConfigFromStreamingAssets();
            string host = await GetValueFromUrlCfg(BUNDLE_IP);
            string productName = appConfig.PRODUCT_NAME;
            string platform = appConfig.PLATFORM;

            return Path.Combine($"{host}/{rootFolderName}/{productName}/{platform}", $"{appCfgName}{appCfgExtension}");
        }

        /// <summary>
        /// 取得資源伺服器的 PatchConfig (URL)
        /// </summary>
        /// <returns></returns>
        public static async UniTask<string> GetHostServerPatchConfigPath()
        {
            var appConfig = await GetAppConfigFromStreamingAssets();
            string host = await GetValueFromUrlCfg(BUNDLE_IP);
            string productName = appConfig.PRODUCT_NAME;
            string platform = appConfig.PLATFORM;

            return Path.Combine($"{host}/{rootFolderName}/{productName}/{platform}", $"{patchCfgName}{patchCfgExtension}");
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
