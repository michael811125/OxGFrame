using OxGFrame.CoreFrame.CPFrame;
using UnityEngine;

public class DemoCP1 : CPBase
{
    public override void OnInit()
    {
        Debug.Log($"<color=#FF2A20>InitThis:</color> {this.gameObject.name}");
    }

    protected override void OnBind()
    {
        // Single Bind
        Debug.Log($"<color=#FFA720>Found:</color> {this.gameObject.name} => {this.collector.GetNode("B1").name}");

        // Multi Bind (Same Name)
        Debug.Log($"<color=#FFA720>Found Array Binds:</color> {this.gameObject.name} => {this.collector.GetNodes("B2").Length}");
        foreach (var node in this.collector.GetNodes("B2"))
        {
            Debug.Log($"<color=#FFA720>Found:</color> {this.gameObject.name} => {node.name}");
        }
    }

    protected override void OnShow()
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
