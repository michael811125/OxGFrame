using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OxGFrame.AssetLoader
{
    public interface IKeyCache<T>
    {
        bool HasInCache(int id, string name);

        void AddIntoCache(int id, string name);

        void DelFromCache(int id, string name);

        UniTask PreloadInCache(int id, string name, Progression progression);

        UniTask PreloadInCache(int id, string[] names, Progression progression);

        void ReleaseFromCache(int id, string name);

        void ReleaseCache(int id);
    }
}
