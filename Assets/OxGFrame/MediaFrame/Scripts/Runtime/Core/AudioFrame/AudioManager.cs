using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;

namespace OxGFrame.MediaFrame.AudioFrame
{
    public class AudioManager : MediaManager<AudioBase>
    {
        [Header("Audio Mixer")]
        [SerializeField, Tooltip("Setup AudioMixer in list")]
        private List<AudioMixer> _listMixer = new List<AudioMixer>();                            // 中控混音器
                                                                                                 // 
        private Dictionary<string, float> _dictMixerExpParams = new Dictionary<string, float>(); // 用於記錄Exposed Parameters參數

        private GameObject _goRoot = null;                                                       // 根節點物件
        private Dictionary<string, GameObject> _goNodes = new Dictionary<string, GameObject>();  // 節點物件

        private static readonly object _locker = new object();
        private static AudioManager _instance = null;
        public static AudioManager GetInstance()
        {
            if (_instance == null)
            {
                lock (_locker)
                {
                    _instance = FindObjectOfType<AudioManager>();
                    if (_instance == null) Debug.LogWarning("<color=#FF0000>Connot found 【AudioManager Component】, Please to check your 【AudioManager GameObject】.</color>");
                }
            }
            return _instance;
        }

        private void Awake()
        {
            DontDestroyOnLoad(this);

            this._goRoot = GameObject.Find(AudioSysDefine.AUDIO_MANAGER_NAME);
            if (this._goRoot == null) return;

            foreach (var nodeName in Enum.GetNames(typeof(SoundType)))
            {
                if (!this._goNodes.ContainsKey(nodeName))
                {
                    this._goNodes.Add(nodeName, this.CreateNode(nodeName, this._goRoot.transform));
                }
            }
        }

        protected override void SetParent(AudioBase audBase)
        {
            if (this._goNodes.TryGetValue(audBase.audioType.soundType.ToString(), out GameObject goNode))
            {
                audBase.gameObject.transform.SetParent(goNode.transform);
            }
        }

        #region 中控 Mixer
        /// <summary>
        /// 依照Mixer的名稱與其中的ExposedParam合併成雙key, 執行自動記錄
        /// </summary>
        /// <param name="mixerName"></param>
        /// <param name="expParam"></param>
        /// <param name="val"></param>
        private void _AutoRecordExposedParam(string mixerName, string expParam, float val)
        {
            string key = $"{mixerName},{expParam}"; // 以 , 號隔開 (之後Replace也需要+上 , 號)
            if (!this._dictMixerExpParams.ContainsKey(key))
            {
                this._dictMixerExpParams.Add(key, val);
            }
            else this._dictMixerExpParams[key] = val;
        }

        /// <summary>
        /// 設置指定該Mixer中的Exposed參數
        /// </summary>
        /// <param name="mixer"></param>
        /// <param name="expParam"></param>
        /// <param name="val"></param>
        public void SetMixerExposedParam(AudioMixer mixer, string expParam, float val)
        {
            if (mixer.SetFloat(expParam, val))
            {
                this._AutoRecordExposedParam(mixer.name, expParam, val);
            }
        }

        /// <summary>
        /// 清除指定該Mixer中的Exposed參數
        /// </summary>
        /// <param name="mixer"></param>
        /// <param name="expParam"></param>
        public void ClearMixerExposedParam(AudioMixer mixer, string expParam)
        {
            mixer.ClearFloat(expParam);
        }

        /// <summary>
        /// 自動清除該Mixer的Exposed參數
        /// </summary>
        /// <param name="mixer"></param>
        public void AutoClearMixerExposedParams(AudioMixer mixer)
        {
            if (this._dictMixerExpParams.Count == 0) return;

            foreach (var key in this._dictMixerExpParams.Keys.ToArray())
            {
                string reKey = key.Replace($"{mixer.name},", string.Empty);
                mixer.ClearFloat(reKey);
            }
        }

        /// <summary>
        /// 自動復原該Mixer的Exposed參數
        /// </summary>
        /// <param name="mixer"></param>
        public void AutoRestoreMixerExposedParams(AudioMixer mixer)
        {
            if (this._dictMixerExpParams.Count == 0) return;

            foreach (var expParam in this._dictMixerExpParams.ToArray())
            {
                string reKey = expParam.Key.Replace($"{mixer.name},", string.Empty);
                mixer.SetFloat(reKey, expParam.Value);
            }
        }

        /// <summary>
        /// 設置切換Mixer的Snapshot
        /// </summary>
        /// <param name="mixer"></param>
        /// <param name="snapshotName"></param>
        public void SetMixerSnapshot(AudioMixer mixer, string snapshotName)
        {
            var snapshot = mixer.FindSnapshot(snapshotName);
            if (snapshot == null) return;

            snapshot.TransitionTo(0.02f);
        }

