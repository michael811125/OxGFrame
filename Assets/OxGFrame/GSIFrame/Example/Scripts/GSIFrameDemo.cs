using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GSIFrameDemo : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameStageManagerExample.GetInstance().OnStart();
    }

    // Update is called once per frame
    void Update()
    {
        GameStageManagerExample.GetInstance().OnUpdate(Time.deltaTime);

        if (Keyboard.current.numpad1Key.wasReleasedThisFrame)
        {
            GameStageManagerExample.GetInstance().ChangeGameStage(GameStageManagerExample.startupStage);
        }

        if (Keyboard.current.numpad2Key.wasReleasedThisFrame)
        {
            GameStageManagerExample.GetInstance().ChangeGameStage(GameStageManagerExample.logoStage);
        }

        if (Keyboard.current.numpad3Key.wasReleasedThisFrame)
        {
            GameStageManagerExample.GetInstance().ChangeGameStage(GameStageManagerExample.patchStage);
        }

        if (Keyboard.current.numpad4Key.wasReleasedThisFrame)
        {
            GameStageManagerExample.GetInstance().ChangeGameStage(GameStageManagerExample.loginStage);
        }

        if (Keyboard.current.numpad5Key.wasReleasedThisFrame)
        {
            GameStageManagerExample.GetInstance().ChangeGameStage(GameStageManagerExample.enterStage);
        }
    }
}
