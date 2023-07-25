using MyBox;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace OxGFrame.AssetLoader.Bundle
{
    [DisallowMultipleComponent]
    internal class PatchLauncher : MonoBehaviour
    {
        public static bool isInitialized { get; private set; } = false;

        [Header("Patch Options")]
        public BundleConfig.PlayMode playMode = BundleConfig.PlayMode.EditorSimulateMode;
        [Tooltip("If checker patch field will compare whole version"), ConditionalField(nameof(playMode), false, BundleConfig.PlayMode.HostMode, BundleConfig.PlayMode.WebGLMode)]
        public BundleConfig.SemanticRule semanticRule = new BundleConfig.SemanticRule();
        [Tooltip("If checked will skip default package download step of patch (force download while playing)"), ConditionalField(nameof(playMode), false, BundleConfig.PlayMode.HostMode)]
        public bool skipCreateMainDownloder = false;

        [Header("App Package List")]
        [Tooltip("The first element will be default app package")]
        public List<string> listPackage = new List<string>() { "DefaultPackage", "HotfixPackage" };

        [Header("Download Options")]
        public int maxConcurrencyDownloadCount = BundleConfig.maxConcurrencyDownloadCount;
        public int failedRetryCount = BundleConfig.failedRetryCount;
        [Tooltip("If file size >= [BreakpointFileSizeThreshold] that file will enable breakpoint mechanism (for all downloaders)")]
        public int breakpointFileSizeThreshold = 20 << 20;

        [Header("Cryptogram Options")]
        [Tooltip("AssetBundle decrypt key. \n[NONE], \n[OFFSET, dummySize], \n[XOR, key], \n[HT2XOR, headKey, tailKey, jumpKey], \n[AES, key, iv] \nex: \n\"None\" \n\"offset, 12\" \n\"xor, 23\" \n\"ht2xor, 34, 45, 56\" \n\"aes, key, iv\"")]
        public string decryptArgs = BundleConfig.CryptogramType.NONE;

        private async void Awake()
        {
            string newName = $"[{nameof(PatchLauncher)}]";
            this.gameObject.name = newName;
            if (this.gameObject.transform.root.name == newName)
            {
                var container = GameObject.Find(nameof(OxGFrame));
                if (container == null) container = new GameObject(nameof(OxGFrame));
                this.gameObject.transform.SetParent(container.transform);
                DontDestroyOnLoad(container);
            }
            else DontDestroyOnLoad(this.gameObject.transform.root);

            #region Patch Options
#if !UNITY_EDITOR && OXGFRAME_OFFLINE_MODE
            this.playMode = BundleConfig.PlayMode.OfflineMode;
#elif !UNITY_EDITOR && OXGFRAME_HOST_MODE
            this.playMode = BundleConfig.PlayMode.HostMode;
#elif !UNITY_EDITOR && OXGFRAME_WEBGL_MODE
            this.playMode = BundleConfig.PlayMode.WebGLMode;
#endif
            BundleConfig.playMode = this.playMode;
            // For Host Mode
            if (this.playMode == BundleConfig.PlayMode.HostMode)
            {
                BundleConfig.semanticRule = this.semanticRule;
                BundleConfig.skipCreateMainDownloder = this.skipCreateMainDownloder;
            }
            // For WebGL Mode
            else if (this.playMode == BundleConfig.PlayMode.WebGLMode)
            {
                BundleConfig.semanticRule = this.semanticRule;
            }
            #endregion

            #region Package List
            BundleConfig.listPackage = this.listPackage;
            #endregion

            #region Download Options
            BundleConfig.maxConcurrencyDownloadCount = this.maxConcurrencyDownloadCount <= 0 ? BundleConfig.defaultMaxConcurrencyDownloadCount : this.maxConcurrencyDownloadCount;
            BundleConfig.failedRetryCount = this.failedRetryCount <= 0 ? BundleConfig.defaultFailedRetryCount : this.failedRetryCount;
            // Set download breakpoint size threshold
            YooAssets.SetDownloadSystemBreakpointResumeFileSize(this.breakpointFileSizeThreshold);
            #endregion

            #region Cryptogram Options
            BundleConfig.InitCryptogram(string.IsNullOrEmpty(this.decryptArgs) ? BundleConfig.CryptogramType.NONE : this.decryptArgs);
            #endregion

            // Init Settings and App Packages setup
            await PackageManager.InitSetup();

            Debug.Log($"<color=#32ff94>(Powered by YooAsset) Initialized Play Mode: {BundleConfig.playMode}</color>");

            isInitialized = true;

            Debug.Log("<color=#b5ff00>(Powered by YooAsset) PatchLauncher Setup Completes.</color>");
        }

        private void OnApplicationQuit()
        {
#if !UNITY_WEBGL  
            PackageManager.Release();
            Debug.Log("<color=#ff84d1>(Powered by YooAsset) Release Packages Completes.</color>");
#endif
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
#if UNITY_WEBGL
            switch (this.playMode)
            {
                case BundleConfig.PlayMode.OfflineMode:

                    Debug.Log($"<color=#ff1f4c>[Offline Mode] is not supported on {EditorUserBuildSettings.activeBuildTarget}.</color>");
                    break;
                case BundleConfig.PlayMode.HostMode:
                    Debug.Log($"<color=#ff1f4c>[Host Mode] is not supported on {EditorUserBuildSettings.activeBuildTarget}.</color>");
                    break;
            }
#else
            switch (this.playMode)
            {
                case BundleConfig.PlayMode.WebGLMode:

                    Debug.Log($"<color=#ff1f4c>[WebGL Mode] is not supported on {EditorUserBuildSettings.activeBuildTarget}.</color>");
                    break;
            }
#endif
        }
#endif
    }
}