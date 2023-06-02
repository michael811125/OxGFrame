using System;
using UnityEngine;

namespace OxGFrame.CoreFrame.SRFrame
{
    [Serializable]
    public class SRSetting
    {
        [Tooltip("If checked when call CloseAll method will auto skip process")]
        public bool whenCloseAllToSkip = false;
        [Tooltip("If checked when call HideAll method will auto skip process")]
        public bool whenHideAllToSkip = false;
    }
}
