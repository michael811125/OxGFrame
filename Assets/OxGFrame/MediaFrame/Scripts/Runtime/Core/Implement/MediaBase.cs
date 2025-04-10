using Cysharp.Threading.Tasks;
using MyBox;
using OxGKit.LoggingSystem;
using OxGKit.SaverSystem;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Video;

namespace OxGFrame.MediaFrame
{
    [DisallowMultipleComponent]
    public abstract class MediaBase : MonoBehaviour
    {
        [Serializable]
        public class UrlSet
        {
            [Tooltip("Allow read URL_PATH from UrlCfg .txt file")]
            public bool getUrlPathFromCfg = true;
            [Tooltip("Get URL_PATH from UrlCfg .txt file"), ConditionalField(nameof(getUrlPathFromCfg))]
            public UrlCfg urlCfg = new UrlCfg();
            [Tooltip("If doesn't use [getUrlPathFromCfg] need to type Entire URL, ex: http://localhost/Audio/example.extension")]
            public string url = "";

            ~UrlSet()
            {
                this.urlCfg = null;
                this.url = null;
            }
        }

        [Serializable]
        public class UrlCfg
        {
            public enum RequestType
            {
                Assign,
                StreamingAssets
            }

            public RequestType requestType = RequestType.Assign;
            [Tooltip("Get URL_PATH from UrlCfg .txt file (Assign a file, This is not supports [WebGL])"), ConditionalField(nameof(requestType), false, RequestType.Assign)]
            public TextAsset file = null;
            [Tooltip("Get URL_PATH from UrlCfg .txt file (StreamingAssets Request)"), ConditionalField(nameof(requestType), false, RequestType.StreamingAssets)]
            public string fullPathName = MediaConfig.MEDIA_URL_CFG_NAME;

            #region Get UrlConfig File Content
            private static string _urlCfgContent = null;
            public async UniTask<string> GetFileText()
            {
                switch (this.requestType)
                {
                    case RequestType.Assign:
                        return this.file.text;

                    case RequestType.StreamingAssets:
                        string pathName = System.IO.Path.Combine(GetRequestStreamingAssetsPath(), this.fullPathName);
                        if (string.IsNullOrEmpty(_urlCfgContent))
                            _urlCfgContent = await OxGKit.Utilities.Requester.Requester.RequestText(pathName, null, null, null, false);
                        return _urlCfgContent;
                }

                return null;
            }
            #endregion

            ~UrlCfg()
            {
                this.file = null;
                this.fullPathName = null;
            }
        }

        /// <summary>
        /// 最大準備 Timeout Seconds
        /// </summary>
        internal const int MAX_PREPARE_TIME_SECONDS = 60;

        /// <summary>
        /// 影音名稱
        /// </summary>
        [HideInInspector]
        public string mediaName { get; protected set; } = string.Empty;

        /// <summary>
        /// (Bundle) AssetName = (Resouce) PathName
        /// </summary>
        [HideInInspector]
        public string assetName { get; protected set; } = string.Empty;

        /// <summary>
        /// If checked, it can be directly placed in the scene and driven by MonoBehaviour
        /// </summary>
        [Tooltip("If checked, it can be directly placed in the scene and driven by MonoBehaviour")]
        public bool monoDrive = false;

        /// <summary>
        /// 是否在 MonoDrive 模式下自動播放
        /// </summary>
        [Tooltip("Whether to autoplay in MonoDrive mode"), ConditionalField(nameof(monoDrive))]
        public bool autoPlay = true;

        /// <summary>
        /// 不受 TimeScale 影響
        /// </summary>
        [Tooltip("Not affected by TimeScale")]
        public bool ignoreTimeScale = true;

        /// <summary>
        /// 最大預備時間
        /// </summary>
        public int maxPrepareTimeSeconds = MAX_PREPARE_TIME_SECONDS;

        /// <summary>
        /// 循環次數
        /// </summary>
        [Tooltip("Repeat times 0 (equal to 1). But -1 = loop (Infinitely)")]
        public int loops = 0;

