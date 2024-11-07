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
        public string bundleIp = "http://127.0.0.1";
        [SerializeField]
        public string bundleFallbackIp = "http://127.0.0.1";
        [SerializeField]
        public string storeLink = "http://";

        public BundleUrlPlan()
        {
            this.planName = "Bundle Url Plan";
        }
    }
}