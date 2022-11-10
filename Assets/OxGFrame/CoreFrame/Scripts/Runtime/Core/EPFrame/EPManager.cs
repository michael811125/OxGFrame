using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader;
using OxGFrame.AssetLoader.Cacher;
using System.Collections.Generic;
using UnityEngine;

namespace OxGFrame.CoreFrame.EPFrame
{
    public class EPManager : MonoBehaviour, IEPManager
    {
        public float reqSize { get; protected set; }   // [計算進度條用] 加載數量
        public float totalSize { get; protected set; } // [計算進度條用] 總加載數量

        private GameObject _goRoot = null;             // 根節點物件

        private static readonly object _locker = new object();
        private static EPManager _instance = null;
        public static EPManager GetInstance()
        {
            if (_instance == null)
            {
                lock (_locker)
                {
                    _instance = FindObjectOfType(typeof(EPManager)) as EPManager;
                    if (_instance == null) _instance = new GameObject(EPSysDefine.EP_MANAGER_NAME).AddComponent<EPManager>();
                }
            }
            return _instance;
        }

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        /// <summary>
        /// 建立並且檢查是否有根節點, 如果無則建立
        /// </summary>
        private void _CreateAndCheckSpwaner()
        {
            if (this._goRoot == null || this._goRoot.IsDestroyed()) this._goRoot = new GameObject(EPSysDefine.EP_SPWANER_NAME);
        }

        /// <summary>
        /// 載入 GameObject (Resource)
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        protected virtual async UniTask<GameObject> LoadGameObject(string assetName, Progression progression)
        {
            if (string.IsNullOrEmpty(assetName)) return null;

            this._CreateAndCheckSpwaner();
            GameObject obj = await CacheResource.GetInstance().Load<GameObject>(assetName, progression);
            if (obj == null)
            {
                Debug.LogWarning(string.Format("【 path: {0} 】此路徑找不到所屬資源!!!", assetName));
                return null;
            }

            return obj;
        }

        /// <summary>
        /// 載入 GameObject (AssetBundle)
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        protected virtual async UniTask<GameObject> LoadGameObject(string bundleName, string assetName, Progression progression)
        {
            if (string.IsNullOrEmpty(bundleName) || string.IsNullOrEmpty(assetName)) return null;

            this._CreateAndCheckSpwaner();
            GameObject obj = await CacheBundle.GetInstance().Load<GameObject>(bundleName, assetName, true, progression);
            if (obj == null)
            {
                Debug.LogWarning(string.Format("【 ab: {0}, asset: {1} 】此路徑找不到所屬資源!!!", bundleName, assetName));
                return null;
            }

            return obj;
        }

        /// <summary>
        /// 【Resouces】單個預加載
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public async UniTask Preload(string assetName, Progression progression = null)
        {
            if (!string.IsNullOrEmpty(assetName))
            {
                await CacheResource.GetInstance().Preload(assetName, progression);
            }

            // 等待執行完畢
            await UniTask.Yield();
        }

        /// <summary>
        /// 【Bundle】單個預加載
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public async UniTask Preload(string bundleName, string assetName, Progression progression = null)
        {
            if (!string.IsNullOrEmpty(bundleName) && !string.IsNullOrEmpty(assetName))
            {
                await CacheBundle.GetInstance().Preload(bundleName, progression);
            }

            // 等待執行完畢
            await UniTask.Yield();
        }

        /// <summary>
        /// 【Resouces】一次多個預加載
        /// </summary>
        /// <param name="assetNames"></param>
        /// <returns></returns>
        public async UniTask Preload(string[] assetNames, Progression progression = null)
        {
            if (assetNames.Length > 0)
            {
                this.reqSize = 0;
                this.totalSize = CacheResource.GetInstance().GetAssetsLength(assetNames);

                for (int i = 0; i < assetNames.Length; i++)
                {
                    if (string.IsNullOrEmpty(assetNames[i]))
                    {
                        continue;
                    }

                    float lastSize = 0;
                    await CacheResource.GetInstance().Preload(assetNames[i], (float progress, float reqSize, float totalSize) =>
                    {
                        this.reqSize += reqSize - lastSize;
                        lastSize = reqSize;

                        progression?.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
                    });
                }
            }

            // 等待執行完畢
            await UniTask.Yield();
        }

        /// <summary>
        /// 【Bundle】一次多個預加載
        /// </summary>
        /// <param name="bundleAssetNames"></param>
        /// <returns></returns>
        public async UniTask Preload(string[,] bundleAssetNames, Progression progression = null)
        {
            if (bundleAssetNames.Length > 0)
            {
                List<string> bundleNames = new List<string>();
                for (int row = 0; row < bundleAssetNames.GetLength(0); row++)
                {
                    if (bundleAssetNames.GetLength(1) != 2)
                    {
                        continue;
                    }
                    else if (string.IsNullOrEmpty(bundleAssetNames[row, 0]) || string.IsNullOrEmpty(bundleAssetNames[row, 1]))
                    {
                        continue;
                    }

                    bundleNames.Add(bundleAssetNames[row, 0]);
                }
                this.reqSize = 0;
                this.totalSize = CacheBundle.GetInstance().GetAssetsLength(bundleNames.ToArray());

                for (int row = 0; row < bundleAssetNames.GetLength(0); row++)
                {
                    if (bundleAssetNames.GetLength(1) != 2)
                    {
                        continue;
                    }
                    else if (string.IsNullOrEmpty(bundleAssetNames[row, 0]) || string.IsNullOrEmpty(bundleAssetNames[row, 1]))
                    {
                        continue;
                    }

                    float lastSize = 0;
                    await CacheBundle.GetInstance().Preload(bundleAssetNames[row, 0], (float progress, float reqSize, float totalSize) =>
                    {
                        this.reqSize += reqSize - lastSize;
                        lastSize = reqSize;

                        progression?.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
                    });
                }
            }

            // 等待執行完畢
            await UniTask.Yield();
        }

