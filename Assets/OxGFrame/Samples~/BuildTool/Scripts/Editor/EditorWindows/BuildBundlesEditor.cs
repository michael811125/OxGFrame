using Newtonsoft.Json;
using OxGFrame.AssetLoader;
using OxGFrame.AssetLoader.Bundle;
using OxGFrame.AssetLoader.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using YooAsset;
using YooAsset.Editor;
using static OxGFrame.AssetLoader.Bundle.AppConfig;

namespace OxGFrame.Extensions.BuildTool.Editor
{
    public class BuildBundlesEditor : EditorWindow
    {
        [Serializable]
        public class ExportBundleMap
        {
            public bool includeConfigs;
            public bool includeSemanticPatch;
            public List<GroupInfo> groups;
            public bool generateAll;
            public bool hotfixCompile;
            public List<Package> packages;
        }

        [Serializable]
        public class Package
        {
            public string packageName;
            public string buildPipeline;
            public string bundleEncryptionArgs;
            public string manifestEncryptionArgs;
            public string compressOption;
            public string fileNameStyle;
            public string builtinCopyOption;
            public bool clearBuildCacheFiles;
            public bool useAssetDependencyDB;
            public string exportArgs;
        }

        [Serializable]
        public class GroupInfo
        {
            public string groupName;
            public string[] tags;
        }

        [Serializable]
        public class PackageInfo
        {
            public enum ExportType
            {
                APP,
                DLC
            }

            public string packageName;
            public EBuildPipeline buildPipeline;
            public string[] bundleEncryptionClassNames;
            public int selectedBundleEncryptionIndex;
            public string[] manifestEncryptionClassNames;
            public int selectedManifestEncryptionIndex;
            public ECompressOption compressOption = ECompressOption.LZ4;
            public EFileNameStyle fileNameStyle;
            public EBuildinFileCopyOption copyOption;
            public ExportType exportType;
            public bool withoutPlatform;
            public string dlcVersion = "latest";
            public bool clearBuildCacheFiles;
            public bool useAssetDependencyDB;

            private List<Type> _bundleEncryptionClassTypes = new List<Type>();
            private List<Type> _manifestEncryptionClassTypes = new List<Type>();

            public PackageInfo()
            {
                // Bundle encryption
                this._bundleEncryptionClassTypes = EditorTools.GetAssignableTypes(typeof(IEncryptionServices));
                if (this._bundleEncryptionClassTypes != null && this._bundleEncryptionClassTypes.Count > 0)
                {
                    this.bundleEncryptionClassNames = new string[this._bundleEncryptionClassTypes.Count];
                    for (int i = 0; i < this._bundleEncryptionClassTypes.Count; i++)
                        this.bundleEncryptionClassNames[i] = this._bundleEncryptionClassTypes[i].FullName;
                }
                else
                {
                    this._bundleEncryptionClassTypes = new List<Type>();
                    this.bundleEncryptionClassNames = new string[] { "None" };
                }

                // Manifest encryption
                this._manifestEncryptionClassTypes = EditorTools.GetAssignableTypes(typeof(IManifestProcessServices));
                if (this._manifestEncryptionClassTypes != null && this._manifestEncryptionClassTypes.Count > 0)
                {

                    this.manifestEncryptionClassNames = new string[this._manifestEncryptionClassTypes.Count];
                    for (int i = 0; i < this._manifestEncryptionClassTypes.Count; i++)
                        this.manifestEncryptionClassNames[i] = this._manifestEncryptionClassTypes[i].FullName;
                }
                else
                {
                    this._manifestEncryptionClassTypes = new List<Type>();
                    this.manifestEncryptionClassNames = new string[] { "None" };
                }
            }
        }

        private static BuildBundlesEditor _instance = null;
        internal static BuildBundlesEditor GetInstance()
        {
            if (_instance == null) _instance = GetWindow<BuildBundlesEditor>();
            return _instance;
        }

        [SerializeField]
        public string productName;
        [SerializeField]
        public string appVersion;
        [SerializeField]
        public SemanticRule semanticRule;
        [SerializeField]
        public string jsonInput;
        [SerializeField]
        public bool autoReveal;
        [SerializeField]
        public bool isClearOutputPath;
        [SerializeField]
        public bool includeConfigs;

        [SerializeField]
        public List<GroupInfo> groups = new List<GroupInfo>();
        [SerializeField]
        public bool generateAll = false;
        [SerializeField]
        public bool hotfixCompile = false;
        [SerializeField]
        public List<PackageInfo> collectorPackages;

        private Vector2 _scrollview1;
        private Vector2 _scrollview2;

        private float _splitRatio = 0.5f;
        private float _splitterHeight = 2f;
        private bool _isDragging = false;

        private SerializedObject _serObj;
        private SerializedProperty _groups;
        private SerializedProperty _generateAll;
        private SerializedProperty _hotfixCompile;
        private SerializedProperty _collectorPackagesPty;

        internal static string projectPath;
        internal static string keySaver;

