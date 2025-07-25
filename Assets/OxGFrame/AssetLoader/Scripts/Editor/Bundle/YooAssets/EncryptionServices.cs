using OxGFrame.AssetLoader.Bundle;
using UnityEngine;
using YooAsset;

namespace OxGFrame.AssetLoader.Editor
{
    public class OffsetEncryption : IEncryptionServices, IManifestProcessServices
    {
        private int? _dummySize = null;

        public OffsetEncryption() { }

        public OffsetEncryption(int dummySize)
        {
            this._dummySize = dummySize;
        }

        #region IEncryptionServices
        public EncryptResult Encrypt(EncryptFileInfo fileInfo)
        {
            var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSetting();

            string filePath = fileInfo.FileLoadPath;

            int dummySize = this._dummySize == null ? cryptogramSettings.dummySize : (int)this._dummySize;

            byte[] fileData = FileCryptogram.Offset.EncryptBytes(filePath, dummySize);

            if (fileData != null)
            {
                Debug.Log($"Offset Cryptogram => dummySize: {dummySize}");

                EncryptResult result = new EncryptResult();
                result.Encrypted = true;
                result.EncryptedData = fileData;
                return result;
            }
            else
            {
                EncryptResult result = new EncryptResult();
                result.Encrypted = false;
                return result;
            }
        }
        #endregion

        #region IManifestProcessServices
        public byte[] ProcessManifest(byte[] fileData)
        {
            var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSetting();

            int dummySize = this._dummySize == null ? cryptogramSettings.dummySize : (int)this._dummySize;

            if (FileCryptogram.Offset.EncryptBytes(ref fileData, dummySize))
            {
                Debug.Log($"Manifest Offset Cryptogram => dummySize: {dummySize}");

                return fileData;
            }

            return null;
        }
        #endregion
    }

    public class XorEncryption : IEncryptionServices, IManifestProcessServices
    {
        private byte? _xorKey = null;

        public XorEncryption() { }

        public XorEncryption(byte xorKey)
        {
            this._xorKey = xorKey;
        }

        #region IEncryptionServices
        public EncryptResult Encrypt(EncryptFileInfo fileInfo)
        {
            var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSetting();

            string filePath = fileInfo.FileLoadPath;

            byte xorKey = this._xorKey == null ? cryptogramSettings.xorKey : (byte)this._xorKey;

            byte[] fileData = FileCryptogram.XOR.EncryptBytes(filePath, xorKey);

            if (fileData != null)
            {
                Debug.Log($"XorCryptogram => xorKey: {xorKey}");

                EncryptResult result = new EncryptResult();
                result.Encrypted = true;
                result.EncryptedData = fileData;
                return result;
            }
            else
            {
                EncryptResult result = new EncryptResult();
                result.Encrypted = false;
                return result;
            }
        }
        #endregion

        #region IManifestProcessServices
        public byte[] ProcessManifest(byte[] fileData)
        {
            var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSetting();

            byte xorKey = this._xorKey == null ? cryptogramSettings.xorKey : (byte)this._xorKey;

            if (FileCryptogram.XOR.EncryptBytes(fileData, xorKey))
            {
                Debug.Log($"Manifest XorCryptogram => xorKey: {xorKey}");

                return fileData;
            }

            return null;
        }
        #endregion
    }

    public class HT2XorEncryption : IEncryptionServices, IManifestProcessServices
    {
        private byte? _hXorKey = null;
        private byte? _tXorKey = null;
        private byte? _jXorKey = null;

        public HT2XorEncryption() { }

        public HT2XorEncryption(byte hXorKey, byte tXorKey, byte jXorKey)
        {
            this._hXorKey = hXorKey;
            this._tXorKey = tXorKey;
            this._jXorKey = jXorKey;
        }

