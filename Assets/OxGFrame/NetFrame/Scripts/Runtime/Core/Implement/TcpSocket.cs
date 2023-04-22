using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Threading;

namespace OxGFrame.NetFrame
{
    public delegate void WaitReadNetPacket();

    public class TcpSocket : ISocket
    {
        public const int CONNECTING_TIMEOUT_MSEC = 10000;
        private const int MAX_BUFFER_SIZE = 65536;

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
            this._tcp.ReceiveBufferSize = MAX_BUFFER_SIZE;
            this._readBuffer = new byte[MAX_BUFFER_SIZE];

            Interlocked.Exchange(ref this._failedConnectionCount, 0);

            try
            {
                Debug.Log(netOption.host);
                Debug.Log(netOption.port);
                IPAddress ipa = IPAddress.Parse(netOption.host);
                IAsyncResult iAsyncResult = this._tcp.BeginConnect(ipa, netOption.port, this._ConnectedAction, null);
                iAsyncResult.AsyncWaitHandle.WaitOne(TcpSocket.CONNECTING_TIMEOUT_MSEC);

                if (!iAsyncResult.IsCompleted)
                {
                    if (this._tcp != null) this._tcp.Close();
                    this.OnClose(this, 0);

                    Debug.Log("!iAsyncResult.IsCompleted");
                }
            }
            catch (Exception ex)
            {
                this._tcp.Close();
                this.OnError(this, string.Format("{0}", ex));

                Debug.Log("CreateConnect Failed" + ex.Message);
            }
        }

        private void _ConnectedAction(IAsyncResult ar)
        {
            try
            {
                if (this._tcp != null)
                {
                    this._tcp.EndConnect(ar);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("End Connect Failed" + ex.Message);

                Interlocked.Increment(ref this._failedConnectionCount);

                if (this._failedConnectionCount >= this._netOption.host.Length)
                {
                    this.OnError(this, string.Format("{0}", ex));
                    return;
                }
            }

            this.OnOpen(this, EventArgs.Empty);

            NetworkStream ns = this._tcp.GetStream();
            if (ns != null)
            {
                this._readBufferOffset = 0;
                ns.BeginRead(this._readBuffer, 0, this._readBuffer.Length, new AsyncCallback(this._ReadAction), ns);
            }
        }

        private void _ReadAction(IAsyncResult ar)
        {
            if (this._tcp == null || !this._tcp.Connected) return;

            NetworkStream ns = (NetworkStream)ar.AsyncState;

            int readBytes = 0;
            try
            {
                readBytes = ns.EndRead(ar);
                Debug.Log(string.Format("<color=#C9FF49>Reading Ends: Time {0}, ReadSize: {1} bytes</color>", DateTime.Now, readBytes));
            }
            catch (Exception ex)
            {
                this.Close();
                Debug.LogError(string.Format("EndRead Error {0}, The Connection Has Been Closed. {1}, bytes: {2}", ex.Message, ar, readBytes));
                this.OnError(this, string.Format("{0}", ex));

                return;
            }

            if (readBytes == 0) return;

            this._readBufferOffset += readBytes;

            // socket 裡如果還有資料可以讀取的話就繼續讀取
            if (ns.DataAvailable)
            {
                try
                {
                    ns.BeginRead(this._readBuffer, this._readBufferOffset, this._readBuffer.Length - this._readBufferOffset, new AsyncCallback(this._ReadAction), ns);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
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
                    Debug.LogError(ex.Message);
                }

            }
        }

        private void _WriteAction(IAsyncResult ar)
        {
            NetworkStream ns = (NetworkStream)ar.AsyncState;
            if (ns == null) return;

            ns.EndWrite(ar);

            Debug.Log(string.Format("<color=#C9FF49>Sending Buffer Ends: Time {0}</color>", DateTime.Now));
        }

        public bool Send(byte[] buffer)
        {
            if (this.IsConnected())
            {
                try
                {
                    NetworkStream ns = this._tcp.GetStream();
                    ns.BeginWrite(buffer, 0, buffer.Length, new AsyncCallback(this._WriteAction), ns);
                    Debug.Log(string.Format("<color=#C9FF49>TcpSocket - SentSize: {0} bytes</color>", buffer.Length));
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.LogError(string.Format("Send Error {0}", ex.Message));
                    this.Close();
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
            this._tcp.Close();
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
