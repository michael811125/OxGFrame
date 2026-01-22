using System.IO;

namespace OxGFrame.Hotfixer
{
    public static class HotfixConfig
    {
        /// <summary>
        /// 獲取 StreamingAssets 配置文件請求路徑
        /// </summary>
        /// <returns></returns>
        internal static string GetStreamingAssetsConfigRequestPath()
        {
            return Path.Combine(WebRequester.GetRequestStreamingAssetsPath(), $"{HotfixSettings.settings.hotfixCfgName}{HotfixSettings.settings.hotfixCfgExtension}");
        }
    }
}