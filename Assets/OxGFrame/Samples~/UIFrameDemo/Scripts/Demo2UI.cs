using UnityEngine;
using OxGFrame.CoreFrame.UIFrame;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using OxGFrame.CoreFrame;

public class Demo2UI : UIBase
{
    private Image myImage;
    private Button oepnBtn;

    public override void OnInit()
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
        Debug.Log(string.Format("{0} - Do Somethings OnShow.", this.gameObject.name));
    }

    protected override void OnBind()
    {
        this.myImage = this.collector.GetNode("Image2")?.GetComponent<Image>();
        if (this.myImage != null) Debug.Log(string.Format("Binded GameObject: {0}", this.myImage.name));

        this.oepnBtn = this.collector.GetNode("OpenBtn")?.GetComponent<Button>();
        if (this.oepnBtn != null) Debug.Log(string.Format("Binded GameObject: {0}", this.oepnBtn.name));

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

    protected override void ShowAnime(AnimeEndCb animeEndCb)
    {
        animeEndCb(); // Must Keep, Because Parent Already Set AnimeCallback
    }

    protected override void HideAnime(AnimeEndCb animeEndCb)
    {
        animeEndCb(); // Must Keep, Because Parent Already Set AnimeCallback
    }

    protected override void OnClose()
    {
        Debug.Log(string.Format("{0} - Do Somethings OnClose.", this.gameObject.name));
    }

    protected override void OnHide()
    {
        Debug.Log(string.Format("{0} - Do Somethings OnHide.", this.gameObject.name));
    }

    private async void _ShowDemoPopup3UI()
    {
        if (this.uiSetting.canvasName == UIFrameDemo.CanvasCamera)
            await CoreFrames.UIFrame.Show(Screen.Id, Screen.Demo3UI, null, Screen.DemoLoadingUI, null, null);
        else if (this.uiSetting.canvasName == UIFrameDemo.CanvasWorld)
            await CoreFrames.UIFrame.Show(World.Id, World.Demo3UI, null, World.DemoLoadingUI, null, null);
    }
}
