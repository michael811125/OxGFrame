using AssetLoader.Bundle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CryptogramEditor : EditorWindow
{
    public enum CryptogramType
    {
        OFFSET,
        XOR,
        AES
    }

    private static CryptogramEditor _instance = null;
    internal static CryptogramEditor GetInstance()
    {
        if (_instance == null) _instance = GetWindow<CryptogramEditor>();
        return _instance;
    }

    [SerializeField]
    public CryptogramType cryptogramType;
    [SerializeField]
    public string sourceFolder;
    [SerializeField]
    public bool autoSave = false;

    internal const string KEY_SAVE_DATA_FOR_CRYPTOGRAM_EDITOR = "KEY_SAVE_DATA_FOR_CRYPTOGRAM_EDITOR";

    [MenuItem(BundleDistributorEditor.MenuRoot + "Bundle Cryptogram", false, 799)]
    public static void ShowWindow()
    {
        _instance = null;
        GetInstance().titleContent = new GUIContent("Bundle Cryptogram");
        GetInstance().Show();
        GetInstance().minSize = new Vector2(650f, 175f);
    }

    private void OnEnable()
    {
        this.cryptogramType = (CryptogramType)Convert.ToInt32(EditorStorage.GetData(KEY_SAVE_DATA_FOR_CRYPTOGRAM_EDITOR, "cryptogramType", "0"));
        this.sourceFolder = EditorStorage.GetData(KEY_SAVE_DATA_FOR_CRYPTOGRAM_EDITOR, "sourceFolder", Application.dataPath);
        this.autoSave = Convert.ToBoolean(EditorStorage.GetData(KEY_SAVE_DATA_FOR_CRYPTOGRAM_EDITOR, "autoSave", "false"));
        if (this.autoSave)
        {
            this.randomSeed = Convert.ToInt32(EditorStorage.GetData(KEY_SAVE_DATA_FOR_CRYPTOGRAM_EDITOR, "randomSeed", "1"));
            this.dummySize = Convert.ToInt32(EditorStorage.GetData(KEY_SAVE_DATA_FOR_CRYPTOGRAM_EDITOR, "dummySize", "0"));
            this.xorKey = Convert.ToInt32(EditorStorage.GetData(KEY_SAVE_DATA_FOR_CRYPTOGRAM_EDITOR, "xorKey", "0"));
            this.aesKey = EditorStorage.GetData(KEY_SAVE_DATA_FOR_CRYPTOGRAM_EDITOR, "aesKey", "file_key");
            this.aesIv = EditorStorage.GetData(KEY_SAVE_DATA_FOR_CRYPTOGRAM_EDITOR, "aesIv", "file_vector");
        }
        else this._ClearCryptogramKeyData();
    }

    private void OnDisable()
    {
        base.SaveChanges();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        this.sourceFolder = EditorGUILayout.TextField("Source Folder", this.sourceFolder);
        if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(KEY_SAVE_DATA_FOR_CRYPTOGRAM_EDITOR, "sourceFolder", this.sourceFolder);
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
        EditorGUI.BeginChangeCheck();
        this.cryptogramType = (CryptogramType)EditorGUILayout.EnumPopup("Cryptogram Type", this.cryptogramType);
        if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(KEY_SAVE_DATA_FOR_CRYPTOGRAM_EDITOR, "cryptogramType", ((int)this.cryptogramType).ToString());
        this.autoSave = GUILayout.Toggle(this.autoSave, new GUIContent("Auto Save", "If checked will save cryptogram key data."));
        EditorStorage.SaveData(KEY_SAVE_DATA_FOR_CRYPTOGRAM_EDITOR, "autoSave", this.autoSave.ToString());
        EditorGUILayout.EndHorizontal();

        this._CryptogramType(this.cryptogramType);
    }

    private void _CryptogramType(CryptogramType cryptogramType)
    {
        switch (cryptogramType)
        {
            case CryptogramType.OFFSET:
                this._DrawOffsetView();
                break;
            case CryptogramType.XOR:
                this._DrawXorView();
                break;
            case CryptogramType.AES:
                this._DrawAesView();
                break;
        }
    }

    [SerializeField]
    public int randomSeed = 1;
    [SerializeField]
    public int dummySize = 0;
    private void _DrawOffsetView()
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
        GUILayout.Label(new GUIContent("Offset Settings"), centeredStyle);
        EditorGUILayout.Space();

        this.randomSeed = EditorGUILayout.IntField(new GUIContent("Random Seed", "Fixed random values."), this.randomSeed);
        if (this.randomSeed <= 0) this.randomSeed = 1;
        if (this.autoSave)
        {
            EditorStorage.SaveData(KEY_SAVE_DATA_FOR_CRYPTOGRAM_EDITOR, "randomSeed", this.randomSeed.ToString());
        }

        this.dummySize = EditorGUILayout.IntField(new GUIContent("Offset Dummy Size", "Add dummy bytes into front of file (per byte = Random 0 ~ 255)."), this.dummySize);
        if (this.dummySize < 0) this.dummySize = 0;
        if (this.autoSave)
        {
            EditorStorage.SaveData(KEY_SAVE_DATA_FOR_CRYPTOGRAM_EDITOR, "dummySize", this.dummySize.ToString());
        }

        this._DrawOperateButtons(this.cryptogramType);

        EditorGUILayout.EndVertical();
    }

    [SerializeField]
    public int xorKey = 0;
    private void _DrawXorView()
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
        GUILayout.Label(new GUIContent("XOR Settings"), centeredStyle);
        EditorGUILayout.Space();

        this.xorKey = EditorGUILayout.IntField("XOR KEY (0 ~ 255)", this.xorKey);
        if (this.xorKey < 0) this.xorKey = 0;
        else if (this.xorKey > 255) this.xorKey = 255;
        if (this.autoSave)
        {
            EditorStorage.SaveData(KEY_SAVE_DATA_FOR_CRYPTOGRAM_EDITOR, "xorKey", this.xorKey.ToString());
        }

        this._DrawOperateButtons(this.cryptogramType);

        EditorGUILayout.EndVertical();
    }

    [SerializeField]
    public string aesKey = "file_key";
    [SerializeField]
    public string aesIv = "file_iv";
    private void _DrawAesView()
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
        GUILayout.Label(new GUIContent("AES Settings"), centeredStyle);
        EditorGUILayout.Space();

        this.aesKey = EditorGUILayout.TextField("AES KEY", this.aesKey);
        this.aesIv = EditorGUILayout.TextField("AES IV", this.aesIv);
        if (this.autoSave)
        {
            EditorStorage.SaveData(KEY_SAVE_DATA_FOR_CRYPTOGRAM_EDITOR, "aesKey", this.aesKey);
            EditorStorage.SaveData(KEY_SAVE_DATA_FOR_CRYPTOGRAM_EDITOR, "aesIv", this.aesIv);
        }

        this._DrawOperateButtons(this.cryptogramType);

        EditorGUILayout.EndVertical();
    }

    private void _DrawOperateButtons(CryptogramType cryptogramType)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        Color bc = GUI.backgroundColor;
        GUI.backgroundColor = new Color32(255, 185, 83, 255);
        if (GUILayout.Button("Decrypt", GUILayout.MaxWidth(100f)))
        {
            switch (cryptogramType)
            {
                case CryptogramType.OFFSET:
                    BundleDistributorEditor.OffsetDecryptBundleFiles(this.sourceFolder, this.dummySize);
                    EditorUtility.DisplayDialog("Crytogram Message", "[OFFSET] Decrypt Process.", "OK");
                    break;
                case CryptogramType.XOR:
                    BundleDistributorEditor.XorDecryptBundleFiles(this.sourceFolder, (byte)this.xorKey);
                    EditorUtility.DisplayDialog("Crytogram Message", "[XOR] Decrypt Process.", "OK");
                    break;
                case CryptogramType.AES:
                    if (string.IsNullOrEmpty(this.aesKey) || string.IsNullOrEmpty(this.aesIv))
                    {
                        EditorUtility.DisplayDialog("Crytogram Message", "[AES] KEY or IV is Empty!!! Can't process.", "OK");
                        break;
                    }
                    BundleDistributorEditor.AesDecryptBundleFiles(this.sourceFolder, this.aesKey, this.aesIv);
                    EditorUtility.DisplayDialog("Crytogram Message", "[AES] Decrypt Process.", "OK");
                    break;
            }
        }
        GUI.backgroundColor = bc;

        bc = GUI.backgroundColor;
        GUI.backgroundColor = new Color32(255, 74, 218, 255);
        if (GUILayout.Button("Encrypt", GUILayout.MaxWidth(100f)))
        {
            switch (cryptogramType)
            {
                case CryptogramType.OFFSET:
                    BundleDistributorEditor.OffsetEncryptBundleFiles(this.sourceFolder, this.randomSeed, this.dummySize);
                    EditorUtility.DisplayDialog("Crytogram Message", "[OFFSET] Encrypt Process.", "OK");
                    break;
                case CryptogramType.XOR:
                    BundleDistributorEditor.XorEncryptBundleFiles(this.sourceFolder, (byte)this.xorKey);
                    EditorUtility.DisplayDialog("Crytogram Message", "[XOR] Encrypt Process.", "OK");
                    break;
                case CryptogramType.AES:
                    if (string.IsNullOrEmpty(this.aesKey) || string.IsNullOrEmpty(this.aesIv))
                    {
                        EditorUtility.DisplayDialog("Crytogram Message", "[AES] KEY or IV is Empty!!! Can't process.", "OK");
                        break;
                    }
                    BundleDistributorEditor.AesEncryptBundleFiles(this.sourceFolder, this.aesKey, this.aesIv);
                    EditorUtility.DisplayDialog("Crytogram Message", "[AES] Encrypt Process.", "OK");
                    break;
            }
        }
        GUI.backgroundColor = bc;
        EditorGUILayout.EndHorizontal();
    }

    private void _ClearCryptogramKeyData()
    {
        EditorStorage.DeleteData(KEY_SAVE_DATA_FOR_CRYPTOGRAM_EDITOR, "dummySize");
        EditorStorage.DeleteData(KEY_SAVE_DATA_FOR_CRYPTOGRAM_EDITOR, "xorKey");
        EditorStorage.DeleteData(KEY_SAVE_DATA_FOR_CRYPTOGRAM_EDITOR, "aesKey");
        EditorStorage.DeleteData(KEY_SAVE_DATA_FOR_CRYPTOGRAM_EDITOR, "aesIv");
    }
    private void _OpenSourceFolder()
    {
        string folderPath = EditorStorage.GetData(KEY_SAVE_DATA_FOR_CRYPTOGRAM_EDITOR, "sourceFolder", Application.dataPath);
        this.sourceFolder = EditorUtility.OpenFolderPanel("Open Source Folder", folderPath, string.Empty);
        if (!string.IsNullOrEmpty(this.sourceFolder)) EditorStorage.SaveData(KEY_SAVE_DATA_FOR_CRYPTOGRAM_EDITOR, "sourceFolder", this.sourceFolder);
    }
}
