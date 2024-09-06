using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader;
using OxGFrame.CoreFrame.CPFrame;
using OxGFrame.CoreFrame.SRFrame;
using OxGFrame.CoreFrame.UIFrame;
using OxGFrame.CoreFrame.USFrame;
using OxGKit.LoggingSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OxGFrame.CoreFrame
{
    public static class CoreFrames
    {
        public static class UIFrame
        {
            public static bool ignoreTimeScale
            {
                get { return UIManager.GetInstance().ignoreTimeScale; }
                set { UIManager.GetInstance().ignoreTimeScale = value; }
            }
            public static bool enabledUpdate
            {
                get { return UIManager.GetInstance().enabledUpdate; }
                set { UIManager.GetInstance().enabledUpdate = value; }
            }
            public static bool enabledFixedUpdate
            {
                get { return UIManager.GetInstance().enabledFixedUpdate; }
                set { UIManager.GetInstance().enabledFixedUpdate = value; }
            }
            public static bool enabledLateUpdate
            {
                get { return UIManager.GetInstance().enabledLateUpdate; }
                set { UIManager.GetInstance().enabledLateUpdate = value; }
            }

            public static void InitInstance()
            {
                UIManager.GetInstance();
            }

            /// <summary>
            /// Finds an object by its name and sets and checks if there is a matching UICanvas context
            /// </summary>
            /// <param name="canvasName"></param>
            public static bool SetupAndCheckUICanvas(string canvasName)
            {
                return UIManager.GetInstance().SetupAndCheckUICanvas(canvasName);
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

            public static int GetStackByStackCount(string canvasName)
            {
                return UIManager.GetInstance().GetStackByStackCount(0, canvasName);
            }

            public static int GetStackByStackCount(int groupId, string canvasName)
            {
                return UIManager.GetInstance().GetStackByStackCount(groupId, canvasName);
            }

            /// <summary>
            /// Send refresh message to specific with data
            /// </summary>
            /// <param name="refreshInfo"></param>
            public static void SendRefreshData(RefreshInfo refreshInfo)
            {
                UIManager.GetInstance().SendRefreshData(new RefreshInfo[] { refreshInfo });
            }

            /// <summary>
            /// Send refresh message to specific with data
            /// </summary>
            /// <param name="refreshInfos"></param>
            public static void SendRefreshData(RefreshInfo[] refreshInfos)
            {
                UIManager.GetInstance().SendRefreshData(refreshInfos);
            }

            /// <summary>
            /// Send refresh message to all without data
            /// </summary>
            public static void SendRefreshDataToAll()
            {
                UIManager.GetInstance().SendRefreshDataToAll(null, null);
            }

            /// <summary>
            /// Send refresh message to all with same data
            /// </summary>
            /// <param name="data"></param>
            public static void SendRefreshDataToAll(object data)
            {
                UIManager.GetInstance().SendRefreshDataToAll(data, null);
            }

            /// <summary>
            /// Send refresh message to all without data except specificRefreshInfos
            /// </summary>
            /// <param name="specificRefreshInfos"></param>
            public static void SendRefreshDataToAll(RefreshInfo[] specificRefreshInfos)
            {
                UIManager.GetInstance().SendRefreshDataToAll(null, specificRefreshInfos);
            }

            /// <summary>
            /// Send refresh message to all with same data except specificRefreshInfos
            /// </summary>
            /// <param name="specificRefreshInfos"></param>
            public static void SendRefreshDataToAll(object data, RefreshInfo[] specificRefreshInfos)
            {
                UIManager.GetInstance().SendRefreshDataToAll(data, specificRefreshInfos);
            }

            #region Canvas
            /// <summary>
            /// Get UICanvas by canvas name
            /// </summary>
            /// <param name="canvasName"></param>
            /// <returns></returns>
            public static UICanvas GetUICanvas(string canvasName)
            {
                return UIManager.GetInstance().GetUICanvas(canvasName);
            }
            #endregion

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
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask Preload(string assetName, uint priority = 0, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await UIManager.GetInstance().Preload(packageName, new string[] { assetName }, priority, progression);
            }

            public static async UniTask Preload(string packageName, string assetName, uint priority = 0, Progression progression = null)
            {
                await UIManager.GetInstance().Preload(packageName, new string[] { assetName }, priority, progression);
            }

            public static async UniTask Preload(string[] assetNames, uint priority = 0, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await UIManager.GetInstance().Preload(packageName, assetNames, priority, progression);
            }

            public static async UniTask Preload(string packageName, string[] assetNames, uint priority = 0, Progression progression = null)
            {
                await UIManager.GetInstance().Preload(packageName, assetNames, priority, progression);
            }
            #endregion

            #region Show
            /// <summary>
            /// If use prefix "res#" will load from resources else will load from bundle
            /// </summary>
            /// <param name="assetName"></param>
            /// <param name="data"></param>
            /// <param name="awaitingUIAssetName"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <param name="parent"></param>
            /// <returns></returns>
            public static async UniTask<UIBase> Show(string assetName, object data = null, string awaitingUIAssetName = null, uint priority = 0, Progression progression = null, Transform parent = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await UIManager.GetInstance().Show(0, packageName, assetName, data, awaitingUIAssetName, priority, progression, parent);
            }

            public static async UniTask<UIBase> Show(string packageName, string assetName, object data = null, string awaitingUIAssetName = null, uint priority = 0, Progression progression = null, Transform parent = null)
            {
                return await UIManager.GetInstance().Show(0, packageName, assetName, data, awaitingUIAssetName, priority, progression, parent);
            }

            public static async UniTask<UIBase> Show(int groupId, string assetName, object data = null, string awaitingUIAssetName = null, uint priority = 0, Progression progression = null, Transform parent = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await UIManager.GetInstance().Show(groupId, packageName, assetName, data, awaitingUIAssetName, priority, progression, parent);
            }

            public static async UniTask<UIBase> Show(int groupId, string packageName, string assetName, object data = null, string awaitingUIAssetName = null, uint priority = 0, Progression progression = null, Transform parent = null)
            {
                return await UIManager.GetInstance().Show(groupId, packageName, assetName, data, awaitingUIAssetName, priority, progression, parent);
            }

            public static async UniTask<T> Show<T>(string assetName, object data = null, string awaitingUIAssetName = null, uint priority = 0, Progression progression = null, Transform parent = null) where T : UIBase
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await UIManager.GetInstance().Show(0, packageName, assetName, data, awaitingUIAssetName, priority, progression, parent) as T;
            }

            public static async UniTask<T> Show<T>(string packageName, string assetName, object data = null, string awaitingUIAssetName = null, uint priority = 0, Progression progression = null, Transform parent = null) where T : UIBase
            {
                return await UIManager.GetInstance().Show(0, packageName, assetName, data, awaitingUIAssetName, priority, progression, parent) as T;
            }

            public static async UniTask<T> Show<T>(int groupId, string assetName, object data = null, string awaitingUIAssetName = null, uint priority = 0, Progression progression = null, Transform parent = null) where T : UIBase
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await UIManager.GetInstance().Show(groupId, packageName, assetName, data, awaitingUIAssetName, priority, progression, parent) as T;
            }

            public static async UniTask<T> Show<T>(int groupId, string packageName, string assetName, object data = null, string awaitingUIAssetName = null, uint priority = 0, Progression progression = null, Transform parent = null) where T : UIBase
            {
                return await UIManager.GetInstance().Show(groupId, packageName, assetName, data, awaitingUIAssetName, priority, progression, parent) as T;
            }
            #endregion

            #region Close
            public static void Close(string assetName, bool disabledPreClose = false, bool forceDestroy = false)
            {
                UIManager.GetInstance().Close(assetName, disabledPreClose, forceDestroy);
            }

            public static void CloseAll(bool disabledPreClose = false, bool forceDestroy = false, params string[] withoutAssetNames)
            {
                UIManager.GetInstance().CloseAll(disabledPreClose, forceDestroy, false, withoutAssetNames);
            }

            public static void CloseAll(int groupId, bool disabledPreClose = false, bool forceDestroy = false, params string[] withoutAssetNames)
            {
                UIManager.GetInstance().CloseAll(groupId, disabledPreClose, forceDestroy, false, withoutAssetNames);
            }

            public static void CloseAllAndExcluded(bool disabledPreClose = false, bool forceDestroy = false)
            {
                UIManager.GetInstance().CloseAll(disabledPreClose, forceDestroy, true);
            }

            public static void CloseAllAndExcluded(int groupId, bool disabledPreClose = false, bool forceDestroy = false)
            {
                UIManager.GetInstance().CloseAll(groupId, disabledPreClose, forceDestroy, true);
            }

            /// <summary>
            /// Only allow close stack by stack
            /// </summary>
            /// <param name="canvasName"></param>
            /// <param name="disabledPreClose"></param>
            /// <param name="forceDestroy"></param>
            public static void CloseStackByStack(string canvasName, bool disabledPreClose = false, bool forceDestroy = false)
            {
                UIManager.GetInstance().CloseStackByStack(0, canvasName, disabledPreClose, forceDestroy);
            }

            /// <summary>
            /// Only allow close stack by stack
            /// </summary>
            /// <param name="groupId"></param>
            /// <param name="canvasName"></param>
            /// <param name="disabledPreClose"></param>
            /// <param name="forceDestroy"></param>
            public static void CloseStackByStack(int groupId, string canvasName, bool disabledPreClose = false, bool forceDestroy = false)
            {
                UIManager.GetInstance().CloseStackByStack(groupId, canvasName, disabledPreClose, forceDestroy);
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

            public static void HideAll(params string[] withoutAssetNames)
            {
                UIManager.GetInstance().HideAll(false, withoutAssetNames);
            }

            public static void HideAll(int groupId, params string[] withoutAssetNames)
            {
                UIManager.GetInstance().HideAll(groupId, false, withoutAssetNames);
            }

            public static void HideAllAndExcluded()
            {
                UIManager.GetInstance().HideAll(true);
            }

            public static void HideAllAndExcluded(int groupId)
            {
                UIManager.GetInstance().HideAll(groupId, true);
            }
            #endregion
        }

        public static class SRFrame
        {
            public static bool ignoreTimeScale
            {
                get { return SRManager.GetInstance().ignoreTimeScale; }
                set { SRManager.GetInstance().ignoreTimeScale = value; }
            }
            public static bool enabledUpdate
            {
                get { return SRManager.GetInstance().enabledUpdate; }
                set { SRManager.GetInstance().enabledUpdate = value; }
            }
            public static bool enabledFixedUpdate
            {
                get { return SRManager.GetInstance().enabledFixedUpdate; }
                set { SRManager.GetInstance().enabledFixedUpdate = value; }
            }
            public static bool enabledLateUpdate
            {
                get { return SRManager.GetInstance().enabledLateUpdate; }
                set { SRManager.GetInstance().enabledLateUpdate = value; }
            }

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

            /// <summary>
            /// Send refresh message to specific with data
            /// </summary>
            /// <param name="refreshInfo"></param>
            public static void SendRefreshData(RefreshInfo refreshInfo)
            {
                SRManager.GetInstance().SendRefreshData(new RefreshInfo[] { refreshInfo });
            }

            /// <summary>
            /// Send refresh message to specific with data
            /// </summary>
            /// <param name="refreshInfos"></param>
            public static void SendRefreshData(RefreshInfo[] refreshInfos)
            {
                SRManager.GetInstance().SendRefreshData(refreshInfos);
            }

            /// <summary>
            /// Send refresh message to all without data
            /// </summary>
            public static void SendRefreshDataToAll()
            {
                SRManager.GetInstance().SendRefreshDataToAll(null, null);
            }

            /// <summary>
            /// Send refresh message to all with same data
            /// </summary>
            /// <param name="data"></param>
            public static void SendRefreshDataToAll(object data)
            {
                SRManager.GetInstance().SendRefreshDataToAll(data, null);
            }

            /// <summary>
            /// Send refresh message to all without data except specificRefreshInfos
            /// </summary>
            /// <param name="specificRefreshInfos"></param>
            public static void SendRefreshDataToAll(RefreshInfo[] specificRefreshInfos)
            {
                SRManager.GetInstance().SendRefreshDataToAll(null, specificRefreshInfos);
            }

            /// <summary>
            /// Send refresh message to all with same data except specificRefreshInfos
            /// </summary>
            /// <param name="specificRefreshInfos"></param>
            public static void SendRefreshDataToAll(object data, RefreshInfo[] specificRefreshInfos)
            {
                SRManager.GetInstance().SendRefreshDataToAll(data, specificRefreshInfos);
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
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask Preload(string assetName, uint priority = 0, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await SRManager.GetInstance().Preload(packageName, new string[] { assetName }, priority, progression);
            }

            public static async UniTask Preload(string packageName, string assetName, uint priority = 0, Progression progression = null)
            {
                await SRManager.GetInstance().Preload(packageName, new string[] { assetName }, priority, progression);
            }

            public static async UniTask Preload(string[] assetNames, uint priority = 0, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await SRManager.GetInstance().Preload(packageName, assetNames, priority, progression);
            }

            public static async UniTask Preload(string packageName, string[] assetNames, uint priority = 0, Progression progression = null)
            {
                await SRManager.GetInstance().Preload(packageName, assetNames, priority, progression);
            }
            #endregion

            #region Show
            /// <summary>
            /// If use prefix "res#" will load from resources else will load from bundle
            /// </summary>
            /// <param name="assetName"></param>
            /// <param name="data"></param>
            /// <param name="awaitingUIAssetName"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <param name="parent"></param>
            /// <returns></returns>
            public static async UniTask<SRBase> Show(string assetName, object data = null, string awaitingUIAssetName = null, uint priority = 0, Progression progression = null, Transform parent = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await SRManager.GetInstance().Show(0, packageName, assetName, data, awaitingUIAssetName, priority, progression, parent);
            }

            public static async UniTask<SRBase> Show(string packageName, string assetName, object data = null, string awaitingUIAssetName = null, uint priority = 0, Progression progression = null, Transform parent = null)
            {
                return await SRManager.GetInstance().Show(0, packageName, assetName, data, awaitingUIAssetName, priority, progression, parent);
            }

            public static async UniTask<SRBase> Show(int groupId, string assetName, object data = null, string awaitingUIAssetName = null, uint priority = 0, Progression progression = null, Transform parent = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await SRManager.GetInstance().Show(groupId, packageName, assetName, data, awaitingUIAssetName, priority, progression, parent);
            }

            public static async UniTask<SRBase> Show(int groupId, string packageName, string assetName, object data = null, string awaitingUIAssetName = null, uint priority = 0, Progression progression = null, Transform parent = null)
            {
                return await SRManager.GetInstance().Show(groupId, packageName, assetName, data, awaitingUIAssetName, priority, progression, parent);
            }

            public static async UniTask<T> Show<T>(string assetName, object data = null, string awaitingUIAssetName = null, uint priority = 0, Progression progression = null, Transform parent = null) where T : SRBase
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await SRManager.GetInstance().Show(0, packageName, assetName, data, awaitingUIAssetName, priority, progression, parent) as T;
            }

            public static async UniTask<T> Show<T>(string packageName, string assetName, object data = null, string awaitingUIAssetName = null, uint priority = 0, Progression progression = null, Transform parent = null) where T : SRBase
            {
                return await SRManager.GetInstance().Show(0, packageName, assetName, data, awaitingUIAssetName, priority, progression, parent) as T;
            }

            public static async UniTask<T> Show<T>(int groupId, string assetName, object data = null, string awaitingUIAssetName = null, uint priority = 0, Progression progression = null, Transform parent = null) where T : SRBase
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await SRManager.GetInstance().Show(groupId, packageName, assetName, data, awaitingUIAssetName, priority, progression, parent) as T;
            }

            public static async UniTask<T> Show<T>(int groupId, string packageName, string assetName, object data = null, string awaitingUIAssetName = null, uint priority = 0, Progression progression = null, Transform parent = null) where T : SRBase
            {
                return await SRManager.GetInstance().Show(groupId, packageName, assetName, data, awaitingUIAssetName, priority, progression, parent) as T;
            }
            #endregion

            #region Close
            public static void Close(string assetName, bool disabledPreClose = false, bool forceDestroy = false)
            {
                SRManager.GetInstance().Close(assetName, disabledPreClose, forceDestroy);
            }

            public static void CloseAll(bool disabledPreClose = false, bool forceDestroy = false, params string[] withoutAssetNames)
            {
                SRManager.GetInstance().CloseAll(disabledPreClose, forceDestroy, false, withoutAssetNames);
            }

            public static void CloseAll(int groupId, bool disabledPreClose = false, bool forceDestroy = false, params string[] withoutAssetNames)
            {
                SRManager.GetInstance().CloseAll(groupId, disabledPreClose, forceDestroy, false, withoutAssetNames);
            }

            public static void CloseAllAndExcluded(bool disabledPreClose = false, bool forceDestroy = false)
            {
                SRManager.GetInstance().CloseAll(disabledPreClose, forceDestroy, true);
            }

            public static void CloseAllAndExcluded(int groupId, bool disabledPreClose = false, bool forceDestroy = false)
            {
                SRManager.GetInstance().CloseAll(groupId, disabledPreClose, forceDestroy, true);
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
                SRManager.GetInstance().RevealAll(groupId);
            }
            #endregion

            #region Hide
            public static void Hide(string assetName)
            {
                SRManager.GetInstance().Hide(assetName);
            }

            public static void HideAll(params string[] withoutAssetNames)
            {
                SRManager.GetInstance().HideAll(false, withoutAssetNames);
            }

            public static void HideAll(int groupId, params string[] withoutAssetNames)
            {
                SRManager.GetInstance().HideAll(groupId, false, withoutAssetNames);
            }

            public static void HideAllAndExcluded()
            {
                SRManager.GetInstance().HideAll(true);
            }

            public static void HideAllAndExcluded(int groupId)
            {
                SRManager.GetInstance().HideAll(groupId, true);
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

            public static Scene[] GetAllScenes(params string[] sceneNames)
            {
                return USManager.GetInstance().GetAllScenes(sceneNames);
            }

            public static Scene[] GetAllScenes(params int[] buildIndexes)
            {
                return USManager.GetInstance().GetAllScenes(buildIndexes);
            }

            /// <summary>
            /// Set active all root game objects in scene (If there are many scenes are same name, will process to find all scenes with same name)
            /// </summary>
            /// <param name="sceneName"></param>
            /// <param name="active"></param>
            /// <param name="withoutRootGameObjectNames"></param>
            public static void SetActiveSceneRootGameObjects(string sceneName, bool active, params string[] withoutRootGameObjectNames)
            {
                USManager.GetInstance().SetActiveSceneRootGameObjects(sceneName, active, withoutRootGameObjectNames);
            }

            /// <summary>
            /// Set active all root game objects in scene
            /// </summary>
            /// <param name="scene"></param>
            /// <param name="active"></param>
            /// <param name="withoutRootGameObjectNames"></param>
            public static void SetActiveSceneRootGameObjects(Scene scene, bool active, string[] withoutRootGameObjectNames = null)
            {
                USManager.GetInstance().SetActiveSceneRootGameObjects(scene, active, withoutRootGameObjectNames);
            }

            /// <summary>
            /// (Single) If use prefix "build#" will load from build and else will load from bundle
            /// </summary>
            /// <param name="sceneName"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask LoadSingleSceneAsync(string sceneName, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await LoadSingleSceneAsync(packageName, sceneName, progression);
            }

            /// <summary>
            /// (Single) If use prefix "build#" will load from build and else will load from bundle
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="sceneName"></param>
            /// <param name="progression"></param>
            /// <returns>
            /// <para>From Build is &lt;AsyncOperation&gt;</para>
            /// <para>From Bundle is &lt;BundlePack&gt;</para>
            /// </returns>
            public static async UniTask<T> LoadSingleSceneAsync<T>(string sceneName, Progression progression = null) where T : class
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await LoadSingleSceneAsync<T>(packageName, sceneName, progression);
            }

            /// <summary>
            /// (Single) If use prefix "build#" will load from build and else will load from bundle
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="sceneName"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask LoadSingleSceneAsync(string packageName, string sceneName, Progression progression = null)
            {
                if (RefineBuildScenePath(ref sceneName))
                {
                    await USManager.GetInstance().LoadFromBuildAsync(sceneName, LoadSceneMode.Single, progression);
                }
                else await USManager.GetInstance().LoadFromBundleAsync(packageName, sceneName, LoadSceneMode.Single, true, 100, progression);
            }

            /// <summary>
            /// (Single) If use prefix "build#" will load from build and else will load from bundle
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="packageName"></param>
            /// <param name="sceneName"></param>
            /// <param name="progression"></param>
            /// <returns>
            /// <para>From Build is &lt;AsyncOperation&gt;</para>
            /// <para>From Bundle is &lt;BundlePack&gt;</para>
            /// </returns>
            public static async UniTask<T> LoadSingleSceneAsync<T>(string packageName, string sceneName, Progression progression = null) where T : class
            {
                if (RefineBuildScenePath(ref sceneName))
                {
                    return await USManager.GetInstance().LoadFromBuildAsync(sceneName, LoadSceneMode.Single, progression) as T;
                }
                else return await USManager.GetInstance().LoadFromBundleAsync(packageName, sceneName, LoadSceneMode.Single, true, 100, progression) as T;
            }

            /// <summary>
            /// (Single) If use prefix "build#" will load from build and else will load from bundle
            /// </summary>
            /// <param name="sceneName"></param>
            public static void LoadSingleScene(string sceneName, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                LoadSingleScene(packageName, sceneName, progression);
            }

            /// <summary>
            /// (Single) If use prefix "build#" will load from build and else will load from bundle
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="sceneName"></param>
            public static void LoadSingleScene(string packageName, string sceneName, Progression progression = null)
            {
                if (RefineBuildScenePath(ref sceneName))
                {
                    USManager.GetInstance().LoadFromBuild(sceneName, LoadSceneMode.Single, progression);
                }
                else USManager.GetInstance().LoadFromBundle(packageName, sceneName, LoadSceneMode.Single, progression);
            }

            /// <summary>
            /// (Additive) If use prefix "build#" will load from build and else will load from bundle
            /// </summary>
            /// <param name="sceneName"></param>
            /// <param name="activateOnLoad"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask LoadAdditiveSceneAsync(string sceneName, bool activateOnLoad = true, uint priority = 100, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await LoadAdditiveSceneAsync(packageName, sceneName, activateOnLoad, priority, progression);
            }

            public static async UniTask LoadAdditiveSceneAsync(string sceneName, bool activeRootGameObjects = true, bool activateOnLoad = true, uint priority = 100, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await LoadAdditiveSceneAsync(packageName, sceneName, activeRootGameObjects, activateOnLoad, priority, progression);
            }

            /// <summary>
            /// (Additive) If use prefix "build#" will load from build and else will load from bundle
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
            public static async UniTask<T> LoadAdditiveSceneAsync<T>(string sceneName, bool activateOnLoad = true, uint priority = 100, Progression progression = null) where T : class
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await LoadAdditiveSceneAsync<T>(packageName, sceneName, activateOnLoad, priority, progression);
            }

            public static async UniTask<T> LoadAdditiveSceneAsync<T>(string sceneName, bool activeRootGameObjects = true, bool activateOnLoad = true, uint priority = 100, Progression progression = null) where T : class
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await LoadAdditiveSceneAsync<T>(packageName, sceneName, activeRootGameObjects, activateOnLoad, priority, progression);
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
            public static async UniTask LoadAdditiveSceneAsync(string packageName, string sceneName, bool activateOnLoad = true, uint priority = 100, Progression progression = null)
            {
                if (RefineBuildScenePath(ref sceneName))
                {
                    await USManager.GetInstance().LoadFromBuildAsync(sceneName, LoadSceneMode.Additive, progression);
                }
                else await USManager.GetInstance().LoadFromBundleAsync(packageName, sceneName, LoadSceneMode.Additive, activateOnLoad, priority, progression);
            }

            public static async UniTask LoadAdditiveSceneAsync(string packageName, string sceneName, bool activeRootGameObjects = true, bool activateOnLoad = true, uint priority = 100, Progression progression = null)
            {
                if (RefineBuildScenePath(ref sceneName))
                {
                    await USManager.GetInstance().LoadFromBuildAsync(sceneName, LoadSceneMode.Additive, progression);
                }
                else await USManager.GetInstance().LoadFromBundleAsync(packageName, sceneName, LoadSceneMode.Additive, activateOnLoad, priority, progression);
                if (!activeRootGameObjects) USManager.GetInstance().SetActiveSceneRootGameObjects(sceneName, false);
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
            public static async UniTask<T> LoadAdditiveSceneAsync<T>(string packageName, string sceneName, bool activateOnLoad = true, uint priority = 100, Progression progression = null) where T : class
            {
                if (RefineBuildScenePath(ref sceneName))
                {
                    return await USManager.GetInstance().LoadFromBuildAsync(sceneName, LoadSceneMode.Additive, progression) as T;
                }
                else return await USManager.GetInstance().LoadFromBundleAsync(packageName, sceneName, LoadSceneMode.Additive, activateOnLoad, priority, progression) as T;
            }

            public static async UniTask<T> LoadAdditiveSceneAsync<T>(string packageName, string sceneName, bool activeRootGameObjects = true, bool activateOnLoad = true, uint priority = 100, Progression progression = null) where T : class
            {
                if (RefineBuildScenePath(ref sceneName))
                {
                    var handler = await USManager.GetInstance().LoadFromBuildAsync(sceneName, LoadSceneMode.Additive, progression) as T;
                    if (!activeRootGameObjects) USManager.GetInstance().SetActiveSceneRootGameObjects(sceneName, false);
                    return handler;
                }
                else
                {
                    var handler = await USManager.GetInstance().LoadFromBundleAsync(packageName, sceneName, LoadSceneMode.Additive, activateOnLoad, priority, progression) as T;
                    if (!activeRootGameObjects) USManager.GetInstance().SetActiveSceneRootGameObjects(sceneName, false);
                    return handler;
                }
            }

            /// <summary>
            /// (Additive) If use prefix "build#" will load from build and else will load from bundle
            /// </summary>
            /// <param name="sceneName"></param>
            /// <param name="progression"></param>
            public static void LoadAdditiveScene(string sceneName, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                LoadAdditiveScene(packageName, sceneName, progression);
            }

            public static void LoadAdditiveScene(string sceneName, bool activeRootGameObjects = true, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                LoadAdditiveScene(packageName, sceneName, activeRootGameObjects, progression);
            }

            /// <summary>
            /// (Additive) If use prefix "build#" will load from build and else will load from bundle
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="sceneName"></param>
            public static void LoadAdditiveScene(string packageName, string sceneName, Progression progression = null)
            {
                if (RefineBuildScenePath(ref sceneName))
                {
                    USManager.GetInstance().LoadFromBuild(sceneName, LoadSceneMode.Additive, progression);
                }
                else USManager.GetInstance().LoadFromBundle(packageName, sceneName, LoadSceneMode.Additive, progression);
            }

            public static void LoadAdditiveScene(string packageName, string sceneName, bool activeRootGameObjects = true, Progression progression = null)
            {
                if (RefineBuildScenePath(ref sceneName))
                {
                    USManager.GetInstance().LoadFromBuild(sceneName, LoadSceneMode.Additive, progression);
                }
                else USManager.GetInstance().LoadFromBundle(packageName, sceneName, LoadSceneMode.Additive, progression);
                if (!activeRootGameObjects) USManager.GetInstance().SetActiveSceneRootGameObjects(sceneName, false);
            }

            /// <summary>
            /// Load single scene and additive scenes
            /// </summary>
            /// <param name="singleSceneName"></param>
            /// <param name="additiveSceneInfos"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask LoadMainAndSubScenesAsync(string singleSceneName, AdditiveSceneInfo[] additiveSceneInfos, uint priority = 100, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await LoadMainAndSubScenesAsync(packageName, singleSceneName, additiveSceneInfos, priority, progression);
            }

            /// <summary>
            /// Load single scene and additive scenes
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="singleSceneName"></param>
            /// <param name="additiveSceneInfos"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask LoadMainAndSubScenesAsync(string packageName, string singleSceneName, AdditiveSceneInfo[] additiveSceneInfos, uint priority = 100, Progression progression = null)
            {
                var scene = GetSceneByName(singleSceneName);
                if (!string.IsNullOrEmpty(scene.name) &&
                    scene.isLoaded)
                {
                    Logging.PrintWarning<Logger>($"Single Scene => {singleSceneName} already exists!!!");
                    return;
                }

                float comboCurrentCount = 0;
                float comboTotalCount = 1;

                if (additiveSceneInfos != null)
                    comboTotalCount += additiveSceneInfos.Length;

                // Single scene
                bool loaded = false;
                await LoadSingleSceneAsync(packageName, singleSceneName, (float progress, float currentCount, float totalCount) =>
                {
                    if (!loaded && progress == 1)
                    {
                        comboCurrentCount += progress;
                        loaded = true;
                    }
                    progression?.Invoke(comboCurrentCount / comboTotalCount, comboCurrentCount, comboTotalCount);
                });

                // Additive scenes
                if (additiveSceneInfos != null)
                {
                    uint idx = 0;
                    foreach (var additiveSceneInfo in additiveSceneInfos)
                    {
                        loaded = false;
                        await LoadAdditiveSceneAsync
                        (
                            packageName,
                            additiveSceneInfo.sceneName,
                            additiveSceneInfo.activeRootGameObjects,
                            true,
                            priority + idx,
                            (float progress, float currentCount, float totalCount) =>
                            {
                                if (!loaded && progress == 1)
                                {
                                    comboCurrentCount += progress;
                                    loaded = true;
                                }
                                progression?.Invoke(comboCurrentCount / comboTotalCount, comboCurrentCount, comboTotalCount);
                            }
                        );
                        idx++;
                    }
                }
            }

            /// <summary>
            /// Load additive scenes
            /// </summary>
            /// <param name="additiveSceneInfos"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask LoadSubScenesAsync(AdditiveSceneInfo[] additiveSceneInfos, uint priority = 100, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await LoadSubScenesAsync(packageName, additiveSceneInfos, priority, progression);
            }

            /// <summary>
            /// Load additive scenes
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="additiveSceneInfos"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask LoadSubScenesAsync(string packageName, AdditiveSceneInfo[] additiveSceneInfos, uint priority = 100, Progression progression = null)
            {
                float comboCurrentCount = 0;
                float comboTotalCount = 0;

                if (additiveSceneInfos != null)
                    comboTotalCount += additiveSceneInfos.Length;

                // Additive scenes
                if (additiveSceneInfos != null)
                {
                    uint idx = 0;
                    foreach (var additiveSceneInfo in additiveSceneInfos)
                    {
                        bool loaded = false;
                        await LoadAdditiveSceneAsync
                        (
                            packageName,
                            additiveSceneInfo.sceneName,
                            additiveSceneInfo.activeRootGameObjects,
                            true,
                            priority + idx,
                            (float progress, float currentCount, float totalCount) =>
                            {
                                if (!loaded && progress == 1)
                                {
                                    comboCurrentCount += progress;
                                    loaded = true;
                                }
                                progression?.Invoke(comboCurrentCount / comboTotalCount, comboCurrentCount, comboTotalCount);
                            }
                        );
                        idx++;
                    }
                }
            }

            /// <summary>
            /// Load single scene and additive scenes
            /// </summary>
            /// <param name="singleSceneName"></param>
            /// <param name="additiveSceneInfos"></param>
            /// <param name="progression"></param>
            public static void LoadMainAndSubScenes(string singleSceneName, AdditiveSceneInfo[] additiveSceneInfos, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                LoadMainAndSubScenes(packageName, singleSceneName, additiveSceneInfos, progression);
            }

            public static void LoadMainAndSubScenes(string packageName, string singleSceneName, AdditiveSceneInfo[] additiveSceneInfos, Progression progression = null)
            {
                var scene = GetSceneByName(singleSceneName);
                if (!string.IsNullOrEmpty(scene.name) &&
                    scene.isLoaded)
                {
                    Logging.PrintWarning<Logger>($"Single Scene => {singleSceneName} already exists!!!");
                    return;
                }

                float comboCurrentCount = 0;
                float comboTotalCount = 1;

                if (additiveSceneInfos != null)
                    comboTotalCount += additiveSceneInfos.Length;

                // Single scene
                bool loaded = false;
                LoadSingleScene(packageName, singleSceneName, (float progress, float currentCount, float totalCount) =>
                {
                    if (!loaded && progress == 1)
                    {
                        comboCurrentCount += progress;
                        loaded = true;
                    }
                    progression?.Invoke(comboCurrentCount / comboTotalCount, comboCurrentCount, comboTotalCount);
                });

                // Additive scenes
                if (additiveSceneInfos != null)
                {
                    foreach (var additiveSceneInfo in additiveSceneInfos)
                    {
                        loaded = false;
                        LoadAdditiveScene
                        (
                            packageName,
                            additiveSceneInfo.sceneName,
                            additiveSceneInfo.activeRootGameObjects,
                            (float progress, float currentCount, float totalCount) =>
                            {
                                if (!loaded && progress == 1)
                                {
                                    comboCurrentCount += progress;
                                    loaded = true;
                                }
                                progression?.Invoke(comboCurrentCount / comboTotalCount, comboCurrentCount, comboTotalCount);
                            }
                        );
                    }
                }
            }

            public static void LoadSubScenes(AdditiveSceneInfo[] additiveSceneInfos, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                LoadSubScenes(packageName, additiveSceneInfos, progression);
            }

            public static void LoadSubScenes(string packageName, AdditiveSceneInfo[] additiveSceneInfos, Progression progression = null)
            {
                float comboCurrentCount = 0;
                float comboTotalCount = 0;

                if (additiveSceneInfos != null)
                    comboTotalCount += additiveSceneInfos.Length;

                // Additive scenes
                if (additiveSceneInfos != null)
                {
                    foreach (var additiveSceneInfo in additiveSceneInfos)
                    {
                        bool loaded = false;
                        LoadAdditiveScene
                        (
                            packageName,
                            additiveSceneInfo.sceneName,
                            additiveSceneInfo.activeRootGameObjects,
                            (float progress, float currentCount, float totalCount) =>
                            {
                                if (!loaded && progress == 1)
                                {
                                    comboCurrentCount += progress;
                                    loaded = true;
                                }
                                progression?.Invoke(comboCurrentCount / comboTotalCount, comboCurrentCount, comboTotalCount);
                            }
                        );
                    }
                }
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
            public static async UniTask LoadSceneAsync(string sceneName, LoadSceneMode loadSceneMode, bool activateOnLoad = true, uint priority = 100, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await LoadSceneAsync(packageName, sceneName, loadSceneMode, activateOnLoad, priority, progression);
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
            public static async UniTask<T> LoadSceneAsync<T>(string sceneName, LoadSceneMode loadSceneMode, bool activateOnLoad = true, uint priority = 100, Progression progression = null) where T : class
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await LoadSceneAsync<T>(packageName, sceneName, loadSceneMode, activateOnLoad, priority, progression);
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
            public static async UniTask LoadSceneAsync(string packageName, string sceneName, LoadSceneMode loadSceneMode, bool activateOnLoad = true, uint priority = 100, Progression progression = null)
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
            public static async UniTask<T> LoadSceneAsync<T>(string packageName, string sceneName, LoadSceneMode loadSceneMode, bool activateOnLoad = true, uint priority = 100, Progression progression = null) where T : class
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
            /// If use prefix "build#" will load from build and else will load from bundle
            /// </summary>
            /// <param name="sceneName"></param>
            /// <param name="loadSceneMode"></param>
            /// <param name="progression"></param>
            public static void LoadScene(string sceneName, LoadSceneMode loadSceneMode, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                LoadScene(packageName, sceneName, loadSceneMode, progression);
            }

            /// <summary>
            /// If use prefix "build#" will load from build and else will load from bundle
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="sceneName"></param>
            /// <param name="loadSceneMode"></param>
            /// <param name="progression"></param>
            public static void LoadScene(string packageName, string sceneName, LoadSceneMode loadSceneMode, Progression progression = null)
            {
                if (RefineBuildScenePath(ref sceneName))
                {
                    USManager.GetInstance().LoadFromBuild(sceneName, loadSceneMode, progression);
                }
                else USManager.GetInstance().LoadFromBundle(packageName, sceneName, loadSceneMode, progression);
            }

            /// <summary>
            /// Only load from build via build index
            /// </summary>
            /// <param name="buildIndex"></param>
            /// <param name="loadSceneMode"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static Scene LoadScene(int buildIndex, LoadSceneMode loadSceneMode = LoadSceneMode.Single, Progression progression = null)
            {
                return USManager.GetInstance().LoadFromBuild(buildIndex, loadSceneMode, progression);
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
                if (string.IsNullOrEmpty(sceneName)) return false;
                string prefix = "build#";
                if (sceneName.Length > prefix.Length)
                {
                    if (sceneName.Substring(0, prefix.Length).Equals(prefix))
                    {
                        var count = sceneName.Length - prefix.Length;
                        sceneName = sceneName.Substring(prefix.Length, count);
                        return true;
                    }
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
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask PreloadAsync(string assetName, uint priority = 0, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await CPManager.GetInstance().PreloadAsync(packageName, new string[] { assetName }, priority, progression);
            }

            public static async UniTask PreloadAsync(string packageName, string assetName, uint priority = 0, Progression progression = null)
            {
                await CPManager.GetInstance().PreloadAsync(packageName, new string[] { assetName }, priority, progression);
            }

            public static async UniTask PreloadAsync(string[] assetNames, uint priority = 0, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await CPManager.GetInstance().PreloadAsync(packageName, assetNames, priority, progression);
            }

            public static async UniTask PreloadAsync(string packageName, string[] assetNames, uint priority = 0, Progression progression = null)
            {
                await CPManager.GetInstance().PreloadAsync(packageName, assetNames, priority, progression);
            }

            public static async UniTask PreloadAsync<T>(string assetName, uint priority = 0, Progression progression = null) where T : Object
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await CPManager.GetInstance().PreloadAsync<T>(packageName, new string[] { assetName }, priority, progression);
            }

            public static async UniTask PreloadAsync<T>(string packageName, string assetName, uint priority = 0, Progression progression = null) where T : Object
            {
                await CPManager.GetInstance().PreloadAsync(packageName, new string[] { assetName }, priority, progression);
            }

            public static async UniTask PreloadAsync<T>(string[] assetNames, uint priority = 0, Progression progression = null) where T : Object
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await CPManager.GetInstance().PreloadAsync<T>(packageName, assetNames, priority, progression);
            }

            public static async UniTask PreloadAsync<T>(string packageName, string[] assetNames, uint priority = 0, Progression progression = null) where T : Object
            {
                await CPManager.GetInstance().PreloadAsync<T>(packageName, assetNames, priority, progression);
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

            public static void Preload<T>(string assetName, Progression progression = null) where T : Object
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                CPManager.GetInstance().Preload<T>(packageName, new string[] { assetName }, progression);
            }

            public static void Preload<T>(string packageName, string assetName, Progression progression = null) where T : Object
            {
                CPManager.GetInstance().Preload<T>(packageName, new string[] { assetName }, progression);
            }

            public static void Preload<T>(string[] assetNames, Progression progression = null) where T : Object
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                CPManager.GetInstance().Preload<T>(packageName, assetNames, progression);
            }

            public static void Preload<T>(string packageName, string[] assetNames, Progression progression = null) where T : Object
            {
                CPManager.GetInstance().Preload<T>(packageName, assetNames, progression);
            }

            /// <summary>
            /// If use prefix "res#" will load from resources else will load from bundle
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="assetName"></param>
            /// <param name="parent"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask<T> LoadWithCloneAsync<T>(string assetName, Transform parent = null, uint priority = 0, Progression progression = null) where T : CPBase, new()
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await CPManager.GetInstance().LoadWithCloneAsync<T>(packageName, assetName, parent, priority, progression);
            }

            public static async UniTask<T> LoadWithCloneAsync<T>(string packageName, string assetName, Transform parent = null, uint priority = 0, Progression progression = null) where T : CPBase, new()
            {
                return await CPManager.GetInstance().LoadWithCloneAsync<T>(packageName, assetName, parent, priority, progression);
            }

            public static async UniTask<T> LoadWithCloneAsync<T>(string assetName, Transform parent, bool worldPositionStays, uint priority = 0, Progression progression = null) where T : CPBase, new()
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await CPManager.GetInstance().LoadWithCloneAsync<T>(packageName, assetName, parent, worldPositionStays, priority, progression);
            }

            public static async UniTask<T> LoadWithCloneAsync<T>(string packageName, string assetName, Transform parent, bool worldPositionStays, uint priority = 0, Progression progression = null) where T : CPBase, new()
            {
                return await CPManager.GetInstance().LoadWithCloneAsync<T>(packageName, assetName, parent, worldPositionStays, priority, progression);
            }

            public static async UniTask<T> LoadWithCloneAsync<T>(string assetName, Vector3 position, Quaternion rotation, Transform parent = null, Vector3? scale = null, uint priority = 0, Progression progression = null) where T : CPBase, new()
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await CPManager.GetInstance().LoadWithCloneAsync<T>(packageName, assetName, position, rotation, parent, scale, priority, progression);
            }

            public static async UniTask<T> LoadWithCloneAsync<T>(string packageName, string assetName, Vector3 position, Quaternion rotation, Transform parent = null, Vector3? scale = null, uint priority = 0, Progression progression = null) where T : CPBase, new()
            {
                return await CPManager.GetInstance().LoadWithCloneAsync<T>(packageName, assetName, position, rotation, parent, scale, priority, progression);
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