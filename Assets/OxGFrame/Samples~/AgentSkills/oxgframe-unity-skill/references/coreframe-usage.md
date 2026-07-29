# CoreFrame Usage (CoreFrames.UIFrame / SRFrame / USFrame / CPFrame)

Namespace `OxGFrame.CoreFrame`. Facade `CoreFrames` with four nested static classes. Group id default is `0`. All Show/Preload APIs accept `res#` (Resources) or bundle addresses, and have `(packageName, assetName, ...)` overloads.

## UIFrame

```csharp
CoreFrames.UIFrame.InitInstance();                         // boot Awake (recommended)
bool ok = CoreFrames.UIFrame.SetupAndCheckUICanvas(canvasName);

// Preload
await CoreFrames.UIFrame.Preload(assetName);               // + (packageName, assetName) / string[] overloads

// Show — full signature:
// Show(int groupId, string assetName, object data = null, string awaitingUIAssetName = null,
//      uint priority = 0, Progression progression = null, Transform parent = null,
//      float awaitingUIExtraDuration = 0f)
await CoreFrames.UIFrame.Show(ScreenUIs.Id, ScreenUIs.Demo1UI, "hello data", ScreenUIs.DemoLoadingUI, 0);
var ui = await CoreFrames.UIFrame.Show<Demo1UI>(ScreenUIs.Id, ScreenUIs.Demo1UI);

// Close / CloseAll — disableOnPreClose skips OnPreClose(); forceDestroy destroys + unloads
CoreFrames.UIFrame.Close(ScreenUIs.Demo1UI);
CoreFrames.UIFrame.Close(ScreenUIs.Demo1UI, disableOnPreClose: true, forceDestroy: true);
CoreFrames.UIFrame.CloseAll(true, true);                   // current group scope
CoreFrames.UIFrame.CloseAll(groupId, true, true, withoutAssetNames);
CoreFrames.UIFrame.CloseAllForAllGroups(true, true);
CoreFrames.UIFrame.CloseAllAndExcluded(...);               // ignores uiSettings.whenCloseAllToSkip

// Hide / Reveal — Hide keeps the instance; only a Hidden UI can be Revealed
CoreFrames.UIFrame.Hide(assetName);   CoreFrames.UIFrame.HideAll(groupId);
CoreFrames.UIFrame.Reveal(assetName); CoreFrames.UIFrame.RevealAll(groupId);

// Stack-by-stack (ESC menu pattern)
if (CoreFrames.UIFrame.GetStackByStackCount(groupId, canvasName) > 0)
    CoreFrames.UIFrame.CloseStackByStack(groupId, canvasName);

// Queries & access
CoreFrames.UIFrame.CheckIsShowing(assetName);  CheckIsHiding(assetName);  CheckHasAnyHiding();
var comp = CoreFrames.UIFrame.GetComponent<Demo1UI>(assetName);

// Refresh messaging -> OnReceiveAndRefresh(object) on showing/hiding instances
CoreFrames.UIFrame.SendRefreshData(new RefreshInfo { assetName = ..., data = ... });
CoreFrames.UIFrame.SendRefreshDataToAll();     // e.g. after language change
```

### UI prefab settings (`uiSettings` serialized on the UI prefab)

- `canvasName` — must match a `UICanvas` GameObject in the scene (demo: `CanvasCamera`, `CanvasWorld`).
- `nodeType` + order: `Fixed(0) < TopFixed(1000) < Popup(2000) < TopPopup(3000) < LoadingPopup(4000) < SysPopup(5000) < TopSysPopup(6000) < AwaitingPopup(7000)`; per-UI `order` 0–1000 when not stacked.
- `stack` — stacked UIs auto-sort by open order; `allowCloseStackByStack` enables the ESC-pop pattern.
- `whenCloseAllToSkip` / `whenHideAllToSkip` — exclude from bulk operations.
- `autoMask` + `maskSettings` (color/sprite/`isClickMaskToClose`) — pooled `_UIMaskNode` behind popups.
- `reverseChanges` — showing this UI hides the previous reverse UI (data preserved); closing reveals it. Freeze (`_UIFreezeNode`) blocks input during show/close animations.
- `FrameBase` fields: `groupId`, `monoDrive` (scene-driven, no manager), `allowInstantiate` (multi-instance, always destroy on close), `onCloseAndDestroy`.

