using Cysharp.Threading.Tasks;
using OxGFrame.MediaFrame.VideoFrame;
using System.Reflection;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Audio;
using UnityEngine.Experimental.Video;
using UnityEngine.Video;

namespace OxGFrame.MediaFrame.Editor
{
    [CustomEditor(typeof(VideoBase))]
    public class VideoBaseEditor : UnityEditor.Editor
    {
        /// <summary>
        /// 預覽準備的最大 Timeout Seconds
        /// </summary>
        private const int _MAX_PREPARE_TIME_SECONDS = 60;

        protected VideoBase _target = null;

        // Preview
        protected GameObject _previewGo = null;
        protected VideoPlayer _previewVideoPlayer = null;
        protected bool _isPreviewRequesting = false;
        private int _previewRequestId = 0;

        // Preview Audio Sampling
        protected AudioSampleProvider _previewAudioSampleProvider = null;
        protected AudioClip _previewAudioStreamClip = null;
        private readonly object _previewAudioLocker = new object();
        private float[] _previewAudioRingBuffer = null;
        private int _previewAudioRingReadIdx = 0;
        private int _previewAudioRingCount = 0;

        // Preview Audio Native Consume (於音訊執行緒直接消費取樣，避免主執行緒卡頓造成爆音)
        private static AudioSampleProvider.ConsumeSampleFramesNativeFunction _consumeSampleFramesNativeFunction = null;
        private volatile bool _useNativeAudioConsume = false;
        private uint _previewAudioProviderId = 0;
        private int _previewAudioChannelCount = 0;

        private void OnEnable()
        {
            this._target = (VideoBase)target;
        }

        private void OnDisable()
        {
            // 關閉 Inspector 時，必須停止預覽
            this._StopPreviewVideo();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // Draw Views
            this.DrawVideoPreviewView();
        }

        protected virtual void DrawVideoPreviewView()
        {
            _InitPreviewIcons();

            var style = new GUIStyle(GUI.skin.box);
            style.normal.background = _Make2DTexture(1, 1, new Color(0f, 0.5f, 0.25f, 0.5f));
            GUILayout.BeginHorizontal(style);
            GUI.backgroundColor = Color.cyan;

            // 請求期間，禁用 Play 按鈕，避免重複請求
            EditorGUI.BeginDisabledGroup(this._isPreviewRequesting);
            if (GUILayout.Button(this._isPreviewRequesting ? new GUIContent("Requesting...") : _playIcon))
            {
                this._PlayPreviewVideo();
            }
            EditorGUI.EndDisabledGroup();

            bool isPaused = (this._previewVideoPlayer != null) && this._previewVideoPlayer.isPaused;
            if (GUILayout.Button(isPaused ? _resumeIcon : _pauseIcon))
            {
                this._PauseOrResumePreviewVideo();
            }

            if (GUILayout.Button(_stopIcon))
            {
                this._StopPreviewVideo();
            }

            GUILayout.EndHorizontal();

            // Seek Bar
            this.DrawVideoSeekBarView();

            // 繪製預覽畫面
            if (this._previewVideoPlayer != null &&
                this._previewVideoPlayer.texture != null)
            {
                var texture = this._previewVideoPlayer.texture;
                float aspect = (texture.height > 0) ? (1f * texture.width / texture.height) : (16f / 9f);
                Rect rect = GUILayoutUtility.GetAspectRect(aspect);
                EditorGUI.DrawPreviewTexture(rect, texture, null, ScaleMode.ScaleToFit);
            }
        }

