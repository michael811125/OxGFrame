using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using YooAsset;

namespace OxGFrame.AssetLoader.Bundle
{
    public class RequestBuiltinQuery : IQueryServices
    {
        public bool QueryStreamingAssets(string fileName)
        {
            bool result;

            string builtinFolderName = YooAssets.GetStreamingAssetBuildinFolderName();
            string url = Path.Combine(BundleConfig.GetRequestStreamingAssetsPath(), $"{builtinFolderName}/{fileName}");

            UnityWebRequest request = null;
            try
            {
                request = UnityWebRequest.Get(url);
                request.SendWebRequest();
            }
            catch
            {
                Debug.Log($"<color=#FF0000>Request failed. URL: {url}</color>");
                request?.Dispose();
                result = false;
                return result;
            }

            while (true)
            {
                if (request == null)
                {
                    Debug.Log($"<color=#FF0000>Request is null. URL: {url}</color>");
                    result = false;
                    break;
                }

                // 網路中斷後主線程異常, 編輯器下無法正常運行, 不過發佈模式沒問題
                if (request.result == UnityWebRequest.Result.DataProcessingError ||
                    request.result == UnityWebRequest.Result.ProtocolError ||
                    request.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log($"<color=#FF0000>【Query】Request failed. Code: {request.responseCode}, URL: {url}</color>");
                    request?.Dispose();
                    result = false;
                    break;
                }

                if (request.downloadedBytes > 0 && request.responseCode < 400)
                {
                    Debug.Log($"<color=#00FF00>【Query】Request succeeded. Code: {request.responseCode}, PartialBytes: {request.downloadedBytes}, URL: {url}</color>");
                    request?.Dispose();
                    result = true;
                    break;
                }
            }

            return result;
        }
    }

    public class RequestSandboxQuery : IQueryServices
    {
        private string _packageName;

        public RequestSandboxQuery(string packageName)
        {
            this._packageName = packageName;
        }

        public bool QueryStreamingAssets(string fileName)
        {
            bool result;

            string sandboxPath = YooAssets.GetSandboxRoot();
            string hashPrefix = fileName.Substring(0, 2);
            string hashFolderName = fileName.Split(".")[0];
            string url = Path.Combine
            (
                sandboxPath,
                BundleConfig.yooCacheFolderName,
                this._packageName,
                BundleConfig.yooBundleFolderName,
                hashPrefix,
                hashFolderName,
                BundleConfig.yooBundleFileName
            );

            UnityWebRequest request = null;
            try
            {
                request = UnityWebRequest.Get(url);
                request.SendWebRequest();
            }
            catch
            {
                Debug.Log($"<color=#FF0000>Request failed. URL: {url}</color>");
                request?.Dispose();
                result = false;
                return result;
            }

            while (true)
            {
                if (request == null)
                {
                    Debug.Log($"<color=#FF0000>Request is null. URL: {url}</color>");
                    result = false;
                    break;
                }

                // 網路中斷後主線程異常, 編輯器下無法正常運行, 不過發佈模式沒問題
                if (request.result == UnityWebRequest.Result.DataProcessingError ||
                    request.result == UnityWebRequest.Result.ProtocolError ||
                    request.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log($"<color=#FF0000>【Query】Request failed. Code: {request.responseCode}, URL: {url}</color>");
                    request?.Dispose();
                    result = false;
                    break;
                }

                if (request.downloadedBytes > 0 && request.responseCode < 400)
                {
                    Debug.Log($"<color=#00FF00>【Query】Request succeeded. Code: {request.responseCode}, PartialBytes: {request.downloadedBytes}, URL: {url}</color>");
                    request?.Dispose();
                    result = true;
                    break;
                }
            }

            return result;
        }
    }
}
