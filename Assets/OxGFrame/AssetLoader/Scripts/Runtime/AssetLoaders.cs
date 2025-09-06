using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader.Cacher;
using OxGFrame.AssetLoader.GroupCacher;
using OxGKit.LoggingSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;

namespace OxGFrame.AssetLoader
{
    public static class AssetLoaders
    {
        internal const byte MAX_RETRY_COUNT = 3;

        #region Scene
        /// <summary>
        /// Only load scene from bundle
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="loadSceneMode"></param>
        /// <param name="activateOnLoad"></param>
        /// <param name="priority"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        public static async UniTask<BundlePack> LoadSceneAsync(string assetName, LoadSceneMode loadSceneMode = LoadSceneMode.Single, bool activateOnLoad = true, uint priority = 100, Progression progression = null)
        {
            string packageName = AssetPatcher.GetDefaultPackageName();
            return await CacheBundle.GetInstance().LoadSceneAsync(packageName, assetName, loadSceneMode, LocalPhysicsMode.None, activateOnLoad, priority, progression);
        }

        public static async UniTask<BundlePack> LoadSceneAsync(string assetName, LoadSceneMode loadSceneMode = LoadSceneMode.Single, LocalPhysicsMode localPhysicsMode = LocalPhysicsMode.None, bool activateOnLoad = true, uint priority = 100, Progression progression = null)
        {
            string packageName = AssetPatcher.GetDefaultPackageName();
            return await CacheBundle.GetInstance().LoadSceneAsync(packageName, assetName, loadSceneMode, localPhysicsMode, activateOnLoad, priority, progression);
        }

        public static async UniTask<BundlePack> LoadSceneAsync(string packageName, string assetName, LoadSceneMode loadSceneMode = LoadSceneMode.Single, bool activateOnLoad = true, uint priority = 100, Progression progression = null)
        {
            return await CacheBundle.GetInstance().LoadSceneAsync(packageName, assetName, loadSceneMode, LocalPhysicsMode.None, activateOnLoad, priority, progression);
        }

        public static async UniTask<BundlePack> LoadSceneAsync(string packageName, string assetName, LoadSceneMode loadSceneMode = LoadSceneMode.Single, LocalPhysicsMode localPhysicsMode = LocalPhysicsMode.None, bool activateOnLoad = true, uint priority = 100, Progression progression = null)
        {
            return await CacheBundle.GetInstance().LoadSceneAsync(packageName, assetName, loadSceneMode, localPhysicsMode, activateOnLoad, priority, progression);
        }

        /// <summary>
        /// Only load scene from bundle
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="loadSceneMode"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        public static BundlePack LoadScene(string assetName, LoadSceneMode loadSceneMode = LoadSceneMode.Single, Progression progression = null)
        {
            string packageName = AssetPatcher.GetDefaultPackageName();
            return CacheBundle.GetInstance().LoadScene(packageName, assetName, loadSceneMode, LocalPhysicsMode.None, progression);
        }

        public static BundlePack LoadScene(string assetName, LoadSceneMode loadSceneMode = LoadSceneMode.Single, LocalPhysicsMode localPhysicsMode = LocalPhysicsMode.None, Progression progression = null)
        {
            string packageName = AssetPatcher.GetDefaultPackageName();
            return CacheBundle.GetInstance().LoadScene(packageName, assetName, loadSceneMode, localPhysicsMode, progression);
        }

        public static BundlePack LoadScene(string packageName, string assetName, LoadSceneMode loadSceneMode = LoadSceneMode.Single, Progression progression = null)
        {
            return CacheBundle.GetInstance().LoadScene(packageName, assetName, loadSceneMode, LocalPhysicsMode.None, progression);
        }

        public static BundlePack LoadScene(string packageName, string assetName, LoadSceneMode loadSceneMode = LoadSceneMode.Single, LocalPhysicsMode localPhysicsMode = LocalPhysicsMode.None, Progression progression = null)
        {
            return CacheBundle.GetInstance().LoadScene(packageName, assetName, loadSceneMode, localPhysicsMode, progression);
        }

        /// <summary>
        /// Only load scene from bundle
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="activateOnLoad"></param>
        /// <param name="priority"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        public static async UniTask<BundlePack> LoadSingleSceneAsync(string assetName, bool activateOnLoad = true, uint priority = 100, Progression progression = null)
        {
            string packageName = AssetPatcher.GetDefaultPackageName();
            return await CacheBundle.GetInstance().LoadSceneAsync(packageName, assetName, LoadSceneMode.Single, LocalPhysicsMode.None, activateOnLoad, priority, progression);
        }

        public static async UniTask<BundlePack> LoadSingleSceneAsync(string packageName, string assetName, bool activateOnLoad = true, uint priority = 100, Progression progression = null)
        {
            return await CacheBundle.GetInstance().LoadSceneAsync(packageName, assetName, LoadSceneMode.Single, LocalPhysicsMode.None, activateOnLoad, priority, progression);
        }

        /// <summary>
        /// Only load scene from bundle
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        public static BundlePack LoadSingleScene(string assetName, Progression progression = null)
        {
            string packageName = AssetPatcher.GetDefaultPackageName();
            return CacheBundle.GetInstance().LoadScene(packageName, assetName, LoadSceneMode.Single, LocalPhysicsMode.None, progression);
        }

        public static BundlePack LoadSingleScene(string packageName, string assetName, Progression progression = null)
        {
            return CacheBundle.GetInstance().LoadScene(packageName, assetName, LoadSceneMode.Single, LocalPhysicsMode.None, progression);
        }

        /// <summary>
        /// Only load scene from bundle
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="activateOnLoad"></param>
        /// <param name="priority"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        public static async UniTask<BundlePack> LoadAdditiveSceneAsync(string assetName, bool activateOnLoad = true, uint priority = 100, Progression progression = null)
        {
            string packageName = AssetPatcher.GetDefaultPackageName();
            return await CacheBundle.GetInstance().LoadSceneAsync(packageName, assetName, LoadSceneMode.Additive, LocalPhysicsMode.None, activateOnLoad, priority, progression);
        }

        public static async UniTask<BundlePack> LoadAdditiveSceneAsync(string packageName, string assetName, bool activateOnLoad = true, uint priority = 100, Progression progression = null)
        {
            return await CacheBundle.GetInstance().LoadSceneAsync(packageName, assetName, LoadSceneMode.Additive, LocalPhysicsMode.None, activateOnLoad, priority, progression);
        }