        /// <summary>
        /// 當前循環次數
        /// </summary>
        protected int _loops = 0;

        /// <summary>
        /// 是否結束時自動停止
        /// </summary>
        [Tooltip("When finished playing will auto to set stop (loops = -1 is invalid)")]
        public bool autoEndToStop = true;

        /// <summary>
        /// 是否停止時銷毀
        /// </summary>
        [Tooltip("when stop will destroy")]
        public bool onStopAndDestroy = true;

        /// <summary>
        /// 是否銷毀時卸載
        /// </summary>
        [Tooltip("when destroy will unload asset (not recommend sound type is SoundEffect)")]
        public bool onDestroyAndUnload = false;

        /// <summary>
        /// 是否暫停
        /// </summary>
        protected bool _isPaused = false;

        /// <summary>
        /// 是否播放中
        /// </summary>
        protected bool _isPlaying = false;

        /// <summary>
        /// 初始標記 (表示確認初始完畢)
        /// </summary>
        protected bool _isInit = false;

        /// <summary>
        /// 影音長度
        /// </summary>
        protected float _mediaLength = 0f;

        /// <summary>
        /// 影音當前長度
        /// </summary>
        protected float _currentRemainingLength = 0f;

        /// <summary>
        /// 停止播放時的事件調用
        /// </summary>
        protected Action _endEvent = null;

        /// <summary>
        /// 影音準備好的標記
        /// </summary>
        public bool isPrepared { get; protected set; } = false;

        /// <summary>
        /// 正在被銷毀的標記
        /// </summary>
        internal bool isDestroying = false;

        private async UniTaskVoid Awake()
        {
            if (this.monoDrive)
            {
                string name = $"{nameof(this.monoDrive)}_{this.name}";
                this.SetNames(name, name);
                bool isInitialized = await this.Init();
                if (!isInitialized)
                {
                    Destroy(this.gameObject);
                }
            }
        }

        private async UniTaskVoid Start()
        {
            if (this.monoDrive)
            {
                if (this.autoPlay)
                {
                    var cts = new CancellationTokenSource();
                    cts.CancelAfterSlim(TimeSpan.FromSeconds(this.maxPrepareTimeSeconds <= 0 ? MAX_PREPARE_TIME_SECONDS : this.maxPrepareTimeSeconds));
                    try
                    {
                        do
                        {
                            if (this.isPrepared)
                                break;
                            // load balancing
                            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cts.Token);
                        } while (true);
                    }
                    catch (OperationCanceledException ex)
                    {
                        Logging.PrintException<Logger>(ex);
                        Destroy(this.gameObject);
                    }

                    // Wait until is prepared to play
                    this.Play(0, 0);
                }
            }
        }

        private void OnDestroy()
        {
            this.DetectOnDestroy();
        }

        internal void HandleFixedUpdate(float dt)
        {
            if (!this._isInit)
                return;
            this.OnFixedUpdate(dt);
        }

        #region For MonoDrive
        /// <summary>
        /// Drive by self MonoBehaviour FixedUpdate
        /// </summary>
        /// <param name="dt"></param>
        protected void DriveSelfFixedUpdate(float dt) => this.HandleFixedUpdate(dt);

        /// <summary>
        /// Drive by other MonoBehaviour Update
        /// </summary>
        /// <param name="dt"></param>
        public void DriveFixedUpdate(float dt) => this.HandleFixedUpdate(dt);

        /// <summary>
        /// 檢測銷毀程序
        /// </summary>
        internal protected virtual void DetectOnDestroy() { }
        #endregion

        /// <summary>
        /// 初始用
        /// </summary>
        internal abstract UniTask<bool> Init();

        /// <summary>
        /// 固定每幀被調用
        /// </summary>
        /// <param name="dt"></param>
        protected abstract void OnFixedUpdate(float dt = 0f);

        /// <summary>
        /// 開始播放
        /// </summary>
        public abstract void Play(int loops, float volume);

        /// <summary>
        /// 停止播放
        /// </summary>
        public abstract void Stop();

