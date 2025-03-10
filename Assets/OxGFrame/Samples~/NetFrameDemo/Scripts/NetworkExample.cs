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
        NetNode netNode = null;

        #region WebSocket Example
        netNode = new NetNode(new WebSocketNetProvider(), netTips);
        // Set data receive callback
        netNode.SetResponseBinaryHandler(ProcessRecvData);
        // Set connecting callback
        netNode.SetConnectingHandler(ProcessConnectingEvent);
        // Set heart beat callback
        netNode.SetHeartBeatAction(() =>
        {
            /* Process Heart Beat */
        });
        // Set out receive callback
        netNode.SetOutReceiveAction(() =>
        {
            /* Process Out Of Receive */
        });
        // Set reconnect callback
        netNode.SetReconnectAction(() =>
        {
            /* Process Reconnect */
        });

        // Add net node (register)
        NetFrames.AddNetNode(netNode, 0);
        #endregion

        #region TCP Example
        netNode = new NetNode(new TcpNetProvider(), netTips);
        // Set data receive callback
        netNode.SetResponseBinaryHandler(ProcessRecvData);
        // Set connecting callback
        netNode.SetConnectingHandler(ProcessConnectingEvent);
        // Set heart beat callback
        netNode.SetHeartBeatAction(() =>
        {
            /* Process Heart Beat */
        });
        // Set out receive callback
        netNode.SetOutReceiveAction(() =>
        {
            /* Process Out Of Receive */
        });
        // Set reconnect callback
        netNode.SetReconnectAction(() =>
        {
            /* Process Reconnect */
        });

        // Add net node (register)
        NetFrames.AddNetNode(netNode, 1);
        #endregion

        #region KCP Example
        netNode = new NetNode(new KcpNetProvider(), netTips);
        // Set data receive callback
        netNode.SetResponseBinaryHandler(ProcessRecvData);
        // Set connecting callback
        netNode.SetConnectingHandler(ProcessConnectingEvent);
        // Set heart beat callback
        netNode.SetHeartBeatAction(() =>
        {
            /* Process Heart Beat */
        });
        // Set out receive callback
        netNode.SetOutReceiveAction(() =>
        {
            /* Process Out Of Receive */
        });
        // Set reconnect callback
        netNode.SetReconnectAction(() =>
        {
            /* Process Reconnect */
        });

        // Add net node (register)
        NetFrames.AddNetNode(netNode, 2);
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
    /// <param name="nnid"></param>
    public static void OpenConnection(NetOption netOption, int nnid = 0)
    {
        InitNetNode();
        NetFrames.Connect(netOption, nnid);
    }

    /// <summary>
    /// Close connection
    /// </summary>
    /// <param name="nnid"></param>
    public static void CloseConnection(int nnid = 0)
    {
        NetFrames.Close(nnid, true);
    }

    /// <summary>
    /// Return connection status
    /// </summary>
    /// <param name="nnid"></param>
    /// <returns></returns>
    public static bool IsConnected(int nnid = 0)
    {
        return NetFrames.IsConnected(nnid);
    }

    /// <summary>
    /// Send binary data
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="nnid"></param>
    /// <returns></returns>
    public static bool SendData(byte[] buffer, int nnid = 0)
    {
        return NetFrames.Send(buffer, nnid);
    }
}