using OxGFrame.CoreFrame;
using UnityEngine;

public static class TplPrefs
{
    // if use prefix "res#" will load from resource else will from bundle
    private const string _prefix = "res#";
    private const string _path = "Example/Prefabs/";

    // Assets
    public static readonly string DemoCP1 = $"{_prefix}{_path}DemoCP1";
    public static readonly string DemoCP2 = $"{_prefix}{_path}DemoCP2";
}

public class CPFrameDemo : MonoBehaviour
{
    public Transform container;

    private void Awake()
    {
        // If Init instance can more efficiency
        CoreFrames.CPFrame.InitInstance();
    }

    public void LoadDemoPref1()
    {
        var pref = CoreFrames.CPFrame.LoadWithClone<DemoCP1>(TplPrefs.DemoCP1);
        if (pref != null) pref.MyMethod();
    }

    public async void LoadDemoPref2()
    {
        var pref = await CoreFrames.CPFrame.LoadWithCloneAsync<DemoCP2>(TplPrefs.DemoCP2, this.container);
        if (pref != null) pref.MyMethod();
    }
}
