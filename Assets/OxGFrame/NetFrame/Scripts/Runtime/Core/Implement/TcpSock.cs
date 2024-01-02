using OxGKit.LoggingSystem;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace OxGFrame.NetFrame
{
    public delegate void WaitReadNetPacket();

    public class TcpSock : ISocket
    {
        private const int _CONNECTING_TIMEOUT_MSEC = 10000;
        private const int _MAX_BUFFER_SIZE = 65536;
        private const int _MAX_FAILED_CONNECTION_COUNT = 3;

        private TcpClient _tcp = null;
        private NetOption _netOption = null;
        private int _failedConnectionCount;

        private int _readBufferOffset = 0;
        private byte[] _readBuffer = null;

        public event EventHandler OnOpen;
        public event EventHandler<byte[]> OnMessage;
        public event EventHandler<string> OnError;
        public event EventHandler<ushort> OnClose;

        private WaitReadNetPacket _waitReadNetPacket = null;

        public void CreateConnect(NetOption netOption)
        {
            this._netOption = netOption;
            if (string.IsNullOrEmpty(netOption.host) || netOption.port == 0) return;

            this._tcp = new TcpClient();
            this._tcp.ReceiveBufferSize = _MAX_BUFFER_SIZE;
            this._readBuffer = new byte[_MAX_BUFFER_SIZE];

            Interlocked.Exchange(ref this._failedConnectionCount, 0);

            try
            {
                Logging.Print<Logger>($"<color=#9cd6ff>Connecting... Host {netOption.host}:{netOption.port}</color>");
                IPAddress ipa = IPAddress.Parse(netOption.host);
                IAsyncResult result = this._tcp.BeginConnect(ipa, netOption.port, this._ConnectedAction, null);
                result.AsyncWaitHandle.WaitOne(_CONNECTING_TIMEOUT_MSEC);

                if (!result.IsCompleted)
                {
                    this._tcp.Close();
                    this.OnClose(this, 0);
                    Logging.PrintError<Logger>($"Begin Connect Failed!!! Host {netOption.host}:{netOption.port}");
                }
            }
            catch (Exception ex)
            {
                this._tcp.Close();
                this.OnError(this, $"{ex}");
                Logging.Print<Logger>($"CreateConnect Failed: {ex.Message}");
            }
        }

        private void _ConnectedAction(IAsyncResult result)
        {
            try
            {
                this._tcp?.EndConnect(result);
                Logging.Print<Logger>($"<color=#5dff49>TcpSock Connected.</color>");
            }
            catch (Exception ex)
            {
                Logging.PrintError<Logger>($"End Connect Failed: {ex.Message}");

                Interlocked.Increment(ref this._failedConnectionCount);

                if (this._failedConnectionCount >= _MAX_FAILED_CONNECTION_COUNT)
                {
                    this.OnError(this, $"{ex}");
                    return;
                }
            }

            this.OnOpen(this, EventArgs.Empty);

            NetworkStream ns = this._tcp.GetStream();
            this._readBufferOffset = 0;
            ns.BeginRead(this._readBuffer, this._readBufferOffset, this._readBuffer.Length, new AsyncCallback(this._ReadAction), ns);
        }

        private void _ReadAction(IAsyncResult result)
        {
            if (this._tcp == null || !this._tcp.Connected) return;

            // Get network stream via state object
            NetworkStream ns = (NetworkStream)result.AsyncState;
            if (ns == null)
            {
                Logging.PrintError<Logger>($"{nameof(NetworkStream)} cannot be null.");
                return;
            }

            int readBytes = 0;
            try
            {
                readBytes = ns.EndRead(result);
                Logging.Print<Logger>($"<color=#c9ff49>Reading End Time: {DateTime.Now}, ReadSize: {readBytes} bytes</color>");
            }
            catch (Exception ex)
            {
                this.Close();
                Logging.PrintError<Logger>($"EndRead Error: {ex.Message}, The Connection Has Been Closed: {result}, ReadSize: {readBytes} bytes");
                this.OnError(this, $"{ex}");
                return;
            }

            if (readBytes == 0) return;

            this._readBufferOffset += readBytes;

            // If there are any data just keep reading
            if (ns.DataAvailable)
            {
                try
                {
                    ns.BeginRead(this._readBuffer, this._readBufferOffset, this._readBuffer.Length - this._readBufferOffset, new AsyncCallback(this._ReadAction), ns);
                }
                catch (Exception ex)
                {
                    Logging.PrintError<Logger>(ex.Message);
                }
            }
            else
            {
                if (this._readBufferOffset > 0)
                {
                    this.OnMessage(this, this._readBuffer);
                    this._readBufferOffset = 0;
                }

                if (ns.DataAvailable)
                {
                    this._waitReadNetPacket?.Invoke();
                }

                try
                {
                    ns.BeginRead(this._readBuffer, this._readBufferOffset, this._readBuffer.Length - this._readBufferOffset, new AsyncCallback(this._ReadAction), ns);
                }
                catch (Exception ex)
                {
                    Logging.PrintError<Logger>(ex.Message);
                }
            }
        }

        private void _WriteAction(IAsyncResult result)
        {
            // Get network stream via state object
            NetworkStream ns = (NetworkStream)result.AsyncState;
            if (ns == null)
            {
                Logging.PrintError<Logger>($"{nameof(NetworkStream)} cannot be null.");
                return;
            }

            ns.EndWrite(result);
            Logging.Print<Logger>($"<color=#c9ff49>Sending Buffer End Time: {DateTime.Now}</color>");
        }

        public bool Send(byte[] buffer)
        {
            if (this.IsConnected())
            {
                try
                {
                    NetworkStream ns = this._tcp?.GetStream();
                    ns.BeginWrite(buffer, 0, buffer.Length, new AsyncCallback(this._WriteAction), ns);
                    Logging.Print<Logger>($"<color=#c9ff49>TcpSock - SentSize: {buffer.Length} bytes</color>");
                    return true;
                }
                catch (Exception ex)
                {
                    this.Close();
                    Logging.PrintError<Logger>($"Send Error: {ex.Message}");
                    return false;
                }
            }

            return false;
        }

        public bool IsConnected()
        {
            if (this._tcp == null) return false;
            return this._tcp.Connected;
        }

        public void Close()
        {
            this._tcp?.Close();
            this.OnClose(this, 0);
        }

        /// <summary>
        /// 設置等待讀取封包 Callback
        /// </summary>
        /// <param name="wrnp"></param>
        public void SetWaitReadNetPacket(WaitReadNetPacket wrnp)
        {
            this._waitReadNetPacket = wrnp;
        }
    }
}
