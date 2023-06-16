using UnityEngine;

public class EventCenterDemo : MonoBehaviour
{
    public void OnEventEmit()
    {
        EventCenterExample.Find<EventMsgTest>()?.Emit(10, "Hello Event!");
    }

    public void OnEventHandle()
    {
        EventCenterExample.Find<EventMsgTest>()?.HandleEvent();
    }
}
