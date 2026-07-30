# Production Architecture Patterns (Optional)

Optional guidance for structuring a commercial OxGFrame project. These are proven patterns, not requirements — never force them onto a project that follows different conventions; match what exists.

## Boot stage chain (GSIFrame)

Model the whole app flow as stages (GSIFrameDemo uses exactly this chain):

```
Startup -> Logo -> Patch -> Login/Menu -> Enter/Gameplay -> (Result)
```

- `StartupStage`: wait `AssetPatcher.IsInitialized()`; init loggers/core systems.
- `LogoStage`: show splash UI (from a small built-in package when hotfix is used).
- `PatchStage`: show patch UI, `AssetPatcher.Check()`, poll `AssetPatcher.IsDone()`, then `StopUpdate()` and change stage.
- `MenuStage.OnEnter`: `USFrame.LoadSingleSceneAsync(menuScene)` → `SRFrame.Show(audioListenerSR)` → `UIFrame.Show(menuUI)` → `AudioFrame.Play(menuBgm)`.
- `GameplayStage.OnEnter`: ensure loading UI → preload heavy SRs → load scene with `loadingUI.DrawProgressView` as `Progression` → show HUD → play BGM → close loading UI.
- Every stage `OnExit`: close its UI/SR (`CloseAll`), `MediaFrames.AudioFrame.StopAll()`, return to an empty Build Settings scene (`build#Empty`) before big transitions when memory matters.

Progression delegate shape for loading UIs:

```csharp
public void DrawProgressView(float progress, float currentCount, float totalCount) { /* update bar */ }
```

## Package split strategy (YooAsset collector)

A proven multi-package layout (adapt names to the project):

| Package | Role | Content |
|---|---|---|
| `BuiltinPackage` | usable before any download; keep tiny | splash/patch/blocking UI, startup essentials |
| `HotfixPackage` | code bootstrap payload | hotfix main scene, `AOTDlls/`, `HotfixDlls/` |
| `PatchPackage` | patchable runtime content | UI/SR/CP prefabs, US scenes, audio/video prefabs, feature assets |
| `DataPackage` (optional) | data with an independent update cadence | JSON/tables as raw files or TextAssets |
| `Dlc*Package` | on-demand content via `PackageOperation` | per-DLC assets |

Pack rules of thumb: `PackSeparately` for independently-patched prefabs/scenes; `PackDirectory` for folders that always update together (DLL folders, data folders). With `AddressByFileName`, keep file names unique per package and mirror them 1:1 in constants classes.

## Address constants discipline

```csharp
public static class Pkgs { public const string Patch = "PatchPackage"; }
public static class UIs  { public const string LoginUI = "LoginUI"; public const int Id = 1; }
public static class USs  { public static readonly string Empty = "build#Empty"; public const string Menu = "MenuScene"; }
public static class Auds { public const string MenuBgm = "menu_bgm_01"; }
```

Rules: constants match the active address rule exactly (case-sensitive by default); no address literals in gameplay code; `build#` only for Build Settings scenes; `res#` only for Resources.

## Lifecycle & release checklist

- Every manual `AssetLoaders.Load*` has a matching `Unload*`; feature scopes use `groupId` + `UnloadAssets(groupId)` on exit.
- UI/SR close in stage `OnExit`; `CloseAll(disableOnPreClose, forceDestroy, withoutAssetNames...)` keeps persistent UIs alive via exclusions or `whenCloseAllToSkip`.
- Cancel `CancellationTokenSource`s for loops in `OnPreClose`/`OnRelease`; never let UniTask loops outlive their owner.
- Stop media on every scene/stage transition; release RenderTextures by stopping videos.
- After heavy transitions consider `AssetLoaders.ReleaseAssets()` + returning to an empty scene.

## WebGL / IL2CPP notes

- Async-only discipline on WebGL: no synchronous bundle loads on transition paths; no thread usage (Hotfixer/AssetLoader already switch off thread pools on WebGL).
- Browser autoplay policies gate audio/video until a user gesture.
- `http://` sources require Player Settings "Allow downloads over HTTP" (2022.1+); prefer `https://`.
- IL2CPP stripping: maintain `link.xml` for reflection-only types; HybridCLR AOT metadata list must match the generated plan.
- PlayMode for WebGL: `WebGLMode` (built-in only) or `WebGLRemoteMode` (remote patch).

## Editor-driven build pipeline

The `BuildTool Extension` sample shows a CLI-ready pipeline (`BuildTool.BuildApp()` / `BuildBundles()`): HybridCLR install + generate → compile/copy hotfix DLLs to the collector → YooAsset bundle build per bundle-map JSON → export configs (`ExportAppConfig`, `ExportBundleUrlConfig`) → player build. Use it as the starting point for CI.
