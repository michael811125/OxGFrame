using MyBox;
using OxGFrame.AssetLoader.Bundle;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OxGFrame.AssetLoader.Editor
{
    [Serializable]
    public class BundlePlan
    {
        [SerializeField]
        public string planName = string.Empty;
        [SerializeField]
        public List<string> appPackages = new List<string>();
        [SerializeField]
        public string groupInfoArgs = string.Empty;
        [SerializeField]
        public List<GroupInfo> groupInfos = new List<GroupInfo>();
        [SerializeField]
        public List<DlcInfo> individualPackages = new List<DlcInfo>();

        public BundlePlan()
        {
            this.planName = "Bundle Plan";
        }
    }
}
