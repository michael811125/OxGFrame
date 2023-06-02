using UniFramework.Machine;
using Cysharp.Threading.Tasks;
using HybridCLR;
using OxGFrame.AssetLoader.Bundle;
using UnityEngine;
using System.Linq;
using System.Reflection;
using YooAsset;
using OxGFrame.Hotfixer.HotfixEvent;
using OxGFrame.AssetLoader;

namespace OxGFrame.Hotfixer.HotfixFsm
{
    public static class HotfixFsmStates
    {
        /// <summary>
        /// 1. 流程準備工作
        /// </summary>
        public class FsmHotfixPrepare : IStateNode
        {
            private StateMachine _machine;

            void IStateNode.OnCreate(StateMachine machine)
            {
                this._machine = machine;
            }

            void IStateNode.OnEnter()
            {
                HotfixEvents.HotfixFsmState.SendEventMessage(this);
                this._machine.ChangeState<FsmInitHotfixPackage>();
                Debug.Log("<color=#00cf6b>(Powered by HybridCLR) Hotfix Work</color>");
            }

            void IStateNode.OnUpdate()
            {
            }

            void IStateNode.OnExit()
            {
            }
        }

        /// <summary>
        /// 2. 初始 Hotfix Package
        /// </summary>
        public class FsmInitHotfixPackage : IStateNode
        {
            private StateMachine _machine;

            void IStateNode.OnCreate(StateMachine machine)
            {
                this._machine = machine;
            }

            void IStateNode.OnEnter()
            {
                HotfixEvents.HotfixFsmState.SendEventMessage(this);
                this._InitHotfixPackage().Forget();
            }

            void IStateNode.OnUpdate()
            {
            }

            void IStateNode.OnExit()
            {
            }

            private async UniTask _InitHotfixPackage()
            {
                bool isInitialized = await AssetPatcher.InitPackage(HotfixManager.GetInstance().packageName);
                if (isInitialized) this._machine.ChangeState<FsmUpdateHotfixPackage>();
                else HotfixEvents.HotfixInitFailed.SendEventMessage();
            }
        }

        /// <summary>
        /// 3. 更新 Hotfix Package
        /// </summary>
        public class FsmUpdateHotfixPackage : IStateNode
        {
            private StateMachine _machine;

            void IStateNode.OnCreate(StateMachine machine)
            {
                this._machine = machine;
            }

            void IStateNode.OnEnter()
            {
                HotfixEvents.HotfixFsmState.SendEventMessage(this);
                this._UpdateHotfixPackage().Forget();
            }

            void IStateNode.OnUpdate()
            {
            }

            void IStateNode.OnExit()
            {
            }

            private async UniTask _UpdateHotfixPackage()
            {
                bool isUpdated = await AssetPatcher.UpdatePackage(HotfixManager.GetInstance().packageName);
                if (isUpdated) this._machine.ChangeState<FsmHotfixCreateDownloader>();
                else HotfixEvents.HotfixUpdateFailed.SendEventMessage();
            }
        }

        /// <summary>
        /// 4. 創建 Hotfix 下載器
        /// </summary>
        public class FsmHotfixCreateDownloader : IStateNode
        {
            private StateMachine _machine;

            void IStateNode.OnCreate(StateMachine machine)
            {
                this._machine = machine;
            }

            void IStateNode.OnEnter()
            {
                HotfixEvents.HotfixFsmState.SendEventMessage(this);
                this._CreateHotfixDownloader();
            }

            void IStateNode.OnUpdate()
            {
            }

            void IStateNode.OnExit()
            {
            }

            private void _CreateHotfixDownloader()
            {
                // Get hotfix package
                var package = AssetPatcher.GetPackage(HotfixManager.GetInstance().packageName);

                // Create a downloader
                HotfixManager.GetInstance().mainDownloader = AssetPatcher.GetPackageDownloader(package);
                int totalDownloadCount = HotfixManager.GetInstance().mainDownloader.TotalDownloadCount;

                // Do begin download, if download count > 0
                if (totalDownloadCount > 0) this._machine.ChangeState<FsmHotfixBeginDownload>();
                else this._machine.ChangeState<FsmHotfixDownloadOver>();
            }
        }

        /// <summary>
        /// 5. 開始下載 Hotfix files
        /// </summary>
        public class FsmHotfixBeginDownload : IStateNode
        {
            private StateMachine _machine;

            void IStateNode.OnCreate(StateMachine machine)
            {
                this._machine = machine;
            }

            void IStateNode.OnEnter()
            {
                HotfixEvents.HotfixFsmState.SendEventMessage(this);
                this._StartDownloadHotfix().Forget();
            }

            void IStateNode.OnUpdate()
            {
            }

            void IStateNode.OnExit()
            {
            }

            private async UniTask _StartDownloadHotfix()
            {
                // Get hotfix package
                var package = AssetPatcher.GetPackage(HotfixManager.GetInstance().packageName);

                // Get main downloader
                var downloader = HotfixManager.GetInstance().mainDownloader;
                downloader.OnDownloadErrorCallback = HotfixEvents.HotfixDownloadFailed.SendEventMessage;
                downloader.BeginDownload();

                await downloader;

                if (downloader.Status != EOperationStatus.Succeed) return;

                this._machine.ChangeState<FsmHotfixDownloadOver>();
            }
        }

        /// <summary>
        /// 6. 完成 Hotfix 下載
        /// </summary>
        public class FsmHotfixDownloadOver : IStateNode
        {
            private StateMachine _machine;

            void IStateNode.OnCreate(StateMachine machine)
            {
                this._machine = machine;
            }

            void IStateNode.OnEnter()
            {
                HotfixEvents.HotfixFsmState.SendEventMessage(this);
                this._machine.ChangeState<FsmHotfixClearCache>();
            }

