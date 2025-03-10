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

        byte[] DecryptData(WebDecryptFileInfo fileInfo);
    }

    /// <summary>
    /// 統一接口
    /// </summary>
    public abstract class DecryptionServices : IDecryptionServices, IWebDecryptionServices, IDecryptStream, IDecryptData
    {
        #region OxGFrame Implements
        public abstract byte[] DecryptData(DecryptFileInfo fileInfo);

        public abstract byte[] DecryptData(WebDecryptFileInfo fileInfo);

        public abstract Stream DecryptStream(DecryptFileInfo fileInfo);
        #endregion

        public virtual uint GetManagedReadBufferSize()
        {
            return 1024;
        }

        #region IDecryptionServices
        public abstract DecryptResult LoadAssetBundle(DecryptFileInfo fileInfo);

        public abstract DecryptResult LoadAssetBundleAsync(DecryptFileInfo fileInfo);

        public abstract byte[] ReadFileData(DecryptFileInfo fileInfo);

        public abstract string ReadFileText(DecryptFileInfo fileInfo);
        #endregion

        #region IWebDecryptionServices
        public abstract WebDecryptResult LoadAssetBundle(WebDecryptFileInfo fileInfo);
        #endregion
    }

    public class NoneDecryption : DecryptionServices
    {
        #region OxGFrame Implements
        public override byte[] DecryptData(DecryptFileInfo fileInfo)
        {
            string filePath = fileInfo.FileLoadPath;
            if (File.Exists(filePath) == false)
                return null;
            return File.ReadAllBytes(filePath);
        }

        public override byte[] DecryptData(WebDecryptFileInfo fileInfo)
        {
            return fileInfo.FileData;
        }

        public override Stream DecryptStream(DecryptFileInfo fileInfo)
        {
            string filePath = fileInfo.FileLoadPath;
            var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
            return fs;
        }
        #endregion

        #region IDecryptionServices
        public override DecryptResult LoadAssetBundle(DecryptFileInfo fileInfo)
        {
            DecryptResult result = new DecryptResult
            {
                ManagedStream = this.DecryptStream(fileInfo),
            };
            result.Result = AssetBundle.LoadFromStream(result.ManagedStream, fileInfo.FileLoadCRC, GetManagedReadBufferSize());
            return result;
        }

        public override DecryptResult LoadAssetBundleAsync(DecryptFileInfo fileInfo)
        {
            DecryptResult result = new DecryptResult
            {
                ManagedStream = this.DecryptStream(fileInfo),
            };
            result.CreateRequest = AssetBundle.LoadFromStreamAsync(result.ManagedStream, fileInfo.FileLoadCRC, GetManagedReadBufferSize());
            return result;
        }

        public override byte[] ReadFileData(DecryptFileInfo fileInfo)
        {
            return this.DecryptData(fileInfo);
        }

        public override string ReadFileText(DecryptFileInfo fileInfo)
        {
            return System.Text.Encoding.UTF8.GetString(this.DecryptData(fileInfo));
        }
        #endregion

        #region IWebDecryptionServices
        public override WebDecryptResult LoadAssetBundle(WebDecryptFileInfo fileInfo)
        {
            WebDecryptResult result = new WebDecryptResult();
            result.Result = AssetBundle.LoadFromMemory(this.DecryptData(fileInfo), fileInfo.FileLoadCRC);
            return result;
        }
        #endregion
    }

    #region Offset
    public class OffsetDecryption : DecryptionServices
    {
        #region OxGFrame Implements
        public override byte[] DecryptData(DecryptFileInfo fileInfo)
        {
            OxGFrame.AssetLoader.Utility.SecureMemory.SecuredString[] decryptArgs = BundleConfig.decryptArgs;
            int dummySize = Convert.ToInt32(decryptArgs[1].Decrypt());
            string filePath = fileInfo.FileLoadPath;
            if (File.Exists(filePath) == false)
                return null;
            return FileCryptogram.Offset.OffsetDecryptBytes(filePath, dummySize);
        }

        public override byte[] DecryptData(WebDecryptFileInfo fileInfo)
        {
            OxGFrame.AssetLoader.Utility.SecureMemory.SecuredString[] decryptArgs = BundleConfig.decryptArgs;
            int dummySize = Convert.ToInt32(decryptArgs[1].Decrypt());
            if (FileCryptogram.Offset.OffsetDecryptBytes(ref fileInfo.FileData, dummySize))
                return fileInfo.FileData;
            return null;
        }

        public override Stream DecryptStream(DecryptFileInfo fileInfo)
        {
            OxGFrame.AssetLoader.Utility.SecureMemory.SecuredString[] decryptArgs = BundleConfig.decryptArgs;
            int dummySize = Convert.ToInt32(decryptArgs[1].Decrypt());
            string filePath = fileInfo.FileLoadPath;
            return FileCryptogram.Offset.OffsetDecryptStream(filePath, dummySize);
        }
        #endregion

        #region IDecryptionServices
        public override DecryptResult LoadAssetBundle(DecryptFileInfo fileInfo)
        {
            DecryptResult result = new DecryptResult
            {
                ManagedStream = this.DecryptStream(fileInfo),
            };
            result.Result = AssetBundle.LoadFromStream(result.ManagedStream, fileInfo.FileLoadCRC, GetManagedReadBufferSize());
            return result;
        }

        public override DecryptResult LoadAssetBundleAsync(DecryptFileInfo fileInfo)
        {
            DecryptResult result = new DecryptResult
            {
                ManagedStream = this.DecryptStream(fileInfo),
            };
            result.CreateRequest = AssetBundle.LoadFromStreamAsync(result.ManagedStream, fileInfo.FileLoadCRC, GetManagedReadBufferSize());
            return result;
        }

        public override byte[] ReadFileData(DecryptFileInfo fileInfo)
        {
            return this.DecryptData(fileInfo);
        }

        public override string ReadFileText(DecryptFileInfo fileInfo)
        {
            return System.Text.Encoding.UTF8.GetString(this.DecryptData(fileInfo));
        }
        #endregion

        #region IWebDecryptionServices
        public override WebDecryptResult LoadAssetBundle(WebDecryptFileInfo fileInfo)
        {
            WebDecryptResult result = new WebDecryptResult();
            result.Result = AssetBundle.LoadFromMemory(this.DecryptData(fileInfo), fileInfo.FileLoadCRC);
            return result;
        }
        #endregion
    }
    #endregion

    #region Xor
    public class XorDecryption : DecryptionServices
    {
        #region OxGFrame Implements
        public override byte[] DecryptData(DecryptFileInfo fileInfo)
        {
            OxGFrame.AssetLoader.Utility.SecureMemory.SecuredString[] decryptArgs = BundleConfig.decryptArgs;
            byte xorKey = Convert.ToByte(decryptArgs[1].Decrypt());
            string filePath = fileInfo.FileLoadPath;
            if (File.Exists(filePath) == false)
                return null;
            return FileCryptogram.XOR.XorDecryptBytes(filePath, xorKey);
        }

        public override byte[] DecryptData(WebDecryptFileInfo fileInfo)
        {
            OxGFrame.AssetLoader.Utility.SecureMemory.SecuredString[] decryptArgs = BundleConfig.decryptArgs;
            byte xorKey = Convert.ToByte(decryptArgs[1].Decrypt());
            if (FileCryptogram.XOR.XorDecryptBytes(fileInfo.FileData, xorKey))
                return fileInfo.FileData;
            return null;
        }

        public override Stream DecryptStream(DecryptFileInfo fileInfo)
        {
            OxGFrame.AssetLoader.Utility.SecureMemory.SecuredString[] decryptArgs = BundleConfig.decryptArgs;
            byte xorKey = Convert.ToByte(decryptArgs[1].Decrypt());
            string filePath = fileInfo.FileLoadPath;
            return FileCryptogram.XOR.XorDecryptStream(filePath, xorKey);
        }
        #endregion

        #region IDecryptionServices
        public override DecryptResult LoadAssetBundle(DecryptFileInfo fileInfo)
        {
            DecryptResult result = new DecryptResult
            {
                ManagedStream = this.DecryptStream(fileInfo),
            };
            result.Result = AssetBundle.LoadFromStream(result.ManagedStream, fileInfo.FileLoadCRC, GetManagedReadBufferSize());
            return result;
        }

        public override DecryptResult LoadAssetBundleAsync(DecryptFileInfo fileInfo)
        {
            DecryptResult result = new DecryptResult
            {
                ManagedStream = this.DecryptStream(fileInfo),
            };
            result.CreateRequest = AssetBundle.LoadFromStreamAsync(result.ManagedStream, fileInfo.FileLoadCRC, GetManagedReadBufferSize());
            return result;
        }

        public override byte[] ReadFileData(DecryptFileInfo fileInfo)
        {
            return this.DecryptData(fileInfo);
        }

        public override string ReadFileText(DecryptFileInfo fileInfo)
        {
            return System.Text.Encoding.UTF8.GetString(this.DecryptData(fileInfo));
        }
        #endregion

        #region IWebDecryptionServices
        public override WebDecryptResult LoadAssetBundle(WebDecryptFileInfo fileInfo)
        {
            WebDecryptResult result = new WebDecryptResult();
            result.Result = AssetBundle.LoadFromMemory(this.DecryptData(fileInfo), fileInfo.FileLoadCRC);
            return result;
        }
        #endregion
    }
    #endregion

    #region HT2Xor
    public class HT2XorDecryption : DecryptionServices
    {
        #region OxGFrame Implements
        public override byte[] DecryptData(DecryptFileInfo fileInfo)
        {
            OxGFrame.AssetLoader.Utility.SecureMemory.SecuredString[] decryptArgs = BundleConfig.decryptArgs;
            byte hXorkey = Convert.ToByte(decryptArgs[1].Decrypt());
            byte tXorkey = Convert.ToByte(decryptArgs[2].Decrypt());
            byte jXorKey = Convert.ToByte(decryptArgs[3].Decrypt());
            string filePath = fileInfo.FileLoadPath;
            if (File.Exists(filePath) == false)
                return null;
            return FileCryptogram.HT2XOR.HT2XorDecryptBytes(filePath, hXorkey, tXorkey, jXorKey);
        }

        public override byte[] DecryptData(WebDecryptFileInfo fileInfo)
        {
            OxGFrame.AssetLoader.Utility.SecureMemory.SecuredString[] decryptArgs = BundleConfig.decryptArgs;
            byte hXorkey = Convert.ToByte(decryptArgs[1].Decrypt());
            byte tXorkey = Convert.ToByte(decryptArgs[2].Decrypt());
            byte jXorKey = Convert.ToByte(decryptArgs[3].Decrypt());
            if (FileCryptogram.HT2XOR.HT2XorDecryptBytes(fileInfo.FileData, hXorkey, tXorkey, jXorKey))
                return fileInfo.FileData;
            return null;
        }

        public override Stream DecryptStream(DecryptFileInfo fileInfo)
        {
            OxGFrame.AssetLoader.Utility.SecureMemory.SecuredString[] decryptArgs = BundleConfig.decryptArgs;
            byte hXorkey = Convert.ToByte(decryptArgs[1].Decrypt());
            byte tXorkey = Convert.ToByte(decryptArgs[2].Decrypt());
            byte jXorKey = Convert.ToByte(decryptArgs[3].Decrypt());
            string filePath = fileInfo.FileLoadPath;
            return FileCryptogram.HT2XOR.HT2XorDecryptStream(filePath, hXorkey, tXorkey, jXorKey);
        }
        #endregion

        #region IDecryptionServices
        public override DecryptResult LoadAssetBundle(DecryptFileInfo fileInfo)
        {
            DecryptResult result = new DecryptResult
            {
                ManagedStream = this.DecryptStream(fileInfo),
            };
            result.Result = AssetBundle.LoadFromStream(result.ManagedStream, fileInfo.FileLoadCRC, GetManagedReadBufferSize());
            return result;
        }

        public override DecryptResult LoadAssetBundleAsync(DecryptFileInfo fileInfo)
        {
            DecryptResult result = new DecryptResult
            {
                ManagedStream = this.DecryptStream(fileInfo),
            };
            result.CreateRequest = AssetBundle.LoadFromStreamAsync(result.ManagedStream, fileInfo.FileLoadCRC, GetManagedReadBufferSize());
            return result;
        }

        public override byte[] ReadFileData(DecryptFileInfo fileInfo)
        {
            return this.DecryptData(fileInfo);
        }

        public override string ReadFileText(DecryptFileInfo fileInfo)
        {
            return System.Text.Encoding.UTF8.GetString(this.DecryptData(fileInfo));
        }
        #endregion

        #region IWebDecryptionServices
        public override WebDecryptResult LoadAssetBundle(WebDecryptFileInfo fileInfo)
        {
            WebDecryptResult result = new WebDecryptResult();
            result.Result = AssetBundle.LoadFromMemory(this.DecryptData(fileInfo), fileInfo.FileLoadCRC);
            return result;
        }
        #endregion
    }
    #endregion

    #region HT2XorPlus
    public class HT2XorPlusDecryption : DecryptionServices
    {
        #region OxGFrame Implements
        public override byte[] DecryptData(DecryptFileInfo fileInfo)
        {
            OxGFrame.AssetLoader.Utility.SecureMemory.SecuredString[] decryptArgs = BundleConfig.decryptArgs;
            byte hXorkey = Convert.ToByte(decryptArgs[1].Decrypt());
            byte tXorkey = Convert.ToByte(decryptArgs[2].Decrypt());
            byte j1XorKey = Convert.ToByte(decryptArgs[3].Decrypt());
            byte j2XorKey = Convert.ToByte(decryptArgs[4].Decrypt());
            string filePath = fileInfo.FileLoadPath;
            if (File.Exists(filePath) == false)
                return null;
            return FileCryptogram.HT2XORPlus.HT2XorPlusDecryptBytes(filePath, hXorkey, tXorkey, j1XorKey, j2XorKey);
        }

        public override byte[] DecryptData(WebDecryptFileInfo fileInfo)
        {
            OxGFrame.AssetLoader.Utility.SecureMemory.SecuredString[] decryptArgs = BundleConfig.decryptArgs;
            byte hXorkey = Convert.ToByte(decryptArgs[1].Decrypt());
            byte tXorkey = Convert.ToByte(decryptArgs[2].Decrypt());
            byte j1XorKey = Convert.ToByte(decryptArgs[3].Decrypt());
            byte j2XorKey = Convert.ToByte(decryptArgs[4].Decrypt());
            if (FileCryptogram.HT2XORPlus.HT2XorPlusDecryptBytes(fileInfo.FileData, hXorkey, tXorkey, j1XorKey, j2XorKey))
                return fileInfo.FileData;
            return null;
        }

        public override Stream DecryptStream(DecryptFileInfo fileInfo)
        {
            OxGFrame.AssetLoader.Utility.SecureMemory.SecuredString[] decryptArgs = BundleConfig.decryptArgs;
            byte hXorkey = Convert.ToByte(decryptArgs[1].Decrypt());
            byte tXorkey = Convert.ToByte(decryptArgs[2].Decrypt());
            byte j1XorKey = Convert.ToByte(decryptArgs[3].Decrypt());
            byte j2XorKey = Convert.ToByte(decryptArgs[4].Decrypt());
            string filePath = fileInfo.FileLoadPath;
            return FileCryptogram.HT2XORPlus.HT2XorPlusDecryptStream(filePath, hXorkey, tXorkey, j1XorKey, j2XorKey);
        }
        #endregion

        #region IDecryptionServices
        public override DecryptResult LoadAssetBundle(DecryptFileInfo fileInfo)
        {
            DecryptResult result = new DecryptResult
            {
                ManagedStream = this.DecryptStream(fileInfo),
            };
            result.Result = AssetBundle.LoadFromStream(result.ManagedStream, fileInfo.FileLoadCRC, GetManagedReadBufferSize());
            return result;
        }

        public override DecryptResult LoadAssetBundleAsync(DecryptFileInfo fileInfo)
        {
            DecryptResult result = new DecryptResult
            {
                ManagedStream = this.DecryptStream(fileInfo),
            };
            result.CreateRequest = AssetBundle.LoadFromStreamAsync(result.ManagedStream, fileInfo.FileLoadCRC, GetManagedReadBufferSize());
            return result;
        }

        public override byte[] ReadFileData(DecryptFileInfo fileInfo)
        {
            return this.DecryptData(fileInfo);
        }

        public override string ReadFileText(DecryptFileInfo fileInfo)
        {
            return System.Text.Encoding.UTF8.GetString(this.DecryptData(fileInfo));
        }
        #endregion

        #region IWebDecryptionServices
        public override WebDecryptResult LoadAssetBundle(WebDecryptFileInfo fileInfo)
        {
            WebDecryptResult result = new WebDecryptResult();
            result.Result = AssetBundle.LoadFromMemory(this.DecryptData(fileInfo), fileInfo.FileLoadCRC);
            return result;
        }
        #endregion
    }
    #endregion

    #region AES
    public class AesDecryption : DecryptionServices
    {
        #region OxGFrame Implements
        public override byte[] DecryptData(DecryptFileInfo fileInfo)
        {
            OxGFrame.AssetLoader.Utility.SecureMemory.SecuredString[] decryptArgs = BundleConfig.decryptArgs;
            string aesKey = decryptArgs[1].Decrypt();
            string aesIv = decryptArgs[2].Decrypt();
            string filePath = fileInfo.FileLoadPath;
            if (File.Exists(filePath) == false)
                return null;
            return FileCryptogram.AES.AesDecryptBytes(filePath, aesKey, aesIv);
        }

        public override byte[] DecryptData(WebDecryptFileInfo fileInfo)
        {
            OxGFrame.AssetLoader.Utility.SecureMemory.SecuredString[] decryptArgs = BundleConfig.decryptArgs;
            string aesKey = decryptArgs[1].Decrypt();
            string aesIv = decryptArgs[2].Decrypt();
            if (FileCryptogram.AES.AesDecryptBytes(fileInfo.FileData, aesKey, aesIv))
                return fileInfo.FileData;
            return null;
        }

        public override Stream DecryptStream(DecryptFileInfo fileInfo)
        {
            OxGFrame.AssetLoader.Utility.SecureMemory.SecuredString[] decryptArgs = BundleConfig.decryptArgs;
            string aesKey = decryptArgs[1].Decrypt();
            string aesIv = decryptArgs[2].Decrypt();
            string filePath = fileInfo.FileLoadPath;
            return FileCryptogram.AES.AesDecryptStream(filePath, aesKey, aesIv);
        }
        #endregion

        #region IDecryptionServices
        public override DecryptResult LoadAssetBundle(DecryptFileInfo fileInfo)
        {
            DecryptResult result = new DecryptResult
            {
                ManagedStream = this.DecryptStream(fileInfo),
            };
            result.Result = AssetBundle.LoadFromStream(result.ManagedStream, fileInfo.FileLoadCRC, GetManagedReadBufferSize());
            return result;
        }

        public override DecryptResult LoadAssetBundleAsync(DecryptFileInfo fileInfo)
        {
            DecryptResult result = new DecryptResult
            {
                ManagedStream = this.DecryptStream(fileInfo),
            };
            result.CreateRequest = AssetBundle.LoadFromStreamAsync(result.ManagedStream, fileInfo.FileLoadCRC, GetManagedReadBufferSize());
            return result;
        }

        public override byte[] ReadFileData(DecryptFileInfo fileInfo)
        {
            return this.DecryptData(fileInfo);
        }

        public override string ReadFileText(DecryptFileInfo fileInfo)
        {
            return System.Text.Encoding.UTF8.GetString(this.DecryptData(fileInfo));
        }
        #endregion

        #region IWebDecryptionServices
        public override WebDecryptResult LoadAssetBundle(WebDecryptFileInfo fileInfo)
        {
            WebDecryptResult result = new WebDecryptResult();
            result.Result = AssetBundle.LoadFromMemory(this.DecryptData(fileInfo), fileInfo.FileLoadCRC);
            return result;
        }
        #endregion
    }
    #endregion
}