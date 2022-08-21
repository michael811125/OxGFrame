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

        UniTask Preload(int id, string name, Progression progression);

        UniTask Preload(int id, string[] names, Progression progression);

        void Unload(int id, string name);

        void Release(int id);
    }
}
