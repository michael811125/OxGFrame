using Cysharp.Threading.Tasks;
using OxGKit.LoggingSystem;
using UnityEngine;

namespace OxGFrame.MediaFrame.VideoFrame
{
    internal class VideoManager : MediaManager<VideoBase>
    {
        private static readonly object _locker = new object();
        private static VideoManager _instance = null;
        public static VideoManager GetInstance()
        {
            if (_instance == null)
            {
                lock (_locker)
                {
                    _instance = FindObjectOfType<VideoManager>();
                    if (_instance == null)
                        _instance = new GameObject(nameof(VideoManager)).AddComponent<VideoManager>();
                }
            }
            return _instance;
        }

        private void Awake()
        {
            string newName = $"[{nameof(VideoManager)}]";
            this.gameObject.name = newName;
            if (this.gameObject.transform.root.name == newName)
            {
                var container = GameObject.Find(nameof(OxGFrame));
                if (container == null)
                    container = new GameObject(nameof(OxGFrame));
                this.gameObject.transform.SetParent(container.transform);
                DontDestroyOnLoad(container);
            }
            else
                DontDestroyOnLoad(this.gameObject.transform.root);
        }

        protected override void SetParent(VideoBase vidBase, Transform parent)
        {
            if (parent != null)
                vidBase.gameObject.transform.SetParent(parent);
            else
                vidBase.gameObject.transform.SetParent(this.gameObject.transform);
        }

        #region 播放 Play
        /// <summary>
        /// 統一調用 Play 的私有封裝
        /// </summary>
        /// <param name="vidBase"></param>
        /// <param name="loops"></param>
        /// <returns></returns>
        private void _Play(VideoBase vidBase, int loops, float volume)
        {
            if (vidBase == null)
                return;

            // 處理長期沒有被 Unload 的 Video
            if (!vidBase.onDestroyAndUnload)
                this.TryLRUCache<VideoBase>(vidBase.assetName);

            this.LoadAndPlay(vidBase, loops, volume);

            Logging.Print<Logger>($"Play Video: {vidBase?.mediaName}");
        }

        /// <summary>
        /// 播放
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="parent"></param>
        /// <param name="loops"></param>
        /// <returns></returns>
        public override async UniTask<VideoBase[]> Play(string packageName, string assetName, UnityEngine.Object sourceClip, Transform parent = null, int loops = 0, float volume = 0f)
        {
            if (string.IsNullOrEmpty(assetName))
                return new VideoBase[] { null };

            VideoBase[] vidBases = this.GetMediaComponents<VideoBase>(assetName);
            bool isResume = false;
            if (vidBases.Length > 0)
            {
                VideoBase main = vidBases[0];
                if (main.IsPlaying())
                {
                    Logging.PrintWarning<Logger>($"【Video】{assetName} has already been played!!!");
                    return vidBases;
                }

                if (!main.IsPlaying() ||
                    main.IsPaused())
                    isResume = true;
            }

            if (!isResume)
            {
                GameObject go = await this.LoadAssetIntoCache(packageName, assetName);
                Transform spawnParent = null;
                if (parent == null) spawnParent = this.gameObject.transform;
                VideoBase vidBase = await this.CloneAsset<VideoBase>(assetName, go, sourceClip, parent, spawnParent);
                if (vidBase == null)
                {
                    Logging.PrintError<Logger>($"Video -> No matching component found at path or name: {assetName}");
                    return new VideoBase[] { null };
                }

                this._Play(vidBase, loops, volume);

                return new VideoBase[] { vidBase };
            }
            else
            {
                for (int i = 0; i < vidBases.Length; i++)
                {
                    if (!vidBases[i].IsPlaying())
                        this._Play(vidBases[i], 0, 0f);
                }

                return vidBases;
            }
        }

