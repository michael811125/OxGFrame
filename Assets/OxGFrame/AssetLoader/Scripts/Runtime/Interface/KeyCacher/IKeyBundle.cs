using Cysharp.Threading.Tasks;
using UnityEngine;

namespace OxGFrame.AssetLoader
{
    public interface IKeyBundle
    {
        UniTask<T> Load<T>(int id, string bundleName, string assetName, Progression progression) where T : Object;

        UniTask<GameObject> LoadWithClone(int id, string bundleName, string assetName, Transform parent, Vector3? scale, Progression progression);

        UniTask<GameObject> LoadWithClone(int id, string bundleName, string assetName, Vector3 position, Quaternion rotation, Transform parent, Vector3? scale, Progression progression);
    }
}

