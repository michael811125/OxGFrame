using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OxGFrame.AssetLoader.Editor
{
    public class CryptogramSettingWindow : EditorWindow
    {
        public enum CryptogramType
        {
            Offset,
            Xor,
            HT2Xor,
            HT2XorPlus,
            Aes
        }

        private static CryptogramSettingWindow _instance = null;
        internal static CryptogramSettingWindow GetInstance()
        {
            if (_instance == null) _instance = GetWindow<CryptogramSettingWindow>();
            return _instance;
        }

        [SerializeField]
        public CryptogramType cryptogramType;

        private CryptogramSetting _setting;
        private bool _isDirty = false;

        internal static string projectPath;
        internal static string keySaver;

        private static Vector2 _windowSize = new Vector2(800f, 150f);

        [MenuItem("YooAsset/" + "OxGFrame Cryptogram Setting With YooAsset", false, 999)]
        public static void ShowWindow()
        {
            projectPath = Application.dataPath;
            keySaver = $"{projectPath}_{nameof(CryptogramSettingWindow)}";

            _instance = null;
            GetInstance().titleContent = new GUIContent("Cryptogram Setting");
            GetInstance().Show();
            GetInstance().minSize = _windowSize;
        }

        private void OnEnable()
        {
            this._isDirty = false;
            this._setting = EditorTool.LoadSettingData<CryptogramSetting>();
            this._LoadSettingsData();
            this.cryptogramType = (CryptogramType)Convert.ToInt32(EditorStorage.GetData(keySaver, "cryptogramType", "0"));
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
                EditorStorage.SaveData(keySaver, "cryptogramType", ((int)this.cryptogramType).ToString());
            }

            EditorGUILayout.EndHorizontal();

            this._CryptogramType(this.cryptogramType);
        }

        private void _LoadSettingsData()
        {
            // Offset
            this.randomSeed = this._setting.randomSeed;
            this.dummySize = this._setting.dummySize;

            // XOR
            this.xorKey = this._setting.xorKey;

            // HT2XOR
            this.hXorKey = this._setting.hXorKey;
            this.tXorKey = this._setting.tXorKey;
            this.jXorKey = this._setting.jXorKey;

            // HT2XORPlus
            this.hXorPlusKey = this._setting.hXorPlusKey;
            this.tXorPlusKey = this._setting.tXorPlusKey;
            this.j1XorPlusKey = this._setting.j1XorPlusKey;
            this.j2XorPlusKey = this._setting.j2XorPlusKey;

            // AES
            this.aesKey = this._setting.aesKey;
            this.aesIv = this._setting.aesIv;
        }

        private void _CryptogramType(CryptogramType cryptogramType)
        {
            switch (cryptogramType)
            {
                case CryptogramType.Offset:
                    this._DrawOffsetView();
                    break;
                case CryptogramType.Xor:
                    this._DrawXorView();
                    break;
                case CryptogramType.HT2Xor:
                    this._DrawHT2XorView();
                    break;
                case CryptogramType.HT2XorPlus:
                    this._DrawHT2XorPlusView();
                    break;
                case CryptogramType.Aes:
                    this._DrawAesView();
                    break;
            }
        }

        #region Offset
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

            EditorGUI.BeginChangeCheck();
            this.randomSeed = EditorGUILayout.IntField(new GUIContent("Random Seed", "Fixed random values."), this.randomSeed);
            if (this.randomSeed <= 0) this.randomSeed = 1;
            this.dummySize = EditorGUILayout.IntField(new GUIContent("Offset Dummy Size", "Add dummy bytes into front of file (per byte = Random 0 ~ 255)."), this.dummySize);
            if (this.dummySize < 0) this.dummySize = 0;
            if (EditorGUI.EndChangeCheck()) this._isDirty = true;

            this._DrawOperateButtonsView(this.cryptogramType);

            EditorGUILayout.EndVertical();
        }
        #endregion

        #region Xor
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

            EditorGUI.BeginChangeCheck();
            this.xorKey = EditorGUILayout.IntField("XOR KEY (0 ~ 255)", this.xorKey);
            if (this.xorKey < 0) this.xorKey = 0;
            else if (this.xorKey > 255) this.xorKey = 255;
            if (EditorGUI.EndChangeCheck()) this._isDirty = true;

            this._DrawOperateButtonsView(this.cryptogramType);

            EditorGUILayout.EndVertical();
        }
        #endregion

        #region HT2Xor
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

            EditorGUI.BeginChangeCheck();
            this.hXorKey = EditorGUILayout.IntField("Head XOR KEY (0 ~ 255)", this.hXorKey);
            if (this.hXorKey < 0) this.hXorKey = 0;
            else if (this.hXorKey > 255) this.hXorKey = 255;
            this.tXorKey = EditorGUILayout.IntField("Tail XOR KEY (0 ~ 255)", this.tXorKey);
            if (this.tXorKey < 0) this.tXorKey = 0;
            else if (this.tXorKey > 255) this.tXorKey = 255;
            this.jXorKey = EditorGUILayout.IntField("Jump XOR KEY (0 ~ 255)", this.jXorKey);
            if (this.jXorKey < 0) this.jXorKey = 0;
            else if (this.jXorKey > 255) this.jXorKey = 255;
            if (EditorGUI.EndChangeCheck()) this._isDirty = true;

            this._DrawOperateButtonsView(this.cryptogramType);

            EditorGUILayout.EndVertical();
        }
        #endregion

        #region HT2Xor Plus
        [SerializeField]
        public int hXorPlusKey = 0;
        [SerializeField]
        public int tXorPlusKey = 0;
        [SerializeField]
        public int j1XorPlusKey = 0;
        [SerializeField]
        public int j2XorPlusKey = 0;
        private void _DrawHT2XorPlusView()
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
            GUILayout.Label(new GUIContent("Head-Tail 2 XOR Plus Settings"), centeredStyle);
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 200;
            this.hXorPlusKey = EditorGUILayout.IntField("Head XOR Plus KEY (0 ~ 255)", this.hXorPlusKey);
            if (this.hXorPlusKey < 0) this.hXorPlusKey = 0;
            else if (this.hXorPlusKey > 255) this.hXorPlusKey = 255;
            this.tXorPlusKey = EditorGUILayout.IntField("Tail XOR Plus KEY (0 ~ 255)", this.tXorPlusKey);
            if (this.tXorPlusKey < 0) this.tXorPlusKey = 0;
            else if (this.tXorPlusKey > 255) this.tXorPlusKey = 255;
            this.j1XorPlusKey = EditorGUILayout.IntField("Jump 1 XOR Plus KEY (0 ~ 255)", this.j1XorPlusKey);
            if (this.j1XorPlusKey < 0) this.j1XorPlusKey = 0;
            else if (this.j1XorPlusKey > 255) this.j1XorPlusKey = 255;
            this.j2XorPlusKey = EditorGUILayout.IntField("Jump 2 XOR Plus KEY (0 ~ 255)", this.j2XorPlusKey);
            if (this.j2XorPlusKey < 0) this.j2XorPlusKey = 0;
            else if (this.j2XorPlusKey > 255) this.j2XorPlusKey = 255;
            EditorGUIUtility.labelWidth = labelWidth;
            if (EditorGUI.EndChangeCheck()) this._isDirty = true;

            this._DrawOperateButtonsView(this.cryptogramType);

            EditorGUILayout.EndVertical();
        }
        #endregion

        #region AES
        [SerializeField]
        public string aesKey = "aes_key";
        [SerializeField]
        public string aesIv = "aes_iv";
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

            EditorGUI.BeginChangeCheck();
            this.aesKey = EditorGUILayout.TextField("AES KEY", this.aesKey);
            this.aesIv = EditorGUILayout.TextField("AES IV", this.aesIv);
            if (EditorGUI.EndChangeCheck()) this._isDirty = true;

            this._DrawOperateButtonsView(this.cryptogramType);

            EditorGUILayout.EndVertical();
        }
        #endregion

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
                case CryptogramType.Offset:
                    this._setting.randomSeed = this.randomSeed;
                    this._setting.dummySize = this.dummySize;

                    this._isDirty = false;
                    EditorUtility.SetDirty(this._setting);
                    AssetDatabase.SaveAssets();

                    if (isShowDialog) EditorUtility.DisplayDialog("Crytogram Message", "Saved [OFFSET] Setting.", "OK");
                    break;
                case CryptogramType.Xor:
                    this._setting.xorKey = (byte)this.xorKey;

                    this._isDirty = false;
                    EditorUtility.SetDirty(this._setting);
                    AssetDatabase.SaveAssets();

                    if (isShowDialog) EditorUtility.DisplayDialog("Crytogram Message", "Saved [XOR] Setting.", "OK");
                    break;
                case CryptogramType.HT2Xor:
                    this._setting.hXorKey = (byte)this.hXorKey;
                    this._setting.tXorKey = (byte)this.tXorKey;
                    this._setting.jXorKey = (byte)this.jXorKey;

                    this._isDirty = false;
                    EditorUtility.SetDirty(this._setting);
                    AssetDatabase.SaveAssets();

                    if (isShowDialog) EditorUtility.DisplayDialog("Crytogram Message", "Saved [Head-Tail 2 XOR] Setting.", "OK");
                    break;
                case CryptogramType.HT2XorPlus:
                    this._setting.hXorPlusKey = (byte)this.hXorPlusKey;
                    this._setting.tXorPlusKey = (byte)this.tXorPlusKey;
                    this._setting.j1XorPlusKey = (byte)this.j1XorPlusKey;
                    this._setting.j2XorPlusKey = (byte)this.j2XorPlusKey;

                    this._isDirty = false;
                    EditorUtility.SetDirty(this._setting);
                    AssetDatabase.SaveAssets();

                    if (isShowDialog) EditorUtility.DisplayDialog("Crytogram Message", "Saved [Head-Tail 2 XOR Plus] Setting.", "OK");
                    break;
                case CryptogramType.Aes:
                    if (string.IsNullOrEmpty(this.aesKey) || string.IsNullOrEmpty(this.aesIv))
                    {
                        if (isShowDialog) EditorUtility.DisplayDialog("Crytogram Message", "[AES] KEY or IV is Empty!!! Can't process.", "OK");
                        break;
                    }

                    this._setting.aesKey = this.aesKey;
                    this._setting.aesIv = this.aesIv;

                    this._isDirty = false;
                    EditorUtility.SetDirty(this._setting);
                    AssetDatabase.SaveAssets();

                    if (isShowDialog) EditorUtility.DisplayDialog("Crytogram Message", "Saved [AES] Setting.", "OK");
                    break;
            }
        }
    }
}