using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace OxGFrame.APICenter
{
    public static class Http
    {
        public delegate void ResponseHandle(string response);

        /// <summary>
        /// Asynchronous C# and Xml = Acax
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="headers"></param>
        /// <param name="body"></param>
        /// <param name="success"></param>
        /// <param name="error"></param>
        public static void Acax(string url, string method, string[,] headers, object[,] body, ResponseHandle success = null, ResponseHandle error = null)
        {
            method = method.ToUpper();
            _Request(url, method, headers, body, success, error).Forget();
        }

        /// <summary>
        /// Asynchronous C# and Xml = Acax
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="headers"></param>
        /// <param name="body"></param>
        /// <param name="success"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public async static UniTask AcaxAsync(string url, string method, string[,] headers, object[,] body, ResponseHandle success = null, ResponseHandle error = null)
        {
            method = method.ToUpper();
            await _Request(url, method, headers, body, success, error);
        }

        private static async UniTask _Request(string url, string method, string[,] headers, object[,] body, ResponseHandle success, ResponseHandle error)
        {
            using (UnityWebRequest request = new UnityWebRequest(url, method))
            {
                if (body.Length > 0)
                {
                    Dictionary<string, object> jsonObj = new Dictionary<string, object>();
                    for (int row = 0; row < body.GetLength(0); row++)
                    {
                        if (body.GetLength(1) != 2) continue;
                        jsonObj.Add((string)body[row, 0], body[row, 1]);
                    }
                    string json = JsonConvert.SerializeObject(jsonObj);

                    byte[] jsonBinary = System.Text.Encoding.Default.GetBytes(json);
                    request.uploadHandler = new UploadHandlerRaw(jsonBinary);
                    request.downloadHandler = new DownloadHandlerBuffer();
                }

                if (headers.Length > 0)
                {
                    for (int row = 0; row < headers.GetLength(0); row++)
                    {
                        if (headers.GetLength(1) != 2) continue;
                        request.SetRequestHeader(headers[row, 0], headers[row, 1]);
                    }
                }

                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
                {
                    if (error != null) error(request.error);
                }
                else
                {
                    if (success != null) success(request.downloadHandler.text);
                }
            }
        }
    }
}