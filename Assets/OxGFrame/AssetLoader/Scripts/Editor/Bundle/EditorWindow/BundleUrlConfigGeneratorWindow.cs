using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using OxGFrame.AssetLoader.Bundle;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OxGFrame.AssetLoader.Editor
{
    public class BundleUrlConfigGeneratorWindow : EditorWindow
    {
        private static BundleUrlConfigGeneratorWindow _instance = null;
        internal static BundleUrlConfigGeneratorWindow GetInstance()
        {
            if (_instance == null) _instance = GetWindow<BundleUrlConfigGeneratorWindow>();
            return _instance;
        }

        [SerializeField]
        public string bundleIp;
        [SerializeField]
        public string bundleFallbackIp;
        [SerializeField]
        public string storeLink;
        [SerializeField]
        public bool autoReveal;

        // Preset Plan
        public List<BundleUrlPlan> bundleUrlPlans = new List<BundleUrlPlan>();
        private int _choicePlanIndex = 0;

        private Vector2 _scrollview;

        private SerializedObject _serObj;
        private SerializedProperty _bundleUrlPlansPty;

        internal static string projectPath;
        internal static string keySaver;

        private static Vector2 _windowSize = new Vector2(800f, 150f);

        [MenuItem(BundleHelper.MENU_ROOT + "Bundle Url Config Generator (burlconfig.conf)", false, 899)]
        public static void ShowWindow()
        {
            projectPath = Application.dataPath;
            keySaver = $"{projectPath}_{nameof(BundleUrlConfigGeneratorWindow)}";

            _instance = null;
            GetInstance().titleContent = new GUIContent("Bundle Url Config Generator");
            GetInstance().Show();
            GetInstance().minSize = _windowSize;
        }

        private void OnEnable()
        {
            this._serObj = new SerializedObject(this);
            this._bundleUrlPlansPty = this._serObj.FindProperty("bundleUrlPlans");

            this.bundleIp = EditorStorage.GetData(keySaver, "bundleIp", "127.0.0.1");
            this.bundleFallbackIp = EditorStorage.GetData(keySaver, "bundleFallbackIp", "127.0.0.1");
            this.storeLink = EditorStorage.GetData(keySaver, "storeLink", "http://");

            this.autoReveal = Convert.ToBoolean(EditorStorage.GetData(keySaver, "autoReveal", "true"));

            // Preset Bundle Url Plans
            string jsonBundleUrlPlans = EditorStorage.GetData(keySaver, "bundleUrlPlans", string.Empty);
            if (!string.IsNullOrEmpty(jsonBundleUrlPlans)) this.bundleUrlPlans = JsonConvert.DeserializeObject<List<BundleUrlPlan>>(jsonBundleUrlPlans);
            this._choicePlanIndex = Convert.ToInt32(EditorStorage.GetData(keySaver, "_choicePlanIndex", "0"));
        }

        private void OnGUI()
        {
            // operation type area
            EditorGUI.BeginChangeCheck();

            this._DrawExportBundleUrlConfigToStreamingAssetsView();
            this._DrawBundleUrlPlansView();
        }

        private void _DrawExportBundleUrlConfigToStreamingAssetsView()
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
            GUILayout.Label(new GUIContent("Export BundleUrlConfig To StreamingAssets"), centeredStyle);
            EditorGUILayout.Space();

            // draw here
            this._DrawBundleIPTextFieldView();
            this._DrawBundleFallbackIPTextFieldView();
            this._DrawStoreLinkTextFieldView();
            this._DrawProcessButtonView();

            EditorGUILayout.EndVertical();
        }

        private void _DrawBundleIPTextFieldView()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 180;
            this.bundleIp = EditorGUILayout.TextField("Bundle IP or Domain", this.bundleIp);
            EditorGUIUtility.labelWidth = labelWidth;
            if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(keySaver, "bundleIp", this.bundleIp);
            EditorGUILayout.EndHorizontal();
        }

        private void _DrawBundleFallbackIPTextFieldView()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 180;
            this.bundleFallbackIp = EditorGUILayout.TextField("Bundle Fallback IP or Domain", this.bundleFallbackIp);
            EditorGUIUtility.labelWidth = labelWidth;
            if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(keySaver, "bundleFallbackIp", this.bundleFallbackIp);
            EditorGUILayout.EndHorizontal();
        }

        private void _DrawStoreLinkTextFieldView()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 180;
            this.storeLink = EditorGUILayout.TextField("Store Link", this.storeLink);
            EditorGUIUtility.labelWidth = labelWidth;
            if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(keySaver, "storeLink", this.storeLink);
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
            if (GUILayout.Button("Cipher Process", GUILayout.MaxWidth(110f)))
            {
                string outputPath = Application.streamingAssetsPath;
                BundleHelper.ExportBundleUrlConfig(this.bundleIp, this.bundleFallbackIp, this.storeLink, outputPath, true);
                EditorUtility.DisplayDialog("Process Message", "Export [Cipher] BundleUrlConfig To StreamingAssets.", "OK");
                AssetDatabase.Refresh();
                string bundleUrlFileName = $"{PatchSetting.setting.bundleUrlCfgName}{PatchSetting.BUNDLE_URL_CFG_EXTENSION}";
                if (this.autoReveal) EditorUtility.RevealInFinder($"{outputPath}/{bundleUrlFileName}");
            }
            GUI.backgroundColor = bc;
            GUI.backgroundColor = new Color32(255, 185, 83, 255);
            if (GUILayout.Button("Plaintext Process", GUILayout.MaxWidth(125f)))
            {
                string outputPath = Application.streamingAssetsPath;
                BundleHelper.ExportBundleUrlConfig(this.bundleIp, this.bundleFallbackIp, this.storeLink, outputPath, false);
                EditorUtility.DisplayDialog("Process Message", "Export [Plaintext] BundleUrlConfig To StreamingAssets.", "OK");
                AssetDatabase.Refresh();
                string bundleUrlFileName = $"{PatchSetting.setting.bundleUrlCfgName}{PatchSetting.BUNDLE_URL_CFG_EXTENSION}";
                if (this.autoReveal) EditorUtility.RevealInFinder($"{outputPath}/{bundleUrlFileName}");
            }
            GUI.backgroundColor = bc;
            EditorGUILayout.EndHorizontal();
        }

        #region Preset Bundle Plans
        private void _DrawBundleUrlPlansView()
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
            GUILayout.Label(new GUIContent("Preset Bundle Url Plans"), centeredStyle);
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            // Add popup selection
            List<string> planNames = new List<string>();
            if (this.bundleUrlPlans.Count > 0)
            {
                foreach (var bundleUrlPlan in this.bundleUrlPlans)
                {
                    planNames.Add(bundleUrlPlan.planName);
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
                    $"The plan selection is [{this.bundleUrlPlans[this._choicePlanIndex].planName}]\nDo you want to copy current all values?",
                    "copy current and override",
                    "cancel"
                );

                if (confirmation) this._CopyCurrentToBundleUrlPlan();
            }
            GUI.backgroundColor = bc;
            bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(0, 255, 128, 255);
            if (GUILayout.Button("Load Plan", GUILayout.MaxWidth(100f)))
            {
                this._LoadBundleUrlPlanToCurrent();
            }
            GUI.backgroundColor = bc;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            this._serObj.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(this._bundleUrlPlansPty, true);
            if (EditorGUI.EndChangeCheck())
            {
                this._serObj.ApplyModifiedProperties();
                string json = JsonConvert.SerializeObject(this.bundleUrlPlans);
                EditorStorage.SaveData(keySaver, "bundleUrlPlans", json);
            }

            bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(255, 151, 240, 255);
            if (GUILayout.Button("Reset", GUILayout.MaxWidth(100f)))
            {
                bool confirmation = EditorUtility.DisplayDialog
                (
                    $"Reset Bundle Url Plans Notification",
                    $"Do you want to reset bundle url plans?",
                    "reset",
                    "cancel"
                );

                if (confirmation)
                {
                    // Reset bundle plans
                    this.bundleUrlPlans = new List<BundleUrlPlan>() { new BundleUrlPlan() };
                    string json = JsonConvert.SerializeObject(this.bundleUrlPlans);
                    EditorStorage.SaveData(keySaver, "bundleUrlPlans", json);

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
                this._ExportBundleUrlPlans();
            }
            GUI.backgroundColor = bc;
            // Load File
            bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(0, 249, 255, 255);
            if (GUILayout.Button("Load File", GUILayout.MaxWidth(100f)))
            {
                this._ImportBundleUrlPlans();
            }
            GUI.backgroundColor = bc;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
        }

        private void _LoadBundleUrlPlanToCurrent()
        {
            if (this.bundleUrlPlans.Count == 0) return;

            var bundleUrlPlan = this.bundleUrlPlans[this._choicePlanIndex];

            this.bundleIp = bundleUrlPlan.bundleIp;
            this.bundleFallbackIp = bundleUrlPlan.bundleFallbackIp;
            this.storeLink = bundleUrlPlan.storeLink;

            // Save
            EditorStorage.SaveData(keySaver, "bundleIp", this.bundleIp);
            EditorStorage.SaveData(keySaver, "bundleFallbackIp", this.bundleFallbackIp);
            EditorStorage.SaveData(keySaver, "storeLink", this.storeLink);
        }

        private void _CopyCurrentToBundleUrlPlan()
        {
            if (this.bundleUrlPlans.Count == 0) return;

            var bundleUrlPlan = this.bundleUrlPlans[this._choicePlanIndex];

            // Copy
            bundleUrlPlan.bundleIp = this.bundleIp;
            bundleUrlPlan.bundleFallbackIp = this.bundleFallbackIp;
            bundleUrlPlan.storeLink = this.storeLink;

            this.bundleUrlPlans[this._choicePlanIndex] = bundleUrlPlan;

            // Save
            string json = JsonConvert.SerializeObject(this.bundleUrlPlans);
            EditorStorage.SaveData(keySaver, "bundleUrlPlans", json);
        }

        private void _ExportBundleUrlPlans()
        {
            string savePath = EditorStorage.GetData(keySaver, $"bundleUrlPlanFIlePath", Application.dataPath);
            var filePath = EditorUtility.SaveFilePanel("Save Bundle Url Plan Json File", savePath, "BundleUrlPlan", "json");

            if (!string.IsNullOrEmpty(filePath))
            {
                EditorStorage.SaveData(keySaver, $"bundleUrlPlanFIlePath", Path.GetDirectoryName(filePath));
                string json = JsonConvert.SerializeObject(this.bundleUrlPlans, Formatting.Indented);
                BundleHelper.WriteTxt(json, filePath);
                AssetDatabase.Refresh();
            }
        }

        private void _ImportBundleUrlPlans()
        {
            string loadPath = EditorStorage.GetData(keySaver, $"bundleUrlPlanFIlePath", Application.dataPath);
            string filePath = EditorUtility.OpenFilePanel("Select Bundle Url Plan Json File", !string.IsNullOrEmpty(loadPath) ? loadPath : Application.dataPath, "json");

            if (!string.IsNullOrEmpty(filePath))
            {
                EditorStorage.SaveData(keySaver, $"bundleUrlPlanFIlePath", Path.GetDirectoryName(filePath));
                string json = File.ReadAllText(filePath);
                this.bundleUrlPlans = JsonConvert.DeserializeObject<List<BundleUrlPlan>>(json);

                // Resave bundle plans without format
                json = JsonConvert.SerializeObject(this.bundleUrlPlans);
                EditorStorage.SaveData(keySaver, "bundleUrlPlans", json);

                // Reset index
                this._choicePlanIndex = 0;
                EditorStorage.SaveData(keySaver, "_choicePlanIndex", this._choicePlanIndex.ToString());
            }
        }
        #endregion
    }
}