        private static Vector2 _windowSize = new Vector2(650f, 800f);

        [MenuItem("OxGFrame/Extensions/BuildTool/Build Bundles by Bundle Map JSON", priority = 9999)]
        public static void ShowWindow()
        {
            projectPath = Application.dataPath;
            keySaver = $"{projectPath}_{nameof(BuildBundlesEditor)}";

            _instance = null;
            GetInstance().titleContent = new GUIContent("Build bundles by bundle map json");
            GetInstance().Show();
            GetInstance().minSize = _windowSize;
        }

        private void OnEnable()
        {
            this._serObj = new SerializedObject(this);
            this._groups = this._serObj.FindProperty("groups");
            this._generateAll = this._serObj.FindProperty("generateAll");
            this._hotfixCompile = this._serObj.FindProperty("hotfixCompile");

            this.productName = EditorStorage.GetData(keySaver, "productName", Application.productName);
            this.appVersion = EditorStorage.GetData(keySaver, "appVersion", Application.version);
            this.semanticRule.MAJOR = true;
            this.semanticRule.MINOR = true;
            this.semanticRule.PATCH = Convert.ToBoolean(EditorStorage.GetData(keySaver, "semanticRule.PATCH", "false"));
            this.autoReveal = Convert.ToBoolean(EditorStorage.GetData(keySaver, "autoReveal", "true"));
            this.isClearOutputPath = Convert.ToBoolean(EditorStorage.GetData(keySaver, "isClearOutputPath", "true"));
            this.includeConfigs = Convert.ToBoolean(EditorStorage.GetData(keySaver, "includeConfigs", "true"));
            this.jsonInput = EditorStorage.GetString(keySaver + "jsonInput", string.Empty);
            string json = EditorStorage.GetData(keySaver, "groups", string.Empty);
            if (!string.IsNullOrEmpty(json))
                this.groups = JsonConvert.DeserializeObject<List<GroupInfo>>(json);
            this.generateAll = Convert.ToBoolean(EditorStorage.GetData(keySaver, "generateAll", "true"));
            this.hotfixCompile = Convert.ToBoolean(EditorStorage.GetData(keySaver, "hotfixCompile", "true"));

            this._InitCollectorPackages();
        }

        private void OnGUI()
        {
            float windowHeight = position.height;
            float windowWidth = position.width;

            // 先計算 top/bottom 高度
            float topHeight = (windowHeight - this._splitterHeight) * this._splitRatio;
            // 針對 DrawBuildButtonView 高度的 Offset (避免視窗裁切按鈕)
            float bottomHeightOffset = 5f;
            float bottomHeight = windowHeight - topHeight - bottomHeightOffset - this._splitterHeight;

            // Top
            Rect topRect = new Rect(0, 0, windowWidth, topHeight);
            GUILayout.BeginArea(topRect);
            this._DrawProductNameTextFieldView();
            this._DrawAppVersionTextFieldView();
            this._DrawCollectorPackagesView();
            GUILayout.EndArea();

            // Splitter
            this._DrawResizableSplitter(windowWidth, windowHeight, topHeight);

            // Bottom
            Rect bottomRect = new Rect(0, topHeight + this._splitterHeight, windowWidth, bottomHeight);
            GUILayout.BeginArea(bottomRect);
            this._DrwaJsonClipboardView();
            this._DrawBuildButtonView();
            GUILayout.EndArea();
        }

        private void _InitCollectorPackages(bool forceReload = false)
        {
            string json = EditorStorage.GetData(keySaver, "collectorPackages", string.Empty);
            if (string.IsNullOrEmpty(json) || forceReload)
            {
                var packages = AssetBundleCollectorSettingData.Setting.Packages.ToList();
                this.collectorPackages = new List<PackageInfo>();
                foreach (var pkg in packages)
                {
                    var packageInfo = new PackageInfo();
                    packageInfo.packageName = pkg.PackageName;
                    packageInfo.copyOption = EBuildinFileCopyOption.None;
                    packageInfo.buildPipeline = EBuildPipeline.ScriptableBuildPipeline;
                    this.collectorPackages.Add(packageInfo);
                }

                json = JsonConvert.SerializeObject(this.collectorPackages);
                EditorStorage.SaveData(keySaver, "collectorPackages", json);
            }
            else
            {
                this.collectorPackages = JsonConvert.DeserializeObject<List<PackageInfo>>(json);
            }

            this._collectorPackagesPty = this._serObj.FindProperty("collectorPackages");
        }