        #region IEncryptionServices
        public EncryptResult Encrypt(EncryptFileInfo fileInfo)
        {
            var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSetting();

            string filePath = fileInfo.FileLoadPath;

            byte hXorKey = this._hXorKey == null ? cryptogramSettings.hXorKey : (byte)this._hXorKey;
            byte tXorKey = this._tXorKey == null ? cryptogramSettings.tXorKey : (byte)this._tXorKey;
            byte jXorKey = this._jXorKey == null ? cryptogramSettings.jXorKey : (byte)this._jXorKey;

            byte[] fileData = FileCryptogram.HT2XOR.EncryptBytes(filePath, hXorKey, tXorKey, jXorKey);

            if (fileData != null)
            {
                Debug.Log($"HT2Xor Cryptogram => hXorKey: {hXorKey}, tXorKey: {tXorKey}, jXorKey: {jXorKey}");

                EncryptResult result = new EncryptResult();
                result.Encrypted = true;
                result.EncryptedData = fileData;
                return result;
            }
            else
            {
                EncryptResult result = new EncryptResult();
                result.Encrypted = false;
                return result;
            }
        }
        #endregion

        #region IManifestProcessServices
        public byte[] ProcessManifest(byte[] fileData)
        {
            var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSetting();

            byte hXorKey = this._hXorKey == null ? cryptogramSettings.hXorKey : (byte)this._hXorKey;
            byte tXorKey = this._tXorKey == null ? cryptogramSettings.tXorKey : (byte)this._tXorKey;
            byte jXorKey = this._jXorKey == null ? cryptogramSettings.jXorKey : (byte)this._jXorKey;

            if (FileCryptogram.HT2XOR.EncryptBytes(fileData, hXorKey, tXorKey, jXorKey))
            {
                Debug.Log($"Manifest HT2Xor Cryptogram => hXorKey: {hXorKey}, tXorKey: {tXorKey}, jXorKey: {jXorKey}");

                return fileData;
            }

            return null;
        }
        #endregion
    }

    public class HT2XorPlusEncryption : IEncryptionServices, IManifestProcessServices
    {
        private byte? _hXorKey = null;
        private byte? _tXorKey = null;
        private byte? _j1XorKey = null;
        private byte? _j2XorKey = null;

        public HT2XorPlusEncryption() { }

        public HT2XorPlusEncryption(byte hXorKey, byte tXorKey, byte j1XorKey, byte j2XorKey)
        {
            this._hXorKey = hXorKey;
            this._tXorKey = tXorKey;
            this._j1XorKey = j1XorKey;
            this._j2XorKey = j2XorKey;
        }

        #region IEncryptionServices
        public EncryptResult Encrypt(EncryptFileInfo fileInfo)
        {
            var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSetting();

            string filePath = fileInfo.FileLoadPath;

            byte hXorKey = this._hXorKey == null ? cryptogramSettings.hXorPlusKey : (byte)this._hXorKey;
            byte tXorKey = this._tXorKey == null ? cryptogramSettings.tXorPlusKey : (byte)this._tXorKey;
            byte j1XorKey = this._j1XorKey == null ? cryptogramSettings.j1XorPlusKey : (byte)this._j1XorKey;
            byte j2XorKey = this._j2XorKey == null ? cryptogramSettings.j2XorPlusKey : (byte)this._j2XorKey;

            byte[] fileData = FileCryptogram.HT2XORPlus.EncryptBytes(filePath, hXorKey, tXorKey, j1XorKey, j2XorKey);

            if (fileData != null)
            {
                Debug.Log($"HT2XorPlus Cryptogram => hXorKey: {hXorKey}, tXorKey: {tXorKey}, j1XorKey: {j1XorKey}, j2XorKey: {j2XorKey}");

                EncryptResult result = new EncryptResult();
                result.Encrypted = true;
                result.EncryptedData = fileData;
                return result;
            }
            else
            {
                EncryptResult result = new EncryptResult();
                result.Encrypted = false;
                return result;
            }
        }
        #endregion

        #region IManifestProcessServices
        public byte[] ProcessManifest(byte[] fileData)
        {
            var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSetting();

            byte hXorKey = this._hXorKey == null ? cryptogramSettings.hXorPlusKey : (byte)this._hXorKey;
            byte tXorKey = this._tXorKey == null ? cryptogramSettings.tXorPlusKey : (byte)this._tXorKey;
            byte j1XorKey = this._j1XorKey == null ? cryptogramSettings.j1XorPlusKey : (byte)this._j1XorKey;
            byte j2XorKey = this._j2XorKey == null ? cryptogramSettings.j2XorPlusKey : (byte)this._j2XorKey;

            if (FileCryptogram.HT2XORPlus.EncryptBytes(fileData, hXorKey, tXorKey, j1XorKey, j2XorKey))
            {
                Debug.Log($"Manifest HT2XorPlus Cryptogram => hXorKey: {hXorKey}, tXorKey: {tXorKey}, j1XorKey: {j1XorKey}, j2XorKey: {j2XorKey}");

                return fileData;
            }

            return null;
        }
        #endregion
    }

