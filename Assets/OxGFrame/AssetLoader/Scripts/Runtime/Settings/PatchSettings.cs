using System.IO;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace OxGFrame.AssetLoader
{
    [MovedFrom("PatchSetting")]
    public class PatchSettings : ScriptableObject
    {
        #region Common
        /// <summary>
        /// 配置文件頭標
        /// </summary>
        public const short CIPHER_HEADER = 0x584F;

        /// <summary>
        /// META 文件擴展名
        /// </summary>
        public const string META_FILE_EXTENSION = ".meta";
        #endregion

        #region AppConfig 配置文件
        /// <summary>
        /// APP 配置文件擴展名 (Backup)
        /// </summary>
        public const string APP_CFG_BAK_EXTENSION = ".bak";

        [Header("App Config Settings")]
        /// <summary>
        ///  APP 配置文件的名稱
        /// </summary>
        public string appCfgName = "appconfig";

        /// <summary>
        /// APP 配置文件擴展名
        /// </summary>
        [Tooltip("The file extension must include the dot (e.g., .conf, .json). Please ensure you enter it with the dot.")]
        public string appCfgExtension = ".json";
        #endregion

        #region PatchConfig 配置文件
        /// <summary>
        /// 補丁配置文件擴展名 (Backup)
        /// </summary>
        public const string PATCH_CFG_BAK_EXTENSION = ".bak";

        [Header("Patch Config Settings")]
        /// <summary>
        /// 補丁配置文件的名稱 
        /// </summary>
        public string patchCfgName = "patchconfig";

        /// <summary>
        /// 補丁配置文件擴展名
        /// </summary>
        [Tooltip("The file extension must include the dot (e.g., .conf, .json). Please ensure you enter it with the dot.")]
        public string patchCfgExtension = ".json";
        #endregion

        #region 佈署配置文件中的 KEY
        /// <summary>
        /// 資源鏈接請求鍵值
        /// </summary>
        public const string BUNDLE_IP = "bundle_ip";

        /// <summary>
        /// 資源備援鏈接請求鍵值
        /// </summary>
        public const string BUNDLE_FALLBACK_IP = "bundle_fallback_ip";

        /// <summary>
        /// 商店鏈接
        /// </summary>
        public const string STORE_LINK = "store_link";
        #endregion

        #region 佈署配置文件
        [Header("Bundle URL Config Settings")]
        /// <summary>
        /// 資源請求端點的配置文件金鑰
        /// </summary>
        public byte bundleUrlCfgCipher = 0x4D;

        /// <summary>
        /// 資源請求端點的配置文件名稱
        /// </summary>
        public string bundleUrlCfgName = "burlconfig";

        /// <summary>
        /// 資源請求端點的配置文件擴展名
        /// </summary>
        [Tooltip("The file extension must include the dot (e.g., .conf, .json). Please ensure you enter it with the dot.")]
        public string bundleUrlCfgExtension = ".conf";
        #endregion

        #region Bundle 輸出歸類名稱
        [Header("Folder Settings")]
        /// <summary>
        /// Root 文件夾名稱
        /// </summary>
        public string rootFolderName = "CDN";

        /// <summary>
        /// DLC 文件夾名稱
        /// </summary>
        public string dlcFolderName = "DLC";
        #endregion

        private static PatchSettings _settings = null;
        public static PatchSettings settings
        {
            get
            {
                if (_settings == null)
                    _LoadSettingsData();
                return _settings;
            }
        }

        /// <summary>
        /// 加載配置文件
        /// </summary>
        private static void _LoadSettingsData()
        {
            _settings = Resources.Load<PatchSettings>(nameof(PatchSettings));
            if (_settings == null)
            {
                Debug.Log("[OxGFrame.AssetLoader] use default settings.");
                _settings = ScriptableObject.CreateInstance<PatchSettings>();
            }
            else
            {
                Debug.Log("[OxGFrame.AssetLoader] use user settings.");
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/Create/OxGFrame/Create Settings/Create Patch Settings in Resources", priority = 1000)]
        private static void _CreateSettingsData()
        {
            string selectedPath = _GetSelectedPathOrFallback();

            string folderPath = Path.Combine(selectedPath, "Resources");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                UnityEditor.AssetDatabase.Refresh();
            }

            string assetPath = Path.Combine(folderPath, $"{nameof(PatchSettings)}.asset");

            // 檢查是否已經存在
            PatchSettings existingAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<PatchSettings>(assetPath);
            if (existingAsset != null)
            {
                UnityEditor.EditorUtility.DisplayDialog("Already Exists", $"{nameof(PatchSettings)}.asset already exists.", "OK");
                UnityEditor.Selection.activeObject = existingAsset;
                return;
            }

            // 建立新的 Settings
            var asset = ScriptableObject.CreateInstance<PatchSettings>();
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
