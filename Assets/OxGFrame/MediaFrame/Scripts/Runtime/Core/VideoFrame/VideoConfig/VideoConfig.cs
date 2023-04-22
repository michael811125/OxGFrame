using System;
using UnityEngine;

namespace OxGFrame.MediaFrame.VideoFrame
{
    public enum SourceType
    {
        Video,
        Streaming,
        Url
    }

    public enum RenderMode
    {
        RenderTexture,
        Camera
    }

    [Serializable]
    public class TargetCamera
    {
        public enum OrderType
        {
            Background,
            Foreground
        }
        public Camera camera = null;
        public OrderType orderType = OrderType.Background;
        [Range(0, 1)] public float alpha = 1;
    }
}