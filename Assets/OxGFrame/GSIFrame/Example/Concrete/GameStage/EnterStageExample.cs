using OxGFrame.GSIFrame;
using Cysharp.Threading.Tasks;

public class EnterStageExample : GSIBase
{
    public async override UniTask OnInit()
    {
        /* Do Somthing OnInit once in here */
    }

    public async override UniTask OnEnter()
    {
        /* Do Somthing OnEnter in here */
    }

    public override void OnUpdate(float dt = 0.0f)
    {
        /* Do Somthing OnUpdate in here */
    }

    public override void OnExit()
    {
        /* Do Somthing OnExit in here */
    }
}
