using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader.Cacher;
using OxGFrame.AssetLoader.GroupCacher;
using System.Linq;
using UnityEngine;

namespace OxGFrame.AssetLoader.GroupCahcer
{
    internal class GroupBundle : GroupCache<BundlePack>
    {
        private static GroupBundle _instance = null;
        public static GroupBundle GetInstance()
        {
            if (_instance == null) _instance = new GroupBundle();
            return _instance;
        }

        #region RawFile
        public async UniTask PreloadRawFileAsync(int id, string assetName, Progression progression = null)
        {
            if (string.IsNullOrEmpty(assetName)) return;

            await CacheBundle.GetInstance().PreloadRawFileAsync(assetName, progression);
            if (CacheBundle.GetInstance().HasInCache(assetName)) this.AddIntoCache(id, assetName);

            Debug.Log($"【Preload】 => Current << GroupBundle >> Cache Count: {this.Count}, GroupId: {id}");
        }

        public async UniTask PreloadRawFileAsync(int id, string[] assetNames, Progression progression = null)
        {
            if (assetNames == null || assetNames.Length == 0) return;

            await CacheBundle.GetInstance().PreloadRawFileAsync(assetNames, progression);
            foreach (string assetName in assetNames)
            {
                if (string.IsNullOrEmpty(assetName)) continue;
                if (CacheBundle.GetInstance().HasInCache(assetName)) this.AddIntoCache(id, assetName);
            }

            Debug.Log($"【Preload】 => Current << GroupBundle >> Cache Count: {this.Count}, GroupId: {id}");
        }

        public void PreloadRawFile(int id, string assetName, Progression progression = null)
        {
            if (string.IsNullOrEmpty(assetName)) return;

            CacheBundle.GetInstance().PreloadRawFile(assetName, progression);
            if (CacheBundle.GetInstance().HasInCache(assetName)) this.AddIntoCache(id, assetName);

            Debug.Log($"【Preload】 => Current << GroupBundle >> Cache Count: {this.Count}, GroupId: {id}");
        }

        public void PreloadRawFile(int id, string[] assetNames, Progression progression = null)
        {
            if (assetNames == null || assetNames.Length == 0) return;

            CacheBundle.GetInstance().PreloadRawFile(assetNames, progression);
            foreach (string assetName in assetNames)
            {
                if (string.IsNullOrEmpty(assetName)) continue;
                if (CacheBundle.GetInstance().HasInCache(assetName)) this.AddIntoCache(id, assetName);
            }

            Debug.Log($"【Preload】 => Current << GroupBundle >> Cache Count: {this.Count}, GroupId: {id}");
        }

        /// <summary>
        /// 【GroupBundle】資源加載
        /// </summary>
        /// <param name="id"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public async UniTask<T> LoadRawFileAsync<T>(int id, string assetName, Progression progression = null)
        {
            T asset = default;

            asset = await CacheBundle.GetInstance().LoadRawFileAsync<T>(assetName, progression);

            if (asset != null)
            {
                this.AddIntoCache(id, assetName);
                var keyGroup = this.GetFromCache(id, assetName);
                if (keyGroup != null)
                {
                    keyGroup.AddRef();

                    Debug.Log($"【Load】 => Current << GroupBundle >> Cache Count: {this.Count}, KeyRef: {keyGroup.refCount}, GroupId: {id}");
                }
            }

            return asset;
        }

        public T LoadRawFile<T>(int id, string assetName, Progression progression = null)
        {
            T asset = default;

            asset = CacheBundle.GetInstance().LoadRawFile<T>(assetName, progression);

            if (asset != null)
            {
                this.AddIntoCache(id, assetName);
                var keyGroup = this.GetFromCache(id, assetName);
                if (keyGroup != null)
                {
                    keyGroup.AddRef();

                    Debug.Log($"【Load】 => Current << GroupBundle >> Cache Count: {this.Count}, KeyRef: {keyGroup.refCount}, GroupId: {id}");
                }
            }

            return asset;
        }

        public void UnloadRawFile(int id, string assetName, bool forceUnload = false)
        {
            var keyGroup = this.GetFromCache(id, assetName);
            if (keyGroup != null)
            {
                keyGroup.DelRef();

                Debug.Log($"【Unload】 => Current << GroupBundle >> Cache Count: {this.Count}, KeyRef: {keyGroup.refCount}, GroupId: {id}");

                // 強制釋放
                if (forceUnload)
                {
                    this.DelFromCache(id, keyGroup.assetName);

                    Debug.Log($"【Force Unload Completes】 => Current << GroupBundle >> Cache Count: {this.Count}, GroupId: {id}");
                }
                // 使用引用計數釋放
                else if (keyGroup.refCount <= 0)
                {
                    this.DelFromCache(id, keyGroup.assetName);

                    Debug.Log($"【Unload Completes】 => Current << GroupBundle >> Cache Count: {this.Count}, GroupId: {id}");
                }

                CacheBundle.GetInstance().UnloadRawFile(keyGroup.assetName, forceUnload);
            }
        }

