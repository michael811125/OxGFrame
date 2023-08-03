# CHANGELOG

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
```
// Single
collector.GetNodeComponent<TComponent>("BindName");

// Array by multi-same node name
collector.GetNodeComponents<TComponent>("BindName");
```

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
```
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
```
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
```
// [Load asset and download from specific package]
var packageName = "OtherPackage";
await AssetPatcher.InitPackage(packageName, true, "127.0.0.1/package", "127.0.0.1/package");
var package = AssetPatcher.GetPackage(packageName);
var downloader = AssetPatcher.GetPackageDownloader(package);
Debug.Log($"Patch Size: {BundleUtility.GetBytesToString((ulong) downloader.TotalDownloadBytes)}");
await AssetLoaders.LoadAssetAsync<GameObject>(packageName, assetName);
```

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
- Fixed CacheBundle has parameter namin error issue lead to cannot found reference.

## [1.0.0] - 2022-07-10
- Initial submission for package distribution.
