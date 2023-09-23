using Cysharp.Threading.Tasks;
using OxGFrame.CoreFrame;
using OxGFrame.CoreFrame.CPFrame;
using UnityEngine;

public class Demo2CP : CPBase
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

    public int price = 0;

    #region Factory Mode
    public static async UniTask<Demo2CP> CloneParsedDemo2CP(int price, Transform parent = null)
    {
        var cp = await CoreFrames.CPFrame.LoadWithCloneAsync<Demo2CP>(TplPrefs.Demo2CP, parent);
        if (cp.ParsingDemo2CP(price)) return cp;
        return null;
    }
    #endregion

    #region Parsing Methods
    public bool ParsingDemo2CP(int price)
    {
        // Set price value
        this.price = price;

        /**
         * Template CP parsing ...
         */

        return true;
    }
    #endregion

    public void PrintPrice()
    {
        Debug.Log($"<color=#E553FF>Item Price:</color> {this.price}");
    }
}