        /// <summary>
        /// 設置Mixer與Snapshot的混和過度
        /// </summary>
        /// <param name="mixer"></param>
        /// <param name="snapshots"></param>
        /// <param name="weights"></param>
        /// <param name="timeToReach"></param>
        public void SetMixerTransitionToSnapshot(AudioMixer mixer, AudioMixerSnapshot[] snapshots, float[] weights, float timeToReach = 0.02f)
        {
            mixer.TransitionToSnapshots(snapshots, weights, timeToReach);

            string log = string.Empty;
            for (int i = 0; i < snapshots.Length; i++)
            {
                log += $"<color=#FFA8AF>{snapshots[i].name} : {weights[i]}</color>;  ";
            }
            Debug.Log($"{log}");
        }

        /// <summary>
        /// 取得該Mixer中的Snapshot
        /// </summary>
        /// <param name="mixer"></param>
        /// <param name="snapshotName"></param>
        /// <returns></returns>
        public AudioMixerSnapshot GetMixerSnapshot(AudioMixer mixer, string snapshotName)
        {
            var snapshot = mixer.FindSnapshot(snapshotName);
            return snapshot;
        }

        /// <summary>
        /// 從List中透過Mixer的名稱返回該對應的Mixer
        /// </summary>
        /// <param name="mixerName"></param>
        /// <returns></returns>
        public AudioMixer GetMixerByName(string mixerName)
        {
            if (string.IsNullOrEmpty(mixerName) || this._listMixer.Count == 0) return null;

            foreach (var mixer in this._listMixer)
            {
                if (mixer.name == mixerName) return mixer;
            }

            return null;
        }
        #endregion

        #region 播放 Play
        /// <summary>
        /// 統一調用 Play 的私有封裝
        /// </summary>
        /// <param name="audBase"></param>
        /// <param name="loops"></param>
        /// <returns></returns>
        private void _Play(AudioBase audBase, int loops)
        {
            if (audBase == null) return;

            this.LoadAndPlay(audBase, loops);

            Debug.Log(string.Format("Play Audio: {0}, Current Length: {1} (s)", audBase?.mediaName, audBase?.CurrentLength()));
        }

        /// <summary>
        /// 播放
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="loops"></param>
        /// <returns></returns>
        public override async UniTask<AudioBase[]> Play(string assetName, int loops = 0)
        {
            if (string.IsNullOrEmpty(assetName)) return new AudioBase[] { };

            AudioBase[] audBases = this.GetMediaComponents<AudioBase>(assetName);
            bool isResume = false;
            if (audBases.Length > 0)
            {
                AudioBase main = audBases[0];
                if (main.IsPlaying())
                {
                    switch (main.audioType.soundType)
                    {
                        case SoundType.Sole:
                            Debug.LogWarning(string.Format("【Audio => SoundType: {0}】{1} already played!!!", main.audioType.soundType, assetName));
                            return audBases;
                    }
                }

                if (!main.IsPlaying() || main.IsPaused()) isResume = true;
            }

            if (!isResume)
            {
                GameObject go = await this.LoadAssetIntoCache(assetName);
                AudioBase audBase = await this.CloneAsset<AudioBase>(string.Empty, assetName, go, this._goRoot.transform);
                if (audBase == null)
                {
                    Debug.LogWarning(string.Format("Asset not found at this path!!!【Audio】: {0}", assetName));
                    return new AudioBase[] { };
                }

                this._Play(audBase, loops);

                return new AudioBase[] { audBase };
            }
            else
            {
                for (int i = 0; i < audBases.Length; i++)
                {
                    if (!audBases[i].IsPlaying()) this._Play(audBases[i], 0);
                }

                return audBases;
            }
        }
        public override async UniTask<AudioBase[]> Play(string bundleName, string assetName, int loops = 0)
        {
            if (string.IsNullOrEmpty(bundleName) && string.IsNullOrEmpty(assetName)) return new AudioBase[] { };

            AudioBase[] audBases = this.GetMediaComponents<AudioBase>(assetName);
            bool isResume = false;
            if (audBases.Length > 0)
            {
                AudioBase main = audBases[0];
                if (main.IsPlaying())
                {
                    switch (main.audioType.soundType)
                    {
                        case SoundType.Sole:
                            Debug.LogWarning(string.Format("【Audio => SoundType: {0}】{1} already played!!!", main.audioType.soundType, assetName));
                            return audBases;
                    }
                }

                if (!main.IsPlaying() || main.IsPaused()) isResume = true;
            }

            if (!isResume)
            {
                GameObject go = await this.LoadAssetIntoCache(bundleName, assetName);
                AudioBase audBase = await this.CloneAsset<AudioBase>(bundleName, assetName, go, this._goRoot.transform);
                if (audBase == null)
                {
                    Debug.LogWarning(string.Format("Asset not found at this path!!!【Audio】: {0}", assetName));
                    return new AudioBase[] { };
                }

                this._Play(audBase, loops);

                return new AudioBase[] { audBase };
            }
            else
            {
                for (int i = 0; i < audBases.Length; i++)
                {
                    if (!audBases[i].IsPlaying()) this._Play(audBases[i], 0);
                }

                return audBases;
            }
        }

