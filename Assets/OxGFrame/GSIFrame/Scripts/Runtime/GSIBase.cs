﻿using Cysharp.Threading.Tasks;

namespace OxGFrame.GSIFrame
{
    public abstract class GSIBase
    {
        /// <summary>
        /// 辨識碼
        /// </summary>
        public int id { get; private set; }

        /// <summary>
        /// 是否運行 Update 的開關
        /// </summary>
        public bool runUpdate { get; private set; }

        /// <summary>
        /// 初始標記
        /// </summary>
        private bool _isInitialized = false;

        /// <summary>
        /// 設置 Id
        /// </summary>
        /// <param name="id"></param>
        public void SetId(int id)
        {
            this.id = id;
        }

        /// <summary>
        /// 開始進行初始流程
        /// </summary>
        public async UniTaskVoid BeginInit()
        {
            if (!this._isInitialized)
            {
                this._isInitialized = true;
                await this.OnCreate();
            }

            this.StopUpdate();    // >> Step 1 << 暫停 Stage 刷新
            await this.OnEnter(); // >> Step 2 << 進行 Stage 初始
            this.RunUpdate();     // >> Step 3 << 開始 Stage 刷新
        }

        /// <summary>
        /// 運行刷新 OnUpdate
        /// </summary>
        public void RunUpdate()
        {
            this.runUpdate = true;
        }

        /// <summary>
        /// 停止刷新 OnUpdate
        /// </summary>
        public void StopUpdate()
        {
            this.runUpdate = false;
        }

        #region Implementation Methods
        /// <summary>
        /// 子類實作 OnCreate 方法
        /// </summary>
        /// <returns></returns>
        public abstract UniTask OnCreate();

        /// <summary>
        /// 子類實作 OnEnter 方法
        /// </summary>
        public abstract UniTask OnEnter();

        /// <summary>
        /// 子類實作 OnUpdate
        /// </summary>
        /// <param name="dt"></param>
        public abstract void OnUpdate(float dt = 0.0f);

        /// <summary>
        /// 子類實作 OnExit
        /// </summary>
        public abstract void OnExit();
        #endregion
    }

}
