#if YOOASSET_3
using OxGFrame.AssetLoader.Bundle;
#endif
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
#if YOOASSET_2
            return this.operationHandle is RawFileHandle;
#else
            // [YooAsset v3] RawFileHandle 已移除，Raw File 統一由 AssetHandle 載入 RawFileObject
            if (this.operationHandle is AssetHandle handle)
            {
                if (handle.IsValid)
                    return handle.AssetObject is RawFileObject;
            }
            return false;
#endif
        }

        public bool IsRawFileOperationHandleValid()
        {
            return this.IsValid() && this.IsRawFileOperationHandle();
        }

        public bool IsSceneOperationHandle()
        {
            return this.operationHandle is YooAsset.SceneHandle;
        }

        public bool IsSceneOperationHandleValid()
        {
            return this.IsValid() && this.IsSceneOperationHandle();
        }

        public bool IsAssetOperationHandle()
        {
            return this.operationHandle is AssetHandle;
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
#if YOOASSET_2
            return this.GetOperationHandle<RawFileHandle>().GetRawFileText();
#else
            // [YooAsset v3] 由 RawFileObject 取得文字內容
            return this.GetOperationHandle<AssetHandle>().GetAssetObject<RawFileObject>()?.GetText();
#endif
        }

        public byte[] GetRawFileData()
        {
            if (!this.IsRawFileOperationHandleValid())
                return null;
#if YOOASSET_2
            return this.GetOperationHandle<RawFileHandle>().GetRawFileData();
#else
            // [YooAsset v3] 由 RawFileObject 取得位元組內容
            return this.GetOperationHandle<AssetHandle>().GetAssetObject<RawFileObject>()?.GetBytes();
#endif
        }

        public void UnloadRawFile()
        {
            if (this.IsRawFileOperationHandleValid())
#if YOOASSET_2
                this.GetOperationHandle<RawFileHandle>().Release();
#else
                // [YooAsset v3] Raw File 由 AssetHandle 釋放
                this.GetOperationHandle<AssetHandle>().Release();
#endif
        }
        #endregion

        #region Scene
        public Scene GetScene()
        {
            if (!this.IsSceneOperationHandleValid())
                return new Scene();
            return this.GetOperationHandle<YooAsset.SceneHandle>().SceneObject;
        }

        public bool UnsuspendScene()
        {
            if (this.IsSceneOperationHandleValid())
                return this.GetOperationHandle<YooAsset.SceneHandle>().UnSuspend();
            return false;
        }

        public void UnloadScene()
        {
            if (this.IsSceneOperationHandleValid())
                this.GetOperationHandle<YooAsset.SceneHandle>().UnloadAsync();
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
    }
    #endregion
}
