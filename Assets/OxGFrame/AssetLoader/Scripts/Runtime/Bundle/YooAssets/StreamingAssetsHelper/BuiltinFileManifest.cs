using MyBox;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OxGFrame.AssetLoader.Bundle
{
    // Reference: YooAsset

    /// <summary>
    /// 内置资源清单
    /// </summary>
    public class BuiltinFileManifest : ScriptableObject
    {
        [Serializable]
        public class Element
        {
            [ReadOnly]
            public string PackageName;
            [ReadOnly]
            public string FileName;
            [ReadOnly]
            public string FileCRC32;
        }

        public List<Element> BuiltinFiles = new List<Element>();
    }
}