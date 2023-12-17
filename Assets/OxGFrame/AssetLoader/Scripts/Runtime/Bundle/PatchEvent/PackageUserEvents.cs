using UniFramework.Event;

namespace OxGFrame.AssetLoader.PatchEvent
{
    // 0. UserTryPatchRepair
    // 1. UserTryInitPatchMode
    // 2. UserBeginDownload
    // 3. UserTryPatchVersionUpdate
    // 4. UserTryPatchManifestUpdate
    // 5. UserTryCreateDownloader

    public static class PackageUserEvents
    {
        /// <summary>
        /// User retry patch repair again
        /// </summary>
        internal class UserTryPatchRepair : IEventMessage
        {
            public static void SendEventMessage(int groupId)
            {
                var msg = new UserTryPatchRepair();
                UniEvent.SendMessage(groupId, msg);
            }
        }

        /// <summary>
        /// User retry init patch mode again
        /// </summary>
        internal class UserTryInitPatchMode : IEventMessage
        {
            public static void SendEventMessage(int groupId)
            {
                var msg = new UserTryInitPatchMode();
                UniEvent.SendMessage(groupId, msg);
            }
        }

        /// <summary>
        /// User begin download
        /// </summary>
        public class UserBeginDownload : IEventMessage
        {
            public static void SendEventMessage(int groupId)
            {
                var msg = new UserBeginDownload();
                UniEvent.SendMessage(groupId, msg);
            }
        }

        /// <summary>
        /// User retry update patch version again
        /// </summary>
        internal class UserTryPatchVersionUpdate : IEventMessage
        {
            public static void SendEventMessage(int groupId)
            {
                var msg = new UserTryPatchVersionUpdate();
                UniEvent.SendMessage(groupId, msg);
            }
        }

        /// <summary>
        /// User retry update patch manifest again
        /// </summary>
        internal class UserTryPatchManifestUpdate : IEventMessage
        {
            public static void SendEventMessage(int groupId)
            {
                var msg = new UserTryPatchManifestUpdate();
                UniEvent.SendMessage(groupId, msg);
            }
        }

        /// <summary>
        /// User retry download again
        /// </summary>
        internal class UserTryCreateDownloader : IEventMessage
        {
            public static void SendEventMessage(int groupId)
            {
                var msg = new UserTryCreateDownloader();
                UniEvent.SendMessage(groupId, msg);
            }
        }
    }
}