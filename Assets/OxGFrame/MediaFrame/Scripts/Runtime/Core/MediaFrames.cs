using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader;
using OxGFrame.MediaFrame.AudioFrame;
using OxGFrame.MediaFrame.VideoFrame;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Video;

namespace OxGFrame.MediaFrame
{
    /// <summary>
    /// Provide unified static facade APIs for media playback (audio and video)
    /// </summary>
    public static class MediaFrames
    {
        /// <summary>
        /// Bridge the internal AudioManager as public audio playback APIs
        /// </summary>
        public static class AudioFrame
        {
            /// <summary>
            /// Initialize the AudioManager instance
            /// </summary>
            public static void InitInstance()
            {
                AudioManager.GetInstance();
            }

            /// <summary>
            /// Get the first spawned audio component matching the asset name, returns null if not found
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="assetName"></param>
            /// <returns></returns>
            public static T GetComponent<T>(string assetName) where T : AudioBase
            {
                var components = AudioManager.GetInstance().GetMediaComponents<T>(assetName);
                if (components.Length > 0) return components[0];

                return null;
            }

            /// <summary>
            /// Get all spawned audio components matching the asset name
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="assetName"></param>
            /// <returns></returns>
            public static T[] GetComponents<T>(string assetName) where T : AudioBase
            {
                return AudioManager.GetInstance().GetMediaComponents<T>(assetName);
            }

            #region Mixer
            /// <summary>
            /// Set an exposed parameter value on the mixer and record it for auto restore
            /// </summary>
            /// <param name="mixer"></param>
            /// <param name="expParam"></param>
            /// <param name="val"></param>
            public static void SetMixerExposedParam(AudioMixer mixer, string expParam, float val)
            {
                AudioManager.GetInstance().SetMixerExposedParam(mixer, expParam, val);
            }

            /// <summary>
            /// Clear an exposed parameter of the mixer
            /// </summary>
            /// <param name="mixer"></param>
            /// <param name="expParam"></param>
            public static void ClearMixerExposedParam(AudioMixer mixer, string expParam)
            {
                AudioManager.GetInstance().ClearMixerExposedParam(mixer, expParam);
            }

            /// <summary>
            /// Clear all recorded exposed parameters belonging to the mixer
            /// </summary>
            /// <param name="mixer"></param>
            public static void AutoClearMixerExposedParams(AudioMixer mixer)
            {
                AudioManager.GetInstance().AutoClearMixerExposedParams(mixer);
            }

            /// <summary>
            /// Restore all recorded exposed parameters belonging to the mixer
            /// </summary>
            /// <param name="mixer"></param>
            public static void AutoRestoreMixerExposedParams(AudioMixer mixer)
            {
                AudioManager.GetInstance().AutoRestoreMixerExposedParams(mixer);
            }

            /// <summary>
            /// Switch the mixer to a snapshot by its name
            /// </summary>
            /// <param name="mixer"></param>
            /// <param name="snapshotName"></param>
            public static void SetMixerSnapshot(AudioMixer mixer, string snapshotName)
            {
                AudioManager.GetInstance().SetMixerSnapshot(mixer, snapshotName);
            }

            /// <summary>
            /// Transition the mixer to a weighted blend of snapshots over a specific reach time
            /// </summary>
            /// <param name="mixer"></param>
            /// <param name="snapshots"></param>
            /// <param name="weights"></param>
            /// <param name="timeToReach"></param>
            public static void SetMixerTransitionToSnapshot(AudioMixer mixer, AudioMixerSnapshot[] snapshots, float[] weights, float timeToReach = 0.02f)
            {
                AudioManager.GetInstance().SetMixerTransitionToSnapshot(mixer, snapshots, weights, timeToReach);
            }

