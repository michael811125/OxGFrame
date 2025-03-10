using System;

namespace OxGFrame.AssetLoader.Utility.SecureMemory
{
    internal class StringNone : ISecuredString, IDisposable
    {
        /// <summary>
        /// 加密後的數據
        /// </summary>
        private byte[] _opaqueData;

        /// <summary>
        /// 資源釋放標記
        /// </summary>
        private bool _disposed = false;

        public StringNone(string input)
        {
            this._opaqueData = this.StringToBytes(input);
        }

        public byte[] StringToBytes(string input)
        {
            return StringHelper.StringToBytes(input);
        }

        public string BytesToString()
        {
            return StringHelper.BytesToString(this._opaqueData);
        }

        public byte[] GenerateSaltBytes(int length)
        {
            return null;
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