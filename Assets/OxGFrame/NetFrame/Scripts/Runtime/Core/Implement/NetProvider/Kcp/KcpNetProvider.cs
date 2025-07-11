using kcp2k;
using OxGKit.LoggingSystem;
using System;

namespace OxGFrame.NetFrame
{
    public class KcpNetProvider : INetProvider
    {
        private KcpClient _client;

        /// <summary>
        /// KCP Reliable type
        /// </summary>
        public KcpChannel kcpChannel;

        public event EventHandler<object> OnOpen;
        public event EventHandler<byte[]> OnBinary;
        public event EventHandler<string> OnMessage;
        public event EventHandler<object> OnError;
        public event EventHandler<object> OnClose;

        public void CreateConnect(NetOption netOption)
        {
            if (netOption == null)
            {
                Logging.PrintError<Logger>("<color=#ff2732>ERROR: Connection failed because NetOption is null.</color>");
                return;
            }

            string host = (netOption as KcpNetOption).host;
            int port = (netOption as KcpNetOption).port;
            this.kcpChannel = (netOption as KcpNetOption).kcpChannel;
            KcpConfig config = (netOption as KcpNetOption).config;

            if (string.IsNullOrEmpty(host))
            {
                Logging.PrintError<Logger>("<color=#ff2732>ERROR: KCP connection failed. NetOption.Host cannot be null or empty.</color>");
                return;
            }

            this._client = new KcpClient
            (
                this._OnOpenHandler,
                this._OnMessageHandler,
                this._OnCloseHandler,
                this._OnErrorHandler,
                config
            );

            try
            {
                // Open connection
                this._client.Connect(host, (ushort)port);
            }
            catch (Exception ex)
            {
                this.OnError(this, $"{ex}");
            }
        }

        #region Handlers
        private void _OnOpenHandler()
        {
            this.OnOpen(this, 0);
        }

        private void _OnMessageHandler(ArraySegment<byte> arrSeg, KcpChannel channel)
        {
            var length = arrSeg.Count;
            var rcvData = new byte[length];
            Array.Copy(arrSeg.Array, arrSeg.Offset, rcvData, 0, length);
            this.OnBinary(this, rcvData);
        }

        private void _OnErrorHandler(ErrorCode errorCode, string errorMsg)
        {
            this.OnError(this, $"{errorCode}: {errorMsg}");
        }

        private void _OnCloseHandler()
        {
            this.OnClose(this, -1);
        }
        #endregion

        public bool IsConnected()
        {
            if (this._client == null)
                return false;
            return this._client.connected;
        }

        public bool SendBinary(byte[] buffer)
        {
            return SendBinary(this.kcpChannel, buffer);
        }

        public bool SendBinary(KcpChannel kcpChannel, byte[] buffer)
        {
            if (this.IsConnected())
            {
                try
                {
                    var segment = new ArraySegment<byte>(buffer);
                    this._client.Send(segment, kcpChannel);
                    Logging.Print<Logger>($"<color=#c9ff49>[Binary] KCP - Try Send Size: {buffer.Length} bytes.</color>");
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
            throw new Exception("[Text] KCP not supports SendMessage!!! Please convert string to binary and send by binary.");
        }

        public void OnUpdate()
        {
            if (this._client == null)
                return;
            this._client.Tick();
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
