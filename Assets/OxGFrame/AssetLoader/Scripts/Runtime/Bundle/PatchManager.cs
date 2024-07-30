using OxGFrame.AssetLoader.PatchEvent;
using UniFramework.Event;
using UniFramework.Machine;
using UnityEngine;
using OxGFrame.AssetLoader.PatchFsm;
using YooAsset;
using Newtonsoft.Json;
using OxGKit.LoggingSystem;

namespace OxGFrame.AssetLoader.Bundle
{
    internal class PatchManager
    {
        #region Last Group Info
        internal const string DEFAULT_GROUP_TAG = "#all";
        internal const string LAST_GROUP_INFO_KEY = "LAST_GROUP_INFO_KEY";

        internal static GroupInfo GetLastGroupInfo()
        {
            string json = PlayerPrefs.GetString(LAST_GROUP_INFO_KEY, string.Empty);
            if (!string.IsNullOrEmpty(json)) return JsonConvert.DeserializeObject<GroupInfo>(json);
            return null;
        }

        internal static void SetLastGroupInfo(GroupInfo groupInfo)
        {
            if (groupInfo != null)
            {
                string json = JsonConvert.SerializeObject(groupInfo);
                PlayerPrefs.SetString(LAST_GROUP_INFO_KEY, json);
            }
        }

        internal static void DelLastGroupInfo()
        {
            PlayerPrefs.DeleteKey(LAST_GROUP_INFO_KEY);
        }
        #endregion

        internal static string platform = string.Empty;
        internal static string appVersion = string.Empty;
        internal static string[] patchVersions;
        internal ResourceDownloaderOperation[] mainDownloaders;

        private bool _isCheck = false;
        private bool _isRepair = false;
        private bool _isDone = false;

        private EventGroup _userEvents;
        private StateMachine _patchFsm;

        private static PatchManager _instance = null;
        internal static PatchManager GetInstance()
        {
            if (_instance == null) _instance = new PatchManager();
            return _instance;
        }

        public PatchManager()
        {
#if UNITY_EDITOR
            UniEvent.AddListener<PatchEvents.PatchFsmState>((message) =>
            {
                PatchEvents.PatchFsmState msgData = message as PatchEvents.PatchFsmState;

                switch (msgData.stateNode)
                {
                    case PatchFsmStates.FsmPatchRepair:
                        Logging.Print<Logger>("<color=#2dff4e> >>>> PatchFsmStates.FsmPatchRepair <<<< </color>");
                        break;
                    case PatchFsmStates.FsmPatchPrepare:
                        Logging.Print<Logger>("<color=#2dff4e> >>>> PatchFsmStates.FsmPatchPrepare <<<< </color>");
                        break;
                    case PatchFsmStates.FsmAppVersionUpdate:
                        Logging.Print<Logger>("<color=#2dff4e> >>>> PatchFsmStates.FsmAppVersionUpdate <<<< </color>");
                        break;
                    case PatchFsmStates.FsmInitPatchMode:
                        Logging.Print<Logger>("<color=#2dff4e> >>>> PatchFsmStates.FsmInitPatchMode <<<< </color>");
                        break;
                    case PatchFsmStates.FsmPatchVersionUpdate:
                        Logging.Print<Logger>("<color=#2dff4e> >>>> PatchFsmStates.FsmPatchVersionUpdate <<<< </color>");
                        break;
                    case PatchFsmStates.FsmPatchManifestUpdate:
                        Logging.Print<Logger>("<color=#2dff4e> >>>> PatchFsmStates.FsmPatchManifestUpdate <<<< </color>");
                        break;
                    case PatchFsmStates.FsmCreateDownloader:
                        Logging.Print<Logger>("<color=#2dff4e> >>>> PatchFsmStates.FsmCreateDownloader <<<< </color>");
                        break;
                    case PatchFsmStates.FsmBeginDownload:
                        Logging.Print<Logger>("<color=#2dff4e> >>>> PatchFsmStates.FsmBeginDownloadFiles <<<< </color>");
                        break;
                    case PatchFsmStates.FsmDownloadOver:
                        Logging.Print<Logger>("<color=#2dff4e> >>>> PatchFsmStates.FsmDownloadOver <<<< </color>");
                        break;
                    case PatchFsmStates.FsmClearCache:
                        Logging.Print<Logger>("<color=#2dff4e> >>>> PatchFsmStates.FsmClearCache <<<< </color>");
                        break;
                    case PatchFsmStates.FsmPatchDone:
                        Logging.Print<Logger>("<color=#2dff4e> >>>> PatchFsmStates.FsmPatchDone <<<< </color>");
                        break;
                }
            });
#endif

            // 註冊 UserEvents 監聽事件
            this._userEvents = new EventGroup();
            this._userEvents.AddListener<PatchUserEvents.UserTryPatchRepair>(this._OnHandleEventMessage);
            this._userEvents.AddListener<PatchUserEvents.UserTryAppVersionUpdate>(this._OnHandleEventMessage);
            this._userEvents.AddListener<PatchUserEvents.UserTryInitPatchMode>(this._OnHandleEventMessage);
            this._userEvents.AddListener<PatchUserEvents.UserTryPatchVersionUpdate>(this._OnHandleEventMessage);
            this._userEvents.AddListener<PatchUserEvents.UserTryPatchManifestUpdate>(this._OnHandleEventMessage);
            this._userEvents.AddListener<PatchUserEvents.UserTryCreateDownloader>(this._OnHandleEventMessage);
            this._userEvents.AddListener<PatchUserEvents.UserBeginDownload>(this._OnHandleEventMessage);

            // 註冊 PatchFsm 處理流程
            this._patchFsm = new StateMachine(this);
            this._patchFsm.AddNode<PatchFsmStates.FsmPatchRepair>();
            this._patchFsm.AddNode<PatchFsmStates.FsmPatchPrepare>();
            this._patchFsm.AddNode<PatchFsmStates.FsmAppVersionUpdate>();
            this._patchFsm.AddNode<PatchFsmStates.FsmInitPatchMode>();
            this._patchFsm.AddNode<PatchFsmStates.FsmPatchVersionUpdate>();
            this._patchFsm.AddNode<PatchFsmStates.FsmPatchManifestUpdate>();
            this._patchFsm.AddNode<PatchFsmStates.FsmCreateDownloader>();
            this._patchFsm.AddNode<PatchFsmStates.FsmBeginDownload>();
            this._patchFsm.AddNode<PatchFsmStates.FsmDownloadOver>();
            this._patchFsm.AddNode<PatchFsmStates.FsmClearCache>();
            this._patchFsm.AddNode<PatchFsmStates.FsmPatchDone>();
        }

