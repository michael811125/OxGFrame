using OxGFrame.MediaFrame;
using System;
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

        internal const string KEY_SAVE_DATA_FOR_GENERATE_MEDIA_URL_CONFIG_EDITOR = "KEY_SAVE_DATA_FOR_GENERATE_MEDIA_URL_CONFIG_EDITOR";

        private static Vector2 _windowSize = new Vector2(800f, 150f);

        [MenuItem(MediaHelper.MenuRoot + "Media Url Config Generator", false, 899)]
        public static void ShowWindow()
        {
            _instance = null;
            GetInstance().titleContent = new GUIContent("Media Url Config Generator");
            GetInstance().Show();
            GetInstance().minSize = _windowSize;
        }

        private void OnEnable()
        {
            this.audioUrlset = EditorStorage.GetData(KEY_SAVE_DATA_FOR_GENERATE_MEDIA_URL_CONFIG_EDITOR, "audioUrlset", "127.0.0.1/audio/");
            this.videoUrlset = EditorStorage.GetData(KEY_SAVE_DATA_FOR_GENERATE_MEDIA_URL_CONFIG_EDITOR, "videoUrlset", "127.0.0.1/video/");

            this.autoReveal = Convert.ToBoolean(EditorStorage.GetData(KEY_SAVE_DATA_FOR_GENERATE_MEDIA_URL_CONFIG_EDITOR, "autoReveal", "true"));
        }

        private void OnGUI()
        {
            // operation type area
            EditorGUI.BeginChangeCheck();

            this._DrawExportMediaUrlConfigToStreamingAssetsView();
        }

        private void _DrawExportMediaUrlConfigToStreamingAssetsView()
        {
            EditorGUILayout.Space();

            GUIStyle style = new GUIStyle();
            var bg = new Texture2D(1, 1);
            ColorUtility.TryParseHtmlString("#1c589c", out Color color);
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
            if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(KEY_SAVE_DATA_FOR_GENERATE_MEDIA_URL_CONFIG_EDITOR, "audioUrlset", this.audioUrlset);
            EditorGUILayout.EndHorizontal();
        }

        private void _DrawVideoUrlsetTextFieldView()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            this.videoUrlset = EditorGUILayout.TextField("Video Urlset", this.videoUrlset);
            if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(KEY_SAVE_DATA_FOR_GENERATE_MEDIA_URL_CONFIG_EDITOR, "videoUrlset", this.videoUrlset);
            EditorGUILayout.EndHorizontal();
        }

        private void _DrawProcessButtonView()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            // auto reveal toggle
            this.autoReveal = GUILayout.Toggle(this.autoReveal, new GUIContent("Auto Reveal", "If checked after process will reveal destination folder."));
            EditorStorage.SaveData(KEY_SAVE_DATA_FOR_GENERATE_MEDIA_URL_CONFIG_EDITOR, "autoReveal", this.autoReveal.ToString());

            // process button
            Color bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(255, 185, 83, 255);
            if (GUILayout.Button("Process", GUILayout.MaxWidth(100f)))
            {
                string outputPath = Application.streamingAssetsPath;
                MediaHelper.ExportMediaUrlConfig(this.audioUrlset, this.videoUrlset, outputPath);
                EditorUtility.DisplayDialog("Process Message", "Export MediaUrlConfig To StreamingAssets.", "OK");
                AssetDatabase.Refresh();
                if (this.autoReveal) EditorUtility.RevealInFinder($"{outputPath}/{MediaConfig.mediaUrlFileName}");
            }
            GUI.backgroundColor = bc;
            EditorGUILayout.EndHorizontal();
        }
    }
}