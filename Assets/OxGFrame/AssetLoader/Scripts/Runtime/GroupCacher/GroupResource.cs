using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader.Cacher;
using OxGFrame.AssetLoader.GroupCacher;
using OxGKit.LoggingSystem;
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

        public async UniTask PreloadAssetAsync<T>(int id, string[] assetNames, Progression progression, byte maxRetryCount) where T : Object
        {
            if (assetNames == null || assetNames.Length == 0) return;

            await CacheResource.GetInstance().PreloadAssetAsync<T>(assetNames, progression, maxRetryCount);
            foreach (string assetName in assetNames)
            {
                if (string.IsNullOrEmpty(assetName)) continue;
                if (CacheResource.GetInstance().HasInCache(assetName)) this.AddIntoCache(id, assetName);
            }

            Logging.Print<Logger>($"【Preload】 => Current << GroupResource >> Cache Count: {this.Count}, GroupId: {id}");
        }

        public void PreloadAsset<T>(int id, string[] assetNames, Progression progression, byte maxRetryCount) where T : Object
        {
            if (assetNames == null || assetNames.Length == 0) return;

            CacheResource.GetInstance().PreloadAsset<T>(assetNames, progression, maxRetryCount);
            foreach (string assetName in assetNames)
            {
                if (string.IsNullOrEmpty(assetName)) continue;
                if (CacheResource.GetInstance().HasInCache(assetName)) this.AddIntoCache(id, assetName);
            }

            Logging.Print<Logger>($"【Preload】 => Current << GroupResource >> Cache Count: {this.Count}, GroupId: {id}");
        }

        /// <summary>
        /// 【GroupResource】資源加載
        /// </summary>
        /// <param name="id"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public async UniTask<T> LoadAssetAsync<T>(int id, string assetName, Progression progression, byte maxRetryCount) where T : Object
        {
            T asset = null;

            asset = await CacheResource.GetInstance().LoadAssetAsync<T>(assetName, progression, maxRetryCount);

            if (asset != null)
            {
                this.AddIntoCache(id, assetName);
                var keyGroup = this.GetFromCache(id, assetName);
                if (keyGroup != null)
                {
                    keyGroup.AddRef();
                    Logging.Print<Logger>($"【Load】 => Current << GroupResource >> Cache Count: {this.Count}, KeyRef: {keyGroup.refCount}, GroupId: {id}");
                }
            }

            return asset;
        }

        public T LoadAsset<T>(int id, string assetName, Progression progression, byte maxRetryCount) where T : Object
        {
            T asset = null;

            asset = CacheResource.GetInstance().LoadAsset<T>(assetName, progression, maxRetryCount);

            if (asset != null)
            {
                this.AddIntoCache(id, assetName);
                var keyGroup = this.GetFromCache(id, assetName);
                if (keyGroup != null)
                {
                    keyGroup.AddRef();
                    Logging.Print<Logger>($"【Load】 => Current << GroupResource >> Cache Count: {this.Count}, KeyRef: {keyGroup.refCount}, GroupId: {id}");
                }
            }

            return asset;
        }

        public void UnloadAsset(int id, string assetName, bool forceUnload)
        {
            var keyGroup = this.GetFromCache(id, assetName);
            if (keyGroup != null)
            {
                keyGroup.DelRef();
                Logging.Print<Logger>($"【Unload】 => Current << GroupResource >> Cache Count: {this.Count}, KeyRef: {keyGroup.refCount}, GroupId: {id}");

                // 強制釋放
                if (forceUnload)
                {
                    this.DelFromCache(id, keyGroup.assetName);
                    Logging.Print<Logger>($"【Force Unload Completes】 => Current << GroupResource >> Cache Count: {this.Count}, GroupId: {id}");
                }
                // 使用引用計數釋放
                else if (keyGroup.refCount <= 0)
                {
                    this.DelFromCache(id, keyGroup.assetName);
                    Logging.Print<Logger>($"【Unload Completes】 => Current << GroupResource >> Cache Count: {this.Count}, GroupId: {id}");
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
                        CacheResource.GetInstance().UnloadAsset(keyGroup.assetName, false);
                    }

                    // 完成後, 直接刪除緩存
                    this.DelFromCache(keyGroup.id, keyGroup.assetName);
                }
            }

            Logging.Print<Logger>($"【Release All】 => Current << GroupResource >> Cache Count: {this.Count}");
        }
    }
}