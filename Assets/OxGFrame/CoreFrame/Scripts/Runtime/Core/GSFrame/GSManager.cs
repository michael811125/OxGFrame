using System;
using System.Collections.Generic;
using System.Linq;
using AssetLoader;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CoreFrame
{
    namespace GSFrame
    {
        public class GSManager : FrameManager<GSBase>
        {
            private GameObject _goRoot = null;                                                      // 根節點物件
            private Dictionary<string, GameObject> _goNodes = new Dictionary<string, GameObject>(); // 節點物件

            private static readonly object _locker = new object();
            private static GSManager _instance = null;
            public static GSManager GetInstance()
            {
                if (_instance == null)
                {
                    lock (_locker)
                    {
                        _instance = FindObjectOfType(typeof(GSManager)) as GSManager;
                        if (_instance == null) _instance = new GameObject(GSSysDefine.GS_MANAGER_NAME).AddComponent<GSManager>();
                    }
                }
                return _instance;
            }

            private void Awake()
            {
                DontDestroyOnLoad(this);

                // 先設置GSRoot
                if (this._SetupGSRoot(GSSysDefine.GS_MANAGER_NAME))
                {
                    // 建立GSNodes
                    this._CreateAllGSNode();
                }
            }

            #region 初始建立Node相關方法
            private bool _SetupGSRoot(string name)
            {
                this._goRoot = GameObject.Find(name);
                if (this._goRoot == null) return false;
                return true;
            }

            private void _CreateAllGSNode()
            {
                foreach (var nodeName in Enum.GetNames(typeof(NodeType)))
                {
                    if (!this._goNodes.ContainsKey(nodeName))
                    {
                        this._goNodes.Add(nodeName, this._CreateGSNode(nodeName));
                    }
                }
            }

            private GameObject _CreateGSNode(string name)
            {
                // 檢查是否已經有先被創立了
                GameObject nodeChecker = this._goRoot.transform.Find(name)?.gameObject;
                if (nodeChecker != null) return nodeChecker;

                GameObject nodeGo = new GameObject(name);
                // 設置GSNode為GSRoot的子節點
                nodeGo.transform.SetParent(this._goRoot.transform);

                // 校正Transform
                nodeGo.transform.localScale = Vector3.one;
                nodeGo.transform.localPosition = Vector3.zero;

                return nodeGo;
            }
            #endregion

            #region 實作Loading
            protected override GSBase Instantiate(GSBase gsBase, string bundleName, string assetName, AddIntoCache addIntoCache)
            {
                GameObject instPref = Instantiate(gsBase.gameObject, this._goRoot.transform); // instantiate 【GS Prefab】 (先指定Instantiate Parent為GSRoot)

                // 激活檢查, 如果主體Active為false必須打開
                if (!instPref.activeSelf) instPref.SetActive(true);

                instPref.name = instPref.name.Replace("(Clone)", ""); // Replace Name
                gsBase = instPref.GetComponent<GSBase>();
                if (gsBase == null) return null;

                addIntoCache?.Invoke(gsBase);

                gsBase.SetNames(bundleName, assetName);
                gsBase.BeginInit();                          // Clone取得GSBase組件後, 也初始GSBase相關設定
                gsBase.InitFirst();                         // Clone取得GSBase組件後, 也初始GSBase相關綁定組件設定

                // >>> 需在InitThis之後, 以下設定開始生效 <<<

                if (!this.SetParent(gsBase)) return null;   // 透過NodeType類型, 設置Parent

                gsBase.gameObject.SetActive(false);         // 最後設置完畢後, 關閉GameObject的Active為false

                return gsBase;
            }
            #endregion

            #region 相關校正與設置
            /// <summary>
            /// 依照對應的Node類型設置母節點
            /// </summary>
            /// <param name="gsBase"></param>
            protected override bool SetParent(GSBase gsBase)
            {
                if (this._goNodes.TryGetValue(gsBase.gsSetting.nodeType.ToString(), out GameObject goNode))
                {
                    gsBase.gameObject.transform.SetParent(goNode.transform);
                    return true;
                }

                return false;
            }
            #endregion

            #region Show
            public override async UniTask<GSBase> Show(int groupId, string assetName, object obj = null, string loadingUIAssetName = null, Progression progression = null)
            {
                if (string.IsNullOrEmpty(assetName)) return null;

                GSBase gsBase = this.GetFromAllCache(assetName);

                if (gsBase != null && !gsBase.allowInstantiate)
                {
                    if (this.CheckIsShowing(assetName))
                    {
                        Debug.LogWarning(string.Format("【GS】{0}已經存在了!!!", assetName));
                        return null;
                    }
                }

                await this.ShowLoading(groupId, string.Empty, loadingUIAssetName); // 開啟預顯加載UI

                gsBase = await this.LoadIntoAllCache(string.Empty, assetName, progression, false);
                if (gsBase == null)
                {
                    Debug.LogWarning(string.Format("找不到相對路徑資源【GS】: {0}", assetName));
                    return null;
                }

                gsBase.SetGroupId(groupId);
                gsBase.SetHidden(false);
                await this.LoadAndDispay(gsBase, obj);

                Debug.Log(string.Format("顯示GS: 【{0}】", assetName));

                this.CloseLoading(loadingUIAssetName); // 執行完畢後, 關閉預顯加載UI

                return gsBase;
            }

            public override async UniTask<GSBase> Show(int groupId, string bundleName, string assetName, object obj = null, string loadingUIBundleName = null, string loadingUIAssetName = null, Progression progression = null)
            {
                if (string.IsNullOrEmpty(bundleName) && string.IsNullOrEmpty(assetName)) return null;

                GSBase gsBase = this.GetFromAllCache(assetName);

                if (gsBase != null && !gsBase.allowInstantiate)
                {
                    if (this.CheckIsShowing(assetName))
                    {
                        Debug.LogWarning(string.Format("【GS】{0}已經存在了!!!", assetName));
                        return null;
                    }
                }

                await this.ShowLoading(groupId, loadingUIBundleName, loadingUIAssetName); // 開啟預顯加載UI

                gsBase = await this.LoadIntoAllCache(bundleName, assetName, progression, false);
                if (gsBase == null)
                {
                    Debug.LogWarning(string.Format("找不到相對路徑資源【GS】: {0}", assetName));
                    return null;
                }

                gsBase.SetGroupId(groupId);
                gsBase.SetHidden(false);
                await this.LoadAndDispay(gsBase, obj);

                Debug.Log(string.Format("顯示GS: 【{0}】", assetName));

                this.CloseLoading(loadingUIAssetName); // 執行完畢後, 關閉預顯加載UI

                return gsBase;
            }
            #endregion

            #region Close
            /// <summary>
            /// 將 Close 方法封裝 (由接口 Close 與 CloseAll 統一調用)
            /// </summary>
            /// <param name="assetName"></param>
            /// <param name="disableDoSub"></param>
            /// <param name="withDestroy"></param>
            /// <param name="doAll"></param>
            private void _Close(string assetName, bool disableDoSub, bool withDestroy, bool doAll)
            {
                if (string.IsNullOrEmpty(assetName) || !this.HasInAllCache(assetName)) return;

                if (doAll)
                {
                    FrameStack<GSBase> stack = this.GetStackFromAllCache(assetName);
                    foreach (var gsBase in stack.cache.ToArray())
                    {
                        gsBase.SetHidden(false);
                        this.ExitAndHide(gsBase, disableDoSub);

                        if (withDestroy) this.Destroy(gsBase, assetName);
                        else if (gsBase.allowInstantiate) this.Destroy(gsBase, assetName);
                        else if (gsBase.onCloseAndDestroy) this.Destroy(gsBase, assetName);
                    }
                }
                else
                {
                    GSBase gsBase = this.GetFromAllCache(assetName);
                    if (gsBase == null) return;

                    gsBase.SetHidden(false);
                    this.ExitAndHide(gsBase, disableDoSub);

                    if (withDestroy) this.Destroy(gsBase, assetName);
                    else if (gsBase.allowInstantiate) this.Destroy(gsBase, assetName);
                    else if (gsBase.onCloseAndDestroy) this.Destroy(gsBase, assetName);
                }

                Debug.Log(string.Format("關閉GS: 【{0}】", assetName));
            }

            public override void Close(string assetName, bool disableDoSub = false, bool withDestroy = false)
            {
                // 如果沒有強制Destroy + 不是顯示狀態則直接return
                if (!withDestroy && !this.CheckIsShowing(assetName)) return;
                this._Close(assetName, disableDoSub, withDestroy, false);
            }

            public override void CloseAll(bool disableDoSub = false, bool withDestroy = false, params string[] withoutAssetNames)
            {
                if (this._dictAllCache.Count == 0) return;

                foreach (FrameStack<GSBase> stack in this._dictAllCache.Values.ToArray())
                {
                    string assetName = stack.assetName;

                    var gsBase = stack.Peek();

                    // 檢查排除執行的GS
                    bool checkWithout = false;
                    if (withoutAssetNames.Length > 0)
                    {
                        for (int i = 0; i < withoutAssetNames.Length; i++)
                        {
                            if (assetName == withoutAssetNames[i]) checkWithout = true;
                        }
                    }

                    // 排除在外的GS直接略過處理
                    if (checkWithout) continue;

                    // 如果沒有強制Destroy + 不是顯示狀態則直接略過處理
                    if (!withDestroy && !this.CheckIsShowing(gsBase)) continue;

                    // 如有啟用CloseAll需跳過開關, 則不列入關閉執行
                    if (gsBase.gsSetting.whenCloseAllToSkip) continue;

                    this._Close(assetName, disableDoSub, withDestroy, true);
                }
            }

            public override void CloseAll(int groupId, bool disableDoSub = false, bool withDestroy = false, params string[] withoutAssetNames)
            {
                if (this._dictAllCache.Count == 0) return;

                foreach (FrameStack<GSBase> stack in this._dictAllCache.Values.ToArray())
                {
                    string assetName = stack.assetName;

                    var gsBase = stack.Peek();

                    if (gsBase.groupId != groupId) continue;

                    // 檢查排除執行的GS
                    bool checkWithout = false;
                    if (withoutAssetNames.Length > 0)
                    {
                        for (int i = 0; i < withoutAssetNames.Length; i++)
                        {
                            if (assetName == withoutAssetNames[i]) checkWithout = true;
                        }
                    }

                    // 排除在外的GS直接略過處理
                    if (checkWithout) continue;

                    // 如果沒有強制Destroy + 不是顯示狀態則直接略過處理
                    if (!withDestroy && !this.CheckIsShowing(gsBase)) continue;

                    // 如有啟用CloseAll需跳過開關, 則不列入關閉執行
                    if (gsBase.gsSetting.whenCloseAllToSkip) continue;

                    this._Close(assetName, disableDoSub, withDestroy, true);
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
                if (string.IsNullOrEmpty(assetName) || !this.HasInAllCache(assetName)) return;

                if (this.CheckIsShowing(assetName))
                {
                    Debug.LogWarning(string.Format("【GS】{0}已經解除隱藏了!!!", assetName));
                    return;
                }

                FrameStack<GSBase> stack = this.GetStackFromAllCache(assetName);
                foreach (var gsBase in stack.cache.ToArray())
                {
                    if (!gsBase.isHidden) return;

                    this.LoadAndDispay(gsBase).Forget();

                    Debug.Log(string.Format("解除隱藏GS: 【{0}】", assetName));
                }
            }

            public override void Reveal(string assetName)
            {
                this._Reveal(assetName);
            }

            public override void RevealAll()
            {
                if (this._dictAllCache.Count == 0) return;

                foreach (FrameStack<GSBase> stack in this._dictAllCache.Values.ToArray())
                {
                    string assetName = stack.assetName;

                    var gsBase = stack.Peek();

                    if (!gsBase.isHidden) continue;

                    this._Reveal(assetName);
                }
            }

            public override void RevealAll(int groupId)
            {
                if (this._dictAllCache.Count == 0) return;

                foreach (FrameStack<GSBase> stack in this._dictAllCache.Values.ToArray())
                {
                    string assetName = stack.assetName;

                    var gsBase = stack.Peek();

                    if (gsBase.groupId != groupId) continue;

                    if (!gsBase.isHidden) continue;

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
                if (string.IsNullOrEmpty(assetName) || !this.HasInAllCache(assetName)) return;

                FrameStack<GSBase> stack = this.GetStackFromAllCache(assetName);

                if (!this.CheckIsShowing(stack.Peek())) return;

                foreach (var gsBase in stack.cache.ToArray())
                {
                    gsBase.SetHidden(true);
                    this.ExitAndHide(gsBase);
                }

                Debug.Log(string.Format("隱藏GS: 【{0}】", assetName));
            }

            public override void Hide(string assetName)
            {
                this._Hide(assetName);
            }

            public override void HideAll(params string[] withoutAssetNames)
            {
                if (this._dictAllCache.Count == 0) return;

                foreach (FrameStack<GSBase> stack in this._dictAllCache.Values.ToArray())
                {
                    string assetName = stack.assetName;

                    var gsBase = stack.Peek();

                    // 檢查排除執行的GS
                    bool checkWithout = false;
                    if (withoutAssetNames.Length > 0)
                    {
                        for (int i = 0; i < withoutAssetNames.Length; i++)
                        {
                            if (assetName == withoutAssetNames[i]) checkWithout = true;
                        }
                    }

                    // 排除在外的GS直接略過處理
                    if (checkWithout) continue;

                    // 如有啟用HideAll需跳過開關, 則不列入關閉執行
                    if (gsBase.gsSetting.whenHideAllToSkip) continue;

                    this._Hide(assetName);
                }
            }

            public override void HideAll(int groupId, params string[] withoutAssetNames)
            {
                if (this._dictAllCache.Count == 0) return;

                foreach (FrameStack<GSBase> stack in this._dictAllCache.Values.ToArray())
                {
                    string assetName = stack.assetName;

                    var gsBase = stack.Peek();

                    if (gsBase.groupId != groupId) continue;

                    // 檢查排除執行的GS
                    bool checkWithout = false;
                    if (withoutAssetNames.Length > 0)
                    {
                        for (int i = 0; i < withoutAssetNames.Length; i++)
                        {
                            if (assetName == withoutAssetNames[i]) checkWithout = true;
                        }
                    }

                    // 排除在外的GS直接略過處理
                    if (checkWithout) continue;

                    // 如有啟用HideAll需跳過開關, 則不列入關閉執行
                    if (gsBase.gsSetting.whenHideAllToSkip) continue;

                    this._Hide(assetName);
                }
            }
            #endregion

            #region 顯示場景 & 關閉場景
            protected override async UniTask LoadAndDispay(GSBase gsBase, object obj = null)
            {
                if (!gsBase.isHidden) await gsBase.PreInit();
                gsBase.Display(obj);
            }

            protected override void ExitAndHide(GSBase gsBase, bool disableDoSub = false)
            {
                gsBase.Hide(disableDoSub);
            }
            #endregion
        }
    }
}
