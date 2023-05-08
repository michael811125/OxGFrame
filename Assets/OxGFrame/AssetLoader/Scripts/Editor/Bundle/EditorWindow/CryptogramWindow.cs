using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OxGFrame.AssetLoader.Editor
{
    public class CryptogramWindow : EditorWindow
    {
        public enum CryptogramType
        {
            OFFSET,
            XOR,
            HTXOR,
            AES
        }

        private static CryptogramWindow _instance = null;
        internal static CryptogramWindow GetInstance()
        {
            if (_instance == null) _instance = GetWindow<CryptogramWindow>();
            return _instance;
        }

        [SerializeField]
        public CryptogramType cryptogramType;

        private CryptogramSetting _settings;
        private bool _isDirty = false;

        internal const string KEY_SAVE_DATA_FOR_CRYPTOGRAM_SETTING_EDITOR = "KEY_SAVE_DATA_FOR_CRYPTOGRAM_SETTING_EDITOR";

        private static Vector2 _windowSize = new Vector2(800f, 150f);

        [MenuItem("YooAsset/" + "OxGFrame Cryptogram Setting With YooAsset", false, 999)]
        public static void ShowWindow()
        {
            _instance = null;
            GetInstance().titleContent = new GUIContent("Cryptogram Setting");
            GetInstance().Show();
            GetInstance().minSize = _windowSize;
        }

        private void OnEnable()
        {
            this._isDirty = false;
            this._settings = EditorTool.LoadSettingData<CryptogramSetting>();
            this._LoadSettingsData();
            this.cryptogramType = (CryptogramType)Convert.ToInt32(EditorStorage.GetData(KEY_SAVE_DATA_FOR_CRYPTOGRAM_SETTING_EDITOR, "cryptogramType", "0"));
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();

            // cryptogram options area
            EditorGUI.BeginChangeCheck();
            this.cryptogramType = (CryptogramType)EditorGUILayout.EnumPopup("Cryptogram Type", this.cryptogramType);
            if (EditorGUI.EndChangeCheck())
            {
                this._isDirty = false;
                this._LoadSettingsData();
                EditorStorage.SaveData(KEY_SAVE_DATA_FOR_CRYPTOGRAM_SETTING_EDITOR, "cryptogramType", ((int)this.cryptogramType).ToString());
            }

            EditorGUILayout.EndHorizontal();

            this._CryptogramType(this.cryptogramType);
        }

        private void _LoadSettingsData()
        {
            this.randomSeed = this._settings.randomSeed;
            this.dummySize = this._settings.dummySize;
            this.xorKey = this._settings.xorKey;
            this.hXorKey = this._settings.hXorKey;
            this.tXorKey = this._settings.tXorKey;
            this.aesKey = this._settings.aesKey;
            this.aesIv = this._settings.aesIv;
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
                case CryptogramType.HTXOR:
                    this._DrawHTXorView();
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
            ColorUtility.TryParseHtmlString("#1c589c", out Color color);
            Color[] pixels = Enumerable.Repeat(color, Screen.width * Screen.height).ToArray();
            bg.SetPixels(pixels);
            bg.Apply();
            style.normal.background = bg;
            EditorGUILayout.BeginVertical(style);
            var centeredStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
            centeredStyle.alignment = TextAnchor.UpperCenter;
            GUILayout.Label(new GUIContent("Offset Settings"), centeredStyle);
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            this.randomSeed = EditorGUILayout.IntField(new GUIContent("Random Seed", "Fixed random values."), this.randomSeed);
            if (this.randomSeed <= 0) this.randomSeed = 1;
            this.dummySize = EditorGUILayout.IntField(new GUIContent("Offset Dummy Size", "Add dummy bytes into front of file (per byte = Random 0 ~ 255)."), this.dummySize);
            if (this.dummySize < 0) this.dummySize = 0;
            if (EditorGUI.EndChangeCheck()) this._isDirty = true;

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
            ColorUtility.TryParseHtmlString("#1c589c", out Color color);
            Color[] pixels = Enumerable.Repeat(color, Screen.width * Screen.height).ToArray();
            bg.SetPixels(pixels);
            bg.Apply();
            style.normal.background = bg;
            EditorGUILayout.BeginVertical(style);
            var centeredStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
            centeredStyle.alignment = TextAnchor.UpperCenter;
            GUILayout.Label(new GUIContent("XOR Settings"), centeredStyle);
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            this.xorKey = EditorGUILayout.IntField("XOR KEY (0 ~ 255)", this.xorKey);
            if (this.xorKey < 0) this.xorKey = 0;
            else if (this.xorKey > 255) this.xorKey = 255;
            if (EditorGUI.EndChangeCheck()) this._isDirty = true;

            this._DrawOperateButtonsView(this.cryptogramType);

            EditorGUILayout.EndVertical();
        }

        [SerializeField]
        public int hXorKey = 0;
        [SerializeField]
        public int tXorKey = 0;
        private void _DrawHTXorView()
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
            GUILayout.Label(new GUIContent("Head-Tail XOR Settings"), centeredStyle);
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            this.hXorKey = EditorGUILayout.IntField("Head XOR KEY (0 ~ 255)", this.hXorKey);
            if (this.hXorKey < 0) this.hXorKey = 0;
            else if (this.hXorKey > 255) this.hXorKey = 255;
            this.tXorKey = EditorGUILayout.IntField("Tail XOR KEY (0 ~ 255)", this.tXorKey);
            if (this.tXorKey < 0) this.tXorKey = 0;
            else if (this.tXorKey > 255) this.tXorKey = 255;
            if (EditorGUI.EndChangeCheck()) this._isDirty = true;

            this._DrawOperateButtonsView(this.cryptogramType);

            EditorGUILayout.EndVertical();
        }

        [SerializeField]
        public string aesKey = "aes_key";
        [SerializeField]
        public string aesIv = "aes_iv";
        private void _DrawAesView()
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
            GUILayout.Label(new GUIContent("AES Settings"), centeredStyle);
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            this.aesKey = EditorGUILayout.TextField("AES KEY", this.aesKey);
            this.aesIv = EditorGUILayout.TextField("AES IV", this.aesIv);
            if (EditorGUI.EndChangeCheck()) this._isDirty = true;

            this._DrawOperateButtonsView(this.cryptogramType);

            EditorGUILayout.EndVertical();
        }

        private void _DrawOperateButtonsView(CryptogramType cryptogramType)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            Color bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(0, 255, 128, 255);
            EditorGUI.BeginDisabledGroup(!this._isDirty);
            if (GUILayout.Button("Save", GUILayout.MaxWidth(100f)))
            {
                this._SaveData(cryptogramType, true);
            }
            GUI.backgroundColor = bc;
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
        }

        private void _SaveData(CryptogramType cryptogramType, bool isShowDialog = false)
        {
            switch (cryptogramType)
            {
                case CryptogramType.OFFSET:
                    this._settings.randomSeed = this.randomSeed;
                    this._settings.dummySize = this.dummySize;

                    this._isDirty = false;
                    EditorUtility.SetDirty(this._settings);
                    AssetDatabase.SaveAssets();

                    if (isShowDialog) EditorUtility.DisplayDialog("Crytogram Message", "Saved [OFFSET] Setting.", "OK");
                    break;
                case CryptogramType.XOR:
                    this._settings.xorKey = (byte)this.xorKey;

                    this._isDirty = false;
                    EditorUtility.SetDirty(this._settings);
                    AssetDatabase.SaveAssets();

                    if (isShowDialog) EditorUtility.DisplayDialog("Crytogram Message", "Saved [XOR] Setting.", "OK");
                    break;
                case CryptogramType.HTXOR:
                    this._settings.hXorKey = (byte)this.hXorKey;
                    this._settings.tXorKey = (byte)this.tXorKey;

                    this._isDirty = false;
                    EditorUtility.SetDirty(this._settings);
                    AssetDatabase.SaveAssets();

                    if (isShowDialog) EditorUtility.DisplayDialog("Crytogram Message", "Saved [Head-Tail XOR] Setting.", "OK");
                    break;
                case CryptogramType.AES:
                    if (string.IsNullOrEmpty(this.aesKey) || string.IsNullOrEmpty(this.aesIv))
                    {
                        if (isShowDialog) EditorUtility.DisplayDialog("Crytogram Message", "[AES] KEY or IV is Empty!!! Can't process.", "OK");
                        break;
                    }

                    this._settings.aesKey = this.aesKey;
                    this._settings.aesIv = this.aesIv;

                    this._isDirty = false;
                    EditorUtility.SetDirty(this._settings);
                    AssetDatabase.SaveAssets();

                    if (isShowDialog) EditorUtility.DisplayDialog("Crytogram Message", "Saved [AES] Setting.", "OK");
                    break;
            }
        }
    }
}