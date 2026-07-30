# Hotfixer + HybridCLR Flow

Namespace `OxGFrame.Hotfixer`. Hotfixer downloads and loads HybridCLR hotfix assemblies (plus AOT metadata) from a bundle package, then hands control to the hotfix code.

## Assembly split (mandatory)

- **Main (AOT) assembly** — ships with the player. Contains boot scene logic, the hotfix check, splash/patch UI. Must NOT reference the hotfix assembly (enter hotfix code via a loaded scene, `AddComponent` from a hotfix-collected prefab, or reflection).
- **Hotfix assembly** — compiled to DLL, collected as a bundle asset, downloaded and `Assembly.Load`ed at runtime.

HotfixerDemo layout: `Scripts/Main/` (`HotfixerDemo.Main.Runtime.asmdef`) and `Scripts/Hotfix/` (`HotfixerDemo.Hotfix.Runtime.asmdef`), scenes `Scenes/Main/` + `Scenes/Hotfix/`. Collector groups for the hotfix package: the hotfix scene, `Assets/HotfixCollector/AOTDlls`, `Assets/HotfixCollector/HotfixDlls`, plus any hotfix prefabs.

## CheckHotfix

```csharp
// Explicit lists:
Hotfixers.CheckHotfix(
    "HotfixPackage",
    new string[] { "mscorlib.dll", "UniTask.dll" },              // AOT metadata assemblies
    new string[] { "HotfixerDemo.Hotfix.Runtime.dll" });         // hotfix assemblies

// Or config-driven (reads hotfixdllconfig.conf from StreamingAssets):
Hotfixers.CheckHotfix("HotfixPackage", () =>
{
    Debug.LogWarning("Generate hotfixdllconfig.conf via MenuItem OxGFrame/Hotfixer/Hotfix Dll Config Generator.");
});
```

Gate on the patcher first (`while (!AssetPatcher.IsInitialized()) yield return null;`), register hotfix event listeners BEFORE calling `CheckHotfix`, then poll:

```csharp
if (Hotfixers.IsDone() && !this._isLoaded)
{
    this._isLoaded = true;
    CoreFrames.USFrame.LoadSingleSceneAsync("HotfixPackage", "HotfixMain").Forget();
}
```

Facade summary:

```csharp
Hotfixers.IsDone();          // all dlls downloaded + loaded
Hotfixers.IsDisabled();      // true when OXGFRAME_HYBRIDCLR_DISABLED define is set
Hotfixers.Reset();           // allow CheckHotfix to run again
Hotfixers.GetHotfixAssembly("Name.dll");                 // reflection entry from Main
Hotfixers.GetHotfixAssemblyNamesWithoutExtensions();
Hotfixers.GetAOTAssemblyNames();
```

## Hotfix FSM + events

States: `FsmHotfixPrepare → FsmInitHotfixPackage → FsmUpdateHotfixPackage → FsmHotfixCreateDownloader (pauses when downloads exist) → FsmHotfixBeginDownload → FsmHotfixDownloadOver → FsmHotfixClearCache → FsmLoadAOTAssemblies → FsmLoadHotfixAssemblies → FsmHotfixDone`.

```csharp
private EventGroup _hotfixEvents = new EventGroup();
this._hotfixEvents.AddListener<HotfixEvents.HotfixFsmState>(this._OnHandleEventMessage);
this._hotfixEvents.AddListener<HotfixEvents.HotfixInitFailed>(this._OnHandleEventMessage);
this._hotfixEvents.AddListener<HotfixEvents.HotfixUpdateFailed>(this._OnHandleEventMessage);
this._hotfixEvents.AddListener<HotfixEvents.HotfixCreateDownloader>(this._OnHandleEventMessage);   // totalCount/totalBytes -> confirm UI
this._hotfixEvents.AddListener<HotfixEvents.HotfixDownloadProgression>(this._OnHandleEventMessage);
this._hotfixEvents.AddListener<HotfixEvents.HotfixDownloadFailed>(this._OnHandleEventMessage);

// user events (continue / retry):
HotfixUserEvents.UserBeginDownload.SendEventMessage();
HotfixUserEvents.UserTryInitHotfix.SendEventMessage();
HotfixUserEvents.UserTryUpdateHotfix.SendEventMessage();
HotfixUserEvents.UserTryCreateDownloader.SendEventMessage();
```

## HybridCLR integration details

- `FsmLoadAOTAssemblies` loads each AOT dll (TextAsset bytes) via `HybridCLR.RuntimeApi.LoadMetadataForAOTAssembly(bytes, HomologousImageMode.SuperSet)`.
- `FsmLoadHotfixAssemblies` does `Assembly.Load(bytes)` and registers the assembly with the manager.
- In Editor / `EditorSimulateMode` / when `OXGFRAME_HYBRIDCLR_DISABLED` is defined, DLL loading is skipped and assemblies resolve from `AppDomain.CurrentDomain` (everything is AOT-compiled) — the rest of the flow still runs, so the same game code works with hotfix on or off.
- `HotfixDllPlan.json` feeds the Hotfix Dll Config Generator:

```json
[ { "planName": "Plan", "aotDlls": [ "mscorlib.dll", "UniTask.dll" ], "hotfixDlls": [ "MyGame.Hotfix.Runtime.dll" ] } ]
```

## Boot order recipe (Main scene)

1. `PatchLauncher` prefab in the boot scene; wait `AssetPatcher.IsInitialized()`.
2. Register hotfix events; `Hotfixers.CheckHotfix("HotfixPackage", ...)`.
3. Show splash/progress UI while waiting; drive retries via `HotfixUserEvents`.
4. On `Hotfixers.IsDone()`, load the hotfix main scene from the hotfix package.
5. Hotfix scene's driver starts the runtime stage manager (GSIFrame) — patch remaining packages with `AssetPatcher.Check()`, then enter the game.

## IL2CPP / stripping notes

- Keep `link.xml` covering types used only via reflection from hotfix code.
- AOT generics: run HybridCLR generation before building; keep the AOT metadata list in sync with `HotfixDllPlan.json`.
- Do not place hotfix-only `MonoBehaviour`s in Main scenes; hotfix scenes/prefabs live in the hotfix package.
