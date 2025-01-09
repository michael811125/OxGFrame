using UnityEngine;
using OxGFrame.CoreFrame.UIFrame;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using OxGFrame.CoreFrame;

public class Demo1UI : UIBase
{
    // Use _Node@XXX to Bind

    #region Binding Components
    private Button _openBtn;
    [HideInInspector]
    public Image decorImg;
    [SerializeField]
    protected Image _viewImg;

    /// <summary>
    /// Auto Binding Section
    /// </summary>
    protected override void OnAutoBind()
    {
        base.OnAutoBind();
        this._openBtn = this.collector.GetNodeComponent<Button>("Open*Btn");
        this.decorImg = this.collector.GetNodeComponent<Image>("Decor*Img");
        this._viewImg = this.collector.GetNodeComponent<Image>("View*Img");
    }
    #endregion

    public override void OnCreate()
    {
    }

    protected override async UniTask OnPreShow()
    {
        /**
        * Open Sub With Async
        */
    }

    protected override void OnPreClose()
    {
        /**
        * Close Sub
        */
    }

    private string _msg = null;
    protected override void OnShow(object obj)
    {
        if (obj != null) this._msg = obj as string;
        Debug.Log(string.Format("{0} Do Somethings OnShow.", this.gameObject.name));
    }

    protected override void OnBind()
    {
        this._openBtn.onClick.AddListener(this._ShowDemoPopup2UI);
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

    protected override void OnShowAnimation(AnimationEnd animationEnd)
    {
        Debug.Log($"UI: {this.gameObject.name}, Check Data: {this._msg}");

        animationEnd(); // Must call if animation end
    }

    protected override void OnCloseAnimation(AnimationEnd animationEnd)
    {
        animationEnd(); // Must call if animation end
    }

    protected override void OnClose()
    {
        Debug.Log(string.Format("{0} - Do Somethings OnClose.", this.gameObject.name));
    }

    protected override void OnHide()
    {
        Debug.Log(string.Format("{0} - Do Somethings OnHide.", this.gameObject.name));
    }

    private async void _ShowDemoPopup2UI()
    {
        if (this.uiSetting.canvasName == UIFrameDemo.CanvasCamera)
            await CoreFrames.UIFrame.Show(ScreenUIs.Id, ScreenUIs.Demo2UI, null, ScreenUIs.DemoLoadingUI, 0);
        else if (this.uiSetting.canvasName == UIFrameDemo.CanvasWorld)
            await CoreFrames.UIFrame.Show(WorldUIs.Id, WorldUIs.Demo2UI, null, WorldUIs.DemoLoadingUI, 0);
    }
}