        /// <summary>
        /// Only load scene from bundle
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        public static BundlePack LoadAdditiveScene(string assetName, Progression progression = null)
        {
            string packageName = AssetPatcher.GetDefaultPackageName();
            return CacheBundle.GetInstance().LoadScene(packageName, assetName, LoadSceneMode.Additive, LocalPhysicsMode.None, progression);
        }

        public static BundlePack LoadAdditiveScene(string packageName, string assetName, Progression progression = null)
        {
            return CacheBundle.GetInstance().LoadScene(packageName, assetName, LoadSceneMode.Additive, LocalPhysicsMode.None, progression);
        }

        /// <summary>
        /// Only unload scene from bundle
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="recursively"></param>
        public async static UniTask UnloadScene(string assetName, bool recursively = false)
        {
            if (!AssetPatcher.IsReleased())
                await CacheBundle.GetInstance().UnloadScene(assetName, recursively);
        }

        public async static UniTask ReleaseBundleScenes()
        {
            if (!AssetPatcher.IsReleased())
                await CacheBundle.GetInstance().ReleaseScenes();
        }
        #endregion

        #region Cacher
        public static bool HasInCache(string assetName)
        {
            if (RefineResourcesPath(ref assetName)) return CacheResource.GetInstance().HasInCache(assetName);
            else return CacheBundle.GetInstance().HasInCache(assetName);
        }

        /// <summary>
        /// Get asset object cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName"></param>
        /// <returns>
        /// <para>From Resources is &lt;ResourcePack&gt;</para>
        /// <para>From Bundle is &lt;BundlePack&gt;</para>
        /// </returns>
        public static T GetFromCache<T>(string assetName) where T : AssetObject
        {
            if (RefineResourcesPath(ref assetName)) return CacheResource.GetInstance().GetFromCache(assetName) as T;
            else return CacheBundle.GetInstance().GetFromCache(assetName) as T;
        }

        #region RawFile
        /// <summary>
        /// Get RawFile save path
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static async UniTask<string> GetRawFilePathAsync(string assetName)
        {
            // Use preload to load bundle in cache, but for raw file the memory has not been allocated yet
            await PreloadRawFileAsync(assetName);
            var pack = GetFromCache<BundlePack>(assetName);
            if (pack != null)
            {
                // Get path from operation handle
                var operation = pack.GetOperationHandle<RawFileHandle>();
                string filePath = operation.GetRawFilePath();
                await UnloadRawFile(assetName, true);
                return filePath;
            }

            return null;
        }

        /// <summary>
        /// Get RawFile save path from specific package
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static async UniTask<string> GetRawFilePathAsync(string packageName, string assetName)
        {
            // Use preload to load bundle in cache, but for raw file the memory has not been allocated yet
            await PreloadRawFileAsync(packageName, assetName);
            var pack = GetFromCache<BundlePack>(assetName);
            if (pack != null)
            {
                // Get path from operation handle
                var operation = pack.GetOperationHandle<RawFileHandle>();
                string filePath = operation.GetRawFilePath();
                await UnloadRawFile(assetName, true);
                return filePath;
            }

            return null;
        }

        /// <summary>
        /// Get RawFile save path
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static string GetRawFilePath(string assetName)
        {
            // Use preload to load bundle in cache, but for raw file the memory has not been allocated yet
            PreloadRawFile(assetName);
            var pack = GetFromCache<BundlePack>(assetName);
            if (pack != null)
            {
                var operation = pack.GetOperationHandle<RawFileHandle>();
                // Get path from operation handle
                string filePath = operation.GetRawFilePath();
                UnloadRawFile(assetName, true).Forget();
                return filePath;
            }

            return null;
        }

        /// <summary>
        /// Get RawFile save path from specific package
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static string GetRawFilePath(string packageName, string assetName)
        {
            // Use preload to load bundle in cache, but for raw file the memory has not been allocated yet
            PreloadRawFile(packageName, assetName);
            var pack = GetFromCache<BundlePack>(assetName);
            if (pack != null)
            {
                var operation = pack.GetOperationHandle<RawFileHandle>();
                // Get path from operation handle
                string filePath = operation.GetRawFilePath();
                UnloadRawFile(assetName, true).Forget();
                return filePath;
            }

            return null;
        }

