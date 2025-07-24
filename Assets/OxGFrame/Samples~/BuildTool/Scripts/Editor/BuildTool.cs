using HybridCLR.Editor.Commands;
using HybridCLR.Editor.Installer;
using Newtonsoft.Json.Linq;
using OxGFrame.AssetLoader;
using OxGFrame.AssetLoader.Bundle;
using OxGFrame.AssetLoader.Editor;
using OxGFrame.Hotfixer.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using YooAsset;
using YooAsset.Editor;

namespace OxGFrame.Extensions.BuildTool.Editor
{
    public class TargetDevice
    {
        public class Android
        {
            public enum Index
            {
                ARMv7 = 0,
                ARM64 = 1,
                X86 = 2,
                X86_64 = 3
            }

            public static readonly AndroidArchitecture[] architectures = new AndroidArchitecture[]
            {
            AndroidArchitecture.ARMv7,
            AndroidArchitecture.ARM64,
            AndroidArchitecture.X86,
            AndroidArchitecture.X86_64
            };

            public class BuildAppMode
            {
                public const string APK = "APK";
                public const string AAB = "AAB";
            }
        }

        public class IOS
        {
            public enum Index
            {
                iPhoneOnly = 0,
                iPadOnly = 1,
                iPhoneAndiPad = 2
            }

            public static readonly iOSTargetDevice[] targetDevices = new iOSTargetDevice[]
            {
            iOSTargetDevice.iPhoneOnly,
            iOSTargetDevice.iPadOnly,
            iOSTargetDevice.iPhoneAndiPad
            };

            public class SDK
            {
                public const string DEVICE = "DEVICE";
                public const string SIMULATOR = "SIMULATOR";
            }
        }
    }

    public class ScriptingBackends
    {
        public const string MONO = "MONO";
        public const string IL2CPP = "IL2CPP";
    }

    public class IL2CppMode
    {
        public const string DEBUG = "DEBUG";
        public const string RELEASE = "RELEASE";
        public const string MASTER = "MASTER";
    }

    public class BuildMode
    {
        public const string NONE = "NONE";
        public const string DEBUG = "DEBUG";
    }

    public static class BuildTool
    {
        public static void HybridCLRInstaller()
        {
            var instanller = new InstallerController();

            // 檢測沒安裝時, 則進行 HybridCLR 的安裝
            if (!instanller.HasInstalledHybridCLR())
                instanller.InstallDefaultHybridCLR();
            // HybridCLR 版號不一致時, 也需要重新安裝
            else if (!instanller.PackageVersion.Equals(instanller.InstalledLibil2cppVersion))
                instanller.InstallDefaultHybridCLR();
        }

        public static void HybridCLRGenerateAll()
        {
            PrebuildCommand.GenerateAll();
        }

        public static void AppConfigGenerator(string productName, string appVersion, bool activeBuildTarget, BuildTarget buildTarget)
        {
            string outputPath = Application.streamingAssetsPath;
            BundleHelper.ExportAppConfig(productName, appVersion, outputPath, activeBuildTarget, buildTarget);
        }

        public static void BundleUrlConfigGenerator(string bundleIp, string fallbackBundleIp, string storeLink)
        {
            string outputPath = Application.streamingAssetsPath;
            BundleHelper.ExportBundleUrlConfig(bundleIp, fallbackBundleIp, storeLink, outputPath, true);
        }

