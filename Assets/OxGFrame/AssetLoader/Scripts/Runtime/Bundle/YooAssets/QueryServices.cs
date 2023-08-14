using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using YooAsset;

namespace OxGFrame.AssetLoader.Bundle
{
    public abstract class RequestQueryBase : IBuildinQueryServices
    {
        public DeliveryFileInfo GetDeliveryFileInfo(string packageName, string fileName)
        {
            throw new System.NotImplementedException();
        }

        public bool QueryDeliveryFiles(string packageName, string fileName)
        {
            return false;
        }

        public virtual bool QueryStreamingAssets(string packageName, string fileName, bool isRawFile)
        {
            return false;
        }

        protected IEnumerator WebRequest(string url)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SendWebRequest();

            while (true)
            {
                if (request == null)
                {
                    Debug.Log($"<color=#FF0000>Request is null. URL: {url}</color>");
                    yield return false;
                }

                // 網路中斷後主線程異常, 編輯器下無法正常運行, 不過發佈模式沒問題
                if (request.result == UnityWebRequest.Result.DataProcessingError ||
                    request.result == UnityWebRequest.Result.ProtocolError ||
                    request.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log($"<color=#FF0000>【Query】Request failed. Code: {request.responseCode}, URL: {url}</color>");
                    request?.Dispose();
                    yield return false;
                }

                if (request.downloadedBytes > 0 && request.responseCode < 400)
                {
                    Debug.Log($"<color=#00FF00>【Query】Request succeeded. Code: {request.responseCode}, PartialBytes: {request.downloadedBytes}, URL: {url}</color>");
                    request?.Dispose();
                    yield return true;
                }

                yield return null;
            }
        }

        protected string ConvertToWWWPath(string path)
        {
#if UNITY_EDITOR
            return string.Format("file:///{0}", path);
#elif UNITY_IPHONE
            return string.Format("file://{0}", path);
#elif UNITY_ANDROID
            return path;
#elif UNITY_STANDALONE
            return string.Format("file:///{0}", path);
#endif
        }
    }

    public class RequestBuiltinQuery : RequestQueryBase
    {
        public override bool QueryStreamingAssets(string packageName, string fileName, bool isRawFile)
        {
#if UNITY_WEBGL
            return StreamingAssetsHelper.FileExists(packageName, fileName);
#else
            #region Builtin (StreamingAssets)
            string builtinPackagePath = BundleConfig.GetBuiltinPackagePath(packageName);
            string url = Path.Combine(builtinPackagePath, fileName);
            #endregion

            // Convert url to www path
            var e = this.WebRequest(this.ConvertToWWWPath(url));
            while (e.MoveNext())
                if (e.Current != null)
                    return (bool)e.Current;

            return false;
#endif
        }
    }

    public class RequestSandboxQuery : RequestQueryBase
    {
        public override bool QueryStreamingAssets(string packageName, string fileName, bool isRawFile)
        {
#if UNITY_WEBGL
            return false;
#else
            #region Sandbox (PersistentData)
            string sandboxPackagePath = BundleConfig.GetLocalSandboxPackagePath(packageName);
            string hashPrefix = fileName.Substring(0, 2);
            string hashFolderName = fileName.Split(".")[0];

            string bundleFolderName = isRawFile ? BundleConfig.yooCacheRawFolderName : BundleConfig.yooCacheBundleFolderName;
            string url = Path.Combine
            (
                sandboxPackagePath,
                bundleFolderName,
                hashPrefix,
                hashFolderName,
                BundleConfig.yooBundleFileName
            );
            #endregion

            // Convert url to www path
            var e = this.WebRequest(this.ConvertToWWWPath(url));
            while (e.MoveNext())
                if (e.Current != null)
                    return (bool)e.Current;

            return false;
#endif
        }
    }
}