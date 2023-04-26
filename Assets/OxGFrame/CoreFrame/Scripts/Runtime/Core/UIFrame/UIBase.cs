using Cysharp.Threading.Tasks;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace OxGFrame.CoreFrame.UIFrame
{
    public delegate void AnimeEndCb();

    public class UIBase : FrameBase
    {
        [HideInInspector] public Canvas canvas;
        [HideInInspector] public GraphicRaycaster graphicRaycaster;

        [Tooltip("UI Settings")]
        public UISetting uiSetting = new UISetting();       // 定義UI類型, 用於取決於要新增至 UIRoot 中哪個對應的節點
        [Tooltip("If checked will auto create a mask")]
        public bool autoMask = false;                       // 是否自動生成 Mask
        [ConditionalField(nameof(autoMask)), Tooltip("Mask Settings")]
        public MaskSetting maskSetting = new MaskSetting(); // Mask 設定

        private void Awake()
        {
            this.canvas = this.GetComponent<Canvas>();
            this.graphicRaycaster = this.GetComponent<GraphicRaycaster>();
        }

        /// <summary>
        /// 僅執行一次, 只交由 UIManager 加載資源時呼叫初始參數
        /// </summary>
        public override void OnInit() { }

        /// <summary>
        /// 僅執行一次, 只交由 UIManager 加載資源時呼叫初始相關綁定組件
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

        /// <summary>
        /// UIManager 控制調用 Display
        /// </summary>
        public sealed override void Display(object obj)
        {
            this.gameObject.SetActive(true);

            // 非隱藏才正規處理
            if (!this.isHidden)
            {
                // 進行顯示初始動作【子類 OnShow】
                this.OnShow(obj);
                // 啟用 Mask
                if (this.autoMask) this._AddMask();
            }
            else this.OnReveal();

            this.Freeze();
            this.ShowAnime(() =>
            {
                this.UnFreeze();
            });
        }

        /// <summary>
        ///  UIManager 控制調用 Hide
        /// </summary>
        public sealed override void Hide(bool disableDoSub = false)
        {
            if (!this.gameObject.activeSelf) return;

            this.Freeze();
            this.HideAnime(() =>
            {
                this.UnFreeze();

                // 非隱藏才正規處理
                if (!this.isHidden)
                {
                    // 如果有啟用 Mask, 則需要回收 Mask
                    if (this.autoMask) this._RemoveMask();
                    if (!disableDoSub) this.CloseSub();
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
            UIManager.GetInstance().GetUICanvas(this.uiSetting.canvasName).uiMaskManager.AddMask(this.transform, this.maskSetting.color, this.MaskEvent);
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

        #region UI動畫過度
        protected virtual void ShowAnime(AnimeEndCb animeEndCb)
        {
            animeEndCb();
        }

        protected virtual void HideAnime(AnimeEndCb animeEndCb)
        {
            animeEndCb();
        }
        #endregion
    }
}