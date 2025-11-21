using System.IO;
using UnityEngine;

namespace OxGFrame.Hotfixer
{
    public class HotfixSettings : ScriptableObject
    {
        /// <summary>
        /// 配置文件頭標
        /// </summary>
        public const short CIPHER_HEADER = 0x584F;

        /// <summary>
        /// 配置文件金鑰
        /// </summary>
        public byte cipher = 0x6D;

        /// <summary>
        /// 配置文件名稱
        /// </summary>
        public string hotfixDllCfgName = "hotfixdllconfig";

        /// <summary>
        /// 配置文件擴展名
        /// </summary>
        public string hotfixDllCfgExtension = ".conf";

        private static HotfixSettings _settings = null;
        public static HotfixSettings settings
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
            _settings = Resources.Load<HotfixSettings>(nameof(HotfixSettings));
            if (_settings == null)
            {
                Debug.Log("[OxGFrame.Hotfixer] use default settings.");
                _settings = ScriptableObject.CreateInstance<HotfixSettings>();
            }
            else
            {
                Debug.Log("[OxGFrame.Hotfixer] use user settings.");
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/Create/OxGFrame/Create Settings/Create Hotfix Settings in Resources", priority = 1000)]
        private static void _CreateSettingsData()
        {
            string selectedPath = _GetSelectedPathOrFallback();

            string folderPath = Path.Combine(selectedPath, "Resources");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                UnityEditor.AssetDatabase.Refresh();
            }

            string assetPath = Path.Combine(folderPath, $"{nameof(HotfixSettings)}.asset");

            // 檢查是否已經存在
            HotfixSettings existingAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<HotfixSettings>(assetPath);
            if (existingAsset != null)
            {
                UnityEditor.EditorUtility.DisplayDialog("Already Exists", $"{nameof(HotfixSettings)}.asset already exists.", "OK");
                UnityEditor.Selection.activeObject = existingAsset;
                return;
            }

            // 建立新的 Settings
            var asset = ScriptableObject.CreateInstance<HotfixSettings>();
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
