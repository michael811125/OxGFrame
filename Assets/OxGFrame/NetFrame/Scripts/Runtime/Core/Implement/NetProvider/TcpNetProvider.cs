using OxGKit.LoggingSystem;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace OxGFrame.NetFrame
{
    public delegate void WaitReadNetPacket();

    public class TcpNetProvider : INetProvider
    {
        private const int _CONNECTING_TIMEOUT_MSEC = 10000;
        private const int _MAX_BUFFER_SIZE = 65536;
        private const int _MAX_FAILED_CONNECTION_COUNT = 3;

        private TcpClient _tcp = null;
        private int _failedConnectionCount;

        private int _readBufferOffset = 0;
        private byte[] _readBuffer = null;

        public event EventHandler<object> OnOpen;
        public event EventHandler<byte[]> OnBinary;
        public event EventHandler<string> OnMessage;
        public event EventHandler<string> OnError;
        public event EventHandler<object> OnClose;

        private WaitReadNetPacket _waitReadNetPacket = null;

        public void CreateConnect(NetOption netOption)
        {
            if (netOption == null)
            {
                Logging.Print<Logger>("<color=#ff2732>ERROR: Connect failed, NetOption cannot be null.</color>");
                return;
            }

            string host = (netOption as TcpNetOption).host;
            int port = (netOption as TcpNetOption).port;

            if (string.IsNullOrEmpty(host))
            {
                Logging.Print<Logger>("<color=##FF0000>ERROR: TCP/IP Connect failed, NetOption Host cannot be null or empty.</color>");
                return;
            }

            this._tcp = new TcpClient();
            this._tcp.ReceiveBufferSize = _MAX_BUFFER_SIZE;
            this._readBuffer = new byte[_MAX_BUFFER_SIZE];

            Interlocked.Exchange(ref this._failedConnectionCount, 0);

            try
            {
                Logging.Print<Logger>($"<color=#9cd6ff>Connecting... Host {host}:{port}</color>");
                IPAddress ipa = IPAddress.Parse(host);
                IAsyncResult result = this._tcp.BeginConnect(ipa, port, this._ConnectedAction, null);
                result.AsyncWaitHandle.WaitOne(_CONNECTING_TIMEOUT_MSEC);

                if (!result.IsCompleted)
                {
                    this._tcp.Close();
                    this.OnClose(this, -1);
                    Logging.PrintError<Logger>($"Begin Connect Failed!!! Host {host}:{port}");
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
                Logging.Print<Logger>($"<color=#5dff49>TCP/IP Connected.</color>");
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

            this.OnOpen(this, 0);
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
                    this.OnBinary(this, this._readBuffer);
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

        public bool SendBinary(byte[] buffer)
        {
            if (this.IsConnected())
            {
                try
                {
                    NetworkStream ns = this._tcp?.GetStream();
                    ns.BeginWrite(buffer, 0, buffer.Length, new AsyncCallback(this._WriteAction), ns);
                    Logging.Print<Logger>($"<color=#c9ff49>[Binary] - Send Size: {buffer.Length} bytes.</color>");
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

        public bool SendMessage(string text)
        {
            throw new Exception("[Text] TCP/IP not supports SendMessge!!! Please convert string to binary and send by binary.");
        }

        public bool IsConnected()
        {
            if (this._tcp == null) return false;
            return this._tcp.Connected;
        }

        public void Close()
        {
            this._tcp?.Close();
            this.OnClose(this, -1);
        }

        public void SetWaitReadNetPacket(WaitReadNetPacket wrnp)
        {
            this._waitReadNetPacket = wrnp;
        }
    }
}