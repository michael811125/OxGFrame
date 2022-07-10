using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NetFrame
{
    namespace APICenter
    {
        public abstract class APIBase
        {
            private int _fundId = 0;
            public int GetFuncId() { return this._fundId; }

            public APIBase(int funcId)
            {
                this._fundId = funcId;
            }
        }
    }
}