        private void _ConvertToJSON(string targetPackageName = null)
        {
            List<Package> packages = new List<Package>();
            foreach (var pkgInfo in this.collectorPackages)
            {
                var package = new Package();

                if (!string.IsNullOrEmpty(targetPackageName) &&
                    pkgInfo.packageName != targetPackageName)
                    continue;

                // 包裹名稱
                package.packageName = pkgInfo.packageName;

                // 構建管線
                string buildPipeline = string.Empty;
                if (pkgInfo.buildPipeline == EBuildPipeline.BuiltinBuildPipeline)
                    buildPipeline = "BBP";
                else if (pkgInfo.buildPipeline == EBuildPipeline.ScriptableBuildPipeline)
                    buildPipeline = "SBP";
                else if (pkgInfo.buildPipeline == EBuildPipeline.RawFileBuildPipeline)
                    buildPipeline = "RFBP";
                else
                    buildPipeline = "SBP";
                package.buildPipeline = buildPipeline;

                // Bundle 加密方法
                string encryptionArgs = string.Empty;
                string encryptionCheck = pkgInfo.bundleEncryptionClassNames[pkgInfo.selectedBundleEncryptionIndex].ToUpper();
                if (encryptionCheck.IndexOf(BundleConfig.CryptogramType.NONE) != -1)
                    encryptionArgs = "none";
                else if (encryptionCheck.IndexOf(BundleConfig.CryptogramType.HT2XORPLUS) != -1)
                    encryptionArgs = "ht2xorplus";
                else if (encryptionCheck.IndexOf(BundleConfig.CryptogramType.HT2XOR) != -1)
                    encryptionArgs = "ht2xor";
                else if (encryptionCheck.IndexOf(BundleConfig.CryptogramType.OFFSETXOR) != -1)
                    encryptionArgs = "offsetxor";
                else if (encryptionCheck.IndexOf(BundleConfig.CryptogramType.OFFSET) != -1)
                    encryptionArgs = "offset";
                else if (encryptionCheck.IndexOf(BundleConfig.CryptogramType.XOR) != -1)
                    encryptionArgs = "xor";
                else if (encryptionCheck.IndexOf(BundleConfig.CryptogramType.AES) != -1)
                    encryptionArgs = "aes";
                else if (encryptionCheck.IndexOf(BundleConfig.CryptogramType.CHACHA20) != -1)
                    encryptionArgs = "chacha20";
                else if (encryptionCheck.IndexOf(BundleConfig.CryptogramType.XXTEA) != -1)
                    encryptionArgs = "xxtea";

                package.bundleEncryptionArgs = encryptionArgs;

                // Manifest 加密方法
                encryptionCheck = pkgInfo.manifestEncryptionClassNames[pkgInfo.selectedManifestEncryptionIndex].ToUpper();
                if (encryptionCheck.IndexOf(BundleConfig.CryptogramType.NONE) != -1)
                    encryptionArgs = "none";
                else if (encryptionCheck.IndexOf(BundleConfig.CryptogramType.HT2XORPLUS) != -1)
                    encryptionArgs = "ht2xorplus";
                else if (encryptionCheck.IndexOf(BundleConfig.CryptogramType.HT2XOR) != -1)
                    encryptionArgs = "ht2xor";
                else if (encryptionCheck.IndexOf(BundleConfig.CryptogramType.OFFSETXOR) != -1)
                    encryptionArgs = "offsetxor";
                else if (encryptionCheck.IndexOf(BundleConfig.CryptogramType.OFFSET) != -1)
                    encryptionArgs = "offset";
                else if (encryptionCheck.IndexOf(BundleConfig.CryptogramType.XOR) != -1)
                    encryptionArgs = "xor";
                else if (encryptionCheck.IndexOf(BundleConfig.CryptogramType.AES) != -1)
                    encryptionArgs = "aes";
                else if (encryptionCheck.IndexOf(BundleConfig.CryptogramType.CHACHA20) != -1)
                    encryptionArgs = "chacha20";
                else if (encryptionCheck.IndexOf(BundleConfig.CryptogramType.XXTEA) != -1)
                    encryptionArgs = "xxtea";

                package.manifestEncryptionArgs = encryptionArgs;

                // 壓縮方法
                package.compressOption = pkgInfo.compressOption.ToString();

                // 輸出名稱規則
                package.fileNameStyle = pkgInfo.fileNameStyle.ToString();

                // 內置選項
                package.builtinCopyOption = pkgInfo.copyOption.ToString();

                // 附加選項
                package.clearBuildCacheFiles = pkgInfo.clearBuildCacheFiles;
                package.useAssetDependencyDB = pkgInfo.useAssetDependencyDB;

                // 輸出類型
                string exportArgs = string.Empty;
                if (pkgInfo.exportType == PackageInfo.ExportType.APP)
                    exportArgs = "APP";
                else if (pkgInfo.exportType == PackageInfo.ExportType.DLC)
                    exportArgs = $"DLC, {Convert.ToInt32(pkgInfo.withoutPlatform)}, {pkgInfo.dlcVersion}";

                package.exportArgs = exportArgs;

                // 加入清單
                packages.Add(package);
            }

            ExportBundleMap exportBundleMap = new ExportBundleMap();
            exportBundleMap.includeConfigs = includeConfigs;
            exportBundleMap.includeSemanticPatch = this.semanticRule.PATCH;
            exportBundleMap.groups = this.groups;
            exportBundleMap.generateAll = this.generateAll;
            exportBundleMap.hotfixCompile = this.hotfixCompile;
            exportBundleMap.packages = packages;

            // 讓 TextArea 失去焦點
            GUI.FocusControl(null);
            this.jsonInput = JsonConvert.SerializeObject(exportBundleMap, Formatting.Indented);
            EditorStorage.SaveString(keySaver + "jsonInput", this.jsonInput);
        }

