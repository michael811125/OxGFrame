using System.Linq;
using System;
using UnityEditor;
using UnityEngine;
using System.IO;
using AssetLoader.Zip;
using OxGFrame.AssetLoader.Bundle;
using Cysharp.Threading.Tasks;
using Color = UnityEngine.Color;
using System.Collections.Generic;
using System.Text;
using AssetLoader.Utility;

public class CompressorEditor : EditorWindow
{
    public class DiffMsg
    {
        public string msg { get; private set; }

        private Queue<string> _cache;

        public DiffMsg()
        {
            this._cache = new Queue<string>();
        }

        public void AddDiffMsg(string sMsg, string tMsg)
        {
            string msg = $"\n  <color=#ff005c>● [Diff]</color> <color=#78ffce>Source File Path:</color> {sMsg} <color=#ff005c>!=</color> <color=#ff8700>Config File Path:</color> {tMsg}";
            this._cache.Enqueue(msg);

            this.msg += this._GetString();
        }

        public void AddSameMsg(string sMsg, string tMsg)
        {
            string msg = $"\n  <color=#fffd00>● [Same]</color> <color=#78ffce>Source File Path:</color> {sMsg} <color=#fffd00>==</color> <color=#ff8700>Config File Path:</color> {tMsg}";
            this._cache.Enqueue(msg);

            this.msg += this._GetString();
        }

        public void AddNewMsg(string sMsg)
        {
            string msg = $"\n  <color=#00ff13>● [New]</color> <color=#78ffce>Source File Path:</color> {sMsg}";
            this._cache.Enqueue(msg);

            this.msg += this._GetString();
        }

        private string _GetString()
        {
            StringBuilder strBuilder = new StringBuilder();

            while (this._cache.Count > 0)
            {
                string msg = this._cache.Dequeue();
                strBuilder.Append(msg);
            }

            return strBuilder.ToString();
        }

        public override string ToString()
        {
            return this.msg;
        }

        public void Clear()
        {
            this._cache.Clear();
            this.msg = string.Empty;
        }
    }

    public enum OperationType
    {
        Zip = 0,
        Unzip = 1
    }

    public enum DiffType
    {
        None,
        BrowseToLoadConfig,
        RequestConfigFromServer
    }

    public const string MenuRoot = "BundleCompressor/";

    private static CompressorEditor _instance = null;
    internal static CompressorEditor GetInstance()
    {
        if (_instance == null) _instance = GetWindow<CompressorEditor>();
        return _instance;
    }

    [SerializeField]
    public OperationType operationType;
    [SerializeField]
    public string sourceFolder;
    [SerializeField]
    public string[] exportFolder = new string[2];
    [SerializeField]
    public bool[] isClearExportFolder = new bool[2];
    [SerializeField]
    public string zipName = "abzip";
    [SerializeField]
    public bool md5ForZipName = true;
    [SerializeField]
    public string zipPassword = string.Empty;
    [SerializeField]
    public string unzipPassword = string.Empty;
    [SerializeField]
    public DiffType diffType;
    [SerializeField]
    public string configFilePath;
    [SerializeField]
    public string configFileUrl;
    [SerializeField]
    public string zipFilePath;
    [SerializeField]
    public int unzipBufferSize = 65536;
    [SerializeField]
    public bool autoSave;
    [SerializeField]
    public bool exportIncludingSourceFiles = true;

    private Vector2 _scrollPos;
    private DiffMsg _diffMsg = new DiffMsg();

    private float _progress = 0f;
    private long _actualSize = 0;
    private long _totalSize = 0;

    internal const string KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR = "KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR";

    private static Vector2 _windowSize = new Vector2(800f, 715f);

    private bool _onShowInitView = false;
    private bool _onSwitchInitView = false;

    [MenuItem(BundleDistributorEditor.MenuRoot + "Step 2. Bundle Compressor", false, 799)]
    public static void ShowWindow()
    {
        _instance = null;
        GetInstance().titleContent = new GUIContent("Bundle Compressor");
        GetInstance().Show();
        GetInstance().minSize = _windowSize;
    }

