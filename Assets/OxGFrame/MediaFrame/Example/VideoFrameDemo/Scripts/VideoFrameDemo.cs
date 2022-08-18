using OxGFrame.MediaFrame.VideoFrame;
using UnityEngine;
using UnityEngine.UI;

public class VideoFrameDemo : MonoBehaviour
{
    public const string VIDEO_PATH = "Video/";

    public RawImage rawImage = null;

    private void Start()
    {
        //BundleDistributor.GetInstance().Check();
    }

    #region Video cast to 【Camera】
    public async void PlayVideoCamera()
    {
        // 如果設置使用Camera, 則無需取出RenderTexture並且給予特殊指定, 直接播放 (適合Logo、OP等等全屏影片)
        await VideoManager.GetInstance().Play(VIDEO_PATH + "video_cam_Example");

        // Bundle
        //await VideoManager.GetInstance().Play("mediaframe/video/video_cam_Example", "video_cam_Example");
    }

    public void StopVideoCamera()
    {
        VideoManager.GetInstance().Stop(VIDEO_PATH + "video_cam_Example");

        // Bundle
        //VideoManager.GetInstance().Stop("video_cam_Example");
    }

    public void StopVideoWithDestoryCamera()
    {
        VideoManager.GetInstance().Stop(VIDEO_PATH + "video_cam_Example", false, true);

        // Bundle
        //VideoManager.GetInstance().Stop("video_cam_Example", true);
    }

    public void PauseVideoCamera()
    {
        VideoManager.GetInstance().Pause(VIDEO_PATH + "video_cam_Example");

        // Bundle
        //VideoManager.GetInstance().Pause("video_cam_Example");
    }
    #endregion

    #region Video cast to 【RenderTexture】
    public async void PlayVideoRenderTexture()
    {
        // Play時返回為陣列 (不過只有一個而已, 所以只要取出[0])
        var videos = await VideoManager.GetInstance().Play(VIDEO_PATH + "video_rt_Example");

        // Bundle
        //var videos = await VideoManager.GetInstance().Play("mediaframe/video/video_rt_Example", "video_rt_Example");

        // 取出第一個影片
        if (videos[0].GetTargetRenderTexture() != null)
        {
            // 這邊需要將RawImage.enabled = true (因為在設置Prefab時, 建議把RawImage的enable關掉)
            this.rawImage.enabled = true;
            // 取得影片所投射的RenderTexture, 並且指定給RawImage.texture
            this.rawImage.texture = videos[0].GetTargetRenderTexture();
            // 設置EndEvent的Handler (主要是因為, 等到影片播放結束時, 避免未將RawImage上的Texture清除, 而導致殘留畫面)
            videos[0].SetEndEvent(() =>
            {
                // 指定RawImage.texture = null (清空)
                this.rawImage.texture = null;
                // 最後關閉RawImage.enabled = false (關閉渲染)
                this.rawImage.enabled = false;
            });
        }
    }

    public void StopVideoRenderTexture()
    {
        VideoManager.GetInstance().Stop(VIDEO_PATH + "video_rt_Example");

        // Bundle
        //VideoManager.GetInstance().Stop("video_rt_Example");
    }

    public void StopVideoWithDestoryRenderTexture()
    {
        VideoManager.GetInstance().Stop(VIDEO_PATH + "video_rt_Example", false, true);

        // Bundle
        //VideoManager.GetInstance().Stop("video_rt_Example", true);
    }

    public void PauseVideoRenderTexture()
    {
        VideoManager.GetInstance().Pause(VIDEO_PATH + "video_rt_Example");

        // Bundle
        //VideoManager.GetInstance().Pause("video_rt_Example");
    }
    #endregion

    #region Control All Video
    public void ResumeAll()
    {
        VideoManager.GetInstance().ResumeAll();
    }

    public void StopAll()
    {
        VideoManager.GetInstance().StopAll();
    }

    public void StopAllWithDestroy()
    {
        VideoManager.GetInstance().StopAll(false, true);
    }

    public void PauseAll()
    {
        VideoManager.GetInstance().PauseAll();
    }
    #endregion
}
