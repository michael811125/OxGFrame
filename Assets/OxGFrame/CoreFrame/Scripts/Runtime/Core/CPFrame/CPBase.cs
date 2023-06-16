using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader;
using UnityEngine;

namespace OxGFrame.CoreFrame.CPFrame
{
    [HidePropertiesInInspector("onCloseAndDestroy", "allowInstantiate")]
    public class CPBase : FrameBase
    {
        private void OnEnable()
        {
            if (!this._isInitFirst) return;
            this.OnShow();
        }

        private void OnDisable()
        {
            this.OnClose();
        }

        private void OnDestroy()
        {
            this.OnRelease();
            AssetLoaders.UnloadAsset(this.assetName);
        }

        public override void OnInit() { }

        public sealed override void InitFirst()
        {
            base.InitFirst();
        }

        protected override void OnBind() { }

        protected virtual void OnShow() { }

        protected override void OnUpdate(float dt) { }

        public sealed override void Display(object obj)
        {
            this.gameObject.SetActive(true);
            this.OnShow();
        }

        #region GameObject Set
        public void SetLayerRecursively(string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer == -1) return;

            this.gameObject.layer = layer;
            foreach (Transform child in this.gameObject.transform)
            {
                this.SetLayerRecursively(child.gameObject, layerName);
            }
        }

        public void SetLayerRecursively(GameObject go, string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer == -1) return;

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
        public override void OnReceiveAndRefresh(object obj = null) { }

        [System.Obsolete("This is not supported in this class.")]
        public sealed override void Hide(bool disablePreClose = false) { }

        [System.Obsolete("This is not supported in this class.")]
        protected sealed override void CloseSelf() { }
        #endregion
    }
}
