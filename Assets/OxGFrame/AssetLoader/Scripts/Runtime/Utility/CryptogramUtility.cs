using OxGFrame.AssetLoader.Bundle;
using System.IO;

namespace OxGFrame.AssetLoader.Utility
{
    public class CryptogramUtility
    {
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

        public static void XorEncryptBundleFiles(string dir, byte key = 0)
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

        public static void XorDecryptBundleFiles(string dir, byte key = 0)
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

        public static void HTXorEncryptBundleFiles(string dir, byte hKey = 0, byte tKey = 0)
        {
            // 取得目錄下所有檔案
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有檔案進行加密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各檔案的加密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.HTXOR.WriteFile.HTXorEncryptFile(fPath, hKey, tKey);
            }
        }

        public static void HTXorDecryptBundleFiles(string dir, byte hKey = 0, byte tKey = 0)
        {
            // 取得目錄下所有檔案
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有檔案進行解密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各檔案的解密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.HTXOR.WriteFile.HTXorDecryptFile(fPath, hKey, tKey);
            }
        }

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
    }
}