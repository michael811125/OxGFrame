using OxGFrame.CoreFrame;
using UnityEngine;

public static class Tpl
{
    // if use prefix "res#" will load from resource else will from bundle
    private const string _prefix = "res#";

    // Assets
    public static readonly string DemoEntity1 = $"{_prefix}Example/Entity/DemoEntity1";
    public static readonly string DemoEntity2 = $"{_prefix}Example/Entity/DemoEntity2";
}

public class EPFrameDemo : MonoBehaviour
{
    public Transform container;

    private void Awake()
    {
        // If Init instance can more efficiency
        CoreFrames.EPFrame.InitInstance();
    }

    public void LoadDemoPref1()
    {
        var pref = CoreFrames.EPFrame.LoadWithClone<DemoEntity1>(Tpl.DemoEntity1);
        if (pref != null) pref.MyMethod();
    }

    public async void LoadDemoPref2()
    {
        var pref = await CoreFrames.EPFrame.LoadWithCloneAsync<DemoEntity2>(Tpl.DemoEntity2, this.container);
        if (pref != null) pref.MyMethod();
    }
}
