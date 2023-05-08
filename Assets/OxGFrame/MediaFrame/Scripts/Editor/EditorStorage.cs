using System.Collections.Generic;
using UnityEditor;

namespace OxGFrame.MediaFrame.Editor
{
    public class EditorStorage
    {
        public static void SaveString(string key, string value)
        {
            EditorPrefs.SetString(key, value);
        }

        public static string GetString(string key, string defaultValue = "")
        {
            return EditorPrefs.GetString(key, defaultValue);
        }

        public static void SaveInt(string key, int value)
        {
            EditorPrefs.SetInt(key, value);
        }
        public static int GetInt(string key, int defaultValue = 0)
        {
            return EditorPrefs.GetInt(key, defaultValue);
        }

        public static void SaveFloat(string key, float value)
        {
            EditorPrefs.SetFloat(key, value);
        }
        public static float GetFloat(string key, float defaultValue = 0f)
        {
            return EditorPrefs.GetFloat(key, defaultValue);
        }

        public static bool HasKey(string key)
        {
            return EditorPrefs.HasKey(key);
        }

        public static void DeleteKey(string key)
        {
            EditorPrefs.DeleteKey(key);
        }

        public static void DeleteAll()
        {
            EditorPrefs.DeleteAll();
        }

        /// <summary>
        /// 透過文本形式儲存資料
        /// </summary>
        /// <param name="contentKey"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SaveData(string contentKey, string key, string value)
        {
            string content = GetString(contentKey);

            var allWords = content.Split('\n');
            var lines = new List<string>(allWords);
            Dictionary<string, string> dataMap = new Dictionary<string, string>();
            foreach (var readLine in lines)
            {
                //Debug.Log($"save readline: {readLine}");
                if (readLine.IndexOf('#') != -1 && readLine[0] == '#') continue;
                var args = readLine.Split(' ');
                if (args.Length >= 2)
                {
                    //Debug.Log($"save args => key: {args[0]}, value: {args[1]}");
                    if (!dataMap.ContainsKey(args[0])) dataMap.Add(args[0], args[1].Replace("\n", "").Replace("\r", ""));
                }
            }

            if (dataMap.ContainsKey(key))
            {
                content = content.Replace($"{key} {dataMap[key]}", $"{key} {value}");
            }
            else
            {
                content += $"{key} {value}\n";
            }

            SaveString(contentKey, content);
        }

        /// <summary>
        /// 取得文本中的特定數據
        /// </summary>
        /// <param name="contentKey"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetData(string contentKey, string key, string defaultValue = null)
        {
            string content = GetString(contentKey);
            if (string.IsNullOrEmpty(content)) return defaultValue;

            var allWords = content.Split('\n');
            var lines = new List<string>(allWords);
            Dictionary<string, string> dataMap = new Dictionary<string, string>();
            foreach (var readLine in lines)
            {
                if (readLine.IndexOf('#') != -1 && readLine[0] == '#') continue;
                var args = readLine.Split(' ');
                if (args.Length >= 2)
                {
                    if (!dataMap.ContainsKey(args[0])) dataMap.Add(args[0], args[1].Replace("\n", "").Replace("\r", ""));
                }
            }

            dataMap.TryGetValue(key, out string value);
            if (string.IsNullOrEmpty(value)) return defaultValue;

            return value;
        }

        /// <summary>
        /// 刪除文本中的透定數據
        /// </summary>
        /// <param name="contentKey"></param>
        /// <param name="key"></param>
        public static void DeleteData(string contentKey, string key)
        {
            string content = GetString(contentKey);

            var allWords = content.Split('\n');
            var lines = new List<string>(allWords);
            Dictionary<string, string> dataMap = new Dictionary<string, string>();
            foreach (var readLine in lines)
            {
                if (readLine.IndexOf('#') != -1 && readLine[0] == '#') continue;
                var args = readLine.Split(' ');
                if (args.Length >= 2)
                {
                    if (!dataMap.ContainsKey(args[0])) dataMap.Add(args[0], args[1].Replace("\n", "").Replace("\r", ""));
                }
            }

            if (dataMap.ContainsKey(key))
            {
                content = content.Replace($"{key} {dataMap[key]}\n", string.Empty);
            }

            SaveString(contentKey, content);
        }

        /// <summary>
        /// 刪除全文本數據
        /// </summary>
        /// <param name="contextKey"></param>
        public static void DeleteContext(string contextKey)
        {
            DeleteKey(contextKey);
        }
    }
}