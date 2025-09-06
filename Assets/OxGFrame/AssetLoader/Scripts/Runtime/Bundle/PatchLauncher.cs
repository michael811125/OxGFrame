using MyBox;
using OxGKit.LoggingSystem;
using System.Collections.Generic;
using UnityEngine;

namespace OxGFrame.AssetLoader.Bundle
{
    [DisallowMultipleComponent]
    internal class PatchLauncher : MonoBehaviour
    {
        [Separator("Patch Options")]
        public BundleConfig.PlayMode playMode = BundleConfig.PlayMode.EditorSimulateMode;
        [Tooltip("If checked, the patch field will compare whole version."), ConditionalField(nameof(playMode), false, BundleConfig.PlayMode.HostMode, BundleConfig.PlayMode.WeakHostMode, BundleConfig.PlayMode.WebGLRemoteMode)]
        public BundleConfig.SemanticRule semanticRule = new BundleConfig.SemanticRule();
        [Tooltip("If checked, will skip preset packages download step of the patch (force download while playing)."), ConditionalField(nameof(playMode), false, BundleConfig.PlayMode.HostMode)]
        public bool skipMainDownload = false;
        [Tooltip("If checked, will check disk space is it enough while patch checking."), ConditionalField(nameof(playMode), false, BundleConfig.PlayMode.HostMode, BundleConfig.PlayMode.WeakHostMode)]
        public bool checkDiskSpace = true;

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

        private async void Awake()
        {
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
#endif
            BundleConfig.playMode = this.playMode;
            // For Host Mode
            if (BundleConfig.playMode == BundleConfig.PlayMode.HostMode)
            {
                BundleConfig.semanticRule = this.semanticRule;
                BundleConfig.skipMainDownload = this.skipMainDownload;
                BundleConfig.checkDiskSpace = this.checkDiskSpace;
            }
            // For Weak Host Mode (Does not support skipping download)
            else if (BundleConfig.playMode == BundleConfig.PlayMode.WeakHostMode)
            {
                BundleConfig.semanticRule = this.semanticRule;
                BundleConfig.checkDiskSpace = this.checkDiskSpace;
            }
            // For WebGL Remote Mode
            else if (BundleConfig.playMode == BundleConfig.PlayMode.WebGLRemoteMode)
            {
                BundleConfig.semanticRule = this.semanticRule;
            }
            #endregion

            #region Package List
            BundleConfig.listAppPackages = this.listAppPackages;
            BundleConfig.listDlcPackages = this.listDlcPackages;
            #endregion

            #region Process Options
            BundleConfig.operationSystemMaxTimeSlice = this.operationSystemMaxTimeSlice;
            #endregion

            #region Download Options
            BundleConfig.maxConcurrencyDownloadCount = this.maxConcurrencyDownloadCount <= 0 ? BundleConfig.DEFAULT_MAX_CONCURRENCY_MAX_DOWNLOAD_COUNT : this.maxConcurrencyDownloadCount;
            BundleConfig.failedRetryCount = this.failedRetryCount <= 0 ? BundleConfig.DEFAULT_FAILED_RETRY_COUNT : this.failedRetryCount;
            // Set download breakpoint size threshold
            BundleConfig.breakpointFileSizeThreshold = this.breakpointFileSizeThreshold;
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

            // Init Settings and Setup Preset App Packages
            await PackageManager.InitSetup();

            if (PackageManager.isInitialized)
            {
                Logging.PrintInfo<Logger>($"(Powered by YooAsset) Initialized Play Mode: {BundleConfig.playMode}");
                Logging.PrintInfo<Logger>("(Powered by YooAsset) PatchLauncher Setup Completes.");
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
#if UNITY_WEBGL
            switch (this.playMode)
            {
                case BundleConfig.PlayMode.OfflineMode:
                    this.playMode = BundleConfig.PlayMode.EditorSimulateMode;
                    Debug.Log($"[Offline Mode] is not supported on {UnityEditor.EditorUserBuildSettings.activeBuildTarget}.");
                    break;
                case BundleConfig.PlayMode.HostMode:
                    this.playMode = BundleConfig.PlayMode.EditorSimulateMode;
                    Debug.Log($"[Host Mode] is not supported on {UnityEditor.EditorUserBuildSettings.activeBuildTarget}.");
                    break;
                case BundleConfig.PlayMode.WeakHostMode:
                    this.playMode = BundleConfig.PlayMode.EditorSimulateMode;
                    Debug.Log($"[Weak Host Mode] is not supported on {UnityEditor.EditorUserBuildSettings.activeBuildTarget}.");
                    break;
            }
#else
            switch (this.playMode)
            {
                case BundleConfig.PlayMode.WebGLMode:
                case BundleConfig.PlayMode.WebGLRemoteMode:
                    this.playMode = BundleConfig.PlayMode.EditorSimulateMode;
                    Debug.Log($"[WebGL Mode] is not supported on {UnityEditor.EditorUserBuildSettings.activeBuildTarget}.");
                    break;
            }
#endif
        }
#endif
    }
}