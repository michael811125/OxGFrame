using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader;
using OxGFrame.CoreFrame;
using OxGFrame.Hotfixer;
using OxGFrame.Hotfixer.HotfixEvent;
using OxGFrame.Hotfixer.HotfixFsm;
using System.Collections;
using UniFramework.Event;
using UnityEngine;

public class HotfixerDemo : MonoBehaviour
{
    [Tooltip("If checked, it will automatically request the config file (hotfixdllconfig.conf) from StreamingAssets.")]
    public bool loadFromConfig = false;

    private bool _isLoaded = false;
    private bool _isInitialized = false;
    private EventGroup _hotfixEvents = new EventGroup();

    private enum HotfixStep
    {
        NONE,
        WAITING_FOR_USER_TO_START_HOTFIX,
        START_CHECK_HOTFIX,
        WAITING_FOR_HOTFIX,
        LOAD_HOTFIX_MAIN_SCENE,
        DONE
    }

    private HotfixStep _hotfixStep = HotfixStep.NONE;

    private IEnumerator Start()
    {
        this._isInitialized = false;

        // Wait Until IsInitialized
        while (!AssetPatcher.IsInitialized())
            yield return null;

        // Init Hotfix Events
        this._InitHotfixEvents();
        // Init Hotfix Step
        this._hotfixStep = HotfixStep.WAITING_FOR_USER_TO_START_HOTFIX;

        this._isInitialized = true;
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
        if (!this._isInitialized)
            return;

        switch (this._hotfixStep)
        {
            case HotfixStep.WAITING_FOR_USER_TO_START_HOTFIX:
                /**
                 * Waiting for user to start hotfix event
                 */
                break;

            case HotfixStep.START_CHECK_HOTFIX:
                /**
                 * Start hotfix by user event
                 */

                // Change step
                this._hotfixStep = HotfixStep.WAITING_FOR_HOTFIX;
                break;

            case HotfixStep.WAITING_FOR_HOTFIX:
                // If hotfix files download and load are done
                if (Hotfixers.IsDone() && !this._isLoaded)
                {
                    this._isLoaded = true;
                    // Change step
                    this._hotfixStep = HotfixStep.LOAD_HOTFIX_MAIN_SCENE;
                }
                break;

            case HotfixStep.LOAD_HOTFIX_MAIN_SCENE:
                if (this._isLoaded)
                {
                    // Load Hotfix Main Scene from HotfixPackage
                    CoreFrames.USFrame.LoadSingleSceneAsync("HotfixPackage", "HotfixMain").Forget();

                    /**
                     * The main assembly cannot directly reference the hotfix assembly.
                     * Here, the hotfix code is called through reflection after all are loaded.
                     */

                    //Hotfixers.GetHotfixAssembly("HotfixDemo.Hotfix.Runtime.dll")?.GetType("Hello").GetMethod("Run", null, null);

                    // Change step
                    this._hotfixStep = HotfixStep.NONE;
                }
                break;

            // Nothing to do
            case HotfixStep.DONE:
                break;
        }
    }

    public void StartHotfixCheck()
    {
        // [Recommend do hotfix in background (While Logo showing)]

        // Start hotfix files download and load all (Use YooAsset to collect files)

        if (!this.loadFromConfig)
        {
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
        else
        {
            // Auto try to load hotfixdllconfig.conf from StreamingAssets
            Hotfixers.CheckHotfix
            (
                // Download and load hotfix files from HotfixPackage
                "HotfixPackage",
                () =>
                {
                    Debug.LogWarning("<color=#ff8321>Please generate the hotfixdllconfig.conf file in the StreamingAssets folder using the MenuItem -> OxGFrame/Hotfixer/Hotfix Dll Config Generator.</color>");
                }
            );
        }

        // Change step
        this._hotfixStep = HotfixStep.START_CHECK_HOTFIX;
    }
}