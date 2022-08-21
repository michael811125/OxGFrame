using OxGFrame.NetFrame;
using UnityEngine;

public class NetworkExample : MonoBehaviour
{
    /// <summary>
    /// 初始網路節點 (還未開始連接)
    /// </summary>
    public static void InitNetNode()
    {
        var netTips = new NetTipsExample();

        #region Websocket Example
        NetNode wsNetNode = new NetNode(new WebSock(), netTips);
        // 設置接收回調
        wsNetNode.SetResponseHandler(ProcessRecvData);
        // 設置第一次初始封包回調
        wsNetNode.SetFirstSendHandler(ProcessFirstSend);
        // 設置心跳檢測回調
        wsNetNode.SetHeartBeatAction(() =>
        {
            /* Process Heart Beat */
        });
        // 設置超時處理回調
        wsNetNode.SetOutReciveAction(() =>
        {
            /* Process Out Of Recive */
        });
        // 設置重連處理回調
        wsNetNode.SetReconnectAction(() =>
        {
            /* Process Reconnect */
        });

        // 加入節點至 NetManager
        NetManager.GetInstance().AddNetNode(wsNetNode, 0);
        #endregion

        #region TCP/IP Example
        NetNode tcpNetNode = new NetNode(new TcpSocket(), netTips);
        // 設置接收回調
        tcpNetNode.SetResponseHandler(ProcessRecvData);
        // 設置第一次初始封包回調
        tcpNetNode.SetFirstSendHandler(ProcessFirstSend);
        // 設置心跳檢測回調
        tcpNetNode.SetHeartBeatAction(() =>
        {
            /* Process Heart Beat */
        });
        // 設置超時處理回調
        tcpNetNode.SetOutReciveAction(() =>
        {
            /* Process Out Of Recive */
        });
        // 設置重連處理回調
        tcpNetNode.SetReconnectAction(() =>
        {
            /* Process Reconnect */
        });

        // 加入節點至 NetManager
        NetManager.GetInstance().AddNetNode(tcpNetNode, 1);
        #endregion
    }

    /// <summary>
    /// 數據接收回調
    /// </summary>
    /// <param name="recvData"></param>
    public static void ProcessRecvData(byte[] recvData)
    {
        Debug.Log("Recv Binary Data");
    }

    /// <summary>
    /// 封包初始回調
    /// </summary>
    public static void ProcessFirstSend()
    {
        Debug.Log("Init First Send");
    }

    /// <summary>
    /// 調用建立連線
    /// </summary>
    /// <param name="netOption"></param>
    public static void OpenConnection(NetOption netOption, byte nnid = 0)
    {
        InitNetNode();
        NetManager.GetInstance().Connect(netOption, nnid);
    }

    /// <summary>
    /// 調用關閉連線
    /// </summary>
    public static void CloseConnection(byte nnid = 0)
    {
        NetManager.GetInstance().CloseSocket(nnid);
    }

    /// <summary>
    /// 返回連線狀態
    /// </summary>
    /// <returns></returns>
    public static bool IsConnected(byte nnid = 0)
    {
        return NetManager.GetInstance().IsConnected(nnid);
    }

    /// <summary>
    /// 傳送 Binary 資料至 Server
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static bool SendData(byte[] buffer, byte nnid = 0)
    {
        return NetManager.GetInstance().Send(buffer, nnid);
    }
}
