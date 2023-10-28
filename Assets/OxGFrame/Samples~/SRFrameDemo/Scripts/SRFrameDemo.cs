using OxGFrame.CoreFrame;
using UnityEngine;

public static class ScenePrefs
{
    // If use prefix "res#" will load from resource else will from bundle
    private const string _prefix = "res#";
    private const string _path = "Example/ScenePrefabs/";

    // Assets
    public static readonly string CubeSceneSR = $"{_prefix}{_path}CubeSceneSR";

    // Group Id
    public const int Id = 1;
}

public static class ResPrefs
{
    // If use prefix "res#" will load from resource else will from bundle
    private const string _prefix = "res#";
    private const string _path = "Example/ResPrefabs/";

    // Assets
    public static readonly string MeshResSR = $"{_prefix}{_path}MeshResSR";

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
    public async void PreloadCubeSceneSR()
    {
        await CoreFrames.SRFrame.Preload(ScenePrefs.CubeSceneSR);
    }

    public async void ShowCubeSceneSR()
    {
        await CoreFrames.SRFrame.Show(ScenePrefs.Id, ScenePrefs.CubeSceneSR);
    }

    public async void ShowCubeSceneSRAndSetParent()
    {
        await CoreFrames.SRFrame.Show(ScenePrefs.Id, ScenePrefs.CubeSceneSR, null, null, 0, null, this.parent);
    }

    public void HideCubeSceneSR()
    {
        CoreFrames.SRFrame.Hide(ScenePrefs.CubeSceneSR);
    }

    public void RevealCubeSceneSR()
    {
        CoreFrames.SRFrame.Reveal(ScenePrefs.CubeSceneSR);
    }

    public void CloseCubeSceneSR()
    {
        CoreFrames.SRFrame.Close(ScenePrefs.CubeSceneSR);
    }

    public void CloseWithDestroyCubeSceneSR()
    {
        CoreFrames.SRFrame.Close(ScenePrefs.CubeSceneSR, false, true);
    }
    #endregion

    #region Res
    public async void PreloadMeshResSR()
    {
        await CoreFrames.SRFrame.Preload(ResPrefs.MeshResSR);
    }

    public async void ShowMeshResSR()
    {
        var sr = await CoreFrames.SRFrame.Show<MeshResSR>(ResPrefs.Id, ResPrefs.MeshResSR);
        if (sr == null) sr = CoreFrames.SRFrame.GetComponent<MeshResSR>(ResPrefs.MeshResSR);
        if (sr != null)
        {
            // Get mesh res
            Debug.Log($"Get Cube Mesh => vertexCount: {sr.GetCubeMesh().vertexCount}");
            Debug.Log($"Get Sphere Mesh => vertexCount: {sr.GetSphereMesh().vertexCount}");
        }
    }

    public void HideMeshResSR()
    {
        CoreFrames.SRFrame.Hide(ResPrefs.MeshResSR);
    }

    public void RevealMeshResSR()
    {
        CoreFrames.SRFrame.Reveal(ResPrefs.MeshResSR);
    }

    public void CloseMeshResSR()
    {
        CoreFrames.SRFrame.Close(ResPrefs.MeshResSR);
    }

    public void CloseWithDestroyMeshResSR()
    {
        CoreFrames.SRFrame.Close(ResPrefs.MeshResSR, false, true);
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
