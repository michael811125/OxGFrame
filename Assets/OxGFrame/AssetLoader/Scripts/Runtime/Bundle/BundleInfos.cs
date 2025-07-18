﻿using MyBox;
using OxGFrame.AssetLoader.Utility.SecureMemory;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OxGFrame.AssetLoader.Bundle
{
    [Serializable]
    public class DecryptInfo : IDisposable
    {
        [SerializeField, Tooltip("Decryption (case-insensitive).\n\n[NONE], \n[OFFSET, dummySize], \n[XOR, key], \n[HT2XOR, headKey, tailKey, jumpKey], \n[HT2XORPLUS, headKey, tailKey, jump1Key, jump2Key], \n[AES, key, iv]\n[CHACHA20, key, nonce, counter]\n[XXTEA, key]\n[OFFSETXOR, key, dummySize]\n\nex: \n\"none\" \n\"offset, 12\" \n\"xor, 23\" \n\"ht2xor, 34, 45, 56\" \n\"ht2xorplus, 34, 45, 56, 78\" \n\"aes, key, iv\" \n\"chacha20, key, nonce, 1\" \n\"xxtea, key\" \n\"offsetxor, 12, 23\"")]
        private string _decryptArgs = BundleConfig.CryptogramType.NONE;
        [SerializeField, Tooltip("None: No encryption.\n\nXORWithDummy: simple encryption will be performed in memory (less GC).\n\nAES: complex encryption will be performed in memory (more GC).")]
        public SecuredStringType scuredStringType = SecuredStringType.XORWithDummy;
        [SerializeField, Tooltip("The longer the length, the safer it is. 16 bytes (128 bits), 32 bytes (256 bits)"), ConditionalField(nameof(scuredStringType), false, SecuredStringType.AES)]
        private int _saltSize = 1 << 4;
        [SerializeField, Tooltip("The longer the length, the safer it is. 16 bytes (128 bits), 32 bytes (256 bits)"), ConditionalField(nameof(scuredStringType), false, SecuredStringType.XORWithDummy)]
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
    public abstract class PackageInfoWithBuild
    {
        [Tooltip("Only for EditorSimulateMode")]
        public BundleConfig.BuildMode buildMode;
        public string packageName;

        /// <summary>
        /// Custom host server
        /// </summary>
        [HideInInspector]
        public string hostServer = null;
        /// <summary>
        /// Custom fallback host server
        /// </summary>
        [HideInInspector]
        public string fallbackHostServer = null;
    }

    [Serializable]
    public class AppPackageInfoWithBuild : PackageInfoWithBuild
    {
    }

    [Serializable]
    public class DlcPackageInfoWithBuild : PackageInfoWithBuild
    {
        public bool withoutPlatform = false;
        [Tooltip("If version is null or empty will auto set newest package version by date")]
        public string dlcVersion;
    }

    [Serializable]
    public class DlcInfo
    {
        public bool withoutPlatform = false;
        public string packageName;
        [Tooltip("If version is null or empty will auto set newest package version by date")]
        public string dlcVersion;
    }

    public class PackageInfo
    {
        public string packageName;
        public string packageVersion;
        public string packageVersionEncoded;
        public long packageSize;
    }

    public class AppConfig
    {
        [Serializable]
        public class SemanticRule
        {
            public bool MAJOR = true;
            public bool MINOR = true;
            public bool PATCH = false;
        }

        public string PLATFORM;            // 平台
        public string PRODUCT_NAME;        // 產品名稱
        public string APP_VERSION;         // 主程式版本
        public SemanticRule SEMANTIC_RULE; // 主程式版號規則
    }

    public class PatchConfig
    {
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