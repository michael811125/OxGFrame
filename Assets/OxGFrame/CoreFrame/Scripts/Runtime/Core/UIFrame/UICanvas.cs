using System.Collections;
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
        [HideInInspector] public GameObject goRoot;    // UI根節點物件
        public Dictionary<string, GameObject> goNodes; // UI節點物件
        public UIMaskManager uiMaskManager = null;     // UIMaskMgr, 由UIManager進行單例管控
        public UIFreezeManager uiFreezeManager = null; // UIFreezeMgr, 由UIManager進行單例管控

        public UICanvas()
        {
            this.goNodes = new Dictionary<string, GameObject>();
        }

        private void Awake()
        {
            this.canvas = this.GetComponent<Canvas>();
            this.canvasScaler = this.GetComponent<CanvasScaler>();
            this.graphicRaycaster = this.GetComponent<GraphicRaycaster>();
        }

        public GameObject GetUINode(NodeType nodeType)
        {
            this.goNodes.TryGetValue(nodeType.ToString(), out GameObject goNode);
            return goNode;
        }

        private void OnDestroy()
        {
            if (Time.frameCount == 0 || !Application.isPlaying) return;

            try
            {
                // 如果UICavas在轉換場景被Destroy時, 需要進行連動釋放, 確保UIManager下次運作正常
                UIBase[] uiBases = this.gameObject.GetComponentsInChildren<UIBase>(true);
                foreach (var uiBase in uiBases)
                {
                    UIManager.GetInstance().Close(uiBase.assetName, true, true);
                }

                // 釋放UIManager中的UICanvas快取
                UIManager.GetInstance().RemoveUICanvasFromCache(this.name);
            }
            catch
            {
                /* Not to do */
            }
        }

        ~UICanvas()
        {
            this.goRoot = null;
            this.goNodes = null;
            this.uiMaskManager = null;
            this.uiFreezeManager = null;
        }
    }
}