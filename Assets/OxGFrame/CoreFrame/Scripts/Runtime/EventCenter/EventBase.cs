using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreFrame
{
    namespace EventCenter
    {
        public abstract class EventBase
        {
            private int _funcId = 0;
            public int GetFuncId() { return this._funcId; }

            public EventBase(int funcId)
            {
                this._funcId = funcId;
            }

            public abstract UniTaskVoid HandleEvent();

            protected abstract void Release();
        }
    }
}