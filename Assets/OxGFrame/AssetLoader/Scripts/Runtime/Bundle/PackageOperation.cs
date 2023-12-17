using OxGFrame.AssetLoader.PatchEvent;
using OxGFrame.AssetLoader.PatchFsm;
using OxGKit.LoggingSystem;
using System.Collections.Generic;
using UniFramework.Event;
using UniFramework.Machine;
using YooAsset;

namespace OxGFrame.AssetLoader.Bundle
{
    public class PackageOperation
    {
        /// <summary>
        /// Instance id
        /// </summary>
        public int hashId
        {
            get
            {
                // Return instance hash code
                return this.GetHashCode();
            }
        }

        /// <summary>
        /// Package group info
        /// </summary>
        public GroupInfo groupInfo { get; protected set; }

        /// <summary>
        /// Package operaion event group
        /// </summary>
        public EventGroup eventGroup { get; protected set; }

        /// <summary>
        /// Skip download step
        /// </summary>
        public bool skipDownload { get; protected set; }

        private ResourceDownloaderOperation[] _downloaders;
        private PackageInfoWithBuild[] _packageInfos;

        private bool _isReady = false;
        private bool _isBegin = false;
        private bool _isRepair = false;
        private bool _isDone = false;

        private StateMachine _patchFsm;

        internal const string KEY_PACKAGE_VERSIONS = "packageVersions";

        protected PackageOperation()
        {
            // 註冊 Events 監聽事件
            this.eventGroup = new EventGroup(this.hashId);
            // Patch event receivers
            this.eventGroup.AddListener<PackageEvents.PatchRepairFailed>(this._OnHandleEventMessage);
            this.eventGroup.AddListener<PackageEvents.PatchInitPatchModeFailed>(this._OnHandleEventMessage);
            this.eventGroup.AddListener<PackageEvents.PatchVersionUpdateFailed>(this._OnHandleEventMessage);
            this.eventGroup.AddListener<PackageEvents.PatchManifestUpdateFailed>(this._OnHandleEventMessage);
            this.eventGroup.AddListener<PackageEvents.PatchDownloadFailed>(this._OnHandleEventMessage);
            // User event receivers
            this.eventGroup.AddListener<PackageUserEvents.UserTryPatchRepair>(this._OnHandleEventMessage);
            this.eventGroup.AddListener<PackageUserEvents.UserTryInitPatchMode>(this._OnHandleEventMessage);
            this.eventGroup.AddListener<PackageUserEvents.UserBeginDownload>(this._OnHandleEventMessage);
            this.eventGroup.AddListener<PackageUserEvents.UserTryPatchVersionUpdate>(this._OnHandleEventMessage);
            this.eventGroup.AddListener<PackageUserEvents.UserTryPatchManifestUpdate>(this._OnHandleEventMessage);
            this.eventGroup.AddListener<PackageUserEvents.UserTryCreateDownloader>(this._OnHandleEventMessage);

            // 註冊 PatchFsm 處理流程
            this._patchFsm = new StateMachine(this);
            this._patchFsm.AddNode<PackageFsmStates.FsmPatchRepair>();
            this._patchFsm.AddNode<PackageFsmStates.FsmPatchPrepare>();
            this._patchFsm.AddNode<PackageFsmStates.FsmInitPatchMode>();
            this._patchFsm.AddNode<PackageFsmStates.FsmPatchVersionUpdate>();
            this._patchFsm.AddNode<PackageFsmStates.FsmPatchManifestUpdate>();
            this._patchFsm.AddNode<PackageFsmStates.FsmCreateDownloader>();
            this._patchFsm.AddNode<PackageFsmStates.FsmBeginDownload>();
            this._patchFsm.AddNode<PackageFsmStates.FsmDownloadOver>();
            this._patchFsm.AddNode<PackageFsmStates.FsmClearCache>();
            this._patchFsm.AddNode<PackageFsmStates.FsmPatchDone>();
        }

        public PackageOperation(string groupName, PackageInfoWithBuild packageInfo, bool skipDownload = false) : this(groupName, new PackageInfoWithBuild[] { packageInfo }, skipDownload)
        {
        }

        public PackageOperation(string groupName, PackageInfoWithBuild[] packageInfos, bool skipDownload = false) : this()
        {
            this.groupInfo = new GroupInfo();
            this.groupInfo.groupName = groupName;
            this._packageInfos = packageInfos;
            this.skipDownload = skipDownload;

            // 開始 Package 初始流程
            this._patchFsm.Run<PackageFsmStates.FsmPatchPrepare>();
        }

        ~PackageOperation()
        {
            this.Cancel();
            this.eventGroup.RemoveAllListener();
            this._patchFsm = null;
        }

