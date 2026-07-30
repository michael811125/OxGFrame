using OxGKit.LoggingSystem;
using System.Collections.Generic;

namespace OxGFrame.CenterFrame
{
    /// <summary>
    /// Generic singleton base class for centers that register and look up class instances by id
    /// <para> Inherit to implement a center (e.g. APICenter, EventCenter) and use the static default APIs to access it </para>
    /// </summary>
    /// <typeparam name="TCenter"></typeparam>
    /// <typeparam name="TClass"></typeparam>
    public abstract class CenterBase<TCenter, TClass> where TCenter : CenterBase<TCenter, TClass>, new()
    {
        private Dictionary<int, TClass> _cache = new Dictionary<int, TClass>();

        private static readonly object _locker = new object();
        private static TCenter _instance = null;
        /// <summary>
        /// Get the singleton instance of the center (thread-safe lazy initialization)
        /// </summary>
        /// <returns></returns>
        protected static TCenter GetInstance()
        {
            if (_instance == null)
            {
                lock (_locker)
                {
                    if (_instance == null)
                        _instance = new TCenter();
                }
            }
            return _instance;
        }

        #region Default API
        /// <summary>
        /// Create and register an instance of UClass, using the type's hash code as its id
        /// </summary>
        /// <typeparam name="UClass"></typeparam>
        public static void Add<UClass>() where UClass : TClass, new()
        {
            GetInstance().Register<UClass>();
        }

        /// <summary>
        /// Create and register an instance of UClass with the specified id
        /// </summary>
        /// <typeparam name="UClass"></typeparam>
        /// <param name="id"></param>
        public static void Add<UClass>(int id) where UClass : TClass, new()
        {
            GetInstance().Register<UClass>(id);
        }

        /// <summary>
        /// Register an existing instance with the specified id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="class"></param>
        public static void Add(int id, TClass @class)
        {
            GetInstance().Register(id, @class);
        }

        /// <summary>
        /// Remove the registered instance of UClass, using the type's hash code as its id
        /// </summary>
        /// <typeparam name="UClass"></typeparam>
        public static void Delete<UClass>() where UClass : TClass
        {
            GetInstance().Remove<UClass>();
        }

        /// <summary>
        /// Remove the registered instance with the specified id
        /// </summary>
        /// <param name="id"></param>
        public static void Delete(int id)
        {
            GetInstance().Remove(id);
        }

        /// <summary>
        /// Remove all registered instances from the center
        /// </summary>
        public static void DeleteAll()
        {
            GetInstance().RemoveAll();
        }

        /// <summary>
        /// Get the registered instance of UClass, using the type's hash code as its id
        /// </summary>
        /// <typeparam name="UClass"></typeparam>
        /// <returns></returns>
        public static UClass Find<UClass>() where UClass : TClass
        {
            return GetInstance().Get<UClass>();
        }

        /// <summary>
        /// Get the registered instance of UClass with the specified id
        /// </summary>
        /// <typeparam name="UClass"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public static UClass Find<UClass>(int id) where UClass : TClass
        {
            return GetInstance().Get<UClass>(id);
        }
        #endregion

        /// <summary>
        /// Get the cached instance of UClass, using the type's hash code as its id
        /// </summary>
        /// <typeparam name="UClass"></typeparam>
        /// <returns></returns>
        public UClass Get<UClass>() where UClass : TClass
        {
            System.Type type = typeof(UClass);
            int hashCode = type.GetHashCode();

            return this.Get<UClass>(hashCode);
        }

        /// <summary>
        /// Get the cached instance of UClass with the specified id
        /// </summary>
        /// <typeparam name="UClass"></typeparam>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public UClass Get<UClass>(int eventId) where UClass : TClass
        {
            return (UClass)this.GetFromCache(eventId);
        }

        /// <summary>
        /// Check whether an instance of UClass is cached, using the type's hash code as its id
        /// </summary>
        /// <typeparam name="UClass"></typeparam>
        /// <returns></returns>
        public bool Has<UClass>() where UClass : TClass
        {
            System.Type type = typeof(UClass);
            int hashCode = type.GetHashCode();

            return this.Has<UClass>(hashCode);
        }

        /// <summary>
        /// Check whether an instance is cached with the specified id
        /// </summary>
        /// <typeparam name="UClass"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Has<UClass>(int id) where UClass : TClass
        {
            return this.HasInCache(id);
        }

        /// <summary>
        /// Create and cache an instance of UClass, using the type's hash code as its id
        /// </summary>
        /// <typeparam name="UClass"></typeparam>
        public void Register<UClass>() where UClass : TClass, new()
        {
            System.Type type = typeof(UClass);
            int hashCode = type.GetHashCode();

            UClass @new = new UClass();

            this.Register(hashCode, @new);
        }

        /// <summary>
        /// Create and cache an instance of UClass with the specified id
        /// </summary>
        /// <typeparam name="UClass"></typeparam>
        /// <param name="id"></param>
        public void Register<UClass>(int id) where UClass : TClass, new()
        {
            UClass @new = new UClass();

            this.Register(id, @new);
        }

        /// <summary>
        /// Cache an existing instance with the specified id
        /// <para> Logs a warning and skips registration if the id is already registered </para>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="class"></param>
        public void Register(int id, TClass @class)
        {
            if (this.HasInCache(id))
            {
                Logging.PrintWarning<Logger>($"Repeat registration. Id: {id}, Reg: {@class.GetType().Name}");
                return;
            }

            this._cache.Add(id, @class);
        }

        /// <summary>
        /// Remove the cached instance of UClass, using the type's hash code as its id
        /// </summary>
        /// <typeparam name="UClass"></typeparam>
        public void Remove<UClass>() where UClass : TClass
        {
            System.Type type = typeof(UClass);
            int hashCode = type.GetHashCode();

            this.Remove(hashCode);
        }

        /// <summary>
        /// Remove the cached instance with the specified id
        /// </summary>
        /// <param name="id"></param>
        public void Remove(int id)
        {
            if (this.HasInCache(id))
            {
                this._cache.Remove(id);
            }
        }

        /// <summary>
        /// Remove all cached instances
        /// </summary>
        public void RemoveAll()
        {
            if (this._cache.Count > 0) this._cache.Clear();
        }

        /// <summary>
        /// Get an instance from the cache by id
        /// <para> Logs an error and returns default if the id is not found </para>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected TClass GetFromCache(int id)
        {
            if (!this.HasInCache(id))
            {
                Logging.PrintError<Logger>($"Cannot find event with Id: {id}");
                return default;
            }

            return this._cache[id];
        }

        /// <summary>
        /// Check whether the cache contains an instance with the specified id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected bool HasInCache(int id)
        {
            return this._cache.ContainsKey(id);
        }
    }
}