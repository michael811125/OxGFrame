using UnityEngine;
using UnityEngine.InputSystem;

public class GSIFrameDemo : MonoBehaviour
{
    public bool forceMode = false;

    void Start()
    {
        GSIManagerExample.GetInstance().OnStart();
    }

    void Update()
    {
        GSIManagerExample.GetInstance().OnUpdate(Time.deltaTime);

        if (Keyboard.current.numpad0Key.wasReleasedThisFrame)
        {
            this.forceMode = !this.forceMode;

            if (this.forceMode) Debug.Log("Enable ForceMode");
            else Debug.Log("Disable ForceMode");
        }

        if (Keyboard.current.numpad1Key.wasReleasedThisFrame)
        {
            if (this.forceMode) GSIManagerExample.GetInstance().ChangeGameStageForce<StartupStageExample>();
            else GSIManagerExample.GetInstance().ChangeGameStage<StartupStageExample>();
        }

        if (Keyboard.current.numpad2Key.wasReleasedThisFrame)
        {
            if (this.forceMode) GSIManagerExample.GetInstance().ChangeGameStageForce<LogoStageExample>();
            else GSIManagerExample.GetInstance().ChangeGameStage<LogoStageExample>();
        }

        if (Keyboard.current.numpad3Key.wasReleasedThisFrame)
        {
            if (this.forceMode) GSIManagerExample.GetInstance().ChangeGameStageForce<PatchStageExample>();
            else GSIManagerExample.GetInstance().ChangeGameStage<PatchStageExample>();
        }

        if (Keyboard.current.numpad4Key.wasReleasedThisFrame)
        {
            if (this.forceMode) GSIManagerExample.GetInstance().ChangeGameStageForce<LoginStageExample>();
            else GSIManagerExample.GetInstance().ChangeGameStage<LoginStageExample>();
        }

        if (Keyboard.current.numpad5Key.wasReleasedThisFrame)
        {
            if (this.forceMode) GSIManagerExample.GetInstance().ChangeGameStageForce<EnterStageExample>();
            else GSIManagerExample.GetInstance().ChangeGameStage<EnterStageExample>();
        }
    }
}
