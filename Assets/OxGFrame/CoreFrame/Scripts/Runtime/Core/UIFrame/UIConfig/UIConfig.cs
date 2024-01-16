#define ENABLED_AUTO_SET_LAYER_RECURSIVELY

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
        Fixed,
        TopFixed,
        Popup,
        TopPopup,
        LoadingPopup,
        SysPopup,
        TopSysPopup,
        AwaitingPopup
    }

    [Serializable]
    public class UISetting
    {
        [Tooltip("Canvas name (will find same name of canvas on the scene)")]
        public string canvasName = "Canvas";
        [Tooltip("Node layer type")]
        public NodeType nodeType = NodeType.Fixed;
        [Tooltip("Stack mode (will auto control order in layer)")]
        public bool stack = false;
        [ConditionalField(nameof(stack), inverse: true), Tooltip("Fixed rendering order without stack mode"), Range(0, UIConfig.ORDER_DIFFERENCE)]
        public int order = 0;
        [Tooltip("If checked, allow close stack by stack")]
        public bool allowCloseStackByStack = false;
        [Tooltip("If checked, when call CloseAll method will auto skip process (If ReverseChanges or Stack is enabled, it will not work)")]
        public bool whenCloseAllToSkip = false;
        [Tooltip("If checked, when call HideAll method will auto skip process (If ReverseChanges or Stack is enabled, it will not work)")]
        public bool whenHideAllToSkip = false;
    }

    [Serializable]
    public class MaskSetting
    {
        [Tooltip("Mask color")]
        public Color color = new Color32(0, 0, 0, 192);
        [Tooltip("Mask sprite")]
        public Sprite sprite;
        [Tooltip("Mask material")]
        public Material material;
        [Tooltip("If checked, when click mask will close self")]
        public bool isClickMaskToClose = true;
    }

    public class UIConfig
    {
#if ENABLED_AUTO_SET_LAYER_RECURSIVELY
        public static readonly bool autoSetLayerRecursively = true;
#else
        public static readonly bool autoSetLayerRecursively = false;
#endif

        /* 路徑常量 */
        public const string UI_ROOT_NAME = "UIRoot";
        public const string UI_MASK_NAME = "UIMaskPool";
        public const string UI_FREEZE_NAME = "UIFreezePool";

        /* 節點常量, 可自行定義 NodeType 後, 並且在此新增 【NodeType + Order, 排序小 (後層) -> 大 (前層)】 */
        public static readonly Dictionary<NodeType, int> uiNodes = new Dictionary<NodeType, int>()
            {
                { NodeType.Fixed, 0},
                { NodeType.TopFixed, 1000},
                { NodeType.Popup, 2000},
                { NodeType.TopPopup, 3000},
                { NodeType.LoadingPopup, 4000},
                { NodeType.SysPopup, 5000},
                { NodeType.TopSysPopup, 6000},
                { NodeType.AwaitingPopup, 7000}
            };

        /* 節點之間的排序差值 - ORDER DIFFERENCE */
        public const int ORDER_DIFFERENCE = 1000;
    }
}