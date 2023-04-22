using UnityEngine;

public class EventCenterDemo : MonoBehaviour
{
    public void OnEventEmit()
    {
        EventCenterExample.GetInstance().GetEvent<EventMsgTest>()?.Emit(10, "Hello Event!");
    }

    public void OnEventHandle()
    {
        EventCenterExample.GetInstance().GetEvent<EventMsgTest>()?.HandleEvent();
    }
}
