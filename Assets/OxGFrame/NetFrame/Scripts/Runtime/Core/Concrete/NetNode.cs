using OxGFrame.Utility.Timer;
using System;
using UnityEngine;

namespace OxGFrame.NetFrame
{
    public delegate void ResponseHandler(byte[] data);
    public delegate void FirstSendHandler();

    public class NetOption
    {
        public string url { get; set; }
        public string host { get; set; }
        public int port { get; set; }
        public int autoReconnectCount { get; set; }

        /// <summary>
        /// TCP/IP 初始連線位置參數
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="autoReconnectCount"></param>
        public NetOption(string host, int port, int autoReconnectCount = -1)
        {
            this.host = host;
            this.port = port;
            this.autoReconnectCount = autoReconnectCount;
        }

        /// <summary>
        /// Websocket 初始連線位置參數
        /// </summary>
        /// <param name="url"></param>
        /// <param name="autoReconnectCount"></param>
        public NetOption(string url, int autoReconnectCount = -1)
        {
            this.url = url;
            this.autoReconnectCount = autoReconnectCount;
        }
    }

    public enum NetStatus
    {
        CONNECTING,
        CONNECTED,
        CONNECTION_ERROR,
        DISCONNECTED,
        RECONNECTING
    }

    public class NetNode
    {
        protected NetStatus _netStatus;                      // Net狀態
        protected NetOption _netOption = null;               // 網路設置選項 (TCP or WS)
        protected ISocket _socket = null;                    // Socket介面 (TCP or WS)
        protected INetTips _netTips = null;                  // 網路狀態提示介面

        private bool _isCloseForce = false;                  // 是否強制斷線

        protected RealTimer _hearBeatTicker = null;          // 心跳檢測計循環計時器
        private float _heartBeatTick = 10f;                  // 心跳檢測時間, 預設 = 10秒
        protected Action _heartBeatAction = null;            // 心跳檢測 Callback

        protected RealTimer _outReceiveTicker = null;        // 超時檢測循環計時器
        private float _outReceiveTick = 60f;                 // 超時檢測時間, 預設 = 60秒
        protected Action _outReceiveAction = null;           // 超時檢測 Callback

        protected RealTimer _reconnectTicker = null;         // 斷線重連循環計時器
        private float _reconnectTick = 5f;                   // 斷線重連時間, 預設 = 5秒
        private int _autoReconnectCount = 0;                 // 自動連線次數 (由 NetOption 帶入)
        protected Action _reconnectAction = null;            // 斷線重連 Callback

        protected ResponseHandler _responseHandler = null;   // 接收的回調
        protected FirstSendHandler _firstSendHandler = null; // 第一次封包的回調

        public NetNode()
        {
            this._hearBeatTicker = new RealTimer();
            this._outReceiveTicker = new RealTimer();
            this._reconnectTicker = new RealTimer();
        }

        public NetNode(ISocket socket, INetTips netTips = null)
        {
            this._hearBeatTicker = new RealTimer();
            this._outReceiveTicker = new RealTimer();
            this._reconnectTicker = new RealTimer();

            this.Init(socket, netTips);
        }

        public T GetSocket<T>() where T : ISocket
        {
            T socket = (T)this._socket;
            return socket;
        }

        public void Init(ISocket socket, INetTips netTips = null)
        {
            this._socket = socket;
            this._InitSocketEvents();
            this._netTips = netTips;
            this._netStatus = NetStatus.DISCONNECTED;
        }

        private void _InitSocketEvents()
        {
            this._socket.OnOpen += (sender, e) =>
            {
                this._OnOpen(e);
            };

            this._socket.OnMessage += (sender, data) =>
            {
                this._OnMessage(data);
            };

            this._socket.OnError += (sender, e) =>
            {
                this._OnError(e);
            };

            this._socket.OnClose += (sender, e) =>
            {
                this._OnClose(e);
            };
        }

