using AssetLoader.Bundle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BundleConfigGeneratorEditor : EditorWindow
{
    public enum OperationType
    {
        GenerateConfigToSourceFolder,
        ExportAndConfigFromSourceFolder,
        GenerateConfigToSourceFolderAndOnlyExportSameConfig
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
    public string sourceFolder;
    [SerializeField]
    public string exportFolder;
    [SerializeField]
    public string productName;

    internal const string KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR = "KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR";

    [MenuItem(BundleDistributorEditor.MenuRoot + "Bundle Config Generator", false, 899)]
    public static void ShowWindow()
    {
        _instance = null;
        GetInstance().titleContent = new GUIContent("Bundle Config Generator");
        GetInstance().Show();
        GetInstance().minSize = new Vector2(650f, 175f);
    }

    private void OnEnable()
    {
        this.operationType = (OperationType)Convert.ToInt32(EditorStorage.GetData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, "operationType", "0"));
        this.sourceFolder = EditorStorage.GetData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, "sourceFolder", Path.Combine($"{Application.dataPath}/", $"../AssetBundles"));
        this.exportFolder = EditorStorage.GetData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, "exportFolder", Path.Combine($"{Application.dataPath}/", $"../ExportBundles"));
        this.productName = EditorStorage.GetData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, "productName", Application.productName);
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        this.sourceFolder = EditorGUILayout.TextField("Source Folder", this.sourceFolder);
        if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, "sourceFolder", this.sourceFolder);
        Color bc = GUI.backgroundColor;
        GUI.backgroundColor = new Color32(0, 255, 128, 255);
        if (GUILayout.Button("Open", GUILayout.MaxWidth(100f))) BundleDistributorEditor.OpenFolder(this.sourceFolder, true);
        GUI.backgroundColor = bc;
        bc = GUI.backgroundColor;
        GUI.backgroundColor = new Color32(83, 152, 255, 255);
        if (GUILayout.Button("Browse", GUILayout.MaxWidth(100f))) this._OpenSourceFolder();
        GUI.backgroundColor = bc;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();
        this.operationType = (OperationType)EditorGUILayout.EnumPopup("Operation Type", this.operationType);
        if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, "operationType", ((int)this.operationType).ToString());
        this._OperationType(this.operationType);
    }

    private void _OperationType(OperationType operationType)
    {
        switch (operationType)
        {
            case OperationType.GenerateConfigToSourceFolder:
                this._DrawGenerateConfigToSourceFolderView();
                break;
            case OperationType.ExportAndConfigFromSourceFolder:
                this._DrawExportAndConfigFromSourceFolderView();
                break;
            case OperationType.GenerateConfigToSourceFolderAndOnlyExportSameConfig:
                this._DrawGenerateConfigToSourceFolderAndOnlyExportSameConfigView();
                break;
        }
    }

    private void _DrawGenerateConfigToSourceFolderView()
    {
        EditorGUILayout.Space();

        GUIStyle style = new GUIStyle();
        var bg = new Texture2D(1, 1);
        Color[] pixels = Enumerable.Repeat(new Color(0f, 0.47f, 1f, 0.5f), Screen.width * Screen.height).ToArray();
        bg.SetPixels(pixels);
        bg.Apply();
        style.normal.background = bg;
        EditorGUILayout.BeginVertical(style);
        var centeredStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
        centeredStyle.alignment = TextAnchor.UpperCenter;
        GUILayout.Label(new GUIContent("Generate Config To SourceFolder Settings"), centeredStyle);
        EditorGUILayout.Space();

        this._DrawProcessButton(this.operationType);

        EditorGUILayout.EndVertical();
    }

    private void _DrawExportAndConfigFromSourceFolderView()
    {
        EditorGUILayout.Space();

        GUIStyle style = new GUIStyle();
        var bg = new Texture2D(1, 1);
        Color[] pixels = Enumerable.Repeat(new Color(0f, 0.47f, 1f, 0.5f), Screen.width * Screen.height).ToArray();
        bg.SetPixels(pixels);
        bg.Apply();
        style.normal.background = bg;
        EditorGUILayout.BeginVertical(style);
        var centeredStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
        centeredStyle.alignment = TextAnchor.UpperCenter;
        GUILayout.Label(new GUIContent("Export And Config From SourceFolder Settings"), centeredStyle);
        EditorGUILayout.Space();

        this._DrawProductNameTextField();
        this._DrawExportFolderView();
        this._DrawProcessButton(this.operationType);

        EditorGUILayout.EndVertical();
    }

    private void _DrawGenerateConfigToSourceFolderAndOnlyExportSameConfigView()
    {
        EditorGUILayout.Space();

        GUIStyle style = new GUIStyle();
        var bg = new Texture2D(1, 1);
        Color[] pixels = Enumerable.Repeat(new Color(0f, 0.47f, 1f, 0.5f), Screen.width * Screen.height).ToArray();
        bg.SetPixels(pixels);
        bg.Apply();
        style.normal.background = bg;
        EditorGUILayout.BeginVertical(style);
        var centeredStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
        centeredStyle.alignment = TextAnchor.UpperCenter;
        GUILayout.Label(new GUIContent("Generate Config To SourceFolder And Only Export Same Config Settings"), centeredStyle);
        EditorGUILayout.Space();

        this._DrawProductNameTextField();
        this._DrawExportFolderView();
        this._DrawProcessButton(this.operationType);

        EditorGUILayout.EndVertical();
    }

    private void _DrawExportFolderView()
    {
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        this.exportFolder = EditorGUILayout.TextField("Export Folder", this.exportFolder);
        if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, "exportFolder", this.exportFolder);
        Color bc = GUI.backgroundColor;
        GUI.backgroundColor = new Color32(0, 255, 128, 255);
        if (GUILayout.Button("Open", GUILayout.MaxWidth(100f))) BundleDistributorEditor.OpenFolder(this.exportFolder, true);
        GUI.backgroundColor = bc;
        bc = GUI.backgroundColor;
        GUI.backgroundColor = new Color32(83, 152, 255, 255);
        if (GUILayout.Button("Browse", GUILayout.MaxWidth(100f))) this._OpenExportFolder();
        GUI.backgroundColor = bc;
        EditorGUILayout.EndHorizontal();
    }

    private void _DrawProductNameTextField()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        this.productName = EditorGUILayout.TextField("Product Name", this.productName);
        if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, "productName", this.productName);
        EditorGUILayout.EndHorizontal();
    }

    private void _DrawProcessButton(OperationType operationType)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        Color bc = GUI.backgroundColor;
        GUI.backgroundColor = new Color32(255, 185, 83, 255);
        if (GUILayout.Button("Process", GUILayout.MaxWidth(100f)))
        {
            switch (operationType)
            {
                case OperationType.GenerateConfigToSourceFolder:
                    BundleDistributorEditor.GenerateBundleCfg(this.sourceFolder, this.sourceFolder);
                    EditorUtility.DisplayDialog("Process Message", "Generate Config To SourceFolder.", "OK");
                    break;
                case OperationType.ExportAndConfigFromSourceFolder:
                    BundleDistributorEditor.ExportBundleAndConfig(this.sourceFolder, this.exportFolder, this.productName);
                    EditorUtility.DisplayDialog("Process Message", "Export And Config From SourceFolder.", "OK");
                    break;
                case OperationType.GenerateConfigToSourceFolderAndOnlyExportSameConfig:
                    BundleDistributorEditor.GenerateBundleCfg(this.sourceFolder, this.sourceFolder);
                    string fullExportFolder = $"{this.exportFolder}/{this.productName}";
                    if (Directory.Exists(fullExportFolder)) Directory.Delete(fullExportFolder, true);
                    Directory.CreateDirectory(fullExportFolder);
                    // 來源配置檔路徑
                    string sourceFileName = Path.Combine(this.sourceFolder, BundleConfig.bundleCfgName + BundleConfig.cfgExt);
                    string destFileName = Path.Combine(fullExportFolder, BundleConfig.bundleCfgName + BundleConfig.cfgExt);
                    // 最後將來源配置檔複製至輸出路徑
                    File.Copy(sourceFileName, destFileName);
                    EditorUtility.DisplayDialog("Process Message", "Generate Config To SourceFolder And Only Export Same Config.", "OK");
                    break;
            }
        }
        GUI.backgroundColor = bc;
        EditorGUILayout.EndHorizontal();
    }

    private void _OpenSourceFolder()
    {
        string folderPath = EditorStorage.GetData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, "sourceFolder", Application.dataPath);
        this.sourceFolder = EditorUtility.OpenFolderPanel("Open Source Folder", folderPath, string.Empty);
        if (!string.IsNullOrEmpty(this.sourceFolder)) EditorStorage.SaveData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, "sourceFolder", this.sourceFolder);
    }

    private void _OpenExportFolder()
    {
        string folderPath = EditorStorage.GetData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, "exportFolder", Application.dataPath);
        this.exportFolder = EditorUtility.OpenFolderPanel("Open Export Folder", folderPath, string.Empty);
        if (!string.IsNullOrEmpty(this.exportFolder)) EditorStorage.SaveData(KEY_SAVE_DATA_FOR_GENERATE_BUNDLE_CONFIG_EDITOR, "exportFolder", this.exportFolder);
    }
}
