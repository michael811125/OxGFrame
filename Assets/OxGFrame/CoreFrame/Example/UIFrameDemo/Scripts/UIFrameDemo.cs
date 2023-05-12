using UnityEngine;
using UnityEngine.InputSystem;
using OxGFrame.CoreFrame;
using Cysharp.Threading.Tasks;

public static class Screen
{
    // If use prefix "res#" will load from resource else will from bundle
    private const string _prefix = "res#";

    // Assets
    public static readonly string Demo1UI = $"{_prefix}Example/UI/ScreenUI/Demo1UI";
    public static readonly string Demo2UI = $"{_prefix}Example/UI/ScreenUI/Demo2UI";
    public static readonly string Demo3UI = $"{_prefix}Example/UI/ScreenUI/Demo3UI";
    public static readonly string DemoLoadingUI = $"{_prefix}Example/UI/ScreenUI/DemoLoadingUI";

    // Group Id
    public const int Id = 1;
}

public static class World
{
    // If use prefix "res#" will load from resource else will from bundle
    private const string _prefix = "res#";

    // Assets
    public static readonly string Demo1UI = $"{_prefix}Example/UI/WorldUI/Demo1UI";
    public static readonly string Demo2UI = $"{_prefix}Example/UI/WorldUI/Demo2UI";
    public static readonly string Demo3UI = $"{_prefix}Example/UI/WorldUI/Demo3UI";
    public static readonly string DemoLoadingUI = $"{_prefix}Example/UI/WorldUI/DemoLoadingUI";

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
            CoreFrames.UIFrame.HideAll(World.Id);
        }
        else if (Keyboard.current.numpad3Key.wasReleasedThisFrame)
        {
            CoreFrames.UIFrame.RevealAll(World.Id);
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
            CoreFrames.UIFrame.Show(World.Id, World.Demo3UI).Forget();
        }
        else if (Keyboard.current.numpad7Key.wasReleasedThisFrame)
        {
            CoreFrames.UIFrame.Close(World.Demo3UI, true, true);
        }
    }

    public async void ShowFirstScreenUI()
    {
        await CoreFrames.UIFrame.Show(Screen.Id, Screen.Demo1UI, null, Screen.DemoLoadingUI, null, null);
    }

    private int _dataCount = 0;
    public async void ShowFirstWorldUI()
    {
        if (!CoreFrames.UIFrame.CheckIsShowing(World.Demo1UI)) this._dataCount++;
        await CoreFrames.UIFrame.Show(World.Id, World.Demo1UI, $"Send Msg Data: {this._dataCount}", World.DemoLoadingUI, null, null);
    }

    public async void PreloadFirstWorldUI()
    {
        await CoreFrames.UIFrame.Preload(World.Demo1UI);
    }
}