    public class AesEncryption : IEncryptionServices, IManifestProcessServices
    {
        private string _aesKey = null;
        private string _aesIv = null;

        public AesEncryption() { }

        public AesEncryption(string aesKey, string aesIv)
        {
            this._aesKey = aesKey;
            this._aesIv = aesIv;
        }

        #region IEncryptionServices
        public EncryptResult Encrypt(EncryptFileInfo fileInfo)
        {
            var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSetting();

            string filePath = fileInfo.FileLoadPath;

            string key = string.IsNullOrEmpty(this._aesKey) ? cryptogramSettings.aesKey : this._aesKey;
            string iv = string.IsNullOrEmpty(this._aesIv) ? cryptogramSettings.aesIv : this._aesIv;

            byte[] fileData = FileCryptogram.AES.EncryptBytes(filePath, key, iv);

            if (fileData != null)
            {
                Debug.Log($"AES Cryptogram => key: {key}, iv: {iv}");

                EncryptResult result = new EncryptResult();
                result.Encrypted = true;
                result.EncryptedData = fileData;
                return result;
            }
            else
            {
                EncryptResult result = new EncryptResult();
                result.Encrypted = false;
                return result;
            }
        }
        #endregion

        #region IManifestProcessServices
        public byte[] ProcessManifest(byte[] fileData)
        {
            var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSetting();

            string key = string.IsNullOrEmpty(this._aesKey) ? cryptogramSettings.aesKey : this._aesKey;
            string iv = string.IsNullOrEmpty(this._aesIv) ? cryptogramSettings.aesIv : this._aesIv;

            if (FileCryptogram.AES.EncryptBytes(ref fileData, key, iv))
            {
                Debug.Log($"Manifest AES Cryptogram => key: {key}, iv: {iv}");

                return fileData;
            }

            return null;
        }
        #endregion
    }

    public class ChaCha20Encryption : IEncryptionServices, IManifestProcessServices
    {
        private string _chacha20Key = null;
        private string _chacha20Nonce = null;
        private uint? _chacha20Counter = null;

        public ChaCha20Encryption() { }

        public ChaCha20Encryption(string chacha20Key, string chacha20Nonce, uint chacha20Counter)
        {
            this._chacha20Key = chacha20Key;
            this._chacha20Nonce = chacha20Nonce;
            this._chacha20Counter = chacha20Counter;
        }

        #region IEncryptionServices
        public EncryptResult Encrypt(EncryptFileInfo fileInfo)
        {
            var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSetting();

            string filePath = fileInfo.FileLoadPath;

            string key = string.IsNullOrEmpty(this._chacha20Key) ? cryptogramSettings.chacha20Key : this._chacha20Key;
            string nonce = string.IsNullOrEmpty(this._chacha20Nonce) ? cryptogramSettings.chacha20Nonce : this._chacha20Nonce;
            uint counter = this._chacha20Counter == null ? (uint)cryptogramSettings.chacha20Counter : (uint)this._chacha20Counter;

            byte[] fileData = FileCryptogram.ChaCha20.EncryptBytes(filePath, key, nonce, counter);

            if (fileData != null)
            {
                Debug.Log($"ChaCha20 Cryptogram => key: {key}, nonce: {nonce}, counter: {counter}");

                EncryptResult result = new EncryptResult();
                result.Encrypted = true;
                result.EncryptedData = fileData;
                return result;
            }
            else
            {
                EncryptResult result = new EncryptResult();
                result.Encrypted = false;
                return result;
            }
        }
        #endregion

