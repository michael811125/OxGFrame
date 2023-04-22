using UnityEditor;
using UnityEngine;

public class EditorTool
{
    /// <summary>
    /// 加?相?的配置文件
    /// </summary>
    public static TSetting LoadSettingData<TSetting>() where TSetting : ScriptableObject
    {
        var settingType = typeof(TSetting);
        var guids = AssetDatabase.FindAssets($"t:{settingType.Name}");
        if (guids.Length == 0)
        {
            Debug.LogWarning($"Create new {settingType.Name}.asset");
            var setting = ScriptableObject.CreateInstance<TSetting>();
            string filePath = $"Assets/{settingType.Name}.asset";
            AssetDatabase.CreateAsset(setting, filePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return setting;
        }
        else
        {
            if (guids.Length != 1)
            {
                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    Debug.LogWarning($"Found multiple file : {path}");
                }
                throw new System.Exception($"Found multiple {settingType.Name} files !");
            }

            string filePath = AssetDatabase.GUIDToAssetPath(guids[0]);
            var setting = AssetDatabase.LoadAssetAtPath<TSetting>(filePath);
            return setting;
        }
    }
}
