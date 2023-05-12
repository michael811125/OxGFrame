using System.Collections.Generic;
using UnityEngine;

namespace OxGFrame.NetFrame
{
    public class NetManager
    {
        private Dictionary<byte, NetNode> _dictNetNode = null; // NetNode 緩存

        private static NetManager _instance = null;
        public static NetManager GetInstance()
        {
            if (_instance == null) _instance = new NetManager();
            return _instance;
        }

        public NetManager()
        {
            this._dictNetNode = new Dictionary<byte, NetNode>();
        }

        public void OnUpdate()
        {
            if (this._dictNetNode.Count == 0) return;

            foreach (NetNode netNode in this._dictNetNode.Values)
            {
                if (netNode == null) continue;
                netNode.OnUpdate();
            }
        }

        /// <summary>
        /// 透過 NetNodeID 方式註冊新增 NetNode
        /// </summary>
        /// <param name="netNode"></param>
        /// <param name="nnId">預設 = 0</param>
        public void AddNetNode(NetNode netNode, byte nnId = 0)
        {
            if (!this._dictNetNode.ContainsKey(nnId)) this._dictNetNode.Add(nnId, netNode);
            else this._dictNetNode[nnId] = netNode;
        }

        /// <summary>
        /// 透過 NetNodeID 移除 NetNode
        /// </summary>
        /// <param name="nnId">預設 = 0</param>
        public void RemoveNetNode(byte nnId = 0)
        {
            if (this._dictNetNode.ContainsKey(nnId)) this._dictNetNode.Remove(nnId);
        }

        /// <summary>
        /// 透過 NetNodeID 取得 NetNode
        /// </summary>
        /// <param name="nnId">預設 = 0</param>
        /// <returns></returns>
        public NetNode GetNetNode(byte nnId = 0)
        {
            if (this._dictNetNode.ContainsKey(nnId)) return this._dictNetNode[nnId];
            return null;
        }

        /// <summary>
        /// 設置 NetOption 並且透過 NetNodeID 指定 NetNode 進行連接
        /// </summary>
        /// <param name="netOption"></param>
        /// <param name="nnId">預設 = 0</param>
        public void Connect(NetOption netOption, byte nnId = 0)
        {
            if (this._dictNetNode.ContainsKey(nnId))
            {
                this._dictNetNode[nnId].Connect(netOption);
            }
            else Debug.LogWarning(string.Format("The NodeId: {0} Can't Found !!! Connect Failed.", nnId));
        }

        /// <summary>
        /// 透過 NetNodeID 取得 NetNode 的連線狀態
        /// </summary>
        /// <param name="nnId">預設 = 0</param>
        /// <returns></returns>
        public bool IsConnected(byte nnId = 0)
        {
            if (this._dictNetNode.ContainsKey(nnId))
            {
                if (this._dictNetNode[nnId] == null) return false;
                return this._dictNetNode[nnId].IsConnected();
            }

            return false;
        }
        /// <summary>
        /// 透過 NetNodeID 指定 NetNode 傳送資料至 Server
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="nnId">預設 = 0</param>
        /// <returns></returns>
        public bool Send(byte[] buffer, byte nnId = 0)
        {
            if (this._dictNetNode.ContainsKey(nnId))
            {
                return this._dictNetNode[nnId].Send(buffer);
            }
            else
            {
                Debug.LogWarning(string.Format("The NodeId: {0} Can't Found !!! Send Failed.", nnId));
                return false;
            }
        }

        /// <summary>
        /// 透過 NetNodeID 選擇要關閉的 NetNode
        /// </summary>
        /// <param name="nnId">預設 = 0</param>
        public void CloseSocket(byte nnId = 0)
        {
            if (this._dictNetNode.ContainsKey(nnId))
            {
                this._dictNetNode[nnId].CloseSocket();
                this._dictNetNode.Remove(nnId);
            }
        }
    }
}