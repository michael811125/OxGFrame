using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GSIFrame
{
    public class GStage
    {
        public byte gstId { get; private set; }         // GameStageId
        public bool runUpdate { get; private set; }     // 是否運行Update的開關

        public GStage(byte gstId)
        {
            this.gstId = gstId;
        }

        /// <summary>
        /// 返回是否為定義的GameStageId
        /// </summary>
        /// <param name="checkGstId"></param>
        /// <returns></returns>
        public bool IsStage(byte checkGstId)
        {
            return (this.gstId == checkGstId);
        }

        /// <summary>
        /// 開始進行初始流程 (Invoke By GameFrameBase)
        /// </summary>
        public async UniTaskVoid StartInitStage()
        {
            this.StopUpdateStage();  // >> Step 1 << 暫停Stage刷新
            this.ResetStage();       // >> Step 2 << 進行Stage重置
            await this.InitStage();  // >> Step 3 << 進行Stage初始
            this.RunUpdateStage();   // >> Step 4 << 開始Stage刷新
        }

        /// <summary>
        /// 運行刷新UpdateStage
        /// </summary>
        public void RunUpdateStage()
        {
            this.runUpdate = true;
        }

        /// <summary>
        /// 停止刷新UpdateStage
        /// </summary>
        public void StopUpdateStage()
        {
            this.runUpdate = false;
        }

        #region Implementation Methods
        /// <summary>
        /// 子類實作InitStage方法
        /// </summary>
        public async virtual UniTask InitStage() { }

        /// <summary>
        /// 子類實作ResetStage方法
        /// </summary>
        public virtual void ResetStage() { }

        /// <summary>
        /// 子類實作UpdateStage
        /// </summary>
        /// <param name="dt"></param>
        public virtual void UpdateStage(float dt = 0.0f) { }

        /// <summary>
        /// 子類實作ReleaseStage
        /// </summary>
        public virtual void ReleaseStage() { }
        #endregion
    }

}
