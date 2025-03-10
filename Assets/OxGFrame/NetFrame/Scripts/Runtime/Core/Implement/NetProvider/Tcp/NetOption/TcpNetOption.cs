namespace OxGFrame.NetFrame
{
    public class TcpNetOption : NetOption
    {
        public string host { get; set; }

        public int port { get; set; }

        /// <summary>
        /// processes up to 'limit' messages per tick
        /// </summary>
        public int processLimitPerTick = _DEFAULT_PROCESS_LIMIT_PER_TICK;

        /// <summary>
        /// Max buffer size of single packet
        /// </summary>
        public int maxBufferSize = _MAX_BUFFER_SIZE;

        /// <summary>
        /// 一幀最大處理 64 個封包
        /// </summary>
        private const int _DEFAULT_PROCESS_LIMIT_PER_TICK = 64;

        /// <summary>
        /// 單個封包最大數據量
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
