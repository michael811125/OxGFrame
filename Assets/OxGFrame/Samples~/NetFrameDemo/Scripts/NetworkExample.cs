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
        NetNode wsNetNode = new NetNode(new WebSock(), netTips);
        // Set data receive callback
        wsNetNode.SetResponseHandler(ProcessRecvData);
        // Set first send callback (verification)
        wsNetNode.SetFirstSendHandler(ProcessFirstSend);
        // Set heart beat callback
        wsNetNode.SetHeartBeatAction(() =>
        {
            /* Process Heart Beat */
        });
        // Set out receive callback
        wsNetNode.SetOutReciveAction(() =>
        {
            /* Process Out Of Recive */
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
        NetNode tcpNetNode = new NetNode(new TcpSock(), netTips);
        // Set data receive callback
        tcpNetNode.SetResponseHandler(ProcessRecvData);
        // Set first send callback (verification)
        tcpNetNode.SetFirstSendHandler(ProcessFirstSend);
        // Set heart beat callback
        tcpNetNode.SetHeartBeatAction(() =>
        {
            /* Process Heart Beat */
        });
        // Set out receive callback
        tcpNetNode.SetOutReciveAction(() =>
        {
            /* Process Out Of Recive */
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
    /// Protocol first send verification
    /// </summary>
    public static void ProcessFirstSend()
    {
        Debug.Log("Init First Send");
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
        NetFrames.CloseSocket(nnid);
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
