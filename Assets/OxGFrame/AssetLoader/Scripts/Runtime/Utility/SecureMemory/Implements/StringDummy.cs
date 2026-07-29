using System;
using System.Security.Cryptography;

namespace OxGFrame.AssetLoader.Utility.SecureMemory
{
    internal class StringDummy : ISecuredString, IDisposable
    {
        /// <summary>
        /// 加密後的數據
        /// </summary>
        private byte[] _opaqueData;

        /// <summary>
        /// 用於混淆的長度參數 l1
        /// </summary>
        private int _l1;

        /// <summary>
        /// 用於混淆的長度參數 l2
        /// </summary>
        private int _l2;

        /// <summary>
        /// 資源釋放標記
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// 重用 Random 實例, 降低 GC
        /// </summary>
        private static readonly Random _random = new Random();

        public StringDummy(string input, int dummySize)
        {
            if (dummySize < 1 << 1)
                dummySize = 1 << 1;
            this._l1 = _random.Next(dummySize >> 1, dummySize + 1);
            this._l2 = _random.Next(dummySize >> 1, dummySize + 1);
            this._opaqueData = this.StringToBytes(input);
        }

        public byte[] StringToBytes(string input)
        {
            var l1 = this._l1;
            var l2 = this._l2;

            byte[] stringBytes = StringHelper.StringToBytes(input);

            // Generate d1, d2
            byte[] d1 = this.GenerateSaltBytes(l1);
            byte[] d2 = this.GenerateSaltBytes(l2);

            // Encrypt string bytes
            for (int i = 0; i < stringBytes.Length; i++)
            {
                stringBytes[i] ^= d1[l1 - 1 >> 1];
                stringBytes[i] ^= d2[l2 - 1 >> 1];
            }

            // Combine
            byte[] dataWithDummy = new byte[stringBytes.Length + l1 + l2];
            Array.Copy(d1, 0, dataWithDummy, 0, d1.Length);
            Array.Copy(stringBytes, 0, dataWithDummy, d1.Length, stringBytes.Length);
            Array.Copy(d2, 0, dataWithDummy, d1.Length + stringBytes.Length, d2.Length);

            return dataWithDummy;
        }

        public string BytesToString()
        {
            var l1 = this._l1;
            var l2 = this._l2;

            // Extract string bytes
            byte[] stringBytes = new byte[this._opaqueData.Length - l1 - l2];
            Array.Copy(this._opaqueData, l1, stringBytes, 0, stringBytes.Length);

            // Extract d1, d2
            byte[] d1 = new byte[l1];
            byte[] d2 = new byte[l2];
            Array.Copy(this._opaqueData, 0, d1, 0, d1.Length);
            Array.Copy(this._opaqueData, d1.Length + stringBytes.Length, d2, 0, d2.Length);

            // Decrypt string bytes
            for (int i = 0; i < stringBytes.Length; i++)
            {
                stringBytes[i] ^= d2[l2 - 1 >> 1];
                stringBytes[i] ^= d1[l1 - 1 >> 1];
            }
            string result = StringHelper.BytesToString(stringBytes);

            return result;
        }

        public byte[] GenerateSaltBytes(int length)
        {
            byte[] randomBytes = new byte[length];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes);
            }
            return randomBytes;
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
            }
        }
    }
}