        public async UniTask<T> LoadWithClone<T>(string assetName, Transform parent = null, Progression progression = null) where T : EPBase, new()
        {
            GameObject go = await this.LoadGameObject(assetName, progression);
            if (go == null) return null;

            GameObject instGo = Instantiate(go, (parent == null) ? this._goRoot.transform : parent);

            // 激活檢查, 如果主體 Active 為 false 必須打開
            bool active;
            if (!instGo.activeSelf)
            {
                active = instGo.activeSelf;
                instGo.SetActive(true);
            }
            else active = instGo.activeSelf;

            instGo.transform.localPosition = Vector3.zero;
            instGo.transform.localRotation = Quaternion.identity;

            T entityBase = instGo.GetComponent<T>();
            if (entityBase == null) return null;

            entityBase.SetNames(string.Empty, assetName);
            entityBase.BeginInit();
            entityBase.InitFirst();
            if (active) entityBase.Display(null); // 預製體如果製作時, 本身主體 Active 為 true 才調用 Display => OnShow

            // 最後還原本身預製體的 Active
            instGo.SetActive(active);

            return entityBase;
        }

        public async UniTask<T> LoadWithClone<T>(string assetName, Vector3 position, Quaternion rotation, Transform parent = null, Vector3? scale = null, Progression progression = null) where T : EPBase, new()
        {
            GameObject go = await this.LoadGameObject(assetName, progression);
            if (go == null) return null;

            GameObject instGo = Instantiate(go, (parent == null) ? this._goRoot.transform : parent);

            // 激活檢查, 如果主體 Active 為 false 必須打開
            bool active;
            if (!instGo.activeSelf)
            {
                active = instGo.activeSelf;
                instGo.SetActive(true);
            }
            else active = instGo.activeSelf;

            instGo.transform.localPosition = position;
            instGo.transform.localRotation = rotation;
            Vector3 localScale = (scale == null) ? instGo.transform.localScale : (Vector3)scale;
            instGo.transform.localScale = localScale;

            T entityBase = instGo.GetComponent<T>();
            if (entityBase == null) return null;

            entityBase.SetNames(string.Empty, assetName);
            entityBase.BeginInit();
            entityBase.InitFirst();
            if (active) entityBase.Display(null); // 預製體如果製作時, 本身主體 Active 為 true 才調用 Display => OnShow

            // 最後還原本身預製體的 Active
            instGo.SetActive(active);

            return entityBase;
        }

        public async UniTask<T> LoadWithClone<T>(string bundleName, string assetName, Transform parent = null, Progression progression = null) where T : EPBase, new()
        {
            GameObject go = await this.LoadGameObject(bundleName, assetName, progression);
            if (go == null) return null;

            GameObject instGo = Instantiate(go, (parent == null) ? this._goRoot.transform : parent);

            // 激活檢查, 如果主體 Active 為 false 必須打開
            bool active;
            if (!instGo.activeSelf)
            {
                active = instGo.activeSelf;
                instGo.SetActive(true);
            }
            else active = instGo.activeSelf;

            instGo.transform.localPosition = Vector3.zero;
            instGo.transform.localRotation = Quaternion.identity;

            T entityBase = instGo.GetComponent<T>();
            if (entityBase == null) return null;

            entityBase.SetNames(bundleName, assetName);
            entityBase.BeginInit();
            entityBase.InitFirst();
            if (active) entityBase.Display(null); // 預製體如果製作時, 本身主體 Active 為 true 才調用 Display => OnShow

            // 最後還原本身預製體的 Active
            instGo.SetActive(active);

            return entityBase;
        }

        public async UniTask<T> LoadWithClone<T>(string bundleName, string assetName, Vector3 position, Quaternion rotation, Transform parent = null, Vector3? scale = null, Progression progression = null) where T : EPBase, new()
        {
            GameObject go = await this.LoadGameObject(bundleName, assetName, progression);
            if (go == null) return null;

            GameObject instGo = Instantiate(go, (parent == null) ? this._goRoot.transform : parent);

            // 激活檢查, 如果主體 Active 為 false 必須打開
            bool active;
            if (!instGo.activeSelf)
            {
                active = instGo.activeSelf;
                instGo.SetActive(true);
            }
            else active = instGo.activeSelf;

            instGo.transform.localPosition = position;
            instGo.transform.localRotation = rotation;
            Vector3 localScale = (scale == null) ? instGo.transform.localScale : (Vector3)scale;
            instGo.transform.localScale = localScale;

            T entityBase = instGo.GetComponent<T>();
            if (entityBase == null) return null;

            entityBase.SetNames(bundleName, assetName);
            entityBase.BeginInit();
            entityBase.InitFirst();
            if (active) entityBase.Display(null); // 預製體如果製作時, 本身主體 Active 為 true 才調用 Display => OnShow

            // 最後還原本身預製體的 Active
            instGo.SetActive(active);

            return entityBase;
        }
    }
}
