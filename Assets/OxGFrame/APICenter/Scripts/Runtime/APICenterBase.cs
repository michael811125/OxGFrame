using System.Collections.Generic;
using UnityEngine;

namespace OxGFrame.APICenter
{
    public class APICenterBase<T> where T : APICenterBase<T>, new()
    {
        private Dictionary<int, APIBase> _dictAPIs = new Dictionary<int, APIBase>();

        private static readonly object _locker = new object();
        private static T _instance = null;
        public static T GetInstance()
        {
            if (_instance == null)
            {
                lock (_locker)
                {
                    _instance = new T();
                }
            }
            return _instance;
        }

        /// <summary>
        /// 取得 API
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <returns></returns>
        public U GetAPI<U>() where U : APIBase
        {
            System.Type apiType = typeof(U);
            int hashCode = apiType.GetHashCode();

            return this.GetAPI<U>(hashCode);
        }

        /// <summary>
        /// 取得 API
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="apiId"></param>
        /// <returns></returns>
        public U GetAPI<U>(int apiId) where U : APIBase
        {
            return (U)this._GetFromCache(apiId);
        }

        /// <summary>
        /// 檢查 API
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <returns></returns>
        public bool HasAPI<U>() where U : APIBase
        {
            System.Type apiType = typeof(U);
            int hashCode = apiType.GetHashCode();

            return this.HasAPI<U>(hashCode);
        }

        /// <summary>
        /// 檢查 API
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="apiId"></param>
        /// <returns></returns>
        public bool HasAPI<U>(int apiId) where U : APIBase
        {
            return this._HasInCache(apiId);
        }

        /// <summary>
        /// 註冊 API
        /// </summary>
        /// <typeparam name="U"></typeparam>
        protected void Register<U>() where U : APIBase, new()
        {
            System.Type apiType = typeof(U);
            int hashCode = apiType.GetHashCode();

            U apiBase = new U();

            this.Register(hashCode, apiBase);
        }

        /// <summary>
        /// 註冊 API
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="apiId"></param>
        protected void Register<U>(int apiId) where U : APIBase, new()
        {
            U apiBase = new U();

            this.Register(apiId, apiBase);
        }

        /// <summary>
        /// 註冊 API
        /// </summary>
        /// <param name="apiId"></param>
        /// <param name="apiBase"></param>
        protected void Register(int apiId, APIBase apiBase)
        {
            if (this._HasInCache(apiId))
            {
                Debug.Log(string.Format("<color=#FF0000>Repeat registration. API Id: {0}, API: {1}</color>", apiId, apiBase.GetType().Name));
                return;
            }

            this._dictAPIs.Add(apiId, apiBase);
        }

        private APIBase _GetFromCache(int apiId)
        {
            if (!this._HasInCache(apiId))
            {
                Debug.Log(string.Format("<color=#FF0000>Cannot found API. API Id: {0}</color>", apiId));
                return null;
            }

            return this._dictAPIs[apiId];
        }

        private bool _HasInCache(int apiId)
        {
            return this._dictAPIs.ContainsKey(apiId);
        }
    }
}