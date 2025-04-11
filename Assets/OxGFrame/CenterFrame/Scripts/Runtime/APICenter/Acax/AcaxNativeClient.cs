using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using UnityEngine;

namespace OxGFrame.CenterFrame.APICenter
{
    /// <summary>
    /// Use Native HttpClient
    /// </summary>
    public static class HttpNativeClient
    {
        /// <summary>
        /// Default request timeout in seconds
        /// </summary>
        private const int _MAX_REQUEST_TIME_SECONDS = 60;

        /// <summary>
        /// Shared HttpClient instance
        /// </summary>
        private static HttpClient _client = null;

        /// <summary>
        /// Callback C# and Xml = Acax
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="headers"></param>
        /// <param name="body"></param>
        /// <param name="success"></param>
        /// <param name="error"></param>
        /// <param name="timeoutSeconds"></param>
        public static void Acax(string url, string method, string[,] headers, object[,] body, ResponseHandle success = null, ResponseErrorHandle error = null, int? timeoutSeconds = null)
        {
            method = method.ToUpper();
            RequestAPI(url, method, headers, body, success, error, timeoutSeconds).Forget();
        }

        public static void Acax(string url, string method, string[,] headers, object body, ResponseHandle success = null, ResponseErrorHandle error = null, int? timeoutSeconds = null)
        {
            method = method.ToUpper();
            RequestAPI(url, method, headers, body, success, error, timeoutSeconds).Forget();
        }

        /// <summary>
        /// Asynchronous with Callback C# and Xml = Acax
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="headers"></param>
        /// <param name="body"></param>
        /// <param name="success"></param>
        /// <param name="error"></param>
        /// <param name="timeoutSeconds"></param>
        /// <returns></returns>
        public static async UniTask<string> AcaxAsync(string url, string method, string[,] headers, object[,] body, ResponseHandle success = null, ResponseErrorHandle error = null, int? timeoutSeconds = null)
        {
            method = method.ToUpper();
            return await RequestAPI(url, method, headers, body, success, error, timeoutSeconds);
        }

        public static async UniTask<string> AcaxAsync(string url, string method, string[,] headers, object body, ResponseHandle success = null, ResponseErrorHandle error = null, int? timeoutSeconds = null)
        {
            method = method.ToUpper();
            return await RequestAPI(url, method, headers, body, success, error, timeoutSeconds);
        }

        #region Internal Methods
        internal static async UniTask<string> RequestAPI(string url, string method, string[,] headers, object body, ResponseHandle success, ResponseErrorHandle error, int? timeoutSeconds)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds ?? _MAX_REQUEST_TIME_SECONDS));
                using var request = new HttpRequestMessage(new HttpMethod(method), url);

                // Header args
                string contentType = "application/json";
                if (headers != null && headers.Length > 0)
                {
                    for (int row = 0; row < headers.GetLength(0); row++)
                    {
                        if (headers.GetLength(1) != 2)
                            continue;

                        string headerName = headers[row, 0];
                        string headerValue = headers[row, 1];
                        if (headerName.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                        {
                            contentType = headerValue;
                        }
                        else
                        {
                            request.Headers.TryAddWithoutValidation(headerName, headerValue);
                        }
                    }
                }

                // Body args
                if (body != null)
                {
                    string json = null;
                    if (body is object[,] bodyArray)
                    {
                        if (bodyArray.Length > 0)
                        {
                            var jsonArgs = new Dictionary<string, object>();
                            for (int row = 0; row < bodyArray.GetLength(0); row++)
                            {
                                if (bodyArray.GetLength(1) != 2)
                                    continue;

                                jsonArgs[(string)bodyArray[row, 0]] = bodyArray[row, 1];
                            }
                            json = JsonConvert.SerializeObject(jsonArgs);
                        }
                    }
                    else
                    {
                        json = JsonConvert.SerializeObject(body);
                    }

                    if (!string.IsNullOrEmpty(json))
                    {
                        var content = new StringContent(json, Encoding.UTF8);
                        content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
                        request.Content = content;
                    }
                }

                if (_client == null)
                    _client = new HttpClient();

                // Start send request
                using var response = await _client.SendAsync(request, cts.Token);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                success?.Invoke(result);
                return result;
            }
            catch (Exception ex)
            {
                var errorInfo = new ErrorInfo();
                errorInfo.url = url;
                errorInfo.message = ex.Message;
                errorInfo.exception = ex;
                error?.Invoke(errorInfo);
                Debug.LogWarning($"RequestAPI failed. URL: {errorInfo.url}, ErrorMsg: {errorInfo.message}, Exception: {ex}");
                return null;
            }
        }
        #endregion

        /// <summary>
        /// Release HttpClient
        /// </summary>
        public static void Dispose()
        {
            if (_client != null)
            {
                _client.Dispose();
                _client = null;
            }
        }
    }
}