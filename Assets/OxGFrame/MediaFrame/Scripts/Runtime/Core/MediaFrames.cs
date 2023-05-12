using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader;
using OxGFrame.MediaFrame.AudioFrame;
using OxGFrame.MediaFrame.VideoFrame;
using UnityEngine;
using UnityEngine.Audio;

namespace OxGFrame.MediaFrame
{
    public static class MediaFrames
    {
        public static class AudioFrame
        {
            public static void InitInstance()
            {
                AudioManager.GetInstance();
            }

            public static T GetComponent<T>(string assetName) where T : AudioBase
            {
                var components = AudioManager.GetInstance().GetMediaComponents<T>(assetName);
                if (components.Length > 0) return components[0];

                return null;
            }

            public static T[] GetComponents<T>(string assetName) where T : AudioBase
            {
                return AudioManager.GetInstance().GetMediaComponents<T>(assetName);
            }

            #region Mixer
            public static void SetMixerExposedParam(AudioMixer mixer, string expParam, float val)
            {
                AudioManager.GetInstance().SetMixerExposedParam(mixer, expParam, val);
            }

            public static void ClearMixerExposedParam(AudioMixer mixer, string expParam)
            {
                AudioManager.GetInstance().ClearMixerExposedParam(mixer, expParam);
            }

            public static void AutoClearMixerExposedParams(AudioMixer mixer)
            {
                AudioManager.GetInstance().AutoClearMixerExposedParams(mixer);
            }

            public static void AutoRestoreMixerExposedParams(AudioMixer mixer)
            {
                AudioManager.GetInstance().AutoRestoreMixerExposedParams(mixer);
            }

            public static void SetMixerSnapshot(AudioMixer mixer, string snapshotName)
            {
                AudioManager.GetInstance().SetMixerSnapshot(mixer, snapshotName);
            }

            public static void SetMixerTransitionToSnapshot(AudioMixer mixer, AudioMixerSnapshot[] snapshots, float[] weights, float timeToReach = 0.02f)
            {
                AudioManager.GetInstance().SetMixerTransitionToSnapshot(mixer, snapshots, weights, timeToReach);
            }

            public static AudioMixerSnapshot GetMixerSnapshot(AudioMixer mixer, string snapshotName)
            {
                return AudioManager.GetInstance().GetMixerSnapshot(mixer, snapshotName);
            }

            public static AudioMixer GetMixerByName(string mixerName)
            {
                return AudioManager.GetInstance().GetMixerByName(mixerName);
            }
            #endregion

            #region Audio
            /// <summary>
            /// If use prefix "res#" will load from resources else will load from bundle
            /// </summary>
            /// <param name="assetName"></param>
            /// <returns></returns>
            public static async UniTask Preload(string assetName)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await AudioManager.GetInstance().Preload(packageName, new string[] { assetName });
            }

            public static async UniTask Preload(string packageName, string assetName)
            {
                await AudioManager.GetInstance().Preload(packageName, new string[] { assetName });
            }

            public static async UniTask Preload(string[] assetNames)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await AudioManager.GetInstance().Preload(packageName, assetNames);
            }

            public static async UniTask Preload(string packageName, string[] assetNames)
            {
                await AudioManager.GetInstance().Preload(packageName, assetNames);
            }

