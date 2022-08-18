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

        public abstract UniTask PreloadInCache(int id, string name, Progression progression);

        public abstract UniTask PreloadInCache(int id, string[] names, Progression progression);

        public abstract void ReleaseFromCache(int id, string name);

        public abstract void ReleaseCache(int id);
    }
}