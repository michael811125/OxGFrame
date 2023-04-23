using UnityEngine;
using OxGFrame.CoreFrame.UIFrame;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using OxGFrame.CoreFrame;

public class Demo3UI : UIBase
{
    private Image myImage;
    private Button loadSceneBtn;

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

    private async void _ShowDemoScene()
    {
        if (!CoreFrames.GSFrame.CheckIsShowing(GSFrameDemo.DemoSC)) await CoreFrames.GSFrame.Show(1, GSFrameDemo.DemoSC);
        else CoreFrames.GSFrame.Close(GSFrameDemo.DemoSC);
    }
}