            /// <summary>
            /// <para>If use prefix "res#" will load from resources else will load from bundle</para>
            /// <para>The volume > 0 will be set, the default volume is only set from the AudioSource</para>
            /// </summary>
            /// <param name="assetName"></param>
            /// <param name="parent"></param>
            /// <param name="loops"></param>
            /// <returns></returns>
            public static async UniTask<AudioBase> Play(string assetName, Transform parent = null, int loops = 0, float volume = 0f)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                var media = await AudioManager.GetInstance().Play(packageName, assetName, parent, loops, volume);
                return media[0];
            }

            public static async UniTask<AudioBase> Play(string packageName, string assetName, Transform parent = null, int loops = 0, float volume = 0f)
            {
                var media = await AudioManager.GetInstance().Play(packageName, assetName, parent, loops, volume);
                return media[0];
            }

            public static void ResumeAll()
            {
                AudioManager.GetInstance().ResumeAll();
            }

            public static void Stop(string assetName, bool disableEndEvent = false, bool forceDestroy = false)
            {
                AudioManager.GetInstance().Stop(assetName, disableEndEvent, forceDestroy);
            }

            public static void StopAll(bool disableEndEvent = false, bool forceDestroy = false)
            {
                AudioManager.GetInstance().StopAll(disableEndEvent, forceDestroy);
            }

            public static void Pause(string assetName)
            {
                AudioManager.GetInstance().Pause(assetName);
            }

            public static void PauseAll()
            {
                AudioManager.GetInstance().PauseAll();
            }

            public static void ForceUnload(string assetName)
            {
                AudioManager.GetInstance().ForceUnload(assetName);
            }
            #endregion
        }

        public static class VideoFrame
        {
            public static void InitInstance()
            {
                VideoManager.GetInstance();
            }

            public static T GetComponent<T>(string assetName) where T : VideoBase
            {
                var components = VideoManager.GetInstance().GetMediaComponents<T>(assetName);
                if (components.Length > 0) return components[0];

                return null;
            }

            public static T[] GetComponents<T>(string assetName) where T : VideoBase
            {
                return VideoManager.GetInstance().GetMediaComponents<T>(assetName);
            }

            #region Video
            /// <summary>
            /// If use prefix "res#" will load from resources else will load from bundle
            /// </summary>
            /// <param name="assetName"></param>
            /// <returns></returns>
            public static async UniTask Preload(string assetName)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await VideoManager.GetInstance().Preload(packageName, new string[] { assetName });
            }

            public static async UniTask Preload(string packageName, string assetName)
            {
                await VideoManager.GetInstance().Preload(packageName, new string[] { assetName });
            }

            public static async UniTask Preload(string[] assetNames)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                await VideoManager.GetInstance().Preload(packageName, assetNames);
            }

            public static async UniTask Preload(string packageName, string[] assetNames)
            {
                await VideoManager.GetInstance().Preload(packageName, assetNames);
            }

            /// <summary>
            /// <para>If use prefix "res#" will load from resources else will load from bundle</para>
            /// <para>The volume > 0 will be set, the default volume is only set from the VideoPlayer</para>
            /// </summary>
            /// <param name="assetName"></param>
            /// <param name="parent"></param>
            /// <param name="loops"></param>
            /// <returns></returns>
            public static async UniTask<VideoBase> Play(string assetName, Transform parent = null, int loops = 0, float volume = 0f)
            {
                var packageName = AssetPatcher.GetDefaultPackageName();
                var media = await VideoManager.GetInstance().Play(packageName, assetName, parent, loops, volume);
                return media[0];
            }

            public static async UniTask<VideoBase> Play(string packageName, string assetName, Transform parent = null, int loops = 0, float volume = 0f)
            {
                var media = await VideoManager.GetInstance().Play(packageName, assetName, parent, loops, volume);
                return media[0];
            }

            public static void ResumeAll()
            {
                VideoManager.GetInstance().ResumeAll();
            }

            public static void Stop(string assetName, bool disableEndEvent = false, bool forceDestroy = false)
            {
                VideoManager.GetInstance().Stop(assetName, disableEndEvent, forceDestroy);
            }

            public static void StopAll(bool disableEndEvent = false, bool forceDestroy = false)
            {
                VideoManager.GetInstance().StopAll(disableEndEvent, forceDestroy);
            }

            public static void Pause(string assetName)
            {
                VideoManager.GetInstance().Pause(assetName);
            }

            public static void PauseAll()
            {
                VideoManager.GetInstance().PauseAll();
            }

            public static void ForceUnload(string assetName)
            {
                VideoManager.GetInstance().ForceUnload(assetName);
            }
            #endregion
        }
    }
}
