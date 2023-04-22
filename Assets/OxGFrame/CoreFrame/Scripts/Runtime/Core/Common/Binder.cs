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

        /// <summary>
        /// 開始遞迴綁定
        /// </summary>
        /// <param name="go"></param>
        /// <param name="fBase"></param>
        private static void _BindNode(GameObject go, FrameBase fBase)
        {
            string name = go.name;

            // 檢查是否要結束綁定, 有檢查到【BIND_END】時, 則停止繼續搜尋綁定物件
            if (_CheckIsToBindChildren(name))
            {
                // 只有 EntityBase 的綁定前綴字需要區分, 避免相衝
                if (typeof(EPFrame.EPBase).IsInstanceOfType(fBase) && _CheckNodeHasPrefixEntity(name))
                {
                    _BindIntoCollector(name, go, fBase);
                }
                // 這邊檢查有【BIND_PREFIX】時, 則進入判斷
                else if (_CheckNodeHasPrefix(name))
                {
                    _BindIntoCollector(name, go, fBase);
                }

                // 依序綁定下一個子物件 (遞迴找到符合綁定條件)
                foreach (Transform cTs in go.GetComponentInChildren<Transform>())
                {
                    _BindNode(cTs.gameObject, fBase);
                }
            }
        }

        /// <summary>
        /// 步驟1. 會先執行檢查是否要進行子節點綁定 (如果不進行則在最後面加上【BIND_END】後綴字, 就會停止執行以下步驟)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static bool _CheckIsToBindChildren(string name)
        {
            if (name.Substring(name.Length - 1) != FrameConfig.BIND_END.ToString()) return true;
            return false;
        }

        /// <summary>
        /// 步驟2. 檢查是否有+【BIND_PREFIX】前綴字 (表示想要進行綁定)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static bool _CheckNodeHasPrefix(string name)
        {
            if (name.Substring(0, 1) == FrameConfig.BIND_PREFIX.ToString()) return true;
            return false;
        }

        private static bool _CheckNodeHasPrefixEntity(string name)
        {
            if (name.Substring(0, 1) == FrameConfig.BIND_PREFIX_ENTITY.ToString()) return true;
            return false;
        }

        /// <summary>
        /// 步驟3. 透過【BIND_SEPARATOR】去 Split 字串, 返回取得字串陣列. 
        /// ※備註: (Example) _Node@MyObj => ["_Node", "MyObj"], 之後可以透過取得的陣列去查找表看是否有對應的組件需要綁定
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string[] _GetPrefixSplitNameBySeparator(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            return name.Split(FrameConfig.BIND_SEPARATOR);
        }

        private static void _BindIntoCollector(string name, GameObject go, FrameBase fBase)
        {
            // 之後再透過【BIND_SEPARATOR】去切開取得字串陣列
            string[] names = _GetPrefixSplitNameBySeparator(name);

            string bindType = names[0]; // 綁定類型(會去查找 dictComponentFinder 裡面有沒有符合的類型)
            string bindKey = names[1];  // 要成為取得綁定物件後的Key

            // 再去判斷取得後的字串陣列是否綁定格式資格
            if (names == null || names.Length < 2 || !FrameConfig.BIND_COMPONENTS.ContainsKey(bindType))
            {
                Debug.Log($"{name} => Naming format error. Please check the bind name.");
                return;
            }

            // 找到對應的綁定類型後, 進行綁定
            if (FrameConfig.BIND_COMPONENTS[bindType] == "GameObject")
            {
                // 綁定至 FrameBase 中對應的容器, 此時進行完成綁定
                fBase.collector.AddNode(bindKey, go);
            }
        }
    }
}