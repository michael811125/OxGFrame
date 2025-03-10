using OxGFrame.MediaFrame;
using OxGFrame.MediaFrame.VideoFrame;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public static class VideoPrefs
{
    // If use prefix "res#" will load from resource else will from bundle
    private const string _prefix = "res#";

    // Paths
    private static readonly string _videoPath = $"{_prefix}Video/";

    // Assets
    public static readonly string VideoCamExample = $"{_videoPath}video_cam_Example";
    public static readonly string VideoRtExample = $"{_videoPath}video_rt_Example";
}

public class VideoFrameDemo : MonoBehaviour
{
    public VideoClip[] clips;

    public RawImage rawImage = null;
    public GameObject controlBar = null;
    public Slider progressSld = null;
    public Text speedTxt = null;

    private VideoBase _video = null;

    private void Awake()
    {
        // If Init instance can more efficiency
        MediaFrames.VideoFrame.InitInstance();
    }

    private void Update()
    {
        this.UpdateProgressVideoRenderTexture();
        this.UpdateCurrentSpeedTxtVideoRenderTexture();
    }

    #region Video cast to 【Camera】
    public async void PlayVideoCamera()
    {
        // if render mode is Camera just play directly
        // You can assign a clip to prefab and play it, or load a clip from prefab and play it
        await MediaFrames.VideoFrame.Play(VideoPrefs.VideoCamExample, this.clips[0]);
    }

    public void StopVideoCamera()
    {
        MediaFrames.VideoFrame.Stop(VideoPrefs.VideoCamExample);
    }

    public void StopVideoWithDestoryCamera()
    {
        MediaFrames.VideoFrame.Stop(VideoPrefs.VideoCamExample, false, true);
    }

    public void PauseVideoCamera()
    {
        MediaFrames.VideoFrame.Pause(VideoPrefs.VideoCamExample);
    }
    #endregion

    #region Video cast to 【RenderTexture】
    public async void PlayVideoRenderTexture()
    {
        // You can assign a clip to prefab and play it, or load a clip from prefab and play it
        var video = this._video = await MediaFrames.VideoFrame.Play(VideoPrefs.VideoRtExample, this.clips[1]);

        // Get Video
        if (video != null)
        {
            // Make sure rawImage is enabled
            this.rawImage.enabled = true;
            this.controlBar.SetActive(true);
            // GetTargetRenderTexture and assign to rawImage.texture
            this.rawImage.texture = video.GetTargetRenderTexture();
            // Set EndEvent handler (if video play end can clear rawImage.texture)
            video.SetEndEvent(() =>
            {
                this.rawImage.texture = null;
                this.rawImage.enabled = false;
                this.controlBar.SetActive(false);
            });
        }
    }

    public void StopVideoRenderTexture()
    {
        MediaFrames.VideoFrame.Stop(VideoPrefs.VideoRtExample);
        this._video = null;
    }

    public void StopVideoWithDestoryRenderTexture()
    {
        /*
         * [if Video is not checked OnDestroyAndUnload, can use ForceUnload to stop and unload]
         * 
         * MediaFrames.VideoFrame.ForceUnload(Video.VideoRtExample);
         */

        MediaFrames.VideoFrame.Stop(VideoPrefs.VideoRtExample, false, true);
        this._video = null;
    }

    public void UpdateProgressVideoRenderTexture()
    {
        if (this._video == null ||
            this.progressSld == null)
            return;

        var video = this._video;

        // Calculate progress and skip pct
        float progress = video.CurrentLength() / video.Length();
        this.progressSld.value = progress;
    }

    public void UpdateCurrentSpeedTxtVideoRenderTexture()
    {
        if (this._video == null ||
            this.speedTxt == null)
            return;

        var video = this._video;

        this.speedTxt.text = $"Speed: x{video.GetPlaySpeed().ToString("f1")}";
    }

    public void SpeedUpVideoRenderTexture()
    {
        if (this._video == null)
            return;

        var video = this._video;

        // Set video play speed (Common values include 0.25, 0.5, 1.0, 1.25, 1.5, 1.75, 2.0)
        float currentSpeed = video.playbackSpeed;
        video.SetPlaySpeed(currentSpeed + 0.1f);
    }

    public void SpeedDownVideoRenderTexture()
    {
        if (this._video == null)
            return;

        var video = this._video;

        // Set video play speed (Common values include 0.25, 0.5, 1.0, 1.25, 1.5, 1.75, 2.0)
        float currentSpeed = video.playbackSpeed;
        video.SetPlaySpeed(currentSpeed - 0.1f);
    }

    public void FastForwardVideoRenderTexture()
    {
        if (this._video == null)
            return;

        var video = this._video;

        // Calculate progress and skip pct
        float progress = video.CurrentLength() / video.Length();
        float skipPct = 5f / video.Length();

        // Set fast forward video by 5 seconds
        video.SkipToPercent(progress + skipPct);
    }

    public void RewindVideoRenderTexture()
    {
        if (this._video == null)
            return;

        var video = this._video;

        // Calculate progress and skip pct
        float progress = video.CurrentLength() / video.Length();
        float skipPct = 5f / video.Length();

        // Set rewind video by 5 seconds
        video.SkipToPercent(progress - skipPct);
    }

    public void PauseVideoRenderTexture()
    {
        MediaFrames.VideoFrame.Pause(VideoPrefs.VideoRtExample);
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
        this._video = null;
    }

    public void StopAllWithDestroy()
    {
        MediaFrames.VideoFrame.StopAll(false, true);
        this._video = null;
    }

    public void PauseAll()
    {
        MediaFrames.VideoFrame.PauseAll();
    }
    #endregion
}
