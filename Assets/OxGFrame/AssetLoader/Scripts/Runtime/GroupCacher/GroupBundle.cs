using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader.Cacher;
using OxGKit.LoggingSystem;
using System.Linq;
using UnityEngine;

namespace OxGFrame.AssetLoader.GroupCacher
{
    internal class GroupBundle : GroupCache<BundlePack>
    {
        private static GroupBundle _instance = null;
        public static GroupBundle GetInstance()
        {
            if (_instance == null)
                _instance = new GroupBundle();
            return _instance;
        }

        #region RawFile
        public async UniTask PreloadRawFileAsync(int id, string packageName, string[] assetNames, uint priority, Progression progression, byte maxRetryCount)
        {
            if (assetNames == null || assetNames.Length == 0)
                return;

            await CacheBundle.GetInstance().PreloadRawFileAsync(packageName, assetNames, priority, progression, maxRetryCount);
            foreach (string assetName in assetNames)
            {
                if (string.IsNullOrEmpty(assetName))
                    continue;
                if (CacheBundle.GetInstance().HasInCache(assetName))
                    this.AddIntoCache(id, assetName);
            }

            Logging.Print<Logger>($"【Preload】 => Current << {nameof(GroupBundle)} >> Cache Count: {this.Count}, GroupId: {id}");
        }

        public void PreloadRawFile(int id, string packageName, string[] assetNames, Progression progression, byte maxRetryCount)
        {
            if (assetNames == null || assetNames.Length == 0)
                return;

            CacheBundle.GetInstance().PreloadRawFile(packageName, assetNames, progression, maxRetryCount);
            foreach (string assetName in assetNames)
            {
                if (string.IsNullOrEmpty(assetName))
                    continue;
                if (CacheBundle.GetInstance().HasInCache(assetName))
                    this.AddIntoCache(id, assetName);
            }

            Logging.Print<Logger>($"【Preload】 => Current << {nameof(GroupBundle)} >> Cache Count: {this.Count}, GroupId: {id}");
        }

        /// <summary>
        /// 【GroupBundle】資源加載
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="packageName"></param>
        /// <param name="assetName"></param>
        /// <param name="priority"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        public async UniTask<T> LoadRawFileAsync<T>(int id, string packageName, string assetName, uint priority, Progression progression, byte maxRetryCount)
        {
            T asset = default;

            asset = await CacheBundle.GetInstance().LoadRawFileAsync<T>(packageName, assetName, priority, progression, maxRetryCount);

            if (asset != null)
            {
                this.AddIntoCache(id, assetName);
                var keyGroup = this.GetFromCache(id, assetName);
                if (keyGroup != null)
                {
                    keyGroup.AddRef();
                    Logging.Print<Logger>($"【Load】 => Current << {nameof(GroupBundle)} >> Cache Count: {this.Count}, KeyRef: {keyGroup.refCount}, GroupId: {id}");
                }
            }

            return asset;
        }

        public T LoadRawFile<T>(int id, string packageName, string assetName, Progression progression, byte maxRetryCount)
        {
            T asset = default;

            asset = CacheBundle.GetInstance().LoadRawFile<T>(packageName, assetName, progression, maxRetryCount);

            if (asset != null)
            {
                this.AddIntoCache(id, assetName);
                var keyGroup = this.GetFromCache(id, assetName);
                if (keyGroup != null)
                {
                    keyGroup.AddRef();
                    Logging.Print<Logger>($"【Load】 => Current << {nameof(GroupBundle)} >> Cache Count: {this.Count}, KeyRef: {keyGroup.refCount}, GroupId: {id}");
                }
            }

            return asset;
        }

        public void UnloadRawFile(int id, string assetName, bool forceUnload)
        {
            var keyGroup = this.GetFromCache(id, assetName);
            if (keyGroup != null)
            {
                keyGroup.DelRef();
                Logging.Print<Logger>($"【Unload】 => Current << {nameof(GroupBundle)} >> Cache Count: {this.Count}, KeyRef: {keyGroup.refCount}, GroupId: {id}");

                // 強制釋放
                if (forceUnload)
                {
                    this.DelFromCache(id, keyGroup.assetName);
                    Logging.Print<Logger>($"【Force Unload Completes】 => Current << {nameof(GroupBundle)} >> Cache Count: {this.Count}, GroupId: {id}");
                }
                // 使用引用計數釋放
                else if (keyGroup.refCount <= 0)
                {
                    this.DelFromCache(id, keyGroup.assetName);
                    Logging.Print<Logger>($"【Unload Completes】 => Current << {nameof(GroupBundle)} >> Cache Count: {this.Count}, GroupId: {id}");
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
                    if (keyGroup.id != id)
                        continue;

                    // 依照計數次數釋放
                    for (int i = keyGroup.refCount; i > 0; i--)
                    {
                        CacheBundle.GetInstance().UnloadRawFile(keyGroup.assetName, false);
                    }

                    // 完成後, 直接刪除緩存
                    this.DelFromCache(keyGroup.id, keyGroup.assetName);
                }
            }

            Logging.Print<Logger>($"【Release Group RawFiles】 => Current << {nameof(GroupBundle)} >> Cache Count: {this.Count}");
        }
        #endregion

