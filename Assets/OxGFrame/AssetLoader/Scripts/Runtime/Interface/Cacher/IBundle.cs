using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader.Cacher;
using UnityEngine;

namespace OxGFrame.AssetLoader
{
    public interface IBundle
    {
        UniTask<T> Load<T>(string bundleName, string assetName, bool dependencies = true, Progression progression = null) where T : Object;

        UniTask<BundlePack> LoadBundlePack(string fileName, Progression progression, bool forceNoMd5 = false);

        UniTask<BundlePack> LoadManifest(string bundleName);

        void UnloadManifest();

        void UnloadManifest(string bundleName);

        UniTask<AssetBundleManifest> GetManifestAsync(string bundleName);

        AssetBundleManifest GetManifest(string bundleName);
    }
}
