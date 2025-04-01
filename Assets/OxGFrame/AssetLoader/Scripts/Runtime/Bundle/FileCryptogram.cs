using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace OxGFrame.AssetLoader.Bundle
{
    public class FileCryptogram
    {
        public const int BUFFER_SIZE = 16384;

        public class Offset
        {
            internal const int RANDOM_SEED = int.MaxValue << 1;

            public class WriteFile
            {
                /// <summary>
                /// Offset 加密文件
                /// </summary>
                /// <param name="sourceFile"></param>
                /// <param name="dummySize"></param>
                /// <returns></returns>
                public static bool EncryptFile(string sourceFile, int dummySize)
                {
                    try
                    {
                        // 初始化亂數種子
                        UnityEngine.Random.InitState(RANDOM_SEED);

                        // 取得原始文件的長度
                        long fileLength = new FileInfo(sourceFile).Length;
                        // 計算總長度: 原始文件長度 + dummySize
                        long totalLength = fileLength + dummySize;

                        // 建立緩衝區
                        byte[] buffer = new byte[BUFFER_SIZE];
                        // 用來存放偏移後的字節
                        byte[] offsetBytes = new byte[BUFFER_SIZE];
                        // 已處理的字節數
                        long bytesProcessed = 0;

                        // 使用 FileStream 讀取與寫入文件
                        string tempFile = sourceFile + ".tmp";
                        using (FileStream fsRead = new FileStream(sourceFile, FileMode.Open, FileAccess.Read))
                        using (FileStream fsWrite = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
                        {
                            // 寫入 dummySize 長度的隨機數據
                            for (int i = 0; i < dummySize; i++)
                            {
                                fsWrite.WriteByte((byte)UnityEngine.Random.Range(0, 256));
                            }

                            // 逐步讀取文件並將偏移後的字節寫入
                            int bytesRead;
                            while ((bytesRead = fsRead.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                // 將讀取的數據寫入到目標文件
                                for (int i = 0; i < bytesRead; i++)
                                {
                                    offsetBytes[i] = buffer[i];
                                }
                                fsWrite.Write(offsetBytes, 0, bytesRead);
                                bytesProcessed += bytesRead;
                            }
                        }

                        // 刪除原始文件
                        File.Delete(sourceFile);
                        // 將臨時文件重命名為原始文件
                        File.Move(tempFile, sourceFile);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                        return false;
                    }

                    return true;
                }


                /// <summary>
                /// Offset 解密文件
                /// </summary>
                /// <param name="encryptFile"></param>
                /// <param name="dummySize"></param>
                /// <returns></returns>
                public static bool DecryptFile(string encryptFile, int dummySize)
                {
                    try
                    {
                        // 取得加密文件的長度
                        long fileLength = new FileInfo(encryptFile).Length;
                        // 總長度去掉 dummySize 的部分
                        long totalLength = fileLength - dummySize;

                        // 創建緩衝區
                        byte[] buffer = new byte[BUFFER_SIZE];
                        // 已處理的字節數
                        long bytesProcessed = 0;

                        // 使用 FileStream 逐步讀取加密文件並寫入新文件
                        string tempFile = encryptFile + ".tmp";
                        using (FileStream fsRead = new FileStream(encryptFile, FileMode.Open, FileAccess.Read))
                        using (FileStream fsWrite = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
                        {
                            // 跳過 dummySize 字節
                            fsRead.Seek(dummySize, SeekOrigin.Begin);

                            // 逐步讀取並寫入新文件
                            int bytesRead;
                            while (bytesProcessed < totalLength && (bytesRead = fsRead.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                // 確保不會寫超過需要的部分
                                int bytesToWrite = (int)Math.Min(bytesRead, totalLength - bytesProcessed);
                                fsWrite.Write(buffer, 0, bytesToWrite);
                                bytesProcessed += bytesToWrite;
                            }
                        }

                        // 刪除原文件
                        File.Delete(encryptFile);
                        // 將臨時文件重命名為原文件名
                        File.Move(tempFile, encryptFile);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                        return false;
                    }

                    return true;
                }

            }

            /// <summary>
            /// Offset 加密文件
            /// </summary>
            /// <param name="filePath"></param>
            /// <param name="dummySize"></param>
            /// <returns></returns>
            public static byte[] EncryptBytes(string filePath, int dummySize)
            {
                try
                {
                    // 使用 FileStream 逐步讀取文件
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        // 設置隨機種子
                        UnityEngine.Random.InitState(RANDOM_SEED);

                        // 獲取文件長度
                        long fileLength = fs.Length;
                        long totalLength = fileLength + dummySize;

                        // 創建存放加密數據的 byte[]
                        byte[] encryptedBytes = new byte[totalLength];

                        // 生成 dummySize 的隨機數據
                        for (int i = 0; i < dummySize; i++)
                        {
                            encryptedBytes[i] = (byte)(UnityEngine.Random.Range(0, 256));
                        }

                        // 逐步讀取原始文件數據, 並存入加密後的 byte[]
                        byte[] buffer = new byte[BUFFER_SIZE];
                        int bytesRead;
                        int offset = dummySize;

                        while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            Buffer.BlockCopy(buffer, 0, encryptedBytes, offset, bytesRead);
                            offset += bytesRead;
                        }

                        // 返回加密後的 byte[]
                        return encryptedBytes;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return null;
                }
            }

            /// <summary>
            /// Offset 解密文件
            /// </summary>
            /// <param name="filePath"></param>
            /// <param name="dummySize"></param>
            /// <returns></returns>
            public static byte[] DecryptBytes(string filePath, int dummySize)
            {
                try
                {
                    // 獲取文件長度
                    long fileLength = new FileInfo(filePath).Length;
                    long totalLength = fileLength - dummySize;

                    // 使用 FileStream 來讀取文件
                    using (FileStream fsDecrypt = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        // 跳過 dummySize
                        fsDecrypt.Seek(dummySize, SeekOrigin.Begin);

                        // 創建一個 Span<byte> 來存放解密後的資料
                        byte[] resultArray = new byte[totalLength];
                        Span<byte> resultSpan = new Span<byte>(resultArray);

                        // 逐步讀取數據
                        int offset = 0;
                        byte[] buffer = new byte[BUFFER_SIZE];
                        while (offset < totalLength)
                        {
                            int bytesRead = fsDecrypt.Read(buffer, 0, buffer.Length);
                            if (bytesRead == 0)
                                break;

                            int bytesToWrite = Math.Min(bytesRead, (int)(totalLength - offset));
                            // 使用 Span.Slice 來指定目標區域
                            new Span<byte>(resultArray, offset, bytesToWrite).Clear();
                            new Span<byte>(buffer, 0, bytesRead).Slice(0, bytesToWrite).CopyTo(resultSpan.Slice(offset, bytesToWrite));
                            offset += bytesToWrite;
                        }

                        // 返回解密後的 byte[]
                        return resultArray;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return null;
                }
            }

            /// <summary>
            /// Offset 加密文件
            /// </summary>
            /// <param name="rawBytes"></param>
            /// <param name="dummySize"></param>
            /// <returns></returns>
            public static bool EncryptBytes(ref byte[] rawBytes, int dummySize)
            {
                try
                {
                    UnityEngine.Random.InitState(RANDOM_SEED);

                    byte[] dataBytes = rawBytes;
                    int totalLength = dataBytes.Length + dummySize;
                    byte[] offsetDatabytes = new byte[totalLength];
                    for (int i = 0; i < totalLength; i++)
                    {
                        if (dummySize > 0 && i < dummySize)
                            offsetDatabytes[i] = (byte)(UnityEngine.Random.Range(0, 256));
                        else
                            offsetDatabytes[i] = dataBytes[i - dummySize];
                    }
                    rawBytes = offsetDatabytes;
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return false;
                }

                return true;
            }

            /// <summary>
            /// Offset 解密文件
            /// </summary>
            /// <param name="encryptBytes"></param>
            /// <param name="dummySize"></param>
            /// <returns></returns>
            public static bool DecryptBytes(ref byte[] encryptBytes, int dummySize)
            {
                try
                {
                    int totalLength = encryptBytes.Length - dummySize;
                    Span<byte> encryptSpan = encryptBytes.AsSpan(dummySize, totalLength);
                    encryptBytes = encryptSpan.ToArray();
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return false;
                }

                return true;
            }

            /// <summary>
            /// 返回 Offset 解密 Stream
            /// </summary>
            /// <param name="encryptFile"></param>
            /// <param name="dummySize"></param>
            /// <returns></returns>
            public static Stream DecryptStream(string encryptFile, int dummySize)
            {
                try
                {
                    using (var fsDecrypt = new FileStream(encryptFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        long totalLength = fsDecrypt.Length - dummySize;
                        var msDecrypt = new MemoryStream((int)totalLength);

                        // 跳過 dummySize
                        fsDecrypt.Seek(dummySize, SeekOrigin.Begin);

                        // 創建緩衝區進行讀取和寫入操作
                        byte[] buffer = new byte[BUFFER_SIZE];
                        int bytesRead;
                        long bytesToRead = totalLength;

                        while (bytesToRead > 0 && (bytesRead = fsDecrypt.Read(buffer, 0, (int)Math.Min(buffer.Length, bytesToRead))) > 0)
                        {
                            // 寫入 MemoryStream
                            msDecrypt.Write(buffer, 0, bytesRead);
                            // 減少剩餘需讀取的字節數
                            bytesToRead -= bytesRead;
                        }

                        // 將 MemoryStream 的位置設置為開頭以便後續讀取
                        msDecrypt.Position = 0;
                        return msDecrypt;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return null;
                }
            }
        }

        public class XOR
        {
            public class WriteFile
            {
                /// <summary>
                /// XOR 加密文件
                /// </summary>
                /// <param name="sourceFile"></param>
                /// <param name="key"></param>
                /// <returns></returns>
                public static bool EncryptFile(string sourceFile, byte key)
                {
                    try
                    {
                        // 使用 FileStream 逐步讀取和寫入文件
                        string tempFile = sourceFile + ".tmp";
                        using (FileStream fsRead = new FileStream(sourceFile, FileMode.Open, FileAccess.Read))
                        using (FileStream fsWrite = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
                        {
                            byte[] buffer = new byte[BUFFER_SIZE];
                            int bytesRead;

                            while ((bytesRead = fsRead.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                for (int i = 0; i < bytesRead; i++)
                                {
                                    // 使用 XOR 加密
                                    buffer[i] ^= key;
                                }

                                // 寫入加密後的數據
                                fsWrite.Write(buffer, 0, bytesRead);
                            }
                        }

                        // 刪除原始文件
                        File.Delete(sourceFile);
                        // 將臨時文件重命名為原始文件
                        File.Move(tempFile, sourceFile);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                        return false;
                    }

                    return true;
                }

                /// <summary>
                /// XOR 解密文件
                /// </summary>
                /// <param name="encryptFile"></param>
                /// <param name="key"></param>
                /// <returns></returns>
                public static bool DecryptFile(string encryptFile, byte key)
                {
                    // 解密的邏輯與加密相同
                    return EncryptFile(encryptFile, key);
                }
            }

            /// <summary>
            /// XOR 加密文件
            /// </summary>
            /// <param name="filePath"></param>
            /// <param name="key"></param>
            /// <returns></returns>
            public static byte[] EncryptBytes(string filePath, byte key)
            {
                try
                {
                    // 使用 FileStream 逐步讀取文件
                    using (FileStream fsRead = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    using (MemoryStream msWrite = new MemoryStream((int)fsRead.Length))
                    {
                        byte[] buffer = new byte[BUFFER_SIZE];
                        int bytesRead;

                        while ((bytesRead = fsRead.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            for (int i = 0; i < bytesRead; i++)
                            {
                                // 使用 XOR 加密
                                buffer[i] ^= key;
                            }

                            // 寫入加密後的數據
                            msWrite.Write(buffer, 0, bytesRead);
                        }

                        // 返回加密後的字節數組
                        return msWrite.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return null;
                }
            }

            /// <summary>
            /// XOR 解密文件
            /// </summary>
            /// <param name="filePath"></param>
            /// <param name="key"></param>
            /// <returns></returns>
            public static byte[] DecryptBytes(string filePath, byte key)
            {
                // 解密的邏輯與加密相同
                return EncryptBytes(filePath, key);
            }

            /// <summary>
            /// XOR 加密文件
            /// </summary>
            /// <param name="rawBytes"></param>
            /// <param name="key"></param>
            /// <returns></returns>
            public static bool EncryptBytes(byte[] rawBytes, byte key)
            {
                try
                {
                    for (int i = 0; i < rawBytes.Length; i++)
                    {
                        rawBytes[i] ^= key;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return false;
                }

                return true;
            }

            /// <summary>
            /// XOR 解密文件
            /// </summary>
            /// <param name="encryptBytes"></param>
            /// <param name="key"></param>
            /// <returns></returns>
            public static bool DecryptBytes(byte[] encryptBytes, byte key)
            {
                try
                {
                    for (int i = 0; i < encryptBytes.Length; i++)
                    {
                        encryptBytes[i] ^= key;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return false;
                }

                return true;
            }

            /// <summary>
            /// 返回 XOR 解密 Stream
            /// </summary>
            /// <param name="encryptFile"></param>
            /// <param name="key"></param>
            /// <returns></returns>
            public static Stream DecryptStream(string encryptFile, byte key)
            {
                try
                {
                    // 建立 FileStream 以逐步讀取加密文件
                    using (FileStream fsDecrypt = new FileStream(encryptFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        // 創建 MemoryStream 用於存放解密後的數據
                        MemoryStream msDecrypt = new MemoryStream((int)fsDecrypt.Length);

                        // 使用一個緩衝區來逐步讀取文件
                        byte[] buffer = new byte[BUFFER_SIZE];
                        int bytesRead;

                        // 逐步讀取文件內容, 並對每個字節進行 XOR 解密
                        while ((bytesRead = fsDecrypt.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            for (int i = 0; i < bytesRead; i++)
                            {
                                // 進行 XOR 解密
                                buffer[i] ^= key;
                            }

                            // 將解密後的數據寫入 MemoryStream
                            msDecrypt.Write(buffer, 0, bytesRead);
                        }

                        // 將 MemoryStream 的位置設置為開頭以便後續讀取
                        msDecrypt.Position = 0;
                        return msDecrypt;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return null;
                }
            }
        }

        public class HT2XOR
        {
            public class WriteFile
            {
                /// <summary>
                /// Head-Tail 2 XOR 加密文件
                /// </summary>
                /// <param name="sourceFile"></param>
                /// <param name="hKey"></param>
                /// <param name="tKey"></param>
                /// <param name="jKey"></param>
                /// <returns></returns>
                public static bool EncryptFile(string sourceFile, byte hKey, byte tKey, byte jKey)
                {
                    try
                    {
                        using (var stream = new FileStream(sourceFile, FileMode.Open, FileAccess.ReadWrite))
                        {
                            byte[] buffer = new byte[BUFFER_SIZE];
                            long fileLength = stream.Length;

                            // 加密文件頭
                            stream.Read(buffer, 0, 1);
                            buffer[0] ^= hKey;
                            stream.Position = 0;
                            stream.Write(buffer, 0, 1);

                            // 加密文件尾
                            stream.Position = fileLength - 1;
                            stream.Read(buffer, 0, 1);
                            buffer[0] ^= tKey;
                            stream.Position = fileLength - 1;
                            stream.Write(buffer, 0, 1);

                            // 每隔 2 字節加密
                            stream.Position = 0;
                            int bytesRead;
                            long position = 0;

                            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                for (int i = 0; i < bytesRead; i += 2)
                                {
                                    if (position + i < fileLength)
                                    {
                                        buffer[i] ^= jKey;
                                    }
                                }

                                stream.Position = position;
                                stream.Write(buffer, 0, bytesRead);
                                position += bytesRead;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                        return false;
                    }

                    return true;
                }

                /// <summary>
                /// Head-Tail 2 XOR 解密文件
                /// </summary>
                /// <param name="encryptFile"></param>
                /// <param name="hKey"></param>
                /// <param name="tKey"></param>
                /// <param name="jKey"></param>
                /// <returns></returns>
                public static bool DecryptFile(string encryptFile, byte hKey, byte tKey, byte jKey)
                {
                    try
                    {
                        using (var stream = new FileStream(encryptFile, FileMode.Open, FileAccess.ReadWrite))
                        {
                            byte[] buffer = new byte[BUFFER_SIZE];
                            long fileLength = stream.Length;

                            // 首先處理主體部分 (每隔 2 字節解密)
                            long position = 0;
                            int bytesRead;
                            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                for (int i = 0; i < bytesRead; i += 2)
                                {
                                    if (position + i < fileLength)
                                    {
                                        buffer[i] ^= jKey;
                                    }
                                }

                                stream.Position = position;
                                stream.Write(buffer, 0, bytesRead);
                                position += bytesRead;
                            }

                            // 解密文件頭
                            stream.Position = 0;
                            stream.Read(buffer, 0, 1);
                            buffer[0] ^= hKey;
                            stream.Position = 0;
                            stream.Write(buffer, 0, 1);

                            // 解密文件尾
                            stream.Position = fileLength - 1;
                            stream.Read(buffer, 0, 1);
                            buffer[0] ^= tKey;
                            stream.Position = fileLength - 1;
                            stream.Write(buffer, 0, 1);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                        return false;
                    }

                    return true;
                }
            }

            /// <summary>
            /// Head-Tail 2 XOR 加密文件
            /// </summary>
            /// <param name="filePath"></param>
            /// <param name="hKey"></param>
            /// <param name="tKey"></param>
            /// <param name="jKey"></param>
            /// <returns></returns>
            public static byte[] EncryptBytes(string filePath, byte hKey, byte tKey, byte jKey)
            {
                try
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        byte[] buffer = new byte[BUFFER_SIZE];
                        long fileLength = fileStream.Length;

                        using (var memoryStream = new MemoryStream((int)fileLength))
                        {
                            int bytesRead;
                            long totalBytesRead = 0;

                            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                // 處理第一個字節 (文件頭)
                                if (totalBytesRead == 0)
                                {
                                    buffer[0] ^= hKey;
                                }

                                // 處理最後一個字節 (文件尾)
                                if (totalBytesRead + bytesRead == fileLength)
                                {
                                    buffer[bytesRead - 1] ^= tKey;
                                }

                                // 每隔 2 字節加密
                                for (int i = 0; i < bytesRead; i += 2)
                                {
                                    buffer[i] ^= jKey;
                                }

                                memoryStream.Write(buffer, 0, bytesRead);
                                totalBytesRead += bytesRead;
                            }

                            return memoryStream.ToArray();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return null;
                }
            }

            /// <summary>
            /// Head-Tail 2 XOR 解密文件
            /// </summary>
            /// <param name="filePath"></param>
            /// <param name="hKey"></param>
            /// <param name="tKey"></param>
            /// <param name="jKey"></param>
            /// <returns></returns>
            public static byte[] DecryptBytes(string filePath, byte hKey, byte tKey, byte jKey)
            {
                try
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        byte[] buffer = new byte[BUFFER_SIZE];
                        long fileLength = fileStream.Length;

                        using (var memoryStream = new MemoryStream((int)fileLength))
                        {
                            int bytesRead;
                            long totalBytesRead = 0;

                            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                // 每隔 2 字節解密
                                for (int i = 0; i < bytesRead; i += 2)
                                {
                                    buffer[i] ^= jKey;
                                }

                                // 處理第一個字節 (文件頭)
                                if (totalBytesRead == 0)
                                {
                                    buffer[0] ^= hKey;
                                }

                                // 處理最後一個字節 (文件尾)
                                if (totalBytesRead + bytesRead == fileLength)
                                {
                                    buffer[bytesRead - 1] ^= tKey;
                                }

                                memoryStream.Write(buffer, 0, bytesRead);
                                totalBytesRead += bytesRead;
                            }

                            return memoryStream.ToArray();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return null;
                }
            }

            /// <summary>
            /// Head-Tail 2 XOR 加密文件
            /// </summary>
            /// <param name="rawBytes"></param>
            /// <param name="hKey"></param>
            /// <param name="tKey"></param>
            /// <param name="jKey"></param>
            /// <returns></returns>
            public static bool EncryptBytes(byte[] rawBytes, byte hKey, byte tKey, byte jKey)
            {
                try
                {
                    int length = rawBytes.Length;

                    // head encrypt
                    rawBytes[0] ^= hKey;
                    // tail encrypt
                    rawBytes[length - 1] ^= tKey;

                    // jump 2 encrypt
                    for (int i = 0; i < length >> 1; i++)
                    {
                        rawBytes[i << 1] ^= jKey;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return false;
                }

                return true;
            }

            /// <summary>
            /// Head-Tail 2 XOR 解密文件
            /// </summary>
            /// <param name="encryptBytes"></param>
            /// <param name="hKey"></param>
            /// <param name="tKey"></param>
            /// <param name="jKey"></param>
            /// <returns></returns>
            public static bool DecryptBytes(byte[] encryptBytes, byte hKey, byte tKey, byte jKey)
            {
                try
                {
                    int length = encryptBytes.Length;

                    // jump 2 encrypt
                    for (int i = 0; i < length >> 1; i++)
                    {
                        encryptBytes[i << 1] ^= jKey;
                    }

                    // head encrypt
                    encryptBytes[0] ^= hKey;
                    // tail encrypt
                    encryptBytes[length - 1] ^= tKey;

                    return true;
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return false;
                }
            }

            /// <summary>
            /// 返回 Head-Tail 2 XOR 解密 Stream
            /// </summary>
            /// <param name="encryptFile"></param>
            /// <param name="hKey"></param>
            /// <param name="tKey"></param>
            /// <param name="jKey"></param>
            /// <returns></returns>
            public static Stream DecryptStream(string encryptFile, byte hKey, byte tKey, byte jKey)
            {
                try
                {
                    // 建立 FileStream 以逐步讀取加密文件
                    using (FileStream fsDecrypt = new FileStream(encryptFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        // 使用一個緩衝區來逐步讀取文件
                        byte[] buffer = new byte[BUFFER_SIZE];
                        int bytesRead;
                        long totalBytesRead = 0;
                        long fileLength = fsDecrypt.Length;

                        // 創建 MemoryStream 用於存放解密後的數據
                        MemoryStream msDecrypt = new MemoryStream((int)fileLength);

                        // 逐步讀取文件內容, 並進行解密
                        while ((bytesRead = fsDecrypt.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            // 每隔 2 字節解密
                            for (int i = 0; i < bytesRead; i += 2)
                            {
                                buffer[i] ^= jKey;
                            }

                            // 處理文件頭 (第一個字節)
                            if (totalBytesRead == 0)
                            {
                                buffer[0] ^= hKey;
                            }

                            // 處理文件尾 (最後一個字節)
                            if (totalBytesRead + bytesRead == fileLength)
                            {
                                buffer[bytesRead - 1] ^= tKey;
                            }

                            // 將解密後的數據寫入 MemoryStream
                            msDecrypt.Write(buffer, 0, bytesRead);
                            totalBytesRead += bytesRead;
                        }

                        // 將 MemoryStream 的位置設置為開頭以便後續讀取
                        msDecrypt.Position = 0;
                        return msDecrypt;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return null;
                }
            }
        }

        public class HT2XORPlus
        {
            public class WriteFile
            {
                /// <summary>
                /// Head-Tail 2 XOR Plus 加密文件
                /// </summary>
                /// <param name="sourceFile"></param>
                /// <param name="hKey"></param>
                /// <param name="tKey"></param>
                /// <param name="j1Key"></param>
                /// <param name="j2Key"></param>
                /// <returns></returns>
                public static bool EncryptFile(string sourceFile, byte hKey, byte tKey, byte j1Key, byte j2Key)
                {
                    try
                    {
                        using (var stream = new FileStream(sourceFile, FileMode.Open, FileAccess.ReadWrite))
                        {
                            byte[] buffer = new byte[BUFFER_SIZE];
                            long fileLength = stream.Length;

                            // 加密文件頭
                            stream.Read(buffer, 0, 1);
                            buffer[0] ^= hKey;
                            stream.Position = 0;
                            stream.Write(buffer, 0, 1);

                            // 加密文件尾
                            stream.Position = fileLength - 1;
                            stream.Read(buffer, 0, 1);
                            buffer[0] ^= tKey;
                            stream.Position = fileLength - 1;
                            stream.Write(buffer, 0, 1);

                            // 每隔 2 字節加密
                            stream.Position = 0;
                            int bytesRead;
                            long position = 0;

                            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                for (int i = 0; i < bytesRead; i += 2)
                                {
                                    if (position + i < fileLength)
                                    {
                                        // 每隔 2 字節加密
                                        buffer[i] ^= j1Key;
                                    }

                                    // 確保不超出長度
                                    if (position + i + 1 < fileLength)
                                    {
                                        // 每隔 2 字節加密
                                        buffer[i + 1] ^= j2Key;
                                    }
                                }

                                stream.Position = position;
                                stream.Write(buffer, 0, bytesRead);
                                position += bytesRead;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                        return false;
                    }

                    return true;
                }

                /// <summary>
                /// Head-Tail 2 XOR Plus 解密文件
                /// </summary>
                /// <param name="encryptFile"></param>
                /// <param name="hKey"></param>
                /// <param name="tKey"></param>
                /// <param name="j1Key"></param>
                /// <param name="j2Key"></param>
                /// <returns></returns>
                public static bool DecryptFile(string encryptFile, byte hKey, byte tKey, byte j1Key, byte j2Key)
                {
                    try
                    {
                        using (var stream = new FileStream(encryptFile, FileMode.Open, FileAccess.ReadWrite))
                        {
                            byte[] buffer = new byte[BUFFER_SIZE];
                            long fileLength = stream.Length;

                            // 每隔 2 字節解密
                            stream.Position = 0;
                            int bytesRead;
                            long position = 0;

                            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                for (int i = 0; i < bytesRead; i += 2)
                                {
                                    if (position + i < fileLength)
                                    {
                                        // 每隔 2 字節解密
                                        buffer[i] ^= j1Key;
                                    }

                                    // 確保不超出長度
                                    if (position + i + 1 < fileLength)
                                    {
                                        // 每隔 2 字節解密
                                        buffer[i + 1] ^= j2Key;
                                    }
                                }

                                stream.Position = position;
                                stream.Write(buffer, 0, bytesRead);
                                position += bytesRead;
                            }

                            // 解密文件頭
                            stream.Position = 0;
                            stream.Read(buffer, 0, 1);
                            buffer[0] ^= hKey;
                            stream.Position = 0;
                            stream.Write(buffer, 0, 1);

                            // 解密文件尾
                            stream.Position = fileLength - 1;
                            stream.Read(buffer, 0, 1);
                            buffer[0] ^= tKey;
                            stream.Position = fileLength - 1;
                            stream.Write(buffer, 0, 1);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                        return false;
                    }

                    return true;
                }
            }

            /// <summary>
            /// Head-Tail 2 XOR Plus 加密文件
            /// </summary>
            /// <param name="filePath"></param>
            /// <param name="hKey"></param>
            /// <param name="tKey"></param>
            /// <param name="j1Key"></param>
            /// <param name="j2Key"></param>
            /// <returns></returns>
            public static byte[] EncryptBytes(string filePath, byte hKey, byte tKey, byte j1Key, byte j2Key)
            {
                try
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        byte[] buffer = new byte[BUFFER_SIZE];
                        long fileLength = fileStream.Length;

                        using (var memoryStream = new MemoryStream((int)fileLength))
                        {
                            int bytesRead;
                            long totalBytesRead = 0;

                            // 讀取文件並進行加密
                            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                // 處理第一個字節 (文件頭)
                                if (totalBytesRead == 0)
                                {
                                    buffer[0] ^= hKey;
                                }

                                // 處理最後一個字節 (文件尾)
                                if (totalBytesRead + bytesRead == fileLength)
                                {
                                    buffer[bytesRead - 1] ^= tKey;
                                }

                                // 每隔 2 字節加密
                                for (int i = 0; i < bytesRead >> 1; i++)
                                {
                                    int s1 = i << 1;
                                    int s2 = s1 + 1;
                                    // 加密每隔 2 字節
                                    buffer[s1] ^= j1Key;
                                    // 確保不超出範圍
                                    if (s2 < bytesRead)
                                    {
                                        // 每隔 2 字節加密
                                        buffer[s2] ^= j2Key;
                                    }
                                }

                                memoryStream.Write(buffer, 0, bytesRead);
                                totalBytesRead += bytesRead;
                            }

                            return memoryStream.ToArray();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return null;
                }
            }

            /// <summary>
            /// Head-Tail 2 XOR Plus 解密文件
            /// </summary>
            /// <param name="filePath"></param>
            /// <param name="hKey"></param>
            /// <param name="tKey"></param>
            /// <param name="j1Key"></param>
            /// <param name="j2Key"></param>
            /// <returns></returns>
            public static byte[] DecryptBytes(string filePath, byte hKey, byte tKey, byte j1Key, byte j2Key)
            {
                try
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        byte[] buffer = new byte[BUFFER_SIZE];
                        long fileLength = fileStream.Length;

                        using (var memoryStream = new MemoryStream((int)fileLength))
                        {
                            int bytesRead;
                            long totalBytesRead = 0;

                            // 讀取文件並進行解密
                            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                // 每隔 2 字節解密
                                for (int i = 0; i < bytesRead >> 1; i++)
                                {
                                    int s1 = i << 1;
                                    int s2 = s1 + 1;
                                    // 解密每隔 2 字節
                                    buffer[s1] ^= j1Key;
                                    // 確保不超出範圍
                                    if (s2 < bytesRead)
                                    {
                                        // 每隔 2 字節解密
                                        buffer[s2] ^= j2Key;
                                    }
                                }

                                // 處理第一個字節 (文件頭) 只在第一個區塊處理
                                if (totalBytesRead == 0)
                                {
                                    buffer[0] ^= hKey;
                                }

                                // 處理最後一個字節 (文件尾), 當累計讀取後等於文件長度時處理
                                if (totalBytesRead + bytesRead == fileLength)
                                {
                                    buffer[bytesRead - 1] ^= tKey;
                                }

                                memoryStream.Write(buffer, 0, bytesRead);
                                totalBytesRead += bytesRead;
                            }

                            return memoryStream.ToArray();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return null;
                }
            }

            /// <summary>
            /// Head-Tail 2 XOR Plus 加密文件
            /// </summary>
            /// <param name="rawBytes"></param>
            /// <param name="hKey"></param>
            /// <param name="tKey"></param>
            /// <param name="j1Key"></param>
            /// <param name="j2Key"></param>
            /// <returns></returns>
            public static bool EncryptBytes(byte[] rawBytes, byte hKey, byte tKey, byte j1Key, byte j2Key)
            {
                try
                {
                    int length = rawBytes.Length;

                    // head encrypt
                    rawBytes[0] ^= hKey;
                    // tail encrypt
                    rawBytes[length - 1] ^= tKey;

                    // jump 2 plus encrypt
                    for (int i = 0; i < length >> 1; i++)
                    {
                        int s1 = i << 1;
                        int s2 = s1 + 1;
                        rawBytes[s1] ^= j1Key;
                        if (s2 < length)
                            rawBytes[s2] ^= j2Key;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return false;
                }

                return true;
            }

            /// <summary>
            /// Head-Tail 2 XOR Plus 解密文件
            /// </summary>
            /// <param name="encryptBytes"></param>
            /// <param name="hKey"></param>
            /// <param name="tKey"></param>
            /// <param name="j1Key"></param>
            /// <param name="j2Key"></param>
            /// <returns></returns>
            public static bool DecryptBytes(byte[] encryptBytes, byte hKey, byte tKey, byte j1Key, byte j2Key)
            {
                try
                {
                    int length = encryptBytes.Length;

                    // jump 2 plus decrypt
                    for (int i = 0; i < length >> 1; i++)
                    {
                        int s1 = i << 1;
                        int s2 = s1 + 1;
                        encryptBytes[s1] ^= j1Key;
                        if (s2 < length)
                            encryptBytes[s2] ^= j2Key;
                    }

                    // head decrypt
                    encryptBytes[0] ^= hKey;
                    // tail decrypt
                    encryptBytes[length - 1] ^= tKey;
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return false;
                }

                return true;
            }

            /// <summary>
            /// 返回 Head-Tail 2 XOR Plus 解密 Stream
            /// </summary>
            /// <param name="encryptFile"></param>
            /// <param name="hKey"></param>
            /// <param name="tKey"></param>
            /// <param name="j1Key"></param>
            /// <param name="j2Key"></param>
            /// <returns></returns>
            public static Stream DecryptStream(string encryptFile, byte hKey, byte tKey, byte j1Key, byte j2Key)
            {
                try
                {
                    // 建立 FileStream 以逐步讀取加密文件
                    using (FileStream fsDecrypt = new FileStream(encryptFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        // 使用一個緩衝區來逐步讀取文件
                        byte[] buffer = new byte[BUFFER_SIZE];
                        int bytesRead;
                        long totalBytesRead = 0;
                        long fileLength = fsDecrypt.Length;

                        // 創建 MemoryStream 用於存放解密後的數據
                        MemoryStream msDecrypt = new MemoryStream((int)fileLength);

                        // 逐步讀取文件內容, 並進行解密
                        while ((bytesRead = fsDecrypt.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            // 每隔 2 字節解密
                            for (int i = 0; i < bytesRead >> 1; i++)
                            {
                                int s1 = i << 1;
                                int s2 = s1 + 1;
                                // 解密每隔 2 字節
                                buffer[s1] ^= j1Key;
                                if (s2 < bytesRead)
                                {
                                    // 每隔 2 字節解密
                                    buffer[s2] ^= j2Key;
                                }
                            }

                            // 處理文件頭 (第一個字節)
                            if (totalBytesRead == 0)
                            {
                                // 處理第一個字節
                                buffer[0] ^= hKey;
                            }

                            // 處理文件尾 (最後一個字節)
                            if (totalBytesRead + bytesRead == fileLength)
                            {
                                // 處理最後一個字節
                                buffer[bytesRead - 1] ^= tKey;
                            }

                            // 將解密後的數據寫入 MemoryStream
                            msDecrypt.Write(buffer, 0, bytesRead);
                            totalBytesRead += bytesRead;
                        }

                        // 將 MemoryStream 的位置設置為開頭以便後續讀取
                        msDecrypt.Position = 0;
                        return msDecrypt;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return null;
                }
            }
        }

        public class AES
        {
            public class WriteFile
            {
                /// <summary>
                /// AES 加密文件
                /// </summary>
                /// <param name="sourceFile"></param>
                /// <param name="key"></param>
                /// <param name="iv"></param>
                /// <returns></returns>
                public static bool EncryptFile(string sourceFile, string key, string iv)
                {
                    try
                    {
                        using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
                        using (SHA256 sha256 = SHA256.Create())
                        using (MD5 md5 = MD5.Create())
                        {
                            byte[] keyData = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                            byte[] ivData = md5.ComputeHash(Encoding.UTF8.GetBytes(iv));
                            aes.Key = keyData;
                            aes.IV = ivData;
                            aes.Padding = PaddingMode.PKCS7;

                            // 讀取整個文件數據
                            byte[] dataBytes = File.ReadAllBytes(sourceFile);
                            // 刪除原始文件以便覆蓋寫入
                            File.Delete(sourceFile);

                            using (FileStream fsEncrypt = new FileStream(sourceFile, FileMode.Create, FileAccess.Write))
                            using (CryptoStream cs = new CryptoStream(fsEncrypt, aes.CreateEncryptor(), CryptoStreamMode.Write))
                            {
                                cs.Write(dataBytes, 0, dataBytes.Length);
                                cs.FlushFinalBlock();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                        return false;
                    }

                    return true;
                }

                /// <summary>
                /// AES 解密文件
                /// </summary>
                /// <param name="encryptFile"></param>
                /// <param name="key"></param>
                /// <param name="iv"></param>
                /// <returns></returns>
                public static bool DecryptFile(string encryptFile, string key, string iv)
                {
                    try
                    {
                        using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
                        using (SHA256 sha256 = SHA256.Create())
                        using (MD5 md5 = MD5.Create())
                        {
                            byte[] keyData = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                            byte[] ivData = md5.ComputeHash(Encoding.UTF8.GetBytes(iv));
                            aes.Key = keyData;
                            aes.IV = ivData;
                            aes.Padding = PaddingMode.PKCS7;

                            byte[] dataBytes = File.ReadAllBytes(encryptFile);
                            File.Delete(encryptFile);

                            using (FileStream fsDecrypt = new FileStream(encryptFile, FileMode.Create, FileAccess.Write))
                            using (CryptoStream cs = new CryptoStream(fsDecrypt, aes.CreateDecryptor(), CryptoStreamMode.Write))
                            {
                                cs.Write(dataBytes, 0, dataBytes.Length);
                                cs.FlushFinalBlock();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                        return false;
                    }

                    return true;
                }
            }

            /// <summary>
            /// AES 加密文件
            /// </summary>
            /// <param name="filePath"></param>
            /// <param name="key"></param>
            /// <param name="iv"></param>
            /// <returns></returns>
            public static byte[] EncryptBytes(string filePath, string key, string iv)
            {
                try
                {
                    using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
                    using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
                    using (SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider())
                    using (FileStream fsInput = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    using (MemoryStream msEncrypted = new MemoryStream())
                    {
                        byte[] keyData = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                        byte[] ivData = md5.ComputeHash(Encoding.UTF8.GetBytes(iv));
                        aes.Key = keyData;
                        aes.IV = ivData;
                        aes.Padding = PaddingMode.PKCS7;

                        using (CryptoStream cs = new CryptoStream(msEncrypted, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            byte[] buffer = new byte[BUFFER_SIZE];
                            int bytesRead;
                            while ((bytesRead = fsInput.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                cs.Write(buffer, 0, bytesRead);
                            }
                            cs.FlushFinalBlock();
                        }
                        return msEncrypted.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return null;
                }
            }

            /// <summary>
            /// AES 解密文件
            /// </summary>
            /// <param name="filePath"></param>
            /// <param name="key"></param>
            /// <param name="iv"></param>
            /// <returns></returns>
            public static byte[] DecryptBytes(string filePath, string key, string iv)
            {
                try
                {
                    using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
                    using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
                    using (SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider())
                    using (FileStream fsInput = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    using (MemoryStream msDecrypted = new MemoryStream())
                    {
                        byte[] keyData = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                        byte[] ivData = md5.ComputeHash(Encoding.UTF8.GetBytes(iv));
                        aes.Key = keyData;
                        aes.IV = ivData;
                        aes.Padding = PaddingMode.PKCS7;

                        using (CryptoStream cs = new CryptoStream(fsInput, aes.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            byte[] buffer = new byte[BUFFER_SIZE];
                            int bytesRead;
                            while ((bytesRead = cs.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                msDecrypted.Write(buffer, 0, bytesRead);
                            }
                        }
                        return msDecrypted.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return null;
                }
            }

            /// <summary>
            /// AES 加密文件
            /// </summary>
            /// <param name="rawBytes"></param>
            /// <param name="key"></param>
            /// <param name="iv"></param>
            /// <returns></returns>
            public static bool EncryptBytes(ref byte[] rawBytes, string key, string iv)
            {
                try
                {
                    using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
                    using (SHA256 sha256 = SHA256.Create())
                    using (MD5 md5 = MD5.Create())
                    {
                        byte[] keyData = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                        byte[] ivData = md5.ComputeHash(Encoding.UTF8.GetBytes(iv));
                        aes.Key = keyData;
                        aes.IV = ivData;
                        aes.Padding = PaddingMode.PKCS7;

                        using (MemoryStream msSource = new MemoryStream())
                        {
                            using (CryptoStream cs = new CryptoStream(msSource, aes.CreateEncryptor(), CryptoStreamMode.Write))
                            {
                                cs.Write(rawBytes, 0, rawBytes.Length);
                                cs.FlushFinalBlock();
                            }
                            rawBytes = msSource.ToArray();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return false;
                }

                return true;
            }

            /// <summary>
            /// AES 解密文件
            /// </summary>
            /// <param name="encryptBytes"></param>
            /// <param name="key"></param>
            /// <param name="iv"></param>
            /// <returns></returns>
            public static bool DecryptBytes(ref byte[] encryptBytes, string key, string iv)
            {
                try
                {
                    using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
                    using (SHA256 sha256 = SHA256.Create())
                    using (MD5 md5 = MD5.Create())
                    {
                        byte[] keyData = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                        byte[] ivData = md5.ComputeHash(Encoding.UTF8.GetBytes(iv));
                        aes.Key = keyData;
                        aes.IV = ivData;
                        aes.Padding = PaddingMode.PKCS7;

                        using (MemoryStream msDecrypt = new MemoryStream())
                        using (MemoryStream msEncrypt = new MemoryStream(encryptBytes))
                        using (CryptoStream cs = new CryptoStream(msEncrypt, aes.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            cs.CopyTo(msDecrypt);
                            encryptBytes = msDecrypt.ToArray();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return false;
                }

                return true;
            }

            /// <summary>
            /// 返回 AES 解密 Stream
            /// </summary>
            /// <param name="encryptFile"></param>
            /// <param name="key"></param>
            /// <param name="iv"></param>
            /// <returns></returns>
            public static Stream DecryptStream(string encryptFile, string key, string iv)
            {
                try
                {
                    using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
                    using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
                    using (SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider())
                    using (FileStream fsDecrypt = new FileStream(encryptFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        byte[] keyData = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                        byte[] ivData = md5.ComputeHash(Encoding.UTF8.GetBytes(iv));
                        aes.Key = keyData;
                        aes.IV = ivData;
                        aes.Padding = PaddingMode.PKCS7;

                        using (CryptoStream cs = new CryptoStream(fsDecrypt, aes.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            MemoryStream msDecrypt = new MemoryStream();
                            byte[] buffer = new byte[BUFFER_SIZE];
                            int bytesRead;

                            while ((bytesRead = cs.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                msDecrypt.Write(buffer, 0, bytesRead);
                            }

                            msDecrypt.Position = 0;
                            return msDecrypt;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return null;
                }
            }
        }

        public class ChaCha20
        {
            /// <summary>
            /// Limit to 32 bytes
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            public static byte[] PrepareKeyHash(string input)
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    return sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                }
            }

            /// <summary>
            /// Limit to 12 bytes
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            public static byte[] PrepareNonceHash(string input)
            {
                using (SHA1 sha1 = SHA1.Create())
                {
                    // 計算 SHA-1 哈希值
                    byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));

                    // 裁剪為 12 字節
                    byte[] trimmedHash = new byte[12];
                    Array.Copy(hash, trimmedHash, 12);
                    return trimmedHash;
                }
            }

            public class WriteFile
            {
                /// <summary>
                /// ChaCha20 加密文件
                /// </summary>
                /// <param name="sourceFile"></param>
                /// <param name="key"></param>
                /// <returns></returns>
                public static bool EncryptFile(string sourceFile, string key, string nonce, uint counter)
                {
                    try
                    {
                        byte[] keyBytes = PrepareKeyHash(key);
                        byte[] nonceBytes = PrepareNonceHash(nonce);
                        CSChaCha20.ChaCha20 chacha20 = new CSChaCha20.ChaCha20(keyBytes, nonceBytes, counter);
                        byte[] fileData = File.ReadAllBytes(sourceFile);
                        byte[] encryptedData = chacha20.EncryptBytes(fileData);
                        File.WriteAllBytes(sourceFile, encryptedData);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                        return false;
                    }

                    return true;
                }

                /// <summary>
                /// ChaCha20 解密文件
                /// </summary>
                /// <param name="encryptFile"></param>
                /// <param name="key"></param>
                /// <returns></returns>
                public static bool DecryptFile(string encryptFile, string key, string nonce, uint counter)
                {
                    try
                    {
                        byte[] keyBytes = PrepareKeyHash(key);
                        byte[] nonceBytes = PrepareNonceHash(nonce);
                        CSChaCha20.ChaCha20 chacha20 = new CSChaCha20.ChaCha20(keyBytes, nonceBytes, counter);
                        byte[] encryptedData = File.ReadAllBytes(encryptFile);
                        byte[] decryptedData = chacha20.DecryptBytes(encryptedData);
                        File.WriteAllBytes(encryptFile, decryptedData);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                        return false;
                    }

                    return true;
                }
            }

            /// <summary>
            /// ChaCha20 加密文件
            /// </summary>
            /// <param name="filePath"></param>
            /// <param name="key"></param>
            /// <returns></returns>
            public static byte[] EncryptBytes(string filePath, string key, string nonce, uint counter)
            {
                try
                {
                    byte[] fileData = File.ReadAllBytes(filePath);
                    byte[] keyBytes = PrepareKeyHash(key);
                    byte[] nonceBytes = PrepareNonceHash(nonce);
                    CSChaCha20.ChaCha20 chacha20 = new CSChaCha20.ChaCha20(keyBytes, nonceBytes, counter);
                    byte[] encryptedData = chacha20.EncryptBytes(fileData);
                    return encryptedData;
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return null;
                }
            }

            /// <summary>
            /// ChaCha20 解密文件
            /// </summary>
            /// <param name="filePath"></param>
            /// <param name="key"></param>
            /// <returns></returns>
            public static byte[] DecryptBytes(string filePath, string key, string nonce, uint counter)
            {
                try
                {
                    byte[] encryptedData = File.ReadAllBytes(filePath);
                    byte[] keyBytes = PrepareKeyHash(key);
                    byte[] nonceBytes = PrepareNonceHash(nonce);
                    CSChaCha20.ChaCha20 chacha20 = new CSChaCha20.ChaCha20(keyBytes, nonceBytes, counter);
                    byte[] decryptedData = chacha20.DecryptBytes(encryptedData);
                    return decryptedData;
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return null;
                }
            }

            /// <summary>
            /// ChaCha20 加密位元組
            /// </summary>
            /// <param name="rawBytes"></param>
            /// <param name="key"></param>
            /// <returns></returns>
            public static bool EncryptBytes(ref byte[] rawBytes, string key, string nonce, uint counter)
            {
                try
                {
                    byte[] keyBytes = PrepareKeyHash(key);
                    byte[] nonceBytes = PrepareNonceHash(nonce);
                    CSChaCha20.ChaCha20 chacha20 = new CSChaCha20.ChaCha20(keyBytes, nonceBytes, counter);
                    rawBytes = chacha20.EncryptBytes(rawBytes);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return false;
                }

                return true;
            }

            /// <summary>
            /// ChaCha20 解密位元組
            /// </summary>
            /// <param name="encryptBytes"></param>
            /// <param name="key"></param>
            /// <returns></returns>
            public static bool DecryptBytes(ref byte[] encryptBytes, string key, string nonce, uint counter)
            {
                try
                {
                    byte[] keyBytes = PrepareKeyHash(key);
                    byte[] nonceBytes = PrepareNonceHash(nonce);
                    CSChaCha20.ChaCha20 chacha20 = new CSChaCha20.ChaCha20(keyBytes, nonceBytes, counter);
                    encryptBytes = chacha20.DecryptBytes(encryptBytes);
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return false;
                }
            }

            /// <summary>
            /// 返回 ChaCha20 解密 Stream
            /// </summary>
            /// <param name="encryptFile"></param>
            /// <param name="key"></param>
            /// <returns></returns>
            public static Stream DecryptStream(string encryptFile, string key, string nonce, uint counter)
            {
                try
                {
                    byte[] encryptedData = File.ReadAllBytes(encryptFile);
                    byte[] keyBytes = PrepareKeyHash(key);
                    byte[] nonceBytes = PrepareNonceHash(nonce);
                    CSChaCha20.ChaCha20 chacha20 = new CSChaCha20.ChaCha20(keyBytes, nonceBytes, counter);
                    return new MemoryStream(chacha20.DecryptBytes(encryptedData));
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return null;
                }
            }
        }

        public class XXTEA
        {
            public class WriteFile
            {
                /// <summary>
                /// XXTEA 加密文件
                /// </summary>
                /// <param name="sourceFile"></param>
                /// <param name="key"></param>
                /// <returns></returns>
                public static bool EncryptFile(string sourceFile, string key)
                {
                    try
                    {
                        byte[] fileData = File.ReadAllBytes(sourceFile);
                        byte[] encryptedData = Razensoft.XXTEA.Encrypt(fileData, key);
                        File.WriteAllBytes(sourceFile, encryptedData);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                        return false;
                    }

                    return true;
                }

                /// <summary>
                /// XXTEA 解密文件
                /// </summary>
                /// <param name="encryptFile"></param>
                /// <param name="key"></param>
                /// <returns></returns>
                public static bool DecryptFile(string encryptFile, string key)
                {
                    try
                    {
                        byte[] encryptedData = File.ReadAllBytes(encryptFile);
                        byte[] decryptedData = Razensoft.XXTEA.Decrypt(encryptedData, key);
                        File.WriteAllBytes(encryptFile, decryptedData);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                        return false;
                    }

                    return true;
                }
            }

            /// <summary>
            /// XXTEA 加密文件
            /// </summary>
            /// <param name="filePath"></param>
            /// <param name="key"></param>
            /// <returns></returns>
            public static byte[] EncryptBytes(string filePath, string key)
            {
                try
                {
                    byte[] fileData = File.ReadAllBytes(filePath);
                    return Razensoft.XXTEA.Encrypt(fileData, key);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return null;
                }
            }

            /// <summary>
            /// XXTEA 解密文件
            /// </summary>
            /// <param name="filePath"></param>
            /// <param name="key"></param>
            /// <returns></returns>
            public static byte[] DecryptBytes(string filePath, string key)
            {
                try
                {
                    byte[] encryptedData = File.ReadAllBytes(filePath);
                    return Razensoft.XXTEA.Decrypt(encryptedData, key);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return null;
                }
            }

            /// <summary>
            /// XXTEA 加密位元組
            /// </summary>
            /// <param name="rawBytes"></param>
            /// <param name="key"></param>
            /// <returns></returns>
            public static bool EncryptBytes(ref byte[] rawBytes, string key)
            {
                try
                {
                    byte[] encrypted = Razensoft.XXTEA.Encrypt(rawBytes, key);
                    rawBytes = encrypted;
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return false;
                }

                return true;
            }

            /// <summary>
            /// XXTEA 解密位元組
            /// </summary>
            /// <param name="encryptBytes"></param>
            /// <param name="key"></param>
            /// <returns></returns>
            public static bool DecryptBytes(ref byte[] encryptBytes, string key)
            {
                try
                {
                    encryptBytes = Razensoft.XXTEA.Decrypt(encryptBytes, key);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return false;
                }

                return true;
            }

            /// <summary>
            /// 返回 XXTEA 解密 Stream
            /// </summary>
            /// <param name="encryptFile"></param>
            /// <param name="key"></param>
            /// <returns></returns>
            public static Stream DecryptStream(string encryptFile, string key)
            {
                try
                {
                    byte[] encryptedData = File.ReadAllBytes(encryptFile);
                    byte[] decryptedData = Razensoft.XXTEA.Decrypt(encryptedData, key);
                    return new MemoryStream(decryptedData);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return null;
                }
            }
        }

        public class OffsetXOR
        {
            public class WriteFile
            {
                /// <summary>
                /// OffsetXOR 加密文件
                /// </summary>
                /// <param name="sourceFile"></param>
                /// <param name="key"></param>
                /// <returns></returns>
                public static bool EncryptFile(string sourceFile, byte key, int dummySize)
                {
                    bool finished = Offset.WriteFile.EncryptFile(sourceFile, dummySize);

                    if (finished)
                    {
                        finished = XOR.WriteFile.EncryptFile(sourceFile, key);
                        if (finished)
                            return true;
                    }

                    return false;
                }

                /// <summary>
                /// OffsetXOR 解密文件
                /// </summary>
                /// <param name="encryptFile"></param>
                /// <param name="key"></param>
                /// <returns></returns>
                public static bool DecryptFile(string encryptFile, byte key, int dummySize)
                {
                    bool finished = Offset.WriteFile.DecryptFile(encryptFile, dummySize);

                    if (finished)
                    {
                        finished = XOR.WriteFile.DecryptFile(encryptFile, key);
                        if (finished)
                            return true;
                    }

                    return false;
                }
            }

            /// <summary>
            /// OffsetXOR 加密文件
            /// </summary>
            /// <param name="filePath"></param>
            /// <param name="key"></param>
            /// <returns></returns>
            public static byte[] EncryptBytes(string filePath, byte key, int dummySize)
            {
                var encryptedData = Offset.EncryptBytes(filePath, dummySize);

                bool finished = XOR.EncryptBytes(encryptedData, key);
                if (finished)
                    return encryptedData;

                return null;
            }

            /// <summary>
            /// OffsetXOR 解密文件
            /// </summary>
            /// <param name="filePath"></param>
            /// <param name="key"></param>
            /// <returns></returns>
            public static byte[] DecryptBytes(string filePath, byte key, int dummySize)
            {
                var decryptedData = Offset.DecryptBytes(filePath, dummySize);

                bool finished = XOR.DecryptBytes(decryptedData, key);
                if (finished)
                    return decryptedData;

                return null;
            }

            /// <summary>
            /// OffsetXOR 加密文件
            /// </summary>
            /// <param name="rawBytes"></param>
            /// <param name="key"></param>
            /// <returns></returns>
            public static bool EncryptBytes(byte[] rawBytes, byte key, int dummySize)
            {
                bool finished = Offset.EncryptBytes(ref rawBytes, dummySize);

                if (finished)
                {
                    finished = XOR.EncryptBytes(rawBytes, key);
                    if (finished)
                        return true;
                }

                return false;
            }

            /// <summary>
            /// OffsetXOR 解密文件
            /// </summary>
            /// <param name="encryptBytes"></param>
            /// <param name="key"></param>
            /// <returns></returns>
            public static bool DecryptBytes(byte[] encryptBytes, byte key, int dummySize)
            {
                bool finished = Offset.DecryptBytes(ref encryptBytes, dummySize);

                if (finished)
                {
                    finished = XOR.DecryptBytes(encryptBytes, key);
                    if (finished)
                        return true;
                }

                return false;
            }

            /// <summary>
            /// 返回 OffsetXOR 解密 Stream
            /// </summary>
            /// <param name="encryptFile"></param>
            /// <param name="key"></param>
            /// <returns></returns>
            public static Stream DecryptStream(string encryptFile, byte key, int dummySize)
            {
                try
                {
                    using (Stream fsDecrypt = Offset.DecryptStream(encryptFile, dummySize))
                    {
                        long totalLength = fsDecrypt.Length;
                        var msDecrypt = new MemoryStream((int)totalLength);

                        // 使用一個緩衝區來逐步讀取文件
                        byte[] buffer = new byte[BUFFER_SIZE];
                        int bytesRead;

                        // 逐步讀取文件內容, 並對每個字節進行 XOR 解密
                        while ((bytesRead = fsDecrypt.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            for (int i = 0; i < bytesRead; i++)
                            {
                                // 進行 XOR 解密
                                buffer[i] ^= key;
                            }

                            // 將解密後的數據寫入 MemoryStream
                            msDecrypt.Write(buffer, 0, bytesRead);
                        }

                        // 將 MemoryStream 的位置設置為開頭以便後續讀取
                        msDecrypt.Position = 0;
                        return msDecrypt;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return null;
                }
            }
        }
    }
}