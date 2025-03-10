namespace OxGFrame.NetFrame
{
    public class WebSocketNetOption : NetOption
    {
        public string url { get; set; }

        public WebSocketNetOption(string url, int autoReconnectCount = -1) : base(autoReconnectCount)
        {
            this.url = url;
        }
    }
}