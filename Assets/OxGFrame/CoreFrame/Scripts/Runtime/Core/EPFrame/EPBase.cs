using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader;
using UnityEngine;

namespace OxGFrame.CoreFrame.EPFrame
{
    [HidePropertiesInInspector("onCloseAndDestroy", "allowInstantiate")]
    public class EPBase : FrameBase
    {
        private void OnEnable()
        {
            if (!this._isInitFirst) return;

            this.OnShow(null);
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

        public override void BeginInit() { }

        public sealed override void InitFirst()
        {
            base.InitFirst();
        }

        protected override void InitOnceComponents() { }

        protected override void InitOnceEvents() { }

        protected override void OnShow(object obj) { }

        protected override void OnUpdate(float dt) { }

        public sealed override void Display(object obj)
        {
            this.gameObject.SetActive(true);
            this.OnShow(obj);
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
        protected override async UniTask OpenSub() { }

        [System.Obsolete("This is not supported in this class.")]
        protected override void CloseSub() { }

        [System.Obsolete("This is not supported in this class.")]
        public override void OnReceiveAndRefresh(object obj = null) { }

        [System.Obsolete("This is not supported in this class.")]
        public sealed override void Hide(bool disableDoSub = false) { }

        [System.Obsolete("This is not supported in this class.")]
        protected sealed override void CloseSelf() { }
        #endregion
    }
}
