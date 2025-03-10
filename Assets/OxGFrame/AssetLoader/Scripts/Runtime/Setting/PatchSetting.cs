using OxGKit.LoggingSystem;
using UnityEngine;

namespace OxGFrame.AssetLoader
{
    [CreateAssetMenu(fileName = nameof(PatchSetting), menuName = "OxGFrame/Create Settings/Create Patch Setting")]
    public class PatchSetting : ScriptableObject
    {
        // Common
        public const string META_FILE_EXTENSION = ".meta";

        // AppConfig 配置文件
        public const string APP_CFG_BAK_EXTENSION = ".bak";          // APP 配置文件擴展名 (Backup)
        public const string APP_CFG_EXTENSION = ".json";             // APP 配置文件擴展名
        public string appCfgName = "appconfig";                      // APP 配置文件的名稱 

        // PatchConfig 配置文件
        public const string PATCH_CFG_BAK_EXTENSION = ".bak";        // 補丁配置文件擴展名 (Backup)
        public const string PATCH_CFG_EXTENSION = ".json";           // 補丁配置文件擴展名
        public string patchCfgName = "patchconfig";                  // 補丁配置文件的名稱 

        // BuiltinPackageCatalog 配置文件
        public const string BUILTIN_CATALOG_EXTENSION = ".json";     // 內置 Package 查詢清單文件擴展名
        public string builtinPkgCatalogName = "builtinpkgcatalog";

        // 佈署配置檔中的 KEY
        public const string BUNDLE_IP = "bundle_ip";
        public const string BUNDLE_FALLBACK_IP = "bundle_fallback_ip";
        public const string STORE_LINK = "store_link";

        // 佈署配置檔
        public const string BUNDLE_URL_CFG_EXTENSION = ".conf";       // 主程式配置文件擴展名 (Backup)
        public string bundleUrlCfgName = "burlconfig";

        // Bundle 輸出歸類名稱
        public string rootFolderName = "CDN";                         // Root 文件夾名稱
        public string dlcFolderName = "DLC";                          // DLC 文件夾名稱

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
            _setting = Resources.Load<PatchSetting>(nameof(PatchSetting));
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
    }
}