        /// <summary>
        /// 繪製 Timecode 與拖拉條 (可拖動跳轉預覽進度)
        /// </summary>
        protected virtual void DrawVideoSeekBarView()
        {
            var videoPlayer = this._previewVideoPlayer;
            if (videoPlayer == null ||
                !videoPlayer.isPrepared)
                return;

            double totalTime = videoPlayer.length;
            if (totalTime <= 0d)
                return;

            var style = new GUIStyle(GUI.skin.box);
            style.normal.background = _Make2DTexture(1, 1, new Color(0f, 0.5f, 0.25f, 0.5f));
            GUILayout.BeginHorizontal(style);
            GUI.backgroundColor = Color.cyan;

            EditorGUI.BeginChangeCheck();
            float newTime = GUILayout.HorizontalSlider((float)videoPlayer.time, 0f, (float)totalTime);
            if (EditorGUI.EndChangeCheck())
            {
                // 拖動跳轉進度
                if (videoPlayer.canSetTime)
                {
                    videoPlayer.time = newTime;
                    // 跳轉後，清空音訊取樣緩衝區，避免殘留舊的取樣數據
                    this._FlushPreviewAudioSampleBuffer();
                }
            }
            GUILayout.Label($"{_FormatTimecode(videoPlayer.time)} / {_FormatTimecode(totalTime)}", GUILayout.ExpandWidth(false));

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 播放預覽影片 (支援全部 SourceType)
        /// </summary>
        private void _PlayPreviewVideo()
        {
            // 暫停中，直接恢復預覽
            if (this._previewVideoPlayer != null &&
                this._previewVideoPlayer.isPaused)
            {
                this._ResumePreviewVideo();
                return;
            }

            // 先停止上一次的預覽
            this._StopPreviewVideo();

            UniTask.Void(async () =>
            {
                // 記錄當前請求 Id，用於判斷預覽是否已被停止
                int requestId = this._previewRequestId;

                // 解析各 SourceType 的播放來源
                VideoClip videoClip = null;
                string url = null;
                switch (this._target.sourceType)
                {
                    case SourceType.Video:
                        videoClip = this._target.videoClip;
                        if (videoClip == null)
                        {
                            Debug.LogError("Cannot find VideoClip");
                            return;
                        }
                        break;

                    case SourceType.StreamingAssets:
                        url = this._target.GetVideoUrlFromStreamingAssets();
                        if (string.IsNullOrEmpty(this._target.fullPathName))
                        {
                            Debug.LogError($"VideoClip url request failed!!!");
                            return;
                        }
                        break;

                    case SourceType.Url:
                        this._isPreviewRequesting = true;
                        try
                        {
                            url = await this._target.GetVideoUrlFromURL();
                        }
                        // 請求發生異常時，必須重置請求標記，避免 Play 按鈕卡在禁用狀態
                        catch (System.Exception ex)
                        {
                            Debug.LogError($"VideoClip url request failed!!! Error: {ex.Message}");
                            return;
                        }
                        finally
                        {
                            this._isPreviewRequesting = false;
                        }
                        if (string.IsNullOrEmpty(url))
                        {
                            Debug.LogError($"VideoClip url request failed!!!");
                            return;
                        }
                        // 檢查 http 來源是否被 Insecure HTTP 設定阻擋
                        if (!_CheckInsecureHttpAllowed(url))
                        {
                            Debug.LogError($"Insecure HTTP is not allowed!!! Cannot preview: {url}");
                            return;
                        }
                        break;
                }

                // 如果預覽已被停止或 Editor 已被銷毀，則不進行播放
                if (this == null ||
                    requestId != this._previewRequestId)
                    return;

                // 建立隱藏的 VideoPlayer 進行預覽播放
                this._previewGo = new GameObject("[VideoBasePreview]");
                this._previewGo.hideFlags = HideFlags.HideAndDontSave;
                var videoPlayer = this._previewGo.AddComponent<VideoPlayer>();
                this._previewVideoPlayer = videoPlayer;

                videoPlayer.playOnAwake = false;
                videoPlayer.waitForFirstFrame = this._target.waitForFirstFrame;
                videoPlayer.playbackSpeed = this._target.playbackSpeed;
                videoPlayer.isLooping = this._target.loops == -1;
                videoPlayer.skipOnDrop = true;
                // 使用 APIOnly 模式，直接取用 VideoPlayer 內部的 Texture 進行繪製
                videoPlayer.renderMode = VideoRenderMode.APIOnly;
                videoPlayer.errorReceived += this._OnPreviewVideoError;
                videoPlayer.loopPointReached += this._OnPreviewVideoEnd;

                // 影片音訊使用 APIOnly 模式輸出，透過 AudioSampleProvider 進行取樣試聽
                // ※備註：Edit Mode 下引擎不會處理遊戲音訊 (Direct 或 AudioSource 模式皆無聲)，僅能透過 AudioUtil 的預覽通道發聲
                videoPlayer.audioOutputMode = VideoAudioOutputMode.APIOnly;

                if (videoClip != null)
                {
                    videoPlayer.source = VideoSource.VideoClip;
                    videoPlayer.clip = videoClip;
                }
                else
                {
                    videoPlayer.source = VideoSource.Url;
                    videoPlayer.url = url;
                    // Url 來源在 Prepare 前無法得知音軌數，必須先設置控制的音軌數 (Prepare 後才能取得 AudioSampleProvider)
                    videoPlayer.controlledAudioTrackCount = 1;
                    videoPlayer.EnableAudioTrack(0, true);
                }

                #region Prepare
                // 等待 VideoPlayer 準備完成
                videoPlayer.Prepare();
                double startTime = EditorApplication.timeSinceStartup;
                int maxPrepareTimeSeconds = (this._target.maxPrepareTimeSeconds <= 0) ? _MAX_PREPARE_TIME_SECONDS : this._target.maxPrepareTimeSeconds;
                do
                {
                    if (EditorApplication.timeSinceStartup - startTime >= maxPrepareTimeSeconds)
                        break;
                    // load balancing
                    await UniTask.Yield();
                    // 如果預覽已被停止或 Editor 已被銷毀，則不進行播放
                    if (this == null ||
                        requestId != this._previewRequestId)
                        return;
                } while (!videoPlayer.isPrepared);

                if (!videoPlayer.isPrepared)
                {
                    Debug.LogError($"VideoClip prepare failed!!!");
                    this._StopPreviewVideo();
                    return;
                }
                #endregion

                // 影片有音軌，則建立音訊取樣的試聽流程
                if (videoPlayer.audioTrackCount > 0)
                    this._StartPreviewAudioSampling(videoPlayer);

                videoPlayer.Play();

                // 掛載 EditorApplication.update 持續取樣音訊與 Repaint 更新預覽畫面
                EditorApplication.update -= this._OnEditorUpdate;
                EditorApplication.update += this._OnEditorUpdate;

                string sourceName = (videoClip != null) ? videoClip.name : url;
                Debug.Log($"<color=#ffe700>[Video Preview] Play => Name: {sourceName}, Size: {videoPlayer.width}x{videoPlayer.height}, Length: {videoPlayer.length} (s), AudioTracks: {videoPlayer.audioTrackCount}</color>");

                this.Repaint();
            });
        }

        /// <summary>
        /// 暫停或恢復預覽影片
        /// </summary>
        private void _PauseOrResumePreviewVideo()
        {
            var videoPlayer = this._previewVideoPlayer;
            if (videoPlayer == null ||
                !videoPlayer.isPrepared)
                return;

            if (videoPlayer.isPlaying)
            {
                videoPlayer.Pause();
                _PausePreviewClip();
            }
            else if (videoPlayer.isPaused)
                this._ResumePreviewVideo();
        }

        /// <summary>
        /// 恢復預覽影片 (如果已至結尾，則跳回開頭再繼續播放)
        /// </summary>
        private void _ResumePreviewVideo()
        {
            var videoPlayer = this._previewVideoPlayer;
            if (videoPlayer.length > 0d &&
                videoPlayer.time >= videoPlayer.length - 0.05d)
            {
                videoPlayer.time = 0d;
                this._FlushPreviewAudioSampleBuffer();
            }
            videoPlayer.Play();
            _ResumePreviewClip();
        }

        /// <summary>
        /// 停止預覽影片
        /// </summary>
        private void _StopPreviewVideo()
        {
            // 遞增請求 Id，用於取消尚未完成的預覽請求
            this._previewRequestId++;
            this._isPreviewRequesting = false;

            EditorApplication.update -= this._OnEditorUpdate;

            // 停止音訊取樣的試聽流程
            this._StopPreviewAudioSampling();

            if (this._previewVideoPlayer != null)
            {
                this._previewVideoPlayer.errorReceived -= this._OnPreviewVideoError;
                this._previewVideoPlayer.loopPointReached -= this._OnPreviewVideoEnd;
                this._previewVideoPlayer.Stop();
            }

            // 銷毀預覽用的 VideoPlayer
            if (this._previewGo != null)
                DestroyImmediate(this._previewGo);
            this._previewGo = null;
            this._previewVideoPlayer = null;

            this.Repaint();
        }

        /// <summary>
        /// 播放期間持續取樣音訊與 Repaint 更新預覽畫面
        /// </summary>
        private void _OnEditorUpdate()
        {
            this._PumpPreviewAudioSamples();
            this.Repaint();
        }

        /// <summary>
        /// 預覽播放結束時，自動暫停於結尾畫面 (loops = -1 表示循環播放，不在此列)
        /// <para> 可透過拖拉條跳轉進度，或者 Play 重新播放 </para>
        /// </summary>
        /// <param name="videoPlayer"></param>
        private void _OnPreviewVideoEnd(VideoPlayer videoPlayer)
        {
            if (videoPlayer.isLooping)
                return;
            videoPlayer.Pause();
            _PausePreviewClip();
            this.Repaint();
        }

        /// <summary>
        /// 預覽播放發生錯誤時，自動停止並清除預覽
        /// </summary>
        /// <param name="videoPlayer"></param>
        /// <param name="message"></param>
        private void _OnPreviewVideoError(VideoPlayer videoPlayer, string message)
        {
            Debug.LogError($"VideoClip request failed!!! Error: {message}");
            // 避免在 VideoPlayer 自身的事件派發期間直接銷毀，延遲一幀再停止
            EditorApplication.delayCall += this._StopPreviewVideo;
        }

        #region AudioSampleProvider for Preview
        /// <summary>
        /// 建立音訊取樣的試聽流程 (將 VideoPlayer 解碼的音訊取樣，透過串流式的程序化 AudioClip 交由 AudioUtil 進行試聽)
        /// </summary>
        /// <param name="videoPlayer"></param>
        private void _StartPreviewAudioSampling(VideoPlayer videoPlayer)
        {
            try
            {
                var provider = videoPlayer.GetAudioSampleProvider(0);
                if (provider == null)
                    return;

                int frequency = (int)provider.sampleRate;
                int channelCount = provider.channelCount;
                if (frequency <= 0 ||
                    channelCount <= 0)
                    return;

                this._previewAudioSampleProvider = provider;
                this._previewAudioChannelCount = channelCount;

                // 優先使用 Native Consume 的方式，於音訊執行緒直接消費取樣
                // ※備註：主執行緒卡頓時不會中斷取樣的消費，可避免取樣斷流造成爆音
                try
                {
                    // 官方 API 直接返回 Native Consume 的託管委派 (Windows 與 OSX 平台行為皆一致)
                    if (_consumeSampleFramesNativeFunction == null)
                        _consumeSampleFramesNativeFunction = AudioSampleProvider.consumeSampleFramesNativeFunction;
                    if (_consumeSampleFramesNativeFunction != null)
                    {
                        // 取樣不足時，由 Provider 自動填入靜音
                        provider.enableSilencePadding = true;
                        this._previewAudioProviderId = provider.id;
                        this._useNativeAudioConsume = true;
                    }
                }
                catch
                {
                    /* Nothing to do */
                }

                // Fallback：改由主執行緒取樣寫入環形緩衝區 (預留 1 秒的取樣容量)
                if (!this._useNativeAudioConsume)
                {
                    lock (this._previewAudioLocker)
                    {
                        this._previewAudioRingBuffer = new float[frequency * channelCount];
                        this._previewAudioRingReadIdx = 0;
                        this._previewAudioRingCount = 0;
                    }
                }

                // 建立串流式的程序化 AudioClip，於 PCMReaderCallback 中從緩衝區取出取樣數據進行發聲
                int lengthSamples = (videoPlayer.length > 0d) ? ((int)(videoPlayer.length * frequency) + frequency) : (frequency * 3600);
                this._previewAudioStreamClip = AudioClip.Create("[VideoBasePreviewAudio]", lengthSamples, channelCount, frequency, true, this._OnPreviewAudioRead);
                this._previewAudioStreamClip.hideFlags = HideFlags.HideAndDontSave;

                // 透過 AudioUtil 進行試聽 (影片循環時，音訊同樣循環)
                _PlayPreviewClip(this._previewAudioStreamClip, 0, videoPlayer.isLooping);
            }
            catch
            {
                /* Nothing to do */
            }
        }

        /// <summary>
        /// 將 AudioSampleProvider 的音訊取樣寫入環形緩衝區 (滿了則覆蓋最舊的取樣)
        /// </summary>
        private void _PumpPreviewAudioSamples()
        {
            // Native Consume 模式下，取樣直接於音訊執行緒消費，無需經由主執行緒
            if (this._useNativeAudioConsume)
                return;

            var provider = this._previewAudioSampleProvider;
            if (provider == null)
                return;

            try
            {
                uint availableCount = provider.availableSampleFrameCount;
                if (availableCount == 0)
                    return;

                int channelCount = provider.channelCount;
                var sampleFrames = new NativeArray<float>((int)availableCount * channelCount, Allocator.Temp);
                uint writtenCount = provider.ConsumeSampleFrames(sampleFrames);
                int writtenSampleCount = (int)writtenCount * channelCount;

                lock (this._previewAudioLocker)
                {
                    var ringBuffer = this._previewAudioRingBuffer;
                    if (ringBuffer != null)
                    {
                        int writeIdx = (this._previewAudioRingReadIdx + this._previewAudioRingCount) % ringBuffer.Length;
                        for (int i = 0; i < writtenSampleCount; i++)
                        {
                            ringBuffer[writeIdx] = sampleFrames[i];
                            writeIdx = (writeIdx + 1) % ringBuffer.Length;
                            if (this._previewAudioRingCount < ringBuffer.Length)
                                this._previewAudioRingCount++;
                            else
                                this._previewAudioRingReadIdx = (this._previewAudioRingReadIdx + 1) % ringBuffer.Length;
                        }
                    }
                }

                sampleFrames.Dispose();
            }
            catch
            {
                /* Nothing to do */
            }
        }

        /// <summary>
        /// 串流式的程序化 AudioClip 的 PCMReaderCallback (優先使用 Native Consume 直接消費取樣，Fallback 時從環形緩衝區取出)
        /// <para> ※備註：此回調會在音訊執行緒被調用，僅允許消費取樣與操作緩衝區 </para>
        /// </summary>
        /// <param name="data"></param>
        private void _OnPreviewAudioRead(float[] data)
        {
            // 先填入靜音，避免消費失敗時殘留雜訊數據
            System.Array.Clear(data, 0, data.Length);

            // Native Consume：於音訊執行緒直接消費 AudioSampleProvider 的取樣
            if (this._useNativeAudioConsume)
            {
                var consumeFn = _consumeSampleFramesNativeFunction;
                int channelCount = this._previewAudioChannelCount;
                if (consumeFn != null &&
                    channelCount > 0)
                {
                    var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                    try
                    {
                        uint sampleFrameCount = (uint)(data.Length / channelCount);
                        consumeFn(this._previewAudioProviderId, handle.AddrOfPinnedObject(), sampleFrameCount);
                    }
                    catch
                    {
                        /* Nothing to do */
                    }
                    finally
                    {
                        handle.Free();
                    }
                }
                return;
            }

            // Fallback：從環形緩衝區取出取樣數據 (不足的部分保持靜音)
            lock (this._previewAudioLocker)
            {
                var ringBuffer = this._previewAudioRingBuffer;
                if (ringBuffer == null)
                    return;
                int sampleCount = (data.Length <= this._previewAudioRingCount) ? data.Length : this._previewAudioRingCount;
                for (int i = 0; i < sampleCount; i++)
                {
                    data[i] = ringBuffer[this._previewAudioRingReadIdx];
                    this._previewAudioRingReadIdx = (this._previewAudioRingReadIdx + 1) % ringBuffer.Length;
                    this._previewAudioRingCount--;
                }
            }
        }

        /// <summary>
        /// 清空音訊取樣的環形緩衝區 (跳轉進度時，避免殘留舊的取樣數據)
        /// </summary>
        private void _FlushPreviewAudioSampleBuffer()
        {
            lock (this._previewAudioLocker)
            {
                this._previewAudioRingReadIdx = 0;
                this._previewAudioRingCount = 0;
            }
        }

        /// <summary>
        /// 停止音訊取樣的試聽流程
        /// </summary>
        private void _StopPreviewAudioSampling()
        {
            // 先關閉 Native Consume，讓音訊執行緒停止消費取樣
            this._useNativeAudioConsume = false;
            this._previewAudioProviderId = 0;
            this._previewAudioChannelCount = 0;

            _StopAllPreviewClips();

            if (this._previewAudioSampleProvider != null)
            {
                try
                {
                    this._previewAudioSampleProvider.Dispose();
                }
                catch
                {
                    /* Nothing to do */
                }
                this._previewAudioSampleProvider = null;
            }

            // 銷毀串流式的程序化 AudioClip
            if (this._previewAudioStreamClip != null)
                DestroyImmediate(this._previewAudioStreamClip);
            this._previewAudioStreamClip = null;

            this._FlushPreviewAudioSampleBuffer();
        }
        #endregion

        #region Insecure HTTP for Preview
        /// <summary>
        /// 檢查 http 來源是否被 Player Settings 的 Insecure HTTP 設定阻擋 (2022.1+ 才有此設定)
        /// <para> 如有阻擋，將詢問是否調整為 Only in development builds (僅影響 Editor 與 Development Build，不影響 Release) </para>
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static bool _CheckInsecureHttpAllowed(string url)
        {
#if UNITY_2022_1_OR_NEWER
            if (string.IsNullOrEmpty(url) ||
                !url.Trim().StartsWith("http://", System.StringComparison.OrdinalIgnoreCase))
                return true;

            if (PlayerSettings.insecureHttpOption != InsecureHttpOption.NotAllowed)
                return true;

            bool isConfirmed = EditorUtility.DisplayDialog
            (
                "Insecure HTTP Blocked",
                "\"Allow downloads over HTTP\" is set to \"Not allowed\" in Player Settings, so the http:// source cannot be requested.\n\nSwitch it to \"Only in development builds\"? (Only affects the Editor and Development Builds)",
                "Switch (Development Only)",
                "Cancel"
            );
            if (isConfirmed)
            {
                PlayerSettings.insecureHttpOption = InsecureHttpOption.DevelopmentOnly;
                return true;
            }
            return false;
#else
            return true;
#endif
        }
        #endregion

        #region Editor Icons for Preview
        // Icons
        private static GUIContent _playIcon = null;
        private static GUIContent _pauseIcon = null;
        private static GUIContent _resumeIcon = null;
        private static GUIContent _stopIcon = null;

        /// <summary>
        /// 初始 Preview 使用的內建 Icons (只解析一次)
        /// </summary>
        private static void _InitPreviewIcons()
        {
            if (_playIcon != null)
                return;
            _playIcon = _GetIconContent(new string[] { "PlayButton" }, "Play", "Play");
            _pauseIcon = _GetIconContent(new string[] { "PauseButton" }, "Pause", "Pause");
            _resumeIcon = _GetIconContent(new string[] { "PauseButton On", "PauseButton" }, "Resume", "Resume");
            _stopIcon = _GetIconContent(new string[] { "StopButton", "PreMatQuad" }, "Stop", "Stop");
        }

        /// <summary>
        /// 取得內建 Icon 的 GUIContent (兼容 Unity 2021 ~ 6.x，依候補名稱逐一探測，全部找不到時退回文字顯示)
        /// </summary>
        /// <param name="iconNames"></param>
        /// <param name="fallbackText"></param>
        /// <param name="tooltip"></param>
        /// <returns></returns>
        private static GUIContent _GetIconContent(string[] iconNames, string fallbackText, string tooltip)
        {
            foreach (string iconName in iconNames)
            {
                // Dark 主題優先使用 d_ 前綴的 Icon
                string[] candidateNames = EditorGUIUtility.isProSkin ? new string[] { $"d_{iconName}", iconName } : new string[] { iconName };
                foreach (string name in candidateNames)
                {
                    var iconContent = _LoadIconContent(name);
                    if (iconContent != null)
                        return new GUIContent(iconContent.image, tooltip);
                }
            }
            return new GUIContent(fallbackText, tooltip);
        }

        /// <summary>
        /// 讀取內建 Icon (讀取期間暫時關閉 Logger，避免 Icon 不存在時輸出錯誤訊息)
        /// </summary>
        /// <param name="iconName"></param>
        /// <returns></returns>
        private static GUIContent _LoadIconContent(string iconName)
        {
            bool logEnabled = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = false;
            GUIContent iconContent = null;
            try
            {
                iconContent = EditorGUIUtility.IconContent(iconName);
            }
            catch
            {
                /* Nothing to do */
            }
            finally
            {
                Debug.unityLogger.logEnabled = logEnabled;
            }
            if (iconContent == null ||
                iconContent.image == null)
                return null;
            return iconContent;
        }
        #endregion

        /// <summary>
        /// 將秒數格式化為 Timecode 字串
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        private static string _FormatTimecode(double seconds)
        {
            if (seconds < 0d)
                seconds = 0d;
            var timeSpan = System.TimeSpan.FromSeconds(seconds);
            if (timeSpan.Hours > 0)
                return $"{timeSpan.Hours}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}.{timeSpan.Milliseconds:000}";
            return $"{timeSpan.Minutes:00}:{timeSpan.Seconds:00}.{timeSpan.Milliseconds:000}";
        }

        #region AudioUtil for Preview
        /// <summary>
        /// 透過反射取得 UnityEditor.AudioUtil 的方法
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="argTypes"></param>
        /// <returns></returns>
        private static MethodInfo _GetAudioUtilMethod(string methodName, System.Type[] argTypes = null)
        {
            var audioUtilType = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");
            if (audioUtilType == null)
                return null;
            if (argTypes == null)
                return audioUtilType.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
            return audioUtilType.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public, null, argTypes, null);
        }

