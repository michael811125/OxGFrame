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

public class BundleConfigGeneratorWindow : EditorWindow
{
    public enum OperationType
    {
        ExportAppConfigToStreamingAssets = 0,
        ExportAppConfigAndBundlesForCDN = 1
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
    public string groupInfoArgs;
    [SerializeField]
    public List<GroupInfo> groupInfos;
    [SerializeField]
    public bool autoReveal;

    private Vector2 _scrollview;

    private SerializedObject _serObj;
    private SerializedProperty _groupInfosPty;

    internal const string KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR = "KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR";

    private static Vector2 _windowSize = new Vector2(800f, 325f);

    [MenuItem(BundleHelper.MenuRoot + "Bundle Config Generator", false, 899)]
    public static void ShowWindow()
    {
        _instance = null;
        GetInstance().titleContent = new GUIContent("Bundle Config Generator");
        GetInstance().Show();
        GetInstance().minSize = _windowSize;
    }

    private void OnEnable()
    {
        this._serObj = new SerializedObject(this);
        this._groupInfosPty = this._serObj.FindProperty("groupInfos");

        int operationTypeCount = Enum.GetNames(typeof(OperationType)).Length;
        for (int i = 0; i < operationTypeCount; i++)
        {
            this.sourceFolder[i] = EditorStorage.GetData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, $"sourceFolder{i}", Path.Combine($"{Application.dataPath}/", AssetBundleBuilderHelper.GetDefaultOutputRoot()));
            this.exportFolder[i] = EditorStorage.GetData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, $"exportFolder{i}", Path.Combine($"{Application.dataPath}/", $"{EditorTools.GetProjectPath()}/ExportBundles"));
        }

