using Cysharp.Threading.Tasks;

namespace OxGFrame.CenterFrame.EventCenter
{
    public abstract class EventBase
    {
        public async virtual UniTaskVoid HandleEvent() { }

        public async virtual UniTask HandleEventAsync() { }

        protected abstract void Release();
    }
}