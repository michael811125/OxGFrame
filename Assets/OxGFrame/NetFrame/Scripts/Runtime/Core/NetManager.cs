using OxGKit.LoggingSystem;
using OxGKit.TimeSystem;
using System.Collections.Generic;
using System.Linq;

namespace OxGFrame.NetFrame
{
    internal class NetManager
    {
        /// <summary>
        /// NetNode 緩存
        /// </summary>
        private Dictionary<int, NetNode> _netNodes = null;

        /// <summary>
        /// Updater 驅動器
        /// </summary>
        internal RTUpdater rtUpdater = null;

        private static readonly object _locker = new object();
        private static NetManager _instance = null;

        public static NetManager GetInstance()
        {
            if (_instance == null)
            {
                lock (_locker)
                {
                    _instance = new NetManager();
                }
            }
            return _instance;
        }

        public NetManager()
        {
            this._netNodes = new Dictionary<int, NetNode>();
            this.rtUpdater = new RTUpdater();
            this.rtUpdater.onUpdate = this._OnUpdate;
            this.rtUpdater.Start();
        }

        private void _OnUpdate(float dt)
        {
            if (this._netNodes.Count > 0)
            {
                var nodes = this._netNodes.Values.ToArray();
                foreach (var node in nodes)
                {
                    if (this._netNodes.Count != nodes.Length) break;
                    else if (node == null) continue;
                    node.OnUpdate();
                }
            }
        }

        /// <summary>
        /// Number of network nodes
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            return this._netNodes.Count;
        }

        /// <summary>
        /// Adds a network node
        /// </summary>
        /// <param name="netNode">The net node to add</param>
        /// <param name="nnId">The ID of the net node</param>
        public void AddNetNode(NetNode netNode, int nnId)
        {
            if (!this._netNodes.ContainsKey(nnId))
            {
                this._netNodes.Add(nnId, netNode);
            }
            else
            {
                this._netNodes[nnId].Dispose();
                this._netNodes[nnId] = netNode;
            }
        }

        /// <summary>
        /// Removes a network node
        /// </summary>
        /// <param name="nnId">The ID of the net node to remove</param>
        public void RemoveNetNode(int nnId)
        {
            if (this._netNodes.ContainsKey(nnId))
            {
                this._netNodes[nnId].Dispose();
                this._netNodes.Remove(nnId);
            }
        }

        /// <summary>
        /// Retrieves a network node
        /// </summary>
        /// <param name="nnId">The ID of the net node to retrieve</param>
        /// <returns>The net node with the specified ID, or null if it does not exist</returns>
        public NetNode GetNetNode(int nnId)
        {
            if (this._netNodes.ContainsKey(nnId))
                return this._netNodes[nnId];
            return null;
        }

        /// <summary>
        /// Opens a connection for a specified network node
        /// </summary>
        /// <param name="netOption">The options for the connection</param>
        /// <param name="nnId">The ID of the network node to connect</param>
        public void Connect(NetOption netOption, int nnId)
        {
            if (this._netNodes.ContainsKey(nnId))
                this._netNodes[nnId].Connect(netOption);
            else
                Logging.PrintError<Logger>($"The NodeId: {nnId} can't be found! Connection failed.");
        }

        /// <summary>
        /// Checks the connection state of a network node
        /// </summary>
        /// <param name="nnId">The ID of the network node to check</param>
        /// <returns>True if the network node is connected, false otherwise</returns>
        public bool IsConnected(int nnId)
        {
            if (this._netNodes.ContainsKey(nnId))
            {
                if (this._netNodes[nnId] == null)
                    return false;
                return this._netNodes[nnId].IsConnected();
            }
            return false;
        }

        /// <summary>
        /// Sends binary data to a network node
        /// </summary>
        /// <param name="buffer">The binary data to send</param>
        /// <param name="nnId">The ID of the network node to send the data to</param>
        /// <returns>True if the data was sent successfully, false otherwise</returns>
        public bool Send(byte[] buffer, int nnId)
        {
            if (this._netNodes.ContainsKey(nnId))
            {
                return this._netNodes[nnId].Send(buffer);
            }
            else
            {
                Logging.PrintError<Logger>($"The NodeId: {nnId} can't be found! Send failed.");
                return false;
            }
        }

        /// <summary>
        /// Sends text data to a network node
        /// </summary>
        /// <param name="text">The text data to send</param>
        /// <param name="nnId">The ID of the network node to send the data to</param>
        /// <returns>True if the data was sent successfully, false otherwise</returns>
        public bool Send(string text, int nnId)
        {
            if (this._netNodes.ContainsKey(nnId))
            {
                return this._netNodes[nnId].Send(text);
            }
            else
            {
                Logging.PrintError<Logger>($"The NodeId: {nnId} can't be found! Send failed.");
                return false;
            }
        }

        /// <summary>
        /// Close a network
        /// </summary>
        /// <param name="nnId"></param>
        /// <param name="removeNetNode"></param>
        public void Close(int nnId, bool removeNetNode)
        {
            if (this._netNodes.ContainsKey(nnId))
            {
                this._netNodes[nnId].Close();
                if (removeNetNode)
                    this.RemoveNetNode(nnId);
            }
        }

        /// <summary>
        /// Close all networks
        /// </summary>
        /// <param name="removeNetNode"></param>
        public void CloseAll(bool removeNetNode)
        {
            foreach (var nnId in this._netNodes.Keys.ToArray())
                this.Close(nnId, removeNetNode);
        }
    }
}