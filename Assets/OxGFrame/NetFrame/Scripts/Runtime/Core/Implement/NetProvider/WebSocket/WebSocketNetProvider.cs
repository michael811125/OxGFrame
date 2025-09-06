using OxGKit.LoggingSystem;
using System;
using UnityWebSocket;

namespace OxGFrame.NetFrame
{
    public class WebSocketNetProvider : INetProvider
    {
        private WebSocket _client = null;

        public event EventHandler<object> OnOpen;
        public event EventHandler<byte[]> OnBinary;
        public event EventHandler<string> OnMessage;
        public event EventHandler<object> OnError;
        public event EventHandler<object> OnClose;

        public void CreateConnect(NetOption netOption)
        {
            if (netOption == null)
            {
                Logging.PrintError<Logger>("ERROR: Connection failed because NetOption is null.");
                return;
            }

            string url = (netOption as WebSocketNetOption).url;
            if (string.IsNullOrEmpty(url))
            {
                Logging.PrintError<Logger>("ERROR: WebSocket connection failed. NetOption.url cannot be null or empty.");
                return;
            }

            this._client = new WebSocket(url);

            this._client.OnOpen += this._OnOpenHandler;
            this._client.OnMessage += this._OnMessageHandler;
            this._client.OnError += this._OnErrorHandler;
            this._client.OnClose += this._OnCloseHandler;

            this._client.ConnectAsync();
            Logging.PrintInfo<Logger>($"Endpoint: {url} => {nameof(WebSocketNetProvider)} is Connected.");
        }

        #region Handlers
        private void _OnOpenHandler(object sender, OpenEventArgs e)
        {
            this.OnOpen(sender, e);
        }

        private void _OnMessageHandler(object sender, MessageEventArgs e)
        {
            if (e.IsBinary)
                this.OnBinary(sender, e.RawData);
            else
                this.OnMessage(sender, e.Data);
        }

        private void _OnErrorHandler(object sender, ErrorEventArgs e)
        {
            this.OnError(sender, e.Message);
        }

        private void _OnCloseHandler(object sender, CloseEventArgs e)
        {
            this.OnClose(sender, e.Code);
        }
        #endregion

        public bool IsConnected()
        {
            if (this._client == null)
                return false;
            if (this._client.ReadyState == WebSocketState.Open)
                return true;
            return false;
        }

        public bool SendBinary(byte[] buffer)
        {
            if (this.IsConnected())
            {
                try
                {
                    this._client.SendAsync(buffer);
                    Logging.Print<Logger>($"[Binary] WebSocket - Try Send Size: {buffer.Length} bytes.");
                    return true;
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
            if (this.IsConnected())
            {
                try
                {
                    this._client.SendAsync(text);
                    Logging.Print<Logger>($"[Text] WebSocket - Try Send: {text}.");
                    return true;
                }
                catch (Exception ex)
                {
                    Logging.PrintError<Logger>($"Send Error: {ex.Message}");
                    return false;
                }
            }

            return false;
        }

        public void OnUpdate()
        {
        }

        public void Close()
        {
            if (this._client != null)
            {
                this._client.CloseAsync();
                this._client = null;
            }
        }
    }
}
