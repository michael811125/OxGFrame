using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OxGFrame.APICenter
{
    public class APICenterBase<T> where T : APICenterBase<T>, new()
    {
        public const int API_xBASE = 0x0000;

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

        public T GetAPI<T>(int funcId) where T : APIBase
        {
            return (T)this._GetFromCache(funcId);
        }

        protected void Register(APIBase apiBase)
        {
            if (this._HasInCache(apiBase.GetFuncId()))
            {
                Debug.Log(string.Format("<color=#FF0000>Repeat registration. API funcId: {0}</color>", apiBase.GetFuncId()));
                return;
            }

            this._dictAPIs.Add(apiBase.GetFuncId(), apiBase);
        }

        private APIBase _GetFromCache(int funcId)
        {
            if (!this._HasInCache(funcId))
            {
                Debug.Log(string.Format("<color=#FF0000>Cannot found API. API Id: {0}</color>", funcId));
                return null;
            }

            return this._dictAPIs[funcId];
        }

        private bool _HasInCache(int funcId)
        {
            return this._dictAPIs.ContainsKey(funcId);
        }
    }
}