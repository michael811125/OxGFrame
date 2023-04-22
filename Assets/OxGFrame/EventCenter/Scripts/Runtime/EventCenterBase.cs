using System.Collections.Generic;
using UnityEngine;

namespace OxGFrame.EventCenter
{
    public class EventCenterBase<T> where T : EventCenterBase<T>, new()
    {
        private Dictionary<int, EventBase> _dictEvents = new Dictionary<int, EventBase>();

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
        /// 取得事件
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <returns></returns>
        public U GetEvent<U>() where U : EventBase
        {
            System.Type eventType = typeof(U);
            int hashCode = eventType.GetHashCode();

            return this.GetEvent<U>(hashCode);
        }

        /// <summary>
        /// 取得事件
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public U GetEvent<U>(int eventId) where U : EventBase
        {
            return (U)this._GetFromCache(eventId);
        }

        /// <summary>
        /// 檢查事件
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <returns></returns>
        public bool HasEvent<U>() where U : EventBase
        {
            System.Type eventType = typeof(U);
            int hashCode = eventType.GetHashCode();

            return this.HasEvent<U>(hashCode);
        }

        /// <summary>
        /// 檢查事件
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public bool HasEvent<U>(int eventId) where U : EventBase
        {
            return this._HasInCache(eventId);
        }

        /// <summary>
        /// 註冊事件
        /// </summary>
        /// <typeparam name="U"></typeparam>
        protected void Register<U>() where U : EventBase, new()
        {
            System.Type eventType = typeof(U);
            int hashCode = eventType.GetHashCode();

            U eventBase = new U();

            this.Register(hashCode, eventBase);
        }


        /// <summary>
        /// 註冊事件
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="eventId"></param>
        protected void Register<U>(int eventId) where U : EventBase, new()
        {
            U eventBase = new U();

            this.Register(eventId, eventBase);
        }

        /// <summary>
        /// 註冊事件
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="eventBase"></param>
        protected void Register(int eventId, EventBase eventBase)
        {
            if (this._HasInCache(eventId))
            {
                Debug.Log(string.Format("<color=#FF0000>Repeat registration. Event Id: {0}, Event: {1}</color>", eventId, eventBase.GetType().Name));
                return;
            }

            this._dictEvents.Add(eventId, eventBase);
        }

        private EventBase _GetFromCache(int eventId)
        {
            if (!this._HasInCache(eventId))
            {
                Debug.Log(string.Format("<color=#FF0000>Cannot found Event. Event Id: {0}</color>", eventId));
                return null;
            }

            return this._dictEvents[eventId];
        }

        private bool _HasInCache(int eventId)
        {
            return this._dictEvents.ContainsKey(eventId);
        }
    }
}