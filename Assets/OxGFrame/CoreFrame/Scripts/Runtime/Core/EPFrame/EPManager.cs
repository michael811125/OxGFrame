using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader;
using UnityEngine;

namespace OxGFrame.CoreFrame.EPFrame
{
    public class EPManager
    {
        public float reqSize { get; protected set; }   // [計算進度條用] 加載數量
        public float totalSize { get; protected set; } // [計算進度條用] 總加載數量

        private static readonly object _locker = new object();
        private static EPManager _instance = null;
        internal static EPManager GetInstance()
        {
            if (_instance == null)
            {
                lock (_locker)
                {
                    _instance = new EPManager();
                }
            }
            return _instance;
        }

        /// <summary>
        /// 載入 GameObject
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        protected async UniTask<GameObject> LoadGameObjectAsync(string assetName, Progression progression)
        {
            if (string.IsNullOrEmpty(assetName)) return null;

            GameObject obj = await AssetLoaders.LoadAssetAsync<GameObject>(assetName, progression);

            if (obj == null)
            {
                Debug.LogWarning(string.Format("【 path: {0} 】asset not found at this path!!!", assetName));
                return null;
            }

            return obj;
        }

        protected GameObject LoadGameObject(string assetName, Progression progression)
        {
            if (string.IsNullOrEmpty(assetName)) return null;

            GameObject obj = AssetLoaders.LoadAsset<GameObject>(assetName, progression);

            if (obj == null)
            {
                Debug.LogWarning(string.Format("【 path: {0} 】asset not found at this path!!!", assetName));
                return null;
            }

            return obj;
        }

        /// <summary>
        /// 預加載
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public async UniTask PreloadAsync(string assetName, Progression progression = null)
        {
            await AssetLoaders.PreloadAssetAsync(assetName, progression);
        }

        /// <summary>
        /// 預加載
        /// </summary>
        /// <param name="assetNames"></param>
        /// <returns></returns>
        public async UniTask PreloadAsync(string[] assetNames, Progression progression = null)
        {
            await AssetLoaders.PreloadAssetAsync(assetNames, progression);
        }

        public void Preload(string assetName, Progression progression = null)
        {
            AssetLoaders.PreloadAsset(assetName, progression);
        }

        public void Preload(string[] assetNames, Progression progression = null)
        {
            AssetLoaders.PreloadAsset(assetNames, progression);
        }

        public async UniTask<T> LoadWithCloneAsync<T>(string assetName, Transform parent = null, Progression progression = null) where T : EPBase, new()
        {
            GameObject go = await this.LoadGameObjectAsync(assetName, progression);
            if (go == null) return null;

            GameObject instGo = GameObject.Instantiate(go, parent);

            // 激活檢查, 如果主體 Active 為 false 必須打開
            bool active;
            if (!instGo.activeSelf)
            {
                active = instGo.activeSelf;
                instGo.SetActive(true);
            }
            else active = instGo.activeSelf;

            T entityBase = instGo.GetComponent<T>();
            if (entityBase == null) return null;

            entityBase.SetNames(assetName);
            entityBase.OnInit();
            entityBase.InitFirst();
            if (active) entityBase.Display(null); // 預製體如果製作時, 本身主體 Active 為 true 才調用 Display => OnShow

            // 最後還原本身預製體的 Active
            instGo.SetActive(active);

            return entityBase;
        }

        public async UniTask<T> LoadWithCloneAsync<T>(string assetName, Transform parent, bool worldPositionStays, Progression progression = null) where T : EPBase, new()
        {
            GameObject go = await this.LoadGameObjectAsync(assetName, progression);
            if (go == null) return null;

            GameObject instGo = GameObject.Instantiate(go, parent, worldPositionStays);

            // 激活檢查, 如果主體 Active 為 false 必須打開
            bool active;
            if (!instGo.activeSelf)
            {
                active = instGo.activeSelf;
                instGo.SetActive(true);
            }
            else active = instGo.activeSelf;

            T entityBase = instGo.GetComponent<T>();
            if (entityBase == null) return null;

            entityBase.SetNames(assetName);
            entityBase.OnInit();
            entityBase.InitFirst();
            if (active) entityBase.Display(null); // 預製體如果製作時, 本身主體 Active 為 true 才調用 Display => OnShow

            // 最後還原本身預製體的 Active
            instGo.SetActive(active);

            return entityBase;
        }

