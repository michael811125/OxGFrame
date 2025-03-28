using NUnit.Framework;
using OxGFrame.AssetLoader.Utility;
using System;
using System.Diagnostics;
using System.IO;
using static OxGFrame.AssetLoader.Bundle.FileCryptogram;

namespace OxGFrame.AssetLoader.Editor.Tests
{
    public class OffsetTests
    {
        internal readonly int dummySize = 128;

        [Test]
        public void EncryptDecryptBytesFromData()
        {
            Stopwatch stopwatch = new Stopwatch();

            byte[] testBytes = new byte[CryptogramConfig.DATA_SIZE];
            new Random().NextBytes(testBytes);
            byte[] originalBytes = (byte[])testBytes.Clone();

            stopwatch.Start();
            bool encryptResult = Offset.EncryptBytes(ref testBytes, dummySize);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptBytesFromData] Offset.EncryptBytes execution time: {stopwatch.Elapsed.TotalMilliseconds} ms, CryptogramConfig.DATA_SIZE: {BundleUtility.GetBytesToString(CryptogramConfig.DATA_SIZE)}");
            Assert.IsTrue(encryptResult, "In-place encryption failed");

            stopwatch.Reset();

            stopwatch.Start();
            bool decryptResult = Offset.DecryptBytes(ref testBytes, dummySize);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptBytesFromData] Offset.DecryptBytes execution time: {stopwatch.Elapsed.TotalMilliseconds} ms, CryptogramConfig.DATA_SIZE: {BundleUtility.GetBytesToString(CryptogramConfig.DATA_SIZE)}");
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
            bool encryptResult = Offset.WriteFile.EncryptFile(tempFile, dummySize);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptWriteFile] Offset.WriteFile.EncryptFile execution time: {stopwatch.Elapsed.TotalMilliseconds} ms, CryptogramConfig.DATA_SIZE: {BundleUtility.GetBytesToString(CryptogramConfig.DATA_SIZE)}");
            Assert.IsTrue(encryptResult, "File encryption failed");

            stopwatch.Reset();

            stopwatch.Start();
            bool decryptResult = Offset.WriteFile.DecryptFile(tempFile, dummySize);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptWriteFile] Offset.WriteFile.DecryptFile execution time: {stopwatch.Elapsed.TotalMilliseconds} ms, CryptogramConfig.DATA_SIZE: {BundleUtility.GetBytesToString(CryptogramConfig.DATA_SIZE)}");
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
            byte[] encryptedBytes = Offset.EncryptBytes(tempFile, dummySize);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptBytesFromFile] Offset.EncryptBytes execution time: {stopwatch.Elapsed.TotalMilliseconds} ms, CryptogramConfig.DATA_SIZE: {BundleUtility.GetBytesToString(CryptogramConfig.DATA_SIZE)}");
            Assert.IsNotNull(encryptedBytes, "Encrypted bytes returned null");
            Assert.IsNotEmpty(encryptedBytes, "Encrypted bytes are empty");

            stopwatch.Reset();

            string encryptedFile = tempFile + ".enc";
            File.WriteAllBytes(encryptedFile, encryptedBytes);

            stopwatch.Start();
            byte[] decryptedBytes = Offset.DecryptBytes(encryptedFile, dummySize);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptBytesFromFile] Offset.DecryptBytes execution time: {stopwatch.Elapsed.TotalMilliseconds} ms, CryptogramConfig.DATA_SIZE: {BundleUtility.GetBytesToString(CryptogramConfig.DATA_SIZE)}");
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

            bool encryptResult = Offset.WriteFile.EncryptFile(tempFile, dummySize);
            Assert.IsTrue(encryptResult, "File encryption failed");

            stopwatch.Start();
            using (Stream decryptedStream = Offset.DecryptStream(tempFile, dummySize))
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
            UnityEngine.Debug.Log($"[DecryptStream] Offset.DecryptStream execution time: {stopwatch.Elapsed.TotalMilliseconds} ms, CryptogramConfig.DATA_SIZE: {BundleUtility.GetBytesToString(CryptogramConfig.DATA_SIZE)}");

            File.Delete(tempFile);
        }
    }
}
