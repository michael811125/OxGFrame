using System;
using System.IO;
using UnityEngine;
using YooAsset;

namespace OxGFrame.AssetLoader.Bundle
{
    public interface IDecryptStream
    {
        Stream DecryptStream(DecryptFileInfo fileInfo);
    }

    public interface IDecryptData
    {
        byte[] DecryptData(DecryptFileInfo fileInfo);
    }

    public class NoneDecryption : IDecryptionServices, IDecryptStream, IDecryptData
    {
        #region OxGFrame Implements
        public byte[] DecryptData(DecryptFileInfo fileInfo)
        {
            string filePath = fileInfo.FileLoadPath;
            if (File.Exists(filePath) == false)
                return null;
            return File.ReadAllBytes(filePath);
        }

        public Stream DecryptStream(DecryptFileInfo fileInfo)
        {
            string filePath = fileInfo.FileLoadPath;
            var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
            return fs;
        }
        #endregion

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

        public byte[] LoadRawFileData(DecryptFileInfo fileInfo)
        {
            return this.DecryptData(fileInfo);
        }
    }

    #region Offset
    public class OffsetDecryption : IDecryptionServices, IDecryptStream, IDecryptData
    {
        #region OxGFrame Implements
        public byte[] DecryptData(DecryptFileInfo fileInfo)
        {
            OxGFrame.AssetLoader.Utility.SecureMemory.SecureString[] decryptArgs = BundleConfig.decryptArgs;
            string filePath = fileInfo.FileLoadPath;
            int dummySize = Convert.ToInt32(decryptArgs[1].Decrypt());
            if (File.Exists(filePath) == false)
                return null;
            return FileCryptogram.Offset.OffsetDecryptBytes(filePath, dummySize);
        }

        public Stream DecryptStream(DecryptFileInfo fileInfo)
        {
            OxGFrame.AssetLoader.Utility.SecureMemory.SecureString[] decryptArgs = BundleConfig.decryptArgs;
            string filePath = fileInfo.FileLoadPath;
            int dummySize = Convert.ToInt32(decryptArgs[1].Decrypt());
            return FileCryptogram.Offset.OffsetDecryptStream(filePath, dummySize);
        }
        #endregion

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

        public byte[] LoadRawFileData(DecryptFileInfo fileInfo)
        {
            return this.DecryptData(fileInfo);
        }
    }
    #endregion

    #region Xor
    public class XorDecryption : IDecryptionServices, IDecryptStream, IDecryptData
    {
        #region OxGFrame Implements
        public byte[] DecryptData(DecryptFileInfo fileInfo)
        {
            OxGFrame.AssetLoader.Utility.SecureMemory.SecureString[] decryptArgs = BundleConfig.decryptArgs;
            string filePath = fileInfo.FileLoadPath;
            byte xorKey = Convert.ToByte(decryptArgs[1].Decrypt());
            if (File.Exists(filePath) == false)
                return null;
            return FileCryptogram.XOR.XorDecryptBytes(filePath, xorKey);
        }

        public Stream DecryptStream(DecryptFileInfo fileInfo)
        {
            OxGFrame.AssetLoader.Utility.SecureMemory.SecureString[] decryptArgs = BundleConfig.decryptArgs;
            string filePath = fileInfo.FileLoadPath;
            byte xorKey = Convert.ToByte(decryptArgs[1].Decrypt());
            return FileCryptogram.XOR.XorDecryptStream(filePath, xorKey);
        }
        #endregion

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

        public byte[] LoadRawFileData(DecryptFileInfo fileInfo)
        {
            return this.DecryptData(fileInfo);
        }
    }
    #endregion

    #region HT2Xor
    public class HT2XorDecryption : IDecryptionServices, IDecryptStream, IDecryptData
    {
        #region OxGFrame Implements
        public byte[] DecryptData(DecryptFileInfo fileInfo)
        {
            OxGFrame.AssetLoader.Utility.SecureMemory.SecureString[] decryptArgs = BundleConfig.decryptArgs;
            string filePath = fileInfo.FileLoadPath;
            byte hXorkey = Convert.ToByte(decryptArgs[1].Decrypt());
            byte tXorkey = Convert.ToByte(decryptArgs[2].Decrypt());
            byte jXorKey = Convert.ToByte(decryptArgs[3].Decrypt());
            if (File.Exists(filePath) == false)
                return null;
            return FileCryptogram.HT2XOR.HT2XorDecryptBytes(filePath, hXorkey, tXorkey, jXorKey);
        }

