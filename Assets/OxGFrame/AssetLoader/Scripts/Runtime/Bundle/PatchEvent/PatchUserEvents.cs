using OxGFrame.AssetLoader.Bundle;
using UniFramework.Event;

namespace OxGFrame.AssetLoader.PatchEvent
{
    // 0. UserTryPatchRepair
    // 1. UserTryAppVersionUpdate
    // 2. UserTryInitPatchMode
    // 3. UserTryPatchVersionUpdate
    // 4. UserTryPatchManifestUpdate
    // 5. UserTryCreateDownloader
    // 6. UserBeginDownload

    public class PatchUserEvents
    {
        /// <summary>
        /// User retry patch repair again
        /// </summary>
        public class UserTryPatchRepair : IEventMessage
        {
            public static void SendEventMessage()
            {
                var msg = new UserTryPatchRepair();
                UniEvent.SendMessage(msg);
            }
        }

        /// <summary>
        /// User retry update app version again
        /// </summary>
        public class UserTryAppVersionUpdate : IEventMessage
        {
            public static void SendEventMessage()
            {
                var msg = new UserTryAppVersionUpdate();
                UniEvent.SendMessage(msg);
            }
        }

        /// <summary>
        /// User retry init patch mode again
        /// </summary>
        public class UserTryInitPatchMode : IEventMessage
        {
            public static void SendEventMessage()
            {
                var msg = new UserTryInitPatchMode();
                UniEvent.SendMessage(msg);
            }
        }

        /// <summary>
        /// User retry update patch version again
        /// </summary>
        public class UserTryPatchVersionUpdate : IEventMessage
        {
            public static void SendEventMessage()
            {
                var msg = new UserTryPatchVersionUpdate();
                UniEvent.SendMessage(msg);
            }
        }

        /// <summary>
        /// User retry update patch manifest again
        /// </summary>
        public class UserTryPatchManifestUpdate : IEventMessage
        {
            public static void SendEventMessage()
            {
                var msg = new UserTryPatchManifestUpdate();
                UniEvent.SendMessage(msg);
            }
        }

        /// <summary>
        /// User retry download again
        /// </summary>
        public class UserTryCreateDownloader : IEventMessage
        {
            public static void SendEventMessage()
            {
                var msg = new UserTryCreateDownloader();
                UniEvent.SendMessage(msg);
            }
        }

        /// <summary>
        /// User begin download
        /// </summary>
        public class UserBeginDownload : IEventMessage
        {
            public static void SendEventMessage(GroupInfo groupInfo)
            {
                var msg = new UserBeginDownload();
                PatchManager.SetLastGroupInfo(groupInfo);
                UniEvent.SendMessage(msg);
            }
        }
    }
}