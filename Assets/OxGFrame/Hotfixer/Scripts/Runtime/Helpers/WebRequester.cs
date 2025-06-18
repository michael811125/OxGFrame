using System;
using System.Collections;
using UnityEngine;

namespace OxGFrame.Hotfixer
{
    internal static class WebRequester
    {
        private const int _MAX_REQUEST_TIME_SECONDS = 10;

        public static IEnumerator RequestText(string url, Action<string> successAction, Action errorAction = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogWarning($"<color=#FF0000>Request failed. URL is null or empty.</color>");
                errorAction?.Invoke();
                yield break;
            }

            // 編輯器有需求時, 如果使用 UnityWebRequest 會死機, 只能統一改用 WWW
            WWW www = new WWW(url);

            // 設置超時時間
            float timer = 0f;
            float maxTime = _MAX_REQUEST_TIME_SECONDS;

            while (!www.isDone && timer < maxTime)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            // 檢查是否超時
            if (timer >= maxTime && !www.isDone)
            {
                Debug.Log($"<color=#FF0000>Request timed out. URL: {url}, SpentTime: {timer} (s)</color>");
                errorAction?.Invoke();
                www.Dispose();
                yield break;
            }

            // 檢查是否有錯誤
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.Log($"<color=#FF0000>Request failed. URL: {url}, Error: {www.error}, SpentTime: {timer} (s)</color>");
                errorAction?.Invoke();
                www.Dispose();
                yield break;
            }

            // 成功獲取內容
            Debug.Log($"<color=#90ff67>Successfully requested text file. URL: {url}, SpentTime: {timer} (s)</color>");
            successAction?.Invoke(www.text);
            www.Dispose();
            yield break;
        }

        public static IEnumerator RequestBytes(string url, Action<byte[]> successAction, Action errorAction = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogWarning($"<color=#FF0000>Request failed. URL is null or empty.</color>");
                errorAction?.Invoke();
                yield break;
            }

            // 編輯器有需求時, 如果使用 UnityWebRequest 會死機, 只能統一改用 WWW
            WWW www = new WWW(url);

            // 設置超時時間
            float timer = 0f;
            float maxTime = _MAX_REQUEST_TIME_SECONDS;

            while (!www.isDone && timer < maxTime)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            // 檢查是否超時
            if (timer >= maxTime && !www.isDone)
            {
                Debug.Log($"<color=#FF0000>Request timed out. URL: {url}, SpentTime: {timer} (s)</color>");
                errorAction?.Invoke();
                www.Dispose();
                yield break;
            }

            // 檢查是否有錯誤
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.Log($"<color=#FF0000>Request failed. URL: {url}, Error: {www.error}, SpentTime: {timer} (s)</color>");
                errorAction?.Invoke();
                www.Dispose();
                yield break;
            }

            // 成功獲取內容
            Debug.Log($"<color=#90ff67>Successfully requested bytes file. URL: {url}, SpentTime: {timer} (s)</color>");
            successAction?.Invoke(www.bytes);
            www.Dispose();
            yield break;
        }

        /// <summary>
        /// 獲取 WWW StreamingAssets 路徑 (OSX and iOS 需要 + file://)
        /// </summary>
        /// <returns></returns>
        public static string GetRequestStreamingAssetsPath()
        {
#if UNITY_STANDALONE_OSX || UNITY_IOS
            return $"file://{Application.streamingAssetsPath}";
#else
            return Application.streamingAssetsPath;
#endif
        }
    }
}