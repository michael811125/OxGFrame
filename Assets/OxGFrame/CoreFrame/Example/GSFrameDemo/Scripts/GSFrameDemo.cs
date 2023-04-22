using OxGFrame.CoreFrame;
using UnityEngine;

public class GSFrameDemo : MonoBehaviour
{
    // if use prefix "res#" will load from resource else will from bundle
    public static string DemoSC = "res#Example/Scene/DemoSC";
    public static string DemoRS = "res#Example/Res/DemoRS";

    public static int scId = 1;
    public static int rsId = 2;

    public Transform parent;

    #region Scene
    public async void PreloadDemoSC()
    {
        await CoreFrames.GSFrame.Preload(GSFrameDemo.DemoSC);
    }

    public async void ShowDemoSC()
    {
        await CoreFrames.GSFrame.Show(scId, GSFrameDemo.DemoSC);
    }

    public async void ShowDemoSCAndSetParent()
    {
        await CoreFrames.GSFrame.Show(scId, GSFrameDemo.DemoSC, null, null, null, this.parent);
    }


    public void HideDemoSC()
    {
        CoreFrames.GSFrame.Hide(GSFrameDemo.DemoSC);
    }

    public void RevealDemoSC()
    {
        CoreFrames.GSFrame.Reveal(GSFrameDemo.DemoSC);
    }

    public void CloseDemoSC()
    {
        CoreFrames.GSFrame.Close(GSFrameDemo.DemoSC);
    }

    public void CloseWithDestroyDemoSC()
    {
        CoreFrames.GSFrame.Close(GSFrameDemo.DemoSC, false, true);
    }
    #endregion

    #region Res
    public async void PreloadDemoRS()
    {
        await CoreFrames.GSFrame.Preload(GSFrameDemo.DemoRS);
    }

    public async void ShowDemoRS()
    {
        await CoreFrames.GSFrame.Show(rsId, GSFrameDemo.DemoRS);
    }

    public void HideDemoRS()
    {
        CoreFrames.GSFrame.Hide(GSFrameDemo.DemoRS);
    }

    public void RevealDemoRS()
    {
        CoreFrames.GSFrame.Reveal(GSFrameDemo.DemoRS);
    }

    public void CloseDemoRS()
    {
        CoreFrames.GSFrame.Close(GSFrameDemo.DemoRS);
    }

    public void CloseWithDestroyDemoRS()
    {
        CoreFrames.GSFrame.Close(GSFrameDemo.DemoRS, false, true);
    }
    #endregion

    public void HideAll()
    {
        CoreFrames.GSFrame.HideAll(scId);
        CoreFrames.GSFrame.HideAll(rsId);
    }

    public void ReveaAll()
    {
        CoreFrames.GSFrame.RevealAll(scId);
        CoreFrames.GSFrame.RevealAll(rsId);
    }

    public void CloseAll()
    {
        CoreFrames.GSFrame.CloseAll(scId);
        CoreFrames.GSFrame.CloseAll(rsId);
    }

    public void CloseAllWithDestroy()
    {
        CoreFrames.GSFrame.CloseAll(scId, false, true);
        CoreFrames.GSFrame.CloseAll(rsId, false, true);
    }
}
