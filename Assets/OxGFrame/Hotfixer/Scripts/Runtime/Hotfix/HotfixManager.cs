using OxGFrame.AssetLoader.Bundle;
using OxGFrame.Hotfixer.HotfixEvent;
using OxGFrame.Hotfixer.HotfixFsm;
using OxGKit.LoggingSystem;
using System.Collections.Generic;
using System.Reflection;
using UniFramework.Event;
using UniFramework.Machine;
using YooAsset;

namespace OxGFrame.Hotfixer
{
    internal class HotfixManager
    {
        public string packageName { get; private set; }
        public PackageInfoWithBuild packageInfoWithBuild { get; private set; } = null;
        public ResourceDownloaderOperation mainDownloader;

        private bool _isCheck = false;
        private bool _isDone = false;

        private string[] _aotAssemblies;
        private string[] _hotfixAssemblies;
        private Dictionary<string, Assembly> _dictHotfixAssemblies;

        private EventGroup _userEvents;
        private StateMachine _hotfixFsm;

        private static HotfixManager _instance = null;
        public static HotfixManager GetInstance()
        {
            if (_instance == null)
                _instance = new HotfixManager();
            return _instance;
        }

        public HotfixManager()
        {
            this._dictHotfixAssemblies = new Dictionary<string, Assembly>();

#if UNITY_EDITOR
            UniEvent.AddListener<HotfixEvents.HotfixFsmState>((message) =>
            {
                HotfixEvents.HotfixFsmState msgData = message as HotfixEvents.HotfixFsmState;

                switch (msgData.stateNode)
                {
                    case HotfixFsmStates.FsmHotfixPrepare:
                        Logging.Print<Logger>("<color=#00e7aa> >>>> HotfixFsmStates.FsmHotfixPrepare <<<< </color>");
                        break;
                    case HotfixFsmStates.FsmInitHotfixPackage:
                        Logging.Print<Logger>("<color=#00e7aa> >>>> HotfixFsmStates.FsmInitHotfixPackage <<<< </color>");
                        break;
                    case HotfixFsmStates.FsmUpdateHotfixPackage:
                        Logging.Print<Logger>("<color=#00e7aa> >>>> HotfixFsmStates.FsmUpdateHotfixPackage <<<< </color>");
                        break;
                    case HotfixFsmStates.FsmHotfixCreateDownloader:
                        Logging.Print<Logger>("<color=#00e7aa> >>>> HotfixFsmStates.FsmHotfixCreateDownloader <<<< </color>");
                        break;
                    case HotfixFsmStates.FsmHotfixBeginDownload:
                        Logging.Print<Logger>("<color=#00e7aa> >>>> HotfixFsmStates.FsmHotfixBeginDownload <<<< </color>");
                        break;
                    case HotfixFsmStates.FsmHotfixDownloadOver:
                        Logging.Print<Logger>("<color=#00e7aa> >>>> HotfixFsmStates.FsmHotfixDownloadOver <<<< </color>");
                        break;
                    case HotfixFsmStates.FsmHotfixClearCache:
                        Logging.Print<Logger>("<color=#00e7aa> >>>> HotfixFsmStates.FsmHotfixClearCache <<<< </color>");
                        break;
                    case HotfixFsmStates.FsmLoadAOTAssemblies:
                        Logging.Print<Logger>("<color=#00e7aa> >>>> HotfixFsmStates.FsmLoadAOTAssemblies <<<< </color>");
                        break;
                    case HotfixFsmStates.FsmLoadHotfixAssemblies:
                        Logging.Print<Logger>("<color=#00e7aa> >>>> HotfixFsmStates.FsmLoadHotfixAssemblies <<<< </color>");
                        break;
                    case HotfixFsmStates.FsmHotfixDone:
                        Logging.Print<Logger>("<color=#00e7aa> >>>> HotfixFsmStates.FsmHotfixDone <<<< </color>");
                        break;
                }
            });
#endif

            // 註冊 UserEvents 監聽事件
            this._userEvents = new EventGroup();
            this._userEvents.AddListener<HotfixUserEvents.UserTryInitHotfix>(this._OnHandleEventMessage);
            this._userEvents.AddListener<HotfixUserEvents.UserTryUpdateHotfix>(this._OnHandleEventMessage);
            this._userEvents.AddListener<HotfixUserEvents.UserTryCreateDownloader>(this._OnHandleEventMessage);

            // 註冊 HotfixFsm 處理流程
            this._hotfixFsm = new StateMachine(this);
            this._hotfixFsm.AddNode<HotfixFsmStates.FsmHotfixPrepare>();
            this._hotfixFsm.AddNode<HotfixFsmStates.FsmInitHotfixPackage>();
            this._hotfixFsm.AddNode<HotfixFsmStates.FsmUpdateHotfixPackage>();
            this._hotfixFsm.AddNode<HotfixFsmStates.FsmHotfixCreateDownloader>();
            this._hotfixFsm.AddNode<HotfixFsmStates.FsmHotfixBeginDownload>();
            this._hotfixFsm.AddNode<HotfixFsmStates.FsmHotfixDownloadOver>();
            this._hotfixFsm.AddNode<HotfixFsmStates.FsmHotfixClearCache>();
            this._hotfixFsm.AddNode<HotfixFsmStates.FsmLoadAOTAssemblies>();
            this._hotfixFsm.AddNode<HotfixFsmStates.FsmLoadHotfixAssemblies>();
            this._hotfixFsm.AddNode<HotfixFsmStates.FsmHotfixDone>();
        }

