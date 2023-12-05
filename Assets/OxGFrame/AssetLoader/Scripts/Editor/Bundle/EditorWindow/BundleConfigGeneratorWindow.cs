using Newtonsoft.Json;
using OxGFrame.AssetLoader.Bundle;
using OxGFrame.AssetLoader.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using YooAsset.Editor;

namespace OxGFrame.AssetLoader.Editor
{
    public class BundleConfigGeneratorWindow : EditorWindow
    {
        public enum OperationType
        {
            ExportAppConfigToStreamingAssets = 0,
            ExportConfigsAndAppBundlesForCDN = 1,
            ExportAppBundlesWithoutConfigsForCDN = 2,
            ExportIndividualDLCBundlesForCDN = 3
        }

        private static BundleConfigGeneratorWindow _instance = null;
        internal static BundleConfigGeneratorWindow GetInstance()
        {
            if (_instance == null) _instance = GetWindow<BundleConfigGeneratorWindow>();
            return _instance;
        }

        [SerializeField]
        public OperationType operationType;
        [SerializeField]
        public string[] sourceFolder = new string[Enum.GetNames(typeof(OperationType)).Length];
        [SerializeField]
        public string[] exportFolder = new string[Enum.GetNames(typeof(OperationType)).Length];
        [SerializeField]
        public BuildTarget buildTarget;
        [SerializeField]
        public bool activeBuildTarget;
        [SerializeField]
        public string productName;
        [SerializeField]
        public string appVersion;
        [SerializeField]
        public List<string> exportAppPackages = new List<string>() { "DefaultPackage" };
        [SerializeField]
        public List<DlcInfo> exportIndividualPackages = new List<DlcInfo>();
        [SerializeField]
        public string groupInfoArgs;
        [SerializeField]
        public List<GroupInfo> groupInfos;
        [SerializeField]
        public bool autoReveal;

        // Preset Plan
        public List<BundlePlan> bundlePlans = new List<BundlePlan>();
        private int _choicePlanIndex = 0;

        private Vector2 _scrollview1;
        private Vector2 _scrollview2;

        private SerializedObject _serObj;
        private SerializedProperty _groupInfosPty;
        private SerializedProperty _exportAppPackagesPty;
        private SerializedProperty _exportIndividualPackagesPty;
        private SerializedProperty _bundlePlansPty;

        internal static string PROJECT_PATH;
        internal static string KEY_SAVER;

        private static Vector2 _windowSize = new Vector2(800f, 400f);

        [MenuItem(BundleHelper.MenuRoot + "Export Bundle And Config Generator", false, 889)]
        public static void ShowWindow()
        {
            PROJECT_PATH = Application.dataPath;
            KEY_SAVER = $"{PROJECT_PATH}_{nameof(BundleConfigGeneratorWindow)}";

            _instance = null;
            GetInstance().titleContent = new GUIContent("Export Bundle And Config Generator");
            GetInstance().Show();
            GetInstance().minSize = _windowSize;
        }

