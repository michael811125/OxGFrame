﻿using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader;
using OxGKit.LoggingSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace OxGFrame.CoreFrame
{
    public struct RefreshInfo
    {
        public string assetName;
        public object data;

        public RefreshInfo(string assetName, object data)
        {
            this.assetName = assetName;
            this.data = data;
        }
    }

    [DisallowMultipleComponent]
    internal abstract class FrameManager<T> : MonoBehaviour where T : FrameBase
    {
        internal enum UpdateType
        {
            Update,
            FixedUpdate,
            LateUpdate
        }

        protected delegate void AddIntoCache(T fBase);

        protected class FrameStack<U> where U : T
        {
            public bool isPreloadMode { get; private set; }
            public bool allowInstantiate { get; private set; }
            public string assetName { get; private set; }
            public Stack<U> cache { get; private set; }

            public FrameStack()
            {
                this.cache = new Stack<U>();
            }

            public FrameStack(string assetName)
            {
                this.cache = new Stack<U>();
                this.assetName = assetName;
            }

            public FrameStack(string assetName, bool allowInstantiate)
            {
                this.cache = new Stack<U>();
                this.assetName = assetName;
                this.allowInstantiate = allowInstantiate;
            }

            public FrameStack(string assetName, bool allowInstantiate, bool isPreloadMode)
            {
                this.cache = new Stack<U>();
                this.assetName = assetName;
                this.allowInstantiate = allowInstantiate;
                this.isPreloadMode = isPreloadMode;
            }

            public void SetAssetName(string assetName)
            {
                this.assetName = assetName;
            }

            public void SetAllowInstantiate(bool allowInstantiate)
            {
                this.allowInstantiate = allowInstantiate;
            }

            public void SetIsPreloadMode(bool isPreloadMode)
            {
                this.isPreloadMode = isPreloadMode;
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
                this.cache.TryPeek(out U result);
                return result;
            }

            public U Pop()
            {
                this.cache.TryPop(out U result);
                return result;
            }

            ~FrameStack()
            {
                this.cache = null;
                this.assetName = null;
            }
        }

        public bool ignoreTimeScale = false;
        public bool enabledUpdate = true;
        public bool enabledFixedUpdate = false;
        public bool enabledLateUpdate = false;

        public float currentCount { get; protected set; }                                                     // [計算進度條用] 加載數量
        public float totalCount { get; protected set; }                                                       // [計算進度條用] 總加載數量

        protected Dictionary<string, FrameStack<T>> _dictAllCache = new Dictionary<string, FrameStack<T>>();  // 【常駐】所有緩存 (只會在 Destroy 時, Remove 對應的緩存)
        protected HashSet<string> _loadingFlags = new HashSet<string>();                                      // 用來標記正在加載中的資源 (暫存緩存)

        ~FrameManager()
        {
            this._dictAllCache = null;
            this._loadingFlags = null;
        }

        private static float _dt;
        private static float _fdt;

        private void Update()
        {
            if (!this.enabledUpdate) return;
            this.DriveUpdates(UpdateType.Update);
        }

        private void FixedUpdate()
        {
            if (!this.enabledFixedUpdate) return;
            this.DriveUpdates(UpdateType.FixedUpdate);
        }

        private void LateUpdate()
        {
            if (!this.enabledLateUpdate) return;
            this.DriveUpdates(UpdateType.LateUpdate);
        }

        public void DriveUpdates(UpdateType updateType)
        {
            if (this._dictAllCache.Count > 0)
            {
                var fStacks = this._dictAllCache.Values.ToArray();
                foreach (var fStack in fStacks)
                {
                    // 判斷陣列長度是否有改變, 有改變表示陣列元素有更動
                    if (this._dictAllCache.Count != fStacks.Length) break;

                    var fBases = fStack.cache.ToArray();
                    foreach (var fBase in fBases)
                    {
                        if (fStack.Count() != fBases.Length) break;

                        // 僅刷新激活的物件
                        if (this.CheckIsShowing(fBase))
                        {
                            switch (updateType)
                            {
                                case UpdateType.Update:
                                case UpdateType.LateUpdate:
                                    if (!this.ignoreTimeScale) _dt = Time.deltaTime;
                                    else _dt = Time.unscaledDeltaTime;
                                    break;
                                case UpdateType.FixedUpdate:
                                    if (!this.ignoreTimeScale) _fdt = Time.fixedDeltaTime;
                                    else _fdt = Time.fixedUnscaledDeltaTime;
                                    break;
                            }

                            switch (updateType)
                            {
                                case UpdateType.Update:
                                    fBase.HandleUpdate(_dt);
                                    break;
                                case UpdateType.FixedUpdate:
                                    fBase.HandleFixedUpdate(_fdt);
                                    break;
                                case UpdateType.LateUpdate:
                                    fBase.HandleLateUpdate(_dt);
                                    break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 判斷是否有在 LoadingFlags 中
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public bool HasInLoadingFlags(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return false;
            return this._loadingFlags.Contains(assetName);
        }

        /// <summary>
        /// 判斷是否有 Stack 在 AllCache 中
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public bool HasStackInAllCache(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return false;
            return this._dictAllCache.ContainsKey(assetName);
        }

        /// <summary>
        /// 從 AllCache 取得 Stack 並且 Peek 出 FrameBase as T
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        protected T PeekStackFromAllCache(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return null;

            FrameStack<T> stack = this.GetStackFromAllCache(assetName);
            return stack?.Peek();
        }

        /// <summary>
        /// 從 AllCache 取得 Stack
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        protected FrameStack<T> GetStackFromAllCache(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return null;

            FrameStack<T> stack = null;
            if (this.HasStackInAllCache(assetName)) stack = this._dictAllCache[assetName];
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
            return (U)this.PeekStackFromAllCache(assetName);
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
            T fBase = this.PeekStackFromAllCache(assetName);
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
            T fBase = this.PeekStackFromAllCache(assetName);
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
            foreach (var fBase in this._dictAllCache.Values)
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
            foreach (var fBase in this._dictAllCache.Values)
            {
                if (fBase.Peek().isHidden) return true;
            }

            return false;
        }

        /// <summary>
        /// 載入 GameObject
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="assetName"></param>
        /// <param name="priority"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        protected virtual async UniTask<GameObject> LoadGameObject(string packageName, string assetName, uint priority, Progression progression)
        {
            GameObject obj = await AssetLoaders.LoadAssetAsync<GameObject>(packageName, assetName, priority, progression);
            return obj;
        }

        /// <summary>
        /// 取得載入的 GameObject 後, 開始實例進行 FrameBase 組件的系列處理
        /// </summary>
        /// <param name="fBase"></param>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        protected abstract T Instantiate(T fBase, string assetName, AddIntoCache addIntoCache, Transform parent);

        /// <summary>
        /// 加載資源至緩存
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="assetName"></param>
        /// <param name="priority"></param>
        /// <param name="progression"></param>
        /// <param name="isPreloadMode"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        protected async UniTask<T> LoadIntoAllCache(string packageName, string assetName, uint priority, Progression progression, bool isPreloadMode, Transform parent = null)
        {
            if (this.HasInLoadingFlags(assetName))
            {
                Logging.PrintWarning<Logger>($"Asset: {assetName} is loading...");
                return null;
            }

            GameObject asset;
            T fBase = null;

            // 檢查是否有緩存
            if (this.HasStackInAllCache(assetName))
            {
                // 如果有緩存, 再透過允許多實例區分作法
                FrameStack<T> stack = this.GetStackFromAllCache(assetName);
                if (stack != null)
                {
                    // 如果允許多實例 & 預加載模式, 則直接返回 (主要是 Cacher 已經有加載資源了)
                    if (stack.allowInstantiate && isPreloadMode)
                    {
                        Logging.Print<Logger>($"<color=#FF9149>{stack.assetName} => 【Allow Instantiate + Preload Mode】skip cache process.</color>");
                        return null;
                    }
                    // 允所多實例時, 需要重覆加載 (確保 ref 正確, 不過會多 1 次, 需要額外減去)
                    else if (stack.allowInstantiate)
                    {
                        // 標記加載中
                        this._loadingFlags.Add(assetName);

                        asset = await this.LoadGameObject(packageName, assetName, priority, progression);

                        if (asset == null)
                        {
                            // 移除加載標記
                            this._loadingFlags.Remove(assetName);
                            return null;
                        }

                        fBase = asset.GetComponent<T>();
                        if (fBase == null)
                        {
                            // 移除加載標記
                            this._loadingFlags.Remove(assetName);
                            return null;
                        }

                        // 開始進行多實例
                        fBase = this.Instantiate(fBase, assetName, (fBaseNew) => { stack.Push(fBaseNew); }, parent);
                    }
                    // 不允許多實例, 則返回取得物件
                    else fBase = this.PeekStackFromAllCache(assetName);
                }
            }
            // 無則加載 (針對一開始必須要先執行加載, 取得組件中的參數進行判斷操作, 針對 allowInstantiate 會在 Cahcer 中有多 1 次的 ref)
            else
            {
                // 標記加載中
                this._loadingFlags.Add(assetName);

                asset = await this.LoadGameObject(packageName, assetName, priority, progression);

                if (asset == null)
                {
                    // 移除加載標記
                    this._loadingFlags.Remove(assetName);
                    return null;
                }

                fBase = asset.GetComponent<T>();
                if (fBase == null)
                {
                    // 移除加載標記
                    this._loadingFlags.Remove(assetName);
                    return null;
                }

                // 創建 Stack 空間
                FrameStack<T> stack = new FrameStack<T>(assetName, fBase.allowInstantiate, isPreloadMode);

                // 如果允許多實例 & 預加載模式, 只加入 Stack 至 AllCache (但是會有額外 1 次針對 Cacher 的加載, 需要再 Unload 時校正)
                if (stack.allowInstantiate && stack.isPreloadMode)
                {
                    this._dictAllCache.Add(assetName, stack);
                }
                // 反之, 則需要 Push 實例至 Stack 並且加入 Stack 至 AllCache
                else
                {
                    fBase = this.Instantiate(fBase, assetName, (fBaseNew) =>
                    {
                        stack.Push(fBaseNew);
                        this._dictAllCache.Add(assetName, stack);
                    }, parent);
                }
            }

            // 移除加載標記
            if (this.HasInLoadingFlags(assetName)) this._loadingFlags.Remove(assetName);

            return fBase;
        }

        /// <summary>
        /// 預加載
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="assetNames"></param>
        /// <param name="priority"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        public async UniTask Preload(string packageName, string[] assetNames, uint priority, Progression progression)
        {
            if (assetNames.Length > 0)
            {
                this.currentCount = 0;
                this.totalCount = assetNames.Length;

                for (int i = 0; i < assetNames.Length; i++)
                {
                    if (string.IsNullOrEmpty(assetNames[i]))
                    {
                        continue;
                    }

                    float lastCount = 0;
                    await this.LoadIntoAllCache(packageName, assetNames[i], priority, (float progress, float currentCount, float totalCount) =>
                    {
                        this.currentCount += currentCount - lastCount;
                        lastCount = currentCount;
                        progression?.Invoke(this.currentCount / this.totalCount, this.currentCount, this.totalCount);
                    }, true);
                }
            }
        }

        /// <summary>
        /// 依照對應的 Node 類型設置母節點 (子類實作)
        /// </summary>
        /// <param name="fBase"></param>
        protected virtual bool SetParent(T fBase, Transform parent) { return true; }

        /// <summary>
        /// 開啟預顯加載 UI
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        protected async virtual UniTask ShowAwaiting(int groupId, string packageName, string assetName, uint priority)
        {
            await UIFrame.UIManager.GetInstance().Show(groupId, packageName, assetName, null, null, priority);
        }

        /// <summary>
        /// 關閉預顯加載 UI
        /// </summary>
        protected void CloseAwaiting(string assetName)
        {
            if (!string.IsNullOrEmpty(assetName)) UIFrame.UIManager.GetInstance().Close(assetName, true);
        }

        #region Show
        /// <summary>
        /// 顯示, 可透過 id 進行群組分類
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="assetName"></param>
        /// <param name="obj"></param>
        /// <param name="awaitingUIAssetName"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        public async virtual UniTask<T> Show(int groupId, string packageName, string assetName, object obj = null, string awaitingUIAssetName = null, uint priority = 0, Progression progression = null, Transform parent = null)
        {
            await this.ShowAwaiting(groupId, packageName, awaitingUIAssetName, priority);

            return default;
        }
        #endregion

        #region Close
        /// <summary>
        /// 單個關閉
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="disabledPreClose"></param>
        /// <param name="forceDestroy"></param>
        public virtual void Close(string assetName, bool disabledPreClose = false, bool forceDestroy = false) { }

        /// <summary>
        /// 全部關閉
        /// </summary>
        /// <param name="disabledPreClose"></param>
        /// <param name="forceDestroy"></param>
        /// <param name="withoutAssetNames"></param>
        public virtual void CloseAll(bool disabledPreClose = false, bool forceDestroy = false, params string[] withoutAssetNames) { }

        /// <summary>
        /// 透過 id 群組進行全部關閉
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="disabledPreClose"></param>
        /// <param name="forceDestroy"></param>
        /// <param name="withoutAssetNames"></param>
        public virtual void CloseAll(int groupId, bool disabledPreClose = false, bool forceDestroy = false, params string[] withoutAssetNames) { }
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

        #region Destroy
        /// <summary>
        /// 銷毀釋放
        /// </summary>
        /// <param name="fBase"></param>
        /// <param name="assetName"></param>
        protected virtual void Destroy(T fBase, string assetName)
        {
            // 調用釋放接口
            fBase.OnRelease();

            // 刪除物件
            if (!fBase.gameObject.IsDestroyed()) Destroy(fBase.gameObject);

            // 取出柱列緩存
            FrameStack<T> stack = this.GetStackFromAllCache(assetName);
            stack.Pop();

            // 允許多實例 & 預加載模式, 需要再額外 Unload 1 次 (因為預加載額外進行多 1 次的 Cacher 加載, 所以需要校正 Cacher Ref Count)
            if (stack.allowInstantiate && stack.isPreloadMode && stack.Count() == 0)
            {
                // 額外卸載
                AssetLoaders.UnloadAsset(assetName);

                Logging.Print<Logger>($"<color=#ffa2a3>[FrameManager] Extra Unload Asset: {assetName}</color>");
            }

            // 柱列為空, 則刪除資源緩存
            if (stack.Count() == 0) this._dictAllCache.Remove(assetName);

            // 卸載
            AssetLoaders.UnloadAsset(assetName);

            Logging.Print<Logger>($"<color=#ffb6db>[FrameManager] Unload Asset: {assetName}</color>");

            Logging.Print<Logger>($"<color=#ff9d55>[FrameManager] Destroy Object: {assetName}</color>");
        }
        #endregion

        /// <summary>
        /// 【特殊方法】刷新特定物件
        /// </summary>
        /// <param name="assetNames"></param>
        /// <param name="objs"></param>
        public void SendRefreshData(RefreshInfo[] refreshInfos)
        {
            if (refreshInfos != null)
            {
                for (int i = 0; i < refreshInfos.Length; i++)
                {
                    string assetName = refreshInfos[i].assetName;
                    object data = refreshInfos[i].data;
                    if (this.HasStackInAllCache(assetName))
                    {
                        FrameStack<T> stack = this.GetStackFromAllCache(assetName);
                        if (stack != null)
                        {
                            if (this.CheckIsShowing(stack.Peek()) ||
                                this.CheckIsHiding(stack.Peek()))
                            {
                                foreach (var fBase in stack.cache)
                                {
                                    fBase.OnReceiveAndRefresh(data);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 【特殊方法】刷新特定物件
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="specificRefreshInfos"></param>
        public void SendRefreshDataToAll(object obj, RefreshInfo[] specificRefreshInfos)
        {
            foreach (FrameStack<T> stack in this._dictAllCache.Values)
            {
                if (stack != null)
                {
                    // 特定刷新與傳遞數據
                    bool doSpecific = false;
                    if (specificRefreshInfos != null)
                    {
                        for (int i = 0; i < specificRefreshInfos.Length; i++)
                        {
                            if (stack.assetName.Equals(specificRefreshInfos[i].assetName))
                            {
                                if (this.CheckIsShowing(stack.Peek()) ||
                                    this.CheckIsHiding(stack.Peek()))
                                {
                                    foreach (var fBase in stack.cache)
                                    {
                                        fBase.OnReceiveAndRefresh(specificRefreshInfos[i].data);
                                    }
                                }
                                doSpecific = true;
                                break;
                            }
                        }
                    }

                    if (doSpecific) continue;

                    // 單純刷新
                    if (this.CheckIsShowing(stack.Peek()) ||
                        this.CheckIsHiding(stack.Peek()))
                    {
                        foreach (var fBase in stack.cache)
                        {
                            fBase.OnReceiveAndRefresh(obj);
                        }
                    }
                }
            }
        }
    }

    internal static class ComponentExtensions
    {
        public static T GetCopyOf<T>(this T comp, T other) where T : Component
        {
            Type type = comp.GetType();
            Type othersType = other.GetType();
            if (type != othersType)
            {
                Logging.PrintError<Logger>($"The type \"{type.AssemblyQualifiedName}\" of \"{comp}\" does not match the type \"{othersType.AssemblyQualifiedName}\" of \"{other}\"!");
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

    internal static class GameObjectExtensions
    {
        /// <summary>
        /// Checks if a GameObject has been destroyed.
        /// </summary>
        /// <param name="gameObject">GameObject reference to check for destructiveness</param>
        /// <returns>If the game object has been marked as destroyed by UnityEngine</returns>
        public static bool IsDestroyed(this GameObject gameObject)
        {
            // UnityEngine overloads the == operator for the GameObject type
            // and returns null when the object has been destroyed, but 
            // actually the object is still there but has not been cleaned up yet
            // if we test both we can determine if the object has been destroyed.
            return gameObject == null && !ReferenceEquals(gameObject, null);
        }
    }
}
