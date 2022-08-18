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

        public KeyBundle() : base() { }

        /// <summary>
        /// 【KeyBundle】資源預加載
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        public override async UniTask PreloadInCache(int id, string bundleName, Progression progression = null)
        {
            if (string.IsNullOrEmpty(bundleName)) return;

            await CacheBundle.GetInstance().PreloadInCache(bundleName, progression);
            if (CacheBundle.GetInstance().HasInCache(bundleName)) this.AddIntoCache(id, bundleName);

            Debug.Log("【預加載】 => 當前<< KeyBundle >>快取數量 : " + this.Count);
        }

        public override async UniTask PreloadInCache(int id, string[] bundleNames, Progression progression = null)
        {
            if (bundleNames == null || bundleNames.Length == 0) return;

            await CacheBundle.GetInstance().PreloadInCache(bundleNames, progression);
            foreach (string bundleName in bundleNames)
            {
                if (string.IsNullOrEmpty(bundleName)) continue;
                if (CacheBundle.GetInstance().HasInCache(bundleName)) this.AddIntoCache(id, bundleName);
            }

            Debug.Log("【預加載】 => 當前<< KeyBundle >>快取數量 : " + this.Count);
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
                if (keyGroup != null) keyGroup.AddRef();
            }

            Debug.Log("【載入】 => 當前<< KeyBundle >>快取數量 : " + this.Count);

            return asset;
        }

        /// <summary>
        /// 【KeyBundle】資源加載並且Clone, 指定Parent, Scale
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
                if (keyGroup != null) keyGroup.AddRef();
                instGo = GameObject.Instantiate(assetGo, parent);
                Vector3 localScale = (scale == null) ? instGo.transform.localScale : (Vector3)scale;
                instGo.transform.localScale = localScale;
            }

            Debug.Log("【載入 + Clone】 => 當前<< KeyBundle >>快取數量 : " + this.Count);

            return instGo;
        }

        /// <summary>
        /// 【KeyBundle】資源加載並且Clone, 指定Position, Quternion, Parent, Scale
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
                if (keyGroup != null) keyGroup.AddRef();
                instGo = GameObject.Instantiate(assetGo, parent);
                instGo.transform.localPosition = position;
                instGo.transform.localRotation = rotation;
                Vector3 localScale = (scale == null) ? instGo.transform.localScale : (Vector3)scale;
                instGo.transform.localScale = localScale;
            }

            Debug.Log("【載入 + Clone】 => 當前<< KeyBundle >>快取數量 : " + this.Count);

            return instGo;
        }

        /// <summary>
        /// 【釋放】索引Key快取, 並且釋放資源快取
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bundleName"></param>
        public override void ReleaseFromCache(int id, string bundleName)
        {
            var keyGroup = this.GetFromCache(id, bundleName);
            if (keyGroup != null)
            {
                keyGroup.DelRef();

                // 使用引用計數釋放
                if (keyGroup.refCount <= 0) this.DelFromCache(id, keyGroup.name);
                CacheBundle.GetInstance().ReleaseFromCache(keyGroup.name);
            }

            Debug.Log("【單個釋放】 => 當前<< KeyBundle >>快取數量 : " + this.Count);
        }

        /// <summary>
        /// 【釋放】全部索引Key快取, 並且釋放資源快取
        /// </summary>
        public override void ReleaseCache(int id)
        {
            if (this._keyCacher.Count > 0)
            {
                foreach (var keyGroup in this._keyCacher.ToArray())
                {
                    if (keyGroup.id != id) continue;

                    if (keyGroup.refCount <= 0)
                    {
                        CacheBundle.GetInstance().ReleaseFromCache(keyGroup.name);
                    }
                    else
                    {
                        // 依照計數次數釋放
                        for (int i = 0; i < keyGroup.refCount; i++)
                        {
                            CacheBundle.GetInstance().ReleaseFromCache(keyGroup.name);
                        }
                    }

                    // 完成後, 直接刪除快取
                    this.DelFromCache(keyGroup.id, keyGroup.name);
                }
            }

            Debug.Log("【全部釋放】 => 當前<< KeyBundle >>快取數量 : " + this.Count);
        }
    }
}