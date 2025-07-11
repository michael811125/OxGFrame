using OxGKit.LoggingSystem;
using System.Collections.Generic;
using UnityEngine;

namespace OxGFrame.CoreFrame.UIFrame
{
    public class UIFreezeManager
    {
        #region FreezeNodePool, FreezeUI 物件池
        public class FreezeNodePool
        {
            private List<UIFreeze> _uiFreezePool = new List<UIFreeze>(); // 物件池
            private int _initNum = 5;                                    // 物件池初始數量
            private int _maxNum = 10;                                    // 物件池最大回收持有數量
            private UIFreezeManager _uiFreezeManager = null;

            public FreezeNodePool(UIFreezeManager uiFreezeManager)
            {
                this._uiFreezeManager = uiFreezeManager;
                this._Init();
            }

            private void _Init()
            {
                for (int i = 0; i < this._initNum; i++)
                    this._CreateFreeze();
            }

            private void _CreateFreeze()
            {
                UIFreeze uiFreeze = new GameObject(NODE_NAME).AddComponent<UIFreeze>();
                uiFreeze.gameObject.SetActive(false);
                uiFreeze.gameObject.layer = this._uiFreezeManager.layer;
                uiFreeze.gameObject.transform.SetParent(this._uiFreezeManager.uiFreezeRoot);
                uiFreeze.gameObject.transform.localPosition = Vector3.zero;
                uiFreeze.gameObject.transform.localScale = Vector3.one;
                uiFreeze.InitFreeze();
                this._uiFreezePool.Add(uiFreeze);
            }

            public UIFreeze GetUIFreeze(Transform parent)
            {
                if (this._uiFreezePool.Count <= 0)
                    this._CreateFreeze();

                UIFreeze uiFreeze = this._uiFreezePool[this._uiFreezePool.Count - 1];
                this._uiFreezePool.RemoveAt(this._uiFreezePool.Count - 1);

                uiFreeze.ReUse();
                uiFreeze.gameObject.transform.SetParent(parent);
                uiFreeze.gameObject.transform.SetAsLastSibling();
                uiFreeze.SetLocalScale(Vector3.one);
                uiFreeze.SetStretch();

                return uiFreeze;
            }

            public bool RecycleUIFreeze(Transform parent)
            {
                if (parent.Find(NODE_NAME) == null)
                    return false;

                GameObject uiFreezeNode = parent.Find(NODE_NAME).gameObject;
                if (uiFreezeNode == null ||
                    !uiFreezeNode.GetComponent<UIFreeze>())
                {
                    Logging.PrintWarning<Logger>($"No matching object found: {NODE_NAME}, doesn't need to recycle!");
                    return false;
                }

                uiFreezeNode.transform.SetParent(this._uiFreezeManager.uiFreezeRoot);
                UIFreeze uiFreeze = uiFreezeNode.GetComponent<UIFreeze>();
                uiFreeze.UnUse();
                if (this._uiFreezePool.Count >= this._maxNum)
                    GameObject.Destroy(uiFreeze.gameObject);
                else
                    this._uiFreezePool.Add(uiFreeze);
                return true;
            }
        }
        #endregion

        #region UIFreezeManager, UIFreeze 相關控制
        internal const string NODE_NAME = "_UIFreezeNode";
        public Transform uiFreezeRoot { get; private set; } = null;
        public int layer { get; private set; } = 0;

        private FreezeNodePool _freezeNodePool = null;

        public UIFreezeManager(int layer, Transform uiFreezeRoot)
        {
            this.layer = layer;
            this.uiFreezeRoot = uiFreezeRoot;
            this._freezeNodePool = new FreezeNodePool(this);
        }

        /// <summary>
        /// 新增 Freeze UI
        /// </summary>
        /// <param name="parent"></param>
        public void AddFreeze(Transform parent)
        {
            if (parent.Find(NODE_NAME) ||
                !parent.GetComponent<UIBase>())
                return;

            this._freezeNodePool.GetUIFreeze(parent);
        }

        /// <summary>
        /// 移除 Freeze UI
        /// </summary>
        /// <param name="parent"></param>
        public void RemoveFreeze(Transform parent)
        {
            this._freezeNodePool.RecycleUIFreeze(parent);
        }
        #endregion
    }
}