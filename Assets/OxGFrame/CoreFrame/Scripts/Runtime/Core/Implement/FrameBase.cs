using Cysharp.Threading.Tasks;
using MyBox;
using System.Collections.Generic;
using UnityEngine;

namespace OxGFrame.CoreFrame
{
    /// <summary>
    /// <para>
    /// Init Order: Awake (Once) > OnCreate (Once) > OnAutoBind (Once) > OnBind (Once) > OnPreShow (EveryOpen) > OnShow (EveryOpen)
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class FrameBase : MonoBehaviour, IFrameBase
    {
        #region 綁定物件的收集器
        public class Collector
        {
            #region 依照綁定類型建立緩存容器
            // 用於存放綁定物件的緩存 (GameObject)
            private Dictionary<string, List<GameObject>> _nodes = new Dictionary<string, List<GameObject>>();
            #endregion

            #region 依照綁定類型建立相關方法
            /// <summary>
            /// 加入綁定節點 (GameObject)
            /// </summary>
            /// <param name="bindName"></param>
            /// <param name="go"></param>
            public void AddNode(string bindName, GameObject go)
            {
                if (!this._nodes.ContainsKey(bindName))
                {
                    this._nodes[bindName] = new List<GameObject>() { go };
                }
                else this._nodes[bindName].Add(go);
            }

            /// <summary>
            /// Get Node GameObject
            /// </summary>
            /// <param name="bindName"></param>
            /// <returns></returns>
            public GameObject GetNode(string bindName)
            {
                if (this._nodes.ContainsKey(bindName))
                {
                    return this._nodes[bindName][0];
                }

                return null;
            }

            /// <summary>
            /// Get Node GameObjects
            /// </summary>
            /// <param name="bindName"></param>
            /// <returns></returns>
            public GameObject[] GetNodes(string bindName)
            {
                if (this._nodes.ContainsKey(bindName))
                {
                    return this._nodes[bindName].ToArray();
                }

                return null;
            }

            /// <summary>
            /// Get Node Component
            /// </summary>
            /// <typeparam name="TComponent"></typeparam>
            /// <param name="bindName"></param>
            /// <returns></returns>
            public TComponent GetNodeComponent<TComponent>(string bindName)
            {
                GameObject go = this.GetNode(bindName);
                if (go == null) return default;

                return go.GetComponent<TComponent>();
            }

            /// <summary>
            /// Get Node Components
            /// </summary>
            /// <typeparam name="TComponent"></typeparam>
            /// <param name="bindName"></param>
            /// <returns></returns>
            public TComponent[] GetNodeComponents<TComponent>(string bindName)
            {
                GameObject[] gos = this.GetNodes(bindName);
                if (gos == null) return default;

                TComponent[] components = new TComponent[gos.Length];
                for (int i = 0; i < components.Length; i++)
                {
                    components[i] = gos[i].GetComponent<TComponent>();
                }

                return components;
            }
            #endregion
        }
        #endregion

        [HideInInspector] public Collector collector { get; private set; } = new Collector(); // 綁定物件收集器
        [HideInInspector] public string assetName { get; protected set; } = string.Empty;     // (Bundle) AssetName = (Resource) PathName
        [HideInInspector] public int groupId { get; protected set; } = 0;                     // 群組 id
        [HideInInspector] public bool isHidden { get; protected set; } = false;               // 檢查是否隱藏 (主要區分 Close & Hide 行為)

        [HideInInspector] protected bool _isBinded { get; private set; } = false;             // 檢查是否綁定的開關
        [HideInInspector] protected bool _isInitFirst { get; private set; } = false;          // 是否初次初始

        [Tooltip("Allow instantiate, but when close will destroy directly")]
        public bool allowInstantiate = false;                                                 // 是否允許多實例            
        [Tooltip("If checked, will destroy on close"), ConditionalField(nameof(allowInstantiate), true)]
        public bool onCloseAndDestroy = false;                                                // 是否關閉時就 DestroyUI

        internal virtual void HandleUpdate(float dt)
        {
            if (!this._isInitFirst) return;
            this.OnUpdate(dt);
        }

        internal virtual void HandleFixedUpdate(float dt)
        {
            if (!this._isInitFirst) return;
            this.OnFixedUpdate(dt);
        }

        internal virtual void HandleLateUpdate(float dt)
        {
            if (!this._isInitFirst) return;
            this.OnLateUpdate(dt);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (this.allowInstantiate) this.onCloseAndDestroy = false;
        }
#endif

        /// <summary>
        /// 起始初始 (僅執行一次)
        /// </summary>
        public abstract void OnCreate();

        /// <summary>
        /// 綁定與初次初始
        /// </summary>
        public virtual void InitFirst()
        {
            // 未綁定的話就執行綁定流程
            if (!this._isBinded)
            {
                Binder.BindComponent(this);
                this._isBinded = true;
            }

            // 初次初始相關綁定組件與事件
            if (!this._isInitFirst)
            {
                this.OnAutoBind();
                this.OnBind();
                this._isInitFirst = true;
            }
        }

        /// <summary>
        /// 預初始
        /// </summary>
        /// <returns></returns>
        public async UniTask PreInit()
        {
            // 等待異步加載, 進行異步加載動作
            await this.OnPreShow();
        }

        /// <summary>
        /// 子類實現開啟附屬程序
        /// </summary>
        /// <returns></returns>
        protected abstract UniTask OnPreShow();

        /// <summary>
        /// 子類實現關閉附屬程序
        /// </summary>
        protected abstract void OnPreClose();

        /// <summary>
        /// 自動綁定初始區塊 (僅執行一次)
        /// </summary>
        protected virtual void OnAutoBind() { }

        /// <summary>
        /// 初始相關綁定組件與事件 (僅執行一次)
        /// </summary>
        protected abstract void OnBind();

        /// <summary>
        /// 顯示相關流程
        /// </summary>
        /// <param name="obj"></param>
        public abstract void Display(object obj);

        /// <summary>
        /// 隱藏相關流程
        /// </summary>
        public abstract void Hide(bool disabledPreClose);

        /// <summary>
        /// 開啟時每次都會被呼叫
        /// </summary>
        /// <param name="obj"></param>
        protected abstract void OnShow(object obj);

        /// <summary>
        /// 解除隱藏時每次都會被呼叫
        /// </summary>
        protected virtual void OnReveal() { }

        /// <summary>
        /// 呼叫 SendRefreshData 時被調用
        /// </summary>
        /// <param name="obj"></param>
        public abstract void OnReceiveAndRefresh(object obj = null);

        /// <summary>
        /// 每幀被調用 (預設啟用)
        /// </summary>
        /// <param name="dt"></param>
        protected abstract void OnUpdate(float dt);

        /// <summary>
        /// 每幀被調用 (預設關閉)
        /// </summary>
        /// <param name="dt"></param>
        protected abstract void OnFixedUpdate(float dt);

        /// <summary>
        /// 每幀被調用 (預設關閉)
        /// </summary>
        /// <param name="dt"></param>
        protected abstract void OnLateUpdate(float dt);

        /// <summary>
        /// 關閉時每次都會被呼叫
        /// </summary>
        protected virtual void OnClose() { }

        /// <summary>
        /// 隱藏時每次都會被呼叫
        /// </summary>
        protected virtual void OnHide() { }

        /// <summary>
        /// Destroy 時會被呼叫
        /// </summary>
        public virtual void OnRelease() { }

        /// <summary>
        /// 調用關閉自己
        /// </summary>
        protected abstract void CloseSelf();

        /// <summary>
        /// 設置名稱
        /// </summary>
        /// <param name="assetName"></param>
        public void SetNames(string assetName)
        {
            this.assetName = assetName;
        }

        /// <summary>
        /// 設置群組 Id
        /// </summary>
        /// <param name="groupId"></param>
        public void SetGroupId(int groupId)
        {
            this.groupId = groupId;
        }

        /// <summary>
        /// 設置隱藏開關
        /// </summary>
        /// <param name="isHidden"></param>
        public void SetHidden(bool isHidden)
        {
            this.isHidden = isHidden;
        }
    }
}