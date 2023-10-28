using OxGKit.LoggingSystem;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using YooAsset;

namespace OxGFrame.AssetLoader.Bundle
{
    public abstract class RequestQueryBase : IBuildinQueryServices, IDeliveryQueryServices, IDeliveryLoadServices
    {
        #region IBuildinQueryServices, IDeliveryQueryServices
        public virtual bool Query(string packageName, string fileName)
        {
            return false;
        }
        #endregion

        #region IDeliveryQueryServices
        public string GetFilePath(string packageName, string fileName)
        {
            return null;
        }
        #endregion

        #region IDeliveryLoadServices
        public virtual AssetBundle LoadAssetBundle(DeliveryFileInfo fileInfo)
        {
            return null;
        }

        public virtual AssetBundleCreateRequest LoadAssetBundleAsync(DeliveryFileInfo fileInfo)
        {
            return null;
        }
        #endregion

        #region RequestQueryBase
        protected IEnumerator WebRequest(string url)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SendWebRequest();

            while (true)
            {
                if (request == null)
                {
                    Logging.Print<Logger>($"<color=#FF0000>Request is null. URL: {url}</color>");
                    yield return false;
                }

                // 網路中斷後主線程異常, 編輯器下無法正常運行, 不過發佈模式沒問題
                if (request.result == UnityWebRequest.Result.DataProcessingError ||
                    request.result == UnityWebRequest.Result.ProtocolError ||
                    request.result == UnityWebRequest.Result.ConnectionError)
                {
                    Logging.Print<Logger>($"<color=#FF0000>【Query】Request failed. Code: {request.responseCode}, URL: {url}</color>");
                    request?.Dispose();
                    yield return false;
                }

                if (request.downloadedBytes > 0 && request.responseCode < 400)
                {
                    Logging.Print<Logger>($"<color=#00FF00>【Query】Request succeeded. Code: {request.responseCode}, PartialBytes: {request.downloadedBytes}, URL: {url}</color>");
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
#else
            return path;
#endif
        }
        #endregion
    }

    #region Builtin Services
    public class RequestBuiltinQuery : RequestQueryBase
    {
        public override bool Query(string packageName, string fileName)
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
    #endregion

    #region Delivery Services
    public class RequestDeliveryQuery : RequestQueryBase { }
    #endregion
}