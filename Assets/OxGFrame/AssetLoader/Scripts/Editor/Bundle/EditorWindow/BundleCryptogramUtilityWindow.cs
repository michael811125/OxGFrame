using OxGFrame.AssetLoader.Utility;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using YooAsset.Editor;

namespace OxGFrame.AssetLoader.Editor
{
    public class BundleCryptogramUtilityWindow : EditorWindow
    {
        public enum CryptogramType
        {
            OFFSET,
            XOR,
            HT2XOR,
            AES
        }

        private static BundleCryptogramUtilityWindow _instance = null;
        internal static BundleCryptogramUtilityWindow GetInstance()
        {
            if (_instance == null) _instance = GetWindow<BundleCryptogramUtilityWindow>();
            return _instance;
        }

        [SerializeField]
        public CryptogramType cryptogramType;

        [SerializeField]
        public string sourceFolder;

        private CryptogramSetting _setting;

        internal static string PROJECT_PATH;
        internal static string KEY_SAVER;

        private static Vector2 _windowSize = new Vector2(800f, 150f);

        [MenuItem(BundleHelper.MenuRoot + "Bundle Cryptogram Utility (For Verify)", false, 699)]
        public static void ShowWindow()
        {
            PROJECT_PATH = Application.dataPath;
            KEY_SAVER = $"{PROJECT_PATH}_{nameof(BundleCryptogramUtilityWindow)}";

            _instance = null;
            GetInstance().titleContent = new GUIContent("Bundle Cryptogram Utility");
            GetInstance().Show();
            GetInstance().minSize = _windowSize;
        }

        private void OnEnable()
        {
            this._setting = EditorTool.LoadSettingData<CryptogramSetting>();
            this._LoadSettingsData();
            this.sourceFolder = EditorStorage.GetData(KEY_SAVER, $"sourceFolder", Path.Combine($"{Application.dataPath}/", AssetBundleBuilderHelper.GetDefaultBuildOutputRoot()));
            this.cryptogramType = (CryptogramType)Convert.ToInt32(EditorStorage.GetData(KEY_SAVER, "cryptogramType", "0"));
        }

        private void _LoadSettingsData()
        {
            this.randomSeed = this._setting.randomSeed;
            this.dummySize = this._setting.dummySize;
            this.xorKey = this._setting.xorKey;
            this.hXorKey = this._setting.hXorKey;
            this.tXorKey = this._setting.tXorKey;
            this.jXorKey = this._setting.jXorKey;
            this.aesKey = this._setting.aesKey;
            this.aesIv = this._setting.aesIv;
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
            if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(KEY_SAVER, "sourceFolder", this.sourceFolder);
            Color bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(0, 255, 128, 255);
            if (GUILayout.Button("Open", GUILayout.MaxWidth(100f))) BundleUtility.OpenFolder(this.sourceFolder);
            GUI.backgroundColor = bc;
            bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(83, 152, 255, 255);
            if (GUILayout.Button("Browse", GUILayout.MaxWidth(100f))) this._OpenSourceFolder();
            GUI.backgroundColor = bc;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            // cryptogram options area
            EditorGUI.BeginChangeCheck();
            this.cryptogramType = (CryptogramType)EditorGUILayout.EnumPopup("Cryptogram Type", this.cryptogramType);
            if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(KEY_SAVER, "cryptogramType", ((int)this.cryptogramType).ToString());

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
                case CryptogramType.HT2XOR:
                    this._DrawHT2XorView();
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
            ColorUtility.TryParseHtmlString("#1e3836", out Color color);
            Color[] pixels = Enumerable.Repeat(color, Screen.width * Screen.height).ToArray();
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

            this.dummySize = EditorGUILayout.IntField(new GUIContent("Offset Dummy Size", "Add dummy bytes into front of file (per byte = Random 0 ~ 255)."), this.dummySize);
            if (this.dummySize < 0) this.dummySize = 0;

            this._DrawOperateButtonsView(this.cryptogramType);

            EditorGUILayout.EndVertical();
        }

        [SerializeField]
        public int xorKey = 0;
        private void _DrawXorView()
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
            GUILayout.Label(new GUIContent("XOR Settings"), centeredStyle);
            EditorGUILayout.Space();

            this.xorKey = EditorGUILayout.IntField("XOR KEY (0 ~ 255)", this.xorKey);
            if (this.xorKey < 0) this.xorKey = 0;
            else if (this.xorKey > 255) this.xorKey = 255;

            this._DrawOperateButtonsView(this.cryptogramType);

            EditorGUILayout.EndVertical();
        }

        [SerializeField]
        public int hXorKey = 0;
        [SerializeField]
        public int tXorKey = 0;
        [SerializeField]
        public int jXorKey = 0;
        private void _DrawHT2XorView()
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
            GUILayout.Label(new GUIContent("Head-Tail 2 XOR Settings"), centeredStyle);
            EditorGUILayout.Space();

