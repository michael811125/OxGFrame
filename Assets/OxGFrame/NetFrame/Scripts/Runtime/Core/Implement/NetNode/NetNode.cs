using OxGKit.LoggingSystem;
using OxGKit.Utilities.Timer;
using System;

namespace OxGFrame.NetFrame
{
    public delegate void ResponseHandler<T>(T data);
    public delegate void ConnectingHandler();

    public enum NetStatus
    {
        CONNECTING,
        CONNECTED,
        CONNECTION_ERROR,
        DISCONNECTED,
        RECONNECTING
    }

    public class NetNode : IDisposable
    {
        protected NetStatus _netStatus;                                   // Net 狀態
        protected NetOption _netOption = null;                            // 網路設置選項
        protected INetProvider _netProvider = null;                       // 網路供應者 (TCP/IP, websocket or other)
        protected INetTips _netTips = null;                               // 網路狀態提示介面

        private bool _isCloseForce = false;                               // 是否強制斷線

        protected RealTimer _hearBeatTicker = null;                       // 心跳檢測計循環計時器
        private float _heartBeatTick = 10f;                               // 心跳檢測時間, 預設 = 10秒
        protected Action _heartBeatAction = null;                         // 心跳檢測 Callback

        protected RealTimer _outReceiveTicker = null;                     // 超時檢測循環計時器
        private float _outReceiveTick = 60f;                              // 超時檢測時間, 預設 = 60秒
        protected Action _outReceiveAction = null;                        // 超時檢測 Callback

        protected RealTimer _reconnectTicker = null;                      // 斷線重連循環計時器
        private float _reconnectTick = 5f;                                // 斷線重連時間, 預設 = 5秒
        private int _autoReconnectCount = 0;                              // 自動連線次數
        protected Action _reconnectAction = null;                         // 斷線重連 Callback

        protected ResponseHandler<byte[]> _responseBinaryHandler = null;  // 接收的回調 (Binary)
        protected ResponseHandler<string> _responseMessageHandler;        // 接收的回調 (Text)
        protected ConnectingHandler _connectingHandler = null;            // 連線中的回調

        protected NetNode()
        {
        }

        public NetNode(INetProvider socket, INetTips netTips = null)
        {
            this._hearBeatTicker = new RealTimer();
            this._outReceiveTicker = new RealTimer();
            this._reconnectTicker = new RealTimer();
            this._Initialize(socket, netTips);
        }

        public T GetNetProvider<T>() where T : INetProvider
        {
            T socket = (T)this._netProvider;
            return socket;
        }

        private void _Initialize(INetProvider socket, INetTips netTips = null)
        {
            this._netProvider = socket;
            this._InitNetEvents();
            this._netTips = netTips;
            this._netStatus = NetStatus.DISCONNECTED;
        }

        private void _InitNetEvents()
        {
            this._netProvider.OnOpen += (sender, status) =>
            {
                this._OnOpen(status);
            };

            this._netProvider.OnBinary += (sender, binary) =>
            {
                this._OnBinary(binary);
            };

            this._netProvider.OnMessage += (sender, text) =>
            {
                this._OnMessage(text);
            };

            this._netProvider.OnError += (sender, error) =>
            {
                this._OnError(error);
            };

            this._netProvider.OnClose += (sender, status) =>
            {
                this._OnClose(status);
            };
        }

        public void Connect(NetOption netOption)
        {
            if (this._netProvider == null)
            {
                Logging.PrintError<Logger>("The socket cannot be null, Please init first.");
                return;
            }

            if (this._netStatus == NetStatus.DISCONNECTED ||
                this._netStatus == NetStatus.RECONNECTING)
            {
                this._NetStatusHandler(NetStatus.CONNECTING, null);
                this._netOption = netOption;
                this._connectingHandler?.Invoke();
                this._netProvider.CreateConnect(netOption);
            }
        }

        public void SetNetTips(INetTips netTips)
        {
            this._netTips = netTips;
        }

