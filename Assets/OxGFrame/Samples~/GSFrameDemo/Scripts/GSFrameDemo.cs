using OxGFrame.CoreFrame;
using UnityEngine;

public static class GameScene
{
    // If use prefix "res#" will load from resource else will from bundle
    private const string _prefix = "res#";

    // Assets
    public static readonly string DemoSC = $"{_prefix}Example/Scene/DemoSC";

    // Group Id
    public const int Id = 1;
}

public static class Res
{
    // If use prefix "res#" will load from resource else will from bundle
    private const string _prefix = "res#";

    // Assets
    public static readonly string DemoRS = $"{_prefix}Example/Res/DemoRS";

    // Group Id
    public const int Id = 2;
}

public class GSFrameDemo : MonoBehaviour
{
    public Transform parent;

    private void Awake()
    {
        // If Init instance can more efficiency
        CoreFrames.GSFrame.InitInstance();
    }

    #region Scene
    public async void PreloadDemoSC()
    {
        await CoreFrames.GSFrame.Preload(GameScene.DemoSC);
    }

    public async void ShowDemoSC()
    {
        await CoreFrames.GSFrame.Show(GameScene.Id, GameScene.DemoSC);
    }

    public async void ShowDemoSCAndSetParent()
    {
        await CoreFrames.GSFrame.Show(GameScene.Id, GameScene.DemoSC, null, null, null, this.parent);
    }

    public void HideDemoSC()
    {
        CoreFrames.GSFrame.Hide(GameScene.DemoSC);
    }

    public void RevealDemoSC()
    {
        CoreFrames.GSFrame.Reveal(GameScene.DemoSC);
    }

    public void CloseDemoSC()
    {
        CoreFrames.GSFrame.Close(GameScene.DemoSC);
    }

    public void CloseWithDestroyDemoSC()
    {
        CoreFrames.GSFrame.Close(GameScene.DemoSC, false, true);
    }
    #endregion

    #region Res
    public async void PreloadDemoRS()
    {
        await CoreFrames.GSFrame.Preload(Res.DemoRS);
    }

    public async void ShowDemoRS()
    {
        await CoreFrames.GSFrame.Show(Res.Id, Res.DemoRS);
    }

    public void HideDemoRS()
    {
        CoreFrames.GSFrame.Hide(Res.DemoRS);
    }

    public void RevealDemoRS()
    {
        CoreFrames.GSFrame.Reveal(Res.DemoRS);
    }

    public void CloseDemoRS()
    {
        CoreFrames.GSFrame.Close(Res.DemoRS);
    }

    public void CloseWithDestroyDemoRS()
    {
        CoreFrames.GSFrame.Close(Res.DemoRS, false, true);
    }
    #endregion

    public void HideAll()
    {
        CoreFrames.GSFrame.HideAll(GameScene.Id);
        CoreFrames.GSFrame.HideAll(Res.Id);
    }

    public void ReveaAll()
    {
        CoreFrames.GSFrame.RevealAll(GameScene.Id);
        CoreFrames.GSFrame.RevealAll(Res.Id);
    }

    public void CloseAll()
    {
        CoreFrames.GSFrame.CloseAll(GameScene.Id);
        CoreFrames.GSFrame.CloseAll(Res.Id);
    }

    public void CloseAllWithDestroy()
    {
        CoreFrames.GSFrame.CloseAll(GameScene.Id, false, true);
        CoreFrames.GSFrame.CloseAll(Res.Id, false, true);
    }
}
