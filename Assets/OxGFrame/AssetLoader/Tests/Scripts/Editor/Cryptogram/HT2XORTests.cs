using NUnit.Framework;
using OxGFrame.AssetLoader.Utility;
using System;
using System.Diagnostics;
using System.IO;
using static OxGFrame.AssetLoader.Bundle.FileCryptogram;

namespace OxGFrame.AssetLoader.Editor.Tests
{
    public class HT2XORTests
    {
        internal readonly byte hKey = 1;
        internal readonly byte tKey = 2;
        internal readonly byte jKey = 3;

        [Test]
        public void EncryptDecryptBytesFromData()
        {
            Stopwatch stopwatch = new Stopwatch();

            byte[] testBytes = new byte[CryptogramConfig.DATA_SIZE];
            new Random().NextBytes(testBytes);
            byte[] originalBytes = (byte[])testBytes.Clone();

            stopwatch.Start();
            bool encryptResult = HT2XOR.EncryptBytes(testBytes, hKey, tKey, jKey);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptBytesFromData] HT2XOR.EncryptBytes execution time: {stopwatch.Elapsed.TotalMilliseconds} ms, CryptogramConfig.DATA_SIZE: {BundleUtility.GetBytesToString(CryptogramConfig.DATA_SIZE)}");
            Assert.IsTrue(encryptResult, "In-place encryption failed");

            stopwatch.Reset();

            stopwatch.Start();
            bool decryptResult = HT2XOR.DecryptBytes(testBytes, hKey, tKey, jKey);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptBytesFromData] HT2XOR.DecryptBytes execution time: {stopwatch.Elapsed.TotalMilliseconds} ms, CryptogramConfig.DATA_SIZE: {BundleUtility.GetBytesToString(CryptogramConfig.DATA_SIZE)}");
            Assert.IsTrue(decryptResult, "In-place decryption failed");

            Assert.AreEqual(originalBytes, testBytes, "Decrypted content does not match the original content");
        }

        [Test]
        public void EncryptDecryptWriteFile()
        {
            Stopwatch stopwatch = new Stopwatch();

            string tempFile = Path.GetTempFileName();
            byte[] testData = new byte[CryptogramConfig.DATA_SIZE];
            new Random().NextBytes(testData);
            File.WriteAllBytes(tempFile, testData);

            stopwatch.Start();
            bool encryptResult = HT2XOR.WriteFile.EncryptFile(tempFile, hKey, tKey, jKey);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptWriteFile] HT2XOR.WriteFile.EncryptFile execution time: {stopwatch.Elapsed.TotalMilliseconds} ms, CryptogramConfig.DATA_SIZE: {BundleUtility.GetBytesToString(CryptogramConfig.DATA_SIZE)}");
            Assert.IsTrue(encryptResult, "File encryption failed");

            stopwatch.Reset();

            stopwatch.Start();
            bool decryptResult = HT2XOR.WriteFile.DecryptFile(tempFile, hKey, tKey, jKey);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptWriteFile] HT2XOR.WriteFile.DecryptFile execution time: {stopwatch.Elapsed.TotalMilliseconds} ms, CryptogramConfig.DATA_SIZE: {BundleUtility.GetBytesToString(CryptogramConfig.DATA_SIZE)}");
            Assert.IsTrue(decryptResult, "File decryption failed");

            byte[] decryptedData = File.ReadAllBytes(tempFile);
            Assert.AreEqual(testData, decryptedData, "Decrypted file content does not match the original content");

            File.Delete(tempFile);
        }

        [Test]
        public void EncryptDecryptBytesFromFile()
        {
            Stopwatch stopwatch = new Stopwatch();

            string tempFile = Path.GetTempFileName();
            byte[] testData = new byte[CryptogramConfig.DATA_SIZE];
            new Random().NextBytes(testData);
            File.WriteAllBytes(tempFile, testData);

            stopwatch.Start();
            byte[] encryptedBytes = HT2XOR.EncryptBytes(tempFile, hKey, tKey, jKey);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptBytesFromFile] HT2XOR.EncryptBytes execution time: {stopwatch.Elapsed.TotalMilliseconds} ms, CryptogramConfig.DATA_SIZE: {BundleUtility.GetBytesToString(CryptogramConfig.DATA_SIZE)}");
            Assert.IsNotNull(encryptedBytes, "Encrypted bytes returned null");
            Assert.IsNotEmpty(encryptedBytes, "Encrypted bytes are empty");

            stopwatch.Reset();

            string encryptedFile = tempFile + ".enc";
            File.WriteAllBytes(encryptedFile, encryptedBytes);

            stopwatch.Start();
            byte[] decryptedBytes = HT2XOR.DecryptBytes(encryptedFile, hKey, tKey, jKey);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptBytesFromFile] HT2XOR.DecryptBytes execution time: {stopwatch.Elapsed.TotalMilliseconds} ms, CryptogramConfig.DATA_SIZE: {BundleUtility.GetBytesToString(CryptogramConfig.DATA_SIZE)}");
            Assert.IsNotNull(decryptedBytes, "Decrypted bytes returned null");
            Assert.AreEqual(testData, decryptedBytes, "Decrypted content does not match the original content");

            File.Delete(tempFile);
            File.Delete(encryptedFile);
        }

        [Test]
        public void DecryptStream()
        {
            Stopwatch stopwatch = new Stopwatch();

            string tempFile = Path.GetTempFileName();
            byte[] testData = new byte[CryptogramConfig.DATA_SIZE];
            new Random().NextBytes(testData);
            File.WriteAllBytes(tempFile, testData);


            bool encryptResult = HT2XOR.WriteFile.EncryptFile(tempFile, hKey, tKey, jKey);
            Assert.IsTrue(encryptResult, "File encryption failed");

            stopwatch.Start();
            using (Stream decryptedStream = HT2XOR.DecryptStream(tempFile, hKey, tKey, jKey))
            {
                stopwatch.Stop();
                Assert.IsNotNull(decryptedStream, "Decrypted stream is null");
                using (MemoryStream ms = new MemoryStream())
                {
                    decryptedStream.CopyTo(ms);
                    byte[] decryptedData = ms.ToArray();
                    Assert.AreEqual(testData, decryptedData, "Stream decrypted content does not match");
                }
            }
            UnityEngine.Debug.Log($"[DecryptStream] HT2XOR.DecryptStream execution time: {stopwatch.Elapsed.TotalMilliseconds} ms, CryptogramConfig.DATA_SIZE: {BundleUtility.GetBytesToString(CryptogramConfig.DATA_SIZE)}");

            File.Delete(tempFile);
        }
    }
}
