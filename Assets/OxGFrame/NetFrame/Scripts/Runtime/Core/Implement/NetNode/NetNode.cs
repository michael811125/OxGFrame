using OxGKit.LoggingSystem;
using OxGKit.TimeSystem;
using System;

namespace OxGFrame.NetFrame
{
    public delegate void ResponseHandler<T>(T data);
    public delegate void ConnectingHandler();
    public delegate void ConnectedHandler();

    /// <summary>
    /// Represents the current state of the network connection.
    /// </summary>
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
        protected NetStatus _netStatus;                                  // Current network status
        protected NetOption _netOption = null;                           // Network configuration options
        protected INetProvider _netProvider = null;                      // Underlying network provider (TCP, WebSocket, KCP, etc.)
        protected INetTips _netTips = null;                              // UI/UX interface for displaying network status tips

        private bool _isCloseForce = false;                              // Flag to indicate if the connection was closed intentionally

        protected RealTimer _hearBeatTicker = null;                      // Cyclic timer for sending heartbeats
        private float _heartBeatTick = 10f;                              // Heartbeat interval in seconds (Default: 10s)
        protected Action _heartBeatAction = null;                        // Callback triggered when heartbeat interval elapses

        protected RealTimer _outReceiveTicker = null;                    // Cyclic timer for receive-timeout detection
        private float _outReceiveTick = 60f;                             // Timeout threshold in seconds (Default: 60s)
        protected Action _outReceiveAction = null;                       // Callback triggered when no data is received within the threshold

        protected RealTimer _reconnectTicker = null;                     // Cyclic timer for reconnection attempts
        private float _reconnectTick = 5f;                               // Delay between reconnection attempts (Default: 5s)
        private int _autoReconnectCount = 0;                             // Remaining number of automatic reconnection attempts
        protected Action _reconnectAction = null;                        // Callback triggered upon a reconnection attempt

        protected ResponseHandler<byte[]> _responseBinaryHandler = null; // Delegate for handling incoming binary data
        protected ResponseHandler<string> _responseMessageHandler;       // Delegate for handling incoming text data
        protected ConnectingHandler _connectingHandler = null;           // Callback triggered when the connection starts
        protected ConnectedHandler _connectedHandler = null;             // Callback triggered when the connection is established

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

        ~NetNode()
        {
            this.Dispose();
        }

        /// <summary>
        /// Retrieves the network provider cast to a specific implementation.
        /// </summary>
        /// <typeparam name="T">The type of INetProvider.</typeparam>
        /// <returns>The network provider instance.</returns>
        public T GetNetProvider<T>() where T : INetProvider
        {
            T socket = (T)this._netProvider;
            return socket;
        }

        private void _Initialize(INetProvider socket, INetTips netTips)
        {
            this._netProvider = socket;
            this._InitNetEvents();
            this._netTips = netTips;
            this._netStatus = NetStatus.DISCONNECTED;
        }

        /// <summary>
        /// Hooks into the provider's network events.
        /// </summary>
        private void _InitNetEvents()
        {
            this._netProvider.OnOpen += (sender, payload) =>
            {
                this._OnOpen(payload);
            };

            this._netProvider.OnBinary += (sender, binary) =>
            {
                this._OnBinary(binary);
            };

            this._netProvider.OnMessage += (sender, text) =>
            {
                this._OnMessage(text);
            };

            this._netProvider.OnError += (sender, payload) =>
            {
                this._OnError(payload);
            };

            this._netProvider.OnClose += (sender, payload) =>
            {
                this._OnClose(payload);
            };
        }

