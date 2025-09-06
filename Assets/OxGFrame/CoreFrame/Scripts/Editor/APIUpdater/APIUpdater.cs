using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace OxGFrame.CoreFrame.Editor
{
    public static class APIUpdater
    {
        /// <summary>
        /// 過時的 API 和新 API 的對應關係
        /// </summary>
        private static readonly Dictionary<string, string> _deprecatedApis = new Dictionary<string, string>
        {
            // v2.12.5 -> v2.13.0 or higher
            { "protected override void ShowAnimation(AnimationEnd animationEnd)", "protected override void OnShowAnimation(AnimationEnd animationEnd)"},
            { "protected override void HideAnimation(AnimationEnd animationEnd)", "protected override void OnCloseAnimation(AnimationEnd animationEnd)"}
        };

        [MenuItem("OxGFrame/Others/Try to update the APIs (Search and replace in all files)", false, 999)]
        public static void UpdateAPI()
        {
            StringBuilder sb = new StringBuilder();

            bool hasAnyChanged = false;

            // 查找所有的 .cs 腳本
            string[] scripts = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
            foreach (string scriptPath in scripts)
            {
                bool dirty = false;
                string content = File.ReadAllText(scriptPath);

                // 跳過自身腳本, 防止相關字串被 replace
                if (Path.GetFileName(scriptPath) == $"{nameof(APIUpdater)}.cs")
                    continue;

                // 檢查並替換所有過時的 API 語法
                foreach (var pair in _deprecatedApis)
                {
                    if (content.Contains(pair.Key))
                    {
                        sb.AppendLine($"Found deprecated API: {pair.Key} in {scriptPath}");
                        content = content.Replace(pair.Key, pair.Value);
                        dirty = true;
                    }
                }

                // 寫回文件
                if (dirty)
                {
                    File.WriteAllText(scriptPath, content);
                    hasAnyChanged = true;
                }
            }

            // 刷新資源
            if (hasAnyChanged)
            {
                EditorUtility.DisplayDialog(
                    "API Update",
                    "Update complete. Please manually execute AssetDatabase Refresh.\n\nYou can hit [Ctrl + R] or go to [Assets -> Refresh].",
                    "OK"
                );

                sb.AppendLine("API update completed.");
            }
            else
            {
                sb.AppendLine("API already up-to-date.");
            }

            Debug.Log(sb.ToString());
        }
    }
}