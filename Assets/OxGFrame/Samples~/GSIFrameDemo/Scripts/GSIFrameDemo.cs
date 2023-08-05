using UnityEngine;
using UnityEngine.InputSystem;

public class GSIFrameDemo : MonoBehaviour
{
    public bool forceMode = false;

    void Start()
    {
        GSIManagerExample.Start();
    }

    void Update()
    {
        GSIManagerExample.Update(Time.deltaTime);

        if (Keyboard.current.numpad0Key.wasReleasedThisFrame)
        {
            this.forceMode = !this.forceMode;

            if (this.forceMode) Debug.Log("Enable ForceMode");
            else Debug.Log("Disable ForceMode");
        }

        if (Keyboard.current.numpad1Key.wasReleasedThisFrame)
        {
            GSIManagerExample.ChangeStage<StartupStageExample>(this.forceMode);
        }

        if (Keyboard.current.numpad2Key.wasReleasedThisFrame)
        {
            GSIManagerExample.ChangeStage<LogoStageExample>(this.forceMode);
        }

        if (Keyboard.current.numpad3Key.wasReleasedThisFrame)
        {
            GSIManagerExample.ChangeStage<PatchStageExample>(this.forceMode);
        }

        if (Keyboard.current.numpad4Key.wasReleasedThisFrame)
        {
            GSIManagerExample.ChangeStage<LoginStageExample>(this.forceMode);
        }

        if (Keyboard.current.numpad5Key.wasReleasedThisFrame)
        {
            GSIManagerExample.ChangeStage<EnterStageExample>(this.forceMode);
        }
    }
}
