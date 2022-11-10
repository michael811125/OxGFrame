using Cysharp.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace AssetLoader.Zip
{
    public static class Compressor
    {
        public delegate void Progression(float progress, long actualSize, long totalSize);

        private static CancellationTokenSource _cts = new CancellationTokenSource();

        #region ZipCallback
        public abstract class ZipCallback
        {
            /// <summary>
            /// 壓縮單個文件或文件夾前執行的回調
            /// </summary>
            /// <param name="zEntry"></param>
            /// <returns>如果返回true，則壓縮文件或文件夾，反之則不壓縮文件或文件夾</returns>
            public virtual bool OnPreZip(ZipEntry zEntry)
            {
                return true;
            }

            /// <summary>
            /// 壓縮單個文件或文件夾後執行的回調
            /// </summary>
            /// <param name="zEntry"></param>
            public virtual void OnPostZip(ZipEntry zEntry) { }

            /// <summary>
            /// 壓縮執行完畢後的回調
            /// </summary>
            /// <param name="result">true表示壓縮成功，false表示壓縮失敗</param>
            public virtual void OnFinished(bool result) { }
        }
        #endregion

        #region UnzipCallback
        public abstract class UnzipCallback
        {
            /// <summary>
            /// 解壓單個文件或文件夾前執行的回調
            /// </summary>
            /// <param name="zEntry"></param>
            /// <returns>如果返回true，則壓縮文件或文件夾，反之則不壓縮文件或文件夾</returns>
            public virtual bool OnPreUnzip(ZipEntry zEntry)
            {
                return true;
            }

            /// <summary>
            /// 解壓單個文件或文件夾後執行的回調
            /// </summary>
            /// <param name="zEntry"></param>
            public virtual void OnPostUnzip(ZipEntry zEntry) { }

            /// <summary>
            /// 解壓執行完畢後的回調
            /// </summary>
            /// <param name="result">true表示解壓成功，false表示解壓失敗</param>
            public virtual void OnFinished(bool result) { }
        }
        #endregion

        #region Zip
        /// <summary>
        /// 用於壓縮記錄檔案大小
        /// </summary>
        private static long _actualSize = 0;

        /// <summary>
        /// 【異步壓縮】資料夾
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="outputPath"></param>
        /// <param name="password"></param>
        /// <param name="compressLevel"></param>
        /// <param name="compressionMethod"></param>
        /// <param name="zipCallback"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        public static async UniTask<bool> ZipAsync(string inputPath, string outputPath, string password = null, int compressLevel = 6, CompressionMethod compressionMethod = CompressionMethod.Deflated, ZipCallback zipCallback = null, Progression progression = null)
        {
            // 檔案大小記錄歸零
            _actualSize = 0;

            if (string.IsNullOrEmpty(inputPath) || string.IsNullOrEmpty(outputPath))
            {
                if (zipCallback != null) zipCallback.OnFinished(false);

                return false;
            }

            // 根目錄的檔案
            string[] dirFiles = Directory.GetFiles(inputPath, "*.*", SearchOption.TopDirectoryOnly);
            // 根目錄中的資料夾
            string[] dirs = Directory.GetDirectories(inputPath);
            // 合併 (根目錄的檔案 & 根目錄中的資料夾)
            string[] filesAndDirs = dirFiles.Union(dirs).ToArray();

            // 計算總大小
            string[] files = Directory.GetFiles(inputPath, "*.*", SearchOption.AllDirectories);
            long totalSize = 0;
            foreach (var file in files)
            {
                if (File.Exists(file))
                {
                    FileInfo info = new FileInfo(file);
                    totalSize += info.Length;
                }

                await UniTask.Yield(_cts.Token);
            }

            ZipOutputStream zipOutputStream = new ZipOutputStream(File.Create(outputPath));
            zipOutputStream.SetLevel(compressLevel);
            if (!string.IsNullOrEmpty(password)) zipOutputStream.Password = password;

            for (int index = 0; index < filesAndDirs.Length; ++index)
            {
                bool result = false;
                string fileOrDir = filesAndDirs[index];
                if (Directory.Exists(fileOrDir))
                {
                    result = await _ZipDirectoryAsync(totalSize, fileOrDir, string.Empty, zipOutputStream, compressionMethod, zipCallback, progression);
                }
                else if (File.Exists(fileOrDir))
                {
                    result = await _ZipFileAsync(totalSize, fileOrDir, string.Empty, zipOutputStream, compressionMethod, zipCallback, progression);
                }

                if (!result)
                {
                    if (zipCallback != null) zipCallback.OnFinished(false);

                    return false;
                }

                await UniTask.Yield(_cts.Token);
            }

            if (zipOutputStream != null) zipOutputStream.Dispose();

            // 完成
            if (zipCallback != null) zipCallback.OnFinished(true);

            Debug.Log("Zip Completes");

            return true;
        }

        /// <summary>
        /// 【壓縮】資料夾
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="outputPath"></param>
        /// <param name="password"></param>
        /// <param name="compressLevel"></param>
        /// <param name="compressionMethod"></param>
        /// <param name="zipCallback"></param>
        /// <returns></returns>
        public static bool Zip(string inputPath, string outputPath, string password = null, int compressLevel = 6, CompressionMethod compressionMethod = CompressionMethod.Deflated, ZipCallback zipCallback = null)
        {
            // 檔案大小記錄歸零
            _actualSize = 0;

            if (string.IsNullOrEmpty(inputPath) || string.IsNullOrEmpty(outputPath))
            {
                if (zipCallback != null) zipCallback.OnFinished(false);

                return false;
            }

            // 根目錄的檔案
            string[] dirFiles = Directory.GetFiles(inputPath, "*.*", SearchOption.TopDirectoryOnly);
            // 根目錄中的資料夾
            string[] dirs = Directory.GetDirectories(inputPath);
            // 合併 (根目錄的檔案 & 根目錄中的資料夾)
            string[] filesAndDirs = dirFiles.Union(dirs).ToArray();

            ZipOutputStream zipOutputStream = new ZipOutputStream(File.Create(outputPath));
            zipOutputStream.SetLevel(compressLevel);
            if (!string.IsNullOrEmpty(password)) zipOutputStream.Password = password;

            for (int index = 0; index < filesAndDirs.Length; ++index)
            {
                bool result = false;
                string fileOrDir = filesAndDirs[index];
                if (Directory.Exists(fileOrDir))
                {
                    result = _ZipDirectory(fileOrDir, string.Empty, zipOutputStream, compressionMethod, zipCallback);
                }
                else if (File.Exists(fileOrDir))
                {
                    result = _ZipFile(fileOrDir, string.Empty, zipOutputStream, compressionMethod, zipCallback);
                }

                if (!result)
                {
                    if (zipCallback != null) zipCallback.OnFinished(false);

                    return false;
                }
            }

            if (zipOutputStream != null) zipOutputStream.Dispose();

            // 完成
            if (zipCallback != null) zipCallback.OnFinished(true);

            Debug.Log("Zip Completes");

            return true;
        }

        /// <summary>
        /// 【異步壓縮】檔案
        /// </summary>
        /// <param name="inputFiles"></param>
        /// <param name="outputPath"></param>
        /// <param name="password"></param>
        /// <param name="compressLevel"></param>
        /// <param name="compressionMethod"></param>
        /// <param name="zipCallback"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        public static async UniTask<bool> ZipAsync(string[] inputFiles, string outputPath, string password = null, int compressLevel = 6, CompressionMethod compressionMethod = CompressionMethod.Deflated, ZipCallback zipCallback = null, Progression progression = null)
        {
            // 檔案大小記錄歸零
            _actualSize = 0;

            if (inputFiles == null || inputFiles.Length == 0 || string.IsNullOrEmpty(outputPath))
            {
                if (zipCallback != null) zipCallback.OnFinished(false);

                return false;
            }

            // 計算總大小
            long totalSize = 0;
            foreach (var file in inputFiles)
            {
                if (File.Exists(file))
                {
                    FileInfo info = new FileInfo(file);
                    totalSize += info.Length;
                }
            }

            ZipOutputStream zipOutputStream = new ZipOutputStream(File.Create(outputPath));
            zipOutputStream.SetLevel(compressLevel);
            if (!string.IsNullOrEmpty(password)) zipOutputStream.Password = password;

            for (int index = 0; index < inputFiles.Length; ++index)
            {
                bool result = false;
                string file = inputFiles[index];
                if (File.Exists(file))
                {
                    result = await _ZipFileAsync(totalSize, file, string.Empty, zipOutputStream, compressionMethod, zipCallback, progression);
                }

                if (!result)
                {
                    if (zipCallback != null) zipCallback.OnFinished(false);

                    return false;
                }

                await UniTask.Yield(_cts.Token);
            }

            if (zipOutputStream != null) zipOutputStream.Dispose();

            // 完成
            if (zipCallback != null) zipCallback.OnFinished(true);

            return true;
        }

        /// <summary>
        /// 【壓縮】檔案
        /// </summary>
        /// <param name="inputFiles"></param>
        /// <param name="outputPath"></param>
        /// <param name="password"></param>
        /// <param name="compressLevel"></param>
        /// <param name="compressionMethod"></param>
        /// <param name="zipCallback"></param>
        /// <returns></returns>
        public static bool Zip(string[] inputFiles, string outputPath, string password = null, int compressLevel = 6, CompressionMethod compressionMethod = CompressionMethod.Deflated, ZipCallback zipCallback = null)
        {
            // 檔案大小記錄歸零
            _actualSize = 0;

            if (inputFiles == null || inputFiles.Length == 0 || string.IsNullOrEmpty(outputPath))
            {
                if (zipCallback != null) zipCallback.OnFinished(false);

                return false;
            }

            ZipOutputStream zipOutputStream = new ZipOutputStream(File.Create(outputPath));
            zipOutputStream.SetLevel(compressLevel);
            if (!string.IsNullOrEmpty(password)) zipOutputStream.Password = password;

            for (int index = 0; index < inputFiles.Length; ++index)
            {
                bool result = false;
                string file = inputFiles[index];
                if (File.Exists(file))
                {
                    result = _ZipFile(file, string.Empty, zipOutputStream, compressionMethod, zipCallback);
                }

                if (!result)
                {
                    if (zipCallback != null) zipCallback.OnFinished(false);

                    return false;
                }
            }

            if (zipOutputStream != null) zipOutputStream.Dispose();

            // 完成
            if (zipCallback != null) zipCallback.OnFinished(true);

            return true;
        }

        /// <summary>
        /// 【異步壓縮】資料夾
        /// </summary>
        /// <param name="totalSize"></param>
        /// <param name="dirPath"></param>
        /// <param name="relativePath"></param>
        /// <param name="zipOutputStream"></param>
        /// <param name="compressionMethod"></param>
        /// <param name="zipCallback"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        private static async UniTask<bool> _ZipDirectoryAsync(long totalSize, string dirPath, string relativePath, ZipOutputStream zipOutputStream, CompressionMethod compressionMethod, ZipCallback zipCallback, Progression progression)
        {
            ZipEntry zEntry;

            try
            {
                string entryName = Path.Combine(relativePath, Path.GetFileName(dirPath) + '/');
                zEntry = new ZipEntry(entryName);
                zEntry.CompressionMethod = compressionMethod;
                zEntry.DateTime = DateTime.Now;
                zEntry.Size = 0;

                if ((zipCallback != null) && !zipCallback.OnPreZip(zEntry)) return true;

                zipOutputStream.PutNextEntry(zEntry);
                zipOutputStream.Flush();

                string[] files = Directory.GetFiles(dirPath);
                for (int index = 0; index < files.Length; ++index)
                {
                    await _ZipFileAsync(totalSize, files[index], Path.Combine(relativePath, Path.GetFileName(dirPath)), zipOutputStream, compressionMethod, zipCallback, progression);
                }

            }
            catch (Exception ex)
            {
                Debug.Log($"<color=#ff3a87>【Directory】Zip:</color> {ex}");

                return false;
            }

            string[] directories = Directory.GetDirectories(dirPath);
            for (int index = 0; index < directories.Length; ++index)
            {
                string dir = directories[index];
                string rPath = Path.Combine(relativePath, Path.GetFileName(dirPath));

                if (!await _ZipDirectoryAsync(totalSize, dir, rPath, zipOutputStream, compressionMethod, zipCallback, progression)) return false;
            }

            if (zipCallback != null) zipCallback.OnPostZip(zEntry);

            return true;
        }

        /// <summary>
        /// 【壓縮】資料夾
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="relativePath"></param>
        /// <param name="zipOutputStream"></param>
        /// <param name="compressionMethod"></param>
        /// <param name="zipCallback"></param>
        /// <returns></returns>
        private static bool _ZipDirectory(string dirPath, string relativePath, ZipOutputStream zipOutputStream, CompressionMethod compressionMethod, ZipCallback zipCallback)
        {
            ZipEntry zEntry;

            try
            {
                string entryName = Path.Combine(relativePath, Path.GetFileName(dirPath) + '/');
                zEntry = new ZipEntry(entryName);
                zEntry.CompressionMethod = compressionMethod;
                zEntry.DateTime = DateTime.Now;
                zEntry.Size = 0;

                if ((zipCallback != null) && !zipCallback.OnPreZip(zEntry)) return true;

                zipOutputStream.PutNextEntry(zEntry);
                zipOutputStream.Flush();

                string[] files = Directory.GetFiles(dirPath);
                for (int index = 0; index < files.Length; ++index)
                {
                    _ZipFile(files[index], Path.Combine(relativePath, Path.GetFileName(dirPath)), zipOutputStream, compressionMethod, zipCallback);
                }

            }
            catch (Exception ex)
            {
                Debug.Log($"<color=#ff3a87>【Directory】Zip:</color> {ex}");

                return false;
            }

            string[] directories = Directory.GetDirectories(dirPath);
            for (int index = 0; index < directories.Length; ++index)
            {
                string dir = directories[index];
                string rPath = Path.Combine(relativePath, Path.GetFileName(dirPath));

                if (!_ZipDirectory(dir, rPath, zipOutputStream, compressionMethod, zipCallback)) return false;
            }

            if (zipCallback != null) zipCallback.OnPostZip(zEntry);

            return true;
        }

        /// <summary>
        /// 【異步壓縮】檔案
        /// </summary>
        /// <param name="totalSize"></param>
        /// <param name="filePath"></param>
        /// <param name="relativePath"></param>
        /// <param name="zipOutputStream"></param>
        /// <param name="compressionMethod"></param>
        /// <param name="zipCallback"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        private static async UniTask<bool> _ZipFileAsync(long totalSize, string filePath, string relativePath, ZipOutputStream zipOutputStream, CompressionMethod compressionMethod, ZipCallback zipCallback, Progression progression)
        {
            ZipEntry zEntry;
            FileStream fileStream = null;

            try
            {
                string entryName = relativePath + '/' + Path.GetFileName(filePath);
                zEntry = new ZipEntry(entryName);
                zEntry.CompressionMethod = compressionMethod;
                zEntry.DateTime = DateTime.Now;

                if ((zipCallback != null) && !zipCallback.OnPreZip(zEntry)) return true;

                fileStream = File.OpenRead(filePath);
                zEntry.Size = fileStream.Length;
                zipOutputStream.PutNextEntry(zEntry);
                byte[] buffer = new byte[fileStream.Length];

                while (true)
                {
                    int readSize = fileStream.Read(buffer, 0, buffer.Length);
                    if (readSize > 0)
                    {
                        _actualSize += readSize;

                        var progress = (float)_actualSize / totalSize;
                        if (progression != null) progression.Invoke(progress, _actualSize, totalSize);
                        //Debug.Log($"Zip Progress: {progress.ToString("f2")}%, ActualSize: {BundleDistributor.GetBytesToString((ulong)_actualSize)}, TotalSize: {BundleDistributor.GetBytesToString((ulong)totalSize)}");
                    }
                    else
                    {
                        zipOutputStream.Write(buffer, 0, buffer.Length);

                        if (fileStream != null) fileStream.Dispose();

                        break;
                    }

                    await UniTask.Yield(_cts.Token);
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"<color=#ff3a87>【File】Zip:</color> {ex}");

                if (fileStream != null) fileStream.Dispose();

                try
                {
                    if (zipOutputStream != null) zipOutputStream.Dispose();
                }
                catch
                {
                    zipOutputStream?.Dispose();
                }

                return false;
            }

            if (zipCallback != null) zipCallback.OnPostZip(zEntry);

            return true;
        }

        /// <summary>
        /// 【壓縮】檔案
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="relativePath"></param>
        /// <param name="zipOutputStream"></param>
        /// <param name="compressionMethod"></param>
        /// <param name="zipCallback"></param>
        /// <returns></returns>
        private static bool _ZipFile(string filePath, string relativePath, ZipOutputStream zipOutputStream, CompressionMethod compressionMethod, ZipCallback zipCallback)
        {
            ZipEntry zEntry;
            FileStream fileStream = null;

            try
            {
                string entryName = relativePath + '/' + Path.GetFileName(filePath);
                zEntry = new ZipEntry(entryName);
                zEntry.CompressionMethod = compressionMethod;
                zEntry.DateTime = DateTime.Now;

                if ((zipCallback != null) && !zipCallback.OnPreZip(zEntry)) return true;

                fileStream = File.OpenRead(filePath);
                byte[] buffer = new byte[fileStream.Length];
                fileStream.Read(buffer, 0, buffer.Length);
                fileStream.Close();

                zEntry.Size = buffer.Length;
                zipOutputStream.PutNextEntry(zEntry);
                zipOutputStream.Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                Debug.Log($"<color=#ff3a87>【File】Zip:</color> {ex}");

                if (fileStream != null) fileStream.Dispose();

                try
                {
                    if (zipOutputStream != null) zipOutputStream.Dispose();
                }
                catch
                {
                    zipOutputStream?.Dispose();
                }

                return false;
            }
            finally
            {
                if (fileStream != null) fileStream.Dispose();
            }

            if (zipCallback != null) zipCallback.OnPostZip(zEntry);

            return true;
        }
        #endregion

        #region Unzip
        /// <summary>
        /// 【異步解壓縮】壓縮包路徑 (支援壓縮後刪除壓縮包)
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="outputPath"></param>
        /// <param name="password"></param>
        /// <param name="unzipCallback"></param>
        /// <param name="bufferSize"></param>
        /// <param name="progression"></param>
        /// <param name="delete"></param>
        /// <returns></returns>
        public static async UniTask<bool> UnzipAsync(string inputPath, string outputPath, string password = null, UnzipCallback unzipCallback = null, int bufferSize = 4096, Progression progression = null, bool delete = false)
        {
            if (string.IsNullOrEmpty(inputPath) || string.IsNullOrEmpty(outputPath))
            {
                if (unzipCallback != null) unzipCallback.OnFinished(false);

                return false;
            }

            try
            {
                bool result = await UnzipAsync(File.OpenRead(inputPath), outputPath, password, unzipCallback, bufferSize, progression);
                if (result && delete && File.Exists(inputPath))
                {
                    File.Delete(inputPath);
                    Debug.Log($"<color=#5aff60>Delete Zip File Path: {inputPath}</color>");
                }

                Debug.Log($"<color=#5aff60>Unzip Completes</color>");

                return result;
            }
            catch (Exception ex)
            {
                Debug.Log($"<color=#ff3a87>Unzip:</color> {ex}");

                if (unzipCallback != null) unzipCallback.OnFinished(false);

                return false;
            }
        }

        /// <summary>
        /// 【解壓縮】壓縮包路徑 (支援壓縮後刪除壓縮包)
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="outputPath"></param>
        /// <param name="password"></param>
        /// <param name="unzipCallback"></param>
        /// <param name="bufferSize"></param>
        /// <param name="delete"></param>
        /// <returns></returns>
        public static bool Unzip(string inputPath, string outputPath, string password = null, UnzipCallback unzipCallback = null, int bufferSize = 4096, bool delete = false)
        {
            if (string.IsNullOrEmpty(inputPath) || string.IsNullOrEmpty(outputPath))
            {
                if (unzipCallback != null) unzipCallback.OnFinished(false);

                return false;
            }

            try
            {
                bool result = Unzip(File.OpenRead(inputPath), outputPath, password, unzipCallback, bufferSize);
                if (result && delete && File.Exists(inputPath))
                {
                    File.Delete(inputPath);
                    Debug.Log($"<color=#5aff60>Delete Zip File Path: {inputPath}</color>");
                }

                Debug.Log($"<color=#5aff60>Unzip Completes</color>");

                return result;
            }
            catch (Exception ex)
            {
                Debug.Log($"<color=#ff3a87>Unzip:</color> {ex}");

                if (unzipCallback != null) unzipCallback.OnFinished(false);

                return false;
            }
        }

        /// <summary>
        /// 【異步解壓縮】壓縮包二進制
        /// </summary>
        /// <param name="inputBytes"></param>
        /// <param name="outputPath"></param>
        /// <param name="password"></param>
        /// <param name="unzipCallback"></param>
        /// <param name="bufferSize"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        public static async UniTask<bool> UnzipAsync(byte[] inputBytes, string outputPath, string password = null, UnzipCallback unzipCallback = null, int bufferSize = 4096, Progression progression = null)
        {
            if (inputBytes == null || inputBytes.Length == 0 || string.IsNullOrEmpty(outputPath))
            {
                if (unzipCallback != null) unzipCallback.OnFinished(false);

                return false;
            }

            try
            {
                return await UnzipAsync(new MemoryStream(inputBytes), outputPath, password, unzipCallback, bufferSize, progression);
            }
            catch (Exception ex)
            {
                Debug.Log($"<color=#ff3a87>Unzip:</color> {ex}");

                if (unzipCallback != null) unzipCallback.OnFinished(false);

                return false;
            }
        }

        /// <summary>
        /// 【解壓縮】壓縮包二進制
        /// </summary>
        /// <param name="inputBytes"></param>
        /// <param name="outputPath"></param>
        /// <param name="password"></param>
        /// <param name="unzipCallback"></param>
        /// <param name="bufferSize"></param>
        /// <returns></returns>
        public static bool Unzip(byte[] inputBytes, string outputPath, string password = null, UnzipCallback unzipCallback = null, int bufferSize = 4096)
        {
            if (inputBytes == null || inputBytes.Length == 0 || string.IsNullOrEmpty(outputPath))
            {
                if (unzipCallback != null) unzipCallback.OnFinished(false);

                return false;
            }

            try
            {
                return Unzip(new MemoryStream(inputBytes), outputPath, password, unzipCallback, bufferSize);
            }
            catch (Exception ex)
            {
                Debug.Log($"<color=#ff3a87>Unzip:</color> {ex}");

                if (unzipCallback != null) unzipCallback.OnFinished(false);

                return false;
            }
        }

        /// <summary>
        /// 【異步解壓縮】壓縮包文件流
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="outputPath"></param>
        /// <param name="password"></param>
        /// <param name="unzipCallback"></param>
        /// <param name="bufferSize"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        public static async UniTask<bool> UnzipAsync(Stream inputStream, string outputPath, string password = null, UnzipCallback unzipCallback = null, int bufferSize = 4096, Progression progression = null)
        {
            if (inputStream == null || string.IsNullOrEmpty(outputPath))
            {
                if (unzipCallback != null) unzipCallback.OnFinished(false);

                return false;
            }

            // 判斷目錄不存在, 則進行創建
            if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

            // 開始解壓縮包
            ZipEntry zEntry;
            ZipInputStream zipInputStream = null;

            long totalSize, actualSize = 0;

            try
            {
                zipInputStream = new ZipInputStream(inputStream);
                totalSize = inputStream.Length; // 壓縮包大小 (不是實際大小, 除非壓縮僅用儲存)

                if (!string.IsNullOrEmpty(password)) zipInputStream.Password = password;

                while ((zEntry = zipInputStream.GetNextEntry()) != null)
                {
                    if (string.IsNullOrEmpty(zEntry.Name)) continue;

                    if ((unzipCallback != null) && !unzipCallback.OnPreUnzip(zEntry)) continue;

                    string pathName = Path.Combine(outputPath, zEntry.Name);

                    // 建立目錄
                    if (zEntry.IsDirectory)
                    {
                        if (!Directory.Exists(pathName)) Directory.CreateDirectory(pathName);
                        continue;
                    }

                    FileStream fileStream = null;
                    // 寫入檔案
                    try
                    {
                        fileStream = File.Create(pathName);
                        byte[] bytes = new byte[bufferSize];
                        while (true)
                        {
                            int readSize = zipInputStream.Read(bytes, 0, bytes.Length);

                            if (readSize > 0)
                            {
                                fileStream.Write(bytes, 0, readSize);

                                actualSize += readSize;

                                var progress = (float)actualSize / totalSize;
                                if (progression != null) progression.Invoke(progress, actualSize, totalSize);
                                //Debug.Log($"Unzip Progress: {progress.ToString("f2")}%, ActualSize: {BundleDistributor.GetBytesToString((ulong)actualSize)}, TotalSize: {BundleDistributor.GetBytesToString((ulong)totalSize)}");
                            }
                            else
                            {
                                if (fileStream != null) fileStream.Dispose();

                                if (unzipCallback != null) unzipCallback.OnPostUnzip(zEntry);

                                break;
                            }

                            await UniTask.Yield(_cts.Token);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Log($"<color=#ff3a87>Unzip:</color> {ex}");

                        if (fileStream != null) fileStream.Dispose();

                        if (zipInputStream != null) zipInputStream.Dispose();

                        if (unzipCallback != null) unzipCallback.OnFinished(false);

                        return false;
                    }

                    await UniTask.Yield(_cts.Token);
                }
            }
            catch
            {
                if (inputStream != null) inputStream.Dispose();

                if (zipInputStream != null) zipInputStream.Dispose();
            }

            if (inputStream != null) inputStream.Dispose();

            if (zipInputStream != null) zipInputStream.Dispose();

            // 完成
            if (unzipCallback != null) unzipCallback.OnFinished(true);

            return true;
        }

        /// <summary>
        /// 【解壓縮】壓縮包文件流
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="outputPath"></param>
        /// <param name="password"></param>
        /// <param name="unzipCallback"></param>
        /// <param name="bufferSize"></param>
        /// <returns></returns>
        public static bool Unzip(Stream inputStream, string outputPath, string password = null, UnzipCallback unzipCallback = null, int bufferSize = 4096)
        {
            if (inputStream == null || string.IsNullOrEmpty(outputPath))
            {
                if (unzipCallback != null) unzipCallback.OnFinished(false);

                return false;
            }

            // 判斷目錄不存在, 則進行創建
            if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

            // 開始解壓縮包
            ZipEntry zEntry;
            ZipInputStream zipInputStream = null;

            try
            {
                zipInputStream = new ZipInputStream(inputStream);

                if (!string.IsNullOrEmpty(password)) zipInputStream.Password = password;

                while ((zEntry = zipInputStream.GetNextEntry()) != null)
                {
                    if (string.IsNullOrEmpty(zEntry.Name)) continue;

                    if ((unzipCallback != null) && !unzipCallback.OnPreUnzip(zEntry)) continue;

                    string pathName = Path.Combine(outputPath, zEntry.Name);

                    // 建立目錄
                    if (zEntry.IsDirectory)
                    {
                        if (!Directory.Exists(pathName)) Directory.CreateDirectory(pathName);
                        continue;
                    }

                    FileStream fileStream = null;
                    // 寫入檔案
                    try
                    {
                        fileStream = File.Create(pathName);
                        byte[] bytes = new byte[bufferSize];
                        while (true)
                        {
                            int readSize = zipInputStream.Read(bytes, 0, bytes.Length);

                            if (readSize > 0) fileStream.Write(bytes, 0, readSize);
                            else
                            {
                                if (fileStream != null) fileStream.Dispose();

                                if (unzipCallback != null) unzipCallback.OnPostUnzip(zEntry);

                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Log($"<color=#ff3a87>Unzip:</color> {ex}");

                        if (fileStream != null) fileStream.Dispose();

                        if (zipInputStream != null) zipInputStream.Dispose();

                        if (unzipCallback != null) unzipCallback.OnFinished(false);

                        return false;
                    }
                }
            }
            catch
            {
                if (inputStream != null) inputStream.Dispose();

                if (zipInputStream != null) zipInputStream.Dispose();
            }

            if (inputStream != null) inputStream.Dispose();

            if (zipInputStream != null) zipInputStream.Dispose();

            // 完成
            if (unzipCallback != null) unzipCallback.OnFinished(true);

            return true;
        }
        #endregion

        #region Cancel
        /// <summary>
        /// 取消處理壓縮流程
        /// </summary>
        public static void CancelAsync()
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = new CancellationTokenSource();
        }
        #endregion
    }
}