        #region IManifestProcessServices
        public byte[] ProcessManifest(byte[] fileData)
        {
            var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSetting();

            string key = string.IsNullOrEmpty(this._chacha20Key) ? cryptogramSettings.chacha20Key : this._chacha20Key;
            string nonce = string.IsNullOrEmpty(this._chacha20Nonce) ? cryptogramSettings.chacha20Nonce : this._chacha20Nonce;
            uint counter = this._chacha20Counter == null ? (uint)cryptogramSettings.chacha20Counter : (uint)this._chacha20Counter;

            if (FileCryptogram.ChaCha20.EncryptBytes(ref fileData, key, nonce, counter))
            {
                Debug.Log($"Manifest ChaCha20 Cryptogram => key: {key}, nonce: {nonce}, counter: {counter}");

                return fileData;
            }
            return null;
        }
        #endregion
    }

    public class XXTEAEncryption : IEncryptionServices, IManifestProcessServices
    {
        private string _xxteaKey = null;

        public XXTEAEncryption() { }

        public XXTEAEncryption(string xxteaKey)
        {
            this._xxteaKey = xxteaKey;
        }

        #region IEncryptionServices
        public EncryptResult Encrypt(EncryptFileInfo fileInfo)
        {
            var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSetting();

            string filePath = fileInfo.FileLoadPath;

            string key = string.IsNullOrEmpty(this._xxteaKey) ? cryptogramSettings.xxteaKey : this._xxteaKey;

            byte[] fileData = FileCryptogram.XXTEA.EncryptBytes(filePath, key);

            if (fileData != null)
            {
                Debug.Log($"XXTEA Cryptogram => key: {key}");

                EncryptResult result = new EncryptResult();
                result.Encrypted = true;
                result.EncryptedData = fileData;
                return result;
            }
            else
            {
                EncryptResult result = new EncryptResult();
                result.Encrypted = false;
                return result;
            }
        }
        #endregion

        #region IManifestProcessServices
        public byte[] ProcessManifest(byte[] fileData)
        {
            var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSetting();

            string key = string.IsNullOrEmpty(this._xxteaKey) ? cryptogramSettings.xxteaKey : this._xxteaKey;

            if (FileCryptogram.XXTEA.EncryptBytes(ref fileData, key))
            {
                Debug.Log($"Manifest XXTEA Cryptogram => key: {key}");

                return fileData;
            }

            return null;
        }
        #endregion
    }

    public class OffsetXorEncryption : IEncryptionServices, IManifestProcessServices
    {
        private byte? _offsetXorKey = null;
        private int? _offsetXorDummySize = null;

        public OffsetXorEncryption() { }

        public OffsetXorEncryption(byte offsetXorKey, int offsetXorDummySize)
        {
            this._offsetXorKey = offsetXorKey;
            this._offsetXorDummySize = offsetXorDummySize;
        }

        #region IEncryptionServices
        public EncryptResult Encrypt(EncryptFileInfo fileInfo)
        {
            var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSetting();

            string filePath = fileInfo.FileLoadPath;

            byte key = this._offsetXorKey == null ? (byte)cryptogramSettings.offsetXorKey : (byte)this._offsetXorKey;
            int dummySize = this._offsetXorDummySize == null ? (int)cryptogramSettings.offsetXorDummySize : (int)this._offsetXorDummySize;

            byte[] fileData = FileCryptogram.OffsetXOR.EncryptBytes(filePath, key, dummySize);

            if (fileData != null)
            {
                Debug.Log($"OffsetXOR Cryptogram => key: {key}, dummySize: {dummySize}");

                EncryptResult result = new EncryptResult();
                result.Encrypted = true;
                result.EncryptedData = fileData;
                return result;
            }
            else
            {
                EncryptResult result = new EncryptResult();
                result.Encrypted = false;
                return result;
            }
        }
        #endregion

        #region IManifestProcessServices
        public byte[] ProcessManifest(byte[] fileData)
        {
            var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSetting();

            byte key = this._offsetXorKey == null ? (byte)cryptogramSettings.offsetXorKey : (byte)this._offsetXorKey;
            int dummySize = this._offsetXorDummySize == null ? (int)cryptogramSettings.offsetXorDummySize : (int)this._offsetXorDummySize;

            if (FileCryptogram.OffsetXOR.EncryptBytes(fileData, key, dummySize))
            {
                Debug.Log($"Manifest OffsetXOR Cryptogram => key: {key}, dummySize: {dummySize}");

                return fileData;
            }

            return null;
        }
        #endregion
    }
}