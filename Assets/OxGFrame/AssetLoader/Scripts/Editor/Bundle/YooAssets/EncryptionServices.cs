using OxGFrame.AssetLoader.Bundle;
using System.IO;
using UnityEngine;
using YooAsset;

public static class CryptogramSettingSetup
{
    public static CryptogramSetting cryptogramSettings;

    public static CryptogramSetting GetCryptogramSettings()
    {
        if (cryptogramSettings == null) cryptogramSettings = EditorTool.LoadSettingData<CryptogramSetting>();
        return cryptogramSettings;
    }
}

public class OffSetEncryption : IEncryptionServices
{
    public EncryptResult Encrypt(EncryptFileInfo fileInfo)
    {
        var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSettings();

        string filePath = fileInfo.FilePath;

        int randomSeed = cryptogramSettings.randomSeed;
        int dummySize = cryptogramSettings.dummySize;

        byte[] fileData = File.ReadAllBytes(filePath);

        if (FileCryptogram.Offset.OffsetEncryptBytes(ref fileData, randomSeed, dummySize))
        {
            Debug.Log($"OffsetCryptogram => randomSeed: {randomSeed}, dummySize: {dummySize}");

            EncryptResult result = new EncryptResult();
            result.LoadMethod = EBundleLoadMethod.LoadFromStream;
            result.EncryptedData = fileData;
            return result;
        }
        else
        {
            EncryptResult result = new EncryptResult();
            result.LoadMethod = EBundleLoadMethod.Normal;
            return result;
        }
    }
}

public class XorEncryption : IEncryptionServices
{
    public EncryptResult Encrypt(EncryptFileInfo fileInfo)
    {
        var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSettings();

        string filePath = fileInfo.FilePath;

        byte xorKey = cryptogramSettings.xorKey;

        byte[] fileData = File.ReadAllBytes(filePath);

        if (FileCryptogram.XOR.XorEncryptBytes(fileData, xorKey))
        {
            Debug.Log($"XorCryptogram => xorKey: {xorKey}");

            EncryptResult result = new EncryptResult();
            result.LoadMethod = EBundleLoadMethod.LoadFromStream;
            result.EncryptedData = fileData;
            return result;
        }
        else
        {
            EncryptResult result = new EncryptResult();
            result.LoadMethod = EBundleLoadMethod.Normal;
            return result;
        }
    }
}

public class HTXorEncryption : IEncryptionServices
{
    public EncryptResult Encrypt(EncryptFileInfo fileInfo)
    {
        var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSettings();

        string filePath = fileInfo.FilePath;

        byte hXorKey = cryptogramSettings.hXorKey;
        byte tXorKey = cryptogramSettings.tXorKey;

        byte[] fileData = File.ReadAllBytes(filePath);

        if (FileCryptogram.HTXOR.HTXorEncryptBytes(fileData, hXorKey, tXorKey))
        {
            Debug.Log($"HTXorCryptogram => hXorKey: {hXorKey}, tXorKey: {tXorKey}");

            EncryptResult result = new EncryptResult();
            result.LoadMethod = EBundleLoadMethod.LoadFromStream;
            result.EncryptedData = fileData;
            return result;
        }
        else
        {
            EncryptResult result = new EncryptResult();
            result.LoadMethod = EBundleLoadMethod.Normal;
            return result;
        }
    }
}

public class AesEncryption : IEncryptionServices
{
    public EncryptResult Encrypt(EncryptFileInfo fileInfo)
    {
        var cryptogramSettings = CryptogramSettingSetup.GetCryptogramSettings();

        string filePath = fileInfo.FilePath;

        string aesKey = cryptogramSettings.aesKey;
        string aesIv = cryptogramSettings.aesIv;

        byte[] fileData = File.ReadAllBytes(filePath);

        if (FileCryptogram.AES.AesEncryptBytes(ref fileData, aesKey, aesIv))
        {
            Debug.Log($"AesCryptogram => aesKey: {aesKey}, aesIv: {aesIv}");

            EncryptResult result = new EncryptResult();
            result.LoadMethod = EBundleLoadMethod.LoadFromStream;
            result.EncryptedData = fileData;
            return result;
        }
        else
        {
            EncryptResult result = new EncryptResult();
            result.LoadMethod = EBundleLoadMethod.Normal;
            return result;
        }
    }
}
