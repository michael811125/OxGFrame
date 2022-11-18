using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace OxGFrame.Utility.Timer
{
    public class DTUpdate
    {
        public delegate void DeltaTimeUpdate(float dt);
        public delegate void DeltaTimeFixedUpdate(float dt);

        private bool _firstInitStartTime = false;                   // 是否首次初始啟動時間                         
        public DateTime? startTime { get; protected set; } = null;  // 啟動時間, 自行由主管理程序記錄

        public DeltaTimeUpdate onUpdate = null;
        public DeltaTimeFixedUpdate onFixedUpdate = null;
        public float timeSinceStartup { get; protected set; }       // 自啟動以來的時間
        public float timeAtLastFrame { get; protected set; }        // 記錄最後一幀的時間
        protected float _timeScale = 1f;                            // 時間尺度, 預設 = 1
        public float timeScale
        {
            get { return this._timeScale; }
            set
            {
                if (value >= MAX_TIMESCALE) this._timeScale = MAX_TIMESCALE;
                else if (value < 0f) this._timeScale = 0f;
                else this._timeScale = value;
            }
        }
        public float deltaTime { get; protected set; }
        public float fixedDeltaTime { get; protected set; }
        protected CancellationTokenSource _updateCts = null;

        protected const float FIXED_FRAME = 60;                     // 固定幀數 (固定 1 秒刷新 60 次, 毫秒單位 => 1000 ms / 60 = 16 ms, 秒數單位 => 1 s / 60 = 0.016 s)
        protected const float MAX_TIMESCALE = 10;

        public void StartUpdate()
        {
            if (!this._firstInitStartTime)
            {
                this.startTime = DateTime.Now;
                this._firstInitStartTime = true;
            }

            this._updateCts = new CancellationTokenSource();
            SetInterval(this._updateCts).Forget();
        }

        public void StopUpdate()
        {
            if (this._updateCts == null) return;
            this._updateCts.Cancel();
            this._updateCts.Dispose();
            this._updateCts = null;
        }

        protected async UniTask SetInterval(CancellationTokenSource cts)
        {
            await UniTask.WaitUntil(() => { return (Time.timeScale * this.timeScale) > 0f; }, PlayerLoopTiming.Update, (cts == null) ? default : cts.Token);

            // 幀數率
            float multiTimeScale = Time.timeScale * this.timeScale;
            float frameRate = FIXED_FRAME * multiTimeScale;
            // 計算 fixedDeltaTime
            this.fixedDeltaTime = 1 / frameRate;

            await UniTask.Delay(TimeSpan.FromSeconds(this.fixedDeltaTime), false, PlayerLoopTiming.Update, (cts == null) ? default : cts.Token);

            this.onFixedUpdate?.Invoke(this.fixedDeltaTime);

            // 計算 deltaTime
            this.deltaTime = this.timeSinceStartup - this.timeAtLastFrame;
            this.timeAtLastFrame = this.timeSinceStartup;

            // 計算經過的時間, 當前時間 - 最一開始的時間 = 啟動到現在的經過時間 (換算為【秒】)
            var timeSpan = DateTime.Now.Subtract((DateTime)this.startTime);
            this.timeSinceStartup = (float)timeSpan.TotalSeconds;

            this.onUpdate?.Invoke(this.deltaTime);

            SetInterval(cts).Forget();
        }
    }
}