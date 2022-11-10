using Cysharp.Threading.Tasks;

namespace OxGFrame.GSIFrame
{
    public class GameStageBase
    {
        public byte gstId { get; private set; }         // GameStageId
        public bool runUpdate { get; private set; }     // 是否運行 Update 的開關

        public GameStageBase(byte gstId)
        {
            this.gstId = gstId;
        }

        /// <summary>
        /// 返回是否為定義的 GameStageId
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
            this.StopUpdateStage();  // >> Step 1 << 暫停 Stage 刷新
            this.ResetStage();       // >> Step 2 << 進行 Stage 重置
            await this.InitStage();  // >> Step 3 << 進行 Stage 初始
            this.RunUpdateStage();   // >> Step 4 << 開始 Stage 刷新
        }

        /// <summary>
        /// 運行刷新 UpdateStage
        /// </summary>
        public void RunUpdateStage()
        {
            this.runUpdate = true;
        }

        /// <summary>
        /// 停止刷新 UpdateStage
        /// </summary>
        public void StopUpdateStage()
        {
            this.runUpdate = false;
        }

        #region Implementation Methods
        /// <summary>
        /// 子類實作 InitStage 方法
        /// </summary>
        public async virtual UniTask InitStage() { }

        /// <summary>
        /// 子類實作 ResetStage 方法
        /// </summary>
        public virtual void ResetStage() { }

        /// <summary>
        /// 子類實作 UpdateStage
        /// </summary>
        /// <param name="dt"></param>
        public virtual void UpdateStage(float dt = 0.0f) { }

        /// <summary>
        /// 子類實作 ReleaseStage
        /// </summary>
        public virtual void ReleaseStage() { }
        #endregion
    }

}
