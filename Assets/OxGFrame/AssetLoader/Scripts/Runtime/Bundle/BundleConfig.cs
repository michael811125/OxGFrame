using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using OxGFrame.AssetLoader.Utility;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YooAsset;

namespace OxGFrame.AssetLoader.Bundle
{
    public static class BundleConfig
    {
        public enum PlayMode
        {
            EditorSimulateMode,
            OfflineMode,
            HostMode
        }

        public class CryptogramType
        {
            public const string NONE = "NONE";
            public const string OFFSET = "OFFSET";
            public const string XOR = "XOR";
            public const string HTXOR = "HTXOR";
            public const string AES = "AES";
        }

        #region 執行配置
        /// <summary>
        /// Patch 執行模式
        /// </summary>
        public static PlayMode playMode = PlayMode.EditorSimulateMode;

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
        /// 解密 Key, [NONE], [OFFSET, dummySize], [XOR, key], [HTXOR, hKey, tKey], [AES, key, iv] => ex: "None" or "offset, 12" or "xor, 23" or "htxor, 34, 45" or "aes, key, iv"
        /// </summary>
        private static string _cryptogram;
        public static string[] cryptogramArgs
        {
            get
            {
                if (string.IsNullOrEmpty(_cryptogram)) return new string[] { CryptogramType.NONE };
                else
                {
                    string[] args = _cryptogram.Trim().Split(',');
                    for (int i = 0; i < args.Length; i++)
                    {
                        args[i] = args[i].Trim();
                    }
                    return args;
                }
            }
        }

        public static void InitCryptogram(string cryptogram)
        {
            _cryptogram = cryptogram;
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
        public const string bundleUrlFileName = "burlconfig";

        // Bundle 平台路徑
        public const string rootDir = "/CDN"; // root dir
        #endregion

        /// <summary>
        /// 取得 burlconfig 佈署配置檔的數據
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static async UniTask<string> GetValueFromUrlCfg(string key)
        {
            string pathName = Path.Combine(GetRequestStreamingAssetsPath(), bundleUrlFileName);
            var content = await BundleUtility.FileRequestString(pathName);
            var allWords = content.Split('\n');
            var lines = new List<string>(allWords);
            var fileMap = new Dictionary<string, string>();
            foreach (var readLine in lines)
            {
                if (readLine.IndexOf('#') != -1 && readLine[0] == '#') continue;
                var args = readLine.Split(' ');
                if (args.Length >= 2)
                {
                    if (!fileMap.ContainsKey(args[0])) fileMap.Add(args[0], args[1].Replace("\n", "").Replace("\r", ""));
                }
            }

            fileMap.TryGetValue(key, out string value);
            return value;
        }

        /// <summary>
        /// 從 StreamingAssets 中取得 AppConfig
        /// </summary>
        /// <returns></returns>
        public static async UniTask<AppConfig> GetAppConfigFromStreamingAssets()
        {
            string cfgJson = await BundleUtility.FileRequestString(GetStreamingAssetsAppConfigPath());
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
            string refineAppVersion = $@"/v{appVersion.Split('.')[0]}.{appVersion.Split('.')[1]}";

            return $"{host}{rootDir}/{productName}/{platform}/{refineAppVersion}/{packageName}";
        }

        /// <summary>
        /// 取得 StreamingAssets 中的 Bundle URL (Fallback)
        /// </summary>
        /// <returns></returns>
        public static async UniTask<string> GetFallbackHostServerUrl(string packageName)
        {
            var appConfig = await GetAppConfigFromStreamingAssets();
            string host = await GetValueFromUrlCfg(BUNDLE_FALLBACK_IP);
            string productName = appConfig.PRODUCT_NAME;
            string platform = appConfig.PLATFORM;
            string appVersion = appConfig.APP_VERSION;
            string refineAppVersion = $@"/v{appVersion.Split('.')[0]}.{appVersion.Split('.')[1]}";

            return $"{host}{rootDir}/{productName}/{platform}/{refineAppVersion}/{packageName}";
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
        /// 取得本地持久化路徑 (下載的儲存路徑)
        /// </summary>
        /// <returns></returns>
        public static string GetLocalSandboxPath()
        {
            return YooAssets.GetSandboxRoot();
        }

        /// <summary>
        /// 取得本地持久化路徑中的 AppConfig 路徑 (下載的儲存路徑)
        /// </summary>
        /// <returns></returns>
        public static string GetLocalSandboxAppConfigPath()
        {
            return Path.Combine(YooAssets.GetSandboxRoot(), $"{appCfgName}{appCfgExtension}");
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

            return Path.Combine($"{host}{rootDir}/{productName}/{platform}", $"{appCfgName}{appCfgExtension}");
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

            return Path.Combine($"{host}{rootDir}/{productName}/{platform}", $"{patchCfgName}{patchCfgExtension}");
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
