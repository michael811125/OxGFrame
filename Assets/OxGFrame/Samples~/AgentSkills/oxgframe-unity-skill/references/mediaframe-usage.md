# MediaFrame Usage (MediaFrames.AudioFrame / VideoFrame)

Namespace `OxGFrame.MediaFrame`. Audio/Video are loaded as **prefabs** carrying `AudioBase` / `VideoBase` components (create via `Assets/Create/OxGFrame/Media Frame/...` template prefabs or the clip-to-prefab converters). The same `res#` / bundle address rules apply.

Scene requirements: `MediaFrames.AudioFrame` mixer APIs need the `AudioManager` prefab (with its `MasterMixer`) in the scene; `VideoFrame` needs nothing extra. Call `InitInstance()` in a boot `Awake()`.

## Shared API shape (AudioFrame shown; VideoFrame identical with `VideoBase`/`VideoClip`)

```csharp
MediaFrames.AudioFrame.InitInstance();

await MediaFrames.AudioFrame.Preload(assetName);            // + packageName / string[] overloads

// Play(assetName, Transform parent = null, int loops = 0, float volume = 0f)
//   loops: 0 = play once, -1 = infinite loop
//   volume: > 0 overrides the prefab's setting, 0 keeps it
var audioBase = await MediaFrames.AudioFrame.Play(AudioPrefs.a101);
var withClip  = await MediaFrames.AudioFrame.Play(AudioPrefs.a201, this.clips[0]);   // supply a source clip at runtime

MediaFrames.AudioFrame.Stop(assetName);
MediaFrames.AudioFrame.Stop(assetName, disableEndEvent: false, forceDestroy: true);
MediaFrames.AudioFrame.Pause(assetName);
MediaFrames.AudioFrame.PauseAll();
MediaFrames.AudioFrame.ResumeAll();          // no single-asset Resume; re-Play a paused instance instead
MediaFrames.AudioFrame.StopAll(false, true);
MediaFrames.AudioFrame.ForceUnload(assetName);   // when the prefab's onDestroyAndUnload is unchecked

var ab = MediaFrames.AudioFrame.GetComponent<AudioBase>(assetName);
```

## Audio specifics

Demo path convention mirrors `SoundType` categories (drives auto-parenting under `[AudioManager]`):
`res#Audio/a_BGM/101`, `res#Audio/b_General/201`, `res#Audio/c_Interact/301`, `res#Audio/d_Voice/401`, `res#Audio/e_Atmosphere/501`.

`AudioBase` prefab fields: `audioType { soundType, priority }`, `sourceType` (Audio | StreamingAssets | Url), `audioClip`, `urlSet`, `loops (-1 = infinite)`, `ignoreTimeScale`, `onStopAndDestroy`, `onDestroyAndUnload`, `autoEndToStop`, mixer group assignment (`_mixerName`, `_mixerGroupName`).

### Mixer control (needs AudioManager prefab)

```csharp
var mixer = MediaFrames.AudioFrame.GetMixerByName("MasterMixer");
MediaFrames.AudioFrame.SetMixerExposedParam(mixer, "Master_Vol", vol);   // auto-records for restore
MediaFrames.AudioFrame.AutoClearMixerExposedParams(mixer);
MediaFrames.AudioFrame.AutoRestoreMixerExposedParams(mixer);
MediaFrames.AudioFrame.SetMixerSnapshot(mixer, "Paused");                // "Normal", "Reverb_Light", "Reverb_Heavy"
MediaFrames.AudioFrame.SetMixerTransitionToSnapshot(mixer,
    new[] { MediaFrames.AudioFrame.GetMixerSnapshot(mixer, "Reverb_Light"),
            MediaFrames.AudioFrame.GetMixerSnapshot(mixer, "Reverb_Heavy") },
    new float[] { 1 - val, val });
```

Sample `MasterMixer` exposed params: `Master_Vol`, `BGM_Vol`, `General_Vol`, `Interact_Vol`, `Voice_Vol`, `Atmosphere_Vol`. There is no separate master-volume API — volume control goes through mixer exposed params.

## Video specifics

`VideoBase` prefab render modes: **RenderTexture** (draw into UI via `RawImage`) or **Camera** (direct overlay).

```csharp
// Camera render mode: just play
await MediaFrames.VideoFrame.Play(VideoPrefs.VideoCamExample, this.clips[0]);

// RenderTexture mode: grab the RT and wire the end event
var video = await MediaFrames.VideoFrame.Play(VideoPrefs.VideoRtExample, this.clips[1]);
if (video != null)
{
    this.rawImage.enabled = true;
    this.rawImage.texture = video.GetTargetRenderTexture();
    video.SetEndEvent(() => { this.rawImage.texture = null; this.rawImage.enabled = false; });
}

MediaFrames.VideoFrame.Stop(VideoPrefs.VideoRtExample, false, true);

// VideoBase instance controls
float progress = video.CurrentLength() / video.Length();
video.SkipToPercent(progress + 5f / video.Length());
video.SetPlaySpeed(1.25f);        // recommended: 0.25/0.5/1.0/1.25/1.5/1.75/2.0
```

`VideoBase` sources: `Video` (VideoClip), `StreamingAssets` (`fullPathName` relative to StreamingAssets, with extension), `Url` (UrlSet; either a full URL or a path resolved from the media URL config). Prefab fields: `renderMode`, `_renderTextureSize`, `_targetCamera { camera, orderType, alpha }`, `_aspectRatio`, `waitForFirstFrame`, `playbackSpeed`, `maxPrepareTimeSeconds`, `loops`, `onStopAndDestroy`, `onDestroyAndUnload`.

## Editor preview

`AudioBase` and `VideoBase` inspectors provide Play/Pause/Stop buttons with a seek bar and timecode (works for all source types, including StreamingAssets/Url, without entering Play Mode). Video preview renders the picture (and audio track) inside the Inspector. If an `http://` source is blocked, the editor offers to switch Player Settings "Allow downloads over HTTP" to "Only in development builds".

## Release rules

- Stop audio/video in stage `OnExit` / UI `OnRelease` (`StopAll()` on scene transitions).
- `onStopAndDestroy` destroys the media object on stop; `onDestroyAndUnload` also unloads the asset — when unchecked, call `ForceUnload(assetName)` yourself.
- WebGL: browser autoplay policies require a user gesture; `SourceType.Audio`/`Video` clip assignment is not supported on WebGL for some formats — prefer StreamingAssets/Url there; always stop videos on UI close to release render textures.

## URL config (for SourceType.Url)

`MediaConfig.MEDIA_URL_CFG_NAME` (`MediaConfig.txt`) holds `audio_urlset` / `video_urlset` base URLs; generate via `Assets/Create/OxGFrame/Media Frame/...` or menu tooling (`MediaUrlConfigGeneratorWindow`). `UrlSet.getUrlPathFromCfg` toggles between cfg-based base URL + relative path, or a full URL in `url` directly.
