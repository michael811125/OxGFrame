using UnityEngine;
using UnityEngine.InputSystem;
using OxGFrame.CoreFrame;
using Cysharp.Threading.Tasks;

public static class ScreenUIs
{
    // If use prefix "res#" will load from resource else will from bundle
    private const string _prefix = "res#";
    private const string _path = "Example/UI/ScreenUI/";

    // Assets
    public static readonly string Demo1UI = $"{_prefix}{_path}Demo1UI";
    public static readonly string Demo2UI = $"{_prefix}{_path}Demo2UI";
    public static readonly string Demo3UI = $"{_prefix}{_path}Demo3UI";
    public static readonly string DemoLoadingUI = $"{_prefix}{_path}DemoLoadingUI";

    // Group Id
    public const int Id = 1;
}

public static class WorldUIs
{
    // If use prefix "res#" will load from resource else will from bundle
    private const string _prefix = "res#";
    private const string _path = "Example/UI/WorldUI/";

    // Assets
    public static readonly string Demo1UI = $"{_prefix}{_path}Demo1UI";
    public static readonly string Demo2UI = $"{_prefix}{_path}Demo2UI";
    public static readonly string Demo3UI = $"{_prefix}{_path}Demo3UI";
    public static readonly string DemoLoadingUI = $"{_prefix}{_path}DemoLoadingUI";

    // Group Id
    public const int Id = 2;
}

public class UIFrameDemo : MonoBehaviour
{
    public const string CanvasCamera = "CanvasCamera";
    public const string CanvasWorld = "CanvasWorld";

    private void Awake()
    {
        // If Init instance can more efficiency
        CoreFrames.UIFrame.InitInstance();
    }

    private void Update()
    {
        if (Keyboard.current.numpad0Key.wasReleasedThisFrame)
        {
            this.PreloadFirstWorldUI();
        }
        if (Keyboard.current.numpad1Key.wasReleasedThisFrame)
        {
            this.ShowFirstWorldUI();
        }
        else if (Keyboard.current.numpad2Key.wasReleasedThisFrame)
        {
            CoreFrames.UIFrame.HideAll(WorldUIs.Id);
        }
        else if (Keyboard.current.numpad3Key.wasReleasedThisFrame)
        {
            CoreFrames.UIFrame.RevealAll(WorldUIs.Id);
        }
        else if (Keyboard.current.numpad4Key.wasReleasedThisFrame)
        {
            CoreFrames.UIFrame.CloseAll(true, true);
        }
        else if (Keyboard.current.numpad5Key.wasReleasedThisFrame)
        {
            CoreFrames.UIFrame.CloseAll(true);
        }
        else if (Keyboard.current.numpad6Key.wasReleasedThisFrame)
        {
            CoreFrames.UIFrame.Show(WorldUIs.Id, WorldUIs.Demo3UI).Forget();
        }
        else if (Keyboard.current.numpad7Key.wasReleasedThisFrame)
        {
            CoreFrames.UIFrame.Close(WorldUIs.Demo3UI, true, true);
        }
    }

    public async void ShowFirstScreenUI()
    {
        await CoreFrames.UIFrame.Show(ScreenUIs.Id, ScreenUIs.Demo1UI, null, ScreenUIs.DemoLoadingUI, null, null);
    }

    private int _dataCount = 0;
    public async void ShowFirstWorldUI()
    {
        if (!CoreFrames.UIFrame.CheckIsShowing(WorldUIs.Demo1UI)) this._dataCount++;
        await CoreFrames.UIFrame.Show(WorldUIs.Id, WorldUIs.Demo1UI, $"Send Msg Data: {this._dataCount}", WorldUIs.DemoLoadingUI, null, null);
    }

    public async void PreloadFirstWorldUI()
    {
        await CoreFrames.UIFrame.Preload(WorldUIs.Demo1UI);
    }
}