        public void Connect(NetOption netOption)
        {
            if (this._socket == null)
            {
                Debug.LogError("The socket cannot be null, Please init first.");
                return;
            }

            if (this._netStatus == NetStatus.DISCONNECTED || this._netStatus == NetStatus.RECONNECTING)
            {
                this._netStatus = NetStatus.CONNECTING; // 目前處於 CONNECTING 狀態
                this._NetStatusHandler();

                this._firstSendHandler?.Invoke();       // 重連時重新初始第一次封包

                this._netOption = netOption;            // 設置 NetOption (連線配置)
                this._socket.CreateConnect(netOption);  // 最後再建立 Socket 連線 (TCP/IP => 需先 InitNetSocket 註冊後才能 Handle, Websocket => 透過原先 EventHandler 再進行註冊)  
            }
        }

        /// <summary>
        /// 設置 NetTips (設置其他實作的 NetTips)
        /// </summary>
        /// <param name="netTips"></param>
        public void SetNetTips(INetTips netTips)
        {
            this._netTips = netTips;
        }

        private void _NetStatusHandler(object args = null)
        {
            switch (this._netStatus)
            {
                case NetStatus.CONNECTING:
                    this._netTips?.OnConnecting();
                    break;
                case NetStatus.CONNECTED:
                    this._netTips?.OnConnected(args as EventArgs);
                    break;
                case NetStatus.CONNECTION_ERROR:
                    this._netTips?.OnConnectionError(Convert.ToString(args));
                    break;
                case NetStatus.DISCONNECTED:
                    this._netTips?.OnDisconnected(Convert.ToUInt16(args));
                    break;
                case NetStatus.RECONNECTING:
                    this._netTips?.OnReconnecting();
                    break;
            }
        }

        public void OnUpdate()
        {
            this._ProcessOutReceive();
            this._ProcessHeartBeat();
            this._ProcessAutoReconnect();
        }

        private void _OnOpen(EventArgs e)
        {
            this._netStatus = NetStatus.CONNECTED;
            this._NetStatusHandler(e);

            this._isCloseForce = false;
            this._ResetAutoReconnect();
            this._ResetOutReceiveTicker();
            this._ResetHeartBeatTicker();
        }

        private void _OnMessage(byte[] data)
        {
            this._ResetOutReceiveTicker();

            this._responseHandler?.Invoke(data);
        }

        private void _OnError(string msg)
        {
            this._netStatus = NetStatus.CONNECTION_ERROR;
            this._NetStatusHandler(msg);
        }

        private void _OnClose(ushort code)
        {
            this._netStatus = NetStatus.DISCONNECTED;
            this._NetStatusHandler(code);

            this._StopTicker();

            if (this._isCloseForce) return;

            this._StartAutoReconnect();
        }

        /// <summary>
        /// 傳送 Binary Data 至 Server
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public bool Send(byte[] buffer)
        {
            return this._socket.Send(buffer);
        }

        /// <summary>
        /// 關閉 Socket
        /// </summary>
        public void CloseSocket()
        {
            this._isCloseForce = true;
            if (this._socket != null) this._socket.Close();
        }

        /// <summary>
        /// 返回連線狀態
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            if (this._socket == null) return false;
            return this._socket.IsConnected();
        }

        /// <summary>
        /// 設置接收的 Handler
        /// </summary>
        /// <param name="rh"></param>
        public void SetResponseHandler(ResponseHandler rh)
        {
            this._responseHandler = rh;
        }

        /// <summary>
        /// 設置第一次初始寄送封包的 Handler
        /// </summary>
        /// <param name="fsh"></param>
        public void SetFirstSendHandler(FirstSendHandler fsh)
        {
            this._firstSendHandler = fsh;
        }

        #region 超時 Ticker 處理
        public void SetOutReciveAction(Action outReciveAction)
        {
            this._outReceiveAction = outReciveAction;
        }

