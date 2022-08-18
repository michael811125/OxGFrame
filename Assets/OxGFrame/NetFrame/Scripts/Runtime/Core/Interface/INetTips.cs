using System;

namespace OxGFrame.NetFrame
{
    public interface INetTips
    {
        /// <summary>
        /// 連線中
        /// </summary>
        void OnConnecting();

        /// <summary>
        /// 已連線
        /// </summary>
        /// <param name="e"></param>
        void OnConnected(EventArgs e);

        /// <summary>
        /// 連線錯誤
        /// </summary>
        /// <param name="msg"></param>
        void OnConnectionError(string msg);

        /// <summary>
        /// 關閉連線
        /// </summary>
        /// <param name="code"></param>
        void OnDisconnected(ushort code);

        /// <summary>
        /// 自動重連
        /// </summary>
        void OnReconnecting();
    }
}
