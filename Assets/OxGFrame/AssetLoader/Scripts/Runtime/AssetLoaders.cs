using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader.Cacher;
using OxGFrame.AssetLoader.GroupCahcer;
using OxGFrame.AssetLoader.GroupChacer;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OxGFrame.AssetLoader
{
    public static class AssetLoaders
    {
        #region Scene
        public static async UniTask<BundlePack> LoadSceneAsync(string assetName, LoadSceneMode loadSceneMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100, Progression progression = null)
        {
            return await CacheBundle.GetInstance().LoadSceneAsync(assetName, loadSceneMode, activateOnLoad, priority, progression);
        }

        public static async UniTask<BundlePack> LoadSingleSceneAsync(string assetName, bool activateOnLoad = true, int priority = 100, Progression progression = null)
        {
            return await CacheBundle.GetInstance().LoadSceneAsync(assetName, LoadSceneMode.Single, activateOnLoad, priority, progression);
        }

        public static async UniTask<BundlePack> LoadAddtiveSceneAsync(string assetName, bool activateOnLoad = true, int priority = 100, Progression progression = null)
        {
            return await CacheBundle.GetInstance().LoadSceneAsync(assetName, LoadSceneMode.Additive, activateOnLoad, priority, progression);
        }

        public static void UnloadScene(string assetName, bool recursively = false)
        {
            CacheBundle.GetInstance().UnloadScene(assetName, recursively);
        }
        #endregion

        #region Cacher
        public static bool HasInCache(string assetName)
        {
            if (RefineResourcesPath(ref assetName)) return CacheResource.GetInstance().HasInCache(assetName);
            else return CacheBundle.GetInstance().HasInCache(assetName);

        }

        public static T GetFromCache<T>(string assetName) where T : AssetObject
        {
            if (RefineResourcesPath(ref assetName)) return CacheResource.GetInstance().GetFromCache(assetName) as T;
            else return CacheBundle.GetInstance().GetFromCache(assetName) as T;
        }

        #region RawFile
        public static async UniTask PreloadRawFileAsync(string assetName, Progression progression = null)
        {
            if (RefineResourcesPath(ref assetName)) Debug.Log("<color=#ff0000>【Error】Only Bundle Type</color>");
            else await CacheBundle.GetInstance().PreloadRawFileAsync(assetName, progression);
        }

        public static async UniTask PreloadRawFileAsync(string[] assetNames, Progression progression = null)
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

            if (refineAssetNames.Count > 0) Debug.Log("<color=#ff0000>【Error】Only Bundle Type</color>");
            else await CacheBundle.GetInstance().PreloadRawFileAsync(assetNames, progression);
        }

        public static void PreloadRawFile(string assetName, Progression progression = null)
        {
            if (RefineResourcesPath(ref assetName)) Debug.Log("<color=#ff0000>【Error】Only Bundle Type</color>");
            else CacheBundle.GetInstance().PreloadRawFile(assetName, progression);
        }

        public static void PreloadRawFile(string[] assetNames, Progression progression = null)
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

            if (refineAssetNames.Count > 0) Debug.Log("<color=#ff0000>【Error】Only Bundle Type</color>");
            else CacheBundle.GetInstance().PreloadRawFile(assetNames, progression);
        }

        public static async UniTask<T> LoadRawFileAsync<T>(string assetName, Progression progression = null)
        {
            if (RefineResourcesPath(ref assetName))
            {
                Debug.Log("<color=#ff0000>【Error】Only Bundle Type</color>");
                return default;
            }
            else return await CacheBundle.GetInstance().LoadRawFileAsync<T>(assetName, progression);

        }

        public static T LoadRawFile<T>(string assetName, Progression progression = null)
        {
            if (RefineResourcesPath(ref assetName))
            {
                Debug.Log("<color=#ff0000>【Error】Only Bundle Type</color>");
                return default;
            }
            else return CacheBundle.GetInstance().LoadRawFile<T>(assetName, progression);

        }

        public static void UnloadRawFile(string assetName, bool forceUnload = false)
        {
            if (RefineResourcesPath(ref assetName)) Debug.Log("<color=#ff0000>【Error】Only Bundle Type</color>");
            else CacheBundle.GetInstance().UnloadRawFile(assetName, forceUnload);
        }

        public static void ReleaseBundleRawFiles()
        {
            CacheBundle.GetInstance().ReleaseRawFiles();
        }
        #endregion

        #region Asset
        public static async UniTask PreloadAssetAsync(string assetName, Progression progression = null)
        {
            if (RefineResourcesPath(ref assetName)) await CacheResource.GetInstance().PreloadAssetAsync(assetName, progression);
            else await CacheBundle.GetInstance().PreloadAssetAsync(assetName, progression);
        }

        public static async UniTask PreloadAssetAsync(string[] assetNames, Progression progression = null)
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

            if (refineAssetNames.Count > 0) await CacheResource.GetInstance().PreloadAssetAsync(assetNames, progression);
            else await CacheBundle.GetInstance().PreloadAssetAsync(assetNames, progression);
        }

        public static void PreloadAsset(string assetName, Progression progression = null)
        {
            if (RefineResourcesPath(ref assetName)) CacheResource.GetInstance().PreloadAsset(assetName, progression);
            else CacheBundle.GetInstance().PreloadAsset(assetName, progression);
        }

        public static void PreloadAsset(string[] assetNames, Progression progression = null)
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

            if (refineAssetNames.Count > 0) CacheResource.GetInstance().PreloadAsset(assetNames, progression);
            else CacheBundle.GetInstance().PreloadAsset(assetNames, progression);
        }

        public static async UniTask<T> LoadAssetAsync<T>(string assetName, Progression progression = null) where T : Object
        {
            if (RefineResourcesPath(ref assetName)) return await CacheResource.GetInstance().LoadAssetAsync<T>(assetName, progression);
            else return await CacheBundle.GetInstance().LoadAssetAsync<T>(assetName, progression);
        }

        public static T LoadAsset<T>(string assetName, Progression progression = null) where T : Object
        {
            if (RefineResourcesPath(ref assetName)) return CacheResource.GetInstance().LoadAsset<T>(assetName, progression);
            else return CacheBundle.GetInstance().LoadAsset<T>(assetName, progression);
        }

        public static async UniTask<T> InstantiateAssetAsync<T>(string assetName, Progression progression = null) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = await CacheResource.GetInstance().LoadAssetAsync<T>(assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset);
                return cloneAsset;
            }
            else
            {
                var asset = await CacheBundle.GetInstance().LoadAssetAsync<T>(assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset);
                return cloneAsset;
            }
        }

        public static async UniTask<T> InstantiateAssetAsync<T>(string assetName, Vector3 position, Quaternion rotation, Progression progression = null) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = await CacheResource.GetInstance().LoadAssetAsync<T>(assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
            else
            {
                var asset = await CacheBundle.GetInstance().LoadAssetAsync<T>(assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
        }

        public static async UniTask<T> InstantiateAssetAsync<T>(string assetName, Vector3 position, Quaternion rotation, Transform parent, Progression progression = null) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = await CacheResource.GetInstance().LoadAssetAsync<T>(assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
            else
            {
                var asset = await CacheBundle.GetInstance().LoadAssetAsync<T>(assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
        }

        public static async UniTask<T> InstantiateAssetAsync<T>(string assetName, Transform parent, Progression progression = null) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = await CacheResource.GetInstance().LoadAssetAsync<T>(assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent);
                return cloneAsset;
            }
            else
            {
                var asset = await CacheBundle.GetInstance().LoadAssetAsync<T>(assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent);
                return cloneAsset;
            }
        }

        public static async UniTask<T> InstantiateAssetAsync<T>(string assetName, Transform parent, bool worldPositionStays, Progression progression = null) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = await CacheResource.GetInstance().LoadAssetAsync<T>(assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
            else
            {
                var asset = await CacheBundle.GetInstance().LoadAssetAsync<T>(assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
        }

        public static T InstantiateAsset<T>(string assetName, Progression progression = null) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = CacheResource.GetInstance().LoadAsset<T>(assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset);
                return cloneAsset;
            }
            else
            {
                var asset = CacheBundle.GetInstance().LoadAsset<T>(assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset);
                return cloneAsset;
            }
        }

        public static T InstantiateAsset<T>(string assetName, Vector3 position, Quaternion rotation, Progression progression = null) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = CacheResource.GetInstance().LoadAsset<T>(assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
            else
            {
                var asset = CacheBundle.GetInstance().LoadAsset<T>(assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
        }

        public static T InstantiateAsset<T>(string assetName, Vector3 position, Quaternion rotation, Transform parent, Progression progression = null) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = CacheResource.GetInstance().LoadAsset<T>(assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
            else
            {
                var asset = CacheBundle.GetInstance().LoadAsset<T>(assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
        }

        public static T InstantiateAsset<T>(string assetName, Transform parent, Progression progression = null) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = CacheResource.GetInstance().LoadAsset<T>(assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent);
                return cloneAsset;
            }
            else
            {
                var asset = CacheBundle.GetInstance().LoadAsset<T>(assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent);
                return cloneAsset;
            }
        }

        public static T InstantiateAsset<T>(string assetName, Transform parent, bool worldPositionStays, Progression progression = null) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = CacheResource.GetInstance().LoadAsset<T>(assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
            else
            {
                var asset = CacheBundle.GetInstance().LoadAsset<T>(assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
        }

        public static void UnloadAsset(string assetName, bool forceUnload = false)
        {
            if (RefineResourcesPath(ref assetName)) CacheResource.GetInstance().UnloadAsset(assetName, forceUnload);
            else CacheBundle.GetInstance().UnloadAsset(assetName, forceUnload);
        }

        public static void ReleaseResourceAssets()
        {
            CacheResource.GetInstance().ReleaseAssets();
        }

        public static void ReleaseBundleAssets()
        {
            CacheBundle.GetInstance().ReleaseAssets();
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
        public static async UniTask PreloadRawFileAsync(int groupId, string assetName, Progression progression = null)
        {
            if (RefineResourcesPath(ref assetName)) Debug.Log("<color=#ff0000>【Error】Only Bundle Type</color>");
            else await GroupBundle.GetInstance().PreloadRawFileAsync(groupId, assetName, progression);
        }

        public static async UniTask PreloadRawFileAsync(int groupId, string[] assetNames, Progression progression = null)
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

            if (refineAssetNames.Count > 0) Debug.Log("<color=#ff0000>【Error】Only Bundle Type</color>");
            else await GroupBundle.GetInstance().PreloadRawFileAsync(groupId, assetNames, progression);
        }

        public static void PreloadRawFile(int groupId, string assetName, Progression progression = null)
        {
            if (RefineResourcesPath(ref assetName)) Debug.Log("<color=#ff0000>【Error】Only Bundle Type</color>");
            else GroupBundle.GetInstance().PreloadRawFile(groupId, assetName, progression);
        }

        public static void PreloadRawFile(int groupId, string[] assetNames, Progression progression = null)
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

            if (refineAssetNames.Count > 0) Debug.Log("<color=#ff0000>【Error】Only Bundle Type</color>");
            else GroupBundle.GetInstance().PreloadRawFile(groupId, assetNames, progression);
        }

        public static async UniTask<T> LoadRawFileAsync<T>(int groupId, string assetName, Progression progression = null)
        {
            if (RefineResourcesPath(ref assetName))
            {
                Debug.Log("<color=#ff0000>【Error】Only Bundle Type</color>");
                return default;
            }
            else return await GroupBundle.GetInstance().LoadRawFileAsync<T>(groupId, assetName, progression);
        }

        public static T LoadRawFile<T>(int groupId, string assetName, Progression progression = null)
        {
            if (RefineResourcesPath(ref assetName))
            {
                Debug.Log("<color=#ff0000>【Error】Only Bundle Type</color>");
                return default;
            }
            else return GroupBundle.GetInstance().LoadRawFile<T>(groupId, assetName, progression);
        }

        public static void UnloadRawFile(int groupId, string assetName, bool forceUnload = false)
        {
            if (RefineResourcesPath(ref assetName)) Debug.Log("<color=#ff0000>【Error】Only Bundle Type</color>");
            else GroupBundle.GetInstance().UnloadRawFile(groupId, assetName, forceUnload);
        }

        public static void ReleaseBundleRawFiles(int groupId)
        {
            GroupBundle.GetInstance().ReleaseRawFiles(groupId);
        }
        #endregion

        #region Asset
        public static async UniTask PreloadAssetAsync(int groupId, string assetName, Progression progression = null)
        {
            if (RefineResourcesPath(ref assetName)) await GroupResource.GetInstance().PreloadAssetAsync(groupId, assetName, progression);
            else await GroupBundle.GetInstance().PreloadAssetAsync(groupId, assetName, progression);
        }

        public static async UniTask PreloadAssetAsync(int groupId, string[] assetNames, Progression progression = null)
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

            if (refineAssetNames.Count > 0) await GroupResource.GetInstance().PreloadAssetAsync(groupId, assetNames, progression);
            else await GroupBundle.GetInstance().PreloadAssetAsync(groupId, assetNames, progression);
        }

        public static void PreloadAsset(int groupId, string assetName, Progression progression = null)
        {
            if (RefineResourcesPath(ref assetName)) GroupResource.GetInstance().PreloadAsset(groupId, assetName, progression);
            else GroupBundle.GetInstance().PreloadAsset(groupId, assetName, progression);
        }

        public static void PreloadAsset(int groupId, string[] assetNames, Progression progression = null)
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

            if (refineAssetNames.Count > 0) GroupResource.GetInstance().PreloadAsset(groupId, assetNames, progression);
            else GroupBundle.GetInstance().PreloadAsset(groupId, assetNames, progression);
        }

        public static async UniTask<T> LoadAssetAsync<T>(int groupId, string assetName, Progression progression = null) where T : Object
        {
            if (RefineResourcesPath(ref assetName)) return await GroupResource.GetInstance().LoadAssetAsync<T>(groupId, assetName, progression);
            else return await GroupBundle.GetInstance().LoadAssetAsync<T>(groupId, assetName, progression);
        }

        public static T LoadAsset<T>(int groupId, string assetName, Progression progression = null) where T : Object
        {
            if (RefineResourcesPath(ref assetName)) return GroupResource.GetInstance().LoadAsset<T>(groupId, assetName, progression);
            else return GroupBundle.GetInstance().LoadAsset<T>(groupId, assetName, progression);
        }

        public static async UniTask<T> InstantiateAssetAsync<T>(int groupdId, string assetName, Progression progression = null) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = await GroupResource.GetInstance().LoadAssetAsync<T>(groupdId, assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset);
                return cloneAsset;
            }
            else
            {
                var asset = await GroupBundle.GetInstance().LoadAssetAsync<T>(groupdId, assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset);
                return cloneAsset;
            }
        }

        public static async UniTask<T> InstantiateAssetAsync<T>(int groupdId, string assetName, Vector3 position, Quaternion rotation, Progression progression = null) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = await GroupResource.GetInstance().LoadAssetAsync<T>(groupdId, assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
            else
            {
                var asset = await GroupBundle.GetInstance().LoadAssetAsync<T>(groupdId, assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
        }

        public static async UniTask<T> InstantiateAssetAsync<T>(int groupdId, string assetName, Vector3 position, Quaternion rotation, Transform parent, Progression progression = null) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = await GroupResource.GetInstance().LoadAssetAsync<T>(groupdId, assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
            else
            {
                var asset = await GroupBundle.GetInstance().LoadAssetAsync<T>(groupdId, assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
        }

        public static async UniTask<T> InstantiateAssetAsync<T>(int groupdId, string assetName, Transform parent, Progression progression = null) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = await GroupResource.GetInstance().LoadAssetAsync<T>(groupdId, assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent);
                return cloneAsset;
            }
            else
            {
                var asset = await GroupBundle.GetInstance().LoadAssetAsync<T>(groupdId, assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent);
                return cloneAsset;
            }
        }

        public static async UniTask<T> InstantiateAssetAsync<T>(int groupdId, string assetName, Transform parent, bool worldPositionStays, Progression progression = null) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = await GroupResource.GetInstance().LoadAssetAsync<T>(groupdId, assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
            else
            {
                var asset = await GroupBundle.GetInstance().LoadAssetAsync<T>(groupdId, assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
        }

        public static T InstantiateAsset<T>(int groupdId, string assetName, Progression progression = null) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = GroupResource.GetInstance().LoadAsset<T>(groupdId, assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset);
                return cloneAsset;
            }
            else
            {
                var asset = GroupBundle.GetInstance().LoadAsset<T>(groupdId, assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset);
                return cloneAsset;
            }
        }

        public static T InstantiateAsset<T>(int groupdId, string assetName, Vector3 position, Quaternion rotation, Progression progression = null) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = GroupResource.GetInstance().LoadAsset<T>(groupdId, assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
            else
            {
                var asset = GroupBundle.GetInstance().LoadAsset<T>(groupdId, assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation);
                return cloneAsset;
            }
        }

        public static T InstantiateAsset<T>(int groupdId, string assetName, Vector3 position, Quaternion rotation, Transform parent, Progression progression = null) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = GroupResource.GetInstance().LoadAsset<T>(groupdId, assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
            else
            {
                var asset = GroupBundle.GetInstance().LoadAsset<T>(groupdId, assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, position, rotation, parent);
                return cloneAsset;
            }
        }

        public static T InstantiateAsset<T>(int groupdId, string assetName, Transform parent, Progression progression = null) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = GroupResource.GetInstance().LoadAsset<T>(groupdId, assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent);
                return cloneAsset;
            }
            else
            {
                var asset = GroupBundle.GetInstance().LoadAsset<T>(groupdId, assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent);
                return cloneAsset;
            }
        }

        public static T InstantiateAsset<T>(int groupdId, string assetName, Transform parent, bool worldPositionStays, Progression progression = null) where T : Object
        {
            if (RefineResourcesPath(ref assetName))
            {
                var asset = GroupResource.GetInstance().LoadAsset<T>(groupdId, assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
            else
            {
                var asset = GroupBundle.GetInstance().LoadAsset<T>(groupdId, assetName, progression);
                var cloneAsset = (asset == null) ? null : Object.Instantiate(asset, parent, worldPositionStays);
                return cloneAsset;
            }
        }

        public static void UnloadAsset(int groupId, string assetName, bool forceUnload = false)
        {
            if (RefineResourcesPath(ref assetName)) GroupResource.GetInstance().UnloadAsset(groupId, assetName, forceUnload);
            else GroupBundle.GetInstance().UnloadAsset(groupId, assetName, forceUnload);
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
            string prefix = "res#";

            if (assetName.IndexOf(prefix) != -1)
            {
                assetName = assetName.Replace(prefix, string.Empty);
                return true;
            }

            return false;
        }
    }
}