    private void OnEnable()
    {
        this._onShowInitView = false;

        int operationTypeCount = Enum.GetNames(typeof(OperationType)).Length;
        for (int i = 0; i < operationTypeCount; i++)
        {
            this.exportFolder[i] = EditorStorage.GetData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, $"exportFolder{i}", Path.Combine($"{Application.dataPath}/", $"../ExportBundles"));
            this.isClearExportFolder[i] = Convert.ToBoolean(EditorStorage.GetData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, $"isClearExportFolder{i}", "true"));
        }

        this.operationType = (OperationType)Convert.ToInt32(EditorStorage.GetData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, "operationType", "0"));
        this.sourceFolder = EditorStorage.GetData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, "sourceFolder", Path.Combine($"{Application.dataPath}/", $"../AssetBundles"));
        this.autoSave = Convert.ToBoolean(EditorStorage.GetData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, "autoSave", "false"));
        if (this.autoSave)
        {
            this.zipName = EditorStorage.GetData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, "zipName", "abzip");
            this.md5ForZipName = Convert.ToBoolean(EditorStorage.GetData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, "md5ForZipName", "true"));
            this.zipPassword = EditorStorage.GetData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, "zipPassword", string.Empty);
            this.unzipPassword = EditorStorage.GetData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, "unzipPassword", string.Empty);
        }
        this.diffType = (DiffType)Convert.ToInt32(EditorStorage.GetData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, "diffType", "0"));
        this.configFilePath = EditorStorage.GetData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, "configFilePath", Application.dataPath);
        this.configFileUrl = EditorStorage.GetData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, "configFileUrl", $"127.0.0.1/{BundleConfig.recordCfgName}");
        this.zipFilePath = EditorStorage.GetData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, "zipFilePath", Path.Combine($"{Application.dataPath}/", $"../ExportBundles"));
        this.unzipBufferSize = Convert.ToInt32(EditorStorage.GetData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, "unzipBufferSize", "65536"));
        this.exportIncludingSourceFiles = Convert.ToBoolean(EditorStorage.GetData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, "exportIncludingSourceFiles", "true"));
    }

    private void OnDisable()
    {
        this._DoCancel();
    }

    private void OnGUI()
    {
        //Debug.Log("x: " + GetInstance().position.width + ", y: " + GetInstance().position.height);

        EditorGUILayout.BeginHorizontal();

        // operation options area
        EditorGUI.BeginChangeCheck();
        this.operationType = (OperationType)EditorGUILayout.EnumPopup("Operation Type", this.operationType, GUILayout.Width(GetInstance().position.width - 100f));
        if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, "operationType", ((int)this.operationType).ToString());

        // auto save toggle area
        GUILayout.FlexibleSpace();
        this.autoSave = GUILayout.Toggle(this.autoSave, new GUIContent("Auto Save", "If checked will save and zip name and zip password data."));
        EditorStorage.SaveData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, "autoSave", this.autoSave.ToString());

        EditorGUILayout.EndHorizontal();

        this._OperationType(this.operationType);
        this._DrawProgressBarView();
        this._DrawOperateButtons(this.operationType);
    }

    private OperationType _lastOperationType;
    private void _OperationType(OperationType operationType)
    {
        switch (operationType)
        {
            case OperationType.Zip:
                this._DrawZipView();
                if (!this._onShowInitView)
                {
                    if (this.diffType == DiffType.None)
                    {
                        float minHeight = (_windowSize.y / 2f) - 75f;
                        GetInstance().minSize = new Vector2(_windowSize.x, minHeight);
                        // window 由大變小需要設置 postion
                        GetInstance().position = new Rect(GetInstance().position.position.x, GetInstance().position.position.y, _windowSize.x, minHeight);
                    }

                    this._onShowInitView = true;
                }
                else if (this._lastOperationType != OperationType.Zip)
                {
                    if (!this._onSwitchInitView)
                    {
                        if (this._lastDiffType == DiffType.None)
                        {
                            float minHeight = (_windowSize.y / 2f) - 75f;
                            GetInstance().minSize = new Vector2(_windowSize.x, minHeight);
                            // window 由大變小需要設置 postion
                            GetInstance().position = new Rect(GetInstance().position.position.x, GetInstance().position.position.y, _windowSize.x, minHeight);

                            this._onSwitchInitView = true;
                        }
                    }
                    else
                    {
                        // window 由小變大僅設置 minSize 就好
                        GetInstance().minSize = new Vector2(_windowSize.x, _windowSize.y);
                    }
                }
                break;
            case OperationType.Unzip:
                this._DrawUnzipView();
                if (this._lastOperationType != OperationType.Unzip)
                {
                    if (this._lastDiffType == DiffType.None) this._onSwitchInitView = false;

                    float minHeight = (_windowSize.y / 2f) - 75f;
                    GetInstance().minSize = new Vector2(_windowSize.x, minHeight);
                    // window 由大變小需要設置 postion
                    GetInstance().position = new Rect(GetInstance().position.position.x, GetInstance().position.position.y, _windowSize.x, minHeight);
                }
                break;
        }

        this._lastOperationType = operationType;
    }

    private void _DrawOperateButtons(OperationType operationType)
    {
        string executeTxt = string.Empty;
        string cancelTxt = "Cancel";
        switch (operationType)
        {
            case OperationType.Zip:
                switch (this.diffType)
                {
                    case DiffType.BrowseToLoadConfig:
                    case DiffType.RequestConfigFromServer:
                        executeTxt = "Diff And Zip";
                        break;

                    default:
                        executeTxt = "Zip";
                        break;
                }
                break;
            case OperationType.Unzip:
                executeTxt = "Unzip";
                break;
        }

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        Color bc = GUI.backgroundColor;

        // Cancel
        EditorGUI.BeginDisabledGroup(this._progress <= 0f);
        GUI.backgroundColor = new Color32(255, 0, 0, 255);
        if (GUILayout.Button(cancelTxt, GUILayout.MaxWidth(100f)))
        {
            this._DoCancel((result) => EditorUtility.DisplayDialog("Message", result, "OK"));
        }
        GUI.backgroundColor = bc;
        EditorGUI.EndDisabledGroup();

        // Zip or Unzip
        EditorGUI.BeginDisabledGroup(this._progress > 0f);
        GUI.backgroundColor = new Color32(0, 255, 0, 255);
        if (GUILayout.Button(executeTxt, GUILayout.MaxWidth(100f)))
        {
            switch (operationType)
            {
                case OperationType.Zip:
                    this.DoZip((result) => EditorUtility.DisplayDialog("Zip Message", result, "OK")).Forget();
                    break;
                case OperationType.Unzip:
                    this._DoUnzip((result) => EditorUtility.DisplayDialog("Unzip Message", result, "OK")).Forget();
                    break;
            }
        }
        GUI.backgroundColor = bc;
        EditorGUI.EndDisabledGroup();

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private void _DrawZipView()
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
        GUILayout.Label(new GUIContent("Zip Process"), centeredStyle);
        EditorGUILayout.Space();

        // draw zip type area
        this._DrawSourceFolderView();
        this._DrawExportFolderView();
        this._DrawZipNameView();
        this._DrawZipPasswordView();
        this._DrawExportIncludingSourceFilesToggleView();
        this._DrawDiffTypeView();

        EditorGUILayout.EndVertical();
    }

    private void _DrawUnzipView()
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
        GUILayout.Label(new GUIContent("Unzip Process"), centeredStyle);
        EditorGUILayout.Space();

        // draw unzip type area
        this._DrawBrowseToLoadZipView();
        this._DrawExportFolderView();
        this._DrawZipPasswordView();
        this._DrawUnzipBufferSizeView();

        EditorGUILayout.EndVertical();
    }

    private void _DrawSourceFolderView()
    {
        EditorGUILayout.Space();

        // source folder area
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        this.sourceFolder = EditorGUILayout.TextField("Source Folder", this.sourceFolder);
        if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, "sourceFolder", this.sourceFolder);
        Color bc = GUI.backgroundColor;
        GUI.backgroundColor = new Color32(0, 255, 128, 255);
        if (GUILayout.Button("Open", GUILayout.MaxWidth(100f))) BundleDistributorEditor.OpenFolder(this.sourceFolder, true);
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
        this.exportFolder[(int)this.operationType] = EditorGUILayout.TextField("Export Folder", this.exportFolder[(int)this.operationType], GUILayout.Width(GetInstance().position.width - 310f));
        if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, $"exportFolder{(int)this.operationType}", this.exportFolder[(int)this.operationType]);
        Color bc = GUI.backgroundColor;
        GUI.backgroundColor = new Color32(0, 255, 128, 255);
        if (GUILayout.Button("Open", GUILayout.MaxWidth(100f))) BundleDistributorEditor.OpenFolder(this.exportFolder[(int)this.operationType], true);
        GUI.backgroundColor = bc;
        bc = GUI.backgroundColor;
        GUI.backgroundColor = new Color32(83, 152, 255, 255);
        if (GUILayout.Button("Browse", GUILayout.MaxWidth(100f))) this._OpenExportFolder();
        GUI.backgroundColor = bc;
        GUILayout.FlexibleSpace();
        this.isClearExportFolder[(int)this.operationType] = GUILayout.Toggle(this.isClearExportFolder[(int)this.operationType], new GUIContent("Clear Folder", "If checked when export will clear export folder."));
        EditorGUILayout.EndHorizontal();
    }

    private void _DrawZipNameView()
    {
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        this.zipName = EditorGUILayout.TextField(new GUIContent("Zip Name", "Export zip name (with extension or without extension)."), this.zipName, GUILayout.Width(GetInstance().position.width - 140f));
        GUILayout.FlexibleSpace();
        this.md5ForZipName = GUILayout.Toggle(this.md5ForZipName, new GUIContent("MD5 For Zip Name", "If checked will use zip name to make a md5 to replace zip name."));

        if (this.autoSave)
        {
            EditorStorage.SaveData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, "zipName", this.zipName);
            EditorStorage.SaveData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, "md5ForZipName", this.md5ForZipName.ToString());
        }

        EditorGUILayout.EndHorizontal();
    }

    private void _DrawZipPasswordView()
    {
        EditorGUILayout.Space();

        switch (this.operationType)
        {
            case OperationType.Zip:
                {
                    string fieldTxt = "Zip Password";
                    string hintTxt = "Encrypt password for zip.";
                    this.zipPassword = EditorGUILayout.TextField(new GUIContent(fieldTxt, hintTxt), this.zipPassword, GUILayout.Width(GetInstance().position.width - 140f));
                    if (this.autoSave) EditorStorage.SaveData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, "zipPassword", this.zipPassword);
                }
                break;
            case OperationType.Unzip:
                {
                    string fieldTxt = "Unzip Password";
                    string hintTxt = "Decrypt password for unzip.";
                    this.unzipPassword = EditorGUILayout.TextField(new GUIContent(fieldTxt, hintTxt), this.unzipPassword, GUILayout.Width(GetInstance().position.width - 150f));
                    if (this.autoSave) EditorStorage.SaveData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, "unzipPassword", this.unzipPassword);
                }
                break;
        }
    }

    private void _DrawDiffTypeView()
    {
        EditorGUILayout.Space();

        // diff type area
        EditorGUI.BeginChangeCheck();
        this.diffType = (DiffType)EditorGUILayout.EnumPopup("Diff Type", this.diffType);
        if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, "diffType", ((int)this.diffType).ToString());
        this._DiffType(this.diffType);
    }

    private DiffType _lastDiffType;
    private void _DiffType(DiffType diffType)
    {
        switch (diffType)
        {
            case DiffType.None:
                if (this._lastDiffType != DiffType.None)
                {
                    float minHeight = (_windowSize.y / 2f) - 75f;
                    GetInstance().minSize = new Vector2(_windowSize.x, minHeight);
                    // window 由大變小需要設置 postion
                    GetInstance().position = new Rect(GetInstance().position.position.x, GetInstance().position.position.y, _windowSize.x, minHeight);
                }
                break;
            case DiffType.BrowseToLoadConfig:
                this._DrawBrowseToLoadConfigView();
                this._DrawDiffMsgScrollView();
                if (this._lastDiffType != DiffType.BrowseToLoadConfig)
                {
                    // window 由小變大僅設置 minSize 就好
                    GetInstance().minSize = new Vector2(_windowSize.x, _windowSize.y);
                }
                break;
            case DiffType.RequestConfigFromServer:
                this._DrawRequestConfigFromServerView();
                this._DrawDiffMsgScrollView();
                if (this._lastDiffType != DiffType.RequestConfigFromServer)
                {
                    // window 由小變大僅設置 minSize 就好
                    GetInstance().minSize = new Vector2(_windowSize.x, _windowSize.y);
                }
                break;
        }

        this._lastDiffType = diffType;
    }

    private void _DrawExportIncludingSourceFilesToggleView()
    {
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        this.exportIncludingSourceFiles = GUILayout.Toggle(this.exportIncludingSourceFiles, new GUIContent("Export Including Source Files", "If checked result including zip and source files."));
        EditorStorage.SaveData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, "exportIncludingSourceFiles", this.exportIncludingSourceFiles.ToString());
        EditorGUILayout.EndHorizontal();
    }

    private void _DrawDiffMsgScrollView()
    {
        EditorGUILayout.Space(10f);

        EditorGUILayout.BeginVertical();

        #region msg scrollveiw
        // title
        var centeredStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
        centeredStyle.alignment = TextAnchor.UpperCenter;
        GUILayout.Label(new GUIContent("Diff Msg"), centeredStyle);

        // scrollview style
        GUIStyle style = new GUIStyle();
        var bg = new Texture2D(1, 1);
        ColorUtility.TryParseHtmlString("#040049", out Color color);
        Color[] pixels = Enumerable.Repeat(color, Screen.width * Screen.height).ToArray();
        bg.SetPixels(pixels);
        bg.Apply();
        style.normal.background = bg;

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace(); // fill scrollview left

        this._scrollPos = EditorGUILayout.BeginScrollView(this._scrollPos, style, GUILayout.Width(GetInstance().position.width - 20), GUILayout.Height((GetInstance().position.height / 2f) - 20));

        // label style
        style = new GUIStyle();
        style.richText = true;
        style.normal.textColor = Color.white;
        GUILayout.Label(this._diffMsg.ToString(), style);
        EditorGUILayout.EndScrollView();

        GUILayout.FlexibleSpace(); // file scrollview right
        EditorGUILayout.EndHorizontal();
        #endregion

        GUILayout.Space(10f);

        #region buttons
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        Color bc = GUI.backgroundColor;
        GUI.backgroundColor = new Color32(255, 146, 0, 255);
        if (GUILayout.Button("Clear", GUILayout.MaxWidth(100f))) this._diffMsg.Clear();
        GUI.backgroundColor = bc;

        bc = GUI.backgroundColor;
        GUI.backgroundColor = new Color32(255, 90, 173, 255);
        if (GUILayout.Button("Diff Check", GUILayout.MaxWidth(100f))) this._DiffBundleWithConfig().Forget();
        GUI.backgroundColor = bc;
        EditorGUILayout.EndHorizontal();
        #endregion

        EditorGUILayout.EndVertical();
    }

    private void _DrawBrowseToLoadConfigView()
    {
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        // config text field
        EditorGUI.BeginChangeCheck();
        this.configFilePath = EditorGUILayout.TextField("Config File Path", this.configFilePath);
        if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, "configFilePath", this.configFilePath);

        // open btn
        Color bc = GUI.backgroundColor;
        GUI.backgroundColor = new Color32(0, 255, 128, 255);
        if (GUILayout.Button("Open", GUILayout.MaxWidth(100f)))
        {
            string filePath = this.configFilePath;
            string fileName = Path.GetFileName(this.configFilePath);
            if (!string.IsNullOrEmpty(fileName)) filePath = filePath.Replace(fileName, string.Empty);

            BundleDistributorEditor.OpenFolder(filePath, false);
        }
        GUI.backgroundColor = bc;

        // load btn
        bc = GUI.backgroundColor;
        GUI.backgroundColor = new Color32(83, 152, 255, 255);
        if (GUILayout.Button("Load", GUILayout.MaxWidth(100f))) this._OpenBrowseToLoadConfig();
        GUI.backgroundColor = bc;

        EditorGUILayout.EndHorizontal();
    }

    private void _DrawRequestConfigFromServerView()
    {
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        // config text field
        EditorGUI.BeginChangeCheck();
        this.configFileUrl = EditorGUILayout.TextField("Config File URL", this.configFileUrl);
        if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, "configFileUrl", this.configFileUrl);

        EditorGUILayout.EndHorizontal();
    }

    private void _DrawProgressBarView()
    {
        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace(); // fill left

        Rect rect = EditorGUILayout.BeginVertical(GUILayout.Width(GetInstance().position.width - 20));
        EditorGUI.ProgressBar(rect, this._progress, $"{(this._progress * 100f).ToString("f2")}%, {BundleUtility.GetBytesToString((ulong)this._actualSize)} / {BundleUtility.GetBytesToString((ulong)this._totalSize)}");
        GUILayout.Space(20);
        EditorGUILayout.EndVertical();

        GUILayout.FlexibleSpace(); // fill right
        EditorGUILayout.EndHorizontal();
    }

    private void _DrawBrowseToLoadZipView()
    {
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        // zip text field
        EditorGUI.BeginChangeCheck();
        this.zipFilePath = EditorGUILayout.TextField("Zip File Path", this.zipFilePath);
        if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, "zipFilePath", this.zipFilePath);

        // open btn
        Color bc = GUI.backgroundColor;
        GUI.backgroundColor = new Color32(0, 255, 128, 255);
        if (GUILayout.Button("Open", GUILayout.MaxWidth(100f)))
        {
            string filePath = this.zipFilePath;
            string fileName = Path.GetFileName(this.zipFilePath);
            if (!string.IsNullOrEmpty(fileName)) filePath = filePath.Replace(fileName, string.Empty);

            BundleDistributorEditor.OpenFolder(filePath, false);
        }
        GUI.backgroundColor = bc;

        // load btn
        bc = GUI.backgroundColor;
        GUI.backgroundColor = new Color32(83, 152, 255, 255);
        if (GUILayout.Button("Load", GUILayout.MaxWidth(100f))) this._OpenBroseToLoadZip();
        GUI.backgroundColor = bc;

        EditorGUILayout.EndHorizontal();
    }

    private void _DrawUnzipBufferSizeView()
    {
        EditorGUILayout.Space();

        this.unzipBufferSize = EditorGUILayout.IntField(new GUIContent("Unzip Buffer Size", "buffer size more bigger more faster"), this.unzipBufferSize, GUILayout.Width(GetInstance().position.width - 150f));
        if (this.unzipBufferSize < 0) this.unzipBufferSize = 65536;
        else if (this.unzipBufferSize > (1024 * 1024 * 1024)) this.unzipBufferSize = (1024 * 1024 * 1024);
        EditorStorage.SaveData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, "unzipBufferSize", this.unzipBufferSize.ToString());
    }

    private void _OpenSourceFolder()
    {
        string folderPath = EditorStorage.GetData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, "sourceFolder", Application.dataPath);
        this.sourceFolder = EditorUtility.OpenFolderPanel("Open Source Folder", folderPath, string.Empty);
        if (!string.IsNullOrEmpty(this.sourceFolder)) EditorStorage.SaveData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, "sourceFolder", this.sourceFolder);
    }

    private void _OpenExportFolder()
    {
        string folderPath = EditorStorage.GetData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, "exportFolder", Application.dataPath);
        this.exportFolder[(int)this.operationType] = EditorUtility.OpenFolderPanel("Open Export Folder", folderPath, string.Empty);
        if (!string.IsNullOrEmpty(this.exportFolder[(int)this.operationType])) EditorStorage.SaveData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, $"exportFolder{(int)this.operationType}", this.exportFolder[(int)this.operationType]);
    }

    private void _OpenBrowseToLoadConfig()
    {
        string filePath = EditorStorage.GetData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, "configFilePath", Application.dataPath);

        // 判斷檔案是否還存在, 如果不存在則僅保留路徑
        if (!File.Exists(filePath))
        {
            string fileName = Path.GetFileName(filePath);
            if (!string.IsNullOrEmpty(fileName)) filePath = filePath.Replace(fileName, string.Empty);
        }

        // 選擇檔案
        this.configFilePath = EditorUtility.OpenFilePanel("Select Config File", filePath, "");

        // 開啟檔案, 返回檔案路徑 (不為空則儲存)
        if (!string.IsNullOrEmpty(this.configFilePath))
        {
            EditorStorage.SaveData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, "configFilePath", this.configFilePath);
        }
        // 反之, 如果取消開啟檔案, 返回空字串, 則使用原先的 filePath 進行儲存
        else
        {
            // 判斷檔案是否還存在, 如果不存在則僅保留路徑
            if (!File.Exists(filePath))
            {
                string fileName = Path.GetFileName(filePath);
                if (!string.IsNullOrEmpty(fileName)) filePath = filePath.Replace(fileName, string.Empty);
            }

            this.configFilePath = filePath;
            EditorStorage.SaveData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, "configFilePath", filePath);
        }
    }

    private void _OpenBroseToLoadZip()
    {
        string filePath = EditorStorage.GetData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, "zipFilePath", Application.dataPath);

        // 判斷檔案是否還存在, 如果不存在則僅保留路徑
        if (!File.Exists(filePath))
        {
            string fileName = Path.GetFileName(filePath);
            if (!string.IsNullOrEmpty(fileName)) filePath = filePath.Replace(fileName, string.Empty);
        }

        // 選擇檔案
        this.zipFilePath = EditorUtility.OpenFilePanel("Select Zip File", filePath, "");

        // 開啟檔案, 返回檔案路徑 (不為空則儲存)
        if (!string.IsNullOrEmpty(this.zipFilePath))
        {
            EditorStorage.SaveData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, "zipFilePath", this.zipFilePath);
        }
        // 反之, 如果取消開啟檔案, 返回空字串, 則使用原先的 filePath 進行儲存
        else
        {
            // 判斷檔案是否還存在, 如果不存在則僅保留路徑
            if (!File.Exists(filePath))
            {
                string fileName = Path.GetFileName(filePath);
                if (!string.IsNullOrEmpty(fileName)) filePath = filePath.Replace(fileName, string.Empty);
            }

            this.zipFilePath = filePath;
            EditorStorage.SaveData(KEY_SAVE_DATA_FOR_COMPRESS_BUNDLE_EDITOR, "zipFilePath", filePath);
        }
    }

    private async UniTaskVoid _DiffBundleWithConfig()
    {
        VersionFileCfg diffCfg = null;
        switch (this.diffType)
        {
            case DiffType.BrowseToLoadConfig:
                // 取得比對用配置檔 (File Path)
                diffCfg = BundleDistributorEditor.GetConfigFromLocal(this.configFilePath, (errorMsg) => { EditorUtility.DisplayDialog("Config Error", errorMsg, "OK"); });
                break;
            case DiffType.RequestConfigFromServer:
                // 取得比對用配置檔 (File URL)
                diffCfg = await BundleDistributorEditor.GetConfigFromServerAsyncUnityWebRequest(this.configFileUrl, (errorMsg) => { EditorUtility.DisplayDialog("Config Error", errorMsg, "OK"); });
                break;
        }

        // 進行 Diff 比對
        BundleDistributorEditor.DiffBundleWithConfig
        (
            this.sourceFolder,
            diffCfg,
            ref this._diffMsg,
            null,
            (errorMsg) => { EditorUtility.DisplayDialog("Diff Error", errorMsg, "OK"); }
        );
    }

    private async UniTaskVoid DoZip(Action<string> resultHandler = null)
    {
        bool diffResult = false;
        List<string> delFiles = new List<string>();

        // 僅 DiffType != None 的情況才執行比對
        if (this.diffType != DiffType.None)
        {
            VersionFileCfg diffCfg = null;
            switch (this.diffType)
            {
                case DiffType.BrowseToLoadConfig:
                    // 取得比對用配置檔 (File Path)
                    diffCfg = BundleDistributorEditor.GetConfigFromLocal(this.configFilePath, (errorMsg) => { EditorUtility.DisplayDialog("Config Error", errorMsg, "OK"); });
                    break;
                case DiffType.RequestConfigFromServer:
                    // 取得比對用配置檔 (File URL)
                    diffCfg = await BundleDistributorEditor.GetConfigFromServerAsyncUnityWebRequest(this.configFileUrl, (errorMsg) => { EditorUtility.DisplayDialog("Config Error", errorMsg, "OK"); });
                    break;
            }

            // 進行 Diff 比對
            diffResult = BundleDistributorEditor.DiffBundleWithConfig
            (
                this.sourceFolder,
                diffCfg,
                ref this._diffMsg,
                delFiles,
                (errorMsg) => { EditorUtility.DisplayDialog("Diff Error", errorMsg, "OK"); }
            );
        }
        // 如果 DiffType = None (無條件直接 true)
        else diffResult = true;

        // 判斷 Diff 是否成功, 如果有成功則進行 Zip + DelDiff 的程序
        if (diffResult)
        {
            bool zipResult = await BundleDistributorEditor.ZipAndDelDiffAsync
            (
                this.exportIncludingSourceFiles,
                this.sourceFolder,
                this.exportFolder[(int)this.operationType],
                this.zipName,
                this.md5ForZipName,
                this.zipPassword,
                delFiles,
                true,
                this.isClearExportFolder[(int)this.operationType],
                (errorMsg) => { EditorUtility.DisplayDialog("Zip Error", errorMsg, "OK"); },
                (v1, v2, v3) => { this._progress = v1; this._actualSize = v2; this._totalSize = v3; }
            );

            // 返回結果
            if (zipResult)
            {
                if (this.diffType != DiffType.None) resultHandler?.Invoke("[Zip] and [Diff] Completes.");
                else resultHandler?.Invoke("[Zip] Completes.");
            }
            else
            {
                if (this.diffType != DiffType.None) resultHandler?.Invoke("[Zip] and [Diff] Failed!");
                else resultHandler?.Invoke("[Zip] Failed!");
            }

            // 不管結果如何, 都需要重置 Progression 參數 (其中 progress 會用於控制按鈕開關)
            this._progress = this._actualSize = this._totalSize = 0;
        }
    }

    private async UniTaskVoid _DoUnzip(Action<string> resultHandler = null)
    {
        bool unzipResult = await BundleDistributorEditor.UnzipAsync
        (
            this.zipFilePath,
            this.exportFolder[(int)this.operationType],
            this.zipPassword,
            this.unzipBufferSize,
            this.isClearExportFolder[(int)this.operationType],
            (errorMsg) => { EditorUtility.DisplayDialog("Zip Error", errorMsg, "OK"); },
            (v1, v2, v3) => { this._progress = v1; this._actualSize = v2; this._totalSize = v3; }
        );

        // 返回結果
        if (unzipResult)
        {
            resultHandler?.Invoke("[Unzip] Completes.");
        }
        else
        {
            resultHandler?.Invoke("[Unzip] Failed!");
        }

        // 不管結果如何, 都需要重置 Progression 參數 (其中 progress 會用於控制按鈕開關)
        this._progress = this._actualSize = this._totalSize = 0;
    }

    private void _DoCancel(Action<string> resultHandler = null)
    {
        Compressor.CancelAsync();

        resultHandler?.Invoke("Cancel Processing.");
    }
}
