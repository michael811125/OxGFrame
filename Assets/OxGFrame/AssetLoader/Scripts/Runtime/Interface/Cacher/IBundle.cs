using AssetLoader.AssetObject;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AssetLoader
{
    public interface IBundle
    {
        UniTask<T> Load<T>(string bundleName, string assetName, bool dependencies = true, Progression progression = null) where T : Object;

        UniTask<BundlePack> LoadBundlePack(string fileName, Progression progression);

        UniTask<AssetBundleManifest> LoadManifest();

        UniTask<AssetBundleManifest> GetManifest();

        UniTask<byte[]> FileRequest(string url);
    }
}
