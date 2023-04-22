using Cysharp.Threading.Tasks;
using YooAsset;

namespace OxGFrame.AssetLoader.Bundle
{
    internal static class PackageManager
    {
        private static string _currentPackageName;
        private static ResourcePackage _currentPackage;
        private static IDecryptionServices _decryption;

        public static void InitSetup()
        {
            #region Init YooAssets
            YooAssets.Destroy();
            YooAssets.Initialize();
            YooAssets.SetOperationSystemMaxTimeSlice(10);
            #endregion

            #region Init Decryption Type
            // Init decryption type
            var cryptogramType = BundleConfig.cryptogramArgs[0].ToUpper();
            switch (cryptogramType)
            {
                case BundleConfig.CryptogramType.NONE:
                    break;
                case BundleConfig.CryptogramType.OFFSET:
                    _decryption = new OffsetDecryption();
                    break;
                case BundleConfig.CryptogramType.XOR:
                    _decryption = new XorDecryption();
                    break;
                case BundleConfig.CryptogramType.HTXOR:
                    _decryption = new HTXorDecryption();
                    break;
                case BundleConfig.CryptogramType.AES:
                    _decryption = new AesDecryption();
                    break;
            }
            #endregion

            #region Init Package
            // Init AssetsPackage first
            if (BundleConfig.listPackage != null && BundleConfig.listPackage.Count > 0)
            {
                foreach (var assetsPackage in BundleConfig.listPackage)
                {
                    RegisterPackage(assetsPackage);
                }

                // Set default AssetsPackage
                SetDefaultPackage(0);
            }
            #endregion
        }

        public static async UniTask<InitializationOperation> InitPatchMode()
        {
            // Simulate Mode
            InitializationOperation initializationOperation = null;
            if (BundleConfig.playMode == BundleConfig.PlayMode.EditorSimulateMode)
            {
                var createParameters = new EditorSimulateModeParameters();
                createParameters.SimulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(_currentPackageName);
                initializationOperation = GetDefaultPackage().InitializeAsync(createParameters);
            }

            // Offline Mode
            if (BundleConfig.playMode == BundleConfig.PlayMode.OfflineMode)
            {
                var createParameters = new OfflinePlayModeParameters();
                createParameters.DecryptionServices = _decryption;
                initializationOperation = GetDefaultPackage().InitializeAsync(createParameters);
            }

            // Host Mode
            if (BundleConfig.playMode == BundleConfig.PlayMode.HostMode)
            {
                var createParameters = new HostPlayModeParameters();
                createParameters.DecryptionServices = _decryption;
                createParameters.QueryServices = new RequestQuery();
                createParameters.DefaultHostServer = await BundleConfig.GetHostServerUrl();
                createParameters.FallbackHostServer = await BundleConfig.GetFallbackHostServerUrl();
                initializationOperation = GetDefaultPackage().InitializeAsync(createParameters);
            }

            await initializationOperation;

            return initializationOperation;
        }

        public static void SetDefaultPackage(string packageName)
        {
            var package = RegisterPackage(packageName);
            YooAssets.SetDefaultPackage(package);
            _currentPackageName = package.PackageName;
            _currentPackage = package;
        }

        public static void SetDefaultPackage(int idx)
        {
            var package = RegisterPackage(idx);
            YooAssets.SetDefaultPackage(package);
            _currentPackageName = package.PackageName;
            _currentPackage = package;
        }

        public static void SwitchDefaultPackage(string packageName)
        {
            var package = GetPackage(packageName);
            if (package != null)
            {
                YooAssets.SetDefaultPackage(package);
                _currentPackageName = package.PackageName;
                _currentPackage = package;
            }
        }

        public static void SwitchDefaultPackage(int idx)
        {
            var package = GetPackage(idx);
            if (package != null)
            {
                YooAssets.SetDefaultPackage(package);
                _currentPackageName = package.PackageName;
                _currentPackage = package;
            }
        }

        public static IDecryptionServices GetDecryptionService()
        {
            return _decryption;
        }

        public static string GetDefaultPackageName()
        {
            return _currentPackageName;
        }

        public static ResourcePackage GetDefaultPackage()
        {
            return _currentPackage;
        }

        public static ResourcePackage RegisterPackage(string packageName)
        {
            var package = GetPackage(packageName);
            if (package == null) package = YooAssets.CreatePackage(packageName);
            return package;
        }

        public static ResourcePackage RegisterPackage(int idx)
        {
            var package = GetPackage(idx);
            if (package == null) package = YooAssets.CreatePackage(GetPackageNameByIdx(idx));
            return package;
        }

        public static ResourcePackage GetPackage(string packageName)
        {
            return YooAssets.TryGetPackage(packageName);
        }

        public static ResourcePackage GetPackage(int idx)
        {
            string packageName = GetPackageNameByIdx(idx);
            return GetPackage(packageName);
        }

        public static string GetPackageNameByIdx(int idx)
        {
            if (idx >= BundleConfig.listPackage.Count) idx = BundleConfig.listPackage.Count - 1;
            else if (idx < 0) idx = 0;

            return BundleConfig.listPackage[idx];
        }

        public static ResourceDownloaderOperation GetPacakgeDownloaderByTags(ResourcePackage pacakge, params string[] tags)
        {
            ResourceDownloaderOperation downloader;
            // create all
            if (tags == null || tags.Length == 0) downloader = pacakge.CreateResourceDownloader(BundleConfig.maxConcurrencyDownloadCount, BundleConfig.failedRetryCount);
            // create by tags
            else downloader = pacakge.CreateResourceDownloader(tags, BundleConfig.maxConcurrencyDownloadCount, BundleConfig.failedRetryCount);

            return downloader;
        }

        public static void Release()
        {
            YooAssets.Destroy();
        }
    }
}
