using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OxGFrame.MediaFrame.Editor
{
    public class MediaUrlConfigGeneratorWindow : EditorWindow
    {
        private static MediaUrlConfigGeneratorWindow _instance = null;
        internal static MediaUrlConfigGeneratorWindow GetInstance()
        {
            if (_instance == null) _instance = GetWindow<MediaUrlConfigGeneratorWindow>();
            return _instance;
        }

        [SerializeField]
        public string audioUrlset;
        [SerializeField]
        public string videoUrlset;
        [SerializeField]
        public bool autoReveal;

        // Preset Plan
        public List<MediaUrlPlan> mediaUrlPlans = new List<MediaUrlPlan>();
        private int _choicePlanIndex = 0;

        private Vector2 _scrollview;

        private SerializedObject _serObj;
        private SerializedProperty _mediaUrlPlansPty;

        internal static string projectPath;
        internal static string keySaver;

        private static Vector2 _windowSize = new Vector2(800f, 150f);

        [MenuItem(MediaHelper.MENU_ROOT + "Media Url Config Generator (" + MediaConfig.MEDIA_URL_CFG_NAME + ")", false, 899)]
        public static void ShowWindow()
        {
            projectPath = Application.dataPath;
            keySaver = $"{projectPath}_{nameof(MediaUrlConfigGeneratorWindow)}";

            _instance = null;
            GetInstance().titleContent = new GUIContent("Media Url Config Generator");
            GetInstance().Show();
            GetInstance().minSize = _windowSize;
        }

        private void OnEnable()
        {
            this._serObj = new SerializedObject(this);
            this._mediaUrlPlansPty = this._serObj.FindProperty("mediaUrlPlans");

            this.audioUrlset = EditorStorage.GetData(keySaver, "audioUrlset", "http://127.0.0.1/audio/");
            this.videoUrlset = EditorStorage.GetData(keySaver, "videoUrlset", "http://127.0.0.1/video/");

            this.autoReveal = Convert.ToBoolean(EditorStorage.GetData(keySaver, "autoReveal", "true"));

            // Preset Media Url Plans
            string jsonMediaUrlPlans = EditorStorage.GetData(keySaver, "mediaUrlPlans", string.Empty);
            if (!string.IsNullOrEmpty(jsonMediaUrlPlans)) this.mediaUrlPlans = JsonConvert.DeserializeObject<List<MediaUrlPlan>>(jsonMediaUrlPlans);
            this._choicePlanIndex = Convert.ToInt32(EditorStorage.GetData(keySaver, "_choicePlanIndex", "0"));
        }

        private void OnGUI()
        {
            // operation type area
            EditorGUI.BeginChangeCheck();

            this._DrawExportMediaUrlConfigToStreamingAssetsView();
            this._DrawMediaUrlPlansView();
        }

        private void _DrawExportMediaUrlConfigToStreamingAssetsView()
        {
            EditorGUILayout.Space();

            GUIStyle style = new GUIStyle();
            var bg = new Texture2D(1, 1);
            ColorUtility.TryParseHtmlString("#1e3836", out Color color);
            Color[] pixels = Enumerable.Repeat(color, Screen.width * Screen.height).ToArray();
            bg.SetPixels(pixels);
            bg.Apply();
            style.normal.background = bg;
            EditorGUILayout.BeginVertical(style);
            var centeredStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
            centeredStyle.alignment = TextAnchor.UpperCenter;
            GUILayout.Label(new GUIContent("Export MediaUrlConfig To StreamingAssets"), centeredStyle);
            EditorGUILayout.Space();

            // draw here
            this._DrawAudioUrlsetTextFieldView();
            this._DrawVideoUrlsetTextFieldView();
            this._DrawProcessButtonView();

            EditorGUILayout.EndVertical();
        }

        private void _DrawAudioUrlsetTextFieldView()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            this.audioUrlset = EditorGUILayout.TextField("Audio Urlset", this.audioUrlset);
            if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(keySaver, "audioUrlset", this.audioUrlset);
            EditorGUILayout.EndHorizontal();
        }

        private void _DrawVideoUrlsetTextFieldView()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            this.videoUrlset = EditorGUILayout.TextField("Video Urlset", this.videoUrlset);
            if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(keySaver, "videoUrlset", this.videoUrlset);
            EditorGUILayout.EndHorizontal();
        }

        private void _DrawProcessButtonView()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            // auto reveal toggle
            this.autoReveal = GUILayout.Toggle(this.autoReveal, new GUIContent("Auto Reveal", "If checked, after process will reveal destination folder."));
            EditorStorage.SaveData(keySaver, "autoReveal", this.autoReveal.ToString());

            // process button
            Color bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(255, 185, 83, 255);
            if (GUILayout.Button("Process", GUILayout.MaxWidth(100f)))
            {
                string outputPath = Application.streamingAssetsPath;
                MediaHelper.ExportMediaUrlConfig(this.audioUrlset, this.videoUrlset, outputPath);
                EditorUtility.DisplayDialog("Process Message", "Export MediaUrlConfig To StreamingAssets.", "OK");
                AssetDatabase.Refresh();
                if (this.autoReveal) EditorUtility.RevealInFinder($"{outputPath}/{MediaConfig.MEDIA_URL_CFG_NAME}");
            }
            GUI.backgroundColor = bc;
            EditorGUILayout.EndHorizontal();
        }

        #region Preset Media Plans
        private void _DrawMediaUrlPlansView()
        {
            this._scrollview = EditorGUILayout.BeginScrollView(this._scrollview, true, true);

            EditorGUILayout.Space();

            GUIStyle style = new GUIStyle();
            var bg = new Texture2D(1, 1);
            ColorUtility.TryParseHtmlString("#263840", out Color color);
            Color[] pixels = Enumerable.Repeat(color, Screen.width * Screen.height).ToArray();
            bg.SetPixels(pixels);
            bg.Apply();
            style.normal.background = bg;
            EditorGUILayout.BeginVertical(style);

            var centeredStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
            centeredStyle.alignment = TextAnchor.UpperCenter;
            GUILayout.Label(new GUIContent("Preset Media Url Plans"), centeredStyle);
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            // Add popup selection
            List<string> planNames = new List<string>();
            if (this.mediaUrlPlans.Count > 0)
            {
                foreach (var mediaUrlPlan in this.mediaUrlPlans)
                {
                    planNames.Add(mediaUrlPlan.planName);
                }
            }
            EditorGUI.BeginChangeCheck();
            this._choicePlanIndex = EditorGUILayout.Popup("Plan Selection", this._choicePlanIndex, planNames.ToArray());
            if (this._choicePlanIndex < 0) this._choicePlanIndex = 0;
            if (EditorGUI.EndChangeCheck())
            {
                EditorStorage.SaveData(keySaver, "_choicePlanIndex", this._choicePlanIndex.ToString());
            }

            // Load selection button
            Color bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(83, 152, 255, 255);
            if (GUILayout.Button("Copy Current", GUILayout.MaxWidth(100f)))
            {
                bool confirmation = EditorUtility.DisplayDialog
                (
                    $"Copy Current Notification",
                    $"The plan selection is [{this.mediaUrlPlans[this._choicePlanIndex].planName}]\nDo you want to copy current all values?",
                    "copy current and override",
                    "cancel"
                );

                if (confirmation) this._CopyCurrentToMediaUrlPlan();
            }
            GUI.backgroundColor = bc;
            bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(0, 255, 128, 255);
            if (GUILayout.Button("Load Plan", GUILayout.MaxWidth(100f)))
            {
                this._LoadMediaUrlPlanToCurrent();
            }
            GUI.backgroundColor = bc;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            this._serObj.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(this._mediaUrlPlansPty, true);
            if (EditorGUI.EndChangeCheck())
            {
                this._serObj.ApplyModifiedProperties();
                string json = JsonConvert.SerializeObject(this.mediaUrlPlans);
                EditorStorage.SaveData(keySaver, "mediaUrlPlans", json);
            }

            bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(255, 151, 240, 255);
            if (GUILayout.Button("Reset", GUILayout.MaxWidth(100f)))
            {
                bool confirmation = EditorUtility.DisplayDialog
                (
                    $"Reset Media Url Plans Notification",
                    $"Do you want to reset media url plans?",
                    "reset",
                    "cancel"
                );

                if (confirmation)
                {
                    // Reset media plans
                    this.mediaUrlPlans = new List<MediaUrlPlan>() { new MediaUrlPlan() };
                    string json = JsonConvert.SerializeObject(this.mediaUrlPlans);
                    EditorStorage.SaveData(keySaver, "mediaUrlPlans", json);

                    // Reset index
                    this._choicePlanIndex = 0;
                    EditorStorage.SaveData(keySaver, "_choicePlanIndex", this._choicePlanIndex.ToString());
                }
            }
            GUI.backgroundColor = bc;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            // Save File
            bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(255, 220, 0, 255);
            if (GUILayout.Button("Save File", GUILayout.MaxWidth(100f)))
            {
                this._ExportMediaUrlPlans();
            }
            GUI.backgroundColor = bc;
            // Load File
            bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(0, 249, 255, 255);
            if (GUILayout.Button("Load File", GUILayout.MaxWidth(100f)))
            {
                this._ImportMediaUrlPlans();
            }
            GUI.backgroundColor = bc;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
        }

        private void _LoadMediaUrlPlanToCurrent()
        {
            if (this.mediaUrlPlans.Count == 0) return;

            var mediaUrlPlan = this.mediaUrlPlans[this._choicePlanIndex];

            this.audioUrlset = mediaUrlPlan.audioUrlset;
            this.videoUrlset = mediaUrlPlan.videoUrlset;

            // Save
            EditorStorage.SaveData(keySaver, "audioUrlset", this.audioUrlset);
            EditorStorage.SaveData(keySaver, "videoUrlset", this.videoUrlset);
        }

        private void _CopyCurrentToMediaUrlPlan()
        {
            if (this.mediaUrlPlans.Count == 0) return;

            var mediaUrlPlan = this.mediaUrlPlans[this._choicePlanIndex];

            // Copy
            mediaUrlPlan.audioUrlset = this.audioUrlset;
            mediaUrlPlan.videoUrlset = this.videoUrlset;

            this.mediaUrlPlans[this._choicePlanIndex] = mediaUrlPlan;

            // Save
            string json = JsonConvert.SerializeObject(this.mediaUrlPlans);
            EditorStorage.SaveData(keySaver, "mediaUrlPlans", json);
        }

        private void _ExportMediaUrlPlans()
        {
            string savePath = EditorStorage.GetData(keySaver, $"mediaUrlPlanFIlePath", Application.dataPath);
            var filePath = EditorUtility.SaveFilePanel("Save Media Url Plan Json File", savePath, "MediaUrlPlan", "json");

            if (!string.IsNullOrEmpty(filePath))
            {
                EditorStorage.SaveData(keySaver, $"mediaUrlPlanFIlePath", Path.GetDirectoryName(filePath));
                string json = JsonConvert.SerializeObject(this.mediaUrlPlans, Formatting.Indented);
                MediaHelper.WriteTxt(json, filePath);
                AssetDatabase.Refresh();
            }
        }

        private void _ImportMediaUrlPlans()
        {
            string loadPath = EditorStorage.GetData(keySaver, $"mediaUrlPlanFIlePath", Application.dataPath);
            string filePath = EditorUtility.OpenFilePanel("Select Media Url Plan Json File", !string.IsNullOrEmpty(loadPath) ? loadPath : Application.dataPath, "json");

            if (!string.IsNullOrEmpty(filePath))
            {
                EditorStorage.SaveData(keySaver, $"mediaUrlPlanFIlePath", Path.GetDirectoryName(filePath));
                string json = File.ReadAllText(filePath);
                this.mediaUrlPlans = JsonConvert.DeserializeObject<List<MediaUrlPlan>>(json);

                // Resave media plans without format
                json = JsonConvert.SerializeObject(this.mediaUrlPlans);
                EditorStorage.SaveData(keySaver, "mediaUrlPlans", json);

                // Reset index
                this._choicePlanIndex = 0;
                EditorStorage.SaveData(keySaver, "_choicePlanIndex", this._choicePlanIndex.ToString());
            }
        }
        #endregion
    }
}