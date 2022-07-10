using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GSIFrame
{
    public class GSIM
    {
        protected Dictionary<byte, GStage> _dictGameStage = null;    // GameStage快取
        protected byte _incomingGstId = 0;                           // 用於紀錄Incoming GameStageId (新的GameStageId)
        public byte curGstId { get; private set; }                   // 紀錄當前的GameStageId
        public GStage curGameStage { get; private set; }             // 當前GameStage

        public GSIM()
        {
            this._dictGameStage = new Dictionary<byte, GStage>();

            this.curGstId = 0;
            this.curGameStage = null;
        }

        ~GSIM()
        {
            this.ReleaseAllGameStage();
        }

        /// <summary>
        /// 透過GameStageId返回對應的GameStage
        /// </summary>
        /// <param name="gstId">GameStageId</param>
        /// <returns></returns>
        public GStage GetGameStage(byte gstId)
        {
            if (!this._dictGameStage.ContainsKey(gstId)) return null;
            return this._dictGameStage[gstId];
        }

        /// <summary>
        /// 透過GameStageId返回對應的GameStage (泛型)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gstId"></param>
        /// <returns></returns>
        public T GetGameStage<T>(byte gstId) where T : GStage
        {
            if (!this._dictGameStage.ContainsKey(gstId)) return null;
            return (T)this._dictGameStage[gstId];
        }

        /// <summary>
        /// 加入建立好的GameStageId與GameStage
        /// </summary>
        /// <param name="gstId">GameStageId</param>
        /// <param name="gameStage"></param>
        public void AddGameStage(byte gstId, GStage gameStage)
        {
            if (this._dictGameStage.ContainsKey(gstId))
            {
#if UNITY_EDITOR
                Debug.Log("Fail to add GameStage. Already has same GameStageId in GameFrame Cache");
#endif
                return;
            }

            this._dictGameStage.Add(gstId, gameStage);
        }

        /// <summary>
        /// <para>
        /// 透過指定的GameStageId非立即切換GameStage
        /// </para>
        /// <para>
        /// 說明: 非立即切換GameStage的情況下, 將會交由UpdateGameStage更新依照下一幀數去切換
        /// </para>
        /// </summary>
        /// <param name="gstId">GameStageId</param>
        public void ChangeGameStage(byte gstId)
        {
            this._incomingGstId = gstId;

#if UNITY_EDITOR
            Debug.Log(string.Format("<color=#00B8FF> 【>>>>>> Change GameStage <<<<<<】 : {0}</color>", this._incomingGstId));
#endif
        }

        /// <summary>
        /// 透過指定的GameStageId立即切換GameStage
        /// </summary>
        /// <param name="gstId">GameStageId</param>
        public void ChangeGameStageImmediate(byte gstId)
        {
#if UNITY_EDITOR
            Debug.Log(string.Format("<color=#00B8FF> 【>>>>>> Change GameStage Immediate <<<<<<】 : {0}</color>", gstId));
#endif

            this.ReleaseGameStage();                        // 立即釋放原本的GameStage
            this.curGstId = this._incomingGstId = gstId;    // 立即指定curGstId & incomingGstId = gstId
            this.InitGameStage();                           // 立即開始進行GameStage初始的處理過程
        }

        /// <summary>
        /// 【由OnUpdate調用】處理GameStage要Update的過程
        /// </summary>
        /// <param name="dt"></param>
        public void UpdateGameStage(float dt = 0.0f)
        {
            // 非立即的切換GameStage的情況下會去判斷, 如果curGstId與incomingGstId不同的話, 就會進入切換
            if (this.curGstId != this._incomingGstId)
            {
                this.ReleaseGameStage();                // 釋放原本的GameStage
                this.curGstId = this._incomingGstId;    // 將incomingGstId記錄至curGstId
                this.InitGameStage();                   // 開始進行GameStage初始的處理過程
            }

            if (this.curGameStage == null) return;

            if (this.curGameStage.runUpdate) this.curGameStage.UpdateStage(dt); // 開始刷新當前的GameStage
        }

        /// <summary>
        /// 處理當前GameStage要初始的過程
        /// </summary>
        public void InitGameStage()
        {
            if (!this._dictGameStage.ContainsKey(this.curGstId))
            {
#if UNITY_EDITOR
                Debug.Log(string.Format("Fail to get GameStage: {0}", this.curGstId));
#endif
            }

            this.curGameStage = this.GetGameStage(this.curGstId);   // 透過當前的GameStageId取出GameStage, 並且指定至當前GameStage中
            this.curGameStage.StartInitStage().Forget();            // 開始進行GameStage初始流程
        }

        /// <summary>
        /// 執行當前GameStage釋放的相關程序
        /// </summary>
        public void ReleaseGameStage()
        {
            if (this.curGameStage == null) return;
            this.curGameStage.ReleaseStage();
        }

        /// <summary>
        /// 清除釋放所有GameStage, 直接清空dictGameStage
        /// </summary>
        public void ReleaseAllGameStage()
        {
            this._dictGameStage.Clear();
        }

        /// <summary>
        /// 子類實作, 並且透過主要的MonoBehaviour Start調用
        /// </summary>
        public virtual void OnStart() { }

        /// <summary>
        /// 子類實作, 並且透過主要的MonoBehaviour Update調用
        /// </summary>
        /// <param name="dt"></param>
        public virtual void OnUpdate(float dt = 0.0f)
        {
            this.UpdateGameStage(dt);
        }
    }
}