            /// <summary>
            /// Get a snapshot from the mixer by its name
            /// </summary>
            /// <param name="mixer"></param>
            /// <param name="snapshotName"></param>
            /// <returns></returns>
            public static AudioMixerSnapshot GetMixerSnapshot(AudioMixer mixer, string snapshotName)
            {
                return AudioManager.GetInstance().GetMixerSnapshot(mixer, snapshotName);
            }

            /// <summary>
            /// Get a mixer by its name from the mixer list configured in the AudioManager
            /// </summary>
            /// <param name="mixerName"></param>
            /// <returns></returns>
            public static AudioMixer GetMixerByName(string mixerName)
            {
                return AudioManager.GetInstance().GetMixerByName(mixerName);
            }
            #endregion

            #region Audio
            /// <summary>
            /// <para>Preload an audio asset into the cache by its asset name with the default package name</para>
            /// <para>If use prefix "res#" will load from resources else will load from bundle</para>
            /// </summary>
            /// <param name="assetName"></param>
            /// <returns></returns>
            public static async UniTask Preload(string assetName)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await AudioManager.GetInstance().Preload(packageName, new string[] { assetName });
            }

            /// <summary>
            /// <para>Preload an audio asset into the cache by its asset name with a specific package name</para>
            /// <para>If use prefix "res#" will load from resources else will load from bundle</para>
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="assetName"></param>
            /// <returns></returns>
            public static async UniTask Preload(string packageName, string assetName)
            {
                await AudioManager.GetInstance().Preload(packageName, new string[] { assetName });
            }

            /// <summary>
            /// <para>Preload a set of audio assets into the cache by their asset names with the default package name</para>
            /// <para>If use prefix "res#" will load from resources else will load from bundle</para>
            /// </summary>
            /// <param name="assetNames"></param>
            /// <returns></returns>
            public static async UniTask Preload(string[] assetNames)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await AudioManager.GetInstance().Preload(packageName, assetNames);
            }

            /// <summary>
            /// <para>Preload a set of audio assets into the cache by their asset names with a specific package name</para>
            /// <para>If use prefix "res#" will load from resources else will load from bundle</para>
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="assetNames"></param>
            /// <returns></returns>
            public static async UniTask Preload(string packageName, string[] assetNames)
            {
                await AudioManager.GetInstance().Preload(packageName, assetNames);
            }

