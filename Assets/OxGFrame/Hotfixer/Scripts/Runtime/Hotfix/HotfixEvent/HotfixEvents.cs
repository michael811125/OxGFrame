using UniFramework.Event;
using UniFramework.Machine;

namespace OxGFrame.Hotfixer.HotfixEvent
{
    // 0. HotfixFsmState
    // 1. HotfixInitFailed
    // 2. HotfixUpdateFailed
    // 3. HotfixDownloadFailed

    public static class HotfixEvents
    {
        /// <summary>
        /// Hotfix fsm state
        /// </summary>
        public class HotfixFsmState : IEventMessage
        {
            public IStateNode stateNode;

            public static void SendEventMessage(IStateNode stateNode)
            {
                var msg = new HotfixFsmState();
                msg.stateNode = stateNode;
                UniEvent.SendMessage(msg);
            }
        }

        /// <summary>
        /// Hotfix package init failed
        /// </summary>
        public class HotfixInitFailed : IEventMessage
        {
            public static void SendEventMessage()
            {
                var msg = new HotfixInitFailed();
                UniEvent.SendMessage(msg);
            }
        }

        /// <summary>
        /// Hotifx package update failed
        /// </summary>
        public class HotfixUpdateFailed : IEventMessage
        {
            public static void SendEventMessage()
            {
                var msg = new HotfixUpdateFailed();
                UniEvent.SendMessage(msg);
            }
        }

        /// <summary>
        /// Hotfix download files failed
        /// </summary>
        public class HotfixDownloadFailed : IEventMessage
        {
            public string fileName;
            public string error;

            public static void SendEventMessage(string fileName, string error)
            {
                var msg = new HotfixDownloadFailed();
                msg.fileName = fileName;
                msg.error = error;
                UniEvent.SendMessage(msg);
            }
        }
    }
}
