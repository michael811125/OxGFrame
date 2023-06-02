using OxGFrame.Hotfixer.HotfixEvent;
using OxGFrame.Hotfixer.HotfixFsm;
using System.Collections.Generic;
using System.Reflection;
using UniFramework.Event;
using UniFramework.Machine;
using UnityEngine;
using YooAsset;

namespace OxGFrame.Hotfixer
{
    internal class HotfixManager
    {
        public string packageName { get; private set; }
        public ResourceDownloaderOperation mainDownloader;

        private bool _isCheck = false;
        private bool _isDone = false;

        private List<string> _listAOTAssemblies;
        private List<string> _listHotfixAssemblies;
        private Dictionary<string, Assembly> _dictHotfixAssemblies;

        private EventGroup _userEvents;
        private StateMachine _hotfixFsm;

        private static HotfixManager _instance = null;
        public static HotfixManager GetInstance()
        {
            if (_instance == null) _instance = new HotfixManager();
            return _instance;
        }

        public HotfixManager()
        {
            this._listAOTAssemblies = new List<string>();
            this._listHotfixAssemblies = new List<string>();
            this._dictHotfixAssemblies = new Dictionary<string, Assembly>();

#if UNITY_EDITOR
            UniEvent.AddListener<HotfixEvents.HotfixFsmState>((message) =>
            {
                HotfixEvents.HotfixFsmState msgData = message as HotfixEvents.HotfixFsmState;

                switch (msgData.stateNode)
                {
                    case HotfixFsmStates.FsmHotfixPrepare:
                        Debug.Log("<color=#00e7aa> >>>> HotfixFsmStates.FsmHotfixPrepare <<<< </color>");
                        break;
                    case HotfixFsmStates.FsmInitHotfixPackage:
                        Debug.Log("<color=#00e7aa> >>>> HotfixFsmStates.FsmInitHotfixPackage <<<< </color>");
                        break;
                    case HotfixFsmStates.FsmUpdateHotfixPackage:
                        Debug.Log("<color=#00e7aa> >>>> HotfixFsmStates.FsmUpdateHotfixPackage <<<< </color>");
                        break;
                    case HotfixFsmStates.FsmHotfixCreateDownloader:
                        Debug.Log("<color=#00e7aa> >>>> HotfixFsmStates.FsmHotfixCreateDownloader <<<< </color>");
                        break;
                    case HotfixFsmStates.FsmHotfixBeginDownload:
                        Debug.Log("<color=#00e7aa> >>>> HotfixFsmStates.FsmHotfixBeginDownload <<<< </color>");
                        break;
                    case HotfixFsmStates.FsmHotfixDownloadOver:
                        Debug.Log("<color=#00e7aa> >>>> HotfixFsmStates.FsmHotfixDownloadOver <<<< </color>");
                        break;
                    case HotfixFsmStates.FsmHotfixClearCache:
                        Debug.Log("<color=#00e7aa> >>>> HotfixFsmStates.FsmHotfixClearCache <<<< </color>");
                        break;
                    case HotfixFsmStates.FsmLoadAOTAssemblies:
                        Debug.Log("<color=#00e7aa> >>>> HotfixFsmStates.FsmLoadAOTAssemblies <<<< </color>");
                        break;
                    case HotfixFsmStates.FsmLoadHotfixAssemblies:
                        Debug.Log("<color=#00e7aa> >>>> HotfixFsmStates.FsmLoadHotfixAssemblies <<<< </color>");
                        break;
                    case HotfixFsmStates.FsmHotfixDone:
                        Debug.Log("<color=#00e7aa> >>>> HotfixFsmStates.FsmHotfixDone <<<< </color>");
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

        public void ReleaseMainDownloader()
        {
            this.mainDownloader = null;
        }

        public string[] GetAOTAssemblyNames()
        {
            return this._listAOTAssemblies.ToArray();
        }

        public void ReleaseAOTAssemblyNames()
        {
            this._listAOTAssemblies.Clear();
            this._listAOTAssemblies = null;
        }

        public string[] GetHotfixAssemblyNames()
        {
            return this._listHotfixAssemblies.ToArray();
        }

        public void ReleaseHotfixAssemblyNames()
        {
            this._listHotfixAssemblies.Clear();
            this._listHotfixAssemblies = null;
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
                Debug.Log("<color=#ff8686>Hotfix all are loaded.</color>");
                return;
            }

            if (!this._isCheck)
            {
                this._isCheck = true;

                this._listAOTAssemblies.Clear();
                this._listHotfixAssemblies.Clear();

                // Hotfix package name
                this.packageName = packageName;

                // Add AOT assemblies
                for (int i = 0; i < aotAssemblies.Length; i++)
                {
                    this._listAOTAssemblies.Add(aotAssemblies[i]);
                }

                // Add Hotfix assemblies
                for (int i = 0; i < hotfixAssemblies.Length; i++)
                {
                    this._listHotfixAssemblies.Add(hotfixAssemblies[i]);
                }

                // Run hotfix procedure
                this._hotfixFsm.Run<HotfixFsmStates.FsmHotfixPrepare>();
            }
            else
            {
                Debug.LogWarning("Hotfix Checking...");
            }
        }
        #endregion

        #region Hotfix Flag
        public void MarkAsDone()
        {
            this._isDone = true;
        }

        public bool IsDone()
        {
            return this._isDone;
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
