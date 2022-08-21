using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventCenterDemo : MonoBehaviour
{
    public void OnEventEmit()
    {
        EventCenterExample.GetInstance().GetEvent<EEventTest>(EventCenterExample.EEventTest)?.Emit(10, "Hello Event!");
    }

    public void OnEventHandle()
    {
        EventCenterExample.GetInstance().DirectCall(EventCenterExample.EEventTest);
    }
}
