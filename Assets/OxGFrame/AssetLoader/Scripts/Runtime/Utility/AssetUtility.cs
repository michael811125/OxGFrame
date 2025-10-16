using UnityEngine;

namespace OxGFrame.AssetLoader.Utility
{
    public static class AssetUtility
    {
        /// <summary>
        /// 加載相關配置文件
        /// </summary>
        internal static TSettings LoadSettingsData<TSettings>() where TSettings : ScriptableObject
        {
#if UNITY_EDITOR
            var settingsType = typeof(TSettings);
            var guids = UnityEditor.AssetDatabase.FindAssets($"t:{settingsType.Name}");
            if (guids.Length == 0)
            {
                Debug.LogWarning($"Create new {settingsType.Name}.asset");
                var settings = ScriptableObject.CreateInstance<TSettings>();
                string filePath = $"Assets/{settingsType.Name}.asset";
                UnityEditor.AssetDatabase.CreateAsset(settings, filePath);
                UnityEditor.AssetDatabase.SaveAssets();
                UnityEditor.AssetDatabase.Refresh();
                return settings;
            }
            else
            {
                if (guids.Length != 1)
                {
                    foreach (var guid in guids)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                        Debug.LogWarning($"Found multiple file : {path}");
                    }
                    throw new System.Exception($"Found multiple {settingsType.Name} files !");
                }

                string filePath = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                var settings = UnityEditor.AssetDatabase.LoadAssetAtPath<TSettings>(filePath);
                return settings;
            }
#else
                return default;
#endif
        }
    }
}