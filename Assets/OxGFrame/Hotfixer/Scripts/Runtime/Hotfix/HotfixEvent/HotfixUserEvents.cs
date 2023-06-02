using UniFramework.Event;

namespace OxGFrame.Hotfixer.HotfixEvent
{
    // 0. UserTryInitHotfix
    // 1. UserTryUpdateHotfix
    // 2. UserTryCreateDownloader

    public class HotfixUserEvents
    {
        /// <summary>
        /// User retry init hotfix package again
        /// </summary>
        public class UserTryInitHotfix : IEventMessage
        {
            public static void SendEventMessage()
            {
                var msg = new UserTryInitHotfix();
                UniEvent.SendMessage(msg);
            }
        }

        /// <summary>
        /// User retry update hotfix package again
        /// </summary>
        public class UserTryUpdateHotfix : IEventMessage
        {
            public static void SendEventMessage()
            {
                var msg = new UserTryUpdateHotfix();
                UniEvent.SendMessage(msg);
            }
        }

        /// <summary>
        /// User retry download hotfix files again
        /// </summary>
        public class UserTryCreateDownloader : IEventMessage
        {
            public static void SendEventMessage()
            {
                var msg = new UserTryCreateDownloader();
                UniEvent.SendMessage(msg);
            }
        }
    }
}
