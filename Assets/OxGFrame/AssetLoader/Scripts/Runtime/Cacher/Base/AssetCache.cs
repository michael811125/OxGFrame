using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OxGFrame.AssetLoader.Cacher
{
    public abstract class AssetCache<T> : ICache<T>
    {
        protected Dictionary<string, T> _cacher;

        public float reqSize { get; protected set; }

        public float totalSize { get; protected set; }

        public int Count { get { return this._cacher.Count; } }

        public abstract bool HasInCache(string name);

        public abstract T GetFromCache(string name);

        public abstract UniTask PreloadInCache(string name, Progression progression);

        public abstract UniTask PreloadInCache(string[] names, Progression progression);

        public abstract void ReleaseFromCache(string name);

        public abstract void ReleaseCache();

        public abstract UniTask<int> GetAssetsLength(params string[] names);
    }
}
