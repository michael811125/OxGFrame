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
        [Tooltip("節點分類")]
        public NodeType nodeType = NodeType.None;
        [Tooltip("當執行CloseAll時, 是否跳過處理")]
        public bool whenCloseAllToSkip = false;
        [Tooltip("當執行HideAll時, 是否跳過處理")]
        public bool whenHideAllToSkip = false;
    }

    public class GSSysDefine
    {
        /* 路徑常量 */
        public static readonly string GS_MANAGER_NAME = "GSManager";
    }
}
