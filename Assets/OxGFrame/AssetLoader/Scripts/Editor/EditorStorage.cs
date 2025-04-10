using OxGKit.SaverSystem;
using OxGKit.SaverSystem.Editor;

namespace OxGFrame.AssetLoader.Editor
{
    public class EditorStorage
    {
        private static Saver _saver = new EditorPrefsSaver();

        public static void SaveString(string key, string value)
        {
            _saver.SaveString(key, value);
        }

        public static string GetString(string key, string defaultValue = "")
        {
            return _saver.GetString(key, defaultValue);
        }

        public static void SaveInt(string key, int value)
        {
            _saver.SaveInt(key, value);
        }
        public static int GetInt(string key, int defaultValue = 0)
        {
            return _saver.GetInt(key, defaultValue);
        }

        public static void SaveFloat(string key, float value)
        {
            _saver.SaveFloat(key, value);
        }
        public static float GetFloat(string key, float defaultValue = 0f)
        {
            return _saver.GetFloat(key, defaultValue);
        }

        public static bool HasKey(string key)
        {
            return _saver.HasKey(key);
        }

        public static void DeleteKey(string key)
        {
            _saver.DeleteKey(key);
        }

        public static void DeleteAll()
        {
            _saver.DeleteAll();
        }

        public static void SaveData(string contentKey, string key, string value)
        {
            _saver.SaveData(contentKey, key, value);
        }

        public static string GetData(string contentKey, string key, string defaultValue = null)
        {
            return _saver.GetData(contentKey, key, defaultValue);
        }

        public static void DeleteData(string contentKey, string key)
        {
            _saver.DeleteData(contentKey, key);
        }

        public static void DeleteContext(string contextKey)
        {
            _saver.DeleteKey(contextKey);
        }
    }
}