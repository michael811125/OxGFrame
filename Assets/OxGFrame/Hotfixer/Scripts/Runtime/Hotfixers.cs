using OxGFrame.AssetLoader.Bundle;
using System.Reflection;

namespace OxGFrame.Hotfixer
{
    public static class Hotfixers
    {
        /// <summary>
        /// Reset hotfix flgas and cache
        /// </summary>
        public static void Reset()
        {
            HotfixManager.GetInstance().Reset();
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
