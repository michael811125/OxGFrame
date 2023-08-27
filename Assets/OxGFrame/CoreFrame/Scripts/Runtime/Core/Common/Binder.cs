using OxGFrame.CoreFrame.CPFrame;
using OxGKit.LoggingSystem;
using UnityEngine;

namespace OxGFrame.CoreFrame
{
    public static class Binder
    {
        /// <summary>
        /// 由 FrameBase 調用綁定
        /// </summary>
        /// <param name="fBase"></param>
        public static void BindComponent(FrameBase fBase)
        {
            _BindNode(fBase.gameObject, fBase);
        }

        #region Bind
        /// <summary>
        /// 開始遞迴綁定
        /// </summary>
        /// <param name="go"></param>
        /// <param name="fBase"></param>
        private static bool _BindNode(GameObject go, FrameBase fBase)
        {
            string name = go.name;

            // 檢查是否要結束綁定, 有檢查到【BIND_STOP_END】時, 則停止繼續搜尋綁定物件
            if (CheckNodeHasStopEnd(name))
            {
                // 在 Runtime 時, 還原字串 (主要是在 Transform.Find 時, 可以無視 BIND_STOP_END 標籤)
                go.name = go.name.Replace(FrameConfig.BIND_STOP_END, string.Empty);
                return false;
            }

            // 這邊檢查有【BIND_PREFIXES】時, 則進入判斷
            if (CheckNodeHasPrefix(name, fBase))
            {
                _BindIntoCollector(name, go, fBase);
            }

            // 依序綁定下一個子物件 (遞迴找到符合綁定條件)
            foreach (Transform child in go.GetComponentInChildren<Transform>())
            {
                if (!_BindNode(child.gameObject, fBase)) return false;
            }

            return true;
        }

        private static void _BindIntoCollector(string name, GameObject go, FrameBase fBase)
        {
            // 綁定開頭檢測
            string[] heads = GetHeadSplitNameBySeparator(name);

            string bindType = heads[0]; // 綁定類型(會去查找 dictComponentFinder 裡面有沒有符合的類型)
            string bindName = heads[1]; // 要成為取得綁定物件後的Key

            // 再去判斷取得後的字串陣列是否綁定格式資格
            if (heads == null || heads.Length < 2 || !FrameConfig.BIND_COMPONENTS.ContainsKey(bindType))
            {
                Logging.Print<Logger>($"{name} => Naming format error. Please check the bind name.");
                return;
            }

            // 找到對應的綁定類型後, 進行綁定
            if (FrameConfig.BIND_COMPONENTS[bindType] == "GameObject")
            {
                // 綁定至 FrameBase 中對應的容器, 此時進行完成綁定
                fBase.collector.AddNode(bindName, go);
            }
        }
        #endregion

        #region Helps
        /// <summary>
        /// 檢查是否有 +【BIND_STOP_END】後綴字 (表示停止向下綁定)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool CheckNodeHasStopEnd(string name)
        {
            if (name.Substring(name.Length - 1) == FrameConfig.BIND_STOP_END) return true;
            return false;
        }

        /// <summary>
        /// 檢查是否有 +【BIND_PREFIXES】前綴字 (表示想要進行綁定)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static bool CheckNodeHasPrefix(string name, FrameBase fBase)
        {
            switch (fBase)
            {
                case CPBase:
                    if (name.Substring(0, 1) == FrameConfig.BIND_PREFIXES[1]) return true;
                    break;
                default:
                    if (name.Substring(0, 1) == FrameConfig.BIND_PREFIXES[0]) return true;
                    break;
            }

            return false;
        }

        public static bool CheckNodeHasPrefix(string name)
        {
            if (name.Substring(0, 1) == FrameConfig.BIND_PREFIXES[0] ||
                name.Substring(0, 1) == FrameConfig.BIND_PREFIXES[1]) return true;

            return false;
        }

        /// <summary>
        /// 透過【BIND_HEAD_SEPARATOR】去 Split 字串, 返回取得字串陣列
        /// ※備註: (Example) _Node@MyObj => ["_Node", "MyObj"]
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string[] GetHeadSplitNameBySeparator(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            return name.Split(FrameConfig.BIND_HEAD_SEPARATOR);
        }

        /// <summary>
        /// 透過【BIND_TAIL_SEPARATOR】去 Split 字串, 返回取得字串陣列
        /// ※備註: (Example) _Node@MyObj*Txt => ["MyObj", "Txt"]
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string[] GetTailSplitNameBySeparator(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            return name.Split(FrameConfig.BIND_TAIL_SEPARATOR);
        }
        #endregion
    }
}