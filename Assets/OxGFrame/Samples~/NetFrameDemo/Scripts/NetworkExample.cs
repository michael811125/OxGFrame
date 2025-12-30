using OxGFrame.NetFrame;
using UnityEngine;

public class NetworkExample
{
    /// <summary>
    /// NetNode ID
    /// </summary>
    public enum NNID
    {
        WebSocket = 0,
        TCP = 1,
        KCP = 2
    }

    /// <summary>
    /// Init WebSocket net node
    /// </summary>
    public static void InitWebSocketNetNode()
    {
        var netTips = new NetTipsExample();
        NetNode netNode = null;

        #region WebSocket NetNode Example
        netNode = new NetNode(new WebSocketNetProvider(), netTips);
        // Set data receive callback
        netNode.SetResponseBinaryHandler((recvData) =>
        {
            Debug.Log("Recv Binary Data (WebSocket)");
        });
        // Set connecting callback (Before connection)
        netNode.SetConnectingHandler(() =>
        {
            /**
             * If there is first verification can do somethings in here
             */

            Debug.Log("Process Connecting Event (WebSocket)");
        });
        // Set connected callback (After connection)
        netNode.SetConnectedHandler(() =>
        {
            /**
             * Connection established successfully
             */

            Debug.Log("Process Connected Event (WebSocket)");
        });
        // Set heart beat callback
        netNode.SetHeartBeatTickerTime(10f);
        netNode.SetHeartBeatAction(() =>
        {
            /* Process Heart Beat */
        });
        // Set out receive callback
        netNode.SetOutReceiveTickerTime(60f);
        netNode.SetOutReceiveAction(() =>
        {
            /* Process Out Of Receive */
        });
        // Set reconnect callback
        netNode.SetReconnectTickerTime(5f);
        netNode.SetReconnectAction(() =>
        {
            /* Process Reconnect */
        });

        // Add net node (register)
        NetFrames.AddNetNode(netNode, (int)NNID.WebSocket);
        #endregion
    }

    /// <summary>
    /// Init TCP net node
    /// </summary>
    public static void InitTCPNetNode()
    {
        var netTips = new NetTipsExample();
        NetNode netNode = null;

        #region TCP NetNode Example
        netNode = new NetNode(new TcpNetProvider(), netTips);
        // Set data receive callback
        netNode.SetResponseBinaryHandler((recvData) =>
        {
            Debug.Log("Recv Binary Data (TCP)");
        });
        // Set connecting callback (Before connection)
        netNode.SetConnectingHandler(() =>
        {
            /**
             * If there is first verification can do somethings in here
             */

            Debug.Log("Process Connecting Event (TCP)");
        });
        // Set connected callback (After connection)
        netNode.SetConnectedHandler(() =>
        {
            /**
             * Connection established successfully
             */

            Debug.Log("Process Connected Event (TCP)");
        });
        // Set heart beat callback
        netNode.SetHeartBeatTickerTime(10f);
        netNode.SetHeartBeatAction(() =>
        {
            /* Process Heart Beat */
        });
        // Set out receive callback
        netNode.SetOutReceiveTickerTime(60f);
        netNode.SetOutReceiveAction(() =>
        {
            /* Process Out Of Receive */
        });
        // Set reconnect callback
        netNode.SetReconnectTickerTime(5f);
        netNode.SetReconnectAction(() =>
        {
            /* Process Reconnect */
        });

        // Add net node (register)
        NetFrames.AddNetNode(netNode, (int)NNID.TCP);
        #endregion
    }

    /// <summary>
    /// Init KCP net node
    /// </summary>
    public static void InitKCPNetNode()
    {
        var netTips = new NetTipsExample();
        NetNode netNode = null;

        #region KCP NetNode Example
        netNode = new NetNode(new KcpNetProvider(), netTips);
        // Set data receive callback
        netNode.SetResponseBinaryHandler((recvData) =>
        {
            Debug.Log("Recv Binary Data (KCP)");
        });
        // Set connecting callback (Before connection)
        netNode.SetConnectingHandler(() =>
        {
            /**
             * If there is first verification can do somethings in here
             */

            Debug.Log("Process Connecting Event (KCP)");
        });
        // Set connected callback (After connection)
        netNode.SetConnectedHandler(() =>
        {
            /**
             * Connection established successfully
             */

            Debug.Log("Process Connected Event (KCP)");
        });
        // Set heart beat callback
        netNode.SetHeartBeatTickerTime(10f);
        netNode.SetHeartBeatAction(() =>
        {
            /* Process Heart Beat */
        });
        // Set out receive callback
        netNode.SetOutReceiveTickerTime(60f);
        netNode.SetOutReceiveAction(() =>
        {
            /* Process Out Of Receive */
        });
        // Set reconnect callback
        netNode.SetReconnectTickerTime(5f);
        netNode.SetReconnectAction(() =>
        {
            /* Process Reconnect */
        });

        // Add net node (register)
        NetFrames.AddNetNode(netNode, (int)NNID.KCP);
        #endregion
    }

    /// <summary>
    /// Create connection
    /// </summary>
    /// <param name="netOption"></param>
    /// <param name="nnid"></param>
    public static void OpenConnection(NetOption netOption, int nnid = 0)
    {
        // Init net node if not exist
        switch (nnid)
        {
            case (int)NNID.WebSocket:
                if (NetFrames.GetNetNode((int)NNID.WebSocket) == null)
                    InitWebSocketNetNode();
                break;
            case (int)NNID.TCP:
                if (NetFrames.GetNetNode((int)NNID.TCP) == null)
                    InitTCPNetNode();
                break;
            case (int)NNID.KCP:
                if (NetFrames.GetNetNode((int)NNID.KCP) == null)
                    InitKCPNetNode();
                break;
        }

        // Connect to server
        NetFrames.Connect(netOption, nnid);
    }

    /// <summary>
    /// Close connection
    /// </summary>
    /// <param name="nnid"></param>
    public static void CloseConnection(int nnid = 0)
    {
        // Close connection and remove net node
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
        // Also you can get NetProvider to send data like:
        // NetFrames.GetNetNode((int)NNID.KCP).GetNetProvider<KcpNetProvider>().SendBinary(buffer);
        // NetFrames.GetNetNode((int)NNID.KCP).GetNetProvider<KcpNetProvider>().SendBinary(kcp2k.KcpChannel.Unreliable, buffer);

        return NetFrames.Send(buffer, nnid);
    }
}