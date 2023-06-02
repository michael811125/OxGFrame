using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

namespace OxGFrame.Utility.Btn
{
    [AddComponentMenu("OxGFrame/Utility/ButtonPlus/ButtonPlus")]
    public class ButtonPlus : Button
    {
        #region Transition
        public enum ExtdTransition
        {
            None,
            Scale
        }

        [Serializable]
        public class TransScale
        {
            [HideInInspector] public float originSize;
            [SerializeField, Tooltip("Button scale size")]
            public float size = 0.9f;
        }
        #endregion

        #region Long Click Mode
        public enum ExtdLongClick
        {
            None,
            Once,
            Continuous,
            PressedAndReleased
        }
        #endregion

        #region Extd Transition
        [SerializeField]
        public ExtdTransition extdTransition = ExtdTransition.None;
        [SerializeField]
        public TransScale transScale = new TransScale();
        #endregion

        #region Extd Long Click
        [SerializeField, Tooltip("Long click mode")]
        public ExtdLongClick extdLongClick = ExtdLongClick.None;
        [Tooltip("Not affected by TimeScale")]
        public bool ignoreTimeScale = false;
        [Tooltip("Long click trigger time")]
        public float triggerTime = 1f;
        [Tooltip("Long click interval time")]
        public float intervalTime = 0.1f;

        private float _longClickTimer = 0f;
        private bool _isHold = false;

        [SerializeField]
        private ButtonClickedEvent _onLongClickPressed = new ButtonClickedEvent();
        public ButtonClickedEvent onLongClickPressed
        {
            get { return this._onLongClickPressed; }
            set { this._onLongClickPressed = value; }
        }

        [SerializeField]
        private ButtonClickedEvent _onLongClickReleased = new ButtonClickedEvent();
        public ButtonClickedEvent onLongClickReleased
        {
            get { return this._onLongClickReleased; }
            set { this._onLongClickReleased = value; }
        }

        private ButtonClickedEvent _tempOnClick;
        #endregion

        protected override void Awake()
        {
            this.transScale.originSize = this.transform.localScale.x;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            if (!this.interactable) return;

            // Extended Transition
            switch (this.extdTransition)
            {
                case ExtdTransition.Scale:
                    float size = this.transScale.originSize * this.transScale.size;
                    this.transform.localScale = new Vector3(size, size, size);
                    break;

                // None
                default:
                    break;
            }

            // Extended Long Click
            switch (this.extdLongClick)
            {
                case ExtdLongClick.Once:
                    Invoke("OnLongClick", this.triggerTime);
                    break;
                case ExtdLongClick.Continuous:
                    Invoke("OnLongClick", this.triggerTime);
                    break;
                case ExtdLongClick.PressedAndReleased:
                    Invoke("OnLongClick", this.triggerTime);
                    break;

                // None
                default:
                    break;
            }
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);

            // Invoke long click on released
            if (this.extdLongClick == ExtdLongClick.PressedAndReleased)
            {
                if (this._isHold) this.onLongClickReleased.Invoke();
            }
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (!this._isHold) base.OnPointerClick(eventData);
            else
            {
                // When long click released must save click event to avoid invoke click event
                this._tempOnClick = this.onClick;
                // New click event (don't set to null)
                this.onClick = new ButtonClickedEvent();
                base.OnPointerClick(eventData);
                // Set click event back
                this.onClick = this._tempOnClick;
                // Clear temp
                this._tempOnClick = null;
            }

            this.ResetExtdLongClick();
            this.ResetExtdTransition();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            // Invoke long click on released
            if (this._isHold && this.extdLongClick == ExtdLongClick.PressedAndReleased)
            {
                this.onLongClickReleased.Invoke();
            }

            this.ResetExtdLongClick();
            this.ResetExtdTransition();
        }

        protected void ResetExtdLongClick()
        {
            CancelInvoke("OnLongClick");
            this._isHold = false;
            this._longClickTimer = 0f;
        }

        protected void ResetExtdTransition()
        {
            if (this.interactable)
            {
                switch (this.extdTransition)
                {
                    case ExtdTransition.Scale:
                        this.transform.localScale = new Vector3(this.transScale.originSize, this.transScale.originSize, this.transScale.originSize);
                        break;

                    // None
                    default:
                        break;
                }
            }
        }

        private void Update()
        {
            if (!this.interactable) return;

            // Invoke long click on hold and continuous
            if (this._isHold && this.extdLongClick == ExtdLongClick.Continuous)
            {
                if (this.ignoreTimeScale) this._longClickTimer += Time.unscaledDeltaTime;
                else this._longClickTimer += Time.deltaTime;
                if (this._longClickTimer >= this.intervalTime)
                {
                    this.onLongClickPressed.Invoke();
                    this._longClickTimer = 0f;
                }
            }
        }

        /// <summary>
        /// Invoke long click by Event
        /// </summary>
        protected void OnLongClick()
        {
            this._isHold = true;

            // Invoke long click on pressed
            this.onLongClickPressed.Invoke();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            this.ResetExtdLongClick();
            this.ResetExtdTransition();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            this.ResetExtdLongClick();
            this.ResetExtdTransition();
        }
    }
}