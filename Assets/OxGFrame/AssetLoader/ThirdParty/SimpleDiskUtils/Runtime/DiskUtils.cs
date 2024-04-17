/*

Class: DiskUtils.cs
==============================================
Last update: 2022-08-24  (by keerthiko)
==============================================

Copyright (c) 2016  M Dikra Prasetya

 * MIT LICENSE
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
 * CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

*/

using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

namespace SimpleDiskUtils
{

	public class DiskUtils
	{
        #region DISK_TOOLS

#if UNITY_STANDALONE || UNITY_EDITOR

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
		[DllImport ("diskutils")]
		private static extern int getAvailableDiskSpace ();

		[DllImport ("diskutils")]
		private static extern int getTotalDiskSpace ();

		[DllImport ("diskutils")]
		private static extern int getBusyDiskSpace ();

		/// <summary>
		/// Checks the available space.
		/// </summary>
		/// <returns>The available space in MB.</returns>
		public static int CheckAvailableSpace ()
		{
			return DiskUtils.getAvailableDiskSpace ();
		}

		/// <summary>
		/// Checks the total space.
		/// </summary>
		/// <returns>The total space in MB.</returns>
		public static int CheckTotalSpace ()
		{
			return DiskUtils.getTotalDiskSpace ();
		}

		/// <summary>
		/// Checks the busy space.
		/// </summary>
		/// <returns>The busy space in MB.</returns>
		public static int CheckBusySpace ()
		{
			return DiskUtils.getBusyDiskSpace ();
		}


#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        [DllImport("DiskUtilsWinAPI")]
        private static extern int getAvailableDiskSpace(StringBuilder drive);

        [DllImport("DiskUtilsWinAPI")]
        private static extern int getTotalDiskSpace(StringBuilder drive);

        [DllImport("DiskUtilsWinAPI")]
        private static extern int getBusyDiskSpace(StringBuilder drive);

        private const string DEFAULT_DRIVE = "C:/";

        /// <summary>
	    /// Checks the available space.
	    /// </summary>
	    /// <returns>The available spaces in MB.</returns>
	    /// <param name="diskName">Disk name. For example, "C:/"</param>
		public static int CheckAvailableSpace(string drive = DEFAULT_DRIVE)
        {
            return DiskUtils.getAvailableDiskSpace(new StringBuilder(drive));
        }

        /// <summary>
	    /// Checks the total space.
	    /// </summary>
	    /// <returns>The total space in MB.</returns>
	    /// <param name="diskName">Disk name. For example, "C:/"</param>
        public static int CheckTotalSpace(string drive = DEFAULT_DRIVE)
        {
            return DiskUtils.getTotalDiskSpace(new StringBuilder(drive));
        }

        /// <summary>
	    /// Checks the busy space.
	    /// </summary>
	    /// <returns>The busy space in MB.</returns>
	    /// <param name="diskName">Disk name. For example, "C:/"</param>
        public static int CheckBusySpace(string drive = DEFAULT_DRIVE)
        {
            return DiskUtils.getBusyDiskSpace(new StringBuilder(drive));
        }

        public static string[] GetDriveNames()
        {
            return Directory.GetLogicalDrives();
        }

#else

        private const long MEGA_BYTE = 1048576;
	    private const string DEFAULT_DRIVE = "/";
        
	    /// <summary>
	    /// Checks the available space.
	    /// </summary>
	    /// <returns>The available space in MB.</returns>
	    public static int CheckAvailableSpace(){
		    DriveInfo drive = GetDrive (DEFAULT_DRIVE);
		    if (drive == null)
			    return -1;
		    return int.Parse((drive.AvailableFreeSpace / MEGA_BYTE).ToString());
	    }

	    /// <summary>
	    /// Checks the total space.
	    /// </summary>
	    /// <returns>The total space in MB.</returns>
	    public static int CheckTotalSpace(){
		    DriveInfo drive = GetDrive (DEFAULT_DRIVE);
		    if (drive == null)
			    return -1;
		    return int.Parse ((drive.TotalSize / MEGA_BYTE).ToString());
	    }

