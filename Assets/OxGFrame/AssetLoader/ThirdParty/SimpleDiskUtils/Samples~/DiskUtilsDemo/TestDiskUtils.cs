using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;
using SimpleDiskUtils;

public class TestDiskUtils : MonoBehaviour {

    [SerializeField]
    TextMesh text;

	string obj = "A";


    void PrintDebug(string str)
    {
        if (text != null)
            text.text += str;
        Debug.Log(str);
    }

    void PrintDebugLn(string str = "")
    {
        PrintDebug(str + "\n");
    }

    void Update(){
		if (obj.Length >= 3000000)
			return;
		
		obj += obj;

		// Append until obj size is at least 3 MB
		if (obj.Length < 3000000)
			return;

		StartCoroutine(Tests());
	}

    // Update is called once per frame
    
    void PrintStorageStats () {
        PrintDebugLn("=========== AVAILABLE SPACE  : " + DiskUtils.CheckAvailableSpace() + " MB ===========");
        PrintDebugLn("=========== BUSY SPACE  : " + DiskUtils.CheckBusySpace() + " MB ===========");
        PrintDebugLn("=========== TOTAL SPACE : " + DiskUtils.CheckTotalSpace() + " MB ===========");
	}

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    void PrintStorageStats(string drive)
    {
		PrintDebugLn("=========== AVAILABLE SPACE  : " + DiskUtils.CheckAvailableSpace(drive) + " MB ===========");
        PrintDebugLn("=========== BUSY SPACE  : " + DiskUtils.CheckBusySpace(drive) + " MB ===========");
        PrintDebugLn("=========== TOTAL SPACE : " + DiskUtils.CheckTotalSpace(drive) + " MB ===========");
    }
    #endif

    IEnumerator Tests()
    {
		text.text = "";

        string dir = Application.persistentDataPath + "/TestDiskUtils/";
        string storePath = Application.persistentDataPath + "/TestDiskUtils/Test.txt";


#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        foreach (string drive in DiskUtils.GetDriveNames())
        {
            if (drive != "C:/")
            {
                dir = drive + "TestDiskUtils/";
                storePath = drive + "TestDiskUtils/Test.txt";
            }

            PrintDebugLn();
            PrintDebugLn(">>> NOW TESTING ON DRIVE " + drive + " <<<");
#endif

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (File.Exists(storePath))
                File.Delete(storePath);


PrintStorageStats(
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            drive
#endif
);

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            DiskUtils.SaveFile(obj, storePath);

            PrintDebugLn("===== FILE ADDED!!! (Test File is around 3-4 MB) =====");

PrintStorageStats(
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            drive
#endif
);

            if (File.Exists(storePath))
            {
                File.Delete(storePath);
                PrintDebugLn("===== FILE DELETED!!! =====");
            }
            else {
                PrintDebugLn("===== File not found: most likely also failed on create =====");
            }

PrintStorageStats(
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            drive
#endif
);

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        }
#endif

        yield return null;
	}

}
