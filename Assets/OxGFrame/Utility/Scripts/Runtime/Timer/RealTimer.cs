using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace OxGFrame.Utility.Timer
{
    public class RealTimer
    {
        private bool _playing;
        private float _intervalTime;
        private float _pauseTime;

        private float _timerTime;
        private float _triggerTime;

        private float _tickTime;
        private float _lastTickTime;

        private float _mark;
        private float _speed;

        public RealTimer()
        {
            this.Reset();
        }

        public RealTimer(bool autoPlay)
        {
            this.Reset();
            if (autoPlay) this.Play();
        }

        ~RealTimer()
        {
        }

        public static async UniTask SetInterval(Action action, int milliseconds, CancellationTokenSource cts = null)
        {
            await UniTask.Delay(milliseconds, true, PlayerLoopTiming.Update, (cts == null) ? default : cts.Token);

            action?.Invoke();

            SetInterval(action, milliseconds, cts).Forget();
        }

        public void Reset()
        {
            this._playing = false;
            this._intervalTime = 0.0f;
            this._pauseTime = 0.0f;
            this._timerTime = 0.0f;
            this._triggerTime = 0.0f;
            this._tickTime = 0.0f;
            this._lastTickTime = 0.0f;
            this._mark = 0.0f;
            this._speed = 1.0f;
        }

        public float GetRealTime()
        {
            if (!RealTime.IsInitStartupTime()) return 0f;

            var timeSpan = DateTime.Now.Subtract((DateTime)RealTime.startupTime);
            return (float)timeSpan.TotalSeconds;
        }

        public float GetTime()
        {
            if (!this._playing) return this._pauseTime - this._intervalTime;
            return (this.GetRealTime() - this._intervalTime) * this._speed;
        }

        public void Pause()
        {
            if (!this._playing) return;
            this._pauseTime = this.GetRealTime();
            this._playing = false;
        }

        public void Stop()
        {
            this._playing = false;
            this._intervalTime = 0.0f;
            this._pauseTime = 0.0f;
            this._timerTime = 0.0f;
            this._triggerTime = 0.0f;
            this._tickTime = 0.0f;
            this._lastTickTime = 0.0f;
            this._mark = 0.0f;
        }

        public void Play()
        {
            this._intervalTime += this.GetRealTime() - this._pauseTime;
            this._playing = true;
        }

        public bool IsPause()
        {
            return !this._playing;
        }

        public bool IsPlaying()
        {
            return this._playing;
        }

        #region Timer, 依照設置的時間下去計時
        /// <summary>
        /// 設置要計時的秒數
        /// </summary>
        /// <param name="time"></param>
        public void SetTimer(float time)
        {
            this._timerTime = time;
            this._triggerTime = this.GetTime() + this._timerTime;
        }

        /// <summary>
        /// 計算觸發時間倒數計時, 如果超過設置的觸發時間將直接返回0
        /// </summary>
        /// <returns></returns>
        public float TimerCountdown()
        {
            float time = this.GetTime();
            if (time >= this._triggerTime) return 0.0f;
            return this._triggerTime - time;
        }

        /// <summary>
        /// 返回計時時間是否已經到了
        /// </summary>
        /// <returns></returns>
        public bool IsTimerTimeout()
        {
            if (this.GetTime() < this._triggerTime) return false;
            return true;
        }

        /// <summary>
        /// 取得Timer倒數計時的時間比率 1 遞減至 0, 0 = 時間到
        /// </summary>
        /// <returns></returns>
        public float GetTimerCountdownRatio()
        {
            float countdown = this.TimerCountdown();
            if (countdown <= 0.0f) return 0.0f;
            float ratio = countdown / this._timerTime;
            return ratio;
        }
        #endregion

        #region Tick, 持續依照Set的時間Tick
        /// <summary>
        /// 設置Tick時間, 當TickTimeout時還會持續循環Tick
        /// </summary>
        /// <param name="tick"></param>
        public void SetTick(float tick)
        {
            this._tickTime = tick;
            this._lastTickTime = this.GetTime() + this._tickTime;
        }

        /// <summary>
        /// 取得設置的Tick的時間
        /// </summary>
        /// <returns></returns>
        public float GetTick()
        {
            return this._tickTime;
        }

        /// <summary>
        /// Tick觸發時間倒數, 如果超過設置的觸發時間將直接返回0
        /// </summary>
        /// <returns></returns>
        public float TickCountdown()
        {
            float time = this.GetTime();
            if (time >= this._lastTickTime) return 0.0f;
            return this._lastTickTime - time;
        }

        /// <summary>
        /// 返回Tick時間是否已經到了
        /// </summary>
        /// <returns></returns>
        public bool IsTickTimeout()
        {
            float time = this.GetTime();
            if (time < this._lastTickTime) return false;
            this._lastTickTime = time + this._tickTime;
            return true;
        }

        /// <summary>
        /// 取得Tick倒數計時的時間比率 1 遞減至 0, 0 = 時間到
        /// </summary>
        /// <returns></returns>
        public float GetTickCountdownRatio()
        {
            float countdown = this.TickCountdown();
            if (countdown <= 0.0f) return 0.0f;
            float ratio = countdown / this._tickTime;
            return ratio;
        }
        #endregion

        #region Mark, 標記時間
        /// <summary>
        /// 設置標記時間
        /// </summary>
        public void SetMark()
        {
            this._mark = this.GetTime();
        }

        /// <summary>
        /// 取得標記時間
        /// </summary>
        /// <returns></returns>
        public float GetMark()
        {
            return this._mark;
        }

        /// <summary>
        /// 取得上次標記時的經過時間
        /// </summary>
        /// <returns></returns>
        public float GetElapsedMarkTime()
        {
            float time = this.GetTime();
            if (time == this._mark || time < this._mark) return 0.0f;
            return (time - this._mark);
        }
        #endregion

        /// <summary>
        /// 設置時間運轉速度
        /// </summary>
        /// <param name="speed"></param>
        public void SetSpeed(float speed)
        {
            this._speed = speed;
        }

        public float GetSpeed()
        {
            return this._speed;
        }
    }
}