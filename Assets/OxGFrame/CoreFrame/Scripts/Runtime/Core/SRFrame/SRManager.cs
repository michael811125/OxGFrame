using System.Linq;
using OxGFrame.AssetLoader;
using Cysharp.Threading.Tasks;
using UnityEngine;
using OxGKit.LoggingSystem;

namespace OxGFrame.CoreFrame.SRFrame
{
    internal class SRManager : FrameManager<SRBase>
    {
        private static readonly object _locker = new object();
        private static SRManager _instance = null;
        public static SRManager GetInstance()
        {
            if (_instance == null)
            {
                lock (_locker)
                {
                    _instance = FindObjectOfType(typeof(SRManager)) as SRManager;
                    if (_instance == null) _instance = new GameObject(nameof(SRManager)).AddComponent<SRManager>();
                }
            }
            return _instance;
        }

        private void Awake()
        {
            string newName = $"[{nameof(SRManager)}]";
            this.gameObject.name = newName;
            if (this.gameObject.transform.root.name == newName)
            {
                var container = GameObject.Find(nameof(OxGFrame));
                if (container == null) container = new GameObject(nameof(OxGFrame));
                this.gameObject.transform.SetParent(container.transform);
                DontDestroyOnLoad(container);
            }
            else DontDestroyOnLoad(this.gameObject.transform.root);
        }

        #region 實作 Loading
        protected override SRBase Instantiate(SRBase srBase, string assetName, AddIntoCache addIntoCache, Transform parent)
        {
            GameObject instPref = Instantiate(srBase.gameObject, (parent != null) ? parent : this.transform);

            // 激活檢查, 如果主體 Active 為 false 必須打開
            if (!instPref.activeSelf) instPref.SetActive(true);

            // Replace Name
            instPref.name = instPref.name.Replace("(Clone)", "");
            srBase = instPref.GetComponent<SRBase>();
            if (srBase == null) return null;

            addIntoCache?.Invoke(srBase);

            srBase.SetNames(assetName);
            // Clone 取得 SRBase 組件後, 也初始 SRBase 相關設定
            srBase.OnInit();
            // Clone 取得 SRBase 組件後, 也初始 SRBase 相關綁定組件設定
            srBase.InitFirst();

            // >>> 需在 InitThis 之後, 以下設定開始生效 <<<

            // 透過 NodeName 設置 Parent
            this.SetParent(srBase, parent);

            // 最後設置完畢後, 關閉 GameObject 的 Active 為 false
            srBase.gameObject.SetActive(false);

            return srBase;
        }
        #endregion

        #region 相關校正與設置
        /// <summary>
        /// 依照對應的 Node 類型設置母節點
        /// </summary>
        /// <param name="srBase"></param>
        protected override bool SetParent(SRBase srBase, Transform parent)
        {
            if (parent != null)
            {
                if (parent.gameObject.GetComponent<SRParent>() == null) parent.gameObject.AddComponent<SRParent>();
                srBase.gameObject.transform.SetParent(parent);
                return true;
            }
            else
            {
                srBase.gameObject.transform.SetParent(this.gameObject.transform);
                return true;
            }
        }
        #endregion

        #region Show
        public override async UniTask<SRBase> Show(int groupId, string packageName, string assetName, object obj = null, string awaitingUIAssetName = null, Progression progression = null, Transform parent = null)
        {
            if (string.IsNullOrEmpty(assetName)) return null;

            // 先取出 Stack 主體
            var stack = this.GetStackFromAllCache(assetName);

            // 判斷非多實例直接 return
            if (stack != null && !stack.allowInstantiate)
            {
                if (this.CheckIsShowing(assetName))
                {
                    Debug.LogWarning($"SR: {assetName} already exists!!!");
                    return null;
                }
            }

            await this.ShowAwaiting(groupId, packageName, awaitingUIAssetName); // 開啟預顯加載 UI

            var srBase = await this.LoadIntoAllCache(packageName, assetName, progression, false, parent);
            if (srBase == null)
            {
                Debug.LogWarning($"SR: {assetName} => Asset not found at this path!!!");
                return null;
            }

            srBase.SetGroupId(groupId);
            srBase.SetHidden(false);
            await this.LoadAndDisplay(srBase, obj);

            Logging.Print<Logger>($"<color=#1effad>Show SR: <color=#ffdb1e>{assetName}</color></color>");

            this.CloseAwaiting(awaitingUIAssetName); // 執行完畢後, 關閉預顯加載 UI

            return srBase;
        }
        #endregion

