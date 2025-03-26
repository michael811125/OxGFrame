using OxGFrame.AssetLoader.Utility.SecureMemory;
using System;
using System.IO;
using UnityEngine;
using YooAsset;

namespace OxGFrame.AssetLoader.Bundle
{
    public interface IDecryptInitialize
    {
        bool CheckIsIntialized();

        bool Initialize();
    }

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
    public abstract class DecryptionServices : IDecryptionServices, IWebDecryptionServices, IDecryptStream, IDecryptData, IDecryptInitialize
    {
        /// <summary>
        /// 是否初始標記
        /// </summary>
        protected bool _isInitialized = false;

        #region OxGFrame Implements
        public bool CheckIsIntialized()
        {
            if (this._isInitialized)
                return true;

            this._isInitialized = this.Initialize();

            return this._isInitialized;
        }

        public abstract bool Initialize();

        public abstract byte[] DecryptData(DecryptFileInfo fileInfo);

        public abstract byte[] DecryptData(WebDecryptFileInfo fileInfo);

        public abstract Stream DecryptStream(DecryptFileInfo fileInfo);
        #endregion

        public virtual uint GetManagedReadBufferSize()
        {
            return 1024;
        }

        #region IDecryptionServices
        public DecryptResult LoadAssetBundle(DecryptFileInfo fileInfo)
        {
            DecryptResult result = new DecryptResult
            {
                ManagedStream = this.DecryptStream(fileInfo),
            };
            result.Result = AssetBundle.LoadFromStream(result.ManagedStream, fileInfo.FileLoadCRC, GetManagedReadBufferSize());
            return result;
        }

        public DecryptResult LoadAssetBundleAsync(DecryptFileInfo fileInfo)
        {
            DecryptResult result = new DecryptResult
            {
                ManagedStream = this.DecryptStream(fileInfo),
            };
            result.CreateRequest = AssetBundle.LoadFromStreamAsync(result.ManagedStream, fileInfo.FileLoadCRC, GetManagedReadBufferSize());
            return result;
        }

        public byte[] ReadFileData(DecryptFileInfo fileInfo)
        {
            return this.DecryptData(fileInfo);
        }

        public string ReadFileText(DecryptFileInfo fileInfo)
        {
            return System.Text.Encoding.UTF8.GetString(this.DecryptData(fileInfo));
        }
        #endregion

        #region IWebDecryptionServices
        public WebDecryptResult LoadAssetBundle(WebDecryptFileInfo fileInfo)
        {
            WebDecryptResult result = new WebDecryptResult();
            result.Result = AssetBundle.LoadFromMemory(this.DecryptData(fileInfo), fileInfo.FileLoadCRC);
            return result;
        }
        #endregion
    }

    public class NoneDecryption : DecryptionServices
    {
        #region OxGFrame Implements
        public override bool Initialize()
        {
            return true;
        }

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
    }

    #region Offset
    public class OffsetDecryption : DecryptionServices
    {
        protected int _dummySize;

        #region OxGFrame Implements
        public override bool Initialize()
        {
            try
            {
                SecuredString[] decryptArgs = BundleConfig.decryptArgs;
                this._dummySize = Convert.ToInt32(decryptArgs[1].Decrypt());
            }
            catch
            {
                return false;
            }

            return true;
        }

        public override byte[] DecryptData(DecryptFileInfo fileInfo)
        {
            if (this.CheckIsIntialized())
            {
                string filePath = fileInfo.FileLoadPath;
                if (File.Exists(filePath) == false)
                    return null;
                return FileCryptogram.Offset.DecryptBytes(filePath, this._dummySize);
            }
            return null;
        }

        public override byte[] DecryptData(WebDecryptFileInfo fileInfo)
        {
            if (this.CheckIsIntialized())
            {
                if (FileCryptogram.Offset.DecryptBytes(ref fileInfo.FileData, this._dummySize))
                    return fileInfo.FileData;
            }
            return null;
        }

        public override Stream DecryptStream(DecryptFileInfo fileInfo)
        {
            if (this.CheckIsIntialized())
            {
                string filePath = fileInfo.FileLoadPath;
                return FileCryptogram.Offset.DecryptStream(filePath, this._dummySize);
            }
            return null;
        }
        #endregion
    }
    #endregion

    #region Xor
    public class XorDecryption : DecryptionServices
    {
        protected byte _key;

        #region OxGFrame Implements
        public override bool Initialize()
        {
            try
            {
                SecuredString[] decryptArgs = BundleConfig.decryptArgs;
                this._key = Convert.ToByte(decryptArgs[1].Decrypt());
            }
            catch
            {
                return false;
            }

            return true;
        }

        public override byte[] DecryptData(DecryptFileInfo fileInfo)
        {
            if (this.CheckIsIntialized())
            {
                string filePath = fileInfo.FileLoadPath;
                if (File.Exists(filePath) == false)
                    return null;
                return FileCryptogram.XOR.DecryptBytes(filePath, this._key);
            }
            return null;
        }

