namespace OxGFrame.NetFrame
{
    public abstract class NetOption
    {
        public int autoReconnectCount { get; set; }

        public NetOption(int autoReconnectCount = -1)
        {
            this.autoReconnectCount = autoReconnectCount;
        }
    }
}