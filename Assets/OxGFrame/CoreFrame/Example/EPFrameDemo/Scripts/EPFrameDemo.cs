using OxGFrame.CoreFrame.EPFrame;
using UnityEngine;

public class EPFrameDemo : MonoBehaviour
{
    public Transform container;

    public async void LoadDemoPref1()
    {
        var pref = await EPManager.GetInstance().LoadWithClone<DemoEntity1>("Example/Entity/DemoEntity1");
        if (pref != null) pref.MyMethod();
    }

    public async void LoadDemoPref2()
    {
        var pref = await EPManager.GetInstance().LoadWithClone<DemoEntity2>("Example/Entity/DemoEntity2", this.container);
        if (pref != null) pref.MyMethod();
    }
}
