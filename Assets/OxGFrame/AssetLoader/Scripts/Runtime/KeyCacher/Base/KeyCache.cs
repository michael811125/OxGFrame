using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace OxGFrame.AssetLoader.KeyCacher
{
    public class KeyGroup
    {
        public int id;
        public string name;
        public int refCount { get; protected set; }

        public void AddRef()
        {
            this.refCount++;
        }

        public void DelRef()
        {
            this.refCount--;
        }
    }

    public abstract class KeyCache<T> : IKeyCache<T>
    {
        protected HashSet<KeyGroup> _keyCacher;

        public int Count { get { return this._keyCacher.Count; } }

        public KeyCache()
        {
            this._keyCacher = new HashSet<KeyGroup>();
        }

        public virtual bool HasInCache(int id, string name)
        {
            if (this._keyCacher.Count == 0) return false;

            foreach (var keyGroup in this._keyCacher.ToArray())
            {
                if (keyGroup.id == id && keyGroup.name == name) return true;
            }

            return false;
        }

        public virtual KeyGroup GetFromCache(int id, string name)
        {
            if (this._keyCacher.Count == 0) return null;

            foreach (var keyGroup in this._keyCacher.ToArray())
            {
                if (keyGroup.id == id && keyGroup.name == name) return keyGroup;
            }

            return null;
        }

        public virtual void AddIntoCache(int id, string name)
        {
            if (!this.HasInCache(id, name))
            {
                KeyGroup keyGroup = new KeyGroup();
                keyGroup.id = id;
                keyGroup.name = name;
                this._keyCacher.Add(keyGroup);
            }
        }

        public virtual void DelFromCache(int id, string name)
        {
            if (this.HasInCache(id, name))
            {
                var keyGroup = this.GetFromCache(id, name);
                if (keyGroup != null) this._keyCacher.Remove(keyGroup);
            }
        }

        /// <summary>
        /// 資源預加載
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        public abstract UniTask Preload(int id, string name, Progression progression);

        /// <summary>
        /// 資源預加載
        /// </summary>
        /// <param name="id"></param>
        /// <param name="names"></param>
        /// <param name="progression"></param>
        /// <returns></returns>
        public abstract UniTask Preload(int id, string[] names, Progression progression);

        /// <summary>
        /// 【釋放】索引 Key 快取, 並且釋放資源快取
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        public abstract void Unload(int id, string name);

        /// <summary>
        /// 【釋放】全部索引 Key 快取, 並且釋放資源快取 (依照計數次數釋放)
        /// </summary>
        /// <param name="id"></param>
        public abstract void Release(int id);
    }
}