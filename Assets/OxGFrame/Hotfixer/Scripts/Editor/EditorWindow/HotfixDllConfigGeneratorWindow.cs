using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OxGFrame.Hotfixer.Editor
{
    public class HotfixDllConfigGeneratorWindow : EditorWindow
    {
        private static HotfixDllConfigGeneratorWindow _instance = null;
        internal static HotfixDllConfigGeneratorWindow GetInstance()
        {
            if (_instance == null)
                _instance = GetWindow<HotfixDllConfigGeneratorWindow>();
            return _instance;
        }

        [SerializeField]
        public List<string> aotDlls = new List<string>() { "*.dll" };
        [SerializeField]
        public List<string> hotfixDlls = new List<string>() { "*.dll" };
        [SerializeField]
        public bool autoReveal;

        // Preset Plan
        public List<HotfixDllPlan> hotfixDllPlans = new List<HotfixDllPlan>();
        private int _choicePlanIndex = 0;

        private Vector2 _scrollview1;
        private Vector2 _scrollview2;

        private SerializedObject _serObj;
        private SerializedProperty _aotDllsPty;
        private SerializedProperty _hotfixDllsPty;
        private SerializedProperty _hotfixDllPlansPty;

        internal static string projectPath;
        internal static string keySaver;

        private static Vector2 _windowSize = new Vector2(800f, 150f);

        [MenuItem(HotfixHelper.MENU_ROOT + "Hotfix Dll Config Generator (hotfixdllconfig.conf)", false, 99)]
        public static void ShowWindow()
        {
            projectPath = Application.dataPath;
            keySaver = $"{projectPath}_{nameof(HotfixDllConfigGeneratorWindow)}";

            _instance = null;
            GetInstance().titleContent = new GUIContent("Hotfix Dll Config Generator");
            GetInstance().Show();
            GetInstance().minSize = _windowSize;
        }

        private void OnEnable()
        {
            this._serObj = new SerializedObject(this);
            this._aotDllsPty = this._serObj.FindProperty("aotDlls");
            this._hotfixDllsPty = this._serObj.FindProperty("hotfixDlls");
            this._hotfixDllPlansPty = this._serObj.FindProperty("hotfixDllPlans");

            string json = EditorStorage.GetData(keySaver, "aotDlls", string.Empty);
            if (!string.IsNullOrEmpty(json))
                this.aotDlls = JsonConvert.DeserializeObject<List<string>>(json);
            json = EditorStorage.GetData(keySaver, "hotfixDlls", string.Empty);
            if (!string.IsNullOrEmpty(json))
                this.hotfixDlls = JsonConvert.DeserializeObject<List<string>>(json);

            this.autoReveal = Convert.ToBoolean(EditorStorage.GetData(keySaver, "autoReveal", "true"));

            // Preset Hotfix Dll Plans
            string jsonHotfixDllPlans = EditorStorage.GetData(keySaver, "hotfixDllPlans", string.Empty);
            if (!string.IsNullOrEmpty(jsonHotfixDllPlans))
                this.hotfixDllPlans = JsonConvert.DeserializeObject<List<HotfixDllPlan>>(jsonHotfixDllPlans);
            this._choicePlanIndex = Convert.ToInt32(EditorStorage.GetData(keySaver, "_choicePlanIndex", "0"));
        }

        private void OnGUI()
        {
            this._DrawExportHotfixDllConfigToStreamingAssetsView();
            this._DrawHotfixDllPlansView();
        }

        private void _DrawExportHotfixDllConfigToStreamingAssetsView()
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
            GUILayout.Label(new GUIContent("Export HotfixDllConfig To StreamingAssets"), centeredStyle);
            EditorGUILayout.Space();

            // draw here
            this._scrollview1 = EditorGUILayout.BeginScrollView(this._scrollview1, true, true);
            this._DrawAotDllsView();
            this._DrawHotfixDllsView();
            this._DrawProcessButtonView();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        private void _DrawAotDllsView()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            this._serObj.Update();
            EditorGUI.BeginChangeCheck();
            var label = new GUIContent(this._aotDllsPty.displayName, "You need to append \".dll\" as the suffix, ex: ABC.dll.");
            EditorGUILayout.PropertyField(this._aotDllsPty, label, true);
            if (EditorGUI.EndChangeCheck())
            {
                this._serObj.ApplyModifiedProperties();
                string json = JsonConvert.SerializeObject(this.aotDlls);
                EditorStorage.SaveData(keySaver, "aotDlls", json);
            }

            Color bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(255, 151, 240, 255);
            if (GUILayout.Button("Reset", GUILayout.MaxWidth(100f)))
            {
                this.aotDlls = new List<string>() { "*.dll" };
                string json = JsonConvert.SerializeObject(this.aotDlls);
                EditorStorage.SaveData(keySaver, "aotDlls", json);
            }
            GUI.backgroundColor = bc;
            EditorGUILayout.EndHorizontal();
        }

        private void _DrawHotfixDllsView()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            this._serObj.Update();
            EditorGUI.BeginChangeCheck();
            var label = new GUIContent(this._hotfixDllsPty.displayName, "You need to append \".dll\" as the suffix, ex: ABC.dll.");
            EditorGUILayout.PropertyField(this._hotfixDllsPty, label, true);
            if (EditorGUI.EndChangeCheck())
            {
                this._serObj.ApplyModifiedProperties();
                string json = JsonConvert.SerializeObject(this.hotfixDlls);
                EditorStorage.SaveData(keySaver, "hotfixDlls", json);
            }

            Color bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(255, 151, 240, 255);
            if (GUILayout.Button("Reset", GUILayout.MaxWidth(100f)))
            {
                this.hotfixDlls = new List<string>() { "*.dll" };
                string json = JsonConvert.SerializeObject(this.hotfixDlls);
                EditorStorage.SaveData(keySaver, "hotfixDlls", json);
            }
            GUI.backgroundColor = bc;
            EditorGUILayout.EndHorizontal();
        }

        private void _DrawProcessButtonView()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            // file name label
            {
                var style = new GUIStyle(EditorStyles.label) { richText = true };
                string fileName = $"{HotfixSettings.settings.hotfixDllCfgName}{HotfixSettings.settings.hotfixDllCfgExtension}";
                GUILayout.Label($"Config Name: <b><color=#ffed29>{fileName}</color></b>", style);
            }

            GUILayout.FlexibleSpace();

            // auto reveal toggle
            this.autoReveal = GUILayout.Toggle(this.autoReveal, new GUIContent("Auto Reveal", "If checked, after process will reveal destination folder."));
            EditorStorage.SaveData(keySaver, "autoReveal", this.autoReveal.ToString());

            // process button
            Color bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(255, 185, 83, 255);
            if (GUILayout.Button("Cipher Process", GUILayout.MaxWidth(110f)))
            {
                string fileName = $"{HotfixSettings.settings.hotfixDllCfgName}{HotfixSettings.settings.hotfixDllCfgExtension}";
                string outputPath = Application.streamingAssetsPath;
                HotfixHelper.ExportHotfixDllConfig(this.aotDlls, this.hotfixDlls, true);
                EditorUtility.DisplayDialog("Process Message", $"Export [Cipher] {fileName} To StreamingAssets.", "OK");
                AssetDatabase.Refresh();
                if (this.autoReveal) EditorUtility.RevealInFinder($"{outputPath}/{fileName}");
            }
            GUI.backgroundColor = bc;
            GUI.backgroundColor = new Color32(255, 185, 83, 255);
            if (GUILayout.Button("Plaintext Process", GUILayout.MaxWidth(125f)))
            {
                string fileName = $"{HotfixSettings.settings.hotfixDllCfgName}{HotfixSettings.settings.hotfixDllCfgExtension}";
                string outputPath = Application.streamingAssetsPath;
                HotfixHelper.ExportHotfixDllConfig(this.aotDlls, this.hotfixDlls, false);
                EditorUtility.DisplayDialog("Process Message", $"Export [Plaintext] {fileName} To StreamingAssets.", "OK");
                AssetDatabase.Refresh();
                if (this.autoReveal) EditorUtility.RevealInFinder($"{outputPath}/{fileName}");
            }
            GUI.backgroundColor = bc;
            EditorGUILayout.EndHorizontal();
        }

        #region Preset Hotfix Dll Plans
        private void _DrawHotfixDllPlansView()
        {
            this._scrollview2 = EditorGUILayout.BeginScrollView(this._scrollview2, true, true);

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
            GUILayout.Label(new GUIContent("Preset Hotfix Dll Plans"), centeredStyle);
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            // Add popup selection
            List<string> planNames = new List<string>();
            if (this.hotfixDllPlans.Count > 0)
            {
                foreach (var plan in this.hotfixDllPlans)
                {
                    planNames.Add(plan.planName);
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
                    $"The plan selection is [{this.hotfixDllPlans[this._choicePlanIndex].planName}]\nDo you want to copy current all values?",
                    "copy current and override",
                    "cancel"
                );

                if (confirmation) this._CopyCurrentToHotfixDllPlan();
            }
            GUI.backgroundColor = bc;
            bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(0, 255, 128, 255);
            if (GUILayout.Button("Load Plan", GUILayout.MaxWidth(100f)))
            {
                this._LoadHotfixDllPlanToCurrent();
            }
            GUI.backgroundColor = bc;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            this._serObj.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(this._hotfixDllPlansPty, true);
            if (EditorGUI.EndChangeCheck())
            {
                this._serObj.ApplyModifiedProperties();
                string json = JsonConvert.SerializeObject(this.hotfixDllPlans);
                EditorStorage.SaveData(keySaver, "hotfixDllPlans", json);
            }

            bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(255, 151, 240, 255);
            if (GUILayout.Button("Reset", GUILayout.MaxWidth(100f)))
            {
                bool confirmation = EditorUtility.DisplayDialog
                (
                    $"Reset Hotfix Dll Plans Notification",
                    $"Do you want to reset hotfix dll plans?",
                    "reset",
                    "cancel"
                );

                if (confirmation)
                {
                    // Reset hotfix dll plans
                    this.hotfixDllPlans = new List<HotfixDllPlan>() { new HotfixDllPlan() };
                    string json = JsonConvert.SerializeObject(this.hotfixDllPlans);
                    EditorStorage.SaveData(keySaver, "hotfixDllPlans", json);

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
                this._ExportHotfixDllPlans();
            }
            GUI.backgroundColor = bc;
            // Load File
            bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(0, 249, 255, 255);
            if (GUILayout.Button("Load File", GUILayout.MaxWidth(100f)))
            {
                this._ImportHotfixDllPlans();
            }
            GUI.backgroundColor = bc;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
        }

        private void _LoadHotfixDllPlanToCurrent()
        {
            if (this.hotfixDllPlans.Count == 0) return;

            var plan = this.hotfixDllPlans[this._choicePlanIndex];

            this.aotDlls = plan.aotDlls;
            this.hotfixDlls = plan.hotfixDlls;

            // Save
            string json = JsonConvert.SerializeObject(this.aotDlls);
            EditorStorage.SaveData(keySaver, "aotDlls", json);
            json = JsonConvert.SerializeObject(this.hotfixDlls);
            EditorStorage.SaveData(keySaver, "hotfixDlls", json);
        }

        private void _CopyCurrentToHotfixDllPlan()
        {
            if (this.hotfixDllPlans.Count == 0) return;

            var plan = this.hotfixDllPlans[this._choicePlanIndex];

            // Copy
            plan.aotDlls = this.aotDlls;
            plan.hotfixDlls = this.hotfixDlls;

            this.hotfixDllPlans[this._choicePlanIndex] = plan;

            // Save
            string json = JsonConvert.SerializeObject(this.hotfixDllPlans);
            EditorStorage.SaveData(keySaver, "hotfixDllPlans", json);
        }

        private void _ExportHotfixDllPlans()
        {
            string savePath = EditorStorage.GetData(keySaver, $"hotfixDllPlanFilePath", Application.dataPath);
            var filePath = EditorUtility.SaveFilePanel("Save Hotfix Dll Plan Json File", savePath, "HotfixDllPlan", "json");

            if (!string.IsNullOrEmpty(filePath))
            {
                EditorStorage.SaveData(keySaver, $"hotfixDllPlanFilePath", Path.GetDirectoryName(filePath));
                string json = JsonConvert.SerializeObject(this.hotfixDllPlans, Formatting.Indented);
                HotfixHelper.WriteTxt(json, filePath);
                AssetDatabase.Refresh();
            }
        }

        private void _ImportHotfixDllPlans()
        {
            string loadPath = EditorStorage.GetData(keySaver, $"hotfixDllPlanFilePath", Application.dataPath);
            string filePath = EditorUtility.OpenFilePanel("Select Hotfix Dll Plan Json File", !string.IsNullOrEmpty(loadPath) ? loadPath : Application.dataPath, "json");

            if (!string.IsNullOrEmpty(filePath))
            {
                EditorStorage.SaveData(keySaver, $"hotfixDllPlanFilePath", Path.GetDirectoryName(filePath));
                string json = File.ReadAllText(filePath);
                this.hotfixDllPlans = JsonConvert.DeserializeObject<List<HotfixDllPlan>>(json);

                // Resave hotfix dll plans without format
                json = JsonConvert.SerializeObject(this.hotfixDllPlans);
                EditorStorage.SaveData(keySaver, "hotfixDllPlans", json);

                // Reset index
                this._choicePlanIndex = 0;
                EditorStorage.SaveData(keySaver, "_choicePlanIndex", this._choicePlanIndex.ToString());
            }
        }
        #endregion
    }
}