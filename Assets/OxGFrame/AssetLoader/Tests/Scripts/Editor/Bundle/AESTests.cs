using NUnit.Framework;
using OxGFrame.AssetLoader.Utility;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using static OxGFrame.AssetLoader.Bundle.FileCryptogram;

namespace OxGFrame.AssetLoader.Editor.Tests
{
    public class AESTests
    {
        internal readonly string key = "8F395535C4BA65A9E3EFC7C6BDDC1";
        internal readonly string iv = "nrKpHnV7F1";

        internal readonly ulong dataSize = 1024 * 1024;

        [Test]
        public void EncryptDecryptBytesFromData()
        {
            Stopwatch stopwatch = new Stopwatch();

            byte[] testBytes = new byte[dataSize];
            new Random().NextBytes(testBytes);
            byte[] originalBytes = (byte[])testBytes.Clone();

            stopwatch.Start();
            bool encryptResult = AES.EncryptBytes(ref testBytes, key, iv);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptBytesFromData] AES.EncryptBytes execution time: {stopwatch.ElapsedMilliseconds} ms, DataSize: {BundleUtility.GetBytesToString(dataSize)}");
            Assert.IsTrue(encryptResult, "In-place encryption failed");

            stopwatch.Reset();

            stopwatch.Start();
            bool decryptResult = AES.DecryptBytes(ref testBytes, key, iv);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptBytesFromData] AES.DecryptBytes execution time: {stopwatch.ElapsedMilliseconds} ms, DataSize: {BundleUtility.GetBytesToString(dataSize)}");
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
            bool encryptResult = AES.WriteFile.EncryptFile(tempFile, key, iv);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptWriteFile] AES.WriteFile.EncryptFile execution time: {stopwatch.ElapsedMilliseconds} ms, DataSize: {BundleUtility.GetBytesToString(dataSize)}");
            Assert.IsTrue(encryptResult, "File encryption failed");

            stopwatch.Reset();

            stopwatch.Start();
            bool decryptResult = AES.WriteFile.DecryptFile(tempFile, key, iv);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptWriteFile] AES.WriteFile.DecryptFile execution time: {stopwatch.ElapsedMilliseconds} ms, DataSize: {BundleUtility.GetBytesToString(dataSize)}");
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
            byte[] encryptedBytes = AES.EncryptBytes(tempFile, key, iv);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptBytesFromFile] AES.EncryptBytes execution time: {stopwatch.ElapsedMilliseconds} ms, DataSize: {BundleUtility.GetBytesToString(dataSize)}");
            Assert.IsNotNull(encryptedBytes, "Encrypted bytes returned null");
            Assert.IsNotEmpty(encryptedBytes, "Encrypted bytes are empty");

            stopwatch.Reset();

            string encryptedFile = tempFile + ".enc";
            File.WriteAllBytes(encryptedFile, encryptedBytes);

            stopwatch.Start();
            byte[] decryptedBytes = AES.DecryptBytes(encryptedFile, key, iv);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptBytesFromFile] AES.DecryptBytes execution time: {stopwatch.ElapsedMilliseconds} ms, DataSize: {BundleUtility.GetBytesToString(dataSize)}");
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


            bool encryptResult = AES.WriteFile.EncryptFile(tempFile, key, iv);
            Assert.IsTrue(encryptResult, "File encryption failed");

            stopwatch.Start();
            using (Stream decryptedStream = AES.DecryptStream(tempFile, key, iv))
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
            UnityEngine.Debug.Log($"[DecryptStream] AES.DecryptStream execution time: {stopwatch.ElapsedMilliseconds} ms, DataSize: {BundleUtility.GetBytesToString(dataSize)}");

            File.Delete(tempFile);
        }
    }
}
