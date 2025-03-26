using Cysharp.Threading.Tasks;
using MyBox;
using OxGKit.LoggingSystem;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Video;

namespace OxGFrame.MediaFrame.VideoFrame
{
    [RequireComponent(typeof(VideoPlayer))]
    public class VideoBase : MediaBase
    {
        protected VideoPlayer _videoPlayer = null;

        [Tooltip("Wait for first frame to be ready before starting playback.")]
        public bool waitForFirstFrame = true;
        [Range(0, 10)]
        public float playbackSpeed = 1f;
        public SourceType sourceType = SourceType.Video;
        public int maxPrepareTimeSeconds = VideoManager.MAX_PREPARE_TIME_SECONDS;
        // SourceType => VideoClip
        [Tooltip("Drag video clip. This is not supports [WebGL]"), ConditionalField(nameof(sourceType), false, SourceType.Video)]
        public VideoClip videoClip = null;
        // SourceType => StreamingAssets
        [Tooltip("Default path is [StreamingAssets]. Just set that inside path and file name, Don't forget file name must with extension, ex: Video/example.mp4"), ConditionalField(nameof(sourceType), false, SourceType.StreamingAssets)]
        public string fullPathName = "";
        // SourceType => Url
        [ConditionalField(nameof(sourceType), false, SourceType.Url)]
        public UrlSet urlSet = new UrlSet();

        [Tooltip("Can via [VideoManager] to get ")]
        public RenderMode renderMode = RenderMode.RenderTexture;
        protected RenderTexture _targetRt = null;
        [SerializeField, ConditionalField(nameof(renderMode), false, RenderMode.RenderTexture), Tooltip("Size of RenderTexture")]
        protected Vector2Int _renderTextureSize = new Vector2Int(2048, 2048);
        [SerializeField, ConditionalField(nameof(renderMode), false, RenderMode.Camera)]
        protected TargetCamera _targetCamera = new TargetCamera();
        [SerializeField]
        protected VideoAspectRatio _aspectRatio = VideoAspectRatio.FitHorizontally;

        internal override async UniTask<bool> Init()
        {
            this._videoPlayer = this.GetComponent<VideoPlayer>();
            bool isInitialized = await this._InitVideo();

            if (isInitialized)
                this._isInit = true; // Mark all init is finished.

            return this._isInit;
        }

        protected bool TrySetUrl(string url)
        {
            try
            {
                this._videoPlayer.url = url;
            }
            catch
            {
                return false;
            }

            return true;
        }

        private async UniTask<bool> _InitVideo()
        {
            this.isPrepared = false;

            if (this._videoPlayer == null)
                return false;

            switch (this.sourceType)
            {
                case SourceType.Video:
                    this._videoPlayer.source = VideoSource.VideoClip;
                    this._videoPlayer.clip = this.videoClip;
                    if (this.videoClip == null)
                    {
                        Logging.Print<Logger>($"<color=#FF0000>Cannot find VideoClip: {this.mediaName}</color>");
                        return false;
                    }
                    break;
                case SourceType.StreamingAssets:
                    {
                        this._videoPlayer.source = VideoSource.Url;
                        string url = System.IO.Path.Combine(Application.streamingAssetsPath, this.fullPathName);
                        if (!this.TrySetUrl(url))
                        {
                            Logging.Print<Logger>($"<color=#FF0000>Cannot find VideoClip: {this.mediaName}</color>");
                            return false;
                        }
                    }
                    break;
                case SourceType.Url:
                    {
                        this._videoPlayer.source = VideoSource.Url;
                        string urlCfg = await this.urlSet.urlCfg.GetFileText();
                        string urlSet = this.urlSet.getUrlPathFromCfg ? GetValueFromUrlCfg(urlCfg, VIDEO_URLSET) : string.Empty;
                        string url = (!string.IsNullOrEmpty(urlSet)) ? $"{urlSet.Trim()}{this.urlSet.url.Trim()}" : this.urlSet.url.Trim();
                        if (!this.TrySetUrl(url))
                        {
                            Logging.Print<Logger>($"<color=#FF0000>Cannot find VideoClip: {this.mediaName}</color>");
                            return false;
                        }
                    }
                    break;
            }

            this._videoPlayer.Prepare();
            Logging.Print<Logger>($"{this.mediaName} video is preparing...");
            var cts = new CancellationTokenSource();
            cts.CancelAfterSlim(TimeSpan.FromSeconds(this.maxPrepareTimeSeconds <= 0 ? VideoManager.MAX_PREPARE_TIME_SECONDS : this.maxPrepareTimeSeconds));
            try
            {
                do
                {
                    if (this._videoPlayer.isPrepared)
                        break;
                    // load balancing
                    await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cts.Token);
                } while (true);
                Logging.Print<Logger>($"{this.mediaName} video is prepared");
            }
            catch (OperationCanceledException ex)
            {
                Logging.PrintException<Logger>(ex);
                return false;
            }

