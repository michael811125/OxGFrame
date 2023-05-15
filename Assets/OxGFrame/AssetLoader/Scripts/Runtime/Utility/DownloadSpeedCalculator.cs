using System;

namespace OxGFrame.AssetLoader.Utility
{
    public sealed class DownloadSpeedCalculator
    {
        public delegate void OnDownloadSpeedProgress(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes, long downloadSpeedBytes);

        public OnDownloadSpeedProgress onDownloadSpeedProgress;
        private DateTime _lastTime = default;
        private long _lastDownloadBytes = 0;
        private long _downloadSpeedBytes = 0;
        private long _lastDownloadSpeedBytes = 0;
        private bool _isDone = false;

        public void OnDownloadProgress(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes)
        {
            if (this._isDone)
            {
                this._Reset();
                return;
            }

            if (currentDownloadBytes >= totalDownloadBytes) this._isDone = true;

            var frag = currentDownloadBytes - this._lastDownloadBytes;
            this._lastDownloadBytes = currentDownloadBytes;
            this._Increment(frag);
            var speedBytes = this._GetSpeedBytes();
            this.onDownloadSpeedProgress?.Invoke(totalDownloadCount, currentDownloadCount, totalDownloadBytes, currentDownloadBytes, speedBytes);
        }

        private void _Reset()
        {
            this._lastTime = default;
            this._lastDownloadBytes = 0;
            this._downloadSpeedBytes = 0;
            this._lastDownloadSpeedBytes = 0;
            this._isDone = false;
        }

        private void _Increment(long bytes)
        {
            lock (this)
            {
                this._downloadSpeedBytes += bytes;
            }
        }

        private long _GetSpeedBytes()
        {
            lock (this)
            {
                if (DateTime.Now.Subtract(this._lastTime).TotalSeconds > 1)
                {
                    this._lastDownloadSpeedBytes = this._downloadSpeedBytes;
                    this._downloadSpeedBytes = 0;
                    this._lastTime = DateTime.Now;
                }
                return this._lastDownloadSpeedBytes;
            }
        }
    }
}