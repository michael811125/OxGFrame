using Cysharp.Threading.Tasks;

namespace OxGFrame.CenterFrame.EventCenter
{
    /// <summary>
    /// Base class for events that can be registered into an EventCenter (CenterBase)
    /// </summary>
    public abstract class EventBase
    {
        /// <summary>
        /// Handle the event in fire-and-forget style
        /// <para> Override to implement the event logic and invoke it when the event is emitted </para>
        /// </summary>
        /// <returns></returns>
        public async virtual UniTaskVoid HandleEvent() { }

        /// <summary>
        /// Handle the event asynchronously so that it can be awaited
        /// <para> Override to implement the event logic and invoke it when the event is emitted </para>
        /// </summary>
        /// <returns></returns>
        public async virtual UniTask HandleEventAsync() { }

        /// <summary>
        /// Release the event's cached data and references
        /// <para> Implement the cleanup logic and call it after the event has been handled </para>
        /// </summary>
        protected abstract void Release();
    }
}