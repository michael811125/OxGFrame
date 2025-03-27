using NUnit.Framework;
using OxGFrame.AssetLoader.Utility;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using static OxGFrame.AssetLoader.Bundle.FileCryptogram;

namespace OxGFrame.AssetLoader.Editor.Tests
{
    public class OffsetXORTests
    {
        internal readonly byte key = 1;
        internal readonly int dummySize = 128;

        internal readonly ulong dataSize = 1024 * 1024;

        [Test]
        public void EncryptDecryptBytesFromData()
        {
            Stopwatch stopwatch = new Stopwatch();

            byte[] testBytes = new byte[dataSize];
            new Random().NextBytes(testBytes);
            byte[] originalBytes = (byte[])testBytes.Clone();

            stopwatch.Start();
            bool encryptResult = OffsetXOR.EncryptBytes(testBytes, key, dummySize);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptBytesFromData] OffsetXOR.EncryptBytes execution time: {stopwatch.ElapsedMilliseconds} ms, DataSize: {BundleUtility.GetBytesToString(dataSize)}");
            Assert.IsTrue(encryptResult, "In-place encryption failed");

            stopwatch.Reset();

            stopwatch.Start();
            bool decryptResult = OffsetXOR.DecryptBytes(testBytes, key, dummySize);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptBytesFromData] OffsetXOR.DecryptBytes execution time: {stopwatch.ElapsedMilliseconds} ms, DataSize: {BundleUtility.GetBytesToString(dataSize)}");
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
            bool encryptResult = OffsetXOR.WriteFile.EncryptFile(tempFile, key, dummySize);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptWriteFile] OffsetXOR.WriteFile.EncryptFile execution time: {stopwatch.ElapsedMilliseconds} ms, DataSize: {BundleUtility.GetBytesToString(dataSize)}");
            Assert.IsTrue(encryptResult, "File encryption failed");

            stopwatch.Reset();

            stopwatch.Start();
            bool decryptResult = OffsetXOR.WriteFile.DecryptFile(tempFile, key, dummySize);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptWriteFile] OffsetXOR.WriteFile.DecryptFile execution time: {stopwatch.ElapsedMilliseconds} ms, DataSize: {BundleUtility.GetBytesToString(dataSize)}");
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
            byte[] encryptedBytes = OffsetXOR.EncryptBytes(tempFile, key, dummySize);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptBytesFromFile] OffsetXOR.EncryptBytes execution time: {stopwatch.ElapsedMilliseconds} ms, DataSize: {BundleUtility.GetBytesToString(dataSize)}");
            Assert.IsNotNull(encryptedBytes, "Encrypted bytes returned null");
            Assert.IsNotEmpty(encryptedBytes, "Encrypted bytes are empty");

            stopwatch.Reset();

            string encryptedFile = tempFile + ".enc";
            File.WriteAllBytes(encryptedFile, encryptedBytes);

            stopwatch.Start();
            byte[] decryptedBytes = OffsetXOR.DecryptBytes(encryptedFile, key, dummySize);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptBytesFromFile] OffsetXOR.DecryptBytes execution time: {stopwatch.ElapsedMilliseconds} ms, DataSize: {BundleUtility.GetBytesToString(dataSize)}");
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

            bool encryptResult = OffsetXOR.WriteFile.EncryptFile(tempFile, key, dummySize);
            Assert.IsTrue(encryptResult, "File encryption failed");

            stopwatch.Start();
            using (Stream decryptedStream = OffsetXOR.DecryptStream(tempFile, key, dummySize))
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
            UnityEngine.Debug.Log($"[DecryptStream] OffsetXOR.DecryptStream execution time: {stopwatch.ElapsedMilliseconds} ms, DataSize: {BundleUtility.GetBytesToString(dataSize)}");

            File.Delete(tempFile);
        }
    }
}
