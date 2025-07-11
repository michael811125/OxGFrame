using OxGFrame.NetFrame;
using UnityEngine;

public class NetTipsExample : INetTips
{
    public void OnConnected(object payload)
    {
        Debug.Log("OnConnected Message");
    }

    public void OnConnecting()
    {
        Debug.Log("OnConnecting Message");
    }

    public void OnConnectionError(object payload)
    {
        Debug.Log("OnConnectionError Message");
    }

    public void OnDisconnected(object payload)
    {
        Debug.Log("OnDisconnected Message");
    }

    public void OnReconnecting()
    {
        Debug.Log("OnReconnecting Message");
    }
}