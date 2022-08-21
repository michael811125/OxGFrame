using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace OxGFrame.Utility.Timer
{
    public class RTUpdate
    {
        public delegate void RealTimeUpdate(float dt);
        public delegate void RealTimeFixedUpdate(float dt);

        public RealTimeUpdate onUpdate = null;
        public RealTimeFixedUpdate onFixedUpdate = null;
        public float realtimeSinceStartup { get; protected set; } // 自遊戲啟動以來的時間
        public float timeAtLastFrame { get; protected set; }      // 記錄最後一幀的時間
        protected float _timeScale = 1f;                          // 時間尺度, 預設 = 1
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

        protected const float FIXED_FRAME = 60;                   // 固定幀數 (固定1秒刷新60次, 毫秒單位 => 1000 ms / 60 = 16 ms, 秒數單位 => 1 s / 60 = 0.016 s)
        protected const float MAX_TIMESCALE = 10;

        public void StartUpdate()
        {
            if (!RealTime.IsInitStartupTime()) return;

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
            await UniTask.WaitUntil(() => { return this.timeScale > 0f; }, PlayerLoopTiming.Update, (cts == null) ? default : cts.Token);

            // 幀數率
            float frameRate = FIXED_FRAME * this.timeScale;
            // 計算fixedDeltaTime
            this.fixedDeltaTime = 1 / frameRate;

            await UniTask.Delay(TimeSpan.FromSeconds(this.fixedDeltaTime), true, PlayerLoopTiming.Update, (cts == null) ? default : cts.Token);

            this.onFixedUpdate?.Invoke(this.fixedDeltaTime);

            // 計算deltaTime
            this.deltaTime = this.realtimeSinceStartup - this.timeAtLastFrame;
            this.timeAtLastFrame = this.realtimeSinceStartup;

            // 計算經過的現實時間, 當前時間 - 最一開始的時間 = 遊戲啟動到現在的經過時間 (換算為【秒】)
            var timeSpan = DateTime.Now.Subtract((DateTime)RealTime.startupTime);
            this.realtimeSinceStartup = (float)timeSpan.TotalSeconds;

            this.onUpdate?.Invoke(this.deltaTime);

            SetInterval(cts).Forget();
        }
    }
}