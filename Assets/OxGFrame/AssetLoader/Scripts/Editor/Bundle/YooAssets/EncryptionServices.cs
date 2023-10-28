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

            byte[] fileData = File.ReadAllBytes(filePath);

            if (FileCryptogram.Offset.OffsetEncryptBytes(ref fileData, randomSeed, dummySize))
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

            byte[] fileData = File.ReadAllBytes(filePath);

            if (FileCryptogram.XOR.XorEncryptBytes(fileData, xorKey))
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

            byte[] fileData = File.ReadAllBytes(filePath);

            if (FileCryptogram.HT2XOR.HT2XorEncryptBytes(fileData, hXorKey, tXorKey, jXorKey))
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

    public class AesEncryption : IEncryptionServices
    {
        public EncryptResult Encrypt(EncryptFileInfo fileInfo)
        {
            var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSetting();

            string filePath = fileInfo.FilePath;

            string aesKey = cryptogramSettings.aesKey;
            string aesIv = cryptogramSettings.aesIv;

            byte[] fileData = File.ReadAllBytes(filePath);

            if (FileCryptogram.AES.AesEncryptBytes(ref fileData, aesKey, aesIv))
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