            this._videoPlayer.SetDirectAudioMute(0, true);
            this._videoPlayer.playOnAwake = false;
            this._videoPlayer.waitForFirstFrame = this.waitForFirstFrame;
            this._videoPlayer.isLooping = (this.loops == -1) ? true : false;
            this._videoPlayer.playbackSpeed = this.playbackSpeed;
            this._videoPlayer.aspectRatio = this._aspectRatio;

            switch (this.renderMode)
            {
                case RenderMode.RenderTexture:
                    this._SetTargetRenderTexture();
                    break;
                case RenderMode.Camera:
                    this._SetTargetCamera();
                    break;
            }

            this._mediaLength = this._currentRemainingLength = (1f * this._videoPlayer.frameCount / this._videoPlayer.playbackSpeed / this._videoPlayer.frameRate);

            this.isPrepared = true;

            Logging.Print<Logger>($"<color=#00EEFF>【Init Once】 Video length: {this._mediaLength} (s)</color>");

            return this.isPrepared;
        }

        /// <summary>
        /// 返回取得映射的目標 RenderTexture (Only RenderTexture Mode)
        /// </summary>
        /// <returns></returns>
        public RenderTexture GetTargetRenderTexture()
        {
            return this._targetRt;
        }

        /// <summary>
        /// 使用 RenderTexture.GetTemporary (避免內存膨脹) 取得 RenderTexture
        /// </summary>
        private void _SetTargetRenderTexture()
        {
            RenderTexture tempRt = RenderTexture.GetTemporary(this._renderTextureSize.x, this._renderTextureSize.y, 24, RenderTextureFormat.ARGB32);

            this._videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            this._videoPlayer.targetTexture = tempRt;

            this._targetRt = tempRt;
        }

        /// <summary>
        /// 使用 RenderTexture.ReleaseTemporary (避免內存膨脹) 釋放 RenderTexture
        /// </summary>
        private void _ReleaseTargetRenderTexture()
        {
            RenderTexture.ReleaseTemporary(this._targetRt);
            this._targetRt = null;
        }

        private void _SetTargetCamera()
        {
            if (this._targetCamera.camera == null)
                return;

            switch (this._targetCamera.orderType)
            {
                case TargetCamera.OrderType.Background:
                    this._videoPlayer.renderMode = VideoRenderMode.CameraFarPlane;
                    break;
                case TargetCamera.OrderType.Foreground:
                    this._videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
                    break;
            }
            this._videoPlayer.targetCamera = this._targetCamera.camera;
            this._videoPlayer.targetCameraAlpha = this._targetCamera.alpha;
        }

        protected override void OnFixedUpdate(float dt = 0f)
        {
            if (this._videoPlayer == null)
                return;

            if (!this.isPrepared)
                return;

            if (this.IsPaused())
                return;

            if (this.CurrentRemainingLength() > 0f)
            {
                this._currentRemainingLength -= dt;
                if (this.CurrentRemainingLength() <= 0f)
                {
                    if (this._loops >= 0)
                    {
                        this._videoPlayer.Stop();

                        this._loops--;
                        if (this._loops <= 0)
                        {
                            this._currentRemainingLength = 0;
                            if (this.autoEndToStop)
                                this.StopSelf();
                        }
                        else
                            this._videoPlayer.Play();
                    }
                    this._currentRemainingLength = this.Length();
                }
            }
        }

        internal override void Play(int loops, float volume)
        {
            if (this._videoPlayer == null)
                return;

            this.gameObject.SetActive(true);

            this._videoPlayer.SetDirectAudioMute(0, false);

            if (!this.IsPaused())
                this._loops = (loops == -1 || loops > 0) ? loops : this.loops;

            if (this._loops == -1)
                this._videoPlayer.isLooping = true;

            volume = (volume > 0f) ? volume : this._videoPlayer.GetDirectAudioVolume(0);
            this._videoPlayer.SetDirectAudioVolume(0, volume);

            this._videoPlayer.Play();

            this._isPaused = false; // 取消暫停標記
        }

