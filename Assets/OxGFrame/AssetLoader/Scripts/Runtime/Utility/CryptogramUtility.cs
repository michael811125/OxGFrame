using OxGFrame.AssetLoader.Bundle;
using System.IO;

namespace OxGFrame.AssetLoader.Utility
{
    public class CryptogramUtility
    {
        #region AES
        public static void AesEncryptBundleFiles(string dir, string key = null, string iv = null)
        {
            // 取得目錄下所有檔案
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有檔案進行加密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各檔案的加密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.AES.WriteFile.AesEncryptFile(fPath, key, iv);
            }
        }

        public static void AesDecryptBundleFiles(string dir, string key = null, string iv = null)
        {
            // 取得目錄下所有檔案
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有檔案進行解密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各檔案的解密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.AES.WriteFile.AesDecryptFile(fPath, key, iv);
            }
        }
        #endregion

        #region Xor
        public static void XorEncryptBundleFiles(string dir, byte key)
        {
            // 取得目錄下所有檔案
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有檔案進行加密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各檔案的加密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.XOR.WriteFile.XorEncryptFile(fPath, key);
            }
        }

        public static void XorDecryptBundleFiles(string dir, byte key)
        {
            // 取得目錄下所有檔案
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有檔案進行解密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各檔案的解密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.XOR.WriteFile.XorDecryptFile(fPath, key);
            }
        }
        #endregion

        #region HT2Xor
        public static void HT2XorEncryptBundleFiles(string dir, byte hKey, byte tKey, byte jKey)
        {
            // 取得目錄下所有檔案
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有檔案進行加密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各檔案的加密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.HT2XOR.WriteFile.HT2XorEncryptFile(fPath, hKey, tKey, jKey);
            }
        }

        public static void HT2XorDecryptBundleFiles(string dir, byte hKey, byte tKey, byte jKey)
        {
            // 取得目錄下所有檔案
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有檔案進行解密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各檔案的解密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.HT2XOR.WriteFile.HT2XorDecryptFile(fPath, hKey, tKey, jKey);
            }
        }
        #endregion

        #region HT2XorPlus
        public static void HT2XorPlusEncryptBundleFiles(string dir, byte hKey, byte tKey, byte j1Key, byte j2key)
        {
            // 取得目錄下所有檔案
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有檔案進行加密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各檔案的加密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.HT2XORPlus.WriteFile.HT2XorPlusEncryptFile(fPath, hKey, tKey, j1Key, j2key);
            }
        }

        public static void HT2XorPlusDecryptBundleFiles(string dir, byte hKey, byte tKey, byte j1Key, byte j2key)
        {
            // 取得目錄下所有檔案
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有檔案進行解密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各檔案的解密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.HT2XORPlus.WriteFile.HT2XorPlusDecryptFile(fPath, hKey, tKey, j1Key, j2key);
            }
        }
        #endregion

        #region Offset
        public static void OffsetEncryptBundleFiles(string dir, int randomSeed, int dummySize = 0)
        {
            // 取得目錄下所有檔案
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有檔案進行加密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各檔案的加密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.Offset.WriteFile.OffsetEncryptFile(fPath, randomSeed, dummySize);
            }
        }

        public static void OffsetDecryptBundleFiles(string dir, int dummySize = 0)
        {
            // 取得目錄下所有檔案
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有檔案進行解密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各檔案的解密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.Offset.WriteFile.OffsetDecryptFile(fPath, dummySize);
            }
        }
        #endregion
    }
}