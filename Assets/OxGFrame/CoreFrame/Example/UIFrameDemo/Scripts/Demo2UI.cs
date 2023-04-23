using UnityEngine;
using OxGFrame.CoreFrame.UIFrame;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using OxGFrame.CoreFrame;

public class Demo2UI : UIBase
{
    private Image myImage;
    private Button oepnBtn;

    public override void BeginInit()
    {
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
        Debug.Log(string.Format("{0} - Do Something OnShow.", this.gameObject.name));
    }

    protected override void InitOnceComponents()
    {
        this.myImage = this.collector.GetNode("Image2")?.GetComponent<Image>();
        if (this.myImage != null) Debug.Log(string.Format("Binded GameObject: {0}", this.myImage.name));

        this.oepnBtn = this.collector.GetNode("OpenBtn")?.GetComponent<Button>();
        if (this.oepnBtn != null) Debug.Log(string.Format("Binded GameObject: {0}", this.oepnBtn.name));
    }

    protected override void InitOnceEvents()
    {
        this.oepnBtn.onClick.AddListener(this._ShowDemoPopup3UI);
    }

    protected override void OnUpdate(float dt)
    {
        /**
         * Do Update Per FrameRate
         */
    }

    public override void OnReceiveAndRefresh(object obj = null)
    {
        /**
        * Do Update Once After Protocol Handle
        */
    }

    protected override void ShowAnime(AnimeEndCb animEndCb)
    {
        animEndCb(); // Must Keep, Because Parent Already Set AnimCallback
    }

    protected override void HideAnime(AnimeEndCb animEndCb)
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

    private async void _ShowDemoPopup3UI()
    {
        if (this.uiSetting.canvasName == UIFrameDemo.canvasCamera)
            await CoreFrames.UIFrame.Show(UIFrameDemo.screenId, UIFrameDemo.ScreenDemo3UI, null, UIFrameDemo.ScreenDemoLoadingUI, null);
        else if (this.uiSetting.canvasName == UIFrameDemo.canvasWorld)
            await CoreFrames.UIFrame.Show(UIFrameDemo.worldId, UIFrameDemo.WorldDemo3UI, null, UIFrameDemo.WorldDemoLoadingUI, null);
    }
}
