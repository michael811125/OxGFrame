using NUnit.Framework;
using OxGFrame.AssetLoader.Utility;
using UnityEngine;

namespace OxGFrame.AssetLoader.Editor.Tests
{
    public class PatchVersionEncode
    {
        [Test]
        public void PatchVersionEndoe()
        {
            string result = BundleUtility.GetVersionNumber("2025-03-27-845", 10, "-");
            Debug.Log(result);
            result = BundleUtility.GetVersionNumber("2025-03-27-845", 11, "-");
            Debug.Log(result);
            result = BundleUtility.GetVersionNumber("2025-03-27-845", 12, "-");
            Debug.Log(result);
            result = BundleUtility.GetVersionNumber("2025-03-27-845", 13, "-");
            Debug.Log(result);
            result = BundleUtility.GetVersionNumber("2025-03-27-845", 14, "-");
            Debug.Log(result);
            result = BundleUtility.GetVersionNumber("2025-03-27-845", 15, "-");
            Debug.Log(result);
            result = BundleUtility.GetVersionNumber("2025-03-27-845", 16, "-");
            Debug.Log(result);
            result = BundleUtility.GetVersionNumber("2025-03-27-845", 26, "-");
            Debug.Log(result);
            result = BundleUtility.GetVersionNumber("2025-03-27-845", 32, "-");
            Debug.Log(result);
            result = BundleUtility.GetVersionNumber("2025-03-27-845", 33, "-");
            Debug.Log(result);
        }
    }
}
