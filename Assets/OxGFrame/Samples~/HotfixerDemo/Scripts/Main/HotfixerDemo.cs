using Cysharp.Threading.Tasks;
using OxGFrame.CoreFrame;
using OxGFrame.Hotfixer;
using OxGFrame.Hotfixer.HotfixEvent;
using OxGFrame.Hotfixer.HotfixFsm;
using UniFramework.Event;
using UnityEngine;

public class HotfixerDemo : MonoBehaviour
{
    private bool _isLoaded = false;

    private EventGroup _hotfixEvents = new EventGroup();

    private void Start()
    {
        // Init Hotfix Events
        this._InitHotfixEvents();
    }

    #region Hotfix Event
    private void _InitHotfixEvents()
    {
        // 0. HotfixFsmState
        // 1. HotfixInitFailed
        // 2. HotfixUpdateFailed
        // 3. HotfixDownloadFailed

        #region Add HotfixEvents Handle
        this._hotfixEvents.AddListener<HotfixEvents.HotfixFsmState>(this._OnHandleEventMessage);
        this._hotfixEvents.AddListener<HotfixEvents.HotfixInitFailed>(this._OnHandleEventMessage);
        this._hotfixEvents.AddListener<HotfixEvents.HotfixUpdateFailed>(this._OnHandleEventMessage);
        this._hotfixEvents.AddListener<HotfixEvents.HotfixDownloadFailed>(this._OnHandleEventMessage);
        #endregion
    }

    private void _OnHandleEventMessage(IEventMessage message)
    {
        if (message is HotfixEvents.HotfixFsmState)
        {
            HotfixEvents.HotfixFsmState msgData = message as HotfixEvents.HotfixFsmState;

            switch (msgData.stateNode)
            {
                case HotfixFsmStates.FsmHotfixPrepare:
                    break;
                case HotfixFsmStates.FsmInitHotfixPackage:
                    break;
                case HotfixFsmStates.FsmUpdateHotfixPackage:
                    break;
                case HotfixFsmStates.FsmHotfixCreateDownloader:
                    break;
                case HotfixFsmStates.FsmHotfixBeginDownload:
                    break;
                case HotfixFsmStates.FsmHotfixDownloadOver:
                    break;
                case HotfixFsmStates.FsmHotfixClearCache:
                    break;
                case HotfixFsmStates.FsmLoadAOTAssemblies:
                    break;
                case HotfixFsmStates.FsmLoadHotfixAssemblies:
                    break;
                case HotfixFsmStates.FsmHotfixDone:
                    break;
            }
        }
        else if (message is HotfixEvents.HotfixInitFailed)
        {
            HotfixUserEvents.UserTryInitHotfix.SendEventMessage();
        }
        else if (message is HotfixEvents.HotfixUpdateFailed)
        {
            HotfixUserEvents.UserTryUpdateHotfix.SendEventMessage();
        }
        else if (message is HotfixEvents.HotfixDownloadFailed)
        {
            HotfixUserEvents.UserTryCreateDownloader.SendEventMessage();
        }
        else
        {
            throw new System.NotImplementedException($"{message.GetType()}");
        }
    }
    #endregion

    private void Update()
    {
        // If hotfix files download and load are done
        if (Hotfixers.IsDone() && !this._isLoaded)
        {
            this._isLoaded = true;

            // Load Hotfix Main Scene from HotfixPackage
            UniTask.Void(async () => await CoreFrames.USFrame.LoadSingleSceneAsync("HotfixPackage", "HotfixMain"));

            // The main assembly cannot directly reference the hotfix assembly.
            // Here, the hotfix code is called through reflection after all are loaded.
            //Hotfixers.GetHotfixAssembly("HotfixDemo.Hotfix.Runtime.dll")?.GetType("Hello").GetMethod("Run", null, null);
        }
    }

    public void StartHotfixCheck()
    {
        // [Recommend do hotfix in background (While Logo showing)]

        // Start hotfix files download and load all (Use YooAsset to collect files)
        Hotfixers.CheckHotfix
        (
            // Download and load hotfix files from HotfixPackage
            "HotfixPackage",
            // Metadata for AOT assemblies
            new string[]
            {
                "mscorlib.dll",
                //"System.dll",
                //"System.Core.dll",
                "UniTask.dll"
            },
            // Hotfix assemblies
            new string[]
            {
                "HotfixerDemo.Hotfix.Runtime.dll"
            }
        );
    }
}