        /// <summary>
        /// Build main app for CLI
        /// </summary>
        public static void BuildApp()
        {
            #region 1. 執行 HybridCLR installer
            HybridCLRInstaller();
            #endregion

            // 輸出路徑
            string fullOutPath, destination = CommandParser.GetArgument(CommandParser.ArgName.destination);
            if (string.IsNullOrEmpty(destination))
                fullOutPath = Path.Combine(EditorTools.GetProjectPath(), "Build");
            else
                fullOutPath = Path.Combine(Application.dataPath, destination);
            // 去除副檔名以獲取文件夾路徑
            string directoryPath = Path.GetDirectoryName(fullOutPath);
            // 創建目錄（如果不存在）
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            // 版本
            string buildVersion = CommandParser.GetArgument(CommandParser.ArgName.buildVersion);
            if (!string.IsNullOrEmpty(buildVersion))
                PlayerSettings.bundleVersion = buildVersion;

            // 平台目標設定
            BuildTargetGroup group = BuildTargetGroup.Unknown;
            BuildTarget target = BuildTarget.StandaloneWindows;
            string scriptingDefineSymbols = CommandParser.GetArgument(CommandParser.ArgName.defineSymbols);

            // 平台選項 (使用 CLI 需透過 -buildTarget 參數切換平台)
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.StandaloneWindows:
                    group = BuildTargetGroup.Standalone;
                    target = BuildTarget.StandaloneWindows;
                    // Windows => Export type
                    {
#if UNITY_STANDALONE_WIN
                    UnityEditor.WindowsStandalone.UserBuildSettings.createSolution = false;
#endif
                    }
                    break;
                case BuildTarget.StandaloneWindows64:
                    group = BuildTargetGroup.Standalone;
                    target = BuildTarget.StandaloneWindows64;
                    // Windows => Export type
                    {
#if UNITY_STANDALONE_WIN
                    UnityEditor.WindowsStandalone.UserBuildSettings.createSolution = false;
#endif
                    }
                    break;
                case BuildTarget.StandaloneOSX:
                    group = BuildTargetGroup.Standalone;
                    target = BuildTarget.StandaloneOSX;
                    // OSX => Export type
                    {
                        // 設置輸出 Xcode 項目 (ref: https://discussions.unity.com/t/building-xcode-project-through-editor-script/802920)
                        EditorUserBuildSettings.SetPlatformSettings("OSXUniversal", "CreateXcodeProject", "true");
                    }
                    // OSX => Architecture
                    {
                        // 默認為 Universal 架構 (ref: https://discussions.unity.com/t/building-for-mac-as-a-intel-x64/827920/2)
                        EditorUserBuildSettings.SetPlatformSettings("OSXUniversal", "Architecture", "x64ARM64");
                    }
                    // OSX => Build Version Code (Only for OSX)
                    {
                        string versionCode = CommandParser.GetArgument(CommandParser.ArgName.versionCode);
                        if (string.IsNullOrEmpty(versionCode))
                            versionCode = "0";
                        int vCode = Convert.ToInt32(versionCode);
                        if (vCode <= 0)
                            vCode = 1;
                        PlayerSettings.macOS.buildNumber = vCode.ToString();
                    }
                    break;
                case BuildTarget.Android:
                    group = BuildTargetGroup.Android;
                    target = BuildTarget.Android;
                    // Android => Target Architectures
                    {
                        string targetDevice = CommandParser.GetArgument(CommandParser.ArgName.targetDevice);
                        if (string.IsNullOrEmpty(targetDevice))
                            targetDevice = ((int)TargetDevice.Android.Index.ARM64).ToString();
                        AndroidArchitecture architexture = AndroidArchitecture.None;
                        string[] args = targetDevice.Replace(" ", string.Empty).Split(',');
                        foreach (string arg in args)
                        {
                            int idx = Convert.ToInt32(arg);
                            if (idx > TargetDevice.Android.architectures.Length)
                                idx = TargetDevice.Android.architectures.Length - 1;
                            architexture |= TargetDevice.Android.architectures[idx];
                        }

                        PlayerSettings.Android.targetArchitectures = architexture;
                    }
                    // Android => Bundle Version Code (Only for Android)
                    {
                        string versionCode = CommandParser.GetArgument(CommandParser.ArgName.versionCode);
                        if (string.IsNullOrEmpty(versionCode))
                            versionCode = "0";
                        int vCode = Convert.ToInt32(versionCode);
                        if (vCode <= 0)
                            vCode = 1;
                        PlayerSettings.Android.bundleVersionCode = vCode;
                    }
                    // Android => keystore for Google Play
                    {
                        string androidKeystoreArgs = CommandParser.GetArgument(CommandParser.ArgName.androidKeystoreArgs);
                        if (!string.IsNullOrEmpty(androidKeystoreArgs))
                        {
                            string[] args = androidKeystoreArgs.Trim().Split(',');
                            for (int i = 0; i < args.Length; i++)
                                args[i] = args[i].Trim();
                            // set args for keystore
                            if (args.Length >= 4)
                            {
                                PlayerSettings.Android.useCustomKeystore = true;
                                PlayerSettings.Android.keystoreName = Path.Combine(Application.dataPath, args[0]);
                                PlayerSettings.Android.keystorePass = args[1];
                                PlayerSettings.Android.keyaliasName = args[2];
                                PlayerSettings.Android.keyaliasPass = args[3];
                            }
                        }
                        else PlayerSettings.Android.useCustomKeystore = false;
                    }
                    // Android => enable build aab
                    {
                        string enableAndroidAppBundle = CommandParser.GetArgument(CommandParser.ArgName.enableAndroidAppBundle);
                        if (!string.IsNullOrEmpty(enableAndroidAppBundle))
                        {
                            bool enable = Convert.ToBoolean(Convert.ToInt32(enableAndroidAppBundle));
                            EditorUserBuildSettings.buildAppBundle = enable;
                            PlayerSettings.Android.useAPKExpansionFiles = enable;
                        }
                        else
                        {
                            EditorUserBuildSettings.buildAppBundle = false;
                            PlayerSettings.Android.useAPKExpansionFiles = false;
                        }
                    }
                    break;
                case BuildTarget.iOS:
                    group = BuildTargetGroup.iOS;
                    target = BuildTarget.iOS;
                    // iOS => Export type
                    {
                        EditorUserBuildSettings.SetPlatformSettings("iOS", "CreateXcodeProject", "true");
                    }
                    // iOS => Target Device
                    {
                        string targetDevice = CommandParser.GetArgument(CommandParser.ArgName.targetDevice);
                        if (string.IsNullOrEmpty(targetDevice))
                            targetDevice = ((int)TargetDevice.IOS.Index.iPhoneOnly).ToString();
                        iOSTargetDevice iosTargetDevice = 0;
                        string[] args = targetDevice.Replace(" ", string.Empty).Split(',');
                        foreach (string arg in args)
                        {
                            int idx = Convert.ToInt32(arg);
                            if (idx > TargetDevice.IOS.targetDevices.Length) idx = TargetDevice.IOS.targetDevices.Length - 1;
                            iosTargetDevice |= TargetDevice.IOS.targetDevices[idx];
                        }

                        PlayerSettings.iOS.targetDevice = iosTargetDevice;
                    }
                    // iOS => Target SDK
                    {
                        string iosSdkVersion = CommandParser.GetArgument(CommandParser.ArgName.iosSdkVersion);
                        if (string.IsNullOrEmpty(iosSdkVersion))
                            iosSdkVersion = TargetDevice.IOS.SDK.DEVICE;
                        iosSdkVersion = iosSdkVersion.ToUpper();
                        switch (iosSdkVersion)
                        {
                            case TargetDevice.IOS.SDK.DEVICE:
                                PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
                                break;
                            case TargetDevice.IOS.SDK.SIMULATOR:
                                PlayerSettings.iOS.sdkVersion = iOSSdkVersion.SimulatorSDK;
                                break;
                        }
                    }
                    // iOS => Build Version Code (Only for iOS)
                    {
                        string versionCode = CommandParser.GetArgument(CommandParser.ArgName.versionCode);
                        if (string.IsNullOrEmpty(versionCode))
                            versionCode = "0";
                        int vCode = Convert.ToInt32(versionCode);
                        if (vCode <= 0)
                            vCode = 1;
                        PlayerSettings.iOS.buildNumber = vCode.ToString();
                    }
                    break;
                case BuildTarget.WebGL:
                    group = BuildTargetGroup.WebGL;
                    target = BuildTarget.WebGL;
                    break;
            }

