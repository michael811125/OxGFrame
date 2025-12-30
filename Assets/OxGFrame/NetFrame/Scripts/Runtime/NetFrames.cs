namespace OxGFrame.NetFrame
{
    public static class NetFrames
    {
        /// <summary>
        /// Controls the update rate of NetManager's Updater
        /// </summary>
        /// <param name="timeScale"></param>
        public static void SetUpdaterTimeScale(float timeScale)
        {
            NetManager.GetInstance().rtUpdater.timeScale = timeScale;
        }

        /// <summary>
        /// Checks if the NetManager's Updater is running
        /// </summary>
        /// <returns></returns>
        public static bool IsUpdaterRunning()
        {
            return NetManager.GetInstance().rtUpdater.IsRunning();
        }

        /// <summary>
        /// Starts the NetManager's Updater
        /// </summary>
        public static void StartUpdater()
        {
            NetManager.GetInstance().rtUpdater.Start();
        }

        /// <summary>
        /// Stops the NetManager's Updater
        /// </summary>
        public static void StopUpdater()
        {
            NetManager.GetInstance().rtUpdater.Stop();
        }

        /// <summary>
        /// Resets the NetManager's Updater
        /// </summary>
        public static void ResetUpdater()
        {
            NetManager.GetInstance().ResetUpdater();
        }

        /// <summary>
        /// Number of network nodes
        /// </summary>
        /// <returns></returns>
        public static int Count()
        {
            return NetManager.GetInstance().Count();
        }

        /// <summary>
        /// Adds a network node
        /// </summary>
        /// <param name="netNode">The net node to add</param>
        /// <param name="nnId">The ID of the net node</param>
        public static void AddNetNode(NetNode netNode, int nnId = 0)
        {
            NetManager.GetInstance().AddNetNode(netNode, nnId);
        }

        /// <summary>
        /// Removes a network node
        /// </summary>
        /// <param name="nnId">The ID of the net node to remove</param>
        public static void RemoveNetNode(int nnId = 0)
        {
            NetManager.GetInstance().RemoveNetNode(nnId);
        }

        /// <summary>
        /// Retrieves a network node
        /// </summary>
        /// <param name="nnId">The ID of the net node to retrieve</param>
        /// <returns>The net node with the specified ID, or null if it does not exist</returns>
        public static NetNode GetNetNode(int nnId = 0)
        {
            return NetManager.GetInstance().GetNetNode(nnId);
        }

        /// <summary>
        /// Opens a connection for a specified network node
        /// </summary>
        /// <param name="netOption">The options for the connection</param>
        /// <param name="nnId">The ID of the network node to connect</param>
        public static void Connect(NetOption netOption, int nnId = 0)
        {
            NetManager.GetInstance().Connect(netOption, nnId);
        }

        /// <summary>
        /// Checks the connection state of a network node
        /// </summary>
        /// <param name="nnId">The ID of the network node to check</param>
        /// <returns>True if the network node is connected, false otherwise</returns>
        public static bool IsConnected(int nnId = 0)
        {
            return NetManager.GetInstance().IsConnected(nnId);
        }

        /// <summary>
        /// Sends binary data to a network node
        /// </summary>
        /// <param name="buffer">The binary data to send</param>
        /// <param name="nnId">The ID of the network node to send the data to</param>
        /// <returns>True if the data was sent successfully, false otherwise</returns>
        public static bool Send(byte[] buffer, int nnId = 0)
        {
            return NetManager.GetInstance().Send(buffer, nnId);
        }

        /// <summary>
        /// Sends text data to a network node
        /// </summary>
        /// <param name="text">The text data to send</param>
        /// <param name="nnId">The ID of the network node to send the data to</param>
        /// <returns>True if the data was sent successfully, false otherwise</returns>
        public static bool Send(string text, int nnId = 0)
        {
            return NetManager.GetInstance().Send(text, nnId);
        }

        /// <summary>
        /// Closes a node network
        /// </summary>
        /// <param name="nnId"></param>
        /// <param name="removeNetNode">Whether to remove the NetNode</param>
        public static void Close(int nnId = 0, bool removeNetNode = false)
        {
            NetManager.GetInstance().Close(nnId, removeNetNode);
        }

        /// <summary>
        /// Closes all node networks
        /// </summary>
        /// <param name="removeNetNode"></param>
        public static void CloseAll(bool removeNetNode = false)
        {
            NetManager.GetInstance().CloseAll(removeNetNode);
        }
    }
}