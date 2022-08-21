using UnityEngine;
using OxGFrame.CoreFrame.UIFrame;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using OxGFrame.CoreFrame.GSFrame;

public class DemoPopup3UI : UIBase
{
    private Image myImage;
    private Button loadSceneBtn;

    public override void BeginInit()
    {
        //this.uiType = new UINode(CoreFrame.UIFrame.NodeType.Popup);
        //this.maskType = new MaskType(UIMaskOpacity.OpacityHigh);
        //this.isCloseAndDestroy = false;
    }

    protected override async UniTask OpenSub()
    {
        /**
        * Open Sub With Async
        */
    }

    protected override void CloseSub()
    {
        /**
        * Close Sub
        */
    }

    protected override void OnShow(object obj)
    {
        // Custom MaskEventFunc
        //this.maskEventFunc = () =>
        //{

        //};

        Debug.Log(string.Format("{0} - Do Something OnShow.", this.gameObject.name));
    }
    protected override void InitOnceComponents()
    {
        this.myImage = this.collector.GetNode("Image3")?.GetComponent<Image>();
        if (this.myImage != null) Debug.Log(string.Format("Binded GameObject: {0}", this.myImage.gameObject.name));

        this.loadSceneBtn = this.collector.GetNode("LoadSceneBtn")?.GetComponent<Button>();
        if (this.loadSceneBtn != null) Debug.Log(string.Format("Binded GameObject: {0}", this.loadSceneBtn.gameObject.name));
    }

    protected override void InitOnceEvents()
    {
        this.loadSceneBtn.onClick.AddListener(this._ShowDemoScene);
    }

    protected override void OnUpdate(float dt)
    {
        /**
         * Do Update Per FrameRate
         */
    }

    public override void OnUpdateOnceAfterProtocol(int funcId = 0)
    {
        /**
        * Do Update Once After Protocol Handle
        */
    }

    protected override void ShowAnim(AnimEndCb animEndCb)
    {
        animEndCb(); // Must Keep, Because Parent Already Set AnimCallback
    }

    protected override void HideAnim(AnimEndCb animEndCb)
    {
        animEndCb(); // Must Keep, Because Parent Already Set AnimCallback
    }

    protected override void OnClose()
    {
        Debug.Log(string.Format("{0} - Do Something OnClose.", this.gameObject.name));
    }

    protected override void OnHide()
    {
        Debug.Log(string.Format("{0} - Do Something OnHide.", this.gameObject.name));
    }

    private async void _ShowDemoScene()
    {
        if (!GSManager.GetInstance().CheckIsShowing(GSFrameDemo.DemoSC)) await GSManager.GetInstance().Show(1, GSFrameDemo.DemoSC);
        else GSManager.GetInstance().Close(GSFrameDemo.DemoSC);

        // Bundle
        //if (GSManager.GetInstance().CheckIsShowing(GSFrameDemo.DemoSC)) await GSManager.GetInstance().Show("coreframe/scene/DemoSC", "DemoSC", null, "coreframe/ui/DemoLoadingUI", "DemoLoadingUI");
        //else GSManager.GetInstance().Close("DemoSC");
    }
}
