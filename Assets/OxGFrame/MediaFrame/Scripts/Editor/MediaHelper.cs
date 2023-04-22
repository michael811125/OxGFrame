using OxGFrame.MediaFrame;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class MediaHelper
{
    public const string MenuRoot = "OxGFrame/MediaFrame/";

    /// <summary>
    /// 產生 Media URL 配置檔至輸出路徑
    /// </summary>
    /// <param name="productName"></param>
    /// <param name="appVersion"></param>
    /// <param name="outputPath"></param>
    public static void ExportMediaUrlConfig(string audioUrlset, string videoUrlset, string outputPath)
    {
        if (string.IsNullOrEmpty(audioUrlset)) audioUrlset = "127.0.0.1/audio/";
        if (string.IsNullOrEmpty(videoUrlset)) videoUrlset = "127.0.0.1/video/";

        IEnumerable<string> contents = new string[]
        {
            @$"# {MediaConfig.AUDIO_URLSET} = Audio Source Url Path",
            @$"# {MediaConfig.VIDEO_URLSET} = Video Source Url Path",
            "",
            $"{MediaConfig.AUDIO_URLSET} {audioUrlset}",
            $"{MediaConfig.VIDEO_URLSET} {videoUrlset}",
        };

        string fullOutputPath = Path.Combine(outputPath, MediaConfig.mediaUrlFileName);

        // 寫入配置文件
        File.WriteAllLines(fullOutputPath, contents, System.Text.Encoding.UTF8);

        Debug.Log($"<color=#00FF00>【Export {MediaConfig.mediaUrlFileName} Completes】</color>");
    }
}
