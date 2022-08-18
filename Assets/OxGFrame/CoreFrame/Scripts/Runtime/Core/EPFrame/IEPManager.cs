using OxGFrame.AssetLoader;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace OxGFrame.CoreFrame.EPFrame
{
    public interface IEPManager
    {
        /// <summary>
        /// 預加載 (Resource)
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        UniTask Preload(string assetName, Progression progression = null);

        /// <summary>
        /// 預加載 (Bundle)
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        UniTask Preload(string bundleName, string assetName, Progression progression = null);

        /// <summary>
        /// 批次預加載 (Resource)
        /// </summary>
        /// <param name="assetNames"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        UniTask Preload(string[] assetNames, Progression progression = null);

        /// <summary>
        /// 批次預加載 (Bundle)
        /// </summary>
        /// <param name="bundleAssetNames"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        UniTask Preload(string[,] bundleAssetNames, Progression progression = null);

        /// <summary>
        /// 實例加載 (Resource)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName"></param>
        /// <param name="parent"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        UniTask<T> LoadWithClone<T>(string assetName, Transform parent = null, Progression progression = null) where T : EPBase, new();
        UniTask<T> LoadWithClone<T>(string assetName, Vector3 position, Quaternion rotation, Transform parent = null, Vector3? scale = null, Progression progression = null) where T : EPBase, new();

        /// <summary>
        /// 實例加載 (Bundle)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <param name="parent"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        UniTask<T> LoadWithClone<T>(string bundleName, string assetName, Transform parent = null, Progression progression = null) where T : EPBase, new();
        UniTask<T> LoadWithClone<T>(string bundleName, string assetName, Vector3 position, Quaternion rotation, Transform parent = null, Vector3? scale = null, Progression progression = null) where T : EPBase, new();
    }
}