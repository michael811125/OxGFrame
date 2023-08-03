﻿using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using OxGFrame.AssetLoader;

namespace OxGFrame.CoreFrame.UIFrame
{
    internal class UIManager : FrameManager<UIBase>
    {
        private struct ReverseCache
        {
            public UIBase uiBase;
            public object data;
            public int extraStack;
        }

        private Dictionary<string, UICanvas> _dictUICanvas = new Dictionary<string, UICanvas>();                    // Canvas 物件節點
        private Dictionary<string, int> _dictStackCounter = new Dictionary<string, int>();                          // 堆疊式計數緩存 (Key = id + NodeType)
        private Dictionary<string, List<ReverseCache>> _dictReverse = new Dictionary<string, List<ReverseCache>>(); // 反切緩存

        private static readonly object _locker = new object();
        private static UIManager _instance = null;
        internal static UIManager GetInstance()
        {
            if (_instance == null)
            {
                lock (_locker)
                {
                    _instance = FindObjectOfType(typeof(UIManager)) as UIManager;
                    if (_instance == null) _instance = new GameObject(nameof(UIManager)).AddComponent<UIManager>();
                }
            }
            return _instance;
        }

        private void Awake()
        {
            string newName = $"[{nameof(UIManager)}]";
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

        #region 初始建立 Node 相關方法
        /// <summary>
        /// 透過對應名稱查找物件, 並且設置與檢查是否有匹配的 UICanvas 環境
        /// </summary>
        /// <param name="canvasName"></param>
        private bool _SetupAndCheckUICanvas(string canvasName)
        {
            if (this._dictUICanvas.ContainsKey(canvasName)) return true;

            // 查找與 CanvasName 定義名稱一樣的 Canvas 物件
            GameObject goCanvas = null;
            foreach (var canvas in FindObjectsOfType(typeof(Canvas)) as Canvas[])
            {
                if (canvas.gameObject.name == canvasName)
                {
                    goCanvas = canvas.gameObject;
                    break;
                }
            }

            if (goCanvas != null)
            {
                // 取得或建立 UICanvas
                UICanvas uiCanvas = goCanvas.GetComponent<UICanvas>();
                if (uiCanvas == null) uiCanvas = goCanvas.AddComponent<UICanvas>();

                // 設置 UIRoot (parent = Canvas)
                var goUIRoot = this._CreateUIRoot(uiCanvas, UIConfig.UI_ROOT_NAME, goCanvas.transform);
                uiCanvas.uiRoot = goUIRoot;

                // 設置 UINode (parent = UIRoot)
                foreach (var nodeType in UIConfig.UI_NODES.Keys)
                {
                    if (!uiCanvas.uiNodes.ContainsKey(nodeType.ToString()))
                    {
                        uiCanvas.uiNodes.Add(nodeType.ToString(), this._CreateUINode(uiCanvas, nodeType, goUIRoot.transform));
                    }
                }

                // 建立 UIMask & UIFreeze 容器 (parent = UIRoot)
                GameObject uiMaskGo = this._CreateUIContainer(uiCanvas, UIConfig.UI_MASK_NAME, goUIRoot.transform);
                GameObject uiFreezeGo = this._CreateUIContainer(uiCanvas, UIConfig.UI_FREEZE_NAME, goUIRoot.transform);
                uiCanvas.SetMaskManager(new UIMaskManager(uiCanvas.gameObject.layer, uiMaskGo.transform));
                uiCanvas.SetFreezeManager(new UIFreezeManager(uiCanvas.gameObject.layer, uiFreezeGo.transform));

                this._dictUICanvas.Add(canvasName, uiCanvas);
            }
            else
            {
                Debug.Log($"<color=#FFD600>【Setup Failed】Not found UICanvas:</color> <color=#FF6ECB>{canvasName}</color>");
                return false;
            }

            return true;
        }

        private GameObject _CreateUIRoot(UICanvas uiCanvas, string name, Transform parent)
        {
            // 檢查是否已經有先被創立了
            GameObject uiRootChecker = parent.Find(name)?.gameObject;
            if (uiRootChecker != null) return uiRootChecker;

            GameObject uiRootGo = new GameObject(UIConfig.UI_ROOT_NAME, typeof(RectTransform));
            // 設置繼承主 Canvas 的 Layer
            uiRootGo.layer = uiCanvas.gameObject.layer;
            // 設置 uiRoot 為 Canvas 的子節點
            uiRootGo.transform.SetParent(parent);

            // 校正 RectTransform
            RectTransform uiRootRect = uiRootGo.GetComponent<RectTransform>();
            uiRootRect.anchorMin = Vector2.zero;
            uiRootRect.anchorMax = Vector2.one;
            uiRootRect.sizeDelta = Vector2.zero;
            uiRootRect.localScale = Vector3.one;
            uiRootRect.localPosition = Vector3.zero;

            return uiRootGo;
        }

        private GameObject _CreateUINode(UICanvas uiCanvas, NodeType nodeType, Transform parent)
        {
            // 檢查是否已經有先被創立了
            GameObject uiNodeChecker = parent.Find(nodeType.ToString())?.gameObject;
            if (uiNodeChecker != null) return uiNodeChecker;

            GameObject uiNodeGo = new GameObject(nodeType.ToString(), typeof(RectTransform), typeof(Canvas));
            // 設置繼承主 Canvas 的 Layer
            uiNodeGo.layer = uiCanvas.gameObject.layer;
            // 設置 uiNode 為 uiRoot 的子節點
            uiNodeGo.transform.SetParent(parent);

            // 校正 RectTransform
            RectTransform uiNodeRect = uiNodeGo.GetComponent<RectTransform>();
            uiNodeRect.anchorMin = Vector2.zero;
            uiNodeRect.anchorMax = Vector2.one;
            uiNodeRect.sizeDelta = Vector2.zero;
            uiNodeRect.localScale = Vector3.one;
            uiNodeRect.localPosition = Vector3.zero;

            // 設置 Canvas 參數 (會繼承於主 Canvas 設定 => SortingLayerName, AdditionalShaderChannels)
            Canvas uiNodeCanvas = uiNodeGo.GetComponent<Canvas>();
            Canvas mainCanvas = uiCanvas.GetComponent<Canvas>();
            uiNodeCanvas.overridePixelPerfect = true;
            uiNodeCanvas.pixelPerfect = true;
            uiNodeCanvas.overrideSorting = true;
            uiNodeCanvas.sortingLayerName = mainCanvas.sortingLayerName;
            uiNodeCanvas.sortingOrder = UIConfig.UI_NODES[nodeType];
            uiNodeCanvas.additionalShaderChannels = mainCanvas.additionalShaderChannels;

            return uiNodeGo;
        }

        private GameObject _CreateUIContainer(UICanvas uiCanvas, string name, Transform parent)
        {
            // 檢查是否已經有先被創立了
            GameObject uiContainerChecker = parent.Find(name)?.gameObject;
            if (uiContainerChecker != null) return uiContainerChecker;

            GameObject uiContainerGo = new GameObject(name, typeof(RectTransform));
            // 設置繼承主 Canvas 的 Layer
            uiContainerGo.layer = uiCanvas.gameObject.layer;
            // 設置 uiContainer 為 uiRoot 的子節點
            uiContainerGo.transform.SetParent(parent);

            // 校正 Transform
            uiContainerGo.transform.localScale = Vector3.one;
            uiContainerGo.transform.localPosition = Vector3.zero;

            return uiContainerGo;
        }
        #endregion

        /// <summary>
        /// 透過 CanvasType 取得對應的 UICanvas
        /// </summary>
        /// <param name="canvasName"></param>
        /// <returns></returns>
        public UICanvas GetUICanvas(string canvasName)
        {
            if (this._dictUICanvas.Count == 0) return null;

            if (this._dictUICanvas.TryGetValue(canvasName.ToString(), out var uiCanvas)) return uiCanvas;

            return null;
        }

        #region 實作 Loading
        protected override UIBase Instantiate(UIBase uiBase, string assetName, AddIntoCache addIntoCache, Transform parent)
        {
            // 先從來源物件中取得 UIBase, 需先取得 UICanvas 環境, 後續 Instantiate 時才能取得正常比例
            UICanvas uiCanvas = null;
            if (uiBase != null)
            {
                // 先檢查與設置 UICanvas 環境
                if (this._SetupAndCheckUICanvas(uiBase.uiSetting.canvasName))
                {
                    uiCanvas = this.GetUICanvas(uiBase.uiSetting.canvasName);
                }
            }

            if (uiCanvas == null)
            {
                Debug.Log($"<color=#FFD600>【Loading Failed】Not found UICanvas:</color> <color=#FF6ECB>{uiBase.uiSetting.canvasName}</color>");
                return null;
            }

            // 透過 RenderMode 區分預設父層級
            Transform rootParent;
            if (uiCanvas.canvas.renderMode == RenderMode.WorldSpace) rootParent = uiCanvas.transform;
            else rootParent = uiCanvas.uiRoot.transform;
            // instantiate 【UI Prefab】 (需先指定 Instantiate Parent 為 UIRoot 不然 Canvas 初始會跑掉)
            GameObject instPref = Instantiate(uiBase.gameObject, (parent != null) ? parent : rootParent);

            // 激活檢查, 如果主體 Active 為 false 必須打開
            if (!instPref.activeSelf) instPref.SetActive(true);

            // Replace Name
            instPref.name = instPref.name.Replace("(Clone)", "");
            // 取得 UIBase 組件
            uiBase = instPref.GetComponent<UIBase>();
            if (uiBase == null) return null;

            addIntoCache?.Invoke(uiBase);

            // 調整 Canvas 相關組件參數
            this._AdjustCanvas(uiCanvas, uiBase);

            uiBase.SetNames(assetName);
            // Clone 取得 UIBase 組件後, 也需要初始 UI 相關配置, 不然後面無法正常運作
            uiBase.OnInit();
            // Clone 取得 UIBase 組件後, 也需要初始 UI 相關綁定組件設定
            uiBase.InitFirst();

            // >>> 需在 InitThis 之後, 以下設定開始生效 <<<

            // 透過 UIFormType 類型, 設置 Parent
            if (!this.SetParent(uiBase, parent)) return null;
            // SortingOrder 設置需在 SetActive(false) 之前就設置, 基於 UIFormType 的階層去設置排序
            this._SetSortingOrder(uiBase);
            // 設置 UI 物件底下的 Renderer 組件為 UI 層, 使得 Renderer 為正確的 UI 階層與渲染順序
            this._SetRendererOrder(uiBase);

            // 最後設置完畢後, 關閉 GameObject 的 Active 為 false
            uiBase.gameObject.SetActive(false);

            return uiBase;
        }
        #endregion

        #region 相關校正與設置
        /// <summary>
        /// 依照對應的 Node 類型設置母節點
        /// </summary>
        /// <param name="uiBase"></param>
        protected override bool SetParent(UIBase uiBase, Transform parent)
        {
            if (parent != null)
            {
                if (parent.gameObject.GetComponent<UIParent>() == null) parent.gameObject.AddComponent<UIParent>();
                uiBase.gameObject.transform.SetParent(parent);
                return true;
            }
            else
            {
                var uiCanvas = this.GetUICanvas(uiBase.uiSetting.canvasName);
                if (uiCanvas == null)
                {
                    Debug.Log($"<color=#FF0068>When UI <color=#FF9000>[{uiBase.assetName}]</color> to set parent failed. Not found UICanvas:</color> <color=#FF9000>[{uiBase.uiSetting.canvasName}]</color>");
                    return false;
                }

                var goNode = uiCanvas.GetUINode(uiBase.uiSetting.nodeType);
                if (goNode != null)
                {
                    if (goNode.GetComponent<UIParent>() == null) goNode.AddComponent<UIParent>();
                    uiBase.gameObject.transform.SetParent(goNode.transform);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 調整 UIBase 的 Canvas 相關組件參數
        /// </summary>
        /// <param name="uiBase"></param>
        private void _AdjustCanvas(UICanvas uiCanvas, UIBase uiBase)
        {
            // 調整 uiBase Canvas (會繼承於主 Canvas 設定 => SortingLayerName, AdditionalShaderChannels)
            Canvas uiBaseCanvas = uiBase.canvas;
            Canvas mainCanvas = uiCanvas.canvas;
            uiBaseCanvas.overridePixelPerfect = true;
            uiBaseCanvas.pixelPerfect = true;
            uiBaseCanvas.overrideSorting = true;
            uiBaseCanvas.sortingLayerName = mainCanvas.sortingLayerName;
            uiBaseCanvas.additionalShaderChannels = mainCanvas.additionalShaderChannels;

            // 調整 uiBase Graphic Raycaster (會繼承於主 Canvas 設定 => ignoreReversedGraphics, blockingObjects, blockingMask)
            GraphicRaycaster uiBaseGraphicRaycaster = uiBase.graphicRaycaster;
            GraphicRaycaster mainGraphicRaycaster = uiCanvas.graphicRaycaster;
            uiBaseGraphicRaycaster.ignoreReversedGraphics = mainGraphicRaycaster.ignoreReversedGraphics;
            uiBaseGraphicRaycaster.blockingObjects = mainGraphicRaycaster.blockingObjects;
            uiBaseGraphicRaycaster.blockingMask = mainGraphicRaycaster.blockingMask;

            // 是否自動遞迴設置 LayerMask
            if (UIConfig.autoSetLayerRecursively) this._SetLayerRecursively(uiBase.gameObject, uiCanvas.gameObject.layer);
        }

        /// <summary>
        /// 遞迴設置 LayerMask
        /// </summary>
        /// <param name="go"></param>
        /// <param name="layer"></param>
        private void _SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
            {
                this._SetLayerRecursively(child.gameObject, layer);
            }
        }

        /// <summary>
        /// 設置 SortingOrder 渲染排序
        /// </summary>
        /// <param name="uiBase"></param>
        private void _SetSortingOrder(UIBase uiBase)
        {
            Canvas uiBaseCanvas = uiBase?.canvas;
            if (uiBaseCanvas == null) return;

            // 設置 sortingOrder (UIBase 中設置的 order 需要再 -=1, 保留給 Renderer)
            if ((uiBase.uiSetting.order -= 1) < 0) uiBase.uiSetting.order = 0;
            // ORDER_DIFFERENCE - 2 => 1 保留給下一個UI階層, 另外一個 1 保留給 Renderer
            int uiOrder = (uiBase.uiSetting.order >= (UIConfig.ORDER_DIFFERENCE - 2)) ? (UIConfig.ORDER_DIFFERENCE - 2) : uiBase.uiSetting.order;
            int uiNodeOrder = UIConfig.UI_NODES[uiBase.uiSetting.nodeType];
            // 判斷非 Stack, 則進行設置累加, 反之則不進行
            if (!uiBase.uiSetting.stack) uiBaseCanvas.sortingOrder = uiNodeOrder + uiOrder;
            else uiBaseCanvas.sortingOrder = uiNodeOrder;
        }

        /// <summary>
        /// 設置 UI 底下子物件的 Renderer 階層與渲染順序 (非 UI 物件)
        /// </summary>
        /// <param name="uiBase"></param>
        /// <param name="go">要搜尋的根物件節點</param>
        private void _SetRendererOrder(UIBase uiBase)
        {
            Canvas uiBaseCanvas = uiBase?.canvas;
            if (uiBaseCanvas == null) return;

            Renderer[] renderers = uiBase.gameObject?.GetComponentsInChildren<Renderer>();
            if (renderers == null || renderers.Length == 0) return;

            // 設置 Renderer 的 sortingLayerName
            string sortingLayerName = uiBaseCanvas.sortingLayerName;
            // 設置 Renderer 的 sortingOrder【Renderer 需要 > 本身 UI 的階層才能夠正常顯示, 所以才 +1 (否則會與父節點同屬階層被本身 UI 遮住)】
            int sortingOrder = uiBaseCanvas.sortingOrder + 1;

            // 進行迴圈設置
            foreach (var renderer in renderers)
            {
                renderer.sortingLayerName = sortingLayerName;
                renderer.sortingOrder = sortingOrder;
            }
        }
        #endregion

        #region Show
        public override async UniTask<UIBase> Show(int groupId, string packageName, string assetName, object obj = null, string awaitingUIAssetName = null, Progression progression = null, Transform parent = null)
        {
            if (string.IsNullOrEmpty(assetName)) return null;

            // 先取出 Stack 主體
            var stack = this.GetStackFromAllCache(assetName);

            // 判斷非多實例直接 return
            if (stack != null && !stack.allowInstantiate)
            {
                if (this.CheckIsShowing(assetName))
                {
                    Debug.LogWarning($"UI: {assetName} already exists!!!");
                    return null;
                }
            }

            await this.ShowAwaiting(groupId, packageName, awaitingUIAssetName); // 開啟預顯加載 UI

            var uiBase = await this.LoadIntoAllCache(packageName, assetName, progression, false);
            if (uiBase == null)
            {
                Debug.LogWarning($"UI: {assetName} => Asset not found at this path!!!");
                return null;
            }

            uiBase.SetGroupId(groupId);
            uiBase.SetHidden(false);
            await this.LoadAndDisplay(uiBase, obj);
            this.LoadAndDisplayReverse(uiBase, obj);

            Debug.Log($"<color=#1effad>Show UI: <color=#ffdb1e>{assetName}</color></color>");

            this.CloseAwaiting(awaitingUIAssetName); // 執行完畢後, 關閉預顯加載 UI

            return uiBase;
        }
        #endregion

        #region Reverse Operation
        /// <summary>
        /// 針對 Close 時, 如果有遇到 Destroy 狀況需要進行反切換緩存安全處理
        /// </summary>
        /// <param name="doRemoveSafety"></param>
        /// <param name="uiBase"></param>
        /// <returns></returns>
        private ReverseCache _PreprocessRemoveReserveSafety(bool doRemoveSafety, UIBase uiBase)
        {
            ReverseCache reverseCache = new ReverseCache();

            if (this._dictReverse.Count == 0) return reverseCache;

            if (doRemoveSafety)
            {
                // 透過 CanvasName 當作 Reverse 緩存 Key (主要是獨立切出不同 Canvas 的反切緩存)
                var key = uiBase.uiSetting.canvasName;

                int topIdx = this._dictReverse[key].Count - 1;
                var top = this._dictReverse[key][topIdx];
                UIBase equalsTop = null;
                for (int i = topIdx; i > 0; i--)
                {
                    // 僅取出 Top == 該 UI 的 Reverse 緩存
                    if (i == topIdx && top.uiBase.Equals(uiBase))
                    {
                        equalsTop = top.uiBase;
                        // 額外計算堆疊數 (用於校正堆疊計數)
                        reverseCache.extraStack++;
                        continue;
                    }

                    // 移除所有 Top 以下有關於被銷毀該 UI 的 Reverse 緩存
                    if (uiBase.assetName == this._dictReverse[key][i].uiBase.assetName)
                    {
                        this._dictReverse[key].RemoveAt(i);
                        // 額外計算堆疊數 (用於校正堆疊計數)
                        reverseCache.extraStack++;
                        Debug.Log($"[pre-forceDestroy process] Remove {uiBase.assetName} from reverse cache.");
                    }
                }
                reverseCache.uiBase = equalsTop;
            }

            return reverseCache;
        }

        /// <summary>
        /// 檢查 UI 是否為 Reverse 的最上層 UI
        /// </summary>
        /// <param name="uiBase"></param>
        /// <returns></returns>
        private bool _IsEqualsReverseTop(UIBase uiBase)
        {
            var key = uiBase.uiSetting.canvasName;
            if (this._dictReverse.ContainsKey(key))
            {
                if (this._dictReverse[key].Count > 0 && uiBase.reverseChanges)
                {
                    var top = this._dictReverse[key][this._dictReverse[key].Count - 1];
                    // 該 UI 不等於 Reverse 緩存中的 Top UI 則返回 false
                    if (!top.uiBase.Equals(uiBase)) return false;
                }
            }

            return true;
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
                FrameStack<UIBase> stack = this.GetStackFromAllCache(assetName);
                foreach (var uiBase in stack.cache.ToArray())
                {
                    uiBase.SetHidden(false);
                    this.ExitAndHide(uiBase, disablePreClose);

                    if (forceDestroy) this.Destroy(uiBase, assetName);
                    else if (uiBase.allowInstantiate) this.Destroy(uiBase, assetName);
                    else if (uiBase.onCloseAndDestroy) this.Destroy(uiBase, assetName);
                }

                // 清除 ReverseChanges 跟 Stack 緩存 (主要是為了校正)
                if (this._dictReverse.Count > 0) this._dictReverse.Clear();
                if (this._dictStackCounter.Count > 0) this._dictStackCounter.Clear();
            }
            else
            {
                UIBase uiBase = this.PeekStackFromAllCache(assetName);
                if (uiBase == null) return;

                // 如果強制關閉 UI, 需要處理原本柱列在 Reverse 中的 UI 緩存
                ReverseCache equalsTop = this._PreprocessRemoveReserveSafety(forceDestroy, uiBase);
                uiBase.SetHidden(false);
                this.ExitAndHide(uiBase, disablePreClose, equalsTop.extraStack);
                // 如果檢測到 equalsTop.uiBase != null 則需要進行反切還原
                this.ExitAndHideReverse(uiBase, !forceDestroy || (equalsTop.uiBase != null));

                if (forceDestroy) this.Destroy(uiBase, assetName);
                else if (uiBase.allowInstantiate) this.Destroy(uiBase, assetName);
                else if (uiBase.onCloseAndDestroy) this.Destroy(uiBase, assetName);
            }

            Debug.Log($"<color=#1effad>Close UI: <color=#ffdb1e>{assetName}</color></color>");
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

            foreach (FrameStack<UIBase> stack in this._dictAllCache.Values.ToArray())
            {
                // prevent preload mode
                if (stack.Count() == 0) continue;

                string assetName = stack.assetName;

                var uiBase = stack.Peek();

                // 檢查排除執行的 UI
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

                // 排除在外的 UI 直接略過處理
                if (checkWithout) continue;

                // 如果沒有強制 Destroy + 不是顯示狀態則直接略過處理
                if (!forceDestroy && !this.CheckIsShowing(uiBase) && !uiBase.allowInstantiate) continue;

                // 如有啟用 CloseAll 需跳過開關, 則不列入關閉執行
                if (uiBase.uiSetting.whenCloseAllToSkip) continue;

                this._Close(assetName, disablePreClose, forceDestroy, true);
            }
        }

        public override void CloseAll(int groupId, bool disablePreClose = false, bool forceDestroy = false, params string[] withoutAssetNames)
        {
            if (this._dictAllCache.Count == 0) return;

            foreach (FrameStack<UIBase> stack in this._dictAllCache.Values.ToArray())
            {
                // prevent preload mode
                if (stack.Count() == 0) continue;

                string assetName = stack.assetName;

                var uiBase = stack.Peek();

                if (uiBase.groupId != groupId) continue;

                // 檢查排除執行的 UI
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

                // 排除在外的 UI 直接略過處理
                if (checkWithout) continue;

                // 如果沒有強制 Destroy + 不是顯示狀態則直接略過處理
                if (!forceDestroy && !this.CheckIsShowing(uiBase) && !uiBase.allowInstantiate) continue;

                // 如有啟用 CloseAll 需跳過開關, 則不列入關閉執行
                if (uiBase.uiSetting.whenCloseAllToSkip) continue;

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
                Debug.LogWarning($"UI: {assetName} already reveal!!!");
                return;
            }

            FrameStack<UIBase> stack = this.GetStackFromAllCache(assetName);
            foreach (var uiBase in stack.cache)
            {
                if (!uiBase.isHidden) return;

                // 判斷 UI 跟 Reverse 最上層不相同則跳過 Reveal (相反的如果相同表示是上一次被 Hide 的 UI)
                if (!this._IsEqualsReverseTop(uiBase)) continue;

                this.LoadAndDisplay(uiBase).Forget();

                Debug.Log($"<color=#1effad>Reveal UI: <color=#ffdb1e>{assetName}</color></color>");
            }
        }

        public override void Reveal(string assetName)
        {
            this._Reveal(assetName);
        }

        public override void RevealAll()
        {
            if (this._dictAllCache.Count == 0) return;

            foreach (FrameStack<UIBase> stack in this._dictAllCache.Values)
            {
                // prevent preload mode
                if (stack.Count() == 0) continue;

                string assetName = stack.assetName;

                var uiBase = stack.Peek();

                if (!uiBase.isHidden) continue;

                this._Reveal(assetName);
            }
        }

        public override void RevealAll(int groupId)
        {
            if (this._dictAllCache.Count == 0) return;

            foreach (FrameStack<UIBase> stack in this._dictAllCache.Values)
            {
                // prevent preload mode
                if (stack.Count() == 0) continue;

                string assetName = stack.assetName;

                var uiBase = stack.Peek();

                if (uiBase.groupId != groupId) continue;

                if (!uiBase.isHidden) continue;

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

            FrameStack<UIBase> stack = this.GetStackFromAllCache(assetName);

            if (!this.CheckIsShowing(stack.Peek())) return;

            foreach (var uiBase in stack.cache)
            {
                uiBase.SetHidden(true);
                this.ExitAndHide(uiBase);
            }

            Debug.Log($"<color=#1effad>Hide UI: <color=#ffdb1e>{assetName}</color></color>");
        }

        public override void Hide(string assetName)
        {
            this._Hide(assetName);
        }

        public override void HideAll(params string[] withoutAssetNames)
        {
            if (this._dictAllCache.Count == 0) return;

            // 需要注意緩存需要 temp 出來, 因為如果迴圈裡有功能直接對緩存進行操作會出錯
            foreach (FrameStack<UIBase> stack in this._dictAllCache.Values)
            {
                // prevent preload mode
                if (stack.Count() == 0) continue;

                string assetName = stack.assetName;

                var uiBase = stack.Peek();

                // 檢查排除執行的 UI
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

                // 排除在外的 UI 直接略過處理
                if (checkWithout) continue;

                // 如有啟用 HideAll 需跳過開關, 則不列入關閉執行
                if (!uiBase.reverseChanges && uiBase.uiSetting.whenHideAllToSkip) continue;

                this._Hide(assetName);
            }
        }

        public override void HideAll(int groupId, params string[] withoutAssetNames)
        {
            if (this._dictAllCache.Count == 0) return;

            // 需要注意緩存需要 temp 出來, 因為如果迴圈裡有功能直接對緩存進行操作會出錯
            foreach (FrameStack<UIBase> stack in this._dictAllCache.Values)
            {
                // prevent preload mode
                if (stack.Count() == 0) continue;

                string assetName = stack.assetName;

                var uiBase = stack.Peek();

                if (uiBase.groupId != groupId) continue;

                // 檢查排除執行的 UI
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

                // 排除在外的 UI 直接略過處理
                if (checkWithout) continue;

                // 如有啟用 HideAll 需跳過開關, 則不列入關閉執行
                if (!uiBase.reverseChanges && uiBase.uiSetting.whenHideAllToSkip) continue;

                this._Hide(assetName);
            }
        }
        #endregion

        #region 開啟窗體 & 關閉窗體
        protected async UniTask LoadAndDisplay(UIBase uiBase, object obj = null, bool doStack = true)
        {
            // 非隱藏才正規處理
            if (!uiBase.isHidden) await uiBase.PreInit();
            uiBase.Display(obj);

            // 堆疊式管理 (只有非隱藏才進行堆疊計數管理)
            if (uiBase.uiSetting.stack && !uiBase.isHidden && doStack)
            {
                // canvasType + nodeType (進行歸類處理, 同屬一個 canvas 並且在同一個 node)
                string key = $"{uiBase.uiSetting.canvasName}{uiBase.uiSetting.nodeType}";
                NodeType nodeType = uiBase.uiSetting.nodeType;

                if (!this._dictStackCounter.ContainsKey(key)) this._dictStackCounter.Add(key, 0);

                // 確保在節點中的第一個 UI 物件, 堆疊層數是從 0 開始
                var uiCanvas = this.GetUICanvas(uiBase.uiSetting.canvasName);
                var goNode = uiCanvas?.GetUINode(uiBase.uiSetting.nodeType);
                if (goNode != null && goNode.transform.childCount == 1) this._dictStackCounter[key] = 0;

                // 堆疊層數++
                this._dictStackCounter[key]++;
                Debug.Log($"<color=#45b5ff>[UI Stack Layer] Canvas: {uiBase.uiSetting.canvasName}, Layer: {uiBase.uiSetting.nodeType}, Stack Count: <color=#9dff45>{this._dictStackCounter[key]}</color></color>");
                // 最大差值 -1 是為了保留給下一階層
                if (this._dictStackCounter[key] >= (UIConfig.ORDER_DIFFERENCE - 1))
                {
                    this._dictStackCounter[key] = UIConfig.ORDER_DIFFERENCE - 1;
                }

                Canvas uiBaseCanvas = uiBase?.canvas;
                if (uiBaseCanvas != null)
                {
                    // 需先還原原階層順序, 以下再進行堆疊層數計數的計算 (-1 是要後續保留給 Renderer +1 用)
                    uiBaseCanvas.sortingOrder = UIConfig.UI_NODES[nodeType];
                    uiBaseCanvas.sortingOrder += this._dictStackCounter[key] - 1;
                    // 設置查找 UI 中 Renderer 的 SortingOrder 
                    this._SetRendererOrder(uiBase);
                }

                // 最後將物件設置到最後一個節點
                uiBase.gameObject.transform.SetAsLastSibling();
            }
        }

        protected void ExitAndHide(UIBase uiBase, bool disablePreClose = false, int extraStack = 0)
        {
            uiBase.Hide(disablePreClose);

            // 堆疊式管理 (只有非隱藏才進行堆疊計數管理)
            if (uiBase.uiSetting.stack && !uiBase.isHidden)
            {
                // canvasType + nodeType (進行歸類處理, 同屬一個 canvas 並且在同一個 node)
                string key = $"{uiBase.uiSetting.canvasName}{uiBase.uiSetting.nodeType}";

                if (this._dictStackCounter.ContainsKey(key))
                {
                    // 堆疊層數--
                    this._dictStackCounter[key] = (extraStack > 0) ? this._dictStackCounter[key] - extraStack : --this._dictStackCounter[key];
                    Debug.Log($"<color=#45b5ff>[UI Stack Layer] Canvas: {uiBase.uiSetting.canvasName}, Layer: {uiBase.uiSetting.nodeType} => Stack Count: <color=#ff45be>{this._dictStackCounter[key]}</color></color>");
                    if (this._dictStackCounter[key] <= 0) this._dictStackCounter.Remove(key);
                }
            }
        }
        #endregion

        #region 反切開啟窗體 & 反切關閉窗體
        protected void LoadAndDisplayReverse(UIBase uiBase, object data, bool doReverse = true)
        {
            if (doReverse && uiBase.reverseChanges)
            {
                // 如果屬於多實例 UI 則不需儲存數據 (節省內存)
                if (uiBase.allowInstantiate) data = null;

                // 使用 CanvasName 作為 Reverse 緩存的 Key (主要是獨立切出不同 Canvas 的反切緩存)
                var key = uiBase.uiSetting.canvasName;
                if (this._dictReverse.ContainsKey(key))
                {
                    if (this._dictReverse[key].Count > 0)
                    {
                        // 如果當前 UI 是 Reverse 模式, 則加入緩存
                        this._dictReverse[key].Add(new ReverseCache() { uiBase = uiBase, data = data, extraStack = 0 });

                        // 取出倒數第二個 UI
                        var secondLast = this._dictReverse[key][this._dictReverse[key].Count - 2];
                        // 使用 Hide 方式進行關閉, 避免影響堆疊層級計數
                        secondLast.uiBase.SetHidden(true);
                        // 關閉倒數第二個 UI
                        this.ExitAndHide(secondLast.uiBase, true);
                    }
                }
                else
                {
                    // 如果當前 UI 是 Reverse 模式, 則加入緩存
                    this._dictReverse.Add(key, new List<ReverseCache>());
                    this._dictReverse[key].Add(new ReverseCache() { uiBase = uiBase, data = data, extraStack = 0 });
                }
            }
        }

        protected void ExitAndHideReverse(UIBase uiBase, bool doReverse = true)
        {
            if (doReverse && uiBase.reverseChanges)
            {
                // 使用 CanvasName 作為 Reverse 緩存的 Key (主要是獨立切出不同 Canvas 的反切緩存)
                var key = uiBase.uiSetting.canvasName;
                if (this._dictReverse[key].Count > 1)
                {
                    // 如果當前 UI 是 Reverse 模式, 將會直接移除最上層的緩存
                    this._dictReverse[key].RemoveAt(this._dictReverse[key].Count - 1);
                    // 移除最上層後, 再取一次最上層 (等於是倒數第二變成最 Top)
                    var top = this._dictReverse[key][this._dictReverse[key].Count - 1];
                    // 開啟最上層的 UI (堆疊的緩存中後出的會有 isHidden = false 參考問題, 所以需要強制 doStack = false, 避免影響堆疊計數)
                    // ※備註: 另外就是如果 allowInstantiate = false, 會有 Data Reference 問題, 所以會需要儲存上一次的數據進行還原 (需注意在同參考的情況下, 當 Second Last 被關閉時, Hidden = true 剛好不用數據還原)
                    this.LoadAndDisplay(top.uiBase, top.data, false).Forget();
                }
                else if (this._dictReverse[key].Count > 0 && this._dictReverse[key].Count < 2)
                {
                    this._dictReverse[key].RemoveAt(this._dictReverse[key].Count - 1);
                    this._dictReverse.Remove(key);
                }
            }
        }
        #endregion

        /// <summary>
        /// 從緩存中移除 UICanvas
        /// </summary>
        /// <param name="canvasName"></param>
        public void RemoveUICanvasFromCache(string canvasName)
        {
            if (this._dictUICanvas.ContainsKey(canvasName))
            {
                this._dictUICanvas[canvasName] = null;
                this._dictUICanvas.Remove(canvasName);
            }
        }
    }
}