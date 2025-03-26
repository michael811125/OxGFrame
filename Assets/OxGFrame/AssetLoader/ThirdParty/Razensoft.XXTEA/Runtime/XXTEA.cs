#nullable enable

using System;
using System.Security.Cryptography;
using System.Text;

namespace Razensoft
{
    public sealed class XXTEA
    {
        private static UTF8Encoding UTF8NoBom => new(
            encoderShouldEmitUTF8Identifier: false,
            throwOnInvalidBytes: true
        );

        private readonly uint[] _key;

        public XXTEA(string key)
        {
            key = key ?? throw new ArgumentNullException(nameof(key));
            _key = FixKey(key);
        }

        public XXTEA(byte[] key)
        {
            key = key ?? throw new ArgumentNullException(nameof(key));
            _key = FixKey(key);
        }

        public byte[] Encrypt(string data)
        {
            data = data ?? throw new ArgumentNullException(nameof(data));
            return Encrypt(UTF8NoBom.GetBytes(data));
        }

        public byte[] Encrypt(byte[] data)
        {
            data = data ?? throw new ArgumentNullException(nameof(data));
            return Encrypt(data, _key);
        }

        public static byte[] Encrypt(string data, byte[] key)
        {
            data = data ?? throw new ArgumentNullException(nameof(data));
            key = key ?? throw new ArgumentNullException(nameof(key));
            return Encrypt(UTF8NoBom.GetBytes(data), key);
        }

        public static byte[] Encrypt(byte[] data, string key)
        {
            data = data ?? throw new ArgumentNullException(nameof(data));
            key = key ?? throw new ArgumentNullException(nameof(key));
            return Encrypt(data, UTF8NoBom.GetBytes(key));
        }

        public static byte[] Encrypt(string data, string key)
        {
            data = data ?? throw new ArgumentNullException(nameof(data));
            key = key ?? throw new ArgumentNullException(nameof(key));
            return Encrypt(UTF8NoBom.GetBytes(data), UTF8NoBom.GetBytes(key));
        }

        public static byte[] Encrypt(byte[] data, byte[] key)
        {
            data = data ?? throw new ArgumentNullException(nameof(data));
            key = key ?? throw new ArgumentNullException(nameof(key));
            return Encrypt(data, FixKey(key));
        }

        private static byte[] Encrypt(byte[] data, uint[] key)
        {
            if (data.Length == 0)
            {
                return data;
            }

            var v = ToUInt32Array(data, true);
            var encrypted = Encrypt(v, key);
            return ToByteArray(encrypted, false);
        }

        public string DecryptString(byte[] data)
        {
            data = data ?? throw new ArgumentNullException(nameof(data));
            return UTF8NoBom.GetString(Decrypt(data));
        }

        public byte[] Decrypt(byte[] data)
        {
            data = data ?? throw new ArgumentNullException(nameof(data));
            return Decrypt(data, _key);
        }

        public static byte[] Decrypt(byte[] data, byte[] key)
        {
            data = data ?? throw new ArgumentNullException(nameof(data));
            key = key ?? throw new ArgumentNullException(nameof(key));
            return Decrypt(data, FixKey(key));
        }

        public static byte[] Decrypt(byte[] data, string key)
        {
            data = data ?? throw new ArgumentNullException(nameof(data));
            key = key ?? throw new ArgumentNullException(nameof(key));
            return Decrypt(data, UTF8NoBom.GetBytes(key));
        }

        public static string DecryptString(byte[] data, byte[] key)
        {
            data = data ?? throw new ArgumentNullException(nameof(data));
            key = key ?? throw new ArgumentNullException(nameof(key));
            return UTF8NoBom.GetString(Decrypt(data, key));
        }

        public static string DecryptString(byte[] data, string key)
        {
            data = data ?? throw new ArgumentNullException(nameof(data));
            key = key ?? throw new ArgumentNullException(nameof(key));
            return UTF8NoBom.GetString(Decrypt(data, key));
        }

        private static byte[] Decrypt(byte[] data, uint[] key)
        {
            if (data.Length == 0)
            {
                return data;
            }

            var v = ToUInt32Array(data, false);
            var decrypted = Decrypt(v, key);
            return ToByteArray(decrypted, true);
        }

        private static uint[] FixKey(string key)
        {
            return FixKey(UTF8NoBom.GetBytes(key));
        }

        private static uint[] FixKey(byte[] key)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(key);
                uint[] fixedKey = new uint[4];
                for (int i = 0; i < 4; i++)
                {
                    fixedKey[i] = BitConverter.ToUInt32(hashBytes, i * 4);
                }

                return fixedKey;
            }
        }

        private static uint[] Encrypt(uint[] v, uint[] k)
        {
            const uint delta = 0x9E3779B9;
            var n = v.Length - 1;
            if (n < 1)
            {
                return v;
            }

            var z = v[n];
            uint sum = 0;
            var q = 6 + 52 / (n + 1);
            unchecked
            {
                while (0 < q--)
                {
                    sum += delta;
                    var e = (sum >> 2) & 3;
                    uint y;
                    int p;
                    for (p = 0; p < n; p++)
                    {
                        y = v[p + 1];
                        z = v[p] += MX(sum, y, z, p, e, k);
                    }

                    y = v[0];
                    z = v[n] += MX(sum, y, z, p, e, k);
                }
            }

            return v;
        }

        private static uint[] Decrypt(uint[] v, uint[] k)
        {
            const uint delta = 0x9E3779B9;
            var n = v.Length - 1;
            if (n < 1)
            {
                return v;
            }

            var y = v[0];
            var q = 6 + 52 / (n + 1);
            unchecked
            {
                var sum = (uint)(q * delta);
                while (sum != 0)
                {
                    var e = (sum >> 2) & 3;
                    uint z;
                    int p;
                    for (p = n; p > 0; p--)
                    {
                        z = v[p - 1];
                        y = v[p] -= MX(sum, y, z, p, e, k);
                    }

                    z = v[n];
                    y = v[0] -= MX(sum, y, z, p, e, k);
                    sum -= delta;
                }
            }

            return v;
        }

        private static uint[] ToUInt32Array(byte[] data, bool includeLength)
        {
            var length = data.Length;
            var n = (length & 3) == 0 ? length >> 2 : (length >> 2) + 1;
            uint[] result;
            if (includeLength)
            {
                result = new uint[n + 1];
                result[n] = (uint)length;
            }
            else
            {
                result = new uint[n];
            }

            Buffer.BlockCopy(data, 0, result, 0, length);
            return result;
        }

        private static byte[] ToByteArray(uint[] data, bool includeLength)
        {
            var n = data.Length << 2;
            if (includeLength)
            {
                var m = (int)data[data.Length - 1];
                n -= 4;
                if (m < n - 3 || m > n)
                {
                    throw new Exception("Input data is invalid");
                }

                n = m;
            }

            var result = new byte[n];
            Buffer.BlockCopy(data, 0, result, 0, n);
            return result;
        }

        private static uint MX(uint sum, uint y, uint z, int p, uint e, uint[] k)
        {
            return (((z >> 5) ^ (y << 2)) + ((y >> 3) ^ (z << 4))) ^ ((sum ^ y) + (k[(p & 3) ^ e] ^ z));
        }
    }
}