        #region Factory Mode
        public static PackageOperation CreateOperation(string groupName, PackageInfoWithBuild packageInfo, bool skipDownload = false)
        {
            var packageOperation = new PackageOperation(groupName, packageInfo, skipDownload);
            return packageOperation;
        }

        public static PackageOperation CreateOperation(string groupName, PackageInfoWithBuild[] packageInfos, bool skipDownload = false)
        {
            var packageOperation = new PackageOperation(groupName, packageInfos, skipDownload);
            return packageOperation;
        }
        #endregion

        #region Downloader
        internal void SetDownloaders(ResourceDownloaderOperation[] downloaders)
        {
            this._downloaders = downloaders;
        }
        #endregion

        #region Package Info
        internal PackageInfoWithBuild[] GetPackageInfos()
        {
            return this._packageInfos;
        }

        internal string[] GetPackageNames()
        {
            List<string> packageNames = new List<string>();
            foreach (var packageInfo in this._packageInfos)
            {
                packageNames.Add(packageInfo.packageName);
            }
            return packageNames.ToArray();
        }

        internal ResourcePackage[] GetPackages()
        {
            if (this._packageInfos != null && this._packageInfos.Length > 0)
            {
                List<ResourcePackage> packages = new List<ResourcePackage>();
                foreach (var packageInfo in this._packageInfos)
                {
                    var package = PackageManager.GetPackage(packageInfo.packageName);
                    if (package != null) packages.Add(package);
                }

                return packages.ToArray();
            }

            return new ResourcePackage[] { };
        }
        #endregion

        #region Patch Operation
        /// <summary>
        /// 開啟下載流程
        /// </summary>
        public void Begin()
        {
            if (this._isReady && !this._isBegin && !this._isRepair)
            {
                this.MarkBeginState();
                this._patchFsm.Run<PackageFsmStates.FsmInitPatchMode>();
            }
            else
            {
                Logging.PrintWarning<Logger>($"GroupName: {this.groupInfo.groupName} Patch maybe not ready yet or begin already...");
            }
        }

        /// <summary>
        /// 刪除所有緩存數據跟配置檔 (即清空下載目錄)
        /// </summary>
        public void Repair()
        {
            if (!this._isRepair)
            {
                this._patchFsm.Run<PackageFsmStates.FsmPatchRepair>();
            }
            else
            {
                Logging.PrintWarning<Logger>($"GroupName: {this.groupInfo.groupName} Patch repairing...");
            }
        }

        /// <summary>
        /// 暫停下載
        /// </summary>
        public void Pause()
        {
            if (this._downloaders == null) return;
            foreach (var downloader in this._downloaders)
            {
                downloader.PauseDownload();
            }
        }

        /// <summary>
        /// 繼續下載
        /// </summary>
        public void Resume()
        {
            if (this._downloaders == null) return;
            foreach (var downloader in this._downloaders)
            {
                downloader.ResumeDownload();
            }
        }

        /// <summary>
        /// 取消下載
        /// </summary>
        public void Cancel(bool sendEvent = true)
        {
            if (this._downloaders == null) return;
            foreach (var downloader in this._downloaders)
            {
                downloader.CancelDownload();
            }
            if (sendEvent) PackageEvents.PatchDownloadCanceled.SendEventMessage(this.hashId);
            this.MarkBeginAsDone();
            this.MarkRepairAsDone();
        }
        #endregion

        #region Patch Flag
        /// <summary>
        /// 標記 Ready 狀態
        /// </summary>
        internal void MarkReadyState()
        {
            this._isReady = true;
        }

        /// <summary>
        /// 標記 Ready 結束
        /// </summary>
        internal void MarkReadyAsDone()
        {
            this._isReady = false;
        }

        /// <summary>
        /// 標記 Begin 狀態
        /// </summary>
        internal void MarkBeginState()
        {
            this._isDone = false;
            this._isBegin = true;
        }

        /// <summary>
        /// 標記 Begin 結束
        /// </summary>
        internal void MarkBeginAsDone()
        {
            this._isBegin = false;
        }

        /// <summary>
        /// 標記 Repair 狀態
        /// </summary>
        internal void MarkRepairState()
        {
            this._isDone = false;
            this._isRepair = true;
        }

        /// <summary>
        /// 標記 Repair 結束
        /// </summary>
        internal void MarkRepairAsDone()
        {
            this._isRepair = false;
            this._downloaders = null;
        }

