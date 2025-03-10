using System;

namespace OxGFrame.NetFrame
{
    public interface INetProvider
    {
        event EventHandler<object> OnOpen;

        event EventHandler<byte[]> OnBinary;

        event EventHandler<string> OnMessage;

        event EventHandler<string> OnError;

        event EventHandler<object> OnClose;

        /// <summary>
        /// Open connection
        /// </summary>
        /// <param name="netOption"></param>
        void CreateConnect(NetOption netOption);

        /// <summary>
        /// Connection state
        /// </summary>
        /// <returns></returns>
        bool IsConnected();

        /// <summary>
        /// Send binary data 
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        bool SendBinary(byte[] buffer);

        /// <summary>
        /// Send string data
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        bool SendMessage(string text);

        /// <summary>
        /// Update looping
        /// </summary>
        void OnUpdate();

        /// <summary>
        /// Close connection
        /// </summary>
        void Close();
    }
}
