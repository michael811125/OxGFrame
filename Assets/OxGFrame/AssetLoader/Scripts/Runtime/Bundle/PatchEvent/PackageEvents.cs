using UniFramework.Event;
using UniFramework.Machine;

namespace OxGFrame.AssetLoader.PatchEvent
{
    // 0. PatchRepairFailed
    // 1. PatchFsmState
    // 2. PatchInitPatchModeFailed
    // 3. PatchVersionUpdateFailed
    // 4. PatchManifestUpdateFailed
    // 5. PatchCheckDiskNotEnoughSpace
    // 6. PatchDownloadProgression
    // 7. PatchDownloadFailed
    // 8. PatchDownloadCanceled

    public static class PackageEvents
    {
        /// <summary>
        /// Patch repair failed
        /// </summary>
        internal class PatchRepairFailed : IEventMessage
        {
            public static void SendEventMessage(int groupId)
            {
                var msg = new PatchRepairFailed();
                UniEvent.SendMessage(groupId, msg);
            }
        }

        /// <summary>
        /// Patch fsm state
        /// </summary>
        public class PatchFsmState : IEventMessage
        {
            public IStateNode stateNode;

            public static void SendEventMessage(int groupId, IStateNode stateNode)
            {
                var msg = new PatchFsmState();
                msg.stateNode = stateNode;
                UniEvent.SendMessage(groupId, msg);
            }
        }

        /// <summary>
        /// Patch init default package failed
        /// </summary>
        internal class PatchInitPatchModeFailed : IEventMessage
        {
            public static void SendEventMessage(int groupId)
            {
                var msg = new PatchInitPatchModeFailed();
                UniEvent.SendMessage(groupId, msg);
            }
        }

        /// <summary>
        /// Patch version update failed
        /// </summary>
        internal class PatchVersionUpdateFailed : IEventMessage
        {
            public static void SendEventMessage(int groupId)
            {
                var msg = new PatchVersionUpdateFailed();
                UniEvent.SendMessage(groupId, msg);
            }
        }

        /// <summary>
        /// Patch manifest update failed
        /// </summary>
        internal class PatchManifestUpdateFailed : IEventMessage
        {
            public static void SendEventMessage(int groupId)
            {
                var msg = new PatchManifestUpdateFailed();
                UniEvent.SendMessage(groupId, msg);
            }
        }

        /// <summary>
        /// Patch check disk if not enough space
        /// </summary>
        public class PatchCheckDiskNotEnoughSpace : IEventMessage
        {
            public int availableMegabytes;
            public ulong patchTotalBytes;

            public static void SendEventMessage(int availableMegabytes, ulong patchTotalBytes)
            {
                var msg = new PatchCheckDiskNotEnoughSpace();
                msg.availableMegabytes = availableMegabytes;
                msg.patchTotalBytes = patchTotalBytes;
                UniEvent.SendMessage(msg);
            }
        }

        /// <summary>
        /// Patch download progression
        /// </summary>
        public class PatchDownloadProgression : IEventMessage
        {
            public float progress;
            public int totalDownloadCount;
            public int currentDownloadCount;
            public long totalDownloadSizeBytes;
            public long currentDownloadSizeBytes;
            public long downloadSpeedBytes;

            public static void SendEventMessage(int groupId, int totalDownloadCount, int currentDownloadCount, long totalDownloadSizeBytes, long currentDownloadSizeBytes, long downloadSpeedBytes)
            {
                var msg = new PatchDownloadProgression();
                msg.totalDownloadCount = totalDownloadCount;
                msg.currentDownloadCount = currentDownloadCount;
                msg.totalDownloadSizeBytes = totalDownloadSizeBytes;
                msg.currentDownloadSizeBytes = currentDownloadSizeBytes;
                msg.downloadSpeedBytes = downloadSpeedBytes;
                msg.progress = (msg.currentDownloadSizeBytes * 1f) / (msg.totalDownloadSizeBytes * 1f);
                UniEvent.SendMessage(groupId, msg);
            }
        }

        /// <summary>
        /// Download file error
        /// </summary>
        internal class PatchDownloadFailed : IEventMessage
        {
            public string fileName;
            public string error;

            public static void SendEventMessage(int groupId, string fileName, string error)
            {
                var msg = new PatchDownloadFailed();
                msg.fileName = fileName;
                msg.error = error;
                UniEvent.SendMessage(groupId, msg);
            }
        }

        /// <summary>
        /// Download canceled
        /// </summary>
        public class PatchDownloadCanceled : IEventMessage
        {
            public static void SendEventMessage(int groupId)
            {
                var msg = new PatchDownloadCanceled();
                UniEvent.SendMessage(groupId, msg);
            }
        }
    }
}
