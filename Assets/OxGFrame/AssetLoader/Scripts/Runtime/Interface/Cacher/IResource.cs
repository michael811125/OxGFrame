using Cysharp.Threading.Tasks;
using UnityEngine;

namespace OxGFrame.AssetLoader
{
    public interface IResource
    {
        UniTask<T> Load<T>(string assetName, Progression progression = null) where T : Object;
    }
}
