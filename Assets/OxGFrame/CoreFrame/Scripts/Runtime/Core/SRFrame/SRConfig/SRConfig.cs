using MyBox;
using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace OxGFrame.CoreFrame.SRFrame
{
    [MovedFrom("SRSetting")]
    [Serializable]
    public class SRSettings
    {
        [OverrideLabel("Exclude From Close All")]
        [Tooltip("If checked, when call CloseAll method will auto skip process")]
        public bool whenCloseAllToSkip = false;
        [OverrideLabel("Exclude From Hide All")]
        [Tooltip("If checked, when call HideAll method will auto skip process")]
        public bool whenHideAllToSkip = false;
    }
}