	    /// <summary>
	    /// Checks the busy space.
	    /// </summary>
	    /// <returns>The busy space in MB.</returns>
	    public static int CheckBusySpace(){
		    DriveInfo drive = GetDrive (DEFAULT_DRIVE);
		    if (drive == null)
			    return -1;

		    return int.Parse (((drive.TotalSize - drive.AvailableFreeSpace) / MEGA_BYTE).ToString());
	    }

#endif

#elif UNITY_ANDROID
	private const string package_domain = "com.activetheoryinc.diskutils";
	// private const string package_domain = "com.dikra.diskutils";
	/// <summary>
	/// Checks the available space.
	/// </summary>
	/// <returns>The available space in MB.</returns>
	/// <param name="path">Finds the space remaining in the disk which this path leads to. Default is internal storage</param>
	public static int CheckAvailableSpace(string path){
		AndroidJavaClass dataUtils = new AndroidJavaClass ($"{package_domain}.DiskUtils");
		return dataUtils.CallStatic<int>("availableSpace", path);
	}


	/// <summary>
	/// Checks the available space.
	/// </summary>
	/// <returns>The available space in MB.</returns>
	/// <param name="isExternalStorage">If set to <c>true</c> is external storage.</param>
	public static int CheckAvailableSpace(bool isExternalStorage = true){
		AndroidJavaClass dataUtils = new AndroidJavaClass ($"{package_domain}.DiskUtils");
		return dataUtils.CallStatic<int>("availableSpace", isExternalStorage);
	}

	/// <summary>
	/// Checks the total space.
	/// </summary>
	/// <returns>The total space in MB.</returns>
	/// <param name="isExternalStorage">If set to <c>true</c> is external storage.</param>
	public static int CheckTotalSpace(bool isExternalStorage = true){
	AndroidJavaClass dataUtils = new AndroidJavaClass ($"{package_domain}.DiskUtils");
	return dataUtils.CallStatic<int>("totalSpace", isExternalStorage);
	}

	/// <summary>
	/// Checks the busy space.
	/// </summary>
	/// <returns>The busy space in MB.</returns>
	/// <param name="isExternalStorage">If set to <c>true</c> is external storage.</param>
	public static int CheckBusySpace(bool isExternalStorage = true){
	AndroidJavaClass dataUtils = new AndroidJavaClass ($"{package_domain}.DiskUtils");
	return dataUtils.CallStatic<int>("busySpace", isExternalStorage);
	}

	
#elif UNITY_IOS
	
	[DllImport ("__Internal")]
	private static extern ulong getAvailableDiskSpace();
	[DllImport ("__Internal")]
	private static extern ulong getTotalDiskSpace();
	[DllImport ("__Internal")]
	private static extern ulong getBusyDiskSpace();

	/// <summary>
	/// Checks the available space.
	/// </summary>
	/// <returns>The available space in MB.</returns>
	public static int CheckAvailableSpace(){
	ulong ret = DiskUtils.getAvailableDiskSpace();
	return int.Parse(ret.ToString());
	}

	/// <summary>
	/// Checks the total space.
	/// </summary>
	/// <returns>The total space in MB.</returns>
	public static int CheckTotalSpace(){
	ulong ret = DiskUtils.getTotalDiskSpace();
	return int.Parse(ret.ToString());
	}

	/// <summary>
	/// Checks the busy space.
	/// </summary>
	/// <returns>The busy space in MB.</returns>
	public static int CheckBusySpace(){
	ulong ret = DiskUtils.getBusyDiskSpace();
	return int.Parse(ret.ToString());
	}
#endif

        #endregion

        #region FILE_TOOLS

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <param name="filePath">File path.</param>
        public static void DeleteFile (string filePath)
		{
			#if UNITY_IOS
	if (!filePath.StartsWith("/private"))
	filePath = "/private" + filePath;
			#endif

			if (File.Exists (filePath))
				File.Delete (filePath);
		}

		/// <summary>
		/// Saves object to file.
		/// </summary>
		/// <param name="obj">Object.</param>
		/// <param name="filePath">File path.</param>
		public static void SaveFile (object obj, string filePath)
		{
			if (!obj.GetType ().IsSerializable) {
				throw new ArgumentException ("Passed data is invalid: not serializable.", "obj");
			}

			int i = filePath.Length;
			while (i > 0 && filePath [i - 1] != '/')
				--i;

			if (i <= 0)
				SaveFile (obj, "", filePath);
			else
				SaveFile (obj, filePath.Substring (0, i), filePath.Substring (i));
		}

