using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetLoader
{
    public interface IResource
    {
        UniTask<T> Load<T>(string assetName, Progression progression = null) where T : Object;
    }
}
