using OxGFrame.MediaFrame.VideoFrame;
using UnityEngine;
using UnityEngine.UI;

public class VideoFrameDemo : MonoBehaviour
{
    public const string VIDEO_PATH = "Video/";

    public RawImage rawImage = null;

    #region Video cast to 【Camera】
    public async void PlayVideoCamera()
    {
        // 如果設置使用 Camera, 則無需取出 RenderTexture, 直接播放即可 (適合 Logo, OP 等等全屏影片)

        // Resource
        await VideoManager.GetInstance().Play(VIDEO_PATH + "video_cam_Example");

        // Bundle
        //await VideoManager.GetInstance().Play("mediaframe/video/video_cam_Example", "video_cam_Example");
    }

    public void StopVideoCamera()
    {
        // Resource
        VideoManager.GetInstance().Stop(VIDEO_PATH + "video_cam_Example");

        // Bundle
        //VideoManager.GetInstance().Stop("video_cam_Example");
    }

    public void StopVideoWithDestoryCamera()
    {
        // Resource
        VideoManager.GetInstance().Stop(VIDEO_PATH + "video_cam_Example", false, true);

        // Bundle
        //VideoManager.GetInstance().Stop("video_cam_Example", true);
    }

    public void PauseVideoCamera()
    {
        // Resource
        VideoManager.GetInstance().Pause(VIDEO_PATH + "video_cam_Example");

        // Bundle
        //VideoManager.GetInstance().Pause("video_cam_Example");
    }
    #endregion

    #region Video cast to 【RenderTexture】
    public async void PlayVideoRenderTexture()
    {
        // Play 時返回為陣列 (不過只有一個而已, 所以只要取出[0])

        // Resource
        var videos = await VideoManager.GetInstance().Play(VIDEO_PATH + "video_rt_Example");

        // Bundle
        //var videos = await VideoManager.GetInstance().Play("mediaframe/video/video_rt_Example", "video_rt_Example");

        // 取出第一個影片
        if (videos[0] != null)
        {
            // 這邊需要將 RawImage.enabled = true (因為在設置 Prefab 時, 建議把 RawImage 的 enable 關掉)
            this.rawImage.enabled = true;
            // 取得影片所投射的 RenderTexture, 並且指定給 RawImage.texture
            this.rawImage.texture = videos[0].GetTargetRenderTexture();
            // 設置 EndEvent 的 Handler (主要是因為, 等到影片播放結束時, 避免未將 RawImage 上的 Texture 清除, 而導致殘留畫面)
            videos[0].SetEndEvent(() =>
            {
                // 指定 RawImage.texture = null (清空)
                this.rawImage.texture = null;
                // 最後關閉 RawImage.enabled = false (關閉渲染)
                this.rawImage.enabled = false;
            });
        }
    }

    public void StopVideoRenderTexture()
    {
        VideoManager.GetInstance().Stop(VIDEO_PATH + "video_rt_Example");
    }

    public void StopVideoWithDestoryRenderTexture()
    {
        // 如果該 Video 尚未勾選 OnDestroyAndUnload, 可以使用該方法強制關閉並且卸載 (Resources or Bundle [Overloading])
        //VideoManager.GetInstance().ForceUnload(VIDEO_PATH + "video_rt_Example");

        VideoManager.GetInstance().Stop(VIDEO_PATH + "video_rt_Example", false, true);
    }

    public void PauseVideoRenderTexture()
    {
        VideoManager.GetInstance().Pause(VIDEO_PATH + "video_rt_Example");
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
