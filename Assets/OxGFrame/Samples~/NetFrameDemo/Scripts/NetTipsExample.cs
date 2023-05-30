using OxGFrame.NetFrame;
using System;
using UnityEngine;

public class NetTipsExample : INetTips
{
    public void OnConnected(EventArgs e)
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

    public void OnDisconnected(ushort code)
    {
        Debug.Log("OnDisconnected");
    }

    public void OnReconnecting()
    {
        Debug.Log("OnReconnecting");
    }
}
