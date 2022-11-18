using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OxGFrame.CoreFrame.UIFrame
{
    public class UIFreezeManager
    {
        #region FreezeNodePool, FreezeUI 物件池
        public class FreezeNodePool
        {
            private List<UIFreeze> _uiFreezePool = new List<UIFreeze>();    // 物件池
            private int _initNum = 5;                                       // 物件池初始數量
            private UIFreezeManager _uiFreezeManager = null;

            public FreezeNodePool(UIFreezeManager uiFreezeManager)
            {
                this._uiFreezeManager = uiFreezeManager;
            }

            private void _Init()
            {
                for (int i = 0; i < this._initNum; i++)
                {
                    UIFreeze uiFreeze = new GameObject(nodeName).AddComponent<UIFreeze>();
                    uiFreeze.gameObject.SetActive(false);
                    uiFreeze.gameObject.layer = this._uiFreezeManager.layer;
                    uiFreeze.gameObject.transform.SetParent(this._uiFreezeManager.uiFreezeRoot);
                    uiFreeze.gameObject.transform.localPosition = Vector3.zero;
                    uiFreeze.gameObject.transform.localScale = Vector3.one;
                    uiFreeze.InitFreeze();
                    this._uiFreezePool.Add(uiFreeze);
                }
            }

            public UIFreeze GetUIFreeze(Transform parent)
            {
                if (this._uiFreezePool.Count <= 0) this._Init();

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
                if (parent.Find(nodeName) == null) return false;

                GameObject uiFreezeNode = parent.Find(nodeName).gameObject;
                if (uiFreezeNode == null || !uiFreezeNode.GetComponent<UIFreeze>())
                {
                    Debug.Log(string.Format("No matching object found: {0}, doesn't need to recycle!", nodeName));
                    return false;
                }

                uiFreezeNode.transform.SetParent(this._uiFreezeManager.uiFreezeRoot);
                UIFreeze uiFreeze = uiFreezeNode.GetComponent<UIFreeze>();
                uiFreeze.UnUse();
                this._uiFreezePool.Add(uiFreeze);
                return true;
            }

            public void ClearFreezePool()
            {
                foreach (UIFreeze uiFreeze in this._uiFreezePool)
                {
                    uiFreeze.UnUse();
                }
                this._uiFreezePool.Clear();
            }
        }
        #endregion

        #region UIFreezeManager, UIFreeze 相關控制
        public const string nodeName = "_UIFreezeNode";
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
            if (parent.Find(nodeName) || !parent.GetComponent<UIBase>()) return;

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