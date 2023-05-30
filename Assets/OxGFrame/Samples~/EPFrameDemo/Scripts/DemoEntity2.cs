using OxGFrame.CoreFrame.EPFrame;
using UnityEngine;

public class DemoEntity2 : EPBase
{
    public override void OnInit()
    {
        Debug.Log($"<color=#FF2A20>InitThis:</color> {this.gameObject.name}");
    }

    protected override void OnBind()
    {
        Debug.Log($"<color=#FFA720>Found:</color> {this.gameObject.name} => {this.collector.GetNode("B1").name}");
        Debug.Log($"<color=#FFA720>Found:</color> {this.gameObject.name} => {this.collector.GetNode("B2").name}");
    }

    protected override void OnShow(object obj)
    {
        Debug.Log($"<color=#6FFF20>OnShow:</color> {this.gameObject.name}");
    }

    protected override void OnClose()
    {
        Debug.Log($"<color=#32FFEC>OnClose:</color> {this.gameObject.name}");
    }

    public override void OnRelease()
    {
        /*
         * OnDestroy
         */
    }

    protected override void OnUpdate(float dt)
    {
        /*
         * Update
         */
    }
    public void MyMethod()
    {
        Debug.Log($"<color=#E553FF>MyMethod:</color> {this.gameObject.name}");
    }
}