            /// <summary>
            /// <para>Play an audio by its asset name with the default package name, optionally under a parent Transform</para>
            /// <para>If use prefix "res#" will load from resources else will load from bundle</para>
            /// <para>The volume > 0 will be set, the default volume is only set from the AudioSource</para>
            /// <para>The loops -1 will loop forever, the default loops is only set from the AudioBase</para>
            /// </summary>
            /// <param name="assetName"></param>
            /// <param name="parent"></param>
            /// <param name="loops"></param>
            /// <param name="volume"></param>
            /// <returns></returns>
            public static async UniTask<AudioBase> Play(string assetName, Transform parent = null, int loops = 0, float volume = 0f)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                var media = await AudioManager.GetInstance().Play(packageName, assetName, null, parent, loops, volume);
                return media[0];
            }

            /// <summary>
            /// <para>Play an audio by its asset name with a specific package name, optionally under a parent Transform</para>
            /// <para>If use prefix "res#" will load from resources else will load from bundle</para>
            /// <para>The volume > 0 will be set, the default volume is only set from the AudioSource</para>
            /// <para>The loops -1 will loop forever, the default loops is only set from the AudioBase</para>
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="assetName"></param>
            /// <param name="parent"></param>
            /// <param name="loops"></param>
            /// <param name="volume"></param>
            /// <returns></returns>
            public static async UniTask<AudioBase> Play(string packageName, string assetName, Transform parent = null, int loops = 0, float volume = 0f)
            {
                var media = await AudioManager.GetInstance().Play(packageName, assetName, null, parent, loops, volume);
                return media[0];
            }

            /// <summary>
            /// <para>Play an audio by its asset name with the default package name, assigning the given AudioClip as the source clip</para>
            /// <para>If use prefix "res#" will load from resources else will load from bundle</para>
            /// <para>The volume > 0 will be set, the default volume is only set from the AudioSource</para>
            /// <para>The loops -1 will loop forever, the default loops is only set from the AudioBase</para>
            /// </summary>
            /// <param name="assetName"></param>
            /// <param name="sourceClip"></param>
            /// <param name="parent"></param>
            /// <param name="loops"></param>
            /// <param name="volume"></param>
            /// <returns></returns>
            public static async UniTask<AudioBase> Play(string assetName, AudioClip sourceClip, Transform parent = null, int loops = 0, float volume = 0f)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                var media = await AudioManager.GetInstance().Play(packageName, assetName, sourceClip, parent, loops, volume);
                return media[0];
            }

            /// <summary>
            /// <para>Play an audio by its asset name with a specific package name, assigning the given AudioClip as the source clip</para>
            /// <para>If use prefix "res#" will load from resources else will load from bundle</para>
            /// <para>The volume > 0 will be set, the default volume is only set from the AudioSource</para>
            /// <para>The loops -1 will loop forever, the default loops is only set from the AudioBase</para>
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="assetName"></param>
            /// <param name="sourceClip"></param>
            /// <param name="parent"></param>
            /// <param name="loops"></param>
            /// <param name="volume"></param>
            /// <returns></returns>
            public static async UniTask<AudioBase> Play(string packageName, string assetName, AudioClip sourceClip, Transform parent = null, int loops = 0, float volume = 0f)
            {
                var media = await AudioManager.GetInstance().Play(packageName, assetName, sourceClip, parent, loops, volume);
                return media[0];
            }

            /// <summary>
            /// Resume all paused audios
            /// </summary>
            public static void ResumeAll()
            {
                AudioManager.GetInstance().ResumeAll();
            }

            /// <summary>
            /// <para>Stop all audios matching the asset name</para>
            /// <para>The disableEndEvent will suppress the end event invocation, the forceDestroy will destroy the audio even if its onStopAndDestroy is off</para>
            /// </summary>
            /// <param name="assetName"></param>
            /// <param name="disableEndEvent"></param>
            /// <param name="forceDestroy"></param>
            public static void Stop(string assetName, bool disableEndEvent = false, bool forceDestroy = false)
            {
                AudioManager.GetInstance().Stop(assetName, disableEndEvent, forceDestroy);
            }

            /// <summary>
            /// <para>Stop all cached audios</para>
            /// <para>The disableEndEvent will suppress the end event invocation, the forceDestroy will destroy the audio even if its onStopAndDestroy is off</para>
            /// </summary>
            /// <param name="disableEndEvent"></param>
            /// <param name="forceDestroy"></param>
            public static void StopAll(bool disableEndEvent = false, bool forceDestroy = false)
            {
                AudioManager.GetInstance().StopAll(disableEndEvent, forceDestroy);
            }

            /// <summary>
            /// Pause all audios matching the asset name
            /// </summary>
            /// <param name="assetName"></param>
            public static void Pause(string assetName)
            {
                AudioManager.GetInstance().Pause(assetName);
            }

            /// <summary>
            /// Pause all cached audios
            /// </summary>
            public static void PauseAll()
            {
                AudioManager.GetInstance().PauseAll();
            }

            /// <summary>
            /// <para>Force unload the audio source asset by its asset name</para>
            /// <para>If any spawned instances still exist, all cached audios will be stopped and destroyed first</para>
            /// </summary>
            /// <param name="assetName"></param>
            public static void ForceUnload(string assetName)
            {
                AudioManager.GetInstance().ForceUnload(assetName);
            }
            #endregion
        }

        /// <summary>
        /// Bridge the internal VideoManager as public video playback APIs
        /// </summary>
        public static class VideoFrame
        {
            /// <summary>
            /// Initialize the VideoManager instance
            /// </summary>
            public static void InitInstance()
            {
                VideoManager.GetInstance();
            }

            /// <summary>
            /// Get the first spawned video component matching the asset name, returns null if not found
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="assetName"></param>
            /// <returns></returns>
            public static T GetComponent<T>(string assetName) where T : VideoBase
            {
                var components = VideoManager.GetInstance().GetMediaComponents<T>(assetName);
                if (components.Length > 0) return components[0];

                return null;
            }

            /// <summary>
            /// Get all spawned video components matching the asset name
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="assetName"></param>
            /// <returns></returns>
            public static T[] GetComponents<T>(string assetName) where T : VideoBase
            {
                return VideoManager.GetInstance().GetMediaComponents<T>(assetName);
            }

            #region Video
            /// <summary>
            /// <para>Preload a video asset into the cache by its asset name with the default package name</para>
            /// <para>If use prefix "res#" will load from resources else will load from bundle</para>
            /// </summary>
            /// <param name="assetName"></param>
            /// <returns></returns>
            public static async UniTask Preload(string assetName)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await VideoManager.GetInstance().Preload(packageName, new string[] { assetName });
            }

            /// <summary>
            /// <para>Preload a video asset into the cache by its asset name with a specific package name</para>
            /// <para>If use prefix "res#" will load from resources else will load from bundle</para>
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="assetName"></param>
            /// <returns></returns>
            public static async UniTask Preload(string packageName, string assetName)
            {
                await VideoManager.GetInstance().Preload(packageName, new string[] { assetName });
            }

            /// <summary>
            /// <para>Preload a set of video assets into the cache by their asset names with the default package name</para>
            /// <para>If use prefix "res#" will load from resources else will load from bundle</para>
            /// </summary>
            /// <param name="assetNames"></param>
            /// <returns></returns>
            public static async UniTask Preload(string[] assetNames)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await VideoManager.GetInstance().Preload(packageName, assetNames);
            }

            /// <summary>
            /// <para>Preload a set of video assets into the cache by their asset names with a specific package name</para>
            /// <para>If use prefix "res#" will load from resources else will load from bundle</para>
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="assetNames"></param>
            /// <returns></returns>
            public static async UniTask Preload(string packageName, string[] assetNames)
            {
                await VideoManager.GetInstance().Preload(packageName, assetNames);
            }

            /// <summary>
            /// <para>Play a video by its asset name with the default package name, optionally under a parent Transform</para>
            /// <para>If use prefix "res#" will load from resources else will load from bundle</para>
            /// <para>The volume > 0 will be set, the default volume is only set from the VideoPlayer</para>
            /// <para>The loops -1 will loop forever, the default loops is only set from the VideoBase</para>
            /// </summary>
            /// <param name="assetName"></param>
            /// <param name="parent"></param>
            /// <param name="loops"></param>
            /// <param name="volume"></param>
            /// <returns></returns>
            public static async UniTask<VideoBase> Play(string assetName, Transform parent = null, int loops = 0, float volume = 0f)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                var media = await VideoManager.GetInstance().Play(packageName, assetName, null, parent, loops, volume);
                return media[0];
            }

            /// <summary>
            /// <para>Play a video by its asset name with a specific package name, optionally under a parent Transform</para>
            /// <para>If use prefix "res#" will load from resources else will load from bundle</para>
            /// <para>The volume > 0 will be set, the default volume is only set from the VideoPlayer</para>
            /// <para>The loops -1 will loop forever, the default loops is only set from the VideoBase</para>
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="assetName"></param>
            /// <param name="parent"></param>
            /// <param name="loops"></param>
            /// <param name="volume"></param>
            /// <returns></returns>
            public static async UniTask<VideoBase> Play(string packageName, string assetName, Transform parent = null, int loops = 0, float volume = 0f)
            {
                var media = await VideoManager.GetInstance().Play(packageName, assetName, null, parent, loops, volume);
                return media[0];
            }

            /// <summary>
            /// <para>Play a video by its asset name with the default package name, assigning the given VideoClip as the source clip</para>
            /// <para>If use prefix "res#" will load from resources else will load from bundle</para>
            /// <para>The volume > 0 will be set, the default volume is only set from the VideoPlayer</para>
            /// <para>The loops -1 will loop forever, the default loops is only set from the VideoBase</para>
            /// </summary>
            /// <param name="assetName"></param>
            /// <param name="sourceClip"></param>
            /// <param name="parent"></param>
            /// <param name="loops"></param>
            /// <param name="volume"></param>
            /// <returns></returns>
            public static async UniTask<VideoBase> Play(string assetName, VideoClip sourceClip, Transform parent = null, int loops = 0, float volume = 0f)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                var media = await VideoManager.GetInstance().Play(packageName, assetName, sourceClip, parent, loops, volume);
                return media[0];
            }

            /// <summary>
            /// <para>Play a video by its asset name with a specific package name, assigning the given VideoClip as the source clip</para>
            /// <para>If use prefix "res#" will load from resources else will load from bundle</para>
            /// <para>The volume > 0 will be set, the default volume is only set from the VideoPlayer</para>
            /// <para>The loops -1 will loop forever, the default loops is only set from the VideoBase</para>
            /// </summary>
            /// <param name="packageName"></param>
            /// <param name="assetName"></param>
            /// <param name="sourceClip"></param>
            /// <param name="parent"></param>
            /// <param name="loops"></param>
            /// <param name="volume"></param>
            /// <returns></returns>
            public static async UniTask<VideoBase> Play(string packageName, string assetName, VideoClip sourceClip, Transform parent = null, int loops = 0, float volume = 0f)
            {
                var media = await VideoManager.GetInstance().Play(packageName, assetName, sourceClip, parent, loops, volume);
                return media[0];
            }

            /// <summary>
            /// Resume all paused videos
            /// </summary>
            public static void ResumeAll()
            {
                VideoManager.GetInstance().ResumeAll();
            }

            /// <summary>
            /// <para>Stop all videos matching the asset name</para>
            /// <para>The disableEndEvent will suppress the end event invocation, the forceDestroy will destroy the video even if its onStopAndDestroy is off</para>
            /// </summary>
            /// <param name="assetName"></param>
            /// <param name="disableEndEvent"></param>
            /// <param name="forceDestroy"></param>
            public static void Stop(string assetName, bool disableEndEvent = false, bool forceDestroy = false)
            {
                VideoManager.GetInstance().Stop(assetName, disableEndEvent, forceDestroy);
            }

            /// <summary>
            /// <para>Stop all cached videos</para>
            /// <para>The disableEndEvent will suppress the end event invocation, the forceDestroy will destroy the video even if its onStopAndDestroy is off</para>
            /// </summary>
            /// <param name="disableEndEvent"></param>
            /// <param name="forceDestroy"></param>
            public static void StopAll(bool disableEndEvent = false, bool forceDestroy = false)
            {
                VideoManager.GetInstance().StopAll(disableEndEvent, forceDestroy);
            }

            /// <summary>
            /// Pause all videos matching the asset name
            /// </summary>
            /// <param name="assetName"></param>
            public static void Pause(string assetName)
            {
                VideoManager.GetInstance().Pause(assetName);
            }

            /// <summary>
            /// Pause all cached videos
            /// </summary>
            public static void PauseAll()
            {
                VideoManager.GetInstance().PauseAll();
            }

            /// <summary>
            /// <para>Force unload the video source asset by its asset name</para>
            /// <para>If any spawned instances still exist, all cached videos will be stopped and destroyed first</para>
            /// </summary>
            /// <param name="assetName"></param>
            public static void ForceUnload(string assetName)
            {
                VideoManager.GetInstance().ForceUnload(assetName);
            }
            #endregion
        }
    }
}
