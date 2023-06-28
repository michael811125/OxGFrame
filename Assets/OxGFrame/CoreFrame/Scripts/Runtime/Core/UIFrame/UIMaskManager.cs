using System.Collections.Generic;
using UnityEngine;

namespace OxGFrame.CoreFrame.UIFrame
{
    public class UIMaskManager
    {
        #region MaskNodePool, MaskUI 物件池
        public class MaskNodePool
        {
            private List<UIMask> _uiMaskPool = new List<UIMask>();  // 物件池
            private int _initNum = 5;                               // 物件池初始數量
            private UIMaskManager _uiMaskManager = null;

            public MaskNodePool(UIMaskManager uiMaskManager)
            {
                this._uiMaskManager = uiMaskManager;
            }

            private void _Init()
            {
                for (int i = 0; i < this._initNum; i++)
                {
                    UIMask uiMask = new GameObject(nodeName).AddComponent<UIMask>();
                    uiMask.gameObject.SetActive(false);
                    uiMask.gameObject.layer = this._uiMaskManager.layer;
                    uiMask.gameObject.transform.SetParent(this._uiMaskManager.uiMaskRoot);
                    uiMask.gameObject.transform.localPosition = Vector3.zero;
                    uiMask.gameObject.transform.localScale = Vector3.one;
                    uiMask.InitMask();
                    this._uiMaskPool.Add(uiMask);
                }
            }

            public UIMask GetUIMask(Transform parent)
            {
                if (this._uiMaskPool.Count <= 0) this._Init();

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
                if (parent.Find(nodeName) == null) return false;

                GameObject uiMaskNode = parent.Find(nodeName).gameObject;
                if (uiMaskNode == null || !uiMaskNode.GetComponent<UIMask>())
                {
                    Debug.Log(string.Format("No matching object found: {0}, doesn't need to recycle!", nodeName));
                    return false;
                }

                uiMaskNode.transform.SetParent(this._uiMaskManager.uiMaskRoot);
                UIMask uiMask = uiMaskNode.GetComponent<UIMask>();
                uiMask.UnUse();
                this._uiMaskPool.Add(uiMask);
                return true;
            }

            public void ClearMaskPool()
            {
                foreach (UIMask uiMask in this._uiMaskPool)
                {
                    uiMask.UnUse();
                }
                this._uiMaskPool.Clear();
            }
        }
        #endregion

        #region UIMaskManager, UIMask 相關控制
        public const string nodeName = "_UIMaskNode";
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
            if (parent.Find(nodeName) || !parent.GetComponent<UIBase>()) return;

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