using System;
using UnityEngine;

namespace OxGFrame.CoreFrame.GSFrame
{
    [Serializable]
    public class GSSetting
    {
        [Tooltip("If checked when call CloseAll method will auto skip process")]
        public bool whenCloseAllToSkip = false;
        [Tooltip("If checked when call HideAll method will auto skip process")]
        public bool whenHideAllToSkip = false;
    }
}
