using UnityEngine;
using OxGFrame.CoreFrame.UIFrame;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using OxGFrame.CoreFrame;

public class Demo1UI : UIBase
{
    private Image myImage;
    private Button oepnBtn;
    private Image myImage2;

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

    private string _msg = null;
    protected override void OnShow(object obj)
    {
        if (obj != null) this._msg = obj as string;
        Debug.Log(string.Format("{0} Do Something OnShow.", this.gameObject.name));
    }

    protected override void OnBind()
    {
        this.myImage = this.collector.GetNode("Image1")?.GetComponent<Image>();
        if (this.myImage != null) Debug.Log(string.Format("Binded GameObject: {0}", this.myImage.name));

        this.oepnBtn = this.collector.GetNode("OpenBtn")?.GetComponent<Button>();
        if (this.oepnBtn != null) Debug.Log(string.Format("Binded GameObject: {0}", this.oepnBtn.name));

        this.oepnBtn.onClick.AddListener(this._ShowDemoPopup2UI);
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
        Debug.Log($"UI: {this.gameObject.name}, Check Data: {this._msg}");

        animeEndCb(); // Must Keep, Because Parent Already Set AnimCallback
    }

    protected override void HideAnime(AnimeEndCb animeEndCb)
    {
        animeEndCb(); // Must Keep, Because Parent Already Set AnimCallback
    }

    protected override void OnClose()
    {
        Debug.Log(string.Format("{0} - Do Something OnClose.", this.gameObject.name));
    }

    protected override void OnHide()
    {
        Debug.Log(string.Format("{0} - Do Something OnHide.", this.gameObject.name));
    }

    private async void _ShowDemoPopup2UI()
    {
        if (this.uiSetting.canvasName == UIFrameDemo.CanvasCamera)
            await CoreFrames.UIFrame.Show(Screen.Id, Screen.Demo2UI, null, Screen.DemoLoadingUI, null, null);
        else if (this.uiSetting.canvasName == UIFrameDemo.CanvasWorld)
            await CoreFrames.UIFrame.Show(World.Id, World.Demo2UI, null, World.DemoLoadingUI, null, null);
    }
}
