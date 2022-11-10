using OxGFrame.GSIFrame;
using Cysharp.Threading.Tasks;

public class NoneStageExample : GameStageBase
{
    public NoneStageExample(byte gstId) : base(gstId)
    {
    }

    public override void ResetStage()
    {
        /* Do Somthing Reset in here */
    }

    public async override UniTask InitStage()
    {
        /* Do Somthing Init in here */
    }

    public override void UpdateStage(float dt = 0.0f)
    {
        /* Do Somthing Update in here */
    }

    public override void ReleaseStage()
    {
        /* Do Somthing Release in here */
    }
}
