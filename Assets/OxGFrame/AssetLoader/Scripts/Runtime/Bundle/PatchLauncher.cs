using System.Collections.Generic;
using UnityEngine;

namespace OxGFrame.AssetLoader.Bundle
{
    [DisallowMultipleComponent]
    internal class PatchLauncher : MonoBehaviour
    {
        public static bool isInitialized { get; private set; } = false;

        [Header("Patch Options")]
        public BundleConfig.PlayMode playMode = BundleConfig.PlayMode.EditorSimulateMode;

        [Header("Package List")]
        [Tooltip("First will be DefaultPackge")]
        public List<string> listPackage = new List<string>() { "DefaultPackage" };

        [Header("Download Options")]
        public int maxConcurrencyDownloadCount = BundleConfig.maxConcurrencyDownloadCount;
        public int failedRetryCount = BundleConfig.failedRetryCount;

        [Header("Cryptogram Options")]
        [Tooltip("AssetBundle decrypt key. \n[NONE], \n[OFFSET, dummySize], \n[XOR, key], \n[HTXOR, headKey, tailKey], \n[AES, key, iv] \nex: \n\"None\" \n\"offset, 12\" \n\"xor, 23\" \n\"htxor, 34, 45\" \n\"aes, key, iv\"")]
        public string decryptArgs = BundleConfig.CryptogramType.NONE;

        private async void Awake()
        {
            this.gameObject.name = $"[{nameof(PatchLauncher)}]";
            DontDestroyOnLoad(this);

            // Patch Options
            BundleConfig.playMode = this.playMode;

            // Package List
            BundleConfig.listPackage = this.listPackage;

            // Download Options
            BundleConfig.maxConcurrencyDownloadCount = this.maxConcurrencyDownloadCount <= 0 ? BundleConfig.defaultMaxConcurrencyDownloadCount : this.maxConcurrencyDownloadCount;
            BundleConfig.failedRetryCount = this.failedRetryCount <= 0 ? BundleConfig.defaultFailedRetryCount : this.failedRetryCount;

            // Cryptogram Options
            BundleConfig.InitCryptogram(string.IsNullOrEmpty(this.decryptArgs) ? BundleConfig.CryptogramType.NONE : this.decryptArgs);

            // Init Settings
            PackageManager.InitSetup();

            // Init Patch Mode
            await PackageManager.InitPatchMode();

            Debug.Log($"<color=#32ff94>(Powered by YooAsset) Initialized Play Mode: {BundleConfig.playMode}</color>");

            isInitialized = true;

            Debug.Log("<color=#b5ff00>(Powered by YooAsset) PatchLauncher Setup Completes.</color>");
        }

        private void OnApplicationQuit()
        {
            PackageManager.Release();

            Debug.Log("<color=#ff84d1>(Powered by YooAsset) Release Packages Completes.</color>");
        }
    }
}