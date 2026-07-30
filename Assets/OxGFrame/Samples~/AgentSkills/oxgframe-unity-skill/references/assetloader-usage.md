# AssetLoader Usage (AssetLoaders + AssetPatcher)

Namespace `OxGFrame.AssetLoader`. Two facades: `AssetLoaders` (load/unload) and `AssetPatcher` (patch/download/packages). Everything is UniTask-based. `Progression` = `delegate void Progression(float progress, float currentCount, float totalCount)`.

Prefix rule: asset name starting with `res#` loads from `Resources`; otherwise from the bundle package. Overloads without `packageName` use `AssetPatcher.GetDefaultPackageName()`.

## AssetLoaders — assets

```csharp
// Preload (also: string[] overloads, packageName overloads, sync Preload<T> without priority)
await AssetLoaders.PreloadAssetAsync<GameObject>(assetName, priority, progression);

// Load
GameObject go = await AssetLoaders.LoadAssetAsync<GameObject>(assetName);
GameObject go2 = await AssetLoaders.LoadAssetAsync<GameObject>(packageName, assetName);
T asset = AssetLoaders.LoadAsset<T>(assetName);                  // sync

// Instantiate (8 transform overloads each: (pos, rot[, parent]) / (parent[, worldPositionStays]))
GameObject clone = await AssetLoaders.InstantiateAssetAsync<GameObject>(assetName, parent);

// Cache & unload — ALWAYS pair manual loads with unload
bool cached = AssetLoaders.HasInCache(assetName);
AssetLoaders.UnloadAsset(assetName);          // ref-counted; (assetName, forceUnload: true) bypasses refcount
AssetLoaders.ReleaseAssets();                 // release everything
```

Canonical load/instantiate/destroy pattern (BundleDemo):

```csharp
GameObject go = await AssetLoaders.LoadAssetAsync<GameObject>(this.assetName);
if (go != null) Instantiate(go, this.container.transform);
// on despawn:
Destroy(instance);
AssetLoaders.UnloadAsset(this.assetName);     // must call in pairs with load
```

## AssetLoaders — group cacher (lists / feature scopes)

Same API set with a leading `int groupId` — use for UI lists and per-feature scopes (a practical id is `this.GetHashCode()` of the owner):

```csharp
var tex = await AssetLoaders.LoadAssetAsync<Texture2D>(groupId, packageName, assetName);
AssetLoaders.UnloadAsset(groupId, assetName);
AssetLoaders.UnloadAssets(groupId);           // release the whole group on feature exit
```

## AssetLoaders — scenes (bundle only; prefer CoreFrames.USFrame in game code)

```csharp
BundlePack pack = await AssetLoaders.LoadSceneAsync(sceneName, LoadSceneMode.Single, activateOnLoad, priority, progression);
await AssetLoaders.LoadSingleSceneAsync(packageName, sceneName);
await AssetLoaders.LoadAdditiveSceneAsync(sceneName);
AssetLoaders.UnloadScene(sceneName, recursively: false);   // additive only
AssetLoaders.ReleaseScenes();
```

## AssetLoaders — raw files (bundle only)

```csharp
string text = await AssetLoaders.LoadRawFileAsync<string>(assetName);   // T = string | byte[]
byte[] data = await AssetLoaders.LoadRawFileAsync<byte[]>(packageName, assetName);
string path = await AssetLoaders.GetRawFilePathAsync(assetName);
AssetLoaders.UnloadRawFile(assetName);
AssetLoaders.ReleaseRawFiles();
```

## AssetPatcher — patch flow

The `PatchLauncher` prefab (in the boot scene) initializes the system; gate everything on it:

```csharp
private IEnumerator Start()
{
    this._InitPatchEvents();                       // listeners FIRST
    while (!AssetPatcher.IsInitialized()) yield return null;
}

AssetPatcher.Check();     // start the patch FSM
AssetPatcher.Repair();    // wipe local patch data and re-run
AssetPatcher.Pause();  AssetPatcher.Resume();  AssetPatcher.Cancel();
// state: AssetPatcher.IsCheck() / IsRepair() / IsDone() / IsInitialized() / IsReleased()
```

