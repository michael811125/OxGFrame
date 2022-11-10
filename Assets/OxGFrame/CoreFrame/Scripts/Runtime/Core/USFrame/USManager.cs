using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader;
using OxGFrame.AssetLoader.Bundle;
using OxGFrame.AssetLoader.Cacher;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OxGFrame.CoreFrame.USFrame
{
    public class USManager
    {
        public static int sceneCount { get { return SceneManager.sceneCount; } }

        public float reqSize { get; protected set; }

        public float totalSize { get; protected set; }

        private static USManager _instance = null;
        public static USManager GetInstance()
        {
            if (_instance == null) _instance = new USManager();
            return _instance;
        }

        public Scene GetSceneAt(int index)
        {
            return SceneManager.GetSceneAt(index);
        }

        public Scene GetSceneByName(string sceneName)
        {
            return SceneManager.GetSceneByName(sceneName);
        }

        public Scene GetSceneByBuildIndex(int buildIndex)
        {
            return SceneManager.GetSceneByBuildIndex(buildIndex);
        }

        public Scene[] GetAllScene(params string[] sceneNames)
        {
            List<Scene> scenes = new List<Scene>();
            if (sceneNames.Length > 0)
            {
                for (int i = 0; i < sceneCount; i++)
                {
                    foreach (string sceneName in sceneNames)
                    {
                        if (sceneName == this.GetSceneAt(i).name)
                        {
                            scenes.Add(this.GetSceneAt(i));
                            break;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < sceneCount; i++)
                {
                    scenes.Add(this.GetSceneAt(i));
                }
            }

            return scenes.ToArray();
        }

        public Scene[] GetAllScene(params int[] buildIndexes)
        {
            List<Scene> scenes = new List<Scene>();
            if (buildIndexes.Length > 0)
            {
                for (int i = 0; i < sceneCount; i++)
                {
                    foreach (int buildIndex in buildIndexes)
                    {
                        if (buildIndex == this.GetSceneAt(i).buildIndex)
                        {
                            scenes.Add(this.GetSceneAt(i));
                            break;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < sceneCount; i++)
                {
                    scenes.Add(this.GetSceneAt(i));
                }
            }

            return scenes.ToArray();
        }

        /// <summary>
        /// 【Bundle】預加載場景 Bundle (單個)
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        public async UniTask PreloadSceneBundle(string bundleName, Progression progression = null)
        {
            await CacheBundle.GetInstance().Preload(bundleName, progression);
        }

        /// <summary>
        /// 【Bundle】預先加載場景 Bundle (批次)
        /// </summary>
        /// <param name="bundleNames"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        public async UniTask PreloadSceneBundle(string[] bundleNames, Progression progression = null)
        {
            await CacheBundle.GetInstance().Preload(bundleNames, progression);
        }

        /// <summary>
        /// 【Bundle】從 Bundle 中加載場景
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="sceneName"></param>
        /// <param name="loadSceneMode"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        public async UniTask LoadFromBundle(string bundleName, string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Single, Progression progression = null)
        {
#if UNITY_EDITOR
            if (BundleConfig.assetDatabaseMode)
            {
                var scene = CacheBundle.GetInstance().LoadEditorAsset<UnityEditor.SceneAsset>(bundleName, sceneName);
                string scenePath = UnityEditor.AssetDatabase.GetAssetPath(scene);
                LoadSceneParameters loadSceneParameters = new LoadSceneParameters(loadSceneMode);
                UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(scenePath, loadSceneParameters);
                return;
            }
#endif

            // 初始加載進度
            this.reqSize = 0;
            this.totalSize = 1; // 初始 1 = 必有一場景
            float lastSize = 0;

            // 檢查是否有 bundle 在快取中
            var bundlePack = CacheBundle.GetInstance().GetFromCache(bundleName);
            if (bundlePack == null)
            {
                // 合併 Progression
                this.totalSize += CacheBundle.GetInstance().GetAssetsLength(bundleName);

                // 開始 bundle 預加載
                lastSize = 0;
                await this.PreloadSceneBundle(bundleName, (float progress, float reqSize, float totalSize) =>
                {
                    this.reqSize += reqSize - lastSize;
                    lastSize = reqSize;

                    progression?.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
                });

                // 預加載完成後, 再從快取取出
                bundlePack = CacheBundle.GetInstance().GetFromCache(bundleName);
            }

            // bundle 不為空後, 開始進行場景加載
            if (bundlePack != null)
            {
                var scene = this.GetSceneByName(sceneName);
                if (!string.IsNullOrEmpty(scene.name) && scene.isLoaded && loadSceneMode == LoadSceneMode.Single)
                {
                    Debug.LogWarning(string.Format("【US】{0}已經存在了!!!", sceneName));
                    return;
                }

                string[] scenePaths = bundlePack.assetBundle.GetAllScenePaths();
                foreach (string scenePath in scenePaths)
                {
                    // 只取出路徑中的檔案名稱 (不包含副檔名)
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                    if (sceneName == fileName)
                    {
                        var req = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
                        if (req != null)
                        {
                            req.allowSceneActivation = false;

                            while (!req.isDone)
                            {
                                if (progression != null)
                                {
                                    lastSize = 0;
                                    req.completed += (AsyncOperation ao) =>
                                    {
                                        this.reqSize += (ao.progress - lastSize);
                                        lastSize = ao.progress;

                                        progression.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
                                    };
                                }

                                if (req.progress >= 0.9f) req.allowSceneActivation = true;

                                await UniTask.Yield();
                            }
                        }

                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 【Build Settings】從 Build 設置中加載場景
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="loadSceneMode"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        public async UniTask LoadFromBuild(string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Single, Progression progression = null)
        {
            this.reqSize = 0;
            this.totalSize = 1; // 初始 1 = 必有一場景
            float lastSize = 0;

            var scene = this.GetSceneByName(sceneName);
            if (!string.IsNullOrEmpty(scene.name) && scene.isLoaded && loadSceneMode == LoadSceneMode.Single)
            {
                Debug.LogWarning(string.Format("【US】{0}已經存在了!!!", sceneName));
                return;
            }

            var req = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
            if (req != null)
            {
                req.allowSceneActivation = false;

                while (!req.isDone)
                {
                    if (progression != null)
                    {
                        lastSize = 0;
                        req.completed += (AsyncOperation ao) =>
                        {
                            this.reqSize += (ao.progress - lastSize);
                            lastSize = ao.progress;

                            progression.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
                        };
                    }

                    if (req.progress >= 0.9f) req.allowSceneActivation = true;

                    await UniTask.Yield();
                }
            }
        }

        public async UniTask LoadFromBuild(int buildIndex, LoadSceneMode loadSceneMode = LoadSceneMode.Single, Progression progression = null)
        {
            this.reqSize = 0;
            this.totalSize = 1; // 初始 1 = 必有一場景
            float lastSize = 0;

            var scene = this.GetSceneByBuildIndex(buildIndex);
            if (!string.IsNullOrEmpty(scene.name) && scene.isLoaded && loadSceneMode == LoadSceneMode.Single)
            {
                Debug.LogWarning(string.Format("【US】{0}已經存在了!!!", scene.name));
                return;
            }

            var req = SceneManager.LoadSceneAsync(buildIndex, loadSceneMode);
            if (req != null)
            {
                req.allowSceneActivation = false;

                while (!req.isDone)
                {
                    if (progression != null)
                    {
                        lastSize = 0;
                        req.completed += (AsyncOperation ao) =>
                        {
                            this.reqSize += (ao.progress - lastSize);
                            lastSize = ao.progress;

                            progression.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
                        };
                    }

                    if (req.progress >= 0.9f) req.allowSceneActivation = true;

                    await UniTask.Yield();
                }
            }
        }

        public void Unload(string sceneName)
        {
            if (sceneCount == 1)
            {
                Debug.LogWarning("Cannot unload last scene!!!");
                return;
            }

            var scene = this.GetSceneByName(sceneName);
            if (scene != null && scene.isLoaded) SceneManager.UnloadSceneAsync(sceneName);
        }

        public void Unload(int buildIndex)
        {
            if (sceneCount == 1)
            {
                Debug.LogWarning("Cannot unload last scene!!!");
                return;
            }

            var scene = this.GetSceneByBuildIndex(buildIndex);
            if (scene != null && scene.isLoaded) SceneManager.UnloadSceneAsync(buildIndex);
        }

        public void UnloadAll(params string[] sceneNames)
        {
            if (sceneCount == 1)
            {
                Debug.LogWarning("Cannot unload last scene!!!");
                return;
            }

            if (sceneNames.Length > 0)
            {
                for (int i = 0; i < sceneCount; i++)
                {
                    foreach (string sceneName in sceneNames)
                    {
                        if (sceneName == this.GetSceneAt(i).name)
                        {
                            SceneManager.UnloadSceneAsync(this.GetSceneAt(i));
                            break;
                        }
                    }
                }
            }
            else
            {
                // 僅保留最後一個場景 (count - 1)
                for (int i = 0; i < sceneCount - 1; i++)
                {
                    SceneManager.UnloadSceneAsync(this.GetSceneAt(i));
                }
            }
        }

        public void UnloadAll(params int[] buildIndexes)
        {
            if (sceneCount == 1)
            {
                Debug.LogWarning("Cannot unload last scene!!!");
                return;
            }

            if (buildIndexes.Length > 0)
            {
                for (int i = 0; i < sceneCount; i++)
                {
                    foreach (int buildIndex in buildIndexes)
                    {
                        if (buildIndex == this.GetSceneAt(i).buildIndex)
                        {
                            SceneManager.UnloadSceneAsync(this.GetSceneAt(i));
                            break;
                        }
                    }
                }
            }
            else
            {
                // 僅保留最後一個場景 (count - 1)
                for (int i = 0; i < sceneCount - 1; i++)
                {
                    SceneManager.UnloadSceneAsync(this.GetSceneAt(i));
                }
            }
        }
    }
}