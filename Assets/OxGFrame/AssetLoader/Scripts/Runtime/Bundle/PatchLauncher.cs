using Cysharp.Threading.Tasks;
using MyBox;
using OxGKit.LoggingSystem;
using System.Collections.Generic;
using UnityEngine;

namespace OxGFrame.AssetLoader.Bundle
{
    [DisallowMultipleComponent]
    internal class PatchLauncher : MonoBehaviour
    {
        [Separator("Launch Options")]
        public BundleConfig.PlayMode playMode = BundleConfig.PlayMode.EditorSimulateMode;
        [ConditionalField(nameof(playMode), false, BundleConfig.PlayMode.EditorSimulateMode)]
        public EditorSimulateModeParameters editorSimulateModeParameters = new EditorSimulateModeParameters();
        [ConditionalField(nameof(playMode), false, BundleConfig.PlayMode.OfflineMode)]
        public OfflineModeParameters offlineModeParameters = new OfflineModeParameters();
        [ConditionalField(nameof(playMode), false, BundleConfig.PlayMode.HostMode)]
        public HostModeParameters hostModeParameters = new HostModeParameters();
        [ConditionalField(nameof(playMode), false, BundleConfig.PlayMode.WeakHostMode)]
        public WeakHostModeParameters weakHostModeParameters = new WeakHostModeParameters();
        [ConditionalField(nameof(playMode), false, BundleConfig.PlayMode.WebGLMode)]
        public WebGLModeParameters webGLModeParameters = new WebGLModeParameters();
        [ConditionalField(nameof(playMode), false, BundleConfig.PlayMode.WebGLRemoteMode)]
        public WebGLRemoteModeParameters webGLRemoteModeParameters = new WebGLRemoteModeParameters();
        [ConditionalField(nameof(playMode), false, BundleConfig.PlayMode.CustomMode)]
        public CustomModeParameters customModeParameters = new CustomModeParameters();

#if UNITY_EDITOR
        [ButtonClicker(nameof(SetDefaultParameters), "Restore Defaults", "#ff5786")]
        public bool setDefaultParameters;
#endif

        [Separator("Preset App Packages")]
        [Tooltip("The first element will be 'default app package'.\n\nNote: The presets will combine in main download of the patch.")]
        public List<AppPackageInfoWithBuild> listAppPackages = new List<AppPackageInfoWithBuild>() { new AppPackageInfoWithBuild() { packageName = "DefaultPackage" } };

        [Separator("Preset DLC Packages")]
        [Tooltip("The preset DLC packages must be fixed versions.\n\nNote: The presets will combine in main download of the patch.")]
        public List<DlcPackageInfoWithBuild> listDlcPackages = new List<DlcPackageInfoWithBuild>();

        [Separator("Process Options")]
        [Tooltip("Set the maximum time slice per frame (in milliseconds) for YooAsset's async system.")]
        public long operationSystemMaxTimeSlice = BundleConfig.operationSystemMaxTimeSlice;

        [Separator("Download Options")]
        [Tooltip("Maximum number of concurrent downloads.")]
        public int maxConcurrencyDownloadCount = BundleConfig.maxConcurrencyDownloadCount;
        [Tooltip("Number of retry attempts on download failure.")]
        public int failedRetryCount = BundleConfig.failedRetryCount;
        [Tooltip("If the file size is ≥ 'BreakpointFileSizeThreshold' (in bytes), the breakpoint (resumable) mechanism will be enabled for all downloaders.")]
        public uint breakpointFileSizeThreshold = BundleConfig.breakpointFileSizeThreshold;
        [Tooltip("If no download data is received within the watchdog timeout, the task will be aborted.")]
        public int downloadWatchdogTimeout = BundleConfig.downloadWatchdogTimeout;

        [Separator("Load Options")]
        [Tooltip("Size of the managed read buffer (in bytes) used by AssetBundle.LoadFromStream.")]
        public uint bundleLoadReadBufferSize = BundleConfig.bundleLoadReadBufferSize;
        [Tooltip("Size of the managed read buffer (in bytes) used by FileCryptogram decryption operations.")]
        public uint bundleDecryptReadBufferSize = BundleConfig.bundleDecryptReadBufferSize;

        [Separator("Cryptogram Options")]
        [SerializeField, OverrideLabel("Bundle Decrypt Info")]
        private DecryptInfo _decryptInfo = new DecryptInfo();
        [SerializeField, OverrideLabel("Manifest Decrypt Info")]
        private DecryptInfo _manifestDecryptInfo = new DecryptInfo();

        private static PatchLauncher _instance = null;

