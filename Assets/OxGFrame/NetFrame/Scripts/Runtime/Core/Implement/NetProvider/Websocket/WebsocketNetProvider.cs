using OxGKit.LoggingSystem;
using System;
using UnityWebSocket;

namespace OxGFrame.NetFrame
{
    public class WebsocketNetProvider : INetProvider
    {
        private WebSocket _ws = null;

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

            string url = (netOption as WebsocketNetOption).url;
            if (string.IsNullOrEmpty(url))
            {
                Logging.Print<Logger>("<color=##FF0000>ERROR: Websocket Connect failed, NetOption URL cannot be null or empty.</color>");
                return;
            }

            this._ws = new WebSocket(url);

            this._ws.OnOpen += (sender, e) =>
            {
                this.OnOpen(sender, e);
            };
            this._ws.OnMessage += (sender, e) =>
            {
                if (e.IsBinary) this.OnBinary(sender, e.RawData);
                else this.OnMessage(sender, e.Data);
            };
            this._ws.OnError += (sender, e) =>
            {
                this.OnError(sender, e.Message);
            };
            this._ws.OnClose += (sender, e) =>
            {
                this.OnClose(sender, e.Code);
            };

            this._ws.ConnectAsync();

            Logging.Print<Logger>($"URL: {url} => Websocket is Connected.");
        }

        public bool IsConnected()
        {
            if (this._ws == null) return false;
            if (this._ws.ReadyState == WebSocketState.Open) return true;
            return false;
        }

        public bool SendBinary(byte[] buffer)
        {
            if (this.IsConnected())
            {
                this._ws?.SendAsync(buffer);
                Logging.Print<Logger>($"<color=#C9FF49>[Binary] Websocket - Send Size: {buffer.Length} bytes.</color>");
                return true;
            }

            return false;
        }

        public bool SendMessage(string text)
        {
            if (this.IsConnected())
            {
                this._ws?.SendAsync(text);
                Logging.Print<Logger>($"<color=#C9FF49>[Text] Websocket - Try Send: {text}.</color>");
                return true;
            }

            return false;
        }

        public void Close()
        {
            this._ws?.CloseAsync();
        }
    }
}
