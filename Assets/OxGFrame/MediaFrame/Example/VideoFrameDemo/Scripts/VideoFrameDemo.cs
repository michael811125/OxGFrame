using OxGFrame.MediaFrame;
using UnityEngine;
using UnityEngine.UI;

public class VideoFrameDemo : MonoBehaviour
{
    // if use prefix "res#" will load from resource else will from bundle
    public const string VIDEO_PATH = "res#Video/";

    public RawImage rawImage = null;

    #region Video cast to 【Camera】
    public async void PlayVideoCamera()
    {
        // if render mode is Camera just play directly
        await MediaFrames.VideoFrame.Play(VIDEO_PATH + "video_cam_Example");
    }

    public void StopVideoCamera()
    {
        MediaFrames.VideoFrame.Stop(VIDEO_PATH + "video_cam_Example");
    }

    public void StopVideoWithDestoryCamera()
    {
        MediaFrames.VideoFrame.Stop(VIDEO_PATH + "video_cam_Example", false, true);
    }

    public void PauseVideoCamera()
    {
        MediaFrames.VideoFrame.Pause(VIDEO_PATH + "video_cam_Example");
    }
    #endregion

    #region Video cast to 【RenderTexture】
    public async void PlayVideoRenderTexture()
    {
        var videos = await MediaFrames.VideoFrame.Play(VIDEO_PATH + "video_rt_Example");

        // Get first idx
        if (videos[0] != null)
        {
            // Make sure rawImage is enabled
            this.rawImage.enabled = true;
            // GetTargetRenderTexture and assign to rawImage.texture
            this.rawImage.texture = videos[0].GetTargetRenderTexture();
            // Set EndEvent handler (if video play end can clear rawImage.texture)
            videos[0].SetEndEvent(() =>
            {
                this.rawImage.texture = null;
                this.rawImage.enabled = false;
            });
        }
    }

    public void StopVideoRenderTexture()
    {
        MediaFrames.VideoFrame.Stop(VIDEO_PATH + "video_rt_Example");
    }

    public void StopVideoWithDestoryRenderTexture()
    {
        // if Video is not checked OnDestroyAndUnload, can use ForceUnload to stop and unload
        //MediaFrames.VideoFrame.ForceUnload(VIDEO_PATH + "video_rt_Example");

        MediaFrames.VideoFrame.Stop(VIDEO_PATH + "video_rt_Example", false, true);
    }

    public void PauseVideoRenderTexture()
    {
        MediaFrames.VideoFrame.Pause(VIDEO_PATH + "video_rt_Example");
    }
    #endregion

    #region Control All Video
    public void ResumeAll()
    {
        MediaFrames.VideoFrame.ResumeAll();
    }

    public void StopAll()
    {
        MediaFrames.VideoFrame.StopAll();
    }

    public void StopAllWithDestroy()
    {
        MediaFrames.VideoFrame.StopAll(false, true);
    }

    public void PauseAll()
    {
        MediaFrames.VideoFrame.PauseAll();
    }
    #endregion
}
