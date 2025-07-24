using OxGFrame.AssetLoader.Bundle;
using UnityEngine;
using YooAsset;

namespace OxGFrame.AssetLoader.Editor
{
    public class ManifestOffsetEncryption : IManifestProcessServices
    {
        private int? _dummySize = null;

        public ManifestOffsetEncryption() { }

        public ManifestOffsetEncryption(int dummySize)
        {
            this._dummySize = dummySize;
        }

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

        public byte[] RestoreManifest(byte[] fileData)
        {
            var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSetting();

            int dummySize = this._dummySize == null ? cryptogramSettings.dummySize : (int)this._dummySize;

            if (FileCryptogram.Offset.DecryptBytes(ref fileData, dummySize))
                return fileData;
            return null;
        }
    }

    public class ManifestXorEncryption : IManifestProcessServices
    {
        private byte? _xorKey = null;

        public ManifestXorEncryption() { }

        public ManifestXorEncryption(byte xorKey)
        {
            this._xorKey = xorKey;
        }

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

        public byte[] RestoreManifest(byte[] fileData)
        {
            var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSetting();

            byte xorKey = this._xorKey == null ? cryptogramSettings.xorKey : (byte)this._xorKey;

            if (FileCryptogram.XOR.DecryptBytes(fileData, xorKey))
                return fileData;
            return null;
        }
    }

    public class ManifestHT2XorEncryption : IManifestProcessServices
    {
        private byte? _hXorKey = null;
        private byte? _tXorKey = null;
        private byte? _jXorKey = null;

        public ManifestHT2XorEncryption() { }

        public ManifestHT2XorEncryption(byte hXorKey, byte tXorKey, byte jXorKey)
        {
            this._hXorKey = hXorKey;
            this._tXorKey = tXorKey;
            this._jXorKey = jXorKey;
        }

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

        public byte[] RestoreManifest(byte[] fileData)
        {
            var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSetting();

            byte hXorKey = this._hXorKey == null ? cryptogramSettings.hXorKey : (byte)this._hXorKey;
            byte tXorKey = this._tXorKey == null ? cryptogramSettings.tXorKey : (byte)this._tXorKey;
            byte jXorKey = this._jXorKey == null ? cryptogramSettings.jXorKey : (byte)this._jXorKey;

            if (FileCryptogram.HT2XOR.DecryptBytes(fileData, hXorKey, tXorKey, jXorKey))
                return fileData;
            return null;
        }
    }

    public class ManifestHT2XorPlusEncryption : IManifestProcessServices
    {
        private byte? _hXorKey = null;
        private byte? _tXorKey = null;
        private byte? _j1XorKey = null;
        private byte? _j2XorKey = null;

        public ManifestHT2XorPlusEncryption() { }

        public ManifestHT2XorPlusEncryption(byte hXorKey, byte tXorKey, byte j1XorKey, byte j2XorKey)
        {
            this._hXorKey = hXorKey;
            this._tXorKey = tXorKey;
            this._j1XorKey = j1XorKey;
            this._j2XorKey = j2XorKey;
        }

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

        public byte[] RestoreManifest(byte[] fileData)
        {
            var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSetting();

            byte hXorKey = this._hXorKey == null ? cryptogramSettings.hXorPlusKey : (byte)this._hXorKey;
            byte tXorKey = this._tXorKey == null ? cryptogramSettings.tXorPlusKey : (byte)this._tXorKey;
            byte j1XorKey = this._j1XorKey == null ? cryptogramSettings.j1XorPlusKey : (byte)this._j1XorKey;
            byte j2XorKey = this._j2XorKey == null ? cryptogramSettings.j2XorPlusKey : (byte)this._j2XorKey;

            if (FileCryptogram.HT2XORPlus.DecryptBytes(fileData, hXorKey, tXorKey, j1XorKey, j2XorKey))
                return fileData;
            return null;
        }
    }

    public class ManifestAesEncryption : IManifestProcessServices
    {
        private string _aesKey = null;
        private string _aesIv = null;

        public ManifestAesEncryption() { }

        public ManifestAesEncryption(string aesKey, string aesIv)
        {
            this._aesKey = aesKey;
            this._aesIv = aesIv;
        }

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

        public byte[] RestoreManifest(byte[] fileData)
        {
            var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSetting();

            string key = string.IsNullOrEmpty(this._aesKey) ? cryptogramSettings.aesKey : this._aesKey;
            string iv = string.IsNullOrEmpty(this._aesIv) ? cryptogramSettings.aesIv : this._aesIv;

            if (FileCryptogram.AES.DecryptBytes(ref fileData, key, iv))
                return fileData;
            return null;
        }
    }

    public class ManifestChaCha20Encryption : IManifestProcessServices
    {
        private string _chacha20Key = null;
        private string _chacha20Nonce = null;
        private uint? _chacha20Counter = null;

        public ManifestChaCha20Encryption() { }

        public ManifestChaCha20Encryption(string chacha20Key, string chacha20Nonce, uint chacha20Counter)
        {
            this._chacha20Key = chacha20Key;
            this._chacha20Nonce = chacha20Nonce;
            this._chacha20Counter = chacha20Counter;
        }

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

        public byte[] RestoreManifest(byte[] fileData)
        {
            var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSetting();

            string key = string.IsNullOrEmpty(this._chacha20Key) ? cryptogramSettings.chacha20Key : this._chacha20Key;
            string nonce = string.IsNullOrEmpty(this._chacha20Nonce) ? cryptogramSettings.chacha20Nonce : this._chacha20Nonce;
            uint counter = this._chacha20Counter == null ? (uint)cryptogramSettings.chacha20Counter : (uint)this._chacha20Counter;

            if (FileCryptogram.ChaCha20.DecryptBytes(ref fileData, key, nonce, counter))
                return fileData;
            return null;
        }
    }

    public class ManifestXXTEAEncryption : IManifestProcessServices
    {
        private string _xxteaKey = null;

        public ManifestXXTEAEncryption() { }

        public ManifestXXTEAEncryption(string xxteaKey)
        {
            this._xxteaKey = xxteaKey;
        }

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

        public byte[] RestoreManifest(byte[] fileData)
        {
            var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSetting();

            string key = string.IsNullOrEmpty(this._xxteaKey) ? cryptogramSettings.xxteaKey : this._xxteaKey;

            if (FileCryptogram.XXTEA.DecryptBytes(ref fileData, key))
                return fileData;
            return null;
        }
    }

    public class ManifestOffsetXorEncryption : IManifestProcessServices
    {
        private byte? _offsetXorKey = null;
        private int? _offsetXorDummySize = null;

        public ManifestOffsetXorEncryption() { }

        public ManifestOffsetXorEncryption(byte offsetXorKey, int offsetXorDummySize)
        {
            this._offsetXorKey = offsetXorKey;
            this._offsetXorDummySize = offsetXorDummySize;
        }

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

        public byte[] RestoreManifest(byte[] fileData)
        {
            var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSetting();

            byte key = this._offsetXorKey == null ? (byte)cryptogramSettings.offsetXorKey : (byte)this._offsetXorKey;
            int dummySize = this._offsetXorDummySize == null ? (int)cryptogramSettings.offsetXorDummySize : (int)this._offsetXorDummySize;

            if (FileCryptogram.OffsetXOR.DecryptBytes(fileData, key, dummySize))
                return fileData;
            return null;
        }
    }
}