        private async void Awake()
        {
            _instance = this;

            string newName = $"[{nameof(PatchLauncher)}]";
            this.gameObject.name = newName;
            if (this.gameObject.transform.root.name == newName)
            {
                var container = GameObject.Find(nameof(OxGFrame));
                if (container == null)
                    container = new GameObject(nameof(OxGFrame));
                this.gameObject.transform.SetParent(container.transform);
                DontDestroyOnLoad(container);
            }
            else
                DontDestroyOnLoad(this.gameObject.transform.root);

            #region Patch Options
#if !UNITY_EDITOR && OXGFRAME_OFFLINE_MODE
            this.playMode = BundleConfig.PlayMode.OfflineMode;
#elif !UNITY_EDITOR && OXGFRAME_HOST_MODE
            this.playMode = BundleConfig.PlayMode.HostMode;
#elif !UNITY_EDITOR && OXGFRAME_WEAK_HOST_MODE
            this.playMode = BundleConfig.PlayMode.WeakHostMode;
#elif !UNITY_EDITOR && OXGFRAME_WEBGL_MODE
            this.playMode = BundleConfig.PlayMode.WebGLMode;
#elif !UNITY_EDITOR && OXGFRAME_WEBGL_REMOTE_MODE
            this.playMode = BundleConfig.PlayMode.WebGLRemoteMode;
#elif !UNITY_EDITOR && OXGFRAME_CUSTOM_MODE
            this.playMode = BundleConfig.PlayMode.CustomMode;
#endif
            BundleConfig.playMode = this.playMode;

            switch (this.playMode)
            {
                case BundleConfig.PlayMode.EditorSimulateMode:
                    BundleConfig.playModeParameters = this.editorSimulateModeParameters;
                    break;
                case BundleConfig.PlayMode.OfflineMode:
                    BundleConfig.playModeParameters = this.offlineModeParameters;
                    break;
                case BundleConfig.PlayMode.HostMode:
                    BundleConfig.playModeParameters = this.hostModeParameters;
                    break;
                case BundleConfig.PlayMode.WeakHostMode:
                    BundleConfig.playModeParameters = this.weakHostModeParameters;
                    break;
                case BundleConfig.PlayMode.WebGLMode:
                    BundleConfig.playModeParameters = this.webGLModeParameters;
                    break;
                case BundleConfig.PlayMode.WebGLRemoteMode:
                    BundleConfig.playModeParameters = this.webGLRemoteModeParameters;
                    break;
                case BundleConfig.PlayMode.CustomMode:
                    if (this.customModeParameters.initializePresetPackages)
                        this.customModeParameters.initializePresetPackages = false;
                    BundleConfig.playModeParameters = this.customModeParameters;
                    break;
            }
            #endregion

            #region Package List
            if (this.playMode == BundleConfig.PlayMode.CustomMode)
            {
                this.listAppPackages = new List<AppPackageInfoWithBuild>();
                this.listDlcPackages = new List<DlcPackageInfoWithBuild>();
            }
            BundleConfig.listAppPackages = this.listAppPackages;
            BundleConfig.listDlcPackages = this.listDlcPackages;
            #endregion

            #region Process Options
            BundleConfig.operationSystemMaxTimeSlice = this.operationSystemMaxTimeSlice;
            #endregion

            #region Download Options
            BundleConfig.maxConcurrencyDownloadCount = this.maxConcurrencyDownloadCount <= 0 ? BundleConfig.DEFAULT_MAX_CONCURRENCY_MAX_DOWNLOAD_COUNT : this.maxConcurrencyDownloadCount;
            BundleConfig.failedRetryCount = this.failedRetryCount < 0 ? BundleConfig.DEFAULT_FAILED_RETRY_COUNT : this.failedRetryCount;
            // Set download breakpoint size threshold
            BundleConfig.breakpointFileSizeThreshold = this.breakpointFileSizeThreshold;
            BundleConfig.downloadWatchdogTimeout = this.downloadWatchdogTimeout <= 0 ? BundleConfig.DEFAULT_DOWNLOAD_WATCHDOG_TIMEOUT : this.downloadWatchdogTimeout;
            #endregion

            #region Load Options
            // Set managed read buffer size
            BundleConfig.bundleLoadReadBufferSize = this.bundleLoadReadBufferSize;
            BundleConfig.bundleDecryptReadBufferSize = this.bundleDecryptReadBufferSize;
            #endregion

            #region Cryptogram Options
            BundleConfig.InitBundleDecryptInfo(this._decryptInfo.GetDecryptArgs(), this._decryptInfo.scuredStringType, this._decryptInfo.GetSaltSize(), this._decryptInfo.GetDummySize());
            BundleConfig.InitManifestDecryptInfo(this._manifestDecryptInfo.GetDecryptArgs(), this._manifestDecryptInfo.scuredStringType, this._manifestDecryptInfo.GetSaltSize(), this._manifestDecryptInfo.GetDummySize());
            this._decryptInfo.Dispose();
            this._manifestDecryptInfo.Dispose();
            #endregion

            // Init and setup yoo settings
            PackageManager.Initialize();

            // Init and setup preset packages
            if (BundleConfig.playModeParameters.initializePresetPackages)
            {
                Logging.Print<Logger>($"[{nameof(PatchLauncher)}] The {nameof(BundleConfig.playModeParameters.initializePresetPackages)} flag is {BundleConfig.playModeParameters.initializePresetPackages} -> {nameof(InitializePresetPackages)}() will run in Awake.");
                await InitializePresetPackages();
            }
            else
            {
                Logging.Print<Logger>($"[{nameof(PatchLauncher)}] The {nameof(BundleConfig.playModeParameters.initializePresetPackages)} flag is {BundleConfig.playModeParameters.initializePresetPackages} -> {nameof(InitializePresetPackages)}() will be skipped during Awake. Manual invocation required.");
            }
        }

