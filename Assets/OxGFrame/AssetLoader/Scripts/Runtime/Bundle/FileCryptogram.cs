using OxGKit.LoggingSystem;
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
            public class WriteFile
            {
                /// <summary>
                ///  Offset 加密文件 【檢測OK】
                /// </summary>
                /// <param name="sourceFile"></param>
                /// <param name="randomSeed"></param>
                /// <param name="dummySize"></param>
                /// <returns></returns>
                public static bool OffsetEncryptFile(string sourceFile, int randomSeed, int dummySize = 0)
                {
                    try
                    {
                        // 初始化亂數種子
                        UnityEngine.Random.InitState(randomSeed);

                        // 取得原始文件的長度
                        long fileLength = new FileInfo(sourceFile).Length;
                        long totalLength = fileLength + dummySize; // 計算總長度：原始文件長度 + dummySize

                        // 建立緩衝區
                        byte[] buffer = new byte[BUFFER_SIZE];
                        byte[] offsetBytes = new byte[BUFFER_SIZE]; // 用來存放偏移後的字節
                        long bytesProcessed = 0;                    // 已處理的字節數

                        // 使用 FileStream 讀取與寫入文件
                        using (FileStream fsRead = new FileStream(sourceFile, FileMode.Open, FileAccess.Read))
                        using (FileStream fsWrite = new FileStream(sourceFile + ".tmp", FileMode.Create, FileAccess.Write)) // 建立臨時文件
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

                        // 用偏移後的內容替換原始文件
                        File.Delete(sourceFile);                    // 刪除原始文件
                        File.Move(sourceFile + ".tmp", sourceFile); // 將臨時文件重命名為原始文件
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                        return false;
                    }

                    return true;
                }


                /// <summary>
                /// Offset 解密文件 【檢測OK】
                /// </summary>
                /// <param name="encryptFile"></param>
                /// <param name="dummySize"></param>
                /// <returns></returns>
                public static bool OffsetDecryptFile(string encryptFile, int dummySize = 0)
                {
                    try
                    {
                        // 取得加密文件的長度
                        long fileLength = new FileInfo(encryptFile).Length;
                        long totalLength = fileLength - dummySize; // 總長度去掉 dummySize 的部分

                        // 創建緩衝區
                        byte[] buffer = new byte[BUFFER_SIZE];
                        long bytesProcessed = 0; // 已處理的字節數

                        // 使用 FileStream 逐步讀取加密文件並寫入新文件
                        using (FileStream fsRead = new FileStream(encryptFile, FileMode.Open, FileAccess.Read))
                        using (FileStream fsWrite = new FileStream(encryptFile + ".tmp", FileMode.Create, FileAccess.Write))
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

                        // 用解密後的文件替換原文件
                        File.Delete(encryptFile);                     // 刪除原文件
                        File.Move(encryptFile + ".tmp", encryptFile); // 將臨時文件重命名為原文件名
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
            /// Offset 加密文件 【檢測OK】
            /// </summary>
            /// <param name="filePath"></param>
            /// <param name="randomSeed"></param>
            /// <param name="dummySize"></param>
            /// <returns></returns>
            public static byte[] OffsetEncryptBytes(string filePath, int randomSeed, int dummySize = 0)
            {
                try
                {
                    // 使用 FileStream 逐步讀取文件
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        // 設置隨機種子
                        UnityEngine.Random.InitState(randomSeed);

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
            /// Offset 解密文件 【檢測OK】
            /// </summary>
            /// <param name="filePath"></param>
            /// <param name="dummySize"></param>
            /// <returns></returns>
            public static byte[] OffsetDecryptBytes(string filePath, int dummySize = 0)
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

                        // 創建一個 byte[] 來存放解密後的資料
                        byte[] offsetDatabytes = new byte[totalLength];

                        // 逐步讀取數據
                        int bytesRead;
                        int offset = 0;
                        byte[] buffer = new byte[BUFFER_SIZE];
                        while ((bytesRead = fsDecrypt.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            int bytesToWrite = (int)Math.Min(bytesRead, totalLength - offset);
                            Buffer.BlockCopy(buffer, 0, offsetDatabytes, offset, bytesToWrite);
                            offset += bytesToWrite;
                        }

                        // 返回解密後的 byte[]
                        return offsetDatabytes;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return null;
                }
            }

            /// <summary>
            /// Offset 加密文件 【檢測OK】
            /// </summary>
            /// <param name="rawBytes"></param>
            /// <param name="randomSeed"></param>
            /// <param name="dummySize"></param>
            /// <returns></returns>
            public static bool OffsetEncryptBytes(ref byte[] rawBytes, int randomSeed, int dummySize = 0)
            {
                try
                {
                    UnityEngine.Random.InitState(randomSeed);

                    byte[] dataBytes = rawBytes;
                    int totalLength = dataBytes.Length + dummySize;
                    byte[] offsetDatabytes = new byte[totalLength];
                    for (int i = 0; i < totalLength; i++)
                    {
                        if (dummySize > 0 && i < dummySize) offsetDatabytes[i] = (byte)(UnityEngine.Random.Range(0, 256));
                        else offsetDatabytes[i] = dataBytes[i - dummySize];
                    }
                    rawBytes = offsetDatabytes;
                }
                catch
                {
                    return false;
                }

                return true;
            }

            /// <summary>
            /// Offset 解密文件 【檢測OK】
            /// </summary>
            /// <param name="encryptBytes"></param>
            /// <param name="dummySize"></param>
            /// <returns></returns>
            public static bool OffsetDecryptBytes(ref byte[] encryptBytes, int dummySize = 0)
            {
                try
                {
                    int totalLength = encryptBytes.Length - dummySize;
                    byte[] offsetDatabytes = new byte[totalLength];
                    Buffer.BlockCopy(encryptBytes, dummySize, offsetDatabytes, 0, totalLength);
                    encryptBytes = offsetDatabytes;
                }
                catch
                {
                    return false;
                }

                return true;
            }

            /// <summary>
            /// 返回 Offset 解密 Stream 【檢測OK】
            /// </summary>
            /// <param name="encryptFile"></param>
            /// <param name="dummySize"></param>
            /// <returns></returns>
            public static Stream OffsetDecryptStream(string encryptFile, int dummySize = 0)
            {
                try
                {
                    var fsDecrypt = new FileStream(encryptFile, FileMode.Open, FileAccess.Read, FileShare.Read); // 支持共享讀取
                    long totalLength = fsDecrypt.Length - dummySize;                                             // 計算總長度
                    var msDecrypt = new MemoryStream((int)totalLength);                                          // 預先配置 MemoryStream 容量

                    // 跳過 dummySize
                    fsDecrypt.Seek(dummySize, SeekOrigin.Begin);

                    // 創建緩衝區進行讀取和寫入操作
                    byte[] buffer = new byte[BUFFER_SIZE];
                    int bytesRead;
                    long bytesToRead = totalLength; // 剩餘需讀取的字節數

                    while (bytesToRead > 0 && (bytesRead = fsDecrypt.Read(buffer, 0, (int)Math.Min(buffer.Length, bytesToRead))) > 0)
                    {
                        msDecrypt.Write(buffer, 0, bytesRead); // 寫入 MemoryStream
                        bytesToRead -= bytesRead;              // 減少剩餘需讀取的字節數
                    }

                    msDecrypt.Position = 0; // 將 MemoryStream 的位置設置為開頭以便後續讀取
                    return msDecrypt;
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
                /// XOR 加密文件 【檢測OK】
                /// </summary>
                /// <param name="sourceFile"></param>
                /// <param name="key"></param>
                /// <returns></returns>
                public static bool XorEncryptFile(string sourceFile, byte key)
                {
                    try
                    {
                        // 使用 FileStream 逐步讀取和寫入文件
                        using (FileStream fsRead = new FileStream(sourceFile, FileMode.Open, FileAccess.Read))
                        using (FileStream fsWrite = new FileStream(sourceFile + ".tmp", FileMode.Create, FileAccess.Write))
                        {
                            byte[] buffer = new byte[BUFFER_SIZE];
                            int bytesRead;

                            while ((bytesRead = fsRead.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                for (int i = 0; i < bytesRead; i++)
                                {
                                    buffer[i] ^= key; // 使用 XOR 加密
                                }
                                fsWrite.Write(buffer, 0, bytesRead); // 寫入加密後的數據
                            }
                        }

                        // 用加密後的內容替換原始文件
                        File.Delete(sourceFile); // 刪除原始文件
                        File.Move(sourceFile + ".tmp", sourceFile); // 將臨時文件重命名為原始文件
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                        return false;
                    }

                    return true;
                }

                /// <summary>
                /// XOR 解密文件 【檢測OK】
                /// </summary>
                /// <param name="encryptFile"></param>
                /// <param name="key"></param>
                /// <returns></returns>
                public static bool XorDecryptFile(string encryptFile, byte key)
                {
                    // 解密的邏輯與加密相同
                    return XorEncryptFile(encryptFile, key); // 因為 XOR 是可逆的, 直接調用加密方法
                }
            }

            /// <summary>
            /// XOR 加密文件 【檢測OK】
            /// </summary>
            /// <param name="filePath"></param>
            /// <param name="key"></param>
            /// <returns></returns>
            public static byte[] XorEncryptBytes(string filePath, byte key)
            {
                try
                {
                    // 使用 FileStream 逐步讀取文件
                    using (FileStream fsRead = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    using (MemoryStream msWrite = new MemoryStream())
                    {
                        byte[] buffer = new byte[BUFFER_SIZE];
                        int bytesRead;

                        while ((bytesRead = fsRead.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            for (int i = 0; i < bytesRead; i++)
                            {
                                buffer[i] ^= key; // 使用 XOR 加密
                            }
                            msWrite.Write(buffer, 0, bytesRead); // 寫入加密後的數據
                        }

                        return msWrite.ToArray(); // 返回加密後的字節數組
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return null;
                }
            }

            /// <summary>
            /// XOR 解密文件 【檢測OK】
            /// </summary>
            /// <param name="filePath"></param>
            /// <param name="key"></param>
            /// <returns></returns>
            public static byte[] XorDecryptBytes(string filePath, byte key)
            {
                // 解密的邏輯與加密相同
                return XorEncryptBytes(filePath, key); // 因為 XOR 是可逆的, 直接調用加密方法
            }

            /// <summary>
            /// XOR 加密文件 【檢測OK】
            /// </summary>
            /// <param name="rawBytes"></param>
            /// <param name="key"></param>
            /// <returns></returns>
            public static bool XorEncryptBytes(byte[] rawBytes, byte key)
            {
                try
                {
                    for (int i = 0; i < rawBytes.Length; i++)
                    {
                        rawBytes[i] ^= key;
                    }
                }
                catch
                {
                    return false;
                }

                return true;
            }

            /// <summary>
            /// XOR 解密文件 【檢測OK】
            /// </summary>
            /// <param name="encryptBytes"></param>
            /// <param name="key"></param>
            /// <returns></returns>
            public static bool XorDecryptBytes(byte[] encryptBytes, byte key)
            {
                try
                {
                    for (int i = 0; i < encryptBytes.Length; i++)
                    {
                        encryptBytes[i] ^= key;
                    }
                }
                catch
                {
                    return false;
                }

                return true;
            }

            /// <summary>
            /// 返回 XOR 解密 Stream 【檢測OK】
            /// </summary>
            /// <param name="encryptFile"></param>
            /// <param name="key"></param>
            /// <returns></returns>
            public static Stream XorDecryptStream(string encryptFile, byte key)
            {
                try
                {
                    // 建立 FileStream 以逐步讀取加密文件
                    using (FileStream fsDecrypt = new FileStream(encryptFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        // 創建 MemoryStream 用於存放解密後的數據
                        MemoryStream msDecrypt = new MemoryStream();

                        // 使用一個緩衝區來逐步讀取文件
                        byte[] buffer = new byte[BUFFER_SIZE];
                        int bytesRead;

                        // 逐步讀取文件內容, 並對每個字節進行 XOR 解密
                        while ((bytesRead = fsDecrypt.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            for (int i = 0; i < bytesRead; i++)
                            {
                                buffer[i] ^= key; // 進行 XOR 解密
                            }
                            msDecrypt.Write(buffer, 0, bytesRead); // 將解密後的數據寫入 MemoryStream
                        }

                        msDecrypt.Position = 0; // 將 MemoryStream 的位置設置為開頭以便後續讀取
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
                /// Head-Tail 2 XOR 加密文件 【檢測OK】
                /// </summary>
                /// <param name="sourceFile"></param>
                /// <param name="hKey"></param>
                /// <param name="tKey"></param>
                /// <param name="jKey"></param>
                /// <returns></returns>
                public static bool HT2XorEncryptFile(string sourceFile, byte hKey, byte tKey, byte jKey)
                {
                    byte[] buffer = new byte[BUFFER_SIZE];
                    long fileLength;

                    try
                    {
                        using (var stream = new FileStream(sourceFile, FileMode.Open, FileAccess.ReadWrite))
                        {
                            fileLength = stream.Length;

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
                /// Head-Tail 2 XOR 解密文件 【檢測OK】
                /// </summary>
                /// <param name="encryptFile"></param>
                /// <param name="hKey"></param>
                /// <param name="tKey"></param>
                /// <param name="jKey"></param>
                /// <returns></returns>
                public static bool HT2XorDecryptFile(string encryptFile, byte hKey, byte tKey, byte jKey)
                {
                    byte[] buffer = new byte[BUFFER_SIZE];
                    long fileLength;

                    try
                    {
                        using (var stream = new FileStream(encryptFile, FileMode.Open, FileAccess.ReadWrite))
                        {
                            fileLength = stream.Length;

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
            /// Head-Tail 2 XOR 加密文件 【檢測OK】
            /// </summary>
            /// <param name="filePath"></param>
            /// <param name="hKey"></param>
            /// <param name="tKey"></param>
            /// <param name="jKey"></param>
            /// <returns></returns>
            public static byte[] HT2XorEncryptBytes(string filePath, byte hKey, byte tKey, byte jKey)
            {
                byte[] buffer = new byte[BUFFER_SIZE];
                long fileLength;

                try
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        fileLength = fileStream.Length;
                        using (var memoryStream = new MemoryStream())
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
            /// Head-Tail 2 XOR 解密文件 【檢測OK】
            /// </summary>
            /// <param name="filePath"></param>
            /// <param name="hKey"></param>
            /// <param name="tKey"></param>
            /// <param name="jKey"></param>
            /// <returns></returns>
            public static byte[] HT2XorDecryptBytes(string filePath, byte hKey, byte tKey, byte jKey)
            {
                byte[] buffer = new byte[BUFFER_SIZE];
                long fileLength;

                try
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        fileLength = fileStream.Length;
                        using (var memoryStream = new MemoryStream())
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
            /// Head-Tail 2 XOR 加密文件 【檢測OK】
            /// </summary>
            /// <param name="rawBytes"></param>
            /// <param name="hKey"></param>
            /// <param name="tKey"></param>
            /// <param name="jKey"></param>
            /// <returns></returns>
            public static bool HT2XorEncryptBytes(byte[] rawBytes, byte hKey, byte tKey, byte jKey)
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
                catch
                {
                    return false;
                }

                return true;
            }

            /// <summary>
            /// Head-Tail 2 XOR 解密文件 【檢測OK】
            /// </summary>
            /// <param name="encryptBytes"></param>
            /// <param name="hKey"></param>
            /// <param name="tKey"></param>
            /// <param name="jKey"></param>
            /// <returns></returns>
            public static bool HT2XorDecryptBytes(byte[] encryptBytes, byte hKey, byte tKey, byte jKey)
            {
                // jump 2 encrypt
                for (int i = 0; i < encryptBytes.Length >> 1; i++)
                {
                    encryptBytes[i << 1] ^= jKey;
                }

                // head encrypt
                encryptBytes[0] ^= hKey;
                // tail encrypt
                encryptBytes[encryptBytes.Length - 1] ^= tKey;

                return true;
            }

            /// <summary>
            /// 返回 Head-Tail 2 XOR 解密 Stream 【檢測OK】
            /// </summary>
            /// <param name="encryptFile"></param>
            /// <param name="hKey"></param>
            /// <param name="tKey"></param>
            /// <param name="jKey"></param>
            /// <returns></returns>
            public static Stream HT2XorDecryptStream(string encryptFile, byte hKey, byte tKey, byte jKey)
            {
                try
                {
                    // 建立 FileStream 以逐步讀取加密文件
                    using (FileStream fsDecrypt = new FileStream(encryptFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        // 創建 MemoryStream 用於存放解密後的數據
                        MemoryStream msDecrypt = new MemoryStream();
                        // 使用一個緩衝區來逐步讀取文件
                        byte[] buffer = new byte[BUFFER_SIZE];
                        int bytesRead;
                        long totalBytesRead = 0;
                        long fileLength = fsDecrypt.Length;

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

                            msDecrypt.Write(buffer, 0, bytesRead); // 將解密後的數據寫入 MemoryStream
                            totalBytesRead += bytesRead;
                        }

                        msDecrypt.Position = 0; // 將 MemoryStream 的位置設置為開頭以便後續讀取
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
                /// Head-Tail 2 XOR Plus 加密文件 【檢測OK】
                /// </summary>
                /// <param name="sourceFile"></param>
                /// <param name="hKey"></param>
                /// <param name="tKey"></param>
                /// <param name="j1Key"></param>
                /// <param name="j2Key"></param>
                /// <returns></returns>
                public static bool HT2XorPlusEncryptFile(string sourceFile, byte hKey, byte tKey, byte j1Key, byte j2Key)
                {
                    byte[] buffer = new byte[BUFFER_SIZE];
                    long fileLength;

                    try
                    {
                        using (var stream = new FileStream(sourceFile, FileMode.Open, FileAccess.ReadWrite))
                        {
                            fileLength = stream.Length;

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
                                        buffer[i] ^= j1Key;            // 每隔 2 字節加密
                                    }

                                    if (position + i + 1 < fileLength) // 確保不超出長度
                                    {
                                        buffer[i + 1] ^= j2Key;        // 每隔 2 字節加密
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
                /// Head-Tail 2 XOR Plus 解密文件 【檢測OK】
                /// </summary>
                /// <param name="encryptFile"></param>
                /// <param name="hKey"></param>
                /// <param name="tKey"></param>
                /// <param name="j1Key"></param>
                /// <param name="j2Key"></param>
                /// <returns></returns>
                public static bool HT2XorPlusDecryptFile(string encryptFile, byte hKey, byte tKey, byte j1Key, byte j2Key)
                {
                    byte[] buffer = new byte[BUFFER_SIZE];
                    long fileLength;

                    try
                    {
                        using (var stream = new FileStream(encryptFile, FileMode.Open, FileAccess.ReadWrite))
                        {
                            fileLength = stream.Length;

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
                                        buffer[i] ^= j1Key;            // 每隔 2 字節解密
                                    }

                                    if (position + i + 1 < fileLength) // 確保不超出長度
                                    {
                                        buffer[i + 1] ^= j2Key;        // 每隔 2 字節解密
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
            /// Head-Tail 2 XOR Plus 加密文件 【檢測OK】
            /// </summary>
            /// <param name="filePath"></param>
            /// <param name="hKey"></param>
            /// <param name="tKey"></param>
            /// <param name="j1Key"></param>
            /// <param name="j2Key"></param>
            /// <returns></returns>
            public static byte[] HT2XorPlusEncryptBytes(string filePath, byte hKey, byte tKey, byte j1Key, byte j2Key)
            {
                byte[] buffer = new byte[BUFFER_SIZE];
                long fileLength;

                try
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        fileLength = fileStream.Length;
                        using (var memoryStream = new MemoryStream())
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

                                    buffer[s1] ^= j1Key;     // 加密每隔 2 字節
                                    if (s2 < bytesRead)      // 確保不超出範圍
                                    {
                                        buffer[s2] ^= j2Key; // 每隔 2 字節加密
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
            /// Head-Tail 2 XOR Plus 解密文件 【檢測OK】
            /// </summary>
            /// <param name="filePath"></param>
            /// <param name="hKey"></param>
            /// <param name="tKey"></param>
            /// <param name="j1Key"></param>
            /// <param name="j2Key"></param>
            /// <returns></returns>
            public static byte[] HT2XorPlusDecryptBytes(string filePath, byte hKey, byte tKey, byte j1Key, byte j2Key)
            {
                byte[] buffer = new byte[BUFFER_SIZE];
                long fileLength;

                try
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        fileLength = fileStream.Length;
                        using (var memoryStream = new MemoryStream())
                        {
                            int bytesRead;

                            // 讀取文件並進行解密
                            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                // 每隔 2 字節解密
                                for (int i = 0; i < bytesRead >> 1; i++)
                                {
                                    int s1 = i << 1;
                                    int s2 = s1 + 1;

                                    buffer[s1] ^= j1Key;     // 解密每隔 2 字節
                                    if (s2 < bytesRead)      // 確保不超出範圍
                                    {
                                        buffer[s2] ^= j2Key; // 每隔 2 字節解密
                                    }
                                }

                                // 處理第一個字節 (文件頭)
                                if (memoryStream.Position == 0)
                                {
                                    buffer[0] ^= hKey;
                                }

                                // 處理最後一個字節 (文件尾)
                                if (bytesRead > 0 && bytesRead == buffer.Length)
                                {
                                    buffer[bytesRead - 1] ^= tKey;
                                }

                                memoryStream.Write(buffer, 0, bytesRead);
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
            /// Head-Tail 2 XOR Plus 加密文件 【檢測OK】
            /// </summary>
            /// <param name="rawBytes"></param>
            /// <param name="hKey"></param>
            /// <param name="tKey"></param>
            /// <param name="j1Key"></param>
            /// <param name="j2Key"></param>
            /// <returns></returns>
            public static bool HT2XorPlusEncryptBytes(byte[] rawBytes, byte hKey, byte tKey, byte j1Key, byte j2Key)
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
                catch
                {
                    return false;
                }

                return true;
            }

            /// <summary>
            /// Head-Tail 2 XOR Plus 解密文件 【檢測OK】
            /// </summary>
            /// <param name="encryptBytes"></param>
            /// <param name="hKey"></param>
            /// <param name="tKey"></param>
            /// <param name="j1Key"></param>
            /// <param name="j2Key"></param>
            /// <returns></returns>
            public static bool HT2XorPlusDecryptBytes(byte[] encryptBytes, byte hKey, byte tKey, byte j1Key, byte j2Key)
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

                return true;
            }

            /// <summary>
            /// 返回 Head-Tail 2 XOR Plus 解密 Stream 【檢測OK】
            /// </summary>
            /// <param name="encryptFile"></param>
            /// <param name="hKey"></param>
            /// <param name="tKey"></param>
            /// <param name="j1Key"></param>
            /// <param name="j2Key"></param>
            /// <returns></returns>
            public static Stream HT2XorPlusDecryptStream(string encryptFile, byte hKey, byte tKey, byte j1Key, byte j2Key)
            {
                try
                {
                    // 建立 FileStream 以逐步讀取加密文件
                    using (FileStream fsDecrypt = new FileStream(encryptFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        // 創建 MemoryStream 用於存放解密後的數據
                        MemoryStream msDecrypt = new MemoryStream();
                        // 使用一個緩衝區來逐步讀取文件
                        byte[] buffer = new byte[BUFFER_SIZE];
                        int bytesRead;
                        long totalBytesRead = 0;
                        long fileLength = fsDecrypt.Length;

                        // 逐步讀取文件內容, 並進行解密
                        while ((bytesRead = fsDecrypt.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            // 每隔 2 字節解密
                            for (int i = 0; i < bytesRead >> 1; i++)
                            {
                                int s1 = i << 1;
                                int s2 = s1 + 1;
                                buffer[s1] ^= j1Key;     // 解密每隔 2 字節
                                if (s2 < bytesRead)
                                {
                                    buffer[s2] ^= j2Key; // 每隔 2 字節解密
                                }
                            }

                            // 處理文件頭 (第一個字節)
                            if (totalBytesRead == 0)
                            {
                                buffer[0] ^= hKey; // 處理第一個字節
                            }

                            // 處理文件尾 (最後一個字節)
                            if (totalBytesRead + bytesRead == fileLength)
                            {
                                buffer[bytesRead - 1] ^= tKey;     // 處理最後一個字節
                            }

                            msDecrypt.Write(buffer, 0, bytesRead); // 將解密後的數據寫入 MemoryStream
                            totalBytesRead += bytesRead;
                        }

                        msDecrypt.Position = 0; // 將 MemoryStream 的位置設置為開頭以便後續讀取
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
                /// AES 加密文件 【檢測OK】
                /// </summary>
                /// <param name="sourceFile"></param>
                /// <param name="key"></param>
                /// <param name="iv"></param>
                /// <returns></returns>
                public static bool AesEncryptFile(string sourceFile, string key = null, string iv = null)
                {
                    if (string.IsNullOrEmpty(sourceFile) || !File.Exists(sourceFile))
                    {
                        return false;
                    }

                    try
                    {
                        AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
                        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                        SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
                        byte[] keyData = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                        byte[] ivData = md5.ComputeHash(Encoding.UTF8.GetBytes(iv));
                        aes.Key = keyData;
                        aes.IV = ivData;

                        using (FileStream fsSource = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.None))
                        {
                            byte[] dataBytes = new byte[fsSource.Length];
                            fsSource.Read(dataBytes, 0, dataBytes.Length);
                            fsSource.Dispose();
                            File.Delete(sourceFile);

                            using (FileStream fsEncrypt = new FileStream(sourceFile, FileMode.Create, FileAccess.Write))
                            {
                                //文件加密
                                using (CryptoStream cs = new CryptoStream(fsEncrypt, aes.CreateEncryptor(), CryptoStreamMode.Write))
                                {
                                    cs.Write(dataBytes, 0, dataBytes.Length);
                                    cs.FlushFinalBlock();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.Print<Logger>($"<color=#FF0000>File Encrypt failed.</color> {ex}");
                        return false;
                    }

                    return true;
                }

                /// <summary>
                /// AES 解密文件 【檢測OK】
                /// </summary>
                /// <param name="encryptFile"></param>
                /// <param name="key"></param>
                /// <param name="iv"></param>
                /// <returns></returns>
                public static bool AesDecryptFile(string encryptFile, string key = null, string iv = null)
                {
                    try
                    {
                        AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
                        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                        SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
                        byte[] keyData = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                        byte[] ivData = md5.ComputeHash(Encoding.UTF8.GetBytes(iv));
                        aes.Key = keyData;
                        aes.IV = ivData;

                        using (FileStream fsSource = new FileStream(encryptFile, FileMode.Open, FileAccess.Read, FileShare.None))
                        {
                            byte[] dataBytes = new byte[fsSource.Length];
                            fsSource.Read(dataBytes, 0, dataBytes.Length);
                            fsSource.Dispose();
                            File.Delete(encryptFile);

                            using (FileStream fsDecrypt = new FileStream(encryptFile, FileMode.Create, FileAccess.Write))
                            {
                                //文件解密
                                using (CryptoStream cs = new CryptoStream(fsDecrypt, aes.CreateDecryptor(), CryptoStreamMode.Write))
                                {
                                    cs.Write(dataBytes, 0, dataBytes.Length);
                                    cs.FlushFinalBlock();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.Print<Logger>($"<color=#FF0000>File Decrypt failed.</color> {ex}");
                        return false;
                    }

                    return true;
                }
            }

            /// <summary>
            /// AES 加密文件 【檢測OK】
            /// </summary>
            /// <param name="filePath"></param>
            /// <param name="key"></param>
            /// <param name="iv"></param>
            /// <returns></returns>
            public static byte[] AesEncryptBytes(string filePath, string key = null, string iv = null)
            {
                try
                {
                    using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
                    using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
                    using (SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider())
                    using (FileStream fsInput = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    using (MemoryStream msEncrypted = new MemoryStream())
                    {
                        // 生成加密密鑰和 IV
                        byte[] keyData = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                        byte[] ivData = md5.ComputeHash(Encoding.UTF8.GetBytes(iv));
                        aes.Key = keyData;
                        aes.IV = ivData;

                        // 準備加密流
                        using (CryptoStream cs = new CryptoStream(msEncrypted, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            byte[] buffer = new byte[BUFFER_SIZE];
                            int bytesRead;

                            // 逐步讀取文件並寫入加密流
                            while ((bytesRead = fsInput.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                cs.Write(buffer, 0, bytesRead);
                            }
                            cs.FlushFinalBlock();     // 確保所有數據都被加密
                        }

                        return msEncrypted.ToArray(); // 返回加密後的 byte[]
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return null;
                }
            }

            /// <summary>
            /// AES 解密文件 【檢測OK】
            /// </summary>
            /// <param name="filePath"></param>
            /// <param name="key"></param>
            /// <param name="iv"></param>
            /// <returns></returns>
            public static byte[] AesDecryptBytes(string filePath, string key = null, string iv = null)
            {
                try
                {
                    using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
                    using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
                    using (SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider())
                    using (FileStream fsInput = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    using (MemoryStream msDecrypted = new MemoryStream())
                    {
                        // 生成解密密鑰和 IV
                        byte[] keyData = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                        byte[] ivData = md5.ComputeHash(Encoding.UTF8.GetBytes(iv));
                        aes.Key = keyData;
                        aes.IV = ivData;

                        // 準備解密流
                        using (CryptoStream cs = new CryptoStream(fsInput, aes.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            byte[] buffer = new byte[BUFFER_SIZE];
                            int bytesRead;

                            // 逐步讀取加密數據並寫入解密流
                            while ((bytesRead = cs.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                msDecrypted.Write(buffer, 0, bytesRead);
                            }
                        }

                        return msDecrypted.ToArray(); // 返回解密後的 byte[]
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    return null;
                }
            }

            /// <summary>
            /// AES 加密文件 【檢測OK】
            /// </summary>
            /// <param name="rawBytes"></param>
            /// <param name="key"></param>
            /// <param name="iv"></param>
            /// <returns></returns>
            public static bool AesEncryptBytes(ref byte[] rawBytes, string key = null, string iv = null)
            {
                try
                {
                    AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
                    MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                    SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
                    byte[] keyData = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                    byte[] ivData = md5.ComputeHash(Encoding.UTF8.GetBytes(iv));
                    aes.Key = keyData;
                    aes.IV = ivData;

                    using (MemoryStream msSource = new MemoryStream())
                    {
                        //文件加密
                        using (CryptoStream cs = new CryptoStream(msSource, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(rawBytes, 0, rawBytes.Length);
                        }
                        rawBytes = msSource.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    Logging.Print<Logger>($"<color=#FF0000>File Encrypt failed.</color> {ex}");
                    return false;
                }

                return true;
            }

            /// <summary>
            /// AES 解密文件 【檢測OK】
            /// </summary>
            /// <param name="encryptBytes"></param>
            /// <param name="key"></param>
            /// <param name="iv"></param>
            /// <returns></returns>
            public static bool AesDecryptBytes(byte[] encryptBytes, string key = null, string iv = null)
            {
                try
                {
                    AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
                    MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                    SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
                    byte[] keyData = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                    byte[] ivData = md5.ComputeHash(Encoding.UTF8.GetBytes(iv));
                    aes.Key = keyData;
                    aes.IV = ivData;

                    using (MemoryStream msEncrypt = new MemoryStream(encryptBytes))
                    {
                        // 文件解密
                        using (CryptoStream cs = new CryptoStream(msEncrypt, aes.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            int idx = 0;
                            int data;
                            while ((data = cs.ReadByte()) != -1)
                            {
                                encryptBytes[idx] = (byte)data;
                                idx++;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logging.Print<Logger>($"<color=#FF0000>File Decrypt failed.</color> {ex}");
                    return false;
                }

                return true;
            }

            /// <summary>
            /// 返回 AES 解密 Stream 【檢測OK】
            /// </summary>
            /// <param name="encryptFile"></param>
            /// <param name="key"></param>
            /// <param name="iv"></param>
            /// <returns></returns>
            public static Stream AesDecryptStream(string encryptFile, string key = null, string iv = null)
            {
                try
                {
                    using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
                    using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
                    using (SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider())
                    using (FileStream fsDecrypt = new FileStream(encryptFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        // 生成解密密鑰和 IV
                        byte[] keyData = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                        byte[] ivData = md5.ComputeHash(Encoding.UTF8.GetBytes(iv));
                        aes.Key = keyData;
                        aes.IV = ivData;

                        // 準備解密流
                        using (CryptoStream cs = new CryptoStream(fsDecrypt, aes.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            // 創建 MemoryStream 用於存放解密後的數據
                            MemoryStream msDecrypt = new MemoryStream();
                            byte[] buffer = new byte[BUFFER_SIZE];
                            int bytesRead;

                            // 逐步讀取加密數據並寫入解密流
                            while ((bytesRead = cs.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                msDecrypt.Write(buffer, 0, bytesRead);
                            }

                            msDecrypt.Position = 0; // 將 MemoryStream 的位置設置為開頭以便後續讀取
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
    }
}