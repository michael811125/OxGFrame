using OxGKit.LoggingSystem;
using OxGKit.Utilities.Timer;
using System.Collections.Generic;
using System.Linq;

namespace OxGFrame.NetFrame
{
    internal class NetManager
    {
        private Dictionary<byte, NetNode> _netNodes = null; // NetNode 緩存
        private RTUpdater _rtUpdater = null;

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
            this._netNodes = new Dictionary<byte, NetNode>();
            this._rtUpdater = new RTUpdater();
            this._rtUpdater.onUpdate = this._OnUpdate;
            this._rtUpdater.Start();
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
                    node.OnUpdate(dt);
                }
            }
        }

        /// <summary>
        /// Add net node
        /// </summary>
        /// <param name="netNode"></param>
        /// <param name="nnId"></param>
        public void AddNetNode(NetNode netNode, byte nnId)
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
        /// Remove net node
        /// </summary>
        /// <param name="nnId"></param>
        public void RemoveNetNode(byte nnId)
        {
            if (this._netNodes.ContainsKey(nnId))
            {
                this._netNodes[nnId].Dispose();
                this._netNodes.Remove(nnId);
            }
        }

        /// <summary>
        /// Get net node
        /// </summary>
        /// <param name="nnId"></param>
        /// <returns></returns>
        public NetNode GetNetNode(byte nnId)
        {
            if (this._netNodes.ContainsKey(nnId))
                return this._netNodes[nnId];
            return null;
        }

        /// <summary>
        /// Open connection
        /// </summary>
        /// <param name="netOption"></param>
        /// <param name="nnId"></param>
        public void Connect(NetOption netOption, byte nnId)
        {
            if (this._netNodes.ContainsKey(nnId))
            {
                this._netNodes[nnId].Connect(netOption);
            }
            else Logging.PrintWarning<Logger>(string.Format("The NodeId: {0} Can't Found !!! Connect Failed.", nnId));
        }

        /// <summary>
        /// Connection state
        /// </summary>
        /// <param name="nnId"></param>
        /// <returns></returns>
        public bool IsConnected(byte nnId)
        {
            if (this._netNodes.ContainsKey(nnId))
            {
                if (this._netNodes[nnId] == null) return false;
                return this._netNodes[nnId].IsConnected();
            }
            return false;
        }

        /// <summary>
        /// Send binary data
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="nnId"></param>
        /// <returns></returns>
        public bool Send(byte[] buffer, byte nnId)
        {
            if (this._netNodes.ContainsKey(nnId))
            {
                return this._netNodes[nnId].Send(buffer);
            }
            else
            {
                Logging.PrintWarning<Logger>(string.Format("The NodeId: {0} Can't Found !!! Send Failed.", nnId));
                return false;
            }
        }

        /// <summary>
        /// Send text data
        /// </summary>
        /// <param name="text"></param>
        /// <param name="nnId"></param>
        /// <returns></returns>
        public bool Send(string text, byte nnId)
        {
            if (this._netNodes.ContainsKey(nnId))
            {
                return this._netNodes[nnId].Send(text);
            }
            else
            {
                Logging.PrintWarning<Logger>(string.Format("The NodeId: {0} Can't Found !!! Send Failed.", nnId));
                return false;
            }
        }

        /// <summary>
        /// Close network
        /// </summary>
        /// <param name="nnId"></param>
        /// <param name="remove"></param>
        public void Close(byte nnId, bool remove)
        {
            if (this._netNodes.ContainsKey(nnId))
            {
                this._netNodes[nnId].Close();
                if (remove) this.RemoveNetNode(nnId);
            }
        }
    }
}