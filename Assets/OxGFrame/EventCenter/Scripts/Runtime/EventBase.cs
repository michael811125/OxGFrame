using Cysharp.Threading.Tasks;

namespace OxGFrame.EventCenter
{
    public abstract class EventBase
    {
        //private int _funcId = 0;
        //public int GetFuncId() { return this._funcId; }

        //public EventBase(int funcId)
        //{
        //    this._funcId = funcId;
        //}

        public abstract UniTaskVoid HandleEvent();

        protected abstract void Release();
    }
}