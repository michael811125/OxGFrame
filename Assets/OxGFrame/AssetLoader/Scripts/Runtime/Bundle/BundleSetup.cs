using UnityEngine;

namespace OxGFrame.AssetLoader.Bundle
{
    [DisallowMultipleComponent]
    public class BundleSetup : MonoBehaviour
    {
        [Header("Editor Options")]
        [Tooltip("Enable AssetDatabase Mode (accelerate develop, won't load ab procedure)")]
        public bool assetDatabaseMode = true;

        [Header("Load Options")]
        [Tooltip("Enable Stream Mode")]
        public bool bundleStreamMode = true;
        [Tooltip("Enable read Md5 name of ab")]
        public bool readMd5BundleName = true;

        [Header("Download Options")]
        [Tooltip("Max slice size for large file (it is not recommended to set too high)")]
        public long maxDownloadSliceSize = BundleConfig.defaultMaxDownloadSliceSize;

        [Header("Compression Options")]
        [Tooltip("Zip file name (with extension or without extension)")]
        public string zipFileName = BundleConfig.defaultZipFileName;
        [Tooltip("Enable read Md5 for zip name")]
        public bool md5ForZipFileName = true;
        [Tooltip("Unzip buffer size (buffer size more bigger and more faster)")]
        public int unzipBufferSize = BundleConfig.defaultUnzipBufferSize;
        [Tooltip("Unzip password")]
        public string unzipPassword = string.Empty;

        [Header("Cryptogram Options")]
        [Tooltip("AssetBundle decrypt key. \n[NONE], \n[OFFSET, dummySize], \n[XOR, key], \n[HTXOR, headKey, tailKey], \n[AES, key, iv] \nex: \n\"None\" \n\"offset, 12\" \n\"xor, 23\" \n\"htxor, 34, 45\" \n\"aes, key, iv\"")]
        public string decryptArgs = BundleConfig.CryptogramType.NONE;

        [Header("Manifest Options")]
        [Tooltip("Set internal manifest name (In-App), depends on the name of build bundle")]
        public string internalManifestName = BundleConfig.defaultInternalManifestName;
        [Tooltip("Set external manifest name (Patch), depends on the name of build bundle")]
        public string externalManifestName = BundleConfig.defaultExternalManifestName;

        private void Awake()
        {
            // Editor Options
#if UNITY_EDITOR
            BundleConfig.assetDatabaseMode = this.assetDatabaseMode;
#else
            BundleConfig.assetDatabaseMode = false;
#endif
            // Load Options
            BundleConfig.bundleStreamMode = this.bundleStreamMode;
            BundleConfig.readMd5BundleName = this.readMd5BundleName;

            // Download Options
            BundleConfig.maxDownloadSliceSize = this.maxDownloadSliceSize <= 0 ? BundleConfig.defaultMaxDownloadSliceSize : this.maxDownloadSliceSize;

            // Unzip Options
            BundleConfig.zipFileName = string.IsNullOrEmpty(this.zipFileName) ? BundleConfig.defaultZipFileName : this.zipFileName;
            BundleConfig.md5ForZipFileName = this.md5ForZipFileName;
            BundleConfig.unzipBufferSize = this.unzipBufferSize <= 0 ? BundleConfig.defaultUnzipBufferSize : this.unzipBufferSize;
            BundleConfig.unzipPassword = this.unzipPassword;

            // Cryptogram Options
            BundleConfig.InitCryptogram(string.IsNullOrEmpty(this.decryptArgs) ? BundleConfig.CryptogramType.NONE : this.decryptArgs);

            // Manifest Options
            BundleConfig.internalManifestName = string.IsNullOrEmpty(this.internalManifestName) ? BundleConfig.defaultInternalManifestName : this.internalManifestName;
            BundleConfig.externalManifestName = string.IsNullOrEmpty(this.externalManifestName) ? BundleConfig.defaultExternalManifestName : this.externalManifestName;

            Debug.Log("<color=#b5ff00>BundleSetup Completes.</color>");
        }

        private void OnApplicationQuit()
        {
            // 離開程序時, 需要取消任務 (避免影響後續程序)
            BundleDistributor.GetInstance().TaskCancel(true);
        }
    }
}