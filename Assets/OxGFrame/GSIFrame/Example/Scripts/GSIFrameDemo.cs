using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GSIFrameDemo : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GSMExample.GetInstance().OnStart();
    }

    // Update is called once per frame
    void Update()
    {
        GSMExample.GetInstance().OnUpdate(Time.deltaTime);

        if (Keyboard.current.numpad1Key.wasReleasedThisFrame)
        {
            GSMExample.GetInstance().ChangeGameStage(GSMExample.startupStage);
        }

        if (Keyboard.current.numpad2Key.wasReleasedThisFrame)
        {
            GSMExample.GetInstance().ChangeGameStage(GSMExample.logoStage);
        }

        if (Keyboard.current.numpad3Key.wasReleasedThisFrame)
        {
            GSMExample.GetInstance().ChangeGameStage(GSMExample.patchStage);
        }

        if (Keyboard.current.numpad4Key.wasReleasedThisFrame)
        {
            GSMExample.GetInstance().ChangeGameStage(GSMExample.loginStage);
        }

        if (Keyboard.current.numpad5Key.wasReleasedThisFrame)
        {
            GSMExample.GetInstance().ChangeGameStage(GSMExample.enterStage);
        }
    }
}
