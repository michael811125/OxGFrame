using System;
using System.Collections.Generic;

namespace OxGFrame.AssetLoader.Bundle
{
    [Serializable]
    public class GroupInfo
    {
        [NonSerialized]
        public int totalCount;
        [NonSerialized]
        public long totalBytes;

        public string groupName;
        public string[] tags;
    }

    public class PackageInfo
    {
        public string packageName;
        public string packageVersion;
        public long packageSize;
    }

    public class AppConfig
    {
        [NonSerialized]
        public static readonly string[] keys = new string[]
        {
            "PLATFORM",
            "PRODUCT_NAME",
            "APP_VERSION"
        };

        public string PLATFORM;     // 平台
        public string PRODUCT_NAME; // 產品名稱
        public string APP_VERSION;  // 主程式版本

        ~AppConfig()
        {
            this.PLATFORM = null;
            this.PRODUCT_NAME = null;
            this.APP_VERSION = null;
        }
    }

    public class PatchConfig
    {
        [NonSerialized]
        public static readonly string[] keys = new string[]
        {
            "PACKAGES",
            "GROUP_INFOS"
        };

        public List<PackageInfo> PACKAGES;
        public List<GroupInfo> GROUP_INFOS;

        public PatchConfig()
        {
            this.PACKAGES = new List<PackageInfo>();
            this.GROUP_INFOS = new List<GroupInfo>();
        }

        public void AddPackageInfo(PackageInfo packageInfo)
        {
            if (this.PACKAGES.Contains(packageInfo)) return;
            this.PACKAGES.Add(packageInfo);
        }
    }
}