using Cysharp.Threading.Tasks;
using MyBox;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace OxGFrame.MediaFrame
{
    [DisallowMultipleComponent]
    public abstract class MediaBase : MonoBehaviour, IMediaBase
    {
        [Serializable]
        public class UrlSet
        {
            [SerializeField, Tooltip("Allow read URL_PATH from UrlCfg .txt file")]
            public bool getUrlPathFromCfg = true;
            [SerializeField, Tooltip("Get URL_PATH from UrlCfg .txt file"), ConditionalField(nameof(getUrlPathFromCfg))]
            public UrlCfg urlCfg = new UrlCfg();
            [SerializeField, Tooltip("If doesn't use [getUrlPathFromCfg] need to type Entire URL, ex: http://localhost/Audio/example.extension")]
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

            [SerializeField]
            public RequestType requestType = RequestType.Assign;
            [SerializeField, Tooltip("Get URL_PATH from UrlCfg .txt file (Assign a file, This is not supports [WebGL])"), ConditionalField(nameof(requestType), false, RequestType.Assign)]
            public TextAsset file = null;
            [SerializeField, Tooltip("Get URL_PATH from UrlCfg .txt file (StreamingAssets Request)"), ConditionalField(nameof(requestType), false, RequestType.StreamingAssets)]
            public string fullPathName = string.Empty;

            public async UniTask<string> GetFileText()
            {
                switch (this.requestType)
                {
                    case RequestType.Assign:
                        return this.file.text;

                    case RequestType.StreamingAssets:
                        string pathName = System.IO.Path.Combine(Application.streamingAssetsPath, this.fullPathName);
                        return await TextRequest(pathName);
                }

                return null;
            }

            ~UrlCfg()
            {
                this.file = null;
                this.fullPathName = null;
            }
        }

        [HideInInspector] public string mediaName { get; protected set; } = string.Empty;  // 影音名稱
        [HideInInspector] public string bundleName { get; protected set; } = string.Empty; // BundleName
        [HideInInspector] public string assetName { get; protected set; } = string.Empty;  // (Bundle) AssetName = (Resouce) PathName
        [SerializeField, Tooltip("Not affected by TimeScale")]
        protected bool _ignoreTimeScale = true;                                            // 不受TimeScale影響
        [Tooltip("Repeat times 0 (equal to 1). But -1 = loop (Infinitely)")]
        public int loops = 0;                                                              // 循環次數
        protected int _loops = 0;                                                          // 當前循環次數
        [Tooltip("when stop will destroy")]
        public bool onStopAndDestroy = true;                                               // 是否停止時銷毀
        [Tooltip("when destroy will unload asset (not recommend sound type is SoundEffect)")]
        public bool onDestroyAndUnload = false;
        [SerializeField, Tooltip("When finished playing will auto to set stop (loops = -1 is invalid)")]
        protected bool _autoEndToStop = true;                                              // 是否結束時自動停止
        protected bool _isPaused = false;                                                  // 是否暫停
        protected bool _isInit = false;                                                    // 初始標記 (表示確認初始完畢)
        protected float _mediaLength = 0f;                                                 // 影音長度
        protected float _currentLength = 0f;                                               // 影音當前長度
        protected Action _endEvent = null;                                                 // 停止播放時的事件調用
        public bool isPrepared { get; protected set; } = false;                            // 影音準備好的標記

        private void FixedUpdate()
        {
            if (!this._isInit) return;

            if (!this._ignoreTimeScale) this.OnFixedUpdate(Time.fixedDeltaTime);
            else this.OnFixedUpdate(Time.fixedUnscaledDeltaTime);
        }

        #region IMediaBase
        /// <summary>
        /// 初始用
        /// </summary>
        public abstract UniTask Init();

        /// <summary>
        /// 固定每幀被調用
        /// </summary>
        /// <param name="dt"></param>
        protected abstract void OnFixedUpdate(float dt = 0f);

        /// <summary>
        /// 開始播放
        /// </summary>
        public abstract void Play(int loops);

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
        /// 設置停止播放時的事件Callback
        /// </summary>
        /// <param name="endEvent"></param>
        public virtual void SetEndEvent(Action endEvent)
        {
            this._endEvent = endEvent;
        }

        /// <summary>
        /// Destroy時會被呼叫
        /// </summary>
        public virtual void OnRelease()
        {
            this.mediaName = null;
            this.bundleName = null;
            this.assetName = null;
            this._mediaLength = 0f;
            this._currentLength = 0f;
            this._endEvent = null;
        }
        #endregion

        /// <summary>
        /// 停止播放自己
        /// </summary>
        protected abstract void StopSelf();

        /// <summary>
        /// 重置秒數
        /// </summary>
        protected virtual void ResetLength()
        {
            this._currentLength = this._mediaLength;
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
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        public void SetNames(string bundleName, string assetName, string mediaName)
        {
            this.bundleName = bundleName;
            this.assetName = assetName;
            this.mediaName = mediaName;
        }

        #region MEDIA_URL請求
        public const string VIDEO_URLSET = "video_urlset";
        public const string AUDIO_URLSET = "audio_urlset";
        public string GetValueFromUrlCfg(string urlCfg, string key)
        {
            if (urlCfg == null) return string.Empty;

            var content = urlCfg;
            var allWords = content.Split('\n');
            var lines = new List<string>(allWords);
            Dictionary<string, string> fileMap = new Dictionary<string, string>();
            foreach (var readLine in lines)
            {
                Debug.Log($"readline: {readLine}");
                if (readLine.IndexOf('#') != -1 && readLine[0] == '#') continue;
                var args = readLine.Split(' ');
                if (args.Length >= 2)
                {
                    Debug.Log($"args => key: {args[0]}, value: {args[1]}");
                    if (!fileMap.ContainsKey(args[0])) fileMap.Add(args[0], args[1].Replace("\n", "").Replace("\r", ""));
                }
            }

            fileMap.TryGetValue(key, out string value);
            return value;
        }

        public static async UniTask<string> TextRequest(string url)
        {
            try
            {
                var request = UnityWebRequest.Get(url);
                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log($"<color=#FF0000>Request failed, URL: {url}</color>");
                    request.Dispose();

                    return null;
                }

                string txt = request.downloadHandler.text;
                request.Dispose();

                return txt;
            }
            catch
            {
                Debug.Log($"<color=#FF0000>Request failed, URL: {url}</color>");
                return null;
            }
        }
        #endregion
    }
}
