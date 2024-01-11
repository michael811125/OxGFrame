using System;
using System.IO;
using System.Security.Cryptography;

namespace OxGFrame.AssetLoader.Utility.SecureMemory
{
    internal class SecureString : IDisposable
    {
        // Secured
        private bool _secured = true;

        // Opaque Data
        private byte[] _opaqueData = null;

        // Salt
        private byte[] _salt = null;

        // Dummy
        private int _l1;
        private int _l2;

        public SecureString(string input, bool secured = true, int saltSize = 1 << 4, int dummySize = 1 << 5)
        {
            // Enabled encrypt
            this._secured = secured;

            // Dummy
            if (dummySize < 1 << 1) dummySize = 1 << 1;

            // Random dummy size
            Random rnd = new Random();
            this._l1 = rnd.Next(dummySize >> 1, dummySize + 1);
            this._l2 = rnd.Next(dummySize >> 1, dummySize + 1);

            if (this._secured)
            {
                // Salt
                if (saltSize < 1 << 1) saltSize = 1 << 1;
                this._GenerateSalt(saltSize);

                // Encrypt data with aes
                this._opaqueData = this._Encrypt(input);
            }
            // String to bytes with dummy
            else this._opaqueData = StringWithDummy.StringToBytesWithDummy(input, this._l1, this._l2);
        }

        private void _GenerateSalt(int saltSize)
        {
            using (var random = new RNGCryptoServiceProvider())
            {
                this._salt = new byte[saltSize];
                random.GetBytes(this._salt);
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

                aesAlg.GenerateIV();
                IV = aesAlg.IV;

                aesAlg.Mode = CipherMode.CBC;

                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption. 
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            // Write all data to the stream.
                            swEncrypt.Write(input);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            var combinedIvCt = new byte[IV.Length + encrypted.Length];
            Array.Copy(IV, 0, combinedIvCt, 0, IV.Length);
            Array.Copy(encrypted, 0, combinedIvCt, IV.Length, encrypted.Length);

            // Return the encrypted bytes from the memory stream. 
            return combinedIvCt;

        }

        public string Decrypt()
        {
            if (!this._secured) return StringWithDummy.BytesWithDummyToString(this._opaqueData, this._l1, this._l2);

            // Declare the string used to hold 
            // the decrypted text. 
            string plaintext = null;

            // Create an Aes object 
            // with the specified key and IV. 
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = this._salt;

                byte[] IV = new byte[aesAlg.BlockSize >> 3];
                byte[] cipherText = new byte[this._opaqueData.Length - IV.Length];

                Array.Copy(this._opaqueData, IV, IV.Length);
                Array.Copy(this._opaqueData, IV.Length, cipherText, 0, cipherText.Length);

                aesAlg.IV = IV;

                aesAlg.Mode = CipherMode.CBC;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption. 
                using (var msDecrypt = new MemoryStream(cipherText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }

        public void Dispose()
        {
            if (this._opaqueData != null) Array.Clear(this._opaqueData, 0, this._opaqueData.Length);
            this._opaqueData = null;
            if (this._salt != null) Array.Clear(this._salt, 0, this._salt.Length);
            this._salt = null;
        }
    }
}