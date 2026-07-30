using System;

namespace OxGFrame.CenterFrame.APICenter
{
    /// <summary>
    /// Error info of a failed HTTP request
    /// </summary>
    public struct ErrorInfo
    {
        /// <summary>
        /// The requested url
        /// </summary>
        public string url;
        /// <summary>
        /// The error message
        /// </summary>
        public string message;
        /// <summary>
        /// The caught exception, if any
        /// </summary>
        public Exception exception;
    }

    /// <summary>
    /// Handle a successful response with the response text
    /// </summary>
    /// <param name="response"></param>
    public delegate void ResponseHandle(string response);
    /// <summary>
    /// Handle a failed request with the error info
    /// </summary>
    /// <param name="errorInfo"></param>
    public delegate void ResponseErrorHandle(ErrorInfo errorInfo);
}