using OxGKit.LoggingSystem;
using System;
using Telepathy;
using UnityWebSocket;

namespace OxGFrame.NetFrame
{
    public class TcpNetProvider : INetProvider
    {
        private Client _client = null;

        /// <summary>
        /// 一幀最大處理 N 個封包
        /// </summary>
        public int processLimitPerTick;

        public event EventHandler<object> OnOpen;
        public event EventHandler<byte[]> OnBinary;
        public event EventHandler<string> OnMessage;
        public event EventHandler<string> OnError;
        public event EventHandler<object> OnClose;

        public void CreateConnect(NetOption netOption)
        {
            if (netOption == null)
            {
                Logging.Print<Logger>("<color=#ff2732>ERROR: Connect failed, NetOption cannot be null.</color>");
                return;
            }

            string host = (netOption as TcpNetOption).host;
            int port = (netOption as TcpNetOption).port;
            int maxBufferSize = (netOption as TcpNetOption).maxBufferSize;
            this.processLimitPerTick = (netOption as TcpNetOption).processLimitPerTick;

            if (string.IsNullOrEmpty(host))
            {
                Logging.Print<Logger>("<color=##FF0000>ERROR: TCP Connect failed, NetOption Host cannot be null or empty.</color>");
                return;
            }

            this._client = new Client(maxBufferSize);

            this._client.OnConnected += this._OnOpenHandler;
            this._client.OnData += this._OnMessageHandler;
            this._client.OnDisconnected += this._OnCloseHandler;

            try
            {
                // Open connection
                this._client.Connect(host, port);
            }
            catch (Exception ex)
            {
                this._OnErrorHandler($"{ex}");
            }
        }

        #region Handlers
        private void _OnOpenHandler()
        {
            this.OnOpen(this, 0);
        }

        private void _OnMessageHandler(ArraySegment<byte> arrSeg)
        {
            this.OnBinary(this, arrSeg.Array);
        }

        private void _OnErrorHandler(string errorMsg)
        {
            this.OnError(this, errorMsg);
        }

        private void _OnCloseHandler()
        {
            this.OnClose(this, -1);
        }
        #endregion

        public bool SendBinary(byte[] buffer)
        {
            if (this.IsConnected())
            {
                try
                {
                    var segment = new ArraySegment<byte>(buffer);
                    bool sent = this._client.Send(segment);
                    Logging.Print<Logger>($"<color=#c9ff49>[Binary] TCP - Try Send Size: {buffer.Length} bytes.</color>");
                    return sent;
                }
                catch (Exception ex)
                {
                    Logging.PrintError<Logger>($"Send Error: {ex.Message}");
                    return false;
                }
            }

            return false;
        }

        public bool SendMessage(string text)
        {
            throw new Exception("[Text] TCP not supports SendMessage!!! Please convert string to binary and send by binary.");
        }

        public bool IsConnected()
        {
            if (this._client == null)
                return false;
            return this._client.Connected;
        }

        public void OnUpdate()
        {
            if (this._client == null)
                return;
            this._client.Tick(this.processLimitPerTick);
        }

        public void Close()
        {
            if (this._client != null)
            {
                this._client.Disconnect();
                this._client = null;
            }
        }
    }
}