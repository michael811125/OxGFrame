using System;
using System.Buffers;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace OxGFrame.AssetLoader.Utility.SecureMemory
{
    internal class SecureString : IDisposable
    {
        /// <summary>
        /// 是否啟用加密
        /// </summary>
        private readonly bool _secured;

        /// <summary>
        /// 加密後的數據
        /// </summary>
        private byte[] _opaqueData;

        /// <summary>
        /// 加密用的 salt
        /// </summary>
        private byte[] _salt;

        /// <summary>
        /// 用於混淆的長度參數 l1
        /// </summary>
        private readonly int _l1;

        /// <summary>
        /// 用於混淆的長度參數 l2
        /// </summary>
        private readonly int _l2;

        /// <summary>
        /// 資源釋放標記
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// 重用 Random 實例, 降低 GC
        /// </summary>
        private static readonly Random _random = new Random();

        /// <summary>
        /// 重用 UTF8 編碼實例, 降低 GC
        /// </summary>
        private static readonly UTF8Encoding _utf8Encoding = new UTF8Encoding(false);

        public SecureString(string input, bool secured = true, int saltSize = 1 << 4, int dummySize = 1 << 5)
        {
            this._secured = secured;

            if (dummySize < 1 << 1) dummySize = 1 << 1;

            if (this._secured)
            {
                if (saltSize < 1 << 1) saltSize = 1 << 1;
                this._GenerateSalt(saltSize);
                this._opaqueData = this._Encrypt(input);
            }
            else
            {
                // 使用 lock 來確保 Random 的執行緒安全
                lock (_random)
                {
                    this._l1 = _random.Next(dummySize >> 1, dummySize + 1);
                    this._l2 = _random.Next(dummySize >> 1, dummySize + 1);
                    this._opaqueData = StringWithDummy.StringToBytesWithDummy(input, this._l1, this._l2);
                }
            }
        }

        ~SecureString()
        {
            this.Dispose();
        }

        private void _GenerateSalt(int saltSize)
        {
            this._salt = ArrayPool<byte>.Shared.Rent(saltSize);
            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(this._salt, 0, saltSize);
            }
        }

        private byte[] _Encrypt(string input)
        {
            if (!this._secured) return null;

            byte[] encrypted;
            byte[] IV;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = this._salt;

                // 從池中租用 IV buffer
                IV = ArrayPool<byte>.Shared.Rent(aesAlg.BlockSize >> 3);
                aesAlg.GenerateIV();
                Array.Copy(aesAlg.IV, IV, aesAlg.IV.Length);

                aesAlg.Mode = CipherMode.CBC;

                // 預估加密後的大小 (粗略估計, 可根據實際情況調整)
                int estimatedSize = (input.Length * 2) + 32;
                var msEncrypt = new MemoryStream(estimatedSize);

                using (var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, IV))
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    // 直接使用 UTF8 編碼寫入, 避免 StreamWriter 的額外開銷
                    byte[] inputBytes = _utf8Encoding.GetBytes(input);
                    csEncrypt.Write(inputBytes, 0, inputBytes.Length);
                }

                encrypted = msEncrypt.ToArray();
                msEncrypt.Dispose();
            }

            // 組合 IV 和加密數據
            var result = new byte[IV.Length + encrypted.Length];
            Array.Copy(IV, 0, result, 0, IV.Length);
            Array.Copy(encrypted, 0, result, IV.Length, encrypted.Length);

            // 歸還 IV buffer
            ArrayPool<byte>.Shared.Return(IV);

            return result;
        }

        public string Decrypt()
        {
            if (!this._secured)
                return StringWithDummy.BytesWithDummyToString(this._opaqueData, this._l1, this._l2);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = this._salt;

                // 從池中租用 IV buffer
                int ivLength = aesAlg.BlockSize >> 3;
                byte[] IV = ArrayPool<byte>.Shared.Rent(ivLength);
                byte[] cipherText = new byte[this._opaqueData.Length - ivLength];

                try
                {
                    Array.Copy(this._opaqueData, IV, ivLength);
                    Array.Copy(this._opaqueData, ivLength, cipherText, 0, cipherText.Length);

                    aesAlg.IV = IV;
                    aesAlg.Mode = CipherMode.CBC;

                    using (var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                    using (var msDecrypt = new MemoryStream(cipherText))
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        // 直接將解密後的數據讀入 byte array, 然後轉換為字符串
                        using (var ms = new MemoryStream())
                        {
                            csDecrypt.CopyTo(ms);
                            return _utf8Encoding.GetString(ms.ToArray());
                        }
                    }
                }
                finally
                {
                    // 確保歸還 IV buffer
                    ArrayPool<byte>.Shared.Return(IV);
                }
            }
        }

        public void Dispose()
        {
            if (!this._disposed)
            {
                if (this._opaqueData != null)
                {
                    Array.Clear(this._opaqueData, 0, this._opaqueData.Length);
                    this._opaqueData = null;
                }

                if (this._salt != null)
                {
                    ArrayPool<byte>.Shared.Return(this._salt);
                    this._salt = null;
                }

                this._disposed = true;
            }
        }
    }
}