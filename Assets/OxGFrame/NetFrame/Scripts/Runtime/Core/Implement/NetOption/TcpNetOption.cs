namespace OxGFrame.NetFrame
{
    public class TcpNetOption : NetOption
    {
        public string host { get; set; }
        public int port { get; set; }

        public TcpNetOption(string host, int port, int autoReconnectCount = -1) : base(autoReconnectCount)
        {
            this.host = host;
            this.port = port;
        }
    }
}
