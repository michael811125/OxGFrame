using OxGFrame.CoreFrame.GSFrame;
using UnityEngine;

public class GSFrameDemo : MonoBehaviour
{
    public static string DemoSC = "Example/Scene/DemoSC";
    public static string DemoRS = "Example/Res/DemoRS";

    public static int scId = 1;
    public static int rsId = 2;

    #region Scene
    public async void PreloadDemoSC()
    {
        await GSManager.GetInstance().Preload(GSFrameDemo.DemoSC);
    }

    public async void ShowDemoSC()
    {
        await GSManager.GetInstance().Show(scId, GSFrameDemo.DemoSC);
    }

    public void HideDemoSC()
    {
        GSManager.GetInstance().Hide(GSFrameDemo.DemoSC);
    }

    public void RevealDemoSC()
    {
        GSManager.GetInstance().Reveal(GSFrameDemo.DemoSC);
    }

    public void CloseDemoSC()
    {
        GSManager.GetInstance().Close(GSFrameDemo.DemoSC);
    }

    public void CloseWithDestroyDemoSC()
    {
        GSManager.GetInstance().Close(GSFrameDemo.DemoSC, false, true);
    }
    #endregion

    #region Res
    public async void PreloadDemoRS()
    {
        await GSManager.GetInstance().Preload(GSFrameDemo.DemoRS);
    }

    public async void ShowDemoRS()
    {
        await GSManager.GetInstance().Show(rsId, GSFrameDemo.DemoRS);
    }

    public void HideDemoRS()
    {
        GSManager.GetInstance().Hide(GSFrameDemo.DemoRS);
    }

    public void RevealDemoRS()
    {
        GSManager.GetInstance().Reveal(GSFrameDemo.DemoRS);
    }

    public void CloseDemoRS()
    {
        GSManager.GetInstance().Close(GSFrameDemo.DemoRS);
    }

    public void CloseWithDestroyDemoRS()
    {
        GSManager.GetInstance().Close(GSFrameDemo.DemoRS, false, true);
    }
    #endregion

    public void HideAll()
    {
        GSManager.GetInstance().HideAll(scId);
        GSManager.GetInstance().HideAll(rsId);
    }

    public void ReveaAll()
    {
        GSManager.GetInstance().RevealAll(scId);
        GSManager.GetInstance().RevealAll(rsId);
    }

    public void CloseAll()
    {
        GSManager.GetInstance().CloseAll(scId);
        GSManager.GetInstance().CloseAll(rsId);
    }

    public void CloseAllWithDestroy()
    {
        GSManager.GetInstance().CloseAll(scId, false, true);
        GSManager.GetInstance().CloseAll(rsId, false, true);
    }
}