        /// <summary>
        /// 播放試聽的 AudioClip
        /// </summary>
        /// <param name="audioClip"></param>
        /// <param name="startSample"></param>
        /// <param name="loop"></param>
        private static void _PlayPreviewClip(AudioClip audioClip, int startSample = 0, bool loop = false)
        {
            var method = _GetAudioUtilMethod("PlayPreviewClip", new System.Type[] { typeof(AudioClip), typeof(int), typeof(bool) });
            method?.Invoke(null, new object[] { audioClip, startSample, loop });
        }

        /// <summary>
        /// 暫停試聽的 AudioClip
        /// </summary>
        private static void _PausePreviewClip()
        {
            var method = _GetAudioUtilMethod("PausePreviewClip");
            method?.Invoke(null, null);
        }

        /// <summary>
        /// 恢復試聽的 AudioClip
        /// </summary>
        private static void _ResumePreviewClip()
        {
            var method = _GetAudioUtilMethod("ResumePreviewClip");
            method?.Invoke(null, null);
        }

        /// <summary>
        /// 停止全部試聽的 AudioClip
        /// </summary>
        private static void _StopAllPreviewClips()
        {
            var method = _GetAudioUtilMethod("StopAllPreviewClips");
            method?.Invoke(null, null);
        }
        #endregion

        private Texture2D _Make2DTexture(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}