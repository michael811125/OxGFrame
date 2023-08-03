using OxGFrame.CoreFrame;
using UnityEngine;

public static class ScenePrefs
{
    /**
     * SC = Scene (Sc)
     */

    // If use prefix "res#" will load from resource else will from bundle
    private const string _prefix = "res#";
    private const string _path = "Example/ScenePrefabs/";

    // Assets
    public static readonly string DemoSC = $"{_prefix}{_path}DemoSC";

    // Group Id
    public const int Id = 1;
}

public static class ResPrefs
{
    /**
     * RS = Resource (Res)
     */

    // If use prefix "res#" will load from resource else will from bundle
    private const string _prefix = "res#";
    private const string _path = "Example/ResPrefabs/";

    // Assets
    public static readonly string DemoRS = $"{_prefix}{_path}DemoRS";

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
        await CoreFrames.SRFrame.Preload(ScenePrefs.DemoSC);
    }

    public async void ShowDemoSC()
    {
        await CoreFrames.SRFrame.Show(ScenePrefs.Id, ScenePrefs.DemoSC);
    }

    public async void ShowDemoSCAndSetParent()
    {
        await CoreFrames.SRFrame.Show(ScenePrefs.Id, ScenePrefs.DemoSC, null, null, null, this.parent);
    }

    public void HideDemoSC()
    {
        CoreFrames.SRFrame.Hide(ScenePrefs.DemoSC);
    }

    public void RevealDemoSC()
    {
        CoreFrames.SRFrame.Reveal(ScenePrefs.DemoSC);
    }

    public void CloseDemoSC()
    {
        CoreFrames.SRFrame.Close(ScenePrefs.DemoSC);
    }

    public void CloseWithDestroyDemoSC()
    {
        CoreFrames.SRFrame.Close(ScenePrefs.DemoSC, false, true);
    }
    #endregion

    #region Res
    public async void PreloadDemoRS()
    {
        await CoreFrames.SRFrame.Preload(ResPrefs.DemoRS);
    }

    public async void ShowDemoRS()
    {
        await CoreFrames.SRFrame.Show(ResPrefs.Id, ResPrefs.DemoRS);
    }

    public void HideDemoRS()
    {
        CoreFrames.SRFrame.Hide(ResPrefs.DemoRS);
    }

    public void RevealDemoRS()
    {
        CoreFrames.SRFrame.Reveal(ResPrefs.DemoRS);
    }

    public void CloseDemoRS()
    {
        CoreFrames.SRFrame.Close(ResPrefs.DemoRS);
    }

    public void CloseWithDestroyDemoRS()
    {
        CoreFrames.SRFrame.Close(ResPrefs.DemoRS, false, true);
    }
    #endregion

    public void HideAll()
    {
        CoreFrames.SRFrame.HideAll(ScenePrefs.Id);
        CoreFrames.SRFrame.HideAll(ResPrefs.Id);
    }

    public void ReveaAll()
    {
        CoreFrames.SRFrame.RevealAll(ScenePrefs.Id);
        CoreFrames.SRFrame.RevealAll(ResPrefs.Id);
    }

    public void CloseAll()
    {
        CoreFrames.SRFrame.CloseAll(ScenePrefs.Id);
        CoreFrames.SRFrame.CloseAll(ResPrefs.Id);
    }

    public void CloseAllWithDestroy()
    {
        CoreFrames.SRFrame.CloseAll(ScenePrefs.Id, true, true);
        CoreFrames.SRFrame.CloseAll(ResPrefs.Id, true, true);
    }
}