## SRFrame (scene resources — non-canvas managed prefabs)

Same surface as UIFrame minus canvas/stack concepts:

```csharp
CoreFrames.SRFrame.InitInstance();
await CoreFrames.SRFrame.Preload(ScenePrefs.CubeSceneSR);
await CoreFrames.SRFrame.Show(ScenePrefs.Id, ScenePrefs.CubeSceneSR);
await CoreFrames.SRFrame.Show(ScenePrefs.Id, ScenePrefs.CubeSceneSR, null, null, 0, null, parent);
var sr = await CoreFrames.SRFrame.Show<MeshResSR>(ResPrefs.Id, ResPrefs.MeshResSR);
if (sr == null) sr = CoreFrames.SRFrame.GetComponent<MeshResSR>(ResPrefs.MeshResSR);
CoreFrames.SRFrame.Hide(name); CoreFrames.SRFrame.Reveal(name);
CoreFrames.SRFrame.Close(name, false, true);
CoreFrames.SRFrame.CloseAll(groupId, true, true);
```

Use SR for: camera/audio-listener rigs, gameplay controllers, pools, per-stage world systems.

## USFrame (Unity scenes)

`build#SceneName` loads from Build Settings; no prefix loads a bundle scene (needs PatchLauncher initialized).

```csharp
CoreFrames.USFrame.InitInstance();

await CoreFrames.USFrame.LoadSingleSceneAsync(sceneName, progression);
await CoreFrames.USFrame.LoadSingleSceneAsync(packageName, sceneName, LocalPhysicsMode.None, progression);
await CoreFrames.USFrame.LoadAdditiveSceneAsync(sceneName, activateOnLoad: true, priority: 100);
await CoreFrames.USFrame.LoadSceneAsync(sceneName, LoadSceneMode.Additive, true, 100,
    (progress, currentCount, totalCount) => { /* progress UI */ });
CoreFrames.USFrame.LoadScene(sceneName, LoadSceneMode.Additive, progression);   // sync

// Main + subs in one call
await CoreFrames.USFrame.LoadMainAndSubScenesAsync(mainSceneName,
    new CoreFrames.USFrame.AdditiveSceneInfo[] { new() { sceneName = "Sub01", activeRootGameObjects = true } });

CoreFrames.USFrame.SetActiveSceneRootGameObjects(sceneName, true);
CoreFrames.USFrame.Unload(recursively: true, sceneName);    // all instances vs top-most only
CoreFrames.USFrame.GetSceneByName(sceneName);  SetActiveScene(sceneName);
```

## CPFrame (clone prefabs — cells/items; no Show/Close, lifetime = GameObject)

```csharp
CoreFrames.CPFrame.InitInstance();
await CoreFrames.CPFrame.PreloadAsync(TplPrefs.Demo1CP);

var cp = CoreFrames.CPFrame.LoadWithClone<Demo1CP>(TplPrefs.Demo1CP);                   // sync
var cp2 = await CoreFrames.CPFrame.LoadWithCloneAsync<Demo2CP>(TplPrefs.Demo2CP, parent);
var cp3 = await CoreFrames.CPFrame.LoadWithCloneAsync<Demo2CP>(name, position, rotation, parent, scale);

// Factory-method pattern (recommended, from CPFrameDemo):
public static async UniTask<Demo2CP> CloneParsedDemo2CP(int price, Transform parent = null)
{
    var cp = await CoreFrames.CPFrame.LoadWithCloneAsync<Demo2CP>(TplPrefs.Demo2CP, parent);
    if (cp.ParsingDemo2CP(price)) return cp;
    return null;
}
```

`CPBase` unloads its own asset reference on destroy. Destroy the GameObject to release.

