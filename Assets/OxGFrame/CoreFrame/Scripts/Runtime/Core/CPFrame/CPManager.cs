using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader;
using OxGKit.LoggingSystem;
using UnityEngine;

namespace OxGFrame.CoreFrame.CPFrame
{
    internal class CPManager
    {
        /// <summary>
        /// [計算進度條用] 加載數量
        /// </summary>
        public float currentCount { get; protected set; }

        /// <summary>
        ///  [計算進度條用] 總加載數量
        /// </summary>
        public float totalCount { get; protected set; }

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

        protected async UniTask<GameObject> LoadGameObjectAsync(string packageName, string assetName, uint priority, Progression progression)
        {
            if (string.IsNullOrEmpty(assetName)) return null;

            GameObject obj = await AssetLoaders.LoadAssetAsync<GameObject>(packageName, assetName, priority, progression);

            if (obj == null)
            {
                Logging.PrintWarning<Logger>(string.Format("【 path: {0} 】asset not found at this path!!!", assetName));
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
                Logging.PrintWarning<Logger>(string.Format("【 path: {0} 】asset not found at this path!!!", assetName));
                return null;
            }

            return obj;
        }

        public async UniTask PreloadAsync(string packageName, string[] assetNames, uint priority = 0, Progression progression = null)
        {
            await AssetLoaders.PreloadAssetAsync<Object>(packageName, assetNames, priority, progression);
        }

        public async UniTask PreloadAsync<T>(string packageName, string[] assetNames, uint priority = 0, Progression progression = null) where T : Object
        {
            await AssetLoaders.PreloadAssetAsync<T>(packageName, assetNames, priority, progression);
        }

        public void Preload(string packageName, string[] assetNames, Progression progression = null)
        {
            AssetLoaders.PreloadAsset<Object>(packageName, assetNames, progression);
        }

        public void Preload<T>(string packageName, string[] assetNames, Progression progression = null) where T : Object
        {
            AssetLoaders.PreloadAsset<T>(packageName, assetNames, progression);
        }

        public async UniTask<T> LoadWithCloneAsync<T>(string packageName, string assetName, Transform parent = null, uint priority = 0, Progression progression = null) where T : CPBase, new()
        {
            return await this._LoadWithCloneAsync<T>(packageName, assetName, parent, priority, progression, null, null);
        }

        public async UniTask<T> LoadWithCloneAsync<T>(string packageName, string assetName, Transform parent, bool worldPositionStays, uint priority = 0, Progression progression = null) where T : CPBase, new()
        {
            return await this._LoadWithCloneAsync<T>(packageName, assetName, parent, priority, progression, worldPositionStays, null);
        }

        public async UniTask<T> LoadWithCloneAsync<T>(string packageName, string assetName, Vector3 position, Quaternion rotation, Transform parent = null, Vector3? scale = null, uint priority = 0, Progression progression = null) where T : CPBase, new()
        {
            return await this._LoadWithCloneAsync<T>(packageName, assetName, parent, priority, progression, null, scale, position, rotation);
        }

        private async UniTask<T> _LoadWithCloneAsync<T>(string packageName, string assetName, Transform parent, uint priority, Progression progression, bool? worldPositionStays, Vector3? scale = null, Vector3? position = null, Quaternion? rotation = null) where T : CPBase, new()
        {
            GameObject go = await this.LoadGameObjectAsync(packageName, assetName, priority, progression);
            if (go == null)
                return null;

            GameObject instGo;
            if (position.HasValue && rotation.HasValue)
            {
                instGo = GameObject.Instantiate(go, position.Value, rotation.Value, parent);
            }
            else if (worldPositionStays.HasValue)
            {
                instGo = GameObject.Instantiate(go, parent, worldPositionStays.Value);
            }
            else
            {
                instGo = GameObject.Instantiate(go, parent);
            }

            if (scale.HasValue)
            {
                instGo.transform.localScale = scale.Value;
            }

            T cpBase = instGo.GetComponent<T>();
            if (cpBase == null)
                return null;

            // 激活檢查, 如果主體 Active 為 false 必須打開
            bool active = instGo.activeSelf;
            if (!active)
            {
                cpBase.isMonoDriveDetected = cpBase.monoDrive;
                instGo.SetActive(true);
            }

            cpBase.SetNames(assetName);
            if (!cpBase.monoDrive)
            {
                cpBase.OnCreate();
                cpBase.InitFirst();

                // 預製體如果製作時, 本身主體 Active 為 true 才調用 Display => OnShow
                if (active)
                    cpBase.Display(null);
            }

            // 最後還原本身預製體的 Active
            instGo.SetActive(active);
            cpBase.isMonoDriveDetected = false;

            return cpBase;
        }

        public T LoadWithClone<T>(string packageName, string assetName, Transform parent = null, Progression progression = null) where T : CPBase, new()
        {
            return this._LoadWithClone<T>(packageName, assetName, parent, progression, null, null);
        }

        public T LoadWithClone<T>(string packageName, string assetName, Transform parent, bool worldPositionStays, Progression progression = null) where T : CPBase, new()
        {
            return this._LoadWithClone<T>(packageName, assetName, parent, progression, worldPositionStays, null);
        }

        public T LoadWithClone<T>(string packageName, string assetName, Vector3 position, Quaternion rotation, Transform parent = null, Vector3? scale = null, Progression progression = null) where T : CPBase, new()
        {
            return this._LoadWithClone<T>(packageName, assetName, parent, progression, null, scale, position, rotation);
        }

        private T _LoadWithClone<T>(string packageName, string assetName, Transform parent, Progression progression, bool? worldPositionStays = null, Vector3? scale = null, Vector3? position = null, Quaternion? rotation = null) where T : CPBase, new()
        {
            GameObject go = this.LoadGameObject(packageName, assetName, progression);
            if (go == null)
                return null;

            GameObject instGo;
            if (position.HasValue && rotation.HasValue)
            {
                instGo = GameObject.Instantiate(go, position.Value, rotation.Value, parent);
            }
            else if (worldPositionStays.HasValue)
            {
                instGo = GameObject.Instantiate(go, parent, worldPositionStays.Value);
            }
            else
            {
                instGo = GameObject.Instantiate(go, parent);
            }

            if (scale.HasValue)
            {
                instGo.transform.localScale = scale.Value;
            }

            T cpBase = instGo.GetComponent<T>();
            if (cpBase == null)
                return null;

            // 激活檢查, 如果主體 Active 為 false 必須打開
            bool active = instGo.activeSelf;
            if (!active)
            {
                cpBase.isMonoDriveDetected = cpBase.monoDrive;
                instGo.SetActive(true);
            }

            cpBase.SetNames(assetName);
            if (!cpBase.monoDrive)
            {
                cpBase.OnCreate();
                cpBase.InitFirst();

                // 預製體如果製作時, 本身主體 Active 為 true 才調用 Display => OnShow
                if (active)
                    cpBase.Display(null);
            }

            // 最後還原本身預製體的 Active
            instGo.SetActive(active);
            cpBase.isMonoDriveDetected = false;

            return cpBase;
        }
    }
}
