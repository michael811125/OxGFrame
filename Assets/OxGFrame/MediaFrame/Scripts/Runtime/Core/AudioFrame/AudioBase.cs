using Cysharp.Threading.Tasks;
using MyBox;
using OxGFrame.AssetLoader;
using OxGKit.LoggingSystem;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Audio;

namespace OxGFrame.MediaFrame.AudioFrame
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioBase : MediaBase
    {
        protected AudioSource _audioSource = null;

        public AudioType audioType = new AudioType();
        public UnityEngine.AudioType audioFileType = UnityEngine.AudioType.MPEG;
        public SourceType sourceType = SourceType.Audio;
        // SourceType => AudioClip
        [Tooltip("Drag audio clip. This is not supports [WebGL]"), ConditionalField(nameof(sourceType), false, SourceType.Audio)]
        public AudioClip audioClip = null;
        // SourceType => StreamingAssets, Url
        [Tooltip("Can select the \"CacheType\" from the AudioManager's inspector."), ConditionalField(nameof(sourceType), true, SourceType.Audio)]
        public bool requestCached = true;
        [ConditionalField(new string[] { nameof(sourceType), nameof(requestCached) }, new bool[] { true }, new object[] { SourceType.Audio })]
        public int maxRequestTimeSeconds = AudioManager.MAX_REQUEST_TIME_SECONDS;
        // SourceType => StreamingAssets
        [Tooltip("Default path is [StreamingAssets]. Just set that inside path and file name, Don't forget file name must with extension, ex: Audio/example.mp3"), ConditionalField(nameof(sourceType), false, SourceType.StreamingAssets)]
        public string fullPathName = "";
        // SourceType => Url
        [ConditionalField(nameof(sourceType), false, SourceType.Url)]
        public UrlSet urlSet = new UrlSet();

        [HideInInspector, Tooltip("Manual to set audio length or press preload button to set [Unity has a bug in WebGL, Get an audio via UnityWebRequest cannot return length value]")]
        public float audioLength = 0;

        [SerializeField]
        protected MixerGroupSourceType _mixerGroupSourceType = MixerGroupSourceType.Assign;
        [SerializeField, ConditionalField(nameof(_mixerGroupSourceType), false, MixerGroupSourceType.Assign)]
        protected AudioMixerGroup _mixerGroup = null;
        [SerializeField, ConditionalField(nameof(_mixerGroupSourceType), false, MixerGroupSourceType.Find)]
        protected string _mixerName = "MasterMixer";
        [SerializeField, ConditionalField(nameof(_mixerGroupSourceType), false, MixerGroupSourceType.Find)]
        protected string _mixerGroupName = "";

#if OXGFRAME_AUDIOFRAME_MONODRIVE_FIXEDUPDATE_ON
        private void FixedUpdate()
        {
            if (this.monoDrive)
                this.HandleFixedUpdate(Time.fixedDeltaTime);
        }
#endif

        internal sealed override async UniTask<bool> Init()
        {
            this._audioSource = this.GetComponent<AudioSource>();
            bool isInitialized = await this._InitAudio();

            if (isInitialized)
                this._isInit = true;

            return this._isInit;
        }

        private async UniTask<bool> _InitAudio()
        {
            this.isPrepared = false;

            if (this._audioSource == null)
                return false;

            // Get Audio
            switch (this.sourceType)
            {
                case SourceType.StreamingAssets:
                    this.audioClip = await this.GetAudioFromStreamingAssets(this.requestCached, this.maxRequestTimeSeconds <= 0 ? AudioManager.MAX_REQUEST_TIME_SECONDS : this.maxRequestTimeSeconds);
                    break;
                case SourceType.Url:
                    this.audioClip = await this.GetAudioFromURL(this.requestCached, this.maxRequestTimeSeconds <= 0 ? AudioManager.MAX_REQUEST_TIME_SECONDS : this.maxRequestTimeSeconds);
                    break;
            }

            if (this.audioClip == null)
            {
                Logging.PrintError<Logger>($"Cannot find AudioClip: {this.mediaName}");
                return false;
            }

            // Get Mixer Group
            switch (this._mixerGroupSourceType)
            {
                case MixerGroupSourceType.Assign:
                    break;
                case MixerGroupSourceType.Find:
                    var masterMixer = AudioManager.GetInstance().GetMixerByName(this._mixerName);
                    var mixerGroup = masterMixer?.FindMatchingGroups(this._mixerGroupName)[0];
                    this._mixerGroup = mixerGroup;
                    break;
            }

            this._audioSource.clip = this.audioClip;
            this._audioSource.mute = true;
            this._audioSource.playOnAwake = false;
            this._audioSource.priority = this.audioType.priority;
            this._audioSource.outputAudioMixerGroup = this._mixerGroup;
            this._audioSource.loop = (this.loops == -1) ? true : false;
            this._mediaLength = this._currentRemainingLength = (this.audioLength > 0) ? this.audioLength : this.audioClip.length;

            #region Prepare
            // To make sure audio is ready to play
            if (this._audioSource.clip != null &&
                this._audioSource.clip.loadState != AudioDataLoadState.Loaded)
            {
                this._audioSource.clip.LoadAudioData();
                Logging.Print<Logger>($"{this.mediaName} audio preparation started...");
            }

            var cts = new CancellationTokenSource();
            cts.CancelAfterSlim(TimeSpan.FromSeconds(this.maxPrepareTimeSeconds <= 0 ? MAX_PREPARE_TIME_SECONDS : this.maxPrepareTimeSeconds));
            try
            {
                do
                {
                    if (this._audioSource.clip != null &&
                        this._audioSource.clip.loadState == AudioDataLoadState.Loaded)
                        break;
                    // load balancing
                    await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cts.Token);
                } while (true);
                Logging.Print<Logger>($"{this.mediaName} audio is prepared");
            }
            catch (OperationCanceledException ex)
            {
                Logging.PrintException<Logger>(ex);
                return false;
            }
            #endregion

            this.isPrepared = true;

            Logging.Print<Logger>($"【Init Once】 Asset Name: {this.mediaName}, Audio length: {this._mediaLength} (s). AudioSource => Time: {this._audioSource.time}, TimeSamples: {this._audioSource.timeSamples}; AudioClip => Time: {this._audioSource.clip.length}, Samples: {this._audioSource.clip.samples}, Freq: {this._audioSource.clip.frequency}.");

            return this.isPrepared;
        }

        public async UniTask<AudioClip> GetAudioFromStreamingAssets(bool cached, int requestTimeSeconds = AudioManager.MAX_REQUEST_TIME_SECONDS)
        {
            string pathName = System.IO.Path.Combine(GetRequestStreamingAssetsPath(), this.fullPathName);
            var audioClip = await AudioManager.GetInstance().reuqester.SelfRequestAudio(pathName, this.audioFileType, null, null, null, cached, requestTimeSeconds);
            return audioClip;
        }

        public async UniTask<AudioClip> GetAudioFromURL(bool cached, int requestTimeSeconds = AudioManager.MAX_REQUEST_TIME_SECONDS)
        {
            string urlCfg = await this.urlSet.urlCfg.GetFileText();
            string urlSet = this.urlSet.getUrlPathFromCfg ? GetValueFromUrlCfg(urlCfg, AUDIO_URLSET) : string.Empty;
            string url = (!string.IsNullOrEmpty(urlSet)) ? $"{urlSet.Trim()}{this.urlSet.url.Trim()}" : this.urlSet.url.Trim();
            var audioClip = await AudioManager.GetInstance().reuqester.SelfRequestAudio(url, this.audioFileType, null, null, null, cached, requestTimeSeconds);
            return audioClip;
        }

        protected sealed override void OnFixedUpdate(float dt = 0f)
        {
            if (this._audioSource == null)
                return;

            if (!this.isPrepared)
                return;

            if (this.IsPaused())
                return;

            if (!this.IsPlaying())
                return;

            this.ProcessLooping(this._audioSource, dt);
        }

        public sealed override void Play(int loops, float volume)
        {
            if (this._audioSource == null ||
                this._audioSource.clip == null)
                return;

            this.gameObject.SetActive(true);

            this._audioSource.mute = false;

            if (!this.IsPaused())
                this._loops = (loops == -1 || loops > 0) ? loops : this.loops;

            if (this._loops == -1)
                this._audioSource.loop = true;

            volume = (volume > 0f) ? volume : this._audioSource.volume;
            this._audioSource.volume = volume;

            if (!this.IsPaused())
                this._audioSource.Play();
            else
                this._audioSource.UnPause();

            this._isPlaying = true;
            this._isPaused = false;
        }

        public sealed override void Stop()
        {
            if (this._audioSource == null)
                return;

            this._isPlaying = false;
            this._isPaused = false;

            this._audioSource.Stop();
            this.ResetLength();
            this.ResetLoops();

            this._endEvent?.Invoke();
            this._endEvent = null;

            this.gameObject.SetActive(false);

            // Only for mono drive
            if (this.monoDrive)
            {
                if (this.onStopAndDestroy)
                    Destroy(this.gameObject);
            }
        }

        public sealed override void Pause()
        {
            if (this._audioSource == null)
                return;

            this._isPlaying = false;
            this._isPaused = true;

            this._audioSource.Pause();
        }

        public sealed override bool IsPlaying()
        {
            return this._isPlaying;
        }

        public sealed override bool IsPaused()
        {
            return this._isPaused;
        }

        public sealed override bool IsLooping()
        {
            if (this._audioSource == null)
                return false;
            return this._audioSource.loop;
        }

        protected sealed override void StopSelf()
        {
            if (this.monoDrive)
                this.Stop();
            else
                AudioManager.GetInstance().Stop(this);
        }

        public sealed override float Length()
        {
            return this._mediaLength;
        }

        public sealed override float CurrentLength()
        {
            return this._mediaLength - this._currentRemainingLength;
        }

        public sealed override float CurrentRemainingLength()
        {
            return this._currentRemainingLength;
        }

        public sealed override void OnRelease()
        {
            this._endEvent?.Invoke();

            base.OnRelease();
            this._audioSource = null;
            this._mixerGroup = null;
            this.audioClip = null;
            this.audioType = null;
            this.fullPathName = null;
            this.urlSet = null;
        }

        public AudioSource GetAudioSource()
        {
            return this._audioSource;
        }

        internal protected sealed override void DetectOnDestroy()
        {
            if (this.monoDrive)
            {
                this.OnRelease();
                if (this.onDestroyAndUnload)
                    AssetLoaders.UnloadAsset(this.assetName);
                this.assetName = null;
                this.mediaName = null;
            }
            else
            {
                if (Time.frameCount == 0 || !Application.isPlaying)
                    return;

                try
                {
                    if (!this.isDestroying)
                        AudioManager.GetInstance().Stop(this, true, true);
                }
                catch
                {
                    /* Nothing to do */
                }
            }
        }
    }
}