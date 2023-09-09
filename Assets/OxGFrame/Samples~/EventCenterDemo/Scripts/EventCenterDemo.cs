using UnityEngine;

public class EventCenterDemo : MonoBehaviour
{
    public void OnRecvEventHandle(int valueInt, string valueString)
    {
        valueString = string.IsNullOrEmpty(valueString) ? "null" : valueString;
        Debug.Log($"<color=#ffdf00>[{nameof(OnRecvEventHandle)}] Get Values: {valueInt}, {valueString}</color>");
    }

    private void OnEnable()
    {
        // Add event listener
        EventCenterExample.Find<EventMsgTest>().eventHandler += this.OnRecvEventHandle;
    }

    private void OnDisable()
    {
        // Del event listener
        EventCenterExample.Find<EventMsgTest>().eventHandler -= this.OnRecvEventHandle;
    }

    public void OnEventEmit()
    {
        EventCenterExample.Find<EventMsgTest>()?.Emit(10, "Hello Event!");
    }

    public void OnEventHandle()
    {
        EventCenterExample.Find<EventMsgTest>()?.HandleEvent();
    }
}
