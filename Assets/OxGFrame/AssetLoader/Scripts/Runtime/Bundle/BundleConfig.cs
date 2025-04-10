using Cysharp.Threading.Tasks;
using MyBox;
using Newtonsoft.Json;
using OxGFrame.AssetLoader.Utility.SecureMemory;
using OxGKit.LoggingSystem;
using OxGKit.SaverSystem;
using OxGKit.Utilities.Requester;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace OxGFrame.AssetLoader.Bundle
{
    public static class BundleConfig
    {
        [Serializable]
        public class SemanticRule
        {
            [ReadOnly]
            [SerializeField]
            private bool _major = true;
            public bool major => this._major;

            [ReadOnly]
            [SerializeField]
            private bool _minor = true;
            public bool minor => this._minor;

            [SerializeField]
            private bool _patch = false;
            public bool patch => this._patch;
        }

        public enum PlayMode
        {
            EditorSimulateMode,
            OfflineMode,
            HostMode,
            WeakHostMode,
            WebGLMode,
            WebGLRemoteMode
        }

        public enum BuildMode
        {
            BuiltinBuildPipeline = 1,
            ScriptableBuildPipeline = 0,
            RawFileBuildPipeline = 2
        }

        public class CryptogramType
        {
            public const string NONE = "NONE";
            public const string OFFSET = "OFFSET";
            public const string XOR = "XOR";
            public const string HT2XOR = "HT2XOR";
            public const string HT2XORPLUS = "HT2XORPLUS";
            public const string AES = "AES";
            public const string CHACHA20 = "CHACHA20";
            public const string XXTEA = "XXTEA";
            public const string OFFSETXOR = "OFFSETXOR";
        }

        #region 執行配置
        /// <summary>
        /// 數據儲存器
        /// </summary>
        internal static Saver saver = new PlayerPrefsSaver();

        /// <summary>
        /// 運行群包預設 Tag
        /// </summary>
        internal const string DEFAULT_GROUP_TAG = "#all";

        /// <summary>
        /// 上次運行群包持久數據儲存鍵值
        /// </summary>
        internal const string LAST_GROUP_INFO_KEY = "LAST_GROUP_INFO_KEY";

        /// <summary>
        /// 上次主程式版本持久數據儲存建鍵值 (弱聯網模式)
        /// </summary>
        internal const string LAST_APP_VERSION_KEY = "LAST_APP_VERSION_KEY";

        /// <summary>
        /// 上次資源配置持久數據儲存建鍵值 (弱聯網模式)
        /// </summary>
        internal const string LAST_PATCH_CONFIG_KEY = "LAST_PATCH_CONFIG_KEY";

        /// <summary>
        /// 上次資源版本持久數據儲存鍵值 (弱聯網模式)
        /// </summary>
        internal const string LAST_PACKAGE_VERSIONS_KEY = "LAST_PACKAGE_VERSIONS_KEY";

        /// <summary>
        /// 配置檔標檔頭
        /// </summary>
        public const short CIPHER_HEADER = 0x584F;

        /// <summary>
        /// 配置檔金鑰
        /// </summary>
        public const byte CIPHER = 0x4D;

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
        public static bool checkDiskSpace = false;

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
        /// 解密 Key
        /// <para> [NONE] </para>
        /// <para> [OFFSET, dummySize] </para>
        /// <para> [XOR, key] </para>
        /// <para> [HT2XOR, hKey, tKey, jKey] </para>
        /// <para> [HT2XORPlus, hKey, tKey, j1Key, j2key] </para>
        /// <para> [AES, key, iv] </para>
        /// <para> [ChaCha20, key, nonce, counter] </para>
        /// <para> [XXTEA, key] </para>
        /// <para> [OFFSETXOR, key, dummySize] </para>
        /// </summary>
        private static SecuredString[] _decryptArgs = null;
        internal static SecuredString[] decryptArgs => _decryptArgs;

        /// <summary>
        /// Init decryption args
        /// </summary>
        /// <param name="args"></param>
        /// <param name="securedType"></param>
        /// <param name="saltSize"></param>
        /// <param name="dummySize"></param>
        internal static void InitDecryptInfo(string args, SecuredStringType securedType, int saltSize, int dummySize)
        {
            if (_decryptArgs == null)
            {
                // Check args first, if is none don't need to secure memory
                bool isNone = args.Substring(0, CryptogramType.NONE.Length).ToUpper().Equals(CryptogramType.NONE);
                if (isNone)
                    securedType = SecuredStringType.None;

                // Parsing decrypt keys
                string[] decryptKeys = args.Trim().Split(',');
                _decryptArgs = new SecuredString[decryptKeys.Length];
                for (int i = 0; i < decryptKeys.Length; i++)
                {
                    decryptKeys[i] = decryptKeys[i].Trim();
                    _decryptArgs[i] = new SecuredString(decryptKeys[i], securedType, saltSize, dummySize);
                    decryptKeys[i] = null;
                }
            }
        }

        /// <summary>
        /// Release secure string
        /// </summary>
        internal static void ReleaseSecuredString()
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
        /// 緩存 builtin app config
        /// </summary>
        private static AppConfig _builtinAppConfig = null;

        /// <summary>
        /// 緩存 host app config
        /// </summary>
        private static AppConfig _hostAppConfig = null;

        #region Header Helper
        public static void WriteInt16(short value, byte[] buffer, ref int pos)
        {
            WriteUInt16((ushort)value, buffer, ref pos);
        }

        internal static void WriteUInt16(ushort value, byte[] buffer, ref int pos)
        {
            buffer[pos++] = (byte)value;
            buffer[pos++] = (byte)(value >> 8);
        }

        public static short ReadInt16(byte[] buffer, ref int pos)
        {
            if (BitConverter.IsLittleEndian)
            {
                short value = (short)((buffer[pos]) | (buffer[pos + 1] << 8));
                pos += 2;
                return value;
            }
            else
            {
                short value = (short)((buffer[pos] << 8) | (buffer[pos + 1]));
                pos += 2;
                return value;
            }
        }

        internal static ushort ReadUInt16(byte[] buffer, ref int pos)
        {
            return (ushort)ReadInt16(buffer, ref pos);
        }
        #endregion

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
                var data = await Requester.RequestBytes(pathName);
                if (data.Length == 0)
                    return string.Empty;

                #region Check data type
                string content;
                int pos = 0;

                // Read header (non-encrypted)
                var header = ReadInt16(data, ref pos);
                if (header == CIPHER_HEADER)
                {
                    // Read data without header
                    byte[] dataWithoutHeader = new byte[data.Length - 2];
                    Buffer.BlockCopy(data, pos, dataWithoutHeader, 0, data.Length - pos);
                    // Decrypt
                    for (int i = 0; i < dataWithoutHeader.Length; i++)
                    {
                        dataWithoutHeader[i] ^= CIPHER << 1;
                    }
                    // To string
                    content = Encoding.UTF8.GetString(dataWithoutHeader);
                    Logging.Print<Logger>($"<color=#4eff9e>[Source is Cipher] Check -> burlconfig.conf:</color>\n{content}");
                }
                else
                {
                    content = Encoding.UTF8.GetString(data);
                    Logging.Print<Logger>($"<color=#4eff9e>[Source is Plaintext] Check -> burlconfig.conf:</color>\n{content}");
                }
                #endregion

                // Parsing
                _urlCfgFileMap = Saver.ParsingDataMap(content);
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
            if (_builtinAppConfig == null)
            {
                string cfgJson = await Requester.RequestText(GetStreamingAssetsAppConfigPath(), null, null, null, false);
                if (!string.IsNullOrEmpty(cfgJson))
                    _builtinAppConfig = JsonConvert.DeserializeObject<AppConfig>(cfgJson);
            }
            return _builtinAppConfig;
        }

        /// <summary>
        /// 從 Host Server 中取得 AppConfig
        /// </summary>
        /// <returns></returns>
        public static async UniTask<AppConfig> GetAppConfigFromHostServer()
        {
            if (_hostAppConfig == null)
            {
                var url = await GetHostServerAppConfigPath();
                string cfgJson = await Requester.RequestText(url, null, null, null, false);
                if (!string.IsNullOrEmpty(cfgJson))
                    _hostAppConfig = JsonConvert.DeserializeObject<AppConfig>(cfgJson);
                else
                {
                    // 弱聯網處理
                    if (playMode == PlayMode.WeakHostMode)
                    {
                        cfgJson = saver.GetString(LAST_APP_VERSION_KEY, string.Empty);
                        if (!string.IsNullOrEmpty(cfgJson))
                            _hostAppConfig = JsonConvert.DeserializeObject<AppConfig>(cfgJson);
                    }
                }
            }
            return _hostAppConfig;
        }

        /// <summary>
        /// 取得 StreamingAssets 中的 Bundle URL
        /// </summary>
        /// <returns></returns>
        public static async UniTask<string> GetHostServerUrl(string packageName)
        {
            // 獲取 Host Server 最新的 AppConfig 版本, 主要是為了獲取最新 AppVersion 路徑
            var appConfig = await GetAppConfigFromHostServer();
            string host = await GetValueFromUrlCfg(PatchSetting.BUNDLE_IP);
            string productName = appConfig.PRODUCT_NAME;
            string platform = appConfig.PLATFORM;
            string appVersion = appConfig.APP_VERSION;
            string refineAppVersion = appConfig.SEMANTIC_RULE.PATCH ? $@"v{appVersion.Split('.')[0]}.{appVersion.Split('.')[1]}.{appVersion.Split('.')[2]}" : $@"v{appVersion.Split('.')[0]}.{appVersion.Split('.')[1]}";
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
            // 獲取 Host Server 最新的 AppConfig 版本, 主要是為了獲取最新 AppVersion 路徑
            var appConfig = await GetAppConfigFromHostServer();
            string host = await GetValueFromUrlCfg(PatchSetting.BUNDLE_FALLBACK_IP);
            string productName = appConfig.PRODUCT_NAME;
            string platform = appConfig.PLATFORM;
            string appVersion = appConfig.APP_VERSION;
            string refineAppVersion = appConfig.SEMANTIC_RULE.PATCH ? $@"v{appVersion.Split('.')[0]}.{appVersion.Split('.')[1]}.{appVersion.Split('.')[2]}" : $@"v{appVersion.Split('.')[0]}.{appVersion.Split('.')[1]}";
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
            var appConfig = await GetAppConfigFromHostServer();
            string host = await GetValueFromUrlCfg(PatchSetting.BUNDLE_IP);
            string productName = appConfig.PRODUCT_NAME;
            string platform = appConfig.PLATFORM;
            string rootFolderName = PatchSetting.setting.rootFolderName;
            string dlcFolderName = PatchSetting.setting.dlcFolderName;

            // 預設 DLC 組合路徑
            if (withoutPlatform)
                return $"{host}/{rootFolderName}/{productName}/{dlcFolderName}/{packageName}/{dlcVersion}";
            else
                return $"{host}/{rootFolderName}/{productName}/{platform}/{dlcFolderName}/{packageName}/{dlcVersion}";
        }

        /// <summary>
        /// 取得 StreamingAssets 中的 Bundle Fallback URL (DLC)
        /// </summary>
        /// <returns></returns>
        public static async UniTask<string> GetDlcFallbackHostServerUrl(string packageName, string dlcVersion, bool withoutPlatform = false)
        {
            var appConfig = await GetAppConfigFromHostServer();
            string host = await GetValueFromUrlCfg(PatchSetting.BUNDLE_FALLBACK_IP);
            string productName = appConfig.PRODUCT_NAME;
            string platform = appConfig.PLATFORM;
            string rootFolderName = PatchSetting.setting.rootFolderName;
            string dlcFolderName = PatchSetting.setting.dlcFolderName;

            // 預設 DLC 組合路徑
            if (withoutPlatform)
                return $"{host}/{rootFolderName}/{productName}/{dlcFolderName}/{packageName}/{dlcVersion}";
            else
                return $"{host}/{rootFolderName}/{productName}/{platform}/{dlcFolderName}/{packageName}/{dlcVersion}";
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
            return YooAssetBridge.YooAssetSettingsData.GetYooDefaultCacheRoot();
        }

        /// <summary>
        /// 依照 Package 取得本地持久化路徑 (下載的儲存路徑)
        /// </summary>
        /// <returns></returns>
        public static string GetLocalSandboxPackagePath(string packageName)
        {
            string rootPath = GetLocalSandboxRootPath();
            return Path.Combine(rootPath, packageName);
        }

        /// <summary>
        /// 取得內置資源根目錄
        /// </summary>
        /// <returns></returns>
        public static string GetBuiltinRootPath()
        {
            return YooAssetBridge.YooAssetSettingsData.GetYooDefaultBuildinRoot();
        }

        /// <summary>
        /// 依照 Package 取得內置路徑
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static string GetBuiltinPackagePath(string packageName)
        {
            string rootPath = GetBuiltinRootPath();
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
            // 從 StreamingAssets 中先獲取 AppConfig, 是為了獲取對應的平台與產品名稱路徑
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
            // 從 StreamingAssets 中先獲取 appConfig, 是為了獲取對應的平台與產品名稱路徑
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
