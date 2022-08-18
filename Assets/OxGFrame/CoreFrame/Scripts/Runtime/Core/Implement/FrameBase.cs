using Cysharp.Threading.Tasks;
using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OxGFrame.CoreFrame
{
    /// <summary>
    /// <para>
    /// Init Order: Awake(Once) > BeginInit(Once) > InitOnceComponents(Once) > InitOnceEvents(Once) > PreInit(EveryOpen) > OpenSub(EveryOpen) > OnShow(EveryOpen)
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class FrameBase : MonoBehaviour, IFrameBase
    {
        #region 綁定物件的收集器
        public class Collector
        {
            public string checkName = "";                                                          // 用於存放Prefab Name, 會用於判斷是否同物件執行重複綁定

            #region 依照綁定類型建立快取容器
            private Dictionary<string, GameObject> _nodes = new Dictionary<string, GameObject>();  // 用於存放綁定物件的快取 (GameObject)
            #endregion

            #region 依照綁定類型建立相關方法
            /// <summary>
            /// 加入綁定節點 (GameObject)
            /// </summary>
            /// <param name="key"></param>
            /// <param name="go"></param>
            public void AddNode(string key, GameObject go)
            {
                if (!this._nodes.ContainsKey(key)) this._nodes.Add(key, go);
                else this._nodes[key] = go;
            }

            /// <summary>
            /// 取得綁定節點 (GameObject)
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public GameObject GetNode(string key)
            {
                if (this._nodes.ContainsKey(key))
                {
                    return this._nodes[key];
                }

                return null;
            }
            #endregion
        }
        #endregion

        [HideInInspector] public Collector collector { get; private set; } = new Collector(); // 綁定物件收集器
        [HideInInspector] protected bool _isBinded { get; private set; } = false;             // 檢查是否綁定的開關
        [HideInInspector] protected bool _isInitFirst { get; private set; } = false;          // 是否初次初始

        [HideInInspector] public string bundleName { get; protected set; } = string.Empty;    // BundleName
        [HideInInspector] public string assetName { get; protected set; } = string.Empty;     // (Bundle) AssetName = (Resouce) PathName
        [HideInInspector] public int groupId { get; protected set; } = 0;                     // 群組id
        [HideInInspector] public bool isHidden { get; protected set; } = false;               // 檢查是否隱藏 (主要區分Close & Hide行為)

        [Tooltip("是否允許多實例, 採用堆疊式管理 (會在Close時, 直接銷毀)")]
        public bool allowInstantiate = false;                                                 // 是否允許多實例            
        [Tooltip("是否在Close時, 進行銷毀"), ConditionalField(nameof(allowInstantiate), true)]
        public bool onCloseAndDestroy = false;                                                // 是否關閉時就DestroyUI

        private void Update()
        {
            if (!this._isInitFirst) return;

            this.OnUpdate(Time.deltaTime);
        }

        /// <summary>
        /// 起始初始
        /// </summary>
        public abstract void BeginInit();

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

            // 初次初始相關組件與事件
            if (!this._isInitFirst)
            {
                this.InitOnceComponents();
                this.InitOnceEvents();
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
            await this.OpenSub();
        }

        /// <summary>
        /// 子類實現開啟附屬程序
        /// </summary>
        /// <returns></returns>
        protected abstract UniTask OpenSub();

        /// <summary>
        /// 子類實現關閉附屬程序
        /// </summary>
        protected abstract void CloseSub();

        /// <summary>
        /// 初始相關組件 (僅執行一次)
        /// </summary>
        protected abstract void InitOnceComponents();

        /// <summary>
        /// 初始相關事件 (僅執行一次)
        /// </summary>
        protected abstract void InitOnceEvents();

        /// <summary>
        /// 顯示相關流程
        /// </summary>
        /// <param name="obj"></param>
        public abstract void Display(object obj);

        /// <summary>
        /// 隱藏相關流程
        /// </summary>
        public abstract void Hide(bool disableDoSub);

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
        /// 會由Protocol接收到封包時, 被調用
        /// </summary>
        /// <param name="funcId"></param>
        public abstract void OnUpdateOnceAfterProtocol(int funcId = 0);

        /// <summary>
        /// 每幀被調用
        /// </summary>
        /// <param name="dt"></param>
        protected abstract void OnUpdate(float dt);

        /// <summary>
        /// 關閉時每次都會被呼叫
        /// </summary>
        protected virtual void OnClose() { }

        /// <summary>
        /// 隱藏時每次都會被呼叫
        /// </summary>
        protected virtual void OnHide() { }

        /// <summary>
        /// Destroy時會被呼叫
        /// </summary>
        public virtual void OnRelease() { }

        /// <summary>
        /// 調用關閉自己
        /// </summary>
        protected abstract void CloseSelf();

        /// <summary>
        /// 設置名稱
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        public void SetNames(string bundleName, string assetName)
        {
            this.bundleName = bundleName;
            this.assetName = assetName;
        }

        /// <summary>
        /// 設置群組Id
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