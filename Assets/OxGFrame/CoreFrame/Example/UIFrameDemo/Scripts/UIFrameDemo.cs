using UnityEngine;
using OxGFrame.CoreFrame.UIFrame;
using UnityEngine.InputSystem;
using OxGFrame.CoreFrame.UMT;

public class UIFrameDemo : MonoBehaviour
{
    public static string ScreenDemo1UI = "Example/UI/ScreenUI/Demo1UI";
    public static string ScreenDemo2UI = "Example/UI/ScreenUI/Demo2UI";
    public static string ScreenDemo3UI = "Example/UI/ScreenUI/Demo3UI";
    public static string ScreenDemoLoadingUI = "Example/UI/ScreenUI/DemoLoadingUI";
    public static int screenId = 1;

    public static string WorldDemo1UI = "Example/UI/WorldUI/Demo1UI";
    public static string WorldDemo2UI = "Example/UI/WorldUI/Demo2UI";
    public static string WorldDemo3UI = "Example/UI/WorldUI/Demo3UI";
    public static string WorldDemoLoadingUI = "Example/UI/WorldUI/DemoLoadingUI";
    public static int worldId = 2;

    private void Awake()
    {
        UIManager.GetInstance();
    }

    private void Start()
    {
        //BundleDistributor.GetInstance().Check();
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
            UIManager.GetInstance().HideAll(UIFrameDemo.worldId);
        }
        else if (Keyboard.current.numpad3Key.wasReleasedThisFrame)
        {
            UIManager.GetInstance().RevealAll(UIFrameDemo.worldId);
        }
        else if (Keyboard.current.numpad4Key.wasReleasedThisFrame)
        {
            UIManager.GetInstance().CloseAll(true, true);
        }
    }

    public async void ShowFirstScreenUI()
    {
        var runThread = new RunThread();
        new System.Threading.Thread(runThread.Run).Start();

        //await UIManager.GetInstance().Show(1, UIFrameDemo.Demo1UI, null, UIFrameDemo.DemoLoadingUI, null);

        // Bundle
        //await UIManager.GetInstance().Show(1, "coreframe/ui/Demo1UI", "Demo1UI", null, "coreframe/ui/DemoLoadingUI", "DemoLoadingUI");
    }

    public async void ShowFirstWorldUI()
    {
        await UIManager.GetInstance().Show(UIFrameDemo.worldId, UIFrameDemo.WorldDemo1UI, null, UIFrameDemo.WorldDemoLoadingUI, null);

        // Bundle
        //await UIManager.GetInstance().Show(1, "coreframe/ui/Demo1UI", "Demo1UI", null, "coreframe/ui/DemoLoadingUI", "DemoLoadingUI");
    }

    public async void PreloadFirstWorldUI()
    {
        await UIManager.GetInstance().Preload(UIFrameDemo.WorldDemo1UI);
    }
}

public class RunThread
{
    public void Run()
    {
        //this.Open();
        UnityMainThread.worker.AddJob(this.Open);
    }

    public async void Open()
    {
        try
        {
            await UIManager.GetInstance().Show(UIFrameDemo.screenId, UIFrameDemo.ScreenDemo1UI, null, UIFrameDemo.ScreenDemoLoadingUI, null);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
        }
    }
}