        public void ReleaseRawFiles(int id)
        {
            if (this._keyCacher.Count > 0)
            {
                foreach (var keyGroup in this._keyCacher.ToArray())
                {
                    if (keyGroup.id != id) continue;

                    // 依照計數次數釋放
                    for (int i = keyGroup.refCount; i > 0; i--)
                    {
                        CacheBundle.GetInstance().UnloadRawFile(keyGroup.assetName);
                    }

                    // 完成後, 直接刪除快取
                    this.DelFromCache(keyGroup.id, keyGroup.assetName);
                }
            }

            Debug.Log($"【Release Group RawFiles】 => Current << GroupBundle >> Cache Count: {this.Count}");
        }
        #endregion

        #region Asset
        public async UniTask PreloadAssetAsync(int id, string assetName, Progression progression = null)
        {
            if (string.IsNullOrEmpty(assetName)) return;

            await CacheBundle.GetInstance().PreloadAssetAsync(assetName, progression);
            if (CacheBundle.GetInstance().HasInCache(assetName)) this.AddIntoCache(id, assetName);

            Debug.Log($"【Preload】 => Current << GroupBundle >> Cache Count: {this.Count}, GroupId: {id}");
        }

        public async UniTask PreloadAssetAsync(int id, string[] assetNames, Progression progression = null)
        {
            if (assetNames == null || assetNames.Length == 0) return;

            await CacheBundle.GetInstance().PreloadAssetAsync(assetNames, progression);
            foreach (string assetName in assetNames)
            {
                if (string.IsNullOrEmpty(assetName)) continue;
                if (CacheBundle.GetInstance().HasInCache(assetName)) this.AddIntoCache(id, assetName);
            }

            Debug.Log($"【Preload】 => Current << GroupBundle >> Cache Count: {this.Count}, GroupId: {id}");
        }

        public void PreloadAsset(int id, string assetName, Progression progression = null)
        {
            if (string.IsNullOrEmpty(assetName)) return;

            CacheBundle.GetInstance().PreloadAsset(assetName, progression);
            if (CacheBundle.GetInstance().HasInCache(assetName)) this.AddIntoCache(id, assetName);

            Debug.Log($"【Preload】 => Current << GroupBundle >> Cache Count: {this.Count}, GroupId: {id}");
        }

        public void PreloadAsset(int id, string[] assetNames, Progression progression = null)
        {
            if (assetNames == null || assetNames.Length == 0) return;

            CacheBundle.GetInstance().PreloadAsset(assetNames, progression);
            foreach (string assetName in assetNames)
            {
                if (string.IsNullOrEmpty(assetName)) continue;
                if (CacheBundle.GetInstance().HasInCache(assetName)) this.AddIntoCache(id, assetName);
            }

            Debug.Log($"【Preload】 => Current << GroupBundle >> Cache Count: {this.Count}, GroupId: {id}");
        }

        /// <summary>
        /// 【GroupBundle】資源加載
        /// </summary>
        /// <param name="id"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public async UniTask<T> LoadAssetAsync<T>(int id, string assetName, Progression progression = null) where T : Object
        {
            T asset = null;

            asset = await CacheBundle.GetInstance().LoadAssetAsync<T>(assetName, progression);

            if (asset != null)
            {
                this.AddIntoCache(id, assetName);
                var keyGroup = this.GetFromCache(id, assetName);
                if (keyGroup != null)
                {
                    keyGroup.AddRef();

                    Debug.Log($"【Load】 => Current << GroupBundle >> Cache Count: {this.Count}, KeyRef: {keyGroup.refCount}, GroupId: {id}");
                }
            }

            return asset;
        }

        public T LoadAsset<T>(int id, string assetName, Progression progression = null) where T : Object
        {
            T asset = null;

            asset = CacheBundle.GetInstance().LoadAsset<T>(assetName, progression);

            if (asset != null)
            {
                this.AddIntoCache(id, assetName);
                var keyGroup = this.GetFromCache(id, assetName);
                if (keyGroup != null)
                {
                    keyGroup.AddRef();

                    Debug.Log($"【Load】 => Current << GroupBundle >> Cache Count: {this.Count}, KeyRef: {keyGroup.refCount}, GroupId: {id}");
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

                Debug.Log($"【Unload】 => Current << GroupBundle >> Cache Count: {this.Count}, KeyRef: {keyGroup.refCount}, GroupId: {id}");

                // 強制釋放
                if (forceUnload)
                {
                    this.DelFromCache(id, keyGroup.assetName);

                    Debug.Log($"【Force Unload Completes】 => Current << GroupBundle >> Cache Count: {this.Count}, GroupId: {id}");
                }
                // 使用引用計數釋放
                else if (keyGroup.refCount <= 0)
                {
                    this.DelFromCache(id, keyGroup.assetName);

                    Debug.Log($"【Unload Completes】 => Current << GroupBundle >> Cache Count: {this.Count}, GroupId: {id}");
                }

                CacheBundle.GetInstance().UnloadAsset(keyGroup.assetName, forceUnload);
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
                        CacheBundle.GetInstance().UnloadAsset(keyGroup.assetName);
                    }

                    // 完成後, 直接刪除快取
                    this.DelFromCache(keyGroup.id, keyGroup.assetName);
                }
            }

            Debug.Log($"【Release Group Assets】 => Current << GroupBundle >> Cache Count: {this.Count}");
        }
        #endregion
    }
}