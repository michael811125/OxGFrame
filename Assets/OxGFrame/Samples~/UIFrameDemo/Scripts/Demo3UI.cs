using UnityEngine;
using OxGFrame.CoreFrame.UIFrame;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

public class Demo3UI : UIBase
{
    // Use _Node@XXX to Bind

    #region Binding Components
    protected Image _viewImg;

    /// <summary>
    /// Auto Binding Section
    /// </summary>
    protected override void OnAutoBind()
    {
        base.OnAutoBind();
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

    protected override void OnShow(object obj)
    {
        Debug.Log(string.Format("{0} - Do Somethings OnShow.", this.gameObject.name));
    }

    protected override void OnBind()
    {
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

    protected override void ShowAnimation(AnimationEnd animationEnd)
    {
        animationEnd(); // Must call if animation end
    }

    protected override void HideAnimation(AnimationEnd animationEnd)
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
}
