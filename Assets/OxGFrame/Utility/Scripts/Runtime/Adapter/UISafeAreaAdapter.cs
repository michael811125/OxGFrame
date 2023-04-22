using UnityEngine;

namespace OxGFrame.Utility.Adapter
{
    [DisallowMultipleComponent]
    [AddComponentMenu("OxGFrame/Utility/Adapter/UISafeAreaAdapter")]
    public class UISafeAreaAdapter : MonoBehaviour
    {
        public bool refreshAlways = false;
        public RectTransform panel;

        private Resolution _lateResolution;

        private void Awake()
        {
            this._lateResolution = Screen.currentResolution;
            this._InitPanel();
        }

        private void Start()
        {
            this.RefreshViewSize();
        }

        private void LateUpdate()
        {
            if (this.refreshAlways || this._lateResolution.width != Screen.currentResolution.width || this._lateResolution.height != Screen.currentResolution.height)
            {
                this.RefreshViewSize();
                this._lateResolution = Screen.currentResolution;
            }
        }

        private void _InitPanel()
        {
            if (this.panel == null) this.panel = this.GetComponent<RectTransform>();
        }

        public void RefreshViewSize()
        {
            if (this.panel == null) return;

            Debug.Log($"<color=#FFFF00>Current Safe Area w: {Screen.safeArea.width}, h: {Screen.safeArea.height}, x: {Screen.safeArea.position.x}, y: {Screen.safeArea.position.y}</color>");
            Debug.Log($"<color=#32CD32>Current Resolution w: {Screen.currentResolution.width}, h: {Screen.currentResolution.height}, dpi: {Screen.dpi}</color>");

            Vector2 anchorMin = Screen.safeArea.position;
            Vector2 anchorMax = Screen.safeArea.position + Screen.safeArea.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;
            this.panel.anchorMin = anchorMin;
            this.panel.anchorMax = anchorMax;
        }
    }
}