            void IStateNode.OnUpdate()
            {
            }

            void IStateNode.OnExit()
            {
                HotfixManager.GetInstance().ReleaseMainDownloader();
            }
        }

        /// <summary>
        /// 7. 清理未使用的緩存文件
        /// </summary>
        public class FsmHotfixClearCache : IStateNode
        {
            private StateMachine _machine;

            void IStateNode.OnCreate(StateMachine machine)
            {
                this._machine = machine;
            }

            void IStateNode.OnEnter()
            {
                HotfixEvents.HotfixFsmState.SendEventMessage(this);
                this._ClearUnusedCache().Forget();
            }

            void IStateNode.OnUpdate()
            {
            }

            void IStateNode.OnExit()
            {
            }

            private async UniTask _ClearUnusedCache()
            {
                // Get hotfix package
                var package = AssetPatcher.GetPackage(HotfixManager.GetInstance().packageName);
                var operation = package.ClearUnusedCacheFilesAsync();
                await operation;

                // Start load hotfix assemblies
                if (operation.IsDone) this._machine.ChangeState<FsmLoadAOTAssemblies>();
            }
        }

        /// <summary>
        /// 8. 開始補充 AOT 元數據
        /// </summary>
        public class FsmLoadAOTAssemblies : IStateNode
        {
            private StateMachine _machine;

            void IStateNode.OnCreate(StateMachine machine)
            {
                this._machine = machine;
            }

            void IStateNode.OnEnter()
            {
                HotfixEvents.HotfixFsmState.SendEventMessage(this);
                this._LoadAOTAssemblies().Forget();
            }

            void IStateNode.OnUpdate()
            {
            }

            void IStateNode.OnExit()
            {
                HotfixManager.GetInstance().ReleaseAOTAssemblyNames();
            }

            private async UniTask _LoadAOTAssemblies()
            {
                if (BundleConfig.playMode == BundleConfig.PlayMode.EditorSimulateMode)
                {
                    this._machine.ChangeState<FsmLoadHotfixAssemblies>();
                    return;
                }

                string[] aotMetaAssemblyFiles = HotfixManager.GetInstance().GetAOTAssemblyNames();

                try
                {
                    // 注意, 補充元數據是給 AOT dll 補充元數據, 而不是給熱更新 dll 補充元數據
                    // 熱更新 dll 不缺元數據, 不需要補充, 如果調用 LoadMetadataForAOTAssembly 會返回錯誤
                    HomologousImageMode mode = HomologousImageMode.SuperSet;
                    foreach (var dllName in aotMetaAssemblyFiles)
                    {
                        var dll = await AssetLoaders.LoadAssetAsync<TextAsset>(HotfixManager.GetInstance().packageName, dllName);
                        // 加載 assembly 對應的 dll, 會自動為它 hook, 一旦 aot 泛型函數的 native 函數不存在, 用解釋器版本代碼
                        LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dll.bytes, mode);
                        Debug.Log($"<color=#32fff5>Load <color=#ffde4c>AOT Assembly</color>: <color=#e2b3ff>{dllName}</color>, mode: {mode}, ret: {err}</color>");
                    }
                }
                catch
                {
                }

                this._machine.ChangeState<FsmLoadHotfixAssemblies>();
            }
        }

        /// <summary>
        /// 9. 開始加載 Hotfix 元數據
        /// </summary>
        public class FsmLoadHotfixAssemblies : IStateNode
        {
            private StateMachine _machine;

            void IStateNode.OnCreate(StateMachine machine)
            {
                this._machine = machine;
            }

            void IStateNode.OnEnter()
            {
                HotfixEvents.HotfixFsmState.SendEventMessage(this);
                this._LoadHotfixAssemblies().Forget();
            }

            void IStateNode.OnUpdate()
            {
            }

            void IStateNode.OnExit()
            {
                HotfixManager.GetInstance().ReleaseHotfixAssemblyNames();
            }

            private async UniTask _LoadHotfixAssemblies()
            {
                string[] hotfixAssemblyFiles = HotfixManager.GetInstance().GetHotfixAssemblyNames();

                try
                {
                    foreach (var dllName in hotfixAssemblyFiles)
                    {
                        Assembly hotfixAsm;
                        if (Application.isEditor ||
                            BundleConfig.playMode == BundleConfig.PlayMode.EditorSimulateMode)
                        {
                            // Editor 或 Simulate 下無需加載, 直接查找獲得 Hotfix 程序集
                            hotfixAsm = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == dllName);
                        }
                        else
                        {
                            var dll = await AssetLoaders.LoadAssetAsync<TextAsset>(HotfixManager.GetInstance().packageName, dllName);
                            hotfixAsm = Assembly.Load(dll.bytes);
                        }

                        HotfixManager.GetInstance().AddHotfixAssembly(dllName, hotfixAsm);
                        Debug.Log($"<color=#32fff5>Load <color=#ffde4c>Hotfix Assembly</color>: <color=#e2b3ff>{dllName}</color></color>");
                    }
                }
                catch
                {
                }

                this._machine.ChangeState<FsmHotfixDone>();
            }
        }

        /// <summary>
        /// 10. 完成 Hotfix
        /// </summary>
        public class FsmHotfixDone : IStateNode
        {
            void IStateNode.OnCreate(StateMachine machine)
            {
            }

            void IStateNode.OnEnter()
            {
                HotfixEvents.HotfixFsmState.SendEventMessage(this);
                HotfixManager.GetInstance().MarkAsDone();
            }

            void IStateNode.OnUpdate()
            {
            }

            void IStateNode.OnExit()
            {
            }
        }
    }
}