        private void OnEnable()
        {
            this._serObj = new SerializedObject(this);
            this._groupInfosPty = this._serObj.FindProperty("groupInfos");
            this._exportAppPackagesPty = this._serObj.FindProperty("exportAppPackages");
            this._exportIndividualPackagesPty = this._serObj.FindProperty("exportIndividualPackages");
            this._bundlePlansPty = this._serObj.FindProperty("bundlePlans");

            int operationTypeCount = Enum.GetNames(typeof(OperationType)).Length;
            for (int i = 0; i < operationTypeCount; i++)
            {
                this.sourceFolder[i] = EditorStorage.GetData(KEY_SAVER, $"sourceFolder{i}", Path.Combine($"{Application.dataPath}/", AssetBundleBuilderHelper.GetDefaultBuildOutputRoot()));
                this.exportFolder[i] = EditorStorage.GetData(KEY_SAVER, $"exportFolder{i}", Path.Combine($"{Application.dataPath}/", $"{EditorTools.GetProjectPath()}/ExportBundles"));
            }

            this.operationType = (OperationType)Convert.ToInt32(EditorStorage.GetData(KEY_SAVER, "operationType", "0"));
            this.productName = EditorStorage.GetData(KEY_SAVER, "productName", Application.productName);
            this.appVersion = EditorStorage.GetData(KEY_SAVER, "appVersion", Application.version);
            string jsonExportAppPackages = EditorStorage.GetData(KEY_SAVER, "exportAppPackages", string.Empty);
            if (!string.IsNullOrEmpty(jsonExportAppPackages)) this.exportAppPackages = JsonConvert.DeserializeObject<List<string>>(jsonExportAppPackages);
            string jsonExportIndividualPackages = EditorStorage.GetData(KEY_SAVER, "exportIndividualPackages", string.Empty);
            if (!string.IsNullOrEmpty(jsonExportIndividualPackages)) this.exportIndividualPackages = JsonConvert.DeserializeObject<List<DlcInfo>>(jsonExportIndividualPackages);
            this.groupInfoArgs = EditorStorage.GetData(KEY_SAVER, "groupInfoArgs", string.Empty);
            string jsonGroupInfos = EditorStorage.GetData(KEY_SAVER, "groupInfos", string.Empty);
            if (!string.IsNullOrEmpty(jsonGroupInfos)) this.groupInfos = JsonConvert.DeserializeObject<List<GroupInfo>>(jsonGroupInfos);

            this.buildTarget = (BuildTarget)Convert.ToInt32(EditorStorage.GetData(KEY_SAVER, "buildTarget", $"{(int)BuildTarget.StandaloneWindows64}"));
            this.activeBuildTarget = Convert.ToBoolean(EditorStorage.GetData(KEY_SAVER, "activeBuildTarget", "true"));

            this.autoReveal = Convert.ToBoolean(EditorStorage.GetData(KEY_SAVER, "autoReveal", "true"));

            // Preset Bundle Plans
            string jsonBundlePlans = EditorStorage.GetData(KEY_SAVER, "bundlePlans", string.Empty);
            if (!string.IsNullOrEmpty(jsonBundlePlans)) this.bundlePlans = JsonConvert.DeserializeObject<List<BundlePlan>>(jsonBundlePlans);
            this._choicePlanIndex = Convert.ToInt32(EditorStorage.GetData(KEY_SAVER, "_choicePlanIndex", "0"));
        }

        private void OnGUI()
        {
            // operation type area
            EditorGUI.BeginChangeCheck();
            this.operationType = (OperationType)EditorGUILayout.EnumPopup("Operation Type", this.operationType);
            if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(KEY_SAVER, "operationType", ((int)this.operationType).ToString());
            this._OperationType(this.operationType);
        }

        private OperationType _lastOperationType;
        private void _OperationType(OperationType operationType)
        {
            switch (operationType)
            {
                case OperationType.ExportAppConfigToStreamingAssets:
                    this._DrawExportAppConfigToStreamingAssetsView();
                    if (this._lastOperationType != OperationType.ExportAppConfigToStreamingAssets)
                    {
                        float minHeight = _windowSize.y - 120f;
                        GetInstance().minSize = new Vector2(_windowSize.x, minHeight);
                        // window 由大變小需要設置 postion
                        GetInstance().position = new Rect(GetInstance().position.position.x, GetInstance().position.position.y, _windowSize.x, minHeight);
                    }
                    break;
                case OperationType.ExportConfigsAndAppBundlesForCDN:
                    this._DrawExportConfigsAndAppBundlesForCDNView();
                    if (this._lastOperationType != OperationType.ExportConfigsAndAppBundlesForCDN)
                    {
                        GetInstance().minSize = new Vector2(_windowSize.x, _windowSize.y);
                    }
                    break;
                case OperationType.ExportAppBundlesWithoutConfigsForCDN:
                    this._DrawExportAppBundlesWithoutConfigsForCDNView();
                    if (this._lastOperationType != OperationType.ExportAppBundlesWithoutConfigsForCDN)
                    {
                        GetInstance().minSize = new Vector2(_windowSize.x, _windowSize.y);
                    }
                    break;
                case OperationType.ExportIndividualDLCBundlesForCDN:
                    this._DrawExportIndividualDlcBundlesForCDNView();
                    if (this._lastOperationType != OperationType.ExportConfigsAndAppBundlesForCDN)
                    {
                        GetInstance().minSize = new Vector2(_windowSize.x, _windowSize.y);
                    }
                    break;
            }

            this._lastOperationType = operationType;
        }

