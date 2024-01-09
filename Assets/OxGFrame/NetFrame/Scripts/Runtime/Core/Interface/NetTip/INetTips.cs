using System;

namespace OxGFrame.NetFrame
{
    public interface INetTips
    {
        void OnConnecting();

        void OnConnected(object status);

        void OnConnectionError(string msg);

        void OnDisconnected(object status);

        void OnReconnecting();
    }
}