## Lifecycle (FrameBase order)

`Awake (once) → OnCreate (once) → OnAutoBind (once) → OnBind (once) → OnPreShow (every open) → OnShow (every open)`

| Hook | UIBase | SRBase | CPBase |
|---|---|---|---|
| `OnCreate()` | yes | yes | yes |
| `OnAutoBind()` (generated) | yes | yes | yes |
| `OnBind()` — one-time event wiring | yes | yes | yes |
| `async UniTask OnPreShow()` | yes | yes | obsolete no-op |
| `OnShow(object obj)` — receives `Show(..., data)` | yes | yes | `OnShow()` no arg |
| `OnShowAnimation(AnimationEnd end)` / `OnCloseAnimation` | UI only — MUST call `end()` | – | – |
| `OnReveal()` / `OnHide()` | yes | yes | obsolete |
| `OnPreClose()` / `OnClose()` | yes | yes | `OnClose()` = OnDisable |
| `OnUpdate/OnFixedUpdate/OnLateUpdate(float dt)` | yes | yes | yes |
| `OnReceiveAndRefresh(object)` | yes | yes | obsolete |
| `OnRelease()` — remove listeners, free resources | yes | yes | = OnDestroy |

Self-control from inside the class: `this.CloseSelf()`, `this.CloseSelf(disableOnPreClose, forceDestroy)`, `this.HideSelf()`. Use these for self-close; use `CoreFrames.UIFrame.Close(...)` only from external flows.

## CodeBinding

Hierarchy naming (`FrameConfig`): head `_Node@` for UI/SR, `~Node@` for CP; `@` separates head from name; `*` appends component-tail keys (multi-bind); `$` sets access (`$public`); `[hi]` = `[HideInInspector]`, `[sf]` = `[SerializeField]`; a trailing `#` on any node name stops recursive scanning into its children.

Examples from the demo prefabs:

```text
_Node@Open*Btn$private      ->  private Button _openBtn;
_Node@Decor*Img$public[hi]  ->  [HideInInspector] public Image decorImg;
_Node@View*Img[sf]          ->  [SerializeField] protected Image _viewImg;
~Node@B1                    ->  CP node (GetNode("B1"))
~Node@B2  (x3, same name)   ->  GetNodes("B2") returns the array
```

Generated block (menu `GameObject/OxGFrame/Auto Generate Bind Codes (Shift+B)`):

```csharp
#region Binding Components
private Button _openBtn;

protected override void OnAutoBind()
{
    base.OnAutoBind();
    this._openBtn = this.collector.GetNodeComponent<Button>("Open*Btn");
}
#endregion
```

Collector lookups: `collector.GetNode("Name")`, `GetNodes("Name")`, `GetNodeComponent<T>("Name*Tail")`, `GetNodeComponents<T>(...)` — the key includes the `*Tail` but not the head or modifiers. Tail-to-component mapping lives in `CodeBindingSettings.asset` (`Btn→Button`, `Img→Image`, `Txt→Text`, `TmpTxt→TMP_Text`, `Trans→Transform`, `BtnPlus→ButtonPlus`, ... configurable).

## Minimal UIBase template

```csharp
public class MyUI : UIBase
{
    #region Binding Components
    private Button _closeBtn;
    protected override void OnAutoBind()
    {
        base.OnAutoBind();
        this._closeBtn = this.collector.GetNodeComponent<Button>("Close*Btn");
    }
    #endregion

    public override void OnCreate() { }
    protected override void OnBind() { this._closeBtn.onClick.AddListener(this._OnClickClose); }
    protected override async UniTask OnPreShow() { }
    protected override void OnShow(object obj) { /* consume data */ }
    protected override void OnUpdate(float dt) { }
    public override void OnReceiveAndRefresh(object obj = null) { }
    protected override void OnPreClose() { }
    public override void OnRelease() { this._closeBtn.onClick.RemoveListener(this._OnClickClose); }
    private void _OnClickClose() { this.CloseSelf(); }
}
```
