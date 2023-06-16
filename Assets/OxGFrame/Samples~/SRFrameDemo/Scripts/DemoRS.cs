using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using OxGFrame.CoreFrame.SRFrame;

public class DemoRS : SRBase
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

    protected override void OnShow(object obj)
    {
        Debug.Log("DemoRS OnShow");
    }

    protected override void OnBind()
    {
        /**
         * Do Somethings Init Once Components and Events In Here
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
        * Do Update Once After Protocol Handle
        */
    }

    protected override void OnClose()
    {

    }
}
