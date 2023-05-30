using OxGFrame.GSIFrame;

public class GSIManagerExample : GSIManagerBase<GSIManagerExample>
{
    public GSIManagerExample()
    {
        // none
        this.AddGameStage<NoneStageExample>();

        // 1. Startup Stage
        this.AddGameStage<StartupStageExample>();

        // 2. Logo Show Stage
        this.AddGameStage<LogoStageExample>();

        // 3. Patch Stage
        this.AddGameStage<PatchStageExample>();

        // 4. Login Stage
        this.AddGameStage<LoginStageExample>();

        // 5. Enter Stage
        this.AddGameStage<EnterStageExample>();
    }

    /// <summary>
    /// Call by Main MonoBehaviour (on start)
    /// </summary>
    public override void OnStart()
    {
        // Start first game stage
        this.ChangeGameStage<StartupStageExample>();
    }

    /// <summary>
    /// Call by Main MonoBehaviour (on update)
    /// </summary>
    /// <param name="dt"></param>
    public override void OnUpdate(float dt = 0.0f)
    {
        base.OnUpdate(dt);
    }
}