        /// <summary>
        /// 標記 Patch 結束
        /// </summary>
        internal void MarkPatchAsDone()
        {
            this._isDone = true;
            this._isBegin = false;
            this._isRepair = false;
            this._downloaders = null;
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
        /// 是否開始準備
        /// </summary>
        /// <returns></returns>
        public bool IsReady()
        {
            return this._isReady;
        }

        /// <summary>
        /// 是否開始下載
        /// </summary>
        /// <returns></returns>
        public bool IsBegin()
        {
            return this._isBegin;
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

        #region Event Handle
        private void _OnHandleEventMessage(IEventMessage message)
        {
            // Package events
            if (message is PackageEvents.PatchFsmState)
            {
#if UNITY_EDITOR
                PackageEvents.PatchFsmState msgData = message as PackageEvents.PatchFsmState;

                switch (msgData.stateNode)
                {
                    case PackageFsmStates.FsmPatchRepair:
                        Logging.Print<Logger>("<color=#2dff4e> >>>> PackageFsmStates.FsmPatchRepair <<<< </color>");
                        break;
                    case PackageFsmStates.FsmPatchPrepare:
                        Logging.Print<Logger>("<color=#2dff4e> >>>> PackageFsmStates.FsmPatchPrepare <<<< </color>");
                        break;
                    case PackageFsmStates.FsmInitPatchMode:
                        Logging.Print<Logger>("<color=#2dff4e> >>>> PackageFsmStates.FsmInitPatchMode <<<< </color>");
                        break;
                    case PackageFsmStates.FsmPatchVersionUpdate:
                        Logging.Print<Logger>("<color=#2dff4e> >>>> PackageFsmStates.FsmPatchVersionUpdate <<<< </color>");
                        break;
                    case PackageFsmStates.FsmPatchManifestUpdate:
                        Logging.Print<Logger>("<color=#2dff4e> >>>> PackageFsmStates.FsmPatchManifestUpdate <<<< </color>");
                        break;
                    case PackageFsmStates.FsmCreateDownloader:
                        Logging.Print<Logger>("<color=#2dff4e> >>>> PackageFsmStates.FsmCreateDownloader <<<< </color>");
                        break;
                    case PackageFsmStates.FsmBeginDownload:
                        Logging.Print<Logger>("<color=#2dff4e> >>>> PackageFsmStates.FsmBeginDownloadFiles <<<< </color>");
                        break;
                    case PackageFsmStates.FsmDownloadOver:
                        Logging.Print<Logger>("<color=#2dff4e> >>>> PackageFsmStates.FsmDownloadOver <<<< </color>");
                        break;
                    case PackageFsmStates.FsmClearCache:
                        Logging.Print<Logger>("<color=#2dff4e> >>>> PackageFsmStates.FsmClearCache <<<< </color>");
                        break;
                    case PackageFsmStates.FsmPatchDone:
                        Logging.Print<Logger>("<color=#2dff4e> >>>> PackageFsmStates.FsmPatchDone <<<< </color>");
                        break;
                }

#endif
            }
            else if (message is PackageEvents.PatchRepairFailed)
            {
                PackageUserEvents.UserTryPatchRepair.SendEventMessage(this.hashId);
            }
            else if (message is PackageEvents.PatchInitPatchModeFailed)
            {
                PackageUserEvents.UserTryInitPatchMode.SendEventMessage(this.hashId);
            }
            else if (message is PackageEvents.PatchVersionUpdateFailed)
            {
                PackageUserEvents.UserTryPatchVersionUpdate.SendEventMessage(this.hashId);
            }
            else if (message is PackageEvents.PatchManifestUpdateFailed)
            {
                PackageUserEvents.UserTryPatchManifestUpdate.SendEventMessage(this.hashId);
            }
            else if (message is PackageEvents.PatchDownloadFailed)
            {
                PackageUserEvents.UserTryCreateDownloader.SendEventMessage(this.hashId);
            }
            // Package user events
            else if (message is PackageUserEvents.UserTryPatchRepair)
            {
                this._patchFsm.ChangeState<PackageFsmStates.FsmPatchRepair>();
            }
            else if (message is PackageUserEvents.UserTryInitPatchMode)
            {
                this._patchFsm.ChangeState<PackageFsmStates.FsmInitPatchMode>();
            }
            else if (message is PackageUserEvents.UserBeginDownload)
            {
                this._patchFsm.ChangeState<PackageFsmStates.FsmBeginDownload>();
            }
            else if (message is PackageUserEvents.UserTryPatchVersionUpdate)
            {
                this._patchFsm.ChangeState<PackageFsmStates.FsmPatchVersionUpdate>();
            }
            else if (message is PackageUserEvents.UserTryPatchManifestUpdate)
            {
                this._patchFsm.ChangeState<PackageFsmStates.FsmPatchManifestUpdate>();
            }
            else if (message is PackageUserEvents.UserTryCreateDownloader)
            {
                this._patchFsm.ChangeState<PackageFsmStates.FsmCreateDownloader>();
            }
        }
        #endregion
    }
}