        private void _DrawResizableSplitter(float windowWidth, float windowHeight, float topHeight)
        {
            Rect splitterRect = new Rect(0, topHeight, windowWidth, this._splitterHeight);
            EditorGUI.DrawRect(splitterRect, Color.gray);
            EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeVertical);
            this._HandleSplitter(splitterRect, windowHeight);
        }

        private void _HandleSplitter(Rect splitterRect, float windowHeight)
        {
            Event e = Event.current;

            switch (e.type)
            {
                case EventType.MouseDown:
                    if (splitterRect.Contains(e.mousePosition))
                    {
                        this._isDragging = true;
                        e.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if (this._isDragging)
                    {
                        this._splitRatio = Mathf.Clamp(e.mousePosition.y / windowHeight, 0.1f, 0.9f);
                        e.Use();
                        Repaint();
                    }
                    break;

                case EventType.MouseUp:
                    if (this._isDragging)
                    {
                        this._isDragging = false;
                        e.Use();
                    }
                    break;
            }
        }

        private void _DrawProductNameTextFieldView()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            this.productName = EditorGUILayout.TextField("Product Name", this.productName);
            if (EditorGUI.EndChangeCheck())
                EditorStorage.SaveData(keySaver, "productName", this.productName);

            // Sync button
            Color bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(255, 238, 36, 255);
            if (GUILayout.Button("Sync to Player Settings", GUILayout.MaxWidth(202f)))
            {
                bool confirmation = EditorUtility.DisplayDialog
                (
                    $"Sync Product Name Notification",
                    $"Do you want to sync Product Name: {this.productName} to Player Settings?",
                    "yes",
                    "cancel"
                );

                if (confirmation)
                    PlayerSettings.productName = this.productName;
            }
            GUI.backgroundColor = bc;
            EditorGUILayout.EndHorizontal();
        }

        private void _DrawAppVersionTextFieldView()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            this.appVersion = EditorGUILayout.TextField("App Version", this.appVersion);
            if (EditorGUI.EndChangeCheck())
                EditorStorage.SaveData(keySaver, "appVersion", this.appVersion);

