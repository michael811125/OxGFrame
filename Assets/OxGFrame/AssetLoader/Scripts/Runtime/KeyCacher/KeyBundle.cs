using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader.Cacher;
using OxGFrame.AssetLoader.KeyCacher;
using System.Linq;
using UnityEngine;

namespace OxGFrame.AssetLoader.KeyCahcer
{
    public class KeyBundle : KeyCache<BundlePack>, IKeyBundle
    {
        private static KeyBundle _instance = null;
        public static KeyBundle GetInstance()
        {
            if (_instance == null) _instance = new KeyBundle();
            return _instance;
        }

        public override async UniTask Preload(int id, string bundleName, Progression progression = null)
        {
            if (string.IsNullOrEmpty(bundleName)) return;

            await CacheBundle.GetInstance().Preload(bundleName, progression);
            if (CacheBundle.GetInstance().HasInCache(bundleName)) this.AddIntoCache(id, bundleName);

            Debug.Log($"【Preload】 => Current << KeyBundle >> Cache Count: {this.Count}, GroupId: {id}");
        }

        public override async UniTask Preload(int id, string[] bundleNames, Progression progression = null)
        {
            if (bundleNames == null || bundleNames.Length == 0) return;

            await CacheBundle.GetInstance().Preload(bundleNames, progression);
            foreach (string bundleName in bundleNames)
            {
                if (string.IsNullOrEmpty(bundleName)) continue;
                if (CacheBundle.GetInstance().HasInCache(bundleName)) this.AddIntoCache(id, bundleName);
            }

            Debug.Log($"【Preload】 => Current << KeyBundle >> Cache Count: {this.Count}, GroupId: {id}");
        }

        /// <summary>
        /// 【KeyBundle】資源加載
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public async UniTask<T> Load<T>(int id, string bundleName, string assetName, Progression progression = null) where T : Object
        {
            T asset = null;

            asset = await CacheBundle.GetInstance().Load<T>(bundleName, assetName, true, progression);

            if (asset != null)
            {
                this.AddIntoCache(id, bundleName);
                var keyGroup = this.GetFromCache(id, bundleName);
                if (keyGroup != null)
                {
                    keyGroup.AddRef();

                    Debug.Log($"【Load】 => Current << KeyBundle >> Cache Count: {this.Count}, KeyRef: {keyGroup.refCount}, GroupId: {id}");
                }
            }

            return asset;
        }

        /// <summary>
        /// 【KeyBundle】資源加載並且 Clone
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <param name="parent"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public async UniTask<GameObject> LoadWithClone(int id, string bundleName, string assetName, Transform parent, Vector3? scale = null, Progression progression = null)
        {
            GameObject assetGo = null, instGo = null;

            assetGo = await CacheBundle.GetInstance().Load<GameObject>(bundleName, assetName, true, progression);

            if (assetGo != null)
            {
                this.AddIntoCache(id, bundleName);
                var keyGroup = this.GetFromCache(id, bundleName);
                if (keyGroup != null)
                {
                    keyGroup.AddRef();

                    Debug.Log($"【Load And Clone】 => Current << KeyBundle >> Cache Count: {this.Count}, KeyRef: {keyGroup.refCount}, GroupId: {id}");
                }
                instGo = GameObject.Instantiate(assetGo, parent);
                Vector3 localScale = (scale == null) ? instGo.transform.localScale : (Vector3)scale;
                instGo.transform.localScale = localScale;
            }

            return instGo;
        }

        /// <summary>
        /// 【KeyBundle】資源加載並且 Clone
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="parent"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public async UniTask<GameObject> LoadWithClone(int id, string bundleName, string assetName, Vector3 position, Quaternion rotation, Transform parent, Vector3? scale = null, Progression progression = null)
        {
            GameObject assetGo = null, instGo = null;

            assetGo = await CacheBundle.GetInstance().Load<GameObject>(bundleName, assetName, true, progression);

            if (assetGo != null)
            {
                this.AddIntoCache(id, bundleName);
                var keyGroup = this.GetFromCache(id, bundleName);
                if (keyGroup != null)
                {
                    keyGroup.AddRef();

                    Debug.Log($"【Load And Clone】 => Current << KeyBundle >> Cache Count: {this.Count}, KeyRef: {keyGroup.refCount}, GroupId: {id}");
                }
                instGo = GameObject.Instantiate(assetGo, parent);
                instGo.transform.localPosition = position;
                instGo.transform.localRotation = rotation;
                Vector3 localScale = (scale == null) ? instGo.transform.localScale : (Vector3)scale;
                instGo.transform.localScale = localScale;
            }

            return instGo;
        }

        /// <summary>
        /// 【釋放】索引 Key 快取, 並且釋放資源快取
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bundleName"></param>
        public override void Unload(int id, string bundleName)
        {
            var keyGroup = this.GetFromCache(id, bundleName);
            if (keyGroup != null)
            {
                keyGroup.DelRef();

                Debug.Log($"【Unload】 => Current << KeyBundle >> Cache Count: {this.Count}, KeyRef: {keyGroup.refCount}, GroupId: {id}");

                if (keyGroup.refCount <= 0)
                {
                    this.DelFromCache(id, keyGroup.name);

                    Debug.Log($"【Unload Completes】 => Current << KeyBundle >> Cache Count: {this.Count}, GroupId: {id}");
                }

                CacheBundle.GetInstance().Unload(keyGroup.name);
            }
        }

        /// <summary>
        /// 【強制釋放】全部索引 Key 快取, 並且釋放資源快取
        /// </summary>
        public override void Release(int id)
        {
            if (this._keyCacher.Count > 0)
            {
                foreach (var keyGroup in this._keyCacher.ToArray())
                {
                    if (keyGroup.id != id) continue;

                    // 依照計數次數釋放
                    for (int i = keyGroup.refCount; i > 0; i--)
                    {
                        CacheBundle.GetInstance().Unload(keyGroup.name);
                    }

                    // 完成後, 直接刪除快取
                    this.DelFromCache(keyGroup.id, keyGroup.name);
                }
            }

            Debug.Log($"【Release All】 => Current << KeyBundle >> Cache Count: {this.Count}");
        }
    }
}