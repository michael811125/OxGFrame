using OxGFrame.GSIFrame;

public class GameStageManagerExample : GameStageManagerBase<GameStageManagerExample>
{
    public static byte none = 0;
    public static byte startupStage = 1;
    public static byte logoStage = 2;
    public static byte patchStage = 3;
    public static byte loginStage = 4;
    public static byte enterStage = 5;

    public GameStageManagerExample()
    {
        GameStageBase gameStage;

        // 無
        gameStage = new NoneStageExample(GameStageManagerExample.none);
        this.AddGameStage(GameStageManagerExample.none, gameStage);

        // 1. 遊戲啟動階段
        gameStage = new StartupStageExample(GameStageManagerExample.startupStage);
        this.AddGameStage(GameStageManagerExample.startupStage, gameStage);

        // 2. 遊戲Logo畫面展示階段
        gameStage = new LogoStageExample(GameStageManagerExample.logoStage);
        this.AddGameStage(GameStageManagerExample.logoStage, gameStage);

        // 3. 遊戲補丁階段(App需要Manifest去熱更新; H5無需要熱更新, 所以H5在這個階段可以單純進行資源預加載)
        gameStage = new PatchStageExample(GameStageManagerExample.patchStage);
        this.AddGameStage(GameStageManagerExample.patchStage, gameStage);

        // 4. 玩家登入與創角色階段
        gameStage = new LoginStageExample(GameStageManagerExample.loginStage);
        this.AddGameStage(GameStageManagerExample.loginStage, gameStage);

        // 5. 進入遊戲階段, 並且處理Server傳送數據資料
        gameStage = new EnterStageExample(GameStageManagerExample.enterStage);
        this.AddGameStage(GameStageManagerExample.enterStage, gameStage);
    }

    /// <summary>
    /// 由主要的 MonoBehaviour 的 Start 調用 (Such as CoreSystem)
    /// </summary>
    public override void OnStart()
    {
        // 啟動第一個遊戲階段
        this.ChangeGameStage(GameStageManagerExample.startupStage);
    }

    /// <summary>
    /// 由主要的 MonoBehaviour 的 Update 調用 (Such as CoreSystem)
    /// </summary>
    /// <param name="dt"></param>
    public override void OnUpdate(float dt = 0.0f)
    {
        base.OnUpdate(dt);
    }
}

