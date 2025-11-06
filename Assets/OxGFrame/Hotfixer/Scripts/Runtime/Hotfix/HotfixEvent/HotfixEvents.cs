using UniFramework.Event;
using UniFramework.Machine;

namespace OxGFrame.Hotfixer.HotfixEvent
{
    // 0. HotfixFsmState
    // 1. HotfixInitFailed
    // 2. HotfixUpdateFailed
    // 3. HotfixCreateDownloader
    // 4. HotfixDownloadProgression
    // 5. HotfixDownloadFailed

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
        /// Hotfix create downloaders
        /// </summary>
        public class HotfixCreateDownloader : IEventMessage
        {
            public int totalCount;
            public long totalBytes;

            public static void SendEventMessage(int totalDownloadCount, long totalDownloadSizeBytes)
            {
                var msg = new HotfixCreateDownloader();
                msg.totalCount = totalDownloadCount;
                msg.totalBytes = totalDownloadSizeBytes;
                UniEvent.SendMessage(msg);
            }
        }

        /// <summary>
        /// Hotfix download progression
        /// </summary>
        public class HotfixDownloadProgression : IEventMessage
        {
            public float progress;
            public int totalDownloadCount;
            public int currentDownloadCount;
            public long totalDownloadSizeBytes;
            public long currentDownloadSizeBytes;
            public long downloadSpeedBytes;

            public static void SendEventMessage(int totalDownloadCount, int currentDownloadCount, long totalDownloadSizeBytes, long currentDownloadSizeBytes, long downloadSpeedBytes)
            {
                var msg = new HotfixDownloadProgression();
                msg.totalDownloadCount = totalDownloadCount;
                msg.currentDownloadCount = currentDownloadCount;
                msg.totalDownloadSizeBytes = totalDownloadSizeBytes;
                msg.currentDownloadSizeBytes = currentDownloadSizeBytes;
                msg.downloadSpeedBytes = downloadSpeedBytes;
                msg.progress = (msg.currentDownloadSizeBytes * 1f) / (msg.totalDownloadSizeBytes * 1f);
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
