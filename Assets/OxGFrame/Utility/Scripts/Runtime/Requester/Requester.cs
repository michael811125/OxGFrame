using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace OxGFrame.Utility.Request
{
    public static class Requester
    {
        /// <summary>
        /// Audio request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="audioType"></param>
        /// <param name="successAction"></param>
        /// <param name="errorAction"></param>
        /// <param name="cts"></param>
        /// <returns></returns>
        public static async UniTask<AudioClip> RequestAudio(string url, AudioType audioType = AudioType.MPEG, Action<AudioClip> successAction = null, Action errorAction = null, CancellationTokenSource cts = null)
        {
            UnityWebRequest request = null;
            try
            {
                request = UnityWebRequestMultimedia.GetAudioClip(url, audioType);
                ((DownloadHandlerAudioClip)request.downloadHandler).streamAudio = true;

                if (cts != null) await request.SendWebRequest().WithCancellation(cts.Token);
                else await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ProtocolError ||
                    request.result == UnityWebRequest.Result.ConnectionError)
                {
                    request.Dispose();
                    errorAction?.Invoke();
                    Debug.Log($"<color=#FF0000>Request failed, URL: {url}</color>");
                    return null;
                }

                AudioClip audioClip = ((DownloadHandlerAudioClip)request.downloadHandler).audioClip;
                successAction?.Invoke(audioClip);
#if UNITY_EDITOR
                ulong sizeBytes = (ulong)request.downloadHandler.data.Length;
                Debug.Log($"<color=#90ff67>Request Audio => Channel: {audioClip.channels}, Frequency: {audioClip.frequency}, Sample: {audioClip.samples}, Length: {audioClip.length}, State: {audioClip.loadState}, Size: {GetBytesToString(sizeBytes)}</color>");
#endif
                request.Dispose();
                return audioClip;
            }
            catch
            {
                request.Dispose();
                errorAction?.Invoke();
                Debug.Log($"<color=#FF0000>Request failed, URL: {url}</color>");
                return null;
            }
        }

        /// <summary>
        /// Texture2D request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="successAction"></param>
        /// <param name="errorAction"></param>
        /// <param name="cts"></param>
        /// <returns></returns>
        public static async UniTask<Texture2D> RequestTexture2D(string url, Action<Texture2D> successAction = null, Action errorAction = null, CancellationTokenSource cts = null)
        {
            UnityWebRequest request = null;
            try
            {
                request = new UnityWebRequest(url);
                request.downloadHandler = new DownloadHandlerTexture();

                if (cts != null) await request.SendWebRequest().WithCancellation(cts.Token);
                else await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ProtocolError ||
                    request.result == UnityWebRequest.Result.ConnectionError)
                {
                    request.Dispose();
                    errorAction?.Invoke();
                    Debug.Log($"<color=#FF0000>Request failed, URL: {url}</color>");
                    return null;
                }

                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                successAction?.Invoke(texture);
#if UNITY_EDITOR
                ulong sizeBytes = (ulong)request.downloadHandler.data.Length;
                Debug.Log($"<color=#90ff67>Request Texture2D => Width: {texture.width}, Height: {texture.height}, Size: {GetBytesToString(sizeBytes)}</color>");
#endif
                request.Dispose();
                return texture;
            }
            catch
            {
                request.Dispose();
                errorAction?.Invoke();
                Debug.Log($"<color=#FF0000>Request failed, URL: {url}</color>");
                return null;
            }
        }

        /// <summary>
        /// Sprite request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="successAction"></param>
        /// <param name="errorAction"></param>
        /// <param name="position"></param>
        /// <param name="pivot"></param>
        /// <param name="cts"></param>
        /// <returns></returns>
        public static async UniTask<Sprite> RequestSprite(string url, Action<Sprite> successAction = null, Action errorAction = null, Vector2 position = default, Vector2 pivot = default, CancellationTokenSource cts = null)
        {
            var texture = await RequestTexture2D(url, null, errorAction, cts);
            if (texture != null)
            {
                pivot = pivot != Vector2.zero ? pivot : new Vector2(0.5f, 0.5f);
                Sprite sprite = Sprite.Create(texture, new Rect(position.x, position.y, texture.width, texture.height), pivot);
                successAction?.Invoke(sprite);
                return sprite;
            }

            return null;
        }

        /// <summary>
        /// File bytes request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="successAction"></param>
        /// <param name="errorAction"></param>
        /// <param name="cts"></param>
        /// <returns></returns>
        public static async UniTask<byte[]> RequestBytes(string url, Action<byte[]> successAction = null, Action errorAction = null, CancellationTokenSource cts = null)
        {
            UnityWebRequest request = null;
            try
            {
                request = UnityWebRequest.Get(url);

                if (cts != null) await request.SendWebRequest().WithCancellation(cts.Token);
                else await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ProtocolError ||
                    request.result == UnityWebRequest.Result.ConnectionError)
                {
                    request.Dispose();
                    errorAction?.Invoke();
                    Debug.Log($"<color=#FF0000>Request failed, URL: {url}</color>");
                    return new byte[] { };
                }

                byte[] bytes = request.downloadHandler.data;
                successAction?.Invoke(bytes);
#if UNITY_EDITOR
                ulong sizeBytes = (ulong)bytes.Length;
                Debug.Log($"<color=#90ff67>Request Bytes => Size: {GetBytesToString(sizeBytes)}</color>");
#endif
                request.Dispose();
                return bytes;
            }
            catch
            {
                request.Dispose();
                errorAction?.Invoke();
                Debug.Log($"<color=#FF0000>Request failed, URL: {url}</color>");
                return new byte[] { };
            }
        }

        /// <summary>
        /// File text request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="successAction"></param>
        /// <param name="errorAction"></param>
        /// <param name="cts"></param>
        /// <returns></returns>
        public static async UniTask<string> RequestText(string url, Action<string> successAction = null, Action errorAction = null, CancellationTokenSource cts = null)
        {
            UnityWebRequest request = null;
            try
            {
                request = UnityWebRequest.Get(url);

                if (cts != null) await request.SendWebRequest().WithCancellation(cts.Token);
                else await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ProtocolError ||
                    request.result == UnityWebRequest.Result.ConnectionError)
                {
                    request.Dispose();
                    errorAction?.Invoke();
                    Debug.Log($"<color=#FF0000>Request failed, URL: {url}</color>");
                    return null;
                }

                string text = request.downloadHandler.text;
                successAction?.Invoke(text);
#if UNITY_EDITOR
                ulong sizeBytes = (ulong)request.downloadHandler.data.Length;
                Debug.Log($"<color=#90ff67>Request Text => Size: {GetBytesToString(sizeBytes)}</color>");
#endif
                request.Dispose();
                return text;
            }
            catch
            {
                request?.Dispose();
                errorAction?.Invoke();
                Debug.Log($"<color=#FF0000>Request failed, URL: {url}</color>");
                return null;
            }
        }

        /// <summary>
        /// Bytes ToString (KB, MB, GB)
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        internal static string GetBytesToString(ulong bytes)
        {
            if (bytes < (1024 * 1024 * 1f))
            {
                return (bytes / 1024f).ToString("f2") + "KB";
            }
            else if (bytes >= (1024 * 1024 * 1f) && bytes < (1024 * 1024 * 1024 * 1f))
            {
                return (bytes / (1024 * 1024 * 1f)).ToString("f2") + "MB";
            }
            else
            {
                return (bytes / (1024 * 1024 * 1024 * 1f)).ToString("f2") + "GB";
            }
        }
    }
}
