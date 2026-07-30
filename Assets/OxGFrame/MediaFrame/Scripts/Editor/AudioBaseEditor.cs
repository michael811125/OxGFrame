using Cysharp.Threading.Tasks;
using OxGFrame.MediaFrame.AudioFrame;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace OxGFrame.MediaFrame.Editor
{
    [CustomEditor(typeof(AudioBase))]
    public class AudioBaseEditor : UnityEditor.Editor
    {
        protected AudioBase _target = null;

        // Properties
        protected SerializedProperty _audioLength;

        // Preview
        protected AudioClip _previewAudioClip = null;
        protected bool _isPreviewClipRequested = false;
        protected bool _isPreviewRequesting = false;
        protected bool _isPreviewPaused = false;
        protected float _previewAudioTime = 0f;
        private int _previewRequestId = 0;

        private void OnEnable()
        {
            this._target = (AudioBase)target;

            // Init Properties
            this._audioLength = serializedObject.FindProperty("audioLength");
        }

        private void OnDisable()
        {
            // 關閉 Inspector 時，必須停止試聽
            this._StopPreviewAudio();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // Draw Views
            this.DrawAudioLengthView();
            this.DrawAudioPreviewView();
        }

        protected virtual async void DrawAudioLengthView()
        {
            var style = new GUIStyle(GUI.skin.box);
            style.normal.background = _Make2DTexture(1, 1, new Color(0f, 0.5f, 0.25f, 0.5f));
            GUILayout.BeginHorizontal(style);
            GUI.backgroundColor = Color.cyan;

            serializedObject.Update();
            EditorGUILayout.PropertyField(this._audioLength);
            if (GUILayout.Button("Preload"))
            {
                // must set dirty (save will be success)
                EditorUtility.SetDirty(this._target);

                UniTask.Void(async () =>
                {
                    AudioClip audioClip = null;
                    switch (this._target.sourceType)
                    {
                        case SourceType.Audio:
                            audioClip = this._target.audioClip;
                            if (audioClip != null)
                                this._audioLength.floatValue = this._target.audioLength = this._target.audioClip.length;
                            else
                                Debug.LogError("Cannot find AudioClip");
                            break;

                        case SourceType.StreamingAssets:
                            audioClip = await this._target.GetAudioFromStreamingAssets(false);
                            if (audioClip != null)
                                this._audioLength.floatValue = this._target.audioLength = audioClip.length;
                            break;

                        case SourceType.Url:
                            {
                                // 檢查 http 來源是否被 Insecure HTTP 設定阻擋
                                string url = await this._target.GetAudioUrlFromURL();
                                if (_CheckInsecureHttpAllowed(url))
                                    audioClip = await this._target.GetAudioFromURL(false);
                                if (audioClip != null)
                                    this._audioLength.floatValue = this._target.audioLength = audioClip.length;
                            }
                            break;
                    }

                    if (audioClip != null)
                        Debug.Log($"<color=#ffe700>AudioClip Info => Channel: {audioClip.channels}, Frequency: {audioClip.frequency}, Sample: {audioClip.samples}, Length: {audioClip.length}, State: {audioClip.loadState}, Preload Audio Data*: {audioClip.preloadAudioData}</color>");
                    else
                        Debug.LogError($"AudioClip request failed!!!");

                    serializedObject.ApplyModifiedProperties();
                });
            }
            else
            {
                serializedObject.ApplyModifiedProperties();
            }

            GUILayout.EndHorizontal();
        }

        protected virtual void DrawAudioPreviewView()
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
                this._PlayPreviewAudio();
            }
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button(this._isPreviewPaused ? _resumeIcon : _pauseIcon))
            {
                this._PauseOrResumePreviewAudio();
            }

            if (GUILayout.Button(_stopIcon))
            {
                this._StopPreviewAudio();
            }

            GUILayout.EndHorizontal();

            // Seek Bar
            this.DrawAudioSeekBarView();
        }

        /// <summary>
        /// 繪製 Timecode 與拖拉條 (可拖動跳轉試聽進度)
        /// </summary>
        protected virtual void DrawAudioSeekBarView()
        {
            if (this._previewAudioClip == null)
                return;

            var audioClip = this._previewAudioClip;
            float totalTime = audioClip.length;
            if (totalTime <= 0f)
                return;

            // 試聽期間，同步當前試聽的時間位置
            bool isPlaying = _IsPreviewClipPlaying();
            if (isPlaying &&
                !this._isPreviewPaused)
                this._previewAudioTime = _GetPreviewClipSamplePosition() / (float)audioClip.frequency;
            float currentTime = Mathf.Clamp(this._previewAudioTime, 0f, totalTime);

            var style = new GUIStyle(GUI.skin.box);
            style.normal.background = _Make2DTexture(1, 1, new Color(0f, 0.5f, 0.25f, 0.5f));
            GUILayout.BeginHorizontal(style);
            GUI.backgroundColor = Color.cyan;

            EditorGUI.BeginChangeCheck();
            float newTime = GUILayout.HorizontalSlider(currentTime, 0f, totalTime);
            if (EditorGUI.EndChangeCheck())
            {
                this._previewAudioTime = newTime;
                int samplePosition = Mathf.Clamp((int)(newTime * audioClip.frequency), 0, audioClip.samples - 1);
                // 試聽中或暫停中直接跳轉進度，反之則從指定位置開始試聽
                if (isPlaying || this._isPreviewPaused)
                    _SetPreviewClipSamplePosition(audioClip, samplePosition);
                else
                    _PlayPreviewClip(audioClip, samplePosition, this._target.loops == -1);
            }
            GUILayout.Label($"{_FormatTimecode(currentTime)} / {_FormatTimecode(totalTime)}", GUILayout.ExpandWidth(false));

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 播放試聽音訊 (支援全部 SourceType)
        /// </summary>
        private void _PlayPreviewAudio()
        {
            // 暫停中，直接恢復試聽
            if (this._previewAudioClip != null &&
                this._isPreviewPaused)
            {
                _ResumePreviewClip();
                this._isPreviewPaused = false;
                return;
            }

            // 先停止上一次的試聽
            this._StopPreviewAudio();

            UniTask.Void(async () =>
            {
                // 記錄當前請求 Id，用於判斷試聽是否已被停止
                int requestId = this._previewRequestId;
                // 標記 AudioClip 是否由請求產生 (非 Assign 的 AudioClip 資源)
                bool isRequested = this._target.sourceType != SourceType.Audio;

                AudioClip audioClip = null;
                this._isPreviewRequesting = isRequested;
                try
                {
                    switch (this._target.sourceType)
                    {
                        case SourceType.Audio:
                            audioClip = this._target.audioClip;
                            break;

                        case SourceType.StreamingAssets:
                            audioClip = await this._target.GetAudioFromStreamingAssets(false);
                            break;

                        case SourceType.Url:
                            {
                                // 檢查 http 來源是否被 Insecure HTTP 設定阻擋
                                string url = await this._target.GetAudioUrlFromURL();
                                if (!_CheckInsecureHttpAllowed(url))
                                {
                                    Debug.LogError($"Insecure HTTP is not allowed!!! Cannot preview: {url}");
                                    return;
                                }
                                audioClip = await this._target.GetAudioFromURL(false);
                            }
                            break;
                    }
                }
                // 請求發生異常時，必須重置請求標記，避免 Play 按鈕卡在禁用狀態
                catch (System.Exception ex)
                {
                    Debug.LogError($"AudioClip request failed!!! Error: {ex.Message}");
                    return;
                }
                finally
                {
                    this._isPreviewRequesting = false;
                }

                if (audioClip == null)
                {
                    if (isRequested)
                        Debug.LogError($"AudioClip request failed!!!");
                    else
                        Debug.LogError("Cannot find AudioClip");
                    return;
                }

                // 如果試聽已被停止或 Editor 已被銷毀，則銷毀由請求產生的 AudioClip
                if (this == null ||
                    requestId != this._previewRequestId)
                {
                    if (isRequested)
                        DestroyImmediate(audioClip);
                    return;
                }

                this._previewAudioClip = audioClip;
                this._isPreviewClipRequested = isRequested;

                // 透過 AudioUtil 進行試聽 (loops = -1 表示循環播放)
                _PlayPreviewClip(audioClip, 0, this._target.loops == -1);
                Debug.Log($"<color=#ffe700>[Audio Preview] Play => Name: {audioClip.name}, Length: {audioClip.length} (s)</color>");

                // 掛載 EditorApplication.update 持續 Repaint 更新拖拉條
                EditorApplication.update -= this._OnEditorUpdate;
                EditorApplication.update += this._OnEditorUpdate;

                this.Repaint();
            });
        }

        /// <summary>
        /// 暫停或恢復試聽音訊
        /// </summary>
        private void _PauseOrResumePreviewAudio()
        {
            if (this._previewAudioClip == null)
                return;

            if (!this._isPreviewPaused)
            {
                // 未在試聽中，則不做暫停
                if (!_IsPreviewClipPlaying())
                    return;
                _PausePreviewClip();
                this._isPreviewPaused = true;
            }
            else
            {
                _ResumePreviewClip();
                this._isPreviewPaused = false;
            }
        }

        /// <summary>
        /// 停止試聽音訊
        /// </summary>
        private void _StopPreviewAudio()
        {
            // 遞增請求 Id，用於取消尚未完成的試聽請求
            this._previewRequestId++;
            this._isPreviewRequesting = false;
            this._isPreviewPaused = false;
            this._previewAudioTime = 0f;

            EditorApplication.update -= this._OnEditorUpdate;

            _StopAllPreviewClips();

            // 銷毀由請求產生的 AudioClip (Assign 的 AudioClip 資源不在此列)
            if (this._previewAudioClip != null &&
                this._isPreviewClipRequested)
                DestroyImmediate(this._previewAudioClip);
            this._previewAudioClip = null;
            this._isPreviewClipRequested = false;

            this.Repaint();
        }

        /// <summary>
        /// 試聽期間持續 Repaint 更新拖拉條
        /// </summary>
        private void _OnEditorUpdate()
        {
            this.Repaint();
        }

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

        /// <summary>
        /// 返回是否試聽中
        /// </summary>
        /// <returns></returns>
        private static bool _IsPreviewClipPlaying()
        {
            var method = _GetAudioUtilMethod("IsPreviewClipPlaying");
            object result = method?.Invoke(null, null);
            return (result is bool isPlaying) && isPlaying;
        }

        /// <summary>
        /// 取得當前試聽的 Sample 位置
        /// </summary>
        /// <returns></returns>
        private static int _GetPreviewClipSamplePosition()
        {
            var method = _GetAudioUtilMethod("GetPreviewClipSamplePosition");
            object result = method?.Invoke(null, null);
            return (result is int samplePosition) ? samplePosition : 0;
        }

        /// <summary>
        /// 設置當前試聽的 Sample 位置 (跳轉進度)
        /// </summary>
        /// <param name="audioClip"></param>
        /// <param name="samplePosition"></param>
        private static void _SetPreviewClipSamplePosition(AudioClip audioClip, int samplePosition)
        {
            var method = _GetAudioUtilMethod("SetPreviewClipSamplePosition", new System.Type[] { typeof(AudioClip), typeof(int) });
            method?.Invoke(null, new object[] { audioClip, samplePosition });
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