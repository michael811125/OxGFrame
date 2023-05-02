# CHANGELOG

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
