using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OxGFrame.MediaFrame.Editor
{
    public static class MediaHelper
    {
        internal const string MENU_ROOT = "OxGFrame/MediaFrame/";

        #region Public Methods
        #region Exporter
        /// <summary>
        /// 產生 Media URL 配置檔至輸出路徑 (Export MediaUrlConfig to StreamingAssets [for Built-in])
        /// </summary>
        /// <param name="productName"></param>
        /// <param name="appVersion"></param>
        /// <param name="outputPath"></param>
        public static void ExportMediaUrlConfig(string audioUrlset, string videoUrlset, string outputPath)
        {
            if (string.IsNullOrEmpty(audioUrlset)) audioUrlset = "http://127.0.0.1/audio/";
            if (string.IsNullOrEmpty(videoUrlset)) videoUrlset = "http://127.0.0.1/video/";

            IEnumerable<string> texts = new string[]
            {
            @$"# {MediaConfig.AUDIO_URLSET} = Audio Source Url Path",
            @$"# {MediaConfig.VIDEO_URLSET} = Video Source Url Path",
            "",
            $"{MediaConfig.AUDIO_URLSET} {audioUrlset}",
            $"{MediaConfig.VIDEO_URLSET} {videoUrlset}",
            };

            string fullOutputPath = Path.Combine(outputPath, MediaConfig.MEDIA_URL_CFG_NAME);

            // 寫入配置文件
            File.WriteAllLines(fullOutputPath, texts, System.Text.Encoding.UTF8);

            Debug.Log($"【Export {MediaConfig.MEDIA_URL_CFG_NAME} Completes】");
        }
        #endregion
        #endregion

        /// <summary>
        /// 寫入文字文件檔
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="outputPath"></param>
        internal static void WriteTxt(string txt, string outputPath)
        {
            // 寫入配置文件
            var file = File.CreateText(outputPath);
            file.Write(txt);
            file.Close();
        }
    }
}