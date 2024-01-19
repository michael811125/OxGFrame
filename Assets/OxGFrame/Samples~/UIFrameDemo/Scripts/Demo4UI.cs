using OxGFrame.CoreFrame.UIFrame;
using Cysharp.Threading.Tasks;

public class Demo4UI : UIBase
{
    // Use _Node@XXX to Bind

    public override void OnCreate()
    {
        /**
         * Do Somethings Init Once In Here
         */
    }

    protected override async UniTask OnPreShow()
    {
        /**
         * On Pre-Show With Async
         */
    }

    protected override void OnPreClose()
    {
        /**
         * On Pre-Close
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

    protected override void OnBind()
    {
        /**
         * Do Somethings Init Once Components and Events In Here (For Bind)
         */
    }

    protected override void OnShow(object obj)
    {
        /**
         * Do Somethings Init With Every Showing In Here
         */
    }

    protected override void OnClose()
    {
        /**
         * Do Somethings On Close
         */
    }

    protected override void OnReveal()
    {
        /**
         * Do Somethings On Reveal
         */
    }

    protected override void OnHide()
    {
        /**
         * Do Somethings On Hide
         */
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
         * Do Refresh Once After Data Receive
         */
    }

    public override void OnRelease()
    {
        /**
         * Do Somethings On Release (CloseAndDestroy)
         */
    }
}
