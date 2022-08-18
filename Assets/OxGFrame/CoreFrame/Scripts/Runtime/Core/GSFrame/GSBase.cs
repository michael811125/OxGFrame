using Cysharp.Threading.Tasks;
using UnityEngine;

namespace OxGFrame.CoreFrame.GSFrame
{
    public class GSBase : FrameBase
    {
        [Tooltip("GameScene相關設置")]
        public GSSetting gsSetting = new GSSetting();

        public override void BeginInit() { }

        public sealed override void InitFirst()
        {
            base.InitFirst();
        }

        protected override async UniTask OpenSub()
        {
            await UniTask.Yield();
        }

        protected override void CloseSub() { }

        protected override void InitOnceComponents() { }

        protected override void InitOnceEvents() { }

        protected override void OnShow(object obj) { }

        public override void OnUpdateOnceAfterProtocol(int funcId = 0) { }

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
            GSManager.GetInstance().Close(this.assetName);
        }
    }
}