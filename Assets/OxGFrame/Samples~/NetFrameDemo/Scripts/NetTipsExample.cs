using OxGFrame.NetFrame;
using UnityEngine;

public class NetTipsExample : INetTips
{
    public void OnConnected(object status)
    {
        Debug.Log("OnConnected Message");
    }

    public void OnConnecting()
    {
        Debug.Log("OnConnecting Message");
    }

    public void OnConnectionError(string msg)
    {
        Debug.Log("OnConnectionError");
    }

    public void OnDisconnected(object status)
    {
        Debug.Log("OnDisconnected");
    }

    public void OnReconnecting()
    {
        Debug.Log("OnReconnecting");
    }
}