using Cysharp.Threading.Tasks;
using OxGFrame.AssetLoader.Bundle;
using System;
using System.Reflection;
using UnityEngine;

namespace OxGFrame.Hotfixer
{
    /// <summary>
    /// Static facade for the hotfix flow managed by the HotfixManager
    /// </summary>
    public static class Hotfixers
    {
        /// <summary>
        /// Get AOT assembly names
        /// <para> Strings that include '.dll' extension </para>
        /// <para> Note: When you execute CheckHotfix method, it will be set </para>
        /// </summary>
        /// <returns></returns>
        public static string[] GetAOTAssemblyNames()
        {
            return HotfixManager.GetInstance().GetAOTAssemblyNames();
        }

        /// <summary>
        /// Get AOT assembly names without extensions
        /// <para> Note: When you execute CheckHotfix method, it will be set </para>
        /// </summary>
        /// <returns></returns>
        public static string[] GetAotAssemblyNamesWithoutExtensions()
        {
            return HotfixManager.GetInstance().GetAotAssemblyNamesWithoutExtensions();
        }

        /// <summary>
        /// Get Hotfix assembly names
        /// <para> Strings that include '.dll' extension </para>
        /// <para> Note: When you execute CheckHotfix method, it will be set </para>
        /// </summary>
        /// <returns></returns>
        public static string[] GetHotfixAssemblyNames()
        {
            return HotfixManager.GetInstance().GetHotfixAssemblyNames();
        }

        /// <summary>
        /// Get Hotfix assembly names without extensions
        /// <para> Note: When you execute CheckHotfix method, it will be set </para>
        /// </summary>
        /// <returns></returns>
        public static string[] GetHotfixAssemblyNamesWithoutExtensions()
        {
            return HotfixManager.GetInstance().GetHotfixAssemblyNamesWithoutExtensions();
        }

        /// <summary>
        /// Reset hotfix flags and cache
        /// </summary>
        public static void Reset()
        {
            HotfixManager.GetInstance().Reset();
        }

        /// <summary>
        /// Start hotfix files download and load all (default is AppPackageInfoWithBuild)
        /// <para> Auto request config from StreamingAssets (HotfixManifest.dat) </para>
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="errorAction"></param>
        public static void CheckHotfix(string packageName, Action errorAction = null)
        {
            _CheckHotfixFromConfig(errorAction, (aotDlls, hotfixDlls) => HotfixManager.GetInstance().CheckHotfix(packageName, aotDlls, hotfixDlls));
        }

        /// <summary>
        /// Start hotfix files download and load all
        /// <para> Auto request config from StreamingAssets (HotfixManifest.dat) </para>
        /// </summary>
        /// <param name="packageInfoWithBuild"></param>
        /// <param name="errorAction"></param>
        public static void CheckHotfix(PackageInfoWithBuild packageInfoWithBuild, Action errorAction = null)
        {
            _CheckHotfixFromConfig(errorAction, (aotDlls, hotfixDlls) => HotfixManager.GetInstance().CheckHotfix(packageInfoWithBuild, aotDlls, hotfixDlls));
        }

        /// <summary>
        /// Read the HotfixManifest config from StreamingAssets and then execute CheckHotfix
        /// </summary>
        /// <param name="errorAction"></param>
        /// <param name="checkAction"></param>
        private static void _CheckHotfixFromConfig(Action errorAction, Action<string[], string[]> checkAction)
        {
            string url = HotfixConfig.GetStreamingAssetsConfigRequestPath();
            WebRequester.RequestBytes(url, (data) =>
            {
                // 空數據視為請求失敗，必須回調 errorAction，避免熱更流程靜默卡住
                if (data == null || data.Length == 0)
                {
                    Debug.LogError($"<color=#FF0000>Hotfix config request returned empty data. Cannot start hotfix. URL: {url}</color>");
                    errorAction?.Invoke();
                    return;
                }

                var configInfo = BinaryHelper.DecryptToString(data);
                var config = JsonUtility.FromJson<HotfixManifest>(configInfo.content);
                // 配置解析失敗防護
                if (config == null ||
                    config.aotDlls == null ||
                    config.hotfixDlls == null)
                {
                    Debug.LogError($"<color=#FF0000>Hotfix config parsing failed. Cannot start hotfix. URL: {url}</color>");
                    errorAction?.Invoke();
                    return;
                }

                checkAction.Invoke(config.aotDlls.ToArray(), config.hotfixDlls.ToArray());
            }, errorAction).ToUniTask().Forget();
        }

        /// <summary>
        /// Start hotfix files download and load all (default is AppPackageInfoWithBuild)
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="aotAssemblies"></param>
        /// <param name="hotfixAssemblies"></param>
        public static void CheckHotfix(string packageName, string[] aotAssemblies, string[] hotfixAssemblies)
        {
            HotfixManager.GetInstance().CheckHotfix(packageName, aotAssemblies, hotfixAssemblies);
        }

        /// <summary>
        /// Start hotfix files download and load all
        /// </summary>
        /// <param name="packageInfoWithBuild"></param>
        /// <param name="aotAssemblies"></param>
        /// <param name="hotfixAssemblies"></param>
        public static void CheckHotfix(PackageInfoWithBuild packageInfoWithBuild, string[] aotAssemblies, string[] hotfixAssemblies)
        {
            HotfixManager.GetInstance().CheckHotfix(packageInfoWithBuild, aotAssemblies, hotfixAssemblies);
        }

        /// <summary>
        /// Get hotfix assembly (included .dll suffix)
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        public static Assembly GetHotfixAssembly(string assemblyName)
        {
            return HotfixManager.GetInstance().GetHotfixAssembly(assemblyName);
        }

        /// <summary>
        /// Return hotfix done state
        /// </summary>
        /// <returns></returns>
        public static bool IsDone()
        {
            return HotfixManager.GetInstance().IsDone();
        }

        /// <summary>
        /// Return whether Hotfix is disabled
        /// </summary>
        /// <returns></returns>
        public static bool IsDisabled()
        {
            return HotfixManager.GetInstance().IsDisabled();
        }
    }
}