            // Sync button
            Color bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(255, 238, 36, 255);
            if (GUILayout.Button("Sync to Player Settings", GUILayout.MaxWidth(202f)))
            {
                bool confirmation = EditorUtility.DisplayDialog
                (
                    $"Sync App Version Notification",
                    $"Do you want to sync App Version: {this.appVersion} to Player Settings?",
                    "yes",
                    "cancel"
                );

                if (confirmation)
                    PlayerSettings.bundleVersion = this.appVersion;
            }
            GUI.backgroundColor = bc;
            EditorGUILayout.EndHorizontal();
        }

        private void _DrawSemanticRuleView()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(new GUIContent("Semantic Rule", "[This option applies only to the AppConfig on the Host Server] If the Patch option is checked, the folder will be output with the full version number (to prevent overwriting resources from different versions)."), GUILayout.MaxWidth(147.5f));

            EditorGUI.BeginDisabledGroup(true);
            this.semanticRule.MAJOR = GUILayout.Toggle(this.semanticRule.MAJOR, new GUIContent("Major", ""), GUILayout.MaxWidth(75f));
            this.semanticRule.MINOR = GUILayout.Toggle(this.semanticRule.MINOR, new GUIContent("Minor", ""), GUILayout.MaxWidth(75f));
            EditorGUI.EndDisabledGroup();

            this.semanticRule.PATCH = GUILayout.Toggle(this.semanticRule.PATCH, new GUIContent("Patch", ""), GUILayout.MaxWidth(75f));
            EditorStorage.SaveData(keySaver, "semanticRule.PATCH", this.semanticRule.PATCH.ToString());

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
        }

        private void _DrawIncludeConfigsTglView()
        {
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();

            this.includeConfigs = GUILayout.Toggle(this.includeConfigs, new GUIContent("Export with Configs For CDN", "If checked, the export directory will include configuration files."));

            if (EditorGUI.EndChangeCheck())
            {
                EditorStorage.SaveData(keySaver, "includeConfigs", this.includeConfigs.ToString());
            }
        }

        private void _DrawCollectorPackagesView()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            // Label 靠左顯示
            GUIStyle coloredLabel = new GUIStyle(EditorStyles.boldLabel) { richText = true };
            EditorGUILayout.LabelField("<color=yellow>Package List <color=green>(Please make sure to convert to JSON before executing the build)</color>:</color>", coloredLabel, GUILayout.Width(position.width - 210f));

            EditorGUILayout.EndHorizontal();

            this._DrawIncludeConfigsTglView();

            this._DrawSemanticRuleView();

            this._serObj.Update();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(this._groups, true);

            EditorGUILayout.LabelField("HybridCLR", GUILayout.Width(position.width - 210f));

            var label = new GUIContent
            (
                "Generate All",
                "If you modify the AOT code, be sure to check Generate All (If it hasn’t been run before, you must execute it once)."
            );
            EditorGUILayout.PropertyField(this._generateAll, label);

            label = new GUIContent
            (
                "Hotfix Compile",
                "Compile only the hotfix code, but make sure Generate All has been run at least once."
            );
            EditorGUILayout.PropertyField(this._hotfixCompile, label);

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            Color bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(255, 68, 133, 255);
            if (GUILayout.Button("Reload Package List"))
            {
                // 彈出確認對話框
                bool confirmBuild = EditorUtility.DisplayDialog(
                    "Confirm Reload",
                    "Are you sure you want to reload the [Package List] from the [AssetBundle Collector (by YooAsset)]?",
                    "yes",
                    "cancel"
                );

                if (confirmBuild)
                {
                    this._InitCollectorPackages(true);
                }
            }
            GUI.backgroundColor = bc;

            bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(255, 137, 68, 255);
            if (GUILayout.Button("Add Package"))
            {
                this.collectorPackages.Add(new PackageInfo());
                this._collectorPackagesPty = this._serObj.FindProperty("collectorPackages");
            }
            GUI.backgroundColor = bc;

            bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(231, 255, 68, 255);
            if (GUILayout.Button("Remove All"))
            {
                // 彈出確認對話框
                bool confirmBuild = EditorUtility.DisplayDialog(
                    "Confirm Remove",
                    "Are you sure you want to remove the entire package list?",
                    "yes",
                    "cancel"
                );

                if (confirmBuild)
                {
                    this.collectorPackages = new List<PackageInfo>();
                    this._collectorPackagesPty = this._serObj.FindProperty("collectorPackages");
                }
            }
            GUI.backgroundColor = bc;

            bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(109, 255, 182, 255);
            if (GUILayout.Button("Convert Package List to JSON"))
            {
                // 彈出確認對話框
                bool confirmBuild = EditorUtility.DisplayDialog(
                    "Confirm Convert",
                    "Are you sure you want to convert [Package List] to JSON?",
                    "yes",
                    "cancel"
                );

                if (confirmBuild)
                {
                    this._ConvertToJSON();
                }
            }
            GUI.backgroundColor = bc;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            this._scrollview1 = EditorGUILayout.BeginScrollView(this._scrollview1, true, true);

            // Collector Packages
            if (this.collectorPackages.Count > 0)
            {
                for (int i = 0; i < this._collectorPackagesPty.arraySize; i++)
                {
                    SerializedProperty packageProp = this._collectorPackagesPty.GetArrayElementAtIndex(i);

                    EditorGUILayout.BeginVertical();

                    // 顯示一般欄位
                    EditorGUILayout.PropertyField(packageProp.FindPropertyRelative("packageName"));
                    EditorGUILayout.PropertyField(packageProp.FindPropertyRelative("buildPipeline"));

                    // 顯示 Bundle Encryption 下拉選單
                    SerializedProperty encryptionClassNamesProp = packageProp.FindPropertyRelative("bundleEncryptionClassNames");
                    if (encryptionClassNamesProp != null && encryptionClassNamesProp.isArray)
                    {
                        string[] encryptionOptions = new string[encryptionClassNamesProp.arraySize];
                        for (int j = 0; j < encryptionClassNamesProp.arraySize; j++)
                        {
                            encryptionOptions[j] = encryptionClassNamesProp.GetArrayElementAtIndex(j).stringValue;
                        }

                        var selectedEncryptionIndex = EditorGUILayout.Popup("Bundle Encryption", this.collectorPackages[i].selectedBundleEncryptionIndex, encryptionOptions);
                        this.collectorPackages[i].selectedBundleEncryptionIndex = selectedEncryptionIndex;
                    }

                    // 顯示 Manifest Encryption 下拉選單
                    encryptionClassNamesProp = packageProp.FindPropertyRelative("manifestEncryptionClassNames");
                    if (encryptionClassNamesProp != null && encryptionClassNamesProp.isArray)
                    {
                        string[] encryptionOptions = new string[encryptionClassNamesProp.arraySize];
                        for (int j = 0; j < encryptionClassNamesProp.arraySize; j++)
                        {
                            encryptionOptions[j] = encryptionClassNamesProp.GetArrayElementAtIndex(j).stringValue;
                        }

                        var selectedEncryptionIndex = EditorGUILayout.Popup("Manifest Encryption", this.collectorPackages[i].selectedManifestEncryptionIndex, encryptionOptions);
                        this.collectorPackages[i].selectedManifestEncryptionIndex = selectedEncryptionIndex;
                    }

                    // 顯示一般欄位
                    EditorGUILayout.PropertyField(packageProp.FindPropertyRelative("compressOption"));
                    EditorGUILayout.PropertyField(packageProp.FindPropertyRelative("fileNameStyle"));
                    EditorGUILayout.PropertyField(packageProp.FindPropertyRelative("copyOption"));
                    EditorGUILayout.PropertyField(packageProp.FindPropertyRelative("clearBuildCacheFiles"));
                    EditorGUILayout.PropertyField(packageProp.FindPropertyRelative("useAssetDependencyDB"));

                    // 顯示輸出選項
                    EditorGUILayout.PropertyField(packageProp.FindPropertyRelative("exportType"));
                    if (this.collectorPackages[i].exportType == PackageInfo.ExportType.DLC)
                    {
                        EditorGUILayout.PropertyField(packageProp.FindPropertyRelative("withoutPlatform"));
                        EditorGUILayout.PropertyField(packageProp.FindPropertyRelative("dlcVersion"));
                    }

                    bc = GUI.backgroundColor;
                    GUI.backgroundColor = new Color32(68, 210, 255, 255);
                    if (GUILayout.Button("Remove"))
                    {
                        // 彈出確認對話框
                        bool confirmBuild = EditorUtility.DisplayDialog(
                            "Confirm Remove",
                            $"Are you sure you want to remove the package [{this.collectorPackages[i].packageName}] from the list?",
                            "yes",
                            "cancel"
                        );

                        if (confirmBuild)
                        {
                            this.collectorPackages.RemoveAt(i);
                            this._collectorPackagesPty = this._serObj.FindProperty("collectorPackages");
                        }

                        // 因為 break for 所以需要 end call
                        EditorGUILayout.EndVertical();
                        break;
                    }
                    GUI.backgroundColor = bc;

                    bc = GUI.backgroundColor;
                    GUI.backgroundColor = new Color32(109, 255, 182, 255);
                    if (GUILayout.Button("Convert to JSON"))
                    {
                        string packageName = this.collectorPackages[i].packageName;

                        // 彈出確認對話框
                        bool confirmBuild = EditorUtility.DisplayDialog(
                            "Confirm Convert",
                            $"Are you sure you want to convert only [{packageName}] to [JSON]?",
                            "yes",
                            "cancel"
                        );

                        if (confirmBuild)
                        {
                            this._ConvertToJSON(packageName);
                        }
                    }
                    GUI.backgroundColor = bc;

                    GUI.backgroundColor = new Color32(158, 139, 255, 255);
                    if (GUILayout.Button("Convert to JSON and Build by JSON"))
                    {
                        string packageName = this.collectorPackages[i].packageName;
                        EditorApplication.delayCall += () => this._BuildByJson(packageName, () => { this._ConvertToJSON(packageName); });
                    }
                    GUI.backgroundColor = bc;

                    EditorGUILayout.Space();
                    EditorGUILayout.EndVertical();
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                this._serObj.ApplyModifiedProperties();
                string json = JsonConvert.SerializeObject(this.groups);
                EditorStorage.SaveData(keySaver, "groups", json);
                EditorStorage.SaveData(keySaver, "generateAll", this.generateAll.ToString());
                EditorStorage.SaveData(keySaver, "hotfixCompile", this.hotfixCompile.ToString());
                EditorStorage.SaveData(keySaver, "includeConfigs", this.includeConfigs.ToString());
                json = JsonConvert.SerializeObject(this.collectorPackages);
                EditorStorage.SaveData(keySaver, "collectorPackages", json);
            }

            EditorGUILayout.EndScrollView();
        }

        private void _DrwaJsonClipboardView()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            // Label 靠左顯示
            GUIStyle coloredLabel = new GUIStyle(EditorStyles.boldLabel);
            coloredLabel.normal.textColor = Color.cyan;
            EditorGUILayout.LabelField("Paste bundle map JSON data below:", coloredLabel, GUILayout.Width(position.width - 420f));

            // 空間填充讓按鈕靠右顯示
            GUILayout.FlexibleSpace();

            Color bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(231, 255, 68, 255);
            if (GUILayout.Button("Clear JSON", GUILayout.MaxWidth(250f)))
            {
                // 讓 TextArea 失去焦點
                GUI.FocusControl(null);
                // 清空 JSON 內容
                this.jsonInput = string.Empty;
                EditorStorage.SaveString(keySaver + "jsonInput", this.jsonInput);
            }
            GUI.backgroundColor = bc;

            bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(197, 247, 255, 255);
            if (GUILayout.Button("JSON Format", GUILayout.MaxWidth(250f)))
            {
                // 讓 TextArea 失去焦點
                GUI.FocusControl(null);
                // 呼叫 JSON 格式化功能
                this.jsonInput = this._FormatJson(this.jsonInput);
                EditorStorage.SaveString(keySaver + "jsonInput", this.jsonInput);
            }
            GUI.backgroundColor = bc;

            bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(109, 255, 182, 255);
            if (GUILayout.Button("Convert JSON to Package List", GUILayout.MaxWidth(300f)))
            {
                // 彈出確認對話框
                bool confirmBuild = EditorUtility.DisplayDialog(
                    "Confirm Convert",
                    "Are you sure you want to convert [JSON] to [Package List]?",
                    "yes",
                    "cancel"
                );

                if (confirmBuild)
                {
                    // 讓 TextArea 失去焦點
                    GUI.FocusControl(null);
                    // 解析 JSON
                    ExportBundleMap exportBundleMap = JsonConvert.DeserializeObject<ExportBundleMap>(this.jsonInput);
                    this.includeConfigs = exportBundleMap.includeConfigs;
                    this.semanticRule.PATCH = exportBundleMap.includeSemanticPatch;
                    this.groups = exportBundleMap.groups;
                    this.generateAll = exportBundleMap.generateAll;
                    this.hotfixCompile = exportBundleMap.hotfixCompile;
                    this.collectorPackages = new List<PackageInfo>();
                    foreach (var pkg in exportBundleMap.packages)
                    {
                        PackageInfo packageInfo = new PackageInfo();

                        // 包裹名稱
                        packageInfo.packageName = pkg.packageName;

                        // 構建管線
                        EBuildPipeline buildPipeline;
                        if (pkg.buildPipeline == "BBP")
                            buildPipeline = EBuildPipeline.BuiltinBuildPipeline;
                        else if (pkg.buildPipeline == "SBP")
                            buildPipeline = EBuildPipeline.ScriptableBuildPipeline;
                        else if (pkg.buildPipeline == "RFBP")
                            buildPipeline = EBuildPipeline.RawFileBuildPipeline;
                        else
                            buildPipeline = EBuildPipeline.EditorSimulateBuildPipeline;
                        packageInfo.buildPipeline = buildPipeline;

                        // Bundle 加密方法
                        string[] encryptionClassNames = packageInfo.bundleEncryptionClassNames.ToArray();
                        for (int i = 0; i < encryptionClassNames.Length; i++)
                            encryptionClassNames[i] = encryptionClassNames[i].ToLower();
                        packageInfo.selectedBundleEncryptionIndex = Array.FindIndex(encryptionClassNames, e => e.Contains(pkg.bundleEncryptionArgs));

                        // Manifest 加密方法
                        encryptionClassNames = packageInfo.manifestEncryptionClassNames.ToArray();
                        for (int i = 0; i < encryptionClassNames.Length; i++)
                            encryptionClassNames[i] = encryptionClassNames[i].ToLower();
                        packageInfo.selectedManifestEncryptionIndex = Array.FindIndex(encryptionClassNames, e => e.Contains(pkg.manifestEncryptionArgs));

                        // 壓縮方法
                        packageInfo.compressOption = Enum.Parse<ECompressOption>(pkg.compressOption);

                        // 輸出名稱規則
                        packageInfo.fileNameStyle = Enum.Parse<EFileNameStyle>(pkg.fileNameStyle);

                        // 內置選項
                        packageInfo.copyOption = Enum.Parse<EBuildinFileCopyOption>(pkg.builtinCopyOption);

                        // 附加選項
                        packageInfo.clearBuildCacheFiles = pkg.clearBuildCacheFiles;
                        packageInfo.useAssetDependencyDB = pkg.useAssetDependencyDB;

                        // 輸出類型
                        string[] exportArgs = pkg.exportArgs.Split(",");
                        for (int i = 0; i < exportArgs.Length; i++)
                            exportArgs[i] = exportArgs[i].Trim();
                        packageInfo.exportType = Enum.Parse<PackageInfo.ExportType>(exportArgs[0]);
                        if (exportArgs.Length > 2)
                        {
                            packageInfo.withoutPlatform = Convert.ToBoolean(Convert.ToInt32(exportArgs[1]));
                            packageInfo.dlcVersion = exportArgs[2];
                        }

                        this.collectorPackages.Add(packageInfo);
                    }
                    this._collectorPackagesPty = this._serObj.FindProperty("collectorPackages");

                    EditorStorage.SaveData(keySaver, "includeConfigs", this.includeConfigs.ToString());
                    EditorStorage.SaveData(keySaver, "semanticRule.PATCH", this.semanticRule.PATCH.ToString());
                    string json = JsonConvert.SerializeObject(this.groups);
                    EditorStorage.SaveData(keySaver, "groups", json);
                    EditorStorage.SaveData(keySaver, "generateAll", this.generateAll.ToString());
                    EditorStorage.SaveData(keySaver, "hotfixCompile", this.hotfixCompile.ToString());
                    json = JsonConvert.SerializeObject(this.collectorPackages);
                    EditorStorage.SaveData(keySaver, "collectorPackages", json);
                }
            }
            GUI.backgroundColor = bc;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            this._scrollview2 = EditorGUILayout.BeginScrollView(this._scrollview2, true, true);

            if (string.IsNullOrEmpty(this.jsonInput))
            {
                EditorGUI.BeginChangeCheck();
                this.jsonInput = EditorGUILayout.TextArea(this.jsonInput, GUILayout.Height(position.height));
                if (EditorGUI.EndChangeCheck())
                    EditorStorage.SaveString(keySaver + "jsonInput", this.jsonInput);
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                this.jsonInput = EditorGUILayout.TextArea(this.jsonInput);
                if (EditorGUI.EndChangeCheck())
                    EditorStorage.SaveString(keySaver + "jsonInput", this.jsonInput);
            }

            EditorGUILayout.EndScrollView();
        }

        private void _DrawBuildButtonView()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            // Save File
            Color bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(255, 220, 0, 255);
            if (GUILayout.Button("Save JSON File", GUILayout.MaxWidth(150f)))
            {
                this._ExportJsonFile();
            }
            GUI.backgroundColor = bc;
            // Load File
            bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(0, 249, 255, 255);
            if (GUILayout.Button("Load JSON File", GUILayout.MaxWidth(150f)))
            {
                this._ImportJsonFile();
            }
            GUI.backgroundColor = bc;

            GUILayout.FlexibleSpace();

            // clear output path toggle
            this.isClearOutputPath = GUILayout.Toggle(this.isClearOutputPath, new GUIContent("Clear Ouput Path", "If checked, after process will clear folder."));
            EditorStorage.SaveData(keySaver, "isClearOutputPath", this.isClearOutputPath.ToString());

            // auto reveal toggle
            this.autoReveal = GUILayout.Toggle(this.autoReveal, new GUIContent("Auto Reveal", "If checked, after process will reveal folder."));
            EditorStorage.SaveData(keySaver, "autoReveal", this.autoReveal.ToString());

            // 顯示 Build 按鈕
            bc = GUI.backgroundColor;
            GUI.backgroundColor = new Color32(158, 139, 255, 255);

            if (GUILayout.Button("Build by JSON", GUILayout.MaxWidth(210f)))
            {
                EditorApplication.delayCall += () => this._BuildByJson(null);
            }
            GUI.backgroundColor = bc;

            EditorGUILayout.EndHorizontal();
        }

        private void _BuildByJson(string packageName, Action action = null)
        {
            // 彈出確認對話框
            bool confirmBuild = EditorUtility.DisplayDialog(
                "Confirm Build",
                string.IsNullOrEmpty(packageName) ? "Are you sure you want to start the build process?" : $"Are you sure you want to start the build process with only [{packageName}]?",
                "yes",
                "cancel"
            );

            if (confirmBuild)
            {
                // 開始構建
                try
                {
                    action?.Invoke();
                    BuildTool.BuildBundles(this.jsonInput, this.productName, this.appVersion, false, this.isClearOutputPath);
                    EditorUtility.DisplayDialog("Build Message", "All bundles builded.", "OK");
                    string exportFolder = Path.Combine(EditorTools.GetProjectPath(), "ExportBundles", PatchSetting.setting.rootFolderName);
                    if (this.autoReveal) EditorUtility.RevealInFinder(exportFolder);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);

                    if (ex.ToString().IndexOf("[ErrorCode115]") != -1)
                    {
                        Debug.LogWarning("The target folder with the same date and version already exists. Please wait 1 minute and try building again, or try enabling the [ClearBuildCacheFiles] option.");
                    }
                }
            }
        }

        /// <summary>
        /// JSON 格式化方法
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private string _FormatJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return "";

            try
            {
                var parsedJson = JsonConvert.DeserializeObject(json);
                return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
            }
            catch (JsonException e)
            {
                Debug.LogWarning("Invalid JSON format: " + e.Message);
                return json;
            }
        }

        private void _ExportJsonFile()
        {
            string savePath = EditorStorage.GetData(keySaver, $"buildJsonFilePath", Application.dataPath);
            var filePath = EditorUtility.SaveFilePanel("Save Bundle Map Json File", savePath, "BundleMap", "json");

            if (!string.IsNullOrEmpty(filePath))
            {
                EditorStorage.SaveData(keySaver, $"buildJsonFilePath", Path.GetDirectoryName(filePath));
                string json = this.jsonInput;
                this._WriteTxt(json, filePath);
                AssetDatabase.Refresh();
            }
        }

        private void _ImportJsonFile()
        {
            string loadPath = EditorStorage.GetData(keySaver, $"buildJsonFilePath", Application.dataPath);
            string filePath = EditorUtility.OpenFilePanel("Select Bundle Map Json File", !string.IsNullOrEmpty(loadPath) ? loadPath : Application.dataPath, "json");

            if (!string.IsNullOrEmpty(filePath))
            {
                EditorStorage.SaveData(keySaver, $"buildJsonFilePath", Path.GetDirectoryName(filePath));
                string json = File.ReadAllText(filePath);
                // 讓 TextArea 失去焦點
                GUI.FocusControl(null);
                this.jsonInput = json;

                // Resave
                EditorStorage.SaveString(keySaver + "jsonInput", this.jsonInput);
            }
        }

        /// <summary>
        /// 寫入文字文件檔
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="outputPath"></param>
        private void _WriteTxt(string txt, string outputPath)
        {
            // 寫入配置文件
            var file = File.CreateText(outputPath);
            file.Write(txt);
            file.Close();
        }
    }
}