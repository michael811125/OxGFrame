#define ENABLE_FINEPD_ALL_CANVASES         // 啟用 FindAll 查找是否有相符 Canvas 定義名稱的物件 (會在初始時耗時)
#define ENABLE_AUTO_SET_LAYER_RECURSIVELY  // 啟用自動設置 UI 繼承主 Canvas 的 LayerMask (在每第一次加載資源會耗時設置, 後續快取後如果不進行刪除則不影響)

using UnityEngine;
using System;
using MyBox;
using System.Collections.Generic;

namespace OxGFrame.CoreFrame.UIFrame
{
    public delegate void MaskEventFunc();

    /// <summary>
    /// 節點歸類
    /// </summary>
    public enum NodeType
    {
        Normal,
        Fixed,
        Popup,
        TopPopup,
        LoadingPopup,
        SysPopup,
        TopSysPopup,
        PreloadingPopup
    }

    [Serializable]
    public class UISetting
    {
        [Tooltip("Canvas name (will find same name of canvas on the scene)")]
        public string canvasName = "Canvas";
        [Tooltip("Node layer type")]
        public NodeType nodeType = NodeType.Normal;
        [Tooltip("Stack mode (will auto controll order in layer)")]
        public bool stack = false;
        [ConditionalField(nameof(stack), inverse: true), Tooltip("Fixed rendering order without stack mode"), Range(0, UIConfig.ORDER_DIFFERENCE)]
        public int order = 0;
        [Tooltip("If checked when call CloseAll method will auto skip process")]
        public bool whenCloseAllToSkip = false;
        [Tooltip("If checked when call HideAll method will auto skip process")]
        public bool whenHideAllToSkip = false;
    }

    [Serializable]
    public class MaskSetting
    {
        [Tooltip("Mask color")]
        public Color color = new Color32(0, 0, 0, 192);
        [Tooltip("If checked when click mask will close self")]
        public bool isClickMaskToClose = true;
    }

    public class UIConfig
    {
#if ENABLE_FIND_ALL_CANVASES
        public static readonly bool findAllCanvases = true;
#else
        public static readonly bool findAllCanvases = false;
#endif

#if ENABLE_AUTO_SET_LAYER_RECURSIVELY
        public static readonly bool autoSetLayerRecursively = true;
#else
        public static readonly bool autoSetLayerRecursively = false;
#endif

        /* 路徑常量 */
        public static readonly string UI_ROOT_NAME = "UIRoot";
        public static readonly string UI_MASK_NAME = "UIMaskPool";
        public static readonly string UI_FREEZE_NAME = "UIFreezePool";

        /* 節點常量, 可自行定義 NodeType 後, 並且在此新增 【NodeType + Order, 排序小 (後層) -> 大 (前層)】 */
        public static readonly Dictionary<NodeType, int> UI_NODES = new Dictionary<NodeType, int>()
            {
                { NodeType.Normal, 0},
                { NodeType.Fixed, 1000},
                { NodeType.Popup, 2000},
                { NodeType.TopPopup, 3000},
                { NodeType.LoadingPopup, 4000},
                { NodeType.SysPopup, 5000},
                { NodeType.TopSysPopup, 6000},
                { NodeType.PreloadingPopup, 7000}
            };

        /* 節點之間的排序差值 - ORDER DIFFERENCE */
        public const int ORDER_DIFFERENCE = 1000;
    }
}