        #region Asset
        public async UniTask PreloadAssetAsync<T>(int id, string packageName, string[] assetNames, uint priority, Progression progression, byte maxRetryCount) where T : Object
        {
            if (assetNames == null || assetNames.Length == 0)
                return;

            await CacheBundle.GetInstance().PreloadAssetAsync<T>(packageName, assetNames, priority, progression, maxRetryCount);
            foreach (string assetName in assetNames)
            {
                if (string.IsNullOrEmpty(assetName))
                    continue;
                if (CacheBundle.GetInstance().HasInCache(assetName))
                    this.AddIntoCache(id, assetName);
            }

            Logging.Print<Logger>($"【Preload】 => Current << {nameof(GroupBundle)} >> Cache Count: {this.Count}, GroupId: {id}");
        }

        public void PreloadAsset<T>(int id, string packageName, string[] assetNames, Progression progression, byte maxRetryCount) where T : Object
        {
            if (assetNames == null || assetNames.Length == 0)
                return;

            CacheBundle.GetInstance().PreloadAsset<T>(packageName, assetNames, progression, maxRetryCount);
            foreach (string assetName in assetNames)
            {
                if (string.IsNullOrEmpty(assetName))
                    continue;
                if (CacheBundle.GetInstance().HasInCache(assetName))
                    this.AddIntoCache(id, assetName);
            }

            Logging.Print<Logger>($"【Preload】 => Current << {nameof(GroupBundle)} >> Cache Count: {this.Count}, GroupId: {id}");
        }

        /// <summary>
        /// 【GroupBundle】資源加載
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="packageName"></param>
        /// <param name="assetName"></param>
        /// <param name="priority"></param>
        /// <param name="progression"></param>
        /// <param name="maxRetryCount"></param>
        /// <returns></returns>
        public async UniTask<T> LoadAssetAsync<T>(int id, string packageName, string assetName, uint priority, Progression progression, byte maxRetryCount) where T : Object
        {
            T asset = null;

            asset = await CacheBundle.GetInstance().LoadAssetAsync<T>(packageName, assetName, priority, progression, maxRetryCount);

            if (asset != null)
            {
                this.AddIntoCache(id, assetName);
                var keyGroup = this.GetFromCache(id, assetName);
                if (keyGroup != null)
                {
                    keyGroup.AddRef();
                    Logging.Print<Logger>($"【Load】 => Current << {nameof(GroupBundle)} >> Cache Count: {this.Count}, KeyRef: {keyGroup.refCount}, GroupId: {id}");
                }
            }

            return asset;
        }

        public T LoadAsset<T>(int id, string packageName, string assetName, Progression progression, byte maxRetryCount) where T : Object
        {
            T asset = null;

            asset = CacheBundle.GetInstance().LoadAsset<T>(packageName, assetName, progression, maxRetryCount);

            if (asset != null)
            {
                this.AddIntoCache(id, assetName);
                var keyGroup = this.GetFromCache(id, assetName);
                if (keyGroup != null)
                {
                    keyGroup.AddRef();
                    Logging.Print<Logger>($"【Load】 => Current << {nameof(GroupBundle)} >> Cache Count: {this.Count}, KeyRef: {keyGroup.refCount}, GroupId: {id}");
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
                Logging.Print<Logger>($"【Unload】 => Current << {nameof(GroupBundle)} >> Cache Count: {this.Count}, KeyRef: {keyGroup.refCount}, GroupId: {id}");

                // 強制釋放
                if (forceUnload)
                {
                    this.DelFromCache(id, keyGroup.assetName);
                    Logging.Print<Logger>($"【Force Unload Completes】 => Current << {nameof(GroupBundle)} >> Cache Count: {this.Count}, GroupId: {id}");
                }
                // 使用引用計數釋放
                else if (keyGroup.refCount <= 0)
                {
                    this.DelFromCache(id, keyGroup.assetName);
                    Logging.Print<Logger>($"【Unload Completes】 => Current << {nameof(GroupBundle)} >> Cache Count: {this.Count}, GroupId: {id}");
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
                    if (keyGroup.id != id)
                        continue;

                    // 依照計數次數釋放
                    for (int i = keyGroup.refCount; i > 0; i--)
                    {
                        CacheBundle.GetInstance().UnloadAsset(keyGroup.assetName, false);
                    }

                    // 完成後, 直接刪除緩存
                    this.DelFromCache(keyGroup.id, keyGroup.assetName);
                }
            }

            Logging.Print<Logger>($"【Release Group Assets】 => Current << {nameof(GroupBundle)} >> Cache Count: {this.Count}");
        }
        #endregion
    }
}