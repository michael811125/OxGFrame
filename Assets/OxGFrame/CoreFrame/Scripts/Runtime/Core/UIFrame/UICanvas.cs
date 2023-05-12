using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OxGFrame.CoreFrame.UIFrame
{
    [DisallowMultipleComponent]
    public class UICanvas : MonoBehaviour
    {
        /* 階層 Canvas > UIRoot > UINodes, UIMaskManager, UIFreezeManager */

        [HideInInspector] public Canvas canvas;
        [HideInInspector] public CanvasScaler canvasScaler;
        [HideInInspector] public GraphicRaycaster graphicRaycaster;
        [HideInInspector] public GameObject uiRoot;    // UI 根節點物件
        public Dictionary<string, GameObject> uiNodes; // UI 節點物件
        public UIMaskManager uiMaskManager = null;     // UIMaskMgr, 由 UIManager 進行單例管控
        public UIFreezeManager uiFreezeManager = null; // UIFreezeMgr, 由 UIManager 進行單例管控

        public UICanvas()
        {
            this.uiNodes = new Dictionary<string, GameObject>();
        }

        private void Awake()
        {
            this.canvas = this.GetComponent<Canvas>();
            this.canvasScaler = this.GetComponent<CanvasScaler>();
            this.graphicRaycaster = this.GetComponent<GraphicRaycaster>();
        }

        public GameObject GetUINode(NodeType nodeType)
        {
            this.uiNodes.TryGetValue(nodeType.ToString(), out GameObject goNode);
            return goNode;
        }

        private void OnDestroy()
        {
            if (Time.frameCount == 0 || !Application.isPlaying) return;

            try
            {
                // 如果 UICavas Destroy 時, 需要進行連動釋放, 確保 UIManager 緩存操作正常
                UIBase[] uiBases = this.gameObject.GetComponentsInChildren<UIBase>(true);
                foreach (var uiBase in uiBases)
                {
                    UIManager.GetInstance().Close(uiBase.assetName, true, true);
                }

                // 釋放 UIManager 中的 UICanvas 緩存
                UIManager.GetInstance().RemoveUICanvasFromCache(this.name);
            }
            catch
            {
                /* Nothing to do */
            }
        }

        ~UICanvas()
        {
            this.uiRoot = null;
            this.uiNodes = null;
            this.uiMaskManager = null;
            this.uiFreezeManager = null;
        }
    }
}