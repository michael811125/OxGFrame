using System;

namespace OxGFrame.NetFrame
{
    public interface ISocket
    {
        event EventHandler OnOpen;
        event EventHandler<byte[]> OnMessage;
        event EventHandler<string> OnError;
        event EventHandler<ushort> OnClose;

        /// <summary>
        /// 建立連線
        /// </summary>
        /// <param name="netOption"></param>
        void CreateConnect(NetOption netOption);
        
        /// <summary>
        /// 是否已連線
        /// </summary>
        /// <returns></returns>
        bool IsConnected();

        /// <summary>
        /// 傳送 BinaryData
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        bool Send(byte[] buffer);

        /// <summary>
        /// 關閉連線
        /// </summary>
        void Close();
    }

}
