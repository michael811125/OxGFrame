using OxGFrame.AssetLoader.Bundle;
using UnityEngine;
using YooAsset;

namespace OxGFrame.AssetLoader.Editor
{
    public static class CryptogramSettingSetup
    {
        public static CryptogramSetting cryptogramSetting;

        public static CryptogramSetting GetCryptogramSetting()
        {
            if (cryptogramSetting == null) cryptogramSetting = EditorTool.LoadSettingData<CryptogramSetting>();
            return cryptogramSetting;
        }
    }

    public class OffsetEncryption : IEncryptionServices
    {
        private int? _randomSeed = null;
        private int? _dummySize = null;

        public OffsetEncryption()
        {
        }

        public OffsetEncryption(int randomSeed, int dummySize)
        {
            this._randomSeed = randomSeed;
            this._dummySize = dummySize;
        }

        public EncryptResult Encrypt(EncryptFileInfo fileInfo)
        {
            var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSetting();

            string filePath = fileInfo.FileLoadPath;

            int randomSeed = this._randomSeed == null ? cryptogramSettings.randomSeed : (int)this._randomSeed;
            int dummySize = this._dummySize == null ? cryptogramSettings.dummySize : (int)this._dummySize;

            byte[] fileData = FileCryptogram.Offset.OffsetEncryptBytes(filePath, randomSeed, dummySize);

            if (fileData != null)
            {
                Debug.Log($"OffsetCryptogram => randomSeed: {randomSeed}, dummySize: {dummySize}");

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
    }

    public class XorEncryption : IEncryptionServices
    {
        private byte? _xorKey = null;

        public XorEncryption()
        {
        }

        public XorEncryption(byte xorKey)
        {
            this._xorKey = xorKey;
        }

        public EncryptResult Encrypt(EncryptFileInfo fileInfo)
        {
            var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSetting();

            string filePath = fileInfo.FileLoadPath;

            byte xorKey = this._xorKey == null ? cryptogramSettings.xorKey : (byte)this._xorKey;

            byte[] fileData = FileCryptogram.XOR.XorEncryptBytes(filePath, xorKey);

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
    }

    public class HT2XorEncryption : IEncryptionServices
    {
        private byte? _hXorKey = null;
        private byte? _tXorKey = null;
        private byte? _jXorKey = null;

        public HT2XorEncryption()
        {
        }

        public HT2XorEncryption(byte hXorKey, byte tXorKey, byte jXorKey)
        {
            this._hXorKey = hXorKey;
            this._tXorKey = tXorKey;
            this._jXorKey = jXorKey;
        }

        public EncryptResult Encrypt(EncryptFileInfo fileInfo)
        {
            var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSetting();

            string filePath = fileInfo.FileLoadPath;

            byte hXorKey = this._hXorKey == null ? cryptogramSettings.hXorKey : (byte)this._hXorKey;
            byte tXorKey = this._tXorKey == null ? cryptogramSettings.tXorKey : (byte)this._tXorKey;
            byte jXorKey = this._jXorKey == null ? cryptogramSettings.jXorKey : (byte)this._jXorKey;

            byte[] fileData = FileCryptogram.HT2XOR.HT2XorEncryptBytes(filePath, hXorKey, tXorKey, jXorKey);

            if (fileData != null)
            {
                Debug.Log($"HT2XorCryptogram => hXorKey: {hXorKey}, tXorKey: {tXorKey}, jXorKey: {jXorKey}");

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
    }

    public class HT2XorPlusEncryption : IEncryptionServices
    {
        private byte? _hXorKey = null;
        private byte? _tXorKey = null;
        private byte? _j1XorKey = null;
        private byte? _j2XorKey = null;

        public HT2XorPlusEncryption()
        {
        }

        public HT2XorPlusEncryption(byte hXorKey, byte tXorKey, byte j1XorKey, byte j2XorKey)
        {
            this._hXorKey = hXorKey;
            this._tXorKey = tXorKey;
            this._j1XorKey = j1XorKey;
            this._j2XorKey = j2XorKey;
        }

        public EncryptResult Encrypt(EncryptFileInfo fileInfo)
        {
            var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSetting();

            string filePath = fileInfo.FileLoadPath;

            byte hXorKey = this._hXorKey == null ? cryptogramSettings.hXorPlusKey : (byte)this._hXorKey;
            byte tXorKey = this._tXorKey == null ? cryptogramSettings.tXorPlusKey : (byte)this._tXorKey;
            byte j1XorKey = this._j1XorKey == null ? cryptogramSettings.j1XorPlusKey : (byte)this._j1XorKey;
            byte j2XorKey = this._j2XorKey == null ? cryptogramSettings.j2XorPlusKey : (byte)this._j2XorKey;

            byte[] fileData = FileCryptogram.HT2XORPlus.HT2XorPlusEncryptBytes(filePath, hXorKey, tXorKey, j1XorKey, j2XorKey);

            if (fileData != null)
            {
                Debug.Log($"HT2XorPlusCryptogram => hXorKey: {hXorKey}, tXorKey: {tXorKey}, j1XorKey: {j1XorKey}, j2XorKey: {j2XorKey}");

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
    }

    public class AesEncryption : IEncryptionServices
    {
        private string _aesKey = null;
        private string _aesIv = null;

        public AesEncryption()
        {
        }

        public AesEncryption(string aesKey, string aesIv)
        {
            this._aesKey = aesKey;
            this._aesIv = aesIv;
        }

        public EncryptResult Encrypt(EncryptFileInfo fileInfo)
        {
            var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSetting();

            string filePath = fileInfo.FileLoadPath;

            string aesKey = string.IsNullOrEmpty(this._aesKey) ? cryptogramSettings.aesKey : this._aesKey;
            string aesIv = string.IsNullOrEmpty(this._aesIv) ? cryptogramSettings.aesIv : this._aesIv;

            byte[] fileData = FileCryptogram.AES.AesEncryptBytes(filePath, aesKey, aesIv);

            if (fileData != null)
            {
                Debug.Log($"AesCryptogram => aesKey: {aesKey}, aesIv: {aesIv}");

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
    }
}