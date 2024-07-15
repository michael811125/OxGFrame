using MyBox;
using System;
using UnityEngine;

namespace OxGFrame.CoreFrame.SRFrame
{
    [Serializable]
    public class SRSetting
    {
        [OverrideLabel("Exclude From Close All")]
        [Tooltip("If checked, when call CloseAll method will auto skip process")]
        public bool whenCloseAllToSkip = false;
        [OverrideLabel("Exclude From Hide All")]
        [Tooltip("If checked, when call HideAll method will auto skip process")]
        public bool whenHideAllToSkip = false;
    }
}
