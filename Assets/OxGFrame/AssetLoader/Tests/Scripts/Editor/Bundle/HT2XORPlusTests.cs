using NUnit.Framework;
using OxGFrame.AssetLoader.Utility;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using static OxGFrame.AssetLoader.Bundle.FileCryptogram;

namespace OxGFrame.AssetLoader.Editor.Tests
{
    public class HT2XORPlusTests
    {
        internal readonly byte hKey = 1;
        internal readonly byte tKey = 2;
        internal readonly byte j1Key = 3;
        internal readonly byte j2Key = 4;

        internal readonly ulong dataSize = 1024 * 1024;

        [Test]
        public void EncryptDecryptBytesFromData()
        {
            Stopwatch stopwatch = new Stopwatch();

            byte[] testBytes = new byte[dataSize];
            new Random().NextBytes(testBytes);
            byte[] originalBytes = (byte[])testBytes.Clone();

            stopwatch.Start();
            bool encryptResult = HT2XORPlus.EncryptBytes(testBytes, hKey, tKey, j1Key, j2Key);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptBytesFromData] HT2XORPlus.EncryptBytes execution time: {stopwatch.Elapsed.TotalMilliseconds} ms, DataSize: {BundleUtility.GetBytesToString(dataSize)}");
            Assert.IsTrue(encryptResult, "In-place encryption failed");

            stopwatch.Reset();

            stopwatch.Start();
            bool decryptResult = HT2XORPlus.DecryptBytes(testBytes, hKey, tKey, j1Key, j2Key);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptBytesFromData] HT2XORPlus.DecryptBytes execution time: {stopwatch.Elapsed.TotalMilliseconds} ms, DataSize: {BundleUtility.GetBytesToString(dataSize)}");
            Assert.IsTrue(decryptResult, "In-place decryption failed");

            Assert.AreEqual(originalBytes, testBytes, "Decrypted content does not match the original content");
        }

        [Test]
        public void EncryptDecryptWriteFile()
        {
            Stopwatch stopwatch = new Stopwatch();

            string tempFile = Path.GetTempFileName();
            byte[] testData = new byte[dataSize];
            new Random().NextBytes(testData);
            File.WriteAllBytes(tempFile, testData);

            stopwatch.Start();
            bool encryptResult = HT2XORPlus.WriteFile.EncryptFile(tempFile, hKey, tKey, j1Key, j2Key);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptWriteFile] HT2XORPlus.WriteFile.EncryptFile execution time: {stopwatch.Elapsed.TotalMilliseconds} ms, DataSize: {BundleUtility.GetBytesToString(dataSize)}");
            Assert.IsTrue(encryptResult, "File encryption failed");

            stopwatch.Reset();

            stopwatch.Start();
            bool decryptResult = HT2XORPlus.WriteFile.DecryptFile(tempFile, hKey, tKey, j1Key, j2Key);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptWriteFile] HT2XORPlus.WriteFile.DecryptFile execution time: {stopwatch.Elapsed.TotalMilliseconds} ms, DataSize: {BundleUtility.GetBytesToString(dataSize)}");
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
            byte[] testData = new byte[dataSize];
            new Random().NextBytes(testData);
            File.WriteAllBytes(tempFile, testData);

            stopwatch.Start();
            byte[] encryptedBytes = HT2XORPlus.EncryptBytes(tempFile, hKey, tKey, j1Key, j2Key);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptBytesFromFile] HT2XORPlus.EncryptBytes execution time: {stopwatch.Elapsed.TotalMilliseconds} ms, DataSize: {BundleUtility.GetBytesToString(dataSize)}");
            Assert.IsNotNull(encryptedBytes, "Encrypted bytes returned null");
            Assert.IsNotEmpty(encryptedBytes, "Encrypted bytes are empty");

            stopwatch.Reset();

            string encryptedFile = tempFile + ".enc";
            File.WriteAllBytes(encryptedFile, encryptedBytes);

            stopwatch.Start();
            byte[] decryptedBytes = HT2XORPlus.DecryptBytes(encryptedFile, hKey, tKey, j1Key, j2Key);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptBytesFromFile] HT2XORPlus.DecryptBytes execution time: {stopwatch.Elapsed.TotalMilliseconds} ms, DataSize: {BundleUtility.GetBytesToString(dataSize)}");
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
            byte[] testData = new byte[dataSize];
            new Random().NextBytes(testData);
            File.WriteAllBytes(tempFile, testData);


            bool encryptResult = HT2XORPlus.WriteFile.EncryptFile(tempFile, hKey, tKey, j1Key, j2Key);
            Assert.IsTrue(encryptResult, "File encryption failed");

            stopwatch.Start();
            using (Stream decryptedStream = HT2XORPlus.DecryptStream(tempFile, hKey, tKey, j1Key, j2Key))
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
            UnityEngine.Debug.Log($"[DecryptStream] HT2XORPlus.DecryptStream execution time: {stopwatch.Elapsed.TotalMilliseconds} ms, DataSize: {BundleUtility.GetBytesToString(dataSize)}");

            File.Delete(tempFile);
        }
    }
}
