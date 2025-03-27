using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader;
using UnityEngine;

namespace OxGFrame.CoreFrame.CPFrame
{
    [HidePropertiesInInspector("onCloseAndDestroy", "allowInstantiate")]
    public class CPBase : FrameBase
    {
        private void Awake()
        {
            if (this.monoDrive)
            {
                this.SetNames($"{nameof(this.monoDrive)}_{this.name}");
                this.OnCreate();
                this.InitFirst();
            }
        }

        private void OnEnable()
        {
            if (!this._isInitFirst)
                return;
            if (this.isMonoDriveDetected)
                return;
            this.OnShow();
        }

        private void OnDisable()
        {
            this.OnClose();
        }

        private void OnDestroy()
        {
            this.OnRelease();
            this.Dispose();
            AssetLoaders.UnloadAsset(this.assetName).Forget();
        }

#if OXGFRAME_CPFRAME_MONODRIVE_UPDATE_ON
        private void Update()
        {
            if (this.monoDrive)
                this.HandleUpdate(Time.deltaTime);
        }
#endif

#if OXGFRAME_CPFRAME_MONODRIVE_FIXEDUPDATE_ON
        private void FixedUpdate()
        {
            if (this.monoDrive)
                this.HandleFixedUpdate(Time.fixedDeltaTime);
        }
#endif

#if OXGFRAME_CPFRAME_MONODRIVE_LATEUPDATE_ON
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

        protected override void OnBind() { }

        protected virtual void OnShow() { }

        protected override void OnUpdate(float dt) { }

        protected override void OnFixedUpdate(float dt) { }

        protected override void OnLateUpdate(float dt) { }

        internal sealed override void Display(object obj)
        {
            this.gameObject.SetActive(true);
            this.OnShow();
        }

        #region GameObject Set
        public void SetLayerRecursively(string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer == -1)
                return;

            this.gameObject.layer = layer;
            foreach (Transform child in this.gameObject.transform)
            {
                this.SetLayerRecursively(child.gameObject, layerName);
            }
        }

        public void SetLayerRecursively(GameObject go, string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer == -1)
                return;

            go.layer = layer;
            foreach (Transform child in go.transform)
            {
                this.SetLayerRecursively(child.gameObject, layerName);
            }
        }

        public void SetTag(string tagName)
        {
            this.gameObject.tag = tagName;
        }

        public void SetTag(GameObject go, string tagName)
        {
            go.tag = tagName;
        }
        #endregion

        #region Non-Use
        [System.Obsolete("This is not supported in this class.")]
        protected override async UniTask OnPreShow() { }

        [System.Obsolete("This is not supported in this class.")]
        protected override void OnPreClose() { }

        [System.Obsolete("This is not supported in this class.")]
        protected override void OnShow(object obj) { }

        [System.Obsolete("This is not supported in this class.")]
        protected override void OnHide() { }

        [System.Obsolete("This is not supported in this class.")]
        protected override void OnReveal() { }

        [System.Obsolete("This is not supported in this class.")]
        public override void OnReceiveAndRefresh(object obj = null) { }

        [System.Obsolete("This is not supported in this class.")]
        internal sealed override void Hide(bool disabledPreClose = false) { }

        [System.Obsolete("This is not supported in this class.")]
        protected sealed override void CloseSelf() { }

        [System.Obsolete("This is not supported in this class.")]
        protected sealed override void CloseSelf(bool disabledPreClose, bool forceDestroy) { }

        [System.Obsolete("This is not supported in this class.")]
        protected sealed override void HideSelf() { }
        #endregion
    }
}
