using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader;
using UnityEngine;

namespace OxGFrame.CoreFrame.SRFrame
{
    public class SRBase : FrameBase
    {
        [Tooltip("SceneResource Settings")]
        public SRSetting srSetting = new SRSetting();

        private void Awake()
        {
            if (this.monoDrive)
            {
                this.SetNames($"{nameof(this.monoDrive)}_{this.name}");
                this.OnCreate();
                this.InitFirst();
            }
        }

        private async UniTaskVoid OnEnable()
        {
            if (this.monoDrive)
            {
                if (!this._isInitFirst)
                    return;
                if (this.isMonoDriveDetected)
                    return;
                await this.OnPreShow();
                this.OnShow(null);
            }
        }

        private void OnDisable()
        {
            if (this.monoDrive)
            {
                this.OnPreClose();
                this.OnClose();
                if (this.onCloseAndDestroy)
                    Destroy(this.gameObject);
            }
        }

        private void OnDestroy()
        {
            if (this.monoDrive)
            {
                this.OnRelease();
                this.Dispose();
                AssetLoaders.UnloadAsset(this.assetName).Forget();
            }
        }

#if OXGFRAME_SRFRAME_MONODRIVE_UPDATE_ON
        private void Update()
        {
            if (this.monoDrive)
                this.HandleUpdate(Time.deltaTime);
        }
#endif

#if OXGFRAME_SRFRAME_MONODRIVE_FIXEDUPDATE_ON
        private void FixedUpdate()
        {
            if (this.monoDrive)
                this.HandleFixedUpdate(Time.fixedDeltaTime);
        }
#endif

#if OXGFRAME_SRFRAME_MONODRIVE_LATEUPDATE_ON
        private void LateUpdate()
        {
            if (this.monoDrive)
                this.HandleLateUpdate(Time.deltaTime);
        }
#endif

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
            if (!this.gameObject.activeSelf)
                return;

            if (!this.isHidden)
            {
                if (!disabledPreClose)
                    this.OnPreClose();
                this.OnClose();
            }
            else
                this.OnHide();

            this.gameObject.SetActive(false);
        }

        protected sealed override void CloseSelf()
        {
            this.CloseSelf(false, false);
        }

        protected sealed override void CloseSelf(bool disabledPreClose, bool forceDestroy)
        {
            SRManager.GetInstance().Close(this.assetName, disabledPreClose, forceDestroy);
        }

        protected sealed override void HideSelf()
        {
            SRManager.GetInstance().Hide(this.assetName);
        }
    }
}