namespace OxGFrame.NetFrame
{
    public static class NetFrames
    {
        public static void AddNetNode(NetNode netNode, byte nnId = 0)
        {
            NetManager.GetInstance().AddNetNode(netNode, nnId);
        }

        public static void RemoveNetNode(byte nnId = 0)
        {
            NetManager.GetInstance().RemoveNetNode(nnId);
        }

        public static NetNode GetNetNode(byte nnId = 0)
        {
            return NetManager.GetInstance().GetNetNode(nnId);
        }

        public static void Connect(NetOption netOption, byte nnId = 0)
        {
            NetManager.GetInstance().Connect(netOption, nnId);
        }

        public static bool IsConnected(byte nnId = 0)
        {
            return NetManager.GetInstance().IsConnected(nnId);
        }

        public static bool Send(byte[] buffer, byte nnId = 0)
        {
            return NetManager.GetInstance().Send(buffer, nnId);
        }

        public static bool Send(string text, byte nnId = 0)
        {
            return NetManager.GetInstance().Send(text, nnId);
        }

        public static void Close(byte nnId = 0, bool remove = false)
        {
            NetManager.GetInstance().Close(nnId, remove);
        }
    }
}