using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OxGFrame.AssetLoader
{
    public interface IKeyResource
    {
        UniTask<T> Load<T>(int id, string assetName, Progression progression) where T : Object;

        UniTask<GameObject> LoadWithClone(int id, string assetName, Transform parent, Vector3? scale, Progression progression);

        UniTask<GameObject> LoadWithClone(int id, string assetName, Vector3 position, Quaternion rotation, Transform parent, Vector3? scale, Progression progression);
    }
}

