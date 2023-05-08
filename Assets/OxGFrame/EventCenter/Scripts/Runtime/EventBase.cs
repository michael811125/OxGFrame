using Cysharp.Threading.Tasks;

namespace OxGFrame.EventCenter
{
    public abstract class EventBase
    {
        public abstract UniTaskVoid HandleEvent();

        protected abstract void Release();
    }
}