Patch FSM states (in order, delivered via `PatchEvents.PatchFsmState.stateNode`):
`FsmPatchRepair → FsmPatchPrepare → FsmAppVersionUpdate → FsmInitPatchMode → FsmPatchVersionUpdate → FsmPatchManifestUpdate → FsmCreateDownloader → FsmBeginDownload → FsmDownloadOver → FsmClearCache → FsmPatchDone`

### Patch events (subscribe with UniFramework `EventGroup`)

```csharp
private EventGroup _patchEvents = new EventGroup();

this._patchEvents.AddListener<PatchEvents.PatchFsmState>(this._OnHandleEventMessage);
this._patchEvents.AddListener<PatchEvents.PatchGoToAppStore>(this._OnHandleEventMessage);
this._patchEvents.AddListener<PatchEvents.PatchAppVersionUpdateFailed>(this._OnHandleEventMessage);
this._patchEvents.AddListener<PatchEvents.PatchInitPatchModeFailed>(this._OnHandleEventMessage);
this._patchEvents.AddListener<PatchEvents.PatchVersionUpdateFailed>(this._OnHandleEventMessage);
this._patchEvents.AddListener<PatchEvents.PatchManifestUpdateFailed>(this._OnHandleEventMessage);
this._patchEvents.AddListener<PatchEvents.PatchCreateDownloader>(this._OnHandleEventMessage);   // GroupInfo[] -> confirm UI
this._patchEvents.AddListener<PatchEvents.PatchCheckDiskNotEnoughSpace>(this._OnHandleEventMessage);
this._patchEvents.AddListener<PatchEvents.PatchDownloadProgression>(this._OnHandleEventMessage);
this._patchEvents.AddListener<PatchEvents.PatchDownloadFailed>(this._OnHandleEventMessage);
this._patchEvents.AddListener<PatchEvents.PatchDownloadCanceled>(this._OnHandleEventMessage);
this._patchEvents.AddListener<PatchEvents.PatchRepairFailed>(this._OnHandleEventMessage);
```

User-driven continuation / retries (`PatchUserEvents`):

```csharp
PatchUserEvents.UserBeginDownload.SendEventMessage(selectedGroupInfo);  // after group confirm UI
PatchUserEvents.UserTryAppVersionUpdate.SendEventMessage();             // one retry event per failure type
PatchUserEvents.UserTryInitPatchMode.SendEventMessage();
PatchUserEvents.UserTryPatchVersionUpdate.SendEventMessage();
PatchUserEvents.UserTryPatchManifestUpdate.SendEventMessage();
PatchUserEvents.UserTryCreateDownloader.SendEventMessage();
PatchUserEvents.UserTryPatchRepair.SendEventMessage();
```

Progress payload: `progress, totalDownloadCount, currentDownloadCount, totalDownloadSizeBytes, currentDownloadSizeBytes, downloadSpeedBytes`. Format with `BundleUtility.GetBytesToString((ulong)x)` / `GetSpeedBytesToString(...)` / `GetMegabytesToString(...)`.

Versions after `FsmPatchDone`:

```csharp
string appVersion = AssetPatcher.GetAppVersion();
string patchVersion = AssetPatcher.GetPatchVersion(encode: true);   // encoded form recommended for display
```

## Packages (App / DLC)

```csharp
// Load from a specific app package built to CDN
var packageInfo = new AppPackageInfoWithBuild()
{
    buildMode = BundleConfig.BuildMode.ScriptableBuildPipeline,
    packageName = "OtherPackage"
};
bool ok = await AssetPatcher.InitAppPackage(packageInfo, updatePackage: true);
if (ok)
{
    var package = AssetPatcher.GetPackage("OtherPackage");
    var downloader = AssetPatcher.GetPackageDownloader(package);
    // downloader.TotalDownloadCount == 0 => already local
    await AssetLoaders.LoadAssetAsync<GameObject>("OtherPackage", assetName);
}

// DLC package (versioned)
var dlcInfo = new DlcPackageInfoWithBuild()
{
    buildMode = BundleConfig.BuildMode.ScriptableBuildPipeline,
    packageName = "DlcPackage",
    dlcVersion = "latest",
    withoutPlatform = false
};
bool ok2 = await AssetPatcher.InitDlcPackage(dlcInfo, true);

// Package management
AssetPatcher.SetDefaultPackage(packageName);
AssetPatcher.UpdatePackage(packageName);
AssetPatcher.UnloadPackageAndClearCacheFiles(packageName);
```