        #region Close
        /// <summary>
        /// 將 Close 方法封裝 (由接口 Close 與 CloseAll 統一調用)
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="disablePreClose"></param>
        /// <param name="forceDestroy"></param>
        /// <param name="doAll"></param>
        private void _Close(string assetName, bool disablePreClose, bool forceDestroy, bool doAll)
        {
            if (string.IsNullOrEmpty(assetName) || !this.HasStackInAllCache(assetName)) return;

            if (doAll)
            {
                FrameStack<SRBase> stack = this.GetStackFromAllCache(assetName);
                foreach (var srBase in stack.cache.ToArray())
                {
                    srBase.SetHidden(false);
                    this.ExitAndHide(srBase, disablePreClose);

                    if (forceDestroy) this.Destroy(srBase, assetName);
                    else if (srBase.allowInstantiate) this.Destroy(srBase, assetName);
                    else if (srBase.onCloseAndDestroy) this.Destroy(srBase, assetName);
                }
            }
            else
            {
                SRBase srBase = this.PeekStackFromAllCache(assetName);
                if (srBase == null) return;

                srBase.SetHidden(false);
                this.ExitAndHide(srBase, disablePreClose);

                if (forceDestroy) this.Destroy(srBase, assetName);
                else if (srBase.allowInstantiate) this.Destroy(srBase, assetName);
                else if (srBase.onCloseAndDestroy) this.Destroy(srBase, assetName);
            }

            Logging.Print<Logger>($"<color=#1effad>Close SR: <color=#ffdb1e>{assetName}</color></color>");
        }

        public override void Close(string assetName, bool disablePreClose = false, bool forceDestroy = false)
        {
            // 如果沒有強制 Destroy + 不是顯示狀態則直接 return
            if (!forceDestroy && !this.CheckIsShowing(assetName)) return;
            this._Close(assetName, disablePreClose, forceDestroy, false);
        }

        public override void CloseAll(bool disablePreClose = false, bool forceDestroy = false, params string[] withoutAssetNames)
        {
            if (this._dictAllCache.Count == 0) return;

            foreach (FrameStack<SRBase> stack in this._dictAllCache.Values.ToArray())
            {
                // prevent preload mode
                if (stack.Count() == 0) continue;

                string assetName = stack.assetName;

                var srBase = stack.Peek();

                // 檢查排除執行的 SR
                bool checkWithout = false;
                if (withoutAssetNames.Length > 0)
                {
                    for (int i = 0; i < withoutAssetNames.Length; i++)
                    {
                        if (assetName == withoutAssetNames[i])
                        {
                            checkWithout = true;
                            break;
                        }
                    }
                }

                // 排除在外的 SR 直接略過處理
                if (checkWithout) continue;

                // 如果沒有強制 Destroy + 不是顯示狀態則直接略過處理
                if (!forceDestroy && !this.CheckIsShowing(srBase)) continue;

                // 如有啟用 CloseAll 需跳過開關, 則不列入關閉執行
                if (srBase.srSetting.whenCloseAllToSkip) continue;

                this._Close(assetName, disablePreClose, forceDestroy, true);
            }
        }

        public override void CloseAll(int groupId, bool disablePreClose = false, bool forceDestroy = false, params string[] withoutAssetNames)
        {
            if (this._dictAllCache.Count == 0) return;

            foreach (FrameStack<SRBase> stack in this._dictAllCache.Values.ToArray())
            {
                // prevent preload mode
                if (stack.Count() == 0) continue;

                string assetName = stack.assetName;

                var srBase = stack.Peek();

                if (srBase.groupId != groupId) continue;

                // 檢查排除執行的 SR
                bool checkWithout = false;
                if (withoutAssetNames.Length > 0)
                {
                    for (int i = 0; i < withoutAssetNames.Length; i++)
                    {
                        if (assetName == withoutAssetNames[i])
                        {
                            checkWithout = true;
                            break;
                        }
                    }
                }

                // 排除在外的 SR 直接略過處理
                if (checkWithout) continue;

                // 如果沒有強制 Destroy + 不是顯示狀態則直接略過處理
                if (!forceDestroy && !this.CheckIsShowing(srBase)) continue;

                // 如有啟用 CloseAll 需跳過開關, 則不列入關閉執行
                if (srBase.srSetting.whenCloseAllToSkip) continue;

                this._Close(assetName, disablePreClose, forceDestroy, true);
            }
        }
        #endregion

