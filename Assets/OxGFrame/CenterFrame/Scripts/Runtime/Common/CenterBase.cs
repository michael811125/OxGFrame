using OxGKit.LoggingSystem;
using System.Collections.Generic;

namespace OxGFrame.CenterFrame
{
    public abstract class CenterBase<TCenter, TClass> where TCenter : CenterBase<TCenter, TClass>, new()
    {
        private Dictionary<int, TClass> _cache = new Dictionary<int, TClass>();

        private static readonly object _locker = new object();
        private static TCenter _instance = null;
        protected static TCenter GetInstance()
        {
            if (_instance == null)
            {
                lock (_locker)
                {
                    _instance = new TCenter();
                }
            }
            return _instance;
        }

        #region Default API
        public static void Add<UClass>() where UClass : TClass, new()
        {
            GetInstance().Register<UClass>();
        }

        public static void Add<UClass>(int id) where UClass : TClass, new()
        {
            GetInstance().Register<UClass>(id);
        }

        public static void Add(int id, TClass @class)
        {
            GetInstance().Register(id, @class);
        }

        public static void Delete<UClass>() where UClass : TClass
        {
            GetInstance().Remove<UClass>();
        }

        public static void Delete(int id)
        {
            GetInstance().Remove(id);
        }

        public static void DeleteAll()
        {
            GetInstance().RemoveAll();
        }

        public static UClass Find<UClass>() where UClass : TClass
        {
            return GetInstance().Get<UClass>();
        }

        public static UClass Find<UClass>(int id) where UClass : TClass
        {
            return GetInstance().Get<UClass>(id);
        }
        #endregion

        public UClass Get<UClass>() where UClass : TClass
        {
            System.Type type = typeof(UClass);
            int hashCode = type.GetHashCode();

            return this.Get<UClass>(hashCode);
        }

        public UClass Get<UClass>(int eventId) where UClass : TClass
        {
            return (UClass)this.GetFromCache(eventId);
        }

        public bool Has<UClass>() where UClass : TClass
        {
            System.Type type = typeof(UClass);
            int hashCode = type.GetHashCode();

            return this.Has<UClass>(hashCode);
        }

        public bool Has<UClass>(int id) where UClass : TClass
        {
            return this.HasInCache(id);
        }

        public void Register<UClass>() where UClass : TClass, new()
        {
            System.Type type = typeof(UClass);
            int hashCode = type.GetHashCode();

            UClass @new = new UClass();

            this.Register(hashCode, @new);
        }

        public void Register<UClass>(int id) where UClass : TClass, new()
        {
            UClass @new = new UClass();

            this.Register(id, @new);
        }

        public void Register(int id, TClass @class)
        {
            if (this.HasInCache(id))
            {
                Logging.PrintWarning<Logger>($"<color=#FF0000>Repeat registration. Id: {id}, Reg: {@class.GetType().Name}</color>");
                return;
            }

            this._cache.Add(id, @class);
        }

        public void Remove<UClass>() where UClass : TClass
        {
            System.Type type = typeof(UClass);
            int hashCode = type.GetHashCode();

            this.Remove(hashCode);
        }

        public void Remove(int id)
        {
            if (this.HasInCache(id))
            {
                this._cache.Remove(id);
            }
        }

        public void RemoveAll()
        {
            if (this._cache.Count > 0) this._cache.Clear();
        }

        protected TClass GetFromCache(int id)
        {
            if (!this.HasInCache(id))
            {
                Logging.PrintError<Logger>($"<color=#ff952f>Cannot find event with Id: {id}</color>");
                return default;
            }

            return this._cache[id];
        }

        protected bool HasInCache(int id)
        {
            return this._cache.ContainsKey(id);
        }
    }
}