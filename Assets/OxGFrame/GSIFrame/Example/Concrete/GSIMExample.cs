using GSIFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GSIMExample : GSIM
{
    #region 建議單例
    private static GSIMExample _instance = null;
    public static GSIMExample GetInstance()
    {
        if (_instance == null) _instance = new GSIMExample();
        return _instance;
    }
    #endregion

    public static byte none = 0;
    public static byte startupStage = 1;
    public static byte logoStage = 2;
    public static byte patchStage = 3;
    public static byte loginStage = 4;
    public static byte enterStage = 5;

    public GSIMExample()
    {
        GStage gameStage;

        // 無
        gameStage = new NoneStageExample(GSIMExample.none);
        this.AddGameStage(GSIMExample.none, gameStage);

        // 1. 遊戲啟動階段
        gameStage = new StartupStageExample(GSIMExample.startupStage);
        this.AddGameStage(GSIMExample.startupStage, gameStage);

        // 2. 遊戲Logo畫面展示階段
        gameStage = new LogoStageExample(GSIMExample.logoStage);
        this.AddGameStage(GSIMExample.logoStage, gameStage);

        // 3. 遊戲補丁階段(App需要Manifest去熱更新; H5無需要熱更新, 所以H5在這個階段可以單純進行資源預加載)
        gameStage = new PatchStageExample(GSIMExample.patchStage);
        this.AddGameStage(GSIMExample.patchStage, gameStage);

        // 4. 玩家登入與創角色階段
        gameStage = new LoginStageExample(GSIMExample.loginStage);
        this.AddGameStage(GSIMExample.loginStage, gameStage);

        // 5. 進入遊戲階段, 並且處理Server傳送數據資料
        gameStage = new EnterStageExample(GSIMExample.enterStage);
        this.AddGameStage(GSIMExample.enterStage, gameStage);
    }

    /// <summary>
    /// 由主要的MonoBehaviour的Start調用 (Such as CoreSystem)
    /// </summary>
    public override void OnStart()
    {
        // 啟動第一個遊戲階段
        this.ChangeGameStage(GSIMExample.startupStage);
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

