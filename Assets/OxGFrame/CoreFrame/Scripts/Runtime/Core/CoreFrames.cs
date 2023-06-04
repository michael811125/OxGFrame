using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader;
using OxGFrame.CoreFrame.CPFrame;
using OxGFrame.CoreFrame.SRFrame;
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
            public static void InitInstance()
            {
                UIManager.GetInstance();
            }

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

            public static void SendRefreshData(object data = null)
            {
                UIManager.GetInstance().SendRefreshData(data);
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
            /// <param name="data"></param>
            /// <param name="awaitingUIAssetName"></param>
            /// <param name="progression"></param>
            /// <param name="parent"></param>
            /// <returns></returns>
            public static async UniTask<UIBase> Show(string assetName, object data = null, string awaitingUIAssetName = null, Progression progression = null, Transform parent = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await UIManager.GetInstance().Show(0, packageName, assetName, data, awaitingUIAssetName, progression, parent);
            }

            public static async UniTask<UIBase> Show(string packageName, string assetName, object data = null, string awaitingUIAssetName = null, Progression progression = null, Transform parent = null)
            {
                return await UIManager.GetInstance().Show(0, packageName, assetName, data, awaitingUIAssetName, progression, parent);
            }

            public static async UniTask<UIBase> Show(int groupId, string assetName, object data = null, string awaitingUIAssetName = null, Progression progression = null, Transform parent = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await UIManager.GetInstance().Show(groupId, packageName, assetName, data, awaitingUIAssetName, progression, parent);
            }

            public static async UniTask<UIBase> Show(int groupId, string packageName, string assetName, object data = null, string awaitingUIAssetName = null, Progression progression = null, Transform parent = null)
            {
                return await UIManager.GetInstance().Show(groupId, packageName, assetName, data, awaitingUIAssetName, progression, parent);
            }

            public static async UniTask<T> Show<T>(string assetName, object data = null, string awaitingUIAssetName = null, Progression progression = null, Transform parent = null) where T : UIBase
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await UIManager.GetInstance().Show(0, packageName, assetName, data, awaitingUIAssetName, progression, parent) as T;
            }

            public static async UniTask<T> Show<T>(string packageName, string assetName, object data = null, string awaitingUIAssetName = null, Progression progression = null, Transform parent = null) where T : UIBase
            {
                return await UIManager.GetInstance().Show(0, packageName, assetName, data, awaitingUIAssetName, progression, parent) as T;
            }

            public static async UniTask<T> Show<T>(int groupId, string assetName, object data = null, string awaitingUIAssetName = null, Progression progression = null, Transform parent = null) where T : UIBase
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await UIManager.GetInstance().Show(groupId, packageName, assetName, data, awaitingUIAssetName, progression, parent) as T;
            }

            public static async UniTask<T> Show<T>(int groupId, string packageName, string assetName, object data = null, string awaitingUIAssetName = null, Progression progression = null, Transform parent = null) where T : UIBase
            {
                return await UIManager.GetInstance().Show(groupId, packageName, assetName, data, awaitingUIAssetName, progression, parent) as T;
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
                UIManager.GetInstance().RevealAll(groupId);
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

        public static class SRFrame
        {
            public static void InitInstance()
            {
                SRManager.GetInstance();
            }

            public static bool CheckIsShowing(string assetName)
            {
                return SRManager.GetInstance().CheckIsShowing(assetName);
            }

            public static bool CheckIsShowing(SRBase srBase)
            {
                return SRManager.GetInstance().CheckIsShowing(srBase);
            }

            public static bool CheckIsHiding(string assetName)
            {
                return SRManager.GetInstance().CheckIsHiding(assetName);
            }

            public static bool CheckIsHiding(SRBase srBase)
            {
                return SRManager.GetInstance().CheckIsHiding(srBase);
            }

            public static bool CheckHasAnyHiding()
            {
                return SRManager.GetInstance().CheckHasAnyHiding();
            }

            public static bool CheckHasAnyHiding(int groupId)
            {
                return SRManager.GetInstance().CheckHasAnyHiding(groupId);
            }

            public static void SendRefreshData(object data = null)
            {
                SRManager.GetInstance().SendRefreshData(data);
            }

            #region GetComponent
            public static T GetComponent<T>(string assetName) where T : SRBase
            {
                return SRManager.GetInstance().GetFrameComponent<T>(assetName);
            }

            public static T[] GetComponents<T>(string assetName) where T : SRBase
            {
                return SRManager.GetInstance().GetFrameComponents<T>(assetName);
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
                await SRManager.GetInstance().Preload(packageName, new string[] { assetName }, progression);
            }

            public static async UniTask Preload(string packageName, string assetName, Progression progression = null)
            {
                await SRManager.GetInstance().Preload(packageName, new string[] { assetName }, progression);
            }

            public static async UniTask Preload(string[] assetNames, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await SRManager.GetInstance().Preload(packageName, assetNames, progression);
            }

            public static async UniTask Preload(string packageName, string[] assetNames, Progression progression = null)
            {
                await SRManager.GetInstance().Preload(packageName, assetNames, progression);
            }
            #endregion

            #region Show
            /// <summary>
            /// If use prefix "res#" will load from resources else will load from bundle
            /// </summary>
            /// <param name="assetName"></param>
            /// <param name="data"></param>
            /// <param name="awaitingUIAssetName"></param>
            /// <param name="progression"></param>
            /// <param name="parent"></param>
            /// <returns></returns>
            public static async UniTask<SRBase> Show(string assetName, object data = null, string awaitingUIAssetName = null, Progression progression = null, Transform parent = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await SRManager.GetInstance().Show(0, packageName, assetName, data, awaitingUIAssetName, progression, parent);
            }

            public static async UniTask<SRBase> Show(string packageName, string assetName, object data = null, string awaitingUIAssetName = null, Progression progression = null, Transform parent = null)
            {
                return await SRManager.GetInstance().Show(0, packageName, assetName, data, awaitingUIAssetName, progression, parent);
            }

            public static async UniTask<SRBase> Show(int groupId, string assetName, object data = null, string awaitingUIAssetName = null, Progression progression = null, Transform parent = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await SRManager.GetInstance().Show(groupId, packageName, assetName, data, awaitingUIAssetName, progression, parent);
            }

            public static async UniTask<SRBase> Show(int groupId, string packageName, string assetName, object data = null, string awaitingUIAssetName = null, Progression progression = null, Transform parent = null)
            {
                return await SRManager.GetInstance().Show(groupId, packageName, assetName, data, awaitingUIAssetName, progression, parent);
            }

            public static async UniTask<T> Show<T>(string assetName, object data = null, string awaitingUIAssetName = null, Progression progression = null, Transform parent = null) where T : SRBase
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await SRManager.GetInstance().Show(0, packageName, assetName, data, awaitingUIAssetName, progression, parent) as T;
            }

            public static async UniTask<T> Show<T>(string packageName, string assetName, object data = null, string awaitingUIAssetName = null, Progression progression = null, Transform parent = null) where T : SRBase
            {
                return await SRManager.GetInstance().Show(0, packageName, assetName, data, awaitingUIAssetName, progression, parent) as T;
            }

            public static async UniTask<T> Show<T>(int groupId, string assetName, object data = null, string awaitingUIAssetName = null, Progression progression = null, Transform parent = null) where T : SRBase
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await SRManager.GetInstance().Show(groupId, packageName, assetName, data, awaitingUIAssetName, progression, parent) as T;
            }

            public static async UniTask<T> Show<T>(int groupId, string packageName, string assetName, object data = null, string awaitingUIAssetName = null, Progression progression = null, Transform parent = null) where T : SRBase
            {
                return await SRManager.GetInstance().Show(groupId, packageName, assetName, data, awaitingUIAssetName, progression, parent) as T;
            }
            #endregion

            #region Close
            public static void Close(string assetName, bool disableDoSub = false, bool forceDestroy = false)
            {
                SRManager.GetInstance().Close(assetName, disableDoSub, forceDestroy);
            }

            public static void CloseAll(bool disableDoSub = false, bool forceDestroy = false, params string[] withoutAssetNames)
            {
                SRManager.GetInstance().CloseAll(disableDoSub, forceDestroy, withoutAssetNames);
            }

            public static void CloseAll(int groupId, bool disableDoSub = false, bool forceDestroy = false, params string[] withoutAssetNames)
            {
                SRManager.GetInstance().CloseAll(groupId, disableDoSub, forceDestroy, withoutAssetNames);
            }
            #endregion

            #region Reveal
            public static void Reveal(string assetName)
            {
                SRManager.GetInstance().Reveal(assetName);
            }

            public static void RevealAll()
            {
                SRManager.GetInstance().RevealAll();
            }

            public static void RevealAll(int groupId)
            {
                SRManager.GetInstance().RevealAll(groupId); ;
            }
            #endregion

            #region Hide
            public static void Hide(string assetName)
            {
                SRManager.GetInstance().Hide(assetName);
            }

            public static void HideAll()
            {
                SRManager.GetInstance().HideAll();
            }

            public static void HideAll(int groupId)
            {
                SRManager.GetInstance().HideAll(groupId);
            }
            #endregion
        }

        public static class USFrame
        {
            public static void InitInstance()
            {
                USManager.GetInstance();
            }

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
            /// (Single) If use prefix "build#" will load from build and else will load from bundle
            /// </summary>
            /// <param name="sceneName"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask LoadSingleSceneAsync(string sceneName, bool activateOnLoad = true, int priority = 100, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                if (RefineBuildScenePath(ref sceneName))
                {
                    await USManager.GetInstance().LoadFromBuildAsync(sceneName, LoadSceneMode.Single, progression);
                }
                else await USManager.GetInstance().LoadFromBundleAsync(packageName, sceneName, LoadSceneMode.Single, activateOnLoad, priority, progression);
            }

            /// <summary>
            /// (Single) If use prefix "build#" will load from build and else will load from bundle
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="sceneName"></param>
            /// <param name="activateOnLoad"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns>
            /// <para>From Build is &lt;AsyncOperation&gt;</para>
            /// <para>From Bundle is &lt;BundlePack&gt;</para>
            /// </returns>
            public static async UniTask<T> LoadSingleSceneAsync<T>(string sceneName, bool activateOnLoad = true, int priority = 100, Progression progression = null) where T : class
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                if (RefineBuildScenePath(ref sceneName))
                {
                    return await USManager.GetInstance().LoadFromBuildAsync(sceneName, LoadSceneMode.Single, progression) as T;
                }
                else return await USManager.GetInstance().LoadFromBundleAsync(packageName, sceneName, LoadSceneMode.Single, activateOnLoad, priority, progression) as T;
            }

            /// <summary>
            /// (Single) If use prefix "build#" will load from build and else will load from bundle
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="sceneName"></param>
            /// <param name="activateOnLoad"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask LoadSingleSceneAsync(string packageName, string sceneName, bool activateOnLoad = true, int priority = 100, Progression progression = null)
            {
                if (RefineBuildScenePath(ref sceneName))
                {
                    await USManager.GetInstance().LoadFromBuildAsync(sceneName, LoadSceneMode.Single, progression);
                }
                else await USManager.GetInstance().LoadFromBundleAsync(packageName, sceneName, LoadSceneMode.Single, activateOnLoad, priority, progression);
            }

            /// <summary>
            /// (Single) If use prefix "build#" will load from build and else will load from bundle
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="packageName"></param>
            /// <param name="sceneName"></param>
            /// <param name="activateOnLoad"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns>
            /// <para>From Build is &lt;AsyncOperation&gt;</para>
            /// <para>From Bundle is &lt;BundlePack&gt;</para>
            /// </returns>
            public static async UniTask<T> LoadSingleSceneAsync<T>(string packageName, string sceneName, bool activateOnLoad = true, int priority = 100, Progression progression = null) where T : class
            {
                if (RefineBuildScenePath(ref sceneName))
                {
                    return await USManager.GetInstance().LoadFromBuildAsync(sceneName, LoadSceneMode.Single, progression) as T;
                }
                else return await USManager.GetInstance().LoadFromBundleAsync(packageName, sceneName, LoadSceneMode.Single, activateOnLoad, priority, progression) as T;
            }

            /// <summary>
            /// (Additive) If use prefix "build#" will load from build and else will load from bundle
            /// </summary>
            /// <param name="sceneName"></param>
            /// <param name="activateOnLoad"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask LoadAdditiveSceneAsync(string sceneName, bool activateOnLoad = true, int priority = 100, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                if (RefineBuildScenePath(ref sceneName))
                {
                    await USManager.GetInstance().LoadFromBuildAsync(sceneName, LoadSceneMode.Additive, progression);
                }
                else await USManager.GetInstance().LoadFromBundleAsync(packageName, sceneName, LoadSceneMode.Additive, activateOnLoad, priority, progression);
            }

            /// <summary>
            /// (Additive) If use prefix "build#" will load from build and else will load from bundle
            /// </summary>
            /// <param name="sceneName"></param>
            /// <param name="progression"></param>
            /// <returns>
            /// <para>From Build is &lt;AsyncOperation&gt;</para>
            /// <para>From Bundle is &lt;BundlePack&gt;</para>
            /// </returns>
            public static async UniTask<T> LoadAdditiveSceneAsync<T>(string sceneName, bool activateOnLoad = true, int priority = 100, Progression progression = null) where T : class
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                if (RefineBuildScenePath(ref sceneName))
                {
                    return await USManager.GetInstance().LoadFromBuildAsync(sceneName, LoadSceneMode.Additive, progression) as T;
                }
                else return await USManager.GetInstance().LoadFromBundleAsync(packageName, sceneName, LoadSceneMode.Additive, activateOnLoad, priority, progression) as T;
            }

            /// <summary>
            /// (Additive) If use prefix "build#" will load from build and else will load from bundle
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="sceneName"></param>
            /// <param name="activateOnLoad"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask LoadAdditiveSceneAsync(string packageName, string sceneName, bool activateOnLoad = true, int priority = 100, Progression progression = null)
            {
                if (RefineBuildScenePath(ref sceneName))
                {
                    await USManager.GetInstance().LoadFromBuildAsync(sceneName, LoadSceneMode.Additive, progression);
                }
                else await USManager.GetInstance().LoadFromBundleAsync(packageName, sceneName, LoadSceneMode.Additive, activateOnLoad, priority, progression);
            }

            /// <summary>
            /// (Additive) If use prefix "build#" will load from build and else will load from bundle
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="packageName"></param>
            /// <param name="sceneName"></param>
            /// <param name="activateOnLoad"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns>
            /// <para>From Build is &lt;AsyncOperation&gt;</para>
            /// <para>From Bundle is &lt;BundlePack&gt;</para>
            /// </returns>
            public static async UniTask<T> LoadAdditiveSceneAsync<T>(string packageName, string sceneName, bool activateOnLoad = true, int priority = 100, Progression progression = null) where T : class
            {
                if (RefineBuildScenePath(ref sceneName))
                {
                    return await USManager.GetInstance().LoadFromBuildAsync(sceneName, LoadSceneMode.Additive, progression) as T;
                }
                else return await USManager.GetInstance().LoadFromBundleAsync(packageName, sceneName, LoadSceneMode.Additive, activateOnLoad, priority, progression) as T;
            }

            /// <summary>
            /// If use prefix "build#" will load from build and else will load from bundle
            /// </summary>
            /// <param name="sceneName"></param>
            /// <param name="loadSceneMode"></param>
            /// <param name="activateOnLoad"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask LoadSceneAsync(string sceneName, LoadSceneMode loadSceneMode, bool activateOnLoad = true, int priority = 100, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                if (RefineBuildScenePath(ref sceneName))
                {
                    await USManager.GetInstance().LoadFromBuildAsync(sceneName, loadSceneMode, progression);
                }
                else await USManager.GetInstance().LoadFromBundleAsync(packageName, sceneName, loadSceneMode, activateOnLoad, priority, progression);
            }

            /// <summary>
            /// If use prefix "build#" will load from build and else will load from bundle
            /// </summary>
            /// <param name="sceneName"></param>
            /// <param name="loadSceneMode"></param>
            /// <param name="activateOnLoad"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns>
            /// <para>From Build is &lt;AsyncOperation&gt;</para>
            /// <para>From Bundle is &lt;BundlePack&gt;</para>
            /// </returns>
            public static async UniTask<T> LoadSceneAsync<T>(string sceneName, LoadSceneMode loadSceneMode, bool activateOnLoad = true, int priority = 100, Progression progression = null) where T : class
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                if (RefineBuildScenePath(ref sceneName))
                {
                    return await USManager.GetInstance().LoadFromBuildAsync(sceneName, loadSceneMode, progression) as T;
                }
                else return await USManager.GetInstance().LoadFromBundleAsync(packageName, sceneName, loadSceneMode, activateOnLoad, priority, progression) as T;
            }

            /// <summary>
            /// If use prefix "build#" will load from build and else will load from bundle
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="sceneName"></param>
            /// <param name="loadSceneMode"></param>
            /// <param name="activateOnLoad"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask LoadSceneAsync(string packageName, string sceneName, LoadSceneMode loadSceneMode, bool activateOnLoad = true, int priority = 100, Progression progression = null)
            {
                if (RefineBuildScenePath(ref sceneName))
                {
                    await USManager.GetInstance().LoadFromBuildAsync(sceneName, loadSceneMode, progression);
                }
                else await USManager.GetInstance().LoadFromBundleAsync(packageName, sceneName, loadSceneMode, activateOnLoad, priority, progression);
            }

            /// <summary>
            /// If use prefix "build#" will load from build and else will load from bundle
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="packageName"></param>
            /// <param name="sceneName"></param>
            /// <param name="loadSceneMode"></param>
            /// <param name="activateOnLoad"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns>
            /// <para>From Build is &lt;AsyncOperation&gt;</para>
            /// <para>From Bundle is &lt;BundlePack&gt;</para>
            /// </returns>
            public static async UniTask<T> LoadSceneAsync<T>(string packageName, string sceneName, LoadSceneMode loadSceneMode, bool activateOnLoad = true, int priority = 100, Progression progression = null) where T : class
            {
                if (RefineBuildScenePath(ref sceneName))
                {
                    return await USManager.GetInstance().LoadFromBuildAsync(sceneName, loadSceneMode, progression) as T;
                }
                else return await USManager.GetInstance().LoadFromBundleAsync(packageName, sceneName, loadSceneMode, activateOnLoad, priority, progression) as T;
            }

            /// <summary>
            /// Only load from build via build index
            /// </summary>
            /// <param name="buildIndex"></param>
            /// <param name="loadSceneMode"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask<AsyncOperation> LoadSceneAsync(int buildIndex, LoadSceneMode loadSceneMode = LoadSceneMode.Single, Progression progression = null)
            {
                return await USManager.GetInstance().LoadFromBuildAsync(buildIndex, loadSceneMode, progression);
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

        public static class CPFrame
        {
            public static void InitInstance()
            {
                CPManager.GetInstance();
            }

            /// <summary>
            /// If use prefix "res#" will load from resources else will load from bundle
            /// </summary>
            /// <param name="assetName"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask PreloadAsync(string assetName, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await CPManager.GetInstance().PreloadAsync(packageName, new string[] { assetName }, progression);
            }

            public static async UniTask PreloadAsync(string packageName, string assetName, Progression progression = null)
            {
                await CPManager.GetInstance().PreloadAsync(packageName, new string[] { assetName }, progression);
            }

            public static async UniTask PreloadAsync(string[] assetNames, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await CPManager.GetInstance().PreloadAsync(packageName, assetNames, progression);
            }

            public static async UniTask PreloadAsync(string packageName, string[] assetNames, Progression progression = null)
            {
                await CPManager.GetInstance().PreloadAsync(packageName, assetNames, progression);
            }

            /// <summary>
            /// If use prefix "res#" will load from resources else will load from bundle
            /// </summary>
            /// <param name="assetName"></param>
            /// <param name="progression"></param>
            public static void Preload(string assetName, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                CPManager.GetInstance().Preload(packageName, new string[] { assetName }, progression);
            }

            public static void Preload(string packageName, string assetName, Progression progression = null)
            {
                CPManager.GetInstance().Preload(packageName, new string[] { assetName }, progression);
            }

            public static void Preload(string[] assetNames, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                CPManager.GetInstance().Preload(packageName, assetNames, progression);
            }

            public static void Preload(string packageName, string[] assetNames, Progression progression = null)
            {
                CPManager.GetInstance().Preload(packageName, assetNames, progression);
            }

            /// <summary>
            /// If use prefix "res#" will load from resources else will load from bundle
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="assetName"></param>
            /// <param name="parent"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask<T> LoadWithCloneAsync<T>(string assetName, Transform parent = null, Progression progression = null) where T : CPBase, new()
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await CPManager.GetInstance().LoadWithCloneAsync<T>(packageName, assetName, parent, progression);
            }

            public static async UniTask<T> LoadWithCloneAsync<T>(string packageName, string assetName, Transform parent = null, Progression progression = null) where T : CPBase, new()
            {
                return await CPManager.GetInstance().LoadWithCloneAsync<T>(packageName, assetName, parent, progression);
            }

            public static async UniTask<T> LoadWithCloneAsync<T>(string assetName, Transform parent, bool worldPositionStays, Progression progression = null) where T : CPBase, new()
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await CPManager.GetInstance().LoadWithCloneAsync<T>(packageName, assetName, parent, worldPositionStays, progression);
            }

            public static async UniTask<T> LoadWithCloneAsync<T>(string packageName, string assetName, Transform parent, bool worldPositionStays, Progression progression = null) where T : CPBase, new()
            {
                return await CPManager.GetInstance().LoadWithCloneAsync<T>(packageName, assetName, parent, worldPositionStays, progression);
            }

            public static async UniTask<T> LoadWithCloneAsync<T>(string assetName, Vector3 position, Quaternion rotation, Transform parent = null, Vector3? scale = null, Progression progression = null) where T : CPBase, new()
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await CPManager.GetInstance().LoadWithCloneAsync<T>(packageName, assetName, position, rotation, parent, scale, progression);
            }

            public static async UniTask<T> LoadWithCloneAsync<T>(string packageName, string assetName, Vector3 position, Quaternion rotation, Transform parent = null, Vector3? scale = null, Progression progression = null) where T : CPBase, new()
            {
                return await CPManager.GetInstance().LoadWithCloneAsync<T>(packageName, assetName, position, rotation, parent, scale, progression);
            }

            /// <summary>
            /// If use prefix "res#" will load from resources else will load from bundle
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="assetName"></param>
            /// <param name="parent"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static T LoadWithClone<T>(string assetName, Transform parent = null, Progression progression = null) where T : CPBase, new()
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return CPManager.GetInstance().LoadWithClone<T>(packageName, assetName, parent, progression);
            }

            public static T LoadWithClone<T>(string packageName, string assetName, Transform parent = null, Progression progression = null) where T : CPBase, new()
            {
                return CPManager.GetInstance().LoadWithClone<T>(packageName, assetName, parent, progression);
            }

            public static T LoadWithClone<T>(string assetName, Transform parent, bool worldPositionStays, Progression progression = null) where T : CPBase, new()
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return CPManager.GetInstance().LoadWithClone<T>(packageName, assetName, parent, worldPositionStays, progression);
            }

            public static T LoadWithClone<T>(string packageName, string assetName, Transform parent, bool worldPositionStays, Progression progression = null) where T : CPBase, new()
            {
                return CPManager.GetInstance().LoadWithClone<T>(packageName, assetName, parent, worldPositionStays, progression);
            }

            public static T LoadWithClone<T>(string assetName, Vector3 position, Quaternion rotation, Transform parent = null, Vector3? scale = null, Progression progression = null) where T : CPBase, new()
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return CPManager.GetInstance().LoadWithClone<T>(packageName, assetName, position, rotation, parent, scale, progression);
            }

            public static T LoadWithClone<T>(string packageName, string assetName, Vector3 position, Quaternion rotation, Transform parent = null, Vector3? scale = null, Progression progression = null) where T : CPBase, new()
            {
                return CPManager.GetInstance().LoadWithClone<T>(packageName, assetName, position, rotation, parent, scale, progression);
            }
        }
    }
}
