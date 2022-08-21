using UnityEngine;

namespace OxGFrame.AssetLoader.Cacher
{
    public class AssetObject
    {
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

    public class ResourcePack : AssetObject
    {
        public string assetName = "";
        public Object asset;

        public T GetAsset<T>() where T : Object
        {
            return (T)this.asset;
        }

        ~ResourcePack()
        {
            this.assetName = null;
            this.asset = null;
        }
    }

    public class BundlePack : AssetObject
    {
        public string bundleName = "";
        public AssetBundle assetBundle;

        public T GetAsset<T>(string assetName) where T : Object
        {
            return this.assetBundle.LoadAsset<T>(assetName);
        }

        ~BundlePack()
        {
            this.bundleName = null;
            this.assetBundle = null;
        }
    }
}