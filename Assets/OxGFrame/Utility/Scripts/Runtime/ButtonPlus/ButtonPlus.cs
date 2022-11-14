using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

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
        [HideInInspector] public float originSize;
        [SerializeField, Tooltip("Button scale size")]
        public float size = 0.9f;
    }

    [SerializeField]
    public ExtdTransition extdTransition = ExtdTransition.None;
    [SerializeField]
    public TransScale transScale = new TransScale();

    [SerializeField, Tooltip("enable long press")]
    public bool isLongPress = false;
    [SerializeField, Tooltip("long press trigger time")]
    public float holdTime = 1f;
    [SerializeField, Tooltip("long press interval time")]
    public float cdTime = 0.1f;

    private float _cdTimer = 0;
    private bool _isHold = false;

    [SerializeField]
    private ButtonClickedEvent _onLongClick = new ButtonClickedEvent();
    public ButtonClickedEvent onLongClick { get { return this._onLongClick; } set { this._onLongClick = value; } }

    protected override void Awake()
    {
        this.transScale.originSize = this.transform.localScale.x;
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        // interactable 沒啟用直接 return 以下執行
        if (!base.interactable) return;

        // Extended Transition
        switch (this.extdTransition)
        {
            // 啟用 Scale 縮放
            case ExtdTransition.Scale:
                float size = this.transScale.originSize * this.transScale.size;
                this.transform.localScale = new Vector3(size, size, size);
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
                    this.transform.localScale = new Vector3(this.transScale.originSize, this.transScale.originSize, this.transScale.originSize);
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