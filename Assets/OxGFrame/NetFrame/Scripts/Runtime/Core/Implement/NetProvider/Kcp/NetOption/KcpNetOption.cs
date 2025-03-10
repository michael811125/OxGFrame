using kcp2k;

namespace OxGFrame.NetFrame
{
    public class KcpNetOption : NetOption
    {
        public string host { get; set; }

        public int port { get; set; }

        /// <summary>
        /// KCP Reliable type
        /// </summary>
        public KcpChannel kcpChannel { get; set; }

        public KcpConfig config = new KcpConfig
        (
            // force NoDelay and minimum interval.
            // this way UpdateSeveralTimes() doesn't need to wait very long and
            // tests run a lot faster.
            NoDelay: true,
            // not all platforms support DualMode.
            // run tests without it so they work on all platforms.
            DualMode: false,
            Interval: 1, // 1ms so at interval code at least runs.
            Timeout: 2000,

            // large window sizes so large messages are flushed with very few
            // update calls. otherwise tests take too long.
            SendWindowSize: Kcp.WND_SND * 1000,
            ReceiveWindowSize: Kcp.WND_RCV * 1000,

            // congestion window _heavily_ restricts send/recv window sizes
            // sending a max sized message would require thousands of updates.
            CongestionWindow: false,

            // maximum retransmit attempts until dead_link detected
            // default * 2 to check if configuration works
            MaxRetransmits: Kcp.DEADLINK * 2
        );

        public KcpNetOption(string host, int port, KcpChannel kcpChannel = KcpChannel.Reliable, int autoReconnectCount = -1) : base(autoReconnectCount)
        {
            this.host = host;
            this.port = port;
            this.kcpChannel = kcpChannel;
        }

        public KcpNetOption(string host, int port, KcpConfig config, KcpChannel kcpChannel = KcpChannel.Reliable, int autoReconnectCount = -1) : this(host, port, kcpChannel, autoReconnectCount)
        {
            this.config = config;
        }
    }
}
