using System;
using System.Text;
using UnityEngine;

namespace OxGFrame.Hotfixer
{
    public static class BinaryHelper
    {
        public struct ConfigInfo
        {
            public ConfigFileType type;
            public string content;
        }

        public static byte[] EncryptToBytes(string content)
        {
            byte[] writeBuffer;
            byte[] data = Encoding.UTF8.GetBytes(content);

            // Encrypt
            for (int i = 0; i < data.Length; i++)
            {
                data[i] ^= HotfixConfig.CIPHER << 1;
            }

            // Write data with header
            int pos = 0;
            byte[] dataWithHeader = new byte[data.Length + 2];
            // Write header (non-encrypted)
            WriteInt16(HotfixConfig.CIPHER_HEADER, dataWithHeader, ref pos);
            Buffer.BlockCopy(data, 0, dataWithHeader, pos, data.Length);
            writeBuffer = dataWithHeader;
            return writeBuffer;
        }

        public static ConfigInfo DecryptToString(byte[] data)
        {
            int pos = 0;
            ConfigInfo info = new ConfigInfo();

            // Read header (non-encrypted)
            var header = ReadInt16(data, ref pos);
            if (header == HotfixConfig.CIPHER_HEADER)
            {
                info.type = ConfigFileType.Bytes;

                // Read data without header
                byte[] dataWithoutHeader = new byte[data.Length - 2];
                Buffer.BlockCopy(data, pos, dataWithoutHeader, 0, data.Length - pos);
                // Decrypt
                for (int i = 0; i < dataWithoutHeader.Length; i++)
                {
                    dataWithoutHeader[i] ^= HotfixConfig.CIPHER << 1;
                }

                // To string
                info.content = Encoding.UTF8.GetString(dataWithoutHeader);
                Debug.Log($"[Source is Cipher] Check -> {HotfixConfig.HOTFIX_DLL_CFG_NAME}");
            }
            else
            {
                info.type = ConfigFileType.Json;

                // To string
                info.content = Encoding.UTF8.GetString(data);
                Debug.Log($"[Source is Plaintext] Check -> {HotfixConfig.HOTFIX_DLL_CFG_NAME}");
            }

            return info;
        }

        public static void WriteInt16(short value, byte[] buffer, ref int pos)
        {
            WriteUInt16((ushort)value, buffer, ref pos);
        }

        internal static void WriteUInt16(ushort value, byte[] buffer, ref int pos)
        {
            buffer[pos++] = (byte)value;
            buffer[pos++] = (byte)(value >> 8);
        }

        public static short ReadInt16(byte[] buffer, ref int pos)
        {
            if (BitConverter.IsLittleEndian)
            {
                short value = (short)((buffer[pos]) | (buffer[pos + 1] << 8));
                pos += 2;
                return value;
            }
            else
            {
                short value = (short)((buffer[pos] << 8) | (buffer[pos + 1]));
                pos += 2;
                return value;
            }
        }

        internal static ushort ReadUInt16(byte[] buffer, ref int pos)
        {
            return (ushort)ReadInt16(buffer, ref pos);
        }
    }
}