        /// <summary>
        /// 暫停播放
        /// </summary>
        public abstract void Pause();

        /// <summary>
        /// 返回是否播放中
        /// </summary>
        public abstract bool IsPlaying();

        /// <summary>
        /// 返回是否暫停
        /// </summary>
        /// <returns></returns>
        public abstract bool IsPaused();

        /// <summary>
        /// 返回是否循環
        /// </summary>
        /// <returns></returns>
        public abstract bool IsLooping();

        /// <summary>
        /// 處理循環程序
        /// </summary>
        internal protected void ProcessLooping(UnityEngine.Object component, float dt)
        {
            if (this.CurrentRemainingLength() > 0f)
            {
                this._currentRemainingLength -= dt;
                if (this.CurrentRemainingLength() <= 0f)
                {
                    if (this._loops >= 0)
                    {
                        switch (component)
                        {
                            case AudioSource:
                                (component as AudioSource).Stop();
                                break;
                            case VideoPlayer:
                                (component as VideoPlayer).Stop();
                                break;
                        }

                        this._loops--;
                        if (this._loops <= 0)
                        {
                            this._currentRemainingLength = 0;
                            if (this.autoEndToStop)
                                this.StopSelf();
                        }
                        else
                        {
                            switch (component)
                            {
                                case AudioSource:
                                    (component as AudioSource).Play();
                                    break;
                                case VideoPlayer:
                                    (component as VideoPlayer).Play();
                                    break;
                            }
                        }
                    }
                    this._currentRemainingLength = this.Length();
                }
            }
        }

        /// <summary>
        /// 取得影音長度 (單位: 秒)
        /// </summary>
        /// <returns></returns>
        public abstract float Length();

        /// <summary>
        /// 取得當前影音長度 (單位: 秒)
        /// </summary>
        /// <returns></returns>
        public abstract float CurrentLength();

        /// <summary>
        /// 取得當前影音剩餘長度 -> Remaining time (單位: 秒)
        /// </summary>
        /// <returns></returns>
        public abstract float CurrentRemainingLength();

        /// <summary>
        /// 設置停止播放時的事件 Callback
        /// </summary>
        /// <param name="endEvent"></param>
        public virtual void SetEndEvent(Action endEvent)
        {
            this._endEvent = endEvent;
        }

        /// <summary>
        /// Destroy 時會被呼叫
        /// </summary>
        public virtual void OnRelease()
        {
            this._endEvent = null;
            this._mediaLength = 0f;
            this._currentRemainingLength = 0f;
        }

        /// <summary>
        /// 停止播放自己
        /// </summary>
        protected abstract void StopSelf();

        /// <summary>
        /// 重置秒數
        /// </summary>
        protected virtual void ResetLength()
        {
            this._currentRemainingLength = this._mediaLength;
        }

        /// <summary>
        /// 重置循環次數
        /// </summary>
        protected virtual void ResetLoops()
        {
            this._loops = this.loops;
        }

        /// <summary>
        /// 設置名稱
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="mediaName"></param>
        internal void SetNames(string assetName, string mediaName)
        {
            this.assetName = assetName;
            this.mediaName = mediaName;
        }

        #region MEDIA_URL 請求
        public const string VIDEO_URLSET = "video_urlset";
        public const string AUDIO_URLSET = "audio_urlset";
        internal static string GetValueFromUrlCfg(string urlCfg, string key)
        {
            if (string.IsNullOrEmpty(urlCfg))
                return string.Empty;

            var content = urlCfg;
            var dataMap = Saver.ParsingDataMap(content);
            dataMap.TryGetValue(key, out string value);
            return value;
        }

        /// <summary>
        /// 取得 UnityWebRequest StreamingAssets 路徑 (OSX and iOS 需要 + file://)
        /// </summary>
        /// <returns></returns>
        internal static string GetRequestStreamingAssetsPath()
        {
#if UNITY_STANDALONE_OSX || UNITY_IOS
            return $"file://{Application.streamingAssetsPath}";
#else
            return Application.streamingAssetsPath;
#endif
        }
        #endregion
    }
}
