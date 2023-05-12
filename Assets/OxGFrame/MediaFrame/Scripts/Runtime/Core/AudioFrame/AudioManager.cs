using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;

namespace OxGFrame.MediaFrame.AudioFrame
{
    public class AudioManager : MediaManager<AudioBase>
    {
        [Header("Audio Mixer")]
        [SerializeField, Tooltip("Setup AudioMixer in list")]
        private List<AudioMixer> _listMixer = new List<AudioMixer>();                             // 中控混音器
        private Dictionary<string, float> _dictMixerExpParams = new Dictionary<string, float>();  // 用於記錄 Exposed Parameters 參數
        private Dictionary<string, GameObject> _dictNodes = new Dictionary<string, GameObject>(); // 節點物件

        private static readonly object _locker = new object();
        private static AudioManager _instance = null;
        internal static AudioManager GetInstance()
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
            string newName = $"[{nameof(AudioManager)}]";
            this.gameObject.name = newName;
            if (this.gameObject.transform.root.name == newName)
            {
                var container = GameObject.Find(nameof(OxGFrame));
                if (container == null) container = new GameObject(nameof(OxGFrame));
                this.gameObject.transform.SetParent(container.transform);
                DontDestroyOnLoad(container);
            }
            else DontDestroyOnLoad(this.gameObject.transform.root);

            foreach (var nodeName in Enum.GetNames(typeof(SoundType)))
            {
                if (!this._dictNodes.ContainsKey(nodeName))
                {
                    this._dictNodes.Add(nodeName, this.CreateNode(nodeName, this.transform));
                }
            }
        }

        protected override void SetParent(AudioBase audBase, Transform parent)
        {
            if (parent != null) audBase.gameObject.transform.SetParent(parent);
            else if (this._dictNodes.TryGetValue(audBase.audioType.soundType.ToString(), out GameObject goNode))
            {
                audBase.gameObject.transform.SetParent(goNode.transform);
            }
        }

        #region 中控 Mixer
        /// <summary>
        /// 依照 Mixer 的名稱與其中的 ExposedParam 合併成雙 key, 執行自動記錄
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
        /// 設置指定該 Mixer 中的 Exposed 參數
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
        /// 清除指定該 Mixer 中的 Exposed 參數
        /// </summary>
        /// <param name="mixer"></param>
        /// <param name="expParam"></param>
        public void ClearMixerExposedParam(AudioMixer mixer, string expParam)
        {
            mixer.ClearFloat(expParam);
        }

        /// <summary>
        /// 自動清除該 Mixer 的 Exposed 參數
        /// </summary>
        /// <param name="mixer"></param>
        public void AutoClearMixerExposedParams(AudioMixer mixer)
        {
            if (this._dictMixerExpParams.Count == 0) return;

            foreach (var key in this._dictMixerExpParams.Keys)
            {
                string reKey = key.Replace($"{mixer.name},", string.Empty);
                mixer.ClearFloat(reKey);
            }
        }

        /// <summary>
        /// 自動復原該 Mixer 的 Exposed 參數
        /// </summary>
        /// <param name="mixer"></param>
        public void AutoRestoreMixerExposedParams(AudioMixer mixer)
        {
            if (this._dictMixerExpParams.Count == 0) return;

            foreach (var expParam in this._dictMixerExpParams)
            {
                string reKey = expParam.Key.Replace($"{mixer.name},", string.Empty);
                mixer.SetFloat(reKey, expParam.Value);
            }
        }

        /// <summary>
        /// 設置切換 Mixer 的 Snapshot
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
        /// 設置 Mixer 與 Snapshot 的混和過度
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
        /// 取得該 Mixer 中的 Snapshot
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
        /// 從 List 中透過 Mixer 的名稱返回該對應的 Mixer
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
        private void _Play(AudioBase audBase, int loops, float volume)
        {
            if (audBase == null) return;

            this.LoadAndPlay(audBase, loops, volume);

            Debug.Log(string.Format("Play Audio: {0}, Current Length: {1} (s)", audBase?.mediaName, audBase?.CurrentLength()));
        }

        /// <summary>
        /// 播放
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="parent"></param>
        /// <param name="loops"></param>
        /// <returns></returns>
        public override async UniTask<AudioBase[]> Play(string packageName, string assetName, Transform parent = null, int loops = 0, float volume = 0f)
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
                GameObject go = await this.LoadAssetIntoCache(packageName, assetName);
                Transform spawnParent = null;
                if (parent == null) spawnParent = this.transform;
                AudioBase audBase = await this.CloneAsset<AudioBase>(assetName, go, parent, spawnParent);
                if (audBase == null)
                {
                    Debug.LogWarning(string.Format("Asset not found at this path!!!【Audio】: {0}", assetName));
                    return new AudioBase[] { };
                }

                this._Play(audBase, loops, volume);

                return new AudioBase[] { audBase };
            }
            else
            {
                for (int i = 0; i < audBases.Length; i++)
                {
                    if (!audBases[i].IsPlaying()) this._Play(audBases[i], 0, 0f);
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

            foreach (var audBase in this._listAllCache)
            {
                if (audBase.IsPaused()) this._Play(audBase, 0, 0f);
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

            foreach (var audBase in this._listAllCache)
            {
                this._Pause(audBase);
            }
        }
        #endregion

        protected override void LoadAndPlay(AudioBase audBase, int loops, float volume)
        {
            if (audBase == null) return;
            audBase.Play(loops, volume);
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