namespace OxGFrame.NetFrame
{
    public class TcpNetOption : NetOption
    {
        public string host { get; set; }

        public int port { get; set; }

        /// <summary>
        /// Processes up to 'limit' messages per tick
        /// </summary>
        public int processLimitPerTick = _DEFAULT_PROCESS_LIMIT_PER_TICK;

        /// <summary>
        /// Max buffer size of single packet
        /// </summary>
        public int maxBufferSize = _MAX_BUFFER_SIZE;

        /// <summary>
        /// Default: process up to 64 packets per frame
        /// </summary>
        private const int _DEFAULT_PROCESS_LIMIT_PER_TICK = 64;

        /// <summary>
        /// Max packet size: 64KB (65536 bytes)
        /// </summary>
        private const int _MAX_BUFFER_SIZE = 65536;

        public TcpNetOption(string host, int port, int autoReconnectCount = -1) : base(autoReconnectCount)
        {
            this.host = host;
            this.port = port;
        }

        public TcpNetOption(string host, int port, int processLimitPerTick, int maxBufferSize = _MAX_BUFFER_SIZE, int autoReconnectCount = -1) : this(host, port, autoReconnectCount)
        {
            this.maxBufferSize = maxBufferSize;
            this.processLimitPerTick = processLimitPerTick;
        }
    }
}