        ~PatchManager()
        {
            this.Cancel();
            this._userEvents.RemoveAllListener();
            this._patchFsm = null;
        }

        #region Patch Operation
        /// <summary>
        /// 開啟檢查流程
        /// </summary>
        public void Check()
        {
            if (!this._isCheck && !this._isRepair)
            {
                this._patchFsm.Run<PatchFsmStates.FsmPatchPrepare>();
            }
            else
            {
                Logging.PrintWarning<Logger>("Patch checking...");
            }
        }

        /// <summary>
        /// 刪除所有緩存數據跟配置檔 (即清空下載目錄)
        /// </summary>
        public void Repair()
        {
            if (!this._isRepair)
            {
                this._patchFsm.Run<PatchFsmStates.FsmPatchRepair>();
            }
            else
            {
                Logging.PrintWarning<Logger>("Patch repairing...");
            }
        }

        /// <summary>
        /// 暫停下載
        /// </summary>
        public void Pause()
        {
            if (this.mainDownloaders == null) return;
            foreach (var downloader in this.mainDownloaders)
            {
                downloader.PauseDownload();
            }
        }

        /// <summary>
        /// 繼續下載
        /// </summary>
        public void Resume()
        {
            if (this.mainDownloaders == null) return;
            foreach (var downloader in this.mainDownloaders)
            {
                downloader.ResumeDownload();
            }
        }

        /// <summary>
        /// 取消下載
        /// </summary>
        public void Cancel(bool sendEvent = true)
        {
            if (this.mainDownloaders == null) return;
            foreach (var downloader in this.mainDownloaders)
            {
                downloader.CancelDownload();
            }
            if (sendEvent) PatchEvents.PatchDownloadCanceled.SendEventMessage();
            this.MarkCheckAsDone();
            this.MarkRepairAsDone();
        }
        #endregion

        #region Patch Flag
        /// <summary>
        /// 標記 Check 狀態
        /// </summary>
        public void MarkCheckState()
        {
            this._isDone = false;
            this._isCheck = true;
        }

        /// <summary>
        /// 標記 Check 結束
        /// </summary>
        public void MarkCheckAsDone()
        {
            this._isCheck = false;
        }

        /// <summary>
        /// 標記 Repair 狀態
        /// </summary>
        public void MarkRepairState()
        {
            this._isDone = false;
            this._isRepair = true;
        }

        /// <summary>
        /// 標記 Repair 結束
        /// </summary>
        public void MarkRepairAsDone()
        {
            this._isRepair = false;
            this.mainDownloaders = null;
        }

        /// <summary>
        /// 標記 Patch 結束
        /// </summary>
        public void MarkPatchAsDone()
        {
            this._isDone = true;
            this._isCheck = false;
            this._isRepair = false;
            this.mainDownloaders = null;
        }

        /// <summary>
        /// 是否更新結束
        /// </summary>
        /// <returns></returns>
        public bool IsDone()
        {
            return this._isDone;
        }

        /// <summary>
        /// 是否開始檢查
        /// </summary>
        /// <returns></returns>
        public bool IsCheck()
        {
            return this._isCheck;
        }

        /// <summary>
        /// 是否開始修復
        /// </summary>
        /// <returns></returns>
        public bool IsRepair()
        {
            return this._isRepair;
        }
        #endregion

        #region User Event Handle
        private void _OnHandleEventMessage(IEventMessage message)
        {
            if (message is PatchUserEvents.UserTryPatchRepair)
            {
                this._patchFsm.ChangeState<PatchFsmStates.FsmPatchRepair>();
            }
            else if (message is PatchUserEvents.UserTryAppVersionUpdate)
            {
                this._patchFsm.ChangeState<PatchFsmStates.FsmAppVersionUpdate>();
            }
            else if (message is PatchUserEvents.UserTryInitPatchMode)
            {
                this._patchFsm.ChangeState<PatchFsmStates.FsmInitPatchMode>();
            }
            else if (message is PatchUserEvents.UserTryPatchVersionUpdate)
            {
                this._patchFsm.ChangeState<PatchFsmStates.FsmPatchVersionUpdate>();
            }
            else if (message is PatchUserEvents.UserTryPatchManifestUpdate)
            {
                this._patchFsm.ChangeState<PatchFsmStates.FsmPatchManifestUpdate>();
            }
            else if (message is PatchUserEvents.UserTryCreateDownloader)
            {
                this._patchFsm.ChangeState<PatchFsmStates.FsmCreateDownloader>();
            }
            else if (message is PatchUserEvents.UserBeginDownload)
            {
                this._patchFsm.ChangeState<PatchFsmStates.FsmBeginDownload>();
            }
            else
            {
                throw new System.NotImplementedException($"{message.GetType()}");
            }
        }
        #endregion
    }
}
