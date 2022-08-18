using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GSIFrameDemo : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GSIExample.GetInstance().OnStart();
    }

    // Update is called once per frame
    void Update()
    {
        GSIExample.GetInstance().OnUpdate(Time.deltaTime);

        if (Keyboard.current.numpad1Key.wasReleasedThisFrame)
        {
            GSIExample.GetInstance().ChangeGameStage(GSIExample.startupStage);
        }

        if (Keyboard.current.numpad2Key.wasReleasedThisFrame)
        {
            GSIExample.GetInstance().ChangeGameStage(GSIExample.logoStage);
        }

        if (Keyboard.current.numpad3Key.wasReleasedThisFrame)
        {
            GSIExample.GetInstance().ChangeGameStage(GSIExample.patchStage);
        }

        if (Keyboard.current.numpad4Key.wasReleasedThisFrame)
        {
            GSIExample.GetInstance().ChangeGameStage(GSIExample.loginStage);
        }

        if (Keyboard.current.numpad5Key.wasReleasedThisFrame)
        {
            GSIExample.GetInstance().ChangeGameStage(GSIExample.enterStage);
        }
    }
}
