This library is maintained by Active Theory Inc.

This is a fork of the `simple-disk-utils` asset by M Dikra Prasetya, Available on Unity Asset Store: http://u3d.as/qF1.

# simple-disk-utils
Disk/storage capacity check helper methods for Windows, OSX, iOS, and Android platform. 

Simply checks available, busy, and total storage space of your platform. File managing functions like save and delete to text or binary file with special cases handling are also provided.

The implementation for each platforms are also can be found in this repository.

If you have any idea on improvement or anything, please feel free to contribute!

## Implemented Methods
```

///////////////////////////////////////////////////////////////////////
/// For all platforms. These methods return space size in Mega Bytes.

int CheckAvailableSpace(); 
int CheckBusySpace();
int CheckTotalSpace();

///////////////////////////////////////////////////////////////////////
/// Additional space check methods for Android
/// The default storage for Android, the one that is usually used when saving file, etc., is External.

int CheckAvailableSpace(bool isExternalStorage = true); 
int CheckBusySpace(bool isExternalStorage = true);
int CheckTotalSpace(bool isExternalStorage = true);

///////////////////////////////////////////////////////////////////////
/// Additional methods for Windows

int CheckAvailableSpace(string drive = “C:/“); 
int CheckBusySpace(string drive = “C:/“);
int CheckTotalSpace(string drive = “C:/“);
string[] GetDriveNames();

///////////////////////////////////////////////////////////////////////
/// File helper. Handled directory availability check, iOS special case on deleting files, etc.

void DeleteFile (string filePath);
void SaveFile (object obj, string filePath);
void SaveFile (object obj, string dirPath, string fileName);
void SaveTextFile (string str, string filePath);
void SaveTextFile (string str, string dirPath, string fileName);
string LoadTextFile<T> (string filePath);
byte[] ObjectToByteArray (object obj);
T ByteArrayToObject<T> (byte[] bytes);
```

If the definitions are not clear enough, please see the included sample scene in the project.

## Notes
1. Tested on Windows, OSX, iOS, and Android platform.
2. Implemented file handling methods are not including methods that are already covered in standard library (most likely on System.IO).


## License

MIT.
