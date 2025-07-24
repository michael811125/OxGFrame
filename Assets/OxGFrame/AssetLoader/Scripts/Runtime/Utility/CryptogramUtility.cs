using OxGFrame.AssetLoader.Bundle;
using System.IO;
using UnityEngine;

namespace OxGFrame.AssetLoader.Utility
{
    public class CryptogramUtility
    {
        #region Offset
        public static void OffsetEncryptBundleFiles(string dir, int dummySize)
        {
            // 取得目錄下所有文件
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有文件進行加密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各文件的加密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.Offset.WriteFile.EncryptFile(fPath, dummySize);
            }
        }

        public static void OffsetDecryptBundleFiles(string dir, int dummySize)
        {
            // 取得目錄下所有文件
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有文件進行解密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各文件的解密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.Offset.WriteFile.DecryptFile(fPath, dummySize);
            }
        }
        #endregion

        #region Xor
        public static void XorEncryptBundleFiles(string dir, byte key)
        {
            // 取得目錄下所有文件
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有文件進行加密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各文件的加密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.XOR.WriteFile.EncryptFile(fPath, key);
            }
        }

        public static void XorDecryptBundleFiles(string dir, byte key)
        {
            // 取得目錄下所有文件
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有文件進行解密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各文件的解密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.XOR.WriteFile.DecryptFile(fPath, key);
            }
        }
        #endregion

        #region HT2Xor
        public static void HT2XorEncryptBundleFiles(string dir, byte hKey, byte tKey, byte jKey)
        {
            // 取得目錄下所有文件
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有文件進行加密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各文件的加密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.HT2XOR.WriteFile.EncryptFile(fPath, hKey, tKey, jKey);
            }
        }

        public static void HT2XorDecryptBundleFiles(string dir, byte hKey, byte tKey, byte jKey)
        {
            // 取得目錄下所有文件
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有文件進行解密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各文件的解密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.HT2XOR.WriteFile.DecryptFile(fPath, hKey, tKey, jKey);
            }
        }
        #endregion

        #region HT2XorPlus
        public static void HT2XorPlusEncryptBundleFiles(string dir, byte hKey, byte tKey, byte j1Key, byte j2key)
        {
            // 取得目錄下所有文件
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有文件進行加密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各文件的加密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.HT2XORPlus.WriteFile.EncryptFile(fPath, hKey, tKey, j1Key, j2key);
            }
        }

        public static void HT2XorPlusDecryptBundleFiles(string dir, byte hKey, byte tKey, byte j1Key, byte j2key)
        {
            // 取得目錄下所有文件
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有文件進行解密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各文件的解密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.HT2XORPlus.WriteFile.DecryptFile(fPath, hKey, tKey, j1Key, j2key);
            }
        }
        #endregion

        #region AES
        public static void AesEncryptBundleFiles(string dir, string key, string iv)
        {
            // 取得目錄下所有文件
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有文件進行加密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各文件的加密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.AES.WriteFile.EncryptFile(fPath, key, iv);
            }
        }

        public static void AesDecryptBundleFiles(string dir, string key, string iv)
        {
            // 取得目錄下所有文件
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有文件進行解密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各文件的解密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.AES.WriteFile.DecryptFile(fPath, key, iv);
            }
        }
        #endregion

        #region ChaCha20
        public static void ChaCha20EncryptBundleFiles(string dir, string key, string nonce, uint counter)
        {
            // 取得目錄下所有文件
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有文件進行加密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各文件的加密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.ChaCha20.WriteFile.EncryptFile(fPath, key, nonce, counter);
            }
        }

        public static void ChaCha20DecryptBundleFiles(string dir, string key, string nonce, uint counter)
        {
            // 取得目錄下所有文件
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有文件進行解密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各文件的解密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.ChaCha20.WriteFile.DecryptFile(fPath, key, nonce, counter);
            }
        }
        #endregion

        #region XXTEA
        public static void XXTEAEncryptBundleFiles(string dir, string key)
        {
            // 取得目錄下所有文件
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有文件進行加密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各文件的加密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.XXTEA.WriteFile.EncryptFile(fPath, key);
            }
        }

        public static void XXTEADecryptBundleFiles(string dir, string key)
        {
            // 取得目錄下所有文件
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有文件進行解密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各文件的解密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.XXTEA.WriteFile.DecryptFile(fPath, key);
            }
        }
        #endregion

        #region OffsetXOR
        public static void OffsetXorEncryptBundleFiles(string dir, byte key, int dummySize)
        {
            // 取得目錄下所有文件
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有文件進行加密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各文件的加密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.OffsetXOR.WriteFile.EncryptFile(fPath, key, dummySize);
            }
        }

        public static void OffsetXorDecryptBundleFiles(string dir, byte key, int dummySize)
        {
            // 取得目錄下所有文件
            FileInfo[] files = BundleUtility.GetFilesRecursively(dir);

            // 對所有文件進行解密
            for (int i = 0; i < files.Length; i++)
            {
                // 執行各文件的解密
                string fPath = Path.Combine(files[i].Directory.ToString(), files[i].Name);
                FileCryptogram.OffsetXOR.WriteFile.DecryptFile(fPath, key, dummySize);
            }
        }
        #endregion

        internal static class CryptogramSettingSetup
        {
            private static CryptogramSetting _cryptogramSetting;

            public static CryptogramSetting GetCryptogramSetting()
            {
                if (_cryptogramSetting == null)
                    _cryptogramSetting = LoadSettingData<CryptogramSetting>();
                return _cryptogramSetting;
            }

            /// <summary>
            /// 加載相關配置文件
            /// </summary>
            internal static TSetting LoadSettingData<TSetting>() where TSetting : ScriptableObject
            {
#if UNITY_EDITOR
                var settingType = typeof(TSetting);
                var guids = UnityEditor.AssetDatabase.FindAssets($"t:{settingType.Name}");
                if (guids.Length == 0)
                {
                    Debug.LogWarning($"Create new {settingType.Name}.asset");
                    var setting = ScriptableObject.CreateInstance<TSetting>();
                    string filePath = $"Assets/{settingType.Name}.asset";
                    UnityEditor.AssetDatabase.CreateAsset(setting, filePath);
                    UnityEditor.AssetDatabase.SaveAssets();
                    UnityEditor.AssetDatabase.Refresh();
                    return setting;
                }
                else
                {
                    if (guids.Length != 1)
                    {
                        foreach (var guid in guids)
                        {
                            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                            Debug.LogWarning($"Found multiple file : {path}");
                        }
                        throw new System.Exception($"Found multiple {settingType.Name} files !");
                    }

                    string filePath = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    var setting = UnityEditor.AssetDatabase.LoadAssetAtPath<TSetting>(filePath);
                    return setting;
                }
#else
                return default;
#endif
            }
        }

    }
}