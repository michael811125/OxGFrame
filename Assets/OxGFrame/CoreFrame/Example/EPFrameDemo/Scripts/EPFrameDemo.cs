using OxGFrame.CoreFrame;
using UnityEngine;

public class EPFrameDemo : MonoBehaviour
{
    public Transform container;

    public void LoadDemoPref1()
    {
        // if use prefix "res#" will load from resource else will from bundle
        var pref = CoreFrames.EPFrame.LoadWithClone<DemoEntity1>("res#Example/Entity/DemoEntity1");
        if (pref != null) pref.MyMethod();
    }

    public async void LoadDemoPref2()
    {
        // if use prefix "res#" will load from resource else will from bundle
        var pref = await CoreFrames.EPFrame.LoadWithCloneAsync<DemoEntity2>("res#Example/Entity/DemoEntity2", this.container);
        if (pref != null) pref.MyMethod();
    }
}
