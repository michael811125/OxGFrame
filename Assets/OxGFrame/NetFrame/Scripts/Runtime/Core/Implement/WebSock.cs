using System;
using UnityEngine;
using UnityWebSocket;

namespace OxGFrame.NetFrame
{
    public class WebSock : ISocket
    {
        private WebSocket _ws = null;
        private NetOption _netOption = null;

        public event EventHandler OnOpen;
        public event EventHandler<byte[]> OnMessage;
        public event EventHandler<string> OnError;
        public event EventHandler<ushort> OnClose;

        public void CreateConnect(NetOption netOption)
        {
            this._netOption = netOption;
            if (string.IsNullOrEmpty(netOption.url))
            {
                Debug.Log("<color=##FF0000>ERROR: Websocket Connect failed, net option URL cannot be null.</color>");
                return;
            }

            this._ws = new WebSocket(netOption.url);

            this._ws.OnOpen += (sender, e) =>
            {
                this.OnOpen(sender, e);
            };
            this._ws.OnMessage += (sender, e) =>
            {
                this.OnMessage(sender, e.RawData);
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

            Debug.Log(string.Format("URL: {0} => Websocket is Connected.", this._netOption.url));
        }

        public bool IsConnected()
        {
            if (this._ws == null) return false;

            if (this._ws.ReadyState == WebSocketState.Open) return true;
            return false;
        }

        public bool Send(byte[] buffer)
        {
            if (this.IsConnected())
            {
                this._ws.SendAsync(buffer);
                Debug.Log(string.Format("<color=#C9FF49>Websocket - SentSize: {0} bytes</color>", buffer.Length));
                return true;
            }

            return false;
        }

        public void Close()
        {
            this._ws.CloseAsync();
        }
    }
}
