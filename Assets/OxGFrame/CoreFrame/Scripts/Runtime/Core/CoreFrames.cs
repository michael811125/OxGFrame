using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader;
using OxGFrame.AssetLoader.Cacher;
using OxGFrame.CoreFrame.EPFrame;
using OxGFrame.CoreFrame.GSFrame;
using OxGFrame.CoreFrame.UIFrame;
using OxGFrame.CoreFrame.USFrame;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OxGFrame.CoreFrame
{
    public static class CoreFrames
    {
        public static class UIFrame
        {
            public static bool CheckIsShowing(string assetName)
            {
                return UIManager.GetInstance().CheckIsShowing(assetName);
            }

            public static bool CheckIsShowing(UIBase uiBase)
            {
                return UIManager.GetInstance().CheckIsShowing(uiBase);
            }

            public static bool CheckIsHiding(string assetName)
            {
                return UIManager.GetInstance().CheckIsHiding(assetName);
            }

            public static bool CheckIsHiding(UIBase uiBase)
            {
                return UIManager.GetInstance().CheckIsHiding(uiBase);
            }

            public static bool CheckHasAnyHiding()
            {
                return UIManager.GetInstance().CheckHasAnyHiding();
            }

            public static bool CheckHasAnyHiding(int groupId)
            {
                return UIManager.GetInstance().CheckHasAnyHiding(groupId);
            }

            public static void SendRefreshData(object obj = null)
            {
                UIManager.GetInstance().SendRefreshData(obj);
            }

            #region GetComponent
            public static T GetComponent<T>(string assetName) where T : UIBase
            {
                return UIManager.GetInstance().GetFrameComponent<T>(assetName);
            }

            public static T[] GetComponents<T>(string assetName) where T : UIBase
            {
                return UIManager.GetInstance().GetFrameComponents<T>(assetName);
            }
            #endregion

            #region Preload
            /// <summary>
            /// If use prefix "res#" will load from resources else will load from bundle
            /// </summary>
            /// <param name="assetName"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask Preload(string assetName, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await UIManager.GetInstance().Preload(packageName, new string[] { assetName }, progression);
            }

            public static async UniTask Preload(string packageName, string assetName, Progression progression = null)
            {
                await UIManager.GetInstance().Preload(packageName, new string[] { assetName }, progression);
            }

            public static async UniTask Preload(string[] assetNames, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await UIManager.GetInstance().Preload(packageName, assetNames, progression);
            }

            public static async UniTask Preload(string packageName, string[] assetNames, Progression progression = null)
            {
                await UIManager.GetInstance().Preload(packageName, assetNames, progression);
            }
            #endregion

            #region Show
            /// <summary>
            /// If use prefix "res#" will load from resources else will load from bundle
            /// </summary>
            /// <param name="assetName"></param>
            /// <param name="obj"></param>
            /// <param name="loadingUIAssetName"></param>
            /// <param name="progression"></param>
            /// <param name="parent"></param>
            /// <returns></returns>
            public static async UniTask<UIBase> Show(string assetName, object obj = null, string loadingUIAssetName = null, Progression progression = null, Transform parent = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await UIManager.GetInstance().Show(0, packageName, assetName, obj, loadingUIAssetName, progression, parent);
            }

            public static async UniTask<UIBase> Show(string packageName, string assetName, object obj = null, string loadingUIAssetName = null, Progression progression = null, Transform parent = null)
            {
                return await UIManager.GetInstance().Show(0, packageName, assetName, obj, loadingUIAssetName, progression, parent);
            }

            public static async UniTask<UIBase> Show(int groupId, string assetName, object obj = null, string loadingUIAssetName = null, Progression progression = null, Transform parent = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await UIManager.GetInstance().Show(groupId, packageName, assetName, obj, loadingUIAssetName, progression, parent);
            }

            public static async UniTask<UIBase> Show(int groupId, string packageName, string assetName, object obj = null, string loadingUIAssetName = null, Progression progression = null, Transform parent = null)
            {
                return await UIManager.GetInstance().Show(groupId, packageName, assetName, obj, loadingUIAssetName, progression, parent);
            }

            public static async UniTask<T> Show<T>(string assetName, object obj = null, string loadingUIAssetName = null, Progression progression = null, Transform parent = null) where T : UIBase
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await UIManager.GetInstance().Show(0, packageName, assetName, obj, loadingUIAssetName, progression, parent) as T;
            }

            public static async UniTask<T> Show<T>(string packageName, string assetName, object obj = null, string loadingUIAssetName = null, Progression progression = null, Transform parent = null) where T : UIBase
            {
                return await UIManager.GetInstance().Show(0, packageName, assetName, obj, loadingUIAssetName, progression, parent) as T;
            }

            public static async UniTask<T> Show<T>(int groupId, string assetName, object obj = null, string loadingUIAssetName = null, Progression progression = null, Transform parent = null) where T : UIBase
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await UIManager.GetInstance().Show(groupId, packageName, assetName, obj, loadingUIAssetName, progression, parent) as T;
            }

            public static async UniTask<T> Show<T>(int groupId, string packageName, string assetName, object obj = null, string loadingUIAssetName = null, Progression progression = null, Transform parent = null) where T : UIBase
            {
                return await UIManager.GetInstance().Show(groupId, packageName, assetName, obj, loadingUIAssetName, progression, parent) as T;
            }
            #endregion

            #region Close
            public static void Close(string assetName, bool disableDoSub = false, bool forceDestroy = false)
            {
                UIManager.GetInstance().Close(assetName, disableDoSub, forceDestroy);
            }

            public static void CloseAll(bool disableDoSub = false, bool forceDestroy = false, params string[] withoutAssetNames)
            {
                UIManager.GetInstance().CloseAll(disableDoSub, forceDestroy, withoutAssetNames);
            }

            public static void CloseAll(int groupId, bool disableDoSub = false, bool forceDestroy = false, params string[] withoutAssetNames)
            {
                UIManager.GetInstance().CloseAll(groupId, disableDoSub, forceDestroy, withoutAssetNames);
            }
            #endregion

            #region Reveal
            public static void Reveal(string assetName)
            {
                UIManager.GetInstance().Reveal(assetName);
            }

            public static void RevealAll()
            {
                UIManager.GetInstance().RevealAll();
            }

            public static void RevealAll(int groupId)
            {
                UIManager.GetInstance().RevealAll(groupId); ;
            }
            #endregion

            #region Hide
            public static void Hide(string assetName)
            {
                UIManager.GetInstance().Hide(assetName);
            }

            public static void HideAll()
            {
                UIManager.GetInstance().HideAll();
            }

            public static void HideAll(int groupId)
            {
                UIManager.GetInstance().HideAll(groupId);
            }
            #endregion
        }

        public static class GSFrame
        {
            public static bool CheckIsShowing(string assetName)
            {
                return GSManager.GetInstance().CheckIsShowing(assetName);
            }

            public static bool CheckIsShowing(GSBase gsBase)
            {
                return GSManager.GetInstance().CheckIsShowing(gsBase);
            }

            public static bool CheckIsHiding(string assetName)
            {
                return GSManager.GetInstance().CheckIsHiding(assetName);
            }

            public static bool CheckIsHiding(GSBase gsBase)
            {
                return GSManager.GetInstance().CheckIsHiding(gsBase);
            }

            public static bool CheckHasAnyHiding()
            {
                return GSManager.GetInstance().CheckHasAnyHiding();
            }

            public static bool CheckHasAnyHiding(int groupId)
            {
                return GSManager.GetInstance().CheckHasAnyHiding(groupId);
            }

            public static void SendRefreshData(object obj = null)
            {
                GSManager.GetInstance().SendRefreshData(obj);
            }

            #region GetComponent
            public static T GetComponent<T>(string assetName) where T : GSBase
            {
                return GSManager.GetInstance().GetFrameComponent<T>(assetName);
            }

            public static T[] GetComponents<T>(string assetName) where T : GSBase
            {
                return GSManager.GetInstance().GetFrameComponents<T>(assetName);
            }
            #endregion

            #region Preload
            /// <summary>
            /// If use prefix "res#" will load from resources else will load from bundle
            /// </summary>
            /// <param name="assetName"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask Preload(string assetName, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await GSManager.GetInstance().Preload(packageName, new string[] { assetName }, progression);
            }

            public static async UniTask Preload(string packageName, string assetName, Progression progression = null)
            {
                await GSManager.GetInstance().Preload(packageName, new string[] { assetName }, progression);
            }

            public static async UniTask Preload(string[] assetNames, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await GSManager.GetInstance().Preload(packageName, assetNames, progression);
            }

            public static async UniTask Preload(string packageName, string[] assetNames, Progression progression = null)
            {
                await GSManager.GetInstance().Preload(packageName, assetNames, progression);
            }
            #endregion

            #region Show
            /// <summary>
            /// If use prefix "res#" will load from resources else will load from bundle
            /// </summary>
            /// <param name="assetName"></param>
            /// <param name="obj"></param>
            /// <param name="loadingUIAssetName"></param>
            /// <param name="progression"></param>
            /// <param name="parent"></param>
            /// <returns></returns>
            public static async UniTask<GSBase> Show(string assetName, object obj = null, string loadingUIAssetName = null, Progression progression = null, Transform parent = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await GSManager.GetInstance().Show(0, packageName, assetName, obj, loadingUIAssetName, progression, parent);
            }

            public static async UniTask<GSBase> Show(string packageName, string assetName, object obj = null, string loadingUIAssetName = null, Progression progression = null, Transform parent = null)
            {
                return await GSManager.GetInstance().Show(0, packageName, assetName, obj, loadingUIAssetName, progression, parent);
            }

            public static async UniTask<GSBase> Show(int groupId, string assetName, object obj = null, string loadingUIAssetName = null, Progression progression = null, Transform parent = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await GSManager.GetInstance().Show(groupId, packageName, assetName, obj, loadingUIAssetName, progression, parent);
            }

            public static async UniTask<GSBase> Show(int groupId, string packageName, string assetName, object obj = null, string loadingUIAssetName = null, Progression progression = null, Transform parent = null)
            {
                return await GSManager.GetInstance().Show(groupId, packageName, assetName, obj, loadingUIAssetName, progression, parent);
            }

            public static async UniTask<T> Show<T>(string assetName, object obj = null, string loadingUIAssetName = null, Progression progression = null, Transform parent = null) where T : GSBase
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await GSManager.GetInstance().Show(0, packageName, assetName, obj, loadingUIAssetName, progression, parent) as T;
            }

            public static async UniTask<T> Show<T>(string packageName, string assetName, object obj = null, string loadingUIAssetName = null, Progression progression = null, Transform parent = null) where T : GSBase
            {
                return await GSManager.GetInstance().Show(0, packageName, assetName, obj, loadingUIAssetName, progression, parent) as T;
            }

            public static async UniTask<T> Show<T>(int groupId, string assetName, object obj = null, string loadingUIAssetName = null, Progression progression = null, Transform parent = null) where T : GSBase
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await GSManager.GetInstance().Show(groupId, packageName, assetName, obj, loadingUIAssetName, progression, parent) as T;
            }

            public static async UniTask<T> Show<T>(int groupId, string packageName, string assetName, object obj = null, string loadingUIAssetName = null, Progression progression = null, Transform parent = null) where T : GSBase
            {
                return await GSManager.GetInstance().Show(groupId, packageName, assetName, obj, loadingUIAssetName, progression, parent) as T;
            }
            #endregion

            #region Close
            public static void Close(string assetName, bool disableDoSub = false, bool forceDestroy = false)
            {
                GSManager.GetInstance().Close(assetName, disableDoSub, forceDestroy);
            }

            public static void CloseAll(bool disableDoSub = false, bool forceDestroy = false, params string[] withoutAssetNames)
            {
                GSManager.GetInstance().CloseAll(disableDoSub, forceDestroy, withoutAssetNames);
            }

            public static void CloseAll(int groupId, bool disableDoSub = false, bool forceDestroy = false, params string[] withoutAssetNames)
            {
                GSManager.GetInstance().CloseAll(groupId, disableDoSub, forceDestroy, withoutAssetNames);
            }
            #endregion

            #region Reveal
            public static void Reveal(string assetName)
            {
                GSManager.GetInstance().Reveal(assetName);
            }

            public static void RevealAll()
            {
                GSManager.GetInstance().RevealAll();
            }

            public static void RevealAll(int groupId)
            {
                GSManager.GetInstance().RevealAll(groupId); ;
            }
            #endregion

            #region Hide
            public static void Hide(string assetName)
            {
                GSManager.GetInstance().Hide(assetName);
            }

            public static void HideAll()
            {
                GSManager.GetInstance().HideAll();
            }

            public static void HideAll(int groupId)
            {
                GSManager.GetInstance().HideAll(groupId);
            }
            #endregion
        }

        public static class USFrame
        {
            public static int SceneCount()
            {
                return USManager.sceneCount;
            }

            public static Scene GetSceneAt(int index)
            {
                return USManager.GetInstance().GetSceneAt(index);
            }

            public static Scene GetSceneByName(string sceneName)
            {
                return USManager.GetInstance().GetSceneByName(sceneName);
            }

            public static Scene GetSceneByBuildIndex(int buildIndex)
            {
                return USManager.GetInstance().GetSceneByBuildIndex(buildIndex);
            }

            public static Scene[] GetAllScene(params string[] sceneNames)
            {
                return USManager.GetInstance().GetAllScene(sceneNames);
            }

            public static Scene[] GetAllScene(params int[] buildIndexes)
            {
                return USManager.GetInstance().GetAllScene(buildIndexes);
            }

            /// <summary>
            /// (Single) If use prefix "build#" will load from build and return null else will load from bundle and return BundlePack
            /// </summary>
            /// <param name="sceneName"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask<BundlePack> LoadSingleSceneAsync(string sceneName, bool activateOnLoad = true, int priority = 100, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                if (RefineBuildScenePath(ref sceneName))
                {
                    await USManager.GetInstance().LoadFromBuildAsync(sceneName, LoadSceneMode.Single, progression);
                    return null;
                }
                else return await USManager.GetInstance().LoadFromBundleAsync(packageName, sceneName, LoadSceneMode.Single, activateOnLoad, priority, progression);
            }

            public static async UniTask<BundlePack> LoadSingleSceneAsync(string packageName, string sceneName, bool activateOnLoad = true, int priority = 100, Progression progression = null)
            {
                if (RefineBuildScenePath(ref sceneName))
                {
                    await USManager.GetInstance().LoadFromBuildAsync(sceneName, LoadSceneMode.Single, progression);
                    return null;
                }
                else return await USManager.GetInstance().LoadFromBundleAsync(packageName, sceneName, LoadSceneMode.Single, activateOnLoad, priority, progression);
            }

            /// <summary>
            /// (Additive) If use prefix "build#" will load from build and return null else will load from bundle and return BundlePack
            /// </summary>
            /// <param name="sceneName"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask<BundlePack> LoadAdditiveSceneAsync(string sceneName, bool activateOnLoad = true, int priority = 100, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                if (RefineBuildScenePath(ref sceneName))
                {
                    await USManager.GetInstance().LoadFromBuildAsync(sceneName, LoadSceneMode.Additive, progression);
                    return null;
                }
                else return await USManager.GetInstance().LoadFromBundleAsync(packageName, sceneName, LoadSceneMode.Additive, activateOnLoad, priority, progression);
            }

            public static async UniTask<BundlePack> LoadAdditiveSceneAsync(string packageName, string sceneName, bool activateOnLoad = true, int priority = 100, Progression progression = null)
            {
                if (RefineBuildScenePath(ref sceneName))
                {
                    await USManager.GetInstance().LoadFromBuildAsync(sceneName, LoadSceneMode.Additive, progression);
                    return null;
                }
                else return await USManager.GetInstance().LoadFromBundleAsync(packageName, sceneName, LoadSceneMode.Additive, activateOnLoad, priority, progression);
            }

            /// <summary>
            /// If use prefix "build#" will load from build and return null else will load from bundle and return BundlePack
            /// </summary>
            /// <param name="sceneName"></param>
            /// <param name="loadSceneMode"></param>
            /// <param name="activateOnLoad"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask<BundlePack> LoadSceneAsync(string sceneName, LoadSceneMode loadSceneMode, bool activateOnLoad = true, int priority = 100, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                if (RefineBuildScenePath(ref sceneName))
                {
                    await USManager.GetInstance().LoadFromBuildAsync(sceneName, loadSceneMode, progression);
                    return null;
                }
                else return await USManager.GetInstance().LoadFromBundleAsync(packageName, sceneName, loadSceneMode, activateOnLoad, priority, progression);
            }

            public static async UniTask<BundlePack> LoadSceneAsync(string packageName, string sceneName, LoadSceneMode loadSceneMode, bool activateOnLoad = true, int priority = 100, Progression progression = null)
            {
                if (RefineBuildScenePath(ref sceneName))
                {
                    await USManager.GetInstance().LoadFromBuildAsync(sceneName, loadSceneMode, progression);
                    return null;
                }
                else return await USManager.GetInstance().LoadFromBundleAsync(packageName, sceneName, loadSceneMode, activateOnLoad, priority, progression);
            }

            /// <summary>
            /// only load from build
            /// </summary>
            /// <param name="buildIndex"></param>
            /// <param name="loadSceneMode"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask LoadSceneAsync(int buildIndex, LoadSceneMode loadSceneMode = LoadSceneMode.Single, Progression progression = null)
            {
                await USManager.GetInstance().LoadFromBuildAsync(buildIndex, loadSceneMode, progression);
            }

            /// <summary>
            /// If use prefix "build#" will unload from build else will unload from bundle
            /// </summary>
            /// <param name="recursively"></param>
            /// <param name="sceneNames"></param>
            public static void Unload(bool recursively, params string[] sceneNames)
            {
                foreach (var sceneName in sceneNames)
                {
                    var refineSceneName = sceneName;
                    if (RefineBuildScenePath(ref refineSceneName)) USManager.GetInstance().UnloadFromBuild(recursively, refineSceneName);
                    else USManager.GetInstance().UnloadFromBundle(recursively, refineSceneName);
                }
            }

            /// <summary>
            /// only unload from build
            /// </summary>
            /// <param name="recursively"></param>
            /// <param name="buildIndexes"></param>
            public static void Unload(bool recursively, params int[] buildIndexes)
            {
                USManager.GetInstance().UnloadFromBuild(recursively, buildIndexes);
            }

            internal static bool RefineBuildScenePath(ref string sceneName)
            {
                string prefix = "build#";

                if (sceneName.IndexOf(prefix) != -1)
                {
                    sceneName = sceneName.Replace(prefix, string.Empty);
                    return true;
                }

                return false;
            }
        }

        public static class EPFrame
        {
            /// <summary>
            /// If use prefix "res#" will load from resources else will load from bundle
            /// </summary>
            /// <param name="assetName"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask PreloadAsync(string assetName, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await EPManager.GetInstance().PreloadAsync(packageName, new string[] { assetName }, progression);
            }

            public static async UniTask PreloadAsync(string packageName, string assetName, Progression progression = null)
            {
                await EPManager.GetInstance().PreloadAsync(packageName, new string[] { assetName }, progression);
            }

            public static async UniTask PreloadAsync(string[] assetNames, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await EPManager.GetInstance().PreloadAsync(packageName, assetNames, progression);
            }

            public static async UniTask PreloadAsync(string packageName, string[] assetNames, Progression progression = null)
            {
                await EPManager.GetInstance().PreloadAsync(packageName, assetNames, progression);
            }

            /// <summary>
            /// If use prefix "res#" will load from resources else will load from bundle
            /// </summary>
            /// <param name="assetName"></param>
            /// <param name="progression"></param>
            public static void Preload(string assetName, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                EPManager.GetInstance().Preload(packageName, new string[] { assetName }, progression);
            }

            public static void Preload(string packageName, string assetName, Progression progression = null)
            {
                EPManager.GetInstance().Preload(packageName, new string[] { assetName }, progression);
            }

            public static void Preload(string[] assetNames, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                EPManager.GetInstance().Preload(packageName, assetNames, progression);
            }

            public static void Preload(string packageName, string[] assetNames, Progression progression = null)
            {
                EPManager.GetInstance().Preload(packageName, assetNames, progression);
            }

            /// <summary>
            /// If use prefix "res#" will load from resources else will load from bundle
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="assetName"></param>
            /// <param name="parent"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask<T> LoadWithCloneAsync<T>(string assetName, Transform parent = null, Progression progression = null) where T : EPBase, new()
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await EPManager.GetInstance().LoadWithCloneAsync<T>(packageName, assetName, parent, progression);
            }

            public static async UniTask<T> LoadWithCloneAsync<T>(string packageName, string assetName, Transform parent = null, Progression progression = null) where T : EPBase, new()
            {
                return await EPManager.GetInstance().LoadWithCloneAsync<T>(packageName, assetName, parent, progression);
            }

            public static async UniTask<T> LoadWithCloneAsync<T>(string assetName, Transform parent, bool worldPositionStays, Progression progression = null) where T : EPBase, new()
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await EPManager.GetInstance().LoadWithCloneAsync<T>(packageName, assetName, parent, worldPositionStays, progression);
            }

            public static async UniTask<T> LoadWithCloneAsync<T>(string packageName, string assetName, Transform parent, bool worldPositionStays, Progression progression = null) where T : EPBase, new()
            {
                return await EPManager.GetInstance().LoadWithCloneAsync<T>(packageName, assetName, parent, worldPositionStays, progression);
            }

            public static async UniTask<T> LoadWithCloneAsync<T>(string assetName, Vector3 position, Quaternion rotation, Transform parent = null, Vector3? scale = null, Progression progression = null) where T : EPBase, new()
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await EPManager.GetInstance().LoadWithCloneAsync<T>(packageName, assetName, position, rotation, parent, scale, progression);
            }

            public static async UniTask<T> LoadWithCloneAsync<T>(string packageName, string assetName, Vector3 position, Quaternion rotation, Transform parent = null, Vector3? scale = null, Progression progression = null) where T : EPBase, new()
            {
                return await EPManager.GetInstance().LoadWithCloneAsync<T>(packageName, assetName, position, rotation, parent, scale, progression);
            }

            /// <summary>
            /// If use prefix "res#" will load from resources else will load from bundle
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="assetName"></param>
            /// <param name="parent"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static T LoadWithClone<T>(string assetName, Transform parent = null, Progression progression = null) where T : EPBase, new()
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return EPManager.GetInstance().LoadWithClone<T>(packageName, assetName, parent, progression);
            }

            public static T LoadWithClone<T>(string packageName, string assetName, Transform parent = null, Progression progression = null) where T : EPBase, new()
            {
                return EPManager.GetInstance().LoadWithClone<T>(packageName, assetName, parent, progression);
            }

            public static T LoadWithClone<T>(string assetName, Transform parent, bool worldPositionStays, Progression progression = null) where T : EPBase, new()
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return EPManager.GetInstance().LoadWithClone<T>(packageName, assetName, parent, worldPositionStays, progression);
            }

            public static T LoadWithClone<T>(string packageName, string assetName, Transform parent, bool worldPositionStays, Progression progression = null) where T : EPBase, new()
            {
                return EPManager.GetInstance().LoadWithClone<T>(packageName, assetName, parent, worldPositionStays, progression);
            }

            public static T LoadWithClone<T>(string assetName, Vector3 position, Quaternion rotation, Transform parent = null, Vector3? scale = null, Progression progression = null) where T : EPBase, new()
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return EPManager.GetInstance().LoadWithClone<T>(packageName, assetName, position, rotation, parent, scale, progression);
            }

            public static T LoadWithClone<T>(string packageName, string assetName, Vector3 position, Quaternion rotation, Transform parent = null, Vector3? scale = null, Progression progression = null) where T : EPBase, new()
            {
                return EPManager.GetInstance().LoadWithClone<T>(packageName, assetName, position, rotation, parent, scale, progression);
            }
        }
    }
}
