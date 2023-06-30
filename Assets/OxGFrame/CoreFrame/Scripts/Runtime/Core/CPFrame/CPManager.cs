using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader;
using UnityEngine;

namespace OxGFrame.CoreFrame.CPFrame
{
    internal class CPManager
    {
        public float reqSize { get; protected set; }   // [計算進度條用] 加載數量
        public float totalSize { get; protected set; } // [計算進度條用] 總加載數量

        private static readonly object _locker = new object();
        private static CPManager _instance = null;
        public static CPManager GetInstance()
        {
            if (_instance == null)
            {
                lock (_locker)
                {
                    _instance = new CPManager();
                }
            }
            return _instance;
        }

        protected async UniTask<GameObject> LoadGameObjectAsync(string packageName, string assetName, Progression progression)
        {
            if (string.IsNullOrEmpty(assetName)) return null;

            GameObject obj = await AssetLoaders.LoadAssetAsync<GameObject>(packageName, assetName, progression);

            if (obj == null)
            {
                Debug.LogWarning(string.Format("【 path: {0} 】asset not found at this path!!!", assetName));
                return null;
            }

            return obj;
        }

        protected GameObject LoadGameObject(string packageName, string assetName, Progression progression)
        {
            if (string.IsNullOrEmpty(assetName)) return null;

            GameObject obj = AssetLoaders.LoadAsset<GameObject>(packageName, assetName, progression);

            if (obj == null)
            {
                Debug.LogWarning(string.Format("【 path: {0} 】asset not found at this path!!!", assetName));
                return null;
            }

            return obj;
        }

        public async UniTask PreloadAsync(string packageName, string[] assetNames, Progression progression = null)
        {
            await AssetLoaders.PreloadAssetAsync<Object>(packageName, assetNames, progression);
        }

        public async UniTask PreloadAsync<T>(string packageName, string[] assetNames, Progression progression = null) where T : Object
        {
            await AssetLoaders.PreloadAssetAsync<T>(packageName, assetNames, progression);
        }

        public void Preload(string packageName, string[] assetNames, Progression progression = null)
        {
            AssetLoaders.PreloadAsset<Object>(packageName, assetNames, progression);
        }

        public void Preload<T>(string packageName, string[] assetNames, Progression progression = null) where T : Object
        {
            AssetLoaders.PreloadAsset<T>(packageName, assetNames, progression);
        }

        public async UniTask<T> LoadWithCloneAsync<T>(string packageName, string assetName, Transform parent = null, Progression progression = null) where T : CPBase, new()
        {
            GameObject go = await this.LoadGameObjectAsync(packageName, assetName, progression);
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

            T cpBase = instGo.GetComponent<T>();
            if (cpBase == null) return null;

            cpBase.SetNames(assetName);
            cpBase.OnInit();
            cpBase.InitFirst();
            // 預製體如果製作時, 本身主體 Active 為 true 才調用 Display => OnShow
            if (active) cpBase.Display(null);

            // 最後還原本身預製體的 Active
            instGo.SetActive(active);

            return cpBase;
        }

        public async UniTask<T> LoadWithCloneAsync<T>(string packageName, string assetName, Transform parent, bool worldPositionStays, Progression progression = null) where T : CPBase, new()
        {
            GameObject go = await this.LoadGameObjectAsync(packageName, assetName, progression);
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

            T cpBase = instGo.GetComponent<T>();
            if (cpBase == null) return null;

            cpBase.SetNames(assetName);
            cpBase.OnInit();
            cpBase.InitFirst();
            // 預製體如果製作時, 本身主體 Active 為 true 才調用 Display => OnShow
            if (active) cpBase.Display(null);

            // 最後還原本身預製體的 Active
            instGo.SetActive(active);

            return cpBase;
        }

        public async UniTask<T> LoadWithCloneAsync<T>(string packageName, string assetName, Vector3 position, Quaternion rotation, Transform parent = null, Vector3? scale = null, Progression progression = null) where T : CPBase, new()
        {
            GameObject go = await this.LoadGameObjectAsync(packageName, assetName, progression);
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

            T cpBase = instGo.GetComponent<T>();
            if (cpBase == null) return null;

            cpBase.SetNames(assetName);
            cpBase.OnInit();
            cpBase.InitFirst();
            // 預製體如果製作時, 本身主體 Active 為 true 才調用 Display => OnShow
            if (active) cpBase.Display(null);

            // 最後還原本身預製體的 Active
            instGo.SetActive(active);

            return cpBase;
        }

        public T LoadWithClone<T>(string packageName, string assetName, Transform parent = null, Progression progression = null) where T : CPBase, new()
        {
            GameObject go = this.LoadGameObject(packageName, assetName, progression);
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

            T cpBase = instGo.GetComponent<T>();
            if (cpBase == null) return null;

            cpBase.SetNames(assetName);
            cpBase.OnInit();
            cpBase.InitFirst();
            // 預製體如果製作時, 本身主體 Active 為 true 才調用 Display => OnShow
            if (active) cpBase.Display(null);

            // 最後還原本身預製體的 Active
            instGo.SetActive(active);

            return cpBase;
        }

        public T LoadWithClone<T>(string packageName, string assetName, Transform parent, bool worldPositionStays, Progression progression = null) where T : CPBase, new()
        {
            GameObject go = this.LoadGameObject(packageName, assetName, progression);
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

            T cpBase = instGo.GetComponent<T>();
            if (cpBase == null) return null;

            cpBase.SetNames(assetName);
            cpBase.OnInit();
            cpBase.InitFirst();
            // 預製體如果製作時, 本身主體 Active 為 true 才調用 Display => OnShow
            if (active) cpBase.Display(null);

            // 最後還原本身預製體的 Active
            instGo.SetActive(active);

            return cpBase;
        }

        public T LoadWithClone<T>(string packageName, string assetName, Vector3 position, Quaternion rotation, Transform parent = null, Vector3? scale = null, Progression progression = null) where T : CPBase, new()
        {
            GameObject go = this.LoadGameObject(packageName, assetName, progression);
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

            T cpBase = instGo.GetComponent<T>();
            if (cpBase == null) return null;

            cpBase.SetNames(assetName);
            cpBase.OnInit();
            cpBase.InitFirst();
            // 預製體如果製作時, 本身主體 Active 為 true 才調用 Display => OnShow
            if (active) cpBase.Display(null);

            // 最後還原本身預製體的 Active
            instGo.SetActive(active);

            return cpBase;
        }
    }
}
