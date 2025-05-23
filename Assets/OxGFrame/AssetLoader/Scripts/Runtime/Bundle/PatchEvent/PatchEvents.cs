﻿using OxGFrame.AssetLoader.Bundle;
using UniFramework.Event;
using UniFramework.Machine;

namespace OxGFrame.AssetLoader.PatchEvent
{
    // 0. PatchRepairFailed
    // 1. PatchFsmState
    // 2. PatchGoToAppStore
    // 3. PatchAppVersionUpdateFailed
    // 4. PatchInitPatchModeFailed
    // 5. PatchVersionUpdateFailed
    // 6. PatchManifestUpdateFailed
    // 7. PatchCreateDownloader
    // 8. PatchCheckDiskNotEnoughSpace
    // 9. PatchDownloadProgression
    // 10. PatchDownloadFailed
    // 11. PatchDownloadCanceled

    public static class PatchEvents
    {
        /// <summary>
        /// Patch repair failed
        /// </summary>
        public class PatchRepairFailed : IEventMessage
        {
            public static void SendEventMessage()
            {
                var msg = new PatchRepairFailed();
                UniEvent.SendMessage(msg);
            }
        }

        /// <summary>
        /// Patch fsm state
        /// </summary>
        public class PatchFsmState : IEventMessage
        {
            public IStateNode stateNode;

            public static void SendEventMessage(IStateNode stateNode)
            {
                var msg = new PatchFsmState();
                msg.stateNode = stateNode;
                UniEvent.SendMessage(msg);
            }
        }

        /// <summary>
        /// App version inconsistent
        /// </summary>
        public class PatchGoToAppStore : IEventMessage
        {
            public static void SendEventMessage()
            {
                var msg = new PatchGoToAppStore();
                UniEvent.SendMessage(msg);
            }
        }

        /// <summary>
        /// App version update failed
        /// </summary>
        public class PatchAppVersionUpdateFailed : IEventMessage
        {
            public static void SendEventMessage()
            {
                var msg = new PatchAppVersionUpdateFailed();
                UniEvent.SendMessage(msg);
            }
        }

        /// <summary>
        /// Patch init default package failed
        /// </summary>
        public class PatchInitPatchModeFailed : IEventMessage
        {
            public static void SendEventMessage()
            {
                var msg = new PatchInitPatchModeFailed();
                UniEvent.SendMessage(msg);
            }
        }

        /// <summary>
        /// Patch version update failed
        /// </summary>
        public class PatchVersionUpdateFailed : IEventMessage
        {
            public static void SendEventMessage()
            {
                var msg = new PatchVersionUpdateFailed();
                UniEvent.SendMessage(msg);
            }
        }

        /// <summary>
        /// Patch manifest update failed
        /// </summary>
        public class PatchManifestUpdateFailed : IEventMessage
        {
            public static void SendEventMessage()
            {
                var msg = new PatchManifestUpdateFailed();
                UniEvent.SendMessage(msg);
            }
        }

        /// <summary>
        /// Patch create downloaders
        /// </summary>
        public class PatchCreateDownloader : IEventMessage
        {
            public GroupInfo[] groupInfos;

            public static void SendEventMessage(GroupInfo[] groupInfos)
            {
                var msg = new PatchCreateDownloader();
                msg.groupInfos = groupInfos;
                UniEvent.SendMessage(msg);
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

            public static void SendEventMessage(int totalDownloadCount, int currentDownloadCount, long totalDownloadSizeBytes, long currentDownloadSizeBytes, long downloadSpeedBytes)
            {
                var msg = new PatchDownloadProgression();
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
        /// Download file error
        /// </summary>
        public class PatchDownloadFailed : IEventMessage
        {
            public string fileName;
            public string error;

            public static void SendEventMessage(string error)
            {
                var msg = new PatchDownloadFailed();
                msg.error = error;
                UniEvent.SendMessage(msg);
            }

            public static void SendEventMessage(string fileName, string error)
            {
                var msg = new PatchDownloadFailed();
                msg.fileName = fileName;
                msg.error = error;
                UniEvent.SendMessage(msg);
            }
        }

        /// <summary>
        /// Download canceled
        /// </summary>
        public class PatchDownloadCanceled : IEventMessage
        {
            public static void SendEventMessage()
            {
                var msg = new PatchDownloadCanceled();
                UniEvent.SendMessage(msg);
            }
        }
    }
}
