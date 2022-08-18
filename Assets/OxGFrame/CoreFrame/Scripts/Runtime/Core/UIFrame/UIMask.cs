using UnityEngine;
using UnityEngine.UI;

namespace OxGFrame.CoreFrame.UIFrame
{
    public class UIMask : MonoBehaviour
    {
        private MaskEventFunc _maskClickEvent = null;
        private Image _maskImage = null;

        public void InitMask(Sprite maskSprite)
        {
            // 建立一個Image當作遮罩 & MaskButton Raycast
            this._maskImage = this.gameObject.AddComponent<Image>();
            this._maskImage.sprite = maskSprite;

            // 建立Mask事件
            Button maskBtn = this.gameObject.AddComponent<Button>();
            maskBtn.transition = Selectable.Transition.None;
            maskBtn.onClick.AddListener(() =>
            {
                this._maskClickEvent?.Invoke();
            });
        }

        /// <summary>
        /// 重新使用Mask時, 再指定一次uiName
        /// </summary>
        /// <param name="uiName"></param>
        public void ReUse()
        {
            this.gameObject.SetActive(true);
        }

        /// <summary>
        /// 回收至MaskPool時, 會需要釋放相關參數
        /// </summary>
        public void UnUse()
        {
            this._maskClickEvent = null;
            this._maskImage.color = new Color(0, 0, 0, 0);
            this.gameObject.SetActive(false);
        }

        /// <summary>
        /// 交由UIMaskManager調用顯示Mask Alpha
        /// </summary>
        /// <param name="opacityType"></param>
        public void SetMaskColor(Color color)
        {
            this._maskImage.color = color;
        }

        /// <summary>
        /// 取得當前Mask Alpha Color
        /// </summary>
        /// <returns></returns>
        public Color GetMaskAlpha()
        {
            return this._maskImage.color;
        }

        /// <summary>
        /// 設置LocalScale
        /// </summary>
        /// <param name="scale"></param>
        public void SetLocalScale(Vector3 scale)
        {
            this._maskImage.rectTransform.localScale = scale;
        }

        /// <summary>
        /// 設置Rectransform成延展模式
        /// </summary>
        public void SetStretch()
        {
            this._maskImage.rectTransform.sizeDelta = new Vector2(0, 0);
            this._maskImage.rectTransform.anchorMin = new Vector2(0, 0);
            this._maskImage.rectTransform.anchorMax = new Vector2(1, 1);
            this._maskImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            this._maskImage.rectTransform.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// 設置Mask事件
        /// </summary>
        /// <param name="maskEventFunc"></param>
        public void SetMaskClickEvent(MaskEventFunc maskEventFunc)
        {
            this._maskClickEvent = maskEventFunc;
        }
    }
}