            // 編譯模式 (預先配置好的選項)
            BuildOptions buildOptions = BuildOptions.None;
            string buildMode = CommandParser.GetArgument(CommandParser.ArgName.buildMode);
            if (string.IsNullOrEmpty(buildMode))
                buildMode = BuildMode.NONE;
            buildMode = buildMode.ToUpper();
            switch (buildMode)
            {
                case BuildMode.NONE:
                    break;
                case BuildMode.DEBUG:
                    buildOptions =
                        BuildOptions.Development |
                        BuildOptions.ConnectWithProfiler |
                        BuildOptions.EnableDeepProfilingSupport;
                    break;
            }

            // 後端編譯類型
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneOSX:
                case BuildTarget.Android:
                case BuildTarget.WebGL:
                    // Scripting Backends
                    {
                        string scriptingBackends = CommandParser.GetArgument(CommandParser.ArgName.scriptingBackends);
                        if (string.IsNullOrEmpty(scriptingBackends))
                            scriptingBackends = ScriptingBackends.IL2CPP;
                        scriptingBackends = scriptingBackends.ToUpper();
                        switch (scriptingBackends)
                        {
                            case ScriptingBackends.MONO:
                                // 使用 Mono 為後端編譯類型
                                PlayerSettings.SetScriptingBackend(group, ScriptingImplementation.Mono2x);
                                break;
                            case ScriptingBackends.IL2CPP:
                                // 使用 IL2CPP 為後端編譯類型
                                {
                                    PlayerSettings.SetScriptingBackend(group, ScriptingImplementation.IL2CPP);
                                }
                                // IL2CPP 編譯模式
                                {
                                    string il2cppMode = CommandParser.GetArgument(CommandParser.ArgName.il2CppConfiguration);
                                    if (string.IsNullOrEmpty(il2cppMode))
                                        il2cppMode = IL2CppMode.RELEASE;
                                    il2cppMode = il2cppMode.ToUpper();
                                    switch (il2cppMode)
                                    {
                                        case IL2CppMode.RELEASE:
                                            PlayerSettings.SetIl2CppCompilerConfiguration(group, Il2CppCompilerConfiguration.Release);
                                            break;
                                        case IL2CppMode.MASTER:
                                            PlayerSettings.SetIl2CppCompilerConfiguration(group, Il2CppCompilerConfiguration.Master);
                                            break;
                                    }
                                }
                                break;
                        }
                    }
                    break;
            }

            // 公司名稱
            string companyName = CommandParser.GetArgument(CommandParser.ArgName.companyName);
            if (!string.IsNullOrEmpty(companyName))
                PlayerSettings.companyName = companyName;

            // 專案名稱 (應用程式安裝名稱)
            string displayProductName = CommandParser.GetArgument(CommandParser.ArgName.displayProductName);
            if (!string.IsNullOrEmpty(displayProductName))
                PlayerSettings.productName = displayProductName;

            // 輸出 appconfig.json 至 StreamingAssets
            string productName = CommandParser.GetArgument(CommandParser.ArgName.productName);
            if (string.IsNullOrEmpty(productName))
                productName = "anonymous";
            AppConfigGenerator(productName, buildVersion, true, target);

            // 輸出 burlconfig.conf 至 StreamingAssets
            string urlParams = CommandParser.GetArgument(CommandParser.ArgName.bundleUrlParams);
            if (string.IsNullOrEmpty(urlParams))
                urlParams = "http://127.0.0.1, http://127.0.0.1, https://";
            string[] urlArgs = urlParams.Split(',');
            for (int i = 0; i < urlArgs.Length; i++)
                urlArgs[i] = urlArgs[i].Trim();
            BundleUrlConfigGenerator(urlArgs[0], urlArgs[1], urlArgs[2]);

            // 應用程式識別名稱
            string identifierName = CommandParser.GetArgument(CommandParser.ArgName.identifierName);
            if (!string.IsNullOrEmpty(identifierName))
                PlayerSettings.SetApplicationIdentifier(group, identifierName);

            // 關閉 SplashScreen
            if (Application.HasProLicense())
            {
                Debug.Log("UNITY IS PRO!\n Deactivating SplashScreen");
                PlayerSettings.SplashScreen.show = false;
                PlayerSettings.SplashScreen.showUnityLogo = false;
            }

