using UnityEngine;
using Cysharp.Threading.Tasks;
using OxGFrame.CoreFrame.SRFrame;

public class MeshResSR : SRBase
{
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
        Debug.Log("MeshResSR OnShow");
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

    public Mesh GetCubeMesh()
    {
        return this.collector.GetNode("Cube").GetComponent<MeshFilter>().mesh;
    }

    public Mesh GetSphereMesh()
    {
        return this.collector.GetNode("Sphere").GetComponent<MeshFilter>().mesh;
    }
}
