using Cysharp.Threading.Tasks;
using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OxGFrame.CoreFrame.UIFrame
{
    public delegate void AnimEndCb();

    public class UIBase : FrameBase
    {
        [HideInInspector] public Canvas canvas;
        [HideInInspector] public GraphicRaycaster graphicRaycaster;

        [Tooltip("UI相關設置")]
        public UISetting uiSetting = new UISetting();       // 定義UI類型, 用於取決於要新增至UIRoot中哪個對應的節點
        [Tooltip("是否自動生成Mask")]
        public bool autoMask = false;                       // 是否自動生成Mask
        [ConditionalField(nameof(autoMask)), Tooltip("Mask相關設置")]
        public MaskSetting maskSetting = new MaskSetting(); // 定義Mask類型 (Popup系列)

        private void Awake()
        {
            this.canvas = GetComponent<Canvas>();
            this.graphicRaycaster = this.GetComponent<GraphicRaycaster>();
        }

        /// <summary>
        /// 僅執行一次, 只交由UIManager加載資源時呼叫初始參數
        /// </summary>
        public override void BeginInit() { }

        /// <summary>
        /// 僅執行一次, 只交由UIManager加載資源時呼叫初始相關綁定組件
        /// </summary>
        public sealed override void InitFirst()
        {
            base.InitFirst();
        }

        protected override async UniTask OpenSub()
        {
            await UniTask.Yield();
        }

        protected override void CloseSub() { }

        /// <summary>
        /// UI初始相關UI組件, 僅初始一次
        /// </summary>
        protected override void InitOnceComponents() { }

        /// <summary>
        /// UI初始相關註冊按鈕事件等等, 僅初始一次
        /// </summary>
        protected override void InitOnceEvents() { }

        /// <summary>
        /// 每次開啟UI時都會被執行, 子類override
        /// </summary>
        /// <param name="obj"></param>
        protected override void OnShow(object obj) { }

        /// <summary>
        /// 接收封包後調用控制, 收到封包後的一個刷新點, 可以由FuncId去判斷欲刷新的Protocol (需自行委派 Delegate)
        /// </summary>
        /// <param name="funcId"></param>
        public override void OnUpdateOnceAfterProtocol(int funcId = 0) { }

        protected override void OnUpdate(float dt) { }

        /// <summary>
        /// UIManager控制調用Display
        /// </summary>
        public sealed override void Display(object obj)
        {
            this.gameObject.SetActive(true);

            // 非隱藏才正規處理
            if (!this.isHidden)
            {
                // 進行顯示初始動作【子類OnShow】
                this.OnShow(obj);
                // 啟用Mask
                if (this.autoMask) this._AddMask();
            }
            else this.OnReveal();

            this.Freeze();
            this.ShowAnim(() =>
            {
                this.UnFreeze();
            });
        }

        /// <summary>
        ///  UIManager控制調用Hide
        /// </summary>
        public sealed override void Hide(bool disableDoSub = false)
        {
            if (!this.gameObject.activeSelf) return;

            this.Freeze();
            this.HideAnim(() =>
            {
                this.UnFreeze();

                    // 非隱藏才正規處理
                    if (!this.isHidden)
                {
                        // 如果有啟用Mask, 則需要回收Mask
                        if (this.autoMask) this._RemoveMask();
                    if (!disableDoSub) this.CloseSub();
                    this.OnClose();
                }
                else this.OnHide();

                this.gameObject.SetActive(false);
            });
        }

        /// <summary>
        /// 專屬Popup才有的Mask (增加)
        /// </summary>
        private void _AddMask()
        {
            UIManager.GetInstance().GetUICanvas(this.uiSetting.canvasType).uiMaskManager.AddMask(this.transform, this.maskSetting.color, this.MaskEvent);
        }

        /// <summary>
        /// 專屬Popup才有的Mask (移除)
        /// </summary>
        private void _RemoveMask()
        {
            UIManager.GetInstance().GetUICanvas(this.uiSetting.canvasType).uiMaskManager.RemoveMask(this.transform);
        }

        /// <summary>
        /// Popup系列會由UIManager控制調用Freeze, 其餘類型的會由Display() or Hide()
        /// </summary>
        public void Freeze()
        {
            UIManager.GetInstance().GetUICanvas(this.uiSetting.canvasType).uiFreezeManager.AddFreeze(this.transform);
        }

        /// <summary>
        /// Popup系列會由UIManager控制調用, 其餘類型的會由Display() or Hide()
        /// </summary>
        public void UnFreeze()
        {
            UIManager.GetInstance().GetUICanvas(this.uiSetting.canvasType).uiFreezeManager.RemoveFreeze(this.transform);
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

        #region UI動畫過度
        protected virtual void ShowAnim(AnimEndCb animEndCb)
        {
            animEndCb();
        }

        protected virtual void HideAnim(AnimEndCb animEndCb)
        {
            animEndCb();
        }
        #endregion
    }
}