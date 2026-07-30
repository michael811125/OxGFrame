using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader;
using OxGFrame.CoreFrame.CPFrame;
using OxGFrame.CoreFrame.SRFrame;
using OxGFrame.CoreFrame.UIFrame;
using OxGFrame.CoreFrame.USFrame;
using OxGKit.LoggingSystem;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OxGFrame.CoreFrame
{
    /// <summary>
    /// Provide unified static access to the core frame modules (UIFrame, SRFrame, USFrame, CPFrame)
    /// </summary>
    public static class CoreFrames
    {
        /// <summary>
        /// Default group id
        /// </summary>
        internal const int DEFAULT_GROUP_ID = 0;

        /// <summary>
        /// Do all groups
        /// </summary>
        internal const int DO_ALL_GROUPS = -1;

        /// <summary>
        /// User Interface
        /// </summary>
        public static class UIFrame
        {
            /// <summary>
            /// Toggle whether the driven updates use unscaled delta time (ignores Time.timeScale)
            /// </summary>
            public static bool ignoreTimeScale
            {
                get { return UIManager.GetInstance().ignoreTimeScale; }
                set { UIManager.GetInstance().ignoreTimeScale = value; }
            }
            /// <summary>
            /// Toggle whether the manager drives Update calls for all showing UIs
            /// </summary>
            public static bool enableUpdate
            {
                get { return UIManager.GetInstance().enableUpdate; }
                set { UIManager.GetInstance().enableUpdate = value; }
            }
            /// <summary>
            /// Toggle whether the manager drives FixedUpdate calls for all showing UIs
            /// </summary>
            public static bool enableFixedUpdate
            {
                get { return UIManager.GetInstance().enableFixedUpdate; }
                set { UIManager.GetInstance().enableFixedUpdate = value; }
            }
            /// <summary>
            /// Toggle whether the manager drives LateUpdate calls for all showing UIs
            /// </summary>
            public static bool enableLateUpdate
            {
                get { return UIManager.GetInstance().enableLateUpdate; }
                set { UIManager.GetInstance().enableLateUpdate = value; }
            }

            /// <summary>
            /// Obsolete. Use enableUpdate instead
            /// </summary>
            [System.Obsolete("Use enableUpdate instead")]
            public static bool enabledUpdate
            {
                get { return enableUpdate; }
                set { enableUpdate = value; }
            }
            /// <summary>
            /// Obsolete. Use enableFixedUpdate instead
            /// </summary>
            [System.Obsolete("Use enableFixedUpdate instead")]
            public static bool enabledFixedUpdate
            {
                get { return enableFixedUpdate; }
                set { enableFixedUpdate = value; }
            }
            /// <summary>
            /// Obsolete. Use enableLateUpdate instead
            /// </summary>
            [System.Obsolete("Use enableLateUpdate instead")]
            public static bool enabledLateUpdate
            {
                get { return enableLateUpdate; }
                set { enableLateUpdate = value; }
            }

            /// <summary>
            /// Initialize the UIManager singleton instance
            /// </summary>
            public static void InitInstance()
            {
                UIManager.GetInstance();
            }

            /// <summary>
            /// Find a canvas object in the scene by its name, then set up and check whether there is a matching UICanvas context
            /// </summary>
            /// <param name="canvasName"></param>
            /// <returns></returns>
            public static bool SetupAndCheckUICanvas(string canvasName)
            {
                return UIManager.GetInstance().SetupAndCheckUICanvas(canvasName);
            }

            /// <summary>
            /// Check whether the UI with the given asset name is currently showing
            /// </summary>
            /// <param name="assetName"></param>
            /// <returns></returns>
            public static bool CheckIsShowing(string assetName)
            {
                return UIManager.GetInstance().CheckIsShowing(assetName);
            }

            /// <summary>
            /// Check whether the given UI instance is currently showing
            /// </summary>
            /// <param name="uiBase"></param>
            /// <returns></returns>
            public static bool CheckIsShowing(UIBase uiBase)
            {
                return UIManager.GetInstance().CheckIsShowing(uiBase);
            }

            /// <summary>
            /// Check whether the UI with the given asset name is currently hidden
            /// </summary>
            /// <param name="assetName"></param>
            /// <returns></returns>
            public static bool CheckIsHiding(string assetName)
            {
                return UIManager.GetInstance().CheckIsHiding(assetName);
            }

            /// <summary>
            /// Check whether the given UI instance is currently hidden
            /// </summary>
            /// <param name="uiBase"></param>
            /// <returns></returns>
            public static bool CheckIsHiding(UIBase uiBase)
            {
                return UIManager.GetInstance().CheckIsHiding(uiBase);
            }

            /// <summary>
            /// Check whether any UI in the default group (id 0) is hidden
            /// </summary>
            /// <returns></returns>
            public static bool CheckHasAnyHiding()
            {
                return UIManager.GetInstance().CheckHasAnyHiding(DEFAULT_GROUP_ID);
            }

            /// <summary>
            /// Check whether any UI in the specified group is hidden (set the group id to -1 to check all groups)
            /// </summary>
            /// <param name="groupId"></param>
            /// <returns></returns>
            public static bool CheckHasAnyHiding(int groupId)
            {
                return UIManager.GetInstance().CheckHasAnyHiding(groupId);
            }

            /// <summary>
            /// Check whether any UI in any group is hidden
            /// </summary>
            /// <returns></returns>
            public static bool CheckHasAnyHidingForAllGroups()
            {
                return UIManager.GetInstance().CheckHasAnyHiding(DO_ALL_GROUPS);
            }

            /// <summary>
            /// Get the count of UIs recorded stack by stack for the given canvas in the default group (id 0)
            /// </summary>
            /// <param name="canvasName"></param>
            /// <returns></returns>
            public static int GetStackByStackCount(string canvasName)
            {
                return UIManager.GetInstance().GetStackByStackCount(DEFAULT_GROUP_ID, canvasName);
            }

            /// <summary>
            /// Get the count of UIs recorded stack by stack for the given canvas in the specified group
            /// </summary>
            /// <param name="groupId"></param>
            /// <param name="canvasName"></param>
            /// <returns></returns>
            public static int GetStackByStackCount(int groupId, string canvasName)
            {
                return UIManager.GetInstance().GetStackByStackCount(groupId, canvasName);
            }

            /// <summary>
            /// Send a refresh message with data to the specific UI
            /// </summary>
            /// <param name="refreshInfo"></param>
            public static void SendRefreshData(RefreshInfo refreshInfo)
            {
                UIManager.GetInstance().SendRefreshData(new RefreshInfo[] { refreshInfo });
            }

            /// <summary>
            /// Send refresh messages with data to the specific UIs
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
            /// Send a refresh message without data to all UIs, except those matched in specificRefreshInfos which receive their own specified data
            /// </summary>
            /// <param name="specificRefreshInfos"></param>
            public static void SendRefreshDataToAll(RefreshInfo[] specificRefreshInfos)
            {
                UIManager.GetInstance().SendRefreshDataToAll(null, specificRefreshInfos);
            }

            /// <summary>
            /// Send a refresh message with the same data to all UIs, except those matched in specificRefreshInfos which receive their own specified data
            /// </summary>
            /// <param name="data"></param>
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
            /// <summary>
            /// Get the top-most cached UI component of type T by its asset name
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="assetName"></param>
            /// <returns></returns>
            public static T GetComponent<T>(string assetName) where T : UIBase
            {
                return UIManager.GetInstance().GetFrameComponent<T>(assetName);
            }

            /// <summary>
            /// Get all cached UI components of type T by their asset name
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="assetName"></param>
            /// <returns></returns>
            public static T[] GetComponents<T>(string assetName) where T : UIBase
            {
                return UIManager.GetInstance().GetFrameComponents<T>(assetName);
            }
            #endregion

            #region Preload
            /// <summary>
            /// Preload a UI asset into the cache by its asset name with the default package
            /// <para>If use prefix "res#" will load from resources else will load from bundle</para>
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

            /// <summary>
            /// Preload a UI asset into the cache by its asset name with a specific package name
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="assetName"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask Preload(string packageName, string assetName, uint priority = 0, Progression progression = null)
            {
                await UIManager.GetInstance().Preload(packageName, new string[] { assetName }, priority, progression);
            }

            /// <summary>
            /// Preload multiple UI assets into the cache by their asset names with the default package
            /// </summary>
            /// <param name="assetNames"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask Preload(string[] assetNames, uint priority = 0, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await UIManager.GetInstance().Preload(packageName, assetNames, priority, progression);
            }

            /// <summary>
            /// Preload multiple UI assets into the cache by their asset names with a specific package name
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="assetNames"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask Preload(string packageName, string[] assetNames, uint priority = 0, Progression progression = null)
            {
                await UIManager.GetInstance().Preload(packageName, assetNames, priority, progression);
            }
            #endregion

            #region Show
            /// <summary>
            /// Show a UI by its asset name with the default package
            /// <para>If use prefix "res#" will load from resources else will load from bundle</para>
            /// <para>Default group id is 0</para>
            /// </summary>
            /// <param name="assetName"></param>
            /// <param name="data"></param>
            /// <param name="awaitingUIAssetName"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <param name="parent"></param>
            /// <param name="awaitingUIExtraDuration"> Sec Time </param>
            /// <returns></returns>
            public static async UniTask<UIBase> Show(string assetName, object data = null, string awaitingUIAssetName = null, uint priority = 0, Progression progression = null, Transform parent = null, float awaitingUIExtraDuration = 0f)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await UIManager.GetInstance().Show(DEFAULT_GROUP_ID, packageName, assetName, data, awaitingUIAssetName, awaitingUIExtraDuration, priority, progression, parent);
            }

            /// <summary>
            /// Show a UI by its asset name with a specific package name
            /// <para>Default group id is 0</para>
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="assetName"></param>
            /// <param name="data"></param>
            /// <param name="awaitingUIAssetName"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <param name="parent"></param>
            /// <param name="awaitingUIExtraDuration"></param>
            /// <returns></returns>
            public static async UniTask<UIBase> Show(string packageName, string assetName, object data = null, string awaitingUIAssetName = null, uint priority = 0, Progression progression = null, Transform parent = null, float awaitingUIExtraDuration = 0f)
            {
                return await UIManager.GetInstance().Show(DEFAULT_GROUP_ID, packageName, assetName, data, awaitingUIAssetName, awaitingUIExtraDuration, priority, progression, parent);
            }

            /// <summary>
            /// Show a UI in the specified group by its asset name with the default package
            /// </summary>
            /// <param name="groupId"></param>
            /// <param name="assetName"></param>
            /// <param name="data"></param>
            /// <param name="awaitingUIAssetName"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <param name="parent"></param>
            /// <param name="awaitingUIExtraDuration"></param>
            /// <returns></returns>
            public static async UniTask<UIBase> Show(int groupId, string assetName, object data = null, string awaitingUIAssetName = null, uint priority = 0, Progression progression = null, Transform parent = null, float awaitingUIExtraDuration = 0f)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await UIManager.GetInstance().Show(groupId, packageName, assetName, data, awaitingUIAssetName, awaitingUIExtraDuration, priority, progression, parent);
            }

            /// <summary>
            /// Show a UI in the specified group by its asset name with a specific package name
            /// </summary>
            /// <param name="groupId"></param>
            /// <param name="packageName"></param>
            /// <param name="assetName"></param>
            /// <param name="data"></param>
            /// <param name="awaitingUIAssetName"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <param name="parent"></param>
            /// <param name="awaitingUIExtraDuration"></param>
            /// <returns></returns>
            public static async UniTask<UIBase> Show(int groupId, string packageName, string assetName, object data = null, string awaitingUIAssetName = null, uint priority = 0, Progression progression = null, Transform parent = null, float awaitingUIExtraDuration = 0f)
            {
                return await UIManager.GetInstance().Show(groupId, packageName, assetName, data, awaitingUIAssetName, awaitingUIExtraDuration, priority, progression, parent);
            }

            /// <summary>
            /// Show a UI by its asset name with the default package and return it as T
            /// <para>If use prefix "res#" will load from resources else will load from bundle</para>
            /// <para>Default group id is 0</para>
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="assetName"></param>
            /// <param name="data"></param>
            /// <param name="awaitingUIAssetName"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <param name="parent"></param>
            /// <param name="awaitingUIExtraDuration"></param>
            /// <returns></returns>
            public static async UniTask<T> Show<T>(string assetName, object data = null, string awaitingUIAssetName = null, uint priority = 0, Progression progression = null, Transform parent = null, float awaitingUIExtraDuration = 0f) where T : UIBase
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await UIManager.GetInstance().Show(DEFAULT_GROUP_ID, packageName, assetName, data, awaitingUIAssetName, awaitingUIExtraDuration, priority, progression, parent) as T;
            }

            /// <summary>
            /// Show a UI by its asset name with a specific package name and return it as T
            /// <para>Default group id is 0</para>
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="packageName"></param>
            /// <param name="assetName"></param>
            /// <param name="data"></param>
            /// <param name="awaitingUIAssetName"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <param name="parent"></param>
            /// <param name="awaitingUIExtraDuration"></param>
            /// <returns></returns>
            public static async UniTask<T> Show<T>(string packageName, string assetName, object data = null, string awaitingUIAssetName = null, uint priority = 0, Progression progression = null, Transform parent = null, float awaitingUIExtraDuration = 0f) where T : UIBase
            {
                return await UIManager.GetInstance().Show(DEFAULT_GROUP_ID, packageName, assetName, data, awaitingUIAssetName, awaitingUIExtraDuration, priority, progression, parent) as T;
            }

            /// <summary>
            /// Show a UI in the specified group by its asset name with the default package and return it as T
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="groupId"></param>
            /// <param name="assetName"></param>
            /// <param name="data"></param>
            /// <param name="awaitingUIAssetName"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <param name="parent"></param>
            /// <param name="awaitingUIExtraDuration"></param>
            /// <returns></returns>
            public static async UniTask<T> Show<T>(int groupId, string assetName, object data = null, string awaitingUIAssetName = null, uint priority = 0, Progression progression = null, Transform parent = null, float awaitingUIExtraDuration = 0f) where T : UIBase
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await UIManager.GetInstance().Show(groupId, packageName, assetName, data, awaitingUIAssetName, awaitingUIExtraDuration, priority, progression, parent) as T;
            }

            /// <summary>
            /// Show a UI in the specified group by its asset name with a specific package name and return it as T
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="groupId"></param>
            /// <param name="packageName"></param>
            /// <param name="assetName"></param>
            /// <param name="data"></param>
            /// <param name="awaitingUIAssetName"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <param name="parent"></param>
            /// <param name="awaitingUIExtraDuration"></param>
            /// <returns></returns>
            public static async UniTask<T> Show<T>(int groupId, string packageName, string assetName, object data = null, string awaitingUIAssetName = null, uint priority = 0, Progression progression = null, Transform parent = null, float awaitingUIExtraDuration = 0f) where T : UIBase
            {
                return await UIManager.GetInstance().Show(groupId, packageName, assetName, data, awaitingUIAssetName, awaitingUIExtraDuration, priority, progression, parent) as T;
            }
            #endregion

            #region Close
            /// <summary>
            /// Close the UI with the given asset name (set forceDestroy to destroy and unload it)
            /// </summary>
            /// <param name="assetName"></param>
            /// <param name="disableOnPreClose"></param>
            /// <param name="forceDestroy"></param>
            public static void Close(string assetName, bool disableOnPreClose = false, bool forceDestroy = false)
            {
                UIManager.GetInstance().Close(assetName, disableOnPreClose, forceDestroy);
            }

            /// <summary>
            /// Close all UIs in the default group (id 0), skipping any listed in withoutAssetNames
            /// </summary>
            /// <param name="disableOnPreClose"></param>
            /// <param name="forceDestroy"></param>
            /// <param name="withoutAssetNames"></param>
            public static void CloseAll(bool disableOnPreClose = false, bool forceDestroy = false, params string[] withoutAssetNames)
            {
                UIManager.GetInstance().CloseAll(DEFAULT_GROUP_ID, disableOnPreClose, forceDestroy, false, withoutAssetNames);
            }

            /// <summary>
            /// Close all UIs in the specified group, skipping any listed in withoutAssetNames (set the group id to -1 to do all groups)
            /// </summary>
            /// <param name="groupId"></param>
            /// <param name="disableOnPreClose"></param>
            /// <param name="forceDestroy"></param>
            /// <param name="withoutAssetNames"></param>
            public static void CloseAll(int groupId, bool disableOnPreClose = false, bool forceDestroy = false, params string[] withoutAssetNames)
            {
                UIManager.GetInstance().CloseAll(groupId, disableOnPreClose, forceDestroy, false, withoutAssetNames);
            }

            /// <summary>
            /// Close all UIs in all groups, skipping any listed in withoutAssetNames
            /// </summary>
            /// <param name="disableOnPreClose"></param>
            /// <param name="forceDestroy"></param>
            /// <param name="withoutAssetNames"></param>
            public static void CloseAllForAllGroups(bool disableOnPreClose = false, bool forceDestroy = false, params string[] withoutAssetNames)
            {
                UIManager.GetInstance().CloseAll(DO_ALL_GROUPS, disableOnPreClose, forceDestroy, false, withoutAssetNames);
            }

            /// <summary>
            /// Close all UIs in the default group (id 0), including those flagged with whenCloseAllToSkip, skipping any listed in withoutAssetNames
            /// </summary>
            /// <param name="disableOnPreClose"></param>
            /// <param name="forceDestroy"></param>
            /// <param name="withoutAssetNames"></param>
            public static void CloseAllAndExcluded(bool disableOnPreClose = false, bool forceDestroy = false, params string[] withoutAssetNames)
            {
                UIManager.GetInstance().CloseAll(DEFAULT_GROUP_ID, disableOnPreClose, forceDestroy, true, withoutAssetNames);
            }

            /// <summary>
            /// Close all UIs in the specified group, including those flagged with whenCloseAllToSkip, skipping any listed in withoutAssetNames (set the group id to -1 to do all groups)
            /// </summary>
            /// <param name="groupId"></param>
            /// <param name="disableOnPreClose"></param>
            /// <param name="forceDestroy"></param>
            /// <param name="withoutAssetNames"></param>
            public static void CloseAllAndExcluded(int groupId, bool disableOnPreClose = false, bool forceDestroy = false, params string[] withoutAssetNames)
            {
                UIManager.GetInstance().CloseAll(groupId, disableOnPreClose, forceDestroy, true, withoutAssetNames);
            }

            /// <summary>
            /// Close all UIs in all groups, including those flagged with whenCloseAllToSkip, skipping any listed in withoutAssetNames
            /// </summary>
            /// <param name="disableOnPreClose"></param>
            /// <param name="forceDestroy"></param>
            /// <param name="withoutAssetNames"></param>
            public static void CloseAllAndExcludedForAllGroups(bool disableOnPreClose = false, bool forceDestroy = false, params string[] withoutAssetNames)
            {
                UIManager.GetInstance().CloseAll(DO_ALL_GROUPS, disableOnPreClose, forceDestroy, true, withoutAssetNames);
            }

            /// <summary>
            /// Close the top-most UI recorded stack by stack for the given canvas in the default group (id 0)
            /// <para>Only allow close stack by stack</para>
            /// </summary>
            /// <param name="canvasName"></param>
            /// <param name="disableOnPreClose"></param>
            /// <param name="forceDestroy"></param>
            public static void CloseStackByStack(string canvasName, bool disableOnPreClose = false, bool forceDestroy = false)
            {
                UIManager.GetInstance().CloseStackByStack(DEFAULT_GROUP_ID, canvasName, disableOnPreClose, forceDestroy);
            }

            /// <summary>
            /// Close the top-most UI recorded stack by stack for the given canvas in the specified group
            /// <para>Only allow close stack by stack</para>
            /// </summary>
            /// <param name="groupId"></param>
            /// <param name="canvasName"></param>
            /// <param name="disableOnPreClose"></param>
            /// <param name="forceDestroy"></param>
            public static void CloseStackByStack(int groupId, string canvasName, bool disableOnPreClose = false, bool forceDestroy = false)
            {
                UIManager.GetInstance().CloseStackByStack(groupId, canvasName, disableOnPreClose, forceDestroy);
            }
            #endregion

            #region Reveal
            /// <summary>
            /// Reveal the hidden UI with the given asset name (only works on UIs hidden by Hide, not closed by Close)
            /// </summary>
            /// <param name="assetName"></param>
            public static void Reveal(string assetName)
            {
                UIManager.GetInstance().Reveal(assetName);
            }

            /// <summary>
            /// Reveal all hidden UIs in the default group (id 0)
            /// </summary>
            public static void RevealAll()
            {
                UIManager.GetInstance().RevealAll(DEFAULT_GROUP_ID);
            }

            /// <summary>
            /// Reveal all hidden UIs in the specified group (set the group id to -1 to do all groups)
            /// </summary>
            /// <param name="groupId"></param>
            public static void RevealAll(int groupId)
            {
                UIManager.GetInstance().RevealAll(groupId);
            }

            /// <summary>
            /// Reveal all hidden UIs in all groups
            /// </summary>
            public static void RevealAllForAllGroups()
            {
                UIManager.GetInstance().RevealAll(DO_ALL_GROUPS);
            }
            #endregion

            #region Hide
            /// <summary>
            /// Hide the UI with the given asset name (it can be shown again via Show or Reveal)
            /// </summary>
            /// <param name="assetName"></param>
            public static void Hide(string assetName)
            {
                UIManager.GetInstance().Hide(assetName);
            }

            /// <summary>
            /// Hide all UIs in the default group (id 0), skipping any listed in withoutAssetNames
            /// </summary>
            /// <param name="withoutAssetNames"></param>
            public static void HideAll(params string[] withoutAssetNames)
            {
                UIManager.GetInstance().HideAll(DEFAULT_GROUP_ID, false, withoutAssetNames);
            }

            /// <summary>
            /// Hide all UIs in the specified group, skipping any listed in withoutAssetNames (set the group id to -1 to do all groups)
            /// </summary>
            /// <param name="groupId"></param>
            /// <param name="withoutAssetNames"></param>
            public static void HideAll(int groupId, params string[] withoutAssetNames)
            {
                UIManager.GetInstance().HideAll(groupId, false, withoutAssetNames);
            }

            /// <summary>
            /// Hide all UIs in all groups, skipping any listed in withoutAssetNames
            /// </summary>
            /// <param name="withoutAssetNames"></param>
            public static void HideAllForAllGroups(params string[] withoutAssetNames)
            {
                UIManager.GetInstance().HideAll(DO_ALL_GROUPS, false, withoutAssetNames);
            }

            /// <summary>
            /// Hide all UIs in the default group (id 0), including those flagged with whenHideAllToSkip, skipping any listed in withoutAssetNames
            /// </summary>
            /// <param name="withoutAssetNames"></param>
            public static void HideAllAndExcluded(params string[] withoutAssetNames)
            {
                UIManager.GetInstance().HideAll(DEFAULT_GROUP_ID, true, withoutAssetNames);
            }

            /// <summary>
            /// Hide all UIs in the specified group, including those flagged with whenHideAllToSkip, skipping any listed in withoutAssetNames (set the group id to -1 to do all groups)
            /// </summary>
            /// <param name="groupId"></param>
            /// <param name="withoutAssetNames"></param>
            public static void HideAllAndExcluded(int groupId, params string[] withoutAssetNames)
            {
                UIManager.GetInstance().HideAll(groupId, true, withoutAssetNames);
            }

            /// <summary>
            /// Hide all UIs in all groups, including those flagged with whenHideAllToSkip, skipping any listed in withoutAssetNames
            /// </summary>
            /// <param name="withoutAssetNames"></param>
            public static void HideAllAndExcludedForAllGroups(params string[] withoutAssetNames)
            {
                UIManager.GetInstance().HideAll(DO_ALL_GROUPS, true, withoutAssetNames);
            }
            #endregion
        }

        /// <summary>
        /// Scene Resource
        /// </summary>
        public static class SRFrame
        {
            /// <summary>
            /// Toggle whether the driven updates use unscaled delta time (ignores Time.timeScale)
            /// </summary>
            public static bool ignoreTimeScale
            {
                get { return SRManager.GetInstance().ignoreTimeScale; }
                set { SRManager.GetInstance().ignoreTimeScale = value; }
            }
            /// <summary>
            /// Toggle whether the manager drives Update calls for all showing SRs
            /// </summary>
            public static bool enableUpdate
            {
                get { return SRManager.GetInstance().enableUpdate; }
                set { SRManager.GetInstance().enableUpdate = value; }
            }
            /// <summary>
            /// Toggle whether the manager drives FixedUpdate calls for all showing SRs
            /// </summary>
            public static bool enableFixedUpdate
            {
                get { return SRManager.GetInstance().enableFixedUpdate; }
                set { SRManager.GetInstance().enableFixedUpdate = value; }
            }
            /// <summary>
            /// Toggle whether the manager drives LateUpdate calls for all showing SRs
            /// </summary>
            public static bool enableLateUpdate
            {
                get { return SRManager.GetInstance().enableLateUpdate; }
                set { SRManager.GetInstance().enableLateUpdate = value; }
            }

            /// <summary>
            /// Obsolete. Use enableUpdate instead
            /// </summary>
            [System.Obsolete("Use enableUpdate instead")]
            public static bool enabledUpdate
            {
                get { return enableUpdate; }
                set { enableUpdate = value; }
            }
            /// <summary>
            /// Obsolete. Use enableFixedUpdate instead
            /// </summary>
            [System.Obsolete("Use enableFixedUpdate instead")]
            public static bool enabledFixedUpdate
            {
                get { return enableFixedUpdate; }
                set { enableFixedUpdate = value; }
            }
            /// <summary>
            /// Obsolete. Use enableLateUpdate instead
            /// </summary>
            [System.Obsolete("Use enableLateUpdate instead")]
            public static bool enabledLateUpdate
            {
                get { return enableLateUpdate; }
                set { enableLateUpdate = value; }
            }

            /// <summary>
            /// Initialize the SRManager singleton instance
            /// </summary>
            public static void InitInstance()
            {
                SRManager.GetInstance();
            }

            /// <summary>
            /// Check whether the SR with the given asset name is currently showing
            /// </summary>
            /// <param name="assetName"></param>
            /// <returns></returns>
            public static bool CheckIsShowing(string assetName)
            {
                return SRManager.GetInstance().CheckIsShowing(assetName);
            }

            /// <summary>
            /// Check whether the given SR instance is currently showing
            /// </summary>
            /// <param name="srBase"></param>
            /// <returns></returns>
            public static bool CheckIsShowing(SRBase srBase)
            {
                return SRManager.GetInstance().CheckIsShowing(srBase);
            }

            /// <summary>
            /// Check whether the SR with the given asset name is currently hidden
            /// </summary>
            /// <param name="assetName"></param>
            /// <returns></returns>
            public static bool CheckIsHiding(string assetName)
            {
                return SRManager.GetInstance().CheckIsHiding(assetName);
            }

            /// <summary>
            /// Check whether the given SR instance is currently hidden
            /// </summary>
            /// <param name="srBase"></param>
            /// <returns></returns>
            public static bool CheckIsHiding(SRBase srBase)
            {
                return SRManager.GetInstance().CheckIsHiding(srBase);
            }

            /// <summary>
            /// Check whether any SR in the default group (id 0) is hidden
            /// </summary>
            /// <returns></returns>
            public static bool CheckHasAnyHiding()
            {
                return SRManager.GetInstance().CheckHasAnyHiding(DEFAULT_GROUP_ID);
            }

            /// <summary>
            /// Check whether any SR in the specified group is hidden (set the group id to -1 to check all groups)
            /// </summary>
            /// <param name="groupId"></param>
            /// <returns></returns>
            public static bool CheckHasAnyHiding(int groupId)
            {
                return SRManager.GetInstance().CheckHasAnyHiding(groupId);
            }

            /// <summary>
            /// Check whether any SR in any group is hidden
            /// </summary>
            /// <returns></returns>
            public static bool CheckHasAnyHidingForAllGroups()
            {
                return SRManager.GetInstance().CheckHasAnyHiding(DO_ALL_GROUPS);
            }

            /// <summary>
            /// Send a refresh message with data to the specific SR
            /// </summary>
            /// <param name="refreshInfo"></param>
            public static void SendRefreshData(RefreshInfo refreshInfo)
            {
                SRManager.GetInstance().SendRefreshData(new RefreshInfo[] { refreshInfo });
            }

            /// <summary>
            /// Send refresh messages with data to the specific SRs
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
            /// Send a refresh message without data to all SRs, except those matched in specificRefreshInfos which receive their own specified data
            /// </summary>
            /// <param name="specificRefreshInfos"></param>
            public static void SendRefreshDataToAll(RefreshInfo[] specificRefreshInfos)
            {
                SRManager.GetInstance().SendRefreshDataToAll(null, specificRefreshInfos);
            }

            /// <summary>
            /// Send a refresh message with the same data to all SRs, except those matched in specificRefreshInfos which receive their own specified data
            /// </summary>
            /// <param name="data"></param>
            /// <param name="specificRefreshInfos"></param>
            public static void SendRefreshDataToAll(object data, RefreshInfo[] specificRefreshInfos)
            {
                SRManager.GetInstance().SendRefreshDataToAll(data, specificRefreshInfos);
            }

            #region GetComponent
            /// <summary>
            /// Get the top-most cached SR component of type T by its asset name
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="assetName"></param>
            /// <returns></returns>
            public static T GetComponent<T>(string assetName) where T : SRBase
            {
                return SRManager.GetInstance().GetFrameComponent<T>(assetName);
            }

            /// <summary>
            /// Get all cached SR components of type T by their asset name
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="assetName"></param>
            /// <returns></returns>
            public static T[] GetComponents<T>(string assetName) where T : SRBase
            {
                return SRManager.GetInstance().GetFrameComponents<T>(assetName);
            }
            #endregion

            #region Preload
            /// <summary>
            /// Preload an SR asset into the cache by its asset name with the default package
            /// <para>If use prefix "res#" will load from resources else will load from bundle</para>
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

            /// <summary>
            /// Preload an SR asset into the cache by its asset name with a specific package name
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="assetName"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask Preload(string packageName, string assetName, uint priority = 0, Progression progression = null)
            {
                await SRManager.GetInstance().Preload(packageName, new string[] { assetName }, priority, progression);
            }

            /// <summary>
            /// Preload multiple SR assets into the cache by their asset names with the default package
            /// </summary>
            /// <param name="assetNames"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask Preload(string[] assetNames, uint priority = 0, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await SRManager.GetInstance().Preload(packageName, assetNames, priority, progression);
            }

            /// <summary>
            /// Preload multiple SR assets into the cache by their asset names with a specific package name
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="assetNames"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask Preload(string packageName, string[] assetNames, uint priority = 0, Progression progression = null)
            {
                await SRManager.GetInstance().Preload(packageName, assetNames, priority, progression);
            }
            #endregion

            #region Show
            /// <summary>
            /// Show an SR by its asset name with the default package
            /// <para>If use prefix "res#" will load from resources else will load from bundle</para>
            /// <para>Default group id is 0</para>
            /// </summary>
            /// <param name="assetName"></param>
            /// <param name="data"></param>
            /// <param name="awaitingUIAssetName"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <param name="parent"></param>
            /// <param name="awaitingUIExtraDuration"> Sec Time </param>
            /// <returns></returns>
            public static async UniTask<SRBase> Show(string assetName, object data = null, string awaitingUIAssetName = null, uint priority = 0, Progression progression = null, Transform parent = null, float awaitingUIExtraDuration = 0f)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await SRManager.GetInstance().Show(DEFAULT_GROUP_ID, packageName, assetName, data, awaitingUIAssetName, awaitingUIExtraDuration, priority, progression, parent);
            }

            /// <summary>
            /// Show an SR by its asset name with a specific package name
            /// <para>Default group id is 0</para>
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="assetName"></param>
            /// <param name="data"></param>
            /// <param name="awaitingUIAssetName"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <param name="parent"></param>
            /// <param name="awaitingUIExtraDuration"></param>
            /// <returns></returns>
            public static async UniTask<SRBase> Show(string packageName, string assetName, object data = null, string awaitingUIAssetName = null, uint priority = 0, Progression progression = null, Transform parent = null, float awaitingUIExtraDuration = 0f)
            {
                return await SRManager.GetInstance().Show(DEFAULT_GROUP_ID, packageName, assetName, data, awaitingUIAssetName, awaitingUIExtraDuration, priority, progression, parent);
            }

            /// <summary>
            /// Show an SR in the specified group by its asset name with the default package
            /// </summary>
            /// <param name="groupId"></param>
            /// <param name="assetName"></param>
            /// <param name="data"></param>
            /// <param name="awaitingUIAssetName"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <param name="parent"></param>
            /// <param name="awaitingUIExtraDuration"></param>
            /// <returns></returns>
            public static async UniTask<SRBase> Show(int groupId, string assetName, object data = null, string awaitingUIAssetName = null, uint priority = 0, Progression progression = null, Transform parent = null, float awaitingUIExtraDuration = 0f)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await SRManager.GetInstance().Show(groupId, packageName, assetName, data, awaitingUIAssetName, awaitingUIExtraDuration, priority, progression, parent);
            }

            /// <summary>
            /// Show an SR in the specified group by its asset name with a specific package name
            /// </summary>
            /// <param name="groupId"></param>
            /// <param name="packageName"></param>
            /// <param name="assetName"></param>
            /// <param name="data"></param>
            /// <param name="awaitingUIAssetName"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <param name="parent"></param>
            /// <param name="awaitingUIExtraDuration"></param>
            /// <returns></returns>
            public static async UniTask<SRBase> Show(int groupId, string packageName, string assetName, object data = null, string awaitingUIAssetName = null, uint priority = 0, Progression progression = null, Transform parent = null, float awaitingUIExtraDuration = 0f)
            {
                return await SRManager.GetInstance().Show(groupId, packageName, assetName, data, awaitingUIAssetName, awaitingUIExtraDuration, priority, progression, parent);
            }

            /// <summary>
            /// Show an SR by its asset name with the default package and return it as T
            /// <para>If use prefix "res#" will load from resources else will load from bundle</para>
            /// <para>Default group id is 0</para>
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="assetName"></param>
            /// <param name="data"></param>
            /// <param name="awaitingUIAssetName"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <param name="parent"></param>
            /// <param name="awaitingUIExtraDuration"></param>
            /// <returns></returns>
            public static async UniTask<T> Show<T>(string assetName, object data = null, string awaitingUIAssetName = null, uint priority = 0, Progression progression = null, Transform parent = null, float awaitingUIExtraDuration = 0f) where T : SRBase
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await SRManager.GetInstance().Show(DEFAULT_GROUP_ID, packageName, assetName, data, awaitingUIAssetName, awaitingUIExtraDuration, priority, progression, parent) as T;
            }

            /// <summary>
            /// Show an SR by its asset name with a specific package name and return it as T
            /// <para>Default group id is 0</para>
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="packageName"></param>
            /// <param name="assetName"></param>
            /// <param name="data"></param>
            /// <param name="awaitingUIAssetName"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <param name="parent"></param>
            /// <param name="awaitingUIExtraDuration"></param>
            /// <returns></returns>
            public static async UniTask<T> Show<T>(string packageName, string assetName, object data = null, string awaitingUIAssetName = null, uint priority = 0, Progression progression = null, Transform parent = null, float awaitingUIExtraDuration = 0f) where T : SRBase
            {
                return await SRManager.GetInstance().Show(DEFAULT_GROUP_ID, packageName, assetName, data, awaitingUIAssetName, awaitingUIExtraDuration, priority, progression, parent) as T;
            }

            /// <summary>
            /// Show an SR in the specified group by its asset name with the default package and return it as T
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="groupId"></param>
            /// <param name="assetName"></param>
            /// <param name="data"></param>
            /// <param name="awaitingUIAssetName"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <param name="parent"></param>
            /// <param name="awaitingUIExtraDuration"></param>
            /// <returns></returns>
            public static async UniTask<T> Show<T>(int groupId, string assetName, object data = null, string awaitingUIAssetName = null, uint priority = 0, Progression progression = null, Transform parent = null, float awaitingUIExtraDuration = 0f) where T : SRBase
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await SRManager.GetInstance().Show(groupId, packageName, assetName, data, awaitingUIAssetName, awaitingUIExtraDuration, priority, progression, parent) as T;
            }

            /// <summary>
            /// Show an SR in the specified group by its asset name with a specific package name and return it as T
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="groupId"></param>
            /// <param name="packageName"></param>
            /// <param name="assetName"></param>
            /// <param name="data"></param>
            /// <param name="awaitingUIAssetName"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <param name="parent"></param>
            /// <param name="awaitingUIExtraDuration"></param>
            /// <returns></returns>
            public static async UniTask<T> Show<T>(int groupId, string packageName, string assetName, object data = null, string awaitingUIAssetName = null, uint priority = 0, Progression progression = null, Transform parent = null, float awaitingUIExtraDuration = 0f) where T : SRBase
            {
                return await SRManager.GetInstance().Show(groupId, packageName, assetName, data, awaitingUIAssetName, awaitingUIExtraDuration, priority, progression, parent) as T;
            }
            #endregion

            #region Close
            /// <summary>
            /// Close the SR with the given asset name (set forceDestroy to destroy and unload it)
            /// </summary>
            /// <param name="assetName"></param>
            /// <param name="disableOnPreClose"></param>
            /// <param name="forceDestroy"></param>
            public static void Close(string assetName, bool disableOnPreClose = false, bool forceDestroy = false)
            {
                SRManager.GetInstance().Close(assetName, disableOnPreClose, forceDestroy);
            }

            /// <summary>
            /// Close all SRs in the default group (id 0), skipping any listed in withoutAssetNames
            /// </summary>
            /// <param name="disableOnPreClose"></param>
            /// <param name="forceDestroy"></param>
            /// <param name="withoutAssetNames"></param>
            public static void CloseAll(bool disableOnPreClose = false, bool forceDestroy = false, params string[] withoutAssetNames)
            {
                SRManager.GetInstance().CloseAll(DEFAULT_GROUP_ID, disableOnPreClose, forceDestroy, false, withoutAssetNames);
            }

            /// <summary>
            /// Close all SRs in the specified group, skipping any listed in withoutAssetNames (set the group id to -1 to do all groups)
            /// </summary>
            /// <param name="groupId"></param>
            /// <param name="disableOnPreClose"></param>
            /// <param name="forceDestroy"></param>
            /// <param name="withoutAssetNames"></param>
            public static void CloseAll(int groupId, bool disableOnPreClose = false, bool forceDestroy = false, params string[] withoutAssetNames)
            {
                SRManager.GetInstance().CloseAll(groupId, disableOnPreClose, forceDestroy, false, withoutAssetNames);
            }

            /// <summary>
            /// Close all SRs in all groups, skipping any listed in withoutAssetNames
            /// </summary>
            /// <param name="disableOnPreClose"></param>
            /// <param name="forceDestroy"></param>
            /// <param name="withoutAssetNames"></param>
            public static void CloseAllForAllGroups(bool disableOnPreClose = false, bool forceDestroy = false, params string[] withoutAssetNames)
            {
                SRManager.GetInstance().CloseAll(DO_ALL_GROUPS, disableOnPreClose, forceDestroy, false, withoutAssetNames);
            }

            /// <summary>
            /// Close all SRs in the default group (id 0), including those flagged with whenCloseAllToSkip, skipping any listed in withoutAssetNames
            /// </summary>
            /// <param name="disableOnPreClose"></param>
            /// <param name="forceDestroy"></param>
            /// <param name="withoutAssetNames"></param>
            public static void CloseAllAndExcluded(bool disableOnPreClose = false, bool forceDestroy = false, params string[] withoutAssetNames)
            {
                SRManager.GetInstance().CloseAll(DEFAULT_GROUP_ID, disableOnPreClose, forceDestroy, true, withoutAssetNames);
            }

            /// <summary>
            /// Close all SRs in the specified group, including those flagged with whenCloseAllToSkip, skipping any listed in withoutAssetNames (set the group id to -1 to do all groups)
            /// </summary>
            /// <param name="groupId"></param>
            /// <param name="disableOnPreClose"></param>
            /// <param name="forceDestroy"></param>
            /// <param name="withoutAssetNames"></param>
            public static void CloseAllAndExcluded(int groupId, bool disableOnPreClose = false, bool forceDestroy = false, params string[] withoutAssetNames)
            {
                SRManager.GetInstance().CloseAll(groupId, disableOnPreClose, forceDestroy, true, withoutAssetNames);
            }

            /// <summary>
            /// Close all SRs in all groups, including those flagged with whenCloseAllToSkip, skipping any listed in withoutAssetNames
            /// </summary>
            /// <param name="disableOnPreClose"></param>
            /// <param name="forceDestroy"></param>
            /// <param name="withoutAssetNames"></param>
            public static void CloseAllAndExcludedForAllGroups(bool disableOnPreClose = false, bool forceDestroy = false, params string[] withoutAssetNames)
            {
                SRManager.GetInstance().CloseAll(DO_ALL_GROUPS, disableOnPreClose, forceDestroy, true, withoutAssetNames);
            }
            #endregion

            #region Reveal
            /// <summary>
            /// Reveal the hidden SR with the given asset name (only works on SRs hidden by Hide, not closed by Close)
            /// </summary>
            /// <param name="assetName"></param>
            public static void Reveal(string assetName)
            {
                SRManager.GetInstance().Reveal(assetName);
            }

            /// <summary>
            /// Reveal all hidden SRs in the default group (id 0)
            /// </summary>
            public static void RevealAll()
            {
                SRManager.GetInstance().RevealAll(DEFAULT_GROUP_ID);
            }

            /// <summary>
            /// Reveal all hidden SRs in the specified group (set the group id to -1 to do all groups)
            /// </summary>
            /// <param name="groupId"></param>
            public static void RevealAll(int groupId)
            {
                SRManager.GetInstance().RevealAll(groupId);
            }

            /// <summary>
            /// Reveal all hidden SRs in all groups
            /// </summary>
            public static void RevealAllForAllGroups()
            {
                SRManager.GetInstance().RevealAll(DO_ALL_GROUPS);
            }
            #endregion

            #region Hide
            /// <summary>
            /// Hide the SR with the given asset name (it can be shown again via Show or Reveal)
            /// </summary>
            /// <param name="assetName"></param>
            public static void Hide(string assetName)
            {
                SRManager.GetInstance().Hide(assetName);
            }

            /// <summary>
            /// Hide all SRs in the default group (id 0), skipping any listed in withoutAssetNames
            /// </summary>
            /// <param name="withoutAssetNames"></param>
            public static void HideAll(params string[] withoutAssetNames)
            {
                SRManager.GetInstance().HideAll(DEFAULT_GROUP_ID, false, withoutAssetNames);
            }

            /// <summary>
            /// Hide all SRs in the specified group, skipping any listed in withoutAssetNames (set the group id to -1 to do all groups)
            /// </summary>
            /// <param name="groupId"></param>
            /// <param name="withoutAssetNames"></param>
            public static void HideAll(int groupId, params string[] withoutAssetNames)
            {
                SRManager.GetInstance().HideAll(groupId, false, withoutAssetNames);
            }

            /// <summary>
            /// Hide all SRs in all groups, skipping any listed in withoutAssetNames
            /// </summary>
            /// <param name="withoutAssetNames"></param>
            public static void HideAllForAllGroups(params string[] withoutAssetNames)
            {
                SRManager.GetInstance().HideAll(DO_ALL_GROUPS, false, withoutAssetNames);
            }

            /// <summary>
            /// Hide all SRs in the default group (id 0), including those flagged with whenHideAllToSkip, skipping any listed in withoutAssetNames
            /// </summary>
            /// <param name="withoutAssetNames"></param>
            public static void HideAllAndExcluded(params string[] withoutAssetNames)
            {
                SRManager.GetInstance().HideAll(DEFAULT_GROUP_ID, true, withoutAssetNames);
            }

            /// <summary>
            /// Hide all SRs in the specified group, including those flagged with whenHideAllToSkip, skipping any listed in withoutAssetNames (set the group id to -1 to do all groups)
            /// </summary>
            /// <param name="groupId"></param>
            /// <param name="withoutAssetNames"></param>
            public static void HideAllAndExcluded(int groupId, params string[] withoutAssetNames)
            {
                SRManager.GetInstance().HideAll(groupId, true, withoutAssetNames);
            }

            /// <summary>
            /// Hide all SRs in all groups, including those flagged with whenHideAllToSkip, skipping any listed in withoutAssetNames
            /// </summary>
            /// <param name="withoutAssetNames"></param>
            public static void HideAllAndExcludedForAllGroups(params string[] withoutAssetNames)
            {
                SRManager.GetInstance().HideAll(DO_ALL_GROUPS, true, withoutAssetNames);
            }
            #endregion
        }

        /// <summary>
        /// Unity Scene
        /// </summary>
        public static class USFrame
        {
            /// <summary>
            /// Initialize the USManager singleton instance
            /// </summary>
            public static void InitInstance()
            {
                USManager.GetInstance();
            }

            /// <summary>
            /// Get the count of currently loaded scenes
            /// </summary>
            /// <returns></returns>
            public static int SceneCount()
            {
                return USManager.sceneCount;
            }

            /// <summary>
            /// Create a new empty scene with the given name and creation parameters
            /// </summary>
            /// <param name="sceneName"></param>
            /// <param name="parameters"></param>
            /// <returns></returns>
            public static Scene CreateScene(string sceneName, CreateSceneParameters parameters)
            {
                return USManager.GetInstance().CreateScene(sceneName, parameters);
            }

            /// <summary>
            /// Merge the source scene into the target scene
            /// </summary>
            /// <param name="sourceScene"></param>
            /// <param name="targetScene"></param>
            /// <returns></returns>
            public static bool MergeScenes(Scene sourceScene, Scene targetScene)
            {
                return USManager.GetInstance().MergeScenes(sourceScene, targetScene);
            }

            /// <summary>
            /// Move the given GameObject to the target scene
            /// </summary>
            /// <param name="go"></param>
            /// <param name="targetScene"></param>
            /// <returns></returns>
            public static bool MoveGameObjectToScene(GameObject go, Scene targetScene)
            {
                return USManager.GetInstance().MoveGameObjectToScene(go, targetScene);
            }

            /// <summary>
            /// Move the given GameObject to the currently active scene
            /// </summary>
            /// <param name="go"></param>
            /// <returns></returns>
            public static bool MoveGameObjectToActiveScene(GameObject go)
            {
                return USManager.GetInstance().MoveGameObjectToActiveScene(go);
            }

            /// <summary>
            /// Get the currently active scene
            /// </summary>
            /// <returns></returns>
            public static Scene GetActiveScene()
            {
                return USManager.GetInstance().GetActiveScene();
            }

            /// <summary>
            /// Set the active scene by its index in the loaded scene list
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public static bool SetActiveScene(int index)
            {
                return USManager.GetInstance().SetActiveScene(index);
            }

            /// <summary>
            /// Set the active scene by its scene name
            /// </summary>
            /// <param name="sceneName"></param>
            /// <returns></returns>
            public static bool SetActiveScene(string sceneName)
            {
                return USManager.GetInstance().SetActiveScene(sceneName);
            }

            /// <summary>
            /// Set the given scene as the active scene
            /// </summary>
            /// <param name="scene"></param>
            /// <returns></returns>
            public static bool SetActiveScene(Scene scene)
            {
                return USManager.GetInstance().SetActiveScene(scene);
            }

            /// <summary>
            /// Get the loaded scene at the given index
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public static Scene GetSceneAt(int index)
            {
                return USManager.GetInstance().GetSceneAt(index);
            }

            /// <summary>
            /// Get a loaded scene by its scene name
            /// </summary>
            /// <param name="sceneName"></param>
            /// <returns></returns>
            public static Scene GetSceneByName(string sceneName)
            {
                return USManager.GetInstance().GetSceneByName(sceneName);
            }

            /// <summary>
            /// Get a loaded scene by its build index
            /// </summary>
            /// <param name="buildIndex"></param>
            /// <returns></returns>
            public static Scene GetSceneByBuildIndex(int buildIndex)
            {
                return USManager.GetInstance().GetSceneByBuildIndex(buildIndex);
            }

            /// <summary>
            /// Get all loaded scenes, or only those matching the given scene names
            /// </summary>
            /// <param name="sceneNames"></param>
            /// <returns></returns>
            public static Scene[] GetAllScenes(params string[] sceneNames)
            {
                return USManager.GetInstance().GetAllScenes(sceneNames);
            }

            /// <summary>
            /// Get all loaded scenes, or only those matching the given build indexes
            /// </summary>
            /// <param name="buildIndexes"></param>
            /// <returns></returns>
            public static Scene[] GetAllScenes(params int[] buildIndexes)
            {
                return USManager.GetInstance().GetAllScenes(buildIndexes);
            }

            /// <summary>
            /// Set active all root game objects in the scene by its name, skipping any listed in withoutRootGameObjectNames (if there are many scenes with the same name, will process all scenes with the same name)
            /// </summary>
            /// <param name="sceneName"></param>
            /// <param name="active"></param>
            /// <param name="withoutRootGameObjectNames"></param>
            public static void SetActiveSceneRootGameObjects(string sceneName, bool active, params string[] withoutRootGameObjectNames)
            {
                USManager.GetInstance().SetActiveSceneRootGameObjects(sceneName, active, withoutRootGameObjectNames);
            }

            /// <summary>
            /// Set active all root game objects in the scene by its name in batches (processes activeObjectsPerInterval objects, then waits framesInterval frames), skipping any listed in withoutRootGameObjectNames
            /// </summary>
            /// <param name="sceneName"></param>
            /// <param name="active"></param>
            /// <param name="framesInterval"></param>
            /// <param name="activeObjectsPerInterval"></param>
            /// <param name="withoutRootGameObjectNames"></param>
            /// <returns></returns>
            public async static UniTask SetActiveSceneRootGameObjectsAsync(string sceneName, bool active, int framesInterval = 1, int activeObjectsPerInterval = 3, params string[] withoutRootGameObjectNames)
            {
                await USManager.GetInstance().SetActiveSceneRootGameObjectsAsync(sceneName, active, framesInterval, activeObjectsPerInterval, withoutRootGameObjectNames);
            }

            /// <summary>
            /// Set active all root game objects in the given scene, skipping any listed in withoutRootGameObjectNames
            /// </summary>
            /// <param name="scene"></param>
            /// <param name="active"></param>
            /// <param name="withoutRootGameObjectNames"></param>
            public static void SetActiveSceneRootGameObjects(Scene scene, bool active, string[] withoutRootGameObjectNames = null)
            {
                USManager.GetInstance().SetActiveSceneRootGameObjects(scene, active, withoutRootGameObjectNames);
            }

            /// <summary>
            /// Set active all root game objects in the given scene in batches (processes activeObjectsPerInterval objects, then waits framesInterval frames), skipping any listed in withoutRootGameObjectNames
            /// </summary>
            /// <param name="scene"></param>
            /// <param name="active"></param>
            /// <param name="framesInterval"></param>
            /// <param name="activeObjectsPerInterval"></param>
            /// <param name="withoutRootGameObjectNames"></param>
            /// <returns></returns>
            public async static UniTask SetActiveSceneRootGameObjectsAsync(Scene scene, bool active, int framesInterval = 1, int activeObjectsPerInterval = 3, params string[] withoutRootGameObjectNames)
            {
                await USManager.GetInstance().SetActiveSceneRootGameObjectsAsync(scene, active, framesInterval, activeObjectsPerInterval, withoutRootGameObjectNames);
            }

            /// <summary>
            /// Load a scene by its scene name in Single mode with the default package
            /// <para>If use prefix "build#" will load from build and else will load from bundle</para>
            /// </summary>
            /// <param name="sceneName"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask LoadSingleSceneAsync(string sceneName, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await LoadSingleSceneAsync(packageName, sceneName, LocalPhysicsMode.None, progression);
            }

            /// <summary>
            /// Load a scene by its scene name in Single mode with the default package and return the handler as T
            /// <para>If use prefix "build#" will load from build and else will load from bundle</para>
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
                return await LoadSingleSceneAsync<T>(packageName, sceneName, LocalPhysicsMode.None, progression);
            }

            /// <summary>
            /// Load a scene by its scene name in Single mode with a specific package name
            /// <para>If use prefix "build#" will load from build and else will load from bundle</para>
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="sceneName"></param>
            /// <param name="localPhysicsMode"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask LoadSingleSceneAsync(string packageName, string sceneName, LocalPhysicsMode localPhysicsMode = LocalPhysicsMode.None, Progression progression = null)
            {
                if (TryRefineBuildScenePath(sceneName, out int index))
                {
                    await USManager.GetInstance().LoadFromBuildAsync(sceneName.Substring(index), LoadSceneMode.Single, localPhysicsMode, progression);
                }
                else
                {
                    await USManager.GetInstance().LoadFromBundleAsync(packageName, sceneName, LoadSceneMode.Single, localPhysicsMode, true, 100, progression);
                }
            }

            /// <summary>
            /// Load a scene by its scene name in Single mode with a specific package name and return the handler as T
            /// <para>If use prefix "build#" will load from build and else will load from bundle</para>
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="packageName"></param>
            /// <param name="sceneName"></param>
            /// <param name="localPhysicsMode"></param>
            /// <param name="progression"></param>
            /// <returns>
            /// <para>From Build is &lt;AsyncOperation&gt;</para>
            /// <para>From Bundle is &lt;BundlePack&gt;</para>
            /// </returns>
            public static async UniTask<T> LoadSingleSceneAsync<T>(string packageName, string sceneName, LocalPhysicsMode localPhysicsMode = LocalPhysicsMode.None, Progression progression = null) where T : class
            {
                if (TryRefineBuildScenePath(sceneName, out int index))
                {
                    return await USManager.GetInstance().LoadFromBuildAsync(sceneName.Substring(index), LoadSceneMode.Single, localPhysicsMode, progression) as T;
                }
                else
                {
                    return await USManager.GetInstance().LoadFromBundleAsync(packageName, sceneName, LoadSceneMode.Single, localPhysicsMode, true, 100, progression) as T;
                }
            }

            /// <summary>
            /// Load a scene synchronously by its scene name in Single mode with the default package
            /// <para>If use prefix "build#" will load from build and else will load from bundle</para>
            /// </summary>
            /// <param name="sceneName"></param>
            /// <param name="progression"></param>
            public static void LoadSingleScene(string sceneName, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                LoadSingleScene(packageName, sceneName, LocalPhysicsMode.None, progression);
            }

            /// <summary>
            /// Load a scene synchronously by its scene name in Single mode with a specific package name
            /// <para>If use prefix "build#" will load from build and else will load from bundle</para>
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="sceneName"></param>
            /// <param name="localPhysicsMode"></param>
            /// <param name="progression"></param>
            public static void LoadSingleScene(string packageName, string sceneName, LocalPhysicsMode localPhysicsMode = LocalPhysicsMode.None, Progression progression = null)
            {
                if (TryRefineBuildScenePath(sceneName, out int index))
                {
                    USManager.GetInstance().LoadFromBuild(sceneName.Substring(index), LoadSceneMode.Single, localPhysicsMode, progression);
                }
                else
                {
                    USManager.GetInstance().LoadFromBundle(packageName, sceneName, LoadSceneMode.Single, localPhysicsMode, progression);
                }
            }

            /// <summary>
            /// Load a scene by its scene name in Additive mode with the default package
            /// <para>If use prefix "build#" will load from build and else will load from bundle</para>
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

            /// <summary>
            /// Load a scene by its scene name in Additive mode with the default package, the specified local physics mode and control over activating the root game objects after load
            /// </summary>
            /// <param name="sceneName"></param>
            /// <param name="localPhysicsMode"></param>
            /// <param name="activeRootGameObjects"></param>
            /// <param name="activateOnLoad"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask LoadAdditiveSceneAsync(string sceneName, LocalPhysicsMode localPhysicsMode = LocalPhysicsMode.None, bool activeRootGameObjects = true, bool activateOnLoad = true, uint priority = 100, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await LoadAdditiveSceneAsync(packageName, sceneName, localPhysicsMode, activeRootGameObjects, activateOnLoad, priority, progression);
            }

            /// <summary>
            /// Load a scene by its scene name in Additive mode with the default package and return the handler as T
            /// <para>If use prefix "build#" will load from build and else will load from bundle</para>
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

            /// <summary>
            /// Load a scene by its scene name in Additive mode with the default package, the specified local physics mode and control over activating the root game objects after load, returning the handler as T
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="sceneName"></param>
            /// <param name="localPhysicsMode"></param>
            /// <param name="activeRootGameObjects"></param>
            /// <param name="activateOnLoad"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask<T> LoadAdditiveSceneAsync<T>(string sceneName, LocalPhysicsMode localPhysicsMode = LocalPhysicsMode.None, bool activeRootGameObjects = true, bool activateOnLoad = true, uint priority = 100, Progression progression = null) where T : class
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await LoadAdditiveSceneAsync<T>(packageName, sceneName, localPhysicsMode, activeRootGameObjects, activateOnLoad, priority, progression);
            }

            /// <summary>
            /// Load a scene by its scene name in Additive mode with a specific package name
            /// <para>If use prefix "build#" will load from build and else will load from bundle</para>
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="sceneName"></param>
            /// <param name="activateOnLoad"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask LoadAdditiveSceneAsync(string packageName, string sceneName, bool activateOnLoad = true, uint priority = 100, Progression progression = null)
            {
                await LoadAdditiveSceneAsync(packageName, sceneName, LocalPhysicsMode.None, true, activateOnLoad, priority, progression);
            }

            /// <summary>
            /// Load a scene by its scene name in Additive mode with a specific package name, the specified local physics mode and control over activating the root game objects after load
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="sceneName"></param>
            /// <param name="localPhysicsMode"></param>
            /// <param name="activeRootGameObjects"></param>
            /// <param name="activateOnLoad"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask LoadAdditiveSceneAsync(string packageName, string sceneName, LocalPhysicsMode localPhysicsMode = LocalPhysicsMode.None, bool activeRootGameObjects = true, bool activateOnLoad = true, uint priority = 100, Progression progression = null)
            {
                if (TryRefineBuildScenePath(sceneName, out int index))
                {
                    await USManager.GetInstance().LoadFromBuildAsync(sceneName.Substring(index), LoadSceneMode.Additive, localPhysicsMode, progression);
                    if (!activeRootGameObjects) USManager.GetInstance().SetActiveSceneRootGameObjects(sceneName.Substring(index), false);
                }
                else
                {
                    await USManager.GetInstance().LoadFromBundleAsync(packageName, sceneName, LoadSceneMode.Additive, localPhysicsMode, activateOnLoad, priority, progression);
                    if (!activeRootGameObjects) USManager.GetInstance().SetActiveSceneRootGameObjects(sceneName, false);
                }
            }

            /// <summary>
            /// Load a scene by its scene name in Additive mode with a specific package name and return the handler as T
            /// <para>If use prefix "build#" will load from build and else will load from bundle</para>
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
                return await LoadAdditiveSceneAsync<T>(packageName, sceneName, LocalPhysicsMode.None, true, activateOnLoad, priority, progression);
            }

            /// <summary>
            /// Load a scene by its scene name in Additive mode with a specific package name, the specified local physics mode and control over activating the root game objects after load, returning the handler as T
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="packageName"></param>
            /// <param name="sceneName"></param>
            /// <param name="localPhysicsMode"></param>
            /// <param name="activeRootGameObjects"></param>
            /// <param name="activateOnLoad"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask<T> LoadAdditiveSceneAsync<T>(string packageName, string sceneName, LocalPhysicsMode localPhysicsMode = LocalPhysicsMode.None, bool activeRootGameObjects = true, bool activateOnLoad = true, uint priority = 100, Progression progression = null) where T : class
            {
                if (TryRefineBuildScenePath(sceneName, out int index))
                {
                    var actualSceneName = sceneName.Substring(index);
                    var handler = await USManager.GetInstance().LoadFromBuildAsync(actualSceneName, LoadSceneMode.Additive, localPhysicsMode, progression) as T;
                    if (!activeRootGameObjects) USManager.GetInstance().SetActiveSceneRootGameObjects(actualSceneName, false);
                    return handler;
                }
                else
                {
                    var handler = await USManager.GetInstance().LoadFromBundleAsync(packageName, sceneName, LoadSceneMode.Additive, localPhysicsMode, activateOnLoad, priority, progression) as T;
                    if (!activeRootGameObjects) USManager.GetInstance().SetActiveSceneRootGameObjects(sceneName, false);
                    return handler;
                }
            }

            /// <summary>
            /// Load a scene synchronously by its scene name in Additive mode with the default package
            /// <para>If use prefix "build#" will load from build and else will load from bundle</para>
            /// </summary>
            /// <param name="sceneName"></param>
            /// <param name="progression"></param>
            public static void LoadAdditiveScene(string sceneName, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                LoadAdditiveScene(packageName, sceneName, progression);
            }

            /// <summary>
            /// Load a scene synchronously by its scene name in Additive mode with the default package, the specified local physics mode and control over activating the root game objects after load
            /// </summary>
            /// <param name="sceneName"></param>
            /// <param name="localPhysicsMode"></param>
            /// <param name="activeRootGameObjects"></param>
            /// <param name="progression"></param>
            public static void LoadAdditiveScene(string sceneName, LocalPhysicsMode localPhysicsMode = LocalPhysicsMode.None, bool activeRootGameObjects = true, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                LoadAdditiveScene(packageName, sceneName, localPhysicsMode, activeRootGameObjects, progression);
            }

            /// <summary>
            /// Load a scene synchronously by its scene name in Additive mode with a specific package name
            /// <para>If use prefix "build#" will load from build and else will load from bundle</para>
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="sceneName"></param>
            /// <param name="progression"></param>
            public static void LoadAdditiveScene(string packageName, string sceneName, Progression progression = null)
            {
                LoadAdditiveScene(packageName, sceneName, LocalPhysicsMode.None, true, progression);
            }

            /// <summary>
            /// Load a scene synchronously by its scene name in Additive mode with a specific package name, the specified local physics mode and control over activating the root game objects after load
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="sceneName"></param>
            /// <param name="localPhysicsMode"></param>
            /// <param name="activeRootGameObjects"></param>
            /// <param name="progression"></param>
            public static void LoadAdditiveScene(string packageName, string sceneName, LocalPhysicsMode localPhysicsMode = LocalPhysicsMode.None, bool activeRootGameObjects = true, Progression progression = null)
            {
                if (TryRefineBuildScenePath(sceneName, out int index))
                {
                    var actualSceneName = sceneName.Substring(index);
                    USManager.GetInstance().LoadFromBuild(actualSceneName, LoadSceneMode.Additive, localPhysicsMode, progression);
                    if (!activeRootGameObjects) USManager.GetInstance().SetActiveSceneRootGameObjects(actualSceneName, false);
                }
                else
                {
                    USManager.GetInstance().LoadFromBundle(packageName, sceneName, LoadSceneMode.Additive, localPhysicsMode, progression);
                    if (!activeRootGameObjects) USManager.GetInstance().SetActiveSceneRootGameObjects(sceneName, false);
                }
            }

            /// <summary>
            /// Load a single (main) scene and additive (sub) scenes with the default package (skips loading if the single scene is already loaded)
            /// </summary>
            /// <param name="singleSceneName"></param>
            /// <param name="additiveSceneInfos"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask LoadMainAndSubScenesAsync(string singleSceneName, AdditiveSceneInfo[] additiveSceneInfos, uint priority = 100, Progression progression = null)
            {
                await LoadMainAndSubScenesAsync(singleSceneName, LocalPhysicsMode.None, additiveSceneInfos, priority, progression);
            }

            /// <summary>
            /// Load a single (main) scene and additive (sub) scenes with the default package and the specified local physics mode (skips loading if the single scene is already loaded)
            /// </summary>
            /// <param name="singleSceneName"></param>
            /// <param name="localPhysicsMode"></param>
            /// <param name="additiveSceneInfos"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask LoadMainAndSubScenesAsync(string singleSceneName, LocalPhysicsMode localPhysicsMode, AdditiveSceneInfo[] additiveSceneInfos, uint priority = 100, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await LoadMainAndSubScenesAsync(packageName, singleSceneName, localPhysicsMode, additiveSceneInfos, priority, progression);
            }

            /// <summary>
            /// Load a single (main) scene and additive (sub) scenes with a specific package name (skips loading if the single scene is already loaded)
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="singleSceneName"></param>
            /// <param name="additiveSceneInfos"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask LoadMainAndSubScenesAsync(string packageName, string singleSceneName, AdditiveSceneInfo[] additiveSceneInfos, uint priority = 100, Progression progression = null)
            {
                await LoadMainAndSubScenesAsync(packageName, singleSceneName, LocalPhysicsMode.None, additiveSceneInfos, priority, progression);
            }

            /// <summary>
            /// Load a single (main) scene and additive (sub) scenes with a specific package name and the specified local physics mode (skips loading if the single scene is already loaded)
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="singleSceneName"></param>
            /// <param name="localPhysicsMode"></param>
            /// <param name="additiveSceneInfos"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask LoadMainAndSubScenesAsync(string packageName, string singleSceneName, LocalPhysicsMode localPhysicsMode, AdditiveSceneInfo[] additiveSceneInfos, uint priority = 100, Progression progression = null)
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
                await LoadSingleSceneAsync(packageName, singleSceneName, localPhysicsMode, (float progress, float currentCount, float totalCount) =>
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
                            additiveSceneInfo.localPhysicsMode,
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
            /// Load additive (sub) scenes with the default package
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
            /// Load additive (sub) scenes with a specific package name
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
                            additiveSceneInfo.localPhysicsMode,
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
            /// Load a single (main) scene and additive (sub) scenes synchronously with the default package (skips loading if the single scene is already loaded)
            /// </summary>
            /// <param name="singleSceneName"></param>
            /// <param name="additiveSceneInfos"></param>
            /// <param name="progression"></param>
            public static void LoadMainAndSubScenes(string singleSceneName, AdditiveSceneInfo[] additiveSceneInfos, Progression progression = null)
            {
                LoadMainAndSubScenes(singleSceneName, LocalPhysicsMode.None, additiveSceneInfos, progression);
            }

            /// <summary>
            /// Load a single (main) scene and additive (sub) scenes synchronously with the default package and the specified local physics mode (skips loading if the single scene is already loaded)
            /// </summary>
            /// <param name="singleSceneName"></param>
            /// <param name="localPhysicsMode"></param>
            /// <param name="additiveSceneInfos"></param>
            /// <param name="progression"></param>
            public static void LoadMainAndSubScenes(string singleSceneName, LocalPhysicsMode localPhysicsMode, AdditiveSceneInfo[] additiveSceneInfos, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                LoadMainAndSubScenes(packageName, singleSceneName, localPhysicsMode, additiveSceneInfos, progression);
            }

            /// <summary>
            /// Load a single (main) scene and additive (sub) scenes synchronously with a specific package name (skips loading if the single scene is already loaded)
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="singleSceneName"></param>
            /// <param name="additiveSceneInfos"></param>
            /// <param name="progression"></param>
            public static void LoadMainAndSubScenes(string packageName, string singleSceneName, AdditiveSceneInfo[] additiveSceneInfos, Progression progression = null)
            {
                LoadMainAndSubScenes(packageName, singleSceneName, LocalPhysicsMode.None, additiveSceneInfos, progression);
            }

            /// <summary>
            /// Load a single (main) scene and additive (sub) scenes synchronously with a specific package name and the specified local physics mode (skips loading if the single scene is already loaded)
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="singleSceneName"></param>
            /// <param name="localPhysicsMode"></param>
            /// <param name="additiveSceneInfos"></param>
            /// <param name="progression"></param>
            public static void LoadMainAndSubScenes(string packageName, string singleSceneName, LocalPhysicsMode localPhysicsMode, AdditiveSceneInfo[] additiveSceneInfos, Progression progression = null)
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
                LoadSingleScene(packageName, singleSceneName, localPhysicsMode, (float progress, float currentCount, float totalCount) =>
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
                            additiveSceneInfo.localPhysicsMode,
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
            /// Load additive (sub) scenes synchronously with the default package
            /// </summary>
            /// <param name="additiveSceneInfos"></param>
            /// <param name="progression"></param>
            public static void LoadSubScenes(AdditiveSceneInfo[] additiveSceneInfos, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                LoadSubScenes(packageName, additiveSceneInfos, progression);
            }

            /// <summary>
            /// Load additive (sub) scenes synchronously with a specific package name
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="additiveSceneInfos"></param>
            /// <param name="progression"></param>
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
                            additiveSceneInfo.localPhysicsMode,
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
            /// Load a scene by its scene name with the given load scene mode and the default package
            /// <para>If use prefix "build#" will load from build and else will load from bundle</para>
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
                await LoadSceneAsync(packageName, sceneName, loadSceneMode, LocalPhysicsMode.None, activateOnLoad, priority, progression);
            }

            /// <summary>
            /// Load a scene by its scene name with the given load scene mode, the specified local physics mode and the default package
            /// <para>If use prefix "build#" will load from build and else will load from bundle</para>
            /// </summary>
            /// <param name="sceneName"></param>
            /// <param name="loadSceneMode"></param>
            /// <param name="localPhysicsMode"></param>
            /// <param name="activateOnLoad"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask LoadSceneAsync(string sceneName, LoadSceneMode loadSceneMode, LocalPhysicsMode localPhysicsMode = LocalPhysicsMode.None, bool activateOnLoad = true, uint priority = 100, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await LoadSceneAsync(packageName, sceneName, loadSceneMode, localPhysicsMode, activateOnLoad, priority, progression);
            }

            /// <summary>
            /// Load a scene by its scene name with the given load scene mode and the default package, returning the handler as T
            /// <para>If use prefix "build#" will load from build and else will load from bundle</para>
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="sceneName"></param>
            /// <param name="loadSceneMode"></param>
            /// <param name="activateOnLoad"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask<T> LoadSceneAsync<T>(string sceneName, LoadSceneMode loadSceneMode, bool activateOnLoad = true, uint priority = 100, Progression progression = null) where T : class
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await LoadSceneAsync<T>(packageName, sceneName, loadSceneMode, LocalPhysicsMode.None, activateOnLoad, priority, progression);
            }

            /// <summary>
            /// Load a scene by its scene name with the given load scene mode, the specified local physics mode and the default package, returning the handler as T
            /// <para>If use prefix "build#" will load from build and else will load from bundle</para>
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="sceneName"></param>
            /// <param name="loadSceneMode"></param>
            /// <param name="localPhysicsMode"></param>
            /// <param name="activateOnLoad"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns>
            /// <para>From Build is &lt;AsyncOperation&gt;</para>
            /// <para>From Bundle is &lt;BundlePack&gt;</para>
            /// </returns>
            public static async UniTask<T> LoadSceneAsync<T>(string sceneName, LoadSceneMode loadSceneMode, LocalPhysicsMode localPhysicsMode = LocalPhysicsMode.None, bool activateOnLoad = true, uint priority = 100, Progression progression = null) where T : class
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await LoadSceneAsync<T>(packageName, sceneName, loadSceneMode, localPhysicsMode, activateOnLoad, priority, progression);
            }

            /// <summary>
            /// Load a scene by its scene name with the given load scene mode and a specific package name
            /// <para>If use prefix "build#" will load from build and else will load from bundle</para>
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
                if (TryRefineBuildScenePath(sceneName, out int index))
                {
                    await USManager.GetInstance().LoadFromBuildAsync(sceneName.Substring(index), loadSceneMode, LocalPhysicsMode.None, progression);
                }
                else
                {
                    await USManager.GetInstance().LoadFromBundleAsync(packageName, sceneName, loadSceneMode, LocalPhysicsMode.None, activateOnLoad, priority, progression);
                }
            }

            /// <summary>
            /// Load a scene by its scene name with the given load scene mode, the specified local physics mode and a specific package name
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="sceneName"></param>
            /// <param name="loadSceneMode"></param>
            /// <param name="localPhysicsMode"></param>
            /// <param name="activateOnLoad"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask LoadSceneAsync(string packageName, string sceneName, LoadSceneMode loadSceneMode, LocalPhysicsMode localPhysicsMode = LocalPhysicsMode.None, bool activateOnLoad = true, uint priority = 100, Progression progression = null)
            {
                if (TryRefineBuildScenePath(sceneName, out int index))
                {
                    await USManager.GetInstance().LoadFromBuildAsync(sceneName.Substring(index), loadSceneMode, localPhysicsMode, progression);
                }
                else
                {
                    await USManager.GetInstance().LoadFromBundleAsync(packageName, sceneName, loadSceneMode, localPhysicsMode, activateOnLoad, priority, progression);
                }
            }

            /// <summary>
            /// Load a scene by its scene name with the given load scene mode and a specific package name, returning the handler as T
            /// <para>If use prefix "build#" will load from build and else will load from bundle</para>
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
                if (TryRefineBuildScenePath(sceneName, out int index))
                {
                    return await USManager.GetInstance().LoadFromBuildAsync(sceneName.Substring(index), loadSceneMode, LocalPhysicsMode.None, progression) as T;
                }
                else
                {
                    return await USManager.GetInstance().LoadFromBundleAsync(packageName, sceneName, loadSceneMode, LocalPhysicsMode.None, activateOnLoad, priority, progression) as T;
                }
            }

            /// <summary>
            /// Load a scene by its scene name with the given load scene mode, the specified local physics mode and a specific package name, returning the handler as T
            /// <para>If use prefix "build#" will load from build and else will load from bundle</para>
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="packageName"></param>
            /// <param name="sceneName"></param>
            /// <param name="loadSceneMode"></param>
            /// <param name="localPhysicsMode"></param>
            /// <param name="activateOnLoad"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns>
            /// <para>From Build is &lt;AsyncOperation&gt;</para>
            /// <para>From Bundle is &lt;BundlePack&gt;</para>
            /// </returns>
            public static async UniTask<T> LoadSceneAsync<T>(string packageName, string sceneName, LoadSceneMode loadSceneMode, LocalPhysicsMode localPhysicsMode = LocalPhysicsMode.None, bool activateOnLoad = true, uint priority = 100, Progression progression = null) where T : class
            {
                if (TryRefineBuildScenePath(sceneName, out int index))
                {
                    return await USManager.GetInstance().LoadFromBuildAsync(sceneName.Substring(index), loadSceneMode, localPhysicsMode, progression) as T;
                }
                else
                {
                    return await USManager.GetInstance().LoadFromBundleAsync(packageName, sceneName, loadSceneMode, localPhysicsMode, activateOnLoad, priority, progression) as T;
                }
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
                return await USManager.GetInstance().LoadFromBuildAsync(buildIndex, loadSceneMode, LocalPhysicsMode.None, progression);
            }

            /// <summary>
            /// Only load from build via build index (with the specified local physics mode)
            /// </summary>
            /// <param name="buildIndex"></param>
            /// <param name="loadSceneMode"></param>
            /// <param name="localPhysicsMode"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask<AsyncOperation> LoadSceneAsync(int buildIndex, LoadSceneMode loadSceneMode = LoadSceneMode.Single, LocalPhysicsMode localPhysicsMode = LocalPhysicsMode.None, Progression progression = null)
            {
                return await USManager.GetInstance().LoadFromBuildAsync(buildIndex, loadSceneMode, localPhysicsMode, progression);
            }

            /// <summary>
            /// Load a scene synchronously by its scene name with the given load scene mode and the default package
            /// <para>If use prefix "build#" will load from build and else will load from bundle</para>
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
            /// Load a scene synchronously by its scene name with the given load scene mode and a specific package name
            /// <para>If use prefix "build#" will load from build and else will load from bundle</para>
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="sceneName"></param>
            /// <param name="loadSceneMode"></param>
            /// <param name="progression"></param>
            public static void LoadScene(string packageName, string sceneName, LoadSceneMode loadSceneMode, Progression progression = null)
            {
                if (TryRefineBuildScenePath(sceneName, out int index))
                {
                    USManager.GetInstance().LoadFromBuild(sceneName.Substring(index), loadSceneMode, LocalPhysicsMode.None, progression);
                }
                else
                {
                    USManager.GetInstance().LoadFromBundle(packageName, sceneName, loadSceneMode, LocalPhysicsMode.None, progression);
                }
            }

            /// <summary>
            /// Load a scene synchronously by its scene name with the given load scene mode, the specified local physics mode and a specific package name
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="sceneName"></param>
            /// <param name="loadSceneMode"></param>
            /// <param name="localPhysicsMode"></param>
            /// <param name="progression"></param>
            public static void LoadScene(string packageName, string sceneName, LoadSceneMode loadSceneMode, LocalPhysicsMode localPhysicsMode = LocalPhysicsMode.None, Progression progression = null)
            {
                if (TryRefineBuildScenePath(sceneName, out int index))
                {
                    USManager.GetInstance().LoadFromBuild(sceneName.Substring(index), loadSceneMode, localPhysicsMode, progression);
                }
                else
                {
                    USManager.GetInstance().LoadFromBundle(packageName, sceneName, loadSceneMode, localPhysicsMode, progression);
                }
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
                return USManager.GetInstance().LoadFromBuild(buildIndex, loadSceneMode, LocalPhysicsMode.None, progression);
            }

            /// <summary>
            /// Only load from build via build index (with the specified local physics mode)
            /// </summary>
            /// <param name="buildIndex"></param>
            /// <param name="loadSceneMode"></param>
            /// <param name="localPhysicsMode"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static Scene LoadScene(int buildIndex, LoadSceneMode loadSceneMode = LoadSceneMode.Single, LocalPhysicsMode localPhysicsMode = LocalPhysicsMode.None, Progression progression = null)
            {
                return USManager.GetInstance().LoadFromBuild(buildIndex, loadSceneMode, localPhysicsMode, progression);
            }

            /// <summary>
            /// Unload scenes by their scene names (if recursively is true, unloads every loaded scene matching each name; otherwise only the last matching one)
            /// <para>If use prefix "build#" will unload from build else will unload from bundle</para>
            /// </summary>
            /// <param name="recursively"></param>
            /// <param name="sceneNames"></param>
            public static void Unload(bool recursively, params string[] sceneNames)
            {
                foreach (var sceneName in sceneNames)
                {
                    if (TryRefineBuildScenePath(sceneName, out int index))
                    {
                        USManager.GetInstance().UnloadFromBuild(recursively, sceneName.Substring(index));
                    }
                    else
                    {
                        USManager.GetInstance().UnloadFromBundle(recursively, sceneName);
                    }
                }
            }

            /// <summary>
            /// Only unload from build via build indexes (if recursively is true, unloads every loaded scene matching each build index; otherwise only the last matching one)
            /// </summary>
            /// <param name="recursively"></param>
            /// <param name="buildIndexes"></param>
            public static void Unload(bool recursively, params int[] buildIndexes)
            {
                USManager.GetInstance().UnloadFromBuild(recursively, buildIndexes);
            }

            /// <summary>
            /// Parse the scene name to distinguish the loading name rule between a Build Scene (with the "build#" prefix) and a Bundle Scene, outputting the index where the actual scene name starts
            /// </summary>
            /// <param name="sceneName"></param>
            /// <param name="actualNameIndex"></param>
            /// <returns></returns>
            internal static bool TryRefineBuildScenePath(string sceneName, out int actualNameIndex)
            {
                if (string.IsNullOrEmpty(sceneName))
                {
                    actualNameIndex = 0;
                    return false;
                }

                const string prefix = "build#";
                if (sceneName.AsSpan().StartsWith(prefix.AsSpan()))
                {
                    actualNameIndex = prefix.Length;
                    return true;
                }

                actualNameIndex = 0;
                return false;
            }
        }

        /// <summary>
        /// Clone Prefab
        /// </summary>
        public static class CPFrame
        {
            /// <summary>
            /// Initialize the CPManager singleton instance
            /// </summary>
            public static void InitInstance()
            {
                CPManager.GetInstance();
            }

            /// <summary>
            /// Preload an asset into the cache by its asset name with the default package
            /// <para>If use prefix "res#" will load from resources else will load from bundle</para>
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

            /// <summary>
            /// Preload an asset into the cache by its asset name with a specific package name
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="assetName"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask PreloadAsync(string packageName, string assetName, uint priority = 0, Progression progression = null)
            {
                await CPManager.GetInstance().PreloadAsync(packageName, new string[] { assetName }, priority, progression);
            }

            /// <summary>
            /// Preload multiple assets into the cache by their asset names with the default package
            /// </summary>
            /// <param name="assetNames"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask PreloadAsync(string[] assetNames, uint priority = 0, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await CPManager.GetInstance().PreloadAsync(packageName, assetNames, priority, progression);
            }

            /// <summary>
            /// Preload multiple assets into the cache by their asset names with a specific package name
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="assetNames"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask PreloadAsync(string packageName, string[] assetNames, uint priority = 0, Progression progression = null)
            {
                await CPManager.GetInstance().PreloadAsync(packageName, assetNames, priority, progression);
            }

            /// <summary>
            /// Preload an asset of type T into the cache by its asset name with the default package
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="assetName"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask PreloadAsync<T>(string assetName, uint priority = 0, Progression progression = null) where T : UnityEngine.Object
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await CPManager.GetInstance().PreloadAsync<T>(packageName, new string[] { assetName }, priority, progression);
            }

            /// <summary>
            /// Preload an asset of type T into the cache by its asset name with a specific package name
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="packageName"></param>
            /// <param name="assetName"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask PreloadAsync<T>(string packageName, string assetName, uint priority = 0, Progression progression = null) where T : UnityEngine.Object
            {
                await CPManager.GetInstance().PreloadAsync<T>(packageName, new string[] { assetName }, priority, progression);
            }

            /// <summary>
            /// Preload multiple assets of type T into the cache by their asset names with the default package
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="assetNames"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask PreloadAsync<T>(string[] assetNames, uint priority = 0, Progression progression = null) where T : UnityEngine.Object
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await CPManager.GetInstance().PreloadAsync<T>(packageName, assetNames, priority, progression);
            }

            /// <summary>
            /// Preload multiple assets of type T into the cache by their asset names with a specific package name
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="packageName"></param>
            /// <param name="assetNames"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask PreloadAsync<T>(string packageName, string[] assetNames, uint priority = 0, Progression progression = null) where T : UnityEngine.Object
            {
                await CPManager.GetInstance().PreloadAsync<T>(packageName, assetNames, priority, progression);
            }

            /// <summary>
            /// Preload an asset synchronously into the cache by its asset name with the default package
            /// <para>If use prefix "res#" will load from resources else will load from bundle</para>
            /// </summary>
            /// <param name="assetName"></param>
            /// <param name="progression"></param>
            public static void Preload(string assetName, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                CPManager.GetInstance().Preload(packageName, new string[] { assetName }, progression);
            }

            /// <summary>
            /// Preload an asset synchronously into the cache by its asset name with a specific package name
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="assetName"></param>
            /// <param name="progression"></param>
            public static void Preload(string packageName, string assetName, Progression progression = null)
            {
                CPManager.GetInstance().Preload(packageName, new string[] { assetName }, progression);
            }

            /// <summary>
            /// Preload multiple assets synchronously into the cache by their asset names with the default package
            /// </summary>
            /// <param name="assetNames"></param>
            /// <param name="progression"></param>
            public static void Preload(string[] assetNames, Progression progression = null)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                CPManager.GetInstance().Preload(packageName, assetNames, progression);
            }

            /// <summary>
            /// Preload multiple assets synchronously into the cache by their asset names with a specific package name
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="assetNames"></param>
            /// <param name="progression"></param>
            public static void Preload(string packageName, string[] assetNames, Progression progression = null)
            {
                CPManager.GetInstance().Preload(packageName, assetNames, progression);
            }

            /// <summary>
            /// Preload an asset of type T synchronously into the cache by its asset name with the default package
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="assetName"></param>
            /// <param name="progression"></param>
            public static void Preload<T>(string assetName, Progression progression = null) where T : UnityEngine.Object
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                CPManager.GetInstance().Preload<T>(packageName, new string[] { assetName }, progression);
            }

            /// <summary>
            /// Preload an asset of type T synchronously into the cache by its asset name with a specific package name
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="packageName"></param>
            /// <param name="assetName"></param>
            /// <param name="progression"></param>
            public static void Preload<T>(string packageName, string assetName, Progression progression = null) where T : UnityEngine.Object
            {
                CPManager.GetInstance().Preload<T>(packageName, new string[] { assetName }, progression);
            }

            /// <summary>
            /// Preload multiple assets of type T synchronously into the cache by their asset names with the default package
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="assetNames"></param>
            /// <param name="progression"></param>
            public static void Preload<T>(string[] assetNames, Progression progression = null) where T : UnityEngine.Object
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                CPManager.GetInstance().Preload<T>(packageName, assetNames, progression);
            }

            /// <summary>
            /// Preload multiple assets of type T synchronously into the cache by their asset names with a specific package name
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="packageName"></param>
            /// <param name="assetNames"></param>
            /// <param name="progression"></param>
            public static void Preload<T>(string packageName, string[] assetNames, Progression progression = null) where T : UnityEngine.Object
            {
                CPManager.GetInstance().Preload<T>(packageName, assetNames, progression);
            }

            /// <summary>
            /// Load a prefab by its asset name with the default package and instantiate a clone under the optional parent, returning it as T
            /// <para>If use prefix "res#" will load from resources else will load from bundle</para>
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

            /// <summary>
            /// Load a prefab by its asset name with a specific package name and instantiate a clone under the optional parent, returning it as T
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="packageName"></param>
            /// <param name="assetName"></param>
            /// <param name="parent"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask<T> LoadWithCloneAsync<T>(string packageName, string assetName, Transform parent = null, uint priority = 0, Progression progression = null) where T : CPBase, new()
            {
                return await CPManager.GetInstance().LoadWithCloneAsync<T>(packageName, assetName, parent, priority, progression);
            }

            /// <summary>
            /// Load a prefab by its asset name with the default package and instantiate a clone under the parent using the worldPositionStays option, returning it as T
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="assetName"></param>
            /// <param name="parent"></param>
            /// <param name="worldPositionStays"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask<T> LoadWithCloneAsync<T>(string assetName, Transform parent, bool worldPositionStays, uint priority = 0, Progression progression = null) where T : CPBase, new()
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await CPManager.GetInstance().LoadWithCloneAsync<T>(packageName, assetName, parent, worldPositionStays, priority, progression);
            }

            /// <summary>
            /// Load a prefab by its asset name with a specific package name and instantiate a clone under the parent using the worldPositionStays option, returning it as T
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="packageName"></param>
            /// <param name="assetName"></param>
            /// <param name="parent"></param>
            /// <param name="worldPositionStays"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask<T> LoadWithCloneAsync<T>(string packageName, string assetName, Transform parent, bool worldPositionStays, uint priority = 0, Progression progression = null) where T : CPBase, new()
            {
                return await CPManager.GetInstance().LoadWithCloneAsync<T>(packageName, assetName, parent, worldPositionStays, priority, progression);
            }

            /// <summary>
            /// Load a prefab by its asset name with the default package and instantiate a clone with the given position, rotation, optional parent and scale, returning it as T
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="assetName"></param>
            /// <param name="position"></param>
            /// <param name="rotation"></param>
            /// <param name="parent"></param>
            /// <param name="scale"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask<T> LoadWithCloneAsync<T>(string assetName, Vector3 position, Quaternion rotation, Transform parent = null, Vector3? scale = null, uint priority = 0, Progression progression = null) where T : CPBase, new()
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return await CPManager.GetInstance().LoadWithCloneAsync<T>(packageName, assetName, position, rotation, parent, scale, priority, progression);
            }

            /// <summary>
            /// Load a prefab by its asset name with a specific package name and instantiate a clone with the given position, rotation, optional parent and scale, returning it as T
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="packageName"></param>
            /// <param name="assetName"></param>
            /// <param name="position"></param>
            /// <param name="rotation"></param>
            /// <param name="parent"></param>
            /// <param name="scale"></param>
            /// <param name="priority"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static async UniTask<T> LoadWithCloneAsync<T>(string packageName, string assetName, Vector3 position, Quaternion rotation, Transform parent = null, Vector3? scale = null, uint priority = 0, Progression progression = null) where T : CPBase, new()
            {
                return await CPManager.GetInstance().LoadWithCloneAsync<T>(packageName, assetName, position, rotation, parent, scale, priority, progression);
            }

            /// <summary>
            /// Load a prefab synchronously by its asset name with the default package and instantiate a clone under the optional parent, returning it as T
            /// <para>If use prefix "res#" will load from resources else will load from bundle</para>
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

            /// <summary>
            /// Load a prefab synchronously by its asset name with a specific package name and instantiate a clone under the optional parent, returning it as T
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="packageName"></param>
            /// <param name="assetName"></param>
            /// <param name="parent"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static T LoadWithClone<T>(string packageName, string assetName, Transform parent = null, Progression progression = null) where T : CPBase, new()
            {
                return CPManager.GetInstance().LoadWithClone<T>(packageName, assetName, parent, progression);
            }

            /// <summary>
            /// Load a prefab synchronously by its asset name with the default package and instantiate a clone under the parent using the worldPositionStays option, returning it as T
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="assetName"></param>
            /// <param name="parent"></param>
            /// <param name="worldPositionStays"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static T LoadWithClone<T>(string assetName, Transform parent, bool worldPositionStays, Progression progression = null) where T : CPBase, new()
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return CPManager.GetInstance().LoadWithClone<T>(packageName, assetName, parent, worldPositionStays, progression);
            }

            /// <summary>
            /// Load a prefab synchronously by its asset name with a specific package name and instantiate a clone under the parent using the worldPositionStays option, returning it as T
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="packageName"></param>
            /// <param name="assetName"></param>
            /// <param name="parent"></param>
            /// <param name="worldPositionStays"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static T LoadWithClone<T>(string packageName, string assetName, Transform parent, bool worldPositionStays, Progression progression = null) where T : CPBase, new()
            {
                return CPManager.GetInstance().LoadWithClone<T>(packageName, assetName, parent, worldPositionStays, progression);
            }

            /// <summary>
            /// Load a prefab synchronously by its asset name with the default package and instantiate a clone with the given position, rotation, optional parent and scale, returning it as T
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="assetName"></param>
            /// <param name="position"></param>
            /// <param name="rotation"></param>
            /// <param name="parent"></param>
            /// <param name="scale"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static T LoadWithClone<T>(string assetName, Vector3 position, Quaternion rotation, Transform parent = null, Vector3? scale = null, Progression progression = null) where T : CPBase, new()
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                return CPManager.GetInstance().LoadWithClone<T>(packageName, assetName, position, rotation, parent, scale, progression);
            }

            /// <summary>
            /// Load a prefab synchronously by its asset name with a specific package name and instantiate a clone with the given position, rotation, optional parent and scale, returning it as T
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="packageName"></param>
            /// <param name="assetName"></param>
            /// <param name="position"></param>
            /// <param name="rotation"></param>
            /// <param name="parent"></param>
            /// <param name="scale"></param>
            /// <param name="progression"></param>
            /// <returns></returns>
            public static T LoadWithClone<T>(string packageName, string assetName, Vector3 position, Quaternion rotation, Transform parent = null, Vector3? scale = null, Progression progression = null) where T : CPBase, new()
            {
                return CPManager.GetInstance().LoadWithClone<T>(packageName, assetName, position, rotation, parent, scale, progression);
            }
        }
    }
}