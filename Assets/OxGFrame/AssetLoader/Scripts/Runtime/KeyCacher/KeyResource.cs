using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader.Cacher;
using OxGFrame.AssetLoader.KeyCacher;
using System.Linq;
using UnityEngine;

namespace OxGFrame.AssetLoader.KeyChacer
{
    public class KeyResource : KeyCache<ResourcePack>, IKeyResource
    {
        private static KeyResource _instance = null;
        public static KeyResource GetInstance()
        {
            if (_instance == null) _instance = new KeyResource();
            return _instance;
        }

        public override async UniTask Preload(int id, string assetName, Progression progression = null)
        {
            if (string.IsNullOrEmpty(assetName)) return;

            await CacheResource.GetInstance().Preload(assetName, progression);
            if (CacheResource.GetInstance().HasInCache(assetName)) this.AddIntoCache(id, assetName);

            Debug.Log($"【Preload】 => Current << KeyResource >> Cache Count: {this.Count}, GroupId: {id}");
        }

        public override async UniTask Preload(int id, string[] assetNames, Progression progression = null)
        {
            if (assetNames == null || assetNames.Length == 0) return;

            await CacheResource.GetInstance().Preload(assetNames, progression);
            foreach (string assetName in assetNames)
            {
                if (string.IsNullOrEmpty(assetName)) continue;
                if (CacheResource.GetInstance().HasInCache(assetName)) this.AddIntoCache(id, assetName);
            }

            Debug.Log($"【Preload】 => Current << KeyResource >> Cache Count: {this.Count}, GroupId: {id}");
        }

        /// <summary>
        /// 【KeyResource】資源加載
        /// </summary>
        /// <param name="id"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public async UniTask<T> Load<T>(int id, string assetName, Progression progression = null) where T : Object
        {
            T asset = null;

            asset = await CacheResource.GetInstance().Load<T>(assetName, progression);

            if (asset != null)
            {
                this.AddIntoCache(id, assetName);
                var keyGroup = this.GetFromCache(id, assetName);
                if (keyGroup != null)
                {
                    keyGroup.AddRef();

                    Debug.Log($"【Load】 => Current << KeyResource >> Cache Count: {this.Count}, KeyRef: {keyGroup.refCount}, GroupId: {id}");
                }
            }

            return asset;
        }

        /// <summary>
        /// 【KeyResource】資源加載並且 Clone
        /// </summary>
        /// <param name="id"></param>
        /// <param name="assetName"></param>
        /// <param name="parent"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public async UniTask<GameObject> LoadWithClone(int id, string assetName, Transform parent, Vector3? scale = null, Progression progression = null)
        {
            GameObject assetGo = null, instGo = null;

            assetGo = await CacheResource.GetInstance().Load<GameObject>(assetName, progression);

            if (assetGo != null)
            {
                this.AddIntoCache(id, assetName);
                var keyGroup = this.GetFromCache(id, assetName);
                if (keyGroup != null)
                {
                    keyGroup.AddRef();

                    Debug.Log($"【Load And Clone】 => Current << KeyResource >> Cache Count: {this.Count}, KeyRef: {keyGroup.refCount}, GroupId: {id}");
                }
                instGo = GameObject.Instantiate(assetGo, parent);
                Vector3 localScale = (scale == null) ? instGo.transform.localScale : (Vector3)scale;
                instGo.transform.localScale = localScale;
            }

            return instGo;
        }

        /// <summary>
        /// 【KeyResource】資源加載並且 Clone
        /// </summary>
        /// <param name="id"></param>
        /// <param name="assetName"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="parent"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public async UniTask<GameObject> LoadWithClone(int id, string assetName, Vector3 position, Quaternion rotation, Transform parent, Vector3? scale = null, Progression progression = null)
        {
            GameObject assetGo = null, instGo = null;

            assetGo = await CacheResource.GetInstance().Load<GameObject>(assetName, progression);

            if (assetGo != null)
            {
                this.AddIntoCache(id, assetName);
                var keyGroup = this.GetFromCache(id, assetName);
                if (keyGroup != null)
                {
                    keyGroup.AddRef();

                    Debug.Log($"【Load And Clone】 => Current << KeyResource >> Cache Count: {this.Count}, KeyRef: {keyGroup.refCount}, GroupId: {id}");
                }
                instGo = GameObject.Instantiate(assetGo, parent);
                instGo.transform.localPosition = position;
                instGo.transform.localRotation = rotation;
                Vector3 localScale = (scale == null) ? instGo.transform.localScale : (Vector3)scale;
                instGo.transform.localScale = localScale;
            }

            return instGo;
        }

        public override void Unload(int id, string assetName)
        {
            var keyGroup = this.GetFromCache(id, assetName);
            if (keyGroup != null)
            {
                keyGroup.DelRef();

                Debug.Log($"【Unload】 => Current << KeyResource >> Cache Count: {this.Count}, KeyRef: {keyGroup.refCount}, GroupId: {id}");

                // 使用引用計數釋放
                if (keyGroup.refCount <= 0)
                {
                    this.DelFromCache(id, keyGroup.name);

                    Debug.Log($"【Unload Completes】 => Current << KeyResource >> Cache Count: {this.Count}, GroupId: {id}");
                }

                CacheResource.GetInstance().Unload(keyGroup.name);
            }
        }

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
                        CacheResource.GetInstance().Unload(keyGroup.name);
                    }

                    // 完成後, 直接刪除快取
                    this.DelFromCache(keyGroup.id, keyGroup.name);
                }
            }

            Debug.Log($"【Release All】 => Current << KeyResource >> Cache Count: {this.Count}");
        }
    }
}