            // 設置 Symbols
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, scriptingDefineSymbols);

            // 設置 Stripping level
            string strippingLevel = CommandParser.GetArgument(CommandParser.ArgName.strippingLevel);
            if (!string.IsNullOrEmpty(strippingLevel))
            {
                int level = Convert.ToInt32(strippingLevel);
                PlayerSettings.SetManagedStrippingLevel(group, (ManagedStrippingLevel)level);
            }

            Debug.Log($"Current stripping level: {PlayerSettings.GetManagedStrippingLevel(group)}");

            #region 2. Generate All
            HybridCLRGenerateAll();
            #endregion

            #region 3. 構建 Buitlin-Bundles
            string bundleMap = CommandParser.GetArgument(CommandParser.ArgName.bundleMap);
            // 會自動略過非內置資源的打包
            BuildBundles(bundleMap, productName, buildVersion, true, true);
            #endregion

            #region 4. 構建 IWriteFileProcess
            // 獲取 Domain 中實作 IWriteFileProcess 的接口
            var classTypes = EditorTools.GetAssignableTypes(typeof(IWriteFileProcess));
            foreach (var t in classTypes)
            {
                var obj = Activator.CreateInstance(t);
                if (obj is IWriteFileProcess process)
                    process.WriteFile();
            }
            #endregion

            #region 5. 構建 App
            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, fullOutPath, target, buildOptions);
            #endregion
        }

        /// <summary>
        /// Build bundles for CLI
        /// </summary>
        public static void BuildBundles()
        {
            // Bundle build map
            string bundleMap = CommandParser.GetArgument(CommandParser.ArgName.bundleMap);
            // 產品名稱
            string productName = CommandParser.GetArgument(CommandParser.ArgName.productName);
            // 版本
            string buildVersion = CommandParser.GetArgument(CommandParser.ArgName.buildVersion);
            // 開始構建
            BuildBundles(bundleMap, productName, buildVersion, false, true);
        }

        /// <summary>
        /// YooAsset build bundles
        /// </summary>
        /// <param name="bundleMap"></param>
        /// <param name="productName"></param>
        /// <param name="buildVersion"></param>
        /// <param name="onlyBuiltin"> 是否只處理內置資源打包 (true = 將不會輸出 CDN 文件夾) </param>
        /// <param name="isClearOutputPath"></param>
        /// <exception cref="ArgumentException"></exception>
        public static void BuildBundles(string bundleMap, string productName, string buildVersion, bool onlyBuiltin, bool isClearOutputPath)
        {
            // bundle map json main keys
            const string keyIncludeConfigs = "includeConfigs";
            const string keyIncludeSemanticPatch = "includeSemanticPatch";
            const string keyGroups = "groups";
            const string keyGenerateAll = "generateAll";
            const string keyHotfixCompile = "hotfixCompile";
            const string keyPackages = "packages";

            // 檢查是否執行 hotfix 編譯
            bool? generateAll = JObject.Parse(bundleMap).SelectToken(keyGenerateAll)?.Value<Boolean>();
            bool? hotfixCompile = JObject.Parse(bundleMap).SelectToken(keyHotfixCompile)?.Value<Boolean>();

            #region 1. 執行 HybridCLR installer
            if (generateAll != null && (bool)generateAll)
            {
                HybridCLRInstaller();
            }
            #endregion

            /**
             * 必須執行平台切換 (使用 CLI 透過 -buildTarget 參數切換平台)
             */

            // 平台目標 Symbols 設定
            BuildTargetGroup group = EditorUserBuildSettings.selectedBuildTargetGroup;
            string scriptingDefineSymbols = CommandParser.GetArgument(CommandParser.ArgName.defineSymbols);
            if (!string.IsNullOrEmpty(scriptingDefineSymbols))
                // 設置 Symbols
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, scriptingDefineSymbols);

            #region 2. Generate All
            if (generateAll != null && (bool)generateAll)
            {
                HybridCLRGenerateAll();
            }
            #endregion

            #region 3. HybridCLR hotfix compile
            if (hotfixCompile != null && (bool)hotfixCompile)
            {
                HotfixHelper.CompileAndCopyToHotfixCollector();
            }
            #endregion

            #region 4. 構建 Bundles
            // Build map (json)
            if (string.IsNullOrEmpty(bundleMap))
                throw new ArgumentException("The argument '-bundleMap' cannot be null or empty.");

            StringBuilder successfulResultsBuilder = new StringBuilder();
            StringBuilder failedResultsBuilder = new StringBuilder();

            Debug.Log($"Begin bundle build: {EditorUserBuildSettings.activeBuildTarget}");

            // Bundle build map from json
            JArray packages = JObject.Parse(bundleMap).SelectToken(keyPackages)?.Value<JArray>();
            if (packages != null && packages.Count > 0)
            {
                foreach (var pkg in packages)
                {
                    var buildoutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
                    var streamingAssetsRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();

                    // 取得 package 的各個屬性
                    string packageName = pkg["packageName"]?.Value<string>();
                    string buildPipeline = pkg["buildPipeline"]?.Value<string>();
                    string bundleEncryptionArgs = pkg["bundleEncryptionArgs"]?.Value<string>();
                    string manifestEncryptionArgs = pkg["manifestEncryptionArgs"]?.Value<string>();
                    string compressOption = pkg["compressOption"]?.Value<string>();
                    string fileNameStyle = pkg["fileNameStyle"]?.Value<string>();
                    string builtinCopyOption = pkg["builtinCopyOption"]?.Value<string>();

                    // 參數
                    string[] args;

                    // Bundle 加密方式
                    if (string.IsNullOrEmpty(bundleEncryptionArgs))
                        bundleEncryptionArgs = BundleConfig.CryptogramType.NONE;
                    args = bundleEncryptionArgs.Trim().Split(',');
                    for (int i = 0; i < args.Length; i++)
                        args[i] = args[i].Trim();
                    string encryptionType = args[0].ToUpper();
                    IEncryptionServices bundleEncryptionServices = null;
                    switch (encryptionType)
                    {
                        case BundleConfig.CryptogramType.NONE:
                            bundleEncryptionServices = null;
                            break;
                        case BundleConfig.CryptogramType.OFFSET:
                            if (args.Length > 1)
                                bundleEncryptionServices = new OffsetEncryption(Convert.ToInt32(args[1]));
                            else
                                bundleEncryptionServices = new OffsetEncryption();
                            break;
                        case BundleConfig.CryptogramType.XOR:
                            if (args.Length > 1)
                                bundleEncryptionServices = new XorEncryption(Convert.ToByte(args[1]));
                            else
                                bundleEncryptionServices = new XorEncryption();
                            break;
                        case BundleConfig.CryptogramType.HT2XOR:
                            if (args.Length > 3)
                                bundleEncryptionServices = new HT2XorEncryption(Convert.ToByte(args[1]), Convert.ToByte(args[2]), Convert.ToByte(args[3]));
                            else
                                bundleEncryptionServices = new HT2XorEncryption();
                            break;
                        case BundleConfig.CryptogramType.HT2XORPLUS:
                            if (args.Length > 4)
                                bundleEncryptionServices = new HT2XorPlusEncryption(Convert.ToByte(args[1]), Convert.ToByte(args[2]), Convert.ToByte(args[3]), Convert.ToByte(args[4]));
                            else
                                bundleEncryptionServices = new HT2XorPlusEncryption();
                            break;
                        case BundleConfig.CryptogramType.AES:
                            if (args.Length > 2)
                                bundleEncryptionServices = new AesEncryption(args[1], args[2]);
                            else
                                bundleEncryptionServices = new AesEncryption();
                            break;
                        case BundleConfig.CryptogramType.CHACHA20:
                            if (args.Length > 3)
                                bundleEncryptionServices = new ChaCha20Encryption(args[1], args[2], Convert.ToUInt32(args[3]));
                            else
                                bundleEncryptionServices = new ChaCha20Encryption();
                            break;
                        case BundleConfig.CryptogramType.XXTEA:
                            if (args.Length > 1)
                                bundleEncryptionServices = new XXTEAEncryption(args[1]);
                            else
                                bundleEncryptionServices = new XXTEAEncryption();
                            break;
                        case BundleConfig.CryptogramType.OFFSETXOR:
                            if (args.Length > 2)
                                bundleEncryptionServices = new OffsetXorEncryption(Convert.ToByte(args[1]), Convert.ToInt32(args[2]));
                            else
                                bundleEncryptionServices = new OffsetXorEncryption();
                            break;
                    }

                    // Manifest 加密方式
                    if (string.IsNullOrEmpty(manifestEncryptionArgs))
                        manifestEncryptionArgs = BundleConfig.CryptogramType.NONE;
                    args = manifestEncryptionArgs.Trim().Split(',');
                    for (int i = 0; i < args.Length; i++)
                        args[i] = args[i].Trim();
                    encryptionType = args[0].ToUpper();
                    IManifestProcessServices manifestEncryptionServices = null;
                    IManifestRestoreServices manifestDecryptionServices = null;
                    switch (encryptionType)
                    {
                        case BundleConfig.CryptogramType.NONE:
                            manifestEncryptionServices = null;
                            manifestDecryptionServices = null;
                            break;
                        case BundleConfig.CryptogramType.OFFSET:
                            if (args.Length > 1)
                            {
                                manifestEncryptionServices = new ManifestOffsetEncryption(Convert.ToInt32(args[1]));
                                manifestDecryptionServices = new OffsetDecryption(Convert.ToInt32(args[1]));
                            }
                            else
                            {
                                manifestEncryptionServices = new ManifestOffsetEncryption();
                                manifestDecryptionServices = new OffsetDecryption();
                            }
                            break;
                        case BundleConfig.CryptogramType.XOR:
                            if (args.Length > 1)
                            {
                                manifestEncryptionServices = new ManifestXorEncryption(Convert.ToByte(args[1]));
                                manifestDecryptionServices = new XorDecryption(Convert.ToByte(args[1]));
                            }
                            else
                            {
                                manifestEncryptionServices = new ManifestXorEncryption();
                                manifestDecryptionServices = new XorDecryption();
                            }
                            break;
                        case BundleConfig.CryptogramType.HT2XOR:
                            if (args.Length > 3)
                            {
                                manifestEncryptionServices = new ManifestHT2XorEncryption(Convert.ToByte(args[1]), Convert.ToByte(args[2]), Convert.ToByte(args[3]));
                                manifestDecryptionServices = new HT2XorDecryption(Convert.ToByte(args[1]), Convert.ToByte(args[2]), Convert.ToByte(args[3]));
                            }
                            else
                            {
                                manifestEncryptionServices = new ManifestHT2XorEncryption();
                                manifestDecryptionServices = new HT2XorDecryption();
                            }
                            break;
                        case BundleConfig.CryptogramType.HT2XORPLUS:
                            if (args.Length > 4)
                            {
                                manifestEncryptionServices = new ManifestHT2XorPlusEncryption(Convert.ToByte(args[1]), Convert.ToByte(args[2]), Convert.ToByte(args[3]), Convert.ToByte(args[4]));
                                manifestDecryptionServices = new HT2XorPlusDecryption(Convert.ToByte(args[1]), Convert.ToByte(args[2]), Convert.ToByte(args[3]), Convert.ToByte(args[4]));
                            }
                            else
                            {
                                manifestEncryptionServices = new ManifestHT2XorPlusEncryption();
                                manifestDecryptionServices = new HT2XorPlusDecryption();
                            }
                            break;
                        case BundleConfig.CryptogramType.AES:
                            if (args.Length > 2)
                            {
                                manifestEncryptionServices = new ManifestAesEncryption(args[1], args[2]);
                                manifestDecryptionServices = new AesDecryption(args[1], args[2]);
                            }
                            else
                            {
                                manifestEncryptionServices = new ManifestAesEncryption();
                                manifestDecryptionServices = new AesDecryption();
                            }
                            break;
                        case BundleConfig.CryptogramType.CHACHA20:
                            if (args.Length > 3)
                            {
                                manifestEncryptionServices = new ManifestChaCha20Encryption(args[1], args[2], Convert.ToUInt32(args[3]));
                                manifestDecryptionServices = new ChaCha20Decryption(args[1], args[2], Convert.ToUInt32(args[3]));
                            }
                            else
                            {
                                manifestEncryptionServices = new ManifestChaCha20Encryption();
                                manifestDecryptionServices = new ChaCha20Decryption();
                            }
                            break;
                        case BundleConfig.CryptogramType.XXTEA:
                            if (args.Length > 1)
                            {
                                manifestEncryptionServices = new ManifestXXTEAEncryption(args[1]);
                                manifestDecryptionServices = new XXTEADecryption(args[1]);
                            }
                            else
                            {
                                manifestEncryptionServices = new ManifestXXTEAEncryption();
                                manifestDecryptionServices = new XXTEADecryption();
                            }
                            break;
                        case BundleConfig.CryptogramType.OFFSETXOR:
                            if (args.Length > 2)
                            {
                                manifestEncryptionServices = new ManifestOffsetXorEncryption(Convert.ToByte(args[1]), Convert.ToInt32(args[2]));
                                manifestDecryptionServices = new OffsetXorDecryption(Convert.ToByte(args[1]), Convert.ToInt32(args[2]));
                            }
                            else
                            {
                                manifestEncryptionServices = new ManifestOffsetXorEncryption();
                                manifestDecryptionServices = new OffsetXorDecryption();
                            }
                            break;
                    }

                    // Bundle 壓縮類型
                    ECompressOption bundleCompression = ECompressOption.LZ4;
                    switch (compressOption)
                    {
                        case nameof(ECompressOption.Uncompressed):
                            bundleCompression = ECompressOption.Uncompressed;
                            break;
                        case nameof(ECompressOption.LZMA):
                            bundleCompression = ECompressOption.LZMA;
                            break;
                        case nameof(ECompressOption.LZ4):
                            bundleCompression = ECompressOption.LZ4;
                            break;
                    }

                    // Bundle 命名方式
                    EFileNameStyle bundleNameStyle = EFileNameStyle.HashName;
                    switch (fileNameStyle)
                    {
                        case nameof(EFileNameStyle.HashName):
                            bundleNameStyle = EFileNameStyle.HashName;
                            break;
                        case nameof(EFileNameStyle.BundleName):
                            bundleNameStyle = EFileNameStyle.BundleName;
                            break;
                        case nameof(EFileNameStyle.BundleName_HashName):
                            bundleNameStyle = EFileNameStyle.BundleName_HashName;
                            break;
                    }

                    // 輸出類型 (Builtin or CDN)
                    EBuildinFileCopyOption bundleCopyOption = EBuildinFileCopyOption.None;
                    switch (builtinCopyOption)
                    {
                        case nameof(EBuildinFileCopyOption.None):
                            bundleCopyOption = EBuildinFileCopyOption.None;
                            break;
                        case nameof(EBuildinFileCopyOption.ClearAndCopyAll):
                            bundleCopyOption = EBuildinFileCopyOption.ClearAndCopyAll;
                            break;
                        case nameof(EBuildinFileCopyOption.ClearAndCopyByTags):
                            bundleCopyOption = EBuildinFileCopyOption.ClearAndCopyByTags;
                            break;
                        case nameof(EBuildinFileCopyOption.OnlyCopyAll):
                            bundleCopyOption = EBuildinFileCopyOption.OnlyCopyAll;
                            break;
                        case nameof(EBuildinFileCopyOption.OnlyCopyByTags):
                            bundleCopyOption = EBuildinFileCopyOption.OnlyCopyByTags;
                            break;
                    }

                    // 僅進行內置資源的打包
                    if (onlyBuiltin)
                    {
                        // 非內置選項直接跳過
                        if (bundleCopyOption == EBuildinFileCopyOption.None)
                            continue;
                    }

                    // 構建模式
                    buildPipeline = buildPipeline.ToUpper();
                    switch (buildPipeline)
                    {
                        // BuiltinBuildPipeline
                        case "BBP":
                            {
                                // 構建參數
                                BuiltinBuildParameters buildParameters = new BuiltinBuildParameters();
                                buildParameters.BuildOutputRoot = buildoutputRoot;
                                buildParameters.BuildinFileRoot = streamingAssetsRoot;
                                buildParameters.BuildPipeline = EBuildPipeline.ScriptableBuildPipeline.ToString();
                                buildParameters.BuildBundleType = (int)EBuildBundleType.AssetBundle;
                                buildParameters.BuildTarget = EditorUserBuildSettings.activeBuildTarget;
                                buildParameters.PackageName = packageName;
                                buildParameters.PackageVersion = GetDefaultPackageVersion();
                                buildParameters.VerifyBuildingResult = true;
                                buildParameters.EnableSharePackRule = true;
                                buildParameters.EncryptionServices = bundleEncryptionServices;
                                buildParameters.ManifestProcessServices = manifestEncryptionServices;
                                buildParameters.ManifestRestoreServices = manifestDecryptionServices;
                                buildParameters.CompressOption = bundleCompression;
                                buildParameters.FileNameStyle = bundleNameStyle;
                                buildParameters.BuildinFileCopyOption = bundleCopyOption;
                                buildParameters.BuildinFileCopyParams = string.Empty;
                                buildParameters.ClearBuildCacheFiles = false; // 不清理构建缓存，启用增量构建，可以提高打包速度！
                                buildParameters.UseAssetDependencyDB = true;  // 使用资源依赖关系数据库，可以提高打包速度！

                                // 執行構建
                                BuiltinBuildPipeline pipeline = new BuiltinBuildPipeline();
                                var buildResult = pipeline.Run(buildParameters, true);
                                if (buildResult.Success)
                                {
                                    successfulResultsBuilder.Append($"<color=#a6ff58>Build successful: {buildResult.OutputPackageDirectory}</color>\n");
                                }
                                else
                                {
                                    failedResultsBuilder.Append($"Build failed: {buildResult.ErrorInfo}\n");
                                }
                            }
                            break;

                        // ScriptableBuildPipeline
                        default:
                        case "SBP":
                            {
                                // 構建參數
                                ScriptableBuildParameters buildParameters = new ScriptableBuildParameters();
                                buildParameters.BuildOutputRoot = buildoutputRoot;
                                buildParameters.BuildinFileRoot = streamingAssetsRoot;
                                buildParameters.BuildPipeline = EBuildPipeline.ScriptableBuildPipeline.ToString();
                                buildParameters.BuildBundleType = (int)EBuildBundleType.AssetBundle;
                                buildParameters.BuildTarget = EditorUserBuildSettings.activeBuildTarget;
                                buildParameters.PackageName = packageName;
                                buildParameters.PackageVersion = GetDefaultPackageVersion();
                                buildParameters.VerifyBuildingResult = true;
                                buildParameters.EnableSharePackRule = true;
                                buildParameters.EncryptionServices = bundleEncryptionServices;
                                buildParameters.ManifestProcessServices = manifestEncryptionServices;
                                buildParameters.ManifestRestoreServices = manifestDecryptionServices;
                                buildParameters.CompressOption = bundleCompression;
                                buildParameters.FileNameStyle = bundleNameStyle;
                                buildParameters.BuildinFileCopyOption = bundleCopyOption;
                                buildParameters.BuildinFileCopyParams = string.Empty;
                                buildParameters.ClearBuildCacheFiles = false; // 不清理构建缓存，启用增量构建，可以提高打包速度！
                                buildParameters.UseAssetDependencyDB = true;  // 使用资源依赖关系数据库，可以提高打包速度！
                                buildParameters.BuiltinShadersBundleName = _GetBuiltinShaderBundleName(packageName);

                                // 執行構建
                                ScriptableBuildPipeline pipeline = new ScriptableBuildPipeline();
                                var buildResult = pipeline.Run(buildParameters, true);
                                if (buildResult.Success)
                                {
                                    successfulResultsBuilder.Append($"<color=#06ff88>Build successful: {buildResult.OutputPackageDirectory}, BuildPipeline: <color=#ff69aa>{buildPipeline}</color>, Encryption: <color=#ff8806>{bundleEncryptionServices?.GetType()?.Name}</color>, CompressOption: <color=#eaff06>{bundleCompression}</color>, FileNameStyle: <color=#06f8ff>{bundleNameStyle}</color>, BuildinFileCopyOption: <color=#c263ff>{bundleCopyOption}</color></color>\n");
                                }
                                else
                                {
                                    failedResultsBuilder.Append($"Build failed: {buildResult.ErrorInfo}\n");
                                }
                            }
                            break;

                        // RawFileBuildPipeline
                        case "RFBP":
                            {
                                // 構建參數
                                RawFileBuildParameters buildParameters = new RawFileBuildParameters();
                                buildParameters.BuildOutputRoot = buildoutputRoot;
                                buildParameters.BuildinFileRoot = streamingAssetsRoot;
                                buildParameters.BuildPipeline = EBuildPipeline.ScriptableBuildPipeline.ToString();
                                buildParameters.BuildBundleType = (int)EBuildBundleType.RawBundle;
                                buildParameters.BuildTarget = EditorUserBuildSettings.activeBuildTarget;
                                buildParameters.PackageName = packageName;
                                buildParameters.PackageVersion = GetDefaultPackageVersion();
                                buildParameters.VerifyBuildingResult = true;
                                buildParameters.EnableSharePackRule = true;
                                buildParameters.EncryptionServices = bundleEncryptionServices;
                                buildParameters.ManifestProcessServices = manifestEncryptionServices;
                                buildParameters.ManifestRestoreServices = manifestDecryptionServices;
                                buildParameters.FileNameStyle = bundleNameStyle;
                                buildParameters.BuildinFileCopyOption = bundleCopyOption;
                                buildParameters.BuildinFileCopyParams = string.Empty;
                                buildParameters.ClearBuildCacheFiles = false; // 不清理构建缓存，启用增量构建，可以提高打包速度！
                                buildParameters.UseAssetDependencyDB = true;  // 使用资源依赖关系数据库，可以提高打包速度！

                                // 執行構建
                                RawFileBuildPipeline pipeline = new RawFileBuildPipeline();
                                var buildResult = pipeline.Run(buildParameters, true);
                                if (buildResult.Success)
                                {
                                    successfulResultsBuilder.Append($"<color=#a6ff58>Build successful: {buildResult.OutputPackageDirectory}</color>\n");
                                }
                                else
                                {
                                    failedResultsBuilder.Append($"Build failed: {buildResult.ErrorInfo}\n");
                                }
                            }
                            break;
                    }
                }
            }

            if (successfulResultsBuilder.Length > 0)
                Debug.Log(successfulResultsBuilder.ToString());
            if (failedResultsBuilder.Length > 0)
                Debug.LogError(failedResultsBuilder.ToString());
            #endregion

            #region 3. 輸出 CDN 目錄
            if (!onlyBuiltin)
            {
                // 產品名稱
                if (string.IsNullOrEmpty(productName))
                    productName = "anonymous";

                // 版號
                if (string.IsNullOrEmpty(buildVersion))
                    buildVersion = PlayerSettings.bundleVersion;
                else
                    PlayerSettings.bundleVersion = buildVersion;

                // 輸出 CDN 文件夾
                string sourceFolder = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
                string exportFolder = Path.Combine(EditorTools.GetProjectPath(), "ExportBundles", PatchSetting.setting.rootFolderName);
                ExportBundles(sourceFolder, exportFolder, bundleMap, productName, buildVersion, isClearOutputPath);
            }
            #endregion
        }

        /// <summary>
        /// 内置着色器资源包名称
        /// 注意：和自动收集的着色器资源包名保持一致！
        /// </summary>
        private static string _GetBuiltinShaderBundleName(string packageName)
        {
            var uniqueBundleName = AssetBundleCollectorSettingData.Setting.UniqueBundleName;
            var packRuleResult = DefaultPackRule.CreateShadersPackRuleResult();
            return packRuleResult.GetBundleName(packageName, uniqueBundleName);
        }

        /// <summary>
        /// OxGFrame export bundles for CDN
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="outputPath"></param>
        /// <param name="bundleMap"></param>
        /// <param name="productName"></param>
        /// <param name="buildVersion"></param>
        /// <param name="isClearOutputPath"></param>
        /// <exception cref="ArgumentException"></exception>
        public static void ExportBundles(string inputPath, string outputPath, string bundleMap, string productName, string buildVersion, bool isClearOutputPath)
        {
            // bundle map json main keys
            const string keyIncludeConfigs = "includeConfigs";
            const string keyIncludeSemanticPatch = "includeSemanticPatch";
            const string keyGroups = "groups";
            const string keyGenerateAll = "generateAll";
            const string keyHotfixCompile = "hotfixCompile";
            const string keyPackages = "packages";

            // Build map (json)
            if (string.IsNullOrEmpty(bundleMap))
                throw new ArgumentException("The argument '-bundleMap' cannot be null or empty.");

            // 檢查是否輸出配置文件
            bool? includeConfigs = JObject.Parse(bundleMap).SelectToken(keyIncludeConfigs)?.Value<Boolean>();

            // 檢查輸出規則參數
            bool? includeSemanticPatch = JObject.Parse(bundleMap).SelectToken(keyIncludeSemanticPatch)?.Value<Boolean>();
            AppConfig.SemanticRule semanticRule = new AppConfig.SemanticRule();
            semanticRule.MAJOR = true;
            semanticRule.MINOR = true;
            if (includeSemanticPatch != null && (bool)includeSemanticPatch)
                semanticRule.PATCH = (bool)includeSemanticPatch;

            // 目標平台
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;

            // 打包訊息
            List<string> packageInfos = new List<string>();
            List<string> appPackages = new List<string>();
            List<DlcInfo> dlcPackages = new List<DlcInfo>();

            // Group info map from json
            List<GroupInfo> groupInfos = new List<GroupInfo>();
            JArray groups = JObject.Parse(bundleMap).SelectToken(keyGroups)?.Value<JArray>();
            if (groups != null && groups.Count > 0)
            {
                foreach (var group in groups)
                {
                    // 取得 group 的各個屬性
                    string groupName = group["groupName"]?.Value<string>();
                    string[] tags = group["tags"]?.Value<string[]>();

                    GroupInfo groupInfo = new GroupInfo();
                    groupInfo.groupName = groupName;
                    groupInfo.tags = tags;
                    groupInfos.Add(groupInfo);
                }
            }

            // Bundle build map from json
            JArray packages = JObject.Parse(bundleMap).SelectToken(keyPackages)?.Value<JArray>();
            if (packages != null && packages.Count > 0)
            {
                foreach (var pkg in packages)
                {
                    // 取得 package 的各個屬性
                    string packageName = pkg["packageName"]?.Value<string>();
                    string exportArgs = pkg["exportArgs"]?.Value<string>();

                    // 參數
                    string[] args;

                    // 所有 pacakge 名稱
                    packageInfos.Add(packageName);

                    // 輸出類型
                    args = exportArgs.Trim().Split(',');
                    for (int i = 0; i < args.Length; i++)
                        args[i] = args[i].Trim();
                    string exportType = args[0].ToUpper();
                    switch (exportType)
                    {
                        case "APP":
                            appPackages.Add(packageName);
                            break;
                        case "DLC":
                            DlcInfo dlcInfo = new DlcInfo();
                            dlcInfo.packageName = packageName;
                            dlcInfo.withoutPlatform = Convert.ToBoolean(Convert.ToInt32(args[1]));
                            dlcInfo.dlcVersion = args[2];
                            dlcPackages.Add(dlcInfo);
                            break;
                    }
                }
            }

            // For App
            if (includeConfigs != null && (bool)includeConfigs)
                BundleHelper.ExportConfigsAndAppBundles(inputPath, outputPath, productName, semanticRule, buildVersion, appPackages.ToArray(), groupInfos, packageInfos.ToArray(), true, buildTarget, isClearOutputPath);
            else
                BundleHelper.ExportAppBundles(inputPath, outputPath, productName, semanticRule, buildVersion, appPackages.ToArray(), true, buildTarget, isClearOutputPath);

            // For DLC
            BundleHelper.ExportIndividualDlcBundles(inputPath, outputPath, productName, dlcPackages, true, buildTarget, false);

            Debug.Log($"<color=#a6ff58>Export Configs And Bundles For CDN.</color>\n");
        }

        /// <summary>
        /// yyyy-MM-dd-mmmm
        /// <para> yyyy = years </para>
        /// <para> MM = months </para>
        /// <para> dd = days </para>
        /// <para> mmmm = minutes </para>
        /// </summary>
        /// <returns></returns>
        public static string GetDefaultPackageVersion()
        {
            int totalMinutes = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            return DateTime.Now.ToString("yyyy-MM-dd") + "-" + totalMinutes;
        }
    }

    public static class CommandParser
    {
        public static class ArgName
        {
            // Common
            public const string buildMode = "-buildMode";                                 // None, Debug
            public const string destination = "-destination";                             // Build export detination (Path)
            public const string defineSymbols = "-defineSymbols";                         // Preprocessor tag
            public const string productName = "-productName";                             // Product name
            public const string buildVersion = "-buildVersion";                           // Application.version
            public const string companyName = "-companyName";                             // Company name
            public const string displayProductName = "-displayProductName";               // App Name (Installation Display App Name)
            public const string identifierName = "-identifierName";                       // App Identifier Name
            public const string il2CppConfiguration = "-il2CppConfiguration";             // il2Cpp compiler configuration (Debug, Release, Master)
            public const string scriptingBackends = "-scriptingBackends";                 // Use Mono or IL2CPP to compile
            public const string targetDevice = "-targetDevice";                           // android => Target Architectures, ios => Target Device
            public const string bundleUrlParams = "-bundleUrlParams";                     // Bundle Url config [bundle_ip, bundle_fallback_ip, store_link]
            public const string strippingLevel = "-strippingLevel";                       // Managed stripping level (0 = Disabled, 1 = Low, 2 = Medium, 3 = High, 4 = Minimal)

            // Android
            public const string androidKeystoreArgs = "-androidKeystoreArgs";             // Keystore for Android Google Play [keystore name path, keystore pwd, keyalias name, keyalias pwd]
            public const string enableAndroidAppBundle = "-enableAndroidAppBundle";       // aab for google play "0" or "1"
            public const string versionCode = "-versionCode";                             // Only for android version code in Google Play (bundle code)

            // iOS
            public const string iosSdkVersion = "-iosSdkVersion";                         // iOS SDK version [Device SDK], [Simulator SDK]

            // Bundle
            public const string bundleMap = "-bundleMap";                                 // bundle_map.json (Reference CICDNodejs)
        }

        private static Dictionary<string, string> _dictArgs = new Dictionary<string, string>()
    {
        // Common
        { ArgName.buildMode, null },
        { ArgName.destination, null },
        { ArgName.defineSymbols, null },
        { ArgName.productName, null },
        { ArgName.buildVersion, null },
        { ArgName.companyName, null },
        { ArgName.displayProductName, null },
        { ArgName.identifierName, null },
        { ArgName.il2CppConfiguration, null },
        { ArgName.scriptingBackends, null },
        { ArgName.targetDevice, null },
        { ArgName.bundleUrlParams, null },
        { ArgName.strippingLevel, null },

        // Android
        { ArgName.androidKeystoreArgs, null },
        { ArgName.enableAndroidAppBundle, null },
        { ArgName.versionCode, null },
        
        // iOS
        { ArgName.iosSdkVersion, null },

        // Bundle
        { ArgName.bundleMap, null },
    };

        static CommandParser()
        {
            _ParsingArguments();
        }

        private static void _ParsingArguments()
        {
            string[] argv = Environment.GetCommandLineArgs();
            for (int argc = 0; argc < argv.Length; argc++)
            {
                if (_dictArgs.ContainsKey(argv[argc]))
                {
                    // argc = key, argc + 1 = value
                    _dictArgs[argv[argc]] = argv[argc + 1];
                }
            }
        }

        public static string GetArgument(string argName)
        {
            if (_dictArgs.ContainsKey(argName))
                return _dictArgs[argName];
            return null;
        }
    }
}