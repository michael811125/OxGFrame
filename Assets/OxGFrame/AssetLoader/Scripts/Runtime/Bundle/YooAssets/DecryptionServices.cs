using System;
using System.IO;
using UnityEngine;
using YooAsset;

namespace OxGFrame.AssetLoader.Bundle
{
    internal interface IDecryptStream
    {
        Stream DecryptStream(DecryptFileInfo fileInfo);
    }

    public class NoneDecryption : IDecryptionServices, IDecryptStream
    {
        public Stream DecryptStream(DecryptFileInfo fileInfo)
        {
            string filePath = fileInfo.FileLoadPath;
            var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
            return fs;
        }

        public uint GetManagedReadBufferSize()
        {
            return 1024;
        }

        public AssetBundle LoadAssetBundle(DecryptFileInfo fileInfo, out Stream managedStream)
        {
            managedStream = this.DecryptStream(fileInfo);
            return AssetBundle.LoadFromStream(managedStream, fileInfo.ConentCRC, GetManagedReadBufferSize());
        }

        public AssetBundleCreateRequest LoadAssetBundleAsync(DecryptFileInfo fileInfo, out Stream managedStream)
        {
            managedStream = this.DecryptStream(fileInfo);
            return AssetBundle.LoadFromStreamAsync(managedStream, fileInfo.ConentCRC, GetManagedReadBufferSize());
        }
    }

    public class OffsetDecryption : IDecryptionServices, IDecryptStream
    {
        public Stream DecryptStream(DecryptFileInfo fileInfo)
        {
            OxGFrame.AssetLoader.Utility.SecureMemory.SecureString[] decryptArgs = BundleConfig.decryptArgs;
            string filePath = fileInfo.FileLoadPath;
            int dummySize = Convert.ToInt32(decryptArgs[1].Decrypt());
            return FileCryptogram.Offset.OffsetDecryptStream(filePath, dummySize);
        }

        public uint GetManagedReadBufferSize()
        {
            return 1024;
        }

        public AssetBundle LoadAssetBundle(DecryptFileInfo fileInfo, out Stream managedStream)
        {
            managedStream = this.DecryptStream(fileInfo);
            return AssetBundle.LoadFromStream(managedStream, fileInfo.ConentCRC, GetManagedReadBufferSize());
        }

        public AssetBundleCreateRequest LoadAssetBundleAsync(DecryptFileInfo fileInfo, out Stream managedStream)
        {
            managedStream = this.DecryptStream(fileInfo);
            return AssetBundle.LoadFromStreamAsync(managedStream, fileInfo.ConentCRC, GetManagedReadBufferSize());
        }
    }

    public class XorDecryption : IDecryptionServices, IDecryptStream
    {
        public Stream DecryptStream(DecryptFileInfo fileInfo)
        {
            OxGFrame.AssetLoader.Utility.SecureMemory.SecureString[] decryptArgs = BundleConfig.decryptArgs;
            string filePath = fileInfo.FileLoadPath;
            byte xorKey = Convert.ToByte(decryptArgs[1].Decrypt());
            return FileCryptogram.XOR.XorDecryptStream(filePath, xorKey);
        }

        public uint GetManagedReadBufferSize()
        {
            return 1024;
        }

        public AssetBundle LoadAssetBundle(DecryptFileInfo fileInfo, out Stream managedStream)
        {
            managedStream = this.DecryptStream(fileInfo);
            return AssetBundle.LoadFromStream(managedStream, fileInfo.ConentCRC, GetManagedReadBufferSize());
        }

        public AssetBundleCreateRequest LoadAssetBundleAsync(DecryptFileInfo fileInfo, out Stream managedStream)
        {
            managedStream = this.DecryptStream(fileInfo);
            return AssetBundle.LoadFromStreamAsync(managedStream, fileInfo.ConentCRC, GetManagedReadBufferSize());
        }
    }

    public class HT2XorDecryption : IDecryptionServices, IDecryptStream
    {
        public Stream DecryptStream(DecryptFileInfo fileInfo)
        {
            OxGFrame.AssetLoader.Utility.SecureMemory.SecureString[] decryptArgs = BundleConfig.decryptArgs;
            string filePath = fileInfo.FileLoadPath;
            byte hXorkey = Convert.ToByte(decryptArgs[1].Decrypt());
            byte tXorkey = Convert.ToByte(decryptArgs[2].Decrypt());
            byte jXorKey = Convert.ToByte(decryptArgs[3].Decrypt());
            return FileCryptogram.HT2XOR.HT2XorDecryptStream(filePath, hXorkey, tXorkey, jXorKey);
        }

        public uint GetManagedReadBufferSize()
        {
            return 1024;
        }

        public AssetBundle LoadAssetBundle(DecryptFileInfo fileInfo, out Stream managedStream)
        {
            managedStream = this.DecryptStream(fileInfo);
            return AssetBundle.LoadFromStream(managedStream, fileInfo.ConentCRC, GetManagedReadBufferSize());
        }

        public AssetBundleCreateRequest LoadAssetBundleAsync(DecryptFileInfo fileInfo, out Stream managedStream)
        {
            managedStream = this.DecryptStream(fileInfo);
            return AssetBundle.LoadFromStreamAsync(managedStream, fileInfo.ConentCRC, GetManagedReadBufferSize());
        }
    }

    public class AesDecryption : IDecryptionServices, IDecryptStream
    {
        public Stream DecryptStream(DecryptFileInfo fileInfo)
        {
            OxGFrame.AssetLoader.Utility.SecureMemory.SecureString[] decryptArgs = BundleConfig.decryptArgs;
            string filePath = fileInfo.FileLoadPath;
            string aesKey = decryptArgs[1].Decrypt();
            string aesIv = decryptArgs[2].Decrypt();
            return FileCryptogram.AES.AesDecryptStream(filePath, aesKey, aesIv);
        }

        public uint GetManagedReadBufferSize()
        {
            return 1024;
        }

        public AssetBundle LoadAssetBundle(DecryptFileInfo fileInfo, out Stream managedStream)
        {
            managedStream = this.DecryptStream(fileInfo);
            return AssetBundle.LoadFromStream(managedStream, fileInfo.ConentCRC, GetManagedReadBufferSize());
        }

        public AssetBundleCreateRequest LoadAssetBundleAsync(DecryptFileInfo fileInfo, out Stream managedStream)
        {
            managedStream = this.DecryptStream(fileInfo);
            return AssetBundle.LoadFromStreamAsync(managedStream, fileInfo.ConentCRC, GetManagedReadBufferSize());
        }
    }
}