        /// <summary>
        /// 全部恢復播放
        /// </summary>
        /// <returns></returns>
        public override void ResumeAll()
        {
            if (this._listAllCache.Count == 0) return;

            foreach (var audBase in this._listAllCache.ToArray())
            {
                if (audBase.IsPaused()) this._Play(audBase, 0);
            }
        }
        #endregion

        #region 停止 Stop
        /// <summary>
        /// 統一調用 Stop 的私有封裝
        /// </summary>
        /// <param name="audBase"></param>
        /// <param name="forceDestroy"></param>
        private void _Stop(AudioBase audBase, bool disableEndEvent = false, bool forceDestroy = false)
        {
            if (audBase == null) return;

            this.ExitAndStop(audBase, false, disableEndEvent);

            Debug.Log(string.Format("Stop Audio: {0}", audBase?.mediaName));

            // 確保音訊都設置完畢後才進行 Destroy, 避免異步處理尚未完成, 就被 Destroy 掉導致操作到已銷毀物件
            if (audBase.isPrepared)
            {
                if (forceDestroy) this.Destroy(audBase);
                else if (audBase.onStopAndDestroy) this.Destroy(audBase);
            }
        }

        /// <summary>
        /// 停止
        /// </summary>
        /// <param name="audBase"></param>
        /// <param name="forceDestroy"></param>
        public void Stop(AudioBase audBase, bool disableEndEvent = false, bool forceDestroy = false)
        {
            this._Stop(audBase, disableEndEvent, forceDestroy);
        }

        /// <summary>
        /// 停止
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="forceDestroy"></param>
        public override void Stop(string assetName, bool disableEndEvent = false, bool forceDestroy = false)
        {
            AudioBase[] audBases = this.GetMediaComponents<AudioBase>(assetName);
            if (audBases.Length == 0) return;

            foreach (var audBase in audBases)
            {
                this._Stop(audBase, disableEndEvent, forceDestroy);
            }
        }

        /// <summary>
        /// 全部停止
        /// </summary>
        /// <param name="forceDestroy"></param>
        /// <returns></returns>
        public override void StopAll(bool disableEndEvent = false, bool forceDestroy = false)
        {
            if (this._listAllCache.Count == 0) return;

            foreach (var audBase in this._listAllCache.ToArray())
            {
                this._Stop(audBase, disableEndEvent, forceDestroy);
            }
        }
        #endregion

        #region 暫停 Puase
        /// <summary>
        /// 統一調用 Pause 的私有封裝
        /// </summary>
        /// <param name="audBase"></param>
        private void _Pause(AudioBase audBase)
        {
            if (audBase == null) return;

            this.ExitAndStop(audBase, true, false);

            Debug.Log(string.Format("Pause Audio: {0}, Current Length: {1} (s)", audBase?.mediaName, audBase?.CurrentLength()));
        }

        /// <summary>
        /// 暫停
        /// </summary>
        /// <param name="assetName"></param>
        public override void Pause(string assetName)
        {
            AudioBase[] audBases = this.GetMediaComponents<AudioBase>(assetName);
            if (audBases.Length == 0) return;

            foreach (var audBase in audBases)
            {
                this._Pause(audBase);
            }
        }

        /// <summary>
        /// 全部暫停
        /// </summary>
        /// <returns></returns>
        public override void PauseAll()
        {
            if (this._listAllCache.Count == 0) return;

            foreach (var audBase in this._listAllCache.ToArray())
            {
                this._Pause(audBase);
            }
        }
        #endregion

        protected override void LoadAndPlay(AudioBase audBase, int loops)
        {
            if (audBase == null) return;
            audBase.Play(loops);
        }

        protected override void ExitAndStop(AudioBase audBase, bool pause, bool disableEndEvent)
        {
            if (!pause)
            {
                if (disableEndEvent) audBase.SetEndEvent(null);
                audBase.Stop();
            }
            else audBase.Pause();
        }

        /// <summary>
        /// 請求音訊
        /// </summary>
        /// <param name="url"></param>
        /// <param name="audioType"></param>
        /// <returns></returns>
        public static async UniTask<AudioClip> AudioRequest(string url, UnityEngine.AudioType audioType = UnityEngine.AudioType.MPEG)
        {
            try
            {
                UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, audioType);
                ((DownloadHandlerAudioClip)request.downloadHandler).streamAudio = true;
                request.SendWebRequest().ToUniTask().Forget();

                if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log($"<color=#FF0000>Request failed, URL: {url}</color>");
                    request.Dispose();
                    request = null;

                    return null;
                }

                while (!request.isDone)
                {
                    await UniTask.Yield();
                }

                AudioClip audioClip = ((DownloadHandlerAudioClip)request.downloadHandler).audioClip;
                Debug.Log($"<color=#B1FF00>Request Audio => Channel: {audioClip.channels}, Frequency: {audioClip.frequency}, Sample: {audioClip.samples}, Length: {audioClip.length}, State: {audioClip.loadState}</color>");

                return audioClip;
            }
            catch
            {
                Debug.Log($"<color=#FF0000>Request failed, URL: {url}</color>");
                return null;
            }
        }
    }
}