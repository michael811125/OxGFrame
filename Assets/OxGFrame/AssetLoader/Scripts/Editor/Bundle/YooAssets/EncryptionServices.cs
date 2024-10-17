using OxGFrame.AssetLoader.Bundle;
using System.IO;
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

    public class OffSetEncryption : IEncryptionServices
    {
        public EncryptResult Encrypt(EncryptFileInfo fileInfo)
        {
            var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSetting();

            string filePath = fileInfo.FilePath;

            int randomSeed = cryptogramSettings.randomSeed;
            int dummySize = cryptogramSettings.dummySize;

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
        public EncryptResult Encrypt(EncryptFileInfo fileInfo)
        {
            var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSetting();

            string filePath = fileInfo.FilePath;

            byte xorKey = cryptogramSettings.xorKey;

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
        public EncryptResult Encrypt(EncryptFileInfo fileInfo)
        {
            var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSetting();

            string filePath = fileInfo.FilePath;

            byte hXorKey = cryptogramSettings.hXorKey;
            byte tXorKey = cryptogramSettings.tXorKey;
            byte jXorKey = cryptogramSettings.jXorKey;

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
        public EncryptResult Encrypt(EncryptFileInfo fileInfo)
        {
            var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSetting();

            string filePath = fileInfo.FilePath;

            byte hXorKey = cryptogramSettings.hXorPlusKey;
            byte tXorKey = cryptogramSettings.tXorPlusKey;
            byte j1XorKey = cryptogramSettings.j1XorPlusKey;
            byte j2XorKey = cryptogramSettings.j2XorPlusKey;

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
        public EncryptResult Encrypt(EncryptFileInfo fileInfo)
        {
            var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSetting();

            string filePath = fileInfo.FilePath;

            string aesKey = cryptogramSettings.aesKey;
            string aesIv = cryptogramSettings.aesIv;

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