		/// <summary>
		/// Saves object to file.
		/// </summary>
		/// <param name="obj">Serializable Object.</param>
		/// <param name="dirPath">Directory path.</param>
		/// <param name="fileName">File name.</param>
		public static void SaveFile (object obj, string dirPath, string fileName)
		{
			if (!obj.GetType ().IsSerializable) {
				throw new ArgumentException ("Passed data is invalid: not serializable.", "obj");
			}

			string filePath;
		
			if (dirPath == "") {
				filePath = fileName;
			} else {
				if (dirPath.EndsWith ("/"))
					filePath = dirPath + fileName;
				else
					filePath = dirPath + "/" + fileName;

				if (!Directory.Exists (dirPath))
					Directory.CreateDirectory (dirPath);
			}

			File.WriteAllBytes (filePath, ObjectToByteArray (obj));
		}

		/// <summary>
		/// Loads the file.
		/// </summary>
		/// <returns>The file.</returns>
		/// <param name="filePath">File path.</param>
		/// <typeparam name="T">Return type of the loaded object.</typeparam>
		public static T LoadFile<T> (string filePath)
		{
			if (File.Exists (filePath)) {
				return ByteArrayToObject<T> (File.ReadAllBytes (filePath));
			} else {
				return default(T);
			}
		}

		/// <summary>
		/// Saves a string to text file.
		/// </summary>
		/// <param name="str">String.</param>
		/// <param name="filePath">File path.</param>
		public static void SaveTextFile (string str, string filePath)
		{
			int i = filePath.Length;
			while (i > 0 && filePath [i - 1] != '/')
				--i;

			if (i <= 0)
				SaveTextFile (str, "", filePath);
			else
				SaveTextFile (str, filePath.Substring (0, i), filePath.Substring (i));
		}

		/// <summary>
		/// Saves a string to text file.
		/// </summary>
		/// <param name="str">String.</param>
		/// <param name="dirPath">Directory path.</param>
		/// <param name="fileName">File name.</param>
		public static void SaveTextFile (string str, string dirPath, string fileName)
		{
			string filePath;

			if (dirPath == "") {
				filePath = fileName;
			} else {
				if (dirPath.EndsWith ("/"))
					filePath = dirPath + fileName;
				else
					filePath = dirPath + "/" + fileName;

				if (!Directory.Exists (dirPath))
					Directory.CreateDirectory (dirPath);
			}
		

			StreamWriter sw = new StreamWriter (filePath);
			sw.WriteLine (str);
			sw.Close ();
		}

		/// <summary>
		/// Loads the file.
		/// </summary>
		/// <returns>The file.</returns>
		/// <param name="filePath">File path.</param>
		/// <typeparam name="T">Return type of the loaded object.</typeparam>
		public static string LoadTextFile<T> (string filePath)
		{
			if (File.Exists (filePath)) {
				StreamReader sr = new StreamReader (filePath);
				string str = sr.ReadToEnd ();
				sr.Close ();
				return str;
			} else {
				return null;
			}
		}


		public static byte[] ObjectToByteArray (object obj)
		{
			if (obj == null)
				return null;

			if (obj is byte[])
				return (byte[])obj;

			BinaryFormatter bf = new BinaryFormatter ();
			using (MemoryStream ms = new MemoryStream ()) {
				bf.Serialize (ms, obj);
				byte[] bytes = ms.ToArray ();
				ms.Close ();
				return bytes;
			}
		}

		public static T ByteArrayToObject<T> (byte[] bytes)
		{
			using (MemoryStream memStream = new MemoryStream ()) {
				BinaryFormatter bf = new BinaryFormatter ();
				memStream.Write (bytes, 0, bytes.Length);
				memStream.Seek (0, SeekOrigin.Begin);
				T obj = (T)bf.Deserialize (memStream);
				memStream.Close ();
				return obj;
			}
		}

		#endregion
	}

}