using System.IO;

namespace OxGFrame.Hotfixer
{
    public static class HotfixConfig
    {
        /// <summary>
        /// 配置文件頭標
        /// </summary>
        internal const short CIPHER_HEADER = 0x584F;

        /// <summary>
        /// 配置文件金鑰
        /// </summary>
        internal const byte CIPHER = 0x6D;

        /// <summary>
        /// 佈署配置文件
        /// </summary>
        public const string HOTFIX_DLL_CFG_NAME = "hotfixdllconfig.conf";

        /// <summary>
        /// 獲取 StreamingAssets 配置文件請求路徑
        /// </summary>
        /// <returns></returns>
        internal static string GetStreamingAssetsConfigRequestPath()
        {
            return Path.Combine(WebRequester.GetRequestStreamingAssetsPath(), HOTFIX_DLL_CFG_NAME);
        }
    }
}