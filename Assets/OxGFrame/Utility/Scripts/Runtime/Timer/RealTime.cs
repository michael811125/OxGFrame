using System;
using UnityEngine;

namespace OxGFrame.Utility.Timer
{
    public static class RealTime
    {
        private static bool _firstInitStartupTime = false;                  // 是否首次初始啟動時間                            
        public static DateTime? startupTime { get; private set; } = null;   // 啟動時間, 建議由主程序調用初始 (將會是遊戲啟動時間)

        public static void InitStartupTime()
        {
            if (_firstInitStartupTime) return;

            startupTime = DateTime.Now;

            _firstInitStartupTime = true;
        }

        public static bool IsInitStartupTime()
        {
            if (startupTime == null)
            {
                Debug.Log("<color=#FF0000>Please call <color=#FFB800>RealTime.InitStartupTime()</color> in main program first</color>");
                return false;
            }

            return true;
        }
    }
}