        public override byte[] DecryptData(WebDecryptFileInfo fileInfo)
        {
            if (this.CheckIsIntialized())
            {
                if (FileCryptogram.XOR.DecryptBytes(fileInfo.FileData, this._key))
                    return fileInfo.FileData;
            }
            return null;
        }

        public override Stream DecryptStream(DecryptFileInfo fileInfo)
        {
            if (this.CheckIsIntialized())
            {
                string filePath = fileInfo.FileLoadPath;
                return FileCryptogram.XOR.DecryptStream(filePath, this._key);
            }
            return null;
        }
        #endregion
    }
    #endregion

    #region HT2Xor
    public class HT2XorDecryption : DecryptionServices
    {
        protected byte _hkey;
        protected byte _tKey;
        protected byte _jKey;

        #region OxGFrame Implements
        public override bool Initialize()
        {
            try
            {
                SecuredString[] decryptArgs = BundleConfig.decryptArgs;
                this._hkey = Convert.ToByte(decryptArgs[1].Decrypt());
                this._tKey = Convert.ToByte(decryptArgs[2].Decrypt());
                this._jKey = Convert.ToByte(decryptArgs[3].Decrypt());
            }
            catch
            {
                return false;
            }

            return true;
        }

        public override byte[] DecryptData(DecryptFileInfo fileInfo)
        {
            if (this.CheckIsIntialized())
            {
                string filePath = fileInfo.FileLoadPath;
                if (File.Exists(filePath) == false)
                    return null;
                return FileCryptogram.HT2XOR.DecryptBytes(filePath, this._hkey, this._tKey, this._jKey);
            }
            return null;
        }

        public override byte[] DecryptData(WebDecryptFileInfo fileInfo)
        {
            if (this.CheckIsIntialized())
            {
                if (FileCryptogram.HT2XOR.DecryptBytes(fileInfo.FileData, this._hkey, this._tKey, this._jKey))
                    return fileInfo.FileData;
            }
            return null;
        }

        public override Stream DecryptStream(DecryptFileInfo fileInfo)
        {
            if (this.CheckIsIntialized())
            {
                string filePath = fileInfo.FileLoadPath;
                return FileCryptogram.HT2XOR.DecryptStream(filePath, this._hkey, this._tKey, this._jKey);
            }
            return null;
        }
        #endregion
    }
    #endregion

    #region HT2XorPlus
    public class HT2XorPlusDecryption : DecryptionServices
    {
        protected byte _hKey;
        protected byte _tKey;
        protected byte _j1Key;
        protected byte _j2Key;

        #region OxGFrame Implements
        public override bool Initialize()
        {
            try
            {
                SecuredString[] decryptArgs = BundleConfig.decryptArgs;
                this._hKey = Convert.ToByte(decryptArgs[1].Decrypt());
                this._tKey = Convert.ToByte(decryptArgs[2].Decrypt());
                this._j1Key = Convert.ToByte(decryptArgs[3].Decrypt());
                this._j2Key = Convert.ToByte(decryptArgs[4].Decrypt());
            }
            catch
            {
                return false;
            }

            return true;
        }

        public override byte[] DecryptData(DecryptFileInfo fileInfo)
        {
            if (this.CheckIsIntialized())
            {
                string filePath = fileInfo.FileLoadPath;
                if (File.Exists(filePath) == false)
                    return null;
                return FileCryptogram.HT2XORPlus.DecryptBytes(filePath, this._hKey, this._tKey, this._j1Key, this._j2Key);
            }
            return null;
        }

        public override byte[] DecryptData(WebDecryptFileInfo fileInfo)
        {
            if (this.CheckIsIntialized())
            {
                if (FileCryptogram.HT2XORPlus.DecryptBytes(fileInfo.FileData, this._hKey, this._tKey, this._j1Key, this._j2Key))
                    return fileInfo.FileData;
            }
            return null;
        }

        public override Stream DecryptStream(DecryptFileInfo fileInfo)
        {
            if (this.CheckIsIntialized())
            {
                string filePath = fileInfo.FileLoadPath;
                return FileCryptogram.HT2XORPlus.DecryptStream(filePath, this._hKey, this._tKey, this._j1Key, this._j2Key);
            }
            return null;
        }
        #endregion
    }
    #endregion

    #region AES
    public class AesDecryption : DecryptionServices
    {
        protected string _key;
        protected string _iv;

        #region OxGFrame Implements
        public override bool Initialize()
        {
            try
            {
                SecuredString[] decryptArgs = BundleConfig.decryptArgs;
                this._key = decryptArgs[1].Decrypt();
                this._iv = decryptArgs[2].Decrypt();
            }
            catch
            {
                return false;
            }

            return true;
        }

        public override byte[] DecryptData(DecryptFileInfo fileInfo)
        {
            if (this.CheckIsIntialized())
            {
                string filePath = fileInfo.FileLoadPath;
                if (File.Exists(filePath) == false)
                    return null;
                return FileCryptogram.AES.DecryptBytes(filePath, this._key, this._iv);
            }
            return null;
        }

