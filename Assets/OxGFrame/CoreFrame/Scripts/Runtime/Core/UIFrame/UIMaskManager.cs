using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OxGFrame.CoreFrame.UIFrame
{
    public class UIMaskManager
    {
        #region MaskNodePool, MaskUI物件池
        public class MaskNodePool
        {
            private List<UIMask> _uiMaskPool = new List<UIMask>();  // uiMask物件池
            private int _initNum = 5;                               // 物件池初始數量
            private UIMaskManager _uiMaskManager = null;

            public MaskNodePool(UIMaskManager uiMaskManager)
            {
                this._uiMaskManager = uiMaskManager;
            }

            private void _Init(Sprite maskSprite)
            {
                for (int i = 0; i < this._initNum; i++)
                {
                    UIMask uiMask = new GameObject(nodeName).AddComponent<UIMask>();
                    uiMask.gameObject.SetActive(false);
                    uiMask.gameObject.layer = this._uiMaskManager.layer;
                    uiMask.gameObject.transform.SetParent(this._uiMaskManager.uiMaskRoot);
                    uiMask.gameObject.transform.localPosition = Vector3.zero;
                    uiMask.gameObject.transform.localScale = Vector3.one;
                    uiMask.InitMask(maskSprite);
                    this._uiMaskPool.Add(uiMask);
                }
            }

            public UIMask GetUIMask(Transform parent, Sprite maskSprite)
            {
                if (this._uiMaskPool.Count <= 0) this._Init(maskSprite);

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
                    Debug.Log(string.Format("未找到對應的 {0}, 不需回收!", nodeName));
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

        #region UIMaskManager, UIMask相關控制
        public const string nodeName = "_UIMaskNode";
        public Transform uiMaskRoot { get; private set; } = null;
        public int layer { get; private set; } = 0;

        private UIMask _uiMask = null;
        private Sprite _maskSprite = null;
        private MaskNodePool _maskNodePool = null;

        public UIMaskManager(int layer, Transform uiMaskRoot)
        {
            this.layer = layer;
            this.uiMaskRoot = uiMaskRoot;
            this._maskNodePool = new MaskNodePool(this);
        }

        /// <summary>
        /// 新增MaskUI
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="maskClickEvent"></param>
        public void AddMask(Transform parent, Color color, MaskEventFunc maskClickEvent = null)
        {
            this._maskSprite = this._MakeTexture2dSprite();
            if (parent.Find(nodeName) || !parent.GetComponent<UIBase>()) return;

            this._uiMask = this._maskNodePool.GetUIMask(parent, this._maskSprite);
            this._uiMask.SetMaskColor(color);
            if (maskClickEvent != null) this._uiMask.SetMaskClickEvent(maskClickEvent);
        }

        public void SetMaskColor(Color color)
        {
            this._uiMask.SetMaskColor(color);
        }

        /// <summary>
        /// 移除MaskUI
        /// </summary>
        /// <param name="parent"></param>
        public void RemoveMask(Transform parent)
        {
            this._maskNodePool.RecycleUIMask(parent);
        }

        private Sprite _MakeTexture2dSprite()
        {
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, true);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
            return sprite;
        }
        #endregion
    }
}