            this.hXorKey = EditorGUILayout.IntField("Head XOR KEY (0 ~ 255)", this.hXorKey);
            if (this.hXorKey < 0) this.hXorKey = 0;
            else if (this.hXorKey > 255) this.hXorKey = 255;
            this.tXorKey = EditorGUILayout.IntField("Tail XOR KEY (0 ~ 255)", this.tXorKey);
            if (this.tXorKey < 0) this.tXorKey = 0;
            else if (this.tXorKey > 255) this.tXorKey = 255;
            this.jXorKey = EditorGUILayout.IntField("Jump XOR KEY (0 ~ 255)", this.jXorKey);
            if (this.jXorKey < 0) this.jXorKey = 0;
            else if (this.jXorKey > 255) this.jXorKey = 255;

            this._DrawOperateButtonsView(this.cryptogramType);

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
            ColorUtility.TryParseHtmlString("#1e3836", out Color color);
            Color[] pixels = Enumerable.Repeat(color, Screen.width * Screen.height).ToArray();
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

            this._DrawOperateButtonsView(this.cryptogramType);

            EditorGUILayout.EndVertical();
        }

        private void _DrawOperateButtonsView(CryptogramType cryptogramType)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            Color bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(255, 220, 0, 255);
            if (GUILayout.Button("Decrypt", GUILayout.MaxWidth(100f)))
            {
                switch (cryptogramType)
                {
                    case CryptogramType.OFFSET:
                        CryptogramUtility.OffsetDecryptBundleFiles(this.sourceFolder, this.dummySize);
                        EditorUtility.DisplayDialog("Crytogram Message", "[OFFSET] Decrypt Process.", "OK");
                        break;
                    case CryptogramType.XOR:
                        CryptogramUtility.XorDecryptBundleFiles(this.sourceFolder, (byte)this.xorKey);
                        EditorUtility.DisplayDialog("Crytogram Message", "[XOR] Decrypt Process.", "OK");
                        break;
                    case CryptogramType.HT2XOR:
                        CryptogramUtility.HT2XorDecryptBundleFiles(this.sourceFolder, (byte)this.hXorKey, (byte)this.tXorKey, (byte)this.jXorKey);
                        EditorUtility.DisplayDialog("Crytogram Message", "[Head-Tail 2 XOR] Decrypt Process.", "OK");
                        break;
                    case CryptogramType.AES:
                        if (string.IsNullOrEmpty(this.aesKey) || string.IsNullOrEmpty(this.aesIv))
                        {
                            EditorUtility.DisplayDialog("Crytogram Message", "[AES] KEY or IV is Empty!!! Can't process.", "OK");
                            break;
                        }
                        CryptogramUtility.AesDecryptBundleFiles(this.sourceFolder, this.aesKey, this.aesIv);
                        EditorUtility.DisplayDialog("Crytogram Message", "[AES] Decrypt Process.", "OK");
                        break;
                }
            }
            GUI.backgroundColor = bc;

            bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(0, 249, 255, 255);
            if (GUILayout.Button("Encrypt", GUILayout.MaxWidth(100f)))
            {
                switch (cryptogramType)
                {
                    case CryptogramType.OFFSET:
                        CryptogramUtility.OffsetEncryptBundleFiles(this.sourceFolder, this.randomSeed, this.dummySize);
                        EditorUtility.DisplayDialog("Crytogram Message", "[OFFSET] Encrypt Process.", "OK");
                        break;
                    case CryptogramType.XOR:
                        CryptogramUtility.XorEncryptBundleFiles(this.sourceFolder, (byte)this.xorKey);
                        EditorUtility.DisplayDialog("Crytogram Message", "[XOR] Encrypt Process.", "OK");
                        break;
                    case CryptogramType.HT2XOR:
                        CryptogramUtility.HT2XorEncryptBundleFiles(this.sourceFolder, (byte)this.hXorKey, (byte)this.tXorKey, (byte)this.jXorKey);
                        EditorUtility.DisplayDialog("Crytogram Message", "[Head-Tail 2 XOR] Encrypt Process.", "OK");
                        break;
                    case CryptogramType.AES:
                        if (string.IsNullOrEmpty(this.aesKey) || string.IsNullOrEmpty(this.aesIv))
                        {
                            EditorUtility.DisplayDialog("Crytogram Message", "[AES] KEY or IV is Empty!!! Can't process.", "OK");
                            break;
                        }
                        CryptogramUtility.AesEncryptBundleFiles(this.sourceFolder, this.aesKey, this.aesIv);
                        EditorUtility.DisplayDialog("Crytogram Message", "[AES] Encrypt Process.", "OK");
                        break;
                }
            }
            GUI.backgroundColor = bc;
            EditorGUILayout.EndHorizontal();
        }

        private void _OpenSourceFolder()
        {
            string folderPath = EditorStorage.GetData(KEY_SAVER, "sourceFolder", Path.Combine($"{Application.dataPath}/", AssetBundleBuilderHelper.GetDefaultBuildOutputRoot()));
            this.sourceFolder = EditorUtility.OpenFolderPanel("Open Source Folder", folderPath, string.Empty);
            if (!string.IsNullOrEmpty(this.sourceFolder)) EditorStorage.SaveData(KEY_SAVER, "sourceFolder", this.sourceFolder);
        }
    }
}