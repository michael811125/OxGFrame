using System;
using System.Buffers;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace OxGFrame.AssetLoader.Utility.SecureMemory
{
    internal class StringAes : ISecuredString, IDisposable
    {
        /// <summary>
        /// 加密後的數據
        /// </summary>
        private byte[] _opaqueData;

        /// <summary>
        /// 加密用的 salt
        /// </summary>
        private byte[] _salt;

        /// <summary>
        /// 資源釋放標記
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// 重用 UTF8 編碼實例, 降低 GC
        /// </summary>
        private static readonly UTF8Encoding _utf8Encoding = new UTF8Encoding(false);

        public StringAes(string input, int saltSize)
        {
            if (saltSize < 1 << 1)
                saltSize = 1 << 1;
            this.GenerateSaltBytes(saltSize);
            this._opaqueData = this.StringToBytes(input);
        }

        public byte[] StringToBytes(string input)
        {
            // 開始加密
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

        public string BytesToString()
        {
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

        public byte[] GenerateSaltBytes(int length)
        {
            this._salt = ArrayPool<byte>.Shared.Rent(length);
            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(this._salt, 0, length);
            }
            return this._salt;
        }

        public void Dispose()
        {
            if (!this._disposed)
            {
                this._disposed = true;

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
            }
        }
    }
}