        private void _DrawExportAppConfigToStreamingAssetsView()
        {
            this._scrollview1 = EditorGUILayout.BeginScrollView(this._scrollview1, true, true);

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
            GUILayout.Label(new GUIContent("Export AppConfig To StreamingAssets"), centeredStyle);
            EditorGUILayout.Space();

            // draw here
            this._DrawBuildTargetView();
            this._DrawProductNameTextFieldView();
            this._DrawAppVersionTextFieldView();
            this._DrawProcessButtonView(this.operationType);

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
        }

        private void _DrawExportConfigsAndAppBundlesForCDNView()
        {
            this._scrollview1 = EditorGUILayout.BeginScrollView(this._scrollview1, true, true);

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
            GUILayout.Label(new GUIContent("Export Configs And App Bundles For CDN"), centeredStyle);
            EditorGUILayout.Space();

            // draw here
            this._DrawBuildTargetView();
            this._DrawSourceFolderView();
            this._DrawProductNameTextFieldView();
            this._DrawAppVersionTextFieldView();
            this._DrawExportFolderView();
            this._DrawExportAppPackagesView();
            this._DrawExportIndividualPackagesView();
            this._DrawGroupInfosView();
            this._DrawProcessButtonView(this.operationType);

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();

            this._DrawBundlePlansView();
        }

        private void _DrawExportAppBundlesWithoutConfigsForCDNView()
        {
            this._scrollview1 = EditorGUILayout.BeginScrollView(this._scrollview1, true, true);

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
            GUILayout.Label(new GUIContent("Export App Bundles Without Configs For CDN"), centeredStyle);
            EditorGUILayout.Space();

            // draw here
            this._DrawBuildTargetView();
            this._DrawSourceFolderView();
            this._DrawProductNameTextFieldView();
            this._DrawAppVersionTextFieldView();
            this._DrawExportFolderView();
            this._DrawExportAppPackagesView();
            this._DrawExportIndividualPackagesView();
            this._DrawProcessButtonView(this.operationType);

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();

            this._DrawBundlePlansView();
        }

        private void _DrawExportIndividualDlcBundlesForCDNView()
        {
            this._scrollview1 = EditorGUILayout.BeginScrollView(this._scrollview1, true, true);

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
            GUILayout.Label(new GUIContent("Export Individual DLC Bundles For CDN"), centeredStyle);
            EditorGUILayout.Space();

            // draw here
            this._DrawBuildTargetView();
            this._DrawSourceFolderView();
            this._DrawProductNameTextFieldView();
            this._DrawExportFolderView();
            this._DrawExportIndividualPackagesView();
            this._DrawProcessButtonView(this.operationType);

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();

            this._DrawBundlePlansView();
        }

        private void _DrawSourceFolderView()
        {
            EditorGUILayout.Space();

            // source folder area
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            this.sourceFolder[(int)this.operationType] = EditorGUILayout.TextField("Source Folder (YooAsset)", this.sourceFolder[(int)this.operationType]);
            if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(KEY_SAVER, $"sourceFolder{(int)this.operationType}", this.sourceFolder[(int)this.operationType]);
            Color bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(0, 255, 128, 255);
            if (GUILayout.Button("Open", GUILayout.MaxWidth(100f))) BundleUtility.OpenFolder(this.sourceFolder[(int)this.operationType], true);
            GUI.backgroundColor = bc;
            bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(83, 152, 255, 255);
            if (GUILayout.Button("Browse", GUILayout.MaxWidth(100f))) this._OpenSourceFolder();
            GUI.backgroundColor = bc;
            EditorGUILayout.EndHorizontal();
        }