        /// <summary>
        /// Initiates a connection to the server using the specified options.
        /// </summary>
        /// <param name="netOption">Connection settings including address and port.</param>
        public void Connect(NetOption netOption)
        {
            if (this._netProvider == null)
            {
                Logging.PrintError<Logger>("NetProvider cannot be null. Please initialize it first.");
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

        /// <summary>
        /// Sets the UI feedback interface for network state changes.
        /// </summary>
        public void SetNetTips(INetTips netTips)
        {
            this._netTips = netTips;
        }

        /// <summary>
        /// Internal handler to transition the NetStatus and notify UI listeners.
        /// </summary>
        private void _NetStatusHandler(NetStatus status, object payload)
        {
            this._netStatus = status;
            switch (this._netStatus)
            {
                case NetStatus.CONNECTING:
                    this._netTips?.OnConnecting();
                    break;
                case NetStatus.CONNECTED:
                    this._netTips?.OnConnected(payload);
                    break;
                case NetStatus.CONNECTION_ERROR:
                    this._netTips?.OnConnectionError(payload);
                    break;
                case NetStatus.DISCONNECTED:
                    this._netTips?.OnDisconnected(payload);
                    break;
                case NetStatus.RECONNECTING:
                    this._netTips?.OnReconnecting();
                    break;
            }
        }

        /// <summary>
        /// Main update loop to process timers and provider logic. 
        /// Should be called by a driver (e.g., MonoBehavior Update).
        /// </summary>
        internal void OnUpdate()
        {
            this._netProvider?.OnUpdate();
            this._ProcessOutReceive();
            this._ProcessHeartBeat();
            this._ProcessAutoReconnect();
        }

        private void _OnOpen(object payload)
        {
            this._NetStatusHandler(NetStatus.CONNECTED, payload);

            this._isCloseForce = false;
            this._ResetAutoReconnect();
            this._ResetOutReceiveTicker();
            this._ResetHeartBeatTicker();
            this._connectedHandler?.Invoke();
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

        private void _OnError(object payload)
        {
            this._NetStatusHandler(NetStatus.CONNECTION_ERROR, payload);
        }

        private void _OnClose(object payload)
        {
            this._NetStatusHandler(NetStatus.DISCONNECTED, payload);

            this._StopTicker();
            if (this._isCloseForce)
                return;
            this._StartAutoReconnect();
        }

        /// <summary>
        /// Sends raw binary data through the current provider.
        /// </summary>
        public bool Send(byte[] buffer)
        {
            return this._netProvider.SendBinary(buffer);
        }

        /// <summary>
        /// Sends a string message through the current provider.
        /// </summary>
        public bool Send(string text)
        {
            return this._netProvider.SendMessage(text);
        }

        /// <summary>
        /// Manually closes the connection and prevents automatic reconnection.
        /// </summary>
        public void Close()
        {
            this._isCloseForce = true;
            if (this._netProvider != null)
                this._netProvider.Close();
        }

        /// <summary>
        /// Checks if the network provider is currently connected.
        /// </summary>
        public bool IsConnected()
        {
            if (this._netProvider == null)
                return false;
            return this._netProvider.IsConnected();
        }

        #region Response Handlers
        /// <summary>
        /// Assigns a handler for processing binary responses.
        /// </summary>
        public void SetResponseBinaryHandler(ResponseHandler<byte[]> handler)
        {
            this._responseBinaryHandler = handler;
        }

        /// <summary>
        /// Assigns a handler for processing text responses.
        /// </summary>
        public void SetResponseMessageHandler(ResponseHandler<string> handler)
        {
            this._responseMessageHandler = handler;
        }

        /// <summary>
        /// Assigns a callback to be triggered during the connection phase.
        /// </summary>
        public void SetConnectingHandler(ConnectingHandler handler)
        {
            this._connectingHandler = handler;
        }

        /// <summary>
        /// Assigns a callback to be triggered once the connection is successful.
        /// </summary>
        public void SetConnectedHandler(ConnectedHandler handler)
        {
            this._connectedHandler = handler;
        }
        #endregion

        #region Timeout (Receive) Ticker Logic
        /// <summary>
        /// Sets the action to perform when a receive timeout occurs.
        /// </summary>
        public void SetOutReceiveAction(Action outReceiveAction)
        {
            this._outReceiveAction = outReceiveAction;
        }

        /// <summary>
        /// Sets the maximum time allowed between receiving data before a timeout is triggered.
        /// </summary>
        public void SetOutReceiveTickerTime(float time)
        {
            this._outReceiveTick = time;
        }

        private void _ResetOutReceiveTicker()
        {
            if (this._outReceiveTicker == null)
                this._outReceiveTicker = new RealTimer();

            this._outReceiveTicker.Play();
            this._outReceiveTicker.SetTick(this._outReceiveTick);
        }

        private void _ProcessOutReceive()
        {
            if (this._outReceiveTicker.IsPause())
                return;

            if (this._outReceiveTicker.IsTickTimeout())
            {
                this._outReceiveAction?.Invoke();
                Logging.Print<Logger>("NetNode receive timeout detected.");
            }
        }
        #endregion

        #region Heartbeat Ticker Logic
        /// <summary>
        /// Sets the action to perform (e.g., sending a Ping) when the heartbeat ticker elapses.
        /// </summary>
        public void SetHeartBeatAction(Action heartBeatAction)
        {
            this._heartBeatAction = heartBeatAction;
        }

        /// <summary>
        /// Sets the frequency of the heartbeat pulse.
        /// </summary>
        /// <remarks>
        /// Recommendation: Set this value to less than half of the server's timeout (e.g., Server=8s, Client=4s).
        /// </remarks>
        public void SetHeartBeatTickerTime(float time)
        {
            this._heartBeatTick = time;
        }

        private void _ResetHeartBeatTicker()
        {
            if (this._hearBeatTicker == null)
                this._hearBeatTicker = new RealTimer();

            this._hearBeatTicker.Play();
            this._hearBeatTicker.SetTick(this._heartBeatTick);
        }

        private void _ProcessHeartBeat()
        {
            if (this._hearBeatTicker.IsPause())
                return;

            if (this._hearBeatTicker.IsTickTimeout())
            {
                this._heartBeatAction?.Invoke();
                Logging.Print<Logger>("NetNode sending heartbeat...");
            }
        }
        #endregion

        #region Reconnection Ticker Logic
        /// <summary>
        /// Sets the action to perform when a reconnection attempt is made.
        /// </summary>
        public void SetReconnectAction(Action reconnectAction)
        {
            this._reconnectAction = reconnectAction;
        }

        /// <summary>
        /// Sets the interval between reconnection attempts.
        /// </summary>
        public void SetReconnectTickerTime(float time)
        {
            this._reconnectTick = time;
        }

        private void _ResetAutoReconnect()
        {
            if (this._reconnectTicker == null)
                this._reconnectTicker = new RealTimer();

            if (this._netOption != null)
                this._autoReconnectCount = this._netOption.autoReconnectCount;
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
            if (this._reconnectTicker.IsPause())
                return;

            if (this._IsAutoReconnect() &&
                this._netStatus == NetStatus.RECONNECTING)
            {
                if (this._reconnectTicker.IsTickTimeout())
                {
                    this._netProvider.Close();

                    this.Connect(this._netOption);
                    if (this._autoReconnectCount > 0)
                        this._autoReconnectCount -= 1;

                    this._reconnectAction?.Invoke();
                    Logging.Print<Logger>("NetNode attempting to reconnect...");
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
        /// Stops all active timers (Heartbeat, Timeout, and Reconnect).
        /// </summary>
        private void _StopTicker()
        {
            this._hearBeatTicker?.Stop();
            this._outReceiveTicker?.Stop();
            this._reconnectTicker?.Stop();
        }

        /// <summary>
        /// Releases all resources used by the NetNode and shuts down connections.
        /// </summary>
        public void Dispose()
        {
            if (this._netProvider != null && !this._isCloseForce)
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
            this._responseMessageHandler = null;
            this._connectingHandler = null;
            this._connectedHandler = null;
        }
    }
}