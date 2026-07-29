# Official Samples Guide

The OxGFrame package ships 16 samples under `Samples~/` (import them via Package Manager > OxGFrame > Samples). They are the ground truth for canonical usage — when generating code, mirror their patterns. This file maps what each sample demonstrates and what its scene requires.

## Sample map

| Sample | Teaches | Scene requirements |
|---|---|---|
| `PatchLauncher Prefab` | The required bootstrap prefab for every bundle-driven feature (play mode, package list, download options, decryption settings) | Put the prefab in the boot scene |
| `AudioManager Prefab` | The in-scene manager prefab + `MasterMixer` (groups: BGM/General/Interact/Voice/Atmosphere; snapshots: Normal/Paused/Reverb_*; exposed params: `Master_Vol`, `BGM_Vol`, ...) | Needed for `MediaFrames.AudioFrame` mixer APIs |
| `BuildTool Extension` | Editor-only CLI build pipeline (`BuildTool.BuildApp()` / `BuildBundles()`), bundle-map JSON schema, HybridCLR + YooAsset + player build chaining | Editor asmdef only |
| `Bundle Demo` | Full patch flow: `AssetPatcher.Check()`, the 12 `PatchEvents`, `PatchUserEvents` retries, group selection, progress display, `AssetLoaders` load/unload | PatchLauncher in scene |
| `Bundle DLC Demo` | `PackageOperation` per-DLC-group patching (create → add listeners → `Ready()` → `Begin()`), repair/cancel | PatchLauncher in scene |
| `Hotfixer Demo` | `Hotfixers.CheckHotfix` (explicit lists + config file forms), main/hotfix asmdef split, loading the hotfix scene, reflection entry | PatchLauncher in scene; two asmdefs |
| `UIFrame Demo` | UI constants + groupId pattern, `Show/Close/Hide/Reveal/Preload`, awaiting (loading) UI, stack-by-stack ESC menu, `UICanvas`, full `UIBase` lifecycle, CodeBinding | `CanvasCamera` / `CanvasWorld` UICanvas objects |
| `SRFrame Demo` | Scene-resource prefabs (`SRBase`), Show with parent, `GetComponent<T>` fallback, Hide/Reveal, `GetNodes` array binding | none (Resources-based) |
| `USFrame Demo` | `build#` vs bundle scene loading, `LoadSceneAsync/LoadScene`, additive scenes, `SetActiveSceneRootGameObjects`, `Unload(recursively, ...)` | PatchLauncher (for bundle scenes) |
| `CPFrame Demo` | Clone prefabs (`CPBase`), `LoadWithClone(Async)<T>`, factory-method pattern, `~Node@` binding | none |
| `AudioFrame Demo` | Audio prefab constants, `Play/Stop/Pause/ResumeAll`, playing with a supplied `AudioClip`, mixer snapshots/params | AudioManager prefab |
| `VideoFrame Demo` | Video prefabs (RenderTexture vs Camera render mode), `GetTargetRenderTexture()`, `SetEndEvent`, `SkipToPercent`, `SetPlaySpeed` | none |
| `GSIFrame Demo` | `GSIManagerBase<T>` singleton manager, stage registration in ctor, `DriveStart/DriveUpdate`, `ChangeStage<T>(force)` | a driver MonoBehaviour |
| `NetFrame Demo` | `NetNode` setup (handlers, heartbeat/out-receive/reconnect tickers), `NetFrames.AddNetNode/Connect/Send/Close`, WebSocket/TCP/KCP options, `INetTips` | scripts only |
| `APICenter Demo` | `CenterBase<TCenter, APIBase>` registry, `Find<T>()?.ReqPost(...)`, `Http.Acax/AcaxAsync` helper | scripts only |
| `EventCenter Demo` | `CenterBase<TCenter, EventBase>` registry, `Emit` + `HandleEvent` + `Release` pattern | scripts only |

## The constants + groupId pattern (used by every demo)

```csharp
public static class ScreenUIs
{
    // If use prefix "res#" will load from resource else will from bundle
    private const string _prefix = "res#";
    private const string _path = "Example/UI/ScreenUI/";

    public static readonly string Demo1UI = $"{_prefix}{_path}Demo1UI";
    public static readonly string DemoLoadingUI = $"{_prefix}{_path}DemoLoadingUI";

    // Group Id
    public const int Id = 1;
}
```

- With `res#`, the rest is the `Resources` path.
- Without a prefix, the name is the YooAsset **address** — with the samples' `AddressByFileName` rule this is the bare file name, no folder, no extension (`model_01`, `LevelDemo01`, `HotfixMain`, `HotfixPrefab`).
- `build#` (USFrame only) loads from the Build Settings scene list.

## Boot gating pattern (every bundle-driven demo)

```csharp
private IEnumerator Start()
{
    // Register patch/hotfix event listeners first, then:
    while (!AssetPatcher.IsInitialized())
        yield return null;
    // ... safe to start patching / loading
}
```

## Scene/manager prerequisites summary

- `PatchLauncher` prefab: required by anything that loads from bundles (BundleDemo, BundleDLCDemo, HotfixerDemo, USFrameDemo bundle scenes).
- `AudioManager` prefab (+ mixer asset): required by AudioFrame mixer APIs; auto-parenting nodes are created per `SoundType`.
- `UICanvas` components on GameObjects whose names match each UI prefab's `uiSettings.canvasName` (demo uses `CanvasCamera`, `CanvasWorld`).
- `CoreFrames.*.InitInstance()` / `MediaFrames.*.InitInstance()` called in a boot `Awake()` ("can more efficiency" — optional but recommended).

## Layout conventions for new samples/features

- Scripts under `Scripts/`; loadable prefabs under `Resources/...` (for `res#` demos) or collected by the YooAsset collector (for bundle demos).
- Scene file named `<SampleName>.unity` at the sample root.
- Hotfix projects split `Scripts/Main/` and `Scripts/Hotfix/` with separate asmdefs; Main must NOT reference Hotfix.
- Every file and folder ships with its `.meta` (samples are imported into user projects — stable GUIDs matter).