        private void _DrawExportFolderView()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            this.exportFolder[(int)this.operationType] = EditorGUILayout.TextField("Export Folder", this.exportFolder[(int)this.operationType]);
            if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(KEY_SAVER, $"exportFolder{(int)this.operationType}", this.exportFolder[(int)this.operationType]);
            Color bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(0, 255, 128, 255);
            if (GUILayout.Button("Open", GUILayout.MaxWidth(100f))) BundleUtility.OpenFolder(this.exportFolder[(int)this.operationType], true);
            GUI.backgroundColor = bc;
            bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(83, 152, 255, 255);
            if (GUILayout.Button("Browse", GUILayout.MaxWidth(100f))) this._OpenExportFolder();
            GUI.backgroundColor = bc;
            EditorGUILayout.EndHorizontal();
        }

        private void _DrawBuildTargetView()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            if (!this.activeBuildTarget)
            {
                // build target type
                EditorGUI.BeginChangeCheck();
                this.buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("Build Target", this.buildTarget);
                if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(KEY_SAVER, "buildTarget", ((int)this.operationType).ToString());
            }

            // active build target toggle
            this.activeBuildTarget = GUILayout.Toggle(this.activeBuildTarget, new GUIContent("Use Active Build Target", "If checked, will use active build target."));
            EditorStorage.SaveData(KEY_SAVER, "activeBuildTarget", this.activeBuildTarget.ToString());

