using OxGFrame.AssetLoader.Bundle;
using System.IO;

namespace OxGFrame.AssetLoader.Utility
{
    public class CryptogramUtility
    {
        #region Offset
        public static void OffsetEncryptBundleFiles(string dir, int dummySize)
        {
            // 取得目錄下所有檔案
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有檔案進行加密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各檔案的加密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.Offset.WriteFile.EncryptFile(fPath, dummySize);
            }
        }

        public static void OffsetDecryptBundleFiles(string dir, int dummySize)
        {
            // 取得目錄下所有檔案
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有檔案進行解密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各檔案的解密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.Offset.WriteFile.DecryptFile(fPath, dummySize);
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
                FileCryptogram.XOR.WriteFile.EncryptFile(fPath, key);
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
                FileCryptogram.XOR.WriteFile.DecryptFile(fPath, key);
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
                FileCryptogram.HT2XOR.WriteFile.EncryptFile(fPath, hKey, tKey, jKey);
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
                FileCryptogram.HT2XOR.WriteFile.DecryptFile(fPath, hKey, tKey, jKey);
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
                FileCryptogram.HT2XORPlus.WriteFile.EncryptFile(fPath, hKey, tKey, j1Key, j2key);
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
                FileCryptogram.HT2XORPlus.WriteFile.DecryptFile(fPath, hKey, tKey, j1Key, j2key);
            }
        }
        #endregion

        #region AES
        public static void AesEncryptBundleFiles(string dir, string key, string iv)
        {
            // 取得目錄下所有檔案
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有檔案進行加密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各檔案的加密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.AES.WriteFile.EncryptFile(fPath, key, iv);
            }
        }

        public static void AesDecryptBundleFiles(string dir, string key, string iv)
        {
            // 取得目錄下所有檔案
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有檔案進行解密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各檔案的解密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.AES.WriteFile.DecryptFile(fPath, key, iv);
            }
        }
        #endregion

        #region ChaCha20
        public static void ChaCha20EncryptBundleFiles(string dir, string key, string nonce, uint counter)
        {
            // 取得目錄下所有檔案
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有檔案進行加密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各檔案的加密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.ChaCha20.WriteFile.EncryptFile(fPath, key, nonce, counter);
            }
        }

        public static void ChaCha20DecryptBundleFiles(string dir, string key, string nonce, uint counter)
        {
            // 取得目錄下所有檔案
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有檔案進行解密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各檔案的解密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.ChaCha20.WriteFile.DecryptFile(fPath, key, nonce, counter);
            }
        }
        #endregion

        #region XXTEA
        public static void XXTEAEncryptBundleFiles(string dir, string key)
        {
            // 取得目錄下所有檔案
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有檔案進行加密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各檔案的加密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.XXTEA.WriteFile.EncryptFile(fPath, key);
            }
        }

        public static void XXTEADecryptBundleFiles(string dir, string key)
        {
            // 取得目錄下所有檔案
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有檔案進行解密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各檔案的解密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.XXTEA.WriteFile.DecryptFile(fPath, key);
            }
        }
        #endregion

        #region OffsetXOR
        public static void OffsetXorEncryptBundleFiles(string dir, byte key, int dummySize)
        {
            // 取得目錄下所有檔案
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有檔案進行加密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各檔案的加密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.OffsetXOR.WriteFile.EncryptFile(fPath, key, dummySize);
            }
        }

        public static void OffsetXorDecryptBundleFiles(string dir, byte key, int dummySize)
        {
            // 取得目錄下所有檔案
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有檔案進行解密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各檔案的解密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.OffsetXOR.WriteFile.DecryptFile(fPath, key, dummySize);
            }
        }
        #endregion
    }
}