        this.operationType = (OperationType)Convert.ToInt32(EditorStorage.GetData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, "operationType", "0"));
        this.productName = EditorStorage.GetData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, "productName", Application.productName);
        this.appVersion = EditorStorage.GetData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, "appVersion", Application.version);
        this.groupInfoArgs = EditorStorage.GetData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, "groupInfoArgs", string.Empty);
        string jsonGroupInfos = EditorStorage.GetData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, "groupInfos", string.Empty);
        if (!string.IsNullOrEmpty(jsonGroupInfos)) this.groupInfos = JsonConvert.DeserializeObject<List<GroupInfo>>(jsonGroupInfos);

        this.buildTarget = (BuildTarget)Convert.ToInt32(EditorStorage.GetData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, "buildTarget", $"{(int)BuildTarget.StandaloneWindows64}"));
        this.activeBuildTarget = Convert.ToBoolean(EditorStorage.GetData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, "activeBuildTarget", "true"));

        this.autoReveal = Convert.ToBoolean(EditorStorage.GetData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, "autoReveal", "true"));
    }

    private void OnGUI()
    {
        // operation type area
        EditorGUI.BeginChangeCheck();
        this.operationType = (OperationType)EditorGUILayout.EnumPopup("Operation Type", this.operationType);
        if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, "operationType", ((int)this.operationType).ToString());
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
            case OperationType.ExportAppConfigAndBundlesForCDN:
                this._DrawExportConfigsAndBundlesForCDNView();
                if (this._lastOperationType != OperationType.ExportAppConfigAndBundlesForCDN)
                {
                    GetInstance().minSize = new Vector2(_windowSize.x, _windowSize.y);
                }
                break;
        }

        this._lastOperationType = operationType;
    }

    private void _DrawExportAppConfigToStreamingAssetsView()
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
        GUILayout.Label(new GUIContent("Export AppConfig To StreamingAssets"), centeredStyle);
        EditorGUILayout.Space();

        // draw here
        this._DrawBuildTargetView();
        this._DrawProductNameTextFieldView();
        this._DrawAppVersionTextFieldView();
        this._DrawProcessButtonView(this.operationType);

        EditorGUILayout.EndVertical();
    }

    private void _DrawExportConfigsAndBundlesForCDNView()
    {
        this._scrollview = EditorGUILayout.BeginScrollView(this._scrollview, true, true);

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
        GUILayout.Label(new GUIContent("Export AppConfig And Bundles For CDN"), centeredStyle);
        EditorGUILayout.Space();

        // draw here
        this._DrawBuildTargetView();
        this._DrawSourceFolderView();
        this._DrawProductNameTextFieldView();
        this._DrawAppVersionTextFieldView();
        this._DrawExportFolderView();
        this._DrawGroupInfosView();
        this._DrawProcessButtonView(this.operationType);

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndScrollView();
    }

    private void _DrawSourceFolderView()
    {
        EditorGUILayout.Space();

        // source folder area
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        this.sourceFolder[(int)this.operationType] = EditorGUILayout.TextField("Source Folder (YooAsset)", this.sourceFolder[(int)this.operationType]);
        if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, $"sourceFolder{(int)this.operationType}", this.sourceFolder[(int)this.operationType]);
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
        if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, $"exportFolder{(int)this.operationType}", this.exportFolder[(int)this.operationType]);
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
            if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, "buildTarget", ((int)this.operationType).ToString());
        }

        // active build target toggle
        this.activeBuildTarget = GUILayout.Toggle(this.activeBuildTarget, new GUIContent("Use Active Build Target", "If checked will use active build target."));
        EditorStorage.SaveData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, "activeBuildTarget", this.activeBuildTarget.ToString());

        EditorGUILayout.EndHorizontal();
    }

    private void _DrawProductNameTextFieldView()
    {
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        this.productName = EditorGUILayout.TextField("Product Name", this.productName);
        if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, "productName", this.productName);
        EditorGUILayout.EndHorizontal();
    }

    private void _DrawAppVersionTextFieldView()
    {
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        this.appVersion = EditorGUILayout.TextField("App Version", this.appVersion);
        if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, "appVersion", this.appVersion);
        EditorGUILayout.EndHorizontal();
    }

    private bool _isParsingDirty = false;
    private bool _isConvertDirty = false;
    private void _DrawGroupInfosView()
    {
        EditorGUILayout.Space();

        var centeredStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
        centeredStyle.alignment = TextAnchor.UpperCenter;
        GUILayout.Label(new GUIContent("Group Tags"), centeredStyle);
        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        string placeholder = "g1,t1#g2,t1,t2#g3,t1,t2,t3";
        this.groupInfoArgs = EditorGUILayout.TextField(new GUIContent("Group Info Args", "Group split by '#' and Args split by ','"), string.IsNullOrEmpty(this.groupInfoArgs) ? placeholder : this.groupInfoArgs);
        if (EditorGUI.EndChangeCheck())
        {
            this._isParsingDirty = true;
            EditorStorage.SaveData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, "groupInfoArgs", this.groupInfoArgs);
        }
        Color bc = GUI.backgroundColor;
        GUI.backgroundColor = new Color32(255, 151, 240, 255);
        if (GUILayout.Button("Reset", GUILayout.MaxWidth(100f)))
        {
            this._isParsingDirty = true;
            this.groupInfoArgs = placeholder;
            EditorStorage.SaveData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, "groupInfoArgs", this.groupInfoArgs);
        }
        GUI.backgroundColor = bc;
        bc = GUI.backgroundColor;
        GUI.backgroundColor = new Color32(0, 255, 128, 255);
        EditorGUI.BeginDisabledGroup(!this._isParsingDirty);
        if (GUILayout.Button("Parsing", GUILayout.MaxWidth(100f)))
        {
            this.groupInfos = BundleHelper.ParsingGroupInfosByArgs(this.groupInfoArgs);
            string json = JsonConvert.SerializeObject(this.groupInfos);
            EditorStorage.SaveData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, "groupInfos", json);
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
            EditorStorage.SaveData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, "groupInfos", json);
        }
        GUI.backgroundColor = bc;
        bc = GUI.backgroundColor;
        GUI.backgroundColor = new Color32(255, 80, 106, 255);
        if (GUILayout.Button("Clear", GUILayout.MaxWidth(100f)))
        {
            this.groupInfos = new List<GroupInfo>();
            this._isParsingDirty = true;
        }
        GUI.backgroundColor = bc;

        EditorGUI.BeginDisabledGroup(!this._isConvertDirty);
        bc = GUI.backgroundColor;
        GUI.backgroundColor = new Color32(0, 255, 128, 255);
        if (GUILayout.Button("Convert", GUILayout.MaxWidth(100f)))
        {
            this.groupInfoArgs = BundleHelper.ConvertGroupInfosToArgs(this.groupInfos);
            EditorStorage.SaveData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, "groupInfoArgs", this.groupInfoArgs);
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
        this.autoReveal = GUILayout.Toggle(this.autoReveal, new GUIContent("Auto Reveal", "If checked after process will reveal folder."));
        EditorStorage.SaveData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, "autoReveal", this.autoReveal.ToString());

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
                    if (this.autoReveal) EditorUtility.RevealInFinder($"{outputPath}/{BundleConfig.appCfgName}{BundleConfig.appCfgExtension}");
                    break;
                case OperationType.ExportAppConfigAndBundlesForCDN:
                    inputPath = this.sourceFolder[(int)this.operationType];
                    outputPath = $"{this.exportFolder[(int)this.operationType]}{BundleConfig.rootDir}";
                    BundleHelper.ExportConfigsAndBundles(inputPath, outputPath, this.productName, this.appVersion, this.groupInfos, this.activeBuildTarget, this.buildTarget);
                    EditorUtility.DisplayDialog("Process Message", "Export Bundles And AppConfig For CDN.", "OK");
                    if (this.autoReveal) EditorUtility.RevealInFinder(outputPath);
                    break;
            }
        }
        GUI.backgroundColor = bc;

        EditorGUILayout.EndHorizontal();
    }

    private void _OpenSourceFolder()
    {
        string folderPath = EditorStorage.GetData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, $"sourceFolder{(int)this.operationType}", Application.dataPath);
        this.sourceFolder[(int)this.operationType] = EditorUtility.OpenFolderPanel("Open Source Folder", folderPath, string.Empty);
        if (!string.IsNullOrEmpty(this.sourceFolder[(int)this.operationType])) EditorStorage.SaveData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, $"sourceFolder{(int)this.operationType}", this.sourceFolder[(int)this.operationType]);
    }

    private void _OpenExportFolder()
    {
        string folderPath = EditorStorage.GetData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, $"exportFolder{(int)this.operationType}", Application.dataPath);
        this.exportFolder[(int)this.operationType] = EditorUtility.OpenFolderPanel("Open Export Folder", folderPath, string.Empty);
        if (!string.IsNullOrEmpty(this.exportFolder[(int)this.operationType])) EditorStorage.SaveData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, $"exportFolder{(int)this.operationType}", this.exportFolder[(int)this.operationType]);
    }
}
