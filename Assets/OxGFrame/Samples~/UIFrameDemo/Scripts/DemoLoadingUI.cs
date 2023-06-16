using OxGFrame.CoreFrame.UIFrame;
using Cysharp.Threading.Tasks;

public class DemoLoadingUI : UIBase
{
    public override void OnInit()
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

    protected override void OnBind()
    {
        /**
         * Do Somethings Init Once Components and Events In Here
         */
    }

    protected override void OnShow(object obj)
    {
        /**
         * Do Somethings Init With Every Showing In Here
         */
    }

    protected override void OnUpdate(float dt)
    {
        /**
         * Do Update Per FrameRate
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

    }

    public override void OnRelease()
    {

    }
}
