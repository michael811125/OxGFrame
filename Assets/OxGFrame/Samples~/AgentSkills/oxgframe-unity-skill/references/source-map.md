# OxGFrame Source Map

Use this map to locate and re-verify APIs before applying this skill to a project. Written against OxGFrame v3.6.1+ (Unity 2021.3 – 6.x).

## Official sources

- Repository: `https://github.com/michael811125/OxGFrame`
- Docs: `https://oxgframe.ouhiyo.com/`
- Coding style wiki: `https://github.com/michael811125/OxGFrame/wiki/Coding-Style`

## Package location in a target project

The package id is `com.michaelo.oxgframe`. Depending on how it is installed, the source lives at:

- UPM (git): `Library/PackageCache/com.michaelo.oxgframe@<hash>/`
- Embedded: `Packages/OxGFrame/` or directly under `Assets/OxGFrame/` (framework development repo)

Locate it with:

```
# PowerShell
Get-ChildItem Library/PackageCache -Directory | Where-Object { $_.Name -like "com.michaelo.oxgframe*" }
# or search for the facade
rg -l "static class AssetLoaders" --type cs
```

## Key runtime files per module

| Module | Facade / key files |
|---|---|
| AssetLoader | `AssetLoader/Scripts/Runtime/AssetLoaders.cs` (load facade), `AssetPatcher.cs` (patch facade), `Bundle/BundleConfig.cs`, `Bundle/PatchLauncher.cs`, `Bundle/PackageOperation.cs`, `Bundle/PatchEvent/PatchEvents.cs` + `PatchUserEvents.cs` + `PackageEvents.cs`, `Bundle/PatchFsm/PatchFsmStates.cs`, `Bundle/PlayMode/PlayModeParameters.cs`, `Utility/BundleUtility.cs` |
| CoreFrame | `CoreFrame/Scripts/Runtime/Core/CoreFrames.cs` (facade: UIFrame/SRFrame/USFrame/CPFrame), `Core/Implement/FrameBase.cs` + `FrameManager.cs`, `Core/UIFrame/UIBase.cs` + `UIManager.cs` + `UIConfig/UIConfig.cs`, `Core/SRFrame/SRBase.cs`, `Core/CPFrame/CPBase.cs`, `Core/USFrame/USManager.cs`, `Core/Common/Binder.cs` + `FrameConfig.cs` (binding symbols) |
| MediaFrame | `MediaFrame/Scripts/Runtime/Core/MediaFrames.cs` (facade: AudioFrame/VideoFrame), `Core/AudioFrame/AudioBase.cs` + `AudioManager.cs`, `Core/VideoFrame/VideoBase.cs` + `VideoManager.cs`, `Core/Implement/MediaBase.cs` |
| GSIFrame | `GSIFrame/Scripts/Runtime/GSIManagerBase.cs`, `GSIBase.cs` |
| NetFrame | `NetFrame/Scripts/Runtime/NetFrames.cs` (facade), `Core/NetManager.cs`, `Core/Implement/NetNode/NetNode.cs`, `NetOption/*.cs`, providers: `WebSocketNetProvider.cs`, `TcpNetProvider.cs`, `KcpNetProvider.cs`, `Core/Interface/INetTips.cs` |
| CenterFrame | `CenterFrame/Scripts/Runtime/Common/CenterBase.cs`, `APICenter/APIBase.cs` + `Acax.cs` (Http), `EventCenter/EventBase.cs` |
| Hotfixer | `Hotfixer/Scripts/Runtime/Hotfixers.cs` (facade), `Hotfix/HotfixManager.cs`, `Hotfix/HotfixFsm/HotfixFsmStates.cs`, `HotfixEvent/HotfixEvents.cs` + `HotfixUserEvents.cs` |

## Editor entry points worth knowing

- `GameObject/OxGFrame/Auto Generate Bind Codes (Shift+B)` — CodeBinding generator.
- `GameObject/OxGFrame/Auto Generate Stop End Symbol (Shift+E)` — appends `#` stop-end markers.
- `Assets/Create/OxGFrame/Core Frame/...` — UI/SR/CP template scripts and prefabs.
- `Assets/Create/OxGFrame/Center Frame/...` — API/Event center template scripts.
- `Assets/Create/OxGFrame/Media Frame/...` — Audio/Video template prefabs and clip-to-prefab converters.
- `Assets/Create/OxGFrame/Create Settings/...` — `CodeBindingSettings`, `PatchSettings`, `HotfixSettings` assets.
- `OxGFrame/Hotfixer/Hotfix Dll Config Generator` — generates `hotfixdllconfig.conf` into StreamingAssets.
- `OxGFrame/AssetLoader/...` — patch/bundle tooling windows.

## Verification commands

```
# Find every public facade method quickly
rg -n "public static (async )?(UniTask|void|bool|string|T |U )" <pkg>/CoreFrame/Scripts/Runtime/Core/CoreFrames.cs

# How does the target project use OxGFrame today?
rg -n "CoreFrames\.(UIFrame|SRFrame|USFrame|CPFrame)|MediaFrames\.(AudioFrame|VideoFrame)|AssetLoaders\.|AssetPatcher\.|Hotfixers\." Assets -g "*.cs"

# Address constants / prefixes in use
rg -n "res#|build#|public const string|public static readonly string" Assets -g "*.cs" | rg -v Packages

# Binding conventions present in prefabs/scenes
rg -n "_Node@|~Node@" Assets

# Which play mode & packages the project ships
rg -n "playMode|listAppPackages|listDlcPackages" Assets -g "*.prefab" -g "*.unity"
```

## Dependencies the framework assumes

`com.cysharp.unitask` (UniTask), YooAsset (bundled under `AssetLoader/ThirdParty` or as `com.tuyoogame.yooasset`), `com.code-philosophy.hybridclr` (Hotfixer), OxGKit modules (`com.michaelo.oxgkit.loggingsystem`, `.saversystem`, `.timesystem`, `.utilities`, `.buttonsystem`), LWMyBox, UniFramework (Event/Machine, bundled). Samples additionally use Unity Input System, Newtonsoft.Json, and kcp2k.
