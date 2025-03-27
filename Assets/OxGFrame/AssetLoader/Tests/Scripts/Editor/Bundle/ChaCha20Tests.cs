using NUnit.Framework;
using OxGFrame.AssetLoader.Utility;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using static OxGFrame.AssetLoader.Bundle.FileCryptogram;

namespace OxGFrame.AssetLoader.Editor.Tests
{
    public class ChaCha20Tests
    {
        internal readonly string key = "8F395535C4BA65A9E3EFC7C6BDDC1";
        internal readonly string nonce = "nrKpHnV7F1";
        internal readonly uint counter = 2;

        internal readonly ulong dataSize = 1024 * 1024;

        [Test]
        public void EncryptDecryptBytesFromData()
        {
            Stopwatch stopwatch = new Stopwatch();

            byte[] testBytes = new byte[dataSize];
            new Random().NextBytes(testBytes);
            byte[] originalBytes = (byte[])testBytes.Clone();

            stopwatch.Start();
            bool encryptResult = ChaCha20.EncryptBytes(ref testBytes, key, nonce, counter);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptBytesFromData] ChaCha20.EncryptBytes execution time: {stopwatch.ElapsedMilliseconds} ms, DataSize: {BundleUtility.GetBytesToString(dataSize)}");
            Assert.IsTrue(encryptResult, "In-place encryption failed");

            stopwatch.Reset();

            stopwatch.Start();
            bool decryptResult = ChaCha20.DecryptBytes(ref testBytes, key, nonce, counter);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptBytesFromData] ChaCha20.DecryptBytes execution time: {stopwatch.ElapsedMilliseconds} ms, DataSize: {BundleUtility.GetBytesToString(dataSize)}");
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
            bool encryptResult = ChaCha20.WriteFile.EncryptFile(tempFile, key, nonce, counter);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptWriteFile] ChaCha20.WriteFile.EncryptFile execution time: {stopwatch.ElapsedMilliseconds} ms, DataSize: {BundleUtility.GetBytesToString(dataSize)}");
            Assert.IsTrue(encryptResult, "File encryption failed");

            stopwatch.Reset();

            stopwatch.Start();
            bool decryptResult = ChaCha20.WriteFile.DecryptFile(tempFile, key, nonce, counter);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptWriteFile] ChaCha20.WriteFile.DecryptFile execution time: {stopwatch.ElapsedMilliseconds} ms, DataSize: {BundleUtility.GetBytesToString(dataSize)}");
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
            byte[] encryptedBytes = ChaCha20.EncryptBytes(tempFile, key, nonce, counter);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptBytesFromFile] ChaCha20.EncryptBytes execution time: {stopwatch.ElapsedMilliseconds} ms, DataSize: {BundleUtility.GetBytesToString(dataSize)}");
            Assert.IsNotNull(encryptedBytes, "Encrypted bytes returned null");
            Assert.IsNotEmpty(encryptedBytes, "Encrypted bytes are empty");

            stopwatch.Reset();

            string encryptedFile = tempFile + ".enc";
            File.WriteAllBytes(encryptedFile, encryptedBytes);

            stopwatch.Start();
            byte[] decryptedBytes = ChaCha20.DecryptBytes(encryptedFile, key, nonce, counter);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EncryptDecryptBytesFromFile] ChaCha20.DecryptBytes execution time: {stopwatch.ElapsedMilliseconds} ms, DataSize: {BundleUtility.GetBytesToString(dataSize)}");
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


            bool encryptResult = ChaCha20.WriteFile.EncryptFile(tempFile, key, nonce, counter);
            Assert.IsTrue(encryptResult, "File encryption failed");

            stopwatch.Start();
            using (Stream decryptedStream = ChaCha20.DecryptStream(tempFile, key, nonce, counter))
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
            UnityEngine.Debug.Log($"[DecryptStream] ChaCha20.DecryptStream execution time: {stopwatch.ElapsedMilliseconds} ms, DataSize: {BundleUtility.GetBytesToString(dataSize)}");

            File.Delete(tempFile);
        }
    }
}