        public async UniTask<T> LoadWithCloneAsync<T>(string assetName, Vector3 position, Quaternion rotation, Transform parent = null, Vector3? scale = null, Progression progression = null) where T : EPBase, new()
        {
            GameObject go = await this.LoadGameObjectAsync(assetName, progression);
            if (go == null) return null;

            GameObject instGo = GameObject.Instantiate(go, position, rotation, parent);

            // 激活檢查, 如果主體 Active 為 false 必須打開
            bool active;
            if (!instGo.activeSelf)
            {
                active = instGo.activeSelf;
                instGo.SetActive(true);
            }
            else active = instGo.activeSelf;

            Vector3 localScale = (scale == null) ? instGo.transform.localScale : (Vector3)scale;
            instGo.transform.localScale = localScale;

            T entityBase = instGo.GetComponent<T>();
            if (entityBase == null) return null;

            entityBase.SetNames(assetName);
            entityBase.OnInit();
            entityBase.InitFirst();
            if (active) entityBase.Display(null); // 預製體如果製作時, 本身主體 Active 為 true 才調用 Display => OnShow

            // 最後還原本身預製體的 Active
            instGo.SetActive(active);

            return entityBase;
        }

        public T LoadWithClone<T>(string assetName, Transform parent = null, Progression progression = null) where T : EPBase, new()
        {
            GameObject go = this.LoadGameObject(assetName, progression);
            if (go == null) return null;

            GameObject instGo = GameObject.Instantiate(go, parent);

            // 激活檢查, 如果主體 Active 為 false 必須打開
            bool active;
            if (!instGo.activeSelf)
            {
                active = instGo.activeSelf;
                instGo.SetActive(true);
            }
            else active = instGo.activeSelf;

            T entityBase = instGo.GetComponent<T>();
            if (entityBase == null) return null;

            entityBase.SetNames(assetName);
            entityBase.OnInit();
            entityBase.InitFirst();
            if (active) entityBase.Display(null); // 預製體如果製作時, 本身主體 Active 為 true 才調用 Display => OnShow

            // 最後還原本身預製體的 Active
            instGo.SetActive(active);

            return entityBase;
        }

        public T LoadWithClone<T>(string assetName, Transform parent, bool worldPositionStays, Progression progression = null) where T : EPBase, new()
        {
            GameObject go = this.LoadGameObject(assetName, progression);
            if (go == null) return null;

            GameObject instGo = GameObject.Instantiate(go, parent, worldPositionStays);

            // 激活檢查, 如果主體 Active 為 false 必須打開
            bool active;
            if (!instGo.activeSelf)
            {
                active = instGo.activeSelf;
                instGo.SetActive(true);
            }
            else active = instGo.activeSelf;

            T entityBase = instGo.GetComponent<T>();
            if (entityBase == null) return null;

            entityBase.SetNames(assetName);
            entityBase.OnInit();
            entityBase.InitFirst();
            if (active) entityBase.Display(null); // 預製體如果製作時, 本身主體 Active 為 true 才調用 Display => OnShow

            // 最後還原本身預製體的 Active
            instGo.SetActive(active);

            return entityBase;
        }

        public T LoadWithClone<T>(string assetName, Vector3 position, Quaternion rotation, Transform parent = null, Vector3? scale = null, Progression progression = null) where T : EPBase, new()
        {
            GameObject go = this.LoadGameObject(assetName, progression);
            if (go == null) return null;

            GameObject instGo = GameObject.Instantiate(go, position, rotation, parent);

            // 激活檢查, 如果主體 Active 為 false 必須打開
            bool active;
            if (!instGo.activeSelf)
            {
                active = instGo.activeSelf;
                instGo.SetActive(true);
            }
            else active = instGo.activeSelf;

            Vector3 localScale = (scale == null) ? instGo.transform.localScale : (Vector3)scale;
            instGo.transform.localScale = localScale;

            T entityBase = instGo.GetComponent<T>();
            if (entityBase == null) return null;

            entityBase.SetNames(assetName);
            entityBase.OnInit();
            entityBase.InitFirst();
            if (active) entityBase.Display(null); // 預製體如果製作時, 本身主體 Active 為 true 才調用 Display => OnShow

            // 最後還原本身預製體的 Active
            instGo.SetActive(active);

            return entityBase;
        }
    }
}
