namespace OxGFrame.NetFrame
{
    public static class NetFrames
    {
        public static void AddNetNode(NetNode netNode, int nnId = 0)
        {
            NetManager.GetInstance().AddNetNode(netNode, nnId);
        }

        public static void RemoveNetNode(int nnId = 0)
        {
            NetManager.GetInstance().RemoveNetNode(nnId);
        }

        public static NetNode GetNetNode(int nnId = 0)
        {
            return NetManager.GetInstance().GetNetNode(nnId);
        }

        public static void Connect(NetOption netOption, int nnId = 0)
        {
            NetManager.GetInstance().Connect(netOption, nnId);
        }

        public static bool IsConnected(int nnId = 0)
        {
            return NetManager.GetInstance().IsConnected(nnId);
        }

        public static bool Send(byte[] buffer, int nnId = 0)
        {
            return NetManager.GetInstance().Send(buffer, nnId);
        }

        public static bool Send(string text, int nnId = 0)
        {
            return NetManager.GetInstance().Send(text, nnId);
        }

        public static void Close(int nnId = 0, bool remove = false)
        {
            NetManager.GetInstance().Close(nnId, remove);
        }
    }
}