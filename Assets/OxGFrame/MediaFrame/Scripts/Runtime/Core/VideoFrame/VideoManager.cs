using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace MediaFrame.VideoFrame
{
    public class VideoManager : MediaManager<VideoBase>
    {
        private GameObject _goRoot = null;                                // 根節點物件

        private static readonly object _locker = new object();
        private static VideoManager _instance = null;
        public static VideoManager GetInstance()
        {
            if (_instance == null)
            {
                lock (_locker)
                {
                    _instance = FindObjectOfType<VideoManager>();
                    if (_instance == null) _instance = new GameObject(VideoSysDefine.VIDEO_MANAGER_NAME).AddComponent<VideoManager>();
                }
            }
            return _instance;
        }

        private void Awake()
        {
            DontDestroyOnLoad(this);

            this._goRoot = GameObject.Find(VideoSysDefine.VIDEO_MANAGER_NAME);
            if (this._goRoot == null) return;
        }

        protected override void SetParent(VideoBase vidBase)
        {
            vidBase.gameObject.transform.SetParent(this._goRoot.transform);
        }

        #region 播放 Play
        /// <summary>
        /// 統一調用 Play 的私有封裝
        /// </summary>
        /// <param name="vidBase"></param>
        /// <param name="loops"></param>
        /// <returns></returns>
        private void _Play(VideoBase vidBase, int loops)
        {
            if (vidBase == null) return;

            this.LoadAndPlay(vidBase, loops);

            Debug.Log(string.Format("播放Video: {0}", vidBase?.mediaName));
        }

        /// <summary>
        /// 播放
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="loops"></param>
        /// <returns></returns>
        public override async UniTask<VideoBase[]> Play(string assetName, int loops = 0)
        {
            if (string.IsNullOrEmpty(assetName)) return new VideoBase[] { };

            VideoBase[] vidBases = this.GetMediaComponents<VideoBase>(assetName);
            bool isResume = false;
            if (vidBases.Length > 0)
            {
                VideoBase main = vidBases[0];
                if (main.IsPlaying())
                {
                    Debug.LogWarning(string.Format("【Video】{0} 已經播放了!!!", assetName));
                    return vidBases;
                }

                if (!main.IsPlaying() || main.IsPaused()) isResume = true;
            }

            if (!isResume)
            {
                GameObject go = await this.LoadAssetIntoCache(assetName);
                VideoBase vidBase = await this.CloneAsset<VideoBase>(string.Empty, assetName, go, this._goRoot.transform);
                if (vidBase == null)
                {
                    Debug.LogWarning(string.Format("找不到相對路徑資源【Video】: {0}", assetName));
                    return new VideoBase[] { };
                }

                this._Play(vidBase, loops);

                return new VideoBase[] { vidBase };
            }
            else
            {
                for (int i = 0; i < vidBases.Length; i++)
                {
                    if (!vidBases[i].IsPlaying()) this._Play(vidBases[i], 0);
                }

                return vidBases;
            }
        }
        public override async UniTask<VideoBase[]> Play(string bundleName, string assetName, int loops = 0)
        {
            if (string.IsNullOrEmpty(bundleName) && string.IsNullOrEmpty(assetName)) return new VideoBase[] { };

            VideoBase[] vidBases = this.GetMediaComponents<VideoBase>(assetName);
            bool isResume = false;
            if (vidBases.Length > 0)
            {
                VideoBase main = vidBases[0];
                if (main.IsPlaying())
                {
                    Debug.LogWarning(string.Format("【Video】{0} 已經播放了!!!", assetName));
                    return vidBases;
                }

                if (!main.IsPlaying() || main.IsPaused()) isResume = true;
            }

            if (!isResume)
            {
                GameObject go = await this.LoadAssetIntoCache(bundleName, assetName);
                VideoBase vidBase = await this.CloneAsset<VideoBase>(bundleName, assetName, go, this._goRoot.transform);
                if (vidBase == null)
                {
                    Debug.LogWarning(string.Format("找不到相對路徑資源【Video】: {0}", assetName));
                    return new VideoBase[] { };
                }

                this._Play(vidBase, loops);

                return new VideoBase[] { vidBase };
            }
            else
            {
                for (int i = 0; i < vidBases.Length; i++)
                {
                    if (!vidBases[i].IsPlaying()) this._Play(vidBases[i], 0);
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
            if (this._listAllCache.Count == 0) return;

            foreach (var vidBase in this._listAllCache.ToArray())
            {
                if (!vidBase.IsPlaying()) this._Play(vidBase, 0);
            }
        }
        #endregion

        #region 停止 Stop
        /// <summary>
        /// 統一調用 Stop 的私有封裝
        /// </summary>
        /// <param name="vidBase"></param>
        /// <param name="withDestroy"></param>
        private void _Stop(VideoBase vidBase, bool disableEndEvent = false, bool withDestroy = false)
        {
            if (vidBase == null) return;

            this.ExitAndStop(vidBase, false, disableEndEvent);

            Debug.Log(string.Format("停止Video: {0}", vidBase?.mediaName));

            // 確保音訊都設置完畢後才進行Destroy, 避免異步處理尚未完成, 就被Destroy掉導致操作到已銷毀物件
            if (vidBase.isPrepared)
            {
                if (withDestroy) this.Destroy(vidBase, vidBase.mediaName);
                else if (vidBase.onStopAndDestroy) this.Destroy(vidBase, vidBase.mediaName);
            }
        }

        /// <summary>
        /// 停止
        /// </summary>
        /// <param name="vidBase"></param>
        /// <param name="withDestroy"></param>
        public void Stop(VideoBase vidBase, bool disableEndEvent = false, bool withDestroy = false)
        {
            this._Stop(vidBase, disableEndEvent, withDestroy);
        }

        /// <summary>
        /// 停止
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="withDestroy"></param>
        public override void Stop(string assetName, bool disableEndEvent = false, bool withDestroy = false)
        {
            VideoBase[] vidBases = this.GetMediaComponents<VideoBase>(assetName);
            if (vidBases.Length == 0) return;

            foreach (var vidBase in vidBases)
            {
                this._Stop(vidBase, disableEndEvent, withDestroy);
            }
        }

        /// <summary>
        /// 全部停止
        /// </summary>
        /// <param name="withDestroy"></param>
        /// <returns></returns>
        public override void StopAll(bool disableEndEvent = false, bool withDestroy = false)
        {
            if (this._listAllCache.Count == 0) return;

            foreach (var vidBase in this._listAllCache.ToArray())
            {
                this._Stop(vidBase, disableEndEvent, withDestroy);
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
            if (vidBase == null) return;

            this.ExitAndStop(vidBase, true, false);

            Debug.Log(string.Format("暫停Video: {0}, 當前長度: {1} 秒", vidBase?.mediaName, vidBase?.CurrentLength()));
        }

        /// <summary>
        /// 暫停
        /// </summary>
        /// <param name="assetName"></param>
        public override void Pause(string assetName)
        {
            VideoBase[] vidBases = this.GetMediaComponents<VideoBase>(assetName);
            if (vidBases.Length == 0) return;

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
            if (this._listAllCache.Count == 0) return;

            foreach (var vidBase in this._listAllCache.ToArray())
            {
                this._Pause(vidBase);
            }
        }
        #endregion

        protected override void LoadAndPlay(VideoBase vidBase, int loops)
        {
            if (vidBase == null) return;
            vidBase.Play(loops);
        }

        protected override void ExitAndStop(VideoBase vidBase, bool pause, bool disableEndEvent)
        {
            if (!pause)
            {
                if (disableEndEvent) vidBase.SetEndEvent(null);
                vidBase.Stop();
            }
            else vidBase.Pause();
        }
    }
}