## PackageOperation — self-contained DLC patch driver (BundleDLCDemo)

Order is load-bearing: **create → add listeners → `Ready()` → `Begin()`**.

```csharp
var op = new PackageOperation(
    "DLC Package 1",                                  // display name
    new DlcPackageInfoWithBuild()
    {
        buildMode = BundleConfig.BuildMode.ScriptableBuildPipeline,
        packageName = "Dlc1Package",
        dlcVersion = "latest"
    },
    skipDownload: false);

op.eventGroup.AddListener<PackageEvents.PatchFsmState>(msg => { /* PackageFsmStates.* */ });
op.eventGroup.AddListener<PackageEvents.PatchDownloadProgression>(msg => { /* progress */ });
op.onPatchRepairFailed            = itself => itself.UserTryPatchRepair();
op.onPatchInitPatchModeFailed     = itself => itself.UserTryInitPatchMode();
op.onPatchVersionUpdateFailed     = itself => itself.UserTryPatchVersionUpdate();
op.onPatchManifestUpdateFailed    = itself => itself.UserTryPatchManifestUpdate();
op.onPatchDownloadFailed          = (itself, fileName, error) => itself.UserTryCreateDownloader();
op.onPatchCheckDiskNotEnoughSpace = (itself, availableMegabytes, patchTotalBytes) => { /* prompt */ };

op.Ready();                 // prepares and (unless skipDownload) raises download confirmation
op.Begin();                 // start download
// op.Repair(); op.Pause(); op.Resume(); op.Cancel();
// op.IsReady() / IsBegin() / IsRepair() / IsDone(); op.groupInfo.totalBytes;
```

## Play modes & PatchLauncher settings

`BundleConfig.PlayMode`: `EditorSimulateMode`, `OfflineMode`, `HostMode`, `WeakHostMode`, `WebGLMode`, `WebGLRemoteMode`, `CustomMode`. Configured on the PatchLauncher prefab; build-time scripting defines can force a mode: `OXGFRAME_OFFLINE_MODE`, `OXGFRAME_HOST_MODE`, `OXGFRAME_WEAK_HOST_MODE`, `OXGFRAME_WEBGL_MODE`, `OXGFRAME_WEBGL_REMOTE_MODE`, `OXGFRAME_CUSTOM_MODE`.

- Host/WeakHost/WebGLRemote fetch the app config from the host server and auto-configure endpoints + preset-package downloaders.
- CustomMode: call `AssetPatcher.SetPresetPackages(appPackages, dlcPackages)` then `AssetPatcher.InitSetupPresetPackages()` at boot (after the PatchLauncher has awoken).

Download tuning on `BundleConfig`: `maxConcurrencyDownloadCount` (10), `failedRetryCount` (3), `breakpointFileSizeThreshold` (20 MB), `downloadWatchdogTimeout` (30 s).

Decryption (`DecryptInfo` on PatchLauncher) arg formats:
`"none"`, `"offset, 12"`, `"xor, 23"`, `"ht2xor, 34, 45, 56"`, `"ht2xorplus, 34, 45, 56, 78"`, `"aes, key, iv"`, `"chacha20, key, nonce, 1"`, `"xxtea, key"`, `"offsetxor, 12, 23"` — separate settings for bundles and manifest.

## Utility

```csharp
BundleUtility.GetBytesToString(bytes);          // "12.34 MB"
BundleUtility.GetSpeedBytesToString(bytes);     // "1.2 MB/s"
BundleUtility.MakeMd5ForFile(path);
AssetPatcher.GetLocalSandboxRootPath();         // persistent patch root
AssetPatcher.GetBuiltinRootPath();              // StreamingAssets/yoo
AssetPatcher.GoToAppStore();                    // when PatchGoToAppStore fires
```