        #region Reveal
        /// <summary>
        /// 將 Reveal 方法封裝 (由接口 Reveal 與 RevealAll 統一調用)
        /// </summary>
        /// <param name="assetName"></param>
        private void _Reveal(string assetName)
        {
            if (string.IsNullOrEmpty(assetName) || !this.HasStackInAllCache(assetName)) return;

            if (this.CheckIsShowing(assetName))
            {
                Debug.LogWarning($"SR: {assetName} already reveal!!!");
                return;
            }

            FrameStack<SRBase> stack = this.GetStackFromAllCache(assetName);
            foreach (var srBase in stack.cache)
            {
                if (!srBase.isHidden) return;

                this.LoadAndDisplay(srBase).Forget();

                Logging.Print<Logger>($"<color=#1effad>Reveal SR: <color=#ffdb1e>{assetName}</color></color>");
            }
        }

        public override void Reveal(string assetName)
        {
            this._Reveal(assetName);
        }

        public override void RevealAll()
        {
            if (this._dictAllCache.Count == 0) return;

            foreach (FrameStack<SRBase> stack in this._dictAllCache.Values)
            {
                // prevent preload mode
                if (stack.Count() == 0) continue;

                string assetName = stack.assetName;

                var srBase = stack.Peek();

                if (!srBase.isHidden) continue;

                this._Reveal(assetName);
            }
        }

        public override void RevealAll(int groupId)
        {
            if (this._dictAllCache.Count == 0) return;

            foreach (FrameStack<SRBase> stack in this._dictAllCache.Values)
            {
                // prevent preload mode
                if (stack.Count() == 0) continue;

                string assetName = stack.assetName;

                var srBase = stack.Peek();

                if (srBase.groupId != groupId) continue;

                if (!srBase.isHidden) continue;

                this._Reveal(assetName);
            }
        }
        #endregion

        #region Hide
        /// <summary>
        /// 將 Hide 方法封裝 (由接口 Hide 與 HideAll 統一調用)
        /// </summary>
        /// <param name="assetName"></param>
        private void _Hide(string assetName)
        {
            if (string.IsNullOrEmpty(assetName) || !this.HasStackInAllCache(assetName)) return;

            FrameStack<SRBase> stack = this.GetStackFromAllCache(assetName);

            if (!this.CheckIsShowing(stack.Peek())) return;

            foreach (var srBase in stack.cache)
            {
                srBase.SetHidden(true);
                this.ExitAndHide(srBase);
            }

            Logging.Print<Logger>($"<color=#1effad>Hide SR: <color=#ffdb1e>{assetName}</color></color>");
        }

        public override void Hide(string assetName)
        {
            this._Hide(assetName);
        }

        public override void HideAll(params string[] withoutAssetNames)
        {
            if (this._dictAllCache.Count == 0) return;

            foreach (FrameStack<SRBase> stack in this._dictAllCache.Values)
            {
                // prevent preload mode
                if (stack.Count() == 0) continue;

                string assetName = stack.assetName;

                var srBase = stack.Peek();

                // 檢查排除執行的 SR
                bool checkWithout = false;
                if (withoutAssetNames.Length > 0)
                {
                    for (int i = 0; i < withoutAssetNames.Length; i++)
                    {
                        if (assetName == withoutAssetNames[i])
                        {
                            checkWithout = true;
                            break;
                        }
                    }
                }

                // 排除在外的 SR 直接略過處理
                if (checkWithout) continue;

                // 如有啟用 HideAll 需跳過開關, 則不列入關閉執行
                if (srBase.srSetting.whenHideAllToSkip) continue;

                this._Hide(assetName);
            }
        }

        public override void HideAll(int groupId, params string[] withoutAssetNames)
        {
            if (this._dictAllCache.Count == 0) return;

            foreach (FrameStack<SRBase> stack in this._dictAllCache.Values)
            {
                // prevent preload mode
                if (stack.Count() == 0) continue;

                string assetName = stack.assetName;

                var srBase = stack.Peek();

                if (srBase.groupId != groupId) continue;

                // 檢查排除執行的 SR
                bool checkWithout = false;
                if (withoutAssetNames.Length > 0)
                {
                    for (int i = 0; i < withoutAssetNames.Length; i++)
                    {
                        if (assetName == withoutAssetNames[i])
                        {
                            checkWithout = true;
                            break;
                        }
                    }
                }

                // 排除在外的 SR 直接略過處理
                if (checkWithout) continue;

                // 如有啟用 HideAll 需跳過開關, 則不列入關閉執行
                if (srBase.srSetting.whenHideAllToSkip) continue;

                this._Hide(assetName);
            }
        }
        #endregion

        #region 顯示場景 & 關閉場景
        protected async UniTask LoadAndDisplay(SRBase srBase, object obj = null)
        {
            if (!srBase.isHidden) await srBase.PreInit();
            srBase.Display(obj);
        }

        protected void ExitAndHide(SRBase srBase, bool disablePreClose = false)
        {
            srBase.Hide(disablePreClose);
        }
        #endregion
    }
}