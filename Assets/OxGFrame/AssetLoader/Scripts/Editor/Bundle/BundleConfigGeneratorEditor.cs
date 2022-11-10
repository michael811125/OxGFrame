using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BundleConfigGeneratorEditor : EditorWindow
{
    public enum OperationType
    {
        GenerateConfigToSourceFolder = 0,
        ExportAndConfigFromSourceFolder = 1,
        GenerateConfigToSourceFolderAndOnlyExportSameConfig = 2
    }

    private static BundleConfigGeneratorEditor _instance = null;
    internal static BundleConfigGeneratorEditor GetInstance()
    {
        if (_instance == null) _instance = GetWindow<BundleConfigGeneratorEditor>();
        return _instance;
    }

    [SerializeField]
    public OperationType operationType;
    [SerializeField]
    public string[] sourceFolder = new string[3];
    [SerializeField]
    public string[] exportFolder = new string[3];
    [SerializeField]
    public string productName;
    [SerializeField]
    public bool compressed;

    internal const string KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR = "KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR";

    private static Vector2 _windowSize = new Vector2(800f, 200f);

    [MenuItem(BundleDistributorEditor.MenuRoot + "Step 3. Bundle Config Generator", false, 899)]
    public static void ShowWindow()
    {
        _instance = null;
        GetInstance().titleContent = new GUIContent("Bundle Config Generator");
        GetInstance().Show();
        GetInstance().minSize = _windowSize;
    }

    private void OnEnable()
    {
        int operationTypeCount = Enum.GetNames(typeof(OperationType)).Length;
        for (int i = 0; i < operationTypeCount; i++)
        {
            this.sourceFolder[i] = EditorStorage.GetData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, $"sourceFolder{i}", Path.Combine($"{Application.dataPath}/", $"../AssetBundles"));
            this.exportFolder[i] = EditorStorage.GetData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, $"exportFolder{i}", Path.Combine($"{Application.dataPath}/", $"../ExportBundles"));
        }

        this.operationType = (OperationType)Convert.ToInt32(EditorStorage.GetData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, "operationType", "0"));
        this.productName = EditorStorage.GetData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, "productName", Application.productName);
        this.compressed = Convert.ToBoolean(EditorStorage.GetData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, "compressed", "false"));
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
            case OperationType.GenerateConfigToSourceFolder:
                this._DrawGenerateConfigToSourceFolderView();
                if (this._lastOperationType != OperationType.GenerateConfigToSourceFolder)
                {
                    float minHeight = _windowSize.y - 55f;
                    GetInstance().minSize = new Vector2(_windowSize.x, minHeight);
                    // window 由大變小需要設置 postion
                    GetInstance().position = new Rect(GetInstance().position.position.x, GetInstance().position.position.y, _windowSize.x, minHeight);
                }
                break;
            case OperationType.ExportAndConfigFromSourceFolder:
                this._DrawExportAndConfigFromSourceFolderView();
                if (this._lastOperationType != OperationType.ExportAndConfigFromSourceFolder)
                {
                    GetInstance().minSize = new Vector2(_windowSize.x, _windowSize.y);
                }
                break;
            case OperationType.GenerateConfigToSourceFolderAndOnlyExportSameConfig:
                this._DrawGenerateConfigToSourceFolderAndOnlyExportSameConfigView();
                if (this._lastOperationType != OperationType.GenerateConfigToSourceFolderAndOnlyExportSameConfig)
                {
                    float minHeight = _windowSize.y - 25f;
                    GetInstance().minSize = new Vector2(_windowSize.x, minHeight);
                    // window 由大變小需要設置 postion
                    if (this._lastOperationType != OperationType.GenerateConfigToSourceFolder) GetInstance().position = new Rect(GetInstance().position.position.x, GetInstance().position.position.y, _windowSize.x, minHeight);
                }
                break;
        }

        this._lastOperationType = operationType;
    }

    private void _DrawGenerateConfigToSourceFolderView()
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
        GUILayout.Label(new GUIContent("Generate Config To SourceFolder Settings"), centeredStyle);
        EditorGUILayout.Space();

        // draw here
        this._DrawSourceFolderView();
        this._DrawProductNameTextFieldView();
        this._DrawProcessButtonView(this.operationType);

        EditorGUILayout.EndVertical();
    }

    private void _DrawExportAndConfigFromSourceFolderView()
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
        GUILayout.Label(new GUIContent("Export And Config From SourceFolder Settings"), centeredStyle);
        EditorGUILayout.Space();

        // draw here
        this._DrawSourceFolderView();
        this._DrawCompressedToggleView();
        this._DrawProductNameTextFieldView();
        this._DrawExportFolderView();
        this._DrawProcessButtonView(this.operationType);

        EditorGUILayout.EndVertical();
    }

    private void _DrawGenerateConfigToSourceFolderAndOnlyExportSameConfigView()
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
        GUILayout.Label(new GUIContent("Generate Config To SourceFolder And Only Export Same Config Settings"), centeredStyle);
        EditorGUILayout.Space();

        // draw here
        this._DrawSourceFolderView();
        this._DrawProductNameTextFieldView();
        this._DrawExportFolderView();
        this._DrawProcessButtonView(this.operationType);

        EditorGUILayout.EndVertical();
    }

    private void _DrawSourceFolderView()
    {
        EditorGUILayout.Space();

        // source folder area
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        this.sourceFolder[(int)this.operationType] = EditorGUILayout.TextField("Source Folder", this.sourceFolder[(int)this.operationType]);
        if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, $"sourceFolder{(int)this.operationType}", this.sourceFolder[(int)this.operationType]);
        Color bc = GUI.backgroundColor;
        GUI.backgroundColor = new Color32(0, 255, 128, 255);
        if (GUILayout.Button("Open", GUILayout.MaxWidth(100f))) BundleDistributorEditor.OpenFolder(this.sourceFolder[(int)this.operationType], true);
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
        if (GUILayout.Button("Open", GUILayout.MaxWidth(100f))) BundleDistributorEditor.OpenFolder(this.exportFolder[(int)this.operationType], true);
        GUI.backgroundColor = bc;
        bc = GUI.backgroundColor;
        GUI.backgroundColor = new Color32(83, 152, 255, 255);
        if (GUILayout.Button("Browse", GUILayout.MaxWidth(100f))) this._OpenExportFolder();
        GUI.backgroundColor = bc;
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

    private void _DrawCompressedToggleView()
    {

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        this.compressed = GUILayout.Toggle(this.compressed, new GUIContent("Bundle Is Compressed", "If checked will after download patch to unzip (must zip bundle first)."));
        EditorStorage.SaveData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, "compressed", this.compressed.ToString());
        EditorGUILayout.EndHorizontal();
    }

    private void _DrawProcessButtonView(OperationType operationType)
    {
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        Color bc = GUI.backgroundColor;
        GUI.backgroundColor = new Color32(255, 185, 83, 255);
        if (GUILayout.Button("Process", GUILayout.MaxWidth(100f)))
        {
            switch (operationType)
            {
                case OperationType.GenerateConfigToSourceFolder:
                    BundleDistributorEditor.GenerateBundleConfig(this.productName, this.sourceFolder[(int)this.operationType], this.sourceFolder[(int)this.operationType]);
                    EditorUtility.DisplayDialog("Process Message", "Generate Config To SourceFolder.", "OK");
                    break;
                case OperationType.ExportAndConfigFromSourceFolder:
                    BundleDistributorEditor.ExportBundleAndConfig(this.sourceFolder[(int)this.operationType], this.exportFolder[(int)this.operationType], this.productName, Convert.ToInt32(this.compressed));
                    EditorUtility.DisplayDialog("Process Message", "Export And Config From SourceFolder.", "OK");
                    break;
                case OperationType.GenerateConfigToSourceFolderAndOnlyExportSameConfig:
                    BundleDistributorEditor.GenerateConfigToSourceFolderAndOnlyExportSameConfig(this.productName, this.sourceFolder[(int)this.operationType], this.exportFolder[(int)this.operationType]);
                    EditorUtility.DisplayDialog("Process Message", "Generate Config To SourceFolder And Only Export Same Config.", "OK");
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
