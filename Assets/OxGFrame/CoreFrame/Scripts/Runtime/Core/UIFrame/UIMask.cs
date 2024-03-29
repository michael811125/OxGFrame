﻿using UnityEngine;
using UnityEngine.UI;

namespace OxGFrame.CoreFrame.UIFrame
{
    public class UIMask : MonoBehaviour
    {
        private MaskEventFunc _maskClickEvent = null;
        private Image _maskImage = null;

        public void InitMask()
        {
            // 建立一個 Image 當作遮罩 & MaskButton Raycast
            this._maskImage = this.gameObject.AddComponent<Image>();

            // 建立 Mask 事件
            Button maskBtn = this.gameObject.AddComponent<Button>();
            maskBtn.transition = Selectable.Transition.None;
            maskBtn.onClick.AddListener(() =>
            {
                this._maskClickEvent?.Invoke();
            });
        }

        /// <summary>
        /// 重新使用 Mask 時, 再指定一次 uiName
        /// </summary>
        /// <param name="uiName"></param>
        public void ReUse()
        {
            this.gameObject.SetActive(true);
        }

        /// <summary>
        /// 回收至 MaskPool 時, 會需要釋放相關參數
        /// </summary>
        public void UnUse()
        {
            this._maskImage.color = Color.white;
            this._maskImage.sprite = null;
            this._maskImage.material = null;
            this._maskClickEvent = null;
            this.gameObject.SetActive(false);
        }

        /// <summary>
        /// 交由 UIMaskManager 調用設置 Mask Color
        /// </summary>
        /// <param name="opacityType"></param>
        public void SetMaskColor(Color color)
        {
            this._maskImage.color = color;
        }

        /// <summary>
        /// 交由 UIMaskManager 調用設置 Mask Sprite
        /// </summary>
        /// <param name="sprite"></param>
        public void SetMaskSprite(Sprite sprite)
        {
            this._maskImage.sprite = sprite;
        }

        /// <summary>
        /// 交由 UIMaskManager 調用設置 Mask Material
        /// </summary>
        /// <param name="material"></param>
        public void SetMaskMaterial(Material material)
        {
            this._maskImage.material = material;
        }

        /// <summary>
        /// 設置 Mask 事件
        /// </summary>
        /// <param name="maskEventFunc"></param>
        public void SetMaskClickEvent(MaskEventFunc maskEventFunc)
        {
            this._maskClickEvent = maskEventFunc;
        }

        /// <summary>
        /// 設置 LocalScale
        /// </summary>
        /// <param name="scale"></param>
        public void SetLocalScale(Vector3 scale)
        {
            this._maskImage.rectTransform.localScale = scale;
        }

        /// <summary>
        /// 設置 Rectransform 成延展模式
        /// </summary>
        public void SetStretch()
        {
            this._maskImage.rectTransform.sizeDelta = new Vector2(0, 0);
            this._maskImage.rectTransform.anchorMin = new Vector2(0, 0);
            this._maskImage.rectTransform.anchorMax = new Vector2(1, 1);
            this._maskImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            this._maskImage.rectTransform.localRotation = Quaternion.identity;
        }
    }
}