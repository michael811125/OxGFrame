using OxGFrame.AssetLoader.Bundle;
using OxGFrame.AssetLoader.Utility;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using YooAsset.Editor;
using static OxGFrame.AssetLoader.Editor.CryptogramSettingWindow;

namespace OxGFrame.AssetLoader.Editor
{
    public class BundleCryptogramUtilityWindow : EditorWindow
    {
        private static BundleCryptogramUtilityWindow _instance = null;
        internal static BundleCryptogramUtilityWindow GetInstance()
        {
            if (_instance == null)
                _instance = GetWindow<BundleCryptogramUtilityWindow>();
            return _instance;
        }

        [SerializeField]
        public CryptogramType cryptogramType;

        [SerializeField]
        public string sourceFolder;

        private CryptogramSetting _setting;

        internal static string projectPath;
        internal static string keySaver;

        private static Vector2 _windowSize = new Vector2(800f, 150f);

        [MenuItem(BundleHelper.MENU_ROOT + "Bundle Cryptogram Utility (For Verify)", false, 699)]
        public static void ShowWindow()
        {
            projectPath = Application.dataPath;
            keySaver = $"{projectPath}_{nameof(BundleCryptogramUtilityWindow)}";

            _instance = null;
            GetInstance().titleContent = new GUIContent("Bundle Cryptogram Utility");
            GetInstance().Show();
            GetInstance().minSize = _windowSize;
        }

        private void OnEnable()
        {
            this._setting = EditorTool.LoadSettingData<CryptogramSetting>();
            this._LoadSettingsData();
            this.sourceFolder = EditorStorage.GetData(keySaver, $"sourceFolder", Path.Combine($"{Application.dataPath}/", AssetBundleBuilderHelper.GetDefaultBuildOutputRoot()));
            this.cryptogramType = (CryptogramType)Convert.ToInt32(EditorStorage.GetData(keySaver, "cryptogramType", "0"));
        }