        public static async UniTask PreloadRawFileAsync(string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName)) Logging.PrintError<Logger>("【Error】Only supports the bundle type.");
            else await CacheBundle.GetInstance().PreloadRawFileAsync(packageName, new string[] { assetName }, priority, progression, maxRetryCount);
        }

        public static async UniTask PreloadRawFileAsync(string packageName, string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            if (RefineResourcesPath(ref assetName)) Logging.PrintError<Logger>("【Error】Only supports the bundle type.");
            else await CacheBundle.GetInstance().PreloadRawFileAsync(packageName, new string[] { assetName }, priority, progression, maxRetryCount);
        }

        public static async UniTask PreloadRawFileAsync(string[] assetNames, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            var packageName = AssetPatcher.GetDefaultPackageName();

            List<string> refineAssetNames = new List<string>();

            for (int i = 0; i < assetNames.Length; i++)
            {
                if (string.IsNullOrEmpty(assetNames[i]))
                {
                    continue;
                }

                if (RefineResourcesPath(ref assetNames[i])) refineAssetNames.Add(assetNames[i]);
            }

            if (refineAssetNames.Count > 0) Logging.PrintError<Logger>("【Error】Only supports the bundle type.");
            else await CacheBundle.GetInstance().PreloadRawFileAsync(packageName, assetNames, priority, progression, maxRetryCount);
        }

        public static async UniTask PreloadRawFileAsync(string packageName, string[] assetNames, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            List<string> refineAssetNames = new List<string>();

            for (int i = 0; i < assetNames.Length; i++)
            {
                if (string.IsNullOrEmpty(assetNames[i]))
                {
                    continue;
                }

                if (RefineResourcesPath(ref assetNames[i])) refineAssetNames.Add(assetNames[i]);
            }

            if (refineAssetNames.Count > 0) Logging.PrintError<Logger>("【Error】Only supports the bundle type.");
            else await CacheBundle.GetInstance().PreloadRawFileAsync(packageName, assetNames, priority, progression, maxRetryCount);
        }

        public static void PreloadRawFile(string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName)) Logging.PrintError<Logger>("【Error】Only supports the bundle type.");
            else CacheBundle.GetInstance().PreloadRawFile(packageName, new string[] { assetName }, progression, maxRetryCount);
        }

        public static void PreloadRawFile(string packageName, string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            if (RefineResourcesPath(ref assetName)) Logging.PrintError<Logger>("【Error】Only supports the bundle type.");
            else CacheBundle.GetInstance().PreloadRawFile(packageName, new string[] { assetName }, progression, maxRetryCount);
        }

        public static void PreloadRawFile(string[] assetNames, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            var packageName = AssetPatcher.GetDefaultPackageName();

            List<string> refineAssetNames = new List<string>();

            for (int i = 0; i < assetNames.Length; i++)
            {
                if (string.IsNullOrEmpty(assetNames[i]))
                {
                    continue;
                }

                if (RefineResourcesPath(ref assetNames[i])) refineAssetNames.Add(assetNames[i]);
            }

            if (refineAssetNames.Count > 0) Logging.PrintError<Logger>("【Error】Only supports the bundle type.");
            else CacheBundle.GetInstance().PreloadRawFile(packageName, assetNames, progression, maxRetryCount);
        }

        public static void PreloadRawFile(string packageName, string[] assetNames, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            List<string> refineAssetNames = new List<string>();

            for (int i = 0; i < assetNames.Length; i++)
            {
                if (string.IsNullOrEmpty(assetNames[i]))
                {
                    continue;
                }

                if (RefineResourcesPath(ref assetNames[i])) refineAssetNames.Add(assetNames[i]);
            }

            if (refineAssetNames.Count > 0) Logging.PrintError<Logger>("【Error】Only supports the bundle type.");
            else CacheBundle.GetInstance().PreloadRawFile(packageName, assetNames, progression, maxRetryCount);
        }

        /// <summary>
        /// Only load string type and byte[] type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName"></param>
        /// <param name="priority"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        public static async UniTask<T> LoadRawFileAsync<T>(string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName))
            {
                Logging.PrintError<Logger>("【Error】Only supports the bundle type.");
                return default;
            }
            return await CacheBundle.GetInstance().LoadRawFileAsync<T>(packageName, assetName, priority, progression, maxRetryCount);
        }

        public static async UniTask<T> LoadRawFileAsync<T>(string packageName, string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            if (RefineResourcesPath(ref assetName))
            {
                Logging.PrintError<Logger>("【Error】Only supports the bundle type.");
                return default;
            }
            return await CacheBundle.GetInstance().LoadRawFileAsync<T>(packageName, assetName, priority, progression, maxRetryCount);
        }

        /// <summary>
        /// Only load string type and byte[] type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        public static T LoadRawFile<T>(string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName))
            {
                Logging.PrintError<Logger>("【Error】Only supports the bundle type.");
                return default;
            }
            return CacheBundle.GetInstance().LoadRawFile<T>(packageName, assetName, progression, maxRetryCount);
        }

        public static T LoadRawFile<T>(string packageName, string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            if (RefineResourcesPath(ref assetName))
            {
                Logging.PrintError<Logger>("【Error】Only supports the bundle type.");
                return default;
            }
            return CacheBundle.GetInstance().LoadRawFile<T>(packageName, assetName, progression, maxRetryCount);
        }

        public async static UniTask UnloadRawFile(string assetName, bool forceUnload = false)
        {
            if (RefineResourcesPath(ref assetName))
                Logging.PrintError<Logger>("【Error】Only supports the bundle type.");
            else if (!AssetPatcher.IsReleased())
                await CacheBundle.GetInstance().UnloadRawFileAsync(assetName, forceUnload);
        }

        public async static UniTask ReleaseBundleRawFiles()
        {
            if (!AssetPatcher.IsReleased())
                await CacheBundle.GetInstance().ReleaseRawFilesAsync();
        }
        #endregion

        #region Asset
        /// <summary>
        /// If use prefix "res#" will load from resources else will load from bundle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName"></param>
        /// <param name="priority"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        public static async UniTask PreloadAssetAsync<T>(string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName)) await CacheResource.GetInstance().PreloadAssetAsync<T>(new string[] { assetName }, progression, maxRetryCount);
            else await CacheBundle.GetInstance().PreloadAssetAsync<T>(packageName, new string[] { assetName }, priority, progression, maxRetryCount);
        }

        public static async UniTask PreloadAssetAsync<T>(string packageName, string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            if (RefineResourcesPath(ref assetName)) await CacheResource.GetInstance().PreloadAssetAsync<T>(new string[] { assetName }, progression, maxRetryCount);
            else await CacheBundle.GetInstance().PreloadAssetAsync<T>(packageName, new string[] { assetName }, priority, progression, maxRetryCount);
        }

        public static async UniTask PreloadAssetAsync<T>(string[] assetNames, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            var packageName = AssetPatcher.GetDefaultPackageName();

            List<string> refineAssetNames = new List<string>();

            for (int i = 0; i < assetNames.Length; i++)
            {
                if (string.IsNullOrEmpty(assetNames[i]))
                {
                    continue;
                }

                if (RefineResourcesPath(ref assetNames[i])) refineAssetNames.Add(assetNames[i]);
            }

            if (refineAssetNames.Count > 0) await CacheResource.GetInstance().PreloadAssetAsync<T>(assetNames, progression, maxRetryCount);
            else await CacheBundle.GetInstance().PreloadAssetAsync<T>(packageName, assetNames, priority, progression, maxRetryCount);
        }

        public static async UniTask PreloadAssetAsync<T>(string packageName, string[] assetNames, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            List<string> refineAssetNames = new List<string>();

            for (int i = 0; i < assetNames.Length; i++)
            {
                if (string.IsNullOrEmpty(assetNames[i]))
                {
                    continue;
                }

                if (RefineResourcesPath(ref assetNames[i])) refineAssetNames.Add(assetNames[i]);
            }

            if (refineAssetNames.Count > 0) await CacheResource.GetInstance().PreloadAssetAsync<T>(assetNames, progression, maxRetryCount);
            else await CacheBundle.GetInstance().PreloadAssetAsync<T>(packageName, assetNames, priority, progression, maxRetryCount);
        }

        /// <summary>
        /// If use prefix "res#" will load from resources else will load from bundle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        public static void PreloadAsset<T>(string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName)) CacheResource.GetInstance().PreloadAsset<T>(new string[] { assetName }, progression, maxRetryCount);
            else CacheBundle.GetInstance().PreloadAsset<T>(packageName, new string[] { assetName }, progression, maxRetryCount);
        }

        public static void PreloadAsset<T>(string packageName, string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            if (RefineResourcesPath(ref assetName)) CacheResource.GetInstance().PreloadAsset<T>(new string[] { assetName }, progression, maxRetryCount);
            else CacheBundle.GetInstance().PreloadAsset<T>(packageName, new string[] { assetName }, progression, maxRetryCount);
        }

        public static void PreloadAsset<T>(string[] assetNames, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            var packageName = AssetPatcher.GetDefaultPackageName();

            List<string> refineAssetNames = new List<string>();

            for (int i = 0; i < assetNames.Length; i++)
            {
                if (string.IsNullOrEmpty(assetNames[i]))
                {
                    continue;
                }

                if (RefineResourcesPath(ref assetNames[i])) refineAssetNames.Add(assetNames[i]);
            }

            if (refineAssetNames.Count > 0) CacheResource.GetInstance().PreloadAsset<T>(assetNames, progression, maxRetryCount);
            else CacheBundle.GetInstance().PreloadAsset<T>(packageName, assetNames, progression, maxRetryCount);
        }

        public static void PreloadAsset<T>(string packageName, string[] assetNames, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            List<string> refineAssetNames = new List<string>();

            for (int i = 0; i < assetNames.Length; i++)
            {
                if (string.IsNullOrEmpty(assetNames[i]))
                {
                    continue;
                }

                if (RefineResourcesPath(ref assetNames[i])) refineAssetNames.Add(assetNames[i]);
            }

            if (refineAssetNames.Count > 0) CacheResource.GetInstance().PreloadAsset<T>(assetNames, progression, maxRetryCount);
            else CacheBundle.GetInstance().PreloadAsset<T>(packageName, assetNames, progression, maxRetryCount);
        }

        /// <summary>
        /// If use prefix "res#" will load from resources else will load from bundle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName"></param>
        /// <param name="priority"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        public static async UniTask<T> LoadAssetAsync<T>(string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName)) return await CacheResource.GetInstance().LoadAssetAsync<T>(assetName, progression, maxRetryCount);
            else return await CacheBundle.GetInstance().LoadAssetAsync<T>(packageName, assetName, priority, progression, maxRetryCount);
        }

        public static async UniTask<T> LoadAssetAsync<T>(string packageName, string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            if (RefineResourcesPath(ref assetName)) return await CacheResource.GetInstance().LoadAssetAsync<T>(assetName, progression, maxRetryCount);
            else return await CacheBundle.GetInstance().LoadAssetAsync<T>(packageName, assetName, priority, progression, maxRetryCount);
        }

        /// <summary>
        /// If use prefix "res#" will load from resources else will load from bundle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        public static T LoadAsset<T>(string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName)) return CacheResource.GetInstance().LoadAsset<T>(assetName, progression, maxRetryCount);
            else return CacheBundle.GetInstance().LoadAsset<T>(packageName, assetName, progression, maxRetryCount);
        }

        public static T LoadAsset<T>(string packageName, string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            if (RefineResourcesPath(ref assetName)) return CacheResource.GetInstance().LoadAsset<T>(assetName, progression, maxRetryCount);
            else return CacheBundle.GetInstance().LoadAsset<T>(packageName, assetName, progression, maxRetryCount);
        }

        /// <summary>
        /// If use prefix "res#" will load from resources else will load from bundle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName"></param>
        /// <param name="priority"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        public static async UniTask<T> InstantiateAssetAsync<T>(string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName))
            {
                var asset = await CacheResource.GetInstance().LoadAssetAsync<T>(assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset);
                return cloneAsset;
            }
            else
            {
                var asset = await CacheBundle.GetInstance().LoadAssetAsync<T>(packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset);
                return cloneAsset;
            }
        }

        public static async UniTask<T> InstantiateAssetAsync<T>(string packageName, string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = await CacheResource.GetInstance().LoadAssetAsync<T>(assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset);
                return cloneAsset;
            }
            else
            {
                var asset = await CacheBundle.GetInstance().LoadAssetAsync<T>(packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset);
                return cloneAsset;
            }
        }

        public static async UniTask<T> InstantiateAssetAsync<T>(string assetName, Vector3 position, Quaternion rotation, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName))
            {
                var asset = await CacheResource.GetInstance().LoadAssetAsync<T>(assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
            else
            {
                var asset = await CacheBundle.GetInstance().LoadAssetAsync<T>(packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
        }

        public static async UniTask<T> InstantiateAssetAsync<T>(string packageName, string assetName, Vector3 position, Quaternion rotation, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = await CacheResource.GetInstance().LoadAssetAsync<T>(assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
            else
            {
                var asset = await CacheBundle.GetInstance().LoadAssetAsync<T>(packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
        }

        public static async UniTask<T> InstantiateAssetAsync<T>(string assetName, Vector3 position, Quaternion rotation, Transform parent, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName))
            {
                var asset = await CacheResource.GetInstance().LoadAssetAsync<T>(assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
            else
            {
                var asset = await CacheBundle.GetInstance().LoadAssetAsync<T>(packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
        }

        public static async UniTask<T> InstantiateAssetAsync<T>(string packageName, string assetName, Vector3 position, Quaternion rotation, Transform parent, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = await CacheResource.GetInstance().LoadAssetAsync<T>(assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
            else
            {
                var asset = await CacheBundle.GetInstance().LoadAssetAsync<T>(packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
        }

        public static async UniTask<T> InstantiateAssetAsync<T>(string assetName, Transform parent, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName))
            {
                var asset = await CacheResource.GetInstance().LoadAssetAsync<T>(assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent);
                return cloneAsset;
            }
            else
            {
                var asset = await CacheBundle.GetInstance().LoadAssetAsync<T>(packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent);
                return cloneAsset;
            }
        }

        public static async UniTask<T> InstantiateAssetAsync<T>(string packageName, string assetName, Transform parent, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = await CacheResource.GetInstance().LoadAssetAsync<T>(assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent);
                return cloneAsset;
            }
            else
            {
                var asset = await CacheBundle.GetInstance().LoadAssetAsync<T>(packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent);
                return cloneAsset;
            }
        }

        public static async UniTask<T> InstantiateAssetAsync<T>(string assetName, Transform parent, bool worldPositionStays, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName))
            {
                var asset = await CacheResource.GetInstance().LoadAssetAsync<T>(assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
            else
            {
                var asset = await CacheBundle.GetInstance().LoadAssetAsync<T>(packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
        }

        public static async UniTask<T> InstantiateAssetAsync<T>(string packageName, string assetName, Transform parent, bool worldPositionStays, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = await CacheResource.GetInstance().LoadAssetAsync<T>(assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
            else
            {
                var asset = await CacheBundle.GetInstance().LoadAssetAsync<T>(packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
        }

        /// <summary>
        /// If use prefix "res#" will load from resources else will load from bundle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        public static T InstantiateAsset<T>(string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName))
            {
                var asset = CacheResource.GetInstance().LoadAsset<T>(assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset);
                return cloneAsset;
            }
            else
            {
                var asset = CacheBundle.GetInstance().LoadAsset<T>(packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset);
                return cloneAsset;
            }
        }

        public static T InstantiateAsset<T>(string packageName, string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = CacheResource.GetInstance().LoadAsset<T>(assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset);
                return cloneAsset;
            }
            else
            {
                var asset = CacheBundle.GetInstance().LoadAsset<T>(packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset);
                return cloneAsset;
            }
        }

        public static T InstantiateAsset<T>(string assetName, Vector3 position, Quaternion rotation, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName))
            {
                var asset = CacheResource.GetInstance().LoadAsset<T>(assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
            else
            {
                var asset = CacheBundle.GetInstance().LoadAsset<T>(packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
        }

        public static T InstantiateAsset<T>(string packageName, string assetName, Vector3 position, Quaternion rotation, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = CacheResource.GetInstance().LoadAsset<T>(assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
            else
            {
                var asset = CacheBundle.GetInstance().LoadAsset<T>(packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
        }

        public static T InstantiateAsset<T>(string assetName, Vector3 position, Quaternion rotation, Transform parent, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName))
            {
                var asset = CacheResource.GetInstance().LoadAsset<T>(assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
            else
            {
                var asset = CacheBundle.GetInstance().LoadAsset<T>(packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
        }

        public static T InstantiateAsset<T>(string packageName, string assetName, Vector3 position, Quaternion rotation, Transform parent, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = CacheResource.GetInstance().LoadAsset<T>(assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
            else
            {
                var asset = CacheBundle.GetInstance().LoadAsset<T>(packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
        }

        public static T InstantiateAsset<T>(string assetName, Transform parent, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName))
            {
                var asset = CacheResource.GetInstance().LoadAsset<T>(assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent);
                return cloneAsset;
            }
            else
            {
                var asset = CacheBundle.GetInstance().LoadAsset<T>(packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent);
                return cloneAsset;
            }
        }

        public static T InstantiateAsset<T>(string packageName, string assetName, Transform parent, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = CacheResource.GetInstance().LoadAsset<T>(assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent);
                return cloneAsset;
            }
            else
            {
                var asset = CacheBundle.GetInstance().LoadAsset<T>(packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent);
                return cloneAsset;
            }
        }

        public static T InstantiateAsset<T>(string assetName, Transform parent, bool worldPositionStays, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName))
            {
                var asset = CacheResource.GetInstance().LoadAsset<T>(assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
            else
            {
                var asset = CacheBundle.GetInstance().LoadAsset<T>(packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
        }

        public static T InstantiateAsset<T>(string packageName, string assetName, Transform parent, bool worldPositionStays, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = CacheResource.GetInstance().LoadAsset<T>(assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
            else
            {
                var asset = CacheBundle.GetInstance().LoadAsset<T>(packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
        }

        public async static UniTask UnloadAsset(string assetName, bool forceUnload = false)
        {
            if (RefineResourcesPath(ref assetName))
                CacheResource.GetInstance().UnloadAsset(assetName, forceUnload);
            else if (!AssetPatcher.IsReleased())
                await CacheBundle.GetInstance().UnloadAsset(assetName, forceUnload);
        }

        public static void ReleaseResourceAssets()
        {
            CacheResource.GetInstance().ReleaseAssets();
        }

        public async static UniTask ReleaseBundleAssets()
        {
            if (!AssetPatcher.IsReleased())
                await CacheBundle.GetInstance().ReleaseAssets();
        }
        #endregion
        #endregion

        #region Group Cacher
        public static bool HasInCache(int groupId, string assetName)
        {
            if (RefineResourcesPath(ref assetName)) return GroupResource.GetInstance().HasInCache(groupId, assetName);
            else return GroupBundle.GetInstance().HasInCache(groupId, assetName);
        }

        #region RawFile
        public static async UniTask PreloadRawFileAsync(int groupId, string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName)) Logging.PrintError<Logger>("【Error】Only supports the bundle type.");
            else await GroupBundle.GetInstance().PreloadRawFileAsync(groupId, packageName, new string[] { assetName }, priority, progression, maxRetryCount);
        }

        public static async UniTask PreloadRawFileAsync(int groupId, string packageName, string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            if (RefineResourcesPath(ref assetName)) Logging.PrintError<Logger>("【Error】Only supports the bundle type.");
            else await GroupBundle.GetInstance().PreloadRawFileAsync(groupId, packageName, new string[] { assetName }, priority, progression, maxRetryCount);
        }

        public static async UniTask PreloadRawFileAsync(int groupId, string[] assetNames, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            var packageName = AssetPatcher.GetDefaultPackageName();

            List<string> refineAssetNames = new List<string>();

            for (int i = 0; i < assetNames.Length; i++)
            {
                if (string.IsNullOrEmpty(assetNames[i]))
                {
                    continue;
                }

                if (RefineResourcesPath(ref assetNames[i])) refineAssetNames.Add(assetNames[i]);
            }

            if (refineAssetNames.Count > 0) Logging.PrintError<Logger>("【Error】Only supports the bundle type.");
            else await GroupBundle.GetInstance().PreloadRawFileAsync(groupId, packageName, assetNames, priority, progression, maxRetryCount);
        }

        public static async UniTask PreloadRawFileAsync(int groupId, string packageName, string[] assetNames, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            List<string> refineAssetNames = new List<string>();

            for (int i = 0; i < assetNames.Length; i++)
            {
                if (string.IsNullOrEmpty(assetNames[i]))
                {
                    continue;
                }

                if (RefineResourcesPath(ref assetNames[i])) refineAssetNames.Add(assetNames[i]);
            }

            if (refineAssetNames.Count > 0) Logging.PrintError<Logger>("【Error】Only supports the bundle type.");
            else await GroupBundle.GetInstance().PreloadRawFileAsync(groupId, packageName, assetNames, priority, progression, maxRetryCount);
        }

        public static void PreloadRawFile(int groupId, string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName)) Logging.PrintError<Logger>("【Error】Only supports the bundle type.");
            else GroupBundle.GetInstance().PreloadRawFile(groupId, packageName, new string[] { assetName }, progression, maxRetryCount);
        }

        public static void PreloadRawFile(int groupId, string packageName, string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            if (RefineResourcesPath(ref assetName)) Logging.PrintError<Logger>("【Error】Only supports the bundle type.");
            else GroupBundle.GetInstance().PreloadRawFile(groupId, packageName, new string[] { assetName }, progression, maxRetryCount);
        }

        public static void PreloadRawFile(int groupId, string[] assetNames, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            var packageName = AssetPatcher.GetDefaultPackageName();

            List<string> refineAssetNames = new List<string>();

            for (int i = 0; i < assetNames.Length; i++)
            {
                if (string.IsNullOrEmpty(assetNames[i]))
                {
                    continue;
                }

                if (RefineResourcesPath(ref assetNames[i])) refineAssetNames.Add(assetNames[i]);
            }

            if (refineAssetNames.Count > 0) Logging.PrintError<Logger>("【Error】Only supports the bundle type.");
            else GroupBundle.GetInstance().PreloadRawFile(groupId, packageName, assetNames, progression, maxRetryCount);
        }

        public static void PreloadRawFile(int groupId, string packageName, string[] assetNames, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            List<string> refineAssetNames = new List<string>();

            for (int i = 0; i < assetNames.Length; i++)
            {
                if (string.IsNullOrEmpty(assetNames[i]))
                {
                    continue;
                }

                if (RefineResourcesPath(ref assetNames[i])) refineAssetNames.Add(assetNames[i]);
            }

            if (refineAssetNames.Count > 0) Logging.PrintError<Logger>("【Error】Only supports the bundle type.");
            else GroupBundle.GetInstance().PreloadRawFile(groupId, packageName, assetNames, progression, maxRetryCount);
        }

        /// <summary>
        /// Only load string type and byte[] type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="groupId"></param>
        /// <param name="assetName"></param>
        /// <param name="priority"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        public static async UniTask<T> LoadRawFileAsync<T>(int groupId, string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName))
            {
                Logging.PrintError<Logger>("【Error】Only supports the bundle type.");
                return default;
            }
            else return await GroupBundle.GetInstance().LoadRawFileAsync<T>(groupId, packageName, assetName, priority, progression, maxRetryCount);
        }

        public static async UniTask<T> LoadRawFileAsync<T>(int groupId, string packageName, string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            if (RefineResourcesPath(ref assetName))
            {
                Logging.PrintError<Logger>("【Error】Only supports the bundle type.");
                return default;
            }
            else return await GroupBundle.GetInstance().LoadRawFileAsync<T>(groupId, packageName, assetName, priority, progression, maxRetryCount);
        }

        /// <summary>
        /// Only load string type and byte[] type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="groupId"></param>
        /// <param name="assetName"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        public static T LoadRawFile<T>(int groupId, string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName))
            {
                Logging.PrintError<Logger>("【Error】Only supports the bundle type.");
                return default;
            }
            else return GroupBundle.GetInstance().LoadRawFile<T>(groupId, packageName, assetName, progression, maxRetryCount);
        }

        public static T LoadRawFile<T>(int groupId, string packageName, string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT)
        {
            if (RefineResourcesPath(ref assetName))
            {
                Logging.PrintError<Logger>("【Error】Only supports the bundle type.");
                return default;
            }
            else return GroupBundle.GetInstance().LoadRawFile<T>(groupId, packageName, assetName, progression, maxRetryCount);
        }

        public static void UnloadRawFile(int groupId, string assetName, bool forceUnload = false)
        {
            if (RefineResourcesPath(ref assetName)) Logging.PrintError<Logger>("【Error】Only supports the bundle type.");
            else if (!AssetPatcher.IsReleased()) GroupBundle.GetInstance().UnloadRawFile(groupId, assetName, forceUnload);
        }

        public static void ReleaseBundleRawFiles(int groupId)
        {
            if (!AssetPatcher.IsReleased())
                GroupBundle.GetInstance().ReleaseRawFiles(groupId);
        }
        #endregion

        #region Asset
        /// <summary>
        /// If use prefix "res#" will load from resources else will load from bundle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="groupId"></param>
        /// <param name="assetName"></param>
        /// <param name="priority"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        public static async UniTask PreloadAssetAsync<T>(int groupId, string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName)) await GroupResource.GetInstance().PreloadAssetAsync<T>(groupId, new string[] { assetName }, progression, maxRetryCount);
            else await GroupBundle.GetInstance().PreloadAssetAsync<T>(groupId, packageName, new string[] { assetName }, priority, progression, maxRetryCount);
        }

        public static async UniTask PreloadAssetAsync<T>(int groupId, string packageName, string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            if (RefineResourcesPath(ref assetName)) await GroupResource.GetInstance().PreloadAssetAsync<T>(groupId, new string[] { assetName }, progression, maxRetryCount);
            else await GroupBundle.GetInstance().PreloadAssetAsync<T>(groupId, packageName, new string[] { assetName }, priority, progression, maxRetryCount);
        }

        public static async UniTask PreloadAssetAsync<T>(int groupId, string[] assetNames, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            var packageName = AssetPatcher.GetDefaultPackageName();

            List<string> refineAssetNames = new List<string>();

            for (int i = 0; i < assetNames.Length; i++)
            {
                if (string.IsNullOrEmpty(assetNames[i]))
                {
                    continue;
                }

                if (RefineResourcesPath(ref assetNames[i])) refineAssetNames.Add(assetNames[i]);
            }

            if (refineAssetNames.Count > 0) await GroupResource.GetInstance().PreloadAssetAsync<T>(groupId, assetNames, progression, maxRetryCount);
            else await GroupBundle.GetInstance().PreloadAssetAsync<T>(groupId, packageName, assetNames, priority, progression, maxRetryCount);
        }

        public static async UniTask PreloadAssetAsync<T>(int groupId, string packageName, string[] assetNames, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            List<string> refineAssetNames = new List<string>();

            for (int i = 0; i < assetNames.Length; i++)
            {
                if (string.IsNullOrEmpty(assetNames[i]))
                {
                    continue;
                }

                if (RefineResourcesPath(ref assetNames[i])) refineAssetNames.Add(assetNames[i]);
            }

            if (refineAssetNames.Count > 0) await GroupResource.GetInstance().PreloadAssetAsync<T>(groupId, assetNames, progression, maxRetryCount);
            else await GroupBundle.GetInstance().PreloadAssetAsync<T>(groupId, packageName, assetNames, priority, progression, maxRetryCount);
        }

        /// <summary>
        /// If use prefix "res#" will load from resources else will load from bundle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="groupId"></param>
        /// <param name="assetName"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        public static void PreloadAsset<T>(int groupId, string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName)) GroupResource.GetInstance().PreloadAsset<T>(groupId, new string[] { assetName }, progression, maxRetryCount);
            else GroupBundle.GetInstance().PreloadAsset<T>(groupId, packageName, new string[] { assetName }, progression, maxRetryCount);
        }

        public static void PreloadAsset<T>(int groupId, string packageName, string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            if (RefineResourcesPath(ref assetName)) GroupResource.GetInstance().PreloadAsset<T>(groupId, new string[] { assetName }, progression, maxRetryCount);
            else GroupBundle.GetInstance().PreloadAsset<T>(groupId, packageName, new string[] { assetName }, progression, maxRetryCount);
        }

        public static void PreloadAsset<T>(int groupId, string[] assetNames, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            var packageName = AssetPatcher.GetDefaultPackageName();

            List<string> refineAssetNames = new List<string>();

            for (int i = 0; i < assetNames.Length; i++)
            {
                if (string.IsNullOrEmpty(assetNames[i]))
                {
                    continue;
                }

                if (RefineResourcesPath(ref assetNames[i])) refineAssetNames.Add(assetNames[i]);
            }

            if (refineAssetNames.Count > 0) GroupResource.GetInstance().PreloadAsset<T>(groupId, assetNames, progression, maxRetryCount);
            else GroupBundle.GetInstance().PreloadAsset<T>(groupId, packageName, assetNames, progression, maxRetryCount);
        }

        public static void PreloadAsset<T>(int groupId, string packageName, string[] assetNames, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            List<string> refineAssetNames = new List<string>();

            for (int i = 0; i < assetNames.Length; i++)
            {
                if (string.IsNullOrEmpty(assetNames[i]))
                {
                    continue;
                }

                if (RefineResourcesPath(ref assetNames[i])) refineAssetNames.Add(assetNames[i]);
            }

            if (refineAssetNames.Count > 0) GroupResource.GetInstance().PreloadAsset<T>(groupId, assetNames, progression, maxRetryCount);
            else GroupBundle.GetInstance().PreloadAsset<T>(groupId, packageName, assetNames, progression, maxRetryCount);
        }

        /// <summary>
        /// If use prefix "res#" will load from resources else will load from bundle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="groupId"></param>
        /// <param name="assetName"></param>
        /// <param name="priority"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        public static async UniTask<T> LoadAssetAsync<T>(int groupId, string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName)) return await GroupResource.GetInstance().LoadAssetAsync<T>(groupId, assetName, progression, maxRetryCount);
            else return await GroupBundle.GetInstance().LoadAssetAsync<T>(groupId, packageName, assetName, priority, progression, maxRetryCount);
        }

        public static async UniTask<T> LoadAssetAsync<T>(int groupId, string packageName, string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            if (RefineResourcesPath(ref assetName)) return await GroupResource.GetInstance().LoadAssetAsync<T>(groupId, assetName, progression, maxRetryCount);
            else return await GroupBundle.GetInstance().LoadAssetAsync<T>(groupId, packageName, assetName, priority, progression, maxRetryCount);
        }

        /// <summary>
        /// If use prefix "res#" will load from resources else will load from bundle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="groupId"></param>
        /// <param name="assetName"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        public static T LoadAsset<T>(int groupId, string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName)) return GroupResource.GetInstance().LoadAsset<T>(groupId, assetName, progression, maxRetryCount);
            else return GroupBundle.GetInstance().LoadAsset<T>(groupId, packageName, assetName, progression, maxRetryCount);
        }

        public static T LoadAsset<T>(int groupId, string packageName, string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            if (RefineResourcesPath(ref assetName)) return GroupResource.GetInstance().LoadAsset<T>(groupId, assetName, progression, maxRetryCount);
            else return GroupBundle.GetInstance().LoadAsset<T>(groupId, packageName, assetName, progression, maxRetryCount);
        }

        /// <summary>
        /// If use prefix "res#" will load from resources else will load from bundle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="groupId"></param>
        /// <param name="assetName"></param>
        /// <param name="priority"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        public static async UniTask<T> InstantiateAssetAsync<T>(int groupId, string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName))
            {
                var asset = await GroupResource.GetInstance().LoadAssetAsync<T>(groupId, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset);
                return cloneAsset;
            }
            else
            {
                var asset = await GroupBundle.GetInstance().LoadAssetAsync<T>(groupId, packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset);
                return cloneAsset;
            }
        }

        public static async UniTask<T> InstantiateAssetAsync<T>(int groupId, string packageName, string assetName, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = await GroupResource.GetInstance().LoadAssetAsync<T>(groupId, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset);
                return cloneAsset;
            }
            else
            {
                var asset = await GroupBundle.GetInstance().LoadAssetAsync<T>(groupId, packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset);
                return cloneAsset;
            }
        }

        public static async UniTask<T> InstantiateAssetAsync<T>(int groupId, string assetName, Vector3 position, Quaternion rotation, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName))
            {
                var asset = await GroupResource.GetInstance().LoadAssetAsync<T>(groupId, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
            else
            {
                var asset = await GroupBundle.GetInstance().LoadAssetAsync<T>(groupId, packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
        }

        public static async UniTask<T> InstantiateAssetAsync<T>(int groupId, string packageName, string assetName, Vector3 position, Quaternion rotation, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = await GroupResource.GetInstance().LoadAssetAsync<T>(groupId, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
            else
            {
                var asset = await GroupBundle.GetInstance().LoadAssetAsync<T>(groupId, packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
        }

        public static async UniTask<T> InstantiateAssetAsync<T>(int groupId, string assetName, Vector3 position, Quaternion rotation, Transform parent, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName))
            {
                var asset = await GroupResource.GetInstance().LoadAssetAsync<T>(groupId, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
            else
            {
                var asset = await GroupBundle.GetInstance().LoadAssetAsync<T>(groupId, packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
        }

        public static async UniTask<T> InstantiateAssetAsync<T>(int groupId, string packageName, string assetName, Vector3 position, Quaternion rotation, Transform parent, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = await GroupResource.GetInstance().LoadAssetAsync<T>(groupId, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
            else
            {
                var asset = await GroupBundle.GetInstance().LoadAssetAsync<T>(groupId, packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
        }

        public static async UniTask<T> InstantiateAssetAsync<T>(int groupId, string assetName, Transform parent, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName))
            {
                var asset = await GroupResource.GetInstance().LoadAssetAsync<T>(groupId, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent);
                return cloneAsset;
            }
            else
            {
                var asset = await GroupBundle.GetInstance().LoadAssetAsync<T>(groupId, packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent);
                return cloneAsset;
            }
        }

        public static async UniTask<T> InstantiateAssetAsync<T>(int groupId, string packageName, string assetName, Transform parent, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = await GroupResource.GetInstance().LoadAssetAsync<T>(groupId, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent);
                return cloneAsset;
            }
            else
            {
                var asset = await GroupBundle.GetInstance().LoadAssetAsync<T>(groupId, packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent);
                return cloneAsset;
            }
        }

        public static async UniTask<T> InstantiateAssetAsync<T>(int groupId, string assetName, Transform parent, bool worldPositionStays, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName))
            {
                var asset = await GroupResource.GetInstance().LoadAssetAsync<T>(groupId, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
            else
            {
                var asset = await GroupBundle.GetInstance().LoadAssetAsync<T>(groupId, packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
        }

        public static async UniTask<T> InstantiateAssetAsync<T>(int groupId, string packageName, string assetName, Transform parent, bool worldPositionStays, uint priority = 0, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = await GroupResource.GetInstance().LoadAssetAsync<T>(groupId, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
            else
            {
                var asset = await GroupBundle.GetInstance().LoadAssetAsync<T>(groupId, packageName, assetName, priority, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
        }

        /// <summary>
        /// If use prefix "res#" will load from resources else will load from bundle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="groupId"></param>
        /// <param name="assetName"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        public static T InstantiateAsset<T>(int groupId, string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName))
            {
                var asset = GroupResource.GetInstance().LoadAsset<T>(groupId, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset);
                return cloneAsset;
            }
            else
            {
                var asset = GroupBundle.GetInstance().LoadAsset<T>(groupId, packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset);
                return cloneAsset;
            }
        }

        public static T InstantiateAsset<T>(int groupId, string packageName, string assetName, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = GroupResource.GetInstance().LoadAsset<T>(groupId, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset);
                return cloneAsset;
            }
            else
            {
                var asset = GroupBundle.GetInstance().LoadAsset<T>(groupId, packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset);
                return cloneAsset;
            }
        }

        public static T InstantiateAsset<T>(int groupId, string assetName, Vector3 position, Quaternion rotation, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName))
            {
                var asset = GroupResource.GetInstance().LoadAsset<T>(groupId, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
            else
            {
                var asset = GroupBundle.GetInstance().LoadAsset<T>(groupId, packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
        }

        public static T InstantiateAsset<T>(int groupId, string packageName, string assetName, Vector3 position, Quaternion rotation, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = GroupResource.GetInstance().LoadAsset<T>(groupId, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
            else
            {
                var asset = GroupBundle.GetInstance().LoadAsset<T>(groupId, packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
        }

        public static T InstantiateAsset<T>(int groupId, string assetName, Vector3 position, Quaternion rotation, Transform parent, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName))
            {
                var asset = GroupResource.GetInstance().LoadAsset<T>(groupId, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
            else
            {
                var asset = GroupBundle.GetInstance().LoadAsset<T>(groupId, packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
        }

        public static T InstantiateAsset<T>(int groupId, string packageName, string assetName, Vector3 position, Quaternion rotation, Transform parent, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = GroupResource.GetInstance().LoadAsset<T>(groupId, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
            else
            {
                var asset = GroupBundle.GetInstance().LoadAsset<T>(groupId, packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
        }

        public static T InstantiateAsset<T>(int groupId, string assetName, Transform parent, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName))
            {
                var asset = GroupResource.GetInstance().LoadAsset<T>(groupId, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent);
                return cloneAsset;
            }
            else
            {
                var asset = GroupBundle.GetInstance().LoadAsset<T>(groupId, packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent);
                return cloneAsset;
            }
        }

        public static T InstantiateAsset<T>(int groupId, string packageName, string assetName, Transform parent, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = GroupResource.GetInstance().LoadAsset<T>(groupId, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent);
                return cloneAsset;
            }
            else
            {
                var asset = GroupBundle.GetInstance().LoadAsset<T>(groupId, packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent);
                return cloneAsset;
            }
        }

        public static T InstantiateAsset<T>(int groupId, string assetName, Transform parent, bool worldPositionStays, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            var packageName = AssetPatcher.GetDefaultPackageName();
            if (RefineResourcesPath(ref assetName))
            {
                var asset = GroupResource.GetInstance().LoadAsset<T>(groupId, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
            else
            {
                var asset = GroupBundle.GetInstance().LoadAsset<T>(groupId, packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
        }

        public static T InstantiateAsset<T>(int groupId, string packageName, string assetName, Transform parent, bool worldPositionStays, Progression progression = null, byte maxRetryCount = MAX_RETRY_COUNT) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = GroupResource.GetInstance().LoadAsset<T>(groupId, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
            else
            {
                var asset = GroupBundle.GetInstance().LoadAsset<T>(groupId, packageName, assetName, progression, maxRetryCount);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
        }

        public static void UnloadAsset(int groupId, string assetName, bool forceUnload = false)
        {
            if (RefineResourcesPath(ref assetName)) GroupResource.GetInstance().UnloadAsset(groupId, assetName, forceUnload);
            else if (!AssetPatcher.IsReleased()) GroupBundle.GetInstance().UnloadAsset(groupId, assetName, forceUnload);
        }

        public static void ReleaseResourceAssets(int groupId)
        {
            GroupResource.GetInstance().ReleaseAssets(groupId);
        }

        public static void ReleaseBundleAssets(int groupId)
        {
            GroupBundle.GetInstance().ReleaseAssets(groupId);
        }
        #endregion
        #endregion

        internal static bool RefineResourcesPath(ref string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return false;
            string prefix = "res#";
            if (assetName.Length > prefix.Length)
            {
                if (assetName.Substring(0, prefix.Length).Equals(prefix))
                {
                    var count = assetName.Length - prefix.Length;
                    assetName = assetName.Substring(prefix.Length, count);
                    return true;
                }
            }
            return false;
        }
    }
}