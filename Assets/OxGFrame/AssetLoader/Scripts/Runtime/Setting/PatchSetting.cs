using OxGKit.LoggingSystem;
using UnityEngine;
using YooAsset;

namespace OxGFrame.AssetLoader
{
    [CreateAssetMenu(fileName = nameof(PatchSetting), menuName = "OxGFrame/Create Settings/Create Patch Setting")]
    public class PatchSetting : ScriptableObject
    {
        // AppConfig 配置檔
        public const string APP_CFG_BAK_EXTENSION = ".bak";          // 主程式配置檔副檔名 (Backup)
        public const string APP_CFG_EXTENSION = ".json";             // 主程式配置檔副檔名
        public string appCfgName = "appconfig";                      // 主程式配置檔的名稱 

        // PatchConfig 配置檔                                           
        public const string PATCH_CFG_BAK_EXTENSION = ".bak";        // 補丁配置檔副檔名 (Backup)
        public const string PATCH_CFG_EXTENSION = ".json";           // 補丁配置檔副檔名
        public string patchCfgName = "patchconfig";                  // 補丁配置檔的名稱 

        // 佈署配置檔中的 KEY
        public const string BUNDLE_IP = "bundle_ip";
        public const string BUNDLE_FALLBACK_IP = "bundle_fallback_ip";
        public const string STORE_LINK = "store_link";

        // 佈署配置檔
        public const string BUNDLE_URL_CFG_EXTENSION = ".conf";       // 主程式配置檔副檔名 (Backup)
        public string bundleUrlCfgName = "burlconfig";

        // Bundle 輸出歸類名稱
        public string rootFolderName = "CDN";                         // Root 資料夾名稱
        public string dlcFolderName = "DLC";                          // DLC 資料夾名稱

        private static PatchSetting _setting = null;
        public static PatchSetting setting
        {
            get
            {
                if (_setting == null) _LoadSettingData();
                return _setting;
            }
        }

        /// <summary>
        /// 加載配置文件
        /// </summary>
        private static void _LoadSettingData()
        {
            _setting = Resources.Load<PatchSetting>("PatchSetting");
            if (_setting == null)
            {
                Logging.Print<Logger>("<color=#84ffe5>[OxGFrame.AssetLoader] use default setting.</color>");
                _setting = ScriptableObject.CreateInstance<PatchSetting>();
            }
            else
            {
                Logging.Print<Logger>("<color=#84ffe5>[OxGFrame.AssetLoader] use user setting.</color>");
            }
        }

        #region YooAsset Setting
        private static YooAssetSettings _yooSettings = null;
        public static YooAssetSettings yooSettings
        {
            get
            {
                if (_yooSettings == null) _LoadYooSettingsData();
                return _yooSettings;
            }
        }

        /// <summary>
        /// 加载配置文件
        /// </summary>
        private static void _LoadYooSettingsData()
        {
            _yooSettings = Resources.Load<YooAssetSettings>("YooAssetSettings");
            if (_yooSettings == null) _yooSettings = ScriptableObject.CreateInstance<YooAssetSettings>();
        }
        #endregion
    }
}
