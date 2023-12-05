using MyBox;
using System;
using System.Collections.Generic;
using UnityEngine;
using static OxGFrame.AssetLoader.Bundle.BundleConfig;

namespace OxGFrame.AssetLoader.Bundle
{
    [Serializable]
    public class DecryptInfo : IDisposable
    {
        [SerializeField, Tooltip("Bundle decryption (case-insensitive).\n\n[NONE], \n[OFFSET, dummySize], \n[XOR, key], \n[HT2XOR, headKey, tailKey, jumpKey], \n[AES, key, iv]\n\nex: \n\"none\" \n\"offset, 12\" \n\"xor, 23\" \n\"ht2xor, 34, 45, 56\" \n\"aes, key, iv\"")]
        private string _decryptArgs = BundleConfig.CryptogramType.NONE;
        [SerializeField, Tooltip("Can encrypt string data in memroy.")]
        public bool secureString = true;
        [SerializeField, Tooltip("The longer the length, the safer it is. 16 bytes (128 bits), 32 bytes (256 bits)"), ConditionalField(nameof(secureString))]
        private int _saltSize = 1 << 4;
        [SerializeField, Tooltip("The longer the length, the safer it is. 16 bytes (128 bits), 32 bytes (256 bits)"), ConditionalField(nameof(secureString))]
        private int _dummySize = 1 << 5;

        public string GetDecryptArgs()
        {
            return string.IsNullOrEmpty(this._decryptArgs) ? BundleConfig.CryptogramType.NONE : this._decryptArgs;
        }

        public int GetSaltSize()
        {
            return this._saltSize;
        }

        public int GetDummySize()
        {
            return this._dummySize;
        }

        public void Dispose()
        {
            this._decryptArgs = null;
            this._saltSize = 0;
            this._dummySize = 0;
        }
    }

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

    [Serializable]
    public class PackageInfoWithBuild
    {
        public BuildMode buildMode;
        public string packageName;
    }

    [Serializable]
    public class AppPackageInfoWithBuild : PackageInfoWithBuild
    {
    }

    [Serializable]
    public class DlcPackageInfoWithBuild : PackageInfoWithBuild
    {
        public bool withoutPlatform = false;
        [Tooltip("If version is null or empty will auto set newset package version by date")]
        public string dlcVersion;
    }

    [Serializable]
    public class DlcInfo
    {
        public bool withoutPlatform = false;
        public string packageName;
        [Tooltip("If version is null or empty will auto set newset package version by date")]
        public string dlcVersion;
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