        public void SetOutReceiveTickerTime(float time)
        {
            this._outReceiveTick = time;
        }

        private void _ResetOutReceiveTicker()
        {
            if (this._outReceiveTicker == null) this._outReceiveTicker = new RealTimer();

            this._outReceiveTicker.Play();
            this._outReceiveTicker.SetTick(this._outReceiveTick);
        }

        private void _ProcessOutReceive()
        {
            if (this._outReceiveTicker.IsPause()) return;

            if (this._outReceiveTicker.IsTickTimeout())
            {
                this._outReceiveAction?.Invoke();

                Debug.Log("<color=#FFC100>NetNode timeout processing...</color>");
            }
        }
        #endregion

        #region 心跳檢測 Ticker 處理
        public void SetHeartBeatAction(Action heartBeatAction)
        {
            this._heartBeatAction = heartBeatAction;
        }

        public void SetHeartBeatTickerTime(float time)
        {
            this._heartBeatTick = time;
        }

        private void _ResetHeartBeatTicker()
        {
            if (this._hearBeatTicker == null) this._hearBeatTicker = new RealTimer();

            this._hearBeatTicker.Play();
            this._hearBeatTicker.SetTick(this._heartBeatTick);
        }

        private void _ProcessHeartBeat()
        {
            if (this._hearBeatTicker.IsPause()) return;

            if (this._hearBeatTicker.IsTickTimeout())
            {
                this._heartBeatAction?.Invoke();

                Debug.Log("<color=#8EFF00>NetNode check heartbeat...</color>");
            }
        }
        #endregion

        #region 斷線重連 Ticker 處理
        public void SetReconnectAction(Action reconnectAction)
        {
            this._reconnectAction = reconnectAction;
        }

        public void SetReconnectTickerTime(float time)
        {
            this._reconnectTick = time;
        }

        private void _ResetAutoReconnect()
        {
            if (this._reconnectTicker == null) this._reconnectTicker = new RealTimer();

            if (this._netOption != null) this._autoReconnectCount = this._netOption.autoReconnectCount;
            this._reconnectTicker.Stop();
        }

        private void _StartAutoReconnect()
        {
            this._netStatus = NetStatus.RECONNECTING;
            this._NetStatusHandler();

            this._reconnectTicker.Play();
            this._reconnectTicker.SetTick(this._reconnectTick);
        }

        private bool _IsAutoReconnect()
        {
            return (this._autoReconnectCount > 0);
        }

        private void _ProcessAutoReconnect()
        {
            if (this._reconnectTicker.IsPause()) return;

            if (this._IsAutoReconnect() && this._netStatus == NetStatus.RECONNECTING)
            {
                if (this._reconnectTicker.IsTickTimeout())
                {
                    this._socket.Close();

                    this.Connect(this._netOption);
                    if (this._autoReconnectCount > 0) this._autoReconnectCount -= 1;

                    this._reconnectAction?.Invoke();

                    Debug.Log("<color=#FF0000>NetNode try to reconnecting...</color>");
                }
            }
            else
            {
                this._netStatus = NetStatus.DISCONNECTED;
                this._NetStatusHandler();
                this._reconnectTicker.Stop();
            }
        }
        #endregion

        /// <summary>
        /// 停止所有計時器
        /// </summary>
        private void _StopTicker()
        {
            if (this._hearBeatTicker != null) this._hearBeatTicker.Stop();
            if (this._outReceiveTicker != null) this._outReceiveTicker.Stop();
            if (this._reconnectTicker != null) this._reconnectTicker.Stop();
        }

        ~NetNode()
        {
            this._socket = null;
            this._netTips = null;
            this._netOption = null;
            this._hearBeatTicker = null;
            this._heartBeatAction = null;
            this._outReceiveTicker = null;
            this._outReceiveAction = null;
            this._reconnectTicker = null;
            this._reconnectAction = null;
            this._responseHandler = null;
            this._firstSendHandler = null;
        }
    }
}

