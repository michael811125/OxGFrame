using System;
using System.IO;
using YooAsset;

namespace OxGFrame.AssetLoader.Bundle
{
    public class OffsetDecryption : IDecryptionServices
    {
        public uint GetManagedReadBufferSize()
        {
            return 1024;
        }

        public ulong LoadFromFileOffset(DecryptFileInfo fileInfo)
        {
            throw new NotImplementedException();
        }

        public byte[] LoadFromMemory(DecryptFileInfo fileInfo)
        {
            throw new NotImplementedException();
        }

        public Stream LoadFromStream(DecryptFileInfo fileInfo)
        {
            string[] cryptogramArgs = BundleConfig.cryptogramArgs;

            string filePath = fileInfo.FilePath;

            int dummySize = Convert.ToInt32(cryptogramArgs[1]);

            Stream fs = FileCryptogram.Offset.OffsetDecryptStream(filePath, dummySize);

            return fs;
        }
    }

    public class XorDecryption : IDecryptionServices
    {
        public uint GetManagedReadBufferSize()
        {
            return 1024;
        }

        public ulong LoadFromFileOffset(DecryptFileInfo fileInfo)
        {
            throw new NotImplementedException();
        }

        public byte[] LoadFromMemory(DecryptFileInfo fileInfo)
        {
            throw new NotImplementedException();
        }

        public Stream LoadFromStream(DecryptFileInfo fileInfo)
        {
            string[] cryptogramArgs = BundleConfig.cryptogramArgs;

            string filePath = fileInfo.FilePath;

            byte xorKey = Convert.ToByte(cryptogramArgs[1]);

            Stream fs = FileCryptogram.XOR.XorDecryptStream(filePath, xorKey);

            return fs;
        }
    }

    public class HTXorDecryption : IDecryptionServices
    {
        public uint GetManagedReadBufferSize()
        {
            return 1024;
        }

        public ulong LoadFromFileOffset(DecryptFileInfo fileInfo)
        {
            throw new NotImplementedException();
        }

        public byte[] LoadFromMemory(DecryptFileInfo fileInfo)
        {
            throw new NotImplementedException();
        }

        public Stream LoadFromStream(DecryptFileInfo fileInfo)
        {
            string[] cryptogramArgs = BundleConfig.cryptogramArgs;

            string filePath = fileInfo.FilePath;

            byte hXorkey = Convert.ToByte(cryptogramArgs[1]);
            byte tXorkey = Convert.ToByte(cryptogramArgs[2]);

            Stream fs = FileCryptogram.HTXOR.HTXorDecryptStream(filePath, hXorkey, tXorkey);

            return fs;
        }
    }

    public class AesDecryption : IDecryptionServices
    {
        public uint GetManagedReadBufferSize()
        {
            return 1024;
        }

        public ulong LoadFromFileOffset(DecryptFileInfo fileInfo)
        {
            throw new NotImplementedException();
        }

        public byte[] LoadFromMemory(DecryptFileInfo fileInfo)
        {
            throw new NotImplementedException();
        }

        public Stream LoadFromStream(DecryptFileInfo fileInfo)
        {
            string[] cryptogramArgs = BundleConfig.cryptogramArgs;

            string filePath = fileInfo.FilePath;

            string aesKey = cryptogramArgs[1];
            string aesIv = cryptogramArgs[2];

            Stream fs = FileCryptogram.AES.AesDecryptStream(filePath, aesKey, aesIv);

            return fs;
        }
    }
}
