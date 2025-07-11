namespace OxGFrame.NetFrame
{
    public interface INetTips
    {
        void OnConnecting();

        void OnConnected(object payload);

        void OnConnectionError(object payload);

        void OnDisconnected(object payload);

        void OnReconnecting();
    }
}