        public override byte[] DecryptData(WebDecryptFileInfo fileInfo)
        {
            if (this.CheckIsIntialized())
            {
                if (FileCryptogram.AES.DecryptBytes(ref fileInfo.FileData, this._key, this._iv))
                    return fileInfo.FileData;
            }
            return null;
        }

        public override Stream DecryptStream(DecryptFileInfo fileInfo)
        {
            if (this.CheckIsIntialized())
            {
                string filePath = fileInfo.FileLoadPath;
                return FileCryptogram.AES.DecryptStream(filePath, this._key, this._iv);
            }
            return null;
        }
        #endregion
    }
    #endregion

    #region ChaCha20
    public class ChaCha20Decryption : DecryptionServices
    {
        protected string _key;
        protected string _nonce;
        protected uint _counter;

        #region OxGFrame Implements
        public override bool Initialize()
        {
            try
            {
                SecuredString[] decryptArgs = BundleConfig.decryptArgs;
                this._key = decryptArgs[1].Decrypt();
                this._nonce = decryptArgs[2].Decrypt();
                this._counter = Convert.ToUInt32(decryptArgs[3].Decrypt());
            }
            catch
            {
                return false;
            }

            return true;
        }

        public override byte[] DecryptData(DecryptFileInfo fileInfo)
        {
            if (this.CheckIsIntialized())
            {
                string filePath = fileInfo.FileLoadPath;
                if (File.Exists(filePath) == false)
                    return null;
                return FileCryptogram.ChaCha20.DecryptBytes(filePath, this._key, this._nonce, this._counter);
            }
            return null;
        }

        public override byte[] DecryptData(WebDecryptFileInfo fileInfo)
        {
            if (this.CheckIsIntialized())
            {
                if (FileCryptogram.ChaCha20.DecryptBytes(ref fileInfo.FileData, this._key, this._nonce, this._counter))
                    return fileInfo.FileData;
            }
            return null;
        }

        public override Stream DecryptStream(DecryptFileInfo fileInfo)
        {
            if (this.CheckIsIntialized())
            {
                string filePath = fileInfo.FileLoadPath;
                return FileCryptogram.ChaCha20.DecryptStream(filePath, this._key, this._nonce, this._counter);
            }
            return null;
        }
        #endregion
    }
    #endregion

    #region XXTEA
    public class XXTEADecryption : DecryptionServices
    {
        protected string _key;

        #region OxGFrame Implements
        public override bool Initialize()
        {
            try
            {
                SecuredString[] decryptArgs = BundleConfig.decryptArgs;
                this._key = decryptArgs[1].Decrypt();
            }
            catch
            {
                return false;
            }

            return true;
        }

        public override byte[] DecryptData(DecryptFileInfo fileInfo)
        {
            if (this.CheckIsIntialized())
            {
                string filePath = fileInfo.FileLoadPath;
                if (File.Exists(filePath) == false)
                    return null;
                return FileCryptogram.XXTEA.DecryptBytes(filePath, this._key);
            }
            return null;
        }

        public override byte[] DecryptData(WebDecryptFileInfo fileInfo)
        {
            if (this.CheckIsIntialized())
            {
                if (FileCryptogram.XXTEA.DecryptBytes(ref fileInfo.FileData, this._key))
                    return fileInfo.FileData;
            }
            return null;
        }

        public override Stream DecryptStream(DecryptFileInfo fileInfo)
        {
            if (this.CheckIsIntialized())
            {
                string filePath = fileInfo.FileLoadPath;
                return FileCryptogram.XXTEA.DecryptStream(filePath, this._key);
            }
            return null;
        }
        #endregion
    }
    #endregion

    #region OffsetXOR
    public class OffsetXorDecryption : DecryptionServices
    {
        protected byte _key;
        protected int _dummySize;

        #region OxGFrame Implements
        public override bool Initialize()
        {
            try
            {
                SecuredString[] decryptArgs = BundleConfig.decryptArgs;
                this._key = Convert.ToByte(decryptArgs[1].Decrypt());
                this._dummySize = this._dummySize = Convert.ToInt32(decryptArgs[2].Decrypt());
            }
            catch
            {
                return false;
            }

            return true;
        }

        public override byte[] DecryptData(DecryptFileInfo fileInfo)
        {
            if (this.CheckIsIntialized())
            {
                string filePath = fileInfo.FileLoadPath;
                if (File.Exists(filePath) == false)
                    return null;
                return FileCryptogram.OffsetXOR.DecryptBytes(filePath, this._key, this._dummySize);
            }
            return null;
        }

        public override byte[] DecryptData(WebDecryptFileInfo fileInfo)
        {
            if (this.CheckIsIntialized())
            {
                if (FileCryptogram.OffsetXOR.DecryptBytes(fileInfo.FileData, this._key, this._dummySize))
                    return fileInfo.FileData;
            }
            return null;
        }

        public override Stream DecryptStream(DecryptFileInfo fileInfo)
        {
            if (this.CheckIsIntialized())
            {
                string filePath = fileInfo.FileLoadPath;
                return FileCryptogram.OffsetXOR.DecryptStream(filePath, this._key, this._dummySize);
            }
            return null;
        }
        #endregion
    }
    #endregion
}