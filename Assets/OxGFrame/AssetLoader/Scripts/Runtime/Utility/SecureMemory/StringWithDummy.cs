using System.Security.Cryptography;
using System.Text;
using System;

namespace OxGFrame.AssetLoader.Utility.SecureMemory
{
    internal class StringWithDummy
    {
        public static byte[] StringToBytes(string inputString)
        {
            byte[] stringBytes = Encoding.UTF8.GetBytes(inputString);
            return stringBytes;
        }

        public static string BytesToString(byte[] stringBytes)
        {
            string result = Encoding.UTF8.GetString(stringBytes);
            return result;
        }

        public static byte[] StringToBytesWithDummy(string inputString, int l1, int l2)
        {
            byte[] stringBytes = StringToBytes(inputString);

            // Generate d1, d2
            byte[] d1 = GenerateRandomBytes(l1);
            byte[] d2 = GenerateRandomBytes(l2);

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

        public static string BytesWithDummyToString(byte[] dataWithDummy, int l1, int l2)
        {
            // Extract string bytes
            byte[] stringBytes = new byte[dataWithDummy.Length - l1 - l2];
            Array.Copy(dataWithDummy, l1, stringBytes, 0, stringBytes.Length);

            // Extract d1, d2
            byte[] d1 = new byte[l1];
            byte[] d2 = new byte[l2];
            Array.Copy(dataWithDummy, 0, d1, 0, d1.Length);
            Array.Copy(dataWithDummy, d1.Length + stringBytes.Length, d2, 0, d2.Length);

            // Decrypt string bytes
            for (int i = 0; i < stringBytes.Length; i++)
            {
                stringBytes[i] ^= d2[l2 - 1 >> 1];
                stringBytes[i] ^= d1[l1 - 1 >> 1];
            }
            string result = BytesToString(stringBytes);

            return result;
        }

        public static byte[] GenerateRandomBytes(int length)
        {
            byte[] randomBytes = new byte[length];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes);
            }
            return randomBytes;
        }
    }
}