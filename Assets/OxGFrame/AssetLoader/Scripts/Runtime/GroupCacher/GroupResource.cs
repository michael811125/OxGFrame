using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader.Cacher;
using OxGFrame.AssetLoader.GroupCacher;
using System.Linq;
using UnityEngine;

namespace OxGFrame.AssetLoader.GroupChacer
{
    internal class GroupResource : GroupCache<ResourcePack>
    {
        private static GroupResource _instance = null;
        public static GroupResource GetInstance()
        {
            if (_instance == null) _instance = new GroupResource();
            return _instance;
        }

        public async UniTask PreloadAssetAsync(int id, string assetName, Progression progression = null)
        {
            if (string.IsNullOrEmpty(assetName)) return;

            await CacheResource.GetInstance().PreloadAssetAsync(assetName, progression);
            if (CacheResource.GetInstance().HasInCache(assetName)) this.AddIntoCache(id, assetName);

            Debug.Log($"【Preload】 => Current << GroupResource >> Cache Count: {this.Count}, GroupId: {id}");
        }

        public async UniTask PreloadAssetAsync(int id, string[] assetNames, Progression progression = null)
        {
            if (assetNames == null || assetNames.Length == 0) return;

            await CacheResource.GetInstance().PreloadAssetAsync(assetNames, progression);
            foreach (string assetName in assetNames)
            {
                if (string.IsNullOrEmpty(assetName)) continue;
                if (CacheResource.GetInstance().HasInCache(assetName)) this.AddIntoCache(id, assetName);
            }

            Debug.Log($"【Preload】 => Current << GroupResource >> Cache Count: {this.Count}, GroupId: {id}");
        }

        public void PreloadAsset(int id, string assetName, Progression progression = null)
        {
            if (string.IsNullOrEmpty(assetName)) return;

            CacheResource.GetInstance().PreloadAsset(assetName, progression);
            if (CacheResource.GetInstance().HasInCache(assetName)) this.AddIntoCache(id, assetName);

            Debug.Log($"【Preload】 => Current << GroupResource >> Cache Count: {this.Count}, GroupId: {id}");
        }

        public void PreloadAsset(int id, string[] assetNames, Progression progression = null)
        {
            if (assetNames == null || assetNames.Length == 0) return;

            CacheResource.GetInstance().PreloadAsset(assetNames, progression);
            foreach (string assetName in assetNames)
            {
                if (string.IsNullOrEmpty(assetName)) continue;
                if (CacheResource.GetInstance().HasInCache(assetName)) this.AddIntoCache(id, assetName);
            }

            Debug.Log($"【Preload】 => Current << GroupResource >> Cache Count: {this.Count}, GroupId: {id}");
        }

        /// <summary>
        /// 【GroupResource】資源加載
        /// </summary>
        /// <param name="id"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public async UniTask<T> LoadAssetAsync<T>(int id, string assetName, Progression progression = null) where T : Object
        {
            T asset = null;

            asset = await CacheResource.GetInstance().LoadAssetAsync<T>(assetName, progression);

            if (asset != null)
            {
                this.AddIntoCache(id, assetName);
                var keyGroup = this.GetFromCache(id, assetName);
                if (keyGroup != null)
                {
                    keyGroup.AddRef();

                    Debug.Log($"【Load】 => Current << GroupResource >> Cache Count: {this.Count}, KeyRef: {keyGroup.refCount}, GroupId: {id}");
                }
            }

            return asset;
        }

        public T LoadAsset<T>(int id, string assetName, Progression progression = null) where T : Object
        {
            T asset = null;

            asset = CacheResource.GetInstance().LoadAsset<T>(assetName, progression);

            if (asset != null)
            {
                this.AddIntoCache(id, assetName);
                var keyGroup = this.GetFromCache(id, assetName);
                if (keyGroup != null)
                {
                    keyGroup.AddRef();

                    Debug.Log($"【Load】 => Current << GroupResource >> Cache Count: {this.Count}, KeyRef: {keyGroup.refCount}, GroupId: {id}");
                }
            }

            return asset;
        }

        public void UnloadAsset(int id, string assetName, bool forceUnload = false)
        {
            var keyGroup = this.GetFromCache(id, assetName);
            if (keyGroup != null)
            {
                keyGroup.DelRef();

                Debug.Log($"【Unload】 => Current << GroupResource >> Cache Count: {this.Count}, KeyRef: {keyGroup.refCount}, GroupId: {id}");

                // 強制釋放
                if (forceUnload)
                {
                    this.DelFromCache(id, keyGroup.assetName);

                    Debug.Log($"【Force Unload Completes】 => Current << GroupResource >> Cache Count: {this.Count}, GroupId: {id}");
                }
                // 使用引用計數釋放
                else if (keyGroup.refCount <= 0)
                {
                    this.DelFromCache(id, keyGroup.assetName);

                    Debug.Log($"【Unload Completes】 => Current << GroupResource >> Cache Count: {this.Count}, GroupId: {id}");
                }

                CacheResource.GetInstance().UnloadAsset(keyGroup.assetName, forceUnload);
            }
        }

        public void ReleaseAssets(int id)
        {
            if (this._keyCacher.Count > 0)
            {
                foreach (var keyGroup in this._keyCacher.ToArray())
                {
                    if (keyGroup.id != id) continue;

                    // 依照計數次數釋放
                    for (int i = keyGroup.refCount; i > 0; i--)
                    {
                        CacheResource.GetInstance().UnloadAsset(keyGroup.assetName);
                    }

                    // 完成後, 直接刪除緩存
                    this.DelFromCache(keyGroup.id, keyGroup.assetName);
                }
            }

            Debug.Log($"【Release All】 => Current << GroupResource >> Cache Count: {this.Count}");
        }
    }
}