        /// <summary>
        /// 全部恢復播放
        /// </summary>
        /// <returns></returns>
        public override void ResumeAll()
        {
            if (this._listAllCache.Count == 0)
                return;

            foreach (var vidBase in this._listAllCache)
            {
                if (vidBase.IsPaused())
                    this._Play(vidBase, 0, 0f);
            }
        }
        #endregion

        #region 停止 Stop
        /// <summary>
        /// 統一調用 Stop 的私有封裝
        /// </summary>
        /// <param name="vidBase"></param>
        /// <param name="forceDestroy"></param>
        private void _Stop(VideoBase vidBase, bool disabledEndEvent = false, bool forceDestroy = false)
        {
            if (vidBase == null)
                return;

            this.ExitAndStop(vidBase, false, disabledEndEvent);

            Logging.Print<Logger>($"Stop Video: {vidBase?.mediaName}");

            // 確保音訊都設置完畢後才進行 Destroy, 避免異步處理尚未完成, 就被 Destroy 掉導致操作到已銷毀物件
            if (vidBase.isPrepared)
            {
                if (forceDestroy)
                    this.Destroy(vidBase);
                else if (vidBase.onStopAndDestroy)
                    this.Destroy(vidBase);
            }
        }

        /// <summary>
        /// 停止
        /// </summary>
        /// <param name="vidBase"></param>
        /// <param name="forceDestroy"></param>
        public void Stop(VideoBase vidBase, bool disabledEndEvent = false, bool forceDestroy = false)
        {
            this._Stop(vidBase, disabledEndEvent, forceDestroy);
        }

        /// <summary>
        /// 停止
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="forceDestroy"></param>
        public override void Stop(string assetName, bool disabledEndEvent = false, bool forceDestroy = false)
        {
            VideoBase[] vidBases = this.GetMediaComponents<VideoBase>(assetName);
            if (vidBases.Length == 0)
                return;

            foreach (var vidBase in vidBases)
            {
                this._Stop(vidBase, disabledEndEvent, forceDestroy);
            }
        }

        /// <summary>
        /// 全部停止
        /// </summary>
        /// <param name="forceDestroy"></param>
        /// <returns></returns>
        public override void StopAll(bool disabledEndEvent = false, bool forceDestroy = false)
        {
            if (this._listAllCache.Count == 0)
                return;

            foreach (var vidBase in this._listAllCache.ToArray())
            {
                this._Stop(vidBase, disabledEndEvent, forceDestroy);
            }
        }
        #endregion

        #region 暫停 Puase
        /// <summary>
        /// 統一調用 Pause 的私有封裝
        /// </summary>
        /// <param name="vidBase"></param>
        private void _Pause(VideoBase vidBase)
        {
            if (vidBase == null)
                return;

            this.ExitAndStop(vidBase, true, false);

            Logging.Print<Logger>($"Pause Video: {vidBase?.mediaName}, Current Length: {vidBase?.CurrentLength()} (s)");
        }

        /// <summary>
        /// 暫停
        /// </summary>
        /// <param name="assetName"></param>
        public override void Pause(string assetName)
        {
            VideoBase[] vidBases = this.GetMediaComponents<VideoBase>(assetName);
            if (vidBases.Length == 0)
                return;

            foreach (var vidBase in vidBases)
            {
                this._Pause(vidBase);
            }
        }

        /// <summary>
        /// 全部暫停
        /// </summary>
        /// <returns></returns>
        public override void PauseAll()
        {
            if (this._listAllCache.Count == 0)
                return;

            foreach (var vidBase in this._listAllCache)
            {
                this._Pause(vidBase);
            }
        }
        #endregion

        protected override void LoadAndPlay(VideoBase vidBase, int loops, float volume)
        {
            if (vidBase == null)
                return;
            vidBase.Play(loops, volume);
        }

        protected override void ExitAndStop(VideoBase vidBase, bool pause, bool disabledEndEvent)
        {
            if (!pause)
            {
                if (disabledEndEvent)
                    vidBase.SetEndEvent(null);
                vidBase.Stop();
            }
            else
                vidBase.Pause();
        }
    }
}
