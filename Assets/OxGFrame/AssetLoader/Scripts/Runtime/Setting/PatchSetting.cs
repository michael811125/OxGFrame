using OxGKit.LoggingSystem;
using System.IO;
using UnityEngine;

namespace OxGFrame.AssetLoader
{
    public class PatchSetting : ScriptableObject
    {
        // Common
        public const string META_FILE_EXTENSION = ".meta";

        // AppConfig 配置文件
        public const string APP_CFG_BAK_EXTENSION = ".bak";            // APP 配置文件擴展名 (Backup)
        public const string APP_CFG_EXTENSION = ".json";               // APP 配置文件擴展名
        public string appCfgName = "appconfig";                        // APP 配置文件的名稱 

        // PatchConfig 配置文件                                         
        public const string PATCH_CFG_BAK_EXTENSION = ".bak";          // 補丁配置文件擴展名 (Backup)
        public const string PATCH_CFG_EXTENSION = ".json";             // 補丁配置文件擴展名
        public string patchCfgName = "patchconfig";                    // 補丁配置文件的名稱 

        // 佈署配置檔中的 KEY
        public const string BUNDLE_IP = "bundle_ip";                   // 資源鏈接請求鍵值
        public const string BUNDLE_FALLBACK_IP = "bundle_fallback_ip"; // 資源備援鏈接請求鍵值
        public const string STORE_LINK = "store_link";                 // 商店鏈接

        // 佈署配置檔
        public const string BUNDLE_URL_CFG_EXTENSION = ".conf";         // 主程式配置文件擴展名 (Backup)
        public string bundleUrlCfgName = "burlconfig";

        // Bundle 輸出歸類名稱
        public string rootFolderName = "CDN";                           // Root 文件夾名稱
        public string dlcFolderName = "DLC";                            // DLC 文件夾名稱

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
                Logging.PrintInfo<Logger>("[OxGFrame.AssetLoader] use default setting.");
                _setting = ScriptableObject.CreateInstance<PatchSetting>();
            }
            else
            {
                Logging.PrintInfo<Logger>("[OxGFrame.AssetLoader] use user setting.");
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/Create/OxGFrame/Create Settings/Create Patch Setting in Resources", priority = 1000)]
        private static void _CreatePatchSetting()
        {
            string selectedPath = _GetSelectedPathOrFallback();

            string folderPath = Path.Combine(selectedPath, "Resources");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                UnityEditor.AssetDatabase.Refresh();
            }

            string assetPath = Path.Combine(folderPath, $"{nameof(PatchSetting)}.asset");

            // 檢查是否已經存在
            PatchSetting existingAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<PatchSetting>(assetPath);
            if (existingAsset != null)
            {
                UnityEditor.EditorUtility.DisplayDialog("Already Exists", $"{nameof(PatchSetting)}.asset already exists.", "OK");
                UnityEditor.Selection.activeObject = existingAsset;
                return;
            }

            // 建立新的 PatchSetting
            var asset = ScriptableObject.CreateInstance<PatchSetting>();
            UnityEditor.AssetDatabase.CreateAsset(asset, assetPath.Replace("\\", "/"));
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            UnityEditor.EditorUtility.FocusProjectWindow();
            UnityEditor.Selection.activeObject = asset;
        }

        private static string _GetSelectedPathOrFallback()
        {
            string path = "Assets";
            foreach (Object obj in UnityEditor.Selection.GetFiltered(typeof(UnityEditor.DefaultAsset), UnityEditor.SelectionMode.Assets))
            {
                path = UnityEditor.AssetDatabase.GetAssetPath(obj);
                if (File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                }
                break;
            }
            return path;
        }
#endif
    }
}
