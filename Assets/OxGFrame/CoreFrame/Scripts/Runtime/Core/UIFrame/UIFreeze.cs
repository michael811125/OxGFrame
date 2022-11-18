using UnityEngine;
using UnityEngine.UI;

namespace OxGFrame.CoreFrame.UIFrame
{
    public class UIFreeze : MonoBehaviour
    {
        private Image _freezeImage = null;

        public void InitFreeze()
        {
            // 建立一個 Image 當作 FreezeButton Raycast 用 (BlockEvent)
            this._freezeImage = this.gameObject.AddComponent<Image>();
            this._freezeImage.color = new Color(0, 0, 0, 0);

            // 建立 Freeze 事件
            Button freezeBtn = this.gameObject.AddComponent<Button>();
            freezeBtn.transition = Selectable.Transition.None;
            freezeBtn.onClick.AddListener(() =>
            {
                Debug.LogWarning("<color=#42BBFF>UI has been frozen</color>");
            });
        }

        /**
        <summary>
        重新使用 UIFreeze
        </summary>
         */
        public void ReUse()
        {
            this.gameObject.SetActive(true);
        }

        /**
        <summary>
        回收至 FreezePool 的相關釋放
        </summary>
         */
        public void UnUse()
        {
            this.gameObject.SetActive(false);
        }

        /// <summary>
        /// 設置 LocalScale
        /// </summary>
        /// <param name="scale"></param>
        public void SetLocalScale(Vector3 scale)
        {
            this._freezeImage.rectTransform.localScale = scale;
        }

        /// <summary>
        /// 設置 Rectransform 成延展模式
        /// </summary>
        public void SetStretch()
        {
            this._freezeImage.rectTransform.sizeDelta = new Vector2(0, 0);
            this._freezeImage.rectTransform.anchorMin = new Vector2(0, 0);
            this._freezeImage.rectTransform.anchorMax = new Vector2(1, 1);
            this._freezeImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            this._freezeImage.rectTransform.localRotation = Quaternion.identity;
        }
    }
}