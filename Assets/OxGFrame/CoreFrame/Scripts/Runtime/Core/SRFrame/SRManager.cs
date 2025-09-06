using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader;
using OxGKit.LoggingSystem;
using System.Linq;
using UnityEngine;

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
                    if (_instance == null)
                        _instance = new GameObject(nameof(SRManager)).AddComponent<SRManager>();
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
                if (container == null)
                    container = new GameObject(nameof(OxGFrame));
                this.gameObject.transform.SetParent(container.transform);
                DontDestroyOnLoad(container);
            }
            else
                DontDestroyOnLoad(this.gameObject.transform.root);
        }

        #region 實作 Loading
        protected override SRBase Instantiate(SRBase srBase, string assetName, AddIntoCache addIntoCache, Transform parent)
        {
            // 暫存來源組件與標記 (用於還原來源組件設置)
            SRBase sourceComponent = srBase;
            bool sourceMonoDriveFlag = false;
            if (sourceComponent.monoDrive)
            {
                // 記錄標記
                sourceMonoDriveFlag = sourceComponent.monoDrive;
                // 動態加載時, 必須取消 monoDrive
                sourceComponent.monoDrive = false;
            }

            GameObject instGo = Instantiate(sourceComponent.gameObject, (parent != null) ? parent : this.transform);

            // 激活檢查, 如果主體 Active 為 false 必須打開
            if (!instGo.activeSelf)
                instGo.SetActive(true);

            // Replace Name
            instGo.name = instGo.name.Replace("(Clone)", "");
            // 取得 SRBase 組件
            srBase = instGo.GetComponent<SRBase>();
            if (srBase == null)
                return null;

            // 加入緩存
            addIntoCache?.Invoke(srBase);

            // 設置 assetName 作為 key
            srBase.SetNames(assetName);
            // Clone 取得 SRBase 組件後, 也初始 SRBase 相關設定
            srBase.OnCreate();
            // Clone 取得 SRBase 組件後, 也初始 SRBase 相關綁定組件設定
            srBase.InitFirst();

            // >>> 需在 InitThis 之後, 以下設定開始生效 <<<

            // 透過 NodeName 設置 Parent
            this.SetParent(srBase, parent);

            // 最後設置完畢後, 關閉 GameObject 的 Active 為 false
            srBase.gameObject.SetActive(false);

            // 還原來源設置
            if (sourceMonoDriveFlag)
                sourceComponent.monoDrive = true;

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
                if (parent.gameObject.GetComponent<SRParent>() == null)
                    parent.gameObject.AddComponent<SRParent>();
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
        public override async UniTask<SRBase> Show(int groupId, string packageName, string assetName, object obj = null, string awaitingUIAssetName = null, float awaitingUIExtraDuration = 0f, uint priority = 0, Progression progression = null, Transform parent = null)
        {
            if (string.IsNullOrEmpty(assetName))
                return null;

            // 先取出 Stack 主體
            var stack = this.GetStackFromAllCache(assetName);

            // 判斷非多實例直接 return
            if (stack != null && !stack.allowInstantiate)
            {
                if (this.CheckIsShowing(assetName))
                {
                    Logging.PrintWarning<Logger>($"SR: {assetName} already exists!!!");
                    return this.GetFrameComponent<SRBase>(assetName);
                }
            }

            // 開啟預顯加載 UI
            if (!string.IsNullOrEmpty(awaitingUIAssetName))
                await this.ShowAwaiting(groupId, packageName, awaitingUIAssetName, awaitingUIExtraDuration, priority);

            var srBase = await this.LoadIntoAllCache(packageName, assetName, priority, progression, false, parent);
            if (srBase == null)
            {
                Logging.PrintError<Logger>($"SR -> Asset not found at path or name: {assetName}");
                return null;
            }

            srBase.SetGroupId(groupId);
            srBase.SetHidden(false);
            await this.LoadAndDisplay(srBase, obj);

            Logging.PrintInfo<Logger>($"Show SR: {assetName}");

            // 執行完畢後, 關閉預顯加載 UI
            this.CloseAwaiting(awaitingUIAssetName);

            return srBase;
        }
        #endregion

        #region Close
        /// <summary>
        /// 將 Close 方法封裝 (由接口 Close 與 CloseAll 統一調用)
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="disabledPreClose"></param>
        /// <param name="forceDestroy"></param>
        /// <param name="doAll"></param>
        private void _Close(string assetName, bool disabledPreClose, bool forceDestroy, bool doAll)
        {
            if (string.IsNullOrEmpty(assetName) ||
                !this.HasStackInAllCache(assetName))
                return;

            if (doAll)
            {
                FrameStack<SRBase> stack = this.GetStackFromAllCache(assetName);
                foreach (var srBase in stack.cache.ToArray())
                {
                    srBase.SetHidden(false);
                    this.ExitAndHide(srBase, disabledPreClose);

                    if (forceDestroy)
                        this.Destroy(srBase, assetName);
                    else if (srBase.allowInstantiate)
                        this.Destroy(srBase, assetName);
                    else if (srBase.onCloseAndDestroy)
                        this.Destroy(srBase, assetName);
                }
            }
            else
            {
                SRBase srBase = this.PeekStackFromAllCache(assetName);
                if (srBase == null)
                    return;

                srBase.SetHidden(false);
                this.ExitAndHide(srBase, disabledPreClose);

                if (forceDestroy)
                    this.Destroy(srBase, assetName);
                else if (srBase.allowInstantiate)
                    this.Destroy(srBase, assetName);
                else if (srBase.onCloseAndDestroy)
                    this.Destroy(srBase, assetName);
            }

            Logging.PrintInfo<Logger>($"Close SR: {assetName}");
        }

        public override void Close(string assetName, bool disabledPreClose = false, bool forceDestroy = false)
        {
            // 如果沒有強制 Destroy + 不是顯示狀態則直接 return
            if (!forceDestroy &&
                !this.CheckIsShowing(assetName))
                return;
            this._Close(assetName, disabledPreClose, forceDestroy, false);
        }

        public override void CloseAll(int groupId, bool disabledPreClose = false, bool forceDestroy = false, bool forceCloseExcluded = false, params string[] withoutAssetNames)
        {
            if (this._dictAllCache.Count == 0)
                return;

            foreach (FrameStack<SRBase> stack in this._dictAllCache.Values.ToArray())
            {
                // prevent preload mode
                if (stack.Count() == 0)
                    continue;

                string assetName = stack.assetName;

                var srBase = stack.Peek();

                // 如果 -1 表示不管任何 groupId
                if (groupId != -1 &&
                    srBase.groupId != groupId)
                    continue;

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
                if (checkWithout)
                    continue;

                // 如果沒有強制 Destroy + 不是顯示狀態則直接略過處理
                if (!forceDestroy &&
                    !this.CheckIsShowing(srBase))
                    continue;

                // 如有啟用 CloseAll 需跳過開關, 則不列入關閉執行
                if (!forceCloseExcluded &&
                    srBase.srSetting.whenCloseAllToSkip)
                    continue;

                this._Close(assetName, disabledPreClose, forceDestroy, true);
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
            if (string.IsNullOrEmpty(assetName) ||
                !this.HasStackInAllCache(assetName))
                return;

            if (this.CheckIsShowing(assetName))
            {
                Logging.PrintWarning<Logger>($"SR: {assetName} already reveal!!!");
                return;
            }

            FrameStack<SRBase> stack = this.GetStackFromAllCache(assetName);
            foreach (var srBase in stack.cache)
            {
                if (!srBase.isHidden)
                    return;

                this.LoadAndDisplay(srBase).Forget();

                Logging.PrintInfo<Logger>($"Reveal SR: {assetName}");
            }
        }

        public override void Reveal(string assetName)
        {
            this._Reveal(assetName);
        }

        public override void RevealAll(int groupId)
        {
            if (this._dictAllCache.Count == 0)
                return;

            foreach (FrameStack<SRBase> stack in this._dictAllCache.Values)
            {
                // prevent preload mode
                if (stack.Count() == 0)
                    continue;

                string assetName = stack.assetName;

                var srBase = stack.Peek();

                // 如果 -1 表示不管任何 groupId
                if (groupId != -1 &&
                    srBase.groupId != groupId)
                    continue;

                if (!srBase.isHidden)
                    continue;

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
            if (string.IsNullOrEmpty(assetName) ||
                !this.HasStackInAllCache(assetName))
                return;

            FrameStack<SRBase> stack = this.GetStackFromAllCache(assetName);

            if (!this.CheckIsShowing(stack.Peek()))
                return;

            foreach (var srBase in stack.cache)
            {
                srBase.SetHidden(true);
                this.ExitAndHide(srBase);
            }

            Logging.PrintInfo<Logger>($"Hide SR: {assetName}");
        }

        public override void Hide(string assetName)
        {
            this._Hide(assetName);
        }

        public override void HideAll(int groupId, bool forceHideExcluded = false, params string[] withoutAssetNames)
        {
            if (this._dictAllCache.Count == 0)
                return;

            foreach (FrameStack<SRBase> stack in this._dictAllCache.Values)
            {
                // prevent preload mode
                if (stack.Count() == 0)
                    continue;

                string assetName = stack.assetName;

                var srBase = stack.Peek();

                // 如果 -1 表示不管任何 groupId
                if (groupId != -1 &&
                    srBase.groupId != groupId)
                    continue;

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
                if (checkWithout)
                    continue;

                // 如有啟用 HideAll 需跳過開關, 則不列入關閉執行
                if (!forceHideExcluded && srBase.srSetting.whenHideAllToSkip)
                    continue;

                this._Hide(assetName);
            }
        }
        #endregion

        #region 顯示場景 & 關閉場景
        protected async UniTask LoadAndDisplay(SRBase srBase, object obj = null)
        {
            if (!srBase.isHidden)
                await srBase.PreInit();
            srBase.Display(obj);
        }

        protected void ExitAndHide(SRBase srBase, bool disabledPreClose = false)
        {
            srBase.Hide(disabledPreClose);
        }
        #endregion
    }
}