        /// <summary>
        /// Init and setup preset packages
        /// </summary>
        /// <returns></returns>
        internal static async UniTask InitializePresetPackages()
        {
            // Init and setup preset packages
            await PackageManager.InitializePresetPackages();

            if (PackageManager.isInitialized)
            {
                Logging.PrintInfo<Logger>("(Powered by YooAsset) Preset Packages Setup Completes.");
            }
        }

        #region Setter
        /// <summary>
        /// Custom app packages and dlc packages at runtime for CustomMode
        /// </summary>
        /// <param name="appPackages"></param>
        /// <param name="dlcPackages"></param>
        internal static void SetPresetPackages(List<AppPackageInfoWithBuild> appPackages, List<DlcPackageInfoWithBuild> dlcPackages)
        {
            BundleConfig.listAppPackages = _instance.listAppPackages = appPackages;
            BundleConfig.listDlcPackages = _instance.listDlcPackages = dlcPackages;
        }
        #endregion

#if UNITY_EDITOR
        /// <summary>
        /// 還原預設參數 (Editor only)
        /// </summary>
        public void SetDefaultParameters()
        {
            // Record undo action
            UnityEditor.Undo.RecordObject(this, nameof(SetDefaultParameters));

            switch (this.playMode)
            {
                case BundleConfig.PlayMode.EditorSimulateMode:
                    this.editorSimulateModeParameters.SetDefaultParameters();
                    break;
                case BundleConfig.PlayMode.OfflineMode:
                    this.offlineModeParameters.SetDefaultParameters();
                    break;
                case BundleConfig.PlayMode.HostMode:
                    this.hostModeParameters.SetDefaultParameters();
                    break;
                case BundleConfig.PlayMode.WeakHostMode:
                    this.weakHostModeParameters.SetDefaultParameters();
                    break;
                case BundleConfig.PlayMode.WebGLMode:
                    this.webGLModeParameters.SetDefaultParameters();
                    break;
                case BundleConfig.PlayMode.WebGLRemoteMode:
                    this.webGLRemoteModeParameters.SetDefaultParameters();
                    break;
                case BundleConfig.PlayMode.CustomMode:
                    this.customModeParameters.SetDefaultParameters();
                    break;
            }

            // Serialized
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
                if (UnityEditor.PrefabUtility.IsPartOfPrefabInstance(this))
                    UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
            }
        }

        private void OnValidate()
        {
#if UNITY_WEBGL
            switch (this.playMode)
            {
                case BundleConfig.PlayMode.OfflineMode:
                    this.playMode = BundleConfig.PlayMode.EditorSimulateMode;
                    Debug.LogWarning($"[Offline Mode] is not supported on {UnityEditor.EditorUserBuildSettings.activeBuildTarget}.");
                    break;
                case BundleConfig.PlayMode.HostMode:
                    this.playMode = BundleConfig.PlayMode.EditorSimulateMode;
                    Debug.LogWarning($"[Host Mode] is not supported on {UnityEditor.EditorUserBuildSettings.activeBuildTarget}.");
                    break;
                case BundleConfig.PlayMode.WeakHostMode:
                    this.playMode = BundleConfig.PlayMode.EditorSimulateMode;
                    Debug.LogWarning($"[Weak Host Mode] is not supported on {UnityEditor.EditorUserBuildSettings.activeBuildTarget}.");
                    break;
            }
#else
            switch (this.playMode)
            {
                case BundleConfig.PlayMode.WebGLMode:
                case BundleConfig.PlayMode.WebGLRemoteMode:
                    this.playMode = BundleConfig.PlayMode.EditorSimulateMode;
                    Debug.LogWarning($"[WebGL Mode] is not supported on {UnityEditor.EditorUserBuildSettings.activeBuildTarget}.");
                    break;
            }
#endif

            // InitializePresetPackagesOnAwake 標記處理
            switch (this.playMode)
            {
                case BundleConfig.PlayMode.CustomMode:
                    if (this.customModeParameters.initializePresetPackages)
                    {
                        this.customModeParameters.initializePresetPackages = false;
                        Debug.LogWarning($"[In Custom Mode] {nameof(BundleConfig.playModeParameters.initializePresetPackages)} is not supported.");
                    }
                    if (this.listAppPackages.Count > 0 || this.listDlcPackages.Count > 0)
                        Debug.LogWarning($"[In Custom Mode] {nameof(listAppPackages)} and {nameof(listDlcPackages)} are not supported.");
                    break;
            }
        }
#endif
    }
}