            EditorGUILayout.EndHorizontal();
        }

        private void _DrawProductNameTextFieldView()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            this.productName = EditorGUILayout.TextField("Product Name", this.productName);
            if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(KEY_SAVER, "productName", this.productName);

            // Sync button
            Color bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(255, 238, 36, 255);
            if (GUILayout.Button("Sync to Player Settings", GUILayout.MaxWidth(202f)))
            {
                bool confirmation = EditorUtility.DisplayDialog
                (
                    $"Sync Product Name Notification",
                    $"Do you want to sync Product Name: {this.productName} to Player Settings?",
                    "yes",
                    "cancel"
                );

                if (confirmation) PlayerSettings.productName = this.productName;
            }
            GUI.backgroundColor = bc;
            EditorGUILayout.EndHorizontal();
        }

        private void _DrawAppVersionTextFieldView()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            this.appVersion = EditorGUILayout.TextField("App Version", this.appVersion);
            if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(KEY_SAVER, "appVersion", this.appVersion);

            // Sync button
            Color bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(255, 238, 36, 255);
            if (GUILayout.Button("Sync to Player Settings", GUILayout.MaxWidth(202f)))
            {
                bool confirmation = EditorUtility.DisplayDialog
                (
                    $"Sync App Version Notification",
                    $"Do you want to sync App Version: {this.appVersion} to Player Settings?",
                    "yes",
                    "cancel"
                );

                if (confirmation) PlayerSettings.bundleVersion = this.appVersion;
            }
            GUI.backgroundColor = bc;
            EditorGUILayout.EndHorizontal();
        }

        private void _DrawExportAppPackagesView(string title = "Export App Packages")
        {
            EditorGUILayout.Space();

            var centeredStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
            centeredStyle.alignment = TextAnchor.UpperCenter;
            GUILayout.Label(new GUIContent(title), centeredStyle);
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            this._serObj.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(this._exportAppPackagesPty, true);
            if (EditorGUI.EndChangeCheck())
            {
                this._serObj.ApplyModifiedProperties();
                string json = JsonConvert.SerializeObject(this.exportAppPackages);
                EditorStorage.SaveData(KEY_SAVER, "exportAppPackages", json);
            }

            Color bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(255, 151, 240, 255);
            if (GUILayout.Button("Reset", GUILayout.MaxWidth(100f)))
            {
                this.exportAppPackages = new List<string>() { "DefaultPackage" };
                string json = JsonConvert.SerializeObject(this.exportAppPackages);
                EditorStorage.SaveData(KEY_SAVER, "exportAppPackages", json);
            }
            GUI.backgroundColor = bc;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void _DrawExportIndividualPackagesView(string title = "Export Individual DLC Packages")
        {
            EditorGUILayout.Space();

            var centeredStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
            centeredStyle.alignment = TextAnchor.UpperCenter;
            GUILayout.Label(new GUIContent(title), centeredStyle);
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            this._serObj.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(this._exportIndividualPackagesPty, true);
            if (EditorGUI.EndChangeCheck())
            {
                this._serObj.ApplyModifiedProperties();
                string json = JsonConvert.SerializeObject(this.exportIndividualPackages);
                EditorStorage.SaveData(KEY_SAVER, "exportIndividualPackages", json);
            }

            Color bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(255, 80, 106, 255);
            if (GUILayout.Button("Clear", GUILayout.MaxWidth(100f)))
            {
                this.exportIndividualPackages = new List<DlcInfo>();
                string json = JsonConvert.SerializeObject(this.exportIndividualPackages);
                EditorStorage.SaveData(KEY_SAVER, "exportIndividualPackages", json);
            }
            GUI.backgroundColor = bc;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private bool _isParsingDirty = false;
        private bool _isConvertDirty = false;
        private void _DrawGroupInfosView()
        {
            EditorGUILayout.Space();

            var centeredStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
            centeredStyle.alignment = TextAnchor.UpperCenter;
            GUILayout.Label(new GUIContent("Group Tags For Preset Packages (Main Download)"), centeredStyle);
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            string placeholder = "g1,t1#g2,t1,t2#g3,t1,t2,t3";
            this.groupInfoArgs = EditorGUILayout.TextField(new GUIContent("Group Info Args", $"Group split by '#' and Args split by ','\n{placeholder}"), this.groupInfoArgs);
            if (EditorGUI.EndChangeCheck())
            {
                this._isParsingDirty = true;
                EditorStorage.SaveData(KEY_SAVER, "groupInfoArgs", this.groupInfoArgs);
            }
            Color bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(255, 151, 240, 255);
            if (GUILayout.Button("Reset", GUILayout.MaxWidth(100f)))
            {
                this._isParsingDirty = true;
                this.groupInfoArgs = string.Empty;
                EditorStorage.SaveData(KEY_SAVER, "groupInfoArgs", this.groupInfoArgs);
            }
            GUI.backgroundColor = bc;
            bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(0, 255, 128, 255);
            EditorGUI.BeginDisabledGroup(!this._isParsingDirty);
            if (GUILayout.Button("Parsing", GUILayout.MaxWidth(100f)))
            {
                this.groupInfos = BundleHelper.ParsingGroupInfosByArgs(this.groupInfoArgs);
                string json = JsonConvert.SerializeObject(this.groupInfos);
                EditorStorage.SaveData(KEY_SAVER, "groupInfos", json);
                this._isParsingDirty = false;
                this._isConvertDirty = false;
            }
            GUI.backgroundColor = bc;
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            this._serObj.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(this._groupInfosPty, true);
            if (EditorGUI.EndChangeCheck())
            {
                this._isConvertDirty = true;
                this._serObj.ApplyModifiedProperties();
                string json = JsonConvert.SerializeObject(this.groupInfos);
                EditorStorage.SaveData(KEY_SAVER, "groupInfos", json);
            }
            GUI.backgroundColor = bc;
            bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(255, 80, 106, 255);
            if (GUILayout.Button("Clear", GUILayout.MaxWidth(100f)))
            {
                this.groupInfos = new List<GroupInfo>();
                this._isParsingDirty = true;
                string json = JsonConvert.SerializeObject(this.groupInfos);
                EditorStorage.SaveData(KEY_SAVER, "groupInfos", json);
            }
            GUI.backgroundColor = bc;

            EditorGUI.BeginDisabledGroup(!this._isConvertDirty);
            bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(0, 255, 128, 255);
            if (GUILayout.Button("Convert", GUILayout.MaxWidth(100f)))
            {
                this.groupInfoArgs = BundleHelper.ConvertGroupInfosToArgs(this.groupInfos);
                EditorStorage.SaveData(KEY_SAVER, "groupInfoArgs", this.groupInfoArgs);
                this._isConvertDirty = false;
            }
            GUI.backgroundColor = bc;
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void _DrawProcessButtonView(OperationType operationType)
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            // auto reveal toggle
            this.autoReveal = GUILayout.Toggle(this.autoReveal, new GUIContent("Auto Reveal", "If checked, after process will reveal folder."));
            EditorStorage.SaveData(KEY_SAVER, "autoReveal", this.autoReveal.ToString());

            string inputPath;
            string outputPath;

            // process button
            Color bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(255, 185, 83, 255);
            if (GUILayout.Button("Process", GUILayout.MaxWidth(100f)))
            {
                switch (operationType)
                {
                    case OperationType.ExportAppConfigToStreamingAssets:
                        outputPath = Application.streamingAssetsPath;
                        BundleHelper.ExportAppConfig(this.productName, this.appVersion, outputPath, this.activeBuildTarget, this.buildTarget);
                        EditorUtility.DisplayDialog("Process Message", "Export AppConfig To StreamingAssets.", "OK");
                        AssetDatabase.Refresh();
                        string appCfgFileName = $"{PatchSetting.setting.appCfgName}{PatchSetting.appCfgExtension}";
                        if (this.autoReveal) EditorUtility.RevealInFinder($"{outputPath}/{appCfgFileName}");
                        break;
                    case OperationType.ExportConfigsAndAppBundlesForCDN:
                        {
                            inputPath = this.sourceFolder[(int)this.operationType];
                            outputPath = $"{this.exportFolder[(int)this.operationType]}/{PatchSetting.setting.rootFolderName}";
                            //List<string> exportDlcPackages = new List<string>();
                            //foreach (var dlcPackage in this.exportIndividualPackages) exportDlcPackages.Add(dlcPackage.packageName);
                            //string[] exportPackages = this.exportAppPackages.Union(exportDlcPackages).ToArray();
                            BundleHelper.ExportConfigsAndAppBundles(inputPath, outputPath, this.productName, this.appVersion, this.groupInfos, this.exportAppPackages.ToArray(), this.activeBuildTarget, this.buildTarget, true);
                            BundleHelper.ExportIndividualDlcBundles(inputPath, outputPath, this.productName, this.exportIndividualPackages, this.activeBuildTarget, this.buildTarget, false);
                            EditorUtility.DisplayDialog("Process Message", "Export Configs And App Bundles For CDN.", "OK");
                            if (this.autoReveal) EditorUtility.RevealInFinder(outputPath);
                        }
                        break;
                    case OperationType.ExportAppBundlesWithoutConfigsForCDN:
                        {
                            inputPath = this.sourceFolder[(int)this.operationType];
                            outputPath = $"{this.exportFolder[(int)this.operationType]}/{PatchSetting.setting.rootFolderName}";
                            //List<string> exportDlcPackages = new List<string>();
                            //foreach (var dlcPackage in this.exportIndividualPackages) exportDlcPackages.Add(dlcPackage.packageName);
                            //string[] exportPackages = this.exportAppPackages.Union(exportDlcPackages).ToArray();
                            BundleHelper.ExportAppBundles(inputPath, outputPath, this.productName, this.appVersion, this.exportAppPackages.ToArray(), this.activeBuildTarget, this.buildTarget, true);
                            BundleHelper.ExportIndividualDlcBundles(inputPath, outputPath, this.productName, this.exportIndividualPackages, this.activeBuildTarget, this.buildTarget, false);
                            EditorUtility.DisplayDialog("Process Message", "Export App Bundles For CDN Without Configs.", "OK");
                            if (this.autoReveal) EditorUtility.RevealInFinder(outputPath);
                        }
                        break;
                    case OperationType.ExportIndividualDLCBundlesForCDN:
                        inputPath = this.sourceFolder[(int)this.operationType];
                        outputPath = $"{this.exportFolder[(int)this.operationType]}/{PatchSetting.setting.rootFolderName}";
                        BundleHelper.ExportIndividualDlcBundles(inputPath, outputPath, this.productName, this.exportIndividualPackages, this.activeBuildTarget, this.buildTarget, true);
                        EditorUtility.DisplayDialog("Process Message", "Export Individual DLC Bundles For CDN.", "OK");
                        if (this.autoReveal) EditorUtility.RevealInFinder(outputPath);
                        break;
                }
            }
            GUI.backgroundColor = bc;

            EditorGUILayout.EndHorizontal();
        }

        #region Preset Bundle Plans
        private void _DrawBundlePlansView()
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
            GUILayout.Label(new GUIContent("Preset Bundle Plans"), centeredStyle);
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            // Add popup selection
            List<string> planNames = new List<string>();
            if (this.bundlePlans.Count > 0)
            {
                foreach (var bundlePlan in this.bundlePlans)
                {
                    planNames.Add(bundlePlan.planName);
                }
            }
            EditorGUI.BeginChangeCheck();
            this._choicePlanIndex = EditorGUILayout.Popup("Plan Selection", this._choicePlanIndex, planNames.ToArray());
            if (this._choicePlanIndex < 0) this._choicePlanIndex = 0;
            if (EditorGUI.EndChangeCheck())
            {
                EditorStorage.SaveData(KEY_SAVER, "_choicePlanIndex", this._choicePlanIndex.ToString());
            }

            // Load selection button
            Color bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(83, 152, 255, 255);
            if (GUILayout.Button("Copy Current", GUILayout.MaxWidth(100f)))
            {
                bool confirmation = EditorUtility.DisplayDialog
                (
                    $"Copy Current Notification",
                    $"The plan selection is [{this.bundlePlans[this._choicePlanIndex].planName}]\nDo you want to copy current all values?",
                    "copy current and override",
                    "cancel"
                );

                if (confirmation) this._CopyCurrentToBundlePlan();
            }
            GUI.backgroundColor = bc;
            bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(0, 255, 128, 255);
            if (GUILayout.Button("Load Plan", GUILayout.MaxWidth(100f)))
            {
                this._LoadBundlePlanToCurrent();
            }
            GUI.backgroundColor = bc;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            this._serObj.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(this._bundlePlansPty, true);
            if (EditorGUI.EndChangeCheck())
            {
                this._serObj.ApplyModifiedProperties();
                string json = JsonConvert.SerializeObject(this.bundlePlans);
                EditorStorage.SaveData(KEY_SAVER, "bundlePlans", json);
            }

            bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(255, 151, 240, 255);
            if (GUILayout.Button("Reset", GUILayout.MaxWidth(100f)))
            {
                bool confirmation = EditorUtility.DisplayDialog
                (
                    $"Reset Bundle Plans Notification",
                    $"Do you want to reset bundle plans?",
                    "reset",
                    "cancel"
                );

                if (confirmation)
                {
                    // Reset bundle plans
                    this.bundlePlans = new List<BundlePlan>() { new BundlePlan() };
                    string json = JsonConvert.SerializeObject(this.bundlePlans);
                    EditorStorage.SaveData(KEY_SAVER, "bundlePlans", json);

                    // Reset index
                    this._choicePlanIndex = 0;
                    EditorStorage.SaveData(KEY_SAVER, "_choicePlanIndex", this._choicePlanIndex.ToString());
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
                this._ExportBundlePlans();
            }
            GUI.backgroundColor = bc;
            // Load File
            bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(0, 249, 255, 255);
            if (GUILayout.Button("Load File", GUILayout.MaxWidth(100f)))
            {
                this._ImportBundlePlans();
            }
            GUI.backgroundColor = bc;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
        }
        #endregion

        private void _OpenSourceFolder()
        {
            string folderPath = EditorStorage.GetData(KEY_SAVER, $"sourceFolder{(int)this.operationType}", Application.dataPath);
            this.sourceFolder[(int)this.operationType] = EditorUtility.OpenFolderPanel("Open Source Folder", folderPath, string.Empty);
            if (!string.IsNullOrEmpty(this.sourceFolder[(int)this.operationType])) EditorStorage.SaveData(KEY_SAVER, $"sourceFolder{(int)this.operationType}", this.sourceFolder[(int)this.operationType]);
        }

        private void _OpenExportFolder()
        {
            string folderPath = EditorStorage.GetData(KEY_SAVER, $"exportFolder{(int)this.operationType}", Application.dataPath);
            this.exportFolder[(int)this.operationType] = EditorUtility.OpenFolderPanel("Open Export Folder", folderPath, string.Empty);
            if (!string.IsNullOrEmpty(this.exportFolder[(int)this.operationType])) EditorStorage.SaveData(KEY_SAVER, $"exportFolder{(int)this.operationType}", this.exportFolder[(int)this.operationType]);
        }

        private void _LoadBundlePlanToCurrent()
        {
            if (this.bundlePlans.Count == 0) return;

            var bundlePlan = this.bundlePlans[this._choicePlanIndex];

            this.exportAppPackages = bundlePlan.appPackages.ToList();
            this.groupInfoArgs = bundlePlan.groupInfoArgs;
            this.groupInfos = bundlePlan.groupInfos.ToList();
            this.exportIndividualPackages = bundlePlan.individualPackages.ToList();

            // Save
            string json = JsonConvert.SerializeObject(this.exportAppPackages);
            EditorStorage.SaveData(KEY_SAVER, "exportAppPackages", json);
            EditorStorage.SaveData(KEY_SAVER, "groupInfoArgs", this.groupInfoArgs);
            json = JsonConvert.SerializeObject(this.groupInfos);
            EditorStorage.SaveData(KEY_SAVER, "groupInfos", json);
            json = JsonConvert.SerializeObject(this.exportIndividualPackages);
            EditorStorage.SaveData(KEY_SAVER, "exportIndividualPackages", json);
        }

        private void _CopyCurrentToBundlePlan()
        {
            if (this.bundlePlans.Count == 0) return;

            var bundlePlan = this.bundlePlans[this._choicePlanIndex];

            // Copy
            bundlePlan.appPackages = this.exportAppPackages.ToList();
            bundlePlan.groupInfoArgs = this.groupInfoArgs;
            bundlePlan.groupInfos = this.groupInfos.ToList();
            bundlePlan.individualPackages = this.exportIndividualPackages.ToList();

            this.bundlePlans[this._choicePlanIndex] = bundlePlan;

            // Save
            string json = JsonConvert.SerializeObject(this.bundlePlans);
            EditorStorage.SaveData(KEY_SAVER, "bundlePlans", json);
        }

        private void _ExportBundlePlans()
        {
            string savePath = EditorStorage.GetData(KEY_SAVER, $"bundlePlanFIlePath", Application.dataPath);
            var filePath = EditorUtility.SaveFilePanel("Save Bundle Plan Json File", savePath, "BundlePlan", "json");

            if (!string.IsNullOrEmpty(filePath))
            {
                EditorStorage.SaveData(KEY_SAVER, $"bundlePlanFIlePath", Path.GetDirectoryName(filePath));
                string json = JsonConvert.SerializeObject(this.bundlePlans, Formatting.Indented);
                BundleHelper.WriteTxt(json, filePath);
            }
        }

        private void _ImportBundlePlans()
        {
            string loadPath = EditorStorage.GetData(KEY_SAVER, $"bundlePlanFIlePath", Application.dataPath);
            string filePath = EditorUtility.OpenFilePanel("Select Bundle Plan Json File", !string.IsNullOrEmpty(loadPath) ? loadPath : Application.dataPath, "json");

            if (!string.IsNullOrEmpty(filePath))
            {
                EditorStorage.SaveData(KEY_SAVER, $"bundlePlanFIlePath", Path.GetDirectoryName(filePath));
                string json = File.ReadAllText(filePath);
                this.bundlePlans = JsonConvert.DeserializeObject<List<BundlePlan>>(json);

                // Resave bundle plans without format
                json = JsonConvert.SerializeObject(this.bundlePlans);
                EditorStorage.SaveData(KEY_SAVER, "bundlePlans", json);

                // Reset index
                this._choicePlanIndex = 0;
                EditorStorage.SaveData(KEY_SAVER, "_choicePlanIndex", this._choicePlanIndex.ToString());
            }
        }
    }
}