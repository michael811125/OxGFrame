using OxGFrame.CoreFrame;
using UnityEngine;

public static class ScenePrefab
{
    // If use prefix "res#" will load from resource else will from bundle
    private const string _prefix = "res#";

    // Assets
    public static readonly string DemoSC = $"{_prefix}Example/ScenePrefabs/DemoSC";

    // Group Id
    public const int Id = 1;
}

public static class ResPrefab
{
    // If use prefix "res#" will load from resource else will from bundle
    private const string _prefix = "res#";

    // Assets
    public static readonly string DemoRS = $"{_prefix}Example/ResPrefabs/DemoRS";

    // Group Id
    public const int Id = 2;
}

public class SRFrameDemo : MonoBehaviour
{
    public Transform parent;

    private void Awake()
    {
        // If Init instance can more efficiency
        CoreFrames.SRFrame.InitInstance();
    }

    #region Scene
    public async void PreloadDemoSC()
    {
        await CoreFrames.SRFrame.Preload(ScenePrefab.DemoSC);
    }

    public async void ShowDemoSC()
    {
        await CoreFrames.SRFrame.Show(ScenePrefab.Id, ScenePrefab.DemoSC);
    }

    public async void ShowDemoSCAndSetParent()
    {
        await CoreFrames.SRFrame.Show(ScenePrefab.Id, ScenePrefab.DemoSC, null, null, null, this.parent);
    }

    public void HideDemoSC()
    {
        CoreFrames.SRFrame.Hide(ScenePrefab.DemoSC);
    }

    public void RevealDemoSC()
    {
        CoreFrames.SRFrame.Reveal(ScenePrefab.DemoSC);
    }

    public void CloseDemoSC()
    {
        CoreFrames.SRFrame.Close(ScenePrefab.DemoSC);
    }

    public void CloseWithDestroyDemoSC()
    {
        CoreFrames.SRFrame.Close(ScenePrefab.DemoSC, false, true);
    }
    #endregion

    #region Res
    public async void PreloadDemoRS()
    {
        await CoreFrames.SRFrame.Preload(ResPrefab.DemoRS);
    }

    public async void ShowDemoRS()
    {
        await CoreFrames.SRFrame.Show(ResPrefab.Id, ResPrefab.DemoRS);
    }

    public void HideDemoRS()
    {
        CoreFrames.SRFrame.Hide(ResPrefab.DemoRS);
    }

    public void RevealDemoRS()
    {
        CoreFrames.SRFrame.Reveal(ResPrefab.DemoRS);
    }

    public void CloseDemoRS()
    {
        CoreFrames.SRFrame.Close(ResPrefab.DemoRS);
    }

    public void CloseWithDestroyDemoRS()
    {
        CoreFrames.SRFrame.Close(ResPrefab.DemoRS, false, true);
    }
    #endregion

    public void HideAll()
    {
        CoreFrames.SRFrame.HideAll(ScenePrefab.Id);
        CoreFrames.SRFrame.HideAll(ResPrefab.Id);
    }

    public void ReveaAll()
    {
        CoreFrames.SRFrame.RevealAll(ScenePrefab.Id);
        CoreFrames.SRFrame.RevealAll(ResPrefab.Id);
    }

    public void CloseAll()
    {
        CoreFrames.SRFrame.CloseAll(ScenePrefab.Id);
        CoreFrames.SRFrame.CloseAll(ResPrefab.Id);
    }

    public void CloseAllWithDestroy()
    {
        CoreFrames.SRFrame.CloseAll(ScenePrefab.Id, false, true);
        CoreFrames.SRFrame.CloseAll(ResPrefab.Id, false, true);
    }
}
