using Cysharp.Threading.Tasks;
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
        /// <summary>
        /// 資源運行模式
        /// </summary>
        public enum PlayMode
        {
            EditorSimulateMode,
            OfflineMode,
            HostMode,
            WeakHostMode,
            WebGLMode,
            WebGLRemoteMode,
            CustomMode
        }

        /// <summary>
        /// 資源運行管線模式
        /// </summary>
        public enum BuildMode
        {
            BuiltinBuildPipeline = 1,
            ScriptableBuildPipeline = 0,
            RawFileBuildPipeline = 2
        }

        /// <summary>
        /// 加解密類型
        /// </summary>
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

        #region Runtime Setup
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
        /// 運行模式
        /// </summary>
        public static PlayMode playMode = PlayMode.EditorSimulateMode;

        /// <summary>
        /// 運行模式參數配置
        /// </summary>
        public static PlayModeParameters playModeParameters = null;

        /// <summary>
        /// App Preset Package 清單 (預設包裹)
        /// </summary>
        public static List<AppPackageInfoWithBuild> listAppPackages;

        /// <summary>
        /// DLC Preset Package 清單 (預設包裹)
        /// </summary>
        public static List<DlcPackageInfoWithBuild> listDlcPackages;

        #region Download Options
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
        /// 預設下載器看門狗監控超時時間
        /// </summary>
        public static int DEFAULT_DOWNLOAD_WATCHDOG_TIMEOUT = 30;

        /// <summary>
        /// 下載器看門狗監控超時時間 (監控時間範圍內, 沒接受到任何下載數據, 直接終止任務)
        /// </summary>
        public static int downloadWatchdogTimeout = DEFAULT_DOWNLOAD_WATCHDOG_TIMEOUT;
        #endregion

        #region Load Options
        /// <summary>
        /// 資源讀取緩衝大小 (AssetBundle.LoadFromStream)
        /// </summary>
        public static uint bundleLoadReadBufferSize = 32 * 1 << 10;

        /// <summary>
        /// 資源解密讀取緩衝大小
        /// </summary>
        public static uint bundleDecryptReadBufferSize = 32 * 1 << 10;
        #endregion

        #region Process Options
        /// <summary>
        /// 每帧執行消耗的最大時間切片 (毫秒)
        /// </summary>
        public static long operationSystemMaxTimeSlice = 30;
        #endregion

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
        private static SecuredString[] _bundleDecryptArgs = null;
        internal static SecuredString[] bundleDecryptArgs => _bundleDecryptArgs;

        private static SecuredString[] _manifestDecryptArgs = null;
        internal static SecuredString[] manifestDecryptArgs => _manifestDecryptArgs;

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

        /// <summary>
        /// Init bundle decryption args
        /// </summary>
        /// <param name="args"></param>
        /// <param name="securedType"></param>
        /// <param name="saltSize"></param>
        /// <param name="dummySize"></param>
        internal static void InitBundleDecryptInfo(string args, SecuredStringType securedType, int saltSize, int dummySize)
        {
            _InitDecryptInfo(ref _bundleDecryptArgs, args, securedType, saltSize, dummySize);
        }

        /// <summary>
        /// Init manifest decryption args
        /// </summary>
        /// <param name="args"></param>
        /// <param name="securedType"></param>
        /// <param name="saltSize"></param>
        /// <param name="dummySize"></param>
        internal static void InitManifestDecryptInfo(string args, SecuredStringType securedType, int saltSize, int dummySize)
        {
            _InitDecryptInfo(ref _manifestDecryptArgs, args, securedType, saltSize, dummySize);
        }

        private static void _InitDecryptInfo(ref SecuredString[] decryptArgs, string args, SecuredStringType securedType, int saltSize, int dummySize)
        {
            if (decryptArgs == null)
            {
                // Check args first, if is none don't need to secure memory
                bool isNone = args.Substring(0, CryptogramType.NONE.Length).ToUpper().Equals(CryptogramType.NONE);
                if (isNone)
                    securedType = SecuredStringType.None;

                // Parsing decrypt keys
                string[] decryptKeys = args.Trim().Split(',');
                decryptArgs = new SecuredString[decryptKeys.Length];
                for (int i = 0; i < decryptKeys.Length; i++)
                {
                    decryptKeys[i] = decryptKeys[i].Trim();
                    decryptArgs[i] = new SecuredString(decryptKeys[i], securedType, saltSize, dummySize);
                    decryptKeys[i] = null;
                }
            }
        }

        private static void _ReleaseSecuredString(ref SecuredString[] decryptArgs)
        {
            if (decryptArgs != null)
            {
                foreach (var decryptArg in decryptArgs)
                    decryptArg.Dispose();
                decryptArgs = null;
            }
        }
        #endregion

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

        #region Endpoint & Path Operations
        /// <summary>
        /// 取得 burlconfig 佈署配置文件的數據
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static async UniTask<string> GetValueFromUrlCfg(string key)
        {
            if (_urlCfgFileMap == null)
            {
                string bundleUrlFileName = $"{PatchSettings.settings.bundleUrlCfgName}{PatchSettings.BUNDLE_URL_CFG_EXTENSION}";
                string pathName = Path.Combine(GetRequestStreamingAssetsPath(), bundleUrlFileName);
                var data = await Requester.RequestBytes(pathName);
                if (data.Length == 0)
                    return string.Empty;

                #region Check data type
                string content;
                int pos = 0;

                // Read header (non-encrypted)
                var header = ReadInt16(data, ref pos);
                if (header == PatchSettings.CIPHER_HEADER)
                {
                    // Read data without header
                    byte[] dataWithoutHeader = new byte[data.Length - 2];
                    Buffer.BlockCopy(data, pos, dataWithoutHeader, 0, data.Length - pos);
                    // Decrypt
                    for (int i = 0; i < dataWithoutHeader.Length; i++)
                    {
                        dataWithoutHeader[i] ^= (byte)(PatchSettings.settings.bundleUrlCfgCipher << 1);
                    }
                    // To string
                    content = Encoding.UTF8.GetString(dataWithoutHeader);
                    Logging.Print<Logger>($"[Source is Cipher] Check -> burlconfig.conf");
                }
                else
                {
                    content = Encoding.UTF8.GetString(data);
                    Logging.Print<Logger>($"[Source is Plaintext] Check -> burlconfig.conf");
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
                    if (playModeParameters.enableLastLocalVersionsCheckInWeakNetwork)
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
        /// 獲取 APP 包裹位於 Host 上的端點
        /// </summary>
        /// <returns></returns>
        public static async UniTask<string> GetHostServerUrl(string packageName)
        {
            // 獲取 Host Server 最新的 AppConfig 版本, 主要是為了獲取最新 AppVersion 路徑
            var appConfig = await GetAppConfigFromHostServer();
            string host = await GetValueFromUrlCfg(PatchSettings.BUNDLE_IP);
            string productName = appConfig.PRODUCT_NAME;
            string platform = appConfig.PLATFORM;
            string appVersion = appConfig.APP_VERSION;
            bool ruleIncludesPatchVersion = appConfig.SEMANTIC_RULE != null ? appConfig.SEMANTIC_RULE.PATCH : false;
            string refineAppVersion = ruleIncludesPatchVersion ? $@"v{appVersion.Split('.')[0]}.{appVersion.Split('.')[1]}.{appVersion.Split('.')[2]}" : $@"v{appVersion.Split('.')[0]}.{appVersion.Split('.')[1]}";
            string rootFolderName = PatchSettings.settings.rootFolderName;

            // 預設組合路徑
            return $"{host}/{rootFolderName}/{productName}/{platform}/{refineAppVersion}/{packageName}";
        }

        /// <summary>
        /// 獲取 APP 包裹位於 Host 上的端點 (Fallback)
        /// </summary>
        /// <returns></returns>
        public static async UniTask<string> GetFallbackHostServerUrl(string packageName)
        {
            // 獲取 Host Server 最新的 AppConfig 版本, 主要是為了獲取最新 AppVersion 路徑
            var appConfig = await GetAppConfigFromHostServer();
            string host = await GetValueFromUrlCfg(PatchSettings.BUNDLE_FALLBACK_IP);
            string productName = appConfig.PRODUCT_NAME;
            string platform = appConfig.PLATFORM;
            string appVersion = appConfig.APP_VERSION;
            bool ruleIncludesPatchVersion = appConfig.SEMANTIC_RULE != null ? appConfig.SEMANTIC_RULE.PATCH : false;
            string refineAppVersion = ruleIncludesPatchVersion ? $@"v{appVersion.Split('.')[0]}.{appVersion.Split('.')[1]}.{appVersion.Split('.')[2]}" : $@"v{appVersion.Split('.')[0]}.{appVersion.Split('.')[1]}";
            string rootFolderName = PatchSettings.settings.rootFolderName;

            // 預設組合路徑
            return $"{host}/{rootFolderName}/{productName}/{platform}/{refineAppVersion}/{packageName}";
        }

        /// <summary>
        /// 獲取 DLC 包裹位於 Host 上的端點
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="dlcVersion"></param>
        /// <returns></returns>
        public static async UniTask<string> GetDlcHostServerUrl(string packageName, string dlcVersion, bool withoutPlatform = false)
        {
            var appConfig = await GetAppConfigFromHostServer();
            string host = await GetValueFromUrlCfg(PatchSettings.BUNDLE_IP);
            string productName = appConfig.PRODUCT_NAME;
            string platform = appConfig.PLATFORM;
            string rootFolderName = PatchSettings.settings.rootFolderName;
            string dlcFolderName = PatchSettings.settings.dlcFolderName;

            // 預設 DLC 組合路徑
            if (withoutPlatform)
                return $"{host}/{rootFolderName}/{productName}/{dlcFolderName}/{packageName}/{dlcVersion}";
            else
                return $"{host}/{rootFolderName}/{productName}/{platform}/{dlcFolderName}/{packageName}/{dlcVersion}";
        }

        /// <summary>
        /// 獲取 DLC 包裹位於 Host 上的端點 (Fallback)
        /// </summary>
        /// <returns></returns>
        public static async UniTask<string> GetDlcFallbackHostServerUrl(string packageName, string dlcVersion, bool withoutPlatform = false)
        {
            var appConfig = await GetAppConfigFromHostServer();
            string host = await GetValueFromUrlCfg(PatchSettings.BUNDLE_FALLBACK_IP);
            string productName = appConfig.PRODUCT_NAME;
            string platform = appConfig.PLATFORM;
            string rootFolderName = PatchSettings.settings.rootFolderName;
            string dlcFolderName = PatchSettings.settings.dlcFolderName;

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
            return await GetValueFromUrlCfg(PatchSettings.STORE_LINK);
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
            string appCfgFileName = $"{PatchSettings.settings.appCfgName}{PatchSettings.APP_CFG_EXTENSION}";
            return Path.Combine(GetLocalSandboxRootPath(), appCfgFileName);
        }

        /// <summary>
        /// 取得 StreamingAssets 中的 AppConfig 路徑
        /// </summary>
        /// <returns></returns>
        public static string GetStreamingAssetsAppConfigPath()
        {
            string appCfgFileName = $"{PatchSettings.settings.appCfgName}{PatchSettings.APP_CFG_EXTENSION}";
            return Path.Combine(GetRequestStreamingAssetsPath(), appCfgFileName);
        }

        /// <summary>
        /// 取得資源服務器的 AppConfig (URL)
        /// </summary>
        /// <returns></returns>
        public static async UniTask<string> GetHostServerAppConfigPath()
        {
            // 從 StreamingAssets 中先獲取 AppConfig, 是為了獲取對應的平台與產品名稱路徑
            var appConfig = await GetAppConfigFromStreamingAssets();
            string host = await GetValueFromUrlCfg(PatchSettings.BUNDLE_IP);
            string productName = appConfig.PRODUCT_NAME;
            string platform = appConfig.PLATFORM;
            string rootFolderName = PatchSettings.settings.rootFolderName;
            string appCfgFileName = $"{PatchSettings.settings.appCfgName}{PatchSettings.APP_CFG_EXTENSION}";

            return Path.Combine($"{host}/{rootFolderName}/{productName}/{platform}", appCfgFileName);
        }

        /// <summary>
        /// 取得資源服務器的 PatchConfig (URL)
        /// </summary>
        /// <returns></returns>
        public static async UniTask<string> GetHostServerPatchConfigPath()
        {
            // 從 StreamingAssets 中先獲取 appConfig, 是為了獲取對應的平台與產品名稱路徑
            var appConfig = await GetAppConfigFromStreamingAssets();
            string host = await GetValueFromUrlCfg(PatchSettings.BUNDLE_IP);
            string productName = appConfig.PRODUCT_NAME;
            string platform = appConfig.PLATFORM;
            string rootFolderName = PatchSettings.settings.rootFolderName;
            string patchCfgFileName = $"{PatchSettings.settings.patchCfgName}{PatchSettings.PATCH_CFG_EXTENSION}";

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
        #endregion

        /// <summary>
        /// Release
        /// </summary>
        internal static void Release()
        {
            saver.Dispose();
            saver = null;

            playModeParameters = null;

            listAppPackages = null;
            listDlcPackages = null;

            _urlCfgFileMap = null;
            _builtinAppConfig = null;
            _hostAppConfig = null;

            _ReleaseSecuredString(ref _bundleDecryptArgs);
            _ReleaseSecuredString(ref _manifestDecryptArgs);
        }
    }
}