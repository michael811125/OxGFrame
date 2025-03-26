using OxGFrame.MediaFrame.AudioFrame;
using OxGFrame.MediaFrame.VideoFrame;
using OxGKit.Utilities.Cacher;

namespace OxGFrame.MediaFrame.Cacher
{
    public class MediaObjectRemoveCacheHandler : IRemoveCacheHandler<string, string>
    {
        public void RemoveCache(string key, string value)
        {
            if (key.IndexOf(nameof(AudioBase)) != -1)
                AudioManager.GetInstance().ForceUnload(value);
            else if (key.IndexOf(nameof(VideoBase)) != -1)
                VideoManager.GetInstance().ForceUnload(value);
        }
    }
}