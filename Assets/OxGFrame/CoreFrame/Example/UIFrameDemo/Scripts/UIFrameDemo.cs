using UnityEngine;
using UnityEngine.InputSystem;
using OxGFrame.CoreFrame;
using Cysharp.Threading.Tasks;

public class UIFrameDemo : MonoBehaviour
{
    // if use prefix "res#" will load from resource else will from bundle
    public static string prefix = "res#";
    public static string canvasCamera = "CanvasCamera";
    public static string ScreenDemo1UI = $"{prefix}Example/UI/ScreenUI/Demo1UI";
    public static string ScreenDemo2UI = $"{prefix}Example/UI/ScreenUI/Demo2UI";
    public static string ScreenDemo3UI = $"{prefix}Example/UI/ScreenUI/Demo3UI";
    public static string ScreenDemoLoadingUI = $"{prefix}Example/UI/ScreenUI/DemoLoadingUI";
    public static int screenId = 1;

    public static string canvasWorld = "CanvasWorld";
    public static string WorldDemo1UI = $"{prefix}Example/UI/WorldUI/Demo1UI";
    public static string WorldDemo2UI = $"{prefix}Example/UI/WorldUI/Demo2UI";
    public static string WorldDemo3UI = $"{prefix}Example/UI/WorldUI/Demo3UI";
    public static string WorldDemoLoadingUI = $"{prefix}Example/UI/WorldUI/DemoLoadingUI";
    public static int worldId = 2;

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
            CoreFrames.UIFrame.HideAll(UIFrameDemo.worldId);
        }
        else if (Keyboard.current.numpad3Key.wasReleasedThisFrame)
        {
            CoreFrames.UIFrame.RevealAll(UIFrameDemo.worldId);
        }
        else if (Keyboard.current.numpad4Key.wasReleasedThisFrame)
        {
            CoreFrames.UIFrame.CloseAll(true, true);
        }
        else if (Keyboard.current.numpad5Key.wasReleasedThisFrame)
        {
            CoreFrames.UIFrame.Show(worldId, WorldDemo3UI).Forget();
        }
        else if (Keyboard.current.numpad6Key.wasReleasedThisFrame)
        {
            CoreFrames.UIFrame.Close(WorldDemo3UI, true, true);
        }
    }

    public async void ShowFirstScreenUI()
    {
        await CoreFrames.UIFrame.Show(UIFrameDemo.screenId, UIFrameDemo.ScreenDemo1UI, null, UIFrameDemo.ScreenDemoLoadingUI, null, null);
    }

    public async void ShowFirstWorldUI()
    {
        await CoreFrames.UIFrame.Show(UIFrameDemo.worldId, UIFrameDemo.WorldDemo1UI, null, UIFrameDemo.WorldDemoLoadingUI, null, null);
    }

    public async void PreloadFirstWorldUI()
    {
        await CoreFrames.UIFrame.Preload(UIFrameDemo.WorldDemo1UI);
    }
}
