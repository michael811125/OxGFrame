using System;
using System.Collections.Generic;

namespace OxGFrame.Hotfixer
{
    [Serializable]
    public class HotfixManifest
    {
        public List<string> aotDlls = new List<string>();
        public List<string> hotfixDlls = new List<string>();

        public HotfixManifest() { }

        public HotfixManifest(List<string> aotDlls, List<string> hotfixDlls)
        {
            this.aotDlls = aotDlls;
            this.hotfixDlls = hotfixDlls;
        }
    }
}