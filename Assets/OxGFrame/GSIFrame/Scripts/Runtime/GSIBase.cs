using Cysharp.Threading.Tasks;

namespace OxGFrame.GSIFrame
{
    /// <summary>
    /// Abstract base class of a game stage
    /// Derive it and implement the OnCreate, OnEnter, OnUpdate, OnExit lifecycle driven by the GSI manager
    /// </summary>
    public abstract class GSIBase
    {
        /// <summary>
        /// Identifier of this game stage
        /// </summary>
        public int id { get; private set; }

        /// <summary>
        /// Switch that controls whether OnUpdate is executed
        /// </summary>
        public bool runUpdate { get; private set; }

        /// <summary>
        /// Initialization flag
        /// </summary>
        private bool _isInitialized = false;

        /// <summary>
        /// Set the identifier of this game stage
        /// </summary>
        /// <param name="id"></param>
        public void SetId(int id)
        {
            this.id = id;
        }

        /// <summary>
        /// Begin the init flow (runs OnCreate once on first init, stops update, awaits OnEnter, then resumes update)
        /// </summary>
        internal async UniTaskVoid BeginInit()
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
        /// Enable OnUpdate execution for this game stage
        /// </summary>
        public void RunUpdate()
        {
            this.runUpdate = true;
        }

        /// <summary>
        /// Disable OnUpdate execution for this game stage
        /// </summary>
        public void StopUpdate()
        {
            this.runUpdate = false;
        }

        #region Implementation Methods
        /// <summary>
        /// Implement stage creation logic; invoked once the first time this game stage is initialized (before OnEnter)
        /// </summary>
        /// <returns></returns>
        public abstract UniTask OnCreate();

        /// <summary>
        /// Implement stage entering logic; invoked each time this game stage becomes the current stage (OnUpdate resumes after it completes)
        /// </summary>
        /// <returns></returns>
        public abstract UniTask OnEnter();

        /// <summary>
        /// Implement stage update logic; invoked every frame by the manager while this game stage is current and runUpdate is enabled
        /// </summary>
        /// <param name="dt"></param>
        public abstract void OnUpdate(float dt = 0.0f);

        /// <summary>
        /// Implement stage exiting logic; invoked when the manager switches away from this game stage
        /// </summary>
        public abstract void OnExit();
        #endregion
    }

}
