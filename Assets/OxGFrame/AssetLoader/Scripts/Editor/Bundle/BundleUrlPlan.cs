using System;
using UnityEngine;

namespace OxGFrame.AssetLoader.Editor
{
    [Serializable]
    public class BundleUrlPlan
    {
        [SerializeField]
        public string planName = string.Empty;
        [SerializeField]
        public string bundleIp = "127.0.0.1";
        [SerializeField]
        public string bundleFallbackIp = "127.0.0.1";
        [SerializeField]
        public string storeLink = "http://";

        public BundleUrlPlan()
        {
            this.planName = "Bundle Url Plan";
        }
    }
}