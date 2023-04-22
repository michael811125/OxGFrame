using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;

namespace OxGFrame.AssetLoader.Cacher
{
    #region Base
    public class AssetObject
    {
        public string assetName = string.Empty;
        public int refCount { get; protected set; }

        public void AddRef()
        {
            this.refCount++;
        }

        public void DelRef()
        {
            this.refCount--;
        }
    }
    #endregion

    #region Resource
    public class ResourcePack : AssetObject
    {
        public Object asset;

        public T GetAsset<T>() where T : Object
        {
            return this.asset as T;
        }

        ~ResourcePack()
        {
            this.assetName = null;
            this.asset = null;
        }
    }
    #endregion

    #region Bundle
    public class BundlePack : AssetObject
    {
        public OperationHandleBase operationHandle;

        #region Checker
        public bool IsRawFileOperationHandle()
        {
            return typeof(RawFileOperationHandle).IsInstanceOfType(this.operationHandle);
        }

        public bool IsSceneOperationHandle()
        {
            return typeof(SceneOperationHandle).IsInstanceOfType(this.operationHandle);
        }

        public bool IsAssetOperationHandle()
        {
            return typeof(AssetOperationHandle).IsInstanceOfType(this.operationHandle);
        }
        #endregion

        #region Handle
        public T GetOperationHandle<T>() where T : OperationHandleBase
        {
            return this.operationHandle as T;
        }
        #endregion

        #region Raw
        public string GetRawFileText()
        {
            return this.GetOperationHandle<RawFileOperationHandle>()?.GetRawFileText();
        }

        public byte[] GetRawFileData()
        {
            return this.GetOperationHandle<RawFileOperationHandle>()?.GetRawFileData();
        }

        public void UnloadRawFile()
        {
            if (this.IsValid()) this.GetOperationHandle<RawFileOperationHandle>()?.Release();
        }
        #endregion

        #region Scene
        public Scene GetScene()
        {
            if (this.operationHandle == null) return new Scene();
            return this.GetOperationHandle<SceneOperationHandle>().SceneObject;
        }

        public void UnloadScene()
        {
            if (this.IsValid()) this.GetOperationHandle<SceneOperationHandle>()?.UnloadAsync();
        }
        #endregion

        #region Asset
        public T GetAsset<T>() where T : Object
        {
            return this.GetOperationHandle<AssetOperationHandle>()?.AssetObject as T;
        }

        public void UnloadAsset()
        {
            if (this.IsValid()) this.GetOperationHandle<AssetOperationHandle>()?.Release();
        }
        #endregion

        /// <summary>
        /// Check provider is valid
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return this.operationHandle.IsValid;
        }

        ~BundlePack()
        {
            this.assetName = null;
            this.operationHandle = null;
        }
    }
    #endregion
}