        public Stream DecryptStream(DecryptFileInfo fileInfo)
        {
            OxGFrame.AssetLoader.Utility.SecureMemory.SecureString[] decryptArgs = BundleConfig.decryptArgs;
            string filePath = fileInfo.FileLoadPath;
            byte hXorkey = Convert.ToByte(decryptArgs[1].Decrypt());
            byte tXorkey = Convert.ToByte(decryptArgs[2].Decrypt());
            byte jXorKey = Convert.ToByte(decryptArgs[3].Decrypt());
            return FileCryptogram.HT2XOR.HT2XorDecryptStream(filePath, hXorkey, tXorkey, jXorKey);
        }
        #endregion

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

        public byte[] LoadRawFileData(DecryptFileInfo fileInfo)
        {
            return this.DecryptData(fileInfo);
        }
    }
    #endregion

    #region HT2XorPlus
    public class HT2XorPlusDecryption : IDecryptionServices, IDecryptStream, IDecryptData
    {
        #region OxGFrame Implements
        public byte[] DecryptData(DecryptFileInfo fileInfo)
        {
            OxGFrame.AssetLoader.Utility.SecureMemory.SecureString[] decryptArgs = BundleConfig.decryptArgs;
            string filePath = fileInfo.FileLoadPath;
            byte hXorkey = Convert.ToByte(decryptArgs[1].Decrypt());
            byte tXorkey = Convert.ToByte(decryptArgs[2].Decrypt());
            byte j1XorKey = Convert.ToByte(decryptArgs[3].Decrypt());
            byte j2XorKey = Convert.ToByte(decryptArgs[4].Decrypt());
            if (File.Exists(filePath) == false)
                return null;
            return FileCryptogram.HT2XORPlus.HT2XorPlusDecryptBytes(filePath, hXorkey, tXorkey, j1XorKey, j2XorKey);
        }

        public Stream DecryptStream(DecryptFileInfo fileInfo)
        {
            OxGFrame.AssetLoader.Utility.SecureMemory.SecureString[] decryptArgs = BundleConfig.decryptArgs;
            string filePath = fileInfo.FileLoadPath;
            byte hXorkey = Convert.ToByte(decryptArgs[1].Decrypt());
            byte tXorkey = Convert.ToByte(decryptArgs[2].Decrypt());
            byte j1XorKey = Convert.ToByte(decryptArgs[3].Decrypt());
            byte j2XorKey = Convert.ToByte(decryptArgs[4].Decrypt());
            return FileCryptogram.HT2XORPlus.HT2XorPlusDecryptStream(filePath, hXorkey, tXorkey, j1XorKey, j2XorKey);
        }
        #endregion

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

        public byte[] LoadRawFileData(DecryptFileInfo fileInfo)
        {
            return this.DecryptData(fileInfo);
        }
    }
    #endregion

    #region AES
    public class AesDecryption : IDecryptionServices, IDecryptStream, IDecryptData
    {
        #region OxGFrame Implements
        public byte[] DecryptData(DecryptFileInfo fileInfo)
        {
            OxGFrame.AssetLoader.Utility.SecureMemory.SecureString[] decryptArgs = BundleConfig.decryptArgs;
            string filePath = fileInfo.FileLoadPath;
            string aesKey = decryptArgs[1].Decrypt();
            string aesIv = decryptArgs[2].Decrypt();
            if (File.Exists(filePath) == false)
                return null;
            return FileCryptogram.AES.AesDecryptBytes(filePath, aesKey, aesIv);
        }

        public Stream DecryptStream(DecryptFileInfo fileInfo)
        {
            OxGFrame.AssetLoader.Utility.SecureMemory.SecureString[] decryptArgs = BundleConfig.decryptArgs;
            string filePath = fileInfo.FileLoadPath;
            string aesKey = decryptArgs[1].Decrypt();
            string aesIv = decryptArgs[2].Decrypt();
            return FileCryptogram.AES.AesDecryptStream(filePath, aesKey, aesIv);
        }
        #endregion

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

        public byte[] LoadRawFileData(DecryptFileInfo fileInfo)
        {
            return this.DecryptData(fileInfo);
        }
    }
    #endregion
}