        public void Reset()
        {
            this._isCheck = false;
            this._isDone = false;
            this._dictHotfixAssemblies.Clear();
        }

        public void ReleaseMainDownloader()
        {
            this.packageInfoWithBuild = null;
            this.mainDownloader = null;
        }

        public string[] GetAOTAssemblyNames()
        {
            return this._aotAssemblies;
        }

        public void ReleaseAOTAssemblyNames()
        {
            this._aotAssemblies = null;
        }

        public string[] GetHotfixAssemblyNames()
        {
            return this._hotfixAssemblies;
        }

        public void ReleaseHotfixAssemblyNames()
        {
            this._hotfixAssemblies = null;
        }

        public void AddHotfixAssembly(string assemblyName, Assembly assembly)
        {
            if (!this._dictHotfixAssemblies.ContainsKey(assemblyName))
            {
                this._dictHotfixAssemblies.Add(assemblyName, assembly);
            }
        }

        public Assembly GetHotfixAssembly(string assemblyName)
        {
            this._dictHotfixAssemblies.TryGetValue(assemblyName, out Assembly assembly);
            return assembly;
        }

        #region Hotfix Operation
        public void CheckHotfix(string packageName, string[] aotAssemblies, string[] hotfixAssemblies)
        {
            if (this._isDone)
            {
                Logging.Print<Logger>("<color=#ff8686>Hotfix all are loaded.</color>");
                return;
            }

            if (!this._isCheck)
            {
                this._isCheck = true;

                // Hotfix package name
                this.packageName = packageName;

                // Add AOT assemblies
                this._aotAssemblies = aotAssemblies;

                // Add Hotfix assemblies
                this._hotfixAssemblies = hotfixAssemblies;

                // Run hotfix procedure
                this._hotfixFsm.Run<HotfixFsmStates.FsmHotfixPrepare>();
            }
            else
            {
                Logging.PrintWarning<Logger>("Hotfix Checking...");
            }
        }

        public void CheckHotfix(PackageInfoWithBuild packageInfoWithBuild, string[] aotAssemblies, string[] hotfixAssemblies)
        {
            if (this._isDone)
            {
                Logging.Print<Logger>("<color=#ff8686>Hotfix all are loaded. Please run reset and try again.</color>");
                return;
            }

            if (!this._isCheck)
            {
                this._isCheck = true;

                // Hotfix package info
                this.packageInfoWithBuild = packageInfoWithBuild;

                // Add AOT assemblies
                this._aotAssemblies = aotAssemblies;

                // Add Hotfix assemblies
                this._hotfixAssemblies = hotfixAssemblies;

                // Run hotfix procedure
                this._hotfixFsm.Run<HotfixFsmStates.FsmHotfixPrepare>();
            }
            else
            {
                Logging.PrintWarning<Logger>("Hotfix Checking...");
            }
        }
        #endregion

        #region Hotfix Flag
        public void MarkAsDone()
        {
            this._isCheck = false;
            this._isDone = true;
        }

        public bool IsDone()
        {
            return this._isDone;
        }

        public bool IsDisabled()
        {
#if OXGFRAME_HYBRIDCLR_DISABLED
            return true;
#else
            return false;
#endif
        }
        #endregion

        #region User Event Handle
        private void _OnHandleEventMessage(IEventMessage message)
        {
            if (message is HotfixUserEvents.UserTryInitHotfix)
            {
                this._hotfixFsm.ChangeState<HotfixFsmStates.FsmInitHotfixPackage>();
            }
            else if (message is HotfixUserEvents.UserTryUpdateHotfix)
            {
                this._hotfixFsm.ChangeState<HotfixFsmStates.FsmUpdateHotfixPackage>();
            }
            else if (message is HotfixUserEvents.UserTryCreateDownloader)
            {
                this._hotfixFsm.ChangeState<HotfixFsmStates.FsmHotfixCreateDownloader>();
            }
            else
            {
                throw new System.NotImplementedException($"{message.GetType()}");
            }
        }
        #endregion
    }
}
