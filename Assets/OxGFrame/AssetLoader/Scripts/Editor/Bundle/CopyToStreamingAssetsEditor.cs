using UnityEditor;
using UnityEngine;

public class CopyToStreamingAssetsEditor : EditorWindow
{
    private static CopyToStreamingAssetsEditor _instance = null;
    internal static CopyToStreamingAssetsEditor GetInstance()
    {
        if (_instance == null) _instance = GetWindow<CopyToStreamingAssetsEditor>();
        return _instance;
    }

    [SerializeField]
    public string sourceFolder;

    internal const string KEY_SAVE_DATA_FOR_COPY_TO_STREAMINGASSETS_EDITOR = "KEY_SAVE_DATA_FOR_COPY_TO_STREAMINGASSETS_EDITOR";

    private static Vector2 _windowSize = new Vector2(800f, 70f);

    [MenuItem(BundleDistributorEditor.MenuRoot + "Step 4. Copy to StreamingAssets", false, 999)]
    public static void ShowWindow()
    {
        _instance = null;
        GetInstance().titleContent = new GUIContent("Copy to StreamingAssets");
        GetInstance().Show();
        GetInstance().minSize = _windowSize;
    }

    private void OnEnable()
    {
        this.sourceFolder = EditorStorage.GetData(KEY_SAVE_DATA_FOR_COPY_TO_STREAMINGASSETS_EDITOR, "sourceFolder", Application.dataPath);
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        this.sourceFolder = EditorGUILayout.TextField("Source Folder", this.sourceFolder);
        if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(KEY_SAVE_DATA_FOR_COPY_TO_STREAMINGASSETS_EDITOR, "sourceFolder", this.sourceFolder);
        Color bc = GUI.backgroundColor;
        GUI.backgroundColor = new Color32(0, 255, 128, 255);
        if (GUILayout.Button("Open", GUILayout.MaxWidth(100f))) BundleDistributorEditor.OpenFolder(this.sourceFolder);
        GUI.backgroundColor = bc;
        bc = GUI.backgroundColor;
        GUI.backgroundColor = new Color32(83, 152, 255, 255);
        if (GUILayout.Button("Browse", GUILayout.MaxWidth(100f))) this._OpenSourceFolder();
        GUI.backgroundColor = bc;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        bc = GUI.backgroundColor;
        GUI.backgroundColor = new Color32(255, 185, 83, 255);
        if (GUILayout.Button("Copy to StreamingAssets", GUILayout.MaxWidth(200f)))
        {
            BundleDistributorEditor.CopyToStreamingAssets(this.sourceFolder);
        }
        GUI.backgroundColor = bc;
        EditorGUILayout.EndHorizontal();
    }

    private void _OpenSourceFolder()
    {
        string folderPath = EditorStorage.GetData(KEY_SAVE_DATA_FOR_COPY_TO_STREAMINGASSETS_EDITOR, "sourceFolder", Application.dataPath);
        this.sourceFolder = EditorUtility.OpenFolderPanel("Open Source Folder", folderPath, string.Empty);
        if (!string.IsNullOrEmpty(this.sourceFolder)) EditorStorage.SaveData(KEY_SAVE_DATA_FOR_COPY_TO_STREAMINGASSETS_EDITOR, "sourceFolder", this.sourceFolder);
    }
}
