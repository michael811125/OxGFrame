namespace OxGFrame.NetFrame
{
    public abstract class NetOption
    {
        /// <summary>
        /// Number of automatic reconnection attempts. Set to -1 to disable reconnection.
        /// </summary>
        public int autoReconnectCount { get; set; }

        public NetOption(int autoReconnectCount = -1)
        {
            this.autoReconnectCount = autoReconnectCount;
        }
    }
}