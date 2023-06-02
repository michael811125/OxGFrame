using Cysharp.Threading.Tasks;
using UnityEngine;

namespace OxGFrame.CoreFrame.SRFrame
{
    public class SRBase : FrameBase
    {
        [Tooltip("SceneResource Settings")]
        public SRSetting srSetting = new SRSetting();

        public override void OnInit() { }

        public sealed override void InitFirst()
        {
            base.InitFirst();
        }

        protected override async UniTask OpenSub() { }

        protected override void CloseSub() { }

        protected override void OnBind() { }

        protected override void OnShow(object obj) { }

        public override void OnReceiveAndRefresh(object obj = null) { }

        protected override void OnUpdate(float dt) { }

        public sealed override void Display(object obj)
        {
            this.gameObject.SetActive(true);

            if (!this.isHidden) this.OnShow(obj);
            else this.OnReveal();
        }

        public sealed override void Hide(bool disableDoSub = false)
        {
            if (!this.gameObject.activeSelf) return;

            if (!this.isHidden)
            {
                if (!disableDoSub) this.CloseSub();
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