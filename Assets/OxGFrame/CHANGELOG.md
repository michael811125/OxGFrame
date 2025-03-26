# CHANGELOG

## [3.2.0] - 2025-03-26
- AssetLoader  
  - Added Tests.  
  - Added support for OffsetXOR encryption/decryption - runtime passed.  
  - Added support for ChaCha20 Bundle encryption/decryption - runtime passed (Reference: [CSharp-ChaCha20-NetStandard](https://github.com/mcraiha/CSharp-ChaCha20-NetStandard/tree/netstandard20andnet6)).  
  - Added support for XXTEA Bundle encryption/decryption - runtime passed (Reference: [Razensoft.XXTEA](https://github.com/Razenpok/Razensoft.XXTEA/tree/master)).  
  - Adjusted the error function of Requester in PatchFsmStates ([oxgkit.utilities v1.4.3 or higher](https://github.com/michael811125/OxGKit/blob/master/Assets/OxGKit/Utilities/Scripts/CHANGELOG.md)).  
  - Optimized to significantly reduce GC overhead when retrieving decryption keys.  
  - Optimized DecryptionServices code for better maintainability.  
  - Optimized FileCryptogram code for better maintainability.  
  - Removed Offset random seed (does not affect encryption/decryption).  
  - Updated YooAssets to [v2.3.6](https://github.com/tuyoogame/YooAsset/releases/tag/2.3.6).  
- CoreFrame  
  - Added Tests (TODO).  
  - Added parameters to the CloseSelf method: CloseSelf(bool disabledPreClose = false, bool forceDestroy = false).  
- MediaFrame  
  - Added Tests (TODO).  
  - Added MediaObjectRemoveCacheHandler (requires [oxgkit.utilities v1.4.5 or higher](https://github.com/michael811125/OxGKit/blob/master/Assets/OxGKit/Utilities/Scripts/CHANGELOG.md)).  
  - Removed MediaLRUCache.  
- AudioFrame  
  - Added MaxRequestTimeSeconds (Timeout) configuration.  
  - Adjusted the Request method in AudioManager.  
  - Removed the RequestAudio method and unified instance creation using Requester (requires [oxgkit.utilities v1.4.5 or higher](https://github.com/michael811125/OxGKit/blob/master/Assets/OxGKit/Utilities/Scripts/CHANGELOG.md)).  
- VideoFrame  
  - Added MaxPrepareTimeSeconds (Timeout) configuration.  
  - Fixed an issue where SourceType.Url did not correctly set the source as VideoSource.Url.  
- CenterFrame  
  - Added Tests.  
  - Adjusted Acax's ErrorCallback return type to ErrorInfo -> { string url, string message, Exception exception }.  
- GSIFrame  
  - Added Tests (TODO).  
- Hotfixer  
  - Added Tests (TODO).  
- NetFrame  
  - Added Tests (TODO).  

## [3.1.3] - 2025-03-21
- Added Http.Acax timeout functionality.
- Optimized Http.Acax code.
- Fixed Http.Acax cts disposal error.

## [3.1.2] - 2025-03-19
- Organized and adjusted OxGKit plugin dependencies (see [OxGKit.Utilities v1.4.0](https://github.com/michael811125/OxGKit/blob/master/Assets/OxGKit/Utilities/Scripts/CHANGELOG.md)).

## [3.1.1] - 2025-03-18
- Fixed an issue where StreamingAssets WebRequest did not catch 404 errors.

## [3.1.0] - 2025-03-18
- Modified StreamingAssetsHelper to dynamically detect built-in resources (by checking if the BuildinCatalog.bytes file exists in the Package folder, indicating that the package is present).
- Modified the code in YooAssetBridge.
- Updated YooAsset to use the request for the BuildinCatalog.bytes file as the built-in catalog.
- Removed builtinpkgcatalog.json and the MenuItem pre-export option.

## [3.0.2] - 2025-03-14
- Modified BuildinCatalog generation and reading methods.
- Upgraded YooAsset to v2.3.5-preview (latest commit).

## [3.0.1] - 2025-03-11
- Modified SBP dependency version (v1.21.25).

## [3.0.0] - 2025-03-11
- **Hotfixer**
  - Added the OXGFRAME_HYBRIDCLR_DISABLED symbol, which skips the AOTAssemblies metadata loading process during CheckHotfix, but the initial loading of Hotfix Package and HotfixAssemblies will still be executed. (If HybridCLR functionality is disabled, it is recommended to use this symbol to skip the Hotfix process).
  - Added the Hotfixers.IsDisabled() method, which checks whether the OXGFRAME_HYBRIDCLR_DISABLED symbol is active.
- **AssetLoader**
  - **Others**
    - Added methods to encrypt/decrypt using byte[].
    - Implemented IWebDecryptionServices interface for WebGL.
	- Upgraded YooAsset to the latest ([2.3.4-preview](https://github.com/tuyoogame/YooAsset/releases/tag/2.3.4-preview)), with WebGL supporting multi-package development for versions 2.2.x and above.
  - **PatchLauncher**
    - Added SecuredStringType options (None, XORWithDummy, AES) for clearer explanation, replacing the previous bool SecuredString. It is recommended to default to None or XORWithDummy (for less GC).
    - Added PlayMode - WebGLRemoteMode, which deploys CDN remote resources and configuration files, requests remote resources, and compares the remote App version.
    - Adjusted PlayMode - WebGLMode to not require deploying CDN resources and configuration files, only requesting resources and the App version from StreamingAssets.
    - Optimized and adjusted the implementation of SecuredString.
  - **AssetPatcher**
    - Added builtinpkgcatalog.json to check for built-in packages and to decide whether to use the file system for built-in queries. The builtinpkgcatalog.json will be automatically generated during OnPreprocessBuild (due to Yoo using a new file system, differing from the previous built-in query method).
    - Added UnloadPackage method to destroy and release the package from memory, while the sandbox cache files remain.
    - Added UnloadPackageAndClearCacheFiles method, with a destroyPackage parameter to choose whether to destroy and release the package from memory.
    - Added GetBuiltinRootPath method to retrieve the root path for built-in resources.
    - Changed Release to be processed asynchronously.
    - Removed the BuiltinQueryMode option from PatchLauncher, as the new version of Yoo supports Built-in Query by default.
    - Removed PatchSetting.yooSettings, and switched to using YooAssetBridge.
  - **AssetLoaders**
    - Adjusted Unload to be processed asynchronously.
    - Adjusted Release to be processed asynchronously.
- **CoreFrame**
  - **UIFrame**
    - Optimized the generation method of UIMaskPool and UIFreezePool, creating only one object when the pool is insufficient.
    - Optimized UIMaskPool and UIFreezePool to handle the maximum number of objects in the pool and avoid pool expansion.
  - **USFrame**
    - Adjusted LoadSceneAsync and all related methods, added LocalPhysicsMode parameter.
- **MediaFrame**
  - **VideoFrame**
    - Added GetPlaySpeed() method.
    - Fixed the issue where playbackSpeed wasn't updated synchronously.
- **GSIFrame**
  - Renamed GSIManager's Start and Update methods (Obsolete), now using DriveStart and DriveUpdate instead.
- **NetFrame**
  - Added NetFrames API methods.
  - Added MirrorNetworking [TCP](https://github.com/MirrorNetworking/Telepathy), [KCP2k](https://github.com/MirrorNetworking/kcp2k) libs.
  - Added KCP (Reliable UDP) -> KcpNetProvider based on MirrorNetworking KCP2k (Reference: [F8Framework](https://github.com/TippingGame/F8Framework)).
  - Adjusted TCP to use MirrorNetworking.Telepathy (Reference: [F8Framework](https://github.com/TippingGame/F8Framework)).
  - Renamed WebsocketNetProvider to WebSocketNetProvider.
  - Renamed WebsocketNetOption to WebSocketNetOption.
  - Upgraded [UnityWebSocket](https://github.com/psygames/UnityWebSocket) to v2.8.6.
- Updated all Samples.
- Upgraded SBP dependencies to the latest (2.2.11).

## [2.14.1] - 2025-03-07
- Modified video loading preparation to avoid blocking caused by "wait until".

## [2.14.0] - 2025-02-28
- Added Bundle output rules, allowing the version path request rules to be determined by setting the Semantic Rule option.
- Modified Patch request rules to first request appConfig from the server and use the latest Version as the CDN request path.

## [2.13.3] - 2025-02-11
- Modified PatchLauncher SecureString Tooltip: If checked, complex encryption will be performed in memory (more GC). If unchecked, simple encryption will be performed in memory (less GC).
- Optimized SecureString to reduce GC by more than half.

## [2.13.2] - 2025-02-05
- Added CPBase MonoDrive feature. When enabled, it supports initialization driven by MonoBehaviour, allowing it to be directly placed in the scene without needing to be loaded through CPManager.
- Fixed the issue in FrameManager where an InvalidOperationException could occur due to collection modification during enumeration.
- Modified CPFrameDemo.
- Optimized CPManager code.

## [2.13.1] - 2025-01-09
- Fixed an issue with BindCodeAutoGenerateEditor where using ScriptableObject.CreateInstance caused an "InvalidCastException: Specified cast is not valid." in Unity 6.

## [2.13.0] - 2025-01-09
- Redefined ShowAnimation and HideAnimation methods in UIBase, marked as obsolete, and are no longer in use (must be replaced). Please replace them with OnShowAnimation and OnCloseAnimation for clearer method names. 
  - **Hint: You can use the menu item OxGFrame -> Others -> Try to update the APIs (Search and replace in all files) for automatic replacement.**

## [2.12.5] - 2025-01-02
- Modified the default value of builtinQueryMode in PatchLauncher to BuiltinFileManifest. WebRequest will be deprecated in the future, so it is recommended to switch to BuiltinFileManifest. 
- Optimized to reduce the FrameManager Update GC issue in CoreFrame.
- Optimized to accelerate the garbage collection efficiency of the Collector dictionary in FrameBase.

## [2.12.4] - 2024-12-30
- Fixed the issue with the condition triggering PackageEvents.PatchInitPatchModeFailed.SendEventMessage in PackageFsmStates.

## [2.12.3] - 2024-12-27
- Added HideSelf() method to FrameBase (SRBase, UIBase).
- Added to CoreFrames.SRFrame & CoreFrames.UIFrame:
  - Can use CheckHasAnyHidingForAllGroups() to replace CheckHasAnyHiding(-1).
  - Can use CloseAllForAllGroups() to replace CloseAll(-1).
  - Can use CloseAllAndExcludedForAllGroups() to replace CloseAllAndExcluded(-1).
  - Can use RevealAllForAllGroups() to replace RevealAll(-1).
  - Can use HideAllForAllGroups() to replace HideAll(-1).
  - Can use HideAllAndExcludedForAllGroups() to replace HideAllExcluded(-1).
- Modified access modifiers of certain methods.
- Modified CoreFrames.SRFrame & CoreFrames.UIFrame:
  - Update the rules for CheckHasAnyHiding(). By default, groupId is set to 0, and -1 indicates that all groupIds should be processed.
- Fixed RevealAll not marking hidden as false.

## [2.12.2] - 2024-11-11
- Updated UniEvent of UniFramework.
- Added constructors in EncryptionServices (Editor).

## [2.12.1] - 2024-11-08
- Added MediaFrame (Audio, Video) with an option to specify a sourceClip, handled by the prefab container for playback.
csharp
    // Audio
    public static async UniTask<AudioBase> Play(string assetName, AudioClip sourceClip, Transform parent = null, int loops = 0, float volume = 0f)
    public static async UniTask<AudioBase> Play(string packageName, string assetName, AudioClip sourceClip, Transform parent = null, int loops = 0, float volume = 0f)
	
    // Video
    public static async UniTask<VideoBase> Play(string assetName, VideoClip sourceClip, Transform parent = null, int loops = 0, float volume = 0f)
    public static async UniTask<VideoBase> Play(string packageName, string assetName, VideoClip sourceClip, Transform parent = null, int loops = 0, float volume = 0f)

- Added CoreFrame binding parameters that will compare with the parent class's binding parameters and remove any overlapping parts.
csharp
TestAUI ↓↓↓

    #region Binding Components
    [HideInInspector] public GameObject a;
    protected GameObject _b;
    
    /// <summary>
    /// Auto Binding Section
    /// </summary>
    protected override void OnAutoBind()
    {
        base.OnAutoBind();
        this.a = this.collector.GetNode("A");
        this._b = this.collector.GetNode("B");
    }
    #endregion

TestBUI : TestAUI ↓↓↓
    #region Binding Components
    protected ButtonPlus _cBtnPlus;
    
    /// <summary>
    /// Auto Binding Section
    /// </summary>
    protected override void OnAutoBind()
    {
        base.OnAutoBind();
        this._cBtnPlus = this.collector.GetNodeComponent<ButtonPlus>("C*BtnPlus");
    }
    #endregion
	
TestCUI : TestBUI ↓↓↓
    #region Binding Components
    protected GameObject _d;
    
    /// <summary>
    /// Auto Binding Section
    /// </summary>
    protected override void OnAutoBind()
    {
        base.OnAutoBind();
        this._d = this.collector.GetNode("D");
    }
    #endregion

- Added some methods to CoreFrames.USFrame.
csharp
    public static Scene CreateScene(string sceneName, CreateSceneParameters parameters)
    public static bool MergeScenes(Scene sourceScene, Scene targetScene)
    public static bool MoveGameObjectToScene(GameObject go, Scene targetScene)
    public static bool MoveGameObjectToActiveScene(GameObject go)

- Modified the CoreFrame Attrs binding format to a single-line format.
- Modified the configuration file’s default value from 127.0.0.1 to http://127.0.0.1.
- Fixed handling for MediaFrame when the clip is null.
- Fixed an issue in which the CoreFrame object returned Null when calling Show on an already showing object. It now returns the cached object instead. 

## [2.12.0] - 2024-10-24 (rule changed)
- Modified CloseAll, HideAll, RevealAll rules of method for UIFrame and SRFrame.
  - Set the default group id to 0, but if you don't want to execute based on the group id and want to do all, can set the group id to -1.
  - ex: CloseAll(-1) do all without any group id.
  - ex: HideAll(-1) do all without any group id.
  - ex: RevealAll(-1) do all without any group id.
- Modified CloseAllAndExcluded and HideAllAndExcluded to support the withoutAssetNames param.

## [2.11.12] - 2024-10-17
- Optimized the bundle decryption memory allocation method to avoid allocating excessively large memory in a single ReadAllBytes operation.

## [2.11.11] - 2024-10-15
- Added SkipToPercent method in VideoBase (You can use a percentage to jump to a specific segment of the video).
- Added SetPlaySpeed in VideoBase.
- Added CurrentRemainingLength in MediaBase.
- Fixed CurrentLength return value in MediaBase.

## [2.11.10] - 2024-10-08
- Added CoreFrames.USFrame.GetActiveScene() method.
- Added CoreFrames.USFrame.SetActiveScene(int index) method.
- Added CoreFrames.USFrame.SetActiveScene(string sceneName) method.
- Added CoreFrames.USFrame.SetActiveScene(Scene scene) method.

## [2.11.9] - 2024-09-06
- Added CoreFrames.SRFrame.HideAllAndExcluded() method.
- Added CoreFrames.UIFrame.HideAllAndExcluded() method.
- Added CoreFrames.SRFrame.CloseAllAndExcluded() method.
- Added CoreFrames.UIFrame.CloseAllAndExcluded() method.
- Added CoreFrames.UIFrame.SetupAndCheckUICanvas() method.
- Added Bundle url set plan editor.
- Added Media url set plan editor.
- Fixed UIManager dict check issue.

## [2.11.8] - 2024-08-10
- Added Reset method in Hotfixers.
- Modified nnId byte to int (NetFrame).
- Fixed DiskUtils using bug issue.

## [2.11.7] - 2024-07-30
- Added method AssetPatcher.GetPlatform.
- Modified patchconfig.json include packageVersionEncoded info (just for comparison).
- Fixed AssetPatcher -> GetAppVersion, GetPatchVersion return default value for simulate mode.

## [2.11.6] - 2024-07-29
- Added method AssetsLoaders.ReleaseBundleScenes (for additive scenes).
- Added method AssetPatcher.Release (mainly for yooasset destroy).
- Removed OnApplicationQuit process in PatchLauncher, use manual to call AssetPatcher.Release before the application quits.

## [2.11.5] - 2024-07-22
- Fixed Unload after yooasset destroyed bug issue.
- Fixed same package in PackageFsmStates
- Fixed packageInfos.Length == 0 in PackageFsmStates bug issue (throw Exception).

## [2.11.4] - 2024-06-17
- Fixed WebGL build bug error (close #17).
- Fixed Hotfix unity TextAsset get_bytes bug error.

## [2.11.3] - 2024-05-19
- Added new methods for sync loading of scene.
csharp
    public static void LoadSingleScene(string sceneName)
    public static void LoadSingleScene(string packageName, string sceneName)
    public static void LoadAdditiveScene(string sceneName)
    public static void LoadAdditiveScene(string sceneName, bool activeRootGameObjects = true)
    public static void LoadAdditiveScene(string packageName, string sceneName)
    public static void LoadAdditiveScene(string packageName, string sceneName, bool activeRootGameObjects = true)
    public static void LoadScene(string sceneName, LoadSceneMode loadSceneMode)
    public static void LoadScene(string packageName, string sceneName, LoadSceneMode loadSceneMode)
    public static Scene LoadScene(int buildIndex, LoadSceneMode loadSceneMode = LoadSceneMode.Single)

- Added new methods to combine load a single scene and additive scenes.
csharp
    public static async UniTask LoadMainAndSubScenesAsync(string singleSceneName, AdditiveSceneInfo[] additiveSceneInfos, uint priority = 100, Progression progression = null)
    public static async UniTask LoadMainAndSubScenesAsync(string packageName, string singleSceneName, AdditiveSceneInfo[] additiveSceneInfos, uint priority = 100, Progression progression = null)
    public static void LoadMainAndSubScenes(string singleSceneName, AdditiveSceneInfo[] additiveSceneInfos)
    public static void LoadMainAndSubScenes(string packageName, string singleSceneName, AdditiveSceneInfo[] additiveSceneInfos)

- Added new methods to load additive scenes.
csharp
    public static async UniTask LoadSubScenesAsync(AdditiveSceneInfo[] additiveSceneInfos, uint priority = 100, Progression progression = null)
    public static async UniTask LoadSubScenesAsync(string packageName, AdditiveSceneInfo[] additiveSceneInfos, uint priority = 100, Progression progression = null)
    public static void LoadSubScenes(AdditiveSceneInfo[] additiveSceneInfos)
    public static void LoadSubScenes(string packageName, AdditiveSceneInfo[] additiveSceneInfos)


## [2.11.2] - 2024-05-18
- Added burlconfig.conf can export cipher type (If the output is ciphertext, it will automatically determine whether to execute with the decryption process).
  - Cipher process.
  - Plaintext process.  

## [2.11.1] - 2024-05-17
- Added HT2XORPlus encryption stronger than HT2XOR (Recommended).
- Updated YooAsset to v2.1.2 (new commits).

## [2.11.0] - 2024-05-12
- Updated YooAsset commits.
- Updated Scriptable Build Pipeline to v2.1.3 (Unity).
- Added YooAsset can support RawFile encryption.
- Added Hotfixer can new PackageInfoWithBuild to CheckHotfix method.
- Optimized Support Hotfix to be loaded using other threads (WebGL is not supported).

## [2.10.5] - 2024-04-17
- Fixed SimpleDiskUtils compile error issue on WIN.

## [2.10.4] - 2024-04-08
- Removed UniTask from built-in.
  - Please use install via git URL instead (Package Manager)

https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask


## [2.10.3] - 2024-04-01
- Added CacheType to AudioManager, which will be used for caching when AudioBase is using the request method.
- Modified AudioBase to not rely on the methods of the OxGKit.Utilities' Requester.
- Modified Acax encoding type (use UTF-8).

## [2.10.2] - 2024-03-19
- Fixed When scene load with suspend (activateOnLoad = false) cannot return BundlePack correctly .
csharp
    // activateOnLoad = false (suspend)
    var bundlePack = await CoreFrames.USFrame.LoadAdditiveSceneAsync<BundlePack>("MyPackage", "SceneName", false, 100);

- Added UnsuspendScene in BundlePack.
csharp
    // Method 1
    var bundlePack = await CoreFrames.USFrame.LoadAdditiveSceneAsync<BundlePack>("MyPackage", "SceneName", false, 100);
    bundlePack.GetOperationHandle<SceneHandle>().UnSuspend();
    
    // Method 2
    var bundlePack = await CoreFrames.USFrame.LoadAdditiveSceneAsync<BundlePack>("MyPackage", "SceneName", false, 100);
    bundlePack.UnsuspendScene();

- Modified set #ROOTNAMESPACE# symbol in script templates.
- Optimized remove useless interfaces.

## [2.10.1] - 2024-03-14
- Fixed AudioBase and VideoBase to avoid executing Stop again in OnDestroy if they are being destroyed.
- Added MediaLRUCache to handle least recently used audio or video will be removed (Optimize memory).
- Added binding access modifier rules in BindCodeSetting, split by "$".
  - _Node@MyObj*Txt$public (UIBase, SRBase).
  - ~Node@MyObj*Txt$public (CPBase).

// Default is protected, no set required (depends on the first element in BindCodeSetting)
UIBase, SRBase:
_Node@MyObj*Txt
CPBase:
~Node@MyObj*Txt

// Assign access modifier
UIBase, SRBase:
_Node@MyObj*Txt$public
_Node@MyObj*Txt$private
CPBase:
~Node@MyObj*Txt$public
~Node@MyObj*Txt$private

- Added binding attr rules in BindCodeSetting, pattern is "\[\]" (supported multi-attrs \[\]\[\]...).
  - _Node@MyObj*Txt$public\[hi\] (UIBase, SRBase).
  - ~Node@MyObj*Txt$public\[hi\] (CPBase).

[hi] = [HideInInspector]
UIBase, SRBase:
_Node@MyObj*Txt$public[hi]
CPBase:
~Node@MyObj*Txt$public[hi]

[sf] = [SerializeField]
UIBase, SRBase:
_Node@MyObj*Txt[sf]
CPBase:
~Node@MyObj*Txt[sf]

- Modified UIFrameDemo.
- Modified SRFrameDemo.
- Modified CPFrameDemo.

## [2.10.0] - 2024-03-07
- Added BuiltinQueryMode option on PatchLauncher, can switch built-in query mode.
csharp
    public enum BuiltinQueryMode
    {
        WebRequest,
        BuiltinFileManifest,
        BuiltinFileManifestWithCRC
    }

- Added Auto save binding content to script for UIBase, SRBase, CPBase.
csharp
    // Specific pattern
    #region Binding Components
    #endregion


## [2.9.16] - 2024-02-20
- Updated YooAsset commits.
- Added InitPackage in AssetPatcher.
csharp
    /// <summary>
    /// Init package by type
    /// </summary>
    /// <param name="packageInfo"></param>
    /// <param name="autoUpdate"></param>
    /// <returns></returns>
    public static async UniTask<bool> InitPackage(PackageInfoWithBuild packageInfo, bool autoUpdate = false)

- Modified PackageOperation initialize procedure by manual.
csharp
public class PackageOperation
{
    /// <summary>
    /// Ready operation for initialize (after events added)
    /// </summary>
    public void Ready()
}

- Modified BundleDLCDemo sample.
- Modified SetDefaultPackage determine.
- Removed unuse samples from DiskUtils.

## [2.9.15] - 2024-02-19
- Updated UniTask to v2.5.3.
- Added DriveUpdate methods in CPBase (can call update by other PlayerLoop).
csharp
    public void DriveUpdate(float dt) => this.HandleUpdate(dt);
    public void DriveFixedUpdate(float dt) => this.HandleFixedUpdate(dt);
    public void DriveLateUpdate(float dt) => this.HandleLateUpdate(dt);


## [2.9.14] - 2024-02-02
- Modified PackageOperation user callback events, can reference itself in callback.
csharp
public class PackageOperation
{
    public delegate void OnPatchRepairFailed(PackageOperation itself);
    public delegate void OnPatchInitPatchModeFailed(PackageOperation itself);
    public delegate void OnPatchVersionUpdateFailed(PackageOperation itself);
    public delegate void OnPatchManifestUpdateFailed(PackageOperation itself);
    public delegate void OnPatchCheckDiskNotEnoughSpace(PackageOperation itself, int availableMegabytes, ulong patchTotalBytes);
    public delegate void OnPatchDownloadFailed(PackageOperation itself, string fileName, string error);
}


## [2.9.13] - 2024-02-01
- Updated yooasset to [v2.1.1](https://github.com/tuyoogame/YooAsset/releases/tag/2.1.1).
- Added [DiskUtils by keerthik](https://github.com/keerthik/simple-disk-utils) third party in AssetLoader module (not supported WebGL).
- Added Check available disk space in patch and package download step (not supported WebGL).
  - Must add PatchEvents.PatchCheckDiskNotEnoughSpace in patchEvents to handle it (checkout BundleDemo).
- Added CheckDiskSpace flag setting on PatchLauncher inspector.
- Added Can set user event handler to PackageOperation.
csharp
public class PackageOperation
{
    /// <summary>
    /// Enable or disable disk space check procedure (default is true)
    /// </summary>
    public bool checkDiskSpace = true;

    public OnPatchRepairFailed onPatchRepairFailed;
    public OnPatchInitPatchModeFailed onPatchInitPatchModeFailed;
    public OnPatchVersionUpdateFailed onPatchVersionUpdateFailed;
    public OnPatchManifestUpdateFailed onPatchManifestUpdateFailed;
    public OnPatchCheckDiskNotEnoughSpace onPatchCheckDiskNotEnoughSpace;
    public OnPatchDownloadFailed onPatchDownloadFailed;
    
    public void UserTryPatchRepair()
    public void UserTryInitPatchMode()
    public void UserTryPatchVersionUpdate()
    public void UserTryPatchManifestUpdate()
    public void UserTryCreateDownloader()
}

- Modified UIBase method name [#1adf602](https://github.com/michael811125/OxGFrame/commit/1adf6028aa980169732ea1a40f2d8df1b8c4584e) (Replace all below).

method ShowAnime => ShowAnimation

method HideAnime => HideAnimation

delegate AnimeEndCb => AnimationEnd

param animeEndCb => animationEnd


## [2.9.12] - 2024-01-16
- Added CoreFrames.UIFrame.GetStackByStackCount method.
csharp
    public static int GetStackByStackCount(string canvasName)
    
    public static int GetStackByStackCount(int groupId, string canvasName)


**How to use it**
csharp
    if (Keyboard.current.escapeKey.wasReleasedThisFrame)
    {
        if (CoreFrames.UIFrame.GetStackByStackCount(groupId, canvasName) > 0)
        {
            CoreFrames.UIFrame.CloseStackByStack(groupId, canvasName);
        }
        else
        {
            Debug.Log("Open Esc Menu!!!");
        }
    }

- Modified UI NodeType name (the original settings will not be changed).
csharp
    public enum NodeType
    {
        Fixed,        // Normal => Fixed
        TopFixed,     // Fixed => TopFixed
        Popup,        // Same
        TopPopup,     // Same
        LoadingPopup, // Same
        SysPopup,     // Same
        TopSysPopup,  // Same
        AwaitingPopup // Same
    }


## [2.9.11] - 2024-01-09
- Optimized NetFrame.
- Added TcpNetOption.
- Added WebsocketNetOption.
- Modified NetOption.
- Modified SetResponseHandler to SetResponseBinaryHandler and SetResponseMessageHandler.
- Modified typo SetOutReciveAction to SetOutReceiveAction.
- Renamed TcpSock to TcpNetProvider.
- Renamed WebSock to WebsocketNetProvider.
- Renamed method CloseSocket to Close.
- Renamed ISocket to INetProvider.

## [2.9.10] - 2023-12-28
- Updated YooAsset to v2.1.0 ([CHANGELOG](https://github.com/tuyoogame/YooAsset/releases/tag/2.1.0)).
- Organized coding style ([Wiki](https://github.com/michael811125/OxGFrame/wiki/Coding-Style)).

## [2.9.9] - 2023-12-18
- Added PackageOperation feature, can download packages more easier (please checkout BundleDLCDemo).
csharp
    // Factory Mode
    public static PackageOperation CreateOperation(string groupName, PackageInfoWithBuild packageInfo, bool skipDownload = false)
    public static PackageOperation CreateOperation(string groupName, PackageInfoWithBuild[] packageInfos, bool skipDownload = false)

    // Use Example
    var packageOperations = new PackageOperation[]
    {
        new PackageOperation
        (
            "DLC Package 1",
            new DlcPackageInfoWithBuild()
            {
                buildMode = BuildMode.ScriptableBuildPipeline,
                packageName = "Dlc1Package",
                dlcVersion = "latest"
            },
            false
        ),
        new PackageOperation
        (
            "DLC Pacakge 2",
            new DlcPackageInfoWithBuild()
            {
                buildMode = BuildMode.ScriptableBuildPipeline,
                packageName = "Dlc2Package",
                dlcVersion = "latest"
            },
            false
        )
    };

- Added BundleDLCDemo for operate PackageOperation.
- Modified params to PackageInfoWithBuild.
csharp
    public abstract class PackageInfoWithBuild
    {
        [Tooltip("Only for EditorSimulateMode")]
        public BuildMode buildMode;
        public string packageName;
        
        /// <summary>
        /// Custom host server
        /// </summary>
        [HideInInspector]
        public string hostServer = null;
        /// <summary>
        /// Custom fallback host server
        /// </summary>
        [HideInInspector]
        public string fallbackHostServer = null;
        public IBuildinQueryServices builtinQueryService = null;
        public IDeliveryQueryServices deliveryQueryService = null;
        public IDeliveryLoadServices deliveryLoadService = null;
    }

- Removed method InitCustomPackage from AssetPatcher.

## [2.9.8] - 2023-12-08
- Added Generate binding code rule (MethodType: Manual, Auto, default is Auto).
csharp
    #region Binding Components
    protected Image _bgImg;
    protected Text _msgTxt;
    
    /// <summary>
    /// Auto Binding Section
    /// </summary>
    protected override void OnAutoBind()
    {
        base.OnAutoBind();
        this._bgImg = this.collector.GetNodeComponent<Image>("Bg*Img");
        this._msgTxt = this.collector.GetNodeComponent<Text>("Msg*Txt");
    }
    #endregion


## [2.9.7] - 2023-12-07
- Modified repair procedure (Supports patch repair during download).
- Modified BundleDemo in Samples.
- Fixed AssetPatcher flags bug issue (IsCheck(), IsRepair(), IsDone()).

## [2.9.6] - 2023-12-06
- Fixed a bug where the RawFileBuildPipeline download file was missing an extension.

## [2.9.5] - 2023-12-05
- Added AppPackageInfoWithBuild and DlcPackageInfoWithBuild (BuildMode can be selected when executing on SimulateMode).
csharp
    [Serializable]
    public class PackageInfoWithBuild
    {
        public BuildMode buildMode;
        public string packageName;
    }
    
    [Serializable]
    public class AppPackageInfoWithBuild : PackageInfoWithBuild
    {
    }
    
    [Serializable]
    public class DlcPackageInfoWithBuild : PackageInfoWithBuild
    {
        public bool withoutPlatform = false;
        [Tooltip("If version is null or empty will auto set newset package version by date")]
        public string dlcVersion;
    }

- Fixed unprocessed request error bug issue.

## [2.9.4] - 2023-11-06
- Updated UniMachine ([Blackboard](https://github.com/gmhevinci/UniFramework/commit/3ea882c2fc8d5314c51e66fa35579324d0c7a73c)).
- Updated UniTask to [v2.5.0](https://github.com/Cysharp/UniTask/releases/tag/2.5.0).
- Renamed GetAllScene to GetAllScenes in CoreFrames.USFrame.
csharp
    public static Scene[] GetAllScenes(params string[] sceneNames)
    public static Scene[] GetAllScenes(params int[] buildIndexes)

- Added CoreFrames.UIFrame, CoreFrames.SRFrame can control updates (**enabledUpdate defaults is true, else are false**).
csharp
    public static bool ignoreTimeScale
    public static bool enabledUpdate
    public static bool enabledFixedUpdate
    public static bool enabledLateUpdate

- Added FixedUpdate, LateUpdate behaviour to FrameBase (UIBase, SRBase, CPBase).
csharp
    protected override void OnFixedUpdate(float dt) { }
    
    protected override void OnLateUpdate(float dt) { }

- Added SetActiveSceneRootGameObjects method in CoreFrames.USFrame (Can control the active of scene root GameObjects).
csharp
    public static void SetActiveSceneRootGameObjects(string sceneName, bool active, string[] withoutRootGameObjectNames = null)
    public static void SetActiveSceneRootGameObjects(Scene scene, bool active, string[] withoutRootGameObjectNames = null)

- 
- Added overload methods in CoreFrames.USFrame (LoadAdditiveSceneAsync has activeRootGameObjects param, can control the active of root GameObjects after the scene is loaded).
csharp
    public static async UniTask LoadAdditiveSceneAsync(string sceneName, bool activeRootGameObjects = true, bool activateOnLoad = true, uint priority = 100, Progression progression = null)
    public static async UniTask<T> LoadAdditiveSceneAsync<T>(string sceneName, bool activeRootGameObjects = true, bool activateOnLoad = true, uint priority = 100, Progression progression = null) where T : class
    public static async UniTask LoadAdditiveSceneAsync(string packageName, string sceneName, bool activeRootGameObjects = true, bool activateOnLoad = true, uint priority = 100, Progression progression = null)
    public static async UniTask<T> LoadAdditiveSceneAsync<T>(string packageName, string sceneName, bool activeRootGameObjects = true, bool activateOnLoad = true, uint priority = 100, Progression progression = null) where T : class

- Optmized code.

## [2.9.3] - 2023-10-31
- Optimized SecureString.
- Added disposable interface for DecryptInfo.

## [2.9.2] - 2023-10-30
- Improved DecryptArgs safety.
- Modified SecureString saltSize and dummySize minimum (at least 2 bytes).
- Optimized DecryptArgs parsing.

## [2.9.1] - 2023-10-29
- Added SecureString, StringWithDummy for DecryptArgs (SecureMemory).
- Added DecryptInfo class (You can decide whether to do memory encryption).
- Modified PatchLauncher.

## [2.9.0] - 2023-10-29
- Upgraded YooAsset to v2.0.3-preview ([CHANGELOG](https://github.com/tuyoogame/YooAsset/releases/tag/2.0.3-preview)).
- Fixed DefaultYooFolderName issue.
- Fixed UIManager CloseAll with groupId bug issue.
- Added [AllowCloseStackByStack] setting for UI.
- Added CloseStackByStack method in CoreFrames.UIFrame.
csharp
    // Only allow close stack by stack
    public static void CloseStackByStack(string canvasName, bool disablePreClose = false, bool forceDestroy = false)
    public static void CloseStackByStack(int groupId, string canvasName, bool disablePreClose = false, bool forceDestroy = false)

- Added Preset DLC packages list to PatchLauncher (can set preset DLC packages).
- Added **withoutPlatform** param to DlcInfo class (can export default dlc request path wihtout platform).
- Added PatchSetting ScriptableObject for Bundle (can modify configs name by self).
- Added Priority param for any load async methods (can controls loading priority by YooAsset).
- Optimized cached AppConfig in Runtime.
- Modified BindCodeSetting init param and use region to group binding code (more clear).
csharp
    #region Binding Components
    protected GameObject _openBtn;
    
    /// <summary>
    /// Don't forget to call via OnBind method
    /// </summary>
    protected void InitBindingComponents()
    {
        this._openBtn = this.collector.GetNode("OpenBtn");
    }
    #endregion

- Changed CoreFrame.FrameBase method name (OnInit change to OnCreate).
csharp
    // Replace all OnInit() to OnCreate()
    public override void OnCreate()
    {
        /**
         * Do Somethings Init Once In Here
         */
    }

- Chagned GSIFrame.GSIBase method name (OnInit change to OnCreate).
csharp
    // Replace all OnInit() to OnCreate()
    public async override UniTask OnCreate()
    {
        /* Do Somethings OnCreate once in here */
    }

- Removed methods from AssetPatcher.
csharp
    // Removed
    public static string GetPresetAppPackageNameByIdx(int idx)


## [2.8.2] - 2023-09-28
- Upgraded YooAsset to v1.5.6-preview ([CHANGELOG](https://github.com/tuyoogame/YooAsset/releases/tag/1.5.6-preview)).

## [2.8.1] - 2023-09-25
- Upgraded YooAsset to v1.5.5-preview.

## [2.8.0] - 2023-09-24
- Upgraded UniTask to v2.4.1.
- Added BundlePlan for Export Bundle And Config Generator editor (can save bundle plans).
- Modified SRFrameDemo.
- Modified CPFrameDemo (added Factory mode example).
- Renamed AgencyCenter to CenterFrame (**If already use must replace all AgencyCenter to CenterFrame and recompile**).

## [2.7.13] - 2023-09-22
- Added default constructor for Loggers.

## [2.7.12] - 2023-09-22
- Fixed Acax GET method bug issue, download buffer is null reference if without body data.
- Modified Acax header args and body args can be null.

## [2.7.11] - 2023-09-12
- Added IsRetryActive for RetryCounter (retryCount > 0).
- Fixed RetryCounter reference bug issue.
- Modified RefinePath methods use SubString to process.
- Modified params of SendRefreshData method (use RefreshInfo struct).
csharp
    // CoreFrames.UIFrame & CoreFrames.SRFrame
    public static void SendRefreshData(RefreshInfo refreshInfo)
    public static void SendRefreshData(RefreshInfo[] refreshInfos)
    public static void SendRefreshDataToAll(RefreshInfo[] specificRefreshInfos = null)


## [2.7.10] - 2023-09-11
- Added more check methods for AssetObject.
csharp
    public bool IsRawFileOperationHandleValid()
    public bool IsSceneOperationHandleValid()
    public bool IsAssetOperationHandleValid()


- Modified RefineResourcesPath and RefineBuildScenePath solution.
- Modified AssetObject to optmize determines.
- Modified CacheBundle determines and use package.CheckLocationValid of YooAsset to make sure asset does exist.
- Optimized AssetLoaders (CacheResource and CacheBundle).

## [2.7.9] - 2023-09-10
- Fixed AssetLoader retry counter determine bug issue.
- Fixed CacheBundle wrong unload type while doing retry.
- Modified Progression name of params (corrected reqSize to currentCount, totalSize to totalCount).

## [2.7.8] - 2023-09-09
- Added retry counter for AssetLoader (can set maxRetryCount via API).
- Modified AcaxAsync can return text.
- Optimized Hotfixer.
- Optimized code.

## [2.7.7] - 2023-08-27
- Updated YooAsset to 1.5.4-perview.
- Installed OxGKit.LoggingSystem for all modules log print.
  - Add https://github.com/michael811125/OxGKit.git?path=Assets/OxGKit/LoggingSystem/Scripts to Package Manager

**Note: Must install OxGKit.LoggingSystem, becuase all modules log are dependent it.**

## [2.7.6] - 2023-08-24
- Optimized AudioBase and VideoBase of MediaFrame update behaviour call by MediaManager.
- Updated YooAsset new commit files.

## [2.7.5] - 2023-08-23
- Optimized UIBase and SRBase of CoreFrame update behaviour call by FrameManager.
- Optimized CPBase of CoreFrame update behaviour, if need to update have to call by self to drive.
  - Added DriveSelfUpdate(float dt) method in CPBase (drive by self).
csharp
public class NewTplCP : CPBase
{
    private void Update()
    {
        this.DriveSelfUpdate(Time.deltaTime);
    }
}


## [2.7.4] - 2023-08-15
- Fixed DeliveryQueryService is null bug issue.
- Modified InitAppPackage, InitDlcPackage, InitCustomPackage param of methods, add IDeliveryQueryServices interface.

## [2.7.3] - 2023-08-14
- Updated YooAsset (new commits).
- Updated part of UniFramework.
#### CoreFrames (UIFrame, SRFrame)
- Added API.
csharp
    public static void SendRefreshData(string assetName, object data = null)
    
    public static void SendRefreshData(string[] assetNames, object[] data = null)	

#### AssetPatcher
- Added API.
Common
csharp
    public struct DownloadInfo
    {
        public int totalCount;
        public ulong totalBytes;
    }
    
    public static ResourcePackage[] GetPackages(params string[] packageNames)
    	
    public static async UniTask<bool> BeginDownloadWithCombineDownloaders(ResourceDownloaderOperation[] downloaders, OnDownloadSpeedProgress onDownloadSpeedProgress = null, OnDownloadError onDownloadError = null)

Get Downloader   
csharp
    // All
    public static ResourceDownloaderOperation[] GetDownloadersWithCombinePackages(ResourcePackage[] packages)
    
    public static ResourceDownloaderOperation[] GetDownloadersWithCombinePackages(ResourcePackage[] packages, int maxConcurrencyDownloadCount, int failedRetryCount)
    
    // Tags
    public static ResourceDownloaderOperation[] GetDownloadersWithCombinePackagesByTags(ResourcePackage[] packages, params string[] tags)
    
    public static ResourceDownloaderOperation[] GetDownloadersWithCombinePackagesByTags(ResourcePackage[] packages, int maxConcurrencyDownloadCount, int failedRetryCount, params string[] tags)
    
    // AssetNames
    public static ResourceDownloaderOperation[] GetDownloadersWithCombinePackagesByAssetNames(ResourcePackage[] packages, params string[] assetNames)
    
    public static ResourceDownloaderOperation[] GetDownloadersWithCombinePackagesByAssetNames(ResourcePackage[] packages, int maxConcurrencyDownloadCount, int failedRetryCount, params string[] assetNames)
    
    // AssetInfos
    public static ResourceDownloaderOperation[] GetDownloadersWithCombinePackagesByAssetInfos(ResourcePackage[] packages, params AssetInfo[] assetInfos)
    
    public static ResourceDownloaderOperation[] GetDownloadersWithCombinePackagesByAssetInfos(ResourcePackage[] packages, int maxConcurrencyDownloadCount, int failedRetryCount, params AssetInfo[] assetInfos)

Begin Download
csharp
    // All
    public static async UniTask<bool> BeginDownloadWithCombinePackages(ResourcePackage[] packages, OnDownloadSpeedProgress onDownloadSpeedProgress = null, OnDownloadError onDownloadError = null)
    
    public static async UniTask<bool> BeginDownloadWithCombinePackages(ResourcePackage[] packages, int maxConcurrencyDownloadCount, int failedRetryCount, OnDownloadSpeedProgress onDownloadSpeedProgress = null, OnDownloadError onDownloadError = null)
    
    // Tags
    public static async UniTask<bool> BeginDownloadWithCombinePackagesByTags(ResourcePackage[] packages, string[] tags = null, OnDownloadSpeedProgress onDownloadSpeedProgress = null, OnDownloadError onDownloadError = null)
    
    public static async UniTask<bool> BeginDownloadWithCombinePackagesByTags(ResourcePackage[] packages, int maxConcurrencyDownloadCount, int failedRetryCount, string[] tags = null, OnDownloadSpeedProgress onDownloadSpeedProgress = null, OnDownloadError onDownloadError = null)
    
    // AssetNames
    public static async UniTask<bool> BeginDownloadWithCombinePackagesByAssetNames(ResourcePackage[] packages, string[] assetNames = null, OnDownloadSpeedProgress onDownloadSpeedProgress = null, OnDownloadError onDownloadError = null)
    
    public static async UniTask<bool> BeginDownloadWithCombinePackagesByAssetNames(ResourcePackage[] packages, int maxConcurrencyDownloadCount, int failedRetryCount, string[] assetNames = null, OnDownloadSpeedProgress onDownloadSpeedProgress = null, OnDownloadError onDownloadError = null)
    
    // AssetInfos
    public static async UniTask<bool> BeginDownloadWithCombinePackagesByAssetInfos(ResourcePackage[] packages, AssetInfo[] assetInfos = null, OnDownloadSpeedProgress onDownloadSpeedProgress = null, OnDownloadError onDownloadError = null)
    
    public static async UniTask<bool> BeginDownloadWithCombinePackagesByAssetInfos(ResourcePackage[] packages, int maxConcurrencyDownloadCount, int failedRetryCount, AssetInfo[] assetInfos = null, OnDownloadSpeedProgress onDownloadSpeedProgress = null, OnDownloadError onDownloadError = null)

Get Download Info   
csharp    
    // All
    public static DownloadInfo GetDownloadInfoWithCombinePackages(ResourcePackage[] packages)
    
    // Tags
    public static DownloadInfo GetDownloadInfoWithCombinePackagesByTags(ResourcePackage[] packages, params string[] tags)
    
    // AssetNames
    public static DownloadInfo GetDownloadInfoWithCombinePackagesByAssetNames(ResourcePackage[] packages, params string[] assetNames)
    
    // AssetInfos
    public static DownloadInfo GetDownloadInfoWithCombinePackagesByAssetInfos(ResourcePackage[] packages, params AssetInfo[] assetInfos)


## [2.7.2] - 2023-08-07
- Added CheckPackageHasAnyFilesInLocal(string packageName).
- Added GetPackageSizeInLocal(string packageName).

## [2.7.1] - 2023-08-05
- Added Default API in GSIManagerBase (protected GetInstance() method).
csharp
    public static int GetCurrentId()
    
    public static U GetStage<U>() where U : GSIBase
    
    public static U GetStage<U>(int id) where U : GSIBase
    
    public static void AddStage<U>() where U : GSIBase, new()
    
    public static void AddStage<U>(int id) where U : GSIBase, new()
    
    public static void AddStage(int id, GSIBase gameStage)
    
    public static void ChangeStage<U>(bool force = false) where U : GSIBase
    
    public static void ChangeStage(int id, bool force = false)
    
    public static void Start()
    
    public static void Update(float dt = 0.0f)

- Added Default API in CenterBase (removed Default API from subtype).
csharp
    public static void Add<UClass>() where UClass : TClass, new()
    
    public static void Add<UClass>(int id) where UClass : TClass, new()
    
    public static void Add(int id, TClass @class)
    
    public static UClass Find<UClass>() where UClass : TClass
    
    public static UClass Find<UClass>(int id) where UClass : TClass


## [2.7.0] - 2023-08-03
- Added AssetPatcher.GetPresetAppPackages() method.
- Added [Sort Tail Rules (A-Z)] ContextMenu for BindCodeSetting.
- Upgraded YooAsset to v1.5.3-preview.
- Fixed FixBuildTasks add back (accidentally deleted).
- Modified PatchLauncher preset app packages, will collect all preset app packages in main download.
- Modified CoreFrames.USFrame.LoadSingleSceneAsync params of method (removed activateOnLoad and priority).
- Renamed PatchLauncher param skipCreateMainDownloader to skipMainDownload.
- Changed AssetPatcher.GetPackageNames() => AssetPatcher.GetPresetAppPackageNames().
- Changed AssetPatcher.GetPackageNameByIdx => AssetPatcher.GetPresetAppPackageNameByIdx().
- Removed AssetPatcher.GetPackage(int idx) method.
- Removed AssetPatcher.InitAppPackage(int idx, bool autoUpdate = false) method.
- Removed AssetPatcher.UpdatePackage(int idx) method.
- Removed AssetPatcher.SetDefaultPackage(int idx) method.
- Removed AssetPatcher.SwitchDefaultPackage(int idx) method.
- Optimized AssetLoader (CacheResource and CacheBundle determines).

## [2.6.2] - 2023-07-25
- Added GetRawFilePath (where is raw file local save path).
  - AssetLoaders.GetRawFilePathAsync(string assetName).
  - AssetLoaders.GetRawFilePathAsync(string packageName, string assetName).
  - AssetLoaders.GetRawFilePath(string assetName).
  - AssetLoaders.GetRawFilePath(string packageName, string assetName).
- Modified PatchLauncher init procedure.

## [2.6.1] - 2023-07-25
- Fixed query service on iOS or MacOSX bug issue (Convert url to www path).

## [2.6.0] - 2023-07-21
- Upgraded YooAsset to v1.5.2-preview.
- Added StreamingAssetsHelper from YooAsset sample.
- Added WebGL Mode only for WebGL build.
- Fixed WebGL query service bug issue.
- Modified QueryServices.

## [2.5.5] - 2023-07-18
- Fixed load scene frome Build suspend bug issue.

## [2.5.4] - 2023-07-14
- Modified BindCodeSetting pluralize feature, can edit EndRemoveCount and EndPluralTxt.
  - ex: If text end is y => EndRemoveCount = 1, EndPluralTxt = 'ies'.

## [2.5.3] - 2023-07-13
- Added RemoteServices implements default HostServers.
- Modified BindCodeSetting rules, can custom plural end to adjust grammar.
- Upgraded YooAsset to v1.5.1.

## [2.5.2] - 2023-07-09
- Removed OxGFrame.Utility.
  - Install OxGKit.Utilities Add https://github.com/michael811125/OxGKit.git?path=Assets/OxGKit/Utilities/Scripts to Package Manager.
  - If already use must reassign using OxGKit.Utilities (replace namespace prefix OxGFrame.Utility to OxGKit.Utilities).

**Note: (Reinstall) Remove OxGFrame from Package Manager, and then install OxGKit.Utilities first, Finally reinstall OxGFrame**

## [2.5.1] - 2023-07-07
- Added HT2Xor (hKey, tKey, jKey), time complexity is O((length >> 1) + 2).
  - [For Encrypt] Do hKey and tKey first, after do jKey.
  - [For Decrypt] Do jKey first, after do hKey and tKey.
- Added Bundle Cryptogram Utility (For Verify).
- Deprecated HTXor.

## [2.5.0] - 2023-07-07
- Upgraded YooAsset to v1.5.0 (Breaking Changes).
- Added GetLocalSandboxRootPath() => .../yoo.
- Added GetLocalSandboxPackagePath(string packageName) => .../yoo/\<PackageName\>.
- Added GetBuiltinPackagePath(string packageName) => .../StreamingAssets/\<PackageName\>.
- Modified QueryServices.
- Modified Requester methods has (bool cached) param. The cached param depends on InitCache.
- Modified Audio has requestCached option (depends on Requester.InitCacheForAudio).
- Optimized url configs request (url configs will cached).
- Renamed AssetPatcher.InitPackage to AssetPatcher.InitAppPackage.
- Removed GetLocalSandboxPath method.
- Removed InitCustomPackage by idx method.

## [2.4.4] - 2023-07-03
- Added auto generate stop end symbol feature (Shift+E).
- Fixed when bind detect stop end symbol bug issue.

## [2.4.3] - 2023-06-30
- Added FixBuildTasks for SBP.
- Fixed use SBP to build SpriteAtlas occurred redundancy bug.
- Fixed LoadAsset<T>, LoadAssetAsync<T> bug (cannot cast generic type correctly for Sprite).
- Modified PreloadAsset<T>, PreloadAssetAsync<T> can use generic to cast type.
- Reduced demo size.

## [2.4.2] - 2023-06-28
- Added MissingScriptsFinder.
- Added SymlinkUtility.

## [2.4.1] - 2023-06-28
- Added material for MaskSetting.
- Optimized UIMask.

## [2.4.0] - 2023-06-28
- Updated YooAsset to v1.14.17.
- Added Create Settings MenuItem option.
- Added BindCodeSetting for auto generate bind code feature (Shift+B).
  - _Node@MyObj*Img (use * pointer component type)
- Added symbols for patch play mode are OXGFRAME_OFFLINE_MODE, OXGFRAME_HOST_MODE (force change play mode on build).
- Added RectTransformAdjuster for adjust anchors (Shift+R).

## [2.3.3] - 2023-06-20
- Fixed GetHostServerUrl and GetFallbackHostServerUrl url including redundant symbols bug.

## [2.3.2] - 2023-06-20
- Modified Editor windows key saver including project path.

**Note: If occurred any errors, please restart Unity Project again**

## [2.3.1] - 2023-06-19
- Added bind collector can use GetNodeComponent(string nodeName) to get component.
csharp
// Single
collector.GetNodeComponent<TComponent>("BindName");

// Array by multi-same node name
collector.GetNodeComponents<TComponent>("BindName");


## [2.3.0] - 2023-06-17 (Breaking Changes)
- Combined EventCenter and APICenter group in AgencyCenter. Unified inherit CenterBase<TCenter, TClass>, can more easier maintenance and extension.
  - If already use must reassign using and rename class.
    - CenterBase<TCenter, TClass> (using OxGFrame.AgencyCenter).
	- EventBase (using OxGFrame.AgencyCenter.EventCenter).
	- APIBase (using OxGFrame.AgencyCenter.APICenter).
	
## [2.2.7] - 2023-06-17
- Fixed when use HybridCLR will not found default constructor issue (FsmStates).

## [2.2.6] - 2023-06-16
- Added ARCCache and LRUCache in Utility (using OxGFrame.Utility.Cacher).
- Added Cacher for Requester (If want to active cache must Init).

## [2.2.5] - 2023-06-16
- Added template prefabs for CPFrame (Transform and RectTransform).
- Added virtual OnShow() without params for CPBase, and then deprecated OnShow(object obj).
- Added default API for EventCenter, because use protected access modifier for GetInstance().
  - Use Find to GetEvent.
- Added default API for APICenter, because use protected access modifier for GetInstance().
  - Use Find to GetAPI.
- Renamed method OpenSub() => OnPreShow().
- Renamed method CloseSub() => OnPreClose().

## [2.2.4] - 2023-06-15
- Optimized unload hotfix files after loaded (release memory).

## [2.2.3] - 2023-06-15
- Fixed GSIFrame assembly wrong name issue.

## [2.2.2] - 2023-06-08
- Added Requester in Utility.
- Optimized AssetLoader and MediaFrame to use Requester of Utility.

## [2.2.1] - 2023-06-06
- Modified default Mixer for AudioManager (import from PackageManager sample).
  - changes Fight name to Interact.

## [2.2.0] - 2023-06-02 (Breaking Changes)
- Added HybridCLR hotfix solution (using OxGFrame.Hotfixer).
  - Checkout HotfixerDemo (Import sample from Unity Package Manager).
- Modified NetManager access modifier use NetFrames to call API (using OxGFrame.NetFrame).
- Renamed UserEvents (Patch) => PatchEvents (must manually rename).
- Renamed GSFrame => SRFrame = SR (Scene Resource).
  - If already use, must manually rename and setup again.
- Renamed EPFrame => CPFrame = CP (Clone Prefab).
  - If already use, must manually rename and setup again
- Optimized ButtonPlus and redesigned long click.
- Optimized code.

## [2.1.5] - 2023-05-30
- Added Singleton Utility.
- Can install via git (Organized Samples).

## [2.1.4] - 2023-05-15
- Added DownloadSpeedCalculator (using OxGFrame.AssetLoader.Utility).
csharp
var packageName = "DlcPackage";
bool isInitialized = await AssetPatcher.InitDlcPackage(packageName, "dlcVersion", true);
if (isInitialized)
{
    var package = AssetPatcher.GetPackage(packageName);
    var downloader = AssetPatcher.GetPackageDownloader(package);
    // Create a DownloadSpeedCalculator to helps calculate download speed
    var downloadSpeedCalculator = new DownloadSpeedCalculator();
    downloader.OnDownloadProgressCallback = downloadSpeedCalculator.OnDownloadProgress;
    downloadSpeedCalculator.onDownloadSpeedProgress = (totalCount, currentCount, totalBytes, currentBytes, speedBytes) =>
    {
        /*
         * Display download info
         */
    };
}

- Added BeakpointFileSizeThreshold on PatchLauncher (default is 20 MB).

## [2.1.3] - 2023-05-13
- Modified patch repair procedure (the repair only delete main default package cache files and local files).
- Modified UnloadPackageAndClearCacheFiles() method has return value (true = Successed, false = Failed).
- Added retry patch repair events (PatchEvents and UserEvents).

## [2.1.2] - 2023-05-13
- Added UI supports **Reverse Changes** feature (can auto hide last UI and show back).
- Added InitInstance() method for CoreFrames and MediaFrames.
- Added audio and video can adjust volume via API param.
- Added GetAudioSource() method for AudioBase.
- Added GetVideoPlayer() method for VideoBase.
- Added IsResourcePack() and IsBundlePack() methods for AssetObject. 
- Modified API description and optimized params.
- Modified all Monobehaviour Managers will group together.
- Optimized cache.
- Optimized Examples code.

## [2.1.1] - 2023-05-08
- Fixed init package bug issue (only HostMode needs burlconfig.conf).
- Optimized code (editor).

## [2.1.0] - 2023-05-06
- Added version encode methods in BundleUtility.
- Added semantic version check rule (supported compare rule is X.Y.Z or X.Y).
- Added export individual DLC package option in Bundle Config Generator (allow export specific DLC package version and download by specific DLC package version).
  - Specific DLC Version: CDN/productName/platform/DLC/DlcPackage/v1.0...v1.1 (InitDlcPackage assign dlcVersion is "v1.0" or "v1.1")
  - Newest DLC Version: CDN/productName/platform/DLC/DlcPackage/newest (InitDlcPackage assign fixed dlcVersion is "newest")
- Added DLC request default path rule (CDN/productName/platform/DLC/packageName).
- Added unload package and clear cache files method (can unload specific DLC package and clear files from Sandbox).
- Added local sandbox query service for DLC.
- Modified InitPackage and UpdatePackage methods return value are bool to make sure init or update status.
- Extended GetPatchVersion has (encode, encodeLength, separator) params.
- Fixed OfflineMode patch version is null or empty issue.
- Fixed HostMode local app config won't update to write issue.

## [2.0.6] - 2023-05-04
- Added HostMode can skip download step (force download while playing).
- Extended AssetPatcher methods.
- Modified patch repair procedure.
- Fixed patch download progress handler bug (removed test code).

## [2.0.5] - 2023-05-02
- Renamed AssetPatcher.GetPackageDownloaderByTags to AssetPatcher.GetPackageDownloader.
- Extended CoreFrames.USFrame methods (LoadSingleSceneAsync and LoadAdditiveSceneAsync).
- Extended load asset and download from specific package.
- Optimized code.
csharp
// [Load asset and download from specific package]
var packageName = "OtherPackage";
await AssetPatcher.InitPackage(packageName, true, "127.0.0.1/package", "127.0.0.1/package");
var package = AssetPatcher.GetPackage(packageName);
var downloader = AssetPatcher.GetPackageDownloader(package);
Debug.Log($"Patch Size: {BundleUtility.GetBytesToString((ulong) downloader.TotalDownloadBytes)}");
await AssetLoaders.LoadAssetAsync<GameObject>(packageName, assetName);


## [2.0.4] - 2023-04-26
- Renamed BeginInit be OnInit.
- Combined InitOnceComponents and InitOnceEvents be OnBind (after bind will call this method) method.
- Removed InitOnceComponents method.
- Removed InitOnceEvents method.

## [2.0.3] - 2023-04-25
- Fixed unload issue (When ref is zero will call package.UnloadUnusedAssets()).
- Fixed progression calculate bug.
- Fixed play mode init procedure bug.
- Fixed cannot get GSIBase id issue.
- Extended CoreFrames.UIFrame.Show<T>.
- Extended CoreFrames.GSFrame.Show<T>.

## [2.0.2] - 2023-04-24
- Renamed method ShowAnim => ShowAnime.
- Renamed method HideAnim => HideAnime.
- Renamed delegate AnimEndCb => AnimeEndCb.
- Fixed WebSock file.

## [2.0.1] - 2023-04-23
- Renamed TcpSocket => TcpSock.
- Renamed Websock => WebSock.

# NEW

## [2.0.0] - 2023-04-22 (Breaking Changes)
- New OxGFrame Version (API Changed).
- Added YooAsset.
- Optimized code.

---

# OLD (deprecated)

## [1.9.1] - 2022-12-23
- Modified CacheBundle use GetAsset to load asset from AB.

## [1.9.0] - 2022-12-12
- Added file verification for Downloader (when downloaded file md5 compare with server file md5 is inconsistent will redownload again) to ensure file integrity.
- Added force unload param for Unload method withou ref count (CacheBundle & CacheResource).
- Added GetAssetAsync (LoadAssetAsync) method for BundlePack class.
- Optimized CacheBundle (Load AssetBundle from Memory and Stream).

## [1.8.2] - 2022-11-18
- Fixed request StreamingAssets path failed issue on iOS (because missing file://).
  - Run bundle mode on iOS (test passed)
  - Run audio request from StreamingAssets on iOS (test passed).
- Optimized code.

## [1.8.1] - 2022-11-17
- Fixed request StreamingAssets path failed issue on MacOSX (because missing file://).
  - Run bundle mode on MacOSX (test passed)
  - Run audio request from StreamingAssets on MacOSX (test passed).

## [1.8.0] - 2022-11-16
- Added MacOSX preprocessor tag in BundleConfig and CacheBundle (test passed).
- Modified burlcfg.txt store key name => unified key name is "store_link".

## [1.7.1] - 2022-11-14
- Fixed BundleDistributor offline mode bug issue. When local bcfg already exists will copy bcfg from built-in to override it without update.

## [1.7.0] - 2022-11-13
- Added offline option for bundle in "BundleSetup" to set it. if checked only request config and bundle from StreamingAssets (Built-in).
- Added "Offline_Mode.zip" and "Patch_Mode.zip" in OxGFrame/AssetLoader/Example/BundleDemo for BundleDemo to run offline or patch.
- Added prefab template for MediaFrame (right-click Create/OxGFrame/MediaFrame.../Audio or /Video to create prefab template).
- Modified access modifier of parameters of AudioBase and VideoBase.
- Modified audio tracks volume of "MasterMixer" provided by AudioManager (default audio tracks = BGM, General, Fight, Voice, Atmosphere).

## [1.6.2] - 2022-11-12
- Fixed AudioBase if checked AutoEndToStop will hide audio length field issue.

## [1.6.1] - 2022-11-10
- Fixed Downloader if gets rdlMd5 from PlayerPrefs is empty will download file directly, but actually file is exist leads to file index write error issue.

## [1.6.0] - 2022-11-10 (Breaking Changes)
- Added Stop method for Utility Timer.
- Added ButtonPlus in Utility (implements Unity Button).
- Added OnDestroyAndUnload for MediaBase (can choose when stop and destroy to unload or not).
- Added ForceUnload() methods for MediaFrame (AudioManager & VideoManager).
- Added HTXOR cryptogram for AssetBundle (Head-Tail XOR).
- Added Donwloader supports slice mode for large file (depends on limits set from BundleSetup, default is 16 MB).
- Added RETRYING_DOWNLOAD execute status for BundleDistributor (use switch case instead).
- Added BundleSetup for execution settings (If you use AssetBundle, it must be set in the scene first).
- Added SharpZipLib.1.4.0 dll plugins in AssetLoader for Compressor.
- Added Compressor for AssetLoader (supports sync and async).
- Added zip and unzip procedure for AssetLoader (only first install will download zip pack, but depends on how do you build bundles).
- Added ExecuteStatus.WAITING_FOR_CONFIRM_TO_DOWNLOAD for BundleDistributor (waiting for user to confirm download).
- Added ExecuteStatus.UNZIP_PATCH for BundleDistributor (unzip step).
- Added Bundle Compressor Editor (use async to process).
- Added BundleUtility (using AssetLoader.Utility), move some methods to BundleUtility.
- Added EditorCoroutine in AssetLoader.Editor for custome BuildTool.
- Added LoadingFlags for CacheResource.
- Added custom manifest name in BundleSetup.
- Modified BundleDistributor Editor.
- Modified Downloader Progression params (float progress, int dlCount, long dlBytes, int dlSpeed, ulong totalBytes), use BundleUtility converts to string.
- Modified Repair method return true or false. Respectively means true = is clear data completed, false = clear failed, also removed params and must call check by manual.
- Modified extract some methods to BundleUtility.
- Modified win platform use request config from StreamingAssets (InApp).
- Changed default name "b_cfg" to "bcfg" (b = bundle, cfg = config).
- Changed default name "r_cfg" to "rcfg" (r = record, cfg = config).
- Changed default name "bundle_cfg.txt" to "burlcfg.txt" (b = bundle, url = URL, cfg = config).
- Changed default name "media_cfg.txt" to "murlcfg.txt" (m = media, url = URL, cfg = config).
- Changed BundleDistributor.ExecuteStatus (NO_NEED_TO_UPDATE_PATCH) to BundleDistributor.ExecuteStatus (ALREADY_UP_TO_DATE).
- Changed FrameManager HasInAllCache method name to HasStackInAllCache.
- Changed FrameManager GetFromAllCache method name to PeekStackFromAllCache.
- Fixed CoreFrame when use cacher to load has ref count issue (UIManager, GSManager implements from FrameManager).
- Fixed AudioBaseEditor when press Preload button won't save issue (set dirty).
- Fixed MediaFrame unload issue (cannot unload correctly).
- Fixed MediaManager has release and unload issue (cannot release and unload correctly).
- Fixed VideoBase RenderTexture mode cannot release RenderTexture correctly issue.
- Fixed NetNode heart beat timer reset timing wrong issue.
- Fixed NetNode changes timer pause use stop instead.
- Fixed load manifest issue (when already up-to-date will unload manifest).
- Fixed CacheBundle load dependency bundle name doesn't lowercase issue.
- Fixed BundleUtility bytes to string precision (float).
- Fixed Downloader when file size error will auto download again but has file path error issue.
- Fixed BundleDistributor when repair has bug, because must reset configs.
- Fixed use none cryptogram for bundle has stream issue (file sharing violation).
- Removed default manifest name in BundleConfig (changes set by BundleSetup, default are imf (Internal Manifest) and emf (External Manifest)).
- Optimized AssetLoader (Bundle & Resources).
- Optimized bundle decrypt efficiency.
- Optimized CoreFrame.
- Optimized MediaFrame.
- Renamed GSIFrame GStage => GameStageBase, GStageManager => GameStageManagerBase, **delete old version GSIFrame first, and then import new GISFrame and changes new class name for implements class**.

## [1.5.1] - 2022-09-05
- Optimized MediaFrame (Audio, Video).
- Removed ButtonPlus Component.

## [1.5.0] - 2022-08-21 (Breaking Changes)
- Organized category separate APICenter, EventCenter, Utility.
- Renamed GSM => GStageManager (Game Stage Manager).
- Renamed AssetLoader name of methods.
  - PreloadInCache => Preload
  - ReleaseFromCache => Unload
  - ReleaseCache => Release
- Added HeartBeatAction, OutReciveAction, ReconnectAcion for NetNode (Callback handler).

## [1.4.0] - 2022-08-18 (Breaking Changes)
- Renamed namespace add OxGFrame front of AnyFrame (ex: CoreFrame => OxGFrame.CoreFrame).
- Renamed EntityFrame => EPFrame (Entity Prefab).
- Added APICenter TplScript (Right-Click to Create).
- Added EventCenter TplScript (Right-Click to Create).
- Added Network Example for NetFrame.
- Added GSI (Game System Integration) Example for GSIFrame.
- Added APICenter, EventCenter, GSI singleton (Implement in base class).

#### 【Note】Namespace already chagned (add OxGFrame front of AnyFrame).

## [1.3.0] - 2022-08-18
- Added ButtonPlus Component (inherit UGUI Button).
- Added NodePool Component (GameObject Pool).
- Added Canvas, CanvasScaler, GraphicRaycaster for UICanvas(Init on Awake).
- Added Canvas, GraphicRaycaster for UIBase (Init on Awake).
- Fixed When UI Canvas render mode is WorldSpace occurred transform issue.
- Fixed Bundle Offset Type of Cryptogram encrypt random issue, extend can set random seed to fixed random values.

## [1.2.0] - 2022-08-15
- Added UISafeAreaAdapter Component.
- Modified When install new app version will compare StreamingAssets cfg app version to Local cfg app version, if different will write StreamingAssets cfg app version to Local cfg.
- Modified when generate cfg file must set product name (Not load from Application.productName).
- Fixed when request server config file and request download bundle will try catch to throw error.

## [1.1.0] - 2022-07-25
- Added USFrame (Unity Scene Manager) to controll unity scene, also supported AssetBundle.
- Modified BundleMode (Bundle Stream Mode) only for build, can switch true or false from BundleConfig by define.

## [1.0.2] - 2022-07-12
- Modified right-click feature to group CoreFrame and GSIFrame in OxGFrame path.

## [1.0.1] - 2022-07-11
- Fixed CacheBundle has parameter naming error issue lead to cannot found reference.

## [1.0.0] - 2022-07-10
- Initial submission for package distribution.
