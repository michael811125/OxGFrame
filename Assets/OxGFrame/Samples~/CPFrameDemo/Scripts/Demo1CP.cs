using OxGFrame.CoreFrame.CPFrame;
using UnityEngine;

public class Demo1CP : CPBase
{
    // Use ~Node@XXX to Bind

    #region Binding Components
    protected GameObject _b1;
    protected GameObject[] _b2s;

    /// <summary>
    /// Auto Binding Section
    /// </summary>
    protected override void OnAutoBind()
    {
        base.OnAutoBind();
        this._b1 = this.collector.GetNode("B1");
        this._b2s = this.collector.GetNodes("B2");
    }
    #endregion

    public override void OnCreate()
    {
        Debug.Log($"<color=#FF2A20>InitThis:</color> {this.gameObject.name}");
    }

    protected override void OnBind()
    {
        // Single Bind
        Debug.Log($"<color=#FFA720>Found:</color> {this.gameObject.name} => {_b1.name}");

        // Multi Bind (Same Name)
        Debug.Log($"<color=#FFA720>Found Array Binds:</color> {this.gameObject.name} => {_b2s.Length}");
        foreach (var node in _b2s)
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
