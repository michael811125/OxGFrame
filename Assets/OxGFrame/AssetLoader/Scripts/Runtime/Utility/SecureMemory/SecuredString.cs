using System;

namespace OxGFrame.AssetLoader.Utility.SecureMemory
{
    internal class SecuredString : IDisposable
    {
        /// <summary>
        /// 是否啟用加密
        /// </summary>
        private readonly SecuredStringType _securedStringType;

        /// <summary>
        /// 加密方法
        /// </summary>
        private ISecuredString _securedStringService;

        /// <summary>
        /// 釋放方法
        /// </summary>
        private IDisposable _disposeService;

        public SecuredString(string input, SecuredStringType securedStringType, int saltSize = 1 << 4, int dummySize = 1 << 5)
        {
            this._securedStringType = securedStringType;

            switch (this._securedStringType)
            {
                default:
                case SecuredStringType.None:
                    this._securedStringService = new StringNone(input);
                    this._disposeService = this._securedStringService as StringNone;
                    break;
                case SecuredStringType.XORWithDummy:
                    this._securedStringService = new StringDummy(input, dummySize);
                    this._disposeService = this._securedStringService as StringDummy;
                    break;
                case SecuredStringType.AES:
                    this._securedStringService = new StringAes(input, saltSize);
                    this._disposeService = this._securedStringService as StringAes;
                    break;
            }
        }

        ~SecuredString()
        {
            this.Dispose();
        }

        public string Decrypt()
        {
            switch (this._securedStringType)
            {
                default:
                case SecuredStringType.None:
                    return (this._securedStringService as StringNone).BytesToString();
                case SecuredStringType.XORWithDummy:
                    return (this._securedStringService as StringDummy).BytesToString();
                case SecuredStringType.AES:
                    return (this._securedStringService as StringAes).BytesToString();
            }
        }

        public void Dispose()
        {
            if (this._disposeService != null)
                this._disposeService.Dispose();
        }
    }
}