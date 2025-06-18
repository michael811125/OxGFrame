using System;
using System.Collections.Generic;
using UnityEngine;

namespace OxGFrame.Hotfixer.Editor
{
    [Serializable]
    public class HotfixDllPlan
    {
        [SerializeField]
        public string planName = string.Empty;
        [SerializeField]
        public List<string> aotDlls = new List<string>();
        [SerializeField]
        public List<string> hotfixDlls = new List<string>();

        public HotfixDllPlan()
        {
            this.planName = "Hotfix Dll Plan";
        }
    }
}