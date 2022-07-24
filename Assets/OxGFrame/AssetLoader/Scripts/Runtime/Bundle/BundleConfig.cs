#define ENABLE_BUNDLE_STREAM_MODE // 啟用文件流讀取 (內存較小)

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace AssetLoader.Bundle
{
    public static class BundleConfig
    {
        public class CryptogramType
        {
            public const string NONE = "NONE";
            public const string OFFSET = "OFFSET";
            public const string XOR = "XOR";
            public const string AES = "AES";
        }

        static BundleConfig()
        {
            bAssetDatabaseMode = GetAssetDatabaseMode();
            //bBundleStreamMode = GetBundleStreamMode();
        }

        // 啟用Editor中的AssetDatabase讀取資源模式
        public static bool bAssetDatabaseMode = true;
        public const string KEY_ASSET_DATABASE_MODE = "bAssetDatabaseMode";

#if ENABLE_BUNDLE_STREAM_MODE
        public static bool bBundleStreamMode = true;
#else
        public static bool bBundleStreamMode = false;
#endif
        //public const string KEY_BUNDLE_STREAM_MODE = "bBundleStreamMode";

        public static void SaveAssetDatabaseMode(bool active)
        {
            PlayerPrefs.SetString(KEY_ASSET_DATABASE_MODE, active.ToString());
            PlayerPrefs.Save();
        }

        public static bool GetAssetDatabaseMode()
        {
            return Convert.ToBoolean(PlayerPrefs.GetString(KEY_ASSET_DATABASE_MODE, "true"));
        }

        //public static void SaveBundleStreamMode(bool active)
        //{
        //    PlayerPrefs.SetString(KEY_BUNDLE_STREAM_MODE, active.ToString());
        //    PlayerPrefs.Save();
        //}

        //public static bool GetBundleStreamMode()
        //{
        //    return Convert.ToBoolean(PlayerPrefs.GetString(KEY_BUNDLE_STREAM_MODE, "true"));
        //}

        // [NONE], [OFFSET, dummySize], [XOR, key], [AES, key, iv] => ex: "None" or "offset, 32" or "Xor, 123" or "Aes, key, iv"
        private static string _cryptogram;
        public static string[] cryptogramArgs
        {
            get
            {
                if (string.IsNullOrEmpty(_cryptogram)) return new string[] { CryptogramType.NONE };
                else
                {
                    string[] args = _cryptogram.Trim().Split(',');
                    for (int i = 0; i < args.Length; i++)
                    {
                        args[i] = args[i].Trim();
                    }
                    return args;
                }
            }
        }

        // 配置檔中的KEY
        public const string APP_VERSION = "APP_VERSION";
        public const string RES_VERSION = "RES_VERSION";

        // 配置檔
        public const string cfgExt = "";                 // 自行輸入(.json), 空字串表示無副檔名
        public const string bundleCfgName = "b_cfg";     // 配置檔的名稱   
        public const string recordCfgName = "r_cfg";     // 記錄配置檔的名稱

        /**
         * url_cfg format following
         * bundle_ip 127.0.0.1
         * # => comment
         */

        // 佈署配置檔中的KEY
        public const string BUNDLE_IP = "bundle_ip";
        public const string GOOGLE_STORE = "google_store";
        public const string APPLE_STORE = "apple_store";

        // 佈署配置檔
        public const string cfgFullPathName = "bundle_cfg.txt";

        // Bundle平台路徑
        public const string bundleDir = "/AssetBundles";  // Build 目錄
        public const string exportDir = "/ExportBundles"; // Export 目錄
        public const string winDir = "/win";
        public const string androidDir = "/android";
        public const string iosDir = "/ios";
        public const string h5Dir = "/h5";

        public static void InitCryptogram(string cryptogram)
        {
            _cryptogram = cryptogram;
        }

        public static async UniTask<string> GetValueFromUrlCfg(string key)
        {
            string pathName = System.IO.Path.Combine(Application.streamingAssetsPath, cfgFullPathName);
            var file = await FileRequest(pathName);
            var content = file.text;
            var allWords = content.Split('\n');
            var lines = new List<string>(allWords);
            var fileMap = new Dictionary<string, string>();
            foreach (var readLine in lines)
            {
                if (readLine.IndexOf('#') != -1 && readLine[0] == '#') continue;
                var args = readLine.Split(' ');
                if (args.Length >= 2)
                {
                    if (!fileMap.ContainsKey(args[0])) fileMap.Add(args[0], args[1].Replace("\n", "").Replace("\r", ""));
                }
            }

            fileMap.TryGetValue(key, out string value);
            return value;
        }

        public static async UniTask<string> GetAppStoreLink()
        {
#if UNITY_ANDROID
            return await GetValueFromUrlCfg(GOOGLE_STORE);
#elif UNITY_IPHONE
            return await GetValueFromUrlCfg(APPLE_STORE);
#endif
            return string.Empty;
        }

        /// <summary>
        /// 取得打包後的Bundle資源路徑
        /// </summary>
        /// <returns></returns>
        public static string GetBuildedBundlePath()
        {
#if UNITY_STANDALONE_WIN
            return Path.Combine(Application.dataPath, $"..{bundleDir}{winDir}");
#endif

#if UNITY_ANDROID
            return Path.Combine(Application.dataPath , $"..{bundleDir}{androidDir}");
#endif

#if UNITY_IOS
            return Path.Combine(Application.dataPath , $"..{bundleDir}{iosDir}");
#endif

#if UNITY_WEBGL
            return Path.Combine(Application.dataPath, $"..{bundleDir}{h5Dir}");
#endif

            throw new System.Exception("ERROR Bundle PATH !!!");
        }

        public static string GetExportBundlePath()
        {
#if UNITY_STANDALONE_WIN
            return Path.Combine(Application.dataPath, $"..{exportDir}{winDir}");
#endif

#if UNITY_ANDROID
            return Path.Combine(Application.dataPath , $"..{exportDir}{androidDir}");
#endif

#if UNITY_IOS
            return Path.Combine(Application.dataPath , $"..{exportDir}{iosDir}");
#endif

#if UNITY_WEBGL
            return Path.Combine(Application.dataPath, $"..{exportDir}{h5Dir}");
#endif

            throw new System.Exception("ERROR Export PATH !!!");
        }

        /// <summary>
        /// 取得本地Bundle下載後的儲存路徑 (持久化)
        /// </summary>
        /// <returns></returns>
        public static string GetLocalDlFileSaveDirectory()
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                // IOS要用這個路徑，否則審核不過
                return Application.temporaryCachePath;
            }

            // Android、PC 可以使用這個路徑
            return Application.persistentDataPath + exportDir;
        }

        /// <summary>
        /// 取得本地BundleConfig下載後的儲存路徑 (持久化)
        /// </summary>
        /// <returns></returns>
        public static string GetLocalDlFileSaveBundleConfigPath()
        {
            return Path.Combine(GetLocalDlFileSaveDirectory(), $"{bundleCfgName}{cfgExt}");
        }

        public static string GetStreamingAssetsBundleConfigPath()
        {
            return Path.Combine(Application.streamingAssetsPath, $"{bundleCfgName}{cfgExt}");
        }

        /// <summary>
        /// 取得資源伺服器的Bundle (URL)
        /// </summary>
        /// <returns></returns>
        public static async UniTask<string> GetServerBundleUrl()
        {
#if UNITY_STANDALONE_WIN
            return await GetValueFromUrlCfg(BUNDLE_IP) + $"{exportDir}{winDir}";
#endif

#if UNITY_ANDROID
            return await GetValueFromUrlCfg(BUNDLE_IP) + $"{exportDir}{androidDir}";
#endif

#if UNITY_IOS
            return await GetValueFromUrlCfg(BUNDLE_IP) + $"{exportDir}{iosDir}";
#endif

#if UNITY_WEBGL
            return await GetValueFromUrlCfg(BUNDLE_IP) + $"{exportDir}{h5Dir}";
#endif
        }

        /// <summary>
        /// 取得對應平台的Manifest檔名
        /// </summary>
        /// <returns></returns>
        public static string GetManifestFileName()
        {
#if UNITY_STANDALONE_WIN
            return winDir.Replace("/", string.Empty).Trim();
#endif

#if UNITY_ANDROID
            return androidDir.Replace("/", string.Empty).Trim();
#endif

#if UNITY_IOS
            return iosDir.Replace("/", string.Empty).Trim();
#endif

#if UNITY_WEBGL
            return h5Dir.Replace("/", string.Empty).Trim();
#endif
        }

        public static async UniTask<TextAsset> FileRequest(string url)
        {
            try
            {
                var request = UnityWebRequest.Get(url);
                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log($"<color=#FF0000>Request failed, URL: {url}</color>");
                    request.Dispose();

                    return null;
                }

                string txt = request.downloadHandler.text;
                TextAsset file = new TextAsset(txt);
                request.Dispose();

                return file;
            }
            catch
            {
                Debug.Log($"<color=#FF0000>Request failed, URL: {url}</color>");
                return null;
            }
        }
    }
}
