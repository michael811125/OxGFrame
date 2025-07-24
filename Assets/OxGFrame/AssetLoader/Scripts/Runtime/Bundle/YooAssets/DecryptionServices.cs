using OxGFrame.AssetLoader.Utility;
using OxGFrame.AssetLoader.Utility.SecureMemory;
using System;
using System.IO;
using UnityEngine;
using YooAsset;

namespace OxGFrame.AssetLoader.Bundle
{
    /// <summary>
    /// 處理文件解密操作類型
    /// </summary>
    public enum FileOperationType
    {
        None,
        Custom,
        Bundle,
        Manifest
    }

    /// <summary>
    /// 解密初始接口
    /// </summary>
    public interface IDecryptInitialize
    {
        bool CheckIsIntialized();

        bool Initialize();
    }

    /// <summary>
    /// 解密文件流接口
    /// </summary>
    public interface IDecryptStream
    {
        Stream DecryptStream(DecryptFileInfo fileInfo);
    }

    /// <summary>
    /// 解密內存流接口
    /// </summary>
    public interface IDecryptData
    {
        byte[] DecryptData(DecryptFileInfo fileInfo);

        byte[] DecryptData(WebDecryptFileInfo fileInfo);
    }

    /// <summary>
    /// 解密服務統一接口
    /// </summary>
    public abstract class DecryptionServices :
        IManifestRestoreServices,
        IDecryptionServices,
        IWebDecryptionServices,
        IDecryptStream,
        IDecryptData,
        IDecryptInitialize
    {
        /// <summary>
        /// 是否初始標記
        /// </summary>
        protected bool _isInitialized = false;

        /// <summary>
        /// 解密處理種類
        /// </summary>
        protected FileOperationType _fileOperationType = FileOperationType.None;

        public DecryptionServices() { }

        public DecryptionServices(FileOperationType fileOperationType)
        {
            this._fileOperationType = fileOperationType;
        }

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

        public abstract byte[] DecryptData(byte[] fileData);

        public abstract Stream DecryptStream(DecryptFileInfo fileInfo);
        #endregion

        public virtual uint GetManagedReadBufferSize()
        {
            return BundleConfig.bundleLoadReadBufferSize;
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

        public DecryptResult LoadAssetBundleFallback(DecryptFileInfo fileInfo)
        {
            DecryptResult result = new DecryptResult();
            result.Result = AssetBundle.LoadFromMemory(this.DecryptData(fileInfo), fileInfo.FileLoadCRC);
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

        #region IManifestServices
        public byte[] RestoreManifest(byte[] fileData)
        {
            return this.DecryptData(fileData);
        }
        #endregion
    }

    #region Offset
    public class OffsetDecryption : DecryptionServices
    {
        private int _dummySize;

        public OffsetDecryption() { }

        public OffsetDecryption(int dummySize)
        {
            this._fileOperationType = FileOperationType.Custom;
            this._dummySize = dummySize;
        }

        public OffsetDecryption(FileOperationType fileOperationType) : base(fileOperationType) { }

        #region OxGFrame Implements
        public override bool Initialize()
        {
            try
            {
                switch (this._fileOperationType)
                {
                    case FileOperationType.None:
                        {
                            var cryptogramSettings = CryptogramUtility.CryptogramSettingSetup.GetCryptogramSetting();
                            if (cryptogramSettings != null)
                            {
                                this._dummySize = cryptogramSettings.dummySize;
                            }
                        }
                        break;

                    // Nothing to do
                    case FileOperationType.Custom:
                        break;

                    case FileOperationType.Bundle:
                        {
                            SecuredString[] decryptArgs = BundleConfig.bundleDecryptArgs;
                            this._dummySize = Convert.ToInt32(decryptArgs[1].Decrypt());
                        }
                        break;

                    case FileOperationType.Manifest:
                        {
                            SecuredString[] decryptArgs = BundleConfig.manifestDecryptArgs;
                            this._dummySize = Convert.ToInt32(decryptArgs[1].Decrypt());
                        }
                        break;
                }
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

        public override byte[] DecryptData(byte[] fileData)
        {
            if (this.CheckIsIntialized())
            {
                if (FileCryptogram.Offset.DecryptBytes(ref fileData, this._dummySize))
                    return fileData;
            }
            return null;
        }
        #endregion
    }
    #endregion

    #region Xor
    public class XorDecryption : DecryptionServices
    {
        private byte _key;

        public XorDecryption() { }

        public XorDecryption(byte key)
        {
            this._fileOperationType = FileOperationType.Custom;
            this._key = key;
        }

        public XorDecryption(FileOperationType fileOperationType) : base(fileOperationType) { }

        #region OxGFrame Implements
        public override bool Initialize()
        {
            try
            {
                switch (this._fileOperationType)
                {
                    case FileOperationType.None:
                        {
                            var cryptogramSettings = CryptogramUtility.CryptogramSettingSetup.GetCryptogramSetting();
                            if (cryptogramSettings != null)
                            {
                                this._key = cryptogramSettings.xorKey;
                            }
                        }
                        break;

                    // Nothing to do
                    case FileOperationType.Custom:
                        break;

                    case FileOperationType.Bundle:
                        {
                            SecuredString[] decryptArgs = BundleConfig.bundleDecryptArgs;
                            this._key = Convert.ToByte(decryptArgs[1].Decrypt());
                        }
                        break;

                    case FileOperationType.Manifest:
                        {
                            SecuredString[] decryptArgs = BundleConfig.manifestDecryptArgs;
                            this._key = Convert.ToByte(decryptArgs[1].Decrypt());
                        }
                        break;
                }
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

        public override byte[] DecryptData(byte[] fileData)
        {
            if (this.CheckIsIntialized())
            {
                if (FileCryptogram.XOR.DecryptBytes(fileData, this._key))
                    return fileData;
            }
            return null;
        }
        #endregion
    }
    #endregion

    #region HT2Xor
    public class HT2XorDecryption : DecryptionServices
    {
        private byte _hkey;
        private byte _tKey;
        private byte _jKey;

        public HT2XorDecryption() { }

        public HT2XorDecryption(byte hkey, byte tKey, byte jKey)
        {
            this._fileOperationType = FileOperationType.Custom;
            this._hkey = hkey;
            this._tKey = tKey;
            this._jKey = jKey;
        }

        public HT2XorDecryption(FileOperationType fileOperationType) : base(fileOperationType) { }

        #region OxGFrame Implements
        public override bool Initialize()
        {
            try
            {
                switch (this._fileOperationType)
                {
                    case FileOperationType.None:
                        {
                            var cryptogramSettings = CryptogramUtility.CryptogramSettingSetup.GetCryptogramSetting();
                            if (cryptogramSettings != null)
                            {
                                this._hkey = cryptogramSettings.hXorKey;
                                this._tKey = cryptogramSettings.tXorKey;
                                this._jKey = cryptogramSettings.jXorKey;
                            }
                        }
                        break;

                    // Nothing to do
                    case FileOperationType.Custom:
                        break;

                    case FileOperationType.Bundle:
                        {
                            SecuredString[] decryptArgs = BundleConfig.bundleDecryptArgs;
                            this._hkey = Convert.ToByte(decryptArgs[1].Decrypt());
                            this._tKey = Convert.ToByte(decryptArgs[2].Decrypt());
                            this._jKey = Convert.ToByte(decryptArgs[3].Decrypt());
                        }
                        break;

                    case FileOperationType.Manifest:
                        {
                            SecuredString[] decryptArgs = BundleConfig.manifestDecryptArgs;
                            this._hkey = Convert.ToByte(decryptArgs[1].Decrypt());
                            this._tKey = Convert.ToByte(decryptArgs[2].Decrypt());
                            this._jKey = Convert.ToByte(decryptArgs[3].Decrypt());
                        }
                        break;
                }
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

        public override byte[] DecryptData(byte[] fileData)
        {
            if (this.CheckIsIntialized())
            {
                if (FileCryptogram.HT2XOR.DecryptBytes(fileData, this._hkey, this._tKey, this._jKey))
                    return fileData;
            }
            return null;
        }
        #endregion
    }
    #endregion

    #region HT2XorPlus
    public class HT2XorPlusDecryption : DecryptionServices
    {
        private byte _hKey;
        private byte _tKey;
        private byte _j1Key;
        private byte _j2Key;

        public HT2XorPlusDecryption() { }

        public HT2XorPlusDecryption(byte hKey, byte tKey, byte j1Key, byte j2Key)
        {
            this._fileOperationType = FileOperationType.Custom;
            this._hKey = hKey;
            this._tKey = tKey;
            this._j1Key = j1Key;
            this._j2Key = j2Key;
        }

        public HT2XorPlusDecryption(FileOperationType fileOperationType) : base(fileOperationType) { }

        #region OxGFrame Implements
        public override bool Initialize()
        {
            try
            {
                switch (this._fileOperationType)
                {
                    case FileOperationType.None:
                        {
                            var cryptogramSettings = CryptogramUtility.CryptogramSettingSetup.GetCryptogramSetting();
                            if (cryptogramSettings != null)
                            {
                                this._hKey = cryptogramSettings.hXorPlusKey;
                                this._tKey = cryptogramSettings.tXorPlusKey;
                                this._j1Key = cryptogramSettings.j1XorPlusKey;
                                this._j2Key = cryptogramSettings.j2XorPlusKey;
                            }
                        }
                        break;

                    // Nothing to do
                    case FileOperationType.Custom:
                        break;

                    case FileOperationType.Bundle:
                        {
                            SecuredString[] decryptArgs = BundleConfig.bundleDecryptArgs;
                            this._hKey = Convert.ToByte(decryptArgs[1].Decrypt());
                            this._tKey = Convert.ToByte(decryptArgs[2].Decrypt());
                            this._j1Key = Convert.ToByte(decryptArgs[3].Decrypt());
                            this._j2Key = Convert.ToByte(decryptArgs[4].Decrypt());
                        }
                        break;

                    case FileOperationType.Manifest:
                        {
                            SecuredString[] decryptArgs = BundleConfig.manifestDecryptArgs;
                            this._hKey = Convert.ToByte(decryptArgs[1].Decrypt());
                            this._tKey = Convert.ToByte(decryptArgs[2].Decrypt());
                            this._j1Key = Convert.ToByte(decryptArgs[3].Decrypt());
                            this._j2Key = Convert.ToByte(decryptArgs[4].Decrypt());
                        }
                        break;
                }
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

        public override byte[] DecryptData(byte[] fileData)
        {
            if (this.CheckIsIntialized())
            {
                if (FileCryptogram.HT2XORPlus.DecryptBytes(fileData, this._hKey, this._tKey, this._j1Key, this._j2Key))
                    return fileData;
            }
            return null;
        }
        #endregion
    }
    #endregion

    #region AES
    public class AesDecryption : DecryptionServices
    {
        private string _key;
        private string _iv;

        public AesDecryption() { }

        public AesDecryption(string key, string iv)
        {
            this._fileOperationType = FileOperationType.Custom;
            this._key = key;
            this._iv = iv;
        }

        public AesDecryption(FileOperationType fileOperationType) : base(fileOperationType) { }

        #region OxGFrame Implements
        public override bool Initialize()
        {
            try
            {
                switch (this._fileOperationType)
                {
                    case FileOperationType.None:
                        {
                            var cryptogramSettings = CryptogramUtility.CryptogramSettingSetup.GetCryptogramSetting();
                            if (cryptogramSettings != null)
                            {
                                this._key = cryptogramSettings.aesKey;
                                this._iv = cryptogramSettings.aesIv;
                            }
                        }
                        break;

                    // Nothing to do
                    case FileOperationType.Custom:
                        break;

                    case FileOperationType.Bundle:
                        {
                            SecuredString[] decryptArgs = BundleConfig.bundleDecryptArgs;
                            this._key = decryptArgs[1].Decrypt();
                            this._iv = decryptArgs[2].Decrypt();
                        }
                        break;

                    case FileOperationType.Manifest:
                        {
                            SecuredString[] decryptArgs = BundleConfig.manifestDecryptArgs;
                            this._key = decryptArgs[1].Decrypt();
                            this._iv = decryptArgs[2].Decrypt();
                        }
                        break;
                }
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

        public override byte[] DecryptData(byte[] fileData)
        {
            if (this.CheckIsIntialized())
            {
                if (FileCryptogram.AES.DecryptBytes(ref fileData, this._key, this._iv))
                    return fileData;
            }
            return null;
        }
        #endregion
    }
    #endregion

    #region ChaCha20
    public class ChaCha20Decryption : DecryptionServices
    {
        private string _key;
        private string _nonce;
        private uint _counter;

        public ChaCha20Decryption() { }

        public ChaCha20Decryption(string key, string nonce, uint counter)
        {
            this._fileOperationType = FileOperationType.Custom;
            this._key = key;
            this._nonce = nonce;
            this._counter = counter;
        }

        public ChaCha20Decryption(FileOperationType fileOperationType) : base(fileOperationType) { }

        #region OxGFrame Implements
        public override bool Initialize()
        {
            try
            {
                switch (this._fileOperationType)
                {
                    case FileOperationType.None:
                        {
                            var cryptogramSettings = CryptogramUtility.CryptogramSettingSetup.GetCryptogramSetting();
                            if (cryptogramSettings != null)
                            {
                                this._key = cryptogramSettings.chacha20Key;
                                this._nonce = cryptogramSettings.chacha20Nonce;
                                this._counter = cryptogramSettings.chacha20Counter;
                            }
                        }
                        break;

                    // Nothing to do
                    case FileOperationType.Custom:
                        break;

                    case FileOperationType.Bundle:
                        {
                            SecuredString[] decryptArgs = BundleConfig.bundleDecryptArgs;
                            this._key = decryptArgs[1].Decrypt();
                            this._nonce = decryptArgs[2].Decrypt();
                            this._counter = Convert.ToUInt32(decryptArgs[3].Decrypt());
                        }
                        break;

                    case FileOperationType.Manifest:
                        {
                            SecuredString[] decryptArgs = BundleConfig.manifestDecryptArgs;
                            this._key = decryptArgs[1].Decrypt();
                            this._nonce = decryptArgs[2].Decrypt();
                            this._counter = Convert.ToUInt32(decryptArgs[3].Decrypt());
                        }
                        break;
                }
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

        public override byte[] DecryptData(byte[] fileData)
        {
            if (this.CheckIsIntialized())
            {
                if (FileCryptogram.ChaCha20.DecryptBytes(ref fileData, this._key, this._nonce, this._counter))
                    return fileData;
            }
            return null;
        }
        #endregion
    }
    #endregion

    #region XXTEA
    public class XXTEADecryption : DecryptionServices
    {
        private string _key;

        public XXTEADecryption() { }

        public XXTEADecryption(string key)
        {
            this._fileOperationType = FileOperationType.Custom;
            this._key = key;
        }

        public XXTEADecryption(FileOperationType fileOperationType) : base(fileOperationType) { }

        #region OxGFrame Implements
        public override bool Initialize()
        {
            try
            {
                switch (this._fileOperationType)
                {
                    case FileOperationType.None:
                        {
                            var cryptogramSettings = CryptogramUtility.CryptogramSettingSetup.GetCryptogramSetting();
                            if (cryptogramSettings != null)
                            {
                                this._key = cryptogramSettings.xxteaKey;
                            }
                        }
                        break;

                    // Nothing to do
                    case FileOperationType.Custom:
                        break;

                    case FileOperationType.Bundle:
                        {
                            SecuredString[] decryptArgs = BundleConfig.bundleDecryptArgs;
                            this._key = decryptArgs[1].Decrypt();
                        }
                        break;

                    case FileOperationType.Manifest:
                        {
                            SecuredString[] decryptArgs = BundleConfig.manifestDecryptArgs;
                            this._key = decryptArgs[1].Decrypt();
                        }
                        break;
                }
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

        public override byte[] DecryptData(byte[] fileData)
        {
            if (this.CheckIsIntialized())
            {
                if (FileCryptogram.XXTEA.DecryptBytes(ref fileData, this._key))
                    return fileData;
            }
            return null;
        }
        #endregion
    }
    #endregion

    #region OffsetXOR
    public class OffsetXorDecryption : DecryptionServices
    {
        private byte _key;
        private int _dummySize;

        public OffsetXorDecryption() { }

        public OffsetXorDecryption(byte key, int dummySize)
        {
            this._fileOperationType = FileOperationType.Custom;
            this._key = key;
            this._dummySize = dummySize;
        }

        public OffsetXorDecryption(FileOperationType fileOperationType) : base(fileOperationType) { }

        #region OxGFrame Implements
        public override bool Initialize()
        {
            try
            {
                switch (this._fileOperationType)
                {
                    case FileOperationType.None:
                        {
                            var cryptogramSettings = CryptogramUtility.CryptogramSettingSetup.GetCryptogramSetting();
                            if (cryptogramSettings != null)
                            {
                                this._key = cryptogramSettings.offsetXorKey;
                                this._dummySize = cryptogramSettings.offsetXorDummySize;
                            }
                        }
                        break;

                    // Nothing to do
                    case FileOperationType.Custom:
                        break;

                    case FileOperationType.Bundle:
                        {
                            SecuredString[] decryptArgs = BundleConfig.bundleDecryptArgs;
                            this._key = Convert.ToByte(decryptArgs[1].Decrypt());
                            this._dummySize = this._dummySize = Convert.ToInt32(decryptArgs[2].Decrypt());
                        }
                        break;

                    case FileOperationType.Manifest:
                        {
                            SecuredString[] decryptArgs = BundleConfig.manifestDecryptArgs;
                            this._key = Convert.ToByte(decryptArgs[1].Decrypt());
                            this._dummySize = this._dummySize = Convert.ToInt32(decryptArgs[2].Decrypt());
                        }
                        break;
                }
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

        public override byte[] DecryptData(byte[] fileData)
        {
            if (this.CheckIsIntialized())
            {
                if (FileCryptogram.OffsetXOR.DecryptBytes(fileData, this._key, this._dummySize))
                    return fileData;
            }
            return null;
        }
        #endregion
    }
    #endregion
}