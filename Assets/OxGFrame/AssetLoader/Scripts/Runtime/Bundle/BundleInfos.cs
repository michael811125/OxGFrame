using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetLoader.Bundle
{
    public class ResFileInfo
    {
        public string fileName; // 檔案名稱
        public string dirName;  // 目錄名稱
        public long size;       // 檔案大小
        public string md5;      // 檔案MD5
    }

    public class VersionFileCfg
    {
        public string PRODUCT_NAME;                       // 產品名稱
        public string APP_VERSION;                        // 主程式版本
        public string RES_VERSION;                        // 資源檔版本
        public string EXPORT_NAME;                        // 輸出名稱 (依照時間作為資料夾名稱)
        public Dictionary<string, ResFileInfo> RES_FILES; // 此次版本的資源檔案

        public VersionFileCfg()
        {
            this.RES_FILES = new Dictionary<string, ResFileInfo>();
        }

        public void AddResFileInfo(string fileName, ResFileInfo rf)
        {
            if (this.RES_FILES.ContainsKey(fileName)) return;
            this.RES_FILES.Add(fileName, rf);
        }

        public ResFileInfo GetResFileInfo(string fileName)
        {
            this.RES_FILES.TryGetValue(fileName, out ResFileInfo resFileInfo);
            return resFileInfo;
        }

        public bool HasFile(string fileName)
        {
            return this.RES_FILES.ContainsKey(fileName);
        }
    }
}