        internal override void Stop()
        {
            if (this._videoPlayer == null)
                return;

            this._videoPlayer.Stop();
            this.ResetLength();
            this.ResetLoops();

            // RenderTexture 需要額外釋放, 避免內存膨脹
            if (this._targetRt != null)
                this._ReleaseTargetRenderTexture();

            this._endEvent?.Invoke();
            this._endEvent = null;

            this.gameObject.SetActive(false);
        }

        internal override void Pause()
        {
            if (this._videoPlayer == null)
                return;

            this._isPaused = true; // 標記暫停
            this._videoPlayer.Pause();
        }

        public override bool IsPlaying()
        {
            if (this._videoPlayer == null)
                return false;
            return this._videoPlayer.isPlaying;
        }

        public override bool IsPaused()
        {
            return this._isPaused;
        }

        public override bool IsLooping()
        {
            if (this._videoPlayer == null)
                return false;
            return this._videoPlayer.isLooping;
        }

        protected override void StopSelf()
        {
            VideoManager.GetInstance().Stop(this);
        }

        public override float Length()
        {
            return this._mediaLength;
        }

        public override float CurrentLength()
        {
            return this._mediaLength - this._currentRemainingLength;
        }

        public override float CurrentRemainingLength()
        {
            return this._currentRemainingLength;
        }

        public override void OnRelease()
        {
            // RenderTexture 需要額外釋放, 避免內存膨脹
            if (this._targetRt != null)
                this._ReleaseTargetRenderTexture();

            this._endEvent?.Invoke();

            base.OnRelease();
            this._videoPlayer = null;
            this.videoClip = null;
            this.fullPathName = null;
            this.urlSet = null;
            this._targetCamera = null;
        }

        /// <summary>
        /// Skip range is 0.0 to 1.0 pct
        /// </summary>
        /// <param name="pct"></param>
        public void SkipToPercent(float pct)
        {
            if (this._videoPlayer == null)
                return;

            // Clamp pct to ensure it's between 0.0 and 1.0
            pct = Mathf.Clamp(pct, 0f, 1f);

            var frame = this._videoPlayer.frameCount * pct;
            this._videoPlayer.frame = (long)frame;

            // update length infos
            this._mediaLength = (1f * this._videoPlayer.frameCount / this._videoPlayer.playbackSpeed / this._videoPlayer.frameRate);
            this._currentRemainingLength = this._mediaLength - (1f * frame / this._videoPlayer.playbackSpeed / this._videoPlayer.frameRate);
            // offset remaining time
            if (this._currentRemainingLength <= 0)
                this._currentRemainingLength = 0.1f;
        }

        /// <summary>
        /// Set speed range is 0.01 to float.MaxValue
        /// <para>Generally we recommend these rates: 0.25, 0.5, 1.0, 1.25, 1.5, 1.75, 2.0</para>
        /// </summary>
        /// <param name="speed"></param>
        public void SetPlaySpeed(float speed)
        {
            if (this._videoPlayer == null)
                return;

            // Clamp speed to ensure it does not go below 0.1
            speed = Mathf.Clamp(speed, 0.01f, float.MaxValue);

            this._videoPlayer.playbackSpeed = this.playbackSpeed = speed;
            // use skip to percent to update current length and media total length
            this.SkipToPercent((this._mediaLength - this._currentRemainingLength) / this._mediaLength);
        }

        /// <summary>
        /// Get video play speed
        /// </summary>
        /// <returns></returns>
        public float GetPlaySpeed()
        {
            if (this._videoPlayer == null)
                return this.playbackSpeed;

            return this._videoPlayer.playbackSpeed;
        }

        public VideoPlayer GetVideoPlayer()
        {
            return this._videoPlayer;
        }

        private void OnDestroy()
        {
            if (Time.frameCount == 0 ||
                !Application.isPlaying)
                return;

            try
            {
                if (!this.isDestroying)
                    VideoManager.GetInstance().Stop(this, true, true);
            }
            catch
            {
                /* Nothing to do */
            }
        }
    }
}
