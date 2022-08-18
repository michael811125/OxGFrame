using OxGFrame.GSIFrame;

public class GSMExample : GSM<GSMExample>
{
    public static byte none = 0;
    public static byte startupStage = 1;
    public static byte logoStage = 2;
    public static byte patchStage = 3;
    public static byte loginStage = 4;
    public static byte enterStage = 5;

    public GSMExample()
    {
        GStage gameStage;

        // 無
        gameStage = new NoneStageExample(GSMExample.none);
        this.AddGameStage(GSMExample.none, gameStage);

        // 1. 遊戲啟動階段
        gameStage = new StartupStageExample(GSMExample.startupStage);
        this.AddGameStage(GSMExample.startupStage, gameStage);

        // 2. 遊戲Logo畫面展示階段
        gameStage = new LogoStageExample(GSMExample.logoStage);
        this.AddGameStage(GSMExample.logoStage, gameStage);

        // 3. 遊戲補丁階段(App需要Manifest去熱更新; H5無需要熱更新, 所以H5在這個階段可以單純進行資源預加載)
        gameStage = new PatchStageExample(GSMExample.patchStage);
        this.AddGameStage(GSMExample.patchStage, gameStage);

        // 4. 玩家登入與創角色階段
        gameStage = new LoginStageExample(GSMExample.loginStage);
        this.AddGameStage(GSMExample.loginStage, gameStage);

        // 5. 進入遊戲階段, 並且處理Server傳送數據資料
        gameStage = new EnterStageExample(GSMExample.enterStage);
        this.AddGameStage(GSMExample.enterStage, gameStage);
    }

    /// <summary>
    /// 由主要的MonoBehaviour的Start調用 (Such as CoreSystem)
    /// </summary>
    public override void OnStart()
    {
        // 啟動第一個遊戲階段
        this.ChangeGameStage(GSMExample.startupStage);
    }

    /// <summary>
    /// 由主要的MonoBehaviour的Update調用 (Such as CoreSystem)
    /// </summary>
    /// <param name="dt"></param>
    public override void OnUpdate(float dt = 0.0f)
    {
        base.OnUpdate(dt);
    }
}

