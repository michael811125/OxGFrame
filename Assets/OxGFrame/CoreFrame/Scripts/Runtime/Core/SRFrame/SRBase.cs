using Cysharp.Threading.Tasks;
using UnityEngine;

namespace OxGFrame.CoreFrame.SRFrame
{
    public class SRBase : FrameBase
    {
        [Tooltip("SceneResource Settings")]
        public SRSetting srSetting = new SRSetting();

        public override void OnCreate() { }

        internal sealed override void InitFirst()
        {
            base.InitFirst();
        }

        protected override async UniTask OnPreShow() { }

        protected override void OnPreClose() { }

        protected override void OnBind() { }

        protected override void OnShow(object obj) { }

        public override void OnReceiveAndRefresh(object obj = null) { }

        protected override void OnUpdate(float dt) { }

        protected override void OnFixedUpdate(float dt) { }

        protected override void OnLateUpdate(float dt) { }

        internal sealed override void Display(object obj)
        {
            this.gameObject.SetActive(true);

            if (!this.isHidden)
                this.OnShow(obj);
            else
            {
                this.OnReveal();
                this.SetHidden(false);
            }
        }

        internal sealed override void Hide(bool disabledPreClose = false)
        {
            if (!this.gameObject.activeSelf) return;

            if (!this.isHidden)
            {
                if (!disabledPreClose) this.OnPreClose();
                this.OnClose();
            }
            else this.OnHide();

            this.gameObject.SetActive(false);
        }

        protected sealed override void CloseSelf()
        {
            SRManager.GetInstance().Close(this.assetName);
        }
    }
}