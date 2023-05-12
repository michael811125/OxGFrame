using System.Collections.Generic;
using System.Linq;

namespace OxGFrame.AssetLoader.GroupCacher
{
    public class KeyGroup
    {
        public int id;
        public string assetName;
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

    public abstract class GroupCache<T> : IGroupCache<T>
    {
        protected HashSet<KeyGroup> _keyCacher;

        public int Count { get { return this._keyCacher.Count; } }

        public GroupCache()
        {
            this._keyCacher = new HashSet<KeyGroup>();
        }

        public virtual bool HasInCache(int id, string assetName)
        {
            if (this._keyCacher.Count == 0) return false;

            foreach (var keyGroup in this._keyCacher)
            {
                if (keyGroup.id == id && keyGroup.assetName == assetName) return true;
            }

            return false;
        }

        public virtual KeyGroup GetFromCache(int id, string assetName)
        {
            if (this._keyCacher.Count == 0) return null;

            foreach (var keyGroup in this._keyCacher)
            {
                if (keyGroup.id == id && keyGroup.assetName == assetName) return keyGroup;
            }

            return null;
        }

        public virtual void AddIntoCache(int id, string asestName)
        {
            if (!this.HasInCache(id, asestName))
            {
                KeyGroup keyGroup = new KeyGroup();
                keyGroup.id = id;
                keyGroup.assetName = asestName;
                this._keyCacher.Add(keyGroup);
            }
        }

        public virtual void DelFromCache(int id, string assetName)
        {
            if (this.HasInCache(id, assetName))
            {
                var keyGroup = this.GetFromCache(id, assetName);
                if (keyGroup != null) this._keyCacher.Remove(keyGroup);
            }
        }
    }
}