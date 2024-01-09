using OxGFrame.NetFrame;
using UnityEngine;

public class NetworkExample
{
    /// <summary>
    /// Init net node
    /// </summary>
    public static void InitNetNode()
    {
        var netTips = new NetTipsExample();

        #region Websocket Example
        NetNode wsNetNode = new NetNode(new WebsocketNetProvider(), netTips);
        // Set data receive callback
        wsNetNode.SetResponseBinaryHandler(ProcessRecvData);
        // Set connecting callback
        wsNetNode.SetConnectingHandler(ProcessConnectingEvent);
        // Set heart beat callback
        wsNetNode.SetHeartBeatAction(() =>
        {
            /* Process Heart Beat */
        });
        // Set out receive callback
        wsNetNode.SetOutReceiveAction(() =>
        {
            /* Process Out Of Receive */
        });
        // Set reconnect callback
        wsNetNode.SetReconnectAction(() =>
        {
            /* Process Reconnect */
        });

        // Add net node (register)
        NetFrames.AddNetNode(wsNetNode, 0);
        #endregion

        #region TCP/IP Example
        NetNode tcpNetNode = new NetNode(new TcpNetProvider(), netTips);
        // Set data receive callback
        tcpNetNode.SetResponseBinaryHandler(ProcessRecvData);
        // Set connecting callback
        tcpNetNode.SetConnectingHandler(ProcessConnectingEvent);
        // Set heart beat callback
        tcpNetNode.SetHeartBeatAction(() =>
        {
            /* Process Heart Beat */
        });
        // Set out receive callback
        tcpNetNode.SetOutReceiveAction(() =>
        {
            /* Process Out Of Receive */
        });
        // Set reconnect callback
        tcpNetNode.SetReconnectAction(() =>
        {
            /* Process Reconnect */
        });

        // Add net node (register)
        NetFrames.AddNetNode(tcpNetNode, 1);
        #endregion
    }

    /// <summary>
    /// Data receive callback
    /// </summary>
    /// <param name="recvData"></param>
    public static void ProcessRecvData(byte[] recvData)
    {
        Debug.Log("Recv Binary Data");
    }

    /// <summary>
    /// Connecting handler
    /// </summary>
    public static void ProcessConnectingEvent()
    {
        /**
         * If there is first verification can do somethings in here
         */

        Debug.Log("Process Connecting Event");
    }

    /// <summary>
    /// Create connection
    /// </summary>
    /// <param name="netOption"></param>
    public static void OpenConnection(NetOption netOption, byte nnid = 0)
    {
        InitNetNode();
        NetFrames.Connect(netOption, nnid);
    }

    /// <summary>
    /// Close connection
    /// </summary>
    public static void CloseConnection(byte nnid = 0)
    {
        NetFrames.Close(nnid, true);
    }

    /// <summary>
    /// Return connection status
    /// </summary>
    /// <returns></returns>
    public static bool IsConnected(byte nnid = 0)
    {
        return NetFrames.IsConnected(nnid);
    }

    /// <summary>
    /// Send binary data
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static bool SendData(byte[] buffer, byte nnid = 0)
    {
        return NetFrames.Send(buffer, nnid);
    }
}