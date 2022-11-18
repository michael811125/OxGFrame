using System;
using UnityEngine;

namespace OxGFrame.CoreFrame.GSFrame
{
    /// <summary>
    /// 節點歸類
    /// </summary>
    public enum NodeType
    {
        None,
        GameScene
    }

    [Serializable]
    public class GSSetting
    {
        [Tooltip("Node layer type")]
        public NodeType nodeType = NodeType.None;
        [Tooltip("If checked when call CloseAll method will auto skip process")]
        public bool whenCloseAllToSkip = false;
        [Tooltip("If checked when call HideAll method will auto skip process")]
        public bool whenHideAllToSkip = false;
    }

    public class GSSysDefine
    {
        /* 路徑常量 */
        public static readonly string GS_MANAGER_NAME = "GSManager";
    }
}
