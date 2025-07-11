using OxGKit.LoggingSystem;
using System.Collections.Generic;
using UnityEngine;

namespace OxGFrame.CoreFrame.UIFrame
{
    public class UIMaskManager
    {
        #region MaskNodePool, MaskUI 物件池
        public class MaskNodePool
        {
            private List<UIMask> _uiMaskPool = new List<UIMask>(); // 物件池
            private int _initNum = 5;                              // 物件池初始數量
            private int _maxNum = 10;                              // 物件池最大回收持有數量
            private UIMaskManager _uiMaskManager = null;

            public MaskNodePool(UIMaskManager uiMaskManager)
            {
                this._uiMaskManager = uiMaskManager;
                this._Init();
            }

            private void _Init()
            {
                for (int i = 0; i < this._initNum; i++)
                    this._CreateMask();
            }

            private void _CreateMask()
            {
                UIMask uiMask = new GameObject(NODE_NAME).AddComponent<UIMask>();
                uiMask.gameObject.SetActive(false);
                uiMask.gameObject.layer = this._uiMaskManager.layer;
                uiMask.gameObject.transform.SetParent(this._uiMaskManager.uiMaskRoot);
                uiMask.gameObject.transform.localPosition = Vector3.zero;
                uiMask.gameObject.transform.localScale = Vector3.one;
                uiMask.InitMask();
                this._uiMaskPool.Add(uiMask);
            }

            public UIMask GetUIMask(Transform parent)
            {
                if (this._uiMaskPool.Count <= 0)
                    this._CreateMask();

                UIMask uiMask = this._uiMaskPool[this._uiMaskPool.Count - 1];
                this._uiMaskPool.RemoveAt(this._uiMaskPool.Count - 1);

                uiMask.ReUse();
                uiMask.gameObject.transform.SetParent(parent);
                uiMask.gameObject.transform.SetAsFirstSibling();
                uiMask.SetLocalScale(Vector3.one);
                uiMask.SetStretch();

                return uiMask;
            }

            public bool RecycleUIMask(Transform parent)
            {
                if (parent.Find(NODE_NAME) == null)
                    return false;

                GameObject uiMaskNode = parent.Find(NODE_NAME).gameObject;
                if (uiMaskNode == null ||
                    !uiMaskNode.GetComponent<UIMask>())
                {
                    Logging.PrintWarning<Logger>($"No matching object found: {NODE_NAME}, doesn't need to recycle!");
                    return false;
                }

                uiMaskNode.transform.SetParent(this._uiMaskManager.uiMaskRoot);
                UIMask uiMask = uiMaskNode.GetComponent<UIMask>();
                uiMask.UnUse();
                if (this._uiMaskPool.Count >= this._maxNum)
                    GameObject.Destroy(uiMask.gameObject);
                else
                    this._uiMaskPool.Add(uiMask);
                return true;
            }
        }
        #endregion

        #region UIMaskManager, UIMask 相關控制
        internal const string NODE_NAME = "_UIMaskNode";
        public Transform uiMaskRoot { get; private set; } = null;
        public int layer { get; private set; } = 0;
        private MaskNodePool _maskNodePool = null;

        public UIMaskManager(int layer, Transform uiMaskRoot)
        {
            this.layer = layer;
            this.uiMaskRoot = uiMaskRoot;
            this._maskNodePool = new MaskNodePool(this);
        }

        /// <summary>
        /// 新增 Mask UI
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="maskClickEvent"></param>
        public void AddMask(Transform parent, Color color, Sprite sprite, Material material, MaskEventFunc maskClickEvent = null)
        {
            if (parent.Find(NODE_NAME) ||
                !parent.GetComponent<UIBase>())
                return;

            var uiMask = this._maskNodePool.GetUIMask(parent);
            uiMask.SetMaskColor(color);
            uiMask.SetMaskSprite(sprite);
            uiMask.SetMaskMaterial(material);
            uiMask.SetMaskClickEvent(maskClickEvent);
        }

        /// <summary>
        /// 移除 Mask UI
        /// </summary>
        /// <param name="parent"></param>
        public void RemoveMask(Transform parent)
        {
            this._maskNodePool.RecycleUIMask(parent);
        }
        #endregion
    }
}