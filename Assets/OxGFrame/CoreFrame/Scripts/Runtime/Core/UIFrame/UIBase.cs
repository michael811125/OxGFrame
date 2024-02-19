﻿using Cysharp.Threading.Tasks;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace OxGFrame.CoreFrame.UIFrame
{
    public delegate void AnimationEnd();

    public class UIBase : FrameBase
    {
        [HideInInspector] public Canvas canvas;
        [HideInInspector] public GraphicRaycaster graphicRaycaster;

        [Tooltip("Use reverse changes"), ConditionalField(nameof(onCloseAndDestroy), true)]
        public bool reverseChanges = false;
        [Tooltip("UI Settings")]
        public UISetting uiSetting = new UISetting();       // 定義 UI 類型, 用於取決於要新增至 UIRoot 中哪個對應的節點
        [Tooltip("If checked, will auto create a mask")]
        public bool autoMask = false;                       // 是否自動生成 Mask
        [ConditionalField(nameof(autoMask)), Tooltip("Mask Settings")]
        public MaskSetting maskSetting = new MaskSetting(); // Mask 設定

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (this.allowInstantiate) this.onCloseAndDestroy = false;
            if (this.onCloseAndDestroy) this.reverseChanges = false;
            if (this.reverseChanges || this.uiSetting.stack || this.uiSetting.allowCloseStackByStack)
            {
                this.uiSetting.whenCloseAllToSkip = false;
                this.uiSetting.whenHideAllToSkip = false;
            }
        }
#endif

        private void Awake()
        {
            this.canvas = this.GetComponent<Canvas>();
            this.graphicRaycaster = this.GetComponent<GraphicRaycaster>();
        }

        /// <summary>
        /// 僅執行一次, 只交由 UIManager 加載資源時呼叫初始參數
        /// </summary>
        public override void OnCreate() { }

        /// <summary>
        /// 僅執行一次, 只交由 UIManager 加載資源時呼叫初始相關綁定組件
        /// </summary>
        public sealed override void InitFirst()
        {
            base.InitFirst();
        }

        protected override async UniTask OnPreShow() { }

        protected override void OnPreClose() { }

        /// <summary>
        /// UI 初始相關 UI 綁定組件與註冊事件等 (僅初始一次)
        /// </summary>
        protected override void OnBind() { }

        /// <summary>
        /// 每次開啟 UI 時都會被執行, 子類 override
        /// </summary>
        /// <param name="obj"></param>
        protected override void OnShow(object obj) { }

        /// <summary>
        /// 接收封包後調用控制, 收到封包後的一個刷新點
        /// </summary>
        /// <param name="obj"></param>
        public override void OnReceiveAndRefresh(object obj = null) { }

        protected override void OnUpdate(float dt) { }

        protected override void OnFixedUpdate(float dt) { }

        protected override void OnLateUpdate(float dt) { }

        /// <summary>
        /// UIManager 控制調用 Display
        /// </summary>
        public sealed override void Display(object obj)
        {
            this.gameObject.SetActive(true);

            // 非隱藏才正規處理
            if (!this.isHidden)
            {
                // 啟用 Mask
                if (this.autoMask) this._AddMask();
                // 進行顯示初始動作【子類 OnShow】
                this.OnShow(obj);
            }
            else
            {
                // 確保 Mask
                if (this.autoMask) this._AddMask();
                // 隱藏顯示
                this.OnReveal();
            }

            this.Freeze();
            this.ShowAnimation(this.UnFreeze);
        }

        /// <summary>
        ///  UIManager 控制調用 Hide
        /// </summary>
        public sealed override void Hide(bool disabledPreClose = false)
        {
            if (!this.gameObject.activeSelf) return;

            this.Freeze();
            this.HideAnimation(() =>
            {
                this.UnFreeze();

                // 非隱藏才正規處理
                if (!this.isHidden)
                {
                    // 如果有啟用 Mask, 則需要回收 Mask
                    if (this.autoMask) this._RemoveMask();
                    if (!disabledPreClose) this.OnPreClose();
                    this.OnClose();
                }
                else this.OnHide();

                this.gameObject.SetActive(false);
            });
        }

        /// <summary>
        /// 創建 Mask UI
        /// </summary>
        private void _AddMask()
        {
            UIManager.GetInstance().GetUICanvas(this.uiSetting.canvasName).uiMaskManager.AddMask(this.transform, this.maskSetting.color, this.maskSetting.sprite, this.maskSetting.material, this.MaskEvent);
        }

        /// <summary>
        /// 移除 Mask UI
        /// </summary>
        private void _RemoveMask()
        {
            UIManager.GetInstance().GetUICanvas(this.uiSetting.canvasName).uiMaskManager.RemoveMask(this.transform);
        }

        /// <summary>
        /// 創建 Freeze UI
        /// </summary>
        public void Freeze()
        {
            UIManager.GetInstance().GetUICanvas(this.uiSetting.canvasName).uiFreezeManager.AddFreeze(this.transform);
        }

        /// <summary>
        /// 移除 Freeze UI
        /// </summary>
        public void UnFreeze()
        {
            UIManager.GetInstance().GetUICanvas(this.uiSetting.canvasName).uiFreezeManager.RemoveFreeze(this.transform);
        }

        /// <summary>
        /// 子類調用關閉自己
        /// </summary>
        protected sealed override void CloseSelf()
        {
            UIManager.GetInstance().Close(this.assetName);
        }

        protected virtual void MaskEvent()
        {
            if (this.maskSetting.isClickMaskToClose) UIManager.GetInstance().Close(this.assetName);
        }

        #region UI Transition Animation
        protected virtual void ShowAnimation(AnimationEnd animationEnd)
        {
            animationEnd();
        }

        protected virtual void HideAnimation(AnimationEnd animationEnd)
        {
            animationEnd();
        }
        #endregion
    }
}