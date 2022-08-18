#define ENABLE_FIND_ALL_CANVASES           // 啟用FindAll查找是否有相符CanvasType定義名稱的物件 (會在初始時耗時)
#define ENABLE_AUTO_SET_LAYER_RECURSIVELY  // 啟用自動設置UI繼承主Canvas的LayerMask (在每第一次加載資源會耗時設置, 後續快取後如果不進行刪除則不影響)

using UnityEngine;
using System;
using MyBox;
using System.Collections.Generic;

namespace OxGFrame.CoreFrame.UIFrame
{
    public delegate void MaskEventFunc();

    /// <summary>
    /// Canvas歸類 (※重要: 自行定義時, 必須跟場景上的Canvas命名一樣)
    /// </summary>
    public enum CanvasType
    {
        CanvasOverlay,
        CanvasCamera,
        CanvasWorld
    }

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
        [Tooltip("Canvas分類 (會透過name進行查找場景上的同名Canvas)")]
        public CanvasType canvasType = CanvasType.CanvasOverlay;
        [Tooltip("節點分類")]
        public NodeType nodeType = NodeType.Normal;
        [Tooltip("啟用堆疊式管理 (會自行控制階層)")]
        public bool stack = false;
        [ConditionalField(nameof(stack), inverse: true), Tooltip("設定固定渲染順序 (非堆疊式)"), Range(0, UISysDefine.ORDER_DIFFERENCE)]
        public int order = 0;
        [Tooltip("當執行CloseAll時, 是否跳過處理")]
        public bool whenCloseAllToSkip = false;
        [Tooltip("當執行HideAll時, 是否跳過處理")]
        public bool whenHideAllToSkip = false;
    }

    [Serializable]
    public class MaskSetting
    {
        [Tooltip("Mask顏色")]
        public Color color = new Color32(0, 0, 0, 192);
        [Tooltip("是否點擊Mask進行CloseSelf事件")]
        public bool isClickMaskToClose = true;
    }

    public class UISysDefine
    {
#if ENABLE_FIND_ALL_CANVASES
        public static readonly bool bFindAllCanvases = true;
#else
            public static readonly bool bFindAllCanvases = false;
#endif

#if ENABLE_AUTO_SET_LAYER_RECURSIVELY
        public static readonly bool bAutoSetLayerRecursively = true;
#else
            public static readonly bool bAutoSetLayerRecursively = false;
#endif

        /* 路徑常量 */
        public static readonly string UI_MANAGER_NAME = "UIManager";
        public static readonly string UI_ROOT_NAME = "UIRoot";
        public static readonly string UI_MASK_NAME = "UIMaskPool";
        public static readonly string UI_FREEZE_NAME = "UIFreezePool";

        /* 節點常量, 可自行定義NodeType後, 並且在此新增 【NodeType + Order, 排序小(後層) -> 大(前層)】 */
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