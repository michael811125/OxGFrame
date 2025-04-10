using UnityEngine;

namespace YooAsset
{
    internal abstract class DownloadAssetBundleOperation : DefaultDownloadFileOperation
    {
        internal DownloadAssetBundleOperation(PackageBundle bundle, DownloadFileOptions options) : base(bundle, options)
        {
        }

        public AssetBundle Result;
    }
}