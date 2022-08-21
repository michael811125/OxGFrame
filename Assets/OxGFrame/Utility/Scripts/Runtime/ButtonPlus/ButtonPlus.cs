using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

namespace OxGFrame.Utility
{
    [AddComponentMenu("OxGFrame/Utility/Button/ButtonPlus")]
    public class ButtonPlus : Button
    {
        public enum ExtdTransition
        {
            None,
            Scale
        }

        [Serializable]
        public class TransScale
        {
            [HideInInspector] public float _originSize;
            [SerializeField, Tooltip("按鈕縮放大小")]
            public float size = 0.9f;
        }

        [SerializeField]
        public ExtdTransition extdTransition = ExtdTransition.None;
        [SerializeField]
        public TransScale transScale = new TransScale();

        [SerializeField, Tooltip("是否開啟長按")]
        public bool isLongPress = false;
        [SerializeField, Tooltip("長按時間")]
        public float holdTime = 1f;
        [SerializeField, Tooltip("長按時的事件觸發間隔CD")]
        public float cdTime = 0.1f;

        private float _cdTimer = 0;
        private bool _isHold = false;

        [SerializeField]
        private ButtonClickedEvent _onLongClick = new ButtonClickedEvent();
        public ButtonClickedEvent onLongClick { get { return this._onLongClick; } set { this._onLongClick = value; } }

        protected override void Awake()
        {
            this.transScale._originSize = this.transform.localScale.x;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            // interactable沒啟用直接return以下執行
            if (!base.interactable) return;

            // Extended Transition
            switch (this.extdTransition)
            {
                // 啟用Scale縮放
                case ExtdTransition.Scale:
                    float size_s = this.transScale._originSize * this.transScale.size;
                    this.transform.localScale = new Vector3(size_s, size_s, size_s);
                    break;
                default:
                    break;
            }

            // 判斷是否長按
            if (this.isLongPress) Invoke("OnLongPress", this.holdTime);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (!this._isHold) base.OnPointerUp(eventData);

            this._Reset();
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (!this._isHold) base.OnPointerClick(eventData);

            this._Reset();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            this._Reset();
        }

        private void _Reset()
        {
            CancelInvoke("OnLongPress");
            this._isHold = false;
            this._cdTimer = 0.0f;

            if (base.interactable)
            {
                switch (this.extdTransition)
                {
                    case ExtdTransition.Scale:
                        this.transform.localScale = new Vector3(this.transScale._originSize, this.transScale._originSize, this.transScale._originSize);
                        break;
                    default:
                        break;
                }
            }
        }

        private void Update()
        {
            if (!base.interactable) return;

            if (this._isHold)
            {
                this._cdTimer += Time.deltaTime;
                if (this._cdTimer >= this.cdTime)
                {
                    this.onLongClick.Invoke();
                    this._cdTimer = 0.0f;
                }
            }
        }

        /// <summary>
        /// invoke by Event
        /// </summary>
        private void OnLongPress()
        {
            this._isHold = true;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            this._Reset();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            this._Reset();
        }
    }

    public static class ButtonPlusExtension
    {
        /// <summary>
        /// ButtonPlus事件註冊, 單擊事件 & 長按事件
        /// </summary>
        public static void On(this ButtonPlus btn, UnityAction clickEvent, UnityAction longClickEvent = null, float holdTime = 1f, float cdTime = 0.1f)
        {
            btn.isLongPress = (btn.isLongPress || longClickEvent != null) ? true : false;

            // 配置長按設定
            btn.holdTime = holdTime;
            btn.cdTime = cdTime;

            // 加入單點事件
            btn.onClick?.RemoveAllListeners();
            btn.onClick?.AddListener(clickEvent);

            // 加入長按事件
            btn.onLongClick?.RemoveAllListeners();
            btn.onLongClick?.AddListener(longClickEvent);
        }
    }
}