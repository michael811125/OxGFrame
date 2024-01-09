namespace OxGFrame.NetFrame
{
    public class WebsocketNetOption : NetOption
    {
        public string url { get; set; }

        public WebsocketNetOption(string url, int autoReconnectCount = -1) : base(autoReconnectCount)
        {
            this.url = url;
        }
    }
}