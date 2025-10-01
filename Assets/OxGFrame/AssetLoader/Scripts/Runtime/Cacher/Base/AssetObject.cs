using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;

namespace OxGFrame.AssetLoader.Cacher
{
    #region Base
    public abstract class AssetObject
    {
        /// <summary>
        /// 資源定位名稱
        /// </summary>
        public string assetName { get; protected set; }

        /// <summary>
        /// 資源引用次數
        /// </summary>
        public int refCount { get; protected set; }

        /// <summary>
        /// 計數加 1
        /// </summary>
        public void AddRef()
        {
            this.refCount++;
        }

        /// <summary>
        /// 計數減 1
        /// </summary>
        public void DelRef()
        {
            this.refCount--;
        }

        /// <summary>
        /// 是否可釋放 (計數為 0 返回 true)
        /// </summary>
        /// <returns></returns>
        public bool IsReleasable()
        {
            return this.refCount <= 0;
        }
    }
    #endregion

    #region Resource
    public class ResourcePack : AssetObject
    {
        public Object asset { get; protected set; } = null;

        /// <summary>
        /// Set pack info
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="asset"></param>
        public void SetPack(string assetName, Object asset)
        {
            this.assetName = assetName;
            this.asset = asset;
        }

        public T GetAsset<T>() where T : Object
        {
            if (this.asset == null)
                return default;
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
        /// <summary>
        /// 包裹名稱
        /// </summary>
        public string packageName { get; protected set; }

        /// <summary>
        /// 資源句柄
        /// </summary>
        public HandleBase operationHandle { get; protected set; } = null;

        /// <summary>
        /// Set pack info
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="operationHandle"></param>
        public void SetPack(string packageName, string assetName, HandleBase operationHandle)
        {
            this.packageName = packageName;
            this.assetName = assetName;
            this.operationHandle = operationHandle;
        }

        #region Checker
        /// <summary>
        /// Check provider is valid
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            if (this.operationHandle == null)
                return false;
            return this.operationHandle.IsValid;
        }

        public bool IsRawFileOperationHandle()
        {
            return typeof(RawFileHandle).IsInstanceOfType(this.operationHandle);
        }

        public bool IsRawFileOperationHandleValid()
        {
            return this.IsValid() && this.IsRawFileOperationHandle();
        }

        public bool IsSceneOperationHandle()
        {
            return typeof(SceneHandle).IsInstanceOfType(this.operationHandle);
        }

        public bool IsSceneOperationHandleValid()
        {
            return this.IsValid() && this.IsSceneOperationHandle();
        }

        public bool IsAssetOperationHandle()
        {
            return typeof(AssetHandle).IsInstanceOfType(this.operationHandle);
        }

        public bool IsAssetOperationHandleValid()
        {
            return this.IsValid() && this.IsAssetOperationHandle();
        }
        #endregion

        #region Handle
        public T GetOperationHandle<T>() where T : HandleBase
        {
            return this.operationHandle as T;
        }
        #endregion

        #region RawFile
        public string GetRawFileText()
        {
            if (!this.IsRawFileOperationHandleValid())
                return null;
            return this.GetOperationHandle<RawFileHandle>().GetRawFileText();
        }

        public byte[] GetRawFileData()
        {
            if (!this.IsRawFileOperationHandleValid())
                return null;
            return this.GetOperationHandle<RawFileHandle>().GetRawFileData();
        }

        public void UnloadRawFile()
        {
            if (this.IsRawFileOperationHandleValid())
                this.GetOperationHandle<RawFileHandle>().Release();
        }
        #endregion

        #region Scene
        public Scene GetScene()
        {
            if (!this.IsSceneOperationHandleValid())
                return new Scene();
            return this.GetOperationHandle<SceneHandle>().SceneObject;
        }

        public bool UnsuspendScene()
        {
            if (this.IsSceneOperationHandleValid())
                return this.GetOperationHandle<SceneHandle>().UnSuspend();
            return false;
        }

        public void UnloadScene()
        {
            if (this.IsSceneOperationHandleValid())
                this.GetOperationHandle<SceneHandle>().UnloadAsync();
        }
        #endregion

        #region Asset
        public T GetAsset<T>() where T : Object
        {
            if (!this.IsAssetOperationHandleValid())
                return null;
            return this.GetOperationHandle<AssetHandle>().AssetObject as T;
        }

        public void UnloadAsset()
        {
            if (this.IsAssetOperationHandleValid())
                this.GetOperationHandle<AssetHandle>().Release();
        }
        #endregion

        ~BundlePack()
        {
            this.assetName = null;
            this.operationHandle = null;
        }
    }
    #endregion
}