        private void _NetStatusHandler(NetStatus status, object args)
        {
            this._netStatus = status;
            switch (this._netStatus)
            {
                case NetStatus.CONNECTING:
                    this._netTips?.OnConnecting();
                    break;
                case NetStatus.CONNECTED:
                    this._netTips?.OnConnected(args);
                    break;
                case NetStatus.CONNECTION_ERROR:
                    this._netTips?.OnConnectionError(Convert.ToString(args));
                    break;
                case NetStatus.DISCONNECTED:
                    this._netTips?.OnDisconnected(args);
                    break;
                case NetStatus.RECONNECTING:
                    this._netTips?.OnReconnecting();
                    break;
            }
        }

        internal void OnUpdate()
        {
            this._ProcessOutReceive();
            this._ProcessHeartBeat();
            this._ProcessAutoReconnect();
        }

        private void _OnOpen(object status)
        {
            this._NetStatusHandler(NetStatus.CONNECTED, status);

            this._isCloseForce = false;
            this._ResetAutoReconnect();
            this._ResetOutReceiveTicker();
            this._ResetHeartBeatTicker();
        }

        private void _OnBinary(byte[] binary)
        {
            this._ResetOutReceiveTicker();
            this._responseBinaryHandler?.Invoke(binary);
        }

        private void _OnMessage(string text)
        {
            this._ResetOutReceiveTicker();
            this._responseMessageHandler?.Invoke(text);
        }

        private void _OnError(string msg)
        {
            this._NetStatusHandler(NetStatus.CONNECTION_ERROR, msg);
        }

        private void _OnClose(object status)
        {
            this._NetStatusHandler(NetStatus.DISCONNECTED, status);

            this._StopTicker();
            if (this._isCloseForce) return;
            this._StartAutoReconnect();
        }

        public bool Send(byte[] buffer)
        {
            return this._netProvider.SendBinary(buffer);
        }

        public bool Send(string text)
        {
            return this._netProvider.SendMessage(text);
        }

        public void Close()
        {
            this._isCloseForce = true;
            if (this._netProvider != null)
                this._netProvider.Close();
        }

        /// <summary>
        /// 返回連線狀態
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            if (this._netProvider == null) return false;
            return this._netProvider.IsConnected();
        }

        /// <summary>
        /// 設置接收的 Handler (Binary)
        /// </summary>
        /// <param name="handler"></param>
        public void SetResponseBinaryHandler(ResponseHandler<byte[]> handler)
        {
            this._responseBinaryHandler = handler;
        }

        /// <summary>
        /// 設置接收的 Handler (Text)
        /// </summary>
        /// <param name="handler"></param>
        public void SetResponseMessageHandler(ResponseHandler<string> handler)
        {
            this._responseMessageHandler = handler;
        }

        /// <summary>
        /// 設置每次連線中的 Handler
        /// </summary>
        /// <param name="handler"></param>
        public void SetConnectingHandler(ConnectingHandler handler)
        {
            this._connectingHandler = handler;
        }

        #region 超時 Ticker 處理
        public void SetOutReceiveAction(Action outReceiveAction)
        {
            this._outReceiveAction = outReceiveAction;
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

                Logging.Print<Logger>("<color=#FFC100>NetNode timeout processing...</color>");
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

                Logging.Print<Logger>("<color=#8EFF00>NetNode check heartbeat...</color>");
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
            this._NetStatusHandler(NetStatus.RECONNECTING, null);

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
                    this._netProvider.Close();

                    this.Connect(this._netOption);
                    if (this._autoReconnectCount > 0) this._autoReconnectCount -= 1;

                    this._reconnectAction?.Invoke();

                    Logging.Print<Logger>("<color=#FF0000>NetNode try to reconnecting...</color>");
                }
            }
            else
            {
                this._NetStatusHandler(NetStatus.DISCONNECTED, null);
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

        public void Dispose()
        {
            if (this._netProvider != null)
                this._netProvider.Close();
            this._netProvider = null;
            this._netTips = null;
            this._netOption = null;
            this._hearBeatTicker = null;
            this._heartBeatAction = null;
            this._outReceiveTicker = null;
            this._outReceiveAction = null;
            this._reconnectTicker = null;
            this._reconnectAction = null;
            this._responseBinaryHandler = null;
        }

        ~NetNode()
        {
            this.Dispose();
        }
    }
}