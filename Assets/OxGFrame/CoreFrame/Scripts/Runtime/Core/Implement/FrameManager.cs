using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader;
using OxGFrame.AssetLoader.Cacher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace OxGFrame.CoreFrame
{
    [DisallowMultipleComponent]
    public abstract class FrameManager<T> : MonoBehaviour where T : FrameBase
    {
        protected delegate void AddIntoCache(T fBase);

        protected class FrameStack<U> where U : T
        {
            public string assetName { get; private set; }
            public Stack<U> cache { get; private set; }

            public FrameStack(string assetName)
            {
                this.cache = new Stack<U>();
                this.assetName = assetName;
            }

            public int Count()
            {
                return this.cache.Count;
            }

            public void Push(U fBase)
            {
                this.cache.Push(fBase);
            }

            public U Peek()
            {
                return this.cache.Peek();
            }

            public U Pop()
            {
                return this.cache.Pop();
            }

            ~FrameStack()
            {
                this.cache = null;
            }
        }

        public float reqSize { get; protected set; }
        public float totalSize { get; protected set; }

        protected Dictionary<string, FrameStack<T>> _dictAllCache = new Dictionary<string, FrameStack<T>>();  // 【常駐】所有快取 (只會在Destroy時, Remove對應的快取)
        protected HashSet<string> _loadingFlags = new HashSet<string>();                                      // 用來標記正在加載中的資源 (暫存快取)

        ~FrameManager()
        {
            this._dictAllCache = null;
            this._loadingFlags = null;
        }

        /// <summary>
        /// 判斷是否有在LoadingFlags中
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public bool HasInLoadingFlags(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return false;
            return this._loadingFlags.Contains(assetName);
        }

        /// <summary>
        /// 判斷是否有在AllCache中
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public bool HasInAllCache(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return false;
            return this._dictAllCache.ContainsKey(assetName);
        }

        /// <summary>
        /// 從AllCache中取得FrameBase as T
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        protected T GetFromAllCache(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return null;

            FrameStack<T> stack = this.GetStackFromAllCache(assetName);
            return stack?.Peek();
        }

        protected FrameStack<T> GetStackFromAllCache(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return null;

            FrameStack<T> stack = null;
            if (this.HasInAllCache(assetName)) stack = this._dictAllCache[assetName];
            return stack;
        }

        /// <summary>
        /// 取得組件
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public U GetFrameComponent<U>(string assetName) where U : T
        {
            return (U)this.GetFromAllCache(assetName);
        }

        /// <summary>
        /// 取得組件
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public U[] GetFrameComponents<U>(string assetName) where U : T
        {
            var stack = this.GetStackFromAllCache(assetName);
            if (stack != null) return (U[])this.GetStackFromAllCache(assetName).cache.ToArray();
            return new U[] { };
        }

        /// <summary>
        /// 檢查是否開啟
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public bool CheckIsShowing(string assetName)
        {
            T fBase = this.GetFromAllCache(assetName);
            if (fBase == null) return false;
            return fBase.gameObject.activeSelf;
        }

        /// <summary>
        /// 檢查是否開啟
        /// </summary>
        /// <returns></returns>
        public bool CheckIsShowing(T fBase)
        {
            if (fBase == null) return false;
            return fBase.gameObject.activeSelf;
        }

        /// <summary>
        /// 檢查是否隱藏
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public bool CheckIsHiding(string assetName)
        {
            T fBase = this.GetFromAllCache(assetName);
            if (fBase == null) return false;
            return fBase.isHidden;
        }

        /// <summary>
        /// 檢查是否隱藏
        /// </summary>
        /// <param name="fBase"></param>
        /// <returns></returns>
        public bool CheckIsHiding(T fBase)
        {
            if (fBase == null) return false;
            return fBase.isHidden;
        }

        /// <summary>
        /// 判斷 id 是否有任一隱藏
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public bool CheckHasAnyHiding(int groupId)
        {
            foreach (var fBase in this._dictAllCache.Values.ToArray())
            {
                if (fBase.Peek().groupId == groupId && fBase.Peek().isHidden) return true;
            }

            return false;
        }

        /// <summary>
        /// 判斷是否有任一隱藏
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool CheckHasAnyHiding()
        {
            foreach (var fBase in this._dictAllCache.Values.ToArray())
            {
                if (fBase.Peek().isHidden) return true;
            }

            return false;
        }

        /// <summary>
        /// 載入GameObject (Resource)
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        protected virtual async UniTask<GameObject> LoadGameObject(string assetName, Progression progression)
        {
            if (string.IsNullOrEmpty(assetName)) return null;

            GameObject obj = await CacheResource.GetInstance().Load<GameObject>(assetName, progression);
            if (obj == null)
            {
                Debug.LogWarning(string.Format("【 path: {0} 】此路徑找不到所屬資源!!!", assetName));
                return null;
            }

            return obj;
        }

        /// <summary>
        /// 載入GameObject (AssetBundle)
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        protected virtual async UniTask<GameObject> LoadGameObject(string bundleName, string assetName, Progression progression)
        {
            if (string.IsNullOrEmpty(bundleName) || string.IsNullOrEmpty(assetName)) return null;

            GameObject obj = await CacheBundle.GetInstance().Load<GameObject>(bundleName, assetName, true, progression);
            if (obj == null)
            {
                Debug.LogWarning(string.Format("【 ab: {0}, asset: {1} 】此路徑找不到所屬資源!!!", bundleName, assetName));
                return null;
            }

            return obj;
        }

        /// <summary>
        /// 取得載入的G ameObject 後, 開始實例進行 FrameBase 組件的系列處理
        /// </summary>
        /// <param name="fBase"></param>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        protected abstract T Instantiate(T fBase, string bundleName, string assetName, AddIntoCache addIntoCache);

        protected async UniTask<T> LoadIntoAllCache(string bundleName, string assetName, Progression progression, bool isPreloadMode)
        {
            if (this.HasInLoadingFlags(assetName)) return null;

            // 標記LoadingFlag
            this._loadingFlags.Add(assetName);

            // 開始加載
            var go = (string.IsNullOrEmpty(bundleName)) ?
                await LoadGameObject(assetName, progression) :
                await LoadGameObject(bundleName, assetName, progression);

            if (go == null)
            {
                // 移除LoadingFlag
                this._loadingFlags.Remove(assetName);
                return null;
            }

            T fBase = go.GetComponent<T>();
            if (fBase == null)
            {
                // 移除LoadingFlag
                this._loadingFlags.Remove(assetName);
                return null;
            }

            // 如果允許實例 + 預加載模式則不會先進行快取操作
            if (fBase.allowInstantiate && isPreloadMode)
            {
                Debug.Log($"<color=#FF9149>{fBase.name} => 【允許多實例 + 預加載模式】不先行處理快取</color>");
                return null;
            }

            // 判斷不在 AllCache 中, 才進行加載程序                                  
            if (!this.HasInAllCache(assetName))
            {
                // 實例
                fBase = this.Instantiate(fBase, bundleName, assetName, (fBase) =>
                {
                    FrameStack<T> stack = new FrameStack<T>(assetName);
                    stack.Push(fBase);
                    this._dictAllCache.Add(assetName, stack);
                });

                // 移除LoadingFlag
                this._loadingFlags.Remove(assetName);
            }
            else
            {
                FrameStack<T> stack = this.GetStackFromAllCache(assetName);
                if (fBase.allowInstantiate)
                {
                    // 實例
                    fBase = this.Instantiate(fBase, bundleName, assetName, (fBase) => { stack.Push(fBase); });
                }
                else fBase = this.GetFromAllCache(assetName);

                // 移除LoadingFlag
                this._loadingFlags.Remove(assetName);
            }

            return fBase;
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
                await this.LoadIntoAllCache(string.Empty, assetName, progression, true);
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
                await this.LoadIntoAllCache(bundleName, assetName, progression, true);
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
                this.totalSize = await CacheResource.GetInstance().GetAssetsLength(assetNames);

                for (int i = 0; i < assetNames.Length; i++)
                {
                    if (string.IsNullOrEmpty(assetNames[i]))
                    {
                        continue;
                    }

                    float lastSize = 0;
                    await this.LoadIntoAllCache(string.Empty, assetNames[i], (float progress, float reqSize, float totalSize) =>
                    {
                        this.reqSize += reqSize - lastSize;
                        lastSize = reqSize;

                        progression?.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
                    }, true);
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
                this.totalSize = await CacheBundle.GetInstance().GetAssetsLength(bundleNames.ToArray());

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
                    await this.LoadIntoAllCache(bundleAssetNames[row, 0], bundleAssetNames[row, 1], (float progress, float reqSize, float totalSize) =>
                    {
                        this.reqSize += reqSize - lastSize;
                        lastSize = reqSize;

                        progression?.Invoke(this.reqSize / this.totalSize, this.reqSize, this.totalSize);
                    }, true);
                }
            }

            // 等待執行完畢
            await UniTask.Yield();
        }

        /// <summary>
        /// 依照對應的Node類型設置母節點 (子類實作)
        /// </summary>
        /// <param name="fBase"></param>
        protected virtual bool SetParent(T fBase) { return true; }

        /// <summary>
        /// 開啟預顯加載UI
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        protected async virtual UniTask ShowLoading(int groupId, string bundleName, string assetName)
        {
            if (string.IsNullOrEmpty(bundleName)) await UIFrame.UIManager.GetInstance().Show(groupId, assetName);
            else await UIFrame.UIManager.GetInstance().Show(groupId, bundleName, assetName);
        }

        /// <summary>
        /// 關閉預顯加載UI
        /// </summary>
        protected void CloseLoading(string assetName)
        {
            if (!string.IsNullOrEmpty(assetName)) UIFrame.UIManager.GetInstance().Close(assetName, true);
        }

        #region Show
        /// <summary>
        /// 【Resource】顯示, 可透過 id 進行群組分類
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="assetName"></param>
        /// <param name="obj"></param>
        /// <param name="loadingUIAssetName"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        public async virtual UniTask<T> Show(int groupId, string assetName, object obj = null, string loadingUIAssetName = null, Progression progression = null)
        {
            await this.ShowLoading(groupId, string.Empty, loadingUIAssetName);

            return default;
        }

        /// <summary>
        /// 【Bundle】顯示, 可透過 id 進行群組分類
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <param name="obj"></param>
        /// <param name="loadingUIBundleName"></param>
        /// <param name="loadingUIAssetName"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        public async virtual UniTask<T> Show(int groupId, string bundleName, string assetName, object obj = null, string loadingUIBundleName = null, string loadingUIAssetName = null, Progression progression = null)
        {
            await this.ShowLoading(groupId, loadingUIBundleName, loadingUIAssetName);

            return default;
        }
        #endregion

        #region Close
        /// <summary>
        /// 單個關閉
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="disableDoSub"></param>
        /// <param name="withDestroy"></param>
        public virtual void Close(string assetName, bool disableDoSub = false, bool withDestroy = false) { }

        /// <summary>
        /// 全部關閉
        /// </summary>
        /// <param name="disableDoSub"></param>
        /// <param name="withDestroy"></param>
        /// <param name="withoutAssetNames"></param>
        public virtual void CloseAll(bool disableDoSub = false, bool withDestroy = false, params string[] withoutAssetNames) { }

        /// <summary>
        /// 透過 id 群組進行全部關閉
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="disableDoSub"></param>
        /// <param name="withDestroy"></param>
        /// <param name="withoutAssetNames"></param>
        public virtual void CloseAll(int groupId, bool disableDoSub = false, bool withDestroy = false, params string[] withoutAssetNames) { }
        #endregion

        #region Reveal
        /// <summary>
        /// 單個解除隱藏 (只允許 Hide, 如果透過 Close 則無法進行 Reveal)
        /// </summary>
        /// <param name="assetName"></param>
        public virtual void Reveal(string assetName) { }

        /// <summary>
        /// 全部解除隱藏 (只允許 Hide, 如果透過 Close 則無法進行 Reveal)
        /// </summary>
        public virtual void RevealAll() { }

        /// <summary>
        /// 透過 id 群組進行全部解除隱藏 (只允許 Hide, 如果透過 Close 則無法進行 Reveal)
        /// </summary>
        /// <param name="groupId"></param>
        public virtual void RevealAll(int groupId) { }
        #endregion

        #region Hide
        /// <summary>
        /// 單個隱藏 (可透過 Show 或者 Reveal 進行顯示, 差別在於初始行為)
        /// </summary>
        /// <param name="assetName"></param>
        public virtual void Hide(string assetName) { }

        /// <summary>
        /// 全部隱藏 (可透過 Show 或者 Reveal 進行顯示, 差別在於初始行為)
        /// </summary>
        public virtual void HideAll(params string[] withoutAssetNames) { }

        /// <summary>
        /// 透過 id 群組進行全部隱藏 (可透過 Show 或者 Reveal 進行顯示, 差別在於初始行為)
        /// </summary>
        /// <param name="groupId"></param>
        public virtual void HideAll(int groupId, params string[] withoutAssetNames) { }
        #endregion

        #region Load Display & Exit Hide
        protected virtual async UniTask LoadAndDispay(T fBase, object obj = null) { }

        protected virtual void ExitAndHide(T fBase, bool disableDoSub = false) { }
        #endregion

        #region Destroy
        /// <summary>
        /// 刪除FrameBase
        /// </summary>
        /// <param name="fBase"></param>
        /// <param name="assetName"></param>
        protected virtual void Destroy(T fBase, string assetName)
        {
            fBase.OnRelease(); // 執行FrameBase相關釋放程序

            if (string.IsNullOrEmpty(fBase.bundleName)) CacheResource.GetInstance().ReleaseFromCache(fBase.assetName);
            else CacheBundle.GetInstance().ReleaseFromCache(fBase.bundleName);

            if (!fBase.gameObject.IsDestroyed()) Destroy(fBase.gameObject); // 刪除FrameBase物件

            FrameStack<T> stack = this.GetStackFromAllCache(assetName);
            stack.Pop();
            if (stack.Count() == 0) this._dictAllCache.Remove(assetName);  // 刪除FrameBase快取

            Debug.Log(string.Format("Destroy Cache: {0}", assetName));
        }
        #endregion

        /// <summary>
        /// 【特殊方法】交由Protocol委託Handle (主要用於Server傳送封包給Client後, 進行一次性刷新)
        /// </summary>
        /// <param name="funcId"></param>
        public void UpdateByProtocol(int funcId)
        {
            foreach (FrameStack<T> stack in this._dictAllCache.Values.ToArray())
            {
                if (this.CheckIsShowing(stack.Peek()) || this.CheckIsHiding(stack.Peek()))
                {
                    foreach (var fBase in stack.cache.ToArray())
                    {
                        fBase.OnUpdateOnceAfterProtocol(funcId);
                    }
                }
            }
        }
    }

    public static class ComponentExtensions
    {
        public static T GetCopyOf<T>(this T comp, T other) where T : Component
        {
            Type type = comp.GetType();
            Type othersType = other.GetType();
            if (type != othersType)
            {
                Debug.LogError($"The type \"{type.AssemblyQualifiedName}\" of \"{comp}\" does not match the type \"{othersType.AssemblyQualifiedName}\" of \"{other}\"!");
                return null;
            }

            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default;
            PropertyInfo[] pInfos = type.GetProperties(flags);

            foreach (var pInfo in pInfos)
            {
                if (pInfo.CanWrite)
                {
                    try
                    {
                        if (pInfo.Name == "name") continue;
                        pInfo.SetValue(comp, pInfo.GetValue(other, null), null);
                    }
                    catch
                    {
                        /*
                         * In case of NotImplementedException being thrown.
                         * For some reason specifying that exception didn't seem to catch it,
                         * so I didn't catch anything specific.
                         */
                    }
                }
            }

            FieldInfo[] fInfos = type.GetFields(flags);

            foreach (var fInfo in fInfos)
            {
                fInfo.SetValue(comp, fInfo.GetValue(other));
            }
            return comp as T;
        }
    }

    public static class GameObjectExtensions
    {
        /// <summary>
        /// Checks if a GameObject has been destroyed.
        /// </summary>
        /// <param name="gameObject">GameObject reference to check for destructedness</param>
        /// <returns>If the game object has been marked as destroyed by UnityEngine</returns>
        public static bool IsDestroyed(this GameObject gameObject)
        {
            // UnityEngine overloads the == opeator for the GameObject type
            // and returns null when the object has been destroyed, but 
            // actually the object is still there but has not been cleaned up yet
            // if we test both we can determine if the object has been destroyed.
            return gameObject == null && !ReferenceEquals(gameObject, null);
        }
    }
}