        private void _LoadSettingsData()
        {
            // Offset
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

            // ChaCha20
            this.chacha20Key = this._setting.chacha20Key;
            this.chacha20Nonce = this._setting.chacha20Nonce;
            this.chacha20Counter = this._setting.chacha20Counter;

            // XXTEA
            this.xxteaKey = this._setting.xxteaKey;

            // OffsetXOR
            this.offsetXorKey = this._setting.offsetXorKey;
            this.offsetXorDummySize = this._setting.offsetXorDummySize;
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
            if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(keySaver, "sourceFolder", this.sourceFolder);
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
            if (EditorGUI.EndChangeCheck()) EditorStorage.SaveData(keySaver, "cryptogramType", ((int)this.cryptogramType).ToString());

            EditorGUILayout.EndHorizontal();

            this._CryptogramType(this.cryptogramType);
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

            this.dummySize = EditorGUILayout.IntField(new GUIContent("Offset Dummy Size", "Add dummy bytes into front of file (per byte = Random 0 ~ 255)."), this.dummySize);
            if (this.dummySize < 0) this.dummySize = 0;

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

            this.xorKey = EditorGUILayout.IntField("XOR KEY (0 ~ 255)", this.xorKey);
            if (this.xorKey < 0) this.xorKey = 0;
            else if (this.xorKey > 255) this.xorKey = 255;

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
        #endregion

        #region HT2XorPlus
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

            this.aesKey = EditorGUILayout.TextField("AES KEY", this.aesKey);
            this.aesIv = EditorGUILayout.TextField("AES IV", this.aesIv);

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

            this.chacha20Key = EditorGUILayout.TextField("ChaCha20 KEY", this.chacha20Key);
            this.chacha20Nonce = EditorGUILayout.TextField("ChaCha20 NONCE", this.chacha20Nonce);
            this.chacha20Counter = Convert.ToUInt32(EditorGUILayout.IntField("ChaCha20 COUNTER", Convert.ToInt32(this.chacha20Counter)));

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

            this.xxteaKey = EditorGUILayout.TextField("XXTEA KEY", this.xxteaKey);

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

            this.offsetXorKey = EditorGUILayout.IntField("OffsetXOR KEY (0 ~ 255)", this.offsetXorKey);
            if (this.offsetXorKey < 0) this.offsetXorKey = 0;
            else if (this.offsetXorKey > 255) this.offsetXorKey = 255;
            this.offsetXorDummySize = EditorGUILayout.IntField(new GUIContent("OffsetXOR Dummy Size", "Add dummy bytes into front of file (per byte = Random 0 ~ 255)."), this.offsetXorDummySize);
            if (this.offsetXorDummySize < 0) this.offsetXorDummySize = 0;

            this._DrawOperateButtonsView(this.cryptogramType);

            EditorGUILayout.EndVertical();
        }
        #endregion

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
                    case CryptogramType.Offset:
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
                    case CryptogramType.HT2XORPlus:
                        CryptogramUtility.HT2XorPlusDecryptBundleFiles(this.sourceFolder, (byte)this.hXorPlusKey, (byte)this.tXorPlusKey, (byte)this.j1XorPlusKey, (byte)this.j2XorPlusKey);
                        EditorUtility.DisplayDialog("Crytogram Message", "[Head-Tail 2 XOR Plus] Decrypt Process.", "OK");
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
                    case CryptogramType.ChaCha20:
                        if (string.IsNullOrEmpty(this.chacha20Key) || string.IsNullOrEmpty(this.chacha20Nonce))
                        {
                            EditorUtility.DisplayDialog("Crytogram Message", "[ChaCha20] KEY or NONCE is Empty!!! Can't process.", "OK");
                            break;
                        }
                        CryptogramUtility.ChaCha20DecryptBundleFiles(this.sourceFolder, this.chacha20Key, this.chacha20Nonce, this.chacha20Counter);
                        EditorUtility.DisplayDialog("Crytogram Message", "[ChaCha20] Decrypt Process.", "OK");
                        break;
                    case CryptogramType.XXTEA:
                        if (string.IsNullOrEmpty(this.xxteaKey))
                        {
                            EditorUtility.DisplayDialog("Crytogram Message", "[XXTEA] KEY is Empty!!! Can't process.", "OK");
                            break;
                        }
                        CryptogramUtility.XXTEADecryptBundleFiles(this.sourceFolder, this.xxteaKey);
                        EditorUtility.DisplayDialog("Crytogram Message", "[XXTEA] Decrypt Process.", "OK");
                        break;
                    case CryptogramType.OffsetXOR:
                        CryptogramUtility.OffsetXorDecryptBundleFiles(this.sourceFolder, (byte)this.offsetXorKey, this.offsetXorDummySize);
                        EditorUtility.DisplayDialog("Crytogram Message", "[OffsetXOR] Decrypt Process.", "OK");
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
                    case CryptogramType.Offset:
                        CryptogramUtility.OffsetEncryptBundleFiles(this.sourceFolder, this.dummySize);
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
                    case CryptogramType.HT2XORPlus:
                        CryptogramUtility.HT2XorPlusEncryptBundleFiles(this.sourceFolder, (byte)this.hXorPlusKey, (byte)this.tXorPlusKey, (byte)this.j1XorPlusKey, (byte)this.j2XorPlusKey);
                        EditorUtility.DisplayDialog("Crytogram Message", "[Head-Tail 2 XOR Plus] Encrypt Process.", "OK");
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
                    case CryptogramType.ChaCha20:
                        if (string.IsNullOrEmpty(this.chacha20Key) || string.IsNullOrEmpty(this.chacha20Nonce))
                        {
                            EditorUtility.DisplayDialog("Crytogram Message", "[ChaCha20] KEY or NONCE is Empty!!! Can't process.", "OK");
                            break;
                        }
                        CryptogramUtility.ChaCha20EncryptBundleFiles(this.sourceFolder, this.chacha20Key, this.chacha20Nonce, this.chacha20Counter);
                        EditorUtility.DisplayDialog("Crytogram Message", "[ChaCha20] Encrypt Process.", "OK");
                        break;
                    case CryptogramType.XXTEA:
                        if (string.IsNullOrEmpty(this.xxteaKey))
                        {
                            EditorUtility.DisplayDialog("Crytogram Message", "[XXTEA] KEY is Empty!!! Can't process.", "OK");
                            break;
                        }
                        CryptogramUtility.XXTEAEncryptBundleFiles(this.sourceFolder, this.xxteaKey);
                        EditorUtility.DisplayDialog("Crytogram Message", "[XXTEA] Encrypt Process.", "OK");
                        break;
                    case CryptogramType.OffsetXOR:
                        CryptogramUtility.OffsetXorEncryptBundleFiles(this.sourceFolder, (byte)this.offsetXorKey, this.offsetXorDummySize);
                        EditorUtility.DisplayDialog("Crytogram Message", "[OffsetXOR] Encrypt Process.", "OK");
                        break;
                }
            }
            GUI.backgroundColor = bc;
            EditorGUILayout.EndHorizontal();
        }

        private void _OpenSourceFolder()
        {
            string folderPath = EditorStorage.GetData(keySaver, "sourceFolder", Path.Combine($"{Application.dataPath}/", AssetBundleBuilderHelper.GetDefaultBuildOutputRoot()));
            this.sourceFolder = EditorUtility.OpenFolderPanel("Open Source Folder", folderPath, string.Empty);
            if (!string.IsNullOrEmpty(this.sourceFolder)) EditorStorage.SaveData(keySaver, "sourceFolder", this.sourceFolder);
        }
    }
}