using OxGFrame.AssetLoader.Bundle;
using OxGFrame.AssetLoader.Utility;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OxGFrame.AssetLoader.Editor
{
    public class CryptogramSettingsWindow : EditorWindow
    {
        public enum CryptogramType
        {
            Offset,
            XOR,
            HT2XOR,
            HT2XORPlus,
            AES,
            ChaCha20,
            XXTEA,
            OffsetXOR
        }

        private static CryptogramSettingsWindow _instance = null;
        internal static CryptogramSettingsWindow GetInstance()
        {
            if (_instance == null)
                _instance = GetWindow<CryptogramSettingsWindow>();
            return _instance;
        }

        [SerializeField]
        public CryptogramType cryptogramType;

        private CryptogramSettings _settings;
        private bool _isDirty = false;

        internal static string projectPath;
        internal static string keySaver;

        private static Vector2 _windowSize = new Vector2(800f, 150f);

        [MenuItem("YooAsset/" + "OxGFrame Cryptogram Settings With YooAsset", false, 999)]
        public static void ShowWindow()
        {
            projectPath = Application.dataPath;
            keySaver = $"{projectPath}_{nameof(CryptogramSettingsWindow)}";

            _instance = null;
            GetInstance().titleContent = new GUIContent("Cryptogram Settings");
            GetInstance().Show();
            GetInstance().minSize = _windowSize;
        }

        private void OnEnable()
        {
            this._isDirty = false;
            this._settings = AssetUtility.LoadSettingsData<CryptogramSettings>();
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
            this.dummySize = this._settings.dummySize;

            // XOR
            this.xorKey = this._settings.xorKey;

            // HT2XOR
            this.hXorKey = this._settings.hXorKey;
            this.tXorKey = this._settings.tXorKey;
            this.jXorKey = this._settings.jXorKey;

            // HT2XORPlus
            this.hXorPlusKey = this._settings.hXorPlusKey;
            this.tXorPlusKey = this._settings.tXorPlusKey;
            this.j1XorPlusKey = this._settings.j1XorPlusKey;
            this.j2XorPlusKey = this._settings.j2XorPlusKey;

            // AES
            this.aesKey = this._settings.aesKey;
            this.aesIv = this._settings.aesIv;

            // ChaCha20
            this.chacha20Key = this._settings.chacha20Key;
            this.chacha20Nonce = this._settings.chacha20Nonce;
            this.chacha20Counter = this._settings.chacha20Counter;

            // XXTEA
            this.xxteaKey = this._settings.xxteaKey;

            // OffsetXOR
            this.offsetXorKey = this._settings.offsetXorKey;
            this.offsetXorDummySize = this._settings.offsetXorDummySize;
        }

        private void _CryptogramType(CryptogramType cryptogramType)
        {
            switch (cryptogramType)
            {
                case CryptogramType.Offset:
                    this._DrawOffsetView();
                    break;
                case CryptogramType.XOR:
                    this._DrawXorView();
                    break;
                case CryptogramType.HT2XOR:
                    this._DrawHT2XorView();
                    break;
                case CryptogramType.HT2XORPlus:
                    this._DrawHT2XorPlusView();
                    break;
                case CryptogramType.AES:
                    this._DrawAesView();
                    break;
                case CryptogramType.ChaCha20:
                    this._DrawChaCha20View();
                    break;
                case CryptogramType.XXTEA:
                    this._DrawXXTEAView();
                    break;
                case CryptogramType.OffsetXOR:
                    this._DrawOffsetXorView();
                    break;
            }
        }

        #region Offset
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

        #region ChaCha20
        [SerializeField]
        public string chacha20Key = "chacha20_key";
        [SerializeField]
        public string chacha20Nonce = "chacha20_nonce";
        [SerializeField]
        public uint chacha20Counter = 1;
        private void _DrawChaCha20View()
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
            GUILayout.Label(new GUIContent("ChaCha20 Settings"), centeredStyle);
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            this.chacha20Key = EditorGUILayout.TextField("ChaCha20 KEY", this.chacha20Key);
            this.chacha20Nonce = EditorGUILayout.TextField("ChaCha20 NONCE", this.chacha20Nonce);
            this.chacha20Counter = Convert.ToUInt32(EditorGUILayout.IntField("ChaCha20 COUNTER", Convert.ToInt32(this.chacha20Counter)));
            if (EditorGUI.EndChangeCheck()) this._isDirty = true;

            this._DrawOperateButtonsView(this.cryptogramType);

            EditorGUILayout.EndVertical();
        }
        #endregion

        #region XXTEA
        [SerializeField]
        public string xxteaKey = "xxtea_key";
        private void _DrawXXTEAView()
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
            GUILayout.Label(new GUIContent("XXTEA Settings"), centeredStyle);
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            this.xxteaKey = EditorGUILayout.TextField("XXTEA KEY", this.xxteaKey);
            if (EditorGUI.EndChangeCheck()) this._isDirty = true;

            this._DrawOperateButtonsView(this.cryptogramType);

            EditorGUILayout.EndVertical();
        }
        #endregion

        #region OffsetXOR
        [SerializeField]
        public int offsetXorKey = 1;
        public int offsetXorDummySize = 1;
        private void _DrawOffsetXorView()
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
            GUILayout.Label(new GUIContent("OffsetXOR Settings"), centeredStyle);
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            this.offsetXorKey = EditorGUILayout.IntField("OffsetXOR KEY (0 ~ 255)", this.offsetXorKey);
            if (this.offsetXorKey < 0) this.offsetXorKey = 0;
            else if (this.offsetXorKey > 255) this.offsetXorKey = 255;
            this.offsetXorDummySize = EditorGUILayout.IntField(new GUIContent("OffsetXOR Dummy Size", "Add dummy bytes into front of file (per byte = Random 0 ~ 255)."), this.offsetXorDummySize);
            if (this.offsetXorDummySize < 0) this.offsetXorDummySize = 0;
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
                    this._settings.dummySize = this.dummySize;

                    this._isDirty = false;
                    EditorUtility.SetDirty(this._settings);
                    AssetDatabase.SaveAssets();

                    if (isShowDialog) EditorUtility.DisplayDialog("Crytogram Message", "Saved [OFFSET] Settings.", "OK");
                    break;
                case CryptogramType.XOR:
                    this._settings.xorKey = (byte)this.xorKey;

                    this._isDirty = false;
                    EditorUtility.SetDirty(this._settings);
                    AssetDatabase.SaveAssets();

                    if (isShowDialog) EditorUtility.DisplayDialog("Crytogram Message", "Saved [XOR] Settings.", "OK");
                    break;
                case CryptogramType.HT2XOR:
                    this._settings.hXorKey = (byte)this.hXorKey;
                    this._settings.tXorKey = (byte)this.tXorKey;
                    this._settings.jXorKey = (byte)this.jXorKey;

                    this._isDirty = false;
                    EditorUtility.SetDirty(this._settings);
                    AssetDatabase.SaveAssets();

                    if (isShowDialog) EditorUtility.DisplayDialog("Crytogram Message", "Saved [Head-Tail 2 XOR] Settings.", "OK");
                    break;
                case CryptogramType.HT2XORPlus:
                    this._settings.hXorPlusKey = (byte)this.hXorPlusKey;
                    this._settings.tXorPlusKey = (byte)this.tXorPlusKey;
                    this._settings.j1XorPlusKey = (byte)this.j1XorPlusKey;
                    this._settings.j2XorPlusKey = (byte)this.j2XorPlusKey;

                    this._isDirty = false;
                    EditorUtility.SetDirty(this._settings);
                    AssetDatabase.SaveAssets();

                    if (isShowDialog) EditorUtility.DisplayDialog("Crytogram Message", "Saved [Head-Tail 2 XOR Plus] Settings.", "OK");
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

                    if (isShowDialog) EditorUtility.DisplayDialog("Crytogram Message", "Saved [AES] Settings.", "OK");
                    break;
                case CryptogramType.ChaCha20:
                    if (string.IsNullOrEmpty(this.chacha20Key) || string.IsNullOrEmpty(this.chacha20Nonce))
                    {
                        if (isShowDialog) EditorUtility.DisplayDialog("Crytogram Message", "[ChaCha20] KEY or NONCE is Empty!!! Can't process.", "OK");
                        break;
                    }

                    this._settings.chacha20Key = this.chacha20Key;
                    this._settings.chacha20Nonce = this.chacha20Nonce;
                    this._settings.chacha20Counter = this.chacha20Counter;

                    this._isDirty = false;
                    EditorUtility.SetDirty(this._settings);
                    AssetDatabase.SaveAssets();

                    if (isShowDialog) EditorUtility.DisplayDialog("Crytogram Message", "Saved [ChaCha20] Settings.", "OK");
                    break;
                case CryptogramType.XXTEA:
                    if (string.IsNullOrEmpty(this.xxteaKey))
                    {
                        if (isShowDialog) EditorUtility.DisplayDialog("Crytogram Message", "[XXTEA] KEY is Empty!!! Can't process.", "OK");
                        break;
                    }

                    this._settings.xxteaKey = this.xxteaKey;

                    this._isDirty = false;
                    EditorUtility.SetDirty(this._settings);
                    AssetDatabase.SaveAssets();

                    if (isShowDialog) EditorUtility.DisplayDialog("Crytogram Message", "Saved [XXTEA] Settings.", "OK");
                    break;
                case CryptogramType.OffsetXOR:
                    this._settings.offsetXorKey = (byte)this.offsetXorKey;
                    this._settings.offsetXorDummySize = this.offsetXorDummySize;

                    this._isDirty = false;
                    EditorUtility.SetDirty(this._settings);
                    AssetDatabase.SaveAssets();

                    if (isShowDialog) EditorUtility.DisplayDialog("Crytogram Message", "Saved [OffsetXOR] Settings.", "OK");
                    break;
            }
        }
    }
}