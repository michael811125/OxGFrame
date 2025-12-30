using OxGFrame.NetFrame;
using UnityEngine;

public class NetFrameDemo : MonoBehaviour
{
    #region WebSocket Example
    public void ConnectWebSocket()
    {
        // Open WebSocket connection and 5 times reconnect attempts
        NetworkExample.OpenConnection(new WebSocketNetOption("ws://127.0.0.1:10100", 5), (int)NetworkExample.NNID.WebSocket);
    }

    public void SendWebSocketData()
    {
        NetworkExample.SendData(System.Text.Encoding.UTF8.GetBytes("Hello OxGFrame NetFrame!"), (int)NetworkExample.NNID.WebSocket);
    }

    public bool IsWebSocketConnected()
    {
        return NetworkExample.IsConnected((int)NetworkExample.NNID.WebSocket);
    }

    public void CloseWebSocket()
    {
        NetworkExample.CloseConnection((int)NetworkExample.NNID.WebSocket);
    }
    #endregion

    #region TCP Example
    public void ConnectTCP()
    {
        // Open TCP connection and 5 times reconnect attempts
        NetworkExample.OpenConnection(new TcpNetOption("127.0.0.1", 10100, 5), (int)NetworkExample.NNID.TCP);
    }

    public void SendTCPData()
    {
        NetworkExample.SendData(System.Text.Encoding.UTF8.GetBytes("Hello OxGFrame NetFrame!"), (int)NetworkExample.NNID.TCP);
    }

    public bool IsTCPConnected()
    {
        return NetworkExample.IsConnected((int)NetworkExample.NNID.TCP);
    }

    public void CloseTCP()
    {
        NetworkExample.CloseConnection((int)NetworkExample.NNID.TCP);
    }
    #endregion

    #region KCP Example 
    public void ConnectKCP()
    {
        // Open KCP connection and 5 times reconnect attempts
        NetworkExample.OpenConnection(new KcpNetOption("127.0.0.1", 10100, kcp2k.KcpChannel.Reliable, 5), (int)NetworkExample.NNID.KCP);
    }

    public void SendKCPDataBy()
    {
        NetworkExample.SendData(System.Text.Encoding.UTF8.GetBytes("Hello OxGFrame NetFrame!"), (int)NetworkExample.NNID.KCP);
    }

    public bool IsKCPConnected()
    {
        return NetworkExample.IsConnected((int)NetworkExample.NNID.KCP);
    }

    public void CloseKCP()
    {
        NetworkExample.CloseConnection((int)NetworkExample.NNID.TCP);
    }
    #endregion
}