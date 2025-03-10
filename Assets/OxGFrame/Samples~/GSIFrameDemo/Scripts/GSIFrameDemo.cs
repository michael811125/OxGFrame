using UnityEngine;
using UnityEngine.InputSystem;

public class GSIFrameDemo : MonoBehaviour
{
    public bool forceMode = false;

    private void Start()
    {
        // Drive by main mono's start
        GSIManagerExample.DriveStart();
    }

    private void Update()
    {
        // Drive by main mono's update
        GSIManagerExample.DriveUpdate(Time.deltaTime);

        #region Tests
        if (Keyboard.current.numpad0Key.wasReleasedThisFrame)
        {
            this.forceMode = !this.forceMode;

            if (this.forceMode)
                Debug.Log("Enabled ForceMode");
            else
                Debug.Log("Disabled ForceMode");
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
        #endregion
    }
}
