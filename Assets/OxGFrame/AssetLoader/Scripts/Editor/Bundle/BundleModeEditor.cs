using AssetLoader.Bundle;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BundleModeEditor : EditorWindow
{
    private static BundleModeEditor _instance = null;
    internal static BundleModeEditor GetInstance()
    {
        if (_instance == null) _instance = GetWindow<BundleModeEditor>();
        return _instance;
    }

    [SerializeField]
    public bool enableAssetDatabaseMode = false;
    //[SerializeField]
    //public bool enableBundleLoadFromStream = false;

    internal const string KEY_SAVE_DATA_FOR_BUNDLE_MODE_EDITOR = "KEY_SAVE_DATA_FOR_BUNDLE_MODE_EDITOR";

    [MenuItem(BundleDistributorEditor.MenuRoot + "Bundle Mode", false, 0)]
    public static void ShowWindow()
    {
        _instance = null;
        GetInstance().titleContent = new GUIContent("Bundle Mode");
        GetInstance().Show();
        GetInstance().maxSize = new Vector2(400f, 60f);
        GetInstance().minSize = GetInstance().maxSize;
    }

    private void OnEnable()
    {
        this.enableAssetDatabaseMode = Convert.ToBoolean(EditorStorage.GetData(KEY_SAVE_DATA_FOR_BUNDLE_MODE_EDITOR, "enableAssetDatabaseMode", "true"));
        //this.enableBundleLoadFromStream = Convert.ToBoolean(EditorStorage.GetData(KEY_SAVE_DATA_FOR_BUNDLE_MODE_EDITOR, "enableBundleLoadFromStream", "true"));

        BundleConfig.SaveAssetDatabaseMode(this.enableAssetDatabaseMode);
        //BundleConfig.SaveBundleStreamMode(this.enableBundleLoadFromStream);
    }

    private void OnDisable()
    {
        base.SaveChanges();
    }

    private void OnGUI()
    {
        // ↓↓↓ AssetDatabase Section ↓↓↓
        GUIStyle style = new GUIStyle();
        var bg = new Texture2D(1, 1);
        Color[] pixels = Enumerable.Repeat(new Color(1f, 0.4f, 0.7f, 0.5f), Screen.width * Screen.height).ToArray();
        bg.SetPixels(pixels);
        bg.Apply();
        style.normal.background = bg;
        EditorGUILayout.BeginVertical(style);
        var centeredStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
        centeredStyle.alignment = TextAnchor.UpperCenter;
        GUILayout.Label(new GUIContent("AssetDatabase Mode"), centeredStyle);
        EditorGUILayout.Space();

        this.enableAssetDatabaseMode = GUILayout.Toggle(this.enableAssetDatabaseMode, new GUIContent("Enable AssetDatabase Mode", "If checked will load from AssetDatabase."));
        BundleConfig.SaveAssetDatabaseMode(this.enableAssetDatabaseMode);
        EditorStorage.SaveData(KEY_SAVE_DATA_FOR_BUNDLE_MODE_EDITOR, "enableAssetDatabaseMode", this.enableAssetDatabaseMode.ToString());

        EditorGUILayout.EndVertical();
        // ↑↑↑ AssetDatabase Section ↑↑↑

        //EditorGUILayout.Space();
        //EditorGUILayout.Space();

        // ↓↓↓ Stream Section ↓↓↓
        //style = new GUIStyle();
        //bg = new Texture2D(1, 1);
        //pixels = Enumerable.Repeat(new Color(1f, 0.76f, 0.4f, 0.5f), Screen.width * Screen.height).ToArray();
        //bg.SetPixels(pixels);
        //bg.Apply();
        //style.normal.background = bg;
        //EditorGUILayout.BeginVertical(style);
        //centeredStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
        //centeredStyle.alignment = TextAnchor.UpperCenter;
        //GUILayout.Label(new GUIContent("Bundle Stream Mode"), centeredStyle);
        //EditorGUILayout.Space();

        //this.enableBundleLoadFromStream = GUILayout.Toggle(this.enableBundleLoadFromStream, new GUIContent("Enable Bundle Load From Stream", "If checked will use load from stream, can reduce memory."));
        //BundleConfig.SaveBundleStreamMode(this.enableBundleLoadFromStream);
        //EditorStorage.SaveData(KEY_SAVE_DATA_FOR_BUNDLE_MODE_EDITOR, "enableBundleLoadFromStream", this.enableBundleLoadFromStream.ToString());

        //EditorGUILayout.EndVertical();
        // ↑↑↑ Stream Section ↑↑↑
    }
}
